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
using OROptimizer.Diagnostics.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OROptimizer.ServiceResolver
{
    /// <summary>
    /// Default implementation for <see cref="IDiBasedObjectFactory"/>.
    /// Maintains local mapping of service type to implementation type, and constructs an on object by using public constructor of implementation type
    /// with maximum number of parameters.
    /// Override <see cref="DiBasedObjectFactory.InitServiceToImplementationMappings"/> to define service type to implementation type mapping that
    /// methods <see cref="IDiBasedObjectFactory.CreateInstance(Type, TryResolveConstructorParameterValueDelegate)"/> and <see cref="IDiBasedObjectFactory.CreateInstance{T}(TryResolveConstructorParameterValueDelegate)"/> will use when creating
    /// an instance of an object. 
    /// </summary>
    public class DiBasedObjectFactory : IDiBasedObjectFactory
    {
        private bool _serviceToImplementationTypeMapWereInitialized;

        [NotNull]
        private readonly Dictionary<Type, ImplementationTypeInfo> _serviceToImplementationTypeMap = new Dictionary<Type, ImplementationTypeInfo>();

        /// <inheritdoc />
        public T CreateInstance<T>(TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValue = null) //where T : class
        {
            if (CreateInstance(typeof(T), tryResolveConstructorParameterValue) is T implementation)
                return implementation;

            throw new ArgumentException($"Failed to create an instance of type '{typeof(T).FullName}'.");
        }

        /// <inheritdoc />
        public object CreateInstance(Type type, TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValue = null)
        {
            string GetErrorMessage(Type implementationType, string errorDetails)
            {
                var errorMessage = new StringBuilder();

                errorMessage.Append($"Failed to create an instance of type '{implementationType.FullName}' to which type '{type.FullName}' is mapped.");

                if (!string.IsNullOrWhiteSpace(errorDetails))
                    errorMessage.Append(errorDetails);

                return errorMessage.ToString();
            }

            ImplementationTypeInfo implementationTypeInfo;

            lock (_serviceToImplementationTypeMap)
            {
                if (!_serviceToImplementationTypeMapWereInitialized)
                    this.InitServiceToImplementationMappings();

                _serviceToImplementationTypeMapWereInitialized = true;

                if (!_serviceToImplementationTypeMap.TryGetValue(type, out implementationTypeInfo))
                {
                    Type implementationType;

                    if (type.IsInterface && type.Name.StartsWith("I", StringComparison.CurrentCulture) &&
                        type.Name.Length > 1)
                    {
                        // Lets see if there is c class with pretty similar class name (say for IFilesCache we try to find a class FilesCache in the same namespace
                        // that implements IFilesCache.

                        implementationType = type.Assembly.GetType($"{type.Namespace}.{type.Name.Substring(1)}");
                    }
                    else
                    {
                        implementationType = type;
                    }

                    (bool isValid, ConstructorInfo constructorInfo, string errorMessage) isValidImplementationResult;

                    if (implementationType != null)
                    {
                        isValidImplementationResult = this.IsValidImplementation(type, implementationType);
                    }
                    else
                    {
                        isValidImplementationResult = (false, null, "");
                    }

                    if (!isValidImplementationResult.isValid)
                        throw new ArgumentException(GetErrorMessage(implementationType ?? type,
                            $"No valid implementation type mapping exists for service type '{type.FullName}'. Make sure to specify the mapping from '{type.FullName}' to its implementation type."));

                    implementationTypeInfo = new ImplementationTypeInfo(implementationType, isValidImplementationResult.constructorInfo);
                    _serviceToImplementationTypeMap[type] = implementationTypeInfo;
                }
            }

            var constructorParameterValuesResult = GetConstructorParameterValues(implementationTypeInfo.ImplementationType, implementationTypeInfo.ConstructorInfo, tryResolveConstructorParameterValue);

            if (!constructorParameterValuesResult.isSuccess)
                throw new ArgumentException(GetErrorMessage(implementationTypeInfo.ImplementationType, constructorParameterValuesResult.errorMessage));

            object createdInstance;
            try
            {
                createdInstance = CreateInstance(implementationTypeInfo.ConstructorInfo, constructorParameterValuesResult.parameterValues);
            }
            catch
            {
                LogHelper.Context.Log.Error(GetErrorMessage(implementationTypeInfo.ImplementationType, $"Failed to create an instance of type '{implementationTypeInfo.ImplementationType.FullName}' using constructor with parameters of types [{string.Join(",", implementationTypeInfo.ConstructorInfo.GetParameters().Select(x => x.ParameterType.FullName))}]."));
                throw;
            }

            return createdInstance;
        }

        /// <summary>
        /// Returns a tuple with three values. The first value is true if type <paramref name="implementationType"/> can be used as implementation type for type <paramref name="serviceType"/>.
        /// If the first value is false, then the "errorMessage" value should contain more details.
        /// Otherwise, if "isValid" is true, the value "constructorInfo" contains the constructor info that will be used to construct instance of <paramref name="implementationType"/>.
        /// </summary>
        protected virtual (bool isValid, ConstructorInfo constructorInfo, string errorMessage) IsValidImplementation([NotNull] Type serviceType, [NotNull] Type implementationType)
        {
            string GetErrorMessagePrefix() => $"Service type '{serviceType.FullName}' cannot be mapped to implementation type '{implementationType.FullName}'";

            if (!serviceType.IsAssignableFrom(implementationType))
                return (false, null, $"{GetErrorMessagePrefix()} since service type should be assignable from implementation type.");

            if (!implementationType.IsClass || implementationType.IsAbstract || implementationType.IsInterface)
                return (false, null, $"{GetErrorMessagePrefix()} since the implementation type '{implementationType.FullName}' is either not a class or is an abstract or interface.");

            var constructorInfo = GetConstructorInfo(implementationType);

            if (constructorInfo == null)
                return (false, null, $"{GetErrorMessagePrefix()} since no valid constructor was found in type '{implementationType.FullName}'.");

            return (true, constructorInfo, null);
        }

        /// <summary>
        /// Returns the constructor info to use when constructing an instance of <paramref name="implementationType"/>.
        /// </summary>
        [CanBeNull]
        protected virtual ConstructorInfo GetConstructorInfo([NotNull] Type implementationType)
        {
            var constructorInfos = implementationType.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            ConstructorInfo candidateConstructorInfo = null;
            System.Reflection.ParameterInfo[] candidateConstructorInfoParameters = null;
            foreach (var constructorInfo in constructorInfos)
            {
                var constructorInfoParameters = constructorInfo.GetParameters();

                if (candidateConstructorInfoParameters == null || candidateConstructorInfoParameters.Length < constructorInfoParameters.Length)
                {
                    candidateConstructorInfo = constructorInfo;
                    candidateConstructorInfoParameters = constructorInfoParameters;
                }
            }

            return candidateConstructorInfo;
        }

        /// <summary>
        /// Tries to create parameter values for constructor <paramref name="implementationConstructorInfo"/>.
        /// The result is a tuple with three values.
        /// If the parameter values were successfully created, the value "isSuccess" is true, and the value "parameterValues"
        /// contains a non-null array with parameter values.
        /// Otherwise, if constructor values failed to be created, the value "isSuccess" is false, and the value "errorMessage" contains the error details.
        /// </summary>
        protected virtual (bool isSuccess, object[] parameterValues, string errorMessage) GetConstructorParameterValues([NotNull] Type implementationType, [NotNull] ConstructorInfo implementationConstructorInfo, [CanBeNull] TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValue = null)
        {
            var constructorParameters = implementationConstructorInfo.GetParameters();

            object[] constructorParameterValues = new object[constructorParameters.Length];

            for (var paramIndex = 0; paramIndex < constructorParameterValues.Length; ++paramIndex)
            {
                var parameterInfo = constructorParameters[paramIndex];
                try
                {
                    var parameterValueWasResolved = false;
                    object parameterValue = null;

                    if (tryResolveConstructorParameterValue != null)
                    {
                        var parameterResolutionResult = tryResolveConstructorParameterValue.Invoke(implementationType, parameterInfo);

                        if (parameterResolutionResult.parameterValueWasResolved)
                        {
                            parameterValueWasResolved = true;
                            parameterValue = parameterResolutionResult.resolvedValue;
                        }
                    }

                    if (!parameterValueWasResolved)
                        parameterValue = ServiceResolverAmbientContext.Context.Resolve(parameterInfo.ParameterType);

                    constructorParameterValues[paramIndex] = parameterValue;
                }
                catch (Exception e)
                {
                    LogHelper.Context.Log.Error(e);
                    return (false, null, $"Failed to resolve the value of constructor parameter '{parameterInfo.Name}' of type '{parameterInfo.ParameterType.FullName}' when trying to construct an instance of '{implementationType.FullName}'.");
                }
            }

            return (true, constructorParameterValues, null);
        }

        /// <summary>
        /// Creates an instance of object.
        /// </summary>
        /// <param name="constructorInfo">Constructor info of created object.</param>
        /// <param name="constructorParameterValues">Constructor parameter values.</param>
        /// <returns></returns>
        [NotNull]
        protected virtual object CreateInstance([NotNull] ConstructorInfo constructorInfo, [NotNull] object[] constructorParameterValues)
        {
            return constructorInfo.Invoke(constructorParameterValues);
        }

        /// <summary>
        /// Initializes the service to implementation type mappings. Use methods <see cref="AddServiceToImplementationMapping(Type, Type)"/>,
        /// <see cref="AddServiceToImplementationMapping{TService, TImplementation}"/>, <see cref="TryRemoveServiceToImplementationMapping(Type)"/>,
        /// and <see cref="TryRemoveServiceToImplementationMapping{TService}"/> to initialize the mappings.
        /// </summary>
        protected virtual void InitServiceToImplementationMappings()
        {

        }

        /// <summary>
        /// Adds a new mapping for service type <typeparamref name="TService"/>, or replaces an existing one.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <typeparam name="TImplementation">Implementation type.</typeparam>
        protected void AddServiceToImplementationMapping<TService, TImplementation>()
        {
            AddServiceToImplementationMapping(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Registers a mapping from type <typeparamref name="TService"/> to the same type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        protected void AddSelfBoundServiceImplementation<TService>()
        {
            AddServiceToImplementationMapping(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// Adds a new mapping for service type <paramref name="serviceType"/>, or replaces an existing one.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <param name="implementationType">Implementation type.</param>
        /// <exception cref="ArgumentException"></exception>
        protected void AddServiceToImplementationMapping([NotNull] Type serviceType, [NotNull] Type implementationType)
        {
            lock (_serviceToImplementationTypeMap)
            {
                var isValidImplementationResult = IsValidImplementation(serviceType, implementationType);

                if (!isValidImplementationResult.isValid)
                    throw new ArgumentException($"Cannot map service type '{serviceType.FullName}' to service type '{implementationType.FullName}'. Error details: {isValidImplementationResult.errorMessage}");

                _serviceToImplementationTypeMap[serviceType] = new ImplementationTypeInfo(implementationType, isValidImplementationResult.constructorInfo);
            }
        }

        /// <summary>
        /// Removes the mapping for service type <typeparamref name="TService"/> if it exists.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>Returns true if the mapping existed. Returns false otherwise.</returns>
        protected bool TryRemoveServiceToImplementationMapping<TService>()
        {
            return TryRemoveServiceToImplementationMapping(typeof(TService));
        }

        /// <summary>
        /// Removes the mapping for service type <paramref name="serviceType"/> if it exists.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <returns>Returns true if the mapping existed. Returns false otherwise.</returns>
        protected bool TryRemoveServiceToImplementationMapping([NotNull] Type serviceType)
        {
            lock (_serviceToImplementationTypeMap)
            {
                return _serviceToImplementationTypeMap.Remove(serviceType);
            }
        }

        [CanBeNull]
        protected Type TryGetServiceToImplementationMapping([NotNull] Type serviceType)
        {
            lock (_serviceToImplementationTypeMap)
                return _serviceToImplementationTypeMap.TryGetValue(serviceType, out var implementationTypeInfo) ? implementationTypeInfo.ImplementationType : null;
        }

        private class ImplementationTypeInfo
        {
            public ImplementationTypeInfo(Type implementationType, ConstructorInfo constructorInfo)
            {
                ImplementationType = implementationType;
                ConstructorInfo = constructorInfo;
            }

            [NotNull]
            public Type ImplementationType { get; }

            [NotNull]
            public ConstructorInfo ConstructorInfo { get; }
        }
    }
}