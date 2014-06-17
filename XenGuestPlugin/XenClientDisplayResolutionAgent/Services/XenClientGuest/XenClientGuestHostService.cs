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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using XenClientGuestService.wcf.Contracts.Clients;
using Udbus.Types;
using Udbus.WCF.Client;
using Udbus.WCF.Client.Extensions;

namespace XenClientDisplayResolutionAgent.Services.XenClientGuest
{
    internal class XenClientGuestHostService : IHostService
    {
        DbusEndpointAbsoluteUriBuilder EndpointBuilder;
        ProxyBuilder ProxyBuilder;

        internal XenClientGuestHostService()
        {
            EndpointBuilder = new DbusEndpointAbsoluteUriBuilder();
            ProxyBuilder = new ProxyBuilder(EndpointBuilder);
        }


        #region IHostService Members

        public bool XenStoreWrite(string path, string value)
        {
            XenClientGuestServiceProxy XenClientGuestService;
            ProxyBuilder.Create(out XenClientGuestService, DbusEndpointUriComponents.Create(EndpointBuilder));
            try
            {
                return XenClientGuestService.ProxyInterface.XenStoreWrite(path, value);
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
            return;
        }

        #endregion
    }
}
