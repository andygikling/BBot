//============================================================================
// Name        : main.cpp
//					This is the main entry point for BBot's embedded program
// Author      : Andy Gikling
// Version     : 0
// Copyright   : Your copyright notice
// Description : Main BBot entry point
//============================================================================
#include <stdio.h>
#include <string>
#include <termios.h>    /* Enables us to set baud rate for RX/TX separately */
#include <fcntl.h>      /* Enables use of flags to modify open(), read(), write() functions */
#include <unistd.h>     /* Enables use of open(), read(), write() */
#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <algorithm>
#include <stdlib.h>
#include <semaphore.h>
#include <pthread.h>
#include <ctime>
#include <stdexcept>      // std::out_of_range

#include <BBoneConstants.h>
#include <Utilities/SimpleGPIO.h>
#include <Voice/Voice.h>
#include <Legs/Legs.h>
#include <Mark1FPGA/Mark1FPGA.h>

using namespace std;

//Function definitions
//Used on the BeagleBone Classic
int initializeTIPinMux_K3Dot2();
//Used on the BeagleBone Black (uses the 3.8 Linux Kernel)
int initializeTIPinMux_K3Dot8();
//Data from out XBee (through the FPGA) is on UARt1 - setup Terminos
int initializeUART1();
//This design outputs all debug data to a file - set it up here
//The GUI program is looking for a file called text.txt under
//the folder /home/root/images/ here we make a simlink to the BBot's
//log file
int createLogOutputFile();
int setTextOutputFileSimlink(int target);
int clearTextOutputFileSimlink();
std::ofstream debugFileStream;
int addToLog(std::string Source, std::string Content, bool AlsoPrintf);
int EnableLog_;

//Here we spawn threads that run the main logic.
//Main simply listens on the the XBee interface
//Voice sends formatted messages to the voice module
//Legs drives the motors etc...
int spawnHelperThreads(int UARTfd);

int parseMessage(char *msg, int length);
int talk(char *msg);
int AcknowledgeMessage(std::string AckMessage);

int listenForCommands();

std::vector<std::string> inline StringSplit(const std::string &source, const char *delimiter, bool keepEmpty);

int fd1, c1, res1;
int debugFile;
struct termios oldtio1, newtio1;
char buf1[MAX_SERIAL_BUFFER_SIZE];
char bufResponse[MAX_SERIAL_BUFFER_SIZE];

Mark1_DataBlock_TX Mark1_DataBlock_TX_;
Mark1_DataBlock_RX Mark1_DataBlock_RX_;
Mark1_DataBlock_TX *Mark1_DataBlock_TX_ptr_;
Mark1_DataBlock_RX *Mark1_DataBlock_RX_ptr_;

//Pointers to our primary objects (which spawn their own threads)
Voice *voice_thread_ptr;
Legs *legs_thread_ptr;
Mark1FPGA *mark1FPGA_thread_ptr;

