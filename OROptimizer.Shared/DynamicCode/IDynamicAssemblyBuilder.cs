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

namespace OROptimizer.DynamicCode
{
    /// <summary>
    ///     Dynamic assembly generator for C# files.
    ///     Example:
    ///     <para />
    ///     using(var assemblyBuilder = new DynamicAssemblyBuilder("c:\Assembly1.dll", null))
    ///     {
    ///     assemblyBuilder.AddReferencedAssembly("c:\Assembly1.dll");
    ///    
    ///     var classBuilderGenerator = assemblyBuilder.StartDynamicallyGeneratedClass("AppDataDirectoryProcessor", "OROptimizer");
    ///     // Build the class using methods in OROptimizer.DynamicCode.IDynamicallyGeneratedClass,
    ///     // like StartConstructor(), StartMethod(), AddCodeLine(),  
    ///     classGenerator.StartConstructor(...);
    /// 
    ///     ...
    ///
    ///     assemblyBuilder.AddCSharpFile("namespace N1 {public class A1 {}}");
    ///     }
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IDynamicAssemblyBuilder : IDisposable
    {
        /// <summary>
        ///     Starts the dynamically generated class and adds it to <see cref="IDynamicallyGeneratedClass"/>. Call 
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="classNamespace">The class namespace. If the value is null, the default namespace will be used.</param>
        [NotNull]
        IDynamicallyGeneratedClass StartDynamicallyGeneratedClass([NotNull] string className, [CanBeNull] string classNamespace = null);

        /// <summary>
        ///     Starts the dynamically generated class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="baseClassesAndInterfaces">List of full base class or interface names.</param>
        /// <param name="classNamespace">The class namespace. If the value is null, the default namespace will be used.</param>
        [NotNull]
        IDynamicallyGeneratedClass StartDynamicallyGeneratedClass([NotNull] string className, [NotNull, ItemNotNull] IEnumerable<string> baseClassesAndInterfaces,
            [CanBeNull] string classNamespace = null);

        /// <summary>
        ///     Adds the c# sharp file to assembly generator. Use this if <see cref="StartDynamicallyGeneratedClass(string, IEnumerable{string}, string)"/> and
        ///     <see cref="StartDynamicallyGeneratedClass(string, string)"/> do not provide enough flexibility.
        /// </summary>
        /// <param name="cSharpFileContents">The c# file contents.</param>
        void AddCSharpFile([NotNull] string cSharpFileContents);

        /// <summary>
        ///     Adds a reference to assembly <paramref name="type" />.Assembly in the generated dynamic assembly.
        /// </summary>
        /// <param name="type">The type.</param>
        void AddReferencedAssembly([NotNull] Type type);

        /// <summary>
        ///     Adds a reference to assembly <paramref name="assemblyPath" /> in the generated dynamic assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        void AddReferencedAssembly([NotNull] string assemblyPath);

        /// <summary>
        ///     Gets the build status.
        /// </summary>
        /// <value>
        ///     The build status.
        /// </value>
        AssemblyBuildStatus BuildStatus { get; }

        /// <summary>
        /// Default namespace.
        /// </summary>
        [NotNull]
        string DefaultNamespace { get; }

        /// <summary>
        ///     Finalizes the dynamically generated class and adds it to assembly.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="classNamespace">The class namespace. If the value is null, the default namespace will be used.</param>
        [Obsolete("Will be removed after 05/31/2023. Classes will be finalized when the assembly is build in IDisposable.Dispose(). No need to call FinalizeDynamicallyGeneratedClass() anymore. The new implementation does not anything and will be deleted in the future. Will be removed after 05/31/2023.")]
        void FinalizeDynamicallyGeneratedClass(string className, [CanBeNull] string classNamespace = null);

        /// <summary>
        ///     Gets the dynamically generated class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="classNamespace">The class namespace. If the value is null, the default namespace will be used.</param>
        [CanBeNull]
        IDynamicallyGeneratedClass GetDynamicallyGeneratedClass([NotNull] string className, [CanBeNull] string classNamespace = null);

        /// <summary>
        ///     Call this method if the assembly generation should be aborted.
        /// </summary>
        void SetIsAborted();
    }
}