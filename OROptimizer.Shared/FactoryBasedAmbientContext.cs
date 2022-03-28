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

namespace OROptimizer
{
    public class FactoryBasedAmbientContext<TContext, TAmbientContextFactory> where TContext : class where TAmbientContextFactory : IAmbientContextFactory<TContext>, new()
    {
        private static TContext _context;
        private static readonly TContext DefaultContext;

        static FactoryBasedAmbientContext()
        {
            DefaultContext = new TAmbientContextFactory().Create();
            SetDefaultContext();
        }

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
            _context = DefaultContext;
        }
    }
}