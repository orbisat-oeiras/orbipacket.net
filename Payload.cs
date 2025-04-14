namespace Orbipacket
{
    public class Payload
    {
        public string Value { get; init; }

        public Payload(string payload)
        {
            Value = payload;
        }

        public byte Length()
        {
            return (byte)Value.Length;
        }

        public override string ToString()
        {
            byte[] payloadArray = [.. Value.Split('-').Select(s => Convert.ToByte(s, 16))];
            string payloadMessage = System.Text.Encoding.ASCII.GetString(payloadArray);
            return payloadMessage;
        }
    }
}
