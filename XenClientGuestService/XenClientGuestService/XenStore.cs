//
// Copyright (c) 2014 Citrix Systems, Inc.
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
/// <remarks>XenStoreFunctions wraps the raw dll entry points.
/// Since xs2.dll has to be tracked down at runtime, XenStoreFunctions also does its best to setup the DllDirectory, and tear it down again once the XenStore dll has been loaded
/// assuming that xs2_open is the first function called.
/// The handle classes handle unmanaged resource lifetime so there shouldn't be a need to call the free/unwatch functions explicitly,
/// and XenStoreWrapper is a programmer friendly class to use XenStore functionality.
/// </remarks>
using System.Runtime.InteropServices;
namespace XenStoreLib
{
    internal class XenStoreFunctions
    {
        private const string kernel32Dll = "kernel32.dll";
        private const string XenStoreDll = "xs2.dll";

        #region PInvoke Declarations
        #region Kernel32 functions
        [System.Runtime.InteropServices.DllImport(kernel32Dll, SetLastError = true)]
        static internal extern IntPtr LoadLibrary(string lpPathName);

        [System.Runtime.InteropServices.DllImport(kernel32Dll, SetLastError = true)]
        static internal extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [System.Runtime.InteropServices.DllImport(kernel32Dll, SetLastError = true)]
        static internal extern bool FreeLibrary(IntPtr hModule);
        #endregion// Kernel32 functions

        #region XenStore functions

