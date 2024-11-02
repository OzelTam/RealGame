
#ifndef CONNECTION_H
#define CONNECTION_H

#include <string>
#include <WiFiClient.h>
#include <protocol.h>
#include <WiFi.h>

class ConnectionManager {
public:
    ConnectionManager();
    bool ConnectWifi(const char* ssid, const char* password);
    bool ConnectServer(const char* host, uint16_t port);
    bool SendMessage(Message msg);
    WiFiClient client;  // Make client accessible

    
};


#endif