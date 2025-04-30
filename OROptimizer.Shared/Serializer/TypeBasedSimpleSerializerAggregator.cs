// This software is part of the OROptimizer library
// Copyright � 2018 OROptimizer Contributors
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
using OROptimizer.Diagnostics.Log;

namespace OROptimizer.Serializer
{
    /// <summary>
    ///     A serializer that serializes/deserializes values to/from string values.
    /// </summary>
    /// <seealso cref="OROptimizer.Serializer.ITypeBasedSimpleSerializerAggregator" />
    public class TypeBasedSimpleSerializerAggregator : ITypeBasedSimpleSerializerAggregator
    {
        #region Member Variables

        private static readonly ITypeBasedSimpleSerializerAggregator _defaultSerializerAggregator;

        private readonly Dictionary<Type, ITypeBasedSimpleSerializer> _typeToSerializerMap = new Dictionary<Type, ITypeBasedSimpleSerializer>();

        #endregion

        #region  Constructors

        static TypeBasedSimpleSerializerAggregator()
        {
            _defaultSerializerAggregator = new TypeBasedSimpleSerializerAggregator();
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerDouble());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerLong());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerInt());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerShort());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerByte());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerBoolean());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerDateTime());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerGuid());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerString());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerType());
            _defaultSerializerAggregator.Register(new TypeBasedSimpleSerializerAssembly());
        }

        #endregion

        #region ITypeBasedSimpleSerializerAggregator Interface Implementation

        /// <summary>
        ///     Deserializes value from value specified in parameter <paramref name="valueToDeserialize" /> into an instance
        ///     of type <typeparamref name="T" />.
        ///     The serializer will fail if there no serializer <see cref="T:OROptimizer.Serializer.ITypeBasedSimpleSerializer" />
        ///     was registered for
        ///     type <typeparamref name="T" /> for which
        ///     <see cref="P:OROptimizer.Serializer.ITypeBasedSimpleSerializer.SerializedType" /> == <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueToDeserialize"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public T Deserialize<T>(string valueToDeserialize)
        {
            var type = typeof(T);

            if (!TryDeserialize(type, valueToDeserialize, out var deserializedValue))
                throw new ArgumentException($"Failed to deserialize value '{deserializedValue}' to value of type '{type.FullName}'.");

            return (T) deserializedValue;
        }

        /// <summary>
        ///     Gets the registered serializers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITypeBasedSimpleSerializer> GetRegisteredSerializers()
        {
            foreach (var serializer in _typeToSerializerMap.Values)
                yield return serializer;
        }

        /// <summary>
        ///     Gets the <see cref="T:OROptimizer.Serializer.ITypeBasedSimpleSerializer" /> registered for type
        ///     <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public ITypeBasedSimpleSerializer GetSerializerForType(Type type)
        {
            return GetSerializerForType(type, false);
        }

        /// <summary>
        ///     Determines whether there is a serializer object of type
        ///     <see cref="T:OROptimizer.Serializer.ITypeBasedSimpleSerializer" /> registered to
        ///     serialize/de-serialize value of type <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public bool HasSerializerForType(Type type)
        {
            return _typeToSerializerMap.ContainsKey(type);
        }

        /// <summary>
        ///     Registers the specified type based simple serializer. The serializers registered using this method
        ///     are used by method
        ///     <see cref="M:OROptimizer.Serializer.ITypeBasedSimpleSerializerAggregator.Deserialize``1(System.String)" /> and
        ///     <see
        ///         cref="M:OROptimizer.Serializer.ITypeBasedSimpleSerializerAggregator.TrySerialize(System.Object,System.String@)" />
        ///     .
        /// </summary>
        /// <param name="typeBasedSimpleSerializer">The type based simple serializer.</param>
        /// <returns></returns>
        public bool Register(ITypeBasedSimpleSerializer typeBasedSimpleSerializer)
        {
            if (_typeToSerializerMap.ContainsKey(typeBasedSimpleSerializer.SerializedType))
            {
                LogHelper.Context.Log.WarnFormat("There are multiple implementations of {0} serializer for the same serialized type {1}. The implementation {2} will be used.",
                    typeof(ITypeBasedSimpleSerializer).FullName, typeBasedSimpleSerializer.SerializedType.FullName,
                    _typeToSerializerMap[typeBasedSimpleSerializer.SerializedType].GetType().FullName);

                return false;
            }

            _typeToSerializerMap[typeBasedSimpleSerializer.SerializedType] = typeBasedSimpleSerializer;
            return true;
        }


        /// <summary>
        ///     Removes a serializer for the specified type, if one was registered before.
        /// </summary>
        /// <param name="serializedType">The type being unregistered.</param>
        /// <returns></returns>
        public bool UnRegister(Type serializedType)
        {
            return _typeToSerializerMap.Remove(serializedType);
        }

        /// <summary>
        ///     Tries the deserialize the value in <paramref name="deserializedValue" /> into an on object of type
        ///     <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueToDeserialize">The value to deserialize.</param>
        /// <param name="defaultValueIfFails">The default value if fails.</param>
        /// <param name="deserializedValue">The deserialized value.</param>
        /// <returns>
        ///     Returns <c>true</c>, if the value was successfully de-serialized. returns <c>false</c> otherwise.
        /// </returns>
        public bool TryDeserialize<T>(string valueToDeserialize, T defaultValueIfFails, out T deserializedValue)
        {
            var type = typeof(T);

            if (TryDeserialize(type, valueToDeserialize, out var deserializedValueObj))
            {
                deserializedValue = (T) deserializedValueObj;
                return true;
            }

            deserializedValue = defaultValueIfFails;
            return false;
        }

        /// <summary>
        ///     Tries the deserialize the value in <paramref name="deserializedValue" /> into an on object of type
        ///     <paramref name="typeToDeserializeTo" />.
        /// </summary>
        /// <param name="typeToDeserializeTo">The type to deserialize to.</param>
        /// <param name="valueToDeserialize">The value to deserialize.</param>
        /// <param name="deserializedValue">The deserialized value.</param>
        /// <returns>
        ///     Returns <c>true</c>, if the value was successfully de-serialized. returns <c>false</c> otherwise.
        /// </returns>
        public bool TryDeserialize(Type typeToDeserializeTo, string valueToDeserialize, out object deserializedValue)
        {
            deserializedValue = null;
            var serializer = GetSerializerForType(typeToDeserializeTo, true);

            if (serializer == null)
                return false;

            if (serializer.TryDeserialize(valueToDeserialize, out var deserializedValueLocal))
            {
                if (deserializedValueLocal != null &&
                    !typeToDeserializeTo.IsTypeAssignableFrom(deserializedValueLocal.GetType()))
                {
                    LogHelper.Context.Log.ErrorFormat("The value de-serialized by serializer {0} is of type {1}. The de-serialized value is expected to be of type {2}. The de-serialized value is: {3}.",
                        serializer.GetType(), deserializedValueLocal.GetType(), serializer.SerializedType, deserializedValueLocal);

                    return false;
                }

                deserializedValue = deserializedValueLocal;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Tries the serialize the value in <paramref name="serializedValue" /> into an output parameter
        ///     <paramref name="serializedValue" />.
        /// </summary>
        /// <param name="valueToSerialize">The value to serialize.</param>
        /// <param name="serializedValue">The serialized value.</param>
        /// <returns>
        ///     Returns <c>true</c>, if the value was successfully serialized. returns <c>false</c> otherwise.
        /// </returns>
        public bool TrySerialize(object valueToSerialize, out string serializedValue)
        {
            serializedValue = string.Empty;

            if (valueToSerialize == null)
                return false;

            var serializer = GetSerializerForType(valueToSerialize.GetType(), true);

            if (serializer == null)
                return false;

            if (serializer.TrySerialize(valueToSerialize, out var serializedValueLocal))
            {
                if (serializedValueLocal == null)
                {
                    LogHelper.Context.Log.ErrorFormat("The value serialized by serializer {0} is null.", serializer.GetType());
                    return false;
                }

                serializedValue = serializedValueLocal;
                return true;
            }

            return false;
        }

        #endregion

        #region Member Functions

        /// <summary>
        ///     Gets the default serializer aggregator.
        /// </summary>
        /// <returns></returns>
        public static ITypeBasedSimpleSerializerAggregator GetDefaultSerializerAggregator()
        {
            return _defaultSerializerAggregator;
        }

        [CanBeNull]
        private ITypeBasedSimpleSerializer GetSerializerForType(Type type, bool logAnErrorIfSerializerIsNotFound)
        {
            if (_typeToSerializerMap.TryGetValue(type, out var serializer))
                return serializer;

            if (logAnErrorIfSerializerIsNotFound)
                LogHelper.Context.Log.ErrorFormat("No implementation of '{0}' is available for type {1}.",
                    typeof(ITypeBasedSimpleSerializer).FullName,
                    type.FullName);

            return null;
        }

        #endregion
    }
}