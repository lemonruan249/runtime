// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;
using System.Globalization;

namespace System.Runtime.Serialization.Json
{
    internal class JsonWriterDelegator : XmlWriterDelegator
    {
        private DateTimeFormat _dateTimeFormat;

        public JsonWriterDelegator(XmlWriter writer)
            : base(writer)
        {
        }

        public JsonWriterDelegator(XmlWriter writer, DateTimeFormat dateTimeFormat)
            : this(writer)
        {
            _dateTimeFormat = dateTimeFormat;
        }

        internal override void WriteChar(char value)
        {
            WriteString(XmlConvert.ToString(value));
        }

        internal override void WriteBase64(byte[] bytes)
        {
            if (bytes == null)
            {
                return;
            }

            ByteArrayHelperWithString.Instance.WriteArray(Writer, bytes, 0, bytes.Length);
        }

        internal override void WriteQName(XmlQualifiedName value)
        {
            if (value != XmlQualifiedName.Empty)
            {
                writer.WriteString(value.Name);
                writer.WriteString(JsonGlobals.NameValueSeparatorString);
                writer.WriteString(value.Namespace);
            }
        }

        internal override void WriteUnsignedLong(ulong value)
        {
            WriteDecimal((decimal)value);
        }

        internal override void WriteDecimal(decimal value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteDecimal(value);
        }

        internal override void WriteDouble(double value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteDouble(value);
        }

        internal override void WriteFloat(float value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteFloat(value);
        }

        internal override void WriteLong(long value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteLong(value);
        }

        internal override void WriteSignedByte(sbyte value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteSignedByte(value);
        }

        internal override void WriteUnsignedInt(uint value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteUnsignedInt(value);
        }

        internal override void WriteUnsignedShort(ushort value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteUnsignedShort(value);
        }

        internal override void WriteUnsignedByte(byte value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteUnsignedByte(value);
        }

        internal override void WriteShort(short value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteShort(value);
        }

        internal override void WriteBoolean(bool value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.booleanString);
            base.WriteBoolean(value);
        }

        internal override void WriteInt(int value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteInt(value);
        }


        internal void WriteJsonBooleanArray(bool[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteBoolean(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonDateTimeArray(DateTime[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteDateTime(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonDecimalArray(decimal[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteDecimal(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonInt32Array(int[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteInt(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonInt64Array(long[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteLong(value[i], itemName, itemNamespace);
            }
        }

        internal override void WriteDateTime(DateTime value)
        {
            if (_dateTimeFormat == null)
            {
                WriteDateTimeInDefaultFormat(value);
            }
            else
            {
                writer.WriteString(value.ToString(_dateTimeFormat.FormatString, _dateTimeFormat.FormatProvider));
            }
        }

        private void WriteDateTimeInDefaultFormat(DateTime value)
        {
            // ToUniversalTime() truncates dates to DateTime.MaxValue or DateTime.MinValue instead of throwing
            // This will break round-tripping of these dates (see 
            if (value.Kind != DateTimeKind.Utc)
            {
                //long tickCount = value.Ticks - TimeZone.CurrentTimeZone.GetUtcOffset(value).Ticks;
                long tickCount = value.Ticks - TimeZoneInfo.Local.GetUtcOffset(value).Ticks;
                if ((tickCount > DateTime.MaxValue.Ticks) || (tickCount < DateTime.MinValue.Ticks))
                {
                    throw XmlObjectSerializer.CreateSerializationException(SR.JsonDateTimeOutOfRange, new ArgumentOutOfRangeException(nameof(value)));
                }
            }

            writer.WriteString(JsonGlobals.DateTimeStartGuardReader);
            writer.WriteValue((value.ToUniversalTime().Ticks - JsonGlobals.unixEpochTicks) / 10000);

            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Local:
                    // +"zzzz";
                    //TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(value.ToLocalTime());
                    TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(value.ToLocalTime());
                    if (ts.Ticks < 0)
                    {
                        writer.WriteString("-");
                    }
                    else
                    {
                        writer.WriteString("+");
                    }
                    int hours = Math.Abs(ts.Hours);
                    writer.WriteString((hours < 10) ? "0" + hours : hours.ToString(CultureInfo.InvariantCulture));
                    int minutes = Math.Abs(ts.Minutes);
                    writer.WriteString((minutes < 10) ? "0" + minutes : minutes.ToString(CultureInfo.InvariantCulture));
                    break;
                case DateTimeKind.Utc:
                    break;
            }
            writer.WriteString(JsonGlobals.DateTimeEndGuardReader);
        }

        internal void WriteJsonSingleArray(float[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteFloat(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonDoubleArray(double[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteDouble(value[i], itemName, itemNamespace);
            }
        }

        internal override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (localName != null && localName.Length == 0)
            {
                base.WriteStartElement(JsonGlobals.itemString, JsonGlobals.itemString);
                base.WriteAttributeString(null, JsonGlobals.itemString, null, localName);
            }
            else
            {
                base.WriteStartElement(prefix, localName, ns);
            }
        }
    }
}
