namespace Orbipacket
{
    public class Payload
    {
        public byte[] Value { get; init; }

        public Payload(byte[] value)
        {
            Value = value;
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
            return BitConverter.ToString(Value).Replace("-", "");
        }
    }
}
