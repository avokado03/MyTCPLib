using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using MyTCPLib.Common;

namespace MyTCPLib.Client
{
	/// <summary>
	/// Расширенный вариант TCP-клиента.
	/// </summary>
    public class MyTcpClient : IDisposable
	{
        #region Fields
		/// <summary>
		/// Отдельный поток для прослушивания.
		/// </summary>
        private Thread _rxThread = null;

		/// <summary>
		/// Список сообщений.
		/// </summary>
		private List<byte> _listMsg = new List<byte>();

		/// <summary>
		/// Клиент.
		/// </summary>
		private TcpClient _client = null;
		
		/// <summary>
		/// Событие "отправка сообщения".
		/// </summary>
		public event EventHandler<Sender> DataReceived;
		#endregion

		#region Properties
		/// <summary>
		/// Кодировка для сообщений.
		/// </summary>
		public Encoding StringEncoder { get; set; }

		/// <summary>
		/// Флаг остановки прослушивания.
		/// </summary>
		public bool Stop { get; set; }

		/// <summary>
		/// Интервал чтения.
		/// </summary>
		public int ReadLoopIntervalMs { get; set; }

		/// <summary>
		/// Геттер для получения экземпляра клиента.
		/// </summary>
		public TcpClient TcpClient { get { return _client; } }

		/// <summary>
		/// Список исключений, возникших во время прослушки.
		/// </summary>
        public List<Exception> LoopExceptions { get; private set; }
        #endregion

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MyTcpClient()
		{
			StringEncoder = Encoding.UTF8;
			ReadLoopIntervalMs = 10;
			LoopExceptions = new List<Exception>();
		}

		/// <summary>
		/// Соединение с сервером.
		/// </summary>
		public MyTcpClient Connect(string ipAddress, int port)
		{
			if (string.IsNullOrEmpty(ipAddress))
			{
				throw new ArgumentNullException(NetworkInfoMessages.INVALID_IP);
			}

			_client = new TcpClient();
			_client.Connect(ipAddress, port);

			StartRxThread();

			return this;
		}

		/// <summary>
		/// Разрыв соединения.
		/// </summary>
		public MyTcpClient Disconnect()
		{
			if (_client == null) { return this; }
			_client.Close();
			_client = null;
			return this;
		}

		/// <summary>
		/// Прослушивание сообщений.
		/// </summary>
		private void ListenerLoop()
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

                Thread.Sleep(ReadLoopIntervalMs);
			}

			_rxThread = null;
		}

		/// <summary>
		/// Итерация для цикла <see cref="ListenerLoop()"/>.
		/// </summary>
		private void RunLoopStep()
		{
			if (_client == null) { return; }
			if (_client.Connected == false) { return; }

			var c = _client;

			int bytesAvailable = c.Available;
			if (bytesAvailable == 0)
			{
                Thread.Sleep(10);
				return;
			}

			List<byte> bytesReceived = new List<byte>();

			while (c.Available > 0 && c.Connected)
			{
				byte[] nextByte = new byte[1];
				c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
				bytesReceived.AddRange(nextByte);
				_listMsg.AddRange(nextByte);
			}

			if (bytesReceived.Count > 0)
			{
				NotifyEndTransmissionRx(c, bytesReceived.ToArray());
			}
		}

		/// <summary>
		/// Запускает фоновый поток прослушивания.
		/// </summary>
		private void StartRxThread()
		{
			if (_rxThread != null) { return; }

			_rxThread = new Thread(ListenerLoop);
			_rxThread.IsBackground = true;
			_rxThread.Start();
		}

		/// <summary>
		/// Уведомляет фоновый поток о завершении передачи сообщения.
		/// </summary>
		private void NotifyEndTransmissionRx(TcpClient client, byte[] msg)
		{
			if (DataReceived != null)
			{
				Sender m = new Sender(msg, client, StringEncoder);
				DataReceived(this, m);
			}
		}

		/// <summary>
		/// Записывает сообщение в сетевой поток клиента.
		/// </summary>
		public void Write(byte[] data)
		{
			if (_client == null) 
			{ 
				throw new Exception(NetworkInfoMessages.EMPTY_CONNECTION); 
			}
			_client.GetStream().Write(data, 0, data.Length);
		}

		/// <summary>
		/// Перегрузка для сообщения в виде строки <see cref="Write(byte[])"/>
		/// </summary>
		public void Write(string data)
		{
			if (data == null) { return; }
			Write(StringEncoder.GetBytes(data));
		}

		#region IDisposable Support
		/// <summary>
		/// Флаг "подлежит ли объект сборке мусора".
		/// </summary>
		private bool disposedValue = false;

		/// <summary>
		/// Освобождение клиента, который не управляется CLR.
		/// </summary>
        public void Dispose()
        {
			if (!disposedValue)
			{
				Stop = true;
				if (_client != null)
				{
					try
					{
						_client.Close();
					}
                    finally
                    {
						_client = null;
					}					
				}

				disposedValue = true;
			}
		}
        #endregion
    }
}