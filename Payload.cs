namespace Orbipacket
{
    public class Payload
    {
        public string _payload { get; init; }

        public Payload(string payload)
        {
            _payload = payload;
        }

        public byte Length()
        {
            return (byte)_payload.Length;
        }
        }
    }
}
