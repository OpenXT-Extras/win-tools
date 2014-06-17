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

#if !_MAGIC
// This code was auto-generated, so wouldn't advise editing it...
using System.Collections.Generic;

namespace Udbus.Serialization
{

    public partial class UdbusMessageReader
    {
        #region No result marshalling functions
        #region MarshalEnumerable MarshalReadDelegate overloads returning IEnumerable<T>

        public IEnumerable<bool> MarshalEnumerable (MarshalReadDelegate<bool> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN);
        }

        public IEnumerable<byte> MarshalEnumerable (MarshalReadDelegate<byte> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_BYTE);
        }

        public IEnumerable<string> MarshalEnumerable (MarshalReadDelegate<string> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_STRING);
        }

        public IEnumerable<System.Int16> MarshalEnumerable (MarshalReadDelegate<System.Int16> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT16);
        }

        public IEnumerable<System.UInt16> MarshalEnumerable (MarshalReadDelegate<System.UInt16> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT16);
        }

        public IEnumerable<System.Int32> MarshalEnumerable (MarshalReadDelegate<System.Int32> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT32);
        }

        public IEnumerable<System.UInt32> MarshalEnumerable (MarshalReadDelegate<System.UInt32> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT32);
        }

        public IEnumerable<System.Int64> MarshalEnumerable (MarshalReadDelegate<System.Int64> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT64);
        }

        public IEnumerable<System.UInt64> MarshalEnumerable (MarshalReadDelegate<System.UInt64> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT64);
        }

        public IEnumerable<double> MarshalEnumerable (MarshalReadDelegate<double> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE);
        }

        public IEnumerable<Udbus.Types.UdbusObjectPath> MarshalEnumerable (MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH);
        }

        public IEnumerable<Udbus.Containers.dbus_union> MarshalEnumerable (MarshalReadDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_VARIANT);
        }

        public IEnumerable<IEnumerable<T>> MarshalEnumerable<T> (MarshalReadDelegate<IEnumerable<T>> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_ARRAY);
        }

        public IEnumerable<IDictionary<TKey, TValue>> MarshalEnumerable<TKey, TValue> (MarshalReadDelegate<IDictionary<TKey, TValue>> marshal)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN);
        }

        #endregion // MarshalEnumerable MarshalReadDelegate overloads returning IEnumerable<T>

        #region MarshalEnumerable MarshalReadDelegate overloads with IEnumerable<T> param

        public int MarshalEnumerable (MarshalReadDelegate<bool> marshal, out IEnumerable<bool> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<byte> marshal, out IEnumerable<byte> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_BYTE, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<string> marshal, out IEnumerable<string> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_STRING, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.Int16> marshal, out IEnumerable<System.Int16> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT16, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.UInt16> marshal, out IEnumerable<System.UInt16> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT16, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.Int32> marshal, out IEnumerable<System.Int32> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT32, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.UInt32> marshal, out IEnumerable<System.UInt32> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT32, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.Int64> marshal, out IEnumerable<System.Int64> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT64, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.UInt64> marshal, out IEnumerable<System.UInt64> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT64, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<double> marshal, out IEnumerable<double> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal, out IEnumerable<Udbus.Types.UdbusObjectPath> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<Udbus.Containers.dbus_union> marshal, out IEnumerable<Udbus.Containers.dbus_union> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_VARIANT, out ts);
        }

        public int MarshalEnumerable<T> (MarshalReadDelegate<IEnumerable<T>> marshal, out IEnumerable<IEnumerable<T>> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_ARRAY, out ts);
        }

        public int MarshalEnumerable<TKey, TValue> (MarshalReadDelegate<IDictionary<TKey, TValue>> marshal, out IEnumerable<IDictionary<TKey, TValue>> ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, out ts);
        }

        #endregion // MarshalEnumerable MarshalReadDelegate overloads with IEnumerable<T> param

        #region MarshalEnumerable MarshalReadDelegate overloads with T[] param

        public int MarshalEnumerable (MarshalReadDelegate<bool> marshal, out bool[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<byte> marshal, out byte[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_BYTE, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<string> marshal, out string[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_STRING, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.Int16> marshal, out System.Int16[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT16, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.UInt16> marshal, out System.UInt16[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT16, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.Int32> marshal, out System.Int32[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT32, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.UInt32> marshal, out System.UInt32[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT32, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.Int64> marshal, out System.Int64[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_INT64, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<System.UInt64> marshal, out System.UInt64[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_UINT64, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<double> marshal, out double[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal, out Udbus.Types.UdbusObjectPath[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH, out ts);
        }

        public int MarshalEnumerable (MarshalReadDelegate<Udbus.Containers.dbus_union> marshal, out Udbus.Containers.dbus_union[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_VARIANT, out ts);
        }

        public int MarshalEnumerable<T> (MarshalReadDelegate<T[]> marshal, out T[][] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_ARRAY, out ts);
        }

        public int MarshalEnumerable<TKey, TValue> (MarshalReadDelegate<IDictionary<TKey, TValue>> marshal, out IDictionary<TKey, TValue>[] ts)
        {
            return MarshalEnumerable(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, out ts);
        }

        #endregion // MarshalEnumerable MarshalReadDelegate overloads with T[] param

        #region EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        static public MarshalReadDelegate<IEnumerable<bool>> EnumerableMarshaller(MarshalReadDelegate<bool> marshal)
        {
            return new EnumerableMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<byte>> EnumerableMarshaller(MarshalReadDelegate<byte> marshal)
        {
            return new EnumerableMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<string>> EnumerableMarshaller(MarshalReadDelegate<string> marshal)
        {
            return new EnumerableMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<System.Int16>> EnumerableMarshaller(MarshalReadDelegate<System.Int16> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<System.UInt16>> EnumerableMarshaller(MarshalReadDelegate<System.UInt16> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<System.Int32>> EnumerableMarshaller(MarshalReadDelegate<System.Int32> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<System.UInt32>> EnumerableMarshaller(MarshalReadDelegate<System.UInt32> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<System.Int64>> EnumerableMarshaller(MarshalReadDelegate<System.Int64> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<System.UInt64>> EnumerableMarshaller(MarshalReadDelegate<System.UInt64> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<double>> EnumerableMarshaller(MarshalReadDelegate<double> marshal)
        {
            return new EnumerableMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<Udbus.Types.UdbusObjectPath>> EnumerableMarshaller(MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Types.UdbusObjectPath>(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<Udbus.Containers.dbus_union>> EnumerableMarshaller(MarshalReadDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Containers.dbus_union>(marshal, Udbus.Types.dbus_type.DBUS_VARIANT).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshaller<T>(MarshalReadDelegate<IEnumerable<T>> marshal)
        {
            return new EnumerableMarshallerImpl<IEnumerable<T>>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalReadDelegate<IEnumerable<IDictionary<TKey, TValue>>> EnumerableMarshaller<TKey, TValue>(MarshalReadDelegate<IDictionary<TKey, TValue>> marshal)
        {
            return new EnumerableMarshallerImpl<IDictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        #region ArrayMarshaller MarshalResultDelegate overloads

        static public MarshalReadDelegate<bool[]> ArrayMarshaller(MarshalReadDelegate<bool> marshal)
        {
            return new EnumerableMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalReadDelegate<byte[]> ArrayMarshaller(MarshalReadDelegate<byte> marshal)
        {
            return new EnumerableMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalReadDelegate<string[]> ArrayMarshaller(MarshalReadDelegate<string> marshal)
        {
            return new EnumerableMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalReadDelegate<System.Int16[]> ArrayMarshaller(MarshalReadDelegate<System.Int16> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalReadDelegate<System.UInt16[]> ArrayMarshaller(MarshalReadDelegate<System.UInt16> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalReadDelegate<System.Int32[]> ArrayMarshaller(MarshalReadDelegate<System.Int32> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalReadDelegate<System.UInt32[]> ArrayMarshaller(MarshalReadDelegate<System.UInt32> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalReadDelegate<System.Int64[]> ArrayMarshaller(MarshalReadDelegate<System.Int64> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalReadDelegate<System.UInt64[]> ArrayMarshaller(MarshalReadDelegate<System.UInt64> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalReadDelegate<double[]> ArrayMarshaller(MarshalReadDelegate<double> marshal)
        {
            return new EnumerableMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalReadDelegate<Udbus.Types.UdbusObjectPath[]> ArrayMarshaller(MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Types.UdbusObjectPath>(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH).Marshal;
        }

        static public MarshalReadDelegate<Udbus.Containers.dbus_union[]> ArrayMarshaller(MarshalReadDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Containers.dbus_union>(marshal, Udbus.Types.dbus_type.DBUS_VARIANT).Marshal;
        }

        static public MarshalReadDelegate<T[][]> ArrayMarshaller<T>(MarshalReadDelegate<T[]> marshal)
        {
            return new EnumerableMarshallerImpl<T[]>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalReadDelegate<IDictionary<TKey, TValue>[]> ArrayMarshaller<TKey, TValue>(MarshalReadDelegate<IDictionary<TKey, TValue>> marshal)
        {
            return new EnumerableMarshallerImpl<IDictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // ArrayMarshaller MarshalResultDelegate overloads
        #region Explicit no result marshalling functions
        #region Explicit MarshalEnumerable MarshalReadDelegate overloads returning IEnumerable<T>

        public IEnumerable<bool> MarshalEnumerableBoolean (MarshalReadDelegate<bool> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<byte> MarshalEnumerableByte (MarshalReadDelegate<byte> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<string> MarshalEnumerableString (MarshalReadDelegate<string> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<System.Int16> MarshalEnumerableInt16 (MarshalReadDelegate<System.Int16> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<System.UInt16> MarshalEnumerableUInt16 (MarshalReadDelegate<System.UInt16> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<System.Int32> MarshalEnumerableInt32 (MarshalReadDelegate<System.Int32> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<System.UInt32> MarshalEnumerableUInt32 (MarshalReadDelegate<System.UInt32> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<System.Int64> MarshalEnumerableInt64 (MarshalReadDelegate<System.Int64> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<System.UInt64> MarshalEnumerableUInt64 (MarshalReadDelegate<System.UInt64> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<double> MarshalEnumerableDouble (MarshalReadDelegate<double> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<Udbus.Types.UdbusObjectPath> MarshalEnumerableObjectPath (MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<Udbus.Containers.dbus_union> MarshalEnumerableVariant (MarshalReadDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<IEnumerable<T>> MarshalEnumerableArray<T> (MarshalReadDelegate<IEnumerable<T>> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        public IEnumerable<IDictionary<TKey, TValue>> MarshalEnumerableDictionary<TKey, TValue> (MarshalReadDelegate<IDictionary<TKey, TValue>> marshal)
        {
            return MarshalEnumerable(marshal);
        }

        #endregion // Explicit MarshalEnumerable MarshalReadDelegate overloads returning IEnumerable<T>

        #region Explicit MarshalEnumerable MarshalReadDelegate overloads with IEnumerable<T> param

        public int MarshalEnumerableBoolean (MarshalReadDelegate<bool> marshal, out IEnumerable<bool> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableByte (MarshalReadDelegate<byte> marshal, out IEnumerable<byte> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableString (MarshalReadDelegate<string> marshal, out IEnumerable<string> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableInt16 (MarshalReadDelegate<System.Int16> marshal, out IEnumerable<System.Int16> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableUInt16 (MarshalReadDelegate<System.UInt16> marshal, out IEnumerable<System.UInt16> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableInt32 (MarshalReadDelegate<System.Int32> marshal, out IEnumerable<System.Int32> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableUInt32 (MarshalReadDelegate<System.UInt32> marshal, out IEnumerable<System.UInt32> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableInt64 (MarshalReadDelegate<System.Int64> marshal, out IEnumerable<System.Int64> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableUInt64 (MarshalReadDelegate<System.UInt64> marshal, out IEnumerable<System.UInt64> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableDouble (MarshalReadDelegate<double> marshal, out IEnumerable<double> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableObjectPath (MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal, out IEnumerable<Udbus.Types.UdbusObjectPath> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableVariant (MarshalReadDelegate<Udbus.Containers.dbus_union> marshal, out IEnumerable<Udbus.Containers.dbus_union> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableArray<T> (MarshalReadDelegate<IEnumerable<T>> marshal, out IEnumerable<IEnumerable<T>> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableDictionary<TKey, TValue> (MarshalReadDelegate<IDictionary<TKey, TValue>> marshal, out IEnumerable<IDictionary<TKey, TValue>> ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        #endregion // Explicit MarshalEnumerable MarshalReadDelegate overloads with IEnumerable<T> param

        #region Explicit MarshalEnumerable MarshalReadDelegate overloads with T[] param

        public int MarshalEnumerableBoolean (MarshalReadDelegate<bool> marshal, out bool[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableByte (MarshalReadDelegate<byte> marshal, out byte[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableString (MarshalReadDelegate<string> marshal, out string[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableInt16 (MarshalReadDelegate<System.Int16> marshal, out System.Int16[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableUInt16 (MarshalReadDelegate<System.UInt16> marshal, out System.UInt16[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableInt32 (MarshalReadDelegate<System.Int32> marshal, out System.Int32[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableUInt32 (MarshalReadDelegate<System.UInt32> marshal, out System.UInt32[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableInt64 (MarshalReadDelegate<System.Int64> marshal, out System.Int64[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableUInt64 (MarshalReadDelegate<System.UInt64> marshal, out System.UInt64[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableDouble (MarshalReadDelegate<double> marshal, out double[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableObjectPath (MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal, out Udbus.Types.UdbusObjectPath[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableVariant (MarshalReadDelegate<Udbus.Containers.dbus_union> marshal, out Udbus.Containers.dbus_union[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableArray<T> (MarshalReadDelegate<T[]> marshal, out T[][] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        public int MarshalEnumerableDictionary<TKey, TValue> (MarshalReadDelegate<IDictionary<TKey, TValue>> marshal, out IDictionary<TKey, TValue>[] ts)
        {
            return MarshalEnumerable(marshal, out ts);
        }

        #endregion // Explicit MarshalEnumerable MarshalReadDelegate overloads with T[] param

        #region EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        static public MarshalReadDelegate<IEnumerable<bool>> EnumerableMarshallerBoolean(MarshalReadDelegate<bool> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<byte>> EnumerableMarshallerByte(MarshalReadDelegate<byte> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<string>> EnumerableMarshallerString(MarshalReadDelegate<string> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<System.Int16>> EnumerableMarshallerInt16(MarshalReadDelegate<System.Int16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<System.UInt16>> EnumerableMarshallerUInt16(MarshalReadDelegate<System.UInt16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<System.Int32>> EnumerableMarshallerInt32(MarshalReadDelegate<System.Int32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<System.UInt32>> EnumerableMarshallerUInt32(MarshalReadDelegate<System.UInt32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<System.Int64>> EnumerableMarshallerInt64(MarshalReadDelegate<System.Int64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<System.UInt64>> EnumerableMarshallerUInt64(MarshalReadDelegate<System.UInt64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<double>> EnumerableMarshallerDouble(MarshalReadDelegate<double> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<Udbus.Types.UdbusObjectPath>> EnumerableMarshallerObjectPath(MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<Udbus.Containers.dbus_union>> EnumerableMarshallerVariant(MarshalReadDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshallerArray<T>(MarshalReadDelegate<IEnumerable<T>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalReadDelegate<IEnumerable<IDictionary<TKey, TValue>>> EnumerableMarshallerDictionary<TKey, TValue>(MarshalReadDelegate<IDictionary<TKey, TValue>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        #region MarshalEnumerable MarshalResultDelegate explicit overloads

        static public MarshalReadDelegate<bool[]> ArrayMarshallerBoolean(MarshalReadDelegate<bool> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<byte[]> ArrayMarshallerByte(MarshalReadDelegate<byte> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<string[]> ArrayMarshallerString(MarshalReadDelegate<string> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<System.Int16[]> ArrayMarshallerInt16(MarshalReadDelegate<System.Int16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<System.UInt16[]> ArrayMarshallerUInt16(MarshalReadDelegate<System.UInt16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<System.Int32[]> ArrayMarshallerInt32(MarshalReadDelegate<System.Int32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<System.UInt32[]> ArrayMarshallerUInt32(MarshalReadDelegate<System.UInt32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<System.Int64[]> ArrayMarshallerInt64(MarshalReadDelegate<System.Int64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<System.UInt64[]> ArrayMarshallerUInt64(MarshalReadDelegate<System.UInt64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<double[]> ArrayMarshallerDouble(MarshalReadDelegate<double> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<Udbus.Types.UdbusObjectPath[]> ArrayMarshallerObjectPath(MarshalReadDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<Udbus.Containers.dbus_union[]> ArrayMarshallerVariant(MarshalReadDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<T[][]> ArrayMarshallerArray<T>(MarshalReadDelegate<T[]> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalReadDelegate<IDictionary<TKey, TValue>[]> ArrayMarshallerDictionary<TKey, TValue>(MarshalReadDelegate<IDictionary<TKey, TValue>> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate explicit overloads
        #endregion // Explicit no result marshalling functions
        #endregion // No result marshalling functions
        #region Object marshalling functions
        #region Read as object

        public int ReadBooleanObject(out object val)
        {
            bool realValue;
            int result = this.ReadBoolean(out realValue);
            val = realValue;
            return result;
        }

        public object ReadBooleanObject()
        {
            object val;
            this.ReadBooleanObject(out val);
            return val;
        }

        public object ReadBooleanObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadBooleanObject(out val);
            return val;
        }

        public int ReadByteObject(out object val)
        {
            byte realValue;
            int result = this.ReadByte(out realValue);
            val = realValue;
            return result;
        }

        public object ReadByteObject()
        {
            object val;
            this.ReadByteObject(out val);
            return val;
        }

        public object ReadByteObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadByteObject(out val);
            return val;
        }

        public int ReadStringObject(out object val)
        {
            string realValue;
            int result = this.ReadString(out realValue);
            val = realValue;
            return result;
        }

        public object ReadStringObject()
        {
            object val;
            this.ReadStringObject(out val);
            return val;
        }

        public object ReadStringObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadStringObject(out val);
            return val;
        }

        public int ReadInt16Object(out object val)
        {
            System.Int16 realValue;
            int result = this.ReadInt16(out realValue);
            val = realValue;
            return result;
        }

        public object ReadInt16Object()
        {
            object val;
            this.ReadInt16Object(out val);
            return val;
        }

        public object ReadInt16ObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadInt16Object(out val);
            return val;
        }

        public int ReadUInt16Object(out object val)
        {
            System.UInt16 realValue;
            int result = this.ReadUInt16(out realValue);
            val = realValue;
            return result;
        }

        public object ReadUInt16Object()
        {
            object val;
            this.ReadUInt16Object(out val);
            return val;
        }

        public object ReadUInt16ObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadUInt16Object(out val);
            return val;
        }

        public int ReadInt32Object(out object val)
        {
            System.Int32 realValue;
            int result = this.ReadInt32(out realValue);
            val = realValue;
            return result;
        }

        public object ReadInt32Object()
        {
            object val;
            this.ReadInt32Object(out val);
            return val;
        }

        public object ReadInt32ObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadInt32Object(out val);
            return val;
        }

        public int ReadUInt32Object(out object val)
        {
            System.UInt32 realValue;
            int result = this.ReadUInt32(out realValue);
            val = realValue;
            return result;
        }

        public object ReadUInt32Object()
        {
            object val;
            this.ReadUInt32Object(out val);
            return val;
        }

        public object ReadUInt32ObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadUInt32Object(out val);
            return val;
        }

        public int ReadInt64Object(out object val)
        {
            System.Int64 realValue;
            int result = this.ReadInt64(out realValue);
            val = realValue;
            return result;
        }

        public object ReadInt64Object()
        {
            object val;
            this.ReadInt64Object(out val);
            return val;
        }

        public object ReadInt64ObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadInt64Object(out val);
            return val;
        }

        public int ReadUInt64Object(out object val)
        {
            System.UInt64 realValue;
            int result = this.ReadUInt64(out realValue);
            val = realValue;
            return result;
        }

        public object ReadUInt64Object()
        {
            object val;
            this.ReadUInt64Object(out val);
            return val;
        }

        public object ReadUInt64ObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadUInt64Object(out val);
            return val;
        }

        public int ReadDoubleObject(out object val)
        {
            double realValue;
            int result = this.ReadDouble(out realValue);
            val = realValue;
            return result;
        }

        public object ReadDoubleObject()
        {
            object val;
            this.ReadDoubleObject(out val);
            return val;
        }

        public object ReadDoubleObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadDoubleObject(out val);
            return val;
        }

        public int ReadObjectPathObject(out object val)
        {
            Udbus.Types.UdbusObjectPath realValue;
            int result = this.ReadObjectPath(out realValue);
            val = realValue;
            return result;
        }

        public object ReadObjectPathObject()
        {
            object val;
            this.ReadObjectPathObject(out val);
            return val;
        }

        public object ReadObjectPathObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadObjectPathObject(out val);
            return val;
        }

        public int ReadVariantObject(out object val)
        {
            Udbus.Containers.dbus_union realValue;
            int result = this.ReadVariant(out realValue);
            val = realValue;
            return result;
        }

        public object ReadVariantObject()
        {
            object val;
            this.ReadVariantObject(out val);
            return val;
        }

        public object ReadVariantObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadVariantObject(out val);
            return val;
        }

        public int ReadSignatureObject(out object val)
        {
            Udbus.Types.dbus_sig realValue;
            int result = this.ReadSignature(out realValue);
            val = realValue;
            return result;
        }

        public object ReadSignatureObject()
        {
            object val;
            this.ReadSignatureObject(out val);
            return val;
        }

        public object ReadSignatureObjectValue(out int result)
        {
            object val;
            result = this.result = this.ReadSignatureObject(out val);
            return val;
        }

        #endregion // Read as object

        #region Static read as object


        public static int ReadBooleanObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadBooleanObject(out value);
        }

        public static object ReadBooleanObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadBooleanObjectValue(out result);
        }


        public static int ReadByteObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadByteObject(out value);
        }

        public static object ReadByteObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadByteObjectValue(out result);
        }


        public static int ReadStringObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadStringObject(out value);
        }

        public static object ReadStringObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadStringObjectValue(out result);
        }


        public static int ReadInt16Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadInt16Object(out value);
        }

        public static object ReadInt16ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt16ObjectValue(out result);
        }


        public static int ReadUInt16Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadUInt16Object(out value);
        }

        public static object ReadUInt16ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadUInt16ObjectValue(out result);
        }


        public static int ReadInt32Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadInt32Object(out value);
        }

        public static object ReadInt32ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt32ObjectValue(out result);
        }


        public static int ReadUInt32Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadUInt32Object(out value);
        }

        public static object ReadUInt32ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadUInt32ObjectValue(out result);
        }


        public static int ReadInt64Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadInt64Object(out value);
        }

        public static object ReadInt64ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt64ObjectValue(out result);
        }


        public static int ReadUInt64Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadUInt64Object(out value);
        }

        public static object ReadUInt64ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadUInt64ObjectValue(out result);
        }


        public static int ReadDoubleObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadDoubleObject(out value);
        }

        public static object ReadDoubleObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadDoubleObjectValue(out result);
        }


        public static int ReadObjectPathObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadObjectPathObject(out value);
        }

        public static object ReadObjectPathObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadObjectPathObjectValue(out result);
        }


        public static int ReadVariantObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadVariantObject(out value);
        }

        public static object ReadVariantObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantObjectValue(out result);
        }


        public static int ReadSignatureObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadSignatureObject(out value);
        }

        public static object ReadSignatureObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadSignatureObjectValue(out result);
        }

        #endregion // Static read as object
        #endregion // Object marshalling functions

    } // Ends class UdbusMessageReader
} // Ends Udbus.Serialization
#endif // _MAGIC
