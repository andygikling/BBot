/*
 * Voice.h
 *
 *  Created on: Aug 27, 2013
 *      Author: lasx
 */

#ifndef VOICE_H_
#define VOICE_H_

#include <string>
#include <cstring>
#include <stdlib.h>
#include <fstream>
#include <fcntl.h>
#include <unistd.h>
#include <semaphore.h>
#include <pthread.h>
#include <termios.h>

#include <BBoneConstants.h>

class Voice {
	friend void* pstart_Voice(void*);

private:
	sem_t sem;

	Mark1_DataBlock_TX Mark1_DataBlock_TX_;
	Mark1_DataBlock_RX Mark1_DataBlock_RX_;

	std::string MessageIn;

	int UARTfd_;
	int fd4_, c4_, res4_;
	struct termios oldtio4_, newtio4_;
	char buf4_[MAX_SERIAL_BUFFER_SIZE];
	char bufResponse_[MAX_SERIAL_BUFFER_SIZE];

	int WriteToVoiceModule(std::string Message);
	int AcknowledgeMessage(std::string AckMessage);
	int addToLog(std::string Source, std::string Content, bool AlsoPrintf);

	int CreateLogOutputFile();

	std::ofstream debugFileStream;

	int initializeUART4();
	void Run();

public:
	Voice(int UARTfd, Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX);
	virtual ~Voice();
	int SetMsg(std::string msg);
	int SetSem();

};

#endif /* VOICE_H_ */
