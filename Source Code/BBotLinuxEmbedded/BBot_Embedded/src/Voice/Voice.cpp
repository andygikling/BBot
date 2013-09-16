/*
 * Voice.cpp
 *
 *  Created on: Aug 27, 2013
 *      Author: lasx
 */

#include "Voice.h"

using namespace std;

void* pstart_Voice(void* ref);

Voice::Voice(int UARTfd, Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX):
	MessageIn("Hi"), fd4_(0), UARTfd_(0)
{

	//Get a local pointer to our data block
	Mark1_DataBlock_TX_ = DataBlock_TX;
	Mark1_DataBlock_RX_ = DataBlock_RX;

	sem_init(&sem, 0, 0);

	UARTfd_ = UARTfd;

	CreateLogOutputFile();

	initializeUART4();

	pthread_t thread;
	int e = pthread_create(&thread, NULL, pstart_Voice, (void*)this);

}

Voice::~Voice() {
	// TODO Auto-generated destructor stub
	//tcsetattr(fd4_,TCSANOW,&oldtio4_);
}


int Voice::SetMsg(std::string msg)
{
	MessageIn = msg;
	return 1;
}

int Voice::SetSem()
{
	 sem_post(&sem);
	 return 1;
}

// thread entry point
void* pstart_Voice(void* ref) {
	Voice* v = (Voice*)(ref);
	v->Run();
	return (void*)0;
}

// object's entry point
void Voice::Run()
{

	addToLog("VoiceThread", "Main Loop Entry", true);

	while(1)
	{

		sem_wait(&sem);

		//If we want to hear a response form the device,
		//make set a local bool to true called busy
		//then do the next line:

		//**simply we just do this for BBot
		//Grab msg input and send to UART4


		//then wait for response (block)
		//got response,

		//set busy back to false

		//make another method that "querys" busy" from the main loop
		//then when busy is false, that method returns, not 0 but the response message.

		WriteToVoiceModule(MessageIn);
		AcknowledgeMessage("VoiceThread : Acknowledge");



	}
}

int Voice::WriteToVoiceModule(std::string Message)
{
	//Change string to char *
	addToLog("VoiceThread", "Sending Message To Voice Module", true);
	char *msg = new char[Message.size()+1]; //+1 for the null terminator
	strcpy(msg, Message.c_str());

	//Use digital IO to set the signal's route in the FPGA
	//gpio_set_value(UART_SELECT_0_GPIO_PIN_NUM, HIGH);
	//gpio_set_value(UART_SELECT_1_GPIO_PIN_NUM, LOW);
	//gpio_set_value(UART_SELECT_2_GPIO_PIN_NUM, LOW);

	usleep(100);
	write(fd4_, msg, strlen(msg));

	//Use digital IO to set the signal's route in the FPGA
	//gpio_set_value(UART_SELECT_0_GPIO_PIN_NUM, LOW);
	//gpio_set_value(UART_SELECT_1_GPIO_PIN_NUM, LOW);
	//gpio_set_value(UART_SELECT_2_GPIO_PIN_NUM, LOW);
	return 1;

}

int Voice::AcknowledgeMessage(std::string AckMessage)
{
	addToLog("VoiceThread", "Sending Message Acknowledge", true);

	char *msg = new char[AckMessage.size()];
	strcpy(msg, AckMessage.c_str());

	strcpy(&bufResponse_[0], msg);
	strcat(bufResponse_, "\r");
	strcat(bufResponse_, "\n");
	write(UARTfd_,&bufResponse_,strlen(bufResponse_));

	return 1;

}

