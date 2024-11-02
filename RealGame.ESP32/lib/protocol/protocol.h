#ifndef PROTOCOL_H
#define PROTOCOL_H

#include <stdint.h>   // for uint8_t, uint64_t, etc.
#include <string.h>   // for string operations, if needed
#include <stdbool.h>  // for bool type if needed on certain platforms

// Define the Command struct
struct Command
{
    bool isHigh;
    bool isOutput;
    uint8_t pin;
} __attribute__((packed));

// Define the MessageType enum
enum MessageType {
    BOOL = 0,
    STRING = 1,
    INT = 2,
    FLOAT = 3,
    DOUBLE = 4,
    BYTE = 5,
    JSON = 6,
    STREAM = 7,
    ESP_COMMAND = 8,
    APPROVAL_SIGNAL = 9,
    ERROR_SIGNAL = 10,
    PING = 11,
};

// Define a type alias for MessageDataType
typedef uint8_t MessageDataType;

// Define the Message struct
struct Message
{
    MessageDataType type;
    uint64_t length;
    char* data;
} __attribute__((packed));

// Function prototypes for creating messages
Message MakeStringMessage(char* data);
Message MakeIntMessage(int data);
Message MakeFloatMessage(float data);
Message MakeDoubleMessage(double data);
Message MakeByteMessage(uint8_t data);
Message MakeJsonMessage(char* data);
Message MakeStreamMessage(char* data);
Message MakeEspCommandMessage(Command data);
Message MakeApprovalSignalMessage();
Message MakeErrorSignalMessage(char* data);
Message MakePingMessage();
#endif // PROTOCOL_H
