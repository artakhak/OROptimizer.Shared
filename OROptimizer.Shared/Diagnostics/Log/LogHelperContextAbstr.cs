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
using System.Diagnostics;
using JetBrains.Annotations;

namespace OROptimizer.Diagnostics.Log
{
    public abstract class LogHelperContextAbstr : ILogHelperContext
    {
        #region Member Variables

        [NotNull]
        private static readonly object _lock = new object();

        [NotNull]
        private readonly Dictionary<Type, ILog> _owningTypeToLog = new Dictionary<Type, ILog>();

        #endregion

        #region ILogHelperContext Interface Implementation

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        public ILog Log
        {
            get
            {
                var stackTrace = new StackTrace();

                var owningType = stackTrace.GetFrame(1).GetMethod().DeclaringType;

                if (_owningTypeToLog.TryGetValue(owningType, out var log))
                    return log;

                lock (_lock)
                {
                    if (_owningTypeToLog.TryGetValue(owningType, out log))
                        return log;

                    log = CreateLog(owningType);
                    _owningTypeToLog[owningType] = log;
                    return log;
                }
            }
        }

        #endregion

        #region Current Type Interface

        /// <summary>
        ///     Pass the type where the log will be used.
        /// </summary>
        /// <param name="typeThatOwnsTheLog"></param>
        /// <returns></returns>
        protected abstract ILog CreateLog(Type typeThatOwnsTheLog);

        #endregion
    }
}