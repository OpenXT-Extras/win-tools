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
using System.Runtime.InteropServices;

namespace Udbus.Serialization
{
    [StructLayout(LayoutKind.Sequential)]
    public struct dbus_reader
    {
        //[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)]
        //byte[] data; // TODO IntPtr ?
        IntPtr data;
        UInt32 align_offset;
        UInt32 offset;
        UInt32 length;
        int endianness; /* 0 = little endian, 1 = big endian */
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct dbus_writer
    {
        //byte *buffer; // TODO
        IntPtr buffer;
        UInt32 offset;
        UInt32 length;
        int endianness; /* 0 = little endian, 1 = big endian */
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct dbus_array_writer
    {
        //UInt32 *ptr;
        IntPtr ptr; // TODO
        UInt32 offset;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct dbus_array_reader
    {
        UInt32 length;
        UInt32 offset;
    };
} // Ends namespace Udbus.Serialization
