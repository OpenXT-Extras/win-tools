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

namespace Udbus.WCF.Dbus.Details.Service
{
    /// <summary>
    /// Used by WCF Services to lookup equivalent DbusService.
    /// </summary>
    public struct DbusServiceKey
    {
        /// <summary>
        /// Dbus Busname
        /// </summary>
        public readonly string Busname;
        /// <summary>
        /// Dbus Path
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// Key for implementing dictionary.
        /// </summary>
        private readonly int key;

        /// <param name="busname">Dbus busname.</param>
        /// <param name="path">Dbus path.</param>
        public DbusServiceKey(string busname, string path)
        {
            this.Busname = busname;
            this.Path = path;
            string keyString = busname + path;
            this.key = keyString.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool equal = obj is DbusServiceKey;

            if (equal)
            {
                DbusServiceKey other = (DbusServiceKey)obj;
                if (equal)
                {
                    equal = this.Busname.Equals(other.Busname);

                    if (equal)
                    {
                        equal = this.Path.Equals(other.Path);
                    }
                }
            }
            return equal;
        }

        public override int GetHashCode()
        {
            return this.key;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(". Busname: '{0}'. Path: '{1}'",
                this.Busname, this.Path);
        }

    } // Ends struct DbusServiceKey

    /// <summary>
    /// Functions for WCF services to identify Dbus service instances.
    /// </summary>
    public static class LookupTargetFunctions
    {
        #region NameValueCollection helpers
        private static string GetNameValue(System.Collections.Specialized.NameValueCollection collection, string key, string defaultValue)
        {
            string result = collection[key];
            if (result == null)
            {
                result = defaultValue;
            }

            return result;
        }
        private static string GetNameValue(System.Collections.Specialized.NameValueCollection collection, string key)
        {
            return GetNameValue(collection, key, "");
        }
        #endregion // NameValueCollection helpers

        #region Dbus Service Creation
        /// <summary>
        /// Create a Dbus Service based on the supplied parameters.
        /// </summary>
        /// <typeparam name="T">Dbus Service type.</typeparam>
        /// <param name="wcfserviceParams">WCF service connection parameters.</param>
        /// <param name="createService1">Function to create Dbus Service taking service connection parameters.</param>
        /// <param name="createService2">Function to create Dbus Service taking service connection parameters and Dbus connection parameters.</param>
        /// <param name="defaultConnectionParameters">Default Dbus connection parameters for Dbus Service.</param>
        /// <param name="busname">Dbus bus name for Dbus Service.</param>
        /// <param name="path">Dbus path for Dbus Service.</param>
        /// <returns>new Dbus Service instance.</returns>
        public static T CreateDbusService<T>(Udbus.WCF.Host.WCFServiceParams wcfserviceParams
            , System.Func<Udbus.Core.ServiceConnectionParams, T> createService1
            , System.Func<Udbus.Core.ServiceConnectionParams, Udbus.Serialization.DbusConnectionParameters, T> createService2
            , Udbus.Serialization.ReadonlyDbusConnectionParameters defaultConnectionParameters
            , string busname, string path)
        {
            T result;

            if (string.IsNullOrEmpty(busname) && string.IsNullOrEmpty(path)) // If no parameters provided
            {
                if (wcfserviceParams.DbusConnectionParameters.HasValue) // If WCF Service was initialised with some settings
                {
                    // Create Dbus Service with WCF service's dbus connect settings.
                    result = createService2(wcfserviceParams.ServiceConnectionParams, wcfserviceParams.DbusConnectionParameters.Value);

                } // Ends if WCF Service was initialised with some settings
                else // Else WCF Service was not initialised with some settings
                {
                    // Create Dbus Service with default dbus connect settings.
                    result = createService1(wcfserviceParams.ServiceConnectionParams);

                } // Ends else WCF Service was not initialised with some settings

            } // Ends if no parameters provided
            else // Else parameters provided
            {
                Udbus.Serialization.DbusConnectionParameters connectionParameters;
                if (wcfserviceParams.DbusConnectionParameters.HasValue) // If WCF Service was initialised with some settings
                {
                    // Use WCF service's dbus connect settings as starting point.
                    connectionParameters = wcfserviceParams.DbusConnectionParameters.Value;

                } // Ends if WCF Service was initialised with some settings
                else // Else WCF Service was not initialised with some settings
                {
                    // Use default dbus connect settings as starting point.
                    connectionParameters = defaultConnectionParameters;

                } // Ends else WCF Service was not initialised with some settings

                if (!string.IsNullOrEmpty(busname)) // If busname supplied
                {
                    connectionParameters.Destination = busname;

                } // Ends if busname supplied

                if (!string.IsNullOrEmpty(path)) // If path supplied
                {
                    connectionParameters.Path = path;

                } // Ends if path supplied

                result = createService2(wcfserviceParams.ServiceConnectionParams, connectionParameters);

            } // Ends else parameters provided

            return result;
        }

        /// <summary>
        /// Create a Dbus Service based on the supplied parameters.
        /// </summary>
        /// <typeparam name="T">Dbus Service type.</typeparam>
        /// <param name="query">Query string in URI format.</param>
        /// <param name="wcfserviceParams">WCF service connection parameters.</param>
        /// <param name="createService1">Function to create Dbus Service taking service connection parameters.</param>
        /// <param name="createService2">Function to create Dbus Service taking service connection parameters and Dbus connection parameters.</param>
        /// <param name="defaultConnectionParameters">Default Dbus connection parameters for Dbus Service.</param>
        /// <returns>new Dbus Service instance.</returns>
        public static T CreateDbusService<T>(string query
            , Udbus.WCF.Host.WCFServiceParams wcfserviceParams
            , System.Func<Udbus.Core.ServiceConnectionParams, T> createService1
            , System.Func<Udbus.Core.ServiceConnectionParams, Udbus.Serialization.DbusConnectionParameters, T> createService2
            , Udbus.Serialization.ReadonlyDbusConnectionParameters defaultConnectionParameters)
        {
            string busname;
            string path;
            GetDbusParametersFromQuery(query, out busname, out path);
            return CreateDbusService(wcfserviceParams, createService1, createService2, defaultConnectionParameters, busname, path);
        }