        // Delegate method "types"
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate XenStoreHandle raw_xs2_open();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void raw_xs2_close(System.IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate XenStoreMemoryHandle raw_xs2_read(XenStoreHandle handle, string path, [System.Runtime.InteropServices.Out] out int len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool raw_xs2_write(XenStoreHandle handle, string path, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool raw_xs2_write_bin(XenStoreHandle handle, string path, byte[] data, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void raw_xs2_free(System.IntPtr mem);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate XenStoreStringArrayHandle raw_xs2_directory(XenStoreHandle handle, string path, [System.Runtime.InteropServices.Out] out uint num);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool raw_xs2_remove(XenStoreHandle handle, string path);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate XenStoreWatchHandle raw_xs2_watch(XenStoreHandle handle, string path, Microsoft.Win32.SafeHandles.SafeWaitHandle event_);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void raw_xs2_unwatch(System.IntPtr watch);
        
        #endregion // XenStore functions
        #endregion // PInvoke Declarations
        
        //Methods to be delegated as DLL calls
        public static raw_xs2_open xs2_open;
        public static readonly raw_xs2_close xs2_close;
        private static readonly raw_xs2_read xs2_read_impl;
        public static readonly raw_xs2_free xs2_free;
        public static readonly raw_xs2_write xs2_write;
        private static readonly raw_xs2_write_bin xs2_write_bin_impl;
        private static readonly raw_xs2_directory xs2_directory_impl;
        public static readonly raw_xs2_remove xs2_remove;
        public static readonly raw_xs2_watch xs2_watch;
        public static readonly raw_xs2_unwatch xs2_unwatch;

        //Constants
        private const string XenToolsRegkey = "Software\\Citrix\\XenTools";
        private const string DefaultXS2SubPath = "\\Citrix\\XenTools";

        static XenStoreFunctions()
        {
            // Find the XenStore dll location - default or registry based?
            string xenStorePath;
            
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                xenStorePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + DefaultXS2SubPath + "\\xs2_32.DLL";
            }
            else
            {
                xenStorePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + DefaultXS2SubPath + "\\xs2.DLL";
            }

            Microsoft.Win32.RegistryKey xentoolsRegKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(XenToolsRegkey);
            if (xentoolsRegKey != null)
            {
                xenStorePath = xentoolsRegKey.GetValue(XenStoreDll, xenStorePath).ToString();
            }

            //Get a DLL pointer
            IntPtr pDll = LoadLibrary(xenStorePath);

            //Pointers to DLL functions
            IntPtr pAddressOfxs_open = IntPtr.Zero;
            IntPtr pAddressOfxs_close = IntPtr.Zero;
            IntPtr pAddressOfxs_read = IntPtr.Zero;
            IntPtr pAddressOfxs_write = IntPtr.Zero;
            IntPtr pAddressOfxs_write_bin = IntPtr.Zero;
            IntPtr pAddressOfxs_free = IntPtr.Zero;
            IntPtr pAddressOfxs_directory = IntPtr.Zero;
            IntPtr pAddressOfxs_remove = IntPtr.Zero;
            IntPtr pAddressOfxs_watch = IntPtr.Zero;
            IntPtr pAddressOfxs_unwatch = IntPtr.Zero;

            //Get the pointers to DLL functions based on process bitage
            if (IntPtr.Size == 4)
            {
                pAddressOfxs_open = GetProcAddress(pDll, "_xs2_open@0");
                pAddressOfxs_close = GetProcAddress(pDll, "_xs2_close@4");
                pAddressOfxs_read = GetProcAddress(pDll, "_xs2_read@12");
                pAddressOfxs_write = GetProcAddress(pDll, "_xs2_write@12");
                pAddressOfxs_write_bin = GetProcAddress(pDll, "_xs2_write_bin@16");
                pAddressOfxs_free = GetProcAddress(pDll, "_xs2_free@4");
                pAddressOfxs_directory = GetProcAddress(pDll, "_xs2_directory@12");
                pAddressOfxs_remove = GetProcAddress(pDll, "_xs2_remove@8");
                pAddressOfxs_watch = GetProcAddress(pDll, "_xs2_watch@12");
                pAddressOfxs_unwatch = GetProcAddress(pDll, "_xs2_unwatch@4");
            }
            else if (IntPtr.Size == 8)
            {
                pAddressOfxs_open = GetProcAddress(pDll, "xs2_open");
                pAddressOfxs_close = GetProcAddress(pDll, "xs2_close");
                pAddressOfxs_read = GetProcAddress(pDll, "xs2_read");
                pAddressOfxs_write = GetProcAddress(pDll, "xs2_write");
                pAddressOfxs_write_bin = GetProcAddress(pDll, "xs2_write_bin");
                pAddressOfxs_free = GetProcAddress(pDll, "xs2_free");
                pAddressOfxs_directory = GetProcAddress(pDll, "xs2_directory");
                pAddressOfxs_remove = GetProcAddress(pDll, "xs2_remove");
                pAddressOfxs_watch = GetProcAddress(pDll, "xs2_watch");
                pAddressOfxs_unwatch = GetProcAddress(pDll, "xs2_unwatch");
            }

            #region DLL delegations

            //Delegate the dll open pointer to the method xs2_open()
            xs2_open = (raw_xs2_open)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_open, typeof(raw_xs2_open));

            //Delegate the dll close pointer to the method xs2_close()
            xs2_close = (raw_xs2_close)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_close, typeof(raw_xs2_close));

            //Delegate the dll read pointer to the method xs2_read_impl()
            xs2_read_impl = (raw_xs2_read)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_read, typeof(raw_xs2_read));

            //Delegate the dll write pointer to the method xs2_write
            xs2_write = (raw_xs2_write)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_write, typeof(raw_xs2_write));

            //Delegate the dll write bin pointer to the method xs2_write_bin_impl()
            xs2_write_bin_impl = (raw_xs2_write_bin)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_write_bin, typeof(raw_xs2_write_bin));

