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
using System.IO;
using System.Runtime.InteropServices;
using com.citrix.xenclient.updatemgr;
using com.citrix.xenclient.updatemgr.wcf.Contracts.Clients;
using com.citrix.xenclient.usbdaemon;
using com.citrix.xenclient.usbdaemon.wcf.Contracts.Clients;
using com.citrix.xenclient.xenmgr.host;
using com.citrix.xenclient.xenmgr.host.wcf.Contracts.Clients;
using com.citrix.xenclient.xenmgr.guestreq;
using com.citrix.xenclient.xenmgr.guestreq.wcf.Contracts.Clients;
using com.citrix.xenclient.xenmgr.vm.wcf.Contracts.Clients;
using com.citrix.xenclient.xenmgr.wcf.Contracts.Clients;
using XenClientGuestService.wcf.Contracts.Clients;
using Udbus.Types;
using Udbus.WCF.Client;
using Udbus.WCF.Client.Extensions;
using XenGuestPlugin.Types;

namespace XenGuestPlugin.Services.XenClientGuest
{
    internal class XenClientGuestHostService : IHostService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private XenGuestAgentLib.XenGuestServices xgsc = null;
        DbusEndpointAbsoluteUriBuilder EndpointBuilder;
        ProxyBuilder ProxyBuilder;
        XenmgrProxy ConfigSignal;
        XenmgrProxy StateSignal;
        HostProxy HostSignal;
        UpdatemgrProxy UpdateSignal;
        UsbdaemonProxy USBSignal;
        GuestreqProxy RequestSignal;

        internal XenClientGuestHostService()
        {
            xgsc = new XenGuestAgentLib.XenGuestServicesClass();
            EndpointBuilder = new DbusEndpointAbsoluteUriBuilder();
            ProxyBuilder = new ProxyBuilder(EndpointBuilder);
            CreateSignalProxies();
            SignalRegister();
        }

        private void CreateSignalProxies()
        {
            ProxyBuilder.Create(out ConfigSignal, DbusEndpointUriComponents.Create(EndpointBuilder));
            ProxyBuilder.Create(out StateSignal, DbusEndpointUriComponents.Create(EndpointBuilder));
            ProxyBuilder.Create(out HostSignal, DbusEndpointUriComponents.Create(EndpointBuilder));
            ProxyBuilder.Create(out UpdateSignal, DbusEndpointUriComponents.Create(EndpointBuilder));
            ProxyBuilder.Create(out USBSignal, DbusEndpointUriComponents.Create(EndpointBuilder));
            ProxyBuilder.Create(out RequestSignal, DbusEndpointUriComponents.Create(EndpointBuilder));
        }

        private void SignalRegister()
        {
            // Not required unless we display non-running vms
            // ManagerSignal.VmCreated += OnVMsChanged;
            // ManagerSignal.VmDeleted += OnVMsChanged;
            ConfigSignal.VmConfigChanged += OnVMsChanged;
            StateSignal.VmStateChanged += OnVMsChanged;

            HostSignal.StorageSpaceLow += OnStorageSpaceLow;
            UpdateSignal.UpdateStateChange += OnUpdateStateChange;
            USBSignal.DeviceRejected += OnDeviceRejected;
            RequestSignal.RequestedAttention += OnRequestedAttention;
        }

        internal void OnStorageSpaceLow(object sender, StorageSpaceLowArgs e)
        {
            OnAlert("com.citrix.xenclient.xenmgr.host", "storage_space_low", new string[] { e.PercentFree.ToString() });
        }

        internal void OnUpdateStateChange(object sender, UpdateStateChangeArgs e)
        {
            if (e.State.ToLower() == "downloaded-files")
            {
                OnAlert("com.citrix.xenclient.updatemgr", "update_state_change:downloaded_files", new string[] {});
            }
        }

        internal void OnDeviceRejected(object sender, DeviceRejectedArgs e)
        {
            OnAlert("com.citrix.xenclient.usbdaemon", "device_rejected", new string[] { e.DeviceName, e.Reason });
        }
        
