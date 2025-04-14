using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orbipacket.Library;

namespace Orbipacket.Tests
{
    [TestClass]
    public class PacketTests
    {
        [TestMethod]
        [DataRow("pressure")]
        [DataRow("temperature")]
        [DataRow("humidity")]
        public void TestPacket(string device)
        {
            byte control;
            byte[] payload;

            if (device == "pressure")
            {
                control = 0b1_00000_00; // Control byte for pressure
                payload = "100321.05"u8.ToArray(); // Example payload for pressure
            }
            else if (device == "temperature")
            {
                control = 0b1_00001_00; // Control byte for temperature
                payload = "21.32"u8.ToArray(); // Example payload for temperature
            }
            else if (device == "humidity")
            {
                control = 0b1_00010_00; // Control byte for humidity
                payload = "40.9202"u8.ToArray(); // Example payload for humidity
            }
            else
            {
                throw new ArgumentException("Invalid device type");
            }

            ulong timestamp = (ulong)(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds * 1000000
            );

            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            byte[] packetData =
            [
                0x01,
                (byte)payload.Length,
                control,
                .. timestampBytes,
                .. payload,
            ];
            // Calculate CRC
            byte[] crc = Crc16.GetCRC(packetData);
            // Append CRC and termination byte to the packet data
            packetData = [.. packetData, .. crc, .. new byte[] { 0x00 }];

            Console.WriteLine($"Packet data: {BitConverter.ToString(packetData)}");
            byte[] encodedData = [.. COBS.Encode(packetData[..^1])];
            var packet = Decode.GetPacketInformation(encodedData);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {packet.Payload}, Type: {packet.Type}"
            );
        }
    }
}
