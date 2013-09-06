/*
 * Message.h
 *
 *  Created on: Jul 4, 2013
 *      Author: Andy Gikling
 */

#ifndef MESSAGE_H_
#define MESSAGE_H_


namespace BBot {

#include <stdio.h>
#include <string.h>

class Message {
public:
	Message();
	Message(std::string s);
	void Write(std::string s);
	std::string Read();
	void Append(std::string s);

	virtual ~Message();

private:
	std::string msg;

};

} /* namespace BBot */
#endif /* MESSAGE_H_ */
