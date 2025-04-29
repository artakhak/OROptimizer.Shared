// This software is part of the OROptimizer library
// Copyright © 2018 OROptimizer Contributors
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
    ///     A logger interface.
    ///     See [OROptimizer.Shared.Log4Net.Log4NetLog] in nuget package [OROptimizer.Shared.Log4Net] for specific implementation details.
    /// </summary>
    public interface ILog
    {
        void Debug([NotNull] string message);
        void Debug([NotNull] string message, [NotNull] Exception exception);

        [StringFormatMethod("format")]
        void DebugFormat([NotNull] string format, [CanBeNull] [ItemCanBeNull] params object[] args);

        void Error([NotNull] string message);
        void Error([NotNull] string message, [NotNull] Exception exception);
        void Error([NotNull] Exception exception);

        [StringFormatMethod("format")]
        void ErrorFormat([NotNull] string format, [CanBeNull] [ItemCanBeNull] params object[] args);

        void Fatal([NotNull] string message);
        void Fatal([NotNull] string message, [NotNull] Exception exception);
        void Fatal([NotNull] Exception exception);

        [StringFormatMethod("format")]
        void FatalFormat(string format, [CanBeNull] [ItemCanBeNull] params object[] args);

        void Info([NotNull] string message, [NotNull] Exception exception);
        void Info([NotNull] string message);

        [StringFormatMethod("format")]
        void InfoFormat([NotNull] string format, [CanBeNull] [ItemCanBeNull] params object[] args);

        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }

        void Warn([NotNull] string message);
        void Warn([NotNull] string message, [NotNull] Exception exception);

        [StringFormatMethod("format")]
        void WarnFormat([NotNull] string format, [CanBeNull] [ItemCanBeNull] params object[] args);

        /// <summary>
        /// Adds values to context. The implementation is responsible for including the values with every log, if
        /// logger is configured to show the value.
        /// </summary>
        /// <param name="contextProperties">Collection of key/value pairs added to logger context.</param>
        /// <returns>
        /// Returns disposable object that is responsible for removing the added context properties when <see cref="IDisposable.Dispose"/> is called.
        /// Instance of class <see cref="AddedContextProperties"/> (or similar) can be used as return value.
        /// </returns>
        IDisposable AddContextProperties(IEnumerable<KeyValuePair<string, string>> contextProperties);

        /// <summary>
        /// Adds a value for context key. The implementation is responsible for including the value with every log, if
        /// logger is configured to show the value.
        /// </summary>
        /// <param name="key">Context key.</param>
        void RemoveContextProperty(string key);
    }
}