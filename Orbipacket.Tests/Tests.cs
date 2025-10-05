using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orbipacket.Library;

namespace Orbipacket.Tests
{
    [TestClass]
    public class PacketTests
    {
        private readonly byte[] NOISE_DATA = [0xA1, 0xA2, 0xA3, 0xA4, 0xA5];

        /// <summary>
        /// Tests sending just one full packet, with valid CRC and termination bytes.
        /// </summary>
        /// <param name="device"></param>
        /// <exception cref="ArgumentException"></exception>
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

            byte[] byteArray =
                "04-01-0b-84-15-94-49-2a-e2-aa-61-18-48-65-6c-6c-6f-20-57-6f-72-6c-64-5b-1a"
                    .Split('-')
                    .Select(s => Convert.ToByte(s, 16))
                    .ToArray();

            var packet = Decode.GetPacketInformation(byteArray);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {Encoding.ASCII.GetString(packet.Payload.Value)}, Type: {packet.Type}"
            );
        }

        /// <summary>
        /// Test sending a packet with noise data.
        /// This proves that the implementation is
        /// able to distinguish between real data and uncomplete packets.
        /// </summary>
        /// <param name="device">Device type. Can either be pressure, temperature or humidity.</param>
        /// <exception cref="ArgumentException"></exception>

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
            // Calculate CRC
            byte[] crc = Crc16.GetCRC(packetData);
            // Append CRC to the packet data
            packetData = [.. packetData, .. crc];
            // Encode the data with COBS (because otherwise we'd catch a 0x00 byte in the middle of the packet)
            byte[] encodedDataBeforeNoise = [.. COBS.Encode(packetData)];

            // Add noise to the encoded data
            byte[] noisyData =
            [
                .. NOISE_DATA,
                .. new byte[] { 0x00 },
                .. encodedDataBeforeNoise,
                .. new byte[] { 0x00 },
            ];
            buffer.Add(noisyData);

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

            // Decode the packet to get the information
            var packet = Decode.GetPacketInformation(extractedPacket);
            Console.WriteLine(
                $"Decoded Packet: DeviceId: {packet.DeviceId}, Timestamp: {packet.Timestamp}, Payload: {packet.Payload}, Type: {packet.Type}"
            );
        }

        /// <summary>
        ///  Create a single packet, with variable devices.
        /// </summary>
        /// <param name="device">The device type to test.
        /// Either pressure, temperature or humidity.</param>
        /// <returns>Encoded packet data, along with valid CRC bytes.</returns>
        /// <exception cref="ArgumentException"></exception>

        public static byte[] CreatePacket(string device)
        {
            byte control;
            byte[] payload;
            // Different device types for testing
            if (device == "pressure")
            {
                control = 0b0_00010_00; // Control byte for pressure
                payload = "100321.05"u8.ToArray(); // Example payload for pressure
            }
            else if (device == "temperature")
            {
                control = 0b0_00011_00; // Control byte for temperature
                payload = "21.32"u8.ToArray(); // Example payload for temperature
            }
            else if (device == "humidity")
            {
                control = 0b0_00100_00; // Control byte for humidity
                payload = "40.9202"u8.ToArray(); // Example payload for humidity
            }
            else
            {
                throw new ArgumentException("Invalid device type");
            }

            // Fetch current timestamp in nanoseconds (since Unix epoch)
            ulong timestamp = (ulong)(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds * 1_000_000
            );
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // Create a packet with the specified device type and payload
            byte[] packetData =
            [
                0x01,
                (byte)payload.Length,
                control,
                .. timestampBytes,
                .. payload,
            ];

            byte[] crc = Crc16.GetCRC(packetData);
            // Append CRC to the packet data
            packetData = [.. packetData, .. crc];
            byte[] encodedDataBeforeNoise = [.. COBS.Encode(packetData)];
            byte[] dataWithTerminationByte = [.. encodedDataBeforeNoise, .. new byte[] { 0x00 }];
            Console.WriteLine(BitConverter.ToString(encodedDataBeforeNoise));
            return dataWithTerminationByte;
        }

        /// <summary>
        /// Handles multiple packets.
        /// Proves that the implementation is able to distinguish different packets.
        /// </summary>
        [TestMethod]
        public void HandleMultiplePackets()
        {
            PacketBuffer buffer = new();

            // Create multiple packets with different device types and payloads
            byte[] packet1 = CreatePacket("pressure");
            byte[] packet2 = CreatePacket("temperature");
            byte[] packet3 = CreatePacket("humidity");

            buffer.Add(packet1);

            buffer.Add(packet2);

            buffer.Add(packet3);

            // Loop ExtractFirstValidPacket() until no more valid packets can be extracted
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

        /// <summary>
        /// Handles packets, with embedded uncomplete packets along the way.
        /// Proves that the implementation is able to handle more than one packet coming in a byte array.
        /// </summary>
        [TestMethod]
        public void TestUncompletePacket()
        {
            PacketBuffer buffer = new();

            // Create multiple packets with different device types and payloads
            byte[] packet1 = CreatePacket("pressure");
            byte[] packet2 = CreatePacket("temperature");
            byte[] packet3 = CreatePacket("humidity");

            byte[] uncompletePacket1 = [.. packet1[..^10]]; // Simulate an uncomplete packet

            Console.WriteLine("Packet 1: " + BitConverter.ToString(packet1));
            Console.WriteLine("Packet 1 (uncomplete):" + BitConverter.ToString(uncompletePacket1));

            buffer.Add(uncompletePacket1);
            buffer.Add([0x00]);
            buffer.Add(packet1);

            buffer.Add(packet2);

            buffer.Add(packet3);

            // Loop ExtractFirstValidPacket() until no more valid packets can be extracted
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

        [TestMethod]
        public void TestEncoding()
        {
            byte[] payloadValue = Encoding.ASCII.GetBytes("Hello, World!");
            Payload payload = new(payloadValue);
            Packet packet = new(
                DeviceId.System,
                (ulong)DateTime.UtcNow.Ticks * 100_000,
                payload,
                Packet.PacketType.TcPacket
            );

            byte[] encoded = Encode.EncodePacket(packet);

            Console.WriteLine(BitConverter.ToString(encoded));

            PacketBuffer buffer = new();
            buffer.Add(encoded);
            byte[] extractedPacket = buffer.ExtractFirstValidPacket();

            Packet? decoded = Decode.GetPacketInformation(extractedPacket);

            Console.WriteLine(Encoding.ASCII.GetString(decoded.Payload.Value));
        }

        [TestMethod]
        public void BruteForceTest()
        {
            Random random = new();
            byte[] packetData = new byte[240];
            random.NextBytes(packetData);
            byte[] packet = Encode.EncodePacket(
                new(
                    DeviceId.System,
                    (ulong)DateTime.UtcNow.Ticks * 100,
                    new Payload(packetData),
                    Packet.PacketType.TcPacket
                )
            );
            PacketBuffer buffer = new();
            Stopwatch sw = new();
            sw.Start();
            for (int i = 0; i < 1_000_000; i++)
            {
                buffer.Add(packet);
            }
            sw.Stop();
            Console.WriteLine($"Time taken to add 100,000 packets: {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            sw.Start();
            Console.WriteLine("Starting extraction...");
            byte[] extractedPacket;
            int packetsAnalzyed = 0;
            while ((extractedPacket = buffer.ExtractFirstValidPacket()) != null)
            {
                var decoded = Decode.GetPacketInformation(extractedPacket);
                packetsAnalzyed++;
            }
            sw.Stop();
            Console.WriteLine(
                $"Time taken to extract {packetsAnalzyed} packets: {sw.Elapsed}, for a total of {packetsAnalzyed / sw.Elapsed.TotalSeconds} packets/second and a throughput of {(packetsAnalzyed * packet.Length / sw.Elapsed.TotalSeconds) / 1024} KBytes/s"
            );
        }

        [TestMethod]
        public void Noise()
        {
            Random random = new();
            byte[] noise = new byte[500];
            random.NextBytes(noise);
            PacketBuffer buffer = new();
            buffer.Add(noise);
            byte[] packet = CreatePacket("pressure");
            buffer.Add([0x00]);
            buffer.Add(packet);
            buffer.Add(packet);
            buffer.Add(noise);
            byte[] extractedPacket;
            int packetsAnalzyed = 0;
            while ((extractedPacket = buffer.ExtractFirstValidPacket()) != null)
            {
                var decoded = Decode.GetPacketInformation(extractedPacket);
                Console.WriteLine(
                    $"Decoded Packet: DeviceId: {decoded.DeviceId}, "
                        + $"Timestamp: {decoded.Timestamp}, "
                        + $"Payload: {decoded.Payload}, "
                        + $"Type: {decoded.Type}"
                );
                packetsAnalzyed++;
            }
            Assert.IsTrue(packetsAnalzyed == 2);
        }
    }
}
