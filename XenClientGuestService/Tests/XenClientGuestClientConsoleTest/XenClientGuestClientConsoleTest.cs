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

#define PROXYCONFIG
#define PROXYHOST
#define PROXYMANAGER
#define PROXYVM
#define PROXYXENCLIENTGUESTSERVICE

#if PROXYCONFIG
#   define ISO_PATH
#endif // PROXYCONFIG
#if PROXYMANAGER
#   define LIST_VMS
#   define VMCHANGED
#endif // PROXYMANAGER
#if PROXYVM && LIST_VMS
#   define READICON
#endif // PROXYVM
#if PROXYHOST
#   define STORAGE_SPACE_LOW
#endif // PROXYHOST
#if PROXYXENCLIENTGUESTSERVICE
#define GET_UUID
#endif // PROXYXENCLIENTGUESTSERVICE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Udbus.WCF.Client.Extensions;

namespace XenClientGuestClientConsoleTest
{
    class Program
    {
        #region Run Client
        static void OnStorageSpaceLow(object sender, com.citrix.xenclient.xenmgr.host.StorageSpaceLowArgs e)
        {
            Console.WriteLine("WCF Storage space low: {0}", e.PercentFree);
        }

        static void OnStorageSpaceLow2(object sender, com.citrix.xenclient.xenmgr.host.StorageSpaceLowArgs e)
        {
            Console.WriteLine("WCF No seriously Storage space low !: {0}%", e.PercentFree);
        }

        static public void OnVMsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("OnVMsChanged called");
        }

        static public void OnVMsChanged2(object sender, EventArgs e)
        {
            Console.WriteLine("OnVMsChanged2 called");
        }

        static void MakeClientCalls()
        {
            Console.WriteLine("===MakeClientCalls()===");

            Udbus.WCF.Client.DbusEndpointAbsoluteUriBuilder endpointBuilder = new Udbus.WCF.Client.DbusEndpointAbsoluteUriBuilder();

            Udbus.WCF.Client.ProxyBuilder proxyBuilder = new Udbus.WCF.Client.ProxyBuilder(endpointBuilder);

            //Udbus.WCF.Client.Proxy<com.citrix.xenclient.xenmgr.config.wcf.Contracts.Iconfig> proxyConfig;
#if PROXYCONFIG
            com.citrix.xenclient.xenmgr.config.wcf.Contracts.Clients.ConfigProxy proxyConfig;
            proxyBuilder.Create(out proxyConfig
                , Udbus.WCF.Client.DbusEndpointUriComponents.Create(endpointBuilder, "com.citrix.xenclient.xenmgr", "/")
            );
#endif // PROXYCONFIG

#if PROXYHOST
            com.citrix.xenclient.xenmgr.host.wcf.Contracts.Clients.HostProxy proxyHost;
            proxyBuilder.Create(out proxyHost,
                Udbus.WCF.Client.DbusEndpointUriComponents.CreateForPath(endpointBuilder, "/host")
            );
#   if STORAGE_SPACE_LOW
            proxyHost.StorageSpaceLow += OnStorageSpaceLow;
            proxyHost.StorageSpaceLow += OnStorageSpaceLow2;
#   endif // STORAGE_SPACE_LOW
#endif // PROXYHOST

#if PROXYMANAGER
            com.citrix.xenclient.xenmgr.wcf.Contracts.Clients.XenmgrProxy proxyManager;
            proxyBuilder.Create(out proxyManager,
                Udbus.WCF.Client.DbusEndpointUriComponents.Create(endpointBuilder)
            );
#endif // PROXYMANAGER

#if PROXYXENCLIENTGUESTSERVICE
            XenClientGuestService.wcf.Contracts.Clients.XenClientGuestServiceProxy proxyXenClientGuestService;
            proxyBuilder.Create(out proxyXenClientGuestService,
                Udbus.WCF.Client.DbusEndpointUriComponents.Create(endpointBuilder)
            );
#endif // PROXYXENCLIENTGUESTSERVICE
            Udbus.WCF.Client.IProxy[] proxies = { 
#if PROXYHOST
                proxyHost
                ,
#endif // PROXYHOST
#if PROXYMANAGER
                proxyManager
                ,
#endif //PROXYMANAGER
#if PROXYCONFIG
                proxyConfig
                ,
#endif // PROXYCONFIG
#if PROXYXENCLIENTGUESTSERVICE
                proxyXenClientGuestService
#endif // PROXYXENCLIENTGUESTSERVICE
            };

#if LIST_VMS
            Udbus.Types.UdbusObjectPath[] vms;
            proxyManager.ProxyInterface.list_vms(out vms);
            if (vms.Length > 0)
            {
                Console.WriteLine("vms...");
            }

            foreach (Udbus.Types.UdbusObjectPath vm in vms)
            {
                Console.WriteLine("vm: {0}", vm);
#if PROXYVM
                com.citrix.xenclient.xenmgr.vm.wcf.Contracts.Clients.VmProxy proxyVm;
                proxyBuilder.Create(out proxyVm,
                    Udbus.WCF.Client.DbusEndpointUriComponents.Create(endpointBuilder, "com.citrix.xenclient.xenmgr", vm.Path)
                );
                System.Console.WriteLine(string.Format(" hidden_in_ui: {0}. state: {1}. type: {2}. slot: {3}"
                    ,proxyVm.ProxyInterface.hidden_in_ui, proxyVm.ProxyInterface.state, proxyVm.ProxyInterface.type, proxyVm.ProxyInterface.slot
                )   );
                proxyVm.Close();
#endif // PROXYVM
            } // Ends loop over vms
#endif // LIST_VMS

#if VMCHANGED
            proxyManager.VmConfigChanged += OnVMsChanged;
            proxyManager.VmStateChanged += OnVMsChanged2;
#endif // VMCHANGED

#if ISO_PATH
            string isoPath = proxyConfig.ProxyInterface.iso_path;
            Console.WriteLine("iso_path: {0}", isoPath);
#endif // ISO_PATH

#if GET_UUID
            string uuid = proxyXenClientGuestService.ProxyInterface.uuid;
            Console.WriteLine("VM's uuid: {0}", uuid);
#endif // GET_UUID
            Console.WriteLine();
            Console.WriteLine(@"Signal loop running... 
To trigger a storage_space_low signal open a terminal in dom0 and execute the following command:
dbus-send --system --type=signal /host com.citrix.xenclient.xenmgr.host.storage_space_low int32:96
            ");
            Console.WriteLine("Press a key to close proxy...");
            Console.ReadKey(true);

            foreach (Udbus.WCF.Client.IProxy proxyClose in proxies)
            {
                proxyClose.Close();
            }

            Console.WriteLine("Press a key to continue...");
            Console.ReadKey(true);
            Console.WriteLine();
        }
        #endregion // Run Client
        static void Main(string[] args)
        {
            MakeClientCalls();
        }
    }
}
