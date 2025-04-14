namespace Orbipacket
{
    public class PacketBuffer
    {
        private Queue<byte> _buffer = new();

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
            if (_buffer.Count == 0)
                return null;

            byte[] bufferArray = [.. _buffer];

            // Check for termination byte
            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (bufferArray[i] == Decode._terminationByte)
                {
                    // Extract the packet data after the termination byte
                    int packetLength = bufferArray.Length - (i + 1);
                    if (packetLength <= 0)
                        return null;

                    byte[] packetData = new byte[packetLength];
                    Array.Copy(bufferArray, i + 1, packetData, 0, packetLength);

                    // Update buffer to contain only the extracted data
                    _buffer = new Queue<byte>(packetData);

                    return packetData;
                }
            }
            return null;
        }
    }
}
