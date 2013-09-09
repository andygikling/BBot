/*
 * Legs.h
 *
 *  Created on: Aug 27, 2013
 *      Author: lasx
 */


#ifndef LEGS_H_
#define LEGS_H_

#include <string>
#include <cstring>
#include <stdlib.h>
#include <fstream>
#include <fcntl.h>
#include <unistd.h>
#include <semaphore.h>
#include <pthread.h>


#include <BBoneConstants.h>

class Legs {
	friend void* pstart_Legs(void*);

private:

	sem_t sem;

	int UARTfd_;
	char bufResponse_[MAX_SERIAL_BUFFER_SIZE];

	Mark1_DataBlock_TX &Mark1_DataBlock_TX_;
	Mark1_DataBlock_RX &Mark1_DataBlock_RX_;

	std::ofstream debugFileStream;

	int AddToLog(std::string Source, std::string Content, bool AlsoPrintf);

	bool MotorsSignalSourceSelect_;
	int8_t LeftMotorSpeed_;
	int8_t RightMotorSpeed_;
	int32_t EncoderCountLeft_;
	int32_t EncoderCountRight_;

	int AcknowledgeMessage(std::string AckMessage);

	int UpdateDataBlock();

	int CreateLogOutputFile();

	void Run();


public:
	Legs(int UART1fd, Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX);
	virtual ~Legs();

	int SetDriveSpeeds(int8_t LeftMotorSpeed, int8_t RightMotorSpeed);
	int SetControlSource(bool Source);
	int SetSem();


public:

    int spiWriteRead( unsigned char *data, int length);

private:
    unsigned char mode;
    unsigned char bitsPerWord;
    unsigned int speed;
    int spifd;

    int spiOpen(std::string devspi);
    int spiClose();

};

#endif /* LEGS_H_ */
