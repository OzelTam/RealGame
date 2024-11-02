#include <stdio.h>
#include <protocol.h>
#include <cstring>



Message MakeStringMessage(char* data)
{
    Message msg;
    msg.type = MessageType::STRING;
    msg.length = strlen(data) + 1;
    msg.data = data;
    return msg;
}

Message MakeIntMessage(int data)
{
    Message msg;
    msg.type = MessageType::INT;
    msg.length = sizeof(data);
    msg.data = (char *)&data;
    return msg;
}

Message MakeFloatMessage(float data)
{
    Message msg;
    msg.type = MessageType::FLOAT;
    msg.length = sizeof(data);
    msg.data = (char *)&data;
    return msg;
}

Message MakeDoubleMessage(double data)
{
    Message msg;
    msg.type = MessageType::DOUBLE;
    msg.length = sizeof(data);
    msg.data = (char *)&data;
    return msg;
}

Message MakeByteMessage(uint8_t data)
{
    Message msg;
    msg.type = MessageType::BYTE;
    msg.length = sizeof(data);
    msg.data = (char *)&data;
    return msg;
}

Message MakeJsonMessage(char* data)
{
    Message msg;
    msg.type = MessageType::JSON;
    msg.length = strlen(data) + 1;
    msg.data = data;
    return msg;
}

Message MakeStreamMessage(char* data)
{
    Message msg;
    msg.type = MessageType::STREAM;
    msg.length = strlen(data) + 1;
    msg.data = data;
    return msg;
}

Message MakeEspCommandMessage(Command data)
{
    Message msg;
    msg.type = MessageType::ESP_COMMAND;
    msg.length = sizeof(data);
    msg.data = (char *)&data;
    return msg;
}

Message MakeApprovalSignalMessage()
{
    Message msg;
    msg.type = MessageType::APPROVAL_SIGNAL;
    msg.length = 0;
    msg.data = nullptr;
    return msg;
}

Message MakeErrorSignalMessage(char* data)
{
    Message msg;
    msg.type = MessageType::ERROR_SIGNAL;
    msg.length = strlen(data) + 1;
    msg.data = data;
    return msg;
}



Message MakePingMessage()
{
    Message msg;
    msg.type = MessageType::PING;
    msg.length = 0;
    msg.data = nullptr;
    return msg;
}