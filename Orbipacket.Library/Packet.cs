namespace Orbipacket
{
    public class Packet
    {
        public const byte VERSION = 0x01;
        public DeviceId DeviceId { get; init; }
        public Payload Payload { get; init; }
        public ulong Timestamp { get; init; }

        public enum PacketType
        {
            TmPacket,
            TcPacket,
            Unknown,
        }

        public PacketType Type { get; init; }

        public Packet(DeviceId deviceId, System.UInt64 timestamp, Payload payload, PacketType type)
        {
            DeviceId = deviceId;
            Timestamp = timestamp;
            Payload = payload;
            Type = type;
        }

        public byte Length()
        {
            return Payload.Length();
        }

        public static byte Overhead()
        {
            return 11;
        }

        public byte Size()
        {
            return (byte)(Overhead() + Length());
        }

        public byte Version => 1;
    }
}
