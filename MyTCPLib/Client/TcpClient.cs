using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace MyTCPLib.Client
{
    public class MyTcpClient : IDisposable
	{
        #region Fields
        private Thread _rxThread = null;
		private List<byte> _queuedMsg = new List<byte>();
		private TcpClient _client = null;
	
		public event EventHandler<Sender> DelimiterDataReceived;
		public event EventHandler<Sender> DataReceived;
		#endregion

		#region Properties
		public Encoding StringEncoder { get; set; }
		internal bool QueueStop { get; set; }
		internal int ReadLoopIntervalMs { get; set; }
        #endregion

        public MyTcpClient()
		{
			StringEncoder = Encoding.UTF8;
			ReadLoopIntervalMs = 10;
		}

		public MyTcpClient Connect(string hostNameOrIpAddress, int port)
		{
			if (string.IsNullOrEmpty(hostNameOrIpAddress))
			{
				throw new ArgumentNullException("hostNameOrIpAddress");
			}

			_client = new TcpClient();
			_client.Connect(hostNameOrIpAddress, port);

			StartRxThread();

			return this;
		}

		private void StartRxThread()
		{
			if (_rxThread != null) { return; }

			_rxThread = new Thread(ListenerLoop);
			_rxThread.IsBackground = true;
			_rxThread.Start();
		}

		public MyTcpClient Disconnect()
		{
			if (_client == null) { return this; }
			_client.Close();
			_client = null;
			return this;
		}

		public TcpClient TcpClient { get { return _client; } }

		private void ListenerLoop(object state)
		{
			while (!QueueStop)
			{
				try
				{
					RunLoopStep();
				}
				catch (Exception ex)
				{
				}

                Thread.Sleep(ReadLoopIntervalMs);
			}

			_rxThread = null;
		}

		private void RunLoopStep()
		{
			if (_client == null) { return; }
			if (_client.Connected == false) { return; }

			var c = _client;

			int bytesAvailable = c.Available;
			if (bytesAvailable == 0)
			{
				System.Threading.Thread.Sleep(10);
				return;
			}

			List<byte> bytesReceived = new List<byte>();

			while (c.Available > 0 && c.Connected)
			{
				byte[] nextByte = new byte[1];
				c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
				bytesReceived.AddRange(nextByte);
				_queuedMsg.AddRange(nextByte);
			}

			if (bytesReceived.Count > 0)
			{
				NotifyEndTransmissionRx(c, bytesReceived.ToArray());
			}
		}

		private void NotifyDelimiterSenderRx(TcpClient client, byte[] msg)
		{
			if (DelimiterDataReceived != null)
			{
				Sender m = new Sender(msg, client, StringEncoder);
				DelimiterDataReceived(this, m);
			}
		}

		private void NotifyEndTransmissionRx(TcpClient client, byte[] msg)
		{
			if (DataReceived != null)
			{
				Sender m = new Sender(msg, client, StringEncoder);
				DataReceived(this, m);
			}
		}

		public void Write(byte[] data)
		{
			if (_client == null) 
			{ 
				throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); 
			}
			_client.GetStream().Write(data, 0, data.Length);
		}

		public void Write(string data)
		{
			if (data == null) { return; }
			Write(StringEncoder.GetBytes(data));
		}

		#region IDisposable Support
		private bool disposedValue = false;

        public void Dispose()
        {
			if (!disposedValue)
			{
				QueueStop = true;
				if (_client != null)
				{
					try
					{
						_client.Close();
					}
					catch { }
					_client = null;
				}

				disposedValue = true;
			}
		}
        #endregion
    }
}