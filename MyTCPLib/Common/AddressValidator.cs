using System.Net;
using System.Text.RegularExpressions;

namespace MyTCPLib.Common
{
    /// <summary>
    /// Валидаторы для параметров соединения.
    /// </summary>
    public static class AddressValidator
    {
        /// <summary>
        /// 1024—49151 	Зарегистрированные порты
        /// для процессов, запущенных обычными пользователями.
        /// </summary>
        public const int MIN_PORT_VALUE = 1024;
        public const int MAX_PORT_VALUE = 49151;

        private static string _match = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";

        /// <summary>
        /// Получение результата валидации.
        /// </summary>
        public static string CheckNetworkParams(string ip, string port)
        {
            var result = string.Empty;
            if (!IsIpCorrect(ip))
                result += $"{NetworkInfoMessages.INVALID_IP} ";
            if (!IsPortCorrect(port))
            {
                result += string.IsNullOrEmpty(result) ? string.Empty : "\n";
                result += $"{NetworkInfoMessages.INVALID_PORT}";
            }
            return result;
        }

        /// <summary>
        /// Валидатор IP хоста.
        /// </summary>
        public static bool IsIpCorrect(string ip)
        {
            IPAddress ipAddress;
            var regex = new Regex(_match);
            bool isParsed = regex.IsMatch(ip) && IPAddress.TryParse(ip, out ipAddress);
            return isParsed;
        }

        /// <summary>
        /// Валидатор порта.
        /// </summary>
        public static bool IsPortCorrect (string textPort)
        {
            int port;
            if (int.TryParse(textPort, out port))
            {
                if (port >= MIN_PORT_VALUE && port <= MAX_PORT_VALUE)
                    return true;
            }
            return false;
        }
    }
}
