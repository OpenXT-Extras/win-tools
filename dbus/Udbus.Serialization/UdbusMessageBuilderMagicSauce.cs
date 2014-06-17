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

    public partial class UdbusMessageBuilder
    {
        #region No result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        static public MarshalResultDelegate<IEnumerable<bool>> EnumerableMarshaller (MarshalResultDelegate<bool> marshal)
        {
            return new EnumerableResultMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<byte>> EnumerableMarshaller (MarshalResultDelegate<byte> marshal)
        {
            return new EnumerableResultMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<string>> EnumerableMarshaller (MarshalResultDelegate<string> marshal)
        {
            return new EnumerableResultMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.Int16>> EnumerableMarshaller (MarshalResultDelegate<System.Int16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt16>> EnumerableMarshaller (MarshalResultDelegate<System.UInt16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.Int32>> EnumerableMarshaller (MarshalResultDelegate<System.Int32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt32>> EnumerableMarshaller (MarshalResultDelegate<System.UInt32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.Int64>> EnumerableMarshaller (MarshalResultDelegate<System.Int64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt64>> EnumerableMarshaller (MarshalResultDelegate<System.UInt64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<double>> EnumerableMarshaller (MarshalResultDelegate<double> marshal)
        {
            return new EnumerableResultMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<Udbus.Types.UdbusObjectPath>> EnumerableMarshaller (MarshalResultDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return new EnumerableResultMarshallerImpl<Udbus.Types.UdbusObjectPath>(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<Udbus.Containers.dbus_union>> EnumerableMarshaller (MarshalResultDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return new EnumerableResultMarshallerImpl<Udbus.Containers.dbus_union>(marshal, Udbus.Types.dbus_type.DBUS_VARIANT).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshaller<T> (MarshalResultDelegate<IEnumerable<T>> marshal)
        {
            return new EnumerableResultMarshallerImpl<IEnumerable<T>>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshaller<TKey, TValue> (MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableResultMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        #region MarshalEnumerable MarshalResultDelegate overloads

        public void MarshalEnumerable (IEnumerable<bool> ts, MarshalResultDelegate<bool> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BOOLEAN, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<byte> ts, MarshalResultDelegate<byte> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BYTE, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<string> ts, MarshalResultDelegate<string> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_STRING, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<System.Int16> ts, MarshalResultDelegate<System.Int16> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT16, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<System.UInt16> ts, MarshalResultDelegate<System.UInt16> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT16, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<System.Int32> ts, MarshalResultDelegate<System.Int32> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT32, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<System.UInt32> ts, MarshalResultDelegate<System.UInt32> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT32, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<System.Int64> ts, MarshalResultDelegate<System.Int64> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT64, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<System.UInt64> ts, MarshalResultDelegate<System.UInt64> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT64, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<double> ts, MarshalResultDelegate<double> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DOUBLE, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<Udbus.Types.UdbusObjectPath> ts, MarshalResultDelegate<Udbus.Types.UdbusObjectPath> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_OBJECTPATH, marshal, out result);
        }

        public void MarshalEnumerable (IEnumerable<Udbus.Containers.dbus_union> ts, MarshalResultDelegate<Udbus.Containers.dbus_union> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_VARIANT, marshal, out result);
        }

        public void MarshalEnumerable<T> (IEnumerable<IEnumerable<T>> ts, MarshalResultDelegate<IEnumerable<T>> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_ARRAY, marshal, out result);
        }

        public void MarshalEnumerable<TKey, TValue> (IEnumerable<Dictionary<TKey, TValue>> ts, MarshalResultDelegate<Dictionary<TKey, TValue>> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, marshal, out result);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate overloads

        #region ArrayMarshaller MarshalResultDelegate overloads

        static public MarshalResultDelegate<bool[]> ArrayMarshaller (MarshalResultDelegate<bool> marshal)
        {
            return new EnumerableResultMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalResultDelegate<byte[]> ArrayMarshaller (MarshalResultDelegate<byte> marshal)
        {
            return new EnumerableResultMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalResultDelegate<string[]> ArrayMarshaller (MarshalResultDelegate<string> marshal)
        {
            return new EnumerableResultMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalResultDelegate<System.Int16[]> ArrayMarshaller (MarshalResultDelegate<System.Int16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalResultDelegate<System.UInt16[]> ArrayMarshaller (MarshalResultDelegate<System.UInt16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalResultDelegate<System.Int32[]> ArrayMarshaller (MarshalResultDelegate<System.Int32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalResultDelegate<System.UInt32[]> ArrayMarshaller (MarshalResultDelegate<System.UInt32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalResultDelegate<System.Int64[]> ArrayMarshaller (MarshalResultDelegate<System.Int64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalResultDelegate<System.UInt64[]> ArrayMarshaller (MarshalResultDelegate<System.UInt64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalResultDelegate<double[]> ArrayMarshaller (MarshalResultDelegate<double> marshal)
        {
            return new EnumerableResultMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalResultDelegate<Udbus.Types.UdbusObjectPath[]> ArrayMarshaller (MarshalResultDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return new EnumerableResultMarshallerImpl<Udbus.Types.UdbusObjectPath>(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH).Marshal;
        }

        static public MarshalResultDelegate<Udbus.Containers.dbus_union[]> ArrayMarshaller (MarshalResultDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return new EnumerableResultMarshallerImpl<Udbus.Containers.dbus_union>(marshal, Udbus.Types.dbus_type.DBUS_VARIANT).Marshal;
        }

        static public MarshalResultDelegate<T[][]> ArrayMarshaller<T> (MarshalResultDelegate<T[]> marshal)
        {
            return new EnumerableResultMarshallerImpl<T[]>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalResultDelegate<Dictionary<TKey, TValue>[]> ArrayMarshaller<TKey, TValue> (MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableResultMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // ArrayMarshaller MarshalResultDelegate overloads
        #region Explicit no result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        static public MarshalResultDelegate<IEnumerable<bool>> EnumerableMarshallerBoolean (MarshalResultDelegate<bool> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<byte>> EnumerableMarshallerByte (MarshalResultDelegate<byte> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<string>> EnumerableMarshallerString (MarshalResultDelegate<string> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.Int16>> EnumerableMarshallerInt16 (MarshalResultDelegate<System.Int16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt16>> EnumerableMarshallerUInt16 (MarshalResultDelegate<System.UInt16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.Int32>> EnumerableMarshallerInt32 (MarshalResultDelegate<System.Int32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt32>> EnumerableMarshallerUInt32 (MarshalResultDelegate<System.UInt32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.Int64>> EnumerableMarshallerInt64 (MarshalResultDelegate<System.Int64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt64>> EnumerableMarshallerUInt64 (MarshalResultDelegate<System.UInt64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<double>> EnumerableMarshallerDouble (MarshalResultDelegate<double> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<Udbus.Types.UdbusObjectPath>> EnumerableMarshallerObjectPath (MarshalResultDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<Udbus.Containers.dbus_union>> EnumerableMarshallerVariant (MarshalResultDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshallerArray<T> (MarshalResultDelegate<IEnumerable<T>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshallerDictionary<TKey, TValue> (MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        #region MarshalEnumerable MarshalResultDelegate explicit overloads

        public void MarshalEnumerableBoolean (IEnumerable<bool> ts, MarshalResultDelegate<bool> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableByte (IEnumerable<byte> ts, MarshalResultDelegate<byte> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableString (IEnumerable<string> ts, MarshalResultDelegate<string> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableInt16 (IEnumerable<System.Int16> ts, MarshalResultDelegate<System.Int16> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableUInt16 (IEnumerable<System.UInt16> ts, MarshalResultDelegate<System.UInt16> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableInt32 (IEnumerable<System.Int32> ts, MarshalResultDelegate<System.Int32> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableUInt32 (IEnumerable<System.UInt32> ts, MarshalResultDelegate<System.UInt32> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableInt64 (IEnumerable<System.Int64> ts, MarshalResultDelegate<System.Int64> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableUInt64 (IEnumerable<System.UInt64> ts, MarshalResultDelegate<System.UInt64> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableDouble (IEnumerable<double> ts, MarshalResultDelegate<double> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableObjectPath (IEnumerable<Udbus.Types.UdbusObjectPath> ts, MarshalResultDelegate<Udbus.Types.UdbusObjectPath> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableVariant (IEnumerable<Udbus.Containers.dbus_union> ts, MarshalResultDelegate<Udbus.Containers.dbus_union> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableArray<T> (IEnumerable<IEnumerable<T>> ts, MarshalResultDelegate<IEnumerable<T>> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableDictionary<TKey, TValue> (IEnumerable<Dictionary<TKey, TValue>> ts, MarshalResultDelegate<Dictionary<TKey, TValue>> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate explicit overloads

        #region MarshalEnumerable MarshalResultDelegate explicit overloads

        static public MarshalResultDelegate<bool[]> ArrayMarshallerBoolean (MarshalResultDelegate<bool> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<byte[]> ArrayMarshallerByte (MarshalResultDelegate<byte> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<string[]> ArrayMarshallerString (MarshalResultDelegate<string> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.Int16[]> ArrayMarshallerInt16 (MarshalResultDelegate<System.Int16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.UInt16[]> ArrayMarshallerUInt16 (MarshalResultDelegate<System.UInt16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.Int32[]> ArrayMarshallerInt32 (MarshalResultDelegate<System.Int32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.UInt32[]> ArrayMarshallerUInt32 (MarshalResultDelegate<System.UInt32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.Int64[]> ArrayMarshallerInt64 (MarshalResultDelegate<System.Int64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.UInt64[]> ArrayMarshallerUInt64 (MarshalResultDelegate<System.UInt64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<double[]> ArrayMarshallerDouble (MarshalResultDelegate<double> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<Udbus.Types.UdbusObjectPath[]> ArrayMarshallerObjectPath (MarshalResultDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<Udbus.Containers.dbus_union[]> ArrayMarshallerVariant (MarshalResultDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<T[][]> ArrayMarshallerArray<T> (MarshalResultDelegate<T[]> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<Dictionary<TKey, TValue>[]> ArrayMarshallerDictionary<TKey, TValue> (MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate explicit overloads
        #endregion // Explicit no result marshalling functions
        #endregion // No result marshalling functions
        #region Return result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        static public MarshalDelegate<IEnumerable<bool>> EnumerableMarshaller (MarshalDelegate<bool> marshal)
        {
            return new EnumerableMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalDelegate<IEnumerable<byte>> EnumerableMarshaller (MarshalDelegate<byte> marshal)
        {
            return new EnumerableMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalDelegate<IEnumerable<string>> EnumerableMarshaller (MarshalDelegate<string> marshal)
        {
            return new EnumerableMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.Int16>> EnumerableMarshaller (MarshalDelegate<System.Int16> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.UInt16>> EnumerableMarshaller (MarshalDelegate<System.UInt16> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.Int32>> EnumerableMarshaller (MarshalDelegate<System.Int32> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.UInt32>> EnumerableMarshaller (MarshalDelegate<System.UInt32> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.Int64>> EnumerableMarshaller (MarshalDelegate<System.Int64> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.UInt64>> EnumerableMarshaller (MarshalDelegate<System.UInt64> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalDelegate<IEnumerable<double>> EnumerableMarshaller (MarshalDelegate<double> marshal)
        {
            return new EnumerableMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalDelegate<IEnumerable<Udbus.Types.UdbusObjectPath>> EnumerableMarshaller (MarshalDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Types.UdbusObjectPath>(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH).Marshal;
        }

        static public MarshalDelegate<IEnumerable<Udbus.Containers.dbus_union>> EnumerableMarshaller (MarshalDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Containers.dbus_union>(marshal, Udbus.Types.dbus_type.DBUS_VARIANT).Marshal;
        }

        static public MarshalDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshaller<T> (MarshalDelegate<IEnumerable<T>> marshal)
        {
            return new EnumerableMarshallerImpl<IEnumerable<T>>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshaller<TKey, TValue> (MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        #region MarshalEnumerable MarshalDelegate overloads

        public int MarshalEnumerable (IEnumerable<bool> ts, MarshalDelegate<bool> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BOOLEAN, marshal);
        }

        public int MarshalEnumerable (IEnumerable<byte> ts, MarshalDelegate<byte> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BYTE, marshal);
        }

        public int MarshalEnumerable (IEnumerable<string> ts, MarshalDelegate<string> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_STRING, marshal);
        }

        public int MarshalEnumerable (IEnumerable<System.Int16> ts, MarshalDelegate<System.Int16> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT16, marshal);
        }

        public int MarshalEnumerable (IEnumerable<System.UInt16> ts, MarshalDelegate<System.UInt16> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT16, marshal);
        }

        public int MarshalEnumerable (IEnumerable<System.Int32> ts, MarshalDelegate<System.Int32> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT32, marshal);
        }

        public int MarshalEnumerable (IEnumerable<System.UInt32> ts, MarshalDelegate<System.UInt32> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT32, marshal);
        }

        public int MarshalEnumerable (IEnumerable<System.Int64> ts, MarshalDelegate<System.Int64> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT64, marshal);
        }

        public int MarshalEnumerable (IEnumerable<System.UInt64> ts, MarshalDelegate<System.UInt64> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT64, marshal);
        }

        public int MarshalEnumerable (IEnumerable<double> ts, MarshalDelegate<double> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DOUBLE, marshal);
        }

        public int MarshalEnumerable (IEnumerable<Udbus.Types.UdbusObjectPath> ts, MarshalDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_OBJECTPATH, marshal);
        }

        public int MarshalEnumerable (IEnumerable<Udbus.Containers.dbus_union> ts, MarshalDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_VARIANT, marshal);
        }

        public int MarshalEnumerable<T> (IEnumerable<IEnumerable<T>> ts, MarshalDelegate<IEnumerable<T>> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_ARRAY, marshal);
        }

        public int MarshalEnumerable<TKey, TValue> (IEnumerable<Dictionary<TKey, TValue>> ts, MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, marshal);
        }

        #endregion // MarshalEnumerable MarshalDelegate overloads

        #region ArrayMarshaller MarshalDelegate overloads

        static public MarshalDelegate<bool[]> ArrayMarshaller (MarshalDelegate<bool> marshal)
        {
            return new EnumerableMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalDelegate<byte[]> ArrayMarshaller (MarshalDelegate<byte> marshal)
        {
            return new EnumerableMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalDelegate<string[]> ArrayMarshaller (MarshalDelegate<string> marshal)
        {
            return new EnumerableMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalDelegate<System.Int16[]> ArrayMarshaller (MarshalDelegate<System.Int16> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalDelegate<System.UInt16[]> ArrayMarshaller (MarshalDelegate<System.UInt16> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalDelegate<System.Int32[]> ArrayMarshaller (MarshalDelegate<System.Int32> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalDelegate<System.UInt32[]> ArrayMarshaller (MarshalDelegate<System.UInt32> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalDelegate<System.Int64[]> ArrayMarshaller (MarshalDelegate<System.Int64> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalDelegate<System.UInt64[]> ArrayMarshaller (MarshalDelegate<System.UInt64> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalDelegate<double[]> ArrayMarshaller (MarshalDelegate<double> marshal)
        {
            return new EnumerableMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalDelegate<Udbus.Types.UdbusObjectPath[]> ArrayMarshaller (MarshalDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Types.UdbusObjectPath>(marshal, Udbus.Types.dbus_type.DBUS_OBJECTPATH).Marshal;
        }

        static public MarshalDelegate<Udbus.Containers.dbus_union[]> ArrayMarshaller (MarshalDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return new EnumerableMarshallerImpl<Udbus.Containers.dbus_union>(marshal, Udbus.Types.dbus_type.DBUS_VARIANT).Marshal;
        }

        static public MarshalDelegate<T[][]> ArrayMarshaller<T> (MarshalDelegate<T[]> marshal)
        {
            return new EnumerableMarshallerImpl<T[]>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalDelegate<Dictionary<TKey, TValue>[]> ArrayMarshaller<TKey, TValue> (MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // ArrayMarshaller MarshalDelegate overloads
        #region Explicit return result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        static public MarshalDelegate<IEnumerable<bool>> EnumerableMarshallerBoolean (MarshalDelegate<bool> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<byte>> EnumerableMarshallerByte (MarshalDelegate<byte> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<string>> EnumerableMarshallerString (MarshalDelegate<string> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.Int16>> EnumerableMarshallerInt16 (MarshalDelegate<System.Int16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.UInt16>> EnumerableMarshallerUInt16 (MarshalDelegate<System.UInt16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.Int32>> EnumerableMarshallerInt32 (MarshalDelegate<System.Int32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.UInt32>> EnumerableMarshallerUInt32 (MarshalDelegate<System.UInt32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.Int64>> EnumerableMarshallerInt64 (MarshalDelegate<System.Int64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.UInt64>> EnumerableMarshallerUInt64 (MarshalDelegate<System.UInt64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<double>> EnumerableMarshallerDouble (MarshalDelegate<double> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<Udbus.Types.UdbusObjectPath>> EnumerableMarshallerObjectPath (MarshalDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<Udbus.Containers.dbus_union>> EnumerableMarshallerVariant (MarshalDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshallerArray<T> (MarshalDelegate<IEnumerable<T>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshallerDictionary<TKey, TValue> (MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        #region MarshalEnumerable MarshalDelegate explicit overloads

        public int MarshalEnumerableBoolean (IEnumerable<bool> ts, MarshalDelegate<bool> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableByte (IEnumerable<byte> ts, MarshalDelegate<byte> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableString (IEnumerable<string> ts, MarshalDelegate<string> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableInt16 (IEnumerable<System.Int16> ts, MarshalDelegate<System.Int16> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableUInt16 (IEnumerable<System.UInt16> ts, MarshalDelegate<System.UInt16> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableInt32 (IEnumerable<System.Int32> ts, MarshalDelegate<System.Int32> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableUInt32 (IEnumerable<System.UInt32> ts, MarshalDelegate<System.UInt32> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableInt64 (IEnumerable<System.Int64> ts, MarshalDelegate<System.Int64> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableUInt64 (IEnumerable<System.UInt64> ts, MarshalDelegate<System.UInt64> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableDouble (IEnumerable<double> ts, MarshalDelegate<double> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableObjectPath (IEnumerable<Udbus.Types.UdbusObjectPath> ts, MarshalDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableVariant (IEnumerable<Udbus.Containers.dbus_union> ts, MarshalDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableArray<T> (IEnumerable<IEnumerable<T>> ts, MarshalDelegate<IEnumerable<T>> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableDictionary<TKey, TValue> (IEnumerable<Dictionary<TKey, TValue>> ts, MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        #endregion // MarshalEnumerable MarshalDelegate explicit overloads

        #region MarshalEnumerable MarshalDelegate explicit overloads

        static public MarshalDelegate<bool[]> ArrayMarshallerBoolean (MarshalDelegate<bool> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<byte[]> ArrayMarshallerByte (MarshalDelegate<byte> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<string[]> ArrayMarshallerString (MarshalDelegate<string> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.Int16[]> ArrayMarshallerInt16 (MarshalDelegate<System.Int16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.UInt16[]> ArrayMarshallerUInt16 (MarshalDelegate<System.UInt16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.Int32[]> ArrayMarshallerInt32 (MarshalDelegate<System.Int32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.UInt32[]> ArrayMarshallerUInt32 (MarshalDelegate<System.UInt32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.Int64[]> ArrayMarshallerInt64 (MarshalDelegate<System.Int64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.UInt64[]> ArrayMarshallerUInt64 (MarshalDelegate<System.UInt64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<double[]> ArrayMarshallerDouble (MarshalDelegate<double> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<Udbus.Types.UdbusObjectPath[]> ArrayMarshallerObjectPath (MarshalDelegate<Udbus.Types.UdbusObjectPath> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<Udbus.Containers.dbus_union[]> ArrayMarshallerVariant (MarshalDelegate<Udbus.Containers.dbus_union> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<T[][]> ArrayMarshallerArray<T> (MarshalDelegate<T[]> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<Dictionary<TKey, TValue>[]> ArrayMarshallerDictionary<TKey, TValue> (MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        #endregion // MarshalEnumerable MarshalDelegate explicit overloads
        #endregion // Explicit return result marshalling functions
        #endregion // Return result marshalling functions
        #region Object marshalling functions
        #region Write as object

        public UdbusMessageBuilder BodyAdd_BooleanObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is bool)
            {
                bool castVal = (bool)val;
                result = this.BodyAdd_Boolean(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a bool");
            }
            return result;
        }
        public int BodyAdd_BooleanObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is bool)
            {
                bool castVal = (bool)val;
                result = this.BodyAdd_Boolean(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a bool");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_ByteObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is byte)
            {
                byte castVal = (byte)val;
                result = this.BodyAdd_Byte(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a byte");
            }
            return result;
        }
        public int BodyAdd_ByteObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is byte)
            {
                byte castVal = (byte)val;
                result = this.BodyAdd_Byte(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a byte");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_StringObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is string)
            {
                string castVal = (string)val;
                result = this.BodyAdd_String(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a string");
            }
            return result;
        }
        public int BodyAdd_StringObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is string)
            {
                string castVal = (string)val;
                result = this.BodyAdd_String(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a string");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_Int16Object(object val)
        {
            UdbusMessageBuilder result;
            if (val is System.Int16)
            {
                System.Int16 castVal = (System.Int16)val;
                result = this.BodyAdd_Int16(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a System.Int16");
            }
            return result;
        }
        public int BodyAdd_Int16Object(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is System.Int16)
            {
                System.Int16 castVal = (System.Int16)val;
                result = this.BodyAdd_Int16(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.Int16");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_UInt16Object(object val)
        {
            UdbusMessageBuilder result;
            if (val is System.UInt16)
            {
                System.UInt16 castVal = (System.UInt16)val;
                result = this.BodyAdd_UInt16(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a System.UInt16");
            }
            return result;
        }
        public int BodyAdd_UInt16Object(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is System.UInt16)
            {
                System.UInt16 castVal = (System.UInt16)val;
                result = this.BodyAdd_UInt16(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.UInt16");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_Int32Object(object val)
        {
            UdbusMessageBuilder result;
            if (val is System.Int32)
            {
                System.Int32 castVal = (System.Int32)val;
                result = this.BodyAdd_Int32(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a System.Int32");
            }
            return result;
        }
        public int BodyAdd_Int32Object(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is System.Int32)
            {
                System.Int32 castVal = (System.Int32)val;
                result = this.BodyAdd_Int32(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.Int32");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_UInt32Object(object val)
        {
            UdbusMessageBuilder result;
            if (val is System.UInt32)
            {
                System.UInt32 castVal = (System.UInt32)val;
                result = this.BodyAdd_UInt32(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a System.UInt32");
            }
            return result;
        }
        public int BodyAdd_UInt32Object(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is System.UInt32)
            {
                System.UInt32 castVal = (System.UInt32)val;
                result = this.BodyAdd_UInt32(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.UInt32");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_Int64Object(object val)
        {
            UdbusMessageBuilder result;
            if (val is System.Int64)
            {
                System.Int64 castVal = (System.Int64)val;
                result = this.BodyAdd_Int64(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a System.Int64");
            }
            return result;
        }
        public int BodyAdd_Int64Object(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is System.Int64)
            {
                System.Int64 castVal = (System.Int64)val;
                result = this.BodyAdd_Int64(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.Int64");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_UInt64Object(object val)
        {
            UdbusMessageBuilder result;
            if (val is System.UInt64)
            {
                System.UInt64 castVal = (System.UInt64)val;
                result = this.BodyAdd_UInt64(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a System.UInt64");
            }
            return result;
        }
        public int BodyAdd_UInt64Object(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is System.UInt64)
            {
                System.UInt64 castVal = (System.UInt64)val;
                result = this.BodyAdd_UInt64(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.UInt64");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_DoubleObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is double)
            {
                double castVal = (double)val;
                result = this.BodyAdd_Double(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a double");
            }
            return result;
        }
        public int BodyAdd_DoubleObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is double)
            {
                double castVal = (double)val;
                result = this.BodyAdd_Double(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a double");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_ObjectPathObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is Udbus.Types.UdbusObjectPath)
            {
                Udbus.Types.UdbusObjectPath castVal = (Udbus.Types.UdbusObjectPath)val;
                result = this.BodyAdd_ObjectPath(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a Udbus.Types.UdbusObjectPath");
            }
            return result;
        }
        public int BodyAdd_ObjectPathObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is Udbus.Types.UdbusObjectPath)
            {
                Udbus.Types.UdbusObjectPath castVal = (Udbus.Types.UdbusObjectPath)val;
                result = this.BodyAdd_ObjectPath(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a Udbus.Types.UdbusObjectPath");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_VariantObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is Udbus.Containers.dbus_union)
            {
                Udbus.Containers.dbus_union castVal = (Udbus.Containers.dbus_union)val;
                result = this.BodyAdd_Variant(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a Udbus.Containers.dbus_union");
            }
            return result;
        }
        public int BodyAdd_VariantObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is Udbus.Containers.dbus_union)
            {
                Udbus.Containers.dbus_union castVal = (Udbus.Containers.dbus_union)val;
                result = this.BodyAdd_Variant(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a Udbus.Containers.dbus_union");
            }
            return result;
        }

        public UdbusMessageBuilder BodyAdd_SignatureObject(object val)
        {
            UdbusMessageBuilder result;
            if (val is Udbus.Types.dbus_sig)
            {
                Udbus.Types.dbus_sig castVal = (Udbus.Types.dbus_sig)val;
                result = this.BodyAdd_Signature(castVal);
            }
            else
            {
                result = null;
                throw new System.InvalidCastException("object is not a Udbus.Types.dbus_sig");
            }
            return result;
        }
        public int BodyAdd_SignatureObject(NMessageHandle.UdbusMessageHandle msg, object val)
        {

            int result;

            if (val is Udbus.Types.dbus_sig)
            {
                Udbus.Types.dbus_sig castVal = (Udbus.Types.dbus_sig)val;
                result = this.BodyAdd_Signature(msg, castVal);
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a Udbus.Types.dbus_sig");
            }
            return result;
        }

        #endregion // Write as object

        #region Static write as object


        public static int BodyAdd_BooleanObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is bool)
            {
                bool castVal = (bool)val;
                result = builder.BodyAdd_Boolean(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a bool");
            }
            return result;
        }


        public static int BodyAdd_ByteObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is byte)
            {
                byte castVal = (byte)val;
                result = builder.BodyAdd_Byte(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a byte");
            }
            return result;
        }


        public static int BodyAdd_StringObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is string)
            {
                string castVal = (string)val;
                result = builder.BodyAdd_String(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a string");
            }
            return result;
        }


        public static int BodyAdd_Int16Object(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is System.Int16)
            {
                System.Int16 castVal = (System.Int16)val;
                result = builder.BodyAdd_Int16(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.Int16");
            }
            return result;
        }


        public static int BodyAdd_UInt16Object(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is System.UInt16)
            {
                System.UInt16 castVal = (System.UInt16)val;
                result = builder.BodyAdd_UInt16(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.UInt16");
            }
            return result;
        }


        public static int BodyAdd_Int32Object(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is System.Int32)
            {
                System.Int32 castVal = (System.Int32)val;
                result = builder.BodyAdd_Int32(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.Int32");
            }
            return result;
        }


        public static int BodyAdd_UInt32Object(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is System.UInt32)
            {
                System.UInt32 castVal = (System.UInt32)val;
                result = builder.BodyAdd_UInt32(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.UInt32");
            }
            return result;
        }


        public static int BodyAdd_Int64Object(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is System.Int64)
            {
                System.Int64 castVal = (System.Int64)val;
                result = builder.BodyAdd_Int64(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.Int64");
            }
            return result;
        }


        public static int BodyAdd_UInt64Object(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is System.UInt64)
            {
                System.UInt64 castVal = (System.UInt64)val;
                result = builder.BodyAdd_UInt64(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a System.UInt64");
            }
            return result;
        }


        public static int BodyAdd_DoubleObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is double)
            {
                double castVal = (double)val;
                result = builder.BodyAdd_Double(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a double");
            }
            return result;
        }


        public static int BodyAdd_ObjectPathObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is Udbus.Types.UdbusObjectPath)
            {
                Udbus.Types.UdbusObjectPath castVal = (Udbus.Types.UdbusObjectPath)val;
                result = builder.BodyAdd_ObjectPath(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a Udbus.Types.UdbusObjectPath");
            }
            return result;
        }


        public static int BodyAdd_VariantObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is Udbus.Containers.dbus_union)
            {
                Udbus.Containers.dbus_union castVal = (Udbus.Containers.dbus_union)val;
                result = builder.BodyAdd_Variant(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a Udbus.Containers.dbus_union");
            }
            return result;
        }


        public static int BodyAdd_SignatureObject(UdbusMessageBuilder builder, object val)
        {
            int result;
            if (val is Udbus.Types.dbus_sig)
            {
                Udbus.Types.dbus_sig castVal = (Udbus.Types.dbus_sig)val;
                result = builder.BodyAdd_Signature(castVal).Result;
            }
            else
            {
                result = -1;
                throw new System.InvalidCastException("object is not a Udbus.Types.dbus_sig");
            }
            return result;
        }

        #endregion // Static write as object
        #endregion // Object marshalling functions

    } // Ends class UdbusMessageBuilder
} // Ends Udbus.Serialization
#endif // _MAGIC
