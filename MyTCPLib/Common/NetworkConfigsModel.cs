using System;
using System.Net;

namespace MyTCPLib.Common
{
    /// <summary>
    /// Модель параметров подключения.
    /// </summary>
    public class NetworkConfigsModel
    {
        /// <summary>
        /// Адрес.
        /// </summary>
        public IPAddress Ip { get; private set; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Конструктор модели - включает в себя валидацию параметров.
        /// </summary>
        public NetworkConfigsModel(string ip, string port)
        {
            var validationResult =  AddressValidator.CheckNetworkParams(ip, port);
            
            if (!string.IsNullOrEmpty(validationResult))
                throw new ApplicationException(validationResult);

            Ip = IPAddress.Parse(ip);
            Port = Convert.ToInt32(port);
        }
    }
}
