using RealGame.Server;
using System.Net;
using System.Text;
namespace RealGame.Application
{
    /// <summary>
    /// Class that constructs ESP server and starts it.
    /// </summary>
    internal class EspServerTask
    {
        public EspServer server = new EspServer(new IPEndPoint(IPAddress.Parse("192.168.1.175"), 5555));

        public Task Start() => server.StartAsync();

        public EspServerTask()
        {

            ConsoleHelper.OnCommand += ConsoleHelper_OnCommand; ;

            server.ExpectPingToKeepAlive = TimeSpan.FromSeconds(10);

            server.OnConnected += (s, c) =>
            {
                ConsoleHelper.Print($"Client connected {c.EndPoint}");
            };


            server.OnApprovalSignalReceived += (s, c) => ConsoleHelper.Print($"Approval signal received from {c.EndPoint}", ConsoleHelper.LogLevel.Success);

            server.OnErrorSignalReceived += (s, e) => ConsoleHelper.Print($"Error signal received from {e.Item1.EndPoint}: {e.Item2}", ConsoleHelper.LogLevel.Err);

            server.OnStarted += (s, e) => ConsoleHelper.Print($"Server started: {server.Listener.LocalEndpoint}");

            server.OnDisconnect += (s, c) => ConsoleHelper.Print($"Client disconnected {c.EndPoint}", ConsoleHelper.LogLevel.Warn);

            server.OnConnectionException += (s, e) => ConsoleHelper.Print($"Connection exception {e.Client.EndPoint}: {e.Message} \n Inner Exception Message: {e.InnerException?.Message}", ConsoleHelper.LogLevel.Err);

            //server.OnPingSignalReceived += (s, c) => ConsoleHelper.Print($"Ping signal received from {c.EndPoint}");

            server.OnMessageSent += (s, c) => {
                var obj = c.Message.GetDataAsObject();
                var msg = $"Message sent to {c.Connection.EndPoint} \n" +
                    $"Message Type: {c.Message.header.type} \n" +
                    $"Message Length: {c.Message.header.length} \n" +
                    $"Message Data: {obj}";
                ConsoleHelper.Print(msg);
            };

            server.OnMessageRecieved += (s, c) =>
            {
                var obj = c.Message.GetDataAsObject();
                var msg = $"Message recieved from {c.Connection.EndPoint} \n" +
                    $"Message Type: {c.Message.header.type} \n" +
                    $"Message Length: {c.Message.header.length} \n" +
                    $"Message Data: {obj}";
                ConsoleHelper.Print(msg, ConsoleHelper.LogLevel.Success);
            };


        }


        /// TODO: Clean this mess
        private void ConsoleHelper_OnCommand(string str)
        {
            try
            {
                var parameters = str.Split(' ').Where(s => !String.IsNullOrEmpty(s)).ToList();
                if (parameters.Count() <= 0)
                    return;
                var flagAndValues = new Dictionary<string, string>();

                for (int i = 0; i < parameters.Count(); i++)
                {
                    if (parameters[i].StartsWith("-"))
                    {
                        if (i + 1 < parameters.Count())
                        {
                            if (parameters[i + 1].StartsWith("-"))
                            {
                                flagAndValues.Add(parameters[i], "");
                            }
                            else
                            {
                                flagAndValues.Add(parameters[i], parameters[i + 1]);
                                i++;
                            }
                        }
                        else
                        {
                            flagAndValues.Add(parameters[i], "");
                        }
                    }
                }

                if (flagAndValues.Count() == 0)
                    return;

                if (flagAndValues.ContainsKey("-types"))
                {
                    ConsoleHelper.Print("Message Types: ");
                    foreach (var type in Enum.GetValues(typeof(MessageType)))
                    {
                        ConsoleHelper.Print($"{(byte)type} - {type}", ConsoleHelper.LogLevel.None);
                    }
                }

                if (flagAndValues.ContainsKey("-send"))
                {
                    if (!flagAndValues.ContainsKey("-type") || !flagAndValues.ContainsKey("-data") || !flagAndValues.ContainsKey("-client"))
                    {
                        ConsoleHelper.Print("Usage: -send -type <type> -data <data> -client <client ip>", ConsoleHelper.LogLevel.Warn);
                        return;
                    }

                    var client = server.GetConnection(flagAndValues["-client"]);
                    var type = (MessageType)byte.Parse(flagAndValues["-type"]);
                    var data = flagAndValues["-data"];

                    if (client == null)
                    {
                        ConsoleHelper.Print("Client not found", ConsoleHelper.LogLevel.Err);
                        return;
                    }

                    if (!Enum.IsDefined(typeof(MessageType), type))
                    {
                        ConsoleHelper.Print("Invalid message type", ConsoleHelper.LogLevel.Err);
                        return;
                    }
                    Message msg;
                    switch (type)
                    {
                        case MessageType.Bool:
                            msg = bool.Parse(data).AsMessage();
                            break;
                        case MessageType.String:
                            msg = data.AsMessage();
                            break;
                        case MessageType.Int:
                            msg = int.Parse(data).AsMessage();
                            break;
                        case MessageType.Float:
                            msg = float.Parse(data).AsMessage();
                            break;
                        case MessageType.Double:
                            msg = double.Parse(data).AsMessage();
                            break;
                        case MessageType.Byte:
                            msg = byte.Parse(data).AsMessage();
                            break;
                        case MessageType.Json:
                            msg = data.AsJsonMessage();
                            break;
                        case MessageType.EspCommand:
                            byte isHigh = flagAndValues.ContainsKey("-high") ? (byte)1 : (byte)0;
                            byte isOutput = flagAndValues.ContainsKey("-output") ? (byte)1 : (byte)0;
                            byte pin = byte.Parse(flagAndValues["-pin"]);
                            var cmd = new EspCommand { isHigh = isHigh, isOutput = isOutput, pin = pin };
                            msg = cmd.AsMessage();
                            break;
                        case MessageType.Ping:
                            msg = new Message { header = new MessageHeader { type = (byte)type, length = 0 }, data = new byte[0] };
                            break;
                        default:
                            ConsoleHelper.Print("Invalid or unimplemented message type", ConsoleHelper.LogLevel.Err);
                            return;
                    }
                    client.SendAsync(msg).Wait();
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Print($"Error: {ex.Message}", ConsoleHelper.LogLevel.Err);
            }
        }

    }
}
