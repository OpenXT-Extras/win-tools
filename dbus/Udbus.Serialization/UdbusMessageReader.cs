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

//#define _NOMAGIC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Udbus.Serialization
{
    //internal class UdbusMessageArrayEnumerator :
    //    IE
    public partial class UdbusMessageReader :
        IDisposable
    {
        protected NMessageHandle.UdbusMessageHandle msg;
        protected int result;

        public UdbusMessageReader(NMessageHandle.UdbusMessageHandle msg)
        {
            this.msg = msg;
            this.result = 0;
        }

        /// New stuff.
#if _NOMAGIC
        public int ReadInt32Object(out object val)
        {
            Int32 realValue;
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

        public static int ReadInt32Object(UdbusMessageReader reader, out object value)
        {
            return reader.ReadInt32Object(out value);
        }

        public static object ReadInt32ObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt32ObjectValue(out result);
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

        public static int ReadStringObject(UdbusMessageReader reader, out object value)
        {
            return reader.ReadStringObject(out value);
        }

        public static object ReadStringObjectValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadStringObjectValue(out result);
        }
#endif // _NOMAGIC
        // Ends new stuff.

        /// Data reading.
        public int ReadByte(out byte val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_byte(this.msg, out val);
            return this.result;
        }

        public byte ReadByte()
        {
            byte val;
            this.ReadByte(out val);
            return val;
        }

        public byte ReadByteValue(out int result)
        {
            byte val;
            result = this.result = this.ReadByte(out val);
            return val;
        }

        public int ReadBoolean(out bool val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_boolean(this.msg, out val);
            return this.result;
        }

        public bool ReadBoolean()
        {
            bool val;
            this.ReadBoolean(out val);
            return val;
        }

        public bool ReadBooleanValue(out int result)
        {
            bool val;
            result = this.result = this.ReadBoolean(out val);
            return val;
        }

        public int ReadInt16(out Int16 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_int16(this.msg, out val);
            return this.result;
        }

        public Int16 ReadInt16()
        {
            Int16 val;
            this.ReadInt16(out val);
            return val;
        }

        public Int16 ReadInt16Value(out int result)
        {
            Int16 val;
            result = this.result = this.ReadInt16(out val);
            return val;
        }

        public int ReadUInt16(out UInt16 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_uint16(this.msg, out val);
            return this.result;
        }

        public UInt16 ReadUInt16()
        {
            UInt16 val;
            this.ReadUInt16(out val);
            return val;
        }

        public UInt16 ReadUInt16Value(out int result)
        {
            UInt16 val;
            result = this.result = this.ReadUInt16(out val);
            return val;
        }

        public int ReadInt32(out Int32 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_int32(this.msg, out val);
            return this.result;
        }

        public Int32 ReadInt32()
        {
            Int32 val;
            this.ReadInt32(out val);
            return val;
        }

        public Int32 ReadInt32Value(out int result)
        {
            Int32 val;
            result = this.result = this.ReadInt32(out val);
            return val;
        }

        public int ReadUInt32(out UInt32 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_uint32(this.msg, out val);
            return this.result;
        }

        public UInt32 ReadUInt32()
        {
            UInt32 val;
            this.ReadUInt32(out val);
            return val;
        }

        public UInt32 ReadUInt32Value(out int result)
        {
            UInt32 val;
            result = this.result = this.ReadUInt32(out val);
            return val;
        }

        public int ReadInt64(out Int64 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_int64(this.msg, out val);
            return this.result;
        }

        public Int64 ReadInt64()
        {
            Int64 val;
            this.ReadInt64(out val);
            return val;
        }

        public Int64 ReadInt64Value(out int result)
        {
            Int64 val;
            result = this.result = this.ReadInt64(out val);
            return val;
        }

        public int ReadUInt64(out UInt64 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_uint64(this.msg, out val);
            return this.result;
        }

        public UInt64 ReadUInt64()
        {
            UInt64 val;
            this.ReadUInt64(out val);
            return val;
        }

        public UInt64 ReadUInt64Value(out int result)
        {
            UInt64 val;
            result = this.result = this.ReadUInt64(out val);
            return val;
        }

        public int ReadDouble(out double val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_double(this.msg, out val);
            return this.result;
        }

        public double ReadDouble()
        {
            double val;
            this.ReadDouble(out val);
            return val;
        }

        public double ReadDoubleValue(out int result)
        {
            double val;
            result = this.result = this.ReadDouble(out val);
            return val;
        }

        public int ReadString(out string val)
        {
            using (UdbusFunctions.UdbusMessageBodyStringHandle hval = 
                UdbusFunctions.UdbusMessageBodyStringHandle.Create(
                     UdbusFunctions.dbus_msg_body_get_string
                    ,this.msg
                    ,out this.result))
            {
                val = hval;
            }

            return this.result;
        }

        public string ReadString()
        {
            string val;
            this.ReadString(out val);
            return val;
        }

        public string ReadStringValue(out int result)
        {
            string val;
            result = this.result = this.ReadString(out val);
            return val;
        }

        public int ReadObjectPath(out Udbus.Types.UdbusObjectPath val)
        {
            string stringVal;
            this.result = this.ReadObjectPath(out stringVal);
            val = new Types.UdbusObjectPath(stringVal);
            return this.result;
        }

        public int ReadObjectPath(out string val)
        {
            using (UdbusFunctions.UdbusMessageBodyObjectPathHandle hval =
                UdbusFunctions.UdbusMessageBodyObjectPathHandle.Create(
                     UdbusFunctions.dbus_msg_body_get_object_path
                    , this.msg
                    , out this.result))
            {
                val = hval;
            }

            return this.result;
        }

        public Udbus.Types.UdbusObjectPath ReadObjectPath()
        {
            Udbus.Types.UdbusObjectPath val;
            this.ReadObjectPath(out val);
            return val;
        }

        public string ReadObjectPathString()
        {
            string val;
            this.ReadObjectPath(out val);
            return val;
        }

        public Udbus.Types.UdbusObjectPath ReadObjectPathValue(out int result)
        {
            Udbus.Types.UdbusObjectPath val;
            result = this.result = this.ReadObjectPath(out val);
            return val;
        }

        public string ReadObjectPathStringValue(out int result)
        {
            string val;
            result = this.result = this.ReadObjectPath(out val);
            return val;
        }

        public int ReadSignatureRef(ref Udbus.Types.dbus_sig val)
        {
            this.result = UdbusFunctions.dbus_msg_body_get_variant(this.msg, ref val);
            return this.result;
        }

        public int ReadSignature(out Udbus.Types.dbus_sig val)
        {
            val = Udbus.Types.dbus_sig.Initialiser;
            int result = this.ReadSignatureRef(ref val);
            return result;
        }

        public Udbus.Types.dbus_sig ReadSignature()
        {
            Udbus.Types.dbus_sig val = Udbus.Types.dbus_sig.Initialiser;
            this.ReadSignatureRef(ref val);
            return val;
        }

        public Udbus.Types.dbus_sig ReadSignatureValue(out int result)
        {
            Udbus.Types.dbus_sig val = Udbus.Types.dbus_sig.Initialiser;
            result = this.result = this.ReadSignatureRef(ref val);
            return val;
        }

        public IEnumerable<UdbusMessageReader> ArrayReader(Udbus.Types.dbus_type element)
        {
            dbus_array_reader array_reader = default(dbus_array_reader);
            int result = UdbusFunctions.dbus_msg_body_get_array(this.msg, element, ref array_reader);

            if (result != 0) // If failed to get array
            {
                this.result = result;
                yield break;

            } // Ends if failed to get array
            else // Else got array
            {
                // This is fragile. If for some reason buffer wraps, nextResult is going to get very large.
                for (UInt32 nextResult = UdbusFunctions.dbus_msg_body_get_array_left(this.msg, ref array_reader),
                        lastResult = nextResult;
                     nextResult != 0 && lastResult >= nextResult;
                     lastResult = nextResult,
                     nextResult = UdbusFunctions.dbus_msg_body_get_array_left(this.msg, ref array_reader))
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format("Array nextResult: {0}", nextResult.ToString()));
                    yield return this;

                }
            } // Ends else got array
        }

        public int ReadStructureStart()
        {
            this.result = UdbusFunctions.dbus_msg_body_get_structure(this.msg);
            return this.result;
        }

        public int ReadStructureEnd()
        {
            return this.result;
        }

        public int ReadVariantSignature(out Udbus.Types.dbus_sig sig)
        {
            // Unlike most functions, this one needs some memory allocated.
            sig = Udbus.Types.dbus_sig.Initialiser;
            UdbusFunctions.dbus_msg_body_get_variant(this.msg, ref sig);
            return this.result;
        }

        public Udbus.Types.dbus_sig ReadVariantSignature()
        {
            Udbus.Types.dbus_sig sig;
            this.ReadVariantSignature(out sig);
            return sig;
        }

        public Udbus.Types.dbus_sig ReadVariantSignatureValue(out int result)
        {
            Udbus.Types.dbus_sig sig;
            result = this.result = this.ReadVariantSignature(out sig);
            return sig;
        }

        public int ReadVariant(out Udbus.Containers.dbus_union val)
        {
            Udbus.Types.dbus_sig sig;
            int result = this.ReadVariantSignature(out sig);

            if (result == 0) // If got signature
            {
                Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(new Udbus.Parsing.CodeTypeNoOpHolder());
                Udbus.Serialization.Variant.UdbusVariantIn variantIn = new Udbus.Serialization.Variant.UdbusVariantIn();
                Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new Udbus.Parsing.IDLArgumentTypeNameBuilderNoOp();
                Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(variantIn, nameBuilder, sig.a, context);
                result = this.result;

                if (result == 0) // If read variant ok
                {
                    // Todo - no delegate ? throw an invalid signature exception.
                    result = variantIn.VariantDelegate(this, out val);

                } // Ends if read variant ok
                else // Else failed to read variant
                {
                    val = Udbus.Containers.dbus_union.Create(Udbus.Containers.dbus_union.Invalid);

                } // Ends else failed to read variant

            } // Ends if got signature
            else // Else failed to get signature
            {
                val = Udbus.Containers.dbus_union.Create(Udbus.Containers.dbus_union.Invalid);

            } // Ends else failed to get signature

            return result;
        }

        public Udbus.Containers.dbus_union ReadVariant()
        {
            Udbus.Containers.dbus_union val;
            this.ReadVariant(out val);
            return val;
        }

        public Udbus.Containers.dbus_union ReadVariantValue(out int result)
        {
            Udbus.Containers.dbus_union val;
            result = this.result = this.ReadVariant(out val);
            return val;
        }

        #region Marshalling
        #region Enumerable Marshalling
        /// <summary>
        /// Functor for MarshalEnumerable.
        /// </summary>
        /// <typeparam name="T">Struct type being marshalled.</typeparam>
        private struct EnumerableMarshallerImpl<T>
        {
            private MarshalReadDelegate<T> marshal;
            private Udbus.Types.dbus_type element;

            public EnumerableMarshallerImpl(MarshalReadDelegate<T> marshal, Udbus.Types.dbus_type element)
            {
                this.marshal = marshal;
                this.element = element;
            }

            public int Marshal(Udbus.Serialization.UdbusMessageReader reader, out IEnumerable<T> t)
            {
                return reader.MarshalEnumerable(this.marshal, this.element, out t);
            }

            public int Marshal(Udbus.Serialization.UdbusMessageReader reader, out T[] t)
            {
                return reader.MarshalEnumerable(this.marshal, this.element, out t);
            }

        } // Ends class EnumerableMarshallerImpl

        /// <summary>
        /// Generator for enumerable in message reader.
        /// </summary>
        /// <typeparam name="T">Type being enumerated.</typeparam>
        /// <param name="marshal">Delegate to read with.</param>
        /// <param name="element">dbus_type of element(s) in array.</param>
        /// <returns>Iterator over element(s) in message array.</returns>
        protected IEnumerable<T> MarshalEnumerable<T>(MarshalReadDelegate<T> marshal, Udbus.Types.dbus_type element)
        {
            foreach (Udbus.Serialization.UdbusMessageReader subreader in this.ArrayReader(element))
            {
                T value;
                this.result = marshal(subreader, out value);

                if (this.result != 0) // If failed to marshal
                {
                    break;

                } // Ends if failed to marshal

                yield return value;

            } // Ends loop reading values
        }

        // Returns result (generator).
        public int MarshalEnumerable<T>(MarshalReadDelegate<T> marshal, Udbus.Types.dbus_type element, out IEnumerable<T> ts)
        {
            ts = this.MarshalEnumerable(marshal, element);
            return this.result;
        }

        // Returns result (forces evaluation).
        public int MarshalEnumerable<T>(MarshalReadDelegate<T> marshal, Udbus.Types.dbus_type element, out T[] ts)
        {
            ts = this.MarshalEnumerable(marshal, element).ToArray(); // Force the iterator evaluation.
            return this.result;
        }

        #endregion // Enumerable Marshalling

        #region Struct Marshalling
        public int MarshalStruct<T>(MarshalReadDelegate<T> marshal, out T t)
        {
            int result = this.ReadStructureStart();

            t = default(T);

            if (result == 0) // If added structure ok
            {
                result = marshal(this, out t);

                if (result == 0) // If marshalled ok
                {
                    result = this.ReadStructureEnd();

                } // Ends if marshalled ok
            }

            return result;
        }

        /// <summary>
        /// Functor for MarshalStruct.
        /// </summary>
        /// <typeparam name="T">Struct type being marshalled.</typeparam>
        private struct StructMarshallerImpl<T>
        {
            private MarshalReadDelegate<T> marshal;

            public StructMarshallerImpl(MarshalReadDelegate<T> marshal)
            {
                this.marshal = marshal;
            }

            public int Marshal(Udbus.Serialization.UdbusMessageReader reader, out T t)
            {
                return reader.MarshalStruct(this.marshal, out t);
            }
        } // Ends class StructMarshallerImpl

        /// <summary>
        /// Create a delegate for marshalling a struct type T.
        /// </summary>
        /// <typeparam name="T">Struct type to marshal.</typeparam>
        /// <param name="marshal">Struct marshalling delegate.</param>
        /// <returns>A delegate which wraps marshalling delegate parameter in appropriate struct marshalling calls.</returns>
        static public MarshalReadDelegate<T> StructMarshaller<T>(MarshalReadDelegate<T> marshal)
        {
            return new StructMarshallerImpl<T>(marshal).Marshal;
        }

        // Iterator version.
        public IEnumerable<T> MarshalStructs<T>(MarshalReadDelegate<T> marshal)
        {
            return this.MarshalEnumerable(StructMarshaller(marshal), Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN);
        }

        public int MarshalStructs<T>(MarshalReadDelegate<T> marshal, out IEnumerable<T> ts)
        {
            return this.MarshalEnumerable(StructMarshaller(marshal), Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, out ts);
        }
        #endregion // Struct Marshalling

        #region Dictionary Marshalling
        /// <summary>
        /// KeyValuePair Marshal functor.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        private struct KeyValuePairMarshallerImpl<TKey, TValue>
        {
            private MarshalReadDelegate<TKey> marshalKey;
            private MarshalReadDelegate<TValue> marshalValue;

            public KeyValuePairMarshallerImpl(MarshalReadDelegate<TKey> marshalKey, MarshalReadDelegate<TValue> marshalValue)
            {
                this.marshalKey = marshalKey;
                this.marshalValue = marshalValue;
            }

            public int Marshal(Udbus.Serialization.UdbusMessageReader reader, out KeyValuePair<TKey, TValue> pair)
            {
                pair = default(KeyValuePair<TKey, TValue>);

                TKey key;
                int result = this.marshalKey(reader, out key);

                //System.Diagnostics.Debug.WriteLine(string.Format("Key: {0}", key.ToString()));
                if (result == 0) // If marshalled key ok
                {
                    TValue value;
                    result = this.marshalValue(reader, out value);

                    if (result == 0) // If marshalled value ok
                    {
                        pair = new KeyValuePair<TKey, TValue>(key, value);
                    }
                } // Ends if marshalled key ok

                return result;
            }
        } // Ends KeyValuePairMarshallerImpl

        private static TKey KeyValuePairKey<TKey, TValue>(KeyValuePair<TKey, TValue> pair) { return pair.Key; }
        private static TValue KeyValuePairValue<TKey, TValue>(KeyValuePair<TKey, TValue> pair) { return pair.Value; }

        // TODO: Rewrite handwritten version with these functions.
        // Iterator version.
        public IEnumerable<KeyValuePair<TKey, TValue>> MarshalDict<TKey, TValue>(MarshalReadDelegate<TKey> marshalKey, MarshalReadDelegate<TValue> marshalValue)
        {
            // Could compose this of other functions...
            // It's an array of structs, where each struct has two elements - the key and the value.
            return this.MarshalStructs<KeyValuePair<TKey, TValue>>(new KeyValuePairMarshallerImpl<TKey, TValue>(marshalKey, marshalValue).Marshal);
        }

        public int MarshalDict<TKey, TValue>(MarshalReadDelegate<TKey> marshalKey, MarshalReadDelegate<TValue> marshalValue,
            out IEnumerable<KeyValuePair<TKey, TValue>> dict)
        {
            // Could compose this of other functions...
            // It's an array of structs, where each struct has two elements - the key and the value.
            return this.MarshalStructs<KeyValuePair<TKey, TValue>>(new KeyValuePairMarshallerImpl<TKey, TValue>(marshalKey, marshalValue).Marshal, out dict);
        }

        public int MarshalDict<TKey, TValue>(MarshalReadDelegate<TKey> marshalKey, MarshalReadDelegate<TValue> marshalValue,
            out IDictionary<TKey, TValue> dict)
        {
            IEnumerable<KeyValuePair<TKey, TValue>> enumDict;
            int result = this.MarshalDict(marshalKey, marshalValue, out enumDict);
            dict = enumDict.ToDictionary<KeyValuePair<TKey, TValue>, TKey, TValue>(KeyValuePairKey, KeyValuePairValue);
            return result;
        }

        public int MarshalDict<TKey, TValue>(MarshalReadDelegate<TKey> marshalKey, MarshalReadDelegate<TValue> marshalValue,
             out Dictionary<TKey, TValue> dict)
        {
            IEnumerable<KeyValuePair<TKey, TValue>> enumDict;
            int result = this.MarshalDict(marshalKey, marshalValue, out enumDict);
            dict = enumDict.ToDictionary<KeyValuePair<TKey, TValue>, TKey, TValue>(KeyValuePairKey, KeyValuePairValue);
            return result;
        }

        #endregion // Dictionary Marshalling
        #endregion // Ends Marshalling

        #region Static reader functions
        #region Static Out Param reader functions
        public static int ReadByte(UdbusMessageReader reader, out byte value)
        {
            return reader.ReadByte(out value);
        }

        public static int ReadBoolean(UdbusMessageReader reader, out bool value)
        {
            return reader.ReadBoolean(out value);
        }

        public static int ReadInt16(UdbusMessageReader reader, out Int16  value)
        {
            return reader.ReadInt16(out value);
        }

        public static int ReadUInt16(UdbusMessageReader reader, out UInt16 value)
        {
            return reader.ReadUInt16(out value);
        }

        public static int ReadInt32(UdbusMessageReader reader, out Int32 value)
        {
            return reader.ReadInt32(out value);
        }

        public static int ReadUInt32(UdbusMessageReader reader, out UInt32 value)
        {
            return reader.ReadUInt32(out value);
        }

        public static int ReadInt64(UdbusMessageReader reader, out Int64 value)
        {
            return reader.ReadInt64(out value);
        }

        public static int ReadUInt64(UdbusMessageReader reader, out UInt64 value)
        {
            return reader.ReadUInt64(out value);
        }

        public static int ReadDouble(UdbusMessageReader reader, out double value)
        {
            return reader.ReadDouble(out value);
        }

        public static int ReadString(UdbusMessageReader reader, out string value)
        {
            return reader.ReadString(out value);
        }

        public static int ReadObjectPath(UdbusMessageReader reader, out Udbus.Types.UdbusObjectPath value)
        {
            return reader.ReadObjectPath(out value);
        }

        public static int ReadObjectPath(UdbusMessageReader reader, out string value)
        {
            return reader.ReadObjectPath(out value);
        }

        public static int ReadSignature(UdbusMessageReader reader, out Udbus.Types.dbus_sig value)
        {
            return reader.ReadSignature(out value);
        }

        public static int ReadSignatureRef(UdbusMessageReader reader, ref Udbus.Types.dbus_sig value)
        {
            return reader.ReadSignatureRef(ref value);
        }

        public static int ReadVariant(UdbusMessageReader reader, out Udbus.Types.dbus_sig value)
        {
            return reader.ReadVariantSignature(out value);
        }

        public static int ReadVariant(UdbusMessageReader reader, out Udbus.Containers.dbus_union value)
        {
            return reader.ReadVariant(out value);
        }


        #endregion // Ends Static Out Param reader functions

        #region Static Value reader functions
        public static byte ReadByteValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadByteValue(out result);
        }

        public static bool ReadBooleanValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadBooleanValue(out result);
        }

        public static Int16 ReadInt16Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt16Value(out result);
        }

        public static UInt16 ReadUInt16Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadUInt16Value(out result);
        }

        public static Int32 ReadInt32Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt32Value(out result);
        }

        public static UInt32 ReadUInt32Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadUInt32Value(out result);
        }

        public static Int64 ReadInt64Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadInt64Value(out result);
        }

        public static UInt64 ReadUInt64Value(UdbusMessageReader reader, out int result)
        {
            return reader.ReadUInt64Value(out result);
        }

        public static double ReadDoubleValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadDoubleValue(out result);
        }

        public static string ReadStringValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadStringValue(out result);
        }

        public static Udbus.Types.UdbusObjectPath ReadObjectPathValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadObjectPathValue(out result);
        }

        public static string ReadObjectPathStringValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadObjectPathStringValue(out result);
        }

        public static Udbus.Types.dbus_sig ReadVariantValue(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantSignatureValue(out result);
        }

        public static Udbus.Containers.dbus_union ReadVariant(UdbusMessageReader reader, out int result)
        {
            return reader.ReadVariantValue(out result);
        }

        #endregion // Ends Static Value reader functions
        #endregion // Ends Static reader functions

