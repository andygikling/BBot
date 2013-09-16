/*
 * Mark1FPGA.cpp
 *
 *  Created on: Sep 4, 2013
 *      Author: lasx
 */

#include "Mark1FPGA.h"

using namespace std;

void* pstart_Mark1FPGA(void* ref);

double Mark1FPGA::VelocityLeft = 0.0;
double Mark1FPGA::VelocityRight = 0.0;

Mark1FPGA::Mark1FPGA(Mark1_DataBlock_TX &DataBlock_TX, Mark1_DataBlock_RX &DataBlock_RX) :
		Mark1_DataBlock_TX_(DataBlock_TX), Mark1_DataBlock_RX_(DataBlock_RX),
		previousEncoderCount_Left_(0), previousEncoderCount_Right_(0),
		legsEncoderConvert_cts_per_mm_(5.13306)
{
	sem_init(&sem, 0, 0);

	CreateLogOutputFile();

	pthread_t thread;
	int e = pthread_create(&thread, NULL, pstart_Mark1FPGA, (void*)this);
}

Mark1FPGA::~Mark1FPGA() {
	this->spiClose();
}

// thread entry point
void* pstart_Mark1FPGA(void* ref) {
	Mark1FPGA* v = (Mark1FPGA*)(ref);
	v->Run();
	return (void*)0;
}

// object's entry point
void Mark1FPGA::Run()
{
	addToLog("Mark1FPGAThread", "Main Loop Entry", true);

	this->mode = SPI_MODE_0 ;
	this->bitsPerWord = SPI_BITS_PER_WORD;
	this->speed = SPI_BIT_RATE_HZ;
	this->spifd = -1;

	this->spiOpen(SPI_PATH_TO_SPI_DEVICE);

	unsigned char Data[128];
	void *Data_ptr;
	Data_ptr = &Data;

	MainLoopCount_ = 0;

	while(1)
	{

		usleep(SPI_MARK1_DATABLOCK_UPDATE_RATE_US);

		//Copy data block into local array
		//The local array is what's sent out the door to the Mark1
		//It is also overwritten with the new SPI data in from the Mark1
		memcpy(Data_ptr, &Mark1_DataBlock_TX_, SPI_BLOCK_TRANSFER_NUM_BYTES);
		spiWriteRead( (unsigned char*)Data_ptr, SPI_BLOCK_TRANSFER_NUM_BYTES);

		//Get data block from FPGA
		memcpy(&Mark1_DataBlock_RX_, Data_ptr, SPI_BLOCK_TRANSFER_NUM_BYTES);

		u_int32_t SPI_Interval_ = Mark1_DataBlock_RX_.SPIExchangeInterval;
		u_int32_t currentEncoderCount_Left_ = Mark1_DataBlock_RX_.EncoderLeft_Count;
		u_int32_t currentEncoderCount_Right_ = Mark1_DataBlock_RX_.EncoderRight_Count;

		//Every so often sample the wheel velocity
		if(MainLoopCount_ == SPI_MAIN_LOOP_COUNT_TARGET)
		{
			//Debug
			std::string s1 = std::to_string(SPI_Interval_);
			std::string s2 = std::to_string(currentEncoderCount_Left_);
			std::string s3 = std::to_string(currentEncoderCount_Right_);

			double distanceTraveled_Left_mm = 0;
			double distanceTraveled_Right_mm = 0;
			double velocity_Left, velocity_Right = 0;
			double target = SPI_MAIN_LOOP_COUNT_TARGET;

			//Calc average velocity over MainLoopCount_ == SPI_MAIN_LOOP_COUNT_TARGET time interval
			if(previousEncoderCount_Left_ != 0)
			{
				distanceTraveled_Left_mm = (currentEncoderCount_Left_ - previousEncoderCount_Left_) / legsEncoderConvert_cts_per_mm_;
				double temp1 = distanceTraveled_Left_mm / (target / 100.0);
				velocity_Left = temp1;
			}

			if(previousEncoderCount_Right_ != 0)
			{
				distanceTraveled_Right_mm = (currentEncoderCount_Right_ - previousEncoderCount_Right_) / legsEncoderConvert_cts_per_mm_;
				double temp2 = distanceTraveled_Right_mm / (target / 100.0);
				velocity_Right = temp2;
			}

			previousEncoderCount_Left_ = currentEncoderCount_Left_;
			previousEncoderCount_Right_ = currentEncoderCount_Right_;

			//Set global velocity so the LegsThread can read it and send back to the GUI
			Mark1FPGA::VelocityLeft = velocity_Left;
			Mark1FPGA::VelocityRight = velocity_Right;

			MainLoopCount_ = 0;
		}

		MainLoopCount_++;
	}
}


/**********************************************************
 * spiOpen() :function is called by the constructor.
 * It is responsible for opening the spidev device
 * "devspi" and then setting up the spidev interface.
 * private member variables are used to configure spidev.
 * They must be set appropriately by constructor before calling
 * this function.
 * *********************************************************/
