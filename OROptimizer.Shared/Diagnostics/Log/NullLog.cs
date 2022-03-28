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

namespace OROptimizer.Diagnostics.Log
{
    /// <summary>
    /// An implementation of <see cref="ILog"/> that does not log anywhere.
    /// This can be used to setup the logger when we are not interested in logs.
    /// Normally we should avoid using <see cref="NullLog"/>, however this logger can be used in libraries that 
    /// </summary>
    public class NullLog : ILog
    {
        /// <inheritdoc />
        public void Debug(string message)
        {
        }

        /// <inheritdoc />
        public void Debug(string message, Exception exception)
        {
        }

        /// <inheritdoc />
        public void DebugFormat(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(string message)
        {
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception)
        {
        }

        /// <inheritdoc />
        public void Error(Exception exception)
        {
        }

        /// <inheritdoc />
        public void ErrorFormat(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Fatal(string message)
        {
        }

        /// <inheritdoc />
        public void Fatal(string message, Exception exception)
        {
        }

        /// <inheritdoc />
        public void Fatal(Exception exception)
        {
        }

        /// <inheritdoc />
        public void FatalFormat(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Info(string message, Exception exception)
        {
        }

        /// <inheritdoc />
        public void Info(string message)
        {
        }

        /// <inheritdoc />
        public void InfoFormat(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public bool IsDebugEnabled => false;

        /// <inheritdoc />
        public bool IsErrorEnabled => true;

        /// <inheritdoc />
        public bool IsFatalEnabled => true;

        /// <inheritdoc />
        public bool IsInfoEnabled => false;

        /// <inheritdoc />
        public bool IsWarnEnabled => false;

        /// <inheritdoc />
        public void Warn(string message)
        {
        }

        /// <inheritdoc />
        public void Warn(string message, Exception exception)
        {
        }

        /// <inheritdoc />
        public void WarnFormat(string format, params object[] args)
        {
        }
    }
}