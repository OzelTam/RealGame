using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RealGame.Server
{
    public class Connection
    {
        public event EventHandler<Message>? OnMessageRecieved;
        public event EventHandler<Message>? OnMessageSent;
        public event EventHandler? OnPing;
        public event EventHandler? OnDisconnect;
        public event EventHandler<ConnectionException>? OnConnectionException;
        public event EventHandler? ApprovedSignalReceived;
        public event EventHandler<Message>? ErrorSignalReceived;
        public EndPoint EndPoint { get; }
        public TcpClient? TcpClient { get; }
        public DateTime EstablishTime { get; } = DateTime.Now;
        public DateTime LastPing { get; set; } = DateTime.Now;

        public Connection(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (client?.Client?.RemoteEndPoint == null)
                throw new ArgumentNullException(nameof(client.Client.RemoteEndPoint));

            TcpClient = client;
            EndPoint = client.Client.RemoteEndPoint;
        }
        
        public override bool Equals(object? obj) => obj is Connection c ? c.EndPoint == EndPoint :obj is string endp ? EndPoint.ToString() == endp : base.Equals(obj);
        public override string ToString() =>  $"{EndPoint}";
        public override int GetHashCode() => EndPoint.ToString()!.GetHashCode();

        public void InvokeApprovalSignalReceived() =>  ApprovedSignalReceived?.Invoke(this, EventArgs.Empty);
        public void InvokeMessageRecieved(Message message) => OnMessageRecieved?.Invoke(this, message);
        public void InvokeErrorSignalReceived(Message message) => ErrorSignalReceived?.Invoke(this, message);
       
        /// <summary>
        /// Close client connection and invoke disconnect event
        /// </summary>
        public void Disconnect()
        {
            TcpClient?.Close();
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }

        public void InvokePing()
        {
            LastPing = DateTime.Now;
            OnPing?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Waits for an approval signal from the client with a timeout and invokes the approval signal event
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ConnectionException"></exception>
        public async Task WaitApprovalSignal(TimeSpan timeout)
        {
          
            if (TcpClient == null)
                throw new ArgumentNullException(nameof(TcpClient));
            var stream = TcpClient.GetStream();
            var msg = await EspServer.GetMessageFromStream(this, timeout);

            if (msg.header.type == (byte)MessageType.ErrorSignal)
            {
                ErrorSignalReceived?.Invoke(this, msg);
                throw new ConnectionException("Error signal received", this, new Exception(Encoding.UTF8.GetString(msg.data)));
            }

            if (msg.header.type != (byte)MessageType.ApprovalSignal)
                throw new ConnectionException("Approval signal not received", this);

            ApprovedSignalReceived?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sends a message to the client and waits for an approval signal if the timeout is set
        /// </summary>
        /// <param name="msg"> Message to send </param>
        /// <param name="timeout"> Maz Waiting time for approval (will not wait if null) </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">TcpClient is not connected</exception>
        /// <exception cref="ConnectionException">Throws if errror signal recieved or </exception>
        public async Task SendAsync(Message msg, TimeSpan? timeout = null)
        {
            try
            {

                if (TcpClient == null || !TcpClient.Connected)
                    throw new InvalidOperationException("Client is not connected");

                var msgBuff = msg.ToByteArray();
                var header = msgBuff[0..9]; // Get the header from the message buffer (Rest of it is not expected byte[] of data in the memory)

                var data = msg.data; // Get byte[] of data from the message
                var fullData = header.Concat(data).ToArray(); // Combine header and data into one array

                var stream = TcpClient.GetStream();
                await stream.WriteAsync(fullData, 0, fullData.Length); // Write the full message to the stream
                await stream.FlushAsync();
                if(timeout != null)
                    await WaitApprovalSignal(timeout.Value); // Wait for approval signal
                OnMessageSent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                var exception = new ConnectionException($"Error sending message: {ex.Message}", this, ex);
                OnConnectionException?.Invoke(this, exception);
            }
        }
    }
}
