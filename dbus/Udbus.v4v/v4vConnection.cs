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

namespace Udbus.v4v
{
    internal static class Constants
    {
#if DEBUG
        public const string udbusv4vname = "udbus_v4v_d.dll";
        public const string v4vdllname = "v4vio_d.dll";
#else // !DEBUG
        public const string udbusv4vname = "udbus_v4v.dll";
        public const string v4vdllname = "v4vio.dll";
#endif // DEBUG
        public const UInt32 WAIT_FAILED = 0xFFFFFFFF;
        public const UInt32 WAIT_TIMEOUT = 258; // See WinError.h
    }

    internal class v4vConnectionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private v4vConnectionHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            // Seems like when disposing, IsClosed is true, even though we're in the middle of releasing...
            bool released = this.IsInvalid;
            if (!released)
            {
                released = v4vConnectionFunctions.disconnect_v4v_socket(this.handle);

            }

            return released;
        }
    } // Ends class v4vConnectionHandle

    internal static class v4vConnectionFunctions
    {
        [DllImport(Constants.v4vdllname)]
        // _UDBUS_V4V_API TransportData * _UDBUS_V4V_CALLCONV connect_v4v_socket(void (_UDBUS_V4V_CALLCONV *io_debug)(void *logpriv, const char *buf)=NULL);
        //internal extern static IntPtr connect_v4v_socket(IntPtr io_debug);
        internal extern static v4vConnectionHandle connect_v4v_socket(Udbus.Serialization.UdbusDelegates.D_io_debug io_debug);
        // _UDBUS_V4V_API bool _UDBUS_V4V_CALLCONV disconnect_v4v_socket(TransportData *ptransportdata);
        [DllImport(Constants.v4vdllname)]
        internal extern static bool disconnect_v4v_socket(IntPtr ptransportdata);

        [DllImport(Constants.v4vdllname)]
        internal extern static bool cancel_v4v_socket_io(v4vConnectionHandle ptransportdata);

        //[DllImport(Constants.v4vdllname)]
        //internal extern static UInt32 wait_for_v4v_read_handles(v4vConnectionHandle ptransportdata, String ErrorMessage);

        [DllImport(Constants.v4vdllname)]
        internal extern static UInt32 wait_for_v4v_read_handles_timeout(UInt32 dwMilliseconds, v4vConnectionHandle ptransportdata, String ErrorMessage);

        [DllImport(Constants.udbusv4vname)]
        internal extern static bool populate_dbus_io(ref Udbus.Serialization.ManagedDbusIo pdbus_io, v4vConnectionHandle ptransportdata);
    } // Ends class v4vConnectionFunctions

#if STOP_GARBAGE_COLLECTING_MY_DELEGATES
    internal class v4vConnectionDelegates
    {
        #region Delegates
        internal delegate v4vConnectionHandle connect_v4v_socket_delegate(UdbusDelegates.D_io_debug io_debug);
        internal delegate bool disconnect_v4v_socket_delegate(IntPtr ptransportdata);
        internal delegate bool cancel_v4v_socket_io_delegate(v4vConnectionHandle ptransportdata);
        //internal delegate UInt32 wait_for_v4v_read_handles_delegate(v4vConnectionHandle ptransportdata, String ErrorMessage);
        internal delegate UInt32 wait_for_v4v_read_handles_timeout_delegate(UInt32 dwMilliseconds, v4vConnectionHandle ptransportdata, String ErrorMessage);
        internal delegate bool populate_dbus_io_delegate(out Udbus.Serialization.ManagedDbusIo pdbus_io, v4vConnectionHandle ptransportdata);

        #region Delegate Fields
        internal connect_v4v_socket_delegate connect_v4v_socket = v4vConnectionFunctions.connect_v4v_socket;
        internal disconnect_v4v_socket_delegate disconnect_v4v_socket = v4vConnectionFunctions.disconnect_v4v_socket;
        internal cancel_v4v_socket_io_delegate cancel_v4v_socket_io = v4vConnectionFunctions.cancel_v4v_socket_io;
        //internal wait_for_v4v_read_handles_delegate wait_for_v4v_read_handles = v4vConnectionFunctions.wait_for_v4v_read_handles;
        internal wait_for_v4v_read_handles_timeout_delegate wait_for_v4v_read_handles_timeout = v4vConnectionFunctions.wait_for_v4v_read_handles_timeout;
        internal populate_dbus_io_delegate populate_dbus_io = v4vConnectionFunctions.populate_dbus_io;

        #endregion // Delegate Fields
        #endregion // Delegates

    } // Ends class v4vConnectionDelegates
