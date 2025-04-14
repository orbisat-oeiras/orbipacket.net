namespace Orbipacket
{
    public class PacketBuffer
    {
        public Queue<byte> _buffer = new();

        public PacketBuffer()
        {
            _buffer = new Queue<byte>();
        }

        public void Add(byte[] data)
        {
            foreach (var byteData in data)
            {
                _buffer.Enqueue(byteData);
            }
        }

        public byte[]? ExtractFirstValidPacket()
        {
            if (_buffer.Count < 13)
                return null; // Not enough data for a valid packet

            byte[] bufferArray = [.. _buffer];

            // Find the first occurrence of the termination byte
            int startIndex = -1;
            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (bufferArray[i] == Decode._terminationByte)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex == -1 || startIndex == bufferArray.Length - 1)
                return null; // Termination byte not found

            // Find next termination byte
            int endIndex = -1;
            for (int i = startIndex + 1; i < bufferArray.Length; i++)
            {
                if (bufferArray[i] == Decode._terminationByte)
                {
                    endIndex = i;
                    break;
                }
            }

            // If no second termination byte found, packet is incomplete
            if (endIndex == -1)
            {
                // Keep everything from the first termination byte onwards
                _buffer = new Queue<byte>(bufferArray[startIndex..]);
                return null;
            }

            // Extract the packet between termination bytes
            byte[] packetData = bufferArray[(startIndex + 1)..endIndex];

            // Keep remaining data in buffer
            _buffer = new Queue<byte>(bufferArray[(endIndex + 1)..]);

            return packetData;
        }
    }
}
