namespace RealGame.Server
{
    public class ConnectionMessage
    {
        public Connection Connection { get; }
        public Message Message { get; }

        public ConnectionMessage(Connection connection, Message message)
        {
            Connection = connection;
            Message = message;
        }

    }

}
