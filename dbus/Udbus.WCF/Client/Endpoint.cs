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
    public static class EndpointFunctions
    {
        #region Query String Handling
        /// <summary>
        /// Creates a query string containing Dbus info.
        /// </summary>
        /// <param name="busName">Dbus bus name.</param>
        /// <param name="path">Dbus path.</param>
        /// <returns>Query string in URI format.</returns>
        static public string CreateQueryString(string busName, string path)
        {
            System.Collections.Specialized.NameValueCollection queryCollection = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrEmpty(busName))
            {
                queryCollection[Udbus.WCF.Constants.busnameParam] = busName;
            }

            if (!string.IsNullOrEmpty(path))
            {
                queryCollection[Udbus.WCF.Constants.pathParam] = path;
            }

            return queryCollection.Count == 0 ? string.Empty : "?" + queryCollection.ToString();
        }

        /// <summary>
        /// Creates a query string containing Dbus path and busname.
        /// </summary>
        /// <param name="busName">Dbus bus name.</param>
        /// <param name="path">Dbus path.</param>
        /// <returns>Query string in URI format.</returns>
        static internal System.Uri CreateEndpointUriImpl(string baseAddress, string busName, string path)
        {
            return new System.Uri(baseAddress + CreateQueryString(busName, path));
        }

        /// <summary>
        /// Creates a query string containing Dbus path and busname.
        /// </summary>
        /// <param name="busName">Dbus bus name.</param>
        /// <param name="path">Dbus path.</param>
        /// <returns>Query string in URI format.</returns>
        static public System.Uri CreateEndpointUri(string baseAddress, string busName, string path)
        {
            return CreateEndpointUriImpl(baseAddress, busName, path);
        }

        /// <summary>
        /// Creates a query string containing Dbus busname.
        /// </summary>
        /// <param name="busName">Dbus bus name.</param>
        /// <param name="path">Dbus path.</param>
        /// <returns>Query string in URI format.</returns>
        static public System.Uri CreateEndpointUriForBusname(string baseAddress, string busName)
        {
            return CreateEndpointUriImpl(baseAddress, busName, null);
        }

        /// <summary>
        /// Creates a query string containing Dbus path.
        /// </summary>
        /// <param name="busName">Dbus bus name.</param>
        /// <param name="path">Dbus path.</param>
        /// <returns>Query string in URI format.</returns>
        static public System.Uri CreateEndpointUriForPath(string baseAddress, string path)
        {
            return CreateEndpointUriImpl(baseAddress, null, path);
        }
        #endregion // Query String Handling
    }
}
