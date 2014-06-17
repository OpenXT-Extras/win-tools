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
using System.Windows.Forms;

namespace XenGuestPlugin
{
    public static class XenAlert
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static XenGuestServices xgs;
        private static System.Threading.EventWaitHandle evalerts = null;
        public static event EventHandler BatchEvent;
        public static bool Shutdown = false;
        private static TrayIcon systray;
        private static List<Alert> alertList;

        public static void Initialize()
        {
            Program.AssertOffEventThread();
            Thread thread;
            string evname;
            EventWaitHandleSecurity evsec;
            EventWaitHandleAccessRule evrule;
            string user;

            xgs = Program.GetXGS();
            systray = Program.GetSystray();
            alertList = Program.GetAlertList();

            evname = xgs.GetAlertsEventName();
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

            do
            {
                alertParams = xgs.GetAlert();

                if (alertParams != null && xgs.GetHasFocus())
                {
                    string aInterface = (string)alertParams.GetValue(0);
                    string aMember = (string)alertParams.GetValue(1);
                    Alert alert = LookupAlertCode(aInterface, aMember);
                    if (alert != null)
                        Program.GetSystray().showAlert(alert, alertParams);
                    else
                        log.Debug("Failed to find alert");
                }
            }
            while (alertParams != null);

            OnBatchEvent();
        }

        private static Alert LookupAlertCode(string aInterface, string aMember)
        {
            return alertList.Find(delegate(Alert a) { return ((a.Interface == aInterface) && (a.Member == aMember)); });
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
