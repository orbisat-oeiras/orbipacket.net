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
                control = 0b0_00000_00; // Control byte for pressure
                payload = "100321.05"u8.ToArray(); // Example payload for pressure
            }
            else if (device == "temperature")
            {
                control = 0b0_00001_00; // Control byte for temperature
                payload = "21.32"u8.ToArray(); // Example payload for temperature
            }
            else if (device == "humidity")
            {
                control = 0b0_00010_00; // Control byte for humidity
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
            // Different device types for testing
            if (device == "pressure")
            {
                control = 0b0_00000_00; // Control byte for pressure
                payload = "100321.05"u8.ToArray(); // Example payload for pressure
            }
            else if (device == "temperature")
            {
                control = 0b0_00001_00; // Control byte for temperature
                payload = "21.32"u8.ToArray(); // Example payload for temperature
            }
            else if (device == "humidity")
            {
                control = 0b0_00010_00; // Control byte for humidity
                payload = "40.9202"u8.ToArray(); // Example payload for humidity
            }
            else
            {
                throw new ArgumentException("Invalid device type");
            }

            // Fetch current timestamp in nanoseconds (since Unix epoch)
            ulong timestamp = (ulong)(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds * 1000000
            );
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // Create a new packet buffer
            var buffer = new PacketBuffer();

            // Create a packet with the specified device type and payload
            byte[] packetData =
            [
                0x01,
                (byte)payload.Length,
                control,
                .. timestampBytes,
                .. payload,
            ];

            // Encode the data with COBS (because otherwise we'd catch a 0x00 byte in the middle of the packet)
            byte[] encodedDataBeforeNoise = [.. COBS.Encode(packetData)];

            // Add noise to the encoded data
            byte[] noisyData = [.. NOISE_DATA, .. new byte[] { 0x00 }, .. encodedDataBeforeNoise];

            // Add the noisy data to the buffer
            buffer.Add(noisyData);
            buffer.Add([0x00]);
            // Extract the first valid packet from the buffer
            byte[] extractedPacket = buffer.ExtractFirstValidPacket();
            // Check if the extracted packet is valid
            if (extractedPacket == null)
            {
                Assert.Fail("No valid packet was extracted from the buffer");
            }

            // Console.WriteLine($"Extracted Packet: {BitConverter.ToString(extractedPacket)}");
            // Check if the extracted packet is valid
            // if (extractedPacket != packetData)
            // {
            //     Assert.Fail("Extracted packet does not match the original packet data");
            // }

            // Now we can decode the packet extracted from the buffer to calculate the CRC
            extractedPacket = [.. COBS.Decode(extractedPacket)];

            // Calculate CRC
            byte[] crc = Crc16.GetCRC(extractedPacket);

            // Append CRC and termination byte to the packet data
            packetData = [.. extractedPacket, .. crc, .. new byte[] { 0x00 }];

            // Console.WriteLine($"Packet data: {BitConverter.ToString(packetData)}");

            // Encode the data again with COBS (The Decode class expects COBS encoded data)
            byte[] encodedData = [.. COBS.Encode(packetData[..^1])];

            // Decode the packet to get the information
            var packet = Decode.GetPacketInformation(encodedData);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {packet.Payload}, Type: {packet.Type}"
            );
        }

        private byte[] CreatePacket(string device)
        {
            byte control;
            byte[] payload;
            // Different device types for testing
            if (device == "pressure")
            {
                control = 0b0_00000_00; // Control byte for pressure
                payload = "100321.05"u8.ToArray(); // Example payload for pressure
            }
            else if (device == "temperature")
            {
                control = 0b0_00001_00; // Control byte for temperature
                payload = "21.32"u8.ToArray(); // Example payload for temperature
            }
            else if (device == "humidity")
            {
                control = 0b0_00010_00; // Control byte for humidity
                payload = "40.9202"u8.ToArray(); // Example payload for humidity
            }
            else
            {
                throw new ArgumentException("Invalid device type");
            }

            // Fetch current timestamp in nanoseconds (since Unix epoch)
            ulong timestamp = (ulong)(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds * 1000000
            );
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // Create a new packet buffer
            var buffer = new PacketBuffer();

            // Create a packet with the specified device type and payload
            byte[] packetData =
            [
                0x01,
                (byte)payload.Length,
                control,
                .. timestampBytes,
                .. payload,
            ];

            // Encode the data with COBS (because otherwise we'd catch a 0x00 byte in the middle of the packet)

            byte[] crc = Crc16.GetCRC(packetData);
            // Append CRC to the packet data
            packetData = [.. packetData, .. crc];

            byte[] encodedDataBeforeNoise = [.. COBS.Encode(packetData)];
            return encodedDataBeforeNoise;
        }

        [TestMethod]
        public void HandleMultiplePackets()
        {
            // Create a new packet buffer
            var buffer = new PacketBuffer();

            // Create multiple packets with different device types and payloads
            byte[] packet1 = CreatePacket("pressure");
            byte[] packet2 = CreatePacket("temperature");
            byte[] packet3 = CreatePacket("humidity");

            buffer.Add([0x00]);
            buffer.Add(packet1);
            buffer.Add([0x00]);
            buffer.Add(packet2);
            buffer.Add([0x00]);
            buffer.Add(packet3);
            buffer.Add([0x00]);

            Console.WriteLine("Buffer contents:");
            byte[] bufferArray = [.. buffer._buffer];
            Console.WriteLine($"Buffer size: {bufferArray.Length} bytes");
            Console.WriteLine($"Raw buffer: {BitConverter.ToString(bufferArray)}");

            // Extract the first valid packet from the buffer
            byte[] extractedPacket;
            while ((extractedPacket = buffer.ExtractFirstValidPacket()) != null)
            {
                var packet = Decode.GetPacketInformation(extractedPacket);
                Console.WriteLine(
                    $"Decoded Packet: DeviceId: {packet.DeviceId}, "
                        + $"Timestamp: {packet.Timestamp}, "
                        + $"Payload: {packet.Payload}, "
                        + $"Type: {packet.Type}"
                );
            }
        }
    }
}