        internal void OnRequestedAttention(object sender, RequestedAttentionArgs e)
        {
            VmProxy vmProxy;
            ProxyBuilder.Create(out vmProxy, DbusEndpointUriComponents.Create("com.citrix.xenclient.xenmgr", e.ObjPath.ToString()));
            int slot = vmProxy.ProxyInterface.slot;

            if (slot >= 0 && slot <= 9)
            {
                OnAlert("com.citrix.xenclient.xenmgr.host.guestreq", "requested_attention:switchable", new string[] { vmProxy.ProxyInterface.name, slot.ToString() });
            }
            else
            {
                OnAlert("com.citrix.xenclient.xenmgr.host.guestreq", "requested_attention", new string[] { vmProxy.ProxyInterface.name, slot.ToString() });
            }
        }

        #region IHostService Members

        public event VMChangedEventHandler VMsChanged;
        public event AlertEventHandler NewAlert;

        public string UUID
        {
            get
            {
                XenClientGuestServiceProxy XenClientGuestService;
                ProxyBuilder.Create(out XenClientGuestService, DbusEndpointUriComponents.Create(EndpointBuilder));
                try
                {
                    return XenClientGuestService.ProxyInterface.uuid;
                }
                finally
                {
                    XenClientGuestService.Close();
                }
            }
        }

        public IDictionary<int, XenVM> GetVMs()
        {
            var list = new SortedList<int, XenVM>();

            XenmgrProxy vmManager;
            UdbusObjectPath[] vmPaths;

            ProxyBuilder.Create(out vmManager, DbusEndpointUriComponents.Create(EndpointBuilder));

            vmManager.ProxyInterface.list_vms(out vmPaths);

            VmProxy vmProxy;
            string type;
            Image image = null;
            int slot;

            foreach (Udbus.Types.UdbusObjectPath path in vmPaths)
            {
                ProxyBuilder.Create(out vmProxy, DbusEndpointUriComponents.Create(EndpointBuilder, "com.citrix.xenclient.xenmgr", path.Path));
                bool hidden = false;

                if (vmProxy.ProxyInterface.slot != 0) // If not uivm
                {
                    // Hidden if hidden :)
                    if (vmProxy.ProxyInterface.hidden_in_ui)
                    {
                        continue;
                    }

                    // Hidden if not running
                    if (vmProxy.ProxyInterface.state != "running")
                    {
                        continue;
                    }

                    // Hidden if not user VM type
                    type = vmProxy.ProxyInterface.type;
                    if (type != "svm" && type != "pvm")
                    {
                        continue;
                    }

                    image = getImage(vmProxy);

                } // Ends if not uivm
                else // Else uivm
                {
                    type = vmProxy.ProxyInterface.type;
                    hidden = true;

                } // Ends else uivm

                slot = vmProxy.ProxyInterface.slot;
                // Hidden if out of range
                if (slot > 9)
                {
                    continue;
                }

                list.Add(slot, new XenVM() { Hidden = hidden, Image = image, Name = vmProxy.ProxyInterface.name, Slot = slot, UUID = vmProxy.ProxyInterface.uuid, Path=path });
            }

            return list;
        }

        private Image getImage(VmProxy vmProxy)
        {
            // Default image
            Image image = (Bitmap)Properties.Resources.Blue_VM.Clone();
            byte[] imageBytes;

            if (string.IsNullOrEmpty(vmProxy.ProxyInterface.image_path))
            {
                return image;
            }

            try
            {
                vmProxy.ProxyInterface.read_icon(out imageBytes);
                using (MemoryStream stream = new MemoryStream(imageBytes))
                {
                    image = new Bitmap(stream);
                }
            }
            catch (Exception e)
            {
                log.Info(e);
#if DEBUG
                System.Windows.Forms.MessageBox.Show(e.Message);
#endif
            }

            return image;
        }

        public IDictionary<int, XenUSB> GetUSB()
        {
            var list = new SortedList<int, XenUSB>();
            string uuid = this.UUID;

            UsbdaemonProxy usbManager;
            ProxyBuilder.Create(out usbManager, DbusEndpointUriComponents.Create(EndpointBuilder));

            int[] devices;
            usbManager.ProxyInterface.list_devices(out devices);

            string name;
            int stateID;
            USBState state;
            string vm_name;
            string detail;

            foreach (int device in devices)
            {
                usbManager.ProxyInterface.get_device_info(device, uuid, out name, out stateID, out vm_name, out detail);

                if (stateID < 0)
                {
                    continue;
                }

                switch (stateID)
                {
                    case 0:
                        {
                            state = USBState.Unassigned;
                            break;
                        }
                    case 4:
                    case 5:
                    case 6:
                        {
                            state = USBState.Assigned;
                            break;
                        }
                    default:
                        {
                            state = USBState.Unavailable;
                            break;
                        }
                }

                list.Add(device, new XenUSB() { State = state, Name = name, Description = detail, AssignedVM = vm_name });
            }

            return list;
        }

