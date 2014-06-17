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
using System.ServiceModel;

namespace Udbus.WCF.Service.Host
{
    /// <summary>
    /// Commonly required creation parameters.
    /// </summary>
    public struct DbusServiceCreationParams
    {
        /// <summary>
        /// WCF endpoint relative name.
        /// </summary>
        public readonly string RelativeName;

        /// <summary>
        /// WCF Contract type.
        /// </summary>
        public readonly Type ContractType;

        public DbusServiceCreationParams(string RelativeName, Type ContractType)
        {
            this.RelativeName = RelativeName;
            this.ContractType = ContractType;
        }
    } // End struct DbusServiceCreationParams

    /// <summary>
    /// WCF ServiceHost creating registration parameters.
    /// </summary>
    public struct DbusServiceRegistrationParams
    {
        /// <summary>
        /// Service creation parameters.
        /// </summary>
        public readonly DbusServiceCreationParams CreationParams;
        /// <summary>
        /// WCF Service type to be hosted.
        /// </summary>
        public readonly Type ServiceType;
        /// <summary>
        /// Unique key for dictionary.
        /// </summary>
        private readonly string key;
        /// <summary>
        /// Key's hash code.
        /// </summary>
        private readonly int hash;

        /// <summary>
        /// Format string to produce unique key.
        /// </summary>
        const string RegistrationKeyFormat = "{0}:{1}({2})";


        /// <summary>
        /// Turn registration parameters data into a key.
        /// </summary>
        /// <param name="CreationParams">Creation parameters.</param>
        /// <param name="ServiceType">WCF Service type.</param>
        /// <returns>Key used for dictionary.</returns>
        static string RegistrationKey(DbusServiceCreationParams CreationParams, Type ServiceType)
        {
            return string.Format(RegistrationKeyFormat, ServiceType.FullName, CreationParams.ContractType.FullName, CreationParams.RelativeName);
        }

        #region Constructors
        public DbusServiceRegistrationParams(DbusServiceCreationParams CreationParams, Type ServiceType)
        {
            this.CreationParams = CreationParams;
            this.ServiceType = ServiceType;
            this.key = RegistrationKey(CreationParams, ServiceType);
            this.hash = this.key.GetHashCode();
        }
        #endregion // Constructors

        #region Object override
        public override int GetHashCode()
        {
            return this.hash;
        }

        public override bool Equals(object obj)
        {
            bool equal = obj is DbusServiceRegistrationParams;
            if (equal)
            {
                DbusServiceRegistrationParams other = (DbusServiceRegistrationParams)obj;
                equal = this.CreationParams.Equals(other.CreationParams);
                if (equal)
                {
                    equal = this.ServiceType.Equals(other.ServiceType);
                }
            }
            return equal;
        }
        #endregion // Object override
    } // Ends DbusServiceRegistrationParams

    /// <summary>
    /// WCF ServiceHost and accompanying data used during creation.
    /// </summary>
    public struct ServiceHostCreationData
    {
        /// <summary>
        /// Newly created ServiceHost.
        /// </summary>
        public readonly DbusServiceHost Host;
        public readonly DbusServiceCreationParams CreationParams;

        public ServiceHostCreationData(DbusServiceHost Host, DbusServiceCreationParams CreationParams)
        {
            this.Host = Host;
            this.CreationParams = CreationParams;
        }

        /// <summary>
        /// Creation a predicate for matching on Contract Type.
        /// </summary>
        /// <param name="contractType">Contract type to match on.</param>
        /// <returns>Function matching for specific contract.</returns>
        static public Func<ServiceHostCreationData, bool> BuildMatchesContractType(Type contractType)
        {
            return (ServiceHostCreationData servicehostCreationData) => { return servicehostCreationData.CreationParams.ContractType == contractType; };
        }

        /// <summary>
        /// Accessor for host.
        /// </summary>
        /// <param name="data">Data to extract ServiceHost instance from.</param>
        /// <returns>Created ServiceHost instance.</returns>
        static public ServiceHost DataToHost(ServiceHostCreationData data)
        {
            return data.Host;
        }
    } // Ends struct ServiceHostCreationData

    /// <summary>
    /// Factory that provides instances of DbusServiceHost in managed hosting environments
    /// where the host instance is created dynamically in response to incoming messages.
    /// </summary>
    public class DbusServiceHostFactory : System.ServiceModel.Activation.ServiceHostFactory
    {
        #region Fields
        /// <summary>
        /// Instance provided to pass through to.
        /// </summary>
        protected readonly System.ServiceModel.Dispatcher.IInstanceProvider instanceProvider;
        #endregion // Fields

        #region Constructors
        public DbusServiceHostFactory(System.ServiceModel.Dispatcher.IInstanceProvider instanceProvider)
        {
            this.instanceProvider = instanceProvider;
        }
        #endregion // Constructors

        #region ServiceHostFactory overrides
        /// <summary>
        /// Creates a ServiceHost for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">The Array of type Uri that contains the base addresses for the service hosted.</param>
        /// <returns>A ServiceHost for the type of service specified with a specific base address.</returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new DbusServiceHost(this.instanceProvider, serviceType, baseAddresses);
        }
        #endregion // ServiceHostFactory overrides
    } // Ends class DbusServiceHostFactory

    /// <summary>
    /// IInstanceProvider which just passes through to another instance provider, and also implements IContractBehavior.
    /// </summary>
    public class DbusServiceInstanceProviderPassthrough : System.ServiceModel.Dispatcher.IInstanceProvider,
        System.ServiceModel.Description.IContractBehavior
    {
        #region Fields
        /// <summary>
        /// IInstanceProvider to pass through to.
        /// </summary>
        protected readonly System.ServiceModel.Dispatcher.IInstanceProvider instanceProvider;
        #endregion // Fields

        #region Constructors
        public DbusServiceInstanceProviderPassthrough(System.ServiceModel.Dispatcher.IInstanceProvider instanceProvider)
        {
            if (instanceProvider == null)
            {
                throw new ArgumentNullException("instanceProvider");
            }

            this.instanceProvider = instanceProvider;
        }
        #endregion Constructors

        #region IInstanceProvider functions
        public object GetInstance(InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
        {
            return this.instanceProvider.GetInstance(instanceContext, message);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.instanceProvider.GetInstance(instanceContext);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            this.instanceProvider.ReleaseInstance(instanceContext, instance);
        }
        #endregion // IInstanceProvider functions

        #region IContractBehavior
        public void AddBindingParameters(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // No action.
        }

        public void ApplyClientBehavior(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            // No action.
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint)
        {
            // No action.
        }
        #endregion // IContractBehavior
    } // Ends class DbusServiceInstanceProviderPassthrough

    /// <summary>
    /// ServiceHost which adds IInstanceProvider to contract behaviors.
    /// </summary>
    public partial class DbusServiceHost : System.ServiceModel.ServiceHost
    {
        #region Constructors
        public DbusServiceHost(System.ServiceModel.Dispatcher.IInstanceProvider instanceProvider,
            Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            if (instanceProvider == null)
            {
                throw new ArgumentNullException("instanceProvider");
            }

            foreach (System.ServiceModel.Description.ContractDescription cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new DbusServiceInstanceProviderPassthrough(instanceProvider));
            }
        }
        #endregion // Constructors
    } // Ends class DbusServiceHost
} // Ends namespace Udbus.WCF.Service.Host
