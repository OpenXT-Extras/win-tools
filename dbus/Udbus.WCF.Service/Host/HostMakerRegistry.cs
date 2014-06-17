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
using System.Text;

namespace Udbus.WCF.Service.Host
{
    /// <summary>
    /// Lookup for registration data to instances of IMakeServiceHostCreationData.
    /// </summary>
    public class RegistryDictionaryHolder
    {
        /// <summary>
        /// The actual dictionary of registry data.
        /// </summary>
        private Dictionary<Udbus.WCF.Service.Host.DbusServiceRegistrationParams, Udbus.WCF.Service.Host.IMakeServiceHostCreationData> dict = new Dictionary<Udbus.WCF.Service.Host.DbusServiceRegistrationParams, Udbus.WCF.Service.Host.IMakeServiceHostCreationData>();

        #region Properties
        public IEnumerable<Udbus.WCF.Service.Host.IMakeServiceHostCreationData> Values { get { return this.dict.Values; } }
        public IEnumerable<Udbus.WCF.Service.Host.DbusServiceRegistrationParams> Keys { get { return this.dict.Keys; } }
        public IEnumerable<KeyValuePair<Udbus.WCF.Service.Host.DbusServiceRegistrationParams, Udbus.WCF.Service.Host.IMakeServiceHostCreationData>> Items { get { return this.dict; } }
        #endregion // Properties