int Mark1FPGA::spiOpen(std::string devspi){
    int statusVal = -1;
    this->spifd = open(devspi.c_str(), O_RDWR);
    if(this->spifd < 0){
        perror("could not open SPI device");
        addToLog("Mark1FPGAThread", "CouldNotOpenSPIDevice", true);
        exit(1);
    }

    statusVal = ioctl (this->spifd, SPI_IOC_WR_MODE, &(this->mode));
    if(statusVal < 0){
        perror("Could not set SPIMode (WR)...ioctl fail");
        addToLog("Mark1FPGAThread", "CouldNotSetSPIMode (WR)", true);
        exit(1);
    }

    statusVal = ioctl (this->spifd, SPI_IOC_RD_MODE, &(this->mode));
    if(statusVal < 0) {
      perror("Could not set SPIMode (RD)...ioctl fail");
      addToLog("Mark1FPGAThread", "CouldNotSetSPIMode (RD)", true);
      exit(1);
    }

    statusVal = ioctl (this->spifd, SPI_IOC_WR_BITS_PER_WORD, &(this->bitsPerWord));
    if(statusVal < 0) {
      perror("Could not set SPI bitsPerWord (WR)...ioctl fail");
      addToLog("Mark1FPGAThread", "CouldNotSetBitsPerWord (WR)", true);
      exit(1);
    }

    statusVal = ioctl (this->spifd, SPI_IOC_RD_BITS_PER_WORD, &(this->bitsPerWord));
    if(statusVal < 0) {
      perror("Could not set SPI bitsPerWord(RD)...ioctl fail");
      addToLog("Mark1FPGAThread", "CouldNotSetBitsPerWord (RD)", true);
      exit(1);
    }

    statusVal = ioctl (this->spifd, SPI_IOC_WR_MAX_SPEED_HZ, &(this->speed));
    if(statusVal < 0) {
      perror("Could not set SPI speed (WR)...ioctl fail");
      addToLog("Mark1FPGAThread", "CouldNotSetSpeed (WR)", true);
      exit(1);
    }

    statusVal = ioctl (this->spifd, SPI_IOC_RD_MAX_SPEED_HZ, &(this->speed));
    if(statusVal < 0) {
      perror("Could not set SPI speed (RD)...ioctl fail");
      addToLog("Mark1FPGAThread", "CouldNotSetSpeed (RD)", true);
      exit(1);
    }
    return statusVal;
}

/***********************************************************
 * spiClose(): Responsible for closing the spidev interface.
 * Called in destructor
 * *********************************************************/

int Mark1FPGA::spiClose(){
    int statusVal = -1;
    statusVal = close(this->spifd);
        if(statusVal < 0) {
      perror("Could not close SPI device");
      exit(1);
    }
    return statusVal;
}

/********************************************************************
 * This function writes data "data" of length "length" to the spidev
 * device. Data shifted in from the spidev device is saved back into
 * "data".
 * ******************************************************************/
int Mark1FPGA::spiWriteRead( unsigned char *data, int length){

  struct spi_ioc_transfer spi[length];
  int i = 0;
  int retVal = -1;

// one spi transfer for each byte

  for (i = 0 ; i < length ; i++){

    spi[i].tx_buf        = (unsigned long)(data + i); // transmit from "data"
    spi[i].rx_buf        = (unsigned long)(data + i) ; // receive into "data"
    spi[i].len           = sizeof(*(data + i)) ;
    spi[i].delay_usecs   = 0 ;
    spi[i].speed_hz      = this->speed ;
    spi[i].bits_per_word = this->bitsPerWord ;
    spi[i].cs_change = 0;						//Set to 0 and chip select will assert at the
    											//beginning of length bytes and de-assert at the end
    											//Set to 1 and chip select will assert and de-assert
    											//between each byte sent...
}

 retVal = ioctl (this->spifd, SPI_IOC_MESSAGE(length), &spi) ;

 if(retVal < 0){
    perror("Problem transmitting spi data..ioctl");
    exit(1);
 }

return retVal;

}

int Mark1FPGA::addToLog(std::string Source, std::string Content, bool AlsoPrintf)
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

int Mark1FPGA::CreateLogOutputFile()
{
	ifstream logFile(PATH_TO_MARK1FPGA_THREAD_DEBUG_FILE);
	if(logFile.good())
	{
		//File exists - kill it
		logFile.close();
		unlink(PATH_TO_MARK1FPGA_THREAD_DEBUG_FILE);
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

	std::string entry = "BBot Mark1FPGA Thread Log File Created! " + dateAndTime + "\r\n";

	debugFileStream.open(PATH_TO_MARK1FPGA_THREAD_DEBUG_FILE, std::ofstream::out);
	debugFileStream << entry;
	debugFileStream.flush();
	return 1;
}

