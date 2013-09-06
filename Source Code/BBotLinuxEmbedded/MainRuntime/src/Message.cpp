/*
 * Message.cpp
 *
 *  Created on: Jul 4, 2013
 *      Author: Andy Gikling
 */

#include "Message.h"

namespace BBot {

Message::Message() {
	msg = std::string("null");
}

Message::Message(std::string s) {
	msg = s;
}

void Message::Write(std::string s)
{
	msg = s;
}

std::string Message::Read()
{
	return msg;
}

void Message::Append(std::string s)
{
	msg.append(s);
}

Message::~Message() {
	// TODO Auto-generated destructor stub
}

} /* namespace BBot */
