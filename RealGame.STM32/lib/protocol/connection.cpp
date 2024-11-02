#include <connection.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <protocol.h>


ConnectionManager::ConnectionManager() {}

bool ConnectionManager::ConnectWifi(const char* ssid, const char* password) {
    WiFi.begin(ssid, password);
    Serial.print("Connecting to WiFi...");
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println("Connected to WiFi");
    return true;
}

bool ConnectionManager::ConnectServer(const char* host, uint16_t port) {
    Serial.print("Connecting to server...");
    if (client.connect(host, port)) {
        Serial.println("Connected to server");
        return true;
    } else {
        Serial.println("Connection to server failed");
        return false;
    }
}

bool ConnectionManager::SendMessage(Message msg) {
    if (client.connected()) {
        client.write((uint8_t *)&msg, sizeof(msg.type) + sizeof(msg.length));
        client.write((uint8_t *)msg.data, msg.length);
        if(msg.type == MessageType::PING){
            Serial.println("SENT: PING");
        }
        else if (msg.type == MessageType::APPROVAL_SIGNAL){
            Serial.println("SENT: APPROVAL_SIGNAL");
        }
        else if (msg.type == MessageType::ERROR_SIGNAL){
            Serial.println("SENT: ERROR_SIGNAL");
        }
        else{
            Serial.println("SENT: \n{ type: " + String(msg.type) + ", length: " + String(msg.length) + ", data: " + String(msg.data) + " }");
        }
        return true;
    } else {
        Serial.println("Client not connected. Message not sent.");
        return false;
    }
}