#endif // STOP_GARBAGE_COLLECTING_MY_DELEGATES

    /// <summary>
    /// Managed a v4v connection via unmanaged function calls.
    /// </summary>
    public class v4vConnection : Udbus.Serialization.IUdbusTransport
    {
        private v4vConnectionHandle _handle;
        private Udbus.Serialization.UdbusDelegates.D_io_debug io_debug;
        private v4vConnection(v4vConnectionHandle _handle, Udbus.Serialization.UdbusDelegates.D_io_debug io_debug)
        {
            this._handle = _handle;
            this.io_debug = io_debug;
        }

        public v4vConnection(Udbus.Serialization.UdbusDelegates.D_io_debug io_debug)
        {
            this.io_debug = io_debug;

            // Form the v4v connection.
            v4vConnectionHandle handle = v4vConnectionFunctions.connect_v4v_socket(this.io_debug);

            if (handle == null || handle.IsInvalid)
            {
                throw Udbus.Serialization.Exceptions.TransportFailureException.Create(this, "Error calling connect_v4v_socket");
            }

            this._handle = handle;
        }

        public v4vConnection Release()
        {
            v4vConnectionHandle _handleTemp = this._handle;
            Udbus.Serialization.UdbusDelegates.D_io_debug io_debugTemp = this.io_debug;

            this._handle = null;
            this.io_debug = null;

            v4vConnection connectionRelease = new v4vConnection(_handleTemp, io_debugTemp);
            return connectionRelease;
        }

        public void Swap(v4vConnection other)
        {
            v4vConnectionHandle _handleTemp = this._handle;
            Udbus.Serialization.UdbusDelegates.D_io_debug io_debugTemp = this.io_debug;

            this._handle = other._handle;
            this.io_debug = other.io_debug;

            other._handle = _handleTemp;
            other.io_debug = io_debugTemp;
        }

        // Standard boilerplate disposal code.
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._handle != null && !this._handle.IsInvalid)
            {
                this._handle.Dispose();
                this.io_debug = null;
            }
        }

        #region IUdbusTransport functions
        public virtual bool PopulateDbio(ref Udbus.Serialization.ManagedDbusIo pdbus_io)
        {
            bool result = false;

            if (this.io_debug != null && pdbus_io.io_debug == null)
            {
                pdbus_io.io_debug = this.io_debug;
            }

            if (this._handle != null && !this._handle.IsInvalid)
            {
                result = v4vConnectionFunctions.populate_dbus_io(ref pdbus_io, this._handle);
            }
            else
            {
                pdbus_io = default(Udbus.Serialization.ManagedDbusIo);
            }
            return result;
        }

        public Udbus.Serialization.WaitForReadResult WaitForRead(UInt32 milliseconds)
        {
            Udbus.Serialization.WaitForReadResult result = Udbus.Serialization.WaitForReadResult.Failed;

            if (this._handle != null && !this._handle.IsInvalid)
            {
                UInt32 waitResult = v4vConnectionFunctions.wait_for_v4v_read_handles_timeout(milliseconds, this._handle, "Managed wait");
                switch (waitResult)
                {
                    case Constants.WAIT_TIMEOUT:
                        result = Udbus.Serialization.WaitForReadResult.TimedOut;
                        break;
                    case Constants.WAIT_FAILED:
                        result = Udbus.Serialization.WaitForReadResult.Failed;
                        break;
                    default:
                        result = Udbus.Serialization.WaitForReadResult.Succeeded;
                        break;
                }
            }
            return result;
        }

        public bool Cancel()
        {
            bool result = false;
            if (this._handle != null && !this._handle.IsInvalid)
            {
                result = v4vConnectionFunctions.cancel_v4v_socket_io(this._handle);
            }
            return result;
        }
        #endregion // IUdbusTransport functions

    } // Ends class v4vConnection


} // Ends namespace Udbus.v4v
