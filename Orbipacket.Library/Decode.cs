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
        public const byte _terminationByte = 0x00; // Termination byte

        /// <summary>
        /// Handles decoding of packet data into structured Packet objects
        /// </summary>
        /// <param name="rawpacketData">The raw packet data (excluding termination byte) to decode.
		///	Is supposed to be used with data from the PacketBuffer.
		/// </param>
        public static Packet? GetPacketInformation(byte[] rawpacketData)
        {
            // 1. Decode COBS
            byte[] packetData = [.. COBS.Decode(rawpacketData)];
            // Console.WriteLine("Decoded packet data: " + BitConverter.ToString(packetData));

            // 2. Compute CRC of packet data (without CRC bytes)
            byte[] crc = Crc16.GetCRC(packetData[..^2]);

            // 3. Extract CRC from packet
            byte[] crcFromPacket = rawpacketData[^2..];

            // Console.WriteLine("CRC from packet: " + BitConverter.ToString(crcFromPacket));
            // Console.WriteLine("Computed CRC: " + BitConverter.ToString(crc));
            if (!crc.SequenceEqual(crcFromPacket))
            {
                return null;
            }

            // 4. Extract payload length from packet data and  validate it
            int payloadLength = packetData.Length - PAYLOAD_OFFSET - 2; // Subtract 2 for CRC

            if (!ValidatePacket(packetData, payloadLength))
            {
                Console.WriteLine("Invalid packet, discarding...");
                return new Packet(
                    deviceId: DeviceId.Unknown,
                    timestamp: 0,
                    payload: new Payload([]),
                    type: Packet.PacketType.Unknown
                );
            }

            // 5. Analyze control byte
            byte controlByte = packetData[CONTROL_OFFSET];
            byte deviceId = (byte)((controlByte >> 2) & 0b00011111); // Extract deviceId (bits 2-6) of control byte
            Packet.PacketType type = GetPacketType(controlByte); // Determine packet type

            // 6. Analyze timestamp
            byte[] timestampBytes = packetData[TIMESTAMP_OFFSET..][..8];
            ulong timestamp = BitConverter.ToUInt64(timestampBytes, 0);

            // 7. Extract payload
            byte[] payload = packetData[PAYLOAD_OFFSET..][..payloadLength];

            return new Packet(
                deviceId: (DeviceId)deviceId,
                timestamp: timestamp,
                payload: new Payload(payload),
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
        private static bool ValidatePacket(byte[] packetData, int payloadLength)
        {
            if (packetData[VERSION_OFFSET] != Packet.VERSION)
            {
                Console.WriteLine("Packet version mismatch.");
                return false;
            }

            if (payloadLength != packetData[LENGTH_OFFSET])
            {
                Console.WriteLine("Payload length mismatch.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Appends the termination byte to the packet data.
        /// </summary>
        /// <param name="packetData">The packet data to append the termination byte to.</param>
        /// <returns>The packet data with the termination byte appended.</returns>
        // private static byte[] AppendTerminationByte(byte[] packetData)
        // {
        //     return [.. packetData, _terminationByte];
        // }
        // To be implemented, idk if this has any use (it's more of an encoding thing)
    }
}
