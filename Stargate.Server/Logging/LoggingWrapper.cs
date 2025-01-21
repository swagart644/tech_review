using Stargate.Server.Data;
using Stargate.Server.Data.Models;

namespace Stargate.Server.Logging
{
    public class LoggingWrapper : ILoggingWrapper
    {
        private readonly StargateContext _context;

        public LoggingWrapper(StargateContext context)
        {
            _context = context;
        }

        public async Task Log(string message, bool isSuccess)
        {
            LogEntry logEntry = new()
            {
                IsSuccess = isSuccess,
                Message = message
            };

            await _context.LogEntries.AddAsync(logEntry);
            int logRows = await _context.SaveChangesAsync();
        }
    }
}
