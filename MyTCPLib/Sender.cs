using System.Net.Sockets;
using System.Text;

namespace MyTCPLib
{
    public class Sender
    {
        private TcpClient _tcpClient;
        private Encoding _encoder = null;

        internal Sender(byte[] data, TcpClient tcpClient, Encoding stringEncoder)
        {
            Data = data;
            _tcpClient = tcpClient;
            _encoder = stringEncoder;
        }

        public byte[] Data { get; private set; }
        public string DataString
        {
            get
            {
                return _encoder.GetString(Data);
            }
        }

        public void Send(byte[] data)
        {
            _tcpClient.GetStream().Write(data, 0, data.Length);
        }

        public void Send(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }
            Send(_encoder.GetBytes(data));
        }

        public TcpClient TcpClient {  get { return _tcpClient; } }
    }
}
