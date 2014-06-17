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
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Udbus.WCF.Client
{
    /// <summary>
    /// Gives us tasty tasty binding goodness.
    /// </summary>
    public interface IBindingFactory
    {
        System.ServiceModel.Channels.Binding Create();
    }

    /// <summary>
    /// Base class for implementing IBindingFactory through generics.
    /// </summary>
    /// <typeparam name="TBinding">Type of binding to create.</typeparam>
    abstract public class BindingFactoryBase<TBinding> : IBindingFactory
        where TBinding : System.ServiceModel.Channels.Binding
    {
        public System.ServiceModel.Channels.Binding Create()
        {
            System.ServiceModel.Channels.Binding binding = this.CreateImpl();

            return binding;
        }

        abstract protected TBinding CreateImpl();
    }

    /// <summary>
    /// Make life a bit easier for bindings with a default constructor.
    /// </summary>
    /// <typeparam name="TBinding">Type of binding to create.</typeparam>
    public class BindingFactory<TBinding> : BindingFactoryBase<TBinding>
        where TBinding : System.ServiceModel.Channels.Binding, new()
    {
        protected override TBinding CreateImpl()
        {
            return new TBinding();
        }

        public readonly static BindingFactory<TBinding> Default = new BindingFactory<TBinding>();

    } // Ends class BindingFactory<TBinding>

    /// <summary>
    /// You give me a function which makes bindings and uses arguments, I'll give you bindings.
    /// </summary>
    /// <typeparam name="TBinding">Type of binding to create.</typeparam>
    /// <typeparam name="TArgs">Arguments to binding creation function.</typeparam>
    public class BindingFactory<TBinding, TArgs> : BindingFactoryBase<TBinding>
        where TBinding : System.ServiceModel.Channels.Binding
    {
        TArgs args;
        Func<TArgs, TBinding> createBinding;

        public BindingFactory(TArgs args, Func<TArgs, TBinding> createBinding)
        {
            this.args = args;
            this.createBinding = createBinding;
        }
        protected override TBinding CreateImpl()
        {
            return createBinding(args);
        }
    } // Ends class BindingFactory<TBinding, TArgs>

    /// <summary>
    /// Base class for binding factory chaining.
    /// </summary>
    abstract public class BindingFactoryWrapperBase : IBindingFactory
    {
        private IBindingFactory bindingFactory;

        public BindingFactoryWrapperBase(IBindingFactory bindingFactory)
        {
            this.bindingFactory = bindingFactory;
        }

        abstract protected System.ServiceModel.Channels.Binding AdjustBinding(System.ServiceModel.Channels.Binding binding);

        #region IBindingFactory Members

        public System.ServiceModel.Channels.Binding Create()
        {
            System.ServiceModel.Channels.Binding binding = this.bindingFactory.Create();
            binding = this.AdjustBinding(binding);
            return binding;
        }

        #endregion // IBindingFactory Members
    } // Ends class BindingFactoryWrapperBase

    /// <summary>
    /// Wraps up a binding factory with standard binding settings.
    /// </summary>
    public class BindingFactoryStandardWrapper : BindingFactoryWrapperBase
    {
        public BindingFactoryStandardWrapper(IBindingFactory bindingFactory)
            : base(bindingFactory)
        {
        }

        #region BindingFactoryWrapperBase overrides
        protected override System.ServiceModel.Channels.Binding AdjustBinding(System.ServiceModel.Channels.Binding binding)
        {
            System.ServiceModel.Channels.Binding result = binding;

            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            // [XC-9600] - large byte buffers throw exceptions.
            #region Set timeouts and other binding class specific values

            // This is horrible, why couldn't these bindings all implement the same interface.
            //if (binding is System.ServiceModel.NetNamedPipeBinding)
            //{
            //    var polyBinding = (binding as System.ServiceModel.NetNamedPipeBinding);
            //}

            //if (binding is System.ServiceModel.NetPeerTcpBinding)
            //{
            //    var polyBinding = (binding as System.ServiceModel.NetPeerTcpBinding);
            //}

            if (binding is System.ServiceModel.NetTcpBinding)
            {
                var polyBinding = (binding as System.ServiceModel.NetTcpBinding);
                polyBinding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
                polyBinding.ReliableSession.Ordered = true;
                polyBinding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            }

            if (binding is System.ServiceModel.WSHttpBindingBase)
            {
                var polyBinding = (binding as System.ServiceModel.WSHttpBindingBase);
                polyBinding.ReliableSession.Ordered = true;
                polyBinding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            }

            if (binding is System.ServiceModel.WSDualHttpBinding)
            {
                var polyBinding = (binding as System.ServiceModel.WSDualHttpBinding);
                polyBinding.ReliableSession.Ordered = true;
                polyBinding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            }

            #endregion // Set timeouts and other binding class specific values

            // WCF - making things really complicated, one step at a time.

            // Ok, so unfortunately cannot access the transport directly in the binding.
            // So we have to use a "CustomBinding" wrapping it, which allows access to the TransportBindingElement element
            // (bit bizarre since BindingElementCollection is created dynamically on demand but CustomBinding bakes it at construction time)
            // which in turn allows access to the MaxReceivedMessageSize property, as specified in the exception message for reading too many bytes.
            System.ServiceModel.Channels.CustomBinding cb = new System.ServiceModel.Channels.CustomBinding(binding);
            System.ServiceModel.Channels.TransportBindingElement transport = cb.Elements.Find<System.ServiceModel.Channels.TransportBindingElement>();

            if (transport != null)
            {
                transport.MaxReceivedMessageSize = int.MaxValue;

            }

            // XML serialization may also get its knickers in a twist for which MaxReceivedMessageSize, it does nothing.
            // Changing MaxArrayLength in the encoding quotas on the other hand...
            System.ServiceModel.Channels.BinaryMessageEncodingBindingElement encoding = cb.Elements.Find<System.ServiceModel.Channels.BinaryMessageEncodingBindingElement>();

            if (encoding != null)
            {
                encoding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            }

            result = cb;

            return result;
        }
        #endregion // BindingFactoryWrapperBase overrides
    } // Ends class BindingFactoryStandardWrapper

    /// <summary>
    /// Wraps up a binding factory with some common debug settings.
    /// </summary>
    public class BindingFactoryDebugWrapper : BindingFactoryWrapperBase
    {
        public BindingFactoryDebugWrapper(IBindingFactory bindingFactory)
            : base(bindingFactory)
        {
        }

        #region BindingFactoryWrapperBase overrides
        protected override System.ServiceModel.Channels.Binding AdjustBinding(System.ServiceModel.Channels.Binding binding)
        {
            TimeSpan sendTimeoutDefault = new TimeSpan(1, 0, 0);
            if (binding.SendTimeout < sendTimeoutDefault)
            {
                binding.SendTimeout = sendTimeoutDefault;
            }
            return binding;
        }
        #endregion // BindingFactoryWrapperBase overrides
    } // Ends class BindingFactoryDebugWrapper

    public class ProxyManager
    {
        public static readonly IBindingFactory DefaultBindingFactory =
#if DEBUG
 new BindingFactoryDebugWrapper(
#endif // DEBUG
 new BindingFactoryStandardWrapper(
  BindingFactory<System.ServiceModel.NetNamedPipeBinding>.Default
 )
#if DEBUG
 )
#endif // DEBUG
;
        #region Creation
        /// <summary>
        /// Create a proxy for the given interface.
        /// </summary>
        /// <typeparam name="TProxyInterface">Interface to create proxy for.</typeparam>
        /// <typeparam name="TBinding">Type of binding to use.</typeparam>
        /// <param name="uriEndpoint">Endpoint of service.</param>
        /// <param name="bindingFactory">Binding factory</param>
        /// <returns>Proxy initialised with interface.</returns>
        static public Proxy<TProxyInterface> Create<TProxyInterface>(System.Uri uriEndpoint, IBindingFactory bindingFactory)
            where TProxyInterface : class
        {
            //System.ServiceModel.Channels.Binding binding = bindingFactory.Create();
            //System.ServiceModel.ChannelFactory<TProxyInterface> factory = new System.ServiceModel.ChannelFactory<TProxyInterface>(
            //     binding
            //    , new System.ServiceModel.EndpointAddress(uriEndpoint)
            //);
            //TProxyInterface proxy = factory.CreateChannel();
            //System.ServiceModel.ICommunicationObject comms = proxy as System.ServiceModel.ICommunicationObject;
            //AttemptClientOpen(ref comms, ref proxy, factory);
            //Proxy<TProxyInterface> result = new Proxy<TProxyInterface>(uriEndpoint, comms, proxy);
            Proxy<TProxyInterface> result = new Proxy<TProxyInterface>(bindingFactory, uriEndpoint);
            return result;
        }

        #region Creation with IBindingFactory
        static public Proxy<TProxyInterface> Create<TProxyInterface>(DbusEndpointUriComponents dbusEndpointUri, string relativeAddress, IBindingFactory bindingFactory)
            where TProxyInterface : class
        {
            System.Uri uriEndpoint = dbusEndpointUri.CreateUri(relativeAddress);
            return Create<TProxyInterface>(uriEndpoint, bindingFactory);
        }

        //static public Proxy<TProxyInterface> Create<TProxyInterface>(string baseAddress, string busName, string path, IBindingFactory bindingFactory)
        //    where TProxyInterface : class
        //{
        //    System.Uri uriEndpoint = Udbus.WCF.Dbus.Service.LookupTargetFunctions.CreateEndpointUri(baseAddress
        //        , busName, path
        //    );
        //    return Create<TProxyInterface>(uriEndpoint, bindingFactory);
        //}

        //static public Proxy<TProxyInterface> Create<TProxyInterface>(string baseAddress, IBindingFactory bindingFactory)
        //    where TProxyInterface : class
        //{
        //    System.Uri uriEndpoint = new System.Uri(baseAddress);
        //    return Create<TProxyInterface>(uriEndpoint, bindingFactory);
        //}

        //static public Proxy<TProxyInterface> CreateForBusname<TProxyInterface>(string baseAddress, string busName, IBindingFactory bindingFactory)
        //    where TProxyInterface : class
        //{
        //    System.Uri uriEndpoint = Udbus.WCF.Dbus.Service.LookupTargetFunctions.CreateEndpointUriForBusname(baseAddress
        //        , busName
        //    );
        //    return Create<TProxyInterface>(uriEndpoint, bindingFactory);
        //}

        //static public Proxy<TProxyInterface> CreateForPath<TProxyInterface>(string baseAddress, string path, IBindingFactory bindingFactory)
        //    where TProxyInterface : class
        //{
        //    System.Uri uriEndpoint = Udbus.WCF.Dbus.Service.LookupTargetFunctions.CreateEndpointUriForPath(baseAddress
        //        , path
        //    );
        //    return Create<TProxyInterface>(uriEndpoint, bindingFactory);
        //}
        #endregion // Creation with IBindingFactory

        #region Creation with Default IBindingFactory
        static public Proxy<TProxyInterface> Create<TProxyInterface>(System.Uri uriEndpoint)
            where TProxyInterface : class
        {
            return Create<TProxyInterface>(uriEndpoint, DefaultBindingFactory);
        }

        static public Proxy<TProxyInterface> Create<TProxyInterface>(DbusEndpointUriComponents dbusEndpointUri, string relativeAddress)
            where TProxyInterface : class
        {
            System.Uri uriEndpoint = dbusEndpointUri.CreateUri(relativeAddress);
            return Create<TProxyInterface>(uriEndpoint, DefaultBindingFactory);
        }

        //static public Proxy<TProxyInterface> Create<TProxyInterface>(string baseAddress, string busName, string path)
        //    where TProxyInterface : class
        //{
        //    return Create<TProxyInterface>(baseAddress, busName, path, DefaultBindingFactory);
        //}

        //static public Proxy<TProxyInterface> Create<TProxyInterface>(string baseAddress)
        //    where TProxyInterface : class
        //{
        //    return Create<TProxyInterface>(baseAddress, DefaultBindingFactory);
        //}

        //static public Proxy<TProxyInterface> CreateForBusname<TProxyInterface>(string baseAddress, string busName)
        //    where TProxyInterface : class
        //{
        //    return CreateForBusname<TProxyInterface>(baseAddress, busName, DefaultBindingFactory);
        //}

        //static public Proxy<TProxyInterface> CreateForPath<TProxyInterface>(string baseAddress, string path)
        //    where TProxyInterface : class
        //{
        //    return CreateForPath<TProxyInterface>(baseAddress, path, DefaultBindingFactory);
        //}
        #endregion // Creation with Default IBindingFactory

        #region CallbackProxy
        static public TCallback DefaultCreateCallback<TCallback>() where TCallback : new() { return new TCallback(); }

        static public CallbackProxy<TProxyInterface, TCallback> Create<TProxyInterface, TCallback>(System.Uri uriEndpoint, IBindingFactory bindingFactory)
            where TProxyInterface : class
            where TCallback : class, new()
        {
            CallbackProxy<TProxyInterface, TCallback> result = new CallbackProxy<TProxyInterface, TCallback>(bindingFactory, uriEndpoint);
            return result;
        }

        static public CallbackProxy<TProxyInterface, TCallback> Create<TProxyInterface, TCallback>(DbusEndpointUriComponents dbusEndpointUri, string relativeAddress, IBindingFactory bindingFactory)
            where TProxyInterface : class
            where TCallback : class, new()
        {
            System.Uri uriEndpoint = dbusEndpointUri.CreateUri(relativeAddress);
            return Create<TProxyInterface, TCallback>(uriEndpoint, bindingFactory);
        }

        static public CallbackProxy<TProxyInterface, TCallback> Create<TProxyInterface, TCallback>(DbusEndpointUriComponents dbusEndpointUri, string relativeAddress)
            where TProxyInterface : class
            where TCallback : class, new()
        {
            System.Uri uriEndpoint = dbusEndpointUri.CreateUri(relativeAddress);
            return Create<TProxyInterface, TCallback>(uriEndpoint, DefaultBindingFactory);
        }

        #endregion CallbackProxy
        #endregion // Creation

    } // Ends class ProxyManager
} // Ends namespace Udbus.WCF.Client
