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
            if (_buffer.Count < 13)
                return null; // Not enough data for a valid packet

            byte[] bufferArray = [.. _buffer];

            // Find the first occurrence of the termination byte
            int startIndex = -1;

            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (bufferArray[i] == Decode._terminationByte)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex == -1 || startIndex == bufferArray.Length - 1)
                return null; // Termination byte not found

            // Find next termination byte

            int endIndex = -1;
            for (int i = startIndex + 1; i < bufferArray.Length; i++)
            {
                if (bufferArray[i] == Decode._terminationByte)
                {
                    endIndex = i;
                    break;
                }
            }

            // If no second termination byte found, packet is incomplete
            if (endIndex == -1)
            {
                // Keep everything from the first termination byte onwards
                _buffer = new Queue<byte>(bufferArray[startIndex..]);
                return null;
            }

            // Extract the packet between termination bytes
            byte[] packetData = bufferArray[(startIndex + 1)..endIndex];

            Console.WriteLine(
                "Current buffer contents: " + BitConverter.ToString(_buffer.ToArray())
            );

            if (packetData.Length < 13 || !CheckIfCRCIsValid(packetData))
            {
                // CRC check failed, discard packet
                Console.WriteLine("CRC check failed, discarding packet.");
                Console.WriteLine("Failed packet: " + BitConverter.ToString(packetData));
                _buffer = new Queue<byte>(bufferArray[endIndex..]);
                return ExtractFirstValidPacket();
            }

            // Keep remaining data in buffer
            _buffer = new Queue<byte>(bufferArray[endIndex..]);

            return packetData;
        }

        public static bool CheckIfCRCIsValid(byte[] packetData)
        {
            // Decode packet data using COBS
            byte[] decodedData = [.. COBS.Decode(packetData)];
            // Compute CRC of packet data (without CRC bytes)
            byte[] crc = Crc16.GetCRC(decodedData[..^2]);

            // Extract CRC from packet
            byte[] crcFromPacket = decodedData[^2..];

            // Check if computed CRC matches the one in the packet
            return crc.SequenceEqual(crcFromPacket);
        }
    }
}