int main(int argc,char** argv)
{
	cout << "BBot Main Program - Running" << endl;

	//Here we sleep to allow other start up processes to finish
	//Specifically, we want the RTC clock time to be set before
	//this process starts - otherwise our log files have the wrong time stamp
#ifndef DEBUG
	cout << "BBot Embedded - Release Mode" << endl;
	sleep(25);
#else
	cout << "BBot Embedded - Debug Mode" << endl;
#endif

	cout << "Print: ListenThread : Creating Output Debug File" << endl;
	//Create text file for debug info (this will be displayed on the BBot screen)
	EnableLog_ = true;
	createLogOutputFile();
	clearTextOutputFileSimlink();

	//Hold Mark1 FPGA in reset until done initializing everything
	gpio_export(MARK1_SOFTWARE_RESET_N);
	gpio_set_dir(MARK1_SOFTWARE_RESET_N, OUTPUT_PIN);
	gpio_set_value(MARK1_SOFTWARE_RESET_N, LOW);

	//This is what we use to set the newer BeagleBone Black (Linux kernel 3.8)
	//settings / device tree
	initializeTIPinMux_K3Dot8();

	//Initialize the BBone UART to XBee link
	initializeUART1();

	Mark1_DataBlock_TX_ptr_ = &Mark1_DataBlock_TX_;
	Mark1_DataBlock_RX_ptr_ = &Mark1_DataBlock_RX_;

	//Run Mark1 FPGA
	usleep(10000);
	gpio_set_value(MARK1_SOFTWARE_RESET_N, HIGH);

	spawnHelperThreads(fd1);

	//Now we simply loop doing reads on the UART1 port.
	//The first read only returns when the delimiter is found (\r);
	//Doing a "blocking" read prevents this process's thread from eating up
	//all the CPU time.
	addToLog("ListenThread", "Main Loop Entry", true);

	listenForCommands();

	//Program is finished
	/* restore the old port settings */
	tcsetattr(fd1,TCSANOW,&oldtio1);

}

int initializeTIPinMux_K3Dot2()
{
	//Run a script that sets up the pin mux for the older 3.2 kernel (BeagleBone Classic).
	//This method doesn't use device tree overlays
	addToLog("ListenThread", "Start 3.2 Pinmux", true);
	system("/home/root/BeagleBonePinMux_Kernel3Dot2.sh");
	addToLog("ListenThread", "End 3.2 Pinmux", true);
	return 1;
}

int initializeTIPinMux_K3Dot8()
{
	//This uses the new method of pin muxing based on Device Tree Overlays
	//This code is adapted from Derek Molloy's fantastic video on the topic:
	//http://www.youtube.com/watch?v=wui_wU1AeQc

	//First run the script to enable our device tree overlay
	//If the overlay is already applied there should just be a bash "file already exists" error
	addToLog("ListenThread", "Start: Applying BBot Overlay", true);
	system("/home/root/BBot_ApplyPinMux_Kernel3Dot8.sh");
	addToLog("ListenThread", "End: Done Applying BBot Overlay", true);

	//Now export the pins we need
	gpio_export(UART_SELECT_0_GPIO_PIN_NUM);
	gpio_export(UART_SELECT_1_GPIO_PIN_NUM);
	gpio_export(UART_SELECT_2_GPIO_PIN_NUM);
	//Set the pin's direction
	gpio_set_dir(UART_SELECT_0_GPIO_PIN_NUM, OUTPUT_PIN);
	gpio_set_dir(UART_SELECT_1_GPIO_PIN_NUM, OUTPUT_PIN);
	gpio_set_dir(UART_SELECT_2_GPIO_PIN_NUM, OUTPUT_PIN);

	//Initialize the pin's state
	gpio_set_value(UART_SELECT_0_GPIO_PIN_NUM, LOW);
	gpio_set_value(UART_SELECT_1_GPIO_PIN_NUM, LOW);
	gpio_set_value(UART_SELECT_2_GPIO_PIN_NUM, LOW);

	addToLog("ListenThread", "UART Mux Pins Set to Outputs and defaults!", true);

	return 1;
}