int Voice::initializeUART4()
{
	addToLog("VoiceThread", "Initialize UART4", true);

	fd4_ = open(MODEMDEVICE_UART4, O_RDWR | O_NOCTTY );
	if (fd4_ <0) {perror(MODEMDEVICE_UART4); exit(-1); }

	tcgetattr(fd4_,&oldtio4_); /* save current serial port settings */
	bzero(&newtio4_, sizeof(newtio4_)); /* clear struct for new port settings */

	/*
	  BAUDRATE: Set bps rate. You could also use cfsetispeed and cfsetospeed.
	  CRTSCTS : output hardware flow control (only used if the cable has
				all necessary lines. See sect. 7 of Serial-HOWTO)
	  CS8     : 8n1 (8bit,no parity,1 stopbit)
	  CLOCAL  : local connection, no modem contol
	  CREAD   : enable receiving characters
	*/
	newtio4_.c_cflag = BAUDRATE_UART4 | CRTSCTS | CS8 | CLOCAL | CREAD;

	/*
	  IGNPAR  : ignore bytes with parity errors
	  ICRNL   : map CR to NL (otherwise a CR input on the other computer
				will not terminate input)
	  otherwise make device raw (no other input processing)
	*/
	newtio4_.c_iflag = IGNPAR | ICRNL;

	/*
	 Raw output.
	*/
	newtio4_.c_oflag = 0;

	/*
	  ICANON  : enable canonical input
	  disable all echo functionality, and don't send signals to calling program
	*/
	newtio4_.c_lflag = ICANON;

	/*
	  initialize all control characters
	  default values can be found in /usr/include/termios.h, and are given
	  in the comments, but we don't need them here
	*/
	newtio4_.c_cc[VINTR]    = 0;     /* Ctrl-c */
	newtio4_.c_cc[VQUIT]    = 0;     /* Ctrl-\ */
	newtio4_.c_cc[VERASE]   = 0;     /* del */
	newtio4_.c_cc[VKILL]    = 0;     /* @ */
	newtio4_.c_cc[VEOF]     = 4;     /* Ctrl-d */
	newtio4_.c_cc[VTIME]    = 0;     /* inter-character timer unused */
	newtio4_.c_cc[VMIN]     = 1;     /* blocking read until 1 character arrives */
	newtio4_.c_cc[VSWTC]    = 0;     /* '\0' */
	newtio4_.c_cc[VSTART]   = 0;     /* Ctrl-q */
	newtio4_.c_cc[VSTOP]    = 0;     /* Ctrl-s */
	newtio4_.c_cc[VSUSP]    = 0;     /* Ctrl-z */
	newtio4_.c_cc[VEOL]     = 0;     /* '\0' */
	newtio4_.c_cc[VREPRINT] = 0;     /* Ctrl-r */
	newtio4_.c_cc[VDISCARD] = 0;     /* Ctrl-u */
	newtio4_.c_cc[VWERASE]  = 0;     /* Ctrl-w */
	newtio4_.c_cc[VLNEXT]   = 0;     /* Ctrl-v */
	newtio4_.c_cc[VEOL2]    = 0;     /* '\0' */

	/*
	  now clean the modem line and activate the settings for the port
	*/
	tcflush(fd4_, TCIFLUSH);
	tcsetattr(fd4_,TCSANOW,&newtio4_);

	return 1;
}

int Voice::addToLog(std::string Source, std::string Content, bool AlsoPrintf)
{
	std::string entry = Source + " : " + Content + "\r\n";
	debugFileStream << entry;
	debugFileStream.flush();

	if(AlsoPrintf)
	{
		std::string s = "Print: " + entry + "\r";
		printf(s.c_str());
	}

	return 1;
}

int Voice::CreateLogOutputFile()
{
	ifstream logFile(PATH_TO_VOICE_THREAD_DEBUG_FILE);
	if(logFile.good())
	{
		//File exists - kill it
		logFile.close();
		unlink(PATH_TO_VOICE_THREAD_DEBUG_FILE);
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

	std::string entry = "BBot Voice Thread Log File Created!" + dateAndTime + "\r\n";


	debugFileStream.open(PATH_TO_VOICE_THREAD_DEBUG_FILE, std::ofstream::out);
	debugFileStream << entry;
	debugFileStream.flush();
	return 1;
}






