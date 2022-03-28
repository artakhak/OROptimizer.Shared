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
using JetBrains.Annotations;

namespace OROptimizer.Diagnostics.Log
{
    /// <summary>
    ///     A logger interface.
    ///     See <see cref="Log4NetLog" /> for specific implementation details.
    /// </summary>
    public interface ILog
    {
        void Debug([NotNull] string message);
        void Debug([NotNull] string message, [NotNull] Exception exception);

        [StringFormatMethod("format")]
        void DebugFormat([NotNull] string format, [CanBeNull] [ItemNotNull] params object[] args);

        void Error([NotNull] string message);
        void Error([NotNull] string message, [NotNull] Exception exception);
        void Error([NotNull] Exception exception);

        [StringFormatMethod("format")]
        void ErrorFormat([NotNull] string format, [CanBeNull] [ItemNotNull] params object[] args);

        void Fatal([NotNull] string message);
        void Fatal([NotNull] string message, [NotNull] Exception exception);
        void Fatal([NotNull] Exception exception);

        [StringFormatMethod("format")]
        void FatalFormat(string format, [CanBeNull] [ItemNotNull] params object[] args);

        void Info([NotNull] string message, [NotNull] Exception exception);
        void Info([NotNull] string message);

        [StringFormatMethod("format")]
        void InfoFormat([NotNull] string format, [CanBeNull] [ItemNotNull] params object[] args);

        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }

        void Warn([NotNull] string message);
        void Warn([NotNull] string message, [NotNull] Exception exception);

        [StringFormatMethod("format")]
        void WarnFormat([NotNull] string format, [CanBeNull] [ItemNotNull] params object[] args);
    }
}