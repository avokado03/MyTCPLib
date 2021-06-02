using System.Net;
using System.Net.Sockets;

namespace MyTCPLib.Server
{
    /// <summary>
    /// Наследник-адаптер <see cref="TcpListener"/>.
    /// </summary>
	/// <remarks>Т.к. Active - protected, но доступ к нему необходим.</remarks>
    public class CustomTcpListener : TcpListener
	{
		/// <summary>
		/// Конструктор.
		/// </summary>
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