int initializeUART1()
{
	//The following Canonical example of terminos provided by:
	//http://www.faqs.org/docs/Linux-HOWTO/Serial-Programming-HOWTO.html#AEN125
	/*
	  Open modem device for reading and writing and not as controlling tty
	  because we don't want to get killed if linenoise sends CTRL-C.
	*/
	fd1 = open(MODEMDEVICE_UART1, O_RDWR | O_NOCTTY );
	if (fd1 <0) {perror(MODEMDEVICE_UART1); exit(-1); }

	tcgetattr(fd1,&oldtio1); /* save current serial port settings */
	bzero(&newtio1, sizeof(newtio1)); /* clear struct for new port settings */

	/*
	  BAUDRATE: Set bps rate. You could also use cfsetispeed and cfsetospeed.
	  CRTSCTS : output hardware flow control (only used if the cable has
				all necessary lines. See sect. 7 of Serial-HOWTO)
	  CS8     : 8n1 (8bit,no parity,1 stopbit)
	  CLOCAL  : local connection, no modem contol
	  CREAD   : enable receiving characters
	*/
	newtio1.c_cflag = BAUDRATE_UART1 | CRTSCTS | CS8 | CLOCAL | CREAD;

	/*
	  IGNPAR  : ignore bytes with parity errors
	  ICRNL   : map CR to NL (otherwise a CR input on the other computer
				will not terminate input)
	  otherwise make device raw (no other input processing)
	*/
	newtio1.c_iflag = IGNPAR | ICRNL;

	/*
	 Raw output.
	*/
	newtio1.c_oflag = 0;

	/*
	  ICANON  : enable canonical input
	  disable all echo functionality, and don't send signals to calling program
	*/
	newtio1.c_lflag = ICANON;

	/*
	  initialize all control characters
	  default values can be found in /usr/include/termios.h, and are given
	  in the comments, but we don't need them here
	*/
	newtio1.c_cc[VINTR]    = 0;     /* Ctrl-c */
	newtio1.c_cc[VQUIT]    = 0;     /* Ctrl-\ */
	newtio1.c_cc[VERASE]   = 0;     /* del */
	newtio1.c_cc[VKILL]    = 0;     /* @ */
	newtio1.c_cc[VEOF]     = 4;     /* Ctrl-d */
	newtio1.c_cc[VTIME]    = 0;     /* inter-character timer unused */
	newtio1.c_cc[VMIN]     = 1;     /* blocking read until 1 character arrives */
	newtio1.c_cc[VSWTC]    = 0;     /* '\0' */
	newtio1.c_cc[VSTART]   = 0;     /* Ctrl-q */
	newtio1.c_cc[VSTOP]    = 0;     /* Ctrl-s */
	newtio1.c_cc[VSUSP]    = 0;     /* Ctrl-z */
	newtio1.c_cc[VEOL]     = 0;     /* '\0' */
	newtio1.c_cc[VREPRINT] = 0;     /* Ctrl-r */
	newtio1.c_cc[VDISCARD] = 0;     /* Ctrl-u */
	newtio1.c_cc[VWERASE]  = 0;     /* Ctrl-w */
	newtio1.c_cc[VLNEXT]   = 0;     /* Ctrl-v */
	newtio1.c_cc[VEOL2]    = 0;     /* '\0' */

	/*
	  now clean the modem line and activate the settings for the port
	*/
	tcflush(fd1, TCIFLUSH);
	tcsetattr(fd1,TCSANOW,&newtio1);

	return 1;
}

int createLogOutputFile()
{
	ifstream logFile(PATH_TO_LISTEN_THREAD_DEBUG_FILE);
	if(logFile.good())
	{
		//File exists - kill it
		logFile.close();
		unlink(PATH_TO_LISTEN_THREAD_DEBUG_FILE);
	}
	else
	{
		logFile.close();
	}

	time_t now = time(0);
	// convert now to string form
	char* dt = ctime(&now);
	std::string dateAndTime(dt);
	dateAndTime.erase(dateAndTime.length()-1, 2);

	std::string entry = "BBot Listen Thread Log File Created! " + dateAndTime + "\r\n";

	debugFileStream.open(PATH_TO_LISTEN_THREAD_DEBUG_FILE, std::ofstream::out);
	debugFileStream << entry;
	debugFileStream.flush();
	return 1;
}

