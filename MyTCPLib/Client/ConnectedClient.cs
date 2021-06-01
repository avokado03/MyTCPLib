﻿using System.Net;
using System.Net.Sockets;

namespace MyTCPLib.Client
{
    /// <summary>
    /// Модель соединенного клиента.
    /// </summary>
    public class ConnectedClient
    {
        /// <summary>
        /// Адрес сервера.
        /// </summary>
        public IPAddress ServerIP { get; internal set; }

        /// <summary>
        /// Клиент.
        /// </summary>
        public TcpClient Client { get; internal set; }
    }
}