#if HAVESOMECCODE
		dbus_array_reader array_reader = {0};
		r |= dbus_msg_body_get_array(recv, &array_reader);
		int counter = 0;
		char szMessage[512];

		while(dbus_msg_body_get_array_left(recv, &array_reader) > 0)
		{
			char *val = NULL;
			r |= dbus_msg_body_get_string(recv, &val);
			if (r != 0 || val == NULL)
			{
				dio.io_debug(dio.logpriv, "Oops\n");
				break;
			}
			sprintf_s(szMessage, "Here is string[%d]: %s\n", counter, val);
			//printf("Here is string[%d]: %s\n", counter, val);
			dio.io_debug(dio.logpriv, szMessage);
		
			++counter;
		}
#endif // HAVESOMECCODE

        #region IDisposable Members

        public void Dispose()
        {
            msg.Dispose();
        }

        #endregion // Ends IDisposable Members
#if _NOMAGIC
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

        static public MarshalReadDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshaller<T>(MarshalReadDelegate<IEnumerable<T>> marshal)
        {
            return new EnumerableMarshallerImpl<IEnumerable<T>>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        //static public MarshalReadDelegate<IEnumerable<IDictionary<,>>> EnumerableMarshaller<IDictionary<,>>(MarshalReadDelegate<IDictionary<,>> marshal)
        //{
        //    return new EnumerableMarshallerImpl<IDictionary<,>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        //}

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

#endif // !_NOMAGIC
    } // Ends class UdbusMessageReader
} // Ends namespace Udbus.Serialization
