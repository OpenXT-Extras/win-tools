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

namespace Udbus.WCF.Service.Host
{
    /// <summary>
    /// Creates WCF ServiceHost instances with accompanying creation data.
    /// </summary>
    public interface IMakeServiceHostCreationData
    {
        /// <summary>
        /// Check whether this host maker's WCF Service is specified.
        /// </summary>
        /// <param name="serviceTypes">WCF service types to check in.</param>
        /// <returns>True if this host maker's WCF Service is present in the collection, otherwise false.</returns>
        bool ContainsService(params Type[] serviceTypes);

        /// <summary>
        /// Get registration data for this host maker.
        /// </summary>
        /// <returns></returns>
        DbusServiceRegistrationParams GetDbusServiceRegistrationParams();

        /// <summary>
        /// Create a ServiceHost instance with accompanying creation data.
        /// </summary>
        /// <param name="wcfserviceparams">WCF Service initialisation parameters.</param>
        /// <param name="uriBase">Endpoint base addresses.</param>
        /// <returns>Return ServiceHost with accompanying creatino data.</returns>
        ServiceHostCreationData MakeServiceHostCreationData(Udbus.WCF.Host.WCFServiceParams wcfserviceparams, params System.Uri[] uriBase);

    } // Ends IMakeServiceHostCreationData
} // Ends Udbus.WCF.Service.Host
