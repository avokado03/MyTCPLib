using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyTCPLib.Server
{
    /// <summary>
    /// Логика прослушивания сервера.
    /// </summary>
    internal class TcpListenerService
    {
        #region Fields
        /// <summary>
        /// Экземпляр класса прослушки.
        /// </summary>
        private CustomTcpListener _listener = null;

        /// <summary>
        /// Соединенные клиенты.
        /// </summary>
        private List<TcpClient> _connectedClients = new List<TcpClient>();

        /// <summary>
        /// Отсоединенные клиенты.
        /// </summary>
        private List<TcpClient> _disconnectedClients = new List<TcpClient>();

        /// <summary>
        /// Сервер, который прослушивается.
        /// </summary>
        private MyTcpServer _parentServer = null;

        /// <summary>
        /// Список сообщений.
        /// </summary>
        private List<byte> _listMsg = new List<byte>();
        #endregion

        #region Properties
        /// <summary>
        /// Список соединенных клиентов - геттер.
        /// </summary>
        public IEnumerable<TcpClient> ConnectedClients { get { return _connectedClients; } }

        /// <summary>
        /// Список исключений, вызванных во время прослушивания.
        /// </summary>
        public List<Exception> LoopExceptions { get; private set; }

        /// <summary>
        /// Флаг остановки прослушивания.
        /// </summary>
        internal bool Stop { get; set; }

        /// <summary>
        /// Прослушиваемый адрес.
        /// </summary>
        internal IPAddress IPAddress { get; private set; }

        /// <summary>
        /// Прослушиваемый порт.
        /// </summary>
        internal int Port { get; private set; }

        /// <summary>
        /// Интервал чтения сообщений (ms).
        /// </summary>
        internal int ReadMessageInterval { get; set; }

        /// <summary>
        /// Экземпляр класса прослушки.
        /// </summary>
        internal CustomTcpListener Listener { get { return _listener; } }
        #endregion

        /// <summary>
        /// Конструктор.
        /// </summary>
        internal TcpListenerService(MyTcpServer parentServer, IPAddress ipAddress, int port)
        {
            Stop = false;
            _parentServer = parentServer;
            IPAddress = ipAddress;
            Port = port;
            ReadMessageInterval = 10;
            LoopExceptions = new List<Exception>();

            _listener = new CustomTcpListener(ipAddress, port);
            _listener.Start();

            // Когда приходит запрос, выделяется новый поток, которому запрос передается на обработку.
            // Обслуживая эти запросы в нескольких потоках, 
            // сервер достигает высокой степени параллелизма и оптимальной утилизации.
            // http://crypto.pp.ua/2011/01/ispolzovanie-threadpool/
            ThreadPool.QueueUserWorkItem(ListenerLoop);
        }
		
        /// <summary>
        /// Цикл прослушивания соединений.
        /// </summary>
	    private void ListenerLoop(object state)
        {
            while (!Stop)
            {
                try
                {
                    RunLoopStep();
                }
                catch (Exception ex)
                {
                    LoopExceptions.Add(ex);
                }

                Thread.Sleep(ReadMessageInterval);
            }
	    	_listener.Stop();
        }

        /// <summary>
        /// Проверяет, что сокет все еще подключен.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        /// </remarks>
        bool IsSocketConnected(Socket s)
	    {	        
	        bool part1 = s.Poll(1000, SelectMode.SelectRead);
	        bool part2 = (s.Available == 0);
	        if ((part1 && part2) || !s.Connected)
	    	return false;
	        else
	    	return true;
	    }

	    /// <summary>
        /// Итерация для <see cref="ListenerLoop(object)"/>
        /// </summary>
        private void RunLoopStep()
        {
            //оповещение отсоединенных клиентов 
            if (_disconnectedClients.Count > 0)
            {
                var disconnectedClients = _disconnectedClients.ToArray();
                _disconnectedClients.Clear();

                foreach (var client in disconnectedClients)
                {
                    _connectedClients.Remove(client);
                    _parentServer.NotifyClientDisconnected(this, client);
                }
            }

            //принимаем подключение и оповещаем клиент о соединении
            if (_listener.Pending())
            {
				var newClient = _listener.AcceptTcpClient();
				_connectedClients.Add(newClient);
                _parentServer.NotifyClientConnected(this, newClient);
            }

            foreach (var c in _connectedClients)
            {
		        // находим отсоединенные клиенты, у которых сокет не подключен
		        if ( IsSocketConnected(c.Client) == false)
                {
                    _disconnectedClients.Add(c);
                }
		        
                // получаем сообщения на обработку
                int bytesAvailable = c.Available;
                if (bytesAvailable == 0)
                {
                    continue;
                }

                List<byte> bytesReceived = new List<byte>();

                while (c.Available > 0 && c.Connected)
                {
                    byte[] nextByte = new byte[1];
                    c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
                    bytesReceived.AddRange(nextByte);
                    _listMsg.AddRange(nextByte);
                }

                // оповещаем поток из пула, что обработка сообщений закончена
                // и можно отправлять сообщение
                if (bytesReceived.Count > 0)
                {
                    _parentServer.NotifyThread(this, c, bytesReceived.ToArray());
                }  
            }
        }
    }
}
