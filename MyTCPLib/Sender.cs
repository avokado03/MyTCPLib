using System.Net.Sockets;
using System.Text;

namespace MyTCPLib
{
    /// <summary>
    /// Обработчик сообщения.
    /// </summary>
    public class Sender
    {
        /// <summary>
        /// Клиент.
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// Кодировка сообщения.
        /// </summary>
        private Encoding _encoder = null;

        /// <summary>
        /// Данные сообщения.
        /// </summary>
        public byte[] Data { get; private set; }
        public string DataString
        {
            get
            {
                return _encoder.GetString(Data);
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Sender(byte[] data, TcpClient tcpClient, Encoding stringEncoding)
        {
            Data = data;
            _tcpClient = tcpClient;
            _encoder = stringEncoding;
        }

        /// <summary>
        /// Записывает данные в сетевой поток клиента.
        /// </summary>
        public void Send(byte[] data)
        {
            _tcpClient.GetStream().Write(data, 0, data.Length);
        }

        /// <summary>
        /// Перегрузка для данных в строковом формате <see cref="Send(byte[])"/>.
        /// </summary>
        public void Send(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }
            Send(_encoder.GetBytes(data));
        }
    }
}
