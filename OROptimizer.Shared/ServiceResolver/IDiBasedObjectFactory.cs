﻿// This software is part of the IoC.Configuration library
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
using System;

namespace OROptimizer.ServiceResolver
{
    /// <summary>
    /// Factory of objects created using the type passed as a parameter (the type can be actual type of created object, or could be a parent class, or implemented interface).
    /// Use this factory if we want to get a new instance every time we resolve a service, no matter how the service was registered in DI
    /// container (e.g., singleton or transient, or even if it was not registered at all).
    /// This interface guarantees that the resolved service is always created and a cached version is not used.
    /// </summary>
    public interface IDiBasedObjectFactory
    {
        /// <summary>
        /// Creates new instance (does not re-use) of an object of type <typeparamref name="T"/>.
        /// If value of <paramref name="tryResolveConstructorParameterValue"/> is not null, and it resolves a constructor parameter in created object, the value generated by
        /// <paramref name="tryResolveConstructorParameterValue"/> will be used. Otherwise, the implementation will resolve constructor parameter some other
        /// way (such as using the class <see cref="ServiceResolverAmbientContext"/>.
        /// </summary>
        /// <typeparam name="T">Type of the created object. The type can be an abstract or non abstract class or an interface.</typeparam>
        /// <returns>Return a new instance of type <typeparamref name="T"/>.</returns>
        /// <exception cref="Exception">Throws an exception if type could not be created.</exception>
        [NotNull]
        T CreateInstance<T>([CanBeNull] TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValue = null);

        /// <summary>
        /// Creates new instance (does not re-use) of an object of type specified in parameter <paramref name="type"/>.
        /// If value of <paramref name="tryResolveConstructorParameterValue"/> is not null, and it resolves a constructor parameter in created object, the value generated by
        /// <paramref name="tryResolveConstructorParameterValue"/> will be used. Otherwise, the implementation will resolve constructor parameter some other
        /// way (such as using the class <see cref="ServiceResolverAmbientContext"/>.
        /// </summary>
        /// <exception cref="Exception">Throws an exception if type could not be created.</exception>
        [NotNull]
        object CreateInstance([NotNull] Type type, [CanBeNull] TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValue = null);
    }
}