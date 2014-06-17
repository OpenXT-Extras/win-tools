//
// Copyright (c) 2012 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//


// Auto-generated, so wouldn't recommending editing by hand...

#define _READERMAGIC
#define _WRITERMAGIC

#if _READERMAGIC
namespace Udbus.Serialization
{
    public partial class UdbusMessageReader
    {
        #region Variant reading
        public int ReadVariantObjectPath(Udbus.Containers.dbus_union variant)
        {
            Udbus.Types.UdbusObjectPath val;
            int result = this.ReadObjectPath(out val);
            variant.DbusObjectPath = val;
            return result;
        }

        public int ReadVariantObjectPath(out Udbus.Containers.dbus_union variant)
        {
            Udbus.Types.UdbusObjectPath val;
            int result = this.ReadObjectPath(out val);
            // variant.DbusObjectPath = val;
            variant = Udbus.Containers.dbus_union.CreateObjectPath(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantObjectPath()
        {
            Udbus.Types.UdbusObjectPath val = this.ReadObjectPath();
            return Udbus.Containers.dbus_union.CreateObjectPath(val);
        }

        public Udbus.Containers.dbus_union ReadVariantObjectPathValue(out int result)
        {
            Udbus.Types.UdbusObjectPath val = this.ReadObjectPathValue(out result);
            return Udbus.Containers.dbus_union.CreateObjectPath(val);
        }

        public int ReadVariantBoolean(Udbus.Containers.dbus_union variant)
        {
            bool val;
            int result = this.ReadBoolean(out val);
            variant.DbusBoolean = val;
            return result;
        }

        public int ReadVariantBoolean(out Udbus.Containers.dbus_union variant)
        {
            bool val;
            int result = this.ReadBoolean(out val);
            // variant.DbusBoolean = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantBoolean()
        {
            bool val = this.ReadBoolean();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantBooleanValue(out int result)
        {
            bool val = this.ReadBooleanValue(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantByte(Udbus.Containers.dbus_union variant)
        {
            byte val;
            int result = this.ReadByte(out val);
            variant.DbusByte = val;
            return result;
        }

        public int ReadVariantByte(out Udbus.Containers.dbus_union variant)
        {
            byte val;
            int result = this.ReadByte(out val);
            // variant.DbusByte = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantByte()
        {
            byte val = this.ReadByte();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantByteValue(out int result)
        {
            byte val = this.ReadByteValue(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantString(Udbus.Containers.dbus_union variant)
        {
            string val;
            int result = this.ReadString(out val);
            variant.DbusString = val;
            return result;
        }

        public int ReadVariantString(out Udbus.Containers.dbus_union variant)
        {
            string val;
            int result = this.ReadString(out val);
            // variant.DbusString = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantString()
        {
            string val = this.ReadString();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantStringValue(out int result)
        {
            string val = this.ReadStringValue(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantInt16(Udbus.Containers.dbus_union variant)
        {
            System.Int16 val;
            int result = this.ReadInt16(out val);
            variant.DbusInt16 = val;
            return result;
        }

        public int ReadVariantInt16(out Udbus.Containers.dbus_union variant)
        {
            System.Int16 val;
            int result = this.ReadInt16(out val);
            // variant.DbusInt16 = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantInt16()
        {
            System.Int16 val = this.ReadInt16();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantInt16Value(out int result)
        {
            System.Int16 val = this.ReadInt16Value(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantUInt16(Udbus.Containers.dbus_union variant)
        {
            System.UInt16 val;
            int result = this.ReadUInt16(out val);
            variant.DbusUInt16 = val;
            return result;
        }

        public int ReadVariantUInt16(out Udbus.Containers.dbus_union variant)
        {
            System.UInt16 val;
            int result = this.ReadUInt16(out val);
            // variant.DbusUInt16 = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantUInt16()
        {
            System.UInt16 val = this.ReadUInt16();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantUInt16Value(out int result)
        {
            System.UInt16 val = this.ReadUInt16Value(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantInt32(Udbus.Containers.dbus_union variant)
        {
            System.Int32 val;
            int result = this.ReadInt32(out val);
            variant.DbusInt32 = val;
            return result;
        }

        public int ReadVariantInt32(out Udbus.Containers.dbus_union variant)
        {
            System.Int32 val;
            int result = this.ReadInt32(out val);
            // variant.DbusInt32 = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantInt32()
        {
            System.Int32 val = this.ReadInt32();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantInt32Value(out int result)
        {
            System.Int32 val = this.ReadInt32Value(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantUInt32(Udbus.Containers.dbus_union variant)
        {
            System.UInt32 val;
            int result = this.ReadUInt32(out val);
            variant.DbusUInt32 = val;
            return result;
        }

        public int ReadVariantUInt32(out Udbus.Containers.dbus_union variant)
        {
            System.UInt32 val;
            int result = this.ReadUInt32(out val);
            // variant.DbusUInt32 = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantUInt32()
        {
            System.UInt32 val = this.ReadUInt32();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantUInt32Value(out int result)
        {
            System.UInt32 val = this.ReadUInt32Value(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantInt64(Udbus.Containers.dbus_union variant)
        {
            System.Int64 val;
            int result = this.ReadInt64(out val);
            variant.DbusInt64 = val;
            return result;
        }

        public int ReadVariantInt64(out Udbus.Containers.dbus_union variant)
        {
            System.Int64 val;
            int result = this.ReadInt64(out val);
            // variant.DbusInt64 = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantInt64()
        {
            System.Int64 val = this.ReadInt64();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantInt64Value(out int result)
        {
            System.Int64 val = this.ReadInt64Value(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantUInt64(Udbus.Containers.dbus_union variant)
        {
            System.UInt64 val;
            int result = this.ReadUInt64(out val);
            variant.DbusUInt64 = val;
            return result;
        }

        public int ReadVariantUInt64(out Udbus.Containers.dbus_union variant)
        {
            System.UInt64 val;
            int result = this.ReadUInt64(out val);
            // variant.DbusUInt64 = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantUInt64()
        {
            System.UInt64 val = this.ReadUInt64();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantUInt64Value(out int result)
        {
            System.UInt64 val = this.ReadUInt64Value(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantDouble(Udbus.Containers.dbus_union variant)
        {
            double val;
            int result = this.ReadDouble(out val);
            variant.DbusDouble = val;
            return result;
        }

        public int ReadVariantDouble(out Udbus.Containers.dbus_union variant)
        {
            double val;
            int result = this.ReadDouble(out val);
            // variant.DbusDouble = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantDouble()
        {
            double val = this.ReadDouble();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantDoubleValue(out int result)
        {
            double val = this.ReadDoubleValue(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public int ReadVariantVariant(Udbus.Containers.dbus_union variant)
        {
            Udbus.Containers.dbus_union val;
            int result = this.ReadVariant(out val);
            variant.DbusVariant = val;
            return result;
        }

        public int ReadVariantVariant(out Udbus.Containers.dbus_union variant)
        {
            Udbus.Containers.dbus_union val;
            int result = this.ReadVariant(out val);
            // variant.DbusVariant = val;
            variant = Udbus.Containers.dbus_union.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_union ReadVariantVariant()
        {
            Udbus.Containers.dbus_union val = this.ReadVariant();
            return Udbus.Containers.dbus_union.Create(val);
        }

        public Udbus.Containers.dbus_union ReadVariantVariantValue(out int result)
        {
            Udbus.Containers.dbus_union val = this.ReadVariantValue(out result);
            return Udbus.Containers.dbus_union.Create(val);
        }

        public static int ReadVariantObjectPath(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantObjectPath(value);
        }

        public static int ReadVariantObjectPath(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantObjectPath(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantObjectPathValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantObjectPathValue(out result);
        }

        public static int ReadVariantBoolean(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantBoolean(value);
        }

        public static int ReadVariantBoolean(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantBoolean(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantBooleanValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantBooleanValue(out result);
        }

        public static int ReadVariantByte(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantByte(value);
        }

        public static int ReadVariantByte(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantByte(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantByteValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantByteValue(out result);
        }

        public static int ReadVariantString(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantString(value);
        }

        public static int ReadVariantString(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantString(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantStringValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantStringValue(out result);
        }

        public static int ReadVariantInt16(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantInt16(value);
        }

        public static int ReadVariantInt16(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantInt16(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantInt16Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantInt16Value(out result);
        }

        public static int ReadVariantUInt16(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantUInt16(value);
        }

        public static int ReadVariantUInt16(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantUInt16(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantUInt16Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantUInt16Value(out result);
        }

        public static int ReadVariantInt32(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantInt32(value);
        }

        public static int ReadVariantInt32(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantInt32(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantInt32Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantInt32Value(out result);
        }

        public static int ReadVariantUInt32(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantUInt32(value);
        }

        public static int ReadVariantUInt32(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantUInt32(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantUInt32Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantUInt32Value(out result);
        }

        public static int ReadVariantInt64(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantInt64(value);
        }

        public static int ReadVariantInt64(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantInt64(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantInt64Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantInt64Value(out result);
        }

        public static int ReadVariantUInt64(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantUInt64(value);
        }

        public static int ReadVariantUInt64(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantUInt64(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantUInt64Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantUInt64Value(out result);
        }

        public static int ReadVariantDouble(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantDouble(value);
        }

        public static int ReadVariantDouble(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantDouble(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantDoubleValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantDoubleValue(out result);
        }

        public static int ReadVariantVariant(UdbusMessageReader reader, Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantVariant(value);
        }

        public static int ReadVariantVariant(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariantVariant(out value);
        }

        public static Udbus.Containers.dbus_union ReadVariantVariantValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantVariantValue(out result);
        }

        #endregion // Variant reading
        #region Primitive reading
        public int ReadPrimitiveObjectPath(Udbus.Containers.dbus_primitive variant)
        {
            Udbus.Types.UdbusObjectPath val;
            int result = this.ReadObjectPath(out val);
            variant.DbusObjectPath = val;
            return result;
        }

        public int ReadPrimitiveObjectPath(out Udbus.Containers.dbus_primitive variant)
        {
            Udbus.Types.UdbusObjectPath val;
            int result = this.ReadObjectPath(out val);
            // variant.DbusObjectPath = val;
            variant = Udbus.Containers.dbus_primitive.CreateObjectPath(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveObjectPath()
        {
            Udbus.Types.UdbusObjectPath val = this.ReadObjectPath();
            return Udbus.Containers.dbus_primitive.CreateObjectPath(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveObjectPathValue(out int result)
        {
            Udbus.Types.UdbusObjectPath val = this.ReadObjectPathValue(out result);
            return Udbus.Containers.dbus_primitive.CreateObjectPath(val);
        }

        public int ReadPrimitiveBoolean(Udbus.Containers.dbus_primitive variant)
        {
            bool val;
            int result = this.ReadBoolean(out val);
            variant.DbusBoolean = val;
            return result;
        }

        public int ReadPrimitiveBoolean(out Udbus.Containers.dbus_primitive variant)
        {
            bool val;
            int result = this.ReadBoolean(out val);
            // variant.DbusBoolean = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveBoolean()
        {
            bool val = this.ReadBoolean();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveBooleanValue(out int result)
        {
            bool val = this.ReadBooleanValue(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveByte(Udbus.Containers.dbus_primitive variant)
        {
            byte val;
            int result = this.ReadByte(out val);
            variant.DbusByte = val;
            return result;
        }

        public int ReadPrimitiveByte(out Udbus.Containers.dbus_primitive variant)
        {
            byte val;
            int result = this.ReadByte(out val);
            // variant.DbusByte = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveByte()
        {
            byte val = this.ReadByte();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveByteValue(out int result)
        {
            byte val = this.ReadByteValue(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveString(Udbus.Containers.dbus_primitive variant)
        {
            string val;
            int result = this.ReadString(out val);
            variant.DbusString = val;
            return result;
        }

        public int ReadPrimitiveString(out Udbus.Containers.dbus_primitive variant)
        {
            string val;
            int result = this.ReadString(out val);
            // variant.DbusString = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveString()
        {
            string val = this.ReadString();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveStringValue(out int result)
        {
            string val = this.ReadStringValue(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveInt16(Udbus.Containers.dbus_primitive variant)
        {
            System.Int16 val;
            int result = this.ReadInt16(out val);
            variant.DbusInt16 = val;
            return result;
        }

        public int ReadPrimitiveInt16(out Udbus.Containers.dbus_primitive variant)
        {
            System.Int16 val;
            int result = this.ReadInt16(out val);
            // variant.DbusInt16 = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveInt16()
        {
            System.Int16 val = this.ReadInt16();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveInt16Value(out int result)
        {
            System.Int16 val = this.ReadInt16Value(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveUInt16(Udbus.Containers.dbus_primitive variant)
        {
            System.UInt16 val;
            int result = this.ReadUInt16(out val);
            variant.DbusUInt16 = val;
            return result;
        }

        public int ReadPrimitiveUInt16(out Udbus.Containers.dbus_primitive variant)
        {
            System.UInt16 val;
            int result = this.ReadUInt16(out val);
            // variant.DbusUInt16 = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveUInt16()
        {
            System.UInt16 val = this.ReadUInt16();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveUInt16Value(out int result)
        {
            System.UInt16 val = this.ReadUInt16Value(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveInt32(Udbus.Containers.dbus_primitive variant)
        {
            System.Int32 val;
            int result = this.ReadInt32(out val);
            variant.DbusInt32 = val;
            return result;
        }

        public int ReadPrimitiveInt32(out Udbus.Containers.dbus_primitive variant)
        {
            System.Int32 val;
            int result = this.ReadInt32(out val);
            // variant.DbusInt32 = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveInt32()
        {
            System.Int32 val = this.ReadInt32();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveInt32Value(out int result)
        {
            System.Int32 val = this.ReadInt32Value(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveUInt32(Udbus.Containers.dbus_primitive variant)
        {
            System.UInt32 val;
            int result = this.ReadUInt32(out val);
            variant.DbusUInt32 = val;
            return result;
        }

        public int ReadPrimitiveUInt32(out Udbus.Containers.dbus_primitive variant)
        {
            System.UInt32 val;
            int result = this.ReadUInt32(out val);
            // variant.DbusUInt32 = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveUInt32()
        {
            System.UInt32 val = this.ReadUInt32();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveUInt32Value(out int result)
        {
            System.UInt32 val = this.ReadUInt32Value(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveInt64(Udbus.Containers.dbus_primitive variant)
        {
            System.Int64 val;
            int result = this.ReadInt64(out val);
            variant.DbusInt64 = val;
            return result;
        }

        public int ReadPrimitiveInt64(out Udbus.Containers.dbus_primitive variant)
        {
            System.Int64 val;
            int result = this.ReadInt64(out val);
            // variant.DbusInt64 = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveInt64()
        {
            System.Int64 val = this.ReadInt64();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveInt64Value(out int result)
        {
            System.Int64 val = this.ReadInt64Value(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveUInt64(Udbus.Containers.dbus_primitive variant)
        {
            System.UInt64 val;
            int result = this.ReadUInt64(out val);
            variant.DbusUInt64 = val;
            return result;
        }

        public int ReadPrimitiveUInt64(out Udbus.Containers.dbus_primitive variant)
        {
            System.UInt64 val;
            int result = this.ReadUInt64(out val);
            // variant.DbusUInt64 = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveUInt64()
        {
            System.UInt64 val = this.ReadUInt64();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveUInt64Value(out int result)
        {
            System.UInt64 val = this.ReadUInt64Value(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public int ReadPrimitiveDouble(Udbus.Containers.dbus_primitive variant)
        {
            double val;
            int result = this.ReadDouble(out val);
            variant.DbusDouble = val;
            return result;
        }

        public int ReadPrimitiveDouble(out Udbus.Containers.dbus_primitive variant)
        {
            double val;
            int result = this.ReadDouble(out val);
            // variant.DbusDouble = val;
            variant = Udbus.Containers.dbus_primitive.Create(val);
            return result;
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveDouble()
        {
            double val = this.ReadDouble();
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public Udbus.Containers.dbus_primitive ReadPrimitiveDoubleValue(out int result)
        {
            double val = this.ReadDoubleValue(out result);
            return Udbus.Containers.dbus_primitive.Create(val);
        }

        public static int ReadPrimitiveObjectPath(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveObjectPath(value);
        }

        public static int ReadPrimitiveObjectPath(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveObjectPath(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveObjectPathValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveObjectPathValue(out result);
        }

        public static int ReadPrimitiveBoolean(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveBoolean(value);
        }

        public static int ReadPrimitiveBoolean(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveBoolean(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveBooleanValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveBooleanValue(out result);
        }

        public static int ReadPrimitiveByte(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveByte(value);
        }

        public static int ReadPrimitiveByte(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveByte(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveByteValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveByteValue(out result);
        }

        public static int ReadPrimitiveString(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveString(value);
        }

        public static int ReadPrimitiveString(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveString(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveStringValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveStringValue(out result);
        }

        public static int ReadPrimitiveInt16(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveInt16(value);
        }

        public static int ReadPrimitiveInt16(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveInt16(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveInt16Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveInt16Value(out result);
        }

        public static int ReadPrimitiveUInt16(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveUInt16(value);
        }

        public static int ReadPrimitiveUInt16(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveUInt16(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveUInt16Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveUInt16Value(out result);
        }

        public static int ReadPrimitiveInt32(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveInt32(value);
        }

        public static int ReadPrimitiveInt32(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveInt32(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveInt32Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveInt32Value(out result);
        }

        public static int ReadPrimitiveUInt32(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveUInt32(value);
        }

        public static int ReadPrimitiveUInt32(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveUInt32(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveUInt32Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveUInt32Value(out result);
        }

        public static int ReadPrimitiveInt64(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveInt64(value);
        }

        public static int ReadPrimitiveInt64(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveInt64(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveInt64Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveInt64Value(out result);
        }

        public static int ReadPrimitiveUInt64(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveUInt64(value);
        }

        public static int ReadPrimitiveUInt64(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveUInt64(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveUInt64Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveUInt64Value(out result);
        }

        public static int ReadPrimitiveDouble(UdbusMessageReader reader, Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveDouble(value);
        }

        public static int ReadPrimitiveDouble(UdbusMessageReader reader, out Udbus.Containers.dbus_primitive value)
        {
            return reader.ReadPrimitiveDouble(out value);
        }

        public static Udbus.Containers.dbus_primitive ReadPrimitiveDoubleValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadPrimitiveDoubleValue(out result);
        }

        #endregion // Primitive reading

    } // Ends class UdbusMessageReader
} // Ends namespace Udbus.Serialization
#endif // _READERMAGIC

#if _WRITERMAGIC
namespace Udbus.Serialization
{
    public partial class UdbusMessageBuilder
    {
        #region Variant writing
        public int BodyAdd_VariantObjectPath(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_ObjectPath(variant.DbusObjectPath).Result;
        }

        public int BodyAdd_VariantBoolean(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Boolean(variant.DbusBoolean).Result;
        }

        public int BodyAdd_VariantByte(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Byte(variant.DbusByte).Result;
        }

        public int BodyAdd_VariantString(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_String(variant.DbusString).Result;
        }

        public int BodyAdd_VariantInt16(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Int16(variant.DbusInt16).Result;
        }

        public int BodyAdd_VariantUInt16(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_UInt16(variant.DbusUInt16).Result;
        }

        public int BodyAdd_VariantInt32(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Int32(variant.DbusInt32).Result;
        }

        public int BodyAdd_VariantUInt32(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_UInt32(variant.DbusUInt32).Result;
        }

        public int BodyAdd_VariantInt64(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Int64(variant.DbusInt64).Result;
        }

        public int BodyAdd_VariantUInt64(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_UInt64(variant.DbusUInt64).Result;
        }

        public int BodyAdd_VariantDouble(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Double(variant.DbusDouble).Result;
        }

        public int BodyAdd_VariantVariant(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Variant(variant.DbusVariant).Result;
        }

        public static int BodyAdd_VariantObjectPath(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantObjectPath(value);
        }

        public static int BodyAdd_VariantBoolean(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantBoolean(value);
        }

        public static int BodyAdd_VariantByte(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantByte(value);
        }

        public static int BodyAdd_VariantString(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantString(value);
        }

        public static int BodyAdd_VariantInt16(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantInt16(value);
        }

        public static int BodyAdd_VariantUInt16(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantUInt16(value);
        }

        public static int BodyAdd_VariantInt32(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantInt32(value);
        }

        public static int BodyAdd_VariantUInt32(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantUInt32(value);
        }

        public static int BodyAdd_VariantInt64(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantInt64(value);
        }

        public static int BodyAdd_VariantUInt64(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantUInt64(value);
        }

        public static int BodyAdd_VariantDouble(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantDouble(value);
        }

        public static int BodyAdd_VariantVariant(UdbusMessageBuilder builder, Udbus.Containers.dbus_union value)
        {
            return builder.BodyAdd_VariantVariant(value);
        }

        #endregion // Variant writing
        #region Primitive reading
        public int BodyAdd_PrimitiveObjectPath(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_ObjectPath(variant.DbusObjectPath).Result;
        }

        public int BodyAdd_PrimitiveBoolean(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_Boolean(variant.DbusBoolean).Result;
        }

        public int BodyAdd_PrimitiveByte(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_Byte(variant.DbusByte).Result;
        }

        public int BodyAdd_PrimitiveString(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_String(variant.DbusString).Result;
        }

        public int BodyAdd_PrimitiveInt16(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_Int16(variant.DbusInt16).Result;
        }

        public int BodyAdd_PrimitiveUInt16(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_UInt16(variant.DbusUInt16).Result;
        }

        public int BodyAdd_PrimitiveInt32(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_Int32(variant.DbusInt32).Result;
        }

        public int BodyAdd_PrimitiveUInt32(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_UInt32(variant.DbusUInt32).Result;
        }

        public int BodyAdd_PrimitiveInt64(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_Int64(variant.DbusInt64).Result;
        }

        public int BodyAdd_PrimitiveUInt64(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_UInt64(variant.DbusUInt64).Result;
        }

        public int BodyAdd_PrimitiveDouble(Udbus.Containers.dbus_primitive variant)
        {
            return this.BodyAdd_Double(variant.DbusDouble).Result;
        }

        public static int BodyAdd_PrimitiveObjectPath(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveObjectPath(value);
        }

        public static int BodyAdd_PrimitiveBoolean(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveBoolean(value);
        }

        public static int BodyAdd_PrimitiveByte(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveByte(value);
        }

        public static int BodyAdd_PrimitiveString(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveString(value);
        }

        public static int BodyAdd_PrimitiveInt16(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveInt16(value);
        }

        public static int BodyAdd_PrimitiveUInt16(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveUInt16(value);
        }

        public static int BodyAdd_PrimitiveInt32(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveInt32(value);
        }

        public static int BodyAdd_PrimitiveUInt32(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveUInt32(value);
        }

        public static int BodyAdd_PrimitiveInt64(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveInt64(value);
        }

        public static int BodyAdd_PrimitiveUInt64(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveUInt64(value);
        }

        public static int BodyAdd_PrimitiveDouble(UdbusMessageBuilder builder, Udbus.Containers.dbus_primitive value)
        {
            return builder.BodyAdd_PrimitiveDouble(value);
        }

        #endregion // Primitive reading
    } // Ends class UdbusMessageBuilder
} // Ends namespace Udbus.Serialization
#endif // _WRITERMAGIC
