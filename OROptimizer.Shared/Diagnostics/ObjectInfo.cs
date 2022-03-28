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

using JetBrains.Annotations;

namespace OROptimizer.Diagnostics
{
    /// <summary>
    /// A wrapper for an object of type <see cref="Object"/> passed as a constructor parameter.
    /// Normally used for diagnostics, when we want to store objects in dictionary by Id.
    /// </summary>
    public class ObjectInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">The wrapped object.</param>
        /// <param name="objectId">Unique object Id.</param>
        public ObjectInfo([NotNull] object obj, long objectId)
        {
            Object = obj;
            ObjectId = objectId;
        }

        /// <summary>
        /// The wrapped object.
        /// </summary>
        public object Object { get; internal set; }

        /// <summary>
        /// Unique object Id.
        /// </summary>
        public long ObjectId { get; internal set; }
    }
}