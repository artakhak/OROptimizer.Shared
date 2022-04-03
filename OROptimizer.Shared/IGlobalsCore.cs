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
using System.Reflection;
using JetBrains.Annotations;
using OROptimizer.DynamicCode;
using static OROptimizer.Delegates;

namespace OROptimizer
{
    /// <summary>
    ///     Provides some helper functions.
    ///     This interface should always be bound using singletone scope.
    /// </summary>
    public interface IGlobalsCore
    {

        /// <summary>
        ///     Generates the unique identifier.
        /// </summary>
        /// <returns></returns>
        long GenerateUniqueId();

        /// <summary>
        ///     Checks the type constructor existence.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="constructorParametersTypes">The constructor parameters types.</param>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <param name="errorMessage">The error message.</param>
        bool CheckTypeConstructorExistence([NotNull] Type serviceType, [NotNull] Type implementationType, [NotNull] [ItemNotNull] Type[] constructorParametersTypes,
                                           out ConstructorInfo constructorInfo, out string errorMessage);

        /// <summary>
        ///     Checks the type constructor existence.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constructorParametersTypes">The constructor parameters types.</param>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <param name="errorMessage">The error message.</param>
        bool CheckTypeConstructorExistence([NotNull] Type type, [NotNull] [ItemNotNull] Type[] constructorParametersTypes,
                                           out ConstructorInfo constructorInfo, out string errorMessage);

        /// <summary>
        ///     Creates an instance of type <paramref name="classFullName" /> in assembly <paramref name="assemblyFilePath" />
        ///     using parameters specified in
        ///     <paramref name="constructorParameters" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classFullName">Full name of the class.</param>
        /// <param name="assemblyFilePath">The assembly file path.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <returns>Returns created type, or null if the type instance cannot be created.</returns>
        [CanBeNull]
        T CreateInstance<T>([NotNull] string classFullName, [NotNull] string assemblyFilePath,
                            [CanBeNull] [ItemNotNull] ParameterInfo[] constructorParameters) where T : class;

        /// <summary>
        ///     Tries to create an instance if type <paramref name="implementationType" /> that implements
        ///     <paramref name="serviceType" />.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Returns created type, or null if the type instance cannot be created.</returns>
        [CanBeNull]
        object CreateInstance([NotNull] Type serviceType, [NotNull] Type implementationType, [NotNull] [ItemNotNull] ParameterInfo[] constructorParameters, out string errorMessage);

        /// <summary>
        ///     Tries to create an instance if type <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Returns created type, or null if the type instance cannot be created.</returns>
        [CanBeNull]
        object CreateInstance([NotNull] Type type, [NotNull] [ItemNotNull] ParameterInfo[] constructorParameters, out string errorMessage);

        /// <summary>
        ///     Gets the current in progress dynamic assembly builder.
        /// </summary>
        /// <value>
        ///     The current in progress dynamic assembly builder.
        /// </value>
        IDynamicAssemblyBuilder CurrentInProgressDynamicAssemblyBuilder { get; }

        /// <summary>
        ///     Logs an error and throws an exception if <paramref name="parameterValue" /> is null.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <exception cref="ArgumentNullException">Throws this exception.</exception>
        void EnsureParameterNotNull([NotNull] string parameterName, [CanBeNull] object parameterValue);

        /// <summary>
        ///     This is normally the location where the executable is. For test projects this might be the folder where the test
        ///     execution library is. Therefore, the context might need to be replaced with a one, that returns a valid entry
        ///     folder path.
        /// </summary>
        string EntryAssemblyFolder { get; }

        /// <summary>
        ///     Gets all loaded assemblies.
        /// </summary>
        [Obsolete("Will be removed after 05/31/2023. Use the default implementation 'OROptimizer.AllLoadedAssemblies' of 'OROptimizer.ILoadedAssemblies' instead.")]
        IEnumerable<Assembly> GetAllLoadedAssemblies();

