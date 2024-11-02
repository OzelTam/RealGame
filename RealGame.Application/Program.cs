using RealGame.Application;
using RealGame.Server;
using System.Collections.Concurrent;


#if DEBUG == false
Helpers.HideConsole();
#endif



Console.Clear();

// 
var espServerTask = new EspServerTask();
_ = espServerTask.Start();


var game = new GameTask();
game.OnPlayerDeath += Game_OnPlayerDeath;
game.Run();


void Game_OnPlayerDeath(object? sender, RealGame.Application.GameObjects.Drone e)
{
    ConsoleHelper.Print($"PLAYER JUST DIED: {e.Id}", ConsoleHelper.LogLevel.Warn);
    byte pin = e.Id == "drone1" ? (byte)33 : (byte)34;
    var espCmd = new EspCommand { isHigh=1, isOutput=1, pin = pin };
    var espConnection = espServerTask.server.Connections.FirstOrDefault();
    espConnection?.SendAsync(espCmd.AsMessage());
}

ConsoleHelper.OnCommand += ConsoleHelper_OnCommand;

void ConsoleHelper_OnCommand(string cmd)
{
    if (cmd == "exit")
    {
        Environment.Exit(0);
    }
}

_ = Task.Run(() => ConsoleHelper.RunCommandListener());


while (true)
{
    Thread.Sleep(1000);
}