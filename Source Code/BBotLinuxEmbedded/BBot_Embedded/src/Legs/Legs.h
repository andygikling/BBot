/*
 * Legs.h
 *
 *  Created on: Aug 27, 2013
 *      Author: lasx
 */


/***********************************************************************
 * This header file contains the mcp3008Spi class definition.
 * Its main purpose is to communicate with the MCP3008 chip using
 * the userspace spidev facility.
 * The class contains four variables:
 * mode        -> defines the SPI mode used. In our case it is SPI_MODE_0.
 * bitsPerWord -> defines the bit width of the data transmitted.
 *        This is normally 8. Experimentation with other values
 *        didn't work for me
 * speed       -> Bus speed or SPI clock frequency. According to
 *                https://projects.drogon.net/understanding-spi-on-the-raspberry-pi/
 *            It can be only 0.5, 1, 2, 4, 8, 16, 32 MHz.
 *                Will use 1MHz for now and test it further.
 * spifd       -> file descriptor for the SPI device
 *
 * The class contains two constructors that initialize the above
 * variables and then open the appropriate spidev device using spiOpen().
 * The class contains one destructor that automatically closes the spidev
 * device when object is destroyed by calling spiClose().
 * The spiWriteRead() function sends the data "data" of length "length"
 * to the spidevice and at the same time receives data of the same length.
 * Resulting data is stored in the "data" variable after the function call.
 * ****************************************************************************/


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