int setTextOutputFileSimlink(int target)
{
	//The BBot Qt program is looking for a file called text.txt, here we make a simlink to
	//the file this program writes to for debug info
	int i;

	switch (target)
	{
		case 0 :
			i = symlink(PATH_TO_LISTEN_THREAD_DEBUG_FILE, PATH_TO_DEBUG_FILE_SIMLINK);
			AcknowledgeMessage("Set ListenThread Debug File Simlink");
			break;
		case 1 :
			i = symlink(PATH_TO_VOICE_THREAD_DEBUG_FILE, PATH_TO_DEBUG_FILE_SIMLINK);
			AcknowledgeMessage("Set VoiceThread Debug File Simlink");
			break;
		case 2 :
			i = symlink(PATH_TO_LEGS_THREAD_DEBUG_FILE, PATH_TO_DEBUG_FILE_SIMLINK);
			AcknowledgeMessage("Set LegsThread Debug File Simlink");
			break;
		case 3 :
			i = symlink(PATH_TO_MARK1FPGA_THREAD_DEBUG_FILE, PATH_TO_DEBUG_FILE_SIMLINK);
			AcknowledgeMessage("Set Mark1FPGAThread Debug File Simlink");
			break;
	}
	return i;
}

int clearTextOutputFileSimlink()
{
	//Deletes our simlink to debug file
	addToLog("ListenThread", "Debug File Simlink Removed", true);
	unlink(PATH_TO_DEBUG_FILE_SIMLINK);
	return 1;
}

int spawnHelperThreads(int UARTfd)
{
	//Make Voice Thread
	addToLog("ListenThread", "Launching Helper Threads", true);
	voice_thread_ptr = new Voice(UARTfd, *Mark1_DataBlock_TX_ptr_, *Mark1_DataBlock_RX_ptr_);
	legs_thread_ptr = new Legs(UARTfd, *Mark1_DataBlock_TX_ptr_, *Mark1_DataBlock_RX_ptr_);
	mark1FPGA_thread_ptr = new Mark1FPGA(*Mark1_DataBlock_TX_ptr_, *Mark1_DataBlock_RX_ptr_);

	return 1;
}

int listenForCommands()
{
	//Main thread loop... never ending
	while (1)
	{
		addToLog("ListenThread", "Now Listening...", true);

		res1 = read(fd1,buf1,sizeof(buf1));
		buf1[res1]=0;             /* set end of string, so we can printf */

		for(int i = 0; i < res1; i++)
		{
			//Here we're going to bomb on all the carriage returns and new lines
			//because they can easily confuse parsing the data
			if(buf1[i] == 0xA || buf1[i] == 0xD)
			{
				buf1[i] = 0;
			}
		}

		parseMessage(buf1,res1);
	}
	return 1;
}

