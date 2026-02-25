namespace Orbipacket.Library
{
    public class Encode
    {
        private const byte _terminationByte = 0x00; // Termination byte

        /// <summary>
        /// Encodes a Packet object into a COBS encoded byte array with termination byte.
        /// </summary>
        /// <param name="packet">The Packet object to encode.</param>
        /// <returns>A COBS encoded byte array representing the packet, ending with a termination byte.</returns>
        public static byte[] EncodePacket(Packet packet)
        {
            // Version Byte
            byte version = 0x01;

            // Length Byte
            byte length = packet.Payload.Length();

            // Control byte
            byte controlByte = (byte)((byte)packet.DeviceId << 2);
            // Assign last bit (0x80, 128) if packet is Telecommand packet
            if (packet.Type == Packet.PacketType.TcPacket)
                controlByte += 0x80;

            // Console.WriteLine("Control byte: " + Convert.ToString(controlByte, 2).PadLeft(8, '0'));

            ulong timestamp = packet.Timestamp;

            // Timestamp Bytes
            byte[] timestampBytes = new byte[5];

            if (packet.Timestamp > 0xFF_FF_FF_FF_FF)
            {
                throw new OverflowException("Packet timestamp overflow.");
            }

            for (int i = 0; i < 5; i++)
            {
                timestampBytes[i] = (byte)(timestamp & 0xFF);
                timestamp >>= 8;
            }

            // Payload bytes
            // How the payload value should be converted to a byte array is up to the end user.
            byte[] payloadBytes = packet.Payload.Value;

            // Complete Packet, ready for CRC calculation
            byte[] completePacket =
            [
                version,
                length,
                controlByte,
                .. timestampBytes,
                .. payloadBytes,
            ];

            // CRC is calculated before the COBS encoding
            byte[] crc = Crc16.GetCRC(completePacket);

            completePacket = [.. completePacket, .. crc];

            byte[] encodedPacket = [.. COBS.Encode(completePacket), _terminationByte];

            // Console.WriteLine("Encoded packet:" + BitConverter.ToString(encodedPacket));
            return encodedPacket;
        }
    }
}
