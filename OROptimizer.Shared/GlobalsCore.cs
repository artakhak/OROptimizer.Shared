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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;
using OROptimizer.DynamicCode;

namespace OROptimizer
{
    public class GlobalsCore : IGlobalsCore
    {
        #region Member Variables

        [NotNull]
        private readonly IDynamicAssemblyBuilderFactory _dynamicAssemblyBuilderFactory;

        private long _lastGeneratedId = -1;

        [NotNull]
        private readonly object _lockObjectDynamicAssemblyBuilder = new object();

        [NotNull]
        private readonly object _lockObjectUniqueId = new object();

        #endregion

        #region  Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalsCore" /> class.
        /// </summary>
        public GlobalsCore() : this(new DynamicAssemblyBuilderFactory())
        {
            EntryAssemblyFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalsCore" /> class.
        /// </summary>
        /// <param name="dynamicAssemblyBuilderFactory">The dynamic assembly builder factory.</param>
        public GlobalsCore([NotNull] IDynamicAssemblyBuilderFactory dynamicAssemblyBuilderFactory)
        {
            _dynamicAssemblyBuilderFactory = dynamicAssemblyBuilderFactory;
        }

        #endregion

        #region IGlobalsCore Interface Implementation

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
            constructorInfo = null;

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
                loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFilePath);

                // Normally exception would be thrown, so we shouldn't get here. This is for getting rid of warning.
                if (loadedAssembly == null)
                    return null;
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


                if (_lastGeneratedId >= generatedId)
                    generatedId = _lastGeneratedId + 1;

#if DEBUG && GENERATE_AUTOINCREMENTED_IDS
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

        #endregion

        #region Current Type Interface

#pragma warning disable CS0419, CS1574
        /// <summary>
        ///     Starts the dynamic assembly builder and returns an instance of <see cref="IDynamicAssemblyBuilder" />.
        ///     Use the value of <see cref="IGlobalsCore.CurrentInProgressDynamicAssemblyBuilder" />
        ///     anywhere in code to add C# files and referenced assemblies to the assembly being generated.
        ///     Example usage of this method is as follows:
        ///     <para />
        ///     using(var assemblyBuilder = <see cref="StartDynamicAssemblyBuilder" />("c:\DynamicallyGeneratedAssembly1.dll"))
        ///     <para />
        ///     {
        ///     <para />
        ///     assemblyBuilder.<see cref="IDynamicAssemblyBuilder.AddReferencedAssembly" />(typeof(
        ///     <see cref="ITypeBasedSimpleSerializerAggregator" />));
        ///     <para />
        ///     assemblyBuilder.AddReferencedAssembly("MyReferencedAssembly1.dll");
        ///     <para />
        ///     AddCSharpFile1();
        ///     <para />
        ///     }
        ///     <para />
        ///     public void AddCSharpFile1()
        ///     <para />
        ///     {
        ///     <para />
        ///     var assemblyBuilder = <see cref="GlobalsCoreAmbientContext.Context.CurrentInProgressDynamicAssemblyBuilder" />;
        ///     <para />
        ///     var cSharpFile = null;
        ///     <para />
        ///     // generate C# file contents in cSharpFile
        ///     <para />
        ///     // ...
        ///     <para />
        ///     assemblyBuilder.AddCSharpFile(cSharpFile);
        ///     <para />
        ///     }
        ///     <para />
        /// </summary>
        /// <param name="dynamicAssemblyPath">The dynamic assembly path, where dynamically generated assembly will be saved.</param>
        /// <param name="onDynamicAssemblyEmitComplete">
        ///     Delegate <see cref="OnDynamicAssemblyEmitComplete" /> that will be called,
        ///     when the dynamic assembly generation is complete.
        /// </param>
        /// <param name="addAllLoadedAssembliesAsReferences">
        ///     if set to <c>true</c> all assemblies loaded into current application
        ///     domain will be automatically added as referenced assemblies to the dynamically generated assembly.
        /// </param>
        /// <param name="referencedAssemblyPaths">
        ///     Assembly paths for assemblies that will be added as references to generated
        ///     assembly.
        /// </param>
        /// <returns>
        ///     Returns an instance of <see cref="IDynamicAssemblyBuilder" />.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        public virtual IDynamicAssemblyBuilder StartDynamicAssemblyBuilder(string dynamicAssemblyPath,
                                                                           Delegates.OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete,
                                                                           bool addAllLoadedAssembliesAsReferences,
                                                                           params string[] referencedAssemblyPaths)
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

                if (addAllLoadedAssembliesAsReferences)
                    foreach (var assembly in GlobalsCoreAmbientContext.Context.GetAllLoadedAssemblies())
                        CurrentInProgressDynamicAssemblyBuilder.AddReferencedAssembly(assembly.Location);

                if (referencedAssemblyPaths != null)
                    foreach (var referencedAssemblyPath in referencedAssemblyPaths)
                        CurrentInProgressDynamicAssemblyBuilder.AddReferencedAssembly(referencedAssemblyPath);

                return CurrentInProgressDynamicAssemblyBuilder;
            }
        }
#pragma warning restore CS0419, CS1574

        #endregion

        #region Member Functions

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

        #endregion
    }
}