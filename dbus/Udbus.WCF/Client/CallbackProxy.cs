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
using System.Linq;
using System.Text;

namespace Udbus.WCF.Client
{
    public class CallbackProxyBase<TProxyInterface_, TCallback_> : Udbus.WCF.Client.Proxy<TProxyInterface_>
        where TProxyInterface_ : class
        where TCallback_ : class//, new()
    {
        public delegate TCallback_ CreateCallbackDelegate();

        TCallback_ callback = null;
        CreateCallbackDelegate createCallback;

        public CallbackProxyBase(IBindingFactory bindingFactory, System.Uri uriEndpoint, CreateCallbackDelegate createCallback)
            : base(bindingFactory, uriEndpoint)
        {
            this.createCallback = createCallback;
        }

        public CallbackProxyBase(System.Uri uriEndpoint, CreateCallbackDelegate createCallback)
            : base(ProxyManager.DefaultBindingFactory, uriEndpoint)
        {
            this.createCallback = createCallback;
        }

        #region Udbus.WCF.Client.Proxy overrides
        #region Udbus.WCF.Client.Proxy Properties
        /// <summary>
        /// Lazy load WCF proxy.
        /// </summary>
        public override TProxyInterface_ ProxyInterface
        {
            get
            {
                if (this.proxyInterface == null) // If haven't initialised proxy yet
                {
                    this.InitialiseProxyImpl();

                } // Ends if haven't initialised proxy yet

                return this.proxyInterface;
            }
        }

        #endregion // Udbus.WCF.Client.Proxy Properties
        #endregion // Udbus.WCF.Client.Proxy

        private void InitialiseProxyImpl()
        {
            TProxyInterface_ proxy;
            System.ServiceModel.ICommunicationObject commsProxy;
            TCallback_ callback = this.createCallback();

            BuildProxy(this.uriEndpoint, this.bindingFactory, callback, out proxy, out commsProxy
                , this.MaxClientAttempts, this.WaitForNextClientAttemptInMilliseconds);

            this.callback = callback;
            this.proxyInterface = proxy;
            this.comms = commsProxy;
        }

        static private void BuildProxy<TProxyInterface, TCallback>(System.Uri uriEndpoint, Udbus.WCF.Client.IBindingFactory bindingFactory
            , TCallback callback
            , out TProxyInterface proxy, out System.ServiceModel.ICommunicationObject comms
            , uint maxTries, int wait)
            where TProxyInterface : class
        {
            System.ServiceModel.Channels.Binding binding = bindingFactory.Create();
            System.ServiceModel.InstanceContext context = new System.ServiceModel.InstanceContext(callback);
            System.ServiceModel.DuplexChannelFactory<TProxyInterface> factory = new System.ServiceModel.DuplexChannelFactory<TProxyInterface>(
                context
                , binding
                , new System.ServiceModel.EndpointAddress(uriEndpoint)
            );
            proxy = factory.CreateChannel();
            comms = proxy as System.ServiceModel.ICommunicationObject;
            Proxy<TProxyInterface_>.AttemptClientOpen(ref comms, ref proxy, factory, maxTries, wait);
        }

        public virtual TCallback_ Callback
        {
            get
            {
                if (this.callback == null || this.proxyInterface == null) // If haven't initialised callback or proxy yet
                {
                    this.InitialiseProxyImpl();

                } // Ends if haven't initialised callback or proxy yet

                return this.callback;
            }
        }
    } // Ends class CallbackProxyBase

    public class CallbackProxy<TProxyInterface_, TCallback_> : Udbus.WCF.Client.CallbackProxyBase<TProxyInterface_, TCallback_>
        where TProxyInterface_ : class
        where TCallback_ : class, new()
    {
        public CallbackProxy(IBindingFactory bindingFactory, System.Uri uriEndpoint)
            : base(bindingFactory, uriEndpoint, ProxyManager.DefaultCreateCallback<TCallback_>)
        {
        }

        public CallbackProxy(System.Uri uriEndpoint)
            : base(ProxyManager.DefaultBindingFactory, uriEndpoint, ProxyManager.DefaultCreateCallback<TCallback_>)
        {
        }
    } // Ends class CallbackProxy
}