        /// <summary>
        /// Create a Dbus Service based on the supplied parameters.
        /// </summary>
        /// <typeparam name="T">Dbus Service type.</typeparam>
        /// <param name="wcfserviceParams">WCF service connection parameters.</param>
        /// <param name="createService1">Function to create Dbus Service taking service connection parameters.</param>
        /// <param name="createService2">Function to create Dbus Service taking service connection parameters and Dbus connection parameters.</param>
        /// <param name="defaultConnectionParameters">Default Dbus connection parameters for Dbus Service.</param>
        /// <returns>new Dbus Service instance.</returns>
        public static T CreateDbusService<T>(Udbus.WCF.Host.WCFServiceParams wcfserviceParams
            , System.Func<Udbus.Core.ServiceConnectionParams, T> createService1
            , System.Func<Udbus.Core.ServiceConnectionParams, Udbus.Serialization.DbusConnectionParameters, T> createService2
            , Udbus.Serialization.ReadonlyDbusConnectionParameters defaultConnectionParameters)
        {
            return CreateDbusService(System.ServiceModel.OperationContext.Current.IncomingMessageHeaders.To.Query,
                wcfserviceParams, createService1, createService2, defaultConnectionParameters);
        }
        #endregion // Dbus Service Creation

        #region Method Target Discovery
        static private void GetDbusParametersFromQuery(string query, out string busname, out string path)
        {
            // Look for query parameters.
            System.Collections.Specialized.NameValueCollection queries = System.Web.HttpUtility.ParseQueryString(query);

            busname = GetNameValue(queries, Udbus.WCF.Constants.busnameParam);
            path = GetNameValue(queries, Udbus.WCF.Constants.pathParam);
        }

        /// <summary>
        /// Find the object to pass the method call through to.
        /// </summary>
        /// <typeparam name="T">Type implementing method.</typeparam>
        /// <param name="query">Query string in URI format.</param>
        /// <param name="wcfserviceParams">WCF service connection parameters.</param>
        /// <param name="services">Object registry.</param>
        /// <param name="createService1">Function to create Dbus Service taking service connection parameters.</param>
        /// <param name="createService2">Function to create Dbus Service taking service connection parameters and Dbus connection parameters.</param>
        /// <param name="defaultConnectionParameters">Default Dbus connection parameters for Dbus Service.</param>
        /// <returns>Object implementing method.</returns>
        static public T GetWCFMethodTarget<T>(string query
            , Udbus.WCF.Host.WCFServiceParams wcfserviceParams
            , System.Collections.Generic.IDictionary<DbusServiceKey, T> services
            , System.Func<Udbus.Core.ServiceConnectionParams, T> createService1
            , System.Func<Udbus.Core.ServiceConnectionParams, Udbus.Serialization.DbusConnectionParameters, T> createService2
            , Udbus.Serialization.ReadonlyDbusConnectionParameters defaultConnectionParameters)
        {
            // Look for query parameters.
            string busnameValue;
            string pathValue;
            GetDbusParametersFromQuery(query, out busnameValue, out pathValue);

            // Lookup existing service.
            DbusServiceKey find = new DbusServiceKey(busnameValue, pathValue);
            T result;

            if (services.TryGetValue(find, out result) == false) // If object not in registry
            {
                result = CreateDbusService(wcfserviceParams, createService1, createService2, defaultConnectionParameters, busnameValue, pathValue);

                // Store new service for future generations.
                services[find] = result;

            } // Ends if object not in registry

            return result;
        }

        /// <summary>
        /// Find the object to pass the method call through to, using query information embedded in current WCF method invocation.
        /// </summary>
        /// <typeparam name="T">Type implementing method.</typeparam>
        /// <param name="wcfserviceParams">WCF service connection parameters.</param>
        /// <param name="services">Object registry.</param>
        /// <param name="createService1">Function to create Dbus Service taking service connection parameters.</param>
        /// <param name="createService2">Function to create Dbus Service taking service connection parameters and Dbus connection parameters.</param>
        /// <param name="defaultConnectionParameters">Default Dbus connection parameters for Dbus Service.</param>
        /// <returns>Object implementing method.</returns>
        static public T GetWCFMethodTarget<T>(Udbus.WCF.Host.WCFServiceParams wcfserviceParams
            , System.Collections.Generic.IDictionary<DbusServiceKey, T> services
            , System.Func<Udbus.Core.ServiceConnectionParams, T> createService1
            , System.Func<Udbus.Core.ServiceConnectionParams, Udbus.Serialization.DbusConnectionParameters, T> createService2
            , Udbus.Serialization.ReadonlyDbusConnectionParameters defaultConnectionParameters)
        {
            return GetWCFMethodTarget(System.ServiceModel.OperationContext.Current.IncomingMessageHeaders.To.Query,
                wcfserviceParams, services, createService1, createService2, defaultConnectionParameters);
        }
        #endregion // Method Target Discovery


    } // Ends class LookupTargetFunctions

} // Ends Udbus.WCF.Dbus.Details.Service