            //Delegate the dll free pointer to the method xs2_free()
            xs2_free = (raw_xs2_free)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_free, typeof(raw_xs2_free));

            //Delegate the dll directory pointer to the method xs2_directory_impl()
            xs2_directory_impl = (raw_xs2_directory)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_directory, typeof(raw_xs2_directory));

            //Delegate the dll remove pointer to the method xs2_remove()
            xs2_remove = (raw_xs2_remove)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_remove, typeof(raw_xs2_remove));

            //Delegate the dll watch pointer to the method xs2_watch()
            xs2_watch = (raw_xs2_watch)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_watch, typeof(raw_xs2_watch));

            //Delegate the dll unwatch pointer to the method xs2_unwatch()
            xs2_unwatch = (raw_xs2_unwatch)Marshal.GetDelegateForFunctionPointer(pAddressOfxs_unwatch, typeof(raw_xs2_unwatch));
            #endregion
        }

        /// <summary>
        /// Friendly version of xs2_directory wrapping up results.
        /// </summary>
        /// <param name="handle">XenStore handle.</param>
        /// <param name="path">XenStore path</param>
        /// <returns>XenStoreStringArray which can read out the string values.</returns>
        public static XenStoreStringArray xs2_directory(XenStoreHandle handle, string path)
        {
            XenStoreStringArray result = null;

            uint num;
            XenStoreStringArrayHandle directory = xs2_directory_impl(handle, path, out num);
            result = new XenStoreStringArray(num, directory);

            return result;
        }

        public static bool xs2_write_bin(XenStoreHandle handle, string path, byte[] data, int size)
        {
            return xs2_write_bin_impl(handle, path, data, size);
        }

        public static bool xs2_write_bin(XenStoreHandle handle, string path, byte[] data)
        {
            return xs2_write_bin_impl(handle, path, data, data.Length);
        }

        public static XenStoreMemoryHandle xs2_read(XenStoreHandle handle,
            string path)
        {
            int len;
            XenStoreMemoryHandle mem = xs2_read_impl(handle, path, out len);
            if (mem != null)
            {
                // Output includes nul-terminator, but len doesn't.
                mem.Count = len + 1;
            }

            return mem;
        }
    } // Ends class XenStoreFunctions

    #region Handle Classes
    internal class XenStoreHandle : Microsoft.Win32.SafeHandles.SafeHandleMinusOneIsInvalid
    {
        private XenStoreHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            // Seems like when disposing, IsClosed is true, even though we're in the middle of releasing...
            bool released = this.IsInvalid;
            if (!released) // If valid handle
            {
                System.IntPtr release = this.handle;
                this.handle = System.IntPtr.Zero;
                XenStoreFunctions.xs2_close(release);
                released = true;

            } // Ends if valid handle

            return released;
        }

    } // Ends class XenStoreHandle

    internal class XenStoreMemoryHandle : Microsoft.Win32.SafeHandles.SafeHandleMinusOneIsInvalid
    {
        byte[] value;
        private bool unmarshalled = false;
        private int count;

        private XenStoreMemoryHandle()
            : base(true)
        {
        }

        /// Properties.
        public int Count { set { this.count = value; } }

        public byte[] Value
        {
            get
            {
                if (this.unmarshalled == false && this.IsClosed == false && this.IsInvalid == false && this.handle != System.IntPtr.Zero)
                {
                    byte[] read = new byte[this.count];
                    System.Runtime.InteropServices.Marshal.Copy(this.handle, read, 0, this.count);
                    this.value = read;
                    this.unmarshalled = true;
                }
                return this.value;
            }
        }

        protected override bool ReleaseHandle()
        {
            // Seems like when disposing, IsClosed is true, even though we're in the middle of releasing...
            bool released = this.IsInvalid;
            if (!released) // If valid handle
            {
                System.IntPtr release = this.handle;
                this.handle = System.IntPtr.Zero;
                XenStoreFunctions.xs2_free(release);
                released = true;

            } // Ends if valid handle

            return released;
        }

    } // Ends class XenStoreMemoryHandle

    internal class XenStoreWatchHandle : Microsoft.Win32.SafeHandles.SafeHandleMinusOneIsInvalid
    {
        private bool unmarshalled = false;

        private XenStoreWatchHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            // Seems like when disposing, IsClosed is true, even though we're in the middle of releasing...
            bool released = this.IsInvalid;
            if (!released) // If valid handle
            {
                System.IntPtr release = this.handle;
                this.handle = System.IntPtr.Zero;
                XenStoreFunctions.xs2_unwatch(release);
                this.handle = System.IntPtr.Zero;
                released = true;

            } // Ends if valid handle

            return released;
        }

    } // Ends class XenStoreWatchHandle

    public class XenStoreStringHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        private string value;
        private bool unmarshalled = false;

        /// Construction
        protected XenStoreStringHandle() : base(true) { }
        internal XenStoreStringHandle(System.IntPtr handle) : base(true) { this.SetHandle(handle);  }

        protected override bool ReleaseHandle()
        {
            bool released = false;
            if (!this.IsInvalid)
            {
                System.IntPtr release = this.handle;
                this.handle = System.IntPtr.Zero;
                XenStoreFunctions.xs2_free(release);
                released = true;
            }
            return released;
        }

        /// Properties.
        public string Value
        {
            get
            {
                if (this.unmarshalled == false && this.IsClosed == false && this.IsInvalid == false && this.handle != System.IntPtr.Zero)
                {
                    this.value = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(this.handle);
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
        public static implicit operator string(XenStoreStringHandle h)
        {
            return h.Value;
        }

    } // Ends class XenStoreStringHandle

    public class XenStoreStringArrayHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        /// Construction
        protected XenStoreStringArrayHandle() : base(true) { }

        protected override bool ReleaseHandle()
        {
            bool released = false;
            if (!this.IsInvalid)
            {
                System.IntPtr release = this.handle;
                this.handle = System.IntPtr.Zero;
                XenStoreFunctions.xs2_free(release);
                released = true;
            }
            return released;
        }

        ///// Properties.
        public bool Empty { get { return this.handle == System.IntPtr.Zero; } }


        public System.IntPtr[] GetValues(uint num)
        {
            System.IntPtr[] values = null;

            if (this.IsClosed == false && this.IsInvalid == false && this.handle != System.IntPtr.Zero)
            {
                System.IntPtr[] valuePointers = new System.IntPtr[num];
                int nNum = checked((int)num);
                System.Runtime.InteropServices.Marshal.Copy(this.handle, valuePointers, 0, nNum);
                values = valuePointers;
            }

            return values;
        }

    } // Ends class XenStoreStringArrayHandle

    public class XenStoreStringArray : System.IDisposable
    {
        public readonly uint Num = 0;
        private XenStoreStringArrayHandle strings;
        // Holding all the IntPtrs in a managed SafeHandle array means the memory will be freed by finalizers.
        XenStoreStringHandle[] stringHandles = null;

        private string[] value;
        private bool unmarshalled = false;

        public XenStoreStringArray(uint Num, XenStoreStringArrayHandle strings)
        {
            System.IntPtr[] valuePointers = this.strings.GetValues(Num);
            this.stringHandles = System.Linq.Enumerable.ToArray<XenStoreStringHandle>(
                System.Linq.Enumerable.Select<System.IntPtr, XenStoreStringHandle>(valuePointers, CreateXenStoreStringHandle)
            );
            this.Num = Num;
            this.strings = strings;
        }


        /// Properties.
        public string[] Value
        {
            get
            {
                if (this.unmarshalled == false && strings.IsClosed == false && strings.IsInvalid == false && !strings.Empty)
                {
                    if (this.stringHandles != null)
                    {
                        string[] values = System.Array.ConvertAll <XenStoreStringHandle, string>(this.stringHandles, StringFromXenStoreStringHandle);
                        this.value = values;
                        this.unmarshalled = true;
                    }
                }
                return this.value;
            }
        }

        #region Conversion functions
        static private string StringFromXenStoreStringHandle(XenStoreStringHandle stringHandle)
        {
            return stringHandle.Value;
        }

        static private XenStoreStringHandle CreateXenStoreStringHandle(System.IntPtr handle)
        {
            return new XenStoreStringHandle(handle);
        }
        #endregion // Conversion functions

        #region IDisposable Members

        public void Dispose()
        {
            XenStoreStringHandle[] cleanupStringHandles = this.stringHandles;
            XenStoreStringArrayHandle cleanupStrings = this.strings;

            this.stringHandles = null;
            this.strings = null;

            // Managed code cleanup.
            if (cleanupStringHandles != null)
            {
                // Free the XenStore strings.
                foreach (XenStoreStringHandle stringArrayHandle in cleanupStringHandles)
                {
                    //stringArrayHandle.Dispose();
                }
            }

            if (cleanupStrings != null)
            {
                // Free the XenStore string array.
                //cleanupStrings.Dispose();
            }
        }

        #endregion // Ends IDisposable Members
    } // Ends class XenStoreStringArray

    #endregion // Handle Classes

    public class XenStoreWrapper : System.IDisposable
    {
        static private object xenStoreHandleWeakRefLock = new object();
        static private System.WeakReference xenStoreHandleWeakRef;
        XenStoreLib.XenStoreHandle xenStoreHandle;
        public XenStoreWrapper()
        {
            XenStoreLib.XenStoreHandle theXenStoreHandle = null;
            lock (xenStoreHandleWeakRefLock)
            {
                if (xenStoreHandleWeakRef != null)
                {
                    theXenStoreHandle = (XenStoreLib.XenStoreHandle)xenStoreHandleWeakRef.Target;
                }

                if (theXenStoreHandle == null) // If no central xen store
                {
                    theXenStoreHandle = XenStoreLib.XenStoreFunctions.xs2_open();
                    xenStoreHandleWeakRef = new System.WeakReference(theXenStoreHandle);

                } // Ends if no central xen store
            } // Ends the xenstore handle lock

            this.xenStoreHandle = theXenStoreHandle;

        }

        #region IDisposable members
        public void Dispose()
        {
            // Handle gets cleaned up by GC... eventually.
        }
        #endregion // IDisposable members

        public bool Write(string path, byte[] data)
        {
            return XenStoreFunctions.xs2_write_bin(this.xenStoreHandle, path, data);
        }

        public bool Write(string path, string data)
        {
            return XenStoreFunctions.xs2_write(this.xenStoreHandle, path, data);
        }

        public byte[] Read(string path)
        {
            byte[] read;
            using (XenStoreMemoryHandle mem = XenStoreFunctions.xs2_read(this.xenStoreHandle, path))
            {
                read = mem.Value;
            }
            return read;
        }

        public string ReadString(string path)
        {
            byte[] read = this.Read(path);
            int length = read.Length;
            string result;
            if (length > 0 && read[length - 1] == 0)
            {
                // Don't include the nul-terminator when decoding bytes.
                result = System.Text.Encoding.ASCII.GetString(read, 0, length - 1);
            }
            else
            {
                result = System.Text.Encoding.ASCII.GetString(read);
            }
            return result;
        }

        public string[] Directory(string path)
        {
            string[] contents;
            using (XenStoreStringArray xenStoreDirectory = XenStoreFunctions.xs2_directory(this.xenStoreHandle, path))
            {
                contents = xenStoreDirectory.Value;
            }
            return contents;
        }

        public bool Remove(string path)
        {
            return XenStoreFunctions.xs2_remove(this.xenStoreHandle, path);
        }

        public Microsoft.Win32.SafeHandles.SafeHandleMinusOneIsInvalid Watch(string path, Microsoft.Win32.SafeHandles.SafeWaitHandle event_)
        {
            XenStoreWatchHandle watch = XenStoreFunctions.xs2_watch(this.xenStoreHandle, path, event_);
            return watch;
        }

    } // Ends class XenStoreWrapper
} // Ends namespace XenStoreLib
