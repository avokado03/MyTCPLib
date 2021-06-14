using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MyTCPLib.Server
{
    /// <summary>
    /// Класс сервера для работы с TCP.
    /// </summary>
    public class MyTcpServer
    {
        #region Fields and props
        /// <summary>
        /// Список прослушивателей.
        /// </summary>
        private List<ServerListener> _listeners = new List<ServerListener>();

        /// <summary>
        /// Кодировка сообщений.
        /// </summary>
        public Encoding StringEncoder { get; set; }

        /// <summary>
        /// Событие "клиент подключен".
        /// </summary>
        public event EventHandler<TcpClient> ClientConnected;

        /// <summary>
        /// Событие "клиент отключен".
        /// </summary>
        public event EventHandler<TcpClient> ClientDisconnected;

        /// <summary>
        /// Событие "данные отправлены".
        /// </summary>
        public event EventHandler<Sender> DataReceived;
        #endregion

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MyTcpServer()
        {
            StringEncoder = Encoding.UTF8;
        }

        
        /// <summary>
        /// Оповещает все подключенные клиенты сообщением.
        /// </summary>
        public void Broadcast(byte[] data)
        {
            foreach(var client in _listeners.SelectMany(x => x.ConnectedClients))
            {
                client.GetStream().Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Перегрузка для сообщения типа string <see cref="Broadcast(byte[])"/>.
        /// </summary>
        public void Broadcast(string data)
        {
            if (data == null) { return; }
            Broadcast(StringEncoder.GetBytes(data));
        }

        /// <summary>
        /// Получение конфигураций сетевых интерфейсов.
        /// </summary>
        private static IEnumerable<NetworkInterface> TryGetCurrentNetworkInterfaces()
        {
            try
            {
                //возвращаем только работающие интерфейсы, которые могут передавать пакеты
                return NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up);
            }
            catch (NetworkInformationException)
            {
                return Enumerable.Empty<NetworkInterface>();
            }
        }

        /// <summary>
        /// Запускает сервер.
        /// </summary>
		public MyTcpServer Start(IPAddress ipAddress, int port)
        {
            ServerListener listener = new ServerListener(this, ipAddress, port);
            _listeners.Add(listener);

            return this;
        }

        /// <summary>
        /// Останавливает сервер.
        /// </summary>
        public void Stop()
        {
			_listeners.All(l => l.Stop = true);
			while (_listeners.Any(l => l.Listener.Active)){
				Thread.Sleep(100);
			};
            _listeners.Clear();
        }

        /// <summary>
        /// Сообщает потоку из пула, что сообщения обработаны.
        /// </summary>
        internal void NotifyEndTransmissionRx(ServerListener listener, TcpClient client, byte[] msg)
        {
            if (DataReceived != null)
            {
                Sender m = new Sender(msg, client, StringEncoder);
                DataReceived(this, m);
            }
        }

        /// <summary>
        /// Сообщает о подключении нового клиента.
        /// </summary>
        internal void NotifyClientConnected(ServerListener listener, TcpClient newClient)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, newClient);
            }
        }

        /// <summary>
        /// Сообщает об отключении клиента.
        /// </summary>
        internal void NotifyClientDisconnected(ServerListener listener, TcpClient disconnectedClient)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, disconnectedClient);
            }
        }
	}
}
