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
using System.Security.AccessControl;
using System.Threading;
using XenGuestPlugin.Services;
using XenGuestPlugin.Types;

namespace XenGuestPlugin
{
    public delegate void NewAlertEventHandler(Array alertParams);

    public static class XenAlert
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static System.Threading.EventWaitHandle evalerts = null;
        public static event EventHandler BatchEvent;
        public static bool Shutdown = false;
        private static List<Alert> alertList;
        private static XenGuestAgentLib.XenGuestServices xgsc = null;
        public static event NewAlertEventHandler Alert;

        public static void Initialize(XenGuestAgentLib.XenGuestServices service)
        {
            Program.AssertOffEventThread();
            Thread thread;
            string evname;
            EventWaitHandleSecurity evsec;
            EventWaitHandleAccessRule evrule;
            string user;
            xgsc = service;
            alertList = Program.GetAlertList();

            evname = xgsc.RegisterAlertsEvent();
            user = Environment.UserDomainName + "\\" + Environment.UserName;
            evalerts = EventWaitHandle.OpenExisting(evname,
                EventWaitHandleRights.ReadPermissions | EventWaitHandleRights.ChangePermissions);
            evsec = evalerts.GetAccessControl();
            evrule = new EventWaitHandleAccessRule(user,
                EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify,
                AccessControlType.Deny);
            evsec.RemoveAccessRule(evrule);

            evrule = new EventWaitHandleAccessRule(user,
                EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify,
                AccessControlType.Allow);
            evsec.AddAccessRule(evrule);
            evalerts.SetAccessControl(evsec);

            evalerts = EventWaitHandle.OpenExisting(evname);

            thread = new Thread(alertUpdater);
            thread.Name = "XenClient Alert Thread";
            thread.IsBackground = true;

            thread.Start();
        }

        private static void GetAlertCode()
        {
            Program.AssertOffEventThread();
            Array alertParams;
            XenGuestAgentLib.XenAlertType type;
            do
            {
                type = xgsc.GetNextAlert(out alertParams);
                OnAlert(type, alertParams);
            }
            while (alertParams != null);

            OnBatchEvent();
        }

        private static void OnAlert(XenGuestAgentLib.XenAlertType type, Array alertParams)
        {
            if (type == XenGuestAgentLib.XenAlertType.XenAlertEmpty)
            {
                alertParams = null;
            }

            else if (type == XenGuestAgentLib.XenAlertType.XenAlertError)
            {
                log.Debug("Bad JSON signal received: " + alertParams.GetValue(0));
            }

            if (Alert != null && alertParams != null)
            {
                Alert(alertParams);
            }
        }

        private static void alertUpdater()
        {
            try
            {
                bool signal;

                for (; ; )
                {
                    signal = evalerts.WaitOne(2000, false);
                    if (signal) // if (signaled)
                    {
                        GetAlertCode();
                    }

                    if (Shutdown)
                        break;
                }
            }
            catch (Exception e)
            {
                log.Debug("Exception retrieving alerts from XenGuestAgent", e);
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
