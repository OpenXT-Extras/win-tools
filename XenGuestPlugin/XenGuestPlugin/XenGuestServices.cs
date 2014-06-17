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
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Resources;
using System.Reflection;
using XenGuestPlugin.Agent;

namespace XenGuestPlugin
{
    public class XEN_VM
    {
        public string uuid = "";
        public string name = "";
        public UInt16 domid = 0;
        public bool hidden = false;
        public bool showSwitcher = true;
        public bool uivm = false;
        public UInt32 slot = 0;
        public Image image;
    }

    public class XenGuestServices
    {
        private IAgent xgsc = null;

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string uuid;
        private UInt16 domid;
        private bool showSwitcher;
        private string vmsEventName;
        private string alertsEventName;

        public XenGuestServices(IAgent agent)
        {
            xgsc = agent;
            uuid = xgsc.GetUuid();
            domid = xgsc.GetDomId();
            showSwitcher = xgsc.GetShowSwitcher();
            vmsEventName = xgsc.RegisterVmsEvent();
            alertsEventName = xgsc.RegisterAlertsEvent();
        }

        public void Uninitialize()
        {
            if (xgsc != null)
            {
                xgsc.UnregisterVmsEvent();
                xgsc.UnregisterAlertsEvent();
                xgsc.ReleaseComObject();
                xgsc = null;
            }
        }

        public string GetUuid()
        {
            return uuid;
        }

        public UInt16 GetDomid()
        {
            return domid;
        }

        public string GetVmsEventName()
        {
            return vmsEventName;
        }

        public IEnumerable<XEN_VM> GetVMs()
        {
            return xgsc.GetVMs();
        }

        public void SwitchToVM(XEN_VM xenvm)
        {
            //xgsc.SwitchToVm(xenvm.slot);
            xgsc.SwitchToVm(xenvm.slot);
        }

        public string GetAlertsEventName()
        {
            return alertsEventName;
        }

        public bool GetHasFocus()
        {
            return xgsc.VmHasFocus();
        }

        public Array GetAlert()
        {
            Array alert;
            XenGuestAgentLib.XenAlertType ret = xgsc.GetNextAlert(out alert);

            if (ret == XenGuestAgentLib.XenAlertType.XenAlertEmpty)
            {
                return null;
            }

            else if (ret == XenGuestAgentLib.XenAlertType.XenAlertError)
            {
                log.Debug("Bad JSON signal received: " + alert.GetValue(0));
            }

            return alert;
        }

        public bool ToggleAutorun(bool doToggle)
        {
            return xgsc.TogglePluginAutorun(doToggle);
        }

        public void SetAcceleration(uint a)
        {
            xgsc.SetAcceleration(a);
        }

        public bool GetShowSwitcher()
        {
            return showSwitcher;
        }
    }
}
