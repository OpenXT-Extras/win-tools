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

namespace Udbus.Core.Logging
{
    public class TraceFormatFieldDelegates
    {
        static public string DateTime()
        {
            return System.DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
        }

        static public string Timestamp()
        {
            return System.Diagnostics.Stopwatch.GetTimestamp().ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        static public string ProcessId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            //return Microsoft.Win32.NativeMethods.GetCurrentProcessId().ToString();
        }

        static public string ThreadId()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        static public string Callstack()
        {
            return Environment.StackTrace.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static class Fields
        {
            public const string DateTime = "DateTime";
            public const string Timestamp = "Timestamp";
            public const string ProcessId = "ProcessId";
            public const string ThreadId = "ThreadId";
            public const string Callstack = "Callstack";
            public const string Message = "Message";
        }
    } // Ends class TraceFormatFieldDelegates
}
