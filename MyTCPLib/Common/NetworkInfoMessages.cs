namespace MyTCPLib.Common
{
    /// <summary>
    /// Сообщения клиента.
    /// </summary>
    public class NetworkInfoMessages
    {
        public const string INVALID_IP = "Адрес хоста не задан или задан некорректно. Требуемый формат - 999.999.999.999";
        public const string INVALID_PORT = "Порт не задан или задан некорректно. Диапазон - 1024—49151";
        public const string EMPTY_CONNECTION = "Установите соединение";
    }
}