        public void AssignUSB(int id)
        {
            UsbdaemonProxy usbManager;
            ProxyBuilder.Create(out usbManager, DbusEndpointUriComponents.Create(EndpointBuilder));

            usbManager.ProxyInterface.assign_device(id, this.UUID);
        }

        public void UnassignUSB(int id)
        {
            UsbdaemonProxy usbManager;
            ProxyBuilder.Create(out usbManager, DbusEndpointUriComponents.Create(EndpointBuilder));

            usbManager.ProxyInterface.unassign_device(id);
        }

        private bool CheckVmFocus()
        {
            com.citrix.xenclient.input.wcf.Contracts.Clients.InputProxy input;
            ProxyBuilder.Create(out input, DbusEndpointUriComponents.CreateForPath(EndpointBuilder, "/"));
            com.citrix.xenclient.xenmgr.wcf.Contracts.Clients.XenmgrProxy xenmgr;
            ProxyBuilder.Create(out xenmgr, DbusEndpointUriComponents.CreateForPath(EndpointBuilder, "/"));
            bool gotFocus = false;
            try
            {
                // Get the focus vm's domid.
                Int32 focusDomid;
                input.ProxyInterface.get_focus_domid(out focusDomid);
                Udbus.Types.UdbusObjectPath pathFocusVm;
                // Find the focus vm's path from the domid.
                xenmgr.ProxyInterface.find_vm_by_domid(focusDomid, out pathFocusVm);
                com.citrix.xenclient.xenmgr.vm.wcf.Contracts.Clients.VmProxy focusVm;
                ProxyBuilder.Create(out focusVm, DbusEndpointUriComponents.Create(EndpointBuilder, "com.citrix.xenclient.xenmgr", pathFocusVm.Path));
                try
                {
                    // See if the focus vm's uuid matches our uuid.
                    gotFocus = focusVm.ProxyInterface.uuid == this.UUID;
                }
                finally
                {
                    focusVm.Close();
                }
            }
            finally
            {
                input.Close();
                xenmgr.Close();
            }

            return gotFocus;
        }

        public void Switch(XenVM vm)
        {
            // Don't allow this VM to switch if it does not have focus.
            if (CheckVmFocus()) // If this vm has focus
            {
                com.citrix.xenclient.xenmgr.vm.wcf.Contracts.Clients.VmProxy switchVm;
                ProxyBuilder.Create(out switchVm, DbusEndpointUriComponents.Create(EndpointBuilder, "com.citrix.xenclient.xenmgr", vm.Path.Path));
                try
                {
                    switchVm.ProxyInterface.@switch();
                }
                finally
                {
                    switchVm.Close();
                }
            } // Ends if this vm has focus
        }

        public bool SetAutorun(bool doToggle)
        {
            XenClientGuestServiceProxy XenClientGuestService;
            ProxyBuilder.Create(out XenClientGuestService, DbusEndpointUriComponents.Create(EndpointBuilder));
            try
            {
                return XenClientGuestService.ProxyInterface.TogglePluginAutorun(doToggle);
            }
            finally
            {
                XenClientGuestService.Close();
            }
        }

        public void SetAcceleration(int a)
        {
            XenClientGuestServiceProxy XenClientGuestService;
            ProxyBuilder.Create(out XenClientGuestService, DbusEndpointUriComponents.Create(EndpointBuilder));
            try
            {
                XenClientGuestService.ProxyInterface.SetAcceleration((uint)a);
            }
            finally
            {
                XenClientGuestService.Close();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Marshal.ReleaseComObject(xgsc);
        }

        #endregion

        public void OnVMsChanged(object sender, EventArgs e)
        {
            if (VMsChanged != null)
            {
                VMsChanged();
            }
        }

        public void OnAlert(string iface, string member, string[] args)
        {
            if (NewAlert != null)
            {
                NewAlert(iface, member, args);
            }
        }
    }
}
