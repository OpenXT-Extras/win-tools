//
// Copyright (c) 2011 Citrix Systems, Inc.
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
using System.Threading;
using System.Reflection;
using System.Security.AccessControl;

namespace XenGuestPlugin
{
    public static class XenVmsCache
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ChangeableDictionary<String, XEN_VM> vms = new ChangeableDictionary<String, XEN_VM>();

        internal static XenGuestServices xgs;

        private static System.Threading.EventWaitHandle evvms = null;
        private const Int32 CACHE_MONITOR_TIMEOUT = 2000;

        public static bool Shutdown = false;
        public static event EventHandler BatchEvent;

        public static XEN_VM[] VMs
        {
            get
            {
                lock (vms)
                {
                    XEN_VM[] _vms = new XEN_VM[vms.Count];
                    vms.Values.CopyTo(_vms, 0);
                    return _vms;
                }
            }
        }

        public static void Initialize()
        {
            Program.AssertOffEventThread();
            Thread thread;
            string evname;
            EventWaitHandleSecurity evsec;
            EventWaitHandleAccessRule evrule;
            string user;

            xgs = Program.GetXGS();

            evname = xgs.GetVmsEventName();
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

       
        public static void UpdateVms()
        {
            Program.AssertOffEventThread();

            uint slot = 0;
            uint maxSlot = 0;
            List<XEN_VM> sortedVMs = null;
            IEnumerable<XEN_VM> VMs = null;
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
             
                VMs = xgs.GetVMs();

                foreach (XEN_VM xenvm in VMs)
                {
                    if (xenvm.slot >= 0 && xenvm.slot <= 9)
                    {
                        if (xenvm.slot > maxSlot)
                            maxSlot = xenvm.slot;
                    }
                    else if (xenvm.slot >= 100)
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

            sortedVMs = new List<XEN_VM>();

            while (slot <= maxSlot)
            {
                foreach (XEN_VM xenvm in VMs)
                    if (xenvm.slot == slot)
                        sortedVMs.Add(xenvm);
                slot++;
            }

            foreach (XEN_VM xenvm in sortedVMs)
                lock (vms)
                    vms[xenvm.uuid] = xenvm;

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