int parseMessage(char *msg, int length)
{
	string s(msg);

	//Check for a null message
	//If it is null we need to ignore it - otherwise the logic
	//below will fall down
	if (msg[0] == 0)
	{
		AcknowledgeMessage("Message Acknowledge : Message Is Null : Message = " + s);
		return 0;
	}

	addToLog("ListenThread", "Message In - Parsing Message : " + s + ".", true);

	//Had a lot of trouble with this StringSplit function
	//here we use a try catch block to catch any error conditions
	//that may happen without crashing the program
	std::vector<std::string> splitMessage;
	try
	{
		splitMessage = StringSplit(s, " ", false);
	}
	catch(const std::out_of_range& oor)
	{
		std::string what(oor.what());
		addToLog("ListenThread", "String Split Failed: oor=" + what, true);
	}
	catch(const std::exception& ex)
	{
		std::string what(ex.what());
		addToLog("ListenThread", "String Split Failed: ex=" + what, true);
	}
	catch(...)
	{
		cout << "StringSplit Catch: " << endl;
	}

	addToLog("ListenThread", "Message In - String Split Complete", true);

	int f = s.find_first_of(" ");
	string operand( &(s[f+1]) );

	char v[MAX_SERIAL_BUFFER_SIZE];

	std:string MsgOpCode = *splitMessage.begin();

	if(MsgOpCode == VOICE_FUNC_0)
	{
		// voice type (0..8)
		if(splitMessage.size() >= 2)
		{
			v[0] = 'N';
			strcpy(&v[1], splitMessage[1].c_str());
			strcat(v, "\r");
			talk(v);
			usleep(5000);
		}
		// speed (75..600)
		if(splitMessage.size() >= 3)
		{
			v[0] = 'W';
			strcpy(&v[1], splitMessage[2].c_str());
			strcat(v, "\r");
			talk(v);
			usleep(5000);
		}
		// volume (-48..18 db)
		if(splitMessage.size() >= 4)
		{
			v[0] = 'V';
			strcpy(&v[1], splitMessage[3].c_str());
			strcat(v, "\r");
			talk(v);
			usleep(5000);
		}
	}
	else if(*splitMessage.begin() == VOICE_FUNC_1)
	{
		v[0] = 'S';
		strcpy(&v[1], operand.c_str());
		strcat(v, "\r");
		talk(v);
		usleep(5000);
	}
	else if(*splitMessage.begin() == VOICE_FUNC_2)
	{
		//@@@ tbd
	}
	else if(*splitMessage.begin() == VOICE_FUNC_3)
	{
		//@@@ tbd
	}
	else if(*splitMessage.begin() == VOICE_FUNC_4)
	{
		//@@@ tbd
	}
	else if(*splitMessage.begin() == VOICE_FUNC_5)
	{
		// stop speaking
		v[0] = 'X';
		strcpy(&v[1], "\r");
		talk(v);
		usleep(5000);
	}

	else if(*splitMessage.begin() == LEGS_FUNC_0)
	{
		// time to walk - get motor speeds
		if(splitMessage.size() >= 2)
		{
			int8_t motorSpeed_L;
			int8_t motorSpeed_R;
			//Get left motor value
			strcpy(&v[0], splitMessage[1].c_str());		//Put first operand in v at location 0
			strcat(v, "\0");							//Add null to the end to define the end of the string

			motorSpeed_L = atol(v);

			strcpy(&v[0], splitMessage[2].c_str());		//Put first operand in v at location 0
			strcat(v, "\0");							//Add null to the end to define the end of the string

			motorSpeed_R = atol(v);

			legs_thread_ptr->SetDriveSpeeds(motorSpeed_L, motorSpeed_R);
			legs_thread_ptr->SetSem();
		}
	}
	else if(*splitMessage.begin() == LEGS_FUNC_1)
	{
		legs_thread_ptr->SetControlSource(false);
		legs_thread_ptr->SetSem();
	}
	else if(*splitMessage.begin() == LEGS_FUNC_2)
	{
		legs_thread_ptr->SetControlSource(true);
		legs_thread_ptr->SetSem();
	}
	else if(*splitMessage.begin() == LEGS_FUNC_3)
	{
		if(splitMessage.size() >= 2)
		{
			strcpy(&v[0], splitMessage[1].c_str());
			if(v[0] == '1')
			{
				legs_thread_ptr->SetVelocityMonitorEnable(true);
			}
			else
			{
				legs_thread_ptr->SetVelocityMonitorEnable(false);
			}
		}
	}

	else if(*splitMessage.begin() == CUSTOM_FUNC_0)
	{
		clearTextOutputFileSimlink();
		AcknowledgeMessage("Cleared Text Output File Simlink");
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_1)
	{
		setTextOutputFileSimlink(0);
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_2)
	{
		setTextOutputFileSimlink(1);
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_3)
	{
		setTextOutputFileSimlink(2);
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_4)
	{
		setTextOutputFileSimlink(3);
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_5)
	{
		EnableLog_ = false;
		AcknowledgeMessage("Logging Disabled");
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_6)
	{
		EnableLog_ = true;
		AcknowledgeMessage("Logging Enabled");
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_7)
	{
		Mark1_DataBlock_TX_.GPIO_Out &= 0xFD; //Using GPIO out bit 2
		AcknowledgeMessage("Alarm On");
	}
	else if(*splitMessage.begin() == CUSTOM_FUNC_8)
	{
		Mark1_DataBlock_TX_.GPIO_Out |= 0x2; //Using GPIO out bit 2
		AcknowledgeMessage("Alarm Off");
	}

	else if(*splitMessage.begin() == EYE_FUNC_0)
	{
		//Set Camera signal source to remote control
		Mark1_DataBlock_TX_.Control &= 0xFD;
		AcknowledgeMessage("Camera Control Set to Remote");
	}
	else if(*splitMessage.begin() == EYE_FUNC_1)
	{
		//Set camera signal source to software
		Mark1_DataBlock_TX_.Control |= 0x2;
		AcknowledgeMessage("Camera Control Set to Software");
	}
	else if(*splitMessage.begin() == EYE_FUNC_2)
	{
		if(splitMessage.size() >= 2)
		{
			int8_t panPos;
			int8_t tiltPos;
			//Get left motor value
			strcpy(&v[0], splitMessage[1].c_str());		//Put first operand in v at location 0
			strcat(v, "\0");							//Add null to the end to define the end of the string

			panPos = atol(v);

			strcpy(&v[0], splitMessage[2].c_str());		//Put first operand in v at location 0
			strcat(v, "\0");							//Add null to the end to define the end of the string

			tiltPos = atol(v);

			Mark1_DataBlock_TX_.Servo2_Position = panPos;
			Mark1_DataBlock_TX_.Servo3_Position = tiltPos;
			AcknowledgeMessage("New Camera Pan And Tilt Set");
		}
	}


	//usleep(5000);
	addToLog("ListenThread", "Message In - End Parse", true);

	return 1;
}

