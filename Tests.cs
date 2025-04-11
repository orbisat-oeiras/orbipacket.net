using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orbipacket;

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
                27, // Length
                0b1_10100_00, // Control (TC packet)}
                0b11001111,
                0b11110010,
                0b00110101,
                0b11010000,
                0b00000000,
                0b00000000,
                0b00000000,
                0b00000000, // Timestamp (8 bytes)
                0x09,
                0x0A,
                0x0B,
                0x0C,
                0x0D,
                0x0E,
                0x0F,
                0x10, // Payload lower (8 bytes)
                0x11,
                0x12,
                0x13,
                0x14,
                0x15,
                0x16,
                0x17,
                0x18, // Payload upper (8 bytes)
            ];
            Decode decode = new();
            var packet = decode.GetPacketInformation(packetData);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {packet.Payload}, Type: {packet.Type}"
            );
        }
    }
}
