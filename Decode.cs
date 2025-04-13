using Orbipacket.Library;

namespace Orbipacket
{
    public class Decode
    {
        private const int VERSION_OFFSET = 0;
        private const int LENGTH_OFFSET = 1;
        private const int CONTROL_OFFSET = 2;
        private const int TIMESTAMP_OFFSET = 3;
        private const int PAYLOAD_OFFSET = 11;
        public const int CRC_TERMINATION_BYTE_SIZE = 3; // 2 bytes for CRC, 1 0x00 termination byte
        private const byte _terminationByte = 0x00; // Termination byte

        /// <summary>
        /// Handles decoding of packet data into structured Packet objects
        /// </summary>
        /// <param name="rawpacketData">The raw packet data (excluding termination byte) to decode.</param>
        public static Packet GetPacketInformation(byte[] rawpacketData)
        {
            byte[] packetData = COBS.Decode(rawpacketData).ToArray();
            Console.WriteLine("Decoded packet data: " + BitConverter.ToString(packetData));
            byte[] crc = Crc16.GetCRC(packetData); // Compute CRC for the packet data

            byte[] crcFromPacket = rawpacketData[^2..][..2]; // Extract CRC from the packet data
            Console.WriteLine("CRC from packet: " + BitConverter.ToString(crcFromPacket));
            Console.WriteLine("Computed CRC: " + BitConverter.ToString(crc));
            if (!crc.SequenceEqual(crcFromPacket))
            {
                throw new ArgumentException("CRC mismatch. Packet data may be corrupted.");
            }

            byte controlByte = packetData[CONTROL_OFFSET];
            Packet.PacketType type = GetPacketType(controlByte); // Determine packet type

            byte deviceId = (byte)((controlByte >> 2) & 0b00011111); // Extract deviceId (bits 2-6) of control byte

            byte[] timestampBytes = packetData[TIMESTAMP_OFFSET..][..8];

            ulong timestamp = BitConverter.ToUInt64(timestampBytes, 0);

            int payloadLength = packetData.Length - PAYLOAD_OFFSET - 2; // Subtract 2 for CRC

            ValidatePacket(packetData, payloadLength); // Pass payloadLength to ValidatePacket

            byte[] payload = packetData[PAYLOAD_OFFSET..][..payloadLength];

            return new Packet(
                deviceId: (DeviceId)deviceId,
                timestamp: timestamp,
                payload: new Payload(BitConverter.ToString(payload)),
                type: type
            );
        }

        private static Packet.PacketType GetPacketType(byte controlByte)
        {
            return (controlByte & 0b10000000) == 0
                ? Packet.PacketType.TmPacket
                : Packet.PacketType.TcPacket;
        }

        /// <summary>
        /// Validates the packet data by checking if it matches the expected version and length.
        /// </summary>
        /// <param name="packetData">The packet data to validate.</param>
        /// <param name="payloadLength">The expected length of the payload.</param>
        /// <exception cref="ArgumentException">Thrown if the packet version or length is invalid.</exception>
        private static void ValidatePacket(byte[] packetData, int payloadLength)
        {
            if (packetData[VERSION_OFFSET] != Packet.VERSION)
            {
                throw new ArgumentException("Packet version mismatch.");
            }

            if (payloadLength != packetData[LENGTH_OFFSET])
            {
                throw new ArgumentException(
                    $"Invalid packet length. Expected {packetData[LENGTH_OFFSET]}, got {packetData.Length}"
                );
            }
        }

        /// <summary>
        /// Appends the termination byte to the packet data.
        /// </summary>
        /// <param name="packetData">The packet data to append the termination byte to.</param>
        /// <returns>The packet data with the termination byte appended.</returns>
        private static byte[] AppendTerminationByte(byte[] packetData)
        {
            return [.. packetData, _terminationByte];
        }

        /// <summary>
        /// Determines the location of the termination byte in the packet data.
        /// </summary>
        public static int DetermineTerminationByteLocation(byte[] packetData)
        {
            return Array.IndexOf(packetData, _terminationByte);
        }
    }
}
