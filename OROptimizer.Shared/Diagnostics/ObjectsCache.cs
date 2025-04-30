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

namespace OROptimizer.Diagnostics
{
    /// <summary>
    /// Default implementation for <see cref="IObjectsCache{TObjectInfo}"/>.
    /// </summary>
    /// <typeparam name="TObjectInfo"></typeparam>
    public class ObjectsCache<TObjectInfo> : IObjectsCache<TObjectInfo> where TObjectInfo : ObjectInfo
    {
        [NotNull]
        private readonly CreateObjectInfoDelegate<TObjectInfo> _createObjectInfo;

        [NotNull]
        private readonly Dictionary<object, TObjectInfo> _objectToObjectInfoMap = new Dictionary<object, TObjectInfo>();

        [NotNull]
        private readonly Dictionary<long, TObjectInfo> _objectIdToObjectInfoMap = new Dictionary<long, TObjectInfo>();

        private int _currentId;

        [NotNull]
        private readonly object _lockObject = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="createObjectInfo">A delegate that create a wrapper <see cref="ObjectInfo"/> for an object.</param>
        public ObjectsCache([NotNull] CreateObjectInfoDelegate<TObjectInfo> createObjectInfo)
        {
            _createObjectInfo = createObjectInfo;
        }

        /// <inheritdoc />
        public TObjectInfo GetOrCreateObjectInfo(object obj)
        {
            var objectType = obj.GetType();

            if (!objectType.IsClass)
            {
                // Lets prevent boxing/unboxing, since we will be boxing the same primitive type into different objects,
                // and therefore adding a new object to cache each time.
                throw new ArgumentException("The value of parameter should be  a class and cannot be a string.");
            }

            lock (_lockObject)
            {
                if (!_objectToObjectInfoMap.TryGetValue(obj, out var objectInfo))
                {
                    objectInfo = _createObjectInfo(obj, _currentId++);
                    _objectToObjectInfoMap[obj] = objectInfo;
                    _objectIdToObjectInfoMap[objectInfo.ObjectId] = objectInfo;
                }

                return objectInfo;
            }
        }

        /// <inheritdoc />
        public TObjectInfo TryGetObjectInfoById(long objectId)
        {
            lock(_lockObject)
                return _objectIdToObjectInfoMap.TryGetValue(objectId, out var objectInfo) ? objectInfo : null;
        }
    }
}