using Orbipacket.Library;

namespace Orbipacket
{
    public class Decode
    {
        private const int VERSION_OFFSET = 0;
        private const int LENGTH_OFFSET = 1;
        private const int CONTROL_OFFSET = 2;
        private const int TIMESTAMP_OFFSET = 3;
        private const int TIMESTAMP_SIZE = 5;
        private const int PAYLOAD_OFFSET = TIMESTAMP_OFFSET + TIMESTAMP_SIZE;
        public const byte _terminationByte = 0x00; // Termination byte

        /// <summary>
        /// Handles decoding of packet data into structured Packet objects
        /// </summary>
        /// <param name="packetData">The raw packet data (excluding termination byte) to decode.
        ///	Is supposed to be used with data from the PacketBuffer.
        /// </param>
        public static Packet? GetPacketInformation(byte[] packetData)
        {
            // Extract payload length from packet data and  validate it
            int payloadLength = packetData.Length - PAYLOAD_OFFSET - 2; // Subtract 2 for CRC

            // Analyze timestamp earlier than the rest to make sure it's 5 bytes long
            byte[] timestampBytes = packetData[TIMESTAMP_OFFSET..][..TIMESTAMP_SIZE];
            ulong? timestamp = ReadUInt40(timestampBytes);

            if (!ValidatePacket(packetData, payloadLength) || timestamp == null)
            {
                Console.WriteLine("Invalid packet, discarding...");
                return new Packet(
                    deviceId: DeviceId.Unknown,
                    timestamp: 0,
                    payload: new Payload([]),
                    type: Packet.PacketType.Unknown
                );
            }

            // Analyze control byte
            byte controlByte = packetData[CONTROL_OFFSET];

            byte deviceId = (byte)((controlByte >> 2) & 0b00011111); // Extract deviceId (bits 3-7) of control byte

            // Witchcraft that happens here:
            // We first right-shift the bits twice, so 0b01111100 (TM packet, DevID 32) becomes
            // 0b00011111: and here we can just use the bitwise AND to extract the first 5 bits
            // 0b00011111 = 32, efficiently extracting the deviceID.

            Packet.PacketType type = GetPacketType(controlByte); // Determine packet type

            // Extract payload
            byte[] payload = packetData[PAYLOAD_OFFSET..][..payloadLength];

            return new Packet(
                deviceId: (DeviceId)deviceId,
                timestamp: (ulong)timestamp,
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
        /// <returns>Bool value indicating whether packet is valid or not.</returns>
        private static bool ValidatePacket(byte[] packetData, int payloadLength)
        {
            if (packetData[VERSION_OFFSET] != Packet.VERSION)
            {
                Console.WriteLine("Packet version mismatch.");
                return false;
            }

            if (payloadLength != packetData[LENGTH_OFFSET])
            {
                Console.WriteLine(
                    "Payload length mismatch, expected "
                        + packetData[LENGTH_OFFSET]
                        + " got "
                        + payloadLength
                );
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the CRC of the packet is valid or not
        /// </summary>
        public static byte[]? DecodeCobsAndValidate(byte[] packetData)
        {
            // Decode packet data using COBS
            byte[] decodedData;
            try
            {
                decodedData = [.. COBS.Decode(packetData)];
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("COBS decoding failed, invalid packet length.");
                return null;
            }
            if (decodedData.Length >= 13 && Crc16.IsCrcValid(decodedData))
            {
                return decodedData;
            }
            else
                return null;
        }

        private static ulong? ReadUInt40(byte[] bytes)
        {
            if (bytes.Length < TIMESTAMP_SIZE)
            {
                Console.WriteLine("Timestamp size incorrect.");
                return null;
            }
            ulong value = 0;
            for (int i = 0; i < 5; i++)
            {
                value |= ((ulong)bytes[i]) << (8 * i);
            }
            return value;
        }
    }

    // /// <summary>
    // /// Appends the termination byte to the packet data.
    // /// </summary>
    // /// <param name="packetData">The packet data to append the termination byte to.</param>
    // /// <returns>The packet data with the termination byte appended.</returns>
    // private static byte[] AppendTerminationByte(byte[] packetData)
    // {
    //     return [.. packetData, _terminationByte];
    // }
    // To be implemented, idk if this has any use (it's more of an encoding thing)
}
