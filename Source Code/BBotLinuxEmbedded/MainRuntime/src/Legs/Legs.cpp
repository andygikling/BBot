/*
 * Legs.cpp
 *
 *  Created on: Aug 27, 2013
 *      Author: lasx
 */



#include "Legs.h"

using namespace std;

void* pstart_Legs(void* ref);

Legs::Legs(int UART1fd, Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX) :
		Mark1_DataBlock_TX_(DataBlock_TX), Mark1_DataBlock_RX_(DataBlock_RX)
{

	sem_init(&sem, 0, 0);

	UARTfd_ = UART1fd;

	debugFileStream.open(PATH_TO_DEBUG_FILE, std::ofstream::out);
	debugFileStream << "VoiceThread: Log Handle Created! (Date/Time)\r\n";
	debugFileStream.flush();

	MotorsSignalSourceSelect_ = false;
	LeftMotorSpeed_ = 0x00;
	RightMotorSpeed_ = 0x00;

	pthread_t thread;
	int e = pthread_create(&thread, NULL, pstart_Legs, (void*)this);

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

    addToLog("LegsThread", "Run Entry Point", true);

	while(1)
	{
		sem_wait(&sem);

		UpdateDataBlock();
		AcknowledgeMessage("LegsThread : Acknowledge");
	}
}

int Legs::UpdateDataBlock()
{
	Mark1_DataBlock_TX_.Control = MotorsSignalSourceSelect_ ? 0x1 : 0x0;
	Mark1_DataBlock_TX_.Servo0_Position = LeftMotorSpeed_;
	Mark1_DataBlock_TX_.Servo1_Position = RightMotorSpeed_;
}


int Legs::AcknowledgeMessage(std::string AckMessage)
{
	addToLog("LegsThread", "Got New Leg Params - Sending Message Acknowledge", true);

	char *msg = new char[AckMessage.size()];
	strcpy(msg, AckMessage.c_str());

	strcpy(&bufResponse_[0], msg);
	strcat(bufResponse_, "\r");
	strcat(bufResponse_, "\n");
	write(UARTfd_,&bufResponse_,strlen(bufResponse_));

	return 1;

}


int Legs::addToLog(std::string Source, std::string Content, bool AlsoPrintf)
{
	std::string entry = Source + " : " + Content + "\r\n";
	debugFileStream << entry;
	debugFileStream.flush();

	if(AlsoPrintf)
	{
		std::string s = "Print: " + entry + "\r";
		char *c = new char[s.size()];
		strcpy(c, s.c_str());
		printf(c);
	}

	return 1;
}



