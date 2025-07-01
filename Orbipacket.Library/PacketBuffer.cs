using Orbipacket.Library;

namespace Orbipacket
{
    public class PacketBuffer
    {
        public Queue<byte> _buffer = new();

        public PacketBuffer()
        {
            _buffer = new Queue<byte>();
        }

        /// <summary>
        /// Adds a byte array to the buffer.
        /// </summary>
        public void Add(byte[] data)
        {
            foreach (var byteData in data)
            {
                _buffer.Enqueue(byteData);
            }
        }

        /// <summary>
        /// Manipulates the buffer directly and
        /// searches the buffer for the first available packet and
        /// removes that packet from the buffer.
        /// </summary>
        /// <returns>
        /// A delimited and usable byte array for the Decode class.
        /// </returns>
        public byte[]? ExtractFirstValidPacket()
        {
            while (_buffer.Count >= 13)
            {
                byte[] bufferArray = [.. _buffer];

                // Find the first occurrence of the termination byte
                int startIndex = Array.IndexOf(bufferArray, Decode._terminationByte);

                // TODO: Is it actually better to automatically clear the buffer?
                // Or do we make it so the end user has to manually clear it?
                // How would we deal with invalid packets?
                // But yeah what we have works

                if (startIndex == -1)
                {
                    ClearBuffer(); // Clear invalid data
                    return null;
                }

                // Find the next termination byte
                int endIndex = Array.IndexOf(bufferArray, Decode._terminationByte, startIndex + 1);

                // If no second termination byte found, packet is incomplete
                if (endIndex == -1)
                {
                    // If no second termination byte is found, check if the remaining data forms a valid packet
                    byte[] remainingData = bufferArray[(startIndex + 1)..];
                    if (remainingData.Length >= 13 && IsCRCValid(remainingData))
                    {
                        ClearBuffer(); // Clear the buffer since the packet is complete
                        return remainingData;
                    }

                    // Otherwise, keep everything from the first termination byte onwards
                    _buffer = new Queue<byte>(bufferArray[startIndex..]);
                    return null; // Wait for more data
                }

                // Extract packet data (everything up to but not including the termination byte)
                byte[] packetData = bufferArray[(startIndex + 1)..endIndex];

                // Update the buffer to keep data after the second termination byte
                _buffer = new Queue<byte>(bufferArray[endIndex..]);

                if (packetData.Length < 13 || !IsCRCValid(packetData))
                {
                    Console.WriteLine(
                        "CRC check failed or packet is too short, discarding packet."
                    );
                    Console.WriteLine("Failed packet: " + BitConverter.ToString(packetData));
                    continue;
                }

                return packetData;
            }

            return null;
        }

        /// <summary>
        /// Clears the packet buffer.
        /// </summary>
        public void ClearBuffer()
        {
            _buffer.Clear();
        }

        /// <summary>
        /// Checks whether the CRC of the packet is valid or not
        /// </summary>
        public static bool IsCRCValid(byte[] packetData)
        {
            // Decode packet data using COBS
            byte[] decodedData = [.. COBS.Decode(packetData)];
            // Compute CRC of packet data (without CRC bytes)
            if (decodedData.Length < 2)
                return false; // Not enough data for CRC check

            byte[] crc = Crc16.GetCRC(decodedData[..^2]);

            // Extract CRC from packet
            byte[] crcFromPacket = decodedData[^2..];

            // Check if computed CRC matches the one in the packet
            return crc.SequenceEqual(crcFromPacket);
        }
    }
}
