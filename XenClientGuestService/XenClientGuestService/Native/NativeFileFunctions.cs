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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace XenClientGuestService.Native
{
    // Carefully cut'n'pasted from http://www.pinvoke.net/default.aspx/kernel32.CreateFile
    internal static class FileFunctions
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] EFileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileAttributes flags,
            IntPtr template);
    }


    [Flags]
    enum EFileAccess : uint
    {
        //
        // Standard Section
        //

        AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
        MaximumAllowed = 0x2000000,     // MaximumAllowed access type

        Delete = 0x10000,
        ReadControl = 0x20000,
        WriteDAC = 0x40000,
        WriteOwner = 0x80000,
        Synchronize = 0x100000,

        StandardRightsRequired = 0xF0000,
        StandardRightsRead = ReadControl,
        StandardRightsWrite = ReadControl,
        StandardRightsExecute = ReadControl,
        StandardRightsAll = 0x1F0000,
        SpecificRightsAll = 0xFFFF,

        FILE_READ_DATA = 0x0001,        // file & pipe
        FILE_LIST_DIRECTORY = 0x0001,       // directory
        FILE_WRITE_DATA = 0x0002,       // file & pipe
        FILE_ADD_FILE = 0x0002,         // directory
        FILE_APPEND_DATA = 0x0004,      // file
        FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
        FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
        FILE_READ_EA = 0x0008,          // file & directory
        FILE_WRITE_EA = 0x0010,         // file & directory
        FILE_EXECUTE = 0x0020,          // file
        FILE_TRAVERSE = 0x0020,         // directory
        FILE_DELETE_CHILD = 0x0040,     // directory
        FILE_READ_ATTRIBUTES = 0x0080,      // all
        FILE_WRITE_ATTRIBUTES = 0x0100,     // all

        //
        // Generic Section
        //

        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000,

        SPECIFIC_RIGHTS_ALL = 0x00FFFF,
        FILE_ALL_ACCESS =
        StandardRightsRequired |
        Synchronize |
        0x1FF,

        FILE_GENERIC_READ =
        StandardRightsRead |
        FILE_READ_DATA |
        FILE_READ_ATTRIBUTES |
        FILE_READ_EA |
        Synchronize,

        FILE_GENERIC_WRITE =
        StandardRightsWrite |
        FILE_WRITE_DATA |
        FILE_WRITE_ATTRIBUTES |
        FILE_WRITE_EA |
        FILE_APPEND_DATA |
        Synchronize,

        FILE_GENERIC_EXECUTE =
        StandardRightsExecute |
          FILE_READ_ATTRIBUTES |
          FILE_EXECUTE |
          Synchronize
    }

}
