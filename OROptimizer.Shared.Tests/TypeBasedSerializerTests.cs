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

using OROptimizer.Serializer;
using System;
using NUnit.Framework;

namespace OROptimizer.Shared.Tests
{
    [TestFixture]
    public class TypeBasedSerializerTests
    {
        [Test]
        public void StringSerializerTest()
        {
            var valueToDeserialize = @"c:\users\user1";
            SerializerTest(new TypeBasedSimpleSerializerString(), valueToDeserialize, valueToDeserialize, valueToDeserialize, "@\"c:\\users\\user1\"");
        }

        [Test]
        public void BoolSerializerTest()
        {
            var valueToDeserialize = "true";
            SerializerTest(new TypeBasedSimpleSerializerBoolean(), valueToDeserialize, true, valueToDeserialize, "true");

            valueToDeserialize = "false";
            SerializerTest(new TypeBasedSimpleSerializerBoolean(), valueToDeserialize, false, valueToDeserialize, "false");
        }

        [Test]
        public void LongSerializerTest()
        {
            var valueToDeserialize = "9223372036854775807";
            SerializerTest(new TypeBasedSimpleSerializerLong(), valueToDeserialize, long.MaxValue, valueToDeserialize, "9223372036854775807");

            valueToDeserialize = "-9223372036854775808";
            SerializerTest(new TypeBasedSimpleSerializerLong(), valueToDeserialize, long.MinValue, valueToDeserialize, "-9223372036854775808");
        }

        [Test]
        public void IntSerializerTest()
        {
            var valueToDeserialize = "2147483647";
            SerializerTest(new TypeBasedSimpleSerializerInt(), valueToDeserialize, int.MaxValue, valueToDeserialize, "2147483647");

            valueToDeserialize = "-2147483648";
            SerializerTest(new TypeBasedSimpleSerializerInt(), valueToDeserialize, int.MinValue, valueToDeserialize, "-2147483648");
        }

        [Test]
        public void ShortSerializerTest()
        {
            var valueToDeserialize = "32767";
            SerializerTest(new TypeBasedSimpleSerializerShort(), valueToDeserialize, short.MaxValue, valueToDeserialize, "32767");

            valueToDeserialize = "-32768";
            SerializerTest(new TypeBasedSimpleSerializerShort(), valueToDeserialize, short.MinValue, valueToDeserialize, "-32768");
        }

        [Test]
        public void ByteSerializerTest()
        {
            var valueToDeserialize = "255";
            SerializerTest(new TypeBasedSimpleSerializerByte(), valueToDeserialize, byte.MaxValue, valueToDeserialize, "255");

            valueToDeserialize = "0";
            SerializerTest(new TypeBasedSimpleSerializerByte(), valueToDeserialize, byte.MinValue, valueToDeserialize, "0");
        }

        [Test]
        public void DoubleSerializerTest()
        {
            var valueToDeserialize = "3147483648.5894";
            SerializerTest(new TypeBasedSimpleSerializerDouble(), valueToDeserialize, 3147483648.5894, valueToDeserialize, valueToDeserialize);

            valueToDeserialize = "-3147483648.5894";
            SerializerTest(new TypeBasedSimpleSerializerDouble(), valueToDeserialize, -3147483648.5894, valueToDeserialize, valueToDeserialize);
        }

        [Test]
        public void DateTimeSerializerTest()
        {
            DateTime dateTime = new DateTime(2019, 1, 15, 13, 17, 28, 535);
            var valueToDeserialize = dateTime.Ticks.ToString();

            SerializerTest(new TypeBasedSimpleSerializerDateTime(), valueToDeserialize, dateTime,
                dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF"), $"new System.DateTime({dateTime.Ticks})");
           
            valueToDeserialize = dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF");
            SerializerTest(new TypeBasedSimpleSerializerDateTime(), valueToDeserialize, dateTime, valueToDeserialize, $"new System.DateTime({dateTime.Ticks})");
        }

        [Test]
        public void GuidSerializerTest()
        {
            //var valueToDeserialize = @"c:\users\user1";
            Guid guid = new Guid("30A43779-F16D-4F93-B57B-C4DF845C1B47");
            var valueToDeserialize = "30A43779-F16D-4F93-B57B-C4DF845C1B47";
            var expectedCSharp = $"new System.Guid(\"{guid.ToString()}\")";
            SerializerTest(new TypeBasedSimpleSerializerGuid(), valueToDeserialize, guid, guid.ToString(), expectedCSharp);

            valueToDeserialize = valueToDeserialize.ToLower();
            SerializerTest(new TypeBasedSimpleSerializerGuid(), valueToDeserialize, guid, guid.ToString(), expectedCSharp);
        }

        private void SerializerTest<T>(TypeBasedSimpleSerializerAbstr<T> serializer, string valueToDeserialize, 
                                       T expectedDeserializedValue, string expectedSerializedValue, string expectedCSharpValue)
        {
            Assert.IsTrue(serializer.TryDeserialize(valueToDeserialize, out var deserializedValue));
            Assert.AreEqual(expectedDeserializedValue, deserializedValue);

            Assert.AreEqual(expectedSerializedValue, serializer.Serialize(deserializedValue));

            var cSharpValue = serializer.GenerateCSharpCode(deserializedValue);
            Assert.AreEqual(expectedCSharpValue, cSharpValue);
        }
    }
}