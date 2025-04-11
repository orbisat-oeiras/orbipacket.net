namespace Orbipacket
{
    public class Payload
    {
        public System.UInt128 _payload { get; init; }

        public Payload(System.UInt128 payload)
        {
            _payload = payload;
        }
        public static byte Length()
        {
            return 16;
        }
    }
}
