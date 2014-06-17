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

namespace Udbus.WCF.Client
{
    public interface IProxy
    {
        void Close();
    }

    public class Proxy<TProxyInterface_>
        : IProxy
        where TProxyInterface_ : class
    {
        #region Fields
        const uint DefaultMaxClientAttempts = 15;
        const int DefaultWaitForNextClientAttemptInMilliseconds = 500;

        protected System.ServiceModel.ICommunicationObject comms = null;
        protected TProxyInterface_ proxyInterface = null;
        protected IBindingFactory bindingFactory;
        protected System.Uri uriEndpoint;

        //private System.ServiceModel.ICommunicationObject comms = null;
        //private TProxyInterface_ proxyInterface = null;
        //IBindingFactory bindingFactory;
        //System.Uri uriEndpoint;

        #endregion // Fields

        #region Properties
        /// <summary>
        /// Lazy load WCF proxy.
        /// </summary>
        public virtual TProxyInterface_ ProxyInterface
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

        protected uint MaxClientAttempts { get; set; }
        protected int WaitForNextClientAttemptInMilliseconds { get; set; }

        #endregion // Properties

        #region Constructors
        internal protected Proxy(IBindingFactory bindingFactory, System.Uri uriEndpoint)
        {
            this.bindingFactory = bindingFactory;
            this.uriEndpoint = uriEndpoint;
            this.MaxClientAttempts = DefaultMaxClientAttempts;
            this.WaitForNextClientAttemptInMilliseconds = DefaultWaitForNextClientAttemptInMilliseconds;
        }

        internal protected Proxy(System.Uri uriEndpoint)
            : this(ProxyManager.DefaultBindingFactory, uriEndpoint)
        {
        }

        //protected Proxy(System.Uri uriEndpoint, System.ServiceModel.ICommunicationObject comms, TProxyInterface_ proxy)
        //{
        //    this.comms = comms;
        //    this.proxy = proxy;
        //}
        #endregion // Constructors

        #region Open Connection
        internal static void AttemptClientOpen<T>(ref System.ServiceModel.ICommunicationObject comms, ref T proxy, System.ServiceModel.ChannelFactory<T> factory, uint maxTries, int wait)
        {
            uint attempts = 0;
            while (attempts < maxTries)
            {
                try
                {
                    comms.Open();
                    break;
                }
                catch (System.ServiceModel.EndpointNotFoundException /*exNoEndpoint*/)
                {
                    comms.Abort();

                    if (attempts == maxTries - 1)
                    {
                        throw;
                    }

                    proxy = factory.CreateChannel();
                    comms = proxy as System.ServiceModel.ICommunicationObject;
                    System.Threading.Thread.Sleep(wait);
                    //Console.WriteLine(string.Format("{0} Attempt {1}", proxy.GetType().Name, attempts + 1));
                    ++attempts;
                }
            }
            //Console.WriteLine("{0} Proxy State after opening: {1}", proxy.GetType().Name, comms.State);
        }
        #endregion // Open Connection

        #region WCF Proxy Creation
        static private void BuildProxy<TProxyInterface>(System.Uri uriEndpoint, IBindingFactory bindingFactory, out TProxyInterface proxy, out System.ServiceModel.ICommunicationObject comms
            , uint maxTries, int wait)
            where TProxyInterface : class
        {
            System.ServiceModel.Channels.Binding binding = bindingFactory.Create();
            System.ServiceModel.ChannelFactory<TProxyInterface> factory = new System.ServiceModel.ChannelFactory<TProxyInterface>(
                 binding
                , new System.ServiceModel.EndpointAddress(uriEndpoint)
            );
            proxy = factory.CreateChannel();
            comms = proxy as System.ServiceModel.ICommunicationObject;
            AttemptClientOpen(ref comms, ref proxy, factory, maxTries, wait);
        }

        private void InitialiseProxyImpl()
        {
            TProxyInterface_ proxy;
            System.ServiceModel.ICommunicationObject commsProxy;

            BuildProxy(this.uriEndpoint, this.bindingFactory, out proxy, out commsProxy
                , this.MaxClientAttempts, this.WaitForNextClientAttemptInMilliseconds);

            this.proxyInterface = proxy;
            this.comms = commsProxy;
        }

        public void InitializeProxy()
        {
            if (this.proxyInterface == null) // If haven't initialised proxy yet
            {
                this.InitialiseProxyImpl();

            } // Ends if haven't initialised proxy yet
        }
        #endregion // WCF Proxy Creation

        #region Close Connection
        public void Close()
        {
            if (this.proxyInterface != null) // If got a proxy
            {
                System.ServiceModel.ICommunicationObject commsProxy = this.comms;
                this.comms = null;
                this.proxyInterface = null;

                if (commsProxy.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    commsProxy.Abort();
                }
                else
                {
                    commsProxy.Close();
                }

            } // Ends if got a proxy
        }
        #endregion // Close Connection
    } // Ends class Proxy
}
