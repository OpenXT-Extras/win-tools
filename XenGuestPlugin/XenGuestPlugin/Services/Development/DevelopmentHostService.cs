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
using System.Drawing;
using XenGuestPlugin.Types;

namespace XenGuestPlugin.Services.Development
{
    internal class DevelopmentHostService : IHostService
    {
        #region IHostService Members

        public event VMChangedEventHandler VMsChanged;
        public event AlertEventHandler NewAlert;

        public string UUID
        {
            get
            {
                return "123";
            }
        }

        public IDictionary<int, XenVM> GetVMs()
        {
            var list = new SortedList<int, XenVM>();
            list.Add(5, new XenVM() { Hidden = false, Image = (Bitmap)Properties.Resources.Blue_VM.Clone(), Name = "WinVista", Slot = 1, UUID = "1" });
            list.Add(2, new XenVM() { Hidden = false, Image = (Bitmap)Properties.Resources.Blue_VM.Clone(), Name = "Test Win 7", Slot = 2, UUID = "12" });
            list.Add(3, new XenVM() { Hidden = false, Image = (Bitmap)Properties.Resources.Blue_VM.Clone(), Name = "Windows 7", Slot = 3, UUID = "123" });
            list.Add(4, new XenVM() { Hidden = false, Image = (Bitmap)Properties.Resources.Blue_VM.Clone(), Name = "Windows XP with SP2", Slot = 4, UUID = "1234" });
            list.Add(1, new XenVM() { Hidden = false, Image = (Bitmap)Properties.Resources.Blue_VM.Clone(), Name = "A really long name to see what happens", Slot = 5, UUID = "12345" });
            return list;
        }

        public IDictionary<int, XenUSB> GetUSB()
        {
            var list = new SortedList<int, XenUSB>();
            list.Add(1, new XenUSB() { State = USBState.Assigned, Name = "Mouse", Description = "Mousey", AssignedVM = "TestVM1" });
            list.Add(2, new XenUSB() { State = USBState.Unassigned, Name = "Camera", Description = "Flashy", AssignedVM = "TestVM2" });
            list.Add(3, new XenUSB() { State = USBState.Unavailable, Name = "Memory Stick", Description = "Sticky", AssignedVM = "TestVM3" });
            return list;
        }

        public void AssignUSB(int id)
        {
        }

        public void UnassignUSB(int id)
        {
        }
        
        public void Switch(XenVM vm)
        {
            OnAlert("com.citrix.xenclient.xenmgr.host", "storage_space_low", new string[] { "69" });
        }

        public bool SetAutorun(bool doToggle)
        {
            if (VMsChanged != null)
            {
                VMsChanged();
            }
            return true;
        }

        public void SetAcceleration(int a)
        {
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void OnAlert(string iface, string member, string[] args)
        {
            if (NewAlert != null)
            {
                NewAlert(iface, member, args);
            }
        }
    }
}
