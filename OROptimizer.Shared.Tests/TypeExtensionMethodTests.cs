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

using NUnit.Framework;

namespace OROptimizer.Shared.Tests
{
    [TestFixture]
    public class TypeExtensionMethodTests
    {
        [Test]
        public void TestNullableTypes()
        {
            Assert.AreEqual("System.Nullable<System.Int32>", OROptimizer.TypeExtensionMethods.GetTypeNameInCSharpClass(typeof(int?)));
        }

        [Test]
        public void TestNullableReturnValue()
        {
            var returnType = typeof(TestClass).GetMethod(nameof(TestClass.GetNullableValue)).ReturnType;
            Assert.AreEqual("System.Nullable<System.Double>", TypeExtensionMethods.GetTypeNameInCSharpClass(returnType));
        }

        [Test]
        public void TestNullableParameter()
        {
            var parameterType = typeof(TestClass).GetMethod(nameof(TestClass.SetNullableValue)).GetParameters()[0].ParameterType;
            Assert.AreEqual("System.Nullable<System.Int16>", TypeExtensionMethods.GetTypeNameInCSharpClass(parameterType));
        }

        [Test]
        public void TestNullableRefParameter()
        {
            var parameterType = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithNullableRefParameter)).GetParameters()[0].ParameterType;

            // Enable this test once the ref parameter issue is fixed.
            //Assert.AreEqual("System.Nullable<System.Int32>", TypeExtensionMethods.GetTypeNameInCSharpClass(parameterType));
        }

        private class TestClass
        {
            public double? GetNullableValue()
            {
                return 0;
            }

            public void SetNullableValue(short? value)
            {

            }

            public void MethodWithNullableRefParameter(ref int? value)
            {
                value = 13;
            }
        }
    }
}
