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
using System.Text;

namespace OROptimizer.Diagnostics.Log
{
    /// <summary>
    ///     An exception thrown when <see cref="LogHelper.Context" /> is null, when the property is used.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class LoggerWasNotInitializedException : Exception
    {
        #region  Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoggerWasNotInitializedException" /> class.
        /// </summary>
        public LoggerWasNotInitializedException() : base(GenerateExceptionMessage())
        {
        }

        #endregion

        #region Member Functions

        private static string GenerateExceptionMessage()
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine($"Logger was not initialized. Before using {typeof(LogHelper).FullName}.{nameof(LogHelper.Context)}, make sure to call the method {typeof(LogHelper).FullName}.{nameof(LogHelper.RegisterContext)}({typeof(ILogHelperContext).FullName}).");
            errorMessage.AppendLine($"The easiest way to implement {typeof(ILogHelperContext).FullName} is to subclass the abstract class {typeof(LogHelperContextAbstr).FullName} and to override the method CreateLog(Type typeThatOwnsTheLog) which returns an instance of {typeof(ILog)}.");
            errorMessage.AppendLine($"Example is: {typeof(LogHelper).FullName}.{nameof(LogHelper.RegisterContext)}(new OROptimizer.Shared.Log4Net.Log4NetHelperContext(\"MyApp.log4net.config\"));");
            errorMessage.AppendLine("Class [OROptimizer.Shared.Log4Net.Log4NetHelperContext] can be found in Nuget package OROptimizer.Shared.Log4Net.");
            return errorMessage.ToString();
        }

        #endregion
    }
}