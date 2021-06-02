using System;

namespace MyTCPLib.Common
{
    /// <summary>
    /// Вспомогательный класс для логирования результатов
    /// обращения к серверу.
    /// </summary>
    public static class LogHelpers
    {
        /// <summary>
        /// Преобразует строку в лог с датой.
        /// </summary>
        public static string ToLogMessage(this string message)
        {
            return $"{DateTime.Now}  {message} {Environment.NewLine}";
        }
    }
}
