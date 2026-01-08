using System;

namespace OROptimizer.Diagnostics.Log
{
    /// <summary>
    /// An implementation of <see cref="ILogHelperContext"/> that logs to the console.
    /// </summary>
    public class LogHelperContextLogToConsole: LogHelperContextAbstr
    {
        /// <inheritdoc />
        protected override ILog CreateLog(Type typeThatOwnsTheLog)
        {
            return new LogToConsole(LogLevel.Debug);
        }
    }
}
