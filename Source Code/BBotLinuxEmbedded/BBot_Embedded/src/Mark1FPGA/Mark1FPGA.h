/*
 * Mark1FPGA.h
 *
 *  Created on: Sep 4, 2013
 *      Author: lasx
 */

#ifndef MARK1FPGA_H_
#define MARK1FPGA_H_

#include <string>
#include <fstream>
#include <iostream>
#include <fcntl.h>
#include <cstring>
#include <stdlib.h>
#include <stdio.h>
#include <semaphore.h>
#include <pthread.h>
#include <sys/ioctl.h>
#include <linux/spi/spidev.h>
#include <cmath>

#include <BBoneConstants.h>

class Mark1FPGA {
	friend void* pstart_Mark1FPGA(void*);

private:
	sem_t sem;
	void Run();

	Mark1_DataBlock_TX &Mark1_DataBlock_TX_;
	Mark1_DataBlock_RX &Mark1_DataBlock_RX_;

	unsigned char mode;
	unsigned char bitsPerWord;
	unsigned int speed;
	int spifd;
	int MainLoopCount_;

	u_int32_t previousEncoderCount_Left_;
	u_int32_t previousEncoderCount_Right_;

	double legsEncoderConvert_cts_per_mm_;

	int addToLog(std::string Source, std::string Content, bool AlsoPrintf);

	int CreateLogOutputFile();

	std::ofstream debugFileStream;

	int spiOpen(std::string devspi);
	int spiClose();


public:
	Mark1FPGA(Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX);
	virtual ~Mark1FPGA();

	int spiWriteRead( unsigned char *data, int length);



};

#endif /* MARK1FPGA_H_ */
