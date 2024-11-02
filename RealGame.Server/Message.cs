using System.Runtime.InteropServices;

namespace RealGame.Server
{
    public enum MessageType : byte
    {
        Bool = 0,
        String = 1,
        Int = 2,
        Float = 3,
        Double = 4,
        Byte = 5,
        Json = 6,
        Stream = 7,
        EspCommand = 8,
        ApprovalSignal = 9,
        ErrorSignal = 10,
        Ping = 11
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageHeader
    {
        public byte type;
        public ulong length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Message
    {
        public MessageHeader header;
        public byte[] data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EspCommand
    {
        public byte isHigh;
        public byte isOutput;
        public byte pin;
    }
}
