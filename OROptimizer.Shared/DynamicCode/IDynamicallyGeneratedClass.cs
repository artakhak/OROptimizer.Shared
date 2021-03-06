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

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace OROptimizer.DynamicCode
{
    public interface IDynamicallyGeneratedClass
    {
        #region Current Type Interface

        void AddCode([NotNull] string cSharpCode);
        void AddCodeLine([NotNull] string cSharpCode);

        void AddUsingNamespaceStatment([NotNull] string nameSpace);

        [NotNull]
        string ClassFullName { get; }

        [NotNull]
        string ClassName { get; }

        [NotNull]
        string ClassNamespace { get; }

        /// <summary>
        ///     Finalizes the and add to assembly.
        /// </summary>
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
        IDynamicallyGeneratedMethodData StartInterfaceImplementationMethod([NotNull] MethodInfo methodInfo, bool isExplicitMethod);

        /// <summary>
        ///  Adds an an overridden method without body to class code. Examples of added code is:     
        ///     public override void StartFactoryMethod(int param1).
        /// </summary>
        /// <param name="methodInfo">An instance of <see cref="System.Reflection.MethodInfo"/>. The method in <paramref name="methodInfo"/> should be an interface method.</param>
     
        /// <returns>
        ///     Returns the added method data as an instance of <see cref="IDynamicallyGeneratedMethodData"/>.
        /// </returns>
        IDynamicallyGeneratedMethodData StartOverrideMethod([NotNull] MethodInfo methodInfo);

        #endregion
    }
}