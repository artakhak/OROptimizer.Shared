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
using JetBrains.Annotations;

namespace OROptimizer.Serializer
{
    /// <summary>
    ///     A serializer that serializes/deserializes values to/from string values.
    /// </summary>
    public interface ITypeBasedSimpleSerializerAggregator
    {
        #region Current Type Interface

        /// <summary>
        ///     Deserializes value from value specified in parameter <paramref name="valueToDeserialize" /> into an instance
        ///     of type <typeparamref name="T" />.
        ///     The serializer will fail if there no serializer <see cref="ITypeBasedSimpleSerializer" /> was registered for
        ///     type <typeparamref name="T" /> for which <see cref="ITypeBasedSimpleSerializer.SerializedType" /> ==
        ///     <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueToDeserialize"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Throws this exception if the value cannot be deserialized</exception>
        T Deserialize<T>([CanBeNull] string valueToDeserialize);

        /// <summary>
        ///     Gets the registered serializers.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<ITypeBasedSimpleSerializer> GetRegisteredSerializers();

        /// <summary>
        ///     Gets the <see cref="ITypeBasedSimpleSerializer" /> registered for type <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        ITypeBasedSimpleSerializer GetSerializerForType([NotNull] Type type);

        /// <summary>
        ///     Determines whether there is a serializer object of type <see cref="ITypeBasedSimpleSerializer" /> registered to
        ///     serialize/de-serialize value of type <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        bool HasSerializerForType([NotNull] Type type);

        /// <summary>
        ///     Registers the specified type based simple serializer. The serializers registered using this method
        ///     are used by method <see cref="Deserialize{T}(string)" /> and <see cref="TrySerialize(object, out string)" />.
        /// </summary>
        /// <param name="typeBasedSimpleSerializer">The type based simple serializer.</param>
        /// <returns></returns>
        bool Register([NotNull] ITypeBasedSimpleSerializer typeBasedSimpleSerializer);

        /// <summary>
        ///     Removes a serializer for the specified type, if one was registered before.
        /// </summary>
        /// <param name="serializedType">The type being unregistered.</param>
        /// <returns></returns>
        bool UnRegister([NotNull] Type serializedType);

        /// <summary>
        ///     Tries the deserialize the value in <paramref name="deserializedValue" /> into an on object of type
        ///     <paramref name="typeToDeserializeTo" />.
        /// </summary>
        /// <param name="typeToDeserializeTo">The type to deserialize to.</param>
        /// <param name="valueToDeserialize">The value to deserialize.</param>
        /// <param name="deserializedValue">The deserialized value.</param>
        /// <returns>Returns <c>true</c>, if the value was successfully de-serialized. returns <c>false</c> otherwise.</returns>
        bool TryDeserialize([NotNull] Type typeToDeserializeTo, [CanBeNull] string valueToDeserialize, [CanBeNull] out object deserializedValue);

        /// <summary>
        ///     Tries the deserialize the value in <paramref name="deserializedValue" /> into an on object of type
        ///     <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueToDeserialize">The value to deserialize.</param>
        /// <param name="defaultValueIfFails">The default value if fails.</param>
        /// <param name="deserializedValue">The deserialized value.</param>
        /// <returns>Returns <c>true</c>, if the value was successfully de-serialized. returns <c>false</c> otherwise.</returns>
        bool TryDeserialize<T>([CanBeNull] string valueToDeserialize, T defaultValueIfFails, [CanBeNull] out T deserializedValue);

        /// <summary>
        ///     Tries the serialize the value in <paramref name="serializedValue" /> into an output parameter
        ///     <paramref name="serializedValue" />.
        /// </summary>
        /// <param name="valueToSerialize">The value to serialize.</param>
        /// <param name="serializedValue">The serialized value.</param>
        /// <returns>Returns <c>true</c>, if the value was successfully serialized. returns <c>false</c> otherwise.</returns>
        bool TrySerialize([CanBeNull] object valueToSerialize, [CanBeNull] out string serializedValue);

        #endregion
    }
}