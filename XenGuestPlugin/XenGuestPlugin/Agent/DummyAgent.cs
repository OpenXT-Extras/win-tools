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

namespace XenGuestPlugin.Agent
{
    internal class DummyAgent : IAgent
    {
        #region IAgent Members

        public string GetUuid()
        {
            return "123";
        }

        public ushort GetDomId()
        {
            return 1;
        }

        public string RegisterVmsEvent()
        {
            return "RegisterVmsEvent";
        }

        public string RegisterAlertsEvent()
        {
            return "RegisterAlertsEvent";
        }

        public void UnregisterVmsEvent()
        {
        }

        public void UnregisterAlertsEvent()
        {
        }

        public XenGuestAgentLib.XenVmInfo GetVmObject(uint i)
        {
            var vm = new XenGuestAgentLib.XenVmInfo();
          //  vm.
            return vm;
        }

        public void SwitchToVm(uint p)
        {
        }

        public void SetAcceleration(uint a)
        {
        }

        public bool GetShowSwitcher()
        {
            return true;
        }

        public bool VmHasFocus()
        {
            return false;
        }

        public XenGuestAgentLib.XenAlertType GetNextAlert(out Array alert)
        {
            alert = null;
            return XenGuestAgentLib.XenAlertType.XenAlertEmpty;
        }

        public bool TogglePluginAutorun(bool doToggle)
        {
            return true;
        }

        public void ReleaseComObject()
        {
        }

        public IEnumerable<XEN_VM> GetVMs()
        {
            return new List<XEN_VM> {
                new XEN_VM() { domid = 1, hidden = false, image = (Bitmap)Properties.Resources.Blue_VM.Clone(), uivm = false, name = "WinVista", slot = 1, uuid = "1" },
                new XEN_VM() { domid = 2, hidden = false, image = (Bitmap)Properties.Resources.Blue_VM.Clone(), uivm = false, name = "Test Win 7", slot = 2, uuid = "12" },            
                new XEN_VM() { domid = 3, hidden = false, image = (Bitmap)Properties.Resources.Blue_VM.Clone(), uivm = false, name = "Windows 7", slot = 3, uuid = "123" },
                new XEN_VM() { domid = 4, hidden = false, image = (Bitmap)Properties.Resources.Blue_VM.Clone(), uivm = false, name = "Windows XP with SP2", slot = 4, uuid = "1234" },            
                new XEN_VM() { domid = 5, hidden = false, image = (Bitmap)Properties.Resources.Blue_VM.Clone(), uivm = false, name = "A really long name to see what happens", slot = 5, uuid = "12345" },
            };
        }

        #endregion
    }
}
