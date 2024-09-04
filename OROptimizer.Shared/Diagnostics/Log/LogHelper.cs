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
    ///     A helper class for logging.
    /// </summary>
    public static class LogHelper
    {
        [CanBeNull]
        private static ILogHelperContext _context;

        [NotNull]
        private static readonly object _lockObject = new object();



        // TODO: Consider removing Context and replace with ILog.
        // ILogHelperContext was used originally with the though that ILogHelperContext might have some other logger related
        // properties aside from ILog, however so far no new property was added (see if new properties still might be added).
        /// <summary>
        ///     Gets the context.
        /// </summary>
        /// <value>
        ///     The context.
        /// </value>
        /// <exception cref="OROptimizer.Diagnostics.Log.LoggerWasNotInitializedException"></exception>
        public static ILogHelperContext Context
        {
            get
            {
                if (_context == null)
                    throw new LoggerWasNotInitializedException();

                return _context;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is context initialized.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is context initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool IsContextInitialized => _context != null;

        /// <summary>
        ///     Registers the context. Call this method before accessing the property <see cref="LogHelper.Context" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.Exception"></exception>
        public static void RegisterContext(ILogHelperContext context)
        {
            lock (_lockObject)
            {
                if (_context != null)
                    throw new Exception($"The context was already set in {typeof(LogHelper).FullName}.");

                _context = context;
            }
        }

        /// <summary>
        ///     Nulls the value of <see cref="Context" />. Use this in situations when you want to switch to a different logger.
        ///     Normally the logger should be set only once, when the application starts. However in some scenarios the logger
        ///     might need to be reset
        ///     to something else. One such scenario might be changing the logger in tests.
        ///     After the <see cref="Context" /> is set to null using this method,
        ///     <see cref="RegisterContext(ILogHelperContext)" /> can be called again.
        /// </summary>
        public static void RemoveContext()
        {
            lock (_lockObject)
                _context = null;
        }
    }
}