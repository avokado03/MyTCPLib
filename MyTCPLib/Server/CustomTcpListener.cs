using System.Net;
using System.Net.Sockets;

namespace MyTCPLib.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomTcpListener : TcpListener
	{

		public CustomTcpListener(IPAddress localaddr, int port) : base(localaddr, port)
		{
		}

		/// <summary>
		/// Признак прослушивание порта.
		/// </summary>
		public new bool Active
		{
			get { return base.Active; }
		}
	}
}
