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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using XenGuestPlugin.Types;

namespace XenGuestPlugin
{
    public static class XenVmsCache
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ChangeableDictionary<String, XenVM> vms = new ChangeableDictionary<String, XenVM>();
        private static XenGuestAgentLib.XenGuestServices xgsc = null;
        private static System.Threading.EventWaitHandle evvms = null;
        private const Int32 CACHE_MONITOR_TIMEOUT = 2000;

        public static bool Shutdown = false;
        public static event EventHandler BatchEvent;

        public static XenVM[] VMs
        {
            get
            {
                lock (vms)
                {
                    XenVM[] _vms = new XenVM[vms.Count];
                    vms.Values.CopyTo(_vms, 0);
                    return _vms;
                }
            }
        }

        public static void Initialize(XenGuestAgentLib.XenGuestServices service)
        {
            Program.AssertOffEventThread();
            Thread thread;
            string evname;
            EventWaitHandleSecurity evsec;
            EventWaitHandleAccessRule evrule;
            string user;
            xgsc = service;

            evname = xgsc.RegisterVmsEvent();
            user = Environment.UserDomainName + "\\" + Environment.UserName;
            evvms = EventWaitHandle.OpenExisting(evname,
                EventWaitHandleRights.ReadPermissions | EventWaitHandleRights.ChangePermissions);
            evsec = evvms.GetAccessControl();
            evrule = new EventWaitHandleAccessRule(user,
                EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify,
                AccessControlType.Deny);
            evsec.RemoveAccessRule(evrule);

            evrule = new EventWaitHandleAccessRule(user,
                EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify,
                AccessControlType.Allow);
            evsec.AddAccessRule(evrule);
            evvms.SetAccessControl(evsec);

            evvms = EventWaitHandle.OpenExisting(evname);

            // Start the monitor thread
            thread = new Thread(CacheXgsMonitor);
            thread.Name = "XenClient Cache Thread";
            thread.IsBackground = true;

            thread.Start();
        }

        private static IEnumerable<XenVM> GetVMs()
        {
            uint count, i, j;
            XenGuestAgentLib.XenVmInfo xvic = null;

            try
            {
                count = xgsc.QueryVms();
                if (count > 20)
                {
                    throw new Exception("hack hack hack");
                }
            }
            catch (Exception e)
            {
                log.Debug("Exception reading VMS information from XenGuestServices", e);
                yield break;
            }

            for (i = 0; i < count; i++)
            {
                XenVM xenvm = new XenVM();
                MemoryStream ms;
                Array arr;
                Bitmap blueImage;

                try
                {
                    xvic = xgsc.GetVmObject(i);
                }
                catch (Exception e)
                {
                    log.Debug("Exception reading VMS information at index: " + i, e);
                    yield break;
                }
                if (xvic == null)
                {
                    log.Debug("No VM object returned for index: " + i);
                    yield break;
                }
                xenvm.UUID = xvic.GetUuid();
                xenvm.Name = xvic.GetName();
                xenvm.Slot = (int)xvic.GetSlot();
                xenvm.Hidden = xvic.IsHidden() || xvic.IsUivm();

                if (xvic.HasImage())
                {
                    ms = new MemoryStream();
                    arr = xvic.GetImage();

                    for (j = 0; j < (uint)arr.GetLength(0); j++)
                    {
                        ms.WriteByte((byte)arr.GetValue(j));
                    }
                    xenvm.Image = Image.FromStream(ms);
                }
                else
                {
                    blueImage = (Bitmap)Properties.Resources.Blue_VM.Clone();
                    xenvm.Image = blueImage;
                }

                Marshal.ReleaseComObject(xvic);
                xvic = null;

                yield return xenvm;
            }
        }

        public static void UpdateVms()
        {
            Program.AssertOffEventThread();

            int slot = 0;
            int maxSlot = 0;
            List<XenVM> sortedVMs = null;
            IEnumerable<XenVM> VMs = null;
            int tries = 3;

            // Look at this loop, hard-coded to 3 tries.
            // Look at those slot checks down there, between 0, and 9, or not equal to or over 100.
            // What the hell's going on ? A fix for XC-5824 is what's going on.
            // It's so horrible I'm not allowed to tell you why it's there.
            // Throw this crap away straight after Synergy, and do it again properly.
            // Thanks.
            while (true)
            {
                int rubbish = 0;
                slot = 0;
                maxSlot = 0;

                VMs = GetVMs();

                foreach (XenVM xenvm in VMs)
                {
                    if (xenvm.Slot >= 0 && xenvm.Slot <= 9)
                    {
                        if (xenvm.Slot > maxSlot)
                            maxSlot = xenvm.Slot;
                    }
                    else if (xenvm.Slot >= 100)
                    {
                        rubbish++;
                    }
                }

                if (tries-- == 0)
                    return;
                if (rubbish == 0)
                    break;

                // Dump rubbish and try again
                VMs = null;
                System.Threading.Thread.Sleep(1000);
            }

            sortedVMs = new List<XenVM>();

            while (slot <= maxSlot)
            {
                foreach (XenVM xenvm in VMs)
                    if (xenvm.Slot == slot)
                        sortedVMs.Add(xenvm);
                slot++;
            }

            foreach (XenVM xenvm in sortedVMs)
                lock (vms)
                    vms[xenvm.UUID] = xenvm;

            OnBatchEvent();
        }

        private static void CacheXgsMonitor()
        {
            bool rc;

            try
            {
                // Once up front
                UpdateVms();

                for ( ; ; )
                {
                    rc = evvms.WaitOne(CACHE_MONITOR_TIMEOUT, false);
                    if (rc) // if (signaled)
                    {
                        // RJP our version clears the list first
                        lock (vms)
                            vms.Clear();
                        UpdateVms();
                    }

                    if (Shutdown)
                        break;
                }
            }
            catch (Exception e)
            {
                log.Debug("Exception reading vms directory from XenGuestAgent", e);
            }
        }

        private static void OnBatchEvent()
        {
            Program.AssertOffEventThread();

            try
            {
                if (BatchEvent != null)
                    BatchEvent(null, new EventArgs());
            }
            catch (Exception e)
            {
                log.Debug("Exception processing BatchEvent handlers");
                log.Debug(e, e);
            }
        }
    }
}
