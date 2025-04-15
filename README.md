# Orbipacket Protocol - C# Implementation
[Orbipacket](https://github.com/orbisat-oeiras/orbipacket) is a communication protocol developed by OrbiSat Oeiras for communication with CanSat devices. This is an implementation of that protocol in C#.

## Nuget


## Installation
To install the Orbipacket protocol, simply add the package to your .csproj with:
```bash
dotnet add package Orbipacket
```
## Usage
You can find different uses in the [Tests.cs](https://github.com/orbisat-oeiras/orbipacket.net/blob/main/Tests.cs) file. For example, to add packets to the buffer and analyze them:
```csharp
PacketBuffer buffer = new();
byte[] packet1;
byte[] packet2;
byte[] packet3;

buffer.Add([0x00]);
buffer.Add(packet1);
buffer.Add([0x00]);
buffer.Add(packet2);
buffer.Add([0x00]);
buffer.Add(packet3);
buffer.Add([0x00]);

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
```
### Output
```
Decoded Packet: DeviceId: DeviceId1, Timestamp: Timestamp1, Payload: Payload1; Type: Type1
Decoded Packet: DeviceId: DeviceId2, Timestamp: Timestamp2, Payload: Payload2; Type: Type2
Decoded Packet: DeviceId: DeviceId3, Timestamp: Timestamp3, Payload: Payload3; Type: Type3
```
