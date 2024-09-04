// This software is part of the IoC.Configuration library
// Copyright © 2018 IoC.Configuration Contributors
// http://oroptimizer.com

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OROptimizer.Diagnostics.Log
{
    /// <summary>
    ///     Log4Net implementation of <see cref="ILog" />
    /// </summary>
    /// <seealso cref="OROptimizer.Diagnostics.Log.ILog" />
    public class Log4NetLog : ILog
    {

        [NotNull]
        private readonly log4net.ILog _log;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetLog" /> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public Log4NetLog([NotNull] log4net.ILog log)
        {
            _log = log;
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            _log.Debug(message);
        }

        /// <inheritdoc />
        public void Debug(string message, Exception exception)
        {
            _log.Debug(message, exception);
        }

        /// <inheritdoc />
        public void DebugFormat(string format, params object[] args)
        {
            _log.DebugFormat(format, args);
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            _log.Error(message);
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception)
        {
            _log.Error(message, exception);
        }

        /// <inheritdoc />
        public void Error(Exception exception)
        {
            _log.Error(string.Empty, exception);
        }

        /// <inheritdoc />
        public void ErrorFormat(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        /// <inheritdoc />
        public void Fatal(string message)
        {
            _log.Fatal(message);
        }

        /// <inheritdoc />
        public void Fatal(string message, Exception exception)
        {
            _log.Fatal(message, exception);
        }

        /// <inheritdoc />
        public void Fatal(Exception exception)
        {
            _log.Fatal(string.Empty, exception);
        }

        /// <inheritdoc />
        public void FatalFormat(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }

        /// <inheritdoc />
        public void Info(string message, Exception exception)
        {
            _log.Info(message, exception);
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            _log.Info(message);
        }

        /// <inheritdoc />
        public void InfoFormat(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        /// <inheritdoc />
        public bool IsDebugEnabled => _log.IsDebugEnabled;

        /// <inheritdoc />
        public bool IsErrorEnabled => _log.IsErrorEnabled;

        /// <inheritdoc />
        public bool IsFatalEnabled => _log.IsFatalEnabled;

        /// <inheritdoc />
        public bool IsInfoEnabled => _log.IsInfoEnabled;

        /// <inheritdoc />
        public bool IsWarnEnabled => _log.IsWarnEnabled;

        /// <inheritdoc />
        public void Warn(string message)
        {
            _log.Warn(message);
        }

        /// <inheritdoc />
        public void Warn(string message, Exception exception)
        {
            _log.Warn(message, exception);
        }

        /// <inheritdoc />
        public void WarnFormat(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }

        /// <inheritdoc />
        public IDisposable AddContextProperties(IEnumerable<KeyValuePair<string, string>> contextProperties)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var keyValuePair in contextProperties)
            {
                log4net.ThreadContext.Properties[keyValuePair.Key] = keyValuePair.Value;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            return new AddedContextProperties(contextProperties, this);
        }

        /// <inheritdoc />
        public void RemoveContextProperty(string key)
        {
            log4net.ThreadContext.Properties.Remove(key);
        }
    }
}