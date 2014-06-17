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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;

namespace XenClientGuestService
{
    static class XenClientGuestServiceProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //Look for V4V device before we try to start the service
            ManagementObjectSearcher deviceList = new ManagementObjectSearcher("Select * from Win32_PnPEntity");
            bool v4vfound = false;
            if (deviceList != null)
            {
                // Enumerate the devices
                foreach (ManagementObject device in deviceList.Get())
                {
                    if (device.GetPropertyValue("DeviceID").ToString().StartsWith("XEN\\V4V"))
                    {
                        // Evaluate the device status.
                        string status = device.GetPropertyValue("Status").ToString();
                        bool working = ((status == "OK") || (status == "Degraded") || (status == "Pred Fail"));
                        if (working) v4vfound = true;
                    }
                }
            }
#if DEBUG
            bool bService = true;
            foreach (string arg in args)
            {
                if (arg.ToLower() == "/console")
                {
                    bService = false;
                    break; //We only care about /console, all other args go unloved
                }
            }

            //Running as a service, not console
            if (bService)
            {
#endif
                if (!v4vfound)
                {
                    EventLog.WriteEntry("XenClientGuestService", "Not starting XenClientGuestService, V4V device is unavailable");
                }
                else
                {
                    // System.ArgumentException only occurs if null passed, constructor can not return null
                    // System.ComponentModel.Win32Exception does not happen in .NET 3.5 & above, the tooltip box lies
                    ServiceBase ServiceToRun = new XenClientGuestService();
                    ServiceBase.Run(ServiceToRun);
                }
#if DEBUG
            }
            else
            {
                //No exception handling here as to be honest, if any of this breaks we have serious problems & it's for debugging purposes only!
                String calledFrom = System.IO.Directory.GetCurrentDirectory();
                String executedFrom = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                System.Console.WriteLine("Running executable from {0}", calledFrom);

                if (!calledFrom.Equals(executedFrom))
                {
                    System.Console.WriteLine("Executable actually running in {0}", executedFrom);
                    System.IO.Directory.SetCurrentDirectory(executedFrom);
                }
                
                XenClientGuestService service = new XenClientGuestService();
                service.OnServiceStart(args);

                System.Console.WriteLine("Press enter to end XenClientGuestService...");
                System.Console.ReadLine();

                service.OnServiceStop();
            }
#endif
        }
    }
}
