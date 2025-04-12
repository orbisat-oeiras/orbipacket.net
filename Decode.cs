using Nito.HashAlgorithms;
using Orbipacket;
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
        private static readonly byte _terminationByte = 0x00; // Termination byte

        /// <summary>
        /// Handles decoding of packet data into structured Packet objects
        /// </summary>
        /// <param name="rawpacketData">The raw packet data (excluding termination byte) to decode.</param>
        public static Packet GetPacketInformation(byte[] rawpacketData)
        {
            byte[] packetData = COBS.Decode(rawpacketData).ToArray();
            Console.WriteLine("Decoded packet data: " + BitConverter.ToString(packetData));
            byte[] crc = Crc16.GetCRC(packetData); // Compute CRC for the packet data

            byte[] crcFromPacket = rawpacketData.Skip(rawpacketData.Length - 2).Take(2).ToArray(); // Extract CRC from the packet data
            Console.WriteLine("CRC from packet: " + BitConverter.ToString(crcFromPacket));
            Console.WriteLine("Computed CRC: " + BitConverter.ToString(crc));
            if (!crc.SequenceEqual(crcFromPacket))
            {
                throw new ArgumentException("CRC mismatch. Packet data may be corrupted.");
            }

            byte controlByte = packetData[CONTROL_OFFSET];
            Packet.PacketType type = GetPacketType(controlByte); // Determine packet type

            byte deviceId = (byte)((controlByte >> 2) & 0b00011111); // Extract deviceId (bits 2-6) of control byte

            byte[] timestampBytes = packetData.Skip(TIMESTAMP_OFFSET).Take(8).ToArray();

            ulong timestamp = BitConverter.ToUInt64(timestampBytes, 0);

            int payloadLength = packetData.Length - PAYLOAD_OFFSET - 2; // Subtract 2 for CRC

            ValidatePacket(packetData, payloadLength); // Pass payloadLength to ValidatePacket

            byte[] payload = packetData.Skip(PAYLOAD_OFFSET).Take(payloadLength).ToArray();

            return new Packet(
                deviceId: (DeviceId)deviceId,
                timestamp: timestamp,
                payload: new Payload(BitConverter.ToString(payload)),
                type: type
            );
        }

        private static Packet.PacketType GetPacketType(byte controlByte)
        {
            bool isTcPacket = (controlByte & 0b10000000) != 0; // Check if packet is TcPacket
            if (isTcPacket)
            {
                return Packet.PacketType.TcPacket;
            }
            else
            {
                return Packet.PacketType.TmPacket;
            }
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
            return packetData.Append(_terminationByte).ToArray();
        }

        /// <summary>
        /// Determines the location of the termination byte in the packet data.
        /// </summary>
        public static int DetermineTerminationByteLocation(byte[] packetData)
        {
            // Find the index of the termination byte (0x00)
            int terminationByteIndex = Array.IndexOf(packetData, _terminationByte);
            // If the termination byte is not found, return -1
            if (terminationByteIndex == -1)
            {
                return -1;
            }
            // Return the index of the termination byte
            return terminationByteIndex;
        }
    }
}
