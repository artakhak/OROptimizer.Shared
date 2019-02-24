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
using OROptimizer.Diagnostics.Log;

namespace OROptimizer
{
    /// <summary>
    ///     A generic ambient context to replace static methods.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TContextDefaultImplementation">The type of the context default implementation.</typeparam>
    /// <seealso cref="GlobalsCoreAmbientContext" />
    public class AmbientContext<TContext, TContextDefaultImplementation> where TContext : class
        where TContextDefaultImplementation : class
    {
        #region Member Variables

        private static TContext _context;
        private static readonly TContext _defaultContext;

        #endregion

        #region  Constructors

        static AmbientContext()
        {
            var interfaceType = typeof(TContext);
            var implementationType = typeof(TContextDefaultImplementation);

            if (interfaceType.IsAssignableFrom(implementationType))
            {
                var constructorInfo = implementationType.GetConstructor(new Type[] { });

                if (constructorInfo != null && constructorInfo.IsPublic)
                    _defaultContext = (TContext) constructorInfo.Invoke(new object[] { });
            }

            if (_defaultContext == null)
            {
                LogHelper.Context.Log.Error($"Type '{implementationType.FullName}' should be an implementation of type '{interfaceType.FullName}' and should have a public parameterless constructor.");
                throw new Exception($"Invalid types specified: '{interfaceType.FullName}', '{implementationType.FullName}'.");
            }

            SetDefaultContext();
        }

        #endregion

        #region Member Functions

        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        /// <value>
        ///     The context.
        /// </value>
        public static TContext Context
        {
            get => _context;
            set
            {
                if (value == null)
                    SetDefaultContext();
                else
                    _context = value;
            }
        }

        /// <summary>
        ///     Sets the default context.
        /// </summary>
        public static void SetDefaultContext()
        {
            _context = _defaultContext;
        }

        #endregion
    }
}