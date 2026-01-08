using LogHandler.Core;

namespace ServicesUnitTest.Implementation
{
    public class Differnt
    {

        ILogWriter _logWriter = ServiceLocator.GetService<ILogWriter>();
        // LogService.ILogger _log = ServiceLocator.GetService<LogService.ILogger>();

        public void WriteInfo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _logWriter.WriteAsync(new LogEntry { Message = "Write info" + i.ToString() }).GetAwaiter().GetResult();
                // _logger.WriteInfo("Write info" + i.ToString());
            }
        }
    }
}
