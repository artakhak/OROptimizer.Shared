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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;
using OROptimizer.DynamicCode;

namespace OROptimizer
{
    public class GlobalsCore : IGlobalsCore
    {
        [NotNull]
        private readonly IDynamicAssemblyBuilderFactory _dynamicAssemblyBuilderFactory;

        private long _lastGeneratedId = -1;

        [NotNull]
        private readonly object _lockObjectDynamicAssemblyBuilder = new object();

        [NotNull]
        private readonly object _lockObjectUniqueId = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalsCore" /> class.
        /// </summary>
        public GlobalsCore() : this(new DynamicAssemblyBuilderFactory()) 

        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalsCore" /> class.
        /// </summary>
        /// <param name="dynamicAssemblyBuilderFactory">The dynamic assembly builder factory.</param>
        public GlobalsCore([NotNull] IDynamicAssemblyBuilderFactory dynamicAssemblyBuilderFactory)
        {
            EntryAssemblyFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _dynamicAssemblyBuilderFactory = dynamicAssemblyBuilderFactory;
        }

        /// <summary>
        ///     Checks the type constructor existence.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="constructorParametersTypes">The constructor parameters types.</param>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool CheckTypeConstructorExistence(Type serviceType, Type implementationType, Type[] constructorParametersTypes,
                                                  out ConstructorInfo constructorInfo, out string errorMessage)
        {
            if (implementationType.IsInterface || implementationType.IsAbstract || !serviceType.IsAssignableFrom(implementationType))
            {
                constructorInfo = null;
                errorMessage = $"'{implementationType.FullName}' should be a concrete class that implements interface '{serviceType.FullName}'.";
                return false;
            }

            return CheckTypeConstructorExistence(implementationType, constructorParametersTypes, out constructorInfo, out errorMessage);
        }

