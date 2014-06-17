//
// Copyright (c) 2013 Citrix Systems, Inc.
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
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace XenClientGuestService.Native
{
    internal static class MappingFunctions
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFileMapping(
            SafeFileHandle hFile,
            IntPtr lpFileMappingAttributes,
            FileMapProtection flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPTStr)] string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern FileMappingViewHandle MapViewOfFile(
            SafeFileHandle hFileMappingObject,
            FileMapAccess dwDesiredAccess,
            UInt32 dwFileOffsetHigh,
            UInt32 dwFileOffsetLow,
            UInt32 dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
    } // Ends class MappingFunctions

    public class FileMappingViewHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private FileMappingViewHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            IntPtr closeHandle = this.handle;
            this.handle = System.IntPtr.Zero;
            return MappingFunctions.UnmapViewOfFile(this.handle);
        }

        public byte ReadByte(int offset)
        {
            return Marshal.ReadByte(this.handle, offset);
        }

        public class ViewReader
        {
            private int offset = 0;
            FileMappingViewHandle view;

            internal ViewReader(FileMappingViewHandle view)
            {
                this.view = view;
            }

            public byte ReadByte()
            {
                byte b = this.view.ReadByte(this.offset);
                ++this.offset;
                return b;
            }
        }

        public ViewReader CreateViewReader()
        {
            return new ViewReader(this);
        }
    } // Ends class v4vConnectionHandle

    [Flags]
    enum FileMapProtection : uint
    {
        PageReadonly = 0x02,
        PageReadWrite = 0x04,
        PageWriteCopy = 0x08,
        PageExecuteRead = 0x20,
        PageExecuteReadWrite = 0x40,
        SectionCommit = 0x8000000,
        SectionImage = 0x1000000,
        SectionNoCache = 0x10000000,
        SectionReserve = 0x4000000,
    }

    [Flags]
    public enum FileMapAccess : uint
    {
        FileMapCopy = 0x0001,
        FileMapWrite = 0x0002,
        FileMapRead = 0x0004,
        FileMapAllAccess = 0x001f,
        FileMapExecute = 0x0020,
    }
}
