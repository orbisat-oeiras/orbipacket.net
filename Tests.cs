using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orbipacket.Library;

namespace Orbipacket.Tests
{
    [TestClass]
    public class PacketTests
    {
        private readonly byte[] NOISE_DATA = [0xA1, 0xA2, 0xA3, 0xA4, 0xA5];

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

        [TestMethod]
        [DataRow("pressure")]
        [DataRow("temperature")]
        [DataRow("humidity")]
        public void TestPacketWithNoise(string device)
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

            var buffer = new PacketBuffer();

            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            byte[] packetData =
            [
                0x01,
                (byte)payload.Length,
                control,
                .. timestampBytes,
                .. payload,
            ];
            // Encode the data with COBS
            byte[] encodedDataBeforeNoise = [.. COBS.Encode(packetData)];

            // Add noise to the encoded data
            byte[] noisyData = [.. NOISE_DATA, .. new byte[] { 0x00 }, .. encodedDataBeforeNoise];

            Console.WriteLine("Packet data: " + BitConverter.ToString(packetData));
            // Add the data to the buffer
            buffer.Add(noisyData);
            Console.WriteLine("Buffer data: " + BitConverter.ToString(noisyData));
            // Extract the first valid packet from the buffer
            byte[] extractedPacket = buffer.ExtractFirstValidPacket();
            if (extractedPacket == null)
            {
                Assert.Fail("No valid packet was extracted from the buffer");
            }
            Console.WriteLine($"Extracted Packet: {BitConverter.ToString(extractedPacket)}");
            // Check if the extracted packet is valid
            // if (extractedPacket != packetData)
            // {
            //     Assert.Fail("Extracted packet does not match the original packet data");
            // }

            extractedPacket = [.. COBS.Decode(extractedPacket)];
            // Calculate CRC
            byte[] crc = Crc16.GetCRC(extractedPacket);
            // Append CRC and termination byte to the packet data
            packetData = [.. extractedPacket, .. crc, .. new byte[] { 0x00 }];

            Console.WriteLine($"Packet data: {BitConverter.ToString(packetData)}");
            byte[] encodedData = [.. COBS.Encode(packetData[..^1])];
            var packet = Decode.GetPacketInformation(encodedData);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {packet.Payload}, Type: {packet.Type}"
            );
        }
    }
}
