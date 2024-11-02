using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace RealGame.Server
{
    public static class ServerExtensions
    {
        /// <summary>
        /// Gets the data type of the message based on the message type as Type
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Type DataType(this Message message)
        {
            var messageTypeValues = Enum.GetValues(typeof(MessageType)).Cast<byte>().ToArray();
            if (!messageTypeValues.Contains(message.header.type))
                return typeof(object);

            var type = (MessageType)message.header.type;
            return type switch
            {
                MessageType.Bool => typeof(bool),
                MessageType.String => typeof(string),
                MessageType.Int => typeof(int),
                MessageType.Double => typeof(double),
                MessageType.Float => typeof(float),
                MessageType.Byte => typeof(byte),
                MessageType.Json => typeof(object),
                MessageType.Stream => typeof(byte[]),
                MessageType.EspCommand => typeof(object),
                _ => typeof(object),
            };
        }

        /// <summary>
        /// Tries to casts byte array to a generic struct, if it fails it returns default value of the struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T ToStruct<T>(this byte[] bytes) where T : struct
        {
            T strct = default;
            int size = Marshal.SizeOf(strct);
            nint ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, ptr, size);
                strct = Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return strct;
        }
        
        /// <summary>
        /// Casts the data of the message to the appropriate type and returns it as an object
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static object GetDataAsObject(this Message message)
        {
            var type = message.DataType();
            return type switch
            {
                Type t when t == typeof(bool) => BitConverter.ToBoolean(message.data, 0),
                Type t when t == typeof(string) => Encoding.UTF8.GetString(message.data),
                Type t when t == typeof(int) => BitConverter.ToInt32(message.data, 0),
                Type t when t == typeof(double) => BitConverter.ToDouble(message.data, 0),
                Type t when t == typeof(float) => BitConverter.ToSingle(message.data, 0),
                Type t when t == typeof(byte) => message.data[0],
                Type t when t == typeof(byte[]) => message.data,
                Type t when t == typeof(EspCommand) => message.data.ToStruct<EspCommand>(),
                _ => Encoding.UTF8.GetString(message.data),
            };
        }
        /// <summary>
        /// Converts a struct to a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strct"></param>
        /// <returns></returns>
        public static byte[] ToByteArray<T>(this T strct) where T : struct
        {
            int size = Marshal.SizeOf(strct);
            byte[] byteArray = new byte[size];

            nint ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(strct, ptr, false);
                Marshal.Copy(ptr, byteArray, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return byteArray;
        }

        public static bool IsApprovalSignal(this Message message) => message.header.type == (byte)MessageType.ApprovalSignal;
        public static bool IsErrorSignal(this Message message) =>  message.header.type == (byte)MessageType.ErrorSignal;
        public static bool IsPingSignal(this Message message) => message.header.type == (byte)MessageType.Ping;
        public static bool IsApprovalOrErrorSignal(this Message message) => message.IsApprovalSignal() || message.IsErrorSignal();
        
        public static bool IsErrorSignal(this Message message, out string? errorMessage)
        {
            if (message.IsErrorSignal())
            {
                errorMessage = Encoding.UTF8.GetString(message.data);
                return true;
            }
            errorMessage = null;
            return false;
        }

        public static bool IsApprovalOrErrorSignal(this Message message, out string? errorMessage)
        {
            if (message.IsApprovalSignal())
            {
                errorMessage = null;
                return true;
            }
            return message.IsErrorSignal(out errorMessage);
        }

        /// <summary>
        /// Creates an instance of Message containing the data of the object as a json string 
        /// if the object is a string it will be converted to a byte array and returned as a message
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        public static Message AsJsonMessage(this object objData)
        {
            if(objData is string s)
            {
                var data = Encoding.UTF8.GetBytes(s);
                var length = (ulong)data.Length;
                return new Message { header = new MessageHeader { type = (byte)MessageType.Json, length = length }, data = data };
            }
            var jsonText = JsonSerializer.Serialize(objData);
            return jsonText.AsJsonMessage();
        }

        /// <summary>
        /// Creates an instance of Message containing the data of the object
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        public static Message AsMessage(this object objData)
        {

            var type = objData switch
            {
                bool _ => (byte)MessageType.Bool,
                string _ => (byte)MessageType.String,
                int _ => (byte)MessageType.Int,
                double _ => (byte)MessageType.Double,
                float _ => (byte)MessageType.Float,
                byte _ => (byte)MessageType.Byte,
                byte[] _ => (byte)MessageType.Stream,
                EspCommand _ => (byte)MessageType.EspCommand,
                _ => (byte)MessageType.Json,
            };
            var data = objData switch
            {
                bool b => BitConverter.GetBytes(b),
                string s => Encoding.UTF8.GetBytes(s),
                int i => BitConverter.GetBytes(i),
                double d => BitConverter.GetBytes(d),
                float f => BitConverter.GetBytes(f),
                byte b => new byte[] { b },
                byte[] b => b,
                EspCommand cmd => cmd.ToByteArray(),
                _ => Encoding.UTF8.GetBytes(objData?.ToString() ?? ""),
            };
            var length = (ulong)data.Length;

            Console.WriteLine($"Type: {type}, Length: {length}, Data: {data}");
            return new Message { header = new MessageHeader { type = type, length = length }, data = data };
        }

    }
}
