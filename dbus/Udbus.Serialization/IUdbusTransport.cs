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

namespace Udbus.Serialization
{
    /// <summary>
    /// Delegates used as part of udbus interface.
    /// </summary>
    public class UdbusDelegates
    {
        ///* return 0 if succeed to write count bytes, non-0 otherwise */
        //int (*io_write)(void *priv, const void *buf, uint32_t count);
        ///* return 0 if succeed to read count bytes, non-0 otherwise */
        //int (*io_read)(void *priv, void *buf, uint32_t count);
        ///* debugging logging */
        //void (*io_debug)(void *logpriv, const char *buf);

        ///* return 0 if succeed to write count bytes, non-0 otherwise */
        public delegate int D_io_write(IntPtr priv, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In]byte[] buf, System.UInt32 count);
        //public delegate int D_io_write(IntPtr priv, String buf, System.UInt32 count);
        /* return 0 if succeed to read count bytes, non-0 otherwise */
        public delegate int D_io_read(IntPtr priv, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][Out]byte[] buf, System.UInt32 count);
        /* debugging logging */
        public delegate void D_io_debug(IntPtr logpriv, String buf);
        /* retrieve fd for select operation */
        public delegate int D_io_get_fd(IntPtr priv);

    } // Ends class UdbusDelegates

    /// <summary>
    /// Managed structure equivalent to udbus io.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ManagedDbusIo
    {

        //[MarshalAs(UnmanagedType.I4)]
        //int data;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public UdbusDelegates.D_io_write io_write;
        //internal UdbusDelegates.D_io_write io_write;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public UdbusDelegates.D_io_read io_read;
        //internal UdbusDelegates.D_io_read io_read;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public UdbusDelegates.D_io_debug io_debug;
        //internal UdbusDelegates.D_io_debug io_debug;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        internal UdbusDelegates.D_io_get_fd io_get_fd;

        internal IntPtr priv;
        public IntPtr logpriv;
    };

    public enum WaitForReadResult
    {
         Succeeded
       , Failed
       , TimedOut
    };

    /// <summary>
    /// Functions that must be implemented by a transport for udbus.
    /// </summary>
    public interface IUdbusTransport : IDisposable
    {
        /// <summary>
        /// Populate the function pointers or delegates used by udbus.
        /// </summary>
        /// <param name="pdbus_io">Collection of function pointers to be populated.</param>
        /// <returns>true if successful, otherwise false.</returns>
        bool PopulateDbio(ref Udbus.Serialization.ManagedDbusIo pdbus_io);

        /// <summary>
        /// Wait for next read data.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to wait.</param>
        /// <returns>true if data arrived to be read, otherwise false.</returns>
        WaitForReadResult WaitForRead(UInt32 milliseconds);

        /// <summary>
        /// Cancel transport IO.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        bool Cancel();
    } // Ends interface IUdbusConnection

    public static class UdbusTransportDefaultImpl
    {
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="pdbus_io"></param>
        /// <returns></returns>
        public static bool PopulateDbio(IUdbusTransport transport, out ManagedDbusIo pdbus_io)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Default is a no-op implementation that says there's always data to read.
        /// </summary>
        /// <param name="transport">Transport interface being implemented.</param>
        /// <param name="milliseconds">Number of milliseconds to wait.</param>
        /// <returns>true if data arrived to be read, otherwise false.</returns>
        public static WaitForReadResult WaitForRead(IUdbusTransport transport, UInt32 milliseconds)
        {
            return WaitForReadResult.Succeeded;
        }

        /// <summary>
        /// Force transport to close.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public static bool Cancel()
        {
            return true;
        }
    }

    // Can't do partial interface implementation FFS.
    abstract public partial class UdbusTransportBase : IUdbusTransport
    {
        #region IUdbusTransport functions
        public abstract void Dispose(); // IDisposable method.

        public bool PopulateDbio(ref ManagedDbusIo pdbus_io)
        {
            return this.PopulateDbioImpl(out pdbus_io);
        }

        public WaitForReadResult WaitForRead(UInt32 milliseconds)
        {
            return this.WaitForReadImpl(milliseconds);
        }

        public bool Cancel()
        {
            return this.CancelImpl();
        }
        #endregion // IUdbusTransport functions

        #region IUdbusTransport function implementations
        abstract protected bool PopulateDbioImpl(out ManagedDbusIo pdbus_io);

        virtual protected WaitForReadResult WaitForReadImpl(UInt32 milliseconds)
        {
            return UdbusTransportDefaultImpl.WaitForRead(this, milliseconds);
        }

        virtual protected bool CancelImpl()
        {
            return UdbusTransportDefaultImpl.Cancel();
        }
        #endregion // IUdbusTransport function implementations

    } // Ends class UdbusTransportBase
} // Ends namespace Udbus.Serialization
