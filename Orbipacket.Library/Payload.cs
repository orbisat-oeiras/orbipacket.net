namespace Orbipacket
{
    public class Payload
    {
        public byte[] Value { get; init; }

        public Payload(byte[] payload)
        {
            Value = payload;
        }

        public byte Length()
        {
            return (byte)Value.Length;
        }

        public override string ToString()
        {
            if (Value.Length == 0)
            {
                return string.Empty;
            }
            // Convert byte array to hex string
            string hexString = BitConverter.ToString(Value).Replace("-", "");
            return hexString;
        }
    }
}
