# RealGame
The main purpose of this project was to create a simple 1v1 game that uses an ESP32 to taze losing player in real life.

However, as I progressed with development, the project evolved, and some of the smaller sub-projects became more general-purpose than originally planned (which is fine). Of course, there's always room for improvement.

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
