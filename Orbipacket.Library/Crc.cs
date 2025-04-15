using Crc;

namespace Orbipacket.Library
{
    public class Crc16 : Crc16Base
    {
        /// <summary>
        /// Provides the definition of CRC-16-OPENSAFETY-B (0x755b) for CRC calculation.
        /// </summary>
        public Crc16()
            : base(0x755b, 0x0000, 0x0000, false, false, 0x20fe) { }

        /// <summary>
        /// Computes the CRC for the provided packet data.
        /// </summary>
        /// <param name="packetData">The packet data received.</param>
        /// <returns></returns>
        public static byte[] GetCRC(byte[] packetData)
        {
            Crc16 crc = new();

            byte[] crcData = packetData;

            // Console.WriteLine("CRC computed.");

            byte[] result = crc.ComputeHash(crcData);

            // Reverse the byte order of the CRC result, for some reason this comes reversed (or maybe I'm just wrong)
            Array.Reverse(result);

            return result;
        }
    }
}
