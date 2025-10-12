using System.Collections.Generic;
using Orbipacket.Library;

namespace Orbipacket
{
    public class PacketBuffer
    {
        public List<byte> _buffer = [];
        private const int minPacketSize = 13;
        private int readPosition = 0;

        public PacketBuffer()
        {
            _buffer = [];
        }

        /// <summary>
        /// Adds a byte array to the buffer.
        /// </summary>
        public void Add(byte[] data)
        {
            if (readPosition > 0)
            {
                _buffer.RemoveRange(0, readPosition);
                readPosition = 0;
            }
            _buffer.AddRange(data);
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
            while (readPosition < _buffer.Count)
            {
                // Find first 0x00 byte
                int i = _buffer.IndexOf(0x00, readPosition);

                // Check if everything before the first 0x00 byte is also a valid packet
                if (i - readPosition >= minPacketSize)
                {
                    byte[] packetData = [.. _buffer.GetRange(readPosition, i - readPosition)];
                    packetData = Decode.DecodeCobsAndValidate(packetData);
                    if (packetData != null)
                    {
                        Console.WriteLine("Packet without 0x00 byte found");
                        readPosition = i; // Leave last 0x00 byte for next search
                        return packetData;
                    }
                }
                if (i == -1)
                {
                    // No 0x00 byte found
                    readPosition = _buffer.Count;
                    return null;
                }
                // Find next 0x00 byte
                int j = _buffer.IndexOf(0x00, i + 1);
                if (j == -1)
                {
                    // No second 0x00 byte found: two cases

                    // CASE 1: Everything after 0x00 is a valid packet
                    if (_buffer.Count - i - 1 >= minPacketSize)
                    {
                        byte[] packetData = [.. _buffer.GetRange(i + 1, _buffer.Count - i - 1)];
                        packetData = Decode.DecodeCobsAndValidate(packetData);
                        if (packetData != null)
                        {
                            Console.WriteLine("Packet with initial 0x00 byte found");
                            readPosition = _buffer.Count; // Move read position to the end
                            return packetData;
                        }
                    }
                    // CASE 2: Everything before 0x00 is a valid packet
                    if (i - readPosition >= minPacketSize)
                    {
                        byte[] packetData = [.. _buffer.GetRange(readPosition, i - readPosition)];
                        packetData = Decode.DecodeCobsAndValidate(packetData);
                        if (packetData != null)
                        {
                            Console.WriteLine("Packet with trailing 0x00 byte found");
                            readPosition = i; // Leave last 0x00 byte for next search
                            return packetData;
                        }
                    }
                    readPosition = _buffer.Count; // No valid packets found, wait for more data
                    break;
                }

                // Both 0x00 bytes found, check if the data in between is a valid packet
                if (j - i - 1 >= minPacketSize)
                {
                    byte[] completePacket = [.. _buffer.GetRange(i + 1, j - i - 1)];
                    completePacket = Decode.DecodeCobsAndValidate(completePacket);
                    if (completePacket != null)
                    {
                        readPosition = j;
                        return completePacket;
                    }
                }
                // No valid packets found, wait for more data
                readPosition = i + 1; // Move past the first 0x00 byte
            }
            return null;
        }

        public void Clear()
        {
            _buffer.Clear();
            readPosition = 0;
        }

        public int Length()
        {
            return _buffer.Count;
        }

        public void PrintBuffer()
        {
            Console.WriteLine(BitConverter.ToString([.. _buffer]).Replace("-", " "));
        }

        public byte[] GetBuffer()
        {
            return [.. _buffer];
        }

        public List<byte[]> GetAllPackets()
        {
            List<byte[]> packets = [];
            byte[]? packet;
            while ((packet = ExtractFirstValidPacket()) != null)
            {
                packets.Add(packet);
            }
            return packets;
        }
    }
}
