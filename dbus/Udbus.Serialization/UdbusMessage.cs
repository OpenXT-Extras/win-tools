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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Udbus.Serialization
{
    namespace NMessageHandle
    {
        public class UdbusMessageHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private UdbusMessageHandle() : base(true) { }
            protected override bool ReleaseHandle()
            {
                UdbusFunctions.dbus_msg_free(this.handle);
                return true;
            }


            /// <summary>
            /// Marshal a handle of this type into a structure.
            /// The IntPtr is supposed to be hidden from sight so this seems to be the only way forward.
            /// Not sure that referencing this.handle anywhere but "ReleaseHandle()" is permitted,
            /// but seriously how else is this supposed to get accomplished ?
            /// </summary>
            /// <param name="message"></param>
            public void HandleToStructure(out NMessageStruct.UdbusMessageHandle message)
            {
                message = this.HandleToStructure();
            }

            public NMessageStruct.UdbusMessageHandle HandleToStructure()
            {
                return (NMessageStruct.UdbusMessageHandle)Marshal.PtrToStructure(this.handle, typeof(NMessageStruct.UdbusMessageHandle));
            }

            static public NMessageHandle.UdbusMessageHandle StructureToHandle(NMessageStruct.UdbusMessageHandle msgStruct)
            {
                UdbusMessageBuilder builder = new UdbusMessageBuilder();
                builder.UdbusMessage(msgStruct.serial);

                string path = msgStruct.path;
                string destination = msgStruct.destination;
                string interface_ = msgStruct.interface_;
                string error_name = msgStruct.error_name;
                string method = msgStruct.method;
                string sender = msgStruct.sender;

                msgStruct.path = msgStruct.destination = msgStruct.interface_ = msgStruct.error_name = msgStruct.method = msgStruct.sender = null;

                Marshal.StructureToPtr(msgStruct, builder.Message.handle, false);

                builder
                    .SetPath(path)
                    .SetDestination(destination)
                    .SetInterface(interface_)
                    .SetErrorName(error_name)
                    .SetMethod(method)
                    .SetSender(sender)
                    ;

                return builder.Message;
            }

            public static UdbusMessageHandle Initialiser
            {
                get { return null; }
            }

            public const UdbusMessageHandle EmptyHandle = null;

            /// <summary>
            /// This is going to mess up any ref counting,
            /// but allows a handle that's due for disposal to give up its resource.
            /// </summary>
            /// <returns>New handle holding the released resource.</returns>
            public NMessageHandle.UdbusMessageHandle Release()
            {
                NMessageHandle.UdbusMessageHandle copy = new UdbusMessageHandle();
                if (this.IsClosed || this.IsInvalid) // If not a valid handle
                {
                    copy.handle = this.handle;
                }
                else
                {
                    IntPtr handleTemp = this.handle;
                    base.SetHandleAsInvalid();
                    this.handle = IntPtr.Zero;
                    copy.SetHandle(handleTemp);
                }
                return copy;
            }

        } // Ends class UdbusMessageHandle
    } // Ends namespace NMessageHandle

    namespace NMessageStruct
    {
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        //public struct dbus_msg
        public struct UdbusMessageHandle
        {
            [StructLayout(LayoutKind.Explicit)]
            public struct Stypefield
            {
                [FieldOffset(0)]
                //public byte type;
                public Udbus.Types.dbus_msg_type type;
                [FieldOffset(0)]
                public int _dummy;	 // For padding.
            };
            public Stypefield typefield;
            public UInt32 serial;
            [MarshalAs(UnmanagedType.LPStr)]
            public String destination;
            [MarshalAs(UnmanagedType.LPStr)]
            public String path;
            [MarshalAs(UnmanagedType.LPStr)]
            public String interface_; // TODO, correct name
            [MarshalAs(UnmanagedType.LPStr)]
            public String method;
            [MarshalAs(UnmanagedType.LPStr)]
            public String error_name;
            [MarshalAs(UnmanagedType.LPStr)]
            public String sender;
            public Udbus.Types.dbus_sig signature;
            public UInt32 reply_serial;
            public int w;
            //byte *body;
            public IntPtr body;
            [StructLayout(LayoutKind.Explicit)]
            public struct Scomms
            {
                [FieldOffset(0)]
                public dbus_writer writer; /* writing body */
                [FieldOffset(0)]
                public dbus_reader reader; /* reading body */
            };
            public Scomms comms;

            public static UdbusMessageHandle Initialiser
            {
                get { return default(UdbusMessageHandle); }
            }

            public override string ToString()
            {
                StringBuilder result = new StringBuilder(base.ToString());
                Udbus.Types.dbus_msg_type type = (Udbus.Types.dbus_msg_type)this.typefield.type;
                result.AppendFormat(". serial: {0}", this.serial);
                result.AppendFormat(". reply_serial: {0}", this.reply_serial);
                result.AppendFormat(". type: {0}", type);
                result.AppendFormat(". destination: {0}", this.destination ?? "<No Destination>");
                result.AppendFormat(". path: {0}", this.path ?? "<No Path>");
                result.AppendFormat(". interface: {0}", this.interface_ ?? "<No Interface>");
                result.AppendFormat(". method: {0}", this.method ?? "<No Method>");
                result.AppendFormat(". error_name: {0}", this.error_name ?? "<No Error Name>");
                result.AppendFormat(". sender: {0}", this.sender ?? "<No Sender>");
                result.AppendFormat(". signature: {0}", this.signature);
                result.Append(".");
                return result.ToString();
            }

        }; // Ends struct UdbusMessageHandle

    } // Ends namespace NMessageStruct

} // Ends namespace Udbus.Serialization
