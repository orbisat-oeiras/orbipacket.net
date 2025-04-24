using System.Runtime.CompilerServices;
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

        public void Add(byte[] data)
        {
            foreach (var byteData in data)
            {
                _buffer.Enqueue(byteData);
            }
        }

        public byte[]? ExtractFirstValidPacket()
        {
            while (_buffer.Count >= 13)
            {
                byte[] bufferArray = [.. _buffer];

                // Find the first occurrence of the termination byte
                int startIndex = Array.IndexOf(bufferArray, Decode._terminationByte);

                if (startIndex == -1 || startIndex == bufferArray.Length - 1)
                {
                    _buffer.Clear(); // Clear invalid data
                    return null;
                }

                // Find the next termination byte
                int endIndex = Array.IndexOf(bufferArray, Decode._terminationByte, startIndex + 1);

                // If no second termination byte found, packet is incomplete
                if (endIndex == -1)
                {
                    // Keep everything from the first termination byte onwards
                    _buffer = new Queue<byte>(bufferArray[startIndex..]);
                    return null; // Wait for more data
                }

                // Extract packet data (everything up to but not including the termination byte)
                byte[] packetData = bufferArray[(startIndex + 1)..endIndex];

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
