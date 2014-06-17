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
    /// Switches out dbus connection parameters at WCF service creation time.
    /// </summary>
    public class MakeServiceHostWithDifferentDbusParams : Udbus.WCF.Service.Host.IMakeServiceHostCreationData
    {
        /// <summary>
        /// Host maker to delegate to.
        /// </summary>
        Udbus.WCF.Service.Host.IMakeServiceHostCreationData target;

        /// <summary>
        /// Dbus connection parameters to substitute in.
        /// </summary>
        Udbus.Serialization.DbusConnectionParameters dbusConnectionParams;

        #region Constructors
        public MakeServiceHostWithDifferentDbusParams(Udbus.WCF.Service.Host.IMakeServiceHostCreationData target, Udbus.Serialization.DbusConnectionParameters dbusConnectionParams)
        {
            this.target = target;
            this.dbusConnectionParams = dbusConnectionParams;
        }
        #endregion // Constructors


        #region IMakeServiceHostCreationData Members

        public bool ContainsService(params Type[] contractTypes)
        {
            return this.target.ContainsService(contractTypes);
        }

        public Udbus.WCF.Service.Host.DbusServiceRegistrationParams GetDbusServiceRegistrationParams()
        {
            return this.target.GetDbusServiceRegistrationParams();
        }

        /// <summary>
        /// Replace the dbus connection parameters specified with those supplied at construction.
        /// </summary>
        /// <param name="wcfserviceparams">WCF Service creation paramters.</param>
        /// <param name="uriBase">Endpoing base addresses.</param>
        /// <returns>WCF ServiceHost with creation data.</returns>
        public Udbus.WCF.Service.Host.ServiceHostCreationData MakeServiceHostCreationData(Udbus.WCF.Host.WCFServiceParams wcfserviceparams, params Uri[] uriBase)
        {
            Udbus.WCF.Host.WCFServiceParams bam = new Udbus.WCF.Host.WCFServiceParams(wcfserviceparams.ServiceConnectionParams, this.dbusConnectionParams);
            return this.target.MakeServiceHostCreationData(bam, uriBase);
        }

        #endregion
    } // Ends class MakeServiceHostWithDifferentDbusParams
} // Ends namespace Udbus.WCF.Service.Host
