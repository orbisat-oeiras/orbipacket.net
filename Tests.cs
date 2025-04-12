using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orbipacket;
using Orbipacket.Library;

namespace Orbipacket.Tests
{
    [TestClass]
    public class PacketTests
    {
        [TestMethod]
        public void TestPacket()
        {
            byte[] packetData =
            [
                0x01, // Version
                16, // Length
                0b1_00001_00, // Control (TC packet)}
                0b00000000,
                0b00111101,
                0b11111001,
                0b01010101,
                0b10010000,
                0b01101101,
                0b11000111,
                0b10000000, // Simulated Timestamp - Unix time in nanoseconds (8 bytes)
                0x54,
                0x45,
                0x53,
                0x54,
                0x49,
                0x4E,
                0x47,
                0x50,
                0x41,
                0x59,
                0x4C,
                0x4F,
                0x41,
                0x44,
                0x30,
                0x30,
                0x1F,
                0x7B,
                0x00, // Termination byte
            ];
            Console.WriteLine($"Packet data: {BitConverter.ToString(packetData)}");
            byte[] encodedData = COBS.Encode(packetData.Take(packetData.Length - 1)).ToArray();
            var packet = Decode.GetPacketInformation(encodedData);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {packet.Payload.ToString()}, Type: {packet.Type}"
            );
        }
    }
}
