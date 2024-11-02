#include <protocol.h>
#include <connection.h>
#include <chrono>

ConnectionManager connection;


#define WIFI_SSID "WIFI_SSID"
#define WIFI_PASS "WIFI_PASSWORD"

#define TCP_HOST "192.168.1.175" // Server IP
#define TCP_PORT 5555            // Server port
#define SEND_PING true           // Send ping to server every PING_INTERVAL to keep connection alive
#define PING_INTERVAL 5000       // Ping interval in milliseconds

void MessageReceived(Message &msg);
void ExecuteCommand(Command *command);

void setup()
{
  Serial.begin(115200);
  if(connection.ConnectWifi(WIFI_SSID, WIFI_PASS)){
    if(connection.ConnectServer(TCP_HOST, TCP_PORT)){
      Serial.println("Connected to server");
    }
  }
  else{
    Serial.println("Connection failed");
  }
}


std::chrono::time_point<std::chrono::system_clock> lastPing = std::chrono::system_clock::now();


void Ping(){
  if(SEND_PING && std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - lastPing).count() > PING_INTERVAL){
    connection.SendMessage(MakePingMessage());
    lastPing = std::chrono::system_clock::now();
  }
}

void loop()
{
  
  auto client = connection.client;
  if (client.connected())
  {
    Ping(); // Send ping to server to keep connection alive if SEND_PING is true and PING_INTERVAL has passed
    if (client.available()) // Check if there is data available to read
    {
      // Read header (type and length) of message
      uint8_t msg_type[1];
      client.read(msg_type, 1);
      uint64_t msg_length;
      client.read((uint8_t *)&msg_length, sizeof(msg_length));

      // Reserve memory in heap for data
      char *Data = new char[msg_length];
      client.read((uint8_t *)Data, msg_length);

      // Create message object
      Message msg;
      msg.type = static_cast<MessageDataType>(msg_type[0]);
      msg.length = msg_length;
      msg.data = Data;

      MessageReceived(msg);
    }
  }
}

void MessageReceived(Message &msg)
{
  try{

  if (msg.type == (uint8_t)MessageType::ESP_COMMAND) // ESP Command
  {
    Command *command = (Command *)msg.data;
    ExecuteCommand(command);
  }
  else
  {
    Serial.println("Message type not handled: " + String(msg.type));
  }
  free(msg.data);                 // Free memory
  }
  catch(const std::runtime_error& ex){
    char* error = (char*)ex.what();
    connection.SendMessage(MakeErrorSignalMessage(error));
  }

  connection.SendMessage(MakeApprovalSignalMessage()); // Send approval signal
}

void ExecuteCommand(Command *command)
{
  if (command->isOutput) // Set pin as output
  {
    uint8_t pin = command->pin;
    uint8_t state = command->isHigh ? HIGH : LOW;
    pinMode(pin, OUTPUT);     // Set pin as output
    digitalWrite(pin, state); // Set pin state to high or low
    Serial.println("Executed command: { pin[OUTPUT]: " + String(pin) + ", state: " + String(state) + " [LOW=>0, HIGH=>1] }");
  }
  else // Set pin as input
  {
    pinMode(command->pin, INPUT);                                  // Set pin as input
    Message readValue = MakeIntMessage(digitalRead(command->pin)); // Read pin value and create message
    connection.SendMessage(readValue);                                        // Send message to server
    Serial.println("Executed command: { pin[INPUT]: " + String(command->pin) + ", state: " + String(digitalRead(command->pin)) + " }");
  }
}
