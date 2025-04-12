using System;
using System.Linq;
using Orbipacket;
using Orbipacket.Library;

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

        public override string ToString()
        {
            byte[] payloadArray = _payload.Split('-').Select(s => Convert.ToByte(s, 16)).ToArray();
            string payloadmessage = System.Text.Encoding.ASCII.GetString(payloadArray);
            return payloadmessage;
        }
    }
}
