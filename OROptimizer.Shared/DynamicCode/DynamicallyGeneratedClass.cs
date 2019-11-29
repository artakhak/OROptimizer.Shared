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
using System.Text;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;
using System.Linq;
using System.Reflection;

namespace OROptimizer.DynamicCode
{
    public class DynamicallyGeneratedClass : IDynamicallyGeneratedClass
    {
        #region Member Variables

        [NotNull]
        private readonly IDynamicAssemblyBuilder _dynamicAssemblyBuilder;

        private readonly List<IDynamicallyGeneratedConstructorData> _inProgressConstructorsData = new List<IDynamicallyGeneratedConstructorData>();
        private readonly List<IDynamicallyGeneratedMethodData> _inProgressMethodsData = new List<IDynamicallyGeneratedMethodData>();

        [NotNull]
        private readonly StringBuilder _sharpCode = new StringBuilder();

        private readonly HashSet<string> _usingStatments = new HashSet<string>();

        #endregion

        #region  Constructors

        /// <summary>
        /// Dynamically generated class data.
        /// </summary>
        /// <param name="dynamicAssemblyBuilder">An instance of <see cref="IDynamicAssemblyBuilder"/></param>
        /// <param name="className">Name of the class.</param>
        /// <param name="classNamespace">The class namespace. If the value is null, the default namespace will be used.</param>
        /// <param name="baseClassesAndInterfaces">List of full base class or interface names.</param>
        public DynamicallyGeneratedClass([NotNull] IDynamicAssemblyBuilder dynamicAssemblyBuilder,
                                         [NotNull] string className, [NotNull] string classNamespace,
                                         [NotNull, ItemNotNull] IEnumerable<string> baseClassesAndInterfaces)
        {
            _dynamicAssemblyBuilder = dynamicAssemblyBuilder;

            ClassName = className;
            ClassNamespace = classNamespace;
            ClassFullName = $"{ClassNamespace}.{ClassName}";

            _sharpCode.AppendLine($"namespace {ClassNamespace}");
            _sharpCode.AppendLine("{");
            _sharpCode.AppendLine($"public class {ClassName}");

            if (baseClassesAndInterfaces.Any())
            {
                _sharpCode.Append(": ");
                _sharpCode.AppendLine(string.Join(",", baseClassesAndInterfaces));
            }

            _sharpCode.AppendLine("{");
        }
       
        #endregion

        #region IDynamicallyGeneratedClass Interface Implementation

        public void AddCode(string cSharpCode)
        {
            _sharpCode.Append(cSharpCode);
        }

        public void AddCodeLine(string cSharpCode)
        {
            AddCode(cSharpCode);
            _sharpCode.AppendLine();
        }

        public void AddUsingNamespaceStatment(string nameSpace)
        {
            if (!_usingStatments.Contains(nameSpace))
                _usingStatments.Add(nameSpace);
        }

        public string ClassFullName { get; }
        public string ClassName { get; }

        public string ClassNamespace { get; }

        public void FinalizeAndAddToAssembly()
        {
            if (IsFinalized)
            {
                LogHelper.Context.Log.WarnFormat("The dynamic class '{0}.{1}' was already to dynamic assembly.",
                    ClassNamespace, ClassName);
                return;
            }

            IsFinalized = true;

            foreach (var inProgressFunctionData in _inProgressConstructorsData)
            {
                _sharpCode.Append(inProgressFunctionData.GetCode());
                _sharpCode.AppendLine();
            }

            foreach (var inProgressFunctionData in _inProgressMethodsData)
            {
                _sharpCode.Append(inProgressFunctionData.GetCode());
                _sharpCode.AppendLine();
            }

            _sharpCode.AppendLine("}");
            _sharpCode.AppendLine("}");


            StringBuilder sharpCodeWithUsingStamentes;

            if (_usingStatments.Count > 0)
            {
                sharpCodeWithUsingStamentes = new StringBuilder();

                foreach (var referencedNamespace in _usingStatments)
                    sharpCodeWithUsingStamentes.AppendLine($"using {referencedNamespace};");

                sharpCodeWithUsingStamentes.Append(_sharpCode);
            }
            else
            {
                sharpCodeWithUsingStamentes = _sharpCode;
            }

            _dynamicAssemblyBuilder.AddCSharpFile(sharpCodeWithUsingStamentes.ToString());
        }

