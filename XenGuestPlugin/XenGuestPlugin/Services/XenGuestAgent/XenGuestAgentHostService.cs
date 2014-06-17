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
using System.Runtime.InteropServices;
using XenGuestPlugin.Types;

namespace XenGuestPlugin.Services
{
    public class XenGuestAgentHostService : IHostService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private XenGuestAgentLib.XenGuestServices xgsc = null;
        public event VMChangedEventHandler VMsChanged;
        public event AlertEventHandler NewAlert;

        public XenGuestAgentHostService()
        {
            xgsc = new XenGuestAgentLib.XenGuestServicesClass();
            XenVmsCache.BatchEvent += XenVmsCache_BatchEvent;
            XenAlert.Alert += XenAlert_Alert;
            XenVmsCache.Initialize(xgsc);
            XenAlert.Initialize(xgsc);
        }

        void XenVmsCache_BatchEvent(object sender, EventArgs e)
        {
            if (VMsChanged != null)
            {
                VMsChanged();
            }
        }

        void XenAlert_Alert(Array alertParams)
        {
            if (NewAlert != null)
            {
                string iface = (string)alertParams.GetValue(0);
                string member = (string)alertParams.GetValue(1);
                string[] args = new string[alertParams.Length - 2];

                if (alertParams.Length > 2)
                {
                    // Alert defaults to all available params.
                    Array.Copy(alertParams, 2, args, 0, alertParams.Length - 2);
                }

                NewAlert(iface, member, args);
            }
        }

        #region IHostService Members

        public string UUID
        {
            get
            {
                return xgsc.GetUuid();
            }
        }

        public IDictionary<int, XenVM> GetVMs()
        {
            var VMs = new SortedList<int, XenVM>();
            XenVM vm = null;
            for (int i = 0; i < XenVmsCache.VMs.Length; i++)
            {
                vm = XenVmsCache.VMs[i];
                VMs.Add(vm.Slot, vm);
            }
            return VMs;
        }

        public IDictionary<int, XenUSB> GetUSB()
        {
            return new SortedList<int, XenUSB>();
        }

        public void AssignUSB(int id)
        {
        }

        public void UnassignUSB(int id)
        {
        }

        public void Switch(XenVM vm)
        {
            xgsc.SwitchToVm((uint)vm.Slot);
        }

        public bool SetAutorun(bool doToggle)
        {
            return xgsc.TogglePluginAutorun(doToggle);
        }
        
        public void SetAcceleration(int a)
        {
            xgsc.SetAcceleration((uint)a);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            XenAlert.Shutdown = true;
            XenVmsCache.Shutdown = true;

            xgsc.UnregisterVmsEvent();
            xgsc.UnregisterAlertsEvent();
            Marshal.ReleaseComObject(xgsc);
        }

        #endregion
    }
}
