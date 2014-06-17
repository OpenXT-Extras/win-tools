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

#define _NASTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Udbus.Serialization
{
    internal static class Constants
    {
#if DEBUG
        public const string udbusname = "udbus_d.dll";
#else // !DEBUG
        public const string udbusname = "udbus.dll";
#endif // DEBUG
    }
    //public class UdbusDelegates
    //{
    //    ///* return 0 if succeed to write count bytes, non-0 otherwise */
    //    //int (*io_write)(void *priv, const void *buf, uint32_t count);
    //    ///* return 0 if succeed to read count bytes, non-0 otherwise */
    //    //int (*io_read)(void *priv, void *buf, uint32_t count);
    //    ///* debugging logging */
    //    //void (*io_debug)(void *logpriv, const char *buf);

    //    ///* return 0 if succeed to write count bytes, non-0 otherwise */
    //    public delegate int D_io_write(IntPtr priv, String buf, System.UInt32 count);
    //    /* return 0 if succeed to read count bytes, non-0 otherwise */
    //    public delegate int D_io_read(IntPtr priv, byte[] buf, System.UInt32 count);
    //    /* debugging logging */
    //    public delegate void D_io_debug(IntPtr logpriv, String buf);


    //} // Ends class UdbusDelegates

    //[StructLayout(LayoutKind.Sequential)]
    //public struct ManagedDbusIo
    //{
      
    //  //[MarshalAs(UnmanagedType.I4)]
    //  //int data;
    //  [MarshalAs(UnmanagedType.FunctionPtr)]
    //  UdbusDelegates.D_io_write io_write;

    //  [MarshalAs(UnmanagedType.FunctionPtr)]
    //  UdbusDelegates.D_io_read io_read;

    //  [MarshalAs(UnmanagedType.FunctionPtr)]
    //  UdbusDelegates.D_io_debug io_debug;

    //  IntPtr priv;
    //  IntPtr logpriv;
    //};

    public static class UdbusFunctions
    {
        /// <summary>
        /// Authorisation over comms layer. Just actually use dbus format.
        /// </summary>
        /// <param name="dbus_io">Udbus IO functions.</param>
        /// <param name="auth">Authorisation string.</param>
        /// <returns>0 if successful, otherwise non-zero error code.</returns>
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static int dbus_auth(ref Udbus.Serialization.ManagedDbusIo dbus_io, string auth);

        // Message destructor function.
        [DllImport(Constants.udbusname)]
        internal extern static void dbus_msg_free(IntPtr msg);

        [DllImport(Constants.udbusname)]
        internal extern static void dbus_msg_body_free_string(IntPtr val);

        [DllImport(Constants.udbusname)]
        internal extern static void dbus_msg_body_free_object_path(IntPtr val);

        /// <summary>
        /// Base class implementation for specific subclasses wrapping strings owned by unmanaged code.
        /// Class instance can be created by ClassName.Create(NameOfUnmanagedFunction, msg).
        /// Sub-class will override ReleaseHandle to provide suitable unmanaged memory clearup.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class UdbusMessageBodyStringHandleBase<T> : SafeHandleZeroOrMinusOneIsInvalid
            where T : UdbusMessageBodyStringHandleBase<T>
        {
            protected string value;
            private bool unmarshalled = false;

            /// Construction
            protected UdbusMessageBodyStringHandleBase() : base(true) { }

            /// <summary>
            /// Create a new handle by calling a function.
            /// </summary>
            /// <typeparam name="TParam1">Parameter type to creation function.</typeparam>
            /// <typeparam name="TReturnType">Return type of creation function.</typeparam>
            /// <param name="dMakeIt">Function used to construct handle.</param>
            /// <param name="p1">Parameter to construction function.</param>
            /// <returns>New handle.</returns>
            public delegate TReturnType DMakeIt<TParam1, TReturnType>(TParam1 p1, out T h);
            static public T Create<TParam1, TReturnType>(DMakeIt<TParam1, TReturnType> dMakeIt, TParam1 p1)
            {
                TReturnType result;
                return Create(dMakeIt, p1, out result);
            }

            /// <summary>
            /// Create a new handle by calling a function, and provide the result.
            /// </summary>
            /// <typeparam name="TParam1">Parameter type to creation function.</typeparam>
            /// <typeparam name="TReturnType">Return type of creation function.</typeparam>
            /// <param name="dMakeIt">Function used to construct handle.</param>
            /// <param name="p1">Parameter to construction function.</param>
            /// <param name="result">Result of construction function.</param>
            /// <returns>New handle.</returns>
            static public T Create<TParam1, TReturnType>(DMakeIt<TParam1, TReturnType> dMakeIt, TParam1 p1, out TReturnType result)
            {
                T hCreate;
                result = dMakeIt(p1, out hCreate);
                return hCreate;
            }

            /// Properties.
            public string Value
            {
                get
                {
                    if (this.unmarshalled == false && this.IsClosed == false && this.IsInvalid == false && this.handle != IntPtr.Zero)
                    {
                        this.value = Marshal.PtrToStringAnsi(this.handle);
                        this.unmarshalled = true;
                    }
                    return this.value;
                }
            }

            /// String conversion.
            public override string ToString()
            {
                return this.Value;
            }

            // Would have liked to make this generic, but conversion must be for defining type. Bleh.
            public static implicit operator string(UdbusMessageBodyStringHandleBase<T> t)
            {
                return t.Value;
            }

            /// <summary>
            ///  Initializer for class.
            /// </summary>
            public static T Initialiser
            {
                get { return null; }
            }
        } // Ends class UdbusMessageBodyStringHandleBase

        public class UdbusMessageBodyObjectPathHandle : UdbusMessageBodyStringHandleBase<UdbusMessageBodyObjectPathHandle>
        {
            protected override bool ReleaseHandle()
            {
                if (!this.IsInvalid)
                {
                    UdbusFunctions.dbus_msg_body_free_object_path(this.handle);
                }
                return true;
            }
        } // Ends class UdbusMessageBodyObjectPathHandle

        public class UdbusMessageBodyStringHandle : UdbusMessageBodyStringHandleBase<UdbusMessageBodyStringHandle>
        {
            protected override bool ReleaseHandle()
            {
                if (!this.IsInvalid)
                {
                    UdbusFunctions.dbus_msg_body_free_string(this.handle);
                }

                return true;
            }
        } // Ends class UdbusMessageBodyStringHandle

        /// Message body writing functions.
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_destination(NMessageHandle.UdbusMessageHandle msg, string destination);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_path(NMessageHandle.UdbusMessageHandle msg, string path);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_method(NMessageHandle.UdbusMessageHandle msg, string method);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_error_name(NMessageHandle.UdbusMessageHandle msg, string error_name);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_sender(NMessageHandle.UdbusMessageHandle msg, string sender);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_interface(NMessageHandle.UdbusMessageHandle msg, string interface_);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static void dbus_msg_set_signature(NMessageHandle.UdbusMessageHandle msg, ref Udbus.Types.dbus_sig signature);

        /// Message body adding functions.
        /// <summary>
        /// Create a body buffer of specified length.
        /// BEWARE: need to be called before adding any elements to the body.
        /// </summary>
        /// <param name="msg">Message to add body to.</param>
        /// <param name="length">Length of body.</param>
        /// <returns>Zero if successful, otherwise non-zero.</returns>
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static int dbus_msg_body_add(NMessageHandle.UdbusMessageHandle msg, UInt32 length);

        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_byte       (NMessageHandle.UdbusMessageHandle msg, byte val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_boolean    (NMessageHandle.UdbusMessageHandle msg, bool val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_int16      (NMessageHandle.UdbusMessageHandle msg, Int16 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_uint16     (NMessageHandle.UdbusMessageHandle msg, UInt16 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_int32      (NMessageHandle.UdbusMessageHandle msg, Int32 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_uint32     (NMessageHandle.UdbusMessageHandle msg, UInt32 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_int64      (NMessageHandle.UdbusMessageHandle msg, Int64 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_uint64     (NMessageHandle.UdbusMessageHandle msg, UInt64 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_double     (NMessageHandle.UdbusMessageHandle msg, double val);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static int dbus_msg_body_add_string     (NMessageHandle.UdbusMessageHandle msg, string val);
        [DllImport(Constants.udbusname, CharSet = CharSet.Ansi)]
        internal extern static int dbus_msg_body_add_objectpath (NMessageHandle.UdbusMessageHandle msg, string val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_array_begin(NMessageHandle.UdbusMessageHandle msg, Udbus.Types.dbus_type element, ref dbus_array_writer ptr);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_array_end  (NMessageHandle.UdbusMessageHandle msg, ref dbus_array_writer ptr);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_structure  (NMessageHandle.UdbusMessageHandle msg);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_add_variant    (NMessageHandle.UdbusMessageHandle msg, ref Udbus.Types.dbus_sig signature);

        /// Messsage body reading functions.
        [DllImport(Constants.udbusname)]
        internal extern static  int dbus_msg_body_get_byte       (NMessageHandle.UdbusMessageHandle msg, out byte val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_boolean     (NMessageHandle.UdbusMessageHandle msg, out bool val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_int16       (NMessageHandle.UdbusMessageHandle msg, out Int16 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_uint16      (NMessageHandle.UdbusMessageHandle msg, out UInt16 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_int32       (NMessageHandle.UdbusMessageHandle msg, out Int32 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_uint32      (NMessageHandle.UdbusMessageHandle msg, out UInt32 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_int64       (NMessageHandle.UdbusMessageHandle msg, out Int64 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_uint64      (NMessageHandle.UdbusMessageHandle msg, out UInt64 val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_double      (NMessageHandle.UdbusMessageHandle msg, out double val);

        [DllImport(Constants.udbusname)]
        internal extern static  int dbus_msg_body_get_string      (NMessageHandle.UdbusMessageHandle msg, out UdbusMessageBodyStringHandle val);
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_object_path  (NMessageHandle.UdbusMessageHandle msg, out UdbusMessageBodyObjectPathHandle val);

        [DllImport(Constants.udbusname)]
        internal extern static  int dbus_msg_body_get_structure   (NMessageHandle.UdbusMessageHandle msg);
        [DllImport(Constants.udbusname)]
        internal extern static  int dbus_msg_body_get_variant     (NMessageHandle.UdbusMessageHandle msg, ref Udbus.Types.dbus_sig signature);

        /// Message body array reading functions.
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_body_get_array          (NMessageHandle.UdbusMessageHandle msg, Udbus.Types.dbus_type element, ref dbus_array_reader ar);

        [DllImport(Constants.udbusname)]
        internal extern static UInt32 dbus_msg_body_get_array_left  (NMessageHandle.UdbusMessageHandle msg, ref dbus_array_reader ar);

    } // Ends class UdbusFunctions

    public static class UdbusMsgHandleFunctions
    {
        // Input, output.
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_send(ref Udbus.Serialization.ManagedDbusIo dbus_io, NMessageHandle.UdbusMessageHandle msg);

        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_recv(ref Udbus.Serialization.ManagedDbusIo dbus_io, out NMessageHandle.UdbusMessageHandle msg);

        // Message constructor functions.
        [DllImport(Constants.udbusname, EntryPoint = "dbus_msg_new")]
        internal extern static NMessageHandle.UdbusMessageHandle UdbusMessage(UInt32 serial);

        [DllImport(Constants.udbusname, EntryPoint = "dbus_msg_new_method_call", CharSet = CharSet.Ansi)]
        internal extern static NMessageHandle.UdbusMessageHandle UdbusMethodMessage(UInt32 serial,
            string destination, string path,
            string interface_, string method
        );

        [DllImport(Constants.udbusname, EntryPoint = "dbus_msg_new_signal", CharSet = CharSet.Ansi)]
        internal extern static NMessageHandle.UdbusMessageHandle UdbusSignalMessage(UInt32 serial,
            string path, string interface_, string name
        );

        // Message destructor function.
        [DllImport(Constants.udbusname)]
        internal extern static void dbus_msg_free(IntPtr msg);

    } // Ends class UdbusMsgHandleFunctions

    public static class UdbusMsgStructFunctions
    {
        // Input, output.
        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_send(ref Udbus.Serialization.ManagedDbusIo dbus_io, NMessageStruct.UdbusMessageHandle msg);

        [DllImport(Constants.udbusname)]
        internal extern static int dbus_msg_recv(ref Udbus.Serialization.ManagedDbusIo dbus_io, out NMessageStruct.UdbusMessageHandle msg);

        // Message constructor functions.
        [DllImport(Constants.udbusname, EntryPoint = "dbus_msg_new")]
        [return: MarshalAs(UnmanagedType.LPStruct)]
        internal extern static NMessageStruct.UdbusMessageHandle UdbusMessage(UInt32 serial);

        [DllImport(Constants.udbusname, EntryPoint = "dbus_msg_new_method_call", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStruct)]
        internal extern static NMessageStruct.UdbusMessageHandle UdbusMethodMessage(UInt32 serial,
            string destination, string path,
            string interface_, string method
        );

        [DllImport(Constants.udbusname, EntryPoint = "dbus_msg_new_signal", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStruct)]
        internal extern static NMessageStruct.UdbusMessageHandle UdbusSignalMessage(UInt32 serial,
            string path, string interface_, string name
        );

        // Message destructor function.
        [DllImport(Constants.udbusname)]
        internal extern static void dbus_msg_free(IntPtr msg);

    } // Ends class UdbusMsgStructFunctions
} // Ends namespace Udbus.Serialization
