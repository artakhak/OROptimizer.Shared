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

using JetBrains.Annotations;

namespace OROptimizer.Diagnostics
{
    /// <summary>
    /// A cache of objects stored in object wrappers of type <typeparamref name="TObjectInfo"/>.
    /// </summary>
    /// <typeparam name="TObjectInfo">Generic type of object wrapper.</typeparam>
    public interface IObjectsCache<out TObjectInfo> where TObjectInfo: ObjectInfo
    {
        /// <summary>
        /// Returns the object wrapper for cached object <paramref name="obj"/>.
        /// </summary>
        TObjectInfo GetOrCreateObjectInfo([NotNull] object obj);

        /// <summary>
        /// Returns the object wrapper for cached object with <see cref="ObjectInfo.ObjectId"/> equal to <paramref name="objectId"/>.
        /// </summary>
        /// <param name="objectId">Unique object Id.</param>
        /// <returns></returns>
        TObjectInfo TryGetObjectInfoById(long objectId);
    }
}
