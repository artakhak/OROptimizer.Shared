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
    /// <summary>
    ///     A simple serializer that serializes/de-serializes objects to and from strings.
    ///     The serialized string does not have any information about the type, so specific implementation de-serializes
    ///     specific type.
    ///     For example integer value 3 will be de-serialized from "3".
    /// </summary>
    public interface ITypeBasedSimpleSerializer
    {
        #region Current Type Interface

        [NotNull]
        Type SerializedType { get; }

        bool TryDeserialize([CanBeNull] string valueToDeserialize, [CanBeNull] out object deserializedValue);

        bool TrySerialize([CanBeNull] object valueToSerialize, [CanBeNull] out string serializedValue);

        #endregion
    }
}