//Function from: http://stackoverflow.com/questions/10051679/c-tokenize-string
std::vector<std::string> StringSplit(const std::string &source, const char *delimiter = " ", bool keepEmpty = false)
{
    std::vector<std::string> results;

    size_t prev = 0;
    size_t next = 0;

    while ((next = source.find_first_of(delimiter, prev)) != std::string::npos)
    {
        if (keepEmpty || (next - prev != 0))
        {
            results.push_back(source.substr(prev, next - prev));
        }
        prev = next + 1;
    }

    if (prev < source.size())
    {
        results.push_back(source.substr(prev));
    }

    return results;
}

int talk(char *msg)
{
	voice_thread_ptr->SetMsg(msg);
	voice_thread_ptr->SetSem();
	return 1;
}

int addToLog(std::string Source, std::string Content, bool AlsoPrintf)
{
	// current date/time based on current system
	//time_t now = time(0);

	// convert now to string form
	//char* dt = ctime(&now);

	//std::string dateAndTime(dt);
	//dateAndTime.erase(dateAndTime.length()-1, 2);

	//std::string entry = dateAndTime + " : " + Source + " : " + Content + "\r\n";

	try
	{
		if (EnableLog_)
		{
			std::string entry = Source + " : " + Content + "\r\n";

			debugFileStream << entry;
			debugFileStream.flush();

			if(AlsoPrintf)
			{
				std::string s = "Print: " + entry + "\r";
				printf(s.c_str());
			}
		}
	}
	catch(...)
	{
		cout << "AddTooLog Catch: " << endl;
	}

	return 1;
}


int AcknowledgeMessage(std::string AckMessage)
{
	std::string s = "Sending Message Acknowledge : " + AckMessage;
	addToLog("ListenThread", s, true);

	//The following lines were causing a segmentation fault. Why?
	//char *msg = new char[AckMessage.size()];
	//strcpy(msg, AckMessage.c_str());
	//strcpy(&bufResponse[0], &msg);

	strcpy(&bufResponse[0], AckMessage.c_str());
	strcat(bufResponse, "\r");
	strcat(bufResponse, "\n");
	write(fd1,&bufResponse,strlen(bufResponse));

	return 1;

}