        /// <summary>
        ///     Logs an error specified in <paramref name="loggedErrorMessage" /> and throws an exception.
        /// </summary>
        /// <param name="loggedErrorMessage">Logged error message. Example: "The value of attribute 'name' cannot be null."</param>
        /// <param name="exceptionMessage">
        ///     The message to use in thrown exception. If null, a common error will be used in thrown exception. Otherwise, the
        ///     message
        ///     in this parameter will be used
        /// </param>
        /// <param name="createException">
        ///     A function, that creates an exception object.
        ///     The argument passed to a function is specified in parameter <paramref name="exceptionMessage" /> if the parameter
        ///     value is not null. Otherwise, generic error message
        ///     is used.
        /// </param>
        /// <exception cref="Exception">
        ///     Always throws exception. The exception object is created by <paramref name="createException" /> parameter, if the
        ///     value of parameter is not null.
        ///     Otherwise <see cref="System.Exception" /> is thrown.
        /// </exception>
        [StringFormatMethod("format")]
        void LogAnErrorAndThrowException([NotNull] string loggedErrorMessage,
                                         [CanBeNull] string exceptionMessage = null,
                                         [CanBeNull] Func<string, Exception> createException = null);

#pragma warning disable CS0419, CS1574
        /// <summary>
        ///     Starts the dynamic assembly builder and returns an instance of <see cref="IDynamicAssemblyBuilder" />.
        ///     Use the value of <see cref="IGlobalsCore.CurrentInProgressDynamicAssemblyBuilder" />
        ///     anywhere in code to add C# files and referenced assemblies to the assembly being generated.
        ///     Example usage of this method is as follows:
        ///     <para />
        ///     using(var assemblyBuilder = GlobalsCoreAmbientContext.Context.StartDynamicAssemblyBuilder("c:\DynamicallyGeneratedAssembly1.dll"))
        ///     <para />
        ///     {
        ///     <para />
        ///     assemblyBuilder.AddReferencedAssembly(typeof(<see cref="ITypeBasedSimpleSerializerAggregator" />), ...);
        ///     <para />
        ///     assemblyBuilder.AddReferencedAssembly("MyReferencedAssembly1.dll");
        ///     <para />
        ///     AddCSharpFile1();
        ///     <para />
        ///     }
        ///     <para />
        /// 
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
        ///     ...
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
        /// <returns>Returns an instance of <see cref="IDynamicAssemblyBuilder" />.</returns>
        [Obsolete("Will be removed after 05/31/2023. Use StartDynamicAssemblyBuilder method with ILoadedAssemblies parameter instead")]
        IDynamicAssemblyBuilder StartDynamicAssemblyBuilder([NotNull] string dynamicAssemblyPath, [CanBeNull] OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete,
                                                            bool addAllLoadedAssembliesAsReferences,
                                                            [CanBeNull] [ItemNotNull] params string[] referencedAssemblyPaths);

        /// <summary>
        ///     Starts the dynamic assembly builder and returns an instance of <see cref="IDynamicAssemblyBuilder" />.
        ///     Use the value of <see cref="IGlobalsCore.CurrentInProgressDynamicAssemblyBuilder" />
        ///     anywhere in code to add C# files and referenced assemblies to the assembly being generated.
        ///     Example usage of this method is as follows:
        ///     <para />
        ///     using(var assemblyBuilder = GlobalsCoreAmbientContext.Context.StartDynamicAssemblyBuilder("c:\DynamicallyGeneratedAssembly1.dll", ...))
        ///     <para />
        ///     {
        ///     <para />
        ///         assemblyBuilder.AddReferencedAssembly(typeof(<see cref="ITypeBasedSimpleSerializerAggregator" />));
        ///     <para />
        ///         assemblyBuilder.AddReferencedAssembly("MyReferencedAssembly1.dll");
        ///     <para />
        ///         AddCSharpFile1();
        ///     <para />
        ///     }
        ///     <para />
        /// 
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
        ///     ...
        ///     <para />
        ///         assemblyBuilder.AddCSharpFile(cSharpFile);
        ///     <para />
        ///     }
        ///     <para />
        /// </summary>
        /// <param name="dynamicAssemblyPath">The dynamic assembly path, where dynamically generated assembly will be saved.</param>
        /// <param name="onDynamicAssemblyEmitComplete">
        ///     Delegate <see cref="OnDynamicAssemblyEmitComplete" /> that will be called,
        ///     when the dynamic assembly generation is complete.
        /// </param>
        /// <param name="loadedAssemblies">Instance of <see cref="ILoadedAssemblies"/> used to add add all or some of currently
        ///                     loaded assemblies as dependencies for  dynamically generated assemblies.
        ///                     Use an instance of <see cref="AllLoadedAssemblies"/> to add references to all assemblies loaded into current application
        ///                     domain to the dynamically generated assembly. Use <see cref="NoLoadedAssemblies"/> to not add any additional assemblies
        ///                     references to any additional assemblies as dependencies for dynamically generated assemblies.
        ///                     Provide your own implementation to add only some of loaded assemblies as dependencies.
        /// </param>
        /// <param name="referencedAssemblyPaths">
        ///     Assembly paths for assemblies that will be added as references to generated
        ///     assembly.
        /// </param>
        /// <returns>Returns an instance of <see cref="IDynamicAssemblyBuilder" />.</returns>
        IDynamicAssemblyBuilder StartDynamicAssemblyBuilder([NotNull] string dynamicAssemblyPath, [CanBeNull] OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete,
                                                            [NotNull] ILoadedAssemblies loadedAssemblies,
                                                            [CanBeNull][ItemNotNull] params string[] referencedAssemblyPaths);

#pragma warning restore CS0419, CS1574

        /// <summary>
        /// Loads the assembly using from file <paramref name="assemblyFilePath"/>.
        /// </summary>
        /// <param name="assemblyFilePath">Assembly file path.</param>
        /// <returns>Returns the loaded assembly.</returns>
        /// <exception cref="Exception">Throws an exception if load fails.</exception>
        [NotNull]
        System.Reflection.Assembly LoadAssembly([NotNull] string assemblyFilePath);
    }
}