        /// <summary>
        ///     Checks the type constructor existence.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constructorParametersTypes">The constructor parameters types.</param>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool CheckTypeConstructorExistence(Type type, Type[] constructorParametersTypes,
                                                  out ConstructorInfo constructorInfo, out string errorMessage)
        {
            errorMessage = null;
            constructorInfo = type.GetConstructor(constructorParametersTypes);

            if (constructorInfo == null || !constructorInfo.IsPublic)
            {
                var errorMessageStrBldr = new StringBuilder();
                errorMessageStrBldr.AppendLine($"No constructor exists in type '{type.FullName}' with the following constructor parameter types:");
                errorMessageStrBldr.Append("[");

                for (var i = 0; i < constructorParametersTypes.Length; ++i)
                {
                    if (i > 0)
                        errorMessageStrBldr.Append(", ");

                    errorMessageStrBldr.Append(constructorParametersTypes[i].FullName);
                }

                errorMessageStrBldr.Append("]");

                errorMessage = errorMessageStrBldr.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Creates an instance of type <paramref name="classFullName" /> in assembly <paramref name="assemblyFilePath" />
        ///     using parameters specified in
        ///     <paramref name="constructorParameters" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classFullName">Full name of the class.</param>
        /// <param name="assemblyFilePath">The assembly file path.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <returns>
        ///     Returns created type, or null if the type instance cannot be created.
        /// </returns>
        public T CreateInstance<T>(string classFullName, string assemblyFilePath, ParameterInfo[] constructorParameters) where T : class
        {
            Assembly loadedAssembly = null;
            try
            {
                loadedAssembly = LoadAssembly(assemblyFilePath);
            }
            catch (Exception e)
            {
                LogHelper.Context.Log.Error(e);
            }
            finally
            {
                if (loadedAssembly == null)
                    LogHelper.Context.Log.Error($"Failed to load assembly '{assemblyFilePath}'.");
            }

            var type = loadedAssembly.GetType(classFullName, true, false);

            if (type == null)
            {
                LogHelper.Context.Log.Error($"No type '{classFullName}' found in assembly '{assemblyFilePath}'.");
                return null;
            }

            return (T) CreateInstance(typeof(T), type, constructorParameters, out var errorMessage);
        }

        /// <summary>
        /// Loads the assembly using from file <paramref name="assemblyFilePath"/>.
        /// </summary>
        /// <param name="assemblyFilePath">Assembly file path.</param>
        /// <returns>Returns the loaded assembly.</returns>
        /// <exception cref="Exception">Throws an exception if load fails.</exception>
        public Assembly LoadAssembly(string assemblyFilePath)
        {
            return Assembly.LoadFrom(assemblyFilePath);
        }

        /// <summary>
        ///     Tries to create an instance if type <paramref name="implementationType" /> that implements
        ///     <paramref name="serviceType" />.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///     Returns created type, or null if the type instance cannot be created.
        /// </returns>
        public object CreateInstance(Type serviceType, Type implementationType, ParameterInfo[] constructorParameters, out string errorMessage)
        {
            if (!CheckTypeConstructorExistence(serviceType, implementationType, constructorParameters.Select(x => x.ParameterType).ToArray(), out var constructor, out errorMessage))
                return null;

            return TryCreateInstanceFromType(constructor, constructorParameters, out errorMessage);
        }

        /// <summary>
        ///     Tries to create an instance if type <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///     Returns created type, or null if the type instance cannot be created.
        /// </returns>
        public object CreateInstance(Type type, ParameterInfo[] constructorParameters, out string errorMessage)
        {
            if (!CheckTypeConstructorExistence(type, constructorParameters.Select(x => x.ParameterType).ToArray(), out var constructor, out errorMessage))
                return null;

            return TryCreateInstanceFromType(constructor, constructorParameters, out errorMessage);
        }

        /// <summary>
        ///     Gets the current in progress dynamic assembly builder.
        /// </summary>
        /// <value>
        ///     The current in progress dynamic assembly builder.
        /// </value>
        public IDynamicAssemblyBuilder CurrentInProgressDynamicAssemblyBuilder { get; private set; }

        public void EnsureParameterNotNull(string parameterName, object parameterValue)
        {
            if (parameterValue == null)
                LogAnErrorAndThrowException($"Value of parameter '{parameterName}' cannot be null.",
                    null, message => new ArgumentNullException(parameterName));
        }

        [NotNull]
        public string EntryAssemblyFolder { get; }

        public long GenerateUniqueId()
        {
            lock (_lockObjectUniqueId)
            {
                var generatedId = DateTime.Now.Ticks;

#if DEBUG && GENERATE_AUTOINCREMENTED_IDS
                generatedId = _lastGeneratedId + 1;
#else
                if (_lastGeneratedId >= generatedId)
                    generatedId = _lastGeneratedId + 1;
#endif

                _lastGeneratedId = generatedId;
                return generatedId;
            }
        }

        public IEnumerable<Assembly> GetAllLoadedAssemblies()
        {
            var assembles = new LinkedList<Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                try
                {
                    if (!assembly.IsDynamic)
                        assembles.AddLast(assembly);
                }
                catch
                {
                    // Ignore. Accessing Assembly.Location might result in an exception.
                }

            return assembles;
        }

        public void LogAnErrorAndThrowException(string loggedErrorMessage, string exceptionMessage = null, Func<string, Exception> createException = null)
        {
            LogHelper.Context.Log.ErrorFormat(loggedErrorMessage);

            if (exceptionMessage == null)
                exceptionMessage = $"Error thrown at {DateTime.Now}. Look at the logs for more details.";

            if (createException != null)
                throw createException(exceptionMessage);

            throw new Exception(exceptionMessage);
        }

#pragma warning disable CS0419, CS1574

        /// <inheritdoc />
        public virtual IDynamicAssemblyBuilder StartDynamicAssemblyBuilder(string dynamicAssemblyPath,
                                                                           Delegates.OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete,
                                                                           bool addAllLoadedAssembliesAsReferences,
                                                                           params string[] referencedAssemblyPaths)
        {
            ILoadedAssemblies loadedAssemblies;

            if (addAllLoadedAssembliesAsReferences)
                loadedAssemblies = new AllLoadedAssemblies();
            else
                loadedAssemblies = new NoLoadedAssemblies();

            return StartDynamicAssemblyBuilder(dynamicAssemblyPath, onDynamicAssemblyEmitComplete,
                loadedAssemblies, referencedAssemblyPaths);
        }

        /// <inheritdoc />
        public IDynamicAssemblyBuilder StartDynamicAssemblyBuilder(string dynamicAssemblyPath,
            Delegates.OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete,
            ILoadedAssemblies loadedAssemblies, params string[] referencedAssemblyPaths)
        {
            lock (_lockObjectDynamicAssemblyBuilder)
            {
                if (CurrentInProgressDynamicAssemblyBuilder != null)
                {
                    LogHelper.Context.Log.ErrorFormat("Trying to start a compilation while {0}.CurrentInProgressCSharpCompilation is not null.",
                        typeof(GlobalsCore).FullName);
                    throw new Exception();
                }

                CurrentInProgressDynamicAssemblyBuilder = _dynamicAssemblyBuilderFactory.CreateDynamicAssemblyBuilder(dynamicAssemblyPath,
                    (assemblyPath, success, emitResult) =>
                    {
                        lock (_lockObjectDynamicAssemblyBuilder)
                        {
                            try
                            {
                                onDynamicAssemblyEmitComplete?.Invoke(assemblyPath, success, emitResult);
                            }
                            finally
                            {
                                CurrentInProgressDynamicAssemblyBuilder = null;
                            }
                        }
                    });

                foreach (var assembly in loadedAssemblies.GetAssemblies())
                {
                    CurrentInProgressDynamicAssemblyBuilder.AddReferencedAssembly(assembly.Location);
                }

                if (referencedAssemblyPaths != null)
                    foreach (var referencedAssemblyPath in referencedAssemblyPaths)
                        CurrentInProgressDynamicAssemblyBuilder.AddReferencedAssembly(referencedAssemblyPath);

                return CurrentInProgressDynamicAssemblyBuilder;
            }
        }
#pragma warning restore CS0419, CS1574
       
        private object TryCreateInstanceFromType(ConstructorInfo constructorInfo, ParameterInfo[] constructorParameters, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                return constructorInfo.Invoke(constructorParameters.Select(x => x.ParameterValue).ToArray());
            }
            catch (Exception e)
            {
                LogHelper.Context.Log.Error(e);
                errorMessage = $"Failed to create instance of '{constructorInfo.DeclaringType.FullName}'.";
                return null;
            }
        }
    }
}