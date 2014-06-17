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
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

namespace XenGuestPlugin.Agent
{
    internal class XenGuestAgent : IAgent
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private XenGuestAgentLib.XenGuestServices xgsc = null;

        public XenGuestAgent()
        {
            xgsc = new XenGuestAgentLib.XenGuestServicesClass();
        }

        #region IAgent Members

        public string GetUuid()
        {
            return xgsc.GetUuid();
        }

        public ushort GetDomId()
        {
            return xgsc.GetDomId();
        }

        public string RegisterVmsEvent()
        {
            return xgsc.RegisterVmsEvent();
        }

        public string RegisterAlertsEvent()
        {
            return xgsc.RegisterAlertsEvent();
        }

        public void UnregisterVmsEvent()
        {
            xgsc.UnregisterVmsEvent();
        }

        public void UnregisterAlertsEvent()
        {
            xgsc.UnregisterAlertsEvent();
        }

        public IEnumerable<XEN_VM> GetVMs()
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
                XEN_VM xenvm = new XEN_VM();
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
                xenvm.uuid = xvic.GetUuid();
                xenvm.name = xvic.GetName();
                xenvm.domid = xvic.GetDomId();
                xenvm.slot = xvic.GetSlot();
                xenvm.hidden = xvic.IsHidden();
                xenvm.uivm = xvic.IsUivm();
                xenvm.showSwitcher = xvic.IsSwitcherShown();

                if (xvic.HasImage())
                {
                    ms = new MemoryStream();
                    arr = xvic.GetImage();

                    for (j = 0; j < (uint)arr.GetLength(0); j++)
                    {
                        ms.WriteByte((byte)arr.GetValue(j));
                    }
                    xenvm.image = Image.FromStream(ms);
                }
                else
                {
                    blueImage = (Bitmap)Properties.Resources.Blue_VM.Clone();
                    xenvm.image = blueImage;
                }

                Marshal.ReleaseComObject(xvic);
                xvic = null;

                yield return xenvm;
            }
        }

        public XenGuestAgentLib.XenVmInfo GetVmObject(uint i)
        {
            return xgsc.GetVmObject(i);
        }

        public void SwitchToVm(uint p)
        {
            xgsc.SwitchToVm(p);
        }

        public void SetAcceleration(uint a)
        {
            xgsc.SetAcceleration(a);
        }

        public bool GetShowSwitcher()
        {
            return xgsc.GetShowSwitcher();
        }

        public bool VmHasFocus()
        {
            return xgsc.VmHasFocus();
        }

        public XenGuestAgentLib.XenAlertType GetNextAlert(out Array alert)
        {
            return xgsc.GetNextAlert(out alert);
        }

        public bool TogglePluginAutorun(bool doToggle)
        {
            return xgsc.TogglePluginAutorun(doToggle);
        }

        public void ReleaseComObject()
        {
            Marshal.ReleaseComObject(xgsc);
        }

        #endregion
    }
}