        #region Constructors
        internal RegistryDictionaryHolder()
        {
            this.dict = new Dictionary<Udbus.WCF.Service.Host.DbusServiceRegistrationParams, Udbus.WCF.Service.Host.IMakeServiceHostCreationData>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">Source object.</param>
        internal RegistryDictionaryHolder(RegistryDictionaryHolder source)
        {
            this.dict = new Dictionary<Udbus.WCF.Service.Host.DbusServiceRegistrationParams, Udbus.WCF.Service.Host.IMakeServiceHostCreationData>(source.dict);
        }
        #endregion // Constructors

        /// <summary>
        /// Add the contents of another registry to this registry.
        /// </summary>
        /// <param name="source">Registry to add contents of.</param>
        public void Add(RegistryDictionaryHolder source)
        {
            foreach (KeyValuePair<Udbus.WCF.Service.Host.DbusServiceRegistrationParams, Udbus.WCF.Service.Host.IMakeServiceHostCreationData> kv in source.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Add an entry to the registry overwriting any existing entry.
        /// </summary>
        /// <param name="add">Entry to register.</param>
        public void Register(Udbus.WCF.Service.Host.IMakeServiceHostCreationData add)
        {
            Udbus.WCF.Service.Host.DbusServiceRegistrationParams registration = add.GetDbusServiceRegistrationParams();
            this.dict[registration] = add;
        }

        /// <summary>
        /// Add an entry to the registry overwriting any existing entry by creating a new instance of the specified type.
        /// </summary>
        /// <param name="add">Entry to register.</param>
        public void Register<T>()
            where T : Udbus.WCF.Service.Host.IMakeServiceHostCreationData, new()
        {
            T add = new T();
            Register(add);
        }

        /// <summary>
        /// Find an entry using the specified registration data.
        /// </summary>
        /// <param name="registration">Registration data to look up.</param>
        /// <returns>Entry if found, otherwise null.</returns>
        public Udbus.WCF.Service.Host.IMakeServiceHostCreationData Find(Udbus.WCF.Service.Host.DbusServiceRegistrationParams registration)
        {
            Udbus.WCF.Service.Host.IMakeServiceHostCreationData result = null;
            this.dict.TryGetValue(registration, out result);
            return result;
        }

        /// <summary>
        /// Wrap an existing entry with one which will substitute in alternative DbusConnectionParams when the WCF service is created.
        /// </summary>
        /// <param name="registration">Registration data.</param>
        /// <param name="dbusConnectionParams">Dbus Connection parameters replacing those specified at creation time.</param>
        /// <returns>true if found an entry to wrap, otherwise false.</returns>
        public bool WrapReplaceDbusConnectionParams(Udbus.WCF.Service.Host.DbusServiceRegistrationParams registration,
            Udbus.Serialization.DbusConnectionParameters dbusConnectionParams)
        {
            Udbus.WCF.Service.Host.IMakeServiceHostCreationData target;
            bool result = this.dict.TryGetValue(registration, out target);

            if (result) // If found entry
            {
                // Replace with a wrapper thatn will change the connection params.
                MakeServiceHostWithDifferentDbusParams replacement = new MakeServiceHostWithDifferentDbusParams(target, dbusConnectionParams);
                this.dict[registration] = replacement;

            } // Ends if found entry

            return result;
        }
    } // Ends class RegistryDictionary

    /// <remarks>
    /// Instantiating these entry types adds an entry to the HostMakerRegistry.
    /// Auto-generated code cannot register itself via one single mechanism at startup due to the lack of a module entry point in CSharp.
    /// So the idiocy embarked on is to create a "namespace Udbus.WCF.Dbus.Host.Init { partial class InitHostMakerRegistry }" for every interface,
    /// containing a RegistryListEntry field complete with initialiser.
    /// e.g. partial class InitHostMakerRegistry { RegistryListEntryBase addMyAutoGeneratedService = new RegistryListEntry&lt;MyAutogeneratedService&gt;(); }
    /// External code wanting to populate the registry can then create an instance of InitHostMakerRegistry without even keeping a reference to it.
    /// The initialisation of all the auto-generated fields will populate the registry.
    /// i.e. somewhere at startup just include: new InitHostMakerRegistry();
    /// Sadface.
    /// </remarks>
    #region Registry initialisation idiocy
    /// <summary>
    /// Base class for classes which will add to HostMakerRegistry upon construction.
    /// </summary>
    abstract public class RegistryListEntryBase
    {
    } // Ends class RegistryListEntryBase

    /// <summary>
    /// A disgusting hack to update the HostMakerRegistry via object instantiation because there's no module .cctor in CSharp.
    /// </summary>
    public class RegistryListEntryAdd : RegistryListEntryBase
    {
        public RegistryListEntryAdd(Udbus.WCF.Service.Host.IMakeServiceHostCreationData add)
        {
            HostMakerRegistry.Register(add);
        }
    } // Ends RegistryListEntryAdd

    /// <summary>
    /// A disgusting hack to update the HostMakerRegistry via object instantiation because there's no module .cctor in CSharp.
    /// </summary>
    public class RegistryListEntry<T> : RegistryListEntryBase
            where T : Udbus.WCF.Service.Host.IMakeServiceHostCreationData, new()
    {
        public RegistryListEntry()
        {
            HostMakerRegistry.Register<T>();
        }
    } // Ends class RegistryListEntry
    #endregion // Registry initialisation idiocy

    /// <summary>
    /// Registry singleton for all the little host making doo dads in the universe.
    /// </summary>
    static public class HostMakerRegistry
    {
        // Lil note - static members get initialised BEFORE static class constructors are called, which is a good thing.
        /// <summary>
        /// The master registry.
        /// </summary>
        static private RegistryDictionaryHolder registryDictMaster = new RegistryDictionaryHolder();

        #region Properties
        static public IEnumerable<Udbus.WCF.Service.Host.IMakeServiceHostCreationData> HostMakers { get { return registryDictMaster.Values; } }
        static public RegistryDictionaryHolder RegistryDictionaryClone { get { return new RegistryDictionaryHolder(registryDictMaster); } }
        #endregion // Properties

        /// <summary>
        /// Add an entry to the registry overwriting any existing entry.
        /// </summary>
        /// <param name="add">Entry to register.</param>
        static public void Register(Udbus.WCF.Service.Host.IMakeServiceHostCreationData add)
        {
            registryDictMaster.Register(add);
        }

        /// <summary>
        /// Add an entry to the registry overwriting any existing entry by creating a new instance of the specified type.
        /// </summary>
        /// <param name="add">Entry to register.</param>
        static public void Register<T>()
            where T : Udbus.WCF.Service.Host.IMakeServiceHostCreationData, new()
        {
            registryDictMaster.Register<T>();
        }

        /// <summary>
        /// Find an entry using the specified registration data.
        /// </summary>
        /// <param name="registration">Registration data to look up.</param>
        /// <returns>Entry if found, otherwise null.</returns>
        static public Udbus.WCF.Service.Host.IMakeServiceHostCreationData Find(Udbus.WCF.Service.Host.DbusServiceRegistrationParams registration)
        {
            return registryDictMaster.Find(registration);
        }

        /// <summary>
        /// Build ALL the hosts ? Well all the ones with entries in "hostMakers", apart from any excluded by WCF Service type at run time.
        /// </summary>
        /// <param name="hostMakers">Objects capable of making WCF ServiceHost instances.</param>
        /// <param name="wcfserviceparams">WCF service parameters.</param>
        /// <param name="uriBase">Endpoint base addresses.</param>
        /// <param name="exclude">WCF Service types to exclude from creation.</param>
        /// <returns>Collection of created ServiceHost instances with accompanying creation data.</returns>
        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(IEnumerable<Udbus.WCF.Service.Host.IMakeServiceHostCreationData> hostMakers,
            Udbus.WCF.Host.WCFServiceParams wcfserviceparams, System.Uri[] uriBase, params Type[] exclude)
        {
            List<Udbus.WCF.Service.Host.ServiceHostCreationData> hosts = new List<Udbus.WCF.Service.Host.ServiceHostCreationData>();

            foreach (Udbus.WCF.Service.Host.IMakeServiceHostCreationData maker in hostMakers)
            {
                if (!maker.ContainsService(exclude))
                {
                    hosts.Add(maker.MakeServiceHostCreationData(wcfserviceparams, uriBase));
                }
            }

            return hosts;
        }

        /// <summary>
        /// Build ALL the hosts ? Well all the ones with entries in the registry, apart from any excluded by WCF Service type at run time.
        /// </summary>
        /// <param name="wcfserviceparams">WCF service parameters.</param>
        /// <param name="uriBase">Endpoint base addresses.</param>
        /// <param name="exclude">WCF Service types to exclude from creation.</param>
        /// <returns>Collection of created ServiceHost instances with accompanying creation data.</returns>
        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(Udbus.WCF.Host.WCFServiceParams wcfserviceparams, System.Uri[] uriBase, params Type[] exclude)
        {
            return BuildAllHosts(HostMakers, wcfserviceparams, uriBase, exclude);
        }

        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(IEnumerable<Udbus.WCF.Service.Host.IMakeServiceHostCreationData> hostMakers,
            Udbus.Core.ServiceConnectionParams serviceparams, System.Uri[] uriBase, params Type[] exclude)
        {
            return BuildAllHosts(hostMakers, new Udbus.WCF.Host.WCFServiceParams(serviceparams), uriBase, exclude);
        }

        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(IEnumerable<Udbus.WCF.Service.Host.IMakeServiceHostCreationData> hostMakers,
            Udbus.WCF.Host.WCFServiceParams wcfserviceparams, System.Uri uriBase, params Type[] exclude)
        {
            return BuildAllHosts(hostMakers, wcfserviceparams, new System.Uri[] { uriBase }, exclude);
        }

        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(IEnumerable<Udbus.WCF.Service.Host.IMakeServiceHostCreationData> hostMakers,
            Udbus.Core.ServiceConnectionParams serviceparams, System.Uri uriBase, params Type[] exclude)
        {
            return BuildAllHosts(hostMakers, serviceparams, new System.Uri[] { uriBase }, exclude);
        }

        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(Udbus.Core.ServiceConnectionParams serviceparams,
            System.Uri[] uriBase, params Type[] exclude)
        {
            return BuildAllHosts(new Udbus.WCF.Host.WCFServiceParams(serviceparams), uriBase, exclude);
        }

        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(Udbus.WCF.Host.WCFServiceParams wcfserviceparams,
            System.Uri uriBase, params Type[] exclude)
        {
            return BuildAllHosts(wcfserviceparams, new System.Uri[] { uriBase }, exclude);
        }

        static public ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> BuildAllHosts(Udbus.Core.ServiceConnectionParams serviceparams,
            System.Uri uriBase, params Type[] exclude)
        {
            return BuildAllHosts(serviceparams, new System.Uri[] { uriBase }, exclude);
        }

    } // Ends class HostMakerRegistry
} // Ends namespace Udbus.WCF.Service.Host