        /// <summary>
        ///     Indicates weather the class was added to assembly.
        /// </summary>
        public bool IsFinalized { get; private set; }

        public IDynamicallyGeneratedConstructorData StartConstructor(IEnumerable<IParameterInfo> parametersData, AccessLevel accessLevel, bool isStatic)
        {
            IDynamicallyGeneratedConstructorData dynamicallyGeneratedConstructorData = new DynamicallyGeneratedConstructorData();
            _inProgressConstructorsData.Add(dynamicallyGeneratedConstructorData);

            dynamicallyGeneratedConstructorData.AddCode(GetAccessLevel(accessLevel));
            dynamicallyGeneratedConstructorData.AddCode(" ");

            if (isStatic)
                dynamicallyGeneratedConstructorData.AddCode("static ");

            dynamicallyGeneratedConstructorData.AddCode(ClassName);
            AddMethodSignature(dynamicallyGeneratedConstructorData, parametersData);

            return dynamicallyGeneratedConstructorData;
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedMethodData StartMethod(string methodName, Type returnedValueType, IEnumerable<IMethodParameterInfo> parametersData, AccessLevel accessLevel, bool isStatic, bool addUniqueIdPostfix)
        {
            if (addUniqueIdPostfix)
                methodName = $"{methodName}_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";

            IDynamicallyGeneratedMethodData dynamicallyGeneratedMethodData = new DynamicallyGeneratedMethodData(methodName);
            _inProgressMethodsData.Add(dynamicallyGeneratedMethodData);

            dynamicallyGeneratedMethodData.AddCode(GetAccessLevel(accessLevel));
            dynamicallyGeneratedMethodData.AddCode(" ");

            if (isStatic)
                dynamicallyGeneratedMethodData.AddCode("static ");

            if (returnedValueType == typeof(void))
                dynamicallyGeneratedMethodData.AddCode("void");
            else
                dynamicallyGeneratedMethodData.AddCode(returnedValueType.GetTypeNameInCSharpClass());

            dynamicallyGeneratedMethodData.AddCode(" ");
            dynamicallyGeneratedMethodData.AddCode(methodName);

            AddMethodSignature(dynamicallyGeneratedMethodData, parametersData);
            return dynamicallyGeneratedMethodData;
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedMethodData StartInterfaceImplementationMethod(MethodInfo methodInfo, bool isExplicitMethod)
        {
            if (methodInfo.IsStatic)
                throw new ArgumentException($"Method '{methodInfo.Name}' cannot be a static method.", nameof(methodInfo));

            if (!methodInfo.DeclaringType.IsInterface)
                throw new ArgumentException($"Method '{methodInfo.Name}' should be an interface method.", nameof(methodInfo));

            if (methodInfo.IsAssembly)
                throw new ArgumentException($"Method '{methodInfo.Name}' cannot be overridden or implemented, since it has 'internal' visibility.", nameof(methodInfo));

            string methodName;

            if (isExplicitMethod)
            {
                methodName = $"{methodInfo.DeclaringType.GetTypeNameInCSharpClass()}.{methodInfo.Name}";
            }
            else
            {
                methodName = methodInfo.Name;
            }

            IDynamicallyGeneratedMethodData dynamicallyGeneratedMethodData = new DynamicallyGeneratedMethodData(methodName);
            _inProgressMethodsData.Add(dynamicallyGeneratedMethodData);

            if (!isExplicitMethod)
                dynamicallyGeneratedMethodData.AddCode("public ");
            

            if (methodInfo.ReturnType == typeof(void))
                dynamicallyGeneratedMethodData.AddCode("void");
            else
                dynamicallyGeneratedMethodData.AddCode(methodInfo.ReturnType.GetTypeNameInCSharpClass());

            dynamicallyGeneratedMethodData.AddCode(" ");
          
            dynamicallyGeneratedMethodData.AddCode(methodName);

            AddOverriddenOrImplementedMethodSignature(dynamicallyGeneratedMethodData, methodInfo);

            return dynamicallyGeneratedMethodData;
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedMethodData StartOverrideMethod(MethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
                throw new ArgumentException($"Method '{methodInfo.Name}' cannot be a static method.", nameof(methodInfo));

            if (methodInfo.IsAssembly)
                throw new ArgumentException($"Method '{methodInfo.Name}' cannot be overridden or implemented, since it has 'internal' visibility.", nameof(methodInfo));

            if (methodInfo.DeclaringType.IsInterface)
                throw new ArgumentException($"Method '{methodInfo.Name}' should not be an interface method.", nameof(methodInfo));

            if (methodInfo.IsFinal)
                throw new ArgumentException($"Method '{methodInfo.Name}' is a final method and cannot be overridden.", nameof(methodInfo));

            if (methodInfo.IsPrivate)
                throw new ArgumentException($"Method '{methodInfo.Name}' should have either public or protected visibility.", nameof(methodInfo));

            if (!methodInfo.IsVirtual)
                throw new ArgumentException($"Method '{methodInfo.Name}' should be a virtual method.", nameof(methodInfo));

            IDynamicallyGeneratedMethodData dynamicallyGeneratedMethodData = new DynamicallyGeneratedMethodData(methodInfo.Name);
            _inProgressMethodsData.Add(dynamicallyGeneratedMethodData);

            if (methodInfo.IsPublic)
                dynamicallyGeneratedMethodData.AddCode("public");
            else
                dynamicallyGeneratedMethodData.AddCode("protected");
         

            dynamicallyGeneratedMethodData.AddCode(" override");

            if (methodInfo.ReturnType == typeof(void))
                dynamicallyGeneratedMethodData.AddCode("void");
            else
                dynamicallyGeneratedMethodData.AddCode(methodInfo.ReturnType.GetTypeNameInCSharpClass());

            dynamicallyGeneratedMethodData.AddCode(" ");


            dynamicallyGeneratedMethodData.AddCode(methodInfo.Name);

            AddOverriddenOrImplementedMethodSignature(dynamicallyGeneratedMethodData, methodInfo);

            return dynamicallyGeneratedMethodData;
        }

        private void AddOverriddenOrImplementedMethodSignature([NotNull] IDynamicallyGeneratedMethodData dynamicallyGeneratedMethodData, 
                                                               MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();

            var parametersData = new List<IMethodParameterInfo>(parameterInfos.Length);

            foreach (var parameterInfo in parameterInfos)
            {
                var methodType = MethodParameterType.Normal;

                if (parameterInfo.IsOut)
                    methodType = MethodParameterType.Output;
                else if (parameterInfo.IsRetval)
                    methodType = MethodParameterType.Reference;

                parametersData.Add(new MethodParameterInfo(parameterInfo.ParameterType, parameterInfo.Name, methodType));
            }

            AddMethodSignature(dynamicallyGeneratedMethodData, parametersData);
        }

        #endregion

        #region Member Functions

        private void AddMethodSignature(IDynamicallyGeneratedFunctionData dynamicallyGeneratedFunctionData, IEnumerable<IParameterInfo> parametersData)
        {
            dynamicallyGeneratedFunctionData.AddCode("(");

            var i = 0;
            foreach (var parameterData in parametersData)
            {
                if (i > 0)
                    dynamicallyGeneratedFunctionData.AddCode(", ");

                if (parameterData is IMethodParameterInfo methodParameterInfo)
                {
                    switch (methodParameterInfo.MethodParameterType)
                    {
                        case MethodParameterType.Output:
                            dynamicallyGeneratedFunctionData.AddCode("out ");
                            break;
                        case MethodParameterType.Reference:
                            dynamicallyGeneratedFunctionData.AddCode("ref ");
                            break;
                    }
                }

                dynamicallyGeneratedFunctionData.AddCode(parameterData.ParameterType.GetTypeNameInCSharpClass());
                dynamicallyGeneratedFunctionData.AddCode(" ");
                dynamicallyGeneratedFunctionData.AddCode(parameterData.Name);

                ++i;
            }

            dynamicallyGeneratedFunctionData.AddCode(")");
            dynamicallyGeneratedFunctionData.AddCodeLine();
        }

        private string GetAccessLevel(AccessLevel accessLevel)
        {
            return accessLevel.ToString().ToLower();
        }

        #endregion
    }
}