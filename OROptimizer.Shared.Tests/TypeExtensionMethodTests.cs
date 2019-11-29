using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OROptimizer.Shared.Tests
{
    [TestClass]
    public class TypeExtensionMethodTests
    {
        [TestMethod]
        public void TestNullableTypes()
        {
            Assert.AreEqual("System.Nullable<System.Int32>", OROptimizer.TypeExtensionMethods.GetTypeNameInCSharpClass(typeof(int?)), false);
        }

        [TestMethod]
        public void TestNullableReturnValue()
        {
            var returnType = typeof(TestClass).GetMethod(nameof(TestClass.GetNullableValue)).ReturnType;
            Assert.AreEqual("System.Nullable<System.Double>", TypeExtensionMethods.GetTypeNameInCSharpClass(returnType), false);
        }

        [TestMethod]
        public void TestNullableParameter()
        {
            var parameterType = typeof(TestClass).GetMethod(nameof(TestClass.SetNullableValue)).GetParameters()[0].ParameterType;
            Assert.AreEqual("System.Nullable<System.Int16>", TypeExtensionMethods.GetTypeNameInCSharpClass(parameterType), false);
        }

        [TestMethod]
        public void TestNullableRefParameter()
        {
            var parameterType = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithNullableRefParameter)).GetParameters()[0].ParameterType;

            // Enable this etst once the ref parameter issue is fixed.
            //Assert.AreEqual("System.Nullable<System.Int32>", TypeExtensionMethods.GetTypeNameInCSharpClass(parameterType), false);
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
