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
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace XenClientGuestService.wcf
{
    internal class XenClientGuestWCFService : wcf.Contracts.IXenClientGuestService
    {
        //typedef struct _XENINP_ACCELERATION {
        //    ULONG Acceleration;
        //} XENINP_ACCELERATION, *PXENINP_ACCELERATION;
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct XENINP_ACCELERATION
        {
            internal ulong Acceleration;
        }

        private const string XENINP_USER_FILE_NAME = "\\\\.\\Global\\xeninp";
        private const string InstallRegkeyName = "Software\\Citrix\\XenGuestPlugin";
        private const string XcInstallDirName = "Install_Dir";
        private const string RunKeyName = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string RunInstval = "XciPlugin";

        private readonly string uuid_;
        private string installDir;
        private Udbus.Core.Logging.ILog log;

        private XenClientGuestWCFService(Udbus.Core.Logging.ILog log)
        {
            this.log = log;

            // Initialize the uuid field from XenStore.
            XenStoreLib.XenStoreWrapper xenStoreWrapper = new XenStoreLib.XenStoreWrapper();
            string vm = xenStoreWrapper.ReadString("vm");
            string uuidPath = string.Format("{0}/uuid", vm);
            this.uuid_ = xenStoreWrapper.ReadString(uuidPath);

            // Work out the install directory.
            this.installDir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("PROGRAMFILES") ?? "C:\\Program Files", "Citrix");
            this.installDir = System.IO.Path.Combine(installDir, "XenGuestPlugin");

            Microsoft.Win32.RegistryKey installKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(InstallRegkeyName);
            if (installKey != null)
            {
                string installVal = (string)installKey.GetValue(XcInstallDirName);
                if (!string.IsNullOrEmpty(installVal))
                {
                    this.installDir = installVal;
                }
            }
        }

        internal XenClientGuestWCFService(System.Diagnostics.EventLog eventLog)
            : this(Logging.LogCreation.CreateWCFServiceLogger(eventLog))
        {
        }

        internal XenClientGuestWCFService()
            : this(Logging.LogCreation.CreateWCFServiceLogger())
        {
        }

        static private bool IsPluginAutorunOn()
        {
            Microsoft.Win32.RegistryKey runKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RunKeyName);
            bool pluginAutorun = true;

            if (runKey != null)
            {
                object runInstVal = runKey.GetValue(RunInstval);
                pluginAutorun = runInstVal != null;
                if (pluginAutorun)
                {
                    pluginAutorun = (string)runInstVal != string.Empty;
                }
            }

            return pluginAutorun;
        }

        private bool ConfigurePluginAutorun(bool enable)
        {
            Microsoft.Win32.RegistryKey runKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RunKeyName, true);
            bool result = runKey != null;

            if (result)
            {
                bool entryPopulated = !string.IsNullOrEmpty((string)runKey.GetValue(RunInstval));

                if (!enable) // If turning off
                {
                    result = false;

                    if (entryPopulated) // If entry in registry populated
                    {
                        result = SetRegistryValue(runKey, RunInstval, string.Empty, log);

                        if (!result)
                        {
                            log.Error("Failed to clear registry value \"{0}\\{1}\"", runKey.Name, RunInstval);

                        }
                    } // Ends if entry in registry populated
                    else // Else entry in registry not populated
                    {
                        result = true;

                    } // Ends else entry in registry not populated

                } // Ends if turning off
                else // Else turning off
                {
                    if (!entryPopulated) // If entry in registry not populated
                    {
                        string installPath = System.IO.Path.Combine(this.installDir, "XenGuestPlugin.exe");
                        if (installPath.Contains(' '))
                        {
                            installPath = "\"" + installPath + "\"";
                        }

                        result = SetRegistryValue(runKey, RunInstval, installPath, log);

                    } // Ends if entry in registry not populated
                    else // Else entry in registry populated
                    {
                        result = true;

                    } // Ends else entry in registry populated
                } // Ends else turning off
            }

            return result;
        }

        static private bool SetRegistryValue(Microsoft.Win32.RegistryKey key, string name, string value, Udbus.Core.Logging.ILog log)
        {
            bool result = false;

            try
            {
                key.SetValue(name, value);
                result = true;
            }
            catch (System.ArgumentNullException argnullEx)
            {
                if (log != null)
                {
                    log.Exception("Failed to set registry key (argument null) \"{0}\\{1}\" to \"{2}\". {3}", key.Name, name, value, argnullEx);
                }
            }
            catch (System.ArgumentException argEx)
            {
                if (log != null)
                {
                    log.Exception("Failed to set registry key (argument exception) \"{0}\\{1}\" to \"{2}\". {3}", key.Name, name, value, argEx);
                }
            }
            catch (System.ObjectDisposedException objdispEx)
            {
                if (log != null)
                {
                    log.Exception("Failed to set registry key (object disposed) \"{0}\\{1}\" to \"{2}\". {3}", key.Name, name, value, objdispEx);
                }
            }
            catch (System.UnauthorizedAccessException unauthEx)
            {
                if (log != null)
                {
                    log.Exception("Failed to set registry key (unauthorised) \"{0}\\{1}\" to \"{2}\". {3}", key.Name, name, value, unauthEx);
                }
            }
            catch (System.Security.SecurityException secEx)
            {
                if (log != null)
                {
                    log.Exception("Failed to set registry key (security exception) \"{0}\\{1}\" to \"{2}\". {3}", key.Name, name, value, secEx);
                }
            }
            catch (System.IO.IOException ioEx)
            {
                if (log != null)
                {
                    log.Exception("Failed to set registry key (io exception) \"{0}\\{1}\" to \"{2}\". {3}", key.Name, name, value, ioEx);
                }
            }

            return result;
        }


        #region IXenClientGuestService members
        public string uuid
        {
            get { return this.uuid_; }
        }

        public bool TogglePluginAutorun(bool toggle)
        {
            bool result;
            if (!toggle) // If not toggling
            {
                result = IsPluginAutorunOn();

            } // Ends if not toggling
            else // Else toggling
            {
                bool pluginAutorunEnabled = IsPluginAutorunOn();
                bool configurePluginAutorun = this.ConfigurePluginAutorun(!pluginAutorunEnabled);
                if (!configurePluginAutorun) // If failed to configure
                {
                    // No toggling occurred.
                    result = pluginAutorunEnabled;

                } // Ends if failed to configure
                else // Else configured
                {
                    result = !pluginAutorunEnabled;

                } // Ends else configured
            } // Ends else toggling

            return result;
        }

        public void SetAcceleration(ulong acceleration)
        {
            using (Microsoft.Win32.SafeHandles.SafeFileHandle hFile = Native.FileFunctions.CreateFile(XENINP_USER_FILE_NAME
                , Native.EFileAccess.GenericRead | Native.EFileAccess.GenericWrite
                , System.IO.FileShare.Read | System.IO.FileShare.Write
                , IntPtr.Zero
                , System.IO.FileMode.Open
                , System.IO.FileAttributes.Normal
                , IntPtr.Zero
            ))
            {
                if (hFile == null || hFile.IsInvalid)
                {
                    log.Error("Failed to open XenInput file handle");
                }
                else
                {
                    XENINP_ACCELERATION xaMouAccel;
                    xaMouAccel.Acceleration = acceleration;
                    uint bytesReturned = 0;
                    //if (!DeviceIoControl(hXenInp, XENINP_IOCTL_ACCELERATION, &xaMouAccel, 
                    //    sizeof(XENINP_ACCELERATION), NULL, 0, &dwOut, NULL))
                    if (!Native.IOControlFunctions.DeviceIoControl(hFile
                        , Native.EIOControlCode.XENINP_IOCTL_ACCELERATION
                        , xaMouAccel
                        , (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(XENINP_ACCELERATION))
                        , IntPtr.Zero
                        , 0
                        , ref bytesReturned
                        , IntPtr.Zero
                        ))
                    {
                        log.Error("Failed to send XenInput IOControl Acceleration");
                    }
                }
            }
        }

        public bool XenStoreWrite(string path, string value)
        {
            using (XenStoreLib.XenStoreWrapper xenStoreWrapper = new XenStoreLib.XenStoreWrapper())
            {
                string data = path + " = " + value;
                EventLog.WriteEntry("xenStoreWrite", data);
                return xenStoreWrapper.Write(path, value);
            }
        }
        #endregion // IXenClientGuestService members
    }
}
