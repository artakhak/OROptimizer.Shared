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
using JetBrains.Annotations;

namespace OROptimizer.Serializer
{
    public abstract class TypeBasedSimpleSerializerAbstr<T> : ITypeBasedSimpleSerializer, IValueToCSharpCodeConverter<T>
    {
        #region  Constructors

        public TypeBasedSimpleSerializerAbstr()
        {
            SerializedType = typeof(T);
        }

        #endregion

        #region ITypeBasedSimpleSerializer Interface Implementation

        [NotNull]
        public Type SerializedType { get; }

        bool ITypeBasedSimpleSerializer.TryDeserialize([NotNull] string valueToDeserialize, out object deserializedValue)
        {
            var success = TryDeserialize(valueToDeserialize.Trim('\u202c'), out var deserializedValueOfT);
            deserializedValue = deserializedValueOfT;
            return success;
        }

        bool ITypeBasedSimpleSerializer.TrySerialize(object valueToSerialize, out string serializedValue)
        {
            if (valueToSerialize is T valueOfT)
            {
                serializedValue = Serialize(valueOfT);
                return true;
            }

            serializedValue = null;
            return false;
        }

        #endregion

        #region IValueToCSharpCodeConverter<T> Interface Implementation

        /// <summary>
        ///     Returns C# code for the object in parameter <paramref name="deserializedValue" />.
        ///     For example Guid object with Guid="632243D6-F070-4596-A5DB-A60DE6DBD800" will be converted to
        ///     "new Guid("632243D6-F070-4596-A5DB-A60DE6DBD800")".
        ///     For int value 17 string "17" will be returned.
        /// </summary>
        /// <param name="deserializedValue">The deserialized value.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        string IValueToCSharpCodeConverter.GenerateCSharpCode(object deserializedValue)
        {
            if (deserializedValue is T valueofT)
                return GenerateCSharpCode(valueofT);

            return null;
        }

        #endregion

        #region Current Type Interface

        public abstract string GenerateCSharpCode(T deserializedValue);

        public abstract string Serialize(T valueToSerialize);

        public abstract bool TryDeserialize([NotNull] string valueToDeserialize, out T deserializedValue);

        #endregion
    }
}