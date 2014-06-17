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
    //using NMessage = NMessageStruct;
    //using NMessageFunctions = UdbusMsgStructFunctions;

    using NMessage = NMessageHandle;
    using NMessageFunctions = UdbusMsgHandleFunctions;

    public struct ReadonlyDbusConnectionParameters
    {
        private readonly string destination_;
        private readonly string path_;
        private readonly string interface_;

        public string Destination
        {
            get { return this.destination_; }
        }

        public string Path
        {
            get { return this.path_; }
        }

        public string Interface
        {
            get { return this.interface_; }
        }

        public ReadonlyDbusConnectionParameters(string destination, string path, string interfaceName)
        {
            this.destination_ = destination;
            this.path_ = path;
            this.interface_ = interfaceName;
        }

        public ReadonlyDbusConnectionParameters(DbusConnectionParameters source)
            : this(source.Destination, source.Path, source.Interface)
        {
        }

        public ReadonlyDbusConnectionParameters(ReadonlyDbusConnectionParameters source)
            : this(source.Destination, source.Path, source.Interface)
        {
        }

        public static implicit operator DbusConnectionParameters(ReadonlyDbusConnectionParameters cast)
        {
            return new DbusConnectionParameters(cast);
        }
    } // Ends struct ReadonlyDbusConnectionParameters

    public struct DbusConnectionParameters
    {
        private string destination_;
        private string path_;
        private string interface_;

        public string Destination
        {
            get { return this.destination_; }
            set { this.destination_ = value; }
        }

        public string Path
        {
            get { return this.path_; }
            set { this.path_ = value; }
        }

        public string Interface
        {
            get { return this.interface_; }
            set { this.interface_ = value; }
        }

        public DbusConnectionParameters(string destination, string path, string interfaceName)
        {
            this.destination_ = destination;
            this.path_ = path;
            this.interface_ = interfaceName;
        }

        public DbusConnectionParameters(DbusConnectionParameters source)
            : this(source.Destination, source.Path, source.Interface)
        {
        }

        public DbusConnectionParameters(ReadonlyDbusConnectionParameters source)
            : this(source.Destination, source.Path, source.Interface)
        {
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder(base.ToString());

            result.AppendFormat(". Destination: '{0}'. Interface: '{1}'. Path: '{2}'",
                this.Destination, this.Interface, this.Path);

            return result.ToString();
        }
    } // Ends struct DbusConnectionParameters

    public partial class UdbusMessageBuilder
    {
        protected NMessage.UdbusMessageHandle message;
        protected int result = 0;

        public NMessage.UdbusMessageHandle Message
        {
            get { return this.message; }
        }

        public int Result
        {
            get { return this.result; }
        }

        public UdbusMessageBuilder()
        {
        }

        /// Message construction.
        public UdbusMessageBuilder UdbusMessage(UInt32 serial)
        {
            this.message = NMessageFunctions.UdbusMessage(serial);
            return this;
        }

        public UdbusMessageBuilder UdbusMethodMessage(UInt32 serial,
            string destination, string path,
            string interface_, string method)
        {
            this.message = NMessageFunctions.UdbusMethodMessage(serial, destination, path, interface_, method);
            return this;
        }

        public UdbusMessageBuilder UdbusSignalMessage(UInt32 serial,
            string path, string interface_, string name)
        {
            this.message = NMessageFunctions.UdbusSignalMessage(serial, path, interface_, name);
            return this;
        }

        public UdbusMessageBuilder UdbusMethodMessage(UInt32 serial, DbusConnectionParameters parameters, string method)
        {
            this.message = NMessageFunctions.UdbusMethodMessage(serial, parameters.Destination, parameters.Path, parameters.Interface, method);
            return this;
        }

        public UdbusMessageBuilder UdbusSignalMessage(UInt32 serial, DbusConnectionParameters parameters, string name)
        {
            this.message = NMessageFunctions.UdbusSignalMessage(serial, parameters.Path, parameters.Interface, name);
            return this;
        }

        protected UdbusMessageBuilder(NMessage.UdbusMessageHandle message)
        {
            this.message = message;
        }

        protected virtual UdbusMessageBuilder WrapMessage(NMessage.UdbusMessageHandle message)
        {
            return new UdbusMessageBuilder(message);
        }

        /// Message building functions.
        public UdbusMessageBuilder SetDestination(string destination)
        {
            UdbusFunctions.dbus_msg_set_destination(this.message, destination);
            return this;
        }
        public UdbusMessageBuilder SetPath(string path)
        {
            UdbusFunctions.dbus_msg_set_path(this.message, path);
            return this;
        }
        public UdbusMessageBuilder SetMethod(string method)
        {
            UdbusFunctions.dbus_msg_set_method(this.message, method);
            return this;
        }
        public UdbusMessageBuilder SetErrorName(string error_name)
        {
            UdbusFunctions.dbus_msg_set_error_name(this.message, error_name);
            return this;
        }
        public UdbusMessageBuilder SetSender(string sender)
        {
            UdbusFunctions.dbus_msg_set_sender(this.message, sender);
            return this;
        }
        public UdbusMessageBuilder SetInterface(string interface_)
        {
            UdbusFunctions.dbus_msg_set_interface(this.message, interface_);
            return this;
        }
        public UdbusMessageBuilder SetSignature(ref Udbus.Types.dbus_sig signature)
        {
            UdbusFunctions.dbus_msg_set_signature(this.message, ref signature);
            return this;
        }

        public void SetDestination(NMessageHandle.UdbusMessageHandle msg, string destination)
        {
            UdbusFunctions.dbus_msg_set_destination(msg, destination);
        }
        public void SetPath(NMessageHandle.UdbusMessageHandle msg, string path)
        {
            UdbusFunctions.dbus_msg_set_path(msg, path);
        }
        public void SetMethod(NMessageHandle.UdbusMessageHandle msg, string method)
        {
            UdbusFunctions.dbus_msg_set_method(msg, method);
        }
        public void SetErrorName(NMessageHandle.UdbusMessageHandle msg, string error_name)
        {
            UdbusFunctions.dbus_msg_set_error_name(msg, error_name);
        }
        public void SetSender(NMessageHandle.UdbusMessageHandle msg, string sender)
        {
            UdbusFunctions.dbus_msg_set_sender(msg, sender);
        }
        public void SetInterface(NMessageHandle.UdbusMessageHandle msg, string interface_)
        {
            UdbusFunctions.dbus_msg_set_interface(msg, interface_);
        }
        public void SetSignature(NMessageHandle.UdbusMessageHandle msg, ref Udbus.Types.dbus_sig signature)
        {
            UdbusFunctions.dbus_msg_set_signature(msg, ref signature);
        }

        #region Message body adding functions
        #region Add to internal message body
// New stuff
#if _NOMAGIC
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
                throw new InvalidCastException("object is not a string");
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
                throw new InvalidCastException("object is not a string");
            }
            return result;
        }
        static public int BodyAdd_StringObject(UdbusMessageBuilder builder, object val)
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
                throw new InvalidCastException("object is not a string");
            }
            return result;
        }

        public int BodyAdd_VariantByte(Udbus.Containers.dbus_union variant)
        {
            return this.BodyAdd_Byte(variant.DbusByte).Result;
        }

        static public int BodyAdd_VariantByte(UdbusMessageBuilder builder, Udbus.Containers.dbus_union variant)
        {
            return builder.BodyAdd_VariantByte(variant);
        }
