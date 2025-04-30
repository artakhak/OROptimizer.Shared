using System;
using System.Collections.Generic;
using System.Text;

namespace OROptimizer.Diagnostics.Log
{
    public sealed class LogToConsole: ILog
    {
        private readonly Dictionary<LogLevel, string> _logLevelToLoggedMessagePrefix = new Dictionary<LogLevel, string>
        {
            {LogLevel.Debug, LogLevel.Debug.ToString().ToUpper()},
            {LogLevel.Info, LogLevel.Info.ToString().ToUpper()},
            {LogLevel.Warn, LogLevel.Warn.ToString().ToUpper()},
            {LogLevel.Error, LogLevel.Error.ToString().ToUpper()},
            {LogLevel.Fatal, LogLevel.Fatal.ToString().ToUpper()},
        };

        private readonly LogLevel _logLevel;

        public LogToConsole(): this(LogLevel.Debug)
        {
            
        }

        public LogToConsole(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public bool IsDebugEnabled => ShouldLog(LogLevel.Debug);
        public bool IsErrorEnabled => ShouldLog(LogLevel.Error);
        public bool IsFatalEnabled => ShouldLog(LogLevel.Fatal);
        public bool IsInfoEnabled => ShouldLog(LogLevel.Info);
        public bool IsWarnEnabled => ShouldLog(LogLevel.Warn);

        public void Debug(string message)
        {
            LogMessage(LogLevel.Debug, message);
        }

        public void Debug(string message, Exception exception)
        {
            LogMessageWithException(LogLevel.Debug, message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            LogMessage(LogLevel.Debug, format, args);
        }

        public void Error(string message)
        {
            LogMessage(LogLevel.Error, message);
        }

        public void Error(string message, Exception exception)
        {
            LogMessageWithException(LogLevel.Error, message, exception);
        }

        public void Error(Exception exception)
        {
            LogMessageWithException(LogLevel.Error, String.Empty, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            LogMessage(LogLevel.Error, format, args);
        }

        public void Fatal(string message)
        {
            LogMessage(LogLevel.Fatal, message);
        }

        public void Fatal(string message, Exception exception)
        {
            LogMessageWithException(LogLevel.Fatal, message, exception);
        }

        public void Fatal(Exception exception)
        {
            LogMessageWithException(LogLevel.Fatal, String.Empty, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            LogMessage(LogLevel.Fatal, format, args);
        }

        public void Info(string message, Exception exception)
        {
            LogMessageWithException(LogLevel.Info, message, exception);
        }

        public void Info(string message)
        {
            LogMessage(LogLevel.Info, message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            LogMessage(LogLevel.Info, format, args);
        }

        public void Warn(string message)
        {
            LogMessage(LogLevel.Warn, message);
        }

        public void Warn(string message, Exception exception)
        {
            LogMessageWithException(LogLevel.Warn, message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            LogMessage(LogLevel.Warn, format, args);
        }

        public IDisposable AddContextProperties(IEnumerable<KeyValuePair<string, string>> contextProperties)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            // Currently not supported. Will support in future releases.
            return new AddedContextProperties(contextProperties, this);
        }

        public void RemoveContextProperty(string key)
        {
            // Currently not supported. Will support in future releases.
        }

        private bool ShouldLog(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        private void LogMessageWithException(LogLevel logLevel, string message, Exception exception)
        {
            if (!ShouldLog(logLevel))
                return;

            var loggedMessage = new StringBuilder();

            if (_logLevelToLoggedMessagePrefix.TryGetValue(logLevel, out var prefix))
                loggedMessage.Append(prefix).Append(": ");

            loggedMessage.AppendLine(message);
            loggedMessage.Append("Exception: ");
            loggedMessage.AppendLine(exception.Message);
            loggedMessage.Append(exception.StackTrace);

            Console.Out.WriteLine(loggedMessage.ToString());
        }

        private void LogMessage(LogLevel logLevel, string format, params object[] args)
        {
            if (!ShouldLog(logLevel))
                return;

            var message = args == null || args.Length == 0 ? format : string.Format(format, args);

            var loggedMessage = new StringBuilder();

            if (_logLevelToLoggedMessagePrefix.TryGetValue(logLevel, out var prefix))
                loggedMessage.Append(prefix).Append(": ");

            loggedMessage.AppendLine(message);

            Console.Out.WriteLine(loggedMessage.ToString());
        }
    }
}
