using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RealGame.Server
{


    public class EspServer
    {
        public event EventHandler<Connection>? OnConnected;
        public event EventHandler<Connection>? OnDisconnect;
        public event EventHandler<ConnectionMessage>? OnMessageRecieved;
        public event EventHandler<ConnectionMessage>? OnMessageSent;
        public event EventHandler<ConnectionException>? OnConnectionException;
        public event EventHandler<Connection>? OnApprovalSignalReceived;
        public event EventHandler<Connection>? OnPingSignalReceived;
        public event EventHandler<(Connection, string)>? OnErrorSignalReceived;
        public event EventHandler? OnStarted;
        public event EventHandler? OnStopped;
        public HashSet<Connection> Connections { get; private set; } = new();
        public bool WaitBackApproval { get; set; } = true;
        public TcpListener Listener { get; private set; }

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);
        public TimeSpan? ExpectPingToKeepAlive { get; set; } = null;


        #region Constructors
        public EspServer(int port, TimeSpan? timeout=null)
        {
            Listener = new TcpListener(IPAddress.Any, port);
            _timeout = timeout ?? TimeSpan.FromSeconds(30);
        }
        public EspServer(IPEndPoint localEP, TimeSpan? timeout = null)
        {
            Listener = new TcpListener(localEP);
            _timeout = timeout ?? TimeSpan.FromSeconds(30);
        }
        public EspServer(IPAddress localaddr, int port, TimeSpan? timeout = null)
        {
            Listener = new TcpListener(localaddr, port);
            _timeout = timeout ?? TimeSpan.FromSeconds(30);
        }

        #endregion


        private bool _stopped = false;
        public void Stop()
        {
            _stopped = true;
            Listener.Stop();
            OnStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Starts the server and listens for incoming connections asynchronously
        /// </summary>
        /// <param name="backlog"></param>
        /// <returns></returns>
        public async Task StartAsync(int? backlog = null)
        {
            if (backlog == null)
                Listener.Start();
            else
                Listener.Start(backlog.Value);
            OnStarted?.Invoke(this, EventArgs.Empty);
            while (!_stopped)
            {
                try
                {
                    var client = await Listener.AcceptTcpClientAsync();
                    var connection = new Connection(client); // Create a new connection
                    var endPoint = connection.EndPoint.ToString() ?? "_";
                    Connections.Add(connection); // Add the connection to the connections list
                    _ = HandleConnectionAsync(connection); // Handle the connection asynchronously
                    OnConnected?.Invoke(this, connection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting TCP client: {ex.Message}");
                }

            }
        }

        /// <summary>
        /// Handles a connection asynchronously by reading messages from the stream and invoking the corresponding events
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task HandleConnectionAsync(Connection connection)
        {
            var client = connection.TcpClient;
            try
            {
                if (client == null)
                    throw new ArgumentNullException(nameof(client));

                connection.OnConnectionException += (s, e) => OnConnectionException?.Invoke(this, e); // Forward connection exception event to the server
                connection.OnMessageSent += (s, e) => OnMessageSent?.Invoke(this, new ConnectionMessage(connection, e)); // Forward message sent event to the server

                NetworkStream stream = client.GetStream();
                while (client.Connected)
                {

                    // If ping is enabled, check if the last ping was more than the expected time to keep alive the connection if so, break the loop
                    if (ExpectPingToKeepAlive != null)
                        if (DateTime.Now - connection.LastPing > ExpectPingToKeepAlive)
                            break;


                    // If there is no data available, wait 100ms and continue
                    if (client.Available == 0)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    Message message = await GetMessageFromStream(connection, _timeout); // Read message from stream

                    // If the message is a ping signal, invoke the ping event and continue
                    if(message.IsPingSignal())
                    {
                        connection.InvokePing();
                        OnPingSignalReceived?.Invoke(this, connection);
                        continue;
                    }

                    // If the message is an approval signal, invoke the approval signal event and continue
                    if (message.IsApprovalSignal())
                    {
                        connection.InvokeApprovalSignalReceived();
                        OnApprovalSignalReceived?.Invoke(this, connection);
                        continue;
                    }

                    // If the message is an error signal, invoke the error signal event and continue
                    if (message.IsErrorSignal(out string? error))
                    {
                        connection.InvokeErrorSignalReceived(message);
                        OnErrorSignalReceived?.Invoke(this, (connection, error ?? ""));
                        continue;
                    }

                    // If the message is not a ping, approval or error signal, invoke the message recieved event
                    connection.InvokeMessageRecieved(message);
                    OnMessageRecieved?.Invoke(this, new ConnectionMessage(connection, message));
                    GC.Collect();
                }
            }
            catch (ConnectionException ex) // If a connection exception is thrown, invoke the connection exception event
            {
                OnConnectionException?.Invoke(this, ex);
            }
            catch (Exception ex) // If any other exception is thrown, create a connection exception and invoke the connection exception event
            {
                var exception = new ConnectionException($"Error handling client: {ex.Message}", connection, ex);
                OnConnectionException?.Invoke(this, exception);
            }
            finally // Disconnect the client and remove it from the connections list
            {
                Connections.Remove(connection);
                connection.Disconnect();
                OnDisconnect?.Invoke(this, connection);
            }
        }

        /// <summary>
        /// Reads a message from the stream of a connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ConnectionException"></exception>
        public static async Task<Message> GetMessageFromStream(Connection connection, TimeSpan? timeout = null)
        {
            try
            {
                if(connection.TcpClient == null)
                    throw new ArgumentNullException(nameof(connection.TcpClient));
                var stream = connection.TcpClient!.GetStream();
                byte[] headerBuffer = new byte[9]; // 1st byte is the message type and the next 8 bytes are the message length (long)


                // If timeout is null, read synchronously, otherwise read asynchronously with a timeout cancellation token
                if(timeout == null)
                    await stream.ReadAsync(headerBuffer, 0, 9);
                else
                {
                    var cts = new CancellationTokenSource(timeout.Value);
                    await stream.ReadAsync(headerBuffer, 0, 9, cts.Token);
                }

                // Cast the header buffer to a MessageHeader struct
                var handle = GCHandle.Alloc(headerBuffer, GCHandleType.Pinned);
                var msg = Marshal.PtrToStructure<MessageHeader>(handle.AddrOfPinnedObject());
                handle.Free();
                
                if(msg.length == 0) // If there is no data, return a message with only the header
                    return new Message { header = msg };
                
                
                byte[] dataBuffer = new byte[msg.length]; // Create a buffer to read the data
                await stream.ReadAsync(dataBuffer, 0, dataBuffer.Length); // Read the data from the stream
                var message = new Message // Create a message with the header and data
                {
                    header = new MessageHeader { length = msg.length, type = msg.type },
                    data = dataBuffer
                };

                return message;
            }
            catch (Exception ex)
            {
                throw new ConnectionException("Error reading message from stream", connection, ex);
            }
        }


        
        public Connection? GetConnection(string endpointStr)
        {
            return Connections.FirstOrDefault(c => c.EndPoint.ToString() == endpointStr);
        }

        public Task SendMessageAsync(Connection connection, Message msg)
        {
            return connection.SendAsync(msg, _timeout);
        }

    }

}
