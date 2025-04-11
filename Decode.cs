using Orbipacket;

namespace Orbipacket
{
    public class Decode
    {
        private const int VERSION_OFFSET = 0;
        private const int LENGTH_OFFSET = 1;
        private const int CONTROL_OFFSET = 2;
        private const int TIMESTAMP_OFFSET = 3;
        private const int PAYLOAD_OFFSET = 11;

        /// Decode the packet from a byte array
        public Packet GetPacketInformation(byte[] packetData)
        {
            if (packetData.Length < Packet.Overhead())
            {
                throw new ArgumentException("Packet data is too short.");
            }

            if (packetData[0] != Packet.VERSION)
            {
                throw new ArgumentException("Packet version mismatch.");
            }

            byte length = packetData[1];

            // Check if length is valid
            if (length != Packet.Size())
            {
                throw new ArgumentException("Packet length mismatch.");
            }

            // Treat control as a bit field
            string control = Convert.ToString(packetData[2], 2).PadLeft(8, '0');

            Packet.PacketType type;
            // Check if packet is TM or TC
            if (control[0] == '0')
            {
                isTmPacket = true;
                isTcPacket = false;
                type = Packet.PacketType.TmPacket;
            }
            else
            {
                isTmPacket = false;
                isTcPacket = true;
                type = Packet.PacketType.TcPacket;
            }

            string deviceIdString = control.Substring(1, 4);

            byte[] timestampBytes = packetData.Skip(3).Take(8).ToArray();

            ulong timestamp = BitConverter.ToUInt64(timestampBytes, 0);

            Console.WriteLine("Packet length: " + packetData.Length);
            if (packetData.Length < 27) // 11 bytes overhead + 16 bytes payload
            {
                throw new ArgumentException("Packet data is too short for payload.");
            }

            byte[] payloadData = packetData.Skip(11).Take(16).ToArray();
            if (payloadData.Length < 16)
            {
                throw new ArgumentException("Payload data is incomplete.");
            }
            ulong lower = BitConverter.ToUInt64(payloadData, 0);
            ulong upper = BitConverter.ToUInt64(payloadData, 8);

            return new Packet(
                deviceId: (DeviceId)Convert.ToByte(deviceIdString, 2),
                timestamp: timestamp,
                payload: new Payload(new UInt128(lower, upper)),
                type: type
            );
        }
    }
}
