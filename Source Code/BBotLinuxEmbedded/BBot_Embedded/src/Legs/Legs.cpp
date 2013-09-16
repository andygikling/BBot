/*
 * Legs.cpp
 *
 *  Created on: Aug 27, 2013
 *      Author: lasx
 */



#include "Legs.h"
//Need to access the public velocity values
#include "../Mark1FPGA/Mark1FPGA.h"

using namespace std;

void* pstart_Legs(void* ref);
void* pstart_LegsVelocityMonitor(void* ref);

Legs::Legs(int UART1fd, Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX) :
		Mark1_DataBlock_TX_(DataBlock_TX), Mark1_DataBlock_RX_(DataBlock_RX), VelocityMonitorEnable_(false)
{
	sem_init(&sem, 0, 0);

	UARTfd_ = UART1fd;

	CreateLogOutputFile();

	MotorsSignalSourceSelect_ = false;
	LeftMotorSpeed_ = 0x00;
	RightMotorSpeed_ = 0x00;

	pthread_t thread;
	int err1 = pthread_create(&thread, NULL, pstart_Legs, (void*)this);
	int err2 = pthread_create(&thread, NULL, pstart_LegsVelocityMonitor, (void*)this);
}


Legs::~Legs() {
	// TODO Auto-generated destructor stub
	//tcsetattr(fd4_,TCSANOW,&oldtio4_);

}


// thread entry point
void* pstart_Legs(void* ref) {
	Legs* v = (Legs*)(ref);
	v->Run();
	return (void*)0;
}

// thread entry point
void* pstart_LegsVelocityMonitor(void* ref) {
	Legs* v = (Legs*)(ref);
	v->RunVelocityMonitor();
	return (void*)0;
}

int Legs::SetSem()
{
	 sem_post(&sem);
	 return 1;
}

int Legs::SetDriveSpeeds(int8_t LeftMotorSpeed, int8_t RightMotorSpeed)
{
	LeftMotorSpeed_ = LeftMotorSpeed;
	RightMotorSpeed_ = RightMotorSpeed;
}

int Legs::SetControlSource(bool Source)
{
	MotorsSignalSourceSelect_ = Source;
}

// object's entry point
void Legs::Run()
{

    AddToLog("LegsThread", "Main Loop Entry", true);

	while(1)
	{
		sem_wait(&sem);

		UpdateDataBlock();
		AcknowledgeMessage("Received New Motor Parameters");
	}
}

void Legs::RunVelocityMonitor()
{
	while(true)
	{
		if(VelocityMonitorEnable_)
		{
			double vL = Mark1FPGA::VelocityLeft;
			double vR = Mark1FPGA::VelocityRight;
			std::ostringstream s;
			s << "Velocity " << vL << " " << vR;
			string s1(s.str());
			AcknowledgeMessage(s1);
		}

		usleep(VELOCITY_TRANSMIT_INTERVAL_US);
	}
}

void Legs::SetVelocityMonitorEnable(bool Enable)
{
	VelocityMonitorEnable_ = Enable;
}

int Legs::UpdateDataBlock()
{
	if (MotorsSignalSourceSelect_)
	{
		Mark1_DataBlock_TX_.Control |= 0x1;
	}
	else
	{
		Mark1_DataBlock_TX_.Control &= 0xFE;
	}

	Mark1_DataBlock_TX_.Servo0_Position = LeftMotorSpeed_;
	Mark1_DataBlock_TX_.Servo1_Position = RightMotorSpeed_;
}


int Legs::AcknowledgeMessage(std::string AckMessage)
{
	try
	{
		AddToLog("LegsThread", AckMessage, true);
		string s = "LegsThread : " + AckMessage;

		strcpy(&bufResponse_[0], s.c_str());
		strcat(bufResponse_, "\r");
		strcat(bufResponse_, "\n");
		write(UARTfd_,&bufResponse_,strlen(bufResponse_));

	}
	catch(...)
	{
		printf("LegsThread Ack Message Error");
	}

	return 1;
}


int Legs::AddToLog(std::string Source, std::string Content, bool AlsoPrintf)
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

int Legs::CreateLogOutputFile()
{
	ifstream logFile(PATH_TO_LEGS_THREAD_DEBUG_FILE);
	if(logFile.good())
	{
		//File exists - kill it
		logFile.close();
		unlink(PATH_TO_LEGS_THREAD_DEBUG_FILE);
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

	std::string entry = "BBot Legs Thread Log File Created!" + dateAndTime + "\r\n";

	debugFileStream.open(PATH_TO_LEGS_THREAD_DEBUG_FILE, std::ofstream::out);
	debugFileStream << entry;
	debugFileStream.flush();
	return 1;
}

