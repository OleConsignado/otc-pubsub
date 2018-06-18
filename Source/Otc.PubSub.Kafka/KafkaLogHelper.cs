using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Otc.PubSub.Kafka
{
    internal static class KafkaLogHelper
    {
        private static readonly string LogMessageTemplate =
                "Librdkafka: {@LogMessage}";

        public static void LogKafkaMessage(ILogger logger, LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                // De acordo com https://en.wikipedia.org/wiki/Syslog
                case 0: // Emergency - System is unusable. A panic condition.
                case 1: // Alert - Action must be taken immediately. A condition that should be corrected immediately, such as a corrupted system database.
                case 2: // Critical - Critical conditions, such as hard device errors.
                    logger.LogCritical(LogMessageTemplate, logMessage);
                    break;
                case 3: // Error - Error conditions. 
                    logger.LogError(LogMessageTemplate, logMessage);
                    break;
                case 4: // Warning - Warning conditions.
                    logger.LogWarning(LogMessageTemplate, logMessage);
                    break;
                case 5: // Notice - Normal but significant conditions. Conditions that are not error conditions, but that may require special handling.
                case 6: // Informational - Informational messages.
                    logger.LogInformation(LogMessageTemplate, logMessage);
                    break;
                case 7: // Debug - Debug-level messages. Messages that contain information normally of use only when debugging a program.
                    logger.LogDebug(LogMessageTemplate, logMessage);
                    break;
                default:
                    logger.LogError($"Librdkafka unknow level log: {LogMessageTemplate}", logMessage);
                    break;
            }
        }
    }
}
