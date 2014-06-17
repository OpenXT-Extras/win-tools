//
// Copyright (c) 2014 Citrix Systems, Inc.
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

namespace XenClientGuestService.wcf.Contracts.Clients
{


    public class XenClientGuestServiceProxy : Udbus.WCF.Client.Proxy<XenClientGuestService.wcf.Contracts.IXenClientGuestService>
    {

        public XenClientGuestServiceProxy(Udbus.WCF.Client.IBindingFactory bindingFactory, System.Uri uriEndpoint) :
            base(bindingFactory, uriEndpoint)
        {
        }

        public XenClientGuestServiceProxy(System.Uri uriEndpoint) :
            base(uriEndpoint)
        {
        }
    }
}
namespace Udbus.WCF.Client.Extensions
{


    #region
    static
    public partial class ProxyBuilderExtensions
    {

        public static void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out Udbus.WCF.Client.Proxy<XenClientGuestService.wcf.Contracts.IXenClientGuestService> proxy, Udbus.WCF.Client.DbusEndpointUriComponents dbusEndpointUri)
        {
            proxy = Udbus.WCF.Client.ProxyManager.Create<XenClientGuestService.wcf.Contracts.IXenClientGuestService>(dbusEndpointUri.CreateUri(XenClientGuestService.wcf.Contracts.Constants.DbusServiceRelativeAddress), proxyBuilder.BindingFactory);
        }

        public static void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out Udbus.WCF.Client.Proxy<XenClientGuestService.wcf.Contracts.IXenClientGuestService> proxy)
        {
            proxyBuilder.Create(out proxy, Udbus.WCF.Client.DbusEndpointUriComponents.Create(proxyBuilder.AbsoluteUribuilder));
        }

        public static void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out XenClientGuestService.wcf.Contracts.Clients.XenClientGuestServiceProxy proxy, Udbus.WCF.Client.DbusEndpointUriComponents dbusEndpointUri)
        {
            proxy = new XenClientGuestService.wcf.Contracts.Clients.XenClientGuestServiceProxy(proxyBuilder.BindingFactory, dbusEndpointUri.CreateUri(XenClientGuestService.wcf.Contracts.Constants.DbusServiceRelativeAddress));
        }

        public static void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out XenClientGuestService.wcf.Contracts.Clients.XenClientGuestServiceProxy proxy)
        {
            proxyBuilder.Create(out proxy, Udbus.WCF.Client.DbusEndpointUriComponents.Create(proxyBuilder.AbsoluteUribuilder));
        }
    }
    #endregion
}
namespace XenClientGuestService.wcf.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;


    public partial class Constants
    {

        public const string DbusServiceRelativeAddress = "XenClientGuestService";
    }

    [System.ServiceModel.ServiceContractAttribute()]
    public interface IXenClientGuestService
    {

        // uuid
        // out value "s"
        string uuid
        {
            [OperationContract]
            get;
        }

        [OperationContract]
        bool TogglePluginAutorun(bool toggle);

        [OperationContract]
        void SetAcceleration(ulong acceleration);

        [OperationContract]
        bool XenStoreWrite(string path, string value);
    }
}
