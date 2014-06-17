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

namespace XenClientGuestService.wcf.Host
{
    using System.Linq;
    using System.Collections.Generic;

    public class MakeServiceHostCreationDataXenClientGuestWCFService : Udbus.WCF.Service.Host.IMakeServiceHostCreationData
    {

        static XenClientGuestWCFService CreateService()
        {
            XenClientGuestWCFService wcfService = new XenClientGuestWCFService();
            return wcfService;
        }

        static Udbus.WCF.Service.Host.WCFServiceCreationData<XenClientGuestWCFService> CreateServiceCreationData()
        {
            return new Udbus.WCF.Service.Host.WCFServiceCreationData<XenClientGuestWCFService>(CreateService, GetDbusServiceCreationParams());
        }

        static Udbus.WCF.Service.Host.DbusServiceCreationParams GetDbusServiceCreationParams()
        {
            return new Udbus.WCF.Service.Host.DbusServiceCreationParams(global::XenClientGuestService.wcf.Contracts.Constants.DbusServiceRelativeAddress, typeof(global::XenClientGuestService.wcf.Contracts.IXenClientGuestService));
        }

        public static Udbus.WCF.Service.Host.DbusServiceRegistrationParams GetHostDbusServiceRegistrationParams()
        {
            return new Udbus.WCF.Service.Host.DbusServiceRegistrationParams(GetDbusServiceCreationParams(), typeof(global::XenClientGuestService.wcf.XenClientGuestWCFService));
        }

        #region Udbus.WCF.Service.Host.IMakeServiceHostCreationData functions
        public bool ContainsService(params System.Type[] serviceTypes)
        {
            return serviceTypes.Contains(typeof(global::XenClientGuestService.wcf.XenClientGuestWCFService));
        }

        public Udbus.WCF.Service.Host.DbusServiceRegistrationParams GetDbusServiceRegistrationParams()
        {
            return GetHostDbusServiceRegistrationParams();
        }

        public Udbus.WCF.Service.Host.ServiceHostCreationData MakeServiceHostCreationData(Udbus.WCF.Host.WCFServiceParams wcfserviceparams, params System.Uri[] uriBase)
        {   
            Udbus.WCF.Service.Host.ServiceHostCreationData create = Udbus.WCF.Service.Host.DbusServiceHost.CreateWithData<global::XenClientGuestService.wcf.XenClientGuestWCFService>(CreateServiceCreationData
                    , uriBase);
            return create;
        }
        #endregion
    }
}
namespace XenClientGuestService.wcf.Hosts.Init
{
    public partial class InitHostMakerRegistry
    {

        private Udbus.WCF.Service.Host.RegistryListEntryBase addXenClientGuestService_WCF_XenClientGuestWCFService = new Udbus.WCF.Service.Host.RegistryListEntry<global::XenClientGuestService.wcf.Host.MakeServiceHostCreationDataXenClientGuestWCFService>();
    }
}