#endif // _NOMAGIC

        // Ends new stuff

        public UdbusMessageBuilder BodyAdd(UInt32 length)
        {
            this.result = UdbusFunctions.dbus_msg_body_add(this.message, length);
            return this;
        }

        public UdbusMessageBuilder BodyAdd_Byte(byte val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_byte(this.message, val);
            return this;
        }

        public UdbusMessageBuilder BodyAdd_Boolean(bool val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_boolean(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Int16(Int16 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_int16(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_UInt16(UInt16 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_uint16(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Int32(Int32 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_int32(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_UInt32(UInt32 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_uint32(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Int64(Int64 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_int64(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_UInt64(UInt64 val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_uint64(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Double(double val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_double(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_String(string val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_string(this.message, val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_ObjectPath(string val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_objectpath(this.message, val);
            return this;
        }

        public UdbusMessageBuilder BodyAdd_ObjectPath(Udbus.Types.UdbusObjectPath val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_objectpath(this.message, val.Path);
            return this;
        }

        public UdbusMessageBuilder BodyAdd_Signature(Udbus.Types.dbus_sig val)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_variant(this.message, ref val);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_ArrayBegin(ref dbus_array_writer ptr, Udbus.Types.dbus_type element)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_array_begin(this.message, element, ref ptr);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_ArrayEnd(ref dbus_array_writer ptr)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_array_end(this.message, ref ptr);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Structure()
        {
            this.result = UdbusFunctions.dbus_msg_body_add_structure(this.message);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_StructureEnd()
        {
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Variant(ref Udbus.Types.dbus_sig signature)
        {
            this.result = UdbusFunctions.dbus_msg_body_add_variant(this.message, ref signature);
            return this;
        }
        public UdbusMessageBuilder BodyAdd_Variant(Udbus.Containers.dbus_union val)
        {
            int result = this.BodyAdd_Signature(val.Signature).Result;

            if (result == 0) // If got signature
            {
                Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(new Udbus.Parsing.CodeTypeNoOpHolder());
                Udbus.Serialization.Variant.UdbusVariantOut variantOut = new Udbus.Serialization.Variant.UdbusVariantOut();
                Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new Udbus.Parsing.IDLArgumentTypeNameBuilderNoOp();
                Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(variantOut, nameBuilder, val.Signature.a, context);
                result = this.result;

                if (result == 0) // If write variant ok
                {
                    result = variantOut.VariantDelegate(this, val);

                } // Ends if write variant ok
                else // Else failed to write variant
                {

                } // Ends else failed to write variant

            } // Ends if got signature
            else // Else failed to get signature
            {

            } // Ends else failed to get signature

            return this;
        }

        #endregion Add to internal message body

        #region Add to message parameter body
        public int BodyAdd(NMessageHandle.UdbusMessageHandle msg, UInt32 length)
        {
            int result = UdbusFunctions.dbus_msg_body_add(msg, length);
            return result;
        }

        public int BodyAdd_Byte(NMessageHandle.UdbusMessageHandle msg, byte val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_byte(msg, val);
            return result;
        }

        public int BodyAdd_Boolean(NMessageHandle.UdbusMessageHandle msg, bool val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_boolean(msg, val);
            return result;
        }
        public int BodyAdd_Int16(NMessageHandle.UdbusMessageHandle msg, Int16 val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_int16(msg, val);
            return result;
        }
        public int BodyAdd_UInt16(NMessageHandle.UdbusMessageHandle msg, UInt16 val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_uint16(msg, val);
            return result;
        }
        public int BodyAdd_Int32(NMessageHandle.UdbusMessageHandle msg, Int32 val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_int32(msg, val);
            return result;
        }
        public int BodyAdd_UInt32(NMessageHandle.UdbusMessageHandle msg, UInt32 val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_uint32(msg, val);
            return result;
        }
        public int BodyAdd_Int64(NMessageHandle.UdbusMessageHandle msg, Int64 val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_int64(msg, val);
            return result;
        }
        public int BodyAdd_UInt64(NMessageHandle.UdbusMessageHandle msg, UInt64 val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_uint64(msg, val);
            return result;
        }
        public int BodyAdd_Double(NMessageHandle.UdbusMessageHandle msg, double val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_double(msg, val);
            return result;
        }
        public int BodyAdd_String(NMessageHandle.UdbusMessageHandle msg, string val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_string(msg, val);
            return result;
        }
        public int BodyAdd_ObjectPath(NMessageHandle.UdbusMessageHandle msg, string val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_objectpath(msg, val);
            return result;
        }
        public int BodyAdd_ObjectPath(NMessageHandle.UdbusMessageHandle msg, Udbus.Types.UdbusObjectPath val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_objectpath(msg, val.Path);
            return result;
        }
        public int BodyAdd_Signature(NMessageHandle.UdbusMessageHandle msg, Udbus.Types.dbus_sig val)
        {
            int result = UdbusFunctions.dbus_msg_body_add_variant(msg, ref val);
            return result;
        }
        public int BodyAdd_ArrayBegin(NMessageHandle.UdbusMessageHandle msg, Udbus.Types.dbus_type element, ref dbus_array_writer ptr)
        {
            int result = UdbusFunctions.dbus_msg_body_add_array_begin(msg, element, ref ptr);
            return result;
        }
        public int BodyAdd_ArrayEnd(NMessageHandle.UdbusMessageHandle msg, ref dbus_array_writer ptr)
        {
            int result = UdbusFunctions.dbus_msg_body_add_array_end(msg, ref ptr);
            return result;
        }
        public int BodyAdd_Structure(NMessageHandle.UdbusMessageHandle msg)
        {
            int result = UdbusFunctions.dbus_msg_body_add_structure(msg);
            return result;
        }
        public int BodyAdd_Variant(NMessageHandle.UdbusMessageHandle msg, ref Udbus.Types.dbus_sig signature)
        {
            int result = UdbusFunctions.dbus_msg_body_add_variant(msg, ref signature);
            return result;
        }
        public int BodyAdd_Variant(NMessageHandle.UdbusMessageHandle msg, Udbus.Containers.dbus_union val)
        {
            // Bit of a nasty hack, but since our variant code is tightly coupled with builder instance,
            // the easiest thing to do here is to wrap the message in a builder (if necessary).
            UdbusMessageBuilder wrapper = this;

            if (message != this.Message)
            {
                wrapper = this.WrapMessage(msg);
            }

            wrapper.BodyAdd_Variant(val);
            return wrapper.Result;
        }

        #endregion // Ends Add to message parameter body
        #endregion // Ends Message body adding functions

        #region Static Message body adding functions
        #region Add to internal message body static functions
        static public int BodyAdd(UdbusMessageBuilder builder, UInt32 length)
        {
            return builder.BodyAdd(length).Result;
        }

        static public int BodyAdd_Byte(UdbusMessageBuilder builder, byte val)
        {
            return builder.BodyAdd_Byte(val).Result;
        }

        static public int BodyAdd_Boolean(UdbusMessageBuilder builder, bool val)
        {
            return builder.BodyAdd_Boolean(val).Result;
        }
        static public int BodyAdd_Int16(UdbusMessageBuilder builder, Int16 val)
        {
            return builder.BodyAdd_Int16(val).Result;
        }
        static public int BodyAdd_UInt16(UdbusMessageBuilder builder, UInt16 val)
        {
            return builder.BodyAdd_UInt16(val).Result;
        }
        static public int BodyAdd_Int32(UdbusMessageBuilder builder, Int32 val)
        {
            return builder.BodyAdd_Int32(val).Result;
        }
        static public int BodyAdd_UInt32(UdbusMessageBuilder builder, UInt32 val)
        {
            return builder.BodyAdd_UInt32(val).Result;
        }
        static public int BodyAdd_Int64(UdbusMessageBuilder builder, Int64 val)
        {
            return builder.BodyAdd_Int64(val).Result;
        }
        static public int BodyAdd_UInt64(UdbusMessageBuilder builder, UInt64 val)
        {
            return builder.BodyAdd_UInt64(val).Result;
        }
        static public int BodyAdd_Double(UdbusMessageBuilder builder, double val)
        {
            return builder.BodyAdd_Double(val).Result;
        }
        static public int BodyAdd_String(UdbusMessageBuilder builder, string val)
        {
            return builder.BodyAdd_String(val).Result;
        }
        static public int BodyAdd_ObjectPath(UdbusMessageBuilder builder, string val)
        {
            return builder.BodyAdd_ObjectPath(val).Result;
        }
        static public int BodyAdd_ObjectPath(UdbusMessageBuilder builder, Udbus.Types.UdbusObjectPath val)
        {
            return builder.BodyAdd_ObjectPath(val).Result;
        }
        static public int BodyAdd_Signature(UdbusMessageBuilder builder, Udbus.Types.dbus_sig val)
        {
            return builder.BodyAdd_Signature(val).Result;
        }
        static public int BodyAdd_Variant(UdbusMessageBuilder builder, Udbus.Containers.dbus_union variant)
        {
            return builder.BodyAdd_Variant(variant).Result;
        }
        static public UdbusMessageBuilder BodyAdd_ArrayBegin(UdbusMessageBuilder builder, Udbus.Types.dbus_type element, ref dbus_array_writer ptr)
        {
            return builder.BodyAdd_ArrayBegin(ref ptr, element);
        }
        static public UdbusMessageBuilder BodyAdd_ArrayEnd(UdbusMessageBuilder builder, ref dbus_array_writer ptr)
        {
            return builder.BodyAdd_ArrayEnd(ref ptr);
        }
        static public UdbusMessageBuilder BodyAdd_Structure(UdbusMessageBuilder builder)
        {
            return builder.BodyAdd_Structure();
        }
        static public UdbusMessageBuilder BodyAdd_StructureEnd(UdbusMessageBuilder builder)
        {
            return builder.BodyAdd_StructureEnd();
        }
        static public UdbusMessageBuilder BodyAdd_Variant(UdbusMessageBuilder builder, Udbus.Types.dbus_sig signature)
        {
            return builder.BodyAdd_Variant(ref signature);
        }
        #endregion // Ends Add to internal message body static functions
        #endregion // Ends Static Message body adding functions

        #region Marshalling
        #region Marshalling (no result)
        #region Enumerable Marshalling (no result)
        protected void MarshalEnumerable<T>(IEnumerable<T> ts, Udbus.Types.dbus_type element, MarshalResultDelegate<T> marshal, out int result)
        {
            result = 0;
            Udbus.Serialization.dbus_array_writer arraywriter = new Udbus.Serialization.dbus_array_writer();
            Udbus.Serialization.UdbusMessageBuilder subbuilder = this.BodyAdd_ArrayBegin(ref arraywriter, element);
            foreach (T t in ts)
            {
                marshal(this, t, out result);
                if (result != 0) // If error occurred
                {
                    break;

                } // Ends if error occurred
            }

            if (result == 0) // If marshalled ok
            {
                this.BodyAdd_ArrayEnd(ref arraywriter);

            } // Ends if marshalled ok
        }
        #region MarshalEnumerable overloads

        public void MarshalEnumerableStruct<T>(IEnumerable<T> ts, MarshalResultDelegate<T> marshal, out int result)
        {
            MarshalEnumerable<T>(ts, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, marshal, out result);
        }

        #endregion // MarshalEnumerable overloads

        /// <summary>
        /// Functor for MarshalEnumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private struct EnumerableResultMarshallerImpl<T>
        {
            private MarshalResultDelegate<T> marshal;
            private Udbus.Types.dbus_type element;

            public EnumerableResultMarshallerImpl(MarshalResultDelegate<T> marshal, Udbus.Types.dbus_type element)
            {
                this.marshal = marshal;
                this.element = element;
            }

            public void Marshal(Udbus.Serialization.UdbusMessageBuilder builder, IEnumerable<T> ts, out int result)
            {
                builder.MarshalEnumerable(ts, this.element, this.marshal, out result);
            }
        } // Ends struct EnumerableResultMarshallerImpl

        //static public MarshalResultDelegate<IEnumerable<T>> EnumerableMarshaller<T>(MarshalResultDelegate<T> marshal)
        //{
        //    return new EnumerableResultMarshallerImpl<T>(marshal).Marshal;
        //}

        #region EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        static public MarshalResultDelegate<IEnumerable<T>> EnumerableMarshallerStruct<T>(MarshalResultDelegate<T> marshal)
        {
            return new EnumerableResultMarshallerImpl<T>(marshal, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN).Marshal;
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        #region EnumerableResultMarshallerImpl ArrayMarshaller overloads

        static public MarshalResultDelegate<T[]> ArrayMarshallerStruct<T>(MarshalResultDelegate<T> marshal)
        {
            return new EnumerableResultMarshallerImpl<T>(marshal, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN).Marshal;
            //return ArrayMarshaller(marshal);
        }

        // NOT SURE ABOUT THIS ONE - only solves one level of depth.
        //static public MarshalResultDelegate<IEnumerable<IEnumerable<T>>> ArrayMarshallerEnumerable<T>(MarshalResultDelegate<IEnumerable<T>> marshal)
        //{
        //    return EnumerableMarshaller(marshal);
        //}

        #endregion // EnumerableResultMarshallerImpl ArrayMarshaller overloads
        #endregion // Enumerable Marshalling (no result)

        #region Struct Marshalling (no result)

        public void MarshalStruct<T>(T t, MarshalResultDelegate<T> marshal, out int result)
        {
            this.BodyAdd_Structure();
            marshal(this, t, out result);
            this.BodyAdd_StructureEnd();
        }

        /// <summary>
        /// Functor for MarshalStruct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private struct StructResultMarshallerImpl<T>
        {
            private MarshalResultDelegate<T> marshal;

            public StructResultMarshallerImpl(MarshalResultDelegate<T> marshal)
            {
                this.marshal = marshal;
            }

            public void Marshal(Udbus.Serialization.UdbusMessageBuilder builder, T t, out int result)
            {
                builder.MarshalStruct(t, this.marshal, out result);
            }
        } // Ends struct StructResultMarshallerImpl

        static public MarshalResultDelegate<T> StructMarshaller<T>(MarshalResultDelegate<T> marshal)
        {
            return new StructResultMarshallerImpl<T>(marshal).Marshal;
        }

        public void MarshalStructs<T>(IEnumerable<T> ts, MarshalResultDelegate<T> marshal, out int result)
        {
            this.MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_ARRAY, StructMarshaller(marshal), out result);
        }
        #endregion // Struct Marshalling (no result)

        #region Dictionary Marshalling (no result)

        /// <summary>
        /// KeyValuePair Marshal functor.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        private struct KeyValuePairResultMarshallerImpl<TKey, TValue>
        {
            private MarshalResultDelegate<TKey> marshalKey;
            private MarshalResultDelegate<TValue> marshalValue;

            public KeyValuePairResultMarshallerImpl(MarshalResultDelegate<TKey> marshalKey, MarshalResultDelegate<TValue> marshalValue)
            {
                this.marshalKey = marshalKey;
                this.marshalValue = marshalValue;
            }

            public void Marshal(Udbus.Serialization.UdbusMessageBuilder builder, KeyValuePair<TKey, TValue> pair, out int result)
            {
                this.marshalKey(builder, pair.Key, out result);

                if (result == 0) // If marshalled key ok
                {
                    this.marshalValue(builder, pair.Value, out result);

                } // Ends if marshalled key ok
            }
        } // Ends struct KeyValuePairResultMarshallerImpl

        public void MarshalDict<TKey, TValue>(IDictionary<TKey, TValue> dict,
            MarshalResultDelegate<TKey> marshalKey, MarshalResultDelegate<TValue> marshalValue,
            out int result)
        {
            // Could compose this of other functions...
            // It's an array of structs, where each struct has two elements - the key and the value.
            this.MarshalStructs(dict, new KeyValuePairResultMarshallerImpl<TKey, TValue>(marshalKey, marshalValue).Marshal, out result);
        }

        #endregion // Dictionary Marshalling (no result)
        #endregion // Marshalling (no result)

        #region Marshalling (with result)
        #region Enumerable Marshalling (with result)
        // Returns result.
        public int MarshalEnumerable<T>(IEnumerable<T> ts, Udbus.Types.dbus_type element, MarshalDelegate<T> marshal)
        {
            Udbus.Serialization.dbus_array_writer arraywriter = new Udbus.Serialization.dbus_array_writer();
            Udbus.Serialization.UdbusMessageBuilder subbuilder = this.BodyAdd_ArrayBegin(ref arraywriter, element);
            int result = subbuilder.Result;

            if (result == 0) // If got subbuilder ok
            {
                foreach (T t in ts)
                {
                    result = marshal(this, t);

                    if (result != 0) // If failed to marshal
                    {
                        break;
                    }
                }

                if (result == 0) // If marshalled enumerable ok
                {
                    this.BodyAdd_ArrayEnd(ref arraywriter);
                    result = this.result;

                } // Ends if marshalled enumerable ok
            } // Ends if got subbuilder ok

            return result;
        }

        #region MarshalEnumerable overloads

        public int MarshalEnumerableStruct<T>(IEnumerable<T> ts, MarshalDelegate<T> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, marshal);
        }

        #endregion // MarshalEnumerable overloads

        /// <summary>
        /// Functor for MarshalEnumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private struct EnumerableMarshallerImpl<T>
        {
            private MarshalDelegate<T> marshal;
            private Udbus.Types.dbus_type element;

            public EnumerableMarshallerImpl(MarshalDelegate<T> marshal, Udbus.Types.dbus_type element)
            {
                this.marshal = marshal;
                this.element = element;
            }

            public int Marshal(Udbus.Serialization.UdbusMessageBuilder builder, IEnumerable<T> ts)
            {
                return builder.MarshalEnumerable(ts, this.element, this.marshal);
            }
        } // Ends struct EnumerableMarshallerImpl

        #region EnumerableMarshallerImpl overloads

        static public MarshalDelegate<IEnumerable<T>> EnumerableMarshallerStruct<T>(MarshalDelegate<T> marshal)
        {
            return new EnumerableMarshallerImpl<T>(marshal, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN).Marshal;
        }

        #endregion // EnumerableMarshallerImpl overloads

        #region ArrayMarshaller MarshalDelegate overloads

        static public MarshalDelegate<T[]> ArrayMarshallerStruct<T>(MarshalDelegate<T> marshal)
        {
            return new EnumerableMarshallerImpl<T>(marshal, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN).Marshal;
            //return ArrayMarshaller(marshal);
        }

        #endregion // ArrayMarshaller MarshalDelegate overloads
        #endregion // Enumerable Marshalling (with result)

        #region Struct Marshalling (with result)

        public int MarshalStruct<T>(T t, MarshalDelegate<T> marshal)
        {
            int result = this.BodyAdd_Structure().Result;
            if (result == 0) // If added structure ok
            {
                result = marshal(this, t);

                if (result == 0) // If marshalled ok
                {
                    result = this.BodyAdd_StructureEnd().Result;

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
            private MarshalDelegate<T> marshal;

            public StructMarshallerImpl(MarshalDelegate<T> marshal)
            {
                this.marshal = marshal;
            }

            public int Marshal(Udbus.Serialization.UdbusMessageBuilder builder, T t)
            {
                return builder.MarshalStruct(t, this.marshal);
            }
        } // Ends struct StructMarshallerImpl

        static public MarshalDelegate<T> StructMarshaller<T>(MarshalDelegate<T> marshal)
        {
            return new StructMarshallerImpl<T>(marshal).Marshal;
        }

        public int MarshalStructs<T>(IEnumerable<T> ts, MarshalDelegate<T> marshal)
        {
            return this.MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, StructMarshaller(marshal));
        }
        #endregion // Struct Marshalling (with result)

        #region Dictionary Marshalling (with result)

        /// <summary>
        /// KeyValuePair Marshal functor.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        private struct KeyValuePairMarshallerImpl<TKey, TValue>
        {
            private MarshalDelegate<TKey> marshalKey;
            private MarshalDelegate<TValue> marshalValue;

            public KeyValuePairMarshallerImpl(MarshalDelegate<TKey> marshalKey, MarshalDelegate<TValue> marshalValue)
            {
                this.marshalKey = marshalKey;
                this.marshalValue = marshalValue;
            }

            public int Marshal(Udbus.Serialization.UdbusMessageBuilder builder, KeyValuePair<TKey, TValue> pair)
            {
                int result = this.marshalKey(builder, pair.Key);
                if (result == 0) // If marshalled key ok
                {
                    result =  this.marshalValue(builder, pair.Value);

                } // Ends if marshalled key ok

                return result;
            }
        } // Ends struct KeyValuePairMarshallerImpl

        public int MarshalDict<TKey, TValue>(IDictionary<TKey, TValue[]> dict,
            MarshalDelegate<TKey> marshalKey, MarshalDelegate<IEnumerable<TValue>> marshalValue)
        {
            // Could compose this of other functions...
            // It's an array of structs, where each struct has two elements - the key and the value.
            IDictionary<TKey, IEnumerable<TValue>> id = (IDictionary<TKey, IEnumerable<TValue>>)dict;
            return this.MarshalStructs(id, new KeyValuePairMarshallerImpl<TKey, IEnumerable<TValue>>(marshalKey, marshalValue).Marshal);
        }

        public int MarshalDict<TKey, TValue>(IDictionary<TKey, TValue> dict,
            MarshalDelegate<TKey> marshalKey, MarshalDelegate<TValue> marshalValue)
        {
            // Could compose this of other functions...
            // It's an array of structs, where each struct has two elements - the key and the value.
            return this.MarshalStructs(dict, new KeyValuePairMarshallerImpl<TKey, TValue>(marshalKey, marshalValue).Marshal);
        }

        #endregion // Dictionary Marshalling (with result)
        #endregion // Marshalling (with result)
        #endregion // Ends Marshalling

#if _NOMAGIC
        #region No result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        static public MarshalResultDelegate<IEnumerable<bool>> EnumerableMarshaller(MarshalResultDelegate<bool> marshal)
        {
            return new EnumerableResultMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<byte>> EnumerableMarshaller(MarshalResultDelegate<byte> marshal)
        {
            return new EnumerableResultMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<string>> EnumerableMarshaller(MarshalResultDelegate<string> marshal)
        {
            return new EnumerableResultMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.Int16>> EnumerableMarshaller(MarshalResultDelegate<System.Int16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt16>> EnumerableMarshaller(MarshalResultDelegate<System.UInt16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.Int32>> EnumerableMarshaller(MarshalResultDelegate<System.Int32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt32>> EnumerableMarshaller(MarshalResultDelegate<System.UInt32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.Int64>> EnumerableMarshaller(MarshalResultDelegate<System.Int64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt64>> EnumerableMarshaller(MarshalResultDelegate<System.UInt64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<double>> EnumerableMarshaller(MarshalResultDelegate<double> marshal)
        {
            return new EnumerableResultMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshaller<T>(MarshalResultDelegate<IEnumerable<T>> marshal)
        {
            return new EnumerableResultMarshallerImpl<IEnumerable<T>>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalResultDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshaller<TKey, TValue>(MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableResultMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        #region MarshalEnumerable MarshalResultDelegate overloads

        public void MarshalEnumerable(IEnumerable<bool> ts, MarshalResultDelegate<bool> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BOOLEAN, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<byte> ts, MarshalResultDelegate<byte> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BYTE, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<string> ts, MarshalResultDelegate<string> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_STRING, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<System.Int16> ts, MarshalResultDelegate<System.Int16> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT16, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<System.UInt16> ts, MarshalResultDelegate<System.UInt16> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT16, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<System.Int32> ts, MarshalResultDelegate<System.Int32> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT32, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<System.UInt32> ts, MarshalResultDelegate<System.UInt32> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT32, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<System.Int64> ts, MarshalResultDelegate<System.Int64> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT64, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<System.UInt64> ts, MarshalResultDelegate<System.UInt64> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT64, marshal, out result);
        }

        public void MarshalEnumerable(IEnumerable<double> ts, MarshalResultDelegate<double> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DOUBLE, marshal, out result);
        }

        public void MarshalEnumerable<T>(IEnumerable<IEnumerable<T>> ts, MarshalResultDelegate<IEnumerable<T>> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_ARRAY, marshal, out result);
        }

        public void MarshalEnumerable<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> ts, MarshalResultDelegate<Dictionary<TKey, TValue>> marshal, out int result)
        {
            MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, marshal, out result);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate overloads

        #region ArrayMarshaller MarshalResultDelegate overloads

        static public MarshalResultDelegate<bool[]> ArrayMarshaller(MarshalResultDelegate<bool> marshal)
        {
            return new EnumerableResultMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalResultDelegate<byte[]> ArrayMarshaller(MarshalResultDelegate<byte> marshal)
        {
            return new EnumerableResultMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalResultDelegate<string[]> ArrayMarshaller(MarshalResultDelegate<string> marshal)
        {
            return new EnumerableResultMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalResultDelegate<System.Int16[]> ArrayMarshaller(MarshalResultDelegate<System.Int16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalResultDelegate<System.UInt16[]> ArrayMarshaller(MarshalResultDelegate<System.UInt16> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalResultDelegate<System.Int32[]> ArrayMarshaller(MarshalResultDelegate<System.Int32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalResultDelegate<System.UInt32[]> ArrayMarshaller(MarshalResultDelegate<System.UInt32> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalResultDelegate<System.Int64[]> ArrayMarshaller(MarshalResultDelegate<System.Int64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalResultDelegate<System.UInt64[]> ArrayMarshaller(MarshalResultDelegate<System.UInt64> marshal)
        {
            return new EnumerableResultMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalResultDelegate<double[]> ArrayMarshaller(MarshalResultDelegate<double> marshal)
        {
            return new EnumerableResultMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalResultDelegate<T[][]> ArrayMarshaller<T>(MarshalResultDelegate<T[]> marshal)
        {
            return new EnumerableResultMarshallerImpl<T[]>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalResultDelegate<Dictionary<TKey, TValue>[]> ArrayMarshaller<TKey, TValue>(MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableResultMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // ArrayMarshaller MarshalResultDelegate overloads
        #region Explicit no result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        static public MarshalResultDelegate<IEnumerable<bool>> EnumerableMarshallerBoolean(MarshalResultDelegate<bool> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<byte>> EnumerableMarshallerByte(MarshalResultDelegate<byte> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<string>> EnumerableMarshallerString(MarshalResultDelegate<string> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.Int16>> EnumerableMarshallerInt16(MarshalResultDelegate<System.Int16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt16>> EnumerableMarshallerUInt16(MarshalResultDelegate<System.UInt16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.Int32>> EnumerableMarshallerInt32(MarshalResultDelegate<System.Int32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt32>> EnumerableMarshallerUInt32(MarshalResultDelegate<System.UInt32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.Int64>> EnumerableMarshallerInt64(MarshalResultDelegate<System.Int64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<System.UInt64>> EnumerableMarshallerUInt64(MarshalResultDelegate<System.UInt64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<double>> EnumerableMarshallerDouble(MarshalResultDelegate<double> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshallerArray<T>(MarshalResultDelegate<IEnumerable<T>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalResultDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshallerDictionary<TKey, TValue>(MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        #region MarshalEnumerable MarshalResultDelegate explicit overloads

        public void MarshalEnumerableBoolean(IEnumerable<bool> ts, MarshalResultDelegate<bool> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableByte(IEnumerable<byte> ts, MarshalResultDelegate<byte> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableString(IEnumerable<string> ts, MarshalResultDelegate<string> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableInt16(IEnumerable<System.Int16> ts, MarshalResultDelegate<System.Int16> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableUInt16(IEnumerable<System.UInt16> ts, MarshalResultDelegate<System.UInt16> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableInt32(IEnumerable<System.Int32> ts, MarshalResultDelegate<System.Int32> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableUInt32(IEnumerable<System.UInt32> ts, MarshalResultDelegate<System.UInt32> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableInt64(IEnumerable<System.Int64> ts, MarshalResultDelegate<System.Int64> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableUInt64(IEnumerable<System.UInt64> ts, MarshalResultDelegate<System.UInt64> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableDouble(IEnumerable<double> ts, MarshalResultDelegate<double> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableArray<T>(IEnumerable<IEnumerable<T>> ts, MarshalResultDelegate<IEnumerable<T>> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        public void MarshalEnumerableDictionary<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> ts, MarshalResultDelegate<Dictionary<TKey, TValue>> marshal, out int result)
        {
            MarshalEnumerable(ts, marshal, out result);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate explicit overloads

        #region MarshalEnumerable MarshalResultDelegate explicit overloads

        static public MarshalResultDelegate<bool[]> ArrayMarshallerBoolean(MarshalResultDelegate<bool> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<byte[]> ArrayMarshallerByte(MarshalResultDelegate<byte> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<string[]> ArrayMarshallerString(MarshalResultDelegate<string> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.Int16[]> ArrayMarshallerInt16(MarshalResultDelegate<System.Int16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.UInt16[]> ArrayMarshallerUInt16(MarshalResultDelegate<System.UInt16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.Int32[]> ArrayMarshallerInt32(MarshalResultDelegate<System.Int32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.UInt32[]> ArrayMarshallerUInt32(MarshalResultDelegate<System.UInt32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.Int64[]> ArrayMarshallerInt64(MarshalResultDelegate<System.Int64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<System.UInt64[]> ArrayMarshallerUInt64(MarshalResultDelegate<System.UInt64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<double[]> ArrayMarshallerDouble(MarshalResultDelegate<double> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<T[][]> ArrayMarshallerArray<T>(MarshalResultDelegate<T[]> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalResultDelegate<Dictionary<TKey, TValue>[]> ArrayMarshallerDictionary<TKey, TValue>(MarshalResultDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        #endregion // MarshalEnumerable MarshalResultDelegate explicit overloads
        #endregion // Explicit no result marshalling functions
        #endregion // No result marshalling functions
        #region Return result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        static public MarshalDelegate<IEnumerable<bool>> EnumerableMarshaller(MarshalDelegate<bool> marshal)
        {
            return new EnumerableMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalDelegate<IEnumerable<byte>> EnumerableMarshaller(MarshalDelegate<byte> marshal)
        {
            return new EnumerableMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalDelegate<IEnumerable<string>> EnumerableMarshaller(MarshalDelegate<string> marshal)
        {
            return new EnumerableMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.Int16>> EnumerableMarshaller(MarshalDelegate<System.Int16> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.UInt16>> EnumerableMarshaller(MarshalDelegate<System.UInt16> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.Int32>> EnumerableMarshaller(MarshalDelegate<System.Int32> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.UInt32>> EnumerableMarshaller(MarshalDelegate<System.UInt32> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.Int64>> EnumerableMarshaller(MarshalDelegate<System.Int64> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalDelegate<IEnumerable<System.UInt64>> EnumerableMarshaller(MarshalDelegate<System.UInt64> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalDelegate<IEnumerable<double>> EnumerableMarshaller(MarshalDelegate<double> marshal)
        {
            return new EnumerableMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshaller<T>(MarshalDelegate<IEnumerable<T>> marshal)
        {
            return new EnumerableMarshallerImpl<IEnumerable<T>>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshaller<TKey, TValue>(MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller overloads

        #region MarshalEnumerable MarshalDelegate overloads

        public int MarshalEnumerable(IEnumerable<bool> ts, MarshalDelegate<bool> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BOOLEAN, marshal);
        }

        public int MarshalEnumerable(IEnumerable<byte> ts, MarshalDelegate<byte> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_BYTE, marshal);
        }

        public int MarshalEnumerable(IEnumerable<string> ts, MarshalDelegate<string> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_STRING, marshal);
        }

        public int MarshalEnumerable(IEnumerable<System.Int16> ts, MarshalDelegate<System.Int16> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT16, marshal);
        }

        public int MarshalEnumerable(IEnumerable<System.UInt16> ts, MarshalDelegate<System.UInt16> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT16, marshal);
        }

        public int MarshalEnumerable(IEnumerable<System.Int32> ts, MarshalDelegate<System.Int32> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT32, marshal);
        }

        public int MarshalEnumerable(IEnumerable<System.UInt32> ts, MarshalDelegate<System.UInt32> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT32, marshal);
        }

        public int MarshalEnumerable(IEnumerable<System.Int64> ts, MarshalDelegate<System.Int64> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_INT64, marshal);
        }

        public int MarshalEnumerable(IEnumerable<System.UInt64> ts, MarshalDelegate<System.UInt64> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_UINT64, marshal);
        }

        public int MarshalEnumerable(IEnumerable<double> ts, MarshalDelegate<double> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DOUBLE, marshal);
        }

        public int MarshalEnumerable<T>(IEnumerable<IEnumerable<T>> ts, MarshalDelegate<IEnumerable<T>> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_ARRAY, marshal);
        }

        public int MarshalEnumerable<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> ts, MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return MarshalEnumerable(ts, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, marshal);
        }

        #endregion // MarshalEnumerable MarshalDelegate overloads

        #region ArrayMarshaller MarshalDelegate overloads

        static public MarshalDelegate<bool[]> ArrayMarshaller(MarshalDelegate<bool> marshal)
        {
            return new EnumerableMarshallerImpl<bool>(marshal, Udbus.Types.dbus_type.DBUS_BOOLEAN).Marshal;
        }

        static public MarshalDelegate<byte[]> ArrayMarshaller(MarshalDelegate<byte> marshal)
        {
            return new EnumerableMarshallerImpl<byte>(marshal, Udbus.Types.dbus_type.DBUS_BYTE).Marshal;
        }

        static public MarshalDelegate<string[]> ArrayMarshaller(MarshalDelegate<string> marshal)
        {
            return new EnumerableMarshallerImpl<string>(marshal, Udbus.Types.dbus_type.DBUS_STRING).Marshal;
        }

        static public MarshalDelegate<System.Int16[]> ArrayMarshaller(MarshalDelegate<System.Int16> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int16>(marshal, Udbus.Types.dbus_type.DBUS_INT16).Marshal;
        }

        static public MarshalDelegate<System.UInt16[]> ArrayMarshaller(MarshalDelegate<System.UInt16> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt16>(marshal, Udbus.Types.dbus_type.DBUS_UINT16).Marshal;
        }

        static public MarshalDelegate<System.Int32[]> ArrayMarshaller(MarshalDelegate<System.Int32> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int32>(marshal, Udbus.Types.dbus_type.DBUS_INT32).Marshal;
        }

        static public MarshalDelegate<System.UInt32[]> ArrayMarshaller(MarshalDelegate<System.UInt32> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt32>(marshal, Udbus.Types.dbus_type.DBUS_UINT32).Marshal;
        }

        static public MarshalDelegate<System.Int64[]> ArrayMarshaller(MarshalDelegate<System.Int64> marshal)
        {
            return new EnumerableMarshallerImpl<System.Int64>(marshal, Udbus.Types.dbus_type.DBUS_INT64).Marshal;
        }

        static public MarshalDelegate<System.UInt64[]> ArrayMarshaller(MarshalDelegate<System.UInt64> marshal)
        {
            return new EnumerableMarshallerImpl<System.UInt64>(marshal, Udbus.Types.dbus_type.DBUS_UINT64).Marshal;
        }

        static public MarshalDelegate<double[]> ArrayMarshaller(MarshalDelegate<double> marshal)
        {
            return new EnumerableMarshallerImpl<double>(marshal, Udbus.Types.dbus_type.DBUS_DOUBLE).Marshal;
        }

        static public MarshalDelegate<T[][]> ArrayMarshaller<T>(MarshalDelegate<T[]> marshal)
        {
            return new EnumerableMarshallerImpl<T[]>(marshal, Udbus.Types.dbus_type.DBUS_ARRAY).Marshal;
        }

        static public MarshalDelegate<Dictionary<TKey, TValue>[]> ArrayMarshaller<TKey, TValue>(MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return new EnumerableMarshallerImpl<Dictionary<TKey, TValue>>(marshal, Udbus.Types.dbus_type.DBUS_DICT_BEGIN).Marshal;
        }

        #endregion // ArrayMarshaller MarshalDelegate overloads
        #region Explicit return result marshalling functions
        #region EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        static public MarshalDelegate<IEnumerable<bool>> EnumerableMarshallerBoolean(MarshalDelegate<bool> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<byte>> EnumerableMarshallerByte(MarshalDelegate<byte> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<string>> EnumerableMarshallerString(MarshalDelegate<string> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.Int16>> EnumerableMarshallerInt16(MarshalDelegate<System.Int16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.UInt16>> EnumerableMarshallerUInt16(MarshalDelegate<System.UInt16> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.Int32>> EnumerableMarshallerInt32(MarshalDelegate<System.Int32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.UInt32>> EnumerableMarshallerUInt32(MarshalDelegate<System.UInt32> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.Int64>> EnumerableMarshallerInt64(MarshalDelegate<System.Int64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<System.UInt64>> EnumerableMarshallerUInt64(MarshalDelegate<System.UInt64> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<double>> EnumerableMarshallerDouble(MarshalDelegate<double> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<IEnumerable<T>>> EnumerableMarshallerArray<T>(MarshalDelegate<IEnumerable<T>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        static public MarshalDelegate<IEnumerable<Dictionary<TKey, TValue>>> EnumerableMarshallerDictionary<TKey, TValue>(MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return EnumerableMarshaller(marshal);
        }

        #endregion // EnumerableResultMarshallerImpl EnumerableMarshaller explicit overloads

        #region MarshalEnumerable MarshalDelegate explicit overloads

        public int MarshalEnumerableBoolean(IEnumerable<bool> ts, MarshalDelegate<bool> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableByte(IEnumerable<byte> ts, MarshalDelegate<byte> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableString(IEnumerable<string> ts, MarshalDelegate<string> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableInt16(IEnumerable<System.Int16> ts, MarshalDelegate<System.Int16> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableUInt16(IEnumerable<System.UInt16> ts, MarshalDelegate<System.UInt16> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableInt32(IEnumerable<System.Int32> ts, MarshalDelegate<System.Int32> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableUInt32(IEnumerable<System.UInt32> ts, MarshalDelegate<System.UInt32> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableInt64(IEnumerable<System.Int64> ts, MarshalDelegate<System.Int64> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableUInt64(IEnumerable<System.UInt64> ts, MarshalDelegate<System.UInt64> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableDouble(IEnumerable<double> ts, MarshalDelegate<double> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableArray<T>(IEnumerable<IEnumerable<T>> ts, MarshalDelegate<IEnumerable<T>> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        public int MarshalEnumerableDictionary<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> ts, MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return MarshalEnumerable(ts, marshal);
        }

        #endregion // MarshalEnumerable MarshalDelegate explicit overloads

        #region MarshalEnumerable MarshalDelegate explicit overloads

        static public MarshalDelegate<bool[]> ArrayMarshallerBoolean(MarshalDelegate<bool> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<byte[]> ArrayMarshallerByte(MarshalDelegate<byte> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<string[]> ArrayMarshallerString(MarshalDelegate<string> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.Int16[]> ArrayMarshallerInt16(MarshalDelegate<System.Int16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.UInt16[]> ArrayMarshallerUInt16(MarshalDelegate<System.UInt16> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.Int32[]> ArrayMarshallerInt32(MarshalDelegate<System.Int32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.UInt32[]> ArrayMarshallerUInt32(MarshalDelegate<System.UInt32> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.Int64[]> ArrayMarshallerInt64(MarshalDelegate<System.Int64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<System.UInt64[]> ArrayMarshallerUInt64(MarshalDelegate<System.UInt64> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<double[]> ArrayMarshallerDouble(MarshalDelegate<double> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<T[][]> ArrayMarshallerArray<T>(MarshalDelegate<T[]> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        static public MarshalDelegate<Dictionary<TKey, TValue>[]> ArrayMarshallerDictionary<TKey, TValue>(MarshalDelegate<Dictionary<TKey, TValue>> marshal)
        {
            return ArrayMarshaller(marshal);
        }

        #endregion // MarshalEnumerable MarshalDelegate explicit overloads
        #endregion // Explicit return result marshalling functions
        #endregion // Return result marshalling functions
#endif // !_NOMAGIC
    } // Ends class UdbusMessageBuilder

    /// <summary>
    /// Constructs messages and keeps track of serial number.
    /// </summary>
    public class UdbusMessageBuilderTracker : UdbusMessageBuilder
    {
        protected UInt32 serial = 0;

        public UInt32 MostRecentSerialNumber
        {
            get { return this.serial; }
        }

        public UdbusMessageBuilderTracker() : base()
        {
        }

        public UdbusMessageBuilderTracker(UInt32 serial)
            : base()
        {
            this.serial = serial;
        }

        /// Message construction.
        public UdbusMessageBuilderTracker UdbusMessage()
        {
            base.UdbusMessage(++this.serial);
            return this;
        }

        public UdbusMessageBuilderTracker UdbusMethodMessage(
            string destination, string path,
            string interface_, string method)
        {
            base.UdbusMethodMessage(++this.serial, destination, path, interface_, method);
            return this;
        }

        public UdbusMessageBuilderTracker UdbusSignalMessage(
            string path, string interface_, string name)
        {
            base.UdbusSignalMessage(++this.serial, path, interface_, name);
            return this;
        }

        protected UdbusMessageBuilderTracker(NMessage.UdbusMessageHandle message)
            : base(message)
        {
        }

        protected override UdbusMessageBuilder WrapMessage(NMessage.UdbusMessageHandle message)
        {
            return new UdbusMessageBuilderTracker(message);
        }


    } // Ends UdbusMessageBuilderTracker
} // Ends namespace Udbus.Serialization
