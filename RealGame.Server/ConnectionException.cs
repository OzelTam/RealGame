namespace RealGame.Server
{
    public class ConnectionException : Exception
    {
        public Connection Client { get; }
        public ConnectionException(string message, Connection client) : base(message) { Client = client; }
        public ConnectionException(string message, Connection client, Exception innerException) : base(message, innerException) { Client = client; }
    }

}
