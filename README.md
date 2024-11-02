# RealGame
The main purpose of this project was to create a simple 1v1 game that uses an ESP32 to taze losing player in real life.

However, as I progressed with development, the project evolved, and some of the sub-projects became more general-purpose than originally intended (which is fine). Of course, there's always room for improvement.

# Gameplay
### Rules:
 - Every players aim is to protect its base, and destroy opponents.
 - Players can push each other.
 - Initial health is 100
 - Traps only effect the opponent spawned on the opposite side
 - Traps hit a initial damage that is propotional to collision speed then it drops to a continous steady value.
 - Friendly fire is on for bombs
 - Bombs got damage of 10
 - There is 1 second cooldown for dropping bomb
 - Cannot move while dropping bomb 
 - On drone collisions slow one gets damaged proportional to fast ones speed.
> If drone1 (left side) wins pin 33 of ESP32 will get activated as OUTPUT, HIGH. For second player pin 34 will be activated. Tazers and tazer swiches should be wired appropriately :) .
### Controls
- Player 1
<code>W - UP</code> 
<code>S - DOWN</code>
<code>A - LEFT</code>
<code>D - RIGHT</code>
<code>SPACE - DROP BOMB</code>

- Player 2
<code>Numpad8 - UP</code> 
<code>Numpad5 - DOWN</code>
<code>Numpad4 - LEFT</code>
<code>Numpad6 - RIGHT</code>
<code>Numpad0 - DROP BOMB</code>


### Sample Gameplay
 ![](https://github.com/OzelTam/RealGame/blob/main/gameplay.gif)

> ### Note
> Change the values of <code>WIFI_SSID</code>, <code>WIFI_PASSWORD</code>, <code>TCP_HOST</code>, <code>TCP_PORT</code> definitons in the main.cpp file of esp32 accordingly.

# Protocol
Communication Messages starts with 9 byte header and following data. You can find struct definitions on [protocol.h](https://github.com/OzelTam/RealGame/blob/main/RealGame.ESP32/lib/protocol/protocol.h) and [Message.cs](https://github.com/OzelTam/RealGame/blob/main/RealGame.Server/Message.cs) files.

#### Message Structure
```
TYPE => 1 BYTE (char/byte) // Represents type of the data inerpretation
DATA_LENGTH => 8 BYTES (ulong/uint64_t)  // Represents lenth of the data in bytes
DATA => DATA_LENGTH BYTES (byte[]/char*) // Byre array of represented data
```

#### Data Types
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
#### Behaviour
 - After a message is sent by server, Client is expected to send a approved signal <code>(Header type=9, data length=0)</code> or an error signal <code>(Header type=10, data=(string error message))</code>
 - Client is expected to send Ping Signal <code>(Header type=11, data length=0)</code> with max of 10 sec. timeout to not get disconnected.  

# Features
 ### RealGame.GameEngine (C#)
 > 2D Game engine with physics, uses SFML for rendering. Feasable to extend and re-use out of this projects scope.
  - Game Window
  - Game Scene
  - Game Entity Bases
  - Animations
  - OBB Collision Detection
  - Physics Simulations
  - Game Debug Utilities
  - Game Events, Entity Events, Physics Events

### RealGame.Server (C#)
> Multi threaded server that accepts multiple connections and implements a custom protocol to communicate with esp32 (or any other clients) that uses TcpServer fot handling socket connections. Feasable to extend and re-use out of this projects scope.
- Server
- Connection List
- Custom protocol implementation [SEND/RECEIVE]
- Protocol Message Helpers 
- Server Events

### RealGame.ESP32 (C++)
> Souce code for embedded system (ESP32) that used platform.io and implements custom communication protocol. Feasable to extend and re-use out of this projects scope.
- Wifi & Socket Connection
- Custom protocol implementation [SEND/RECEIVE]
- Execute Esp32Command manupulate specified pin as [Input/Output], [Write/Read], [HIGH/LOW].
- Protocol Message Helpers

### RealGame.Application (C#)
> Console application that implements/runs the Drone Fight game and tcp server, also handles sending message to ESP32 when a player dies. PIN-33-HIGH for drone1 dead PIN-34-HIGH  for drone2.
- Game Objects
- Game Sprites, Animations and Mechanics
- ESP32 Server

### RealGame.Debug (C#)
> Helper to prettify console input/output and prettify logs. (multit hreadded). 
