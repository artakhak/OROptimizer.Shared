using Microsoft.VisualStudio.TestTools.UnitTesting;
using OROptimizer.Serializer;
using System;

namespace OROptimizer.Shared.Tests
{
    [TestClass]
    public class TypeBasedSerializerTests
    {
        [TestMethod]
        public void StringSerializerTest()
        {
            var valueToDeserialize = @"c:\users\user1";
            SerializerTest(new TypeBasedSimpleSerializerString(), valueToDeserialize, valueToDeserialize, valueToDeserialize, "@\"c:\\users\\user1\"");
        }

        [TestMethod]
        public void BoolSerializerTest()
        {
            var valueToDeserialize = "true";
            SerializerTest(new TypeBasedSimpleSerializerBoolean(), valueToDeserialize, true, valueToDeserialize, "true");

            valueToDeserialize = "false";
            SerializerTest(new TypeBasedSimpleSerializerBoolean(), valueToDeserialize, false, valueToDeserialize, "false");
        }

        [TestMethod]
        public void LongSerializerTest()
        {
            var valueToDeserialize = "9223372036854775807";
            SerializerTest(new TypeBasedSimpleSerializerLong(), valueToDeserialize, long.MaxValue, valueToDeserialize, "9223372036854775807");

            valueToDeserialize = "-9223372036854775808";
            SerializerTest(new TypeBasedSimpleSerializerLong(), valueToDeserialize, long.MinValue, valueToDeserialize, "-9223372036854775808");
        }

        [TestMethod]
        public void IntSerializerTest()
        {
            var valueToDeserialize = "2147483647";
            SerializerTest(new TypeBasedSimpleSerializerInt(), valueToDeserialize, int.MaxValue, valueToDeserialize, "2147483647");

            valueToDeserialize = "-2147483648";
            SerializerTest(new TypeBasedSimpleSerializerInt(), valueToDeserialize, int.MinValue, valueToDeserialize, "-2147483648");
        }

        [TestMethod]
        public void ShortSerializerTest()
        {
            var valueToDeserialize = "32767";
            SerializerTest(new TypeBasedSimpleSerializerShort(), valueToDeserialize, short.MaxValue, valueToDeserialize, "32767");

            valueToDeserialize = "-32768";
            SerializerTest(new TypeBasedSimpleSerializerShort(), valueToDeserialize, short.MinValue, valueToDeserialize, "-32768");
        }

        [TestMethod]
        public void ByteSerializerTest()
        {
            var valueToDeserialize = "255";
            SerializerTest(new TypeBasedSimpleSerializerByte(), valueToDeserialize, byte.MaxValue, valueToDeserialize, "255");

            valueToDeserialize = "0";
            SerializerTest(new TypeBasedSimpleSerializerByte(), valueToDeserialize, byte.MinValue, valueToDeserialize, "0");
        }

        [TestMethod]
        public void DoubleSerializerTest()
        {
            var valueToDeserialize = "3147483648.5894";
            SerializerTest(new TypeBasedSimpleSerializerDouble(), valueToDeserialize, 3147483648.5894, valueToDeserialize, valueToDeserialize);

            valueToDeserialize = "-3147483648.5894";
            SerializerTest(new TypeBasedSimpleSerializerDouble(), valueToDeserialize, -3147483648.5894, valueToDeserialize, valueToDeserialize);
        }

        [TestMethod]
        public void DateTimeSerializerTest()
        {
            DateTime dateTime = new DateTime(2019, 1, 15, 13, 17, 28, 535);
            var valueToDeserialize = dateTime.Ticks.ToString();

            SerializerTest(new TypeBasedSimpleSerializerDateTime(), valueToDeserialize, dateTime,
                dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF"), $"new System.DateTime({dateTime.Ticks})");
           
            valueToDeserialize = dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF");
            SerializerTest(new TypeBasedSimpleSerializerDateTime(), valueToDeserialize, dateTime, valueToDeserialize, $"new System.DateTime({dateTime.Ticks})");
        }

        [TestMethod]
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
