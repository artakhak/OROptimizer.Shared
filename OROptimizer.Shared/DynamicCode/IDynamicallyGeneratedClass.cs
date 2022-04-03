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

namespace OROptimizer.DynamicCode
{
    public interface IDynamicallyGeneratedClass
    {
        /// <summary>
        /// Adds a code to class code.
        /// </summary>
        /// <param name="cSharpCode">Added code.</param>
        void AddCode([NotNull] string cSharpCode);

        /// <summary>
        /// Adds code and a line break to class code.
        /// </summary>
        /// <param name="cSharpCode">Added code.</param>
        void AddCodeLine([NotNull] string cSharpCode);

        /// <summary>
        /// Adds a using statement to class. Example: dynamicallyGeneratedClass.AddUsingNamespaceStatement("System.Collections.Generic");
        /// </summary>
        /// <param name="nameSpace">Namespace like.</param>
        void AddUsingNamespaceStatement([NotNull] string nameSpace);

        [Obsolete("Will be removed after 05/31/2023. Use 'AddUsingNamespaceStatement(string nameSpace)' instead.  Will be removed after 05/31/2023.")]
        void AddUsingNamespaceStatment([NotNull] string nameSpace);

        /// <summary>
        /// Class full name.
        /// </summary>
        [NotNull]
        string ClassFullName { get; }

        /// <summary>
        /// Class name.
        /// </summary>
        [NotNull]
        string ClassName { get; }

        /// <summary>
        /// Class namespace.
        /// </summary>
        [NotNull]
        string ClassNamespace { get; }

        /// <summary>
        /// This method will be called by <see cref="IDynamicAssemblyBuilder"/> before the assembly is finalized.
        /// Generates the class contents and returns the generate C# file contents as a string. The implementation might add any missing closing braces, etc before the assembly is build.
        /// Also, the implementation should handle cases when the method is called multiple times (say check if class was already finalized, etc).
        /// </summary>
        [NotNull]
        string GenerateCSharpFile();

        /// <summary>
        ///     Finalizes the and add to assembly.
        /// </summary>
        [Obsolete(" Will be removed after 05/31/2023. Use GenerateCSharpFile() instead. The class is added to assembly when OROptimizer.DynamicCode.IDynamicAssemblyBuilder.StartDynamicallyGeneratedClass() is called. OROptimizer.DynamicCode.IDynamicAssemblyBuilder will call  anymore. The class is finalized in IDynamicAssemblyBuilder.Dispose() method. The new implementation of this method does not do anything. This method will be removed in near future.")]
        void FinalizeAndAddToAssembly();

        /// <summary>
        ///     Indicates weather the class was added to assembly.
        /// </summary>
        bool IsFinalized { get; }
        
        /// <summary>
        ///     Adds a constructor without body to class code. Example of added code is
        ///     public MyClass1(int param1).
        /// </summary>
        /// <param name="parametersData">The parameters data.</param>
        /// <param name="accessLevel">The access level.</param>
        /// <param name="isStatic">If true, the constructor will be static.</param>
        /// <returns>
        ///     Returns an instance of <see cref="IDynamicallyGeneratedFunctionData" /> that can be used to add code to
        ///     constructor.
        /// </returns>
        [NotNull]
        IDynamicallyGeneratedConstructorData StartConstructor([NotNull] [ItemNotNull] IEnumerable<IParameterInfo> parametersData,
                                                              AccessLevel accessLevel, bool isStatic);

        /// <summary>
        ///     Adds a method without body to class code. Example of added code is
        ///     public void StartFactoryMethod(int param1)
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="returnedValueType">Returned value type.</param>
        /// <param name="parametersData">Parameters data.</param>
        /// <param name="isStatic">If true, the method will be static.</param>
        /// <param name="addUniqueIdPostfix">Adds a unique postfix to method name.</param>
        /// <param name="accessLevel">The access level.</param>
        /// <returns>
        ///     Returns the added method data as an instance of <see cref="IDynamicallyGeneratedMethodData"/>. If <paramref name="addUniqueIdPostfix" /> is true, the method name will be
        ///     generated by appending a unique id to <paramref name="methodName" />
        /// </returns>
        [NotNull]
        IDynamicallyGeneratedMethodData StartMethod([NotNull] string methodName, [NotNull] Type returnedValueType, [NotNull] [ItemNotNull] IEnumerable<IMethodParameterInfo> parametersData,
                                                    AccessLevel accessLevel, bool isStatic, bool addUniqueIdPostfix);

        /// <summary>
        ///  Adds an implemented method without body to class code. Examples of added code are
        ///     void IMyInterface.StartFactoryMethod(int param1) or
        ///     public void StartFactoryMethod(int param1).
        /// </summary>
        /// <param name="methodInfo">An instance of <see cref="System.Reflection.MethodInfo"/>. The method in <paramref name="methodInfo"/> should be an interface method.</param>
        /// <param name="isExplicitMethod">If true, the method is an explicit interface method implementation.
        ///     Example: void IMyInterface.StartFactoryMethod(int param1)</param>
        /// <returns>
        ///     Returns the added method data as an instance of <see cref="IDynamicallyGeneratedMethodData"/>.
        /// </returns>
        [NotNull]
        IDynamicallyGeneratedMethodData StartInterfaceImplementationMethod([NotNull] MethodInfo methodInfo, bool isExplicitMethod);

        /// <summary>
        ///  Adds an an overridden method without body to class code. Examples of added code is:     
        ///     public override void StartFactoryMethod(int param1).
        /// </summary>
        /// <param name="methodInfo">An instance of <see cref="System.Reflection.MethodInfo"/>. The method in <paramref name="methodInfo"/> should be an interface method.</param>
        /// <returns>
        ///     Returns the added method data as an instance of <see cref="IDynamicallyGeneratedMethodData"/>.
        /// </returns>
        [NotNull]
        IDynamicallyGeneratedMethodData StartOverrideMethod([NotNull] MethodInfo methodInfo);
    }
}