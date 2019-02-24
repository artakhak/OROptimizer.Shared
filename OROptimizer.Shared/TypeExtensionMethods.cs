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
using System.Text;
using JetBrains.Annotations;

namespace OROptimizer
{
    public static class TypeExtensionMethods
    {
        #region Member Functions

        public static string GetTypeNameInCSharpClass(this Type type)
        {
            if (type == typeof(void))
                return "void";

            var typeFullName = new StringBuilder();

            string removeRefParameterSuffix(string typeName)
            {
                // ref and out parameter types end with &. Example: System.String&.
                if (typeName[typeName.Length - 1] == '&')
                    return typeName.Substring(0, typeName.Length - 1);

                return typeName;
            }

            if (type.GenericTypeArguments?.Length == 0)
            {
                typeFullName.Append(removeRefParameterSuffix(type.FullName.Replace('+', '.')));
            }
            else
            {
                typeFullName.Append(removeRefParameterSuffix(type.FullName.Substring(0, type.FullName.IndexOf('`')).Replace('+', '.')));
                typeFullName.Append('<');
                for (var i = 0; i < type.GenericTypeArguments.Length; ++i)
                {
                    if (i > 0)
                        typeFullName.Append(',');

                    typeFullName.Append(GetTypeNameInCSharpClass(type.GenericTypeArguments[i]));
                }

                typeFullName.Append('>');
            }

            return typeFullName.ToString();
        }

        public static bool IsTypeAssignableFrom(this Type type, [NotNull] Type type2)
        {
            if (type.IsAssignableFrom(type2))
                return true;

            if (type2 == typeof(sbyte) || type2 == typeof(byte))
            {
                if (type == typeof(short) || type == typeof(int) || type == typeof(long))
                    return true;

                if (type2 == typeof(byte))
                    return type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

                return false;
            }

            if (type2 == typeof(short) || type2 == typeof(ushort))
            {
                if (type == typeof(int) || type == typeof(long))
                    return true;

                if (type2 == typeof(ushort))
                    return type == typeof(uint) || type == typeof(ulong);

                return false;
            }

            if (type2 == typeof(int) || type2 == typeof(uint))
            {
                if (type == typeof(long))
                    return true;

                if (type2 == typeof(uint))
                    return type == typeof(ulong);

                return false;
            }

            return false;
        }

        #endregion
    }
}