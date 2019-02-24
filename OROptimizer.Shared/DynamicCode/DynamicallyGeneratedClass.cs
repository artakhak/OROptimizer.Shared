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

        public DynamicallyGeneratedClass([NotNull] IDynamicAssemblyBuilder dynamicAssemblyBuilder,
                                         [NotNull] string className, [NotNull] string classNamespace)
        {
            _dynamicAssemblyBuilder = dynamicAssemblyBuilder;

            ClassName = className;
            ClassNamespace = classNamespace;
            ClassFullName = $"{ClassNamespace}.{ClassName}";

            _sharpCode.AppendLine($"namespace {ClassNamespace}");
            _sharpCode.AppendLine("{");
            _sharpCode.AppendLine($"public class {ClassName}");
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
                dynamicallyGeneratedMethodData.AddCode(returnedValueType.FullName);

            dynamicallyGeneratedMethodData.AddCode(" ");
            dynamicallyGeneratedMethodData.AddCode(methodName);

            AddMethodSignature(dynamicallyGeneratedMethodData, parametersData);
            return dynamicallyGeneratedMethodData;
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

                dynamicallyGeneratedFunctionData.AddCode(parameterData.ParameterType.FullName);
                dynamicallyGeneratedFunctionData.AddCode(" ");
                dynamicallyGeneratedFunctionData.AddCode(parameterData.Name);
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