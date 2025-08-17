using System.Collections.Generic;
using Orbipacket.Library;

namespace Orbipacket
{
    public class PacketBuffer
    {
        public List<byte> _buffer = [];
        private const int minPacketSize = 13;

        public PacketBuffer()
        {
            _buffer = [];
        }

        /// <summary>
        /// Adds a byte array to the buffer.
        /// </summary>
        public void Add(byte[] data) => _buffer.AddRange(data);

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
            int bufferSize = _buffer.Count;

            for (int i = 0; i < bufferSize; i++)
            {
                if (_buffer[i] != Decode._terminationByte)
                    continue; // Start over if not termination byte

                int j = i + 1;

                // Browse for the next termination byte
                while (j < bufferSize && _buffer[j] != Decode._terminationByte)
                {
                    j++;
                }

                // No second termination byte found
                if (j >= bufferSize)
                {
                    // CASE 1: Packet doesn't have initial 0x00 byte, only termination 0x00
                    if (i > minPacketSize && i <= 254)
                    {
                        // We can check if the packet before that termination byte is still valid
                        byte[] previousPacket = [.. _buffer.GetRange(0, i)];
                        if (IsCRCValid(previousPacket))
                        {
                            _buffer.RemoveRange(0, i);
                            return previousPacket;
                        }
                        return null;
                    }
                    
                    // CASE 2: Packet has an initial 0x00 byte, but doesn't end with one
                    byte[] packetData = [.. _buffer.GetRange(i + 1, bufferSize - i - 1)];
                    // index i + 1 because we're excluding the initial 0x00 byte,
                    // starting at the version byte.
                    // We then take (bufferSize - i - 1), which will be the size of the packet.
                    
                    
                    if (packetData.Length >= minPacketSize && IsCRCValid(packetData))
                    {
                        // Remove the packet from the buffer
                        _buffer.RemoveRange(i, bufferSize - i);
                        return packetData;
                    }
                    return null;
                }

                byte[] completePacket = [.. _buffer.GetRange(i + 1, j - i - 1)]; // j - i is the length of the packet, - 1 to exclude the termination byte

                if (completePacket.Length >= minPacketSize && IsCRCValid(completePacket))
                {
                    // Remove the packet from the buffer
                    _buffer.RemoveRange(0, j);
                    return completePacket;
                }
            }
            return null;
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
