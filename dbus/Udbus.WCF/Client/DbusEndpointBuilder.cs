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
    /// <summary>
    /// Build full URI's for Dbus Endpoints.
    /// </summary>
    public class DbusEndpointAbsoluteUriBuilder
    {
        const string DefaultBaseAddressPrefix = Udbus.WCF.Constants.BaseAddressPrefix;

        string baseAddressPrefix;

        public DbusEndpointAbsoluteUriBuilder(string baseAddressPrefix)
        {
            if (baseAddressPrefix == null)
            {
                throw new ArgumentNullException("baseAddressPrefix");
            }
            else if (!baseAddressPrefix.Equals(string.Empty) && !baseAddressPrefix.EndsWith("/"))
            {
                baseAddressPrefix += "/";
            }

            this.baseAddressPrefix = baseAddressPrefix;
        }

        private string AbsoluteAddress(string relative)
        {
            return this.baseAddressPrefix + relative;
        }

        public DbusEndpointAbsoluteUriBuilder()
            : this(DefaultBaseAddressPrefix)
        {
        }

        public System.Uri Create(string relativeAddress, string busName, string path)
        {
            return EndpointFunctions.CreateEndpointUri(this.AbsoluteAddress(relativeAddress)
                , busName, path
            );
        }

        public System.Uri Create(string relativeAddress)
        {
            return new System.Uri(this.AbsoluteAddress(relativeAddress));
        }

        public System.Uri CreateForBusname(string relativeAddress, string busName)
        {
            return EndpointFunctions.CreateEndpointUriForBusname(this.AbsoluteAddress(relativeAddress)
                , busName
            );
        }

        public System.Uri CreateForPath(string relativeAddress, string path)
        {
            return EndpointFunctions.CreateEndpointUriForPath(this.AbsoluteAddress(relativeAddress)
                , path
            );
        }
    } // Ends DbusEndpointAbsoluteUriBuilder

    /// <summary>
    /// Factory class for different EndpointUri components.
    /// </summary>
    public class DbusEndpointUriComponents
    {
        protected DbusEndpointAbsoluteUriBuilder endpointBuilder;

        protected DbusEndpointUriComponents(DbusEndpointAbsoluteUriBuilder endpointBuilder)
        {
            this.endpointBuilder = endpointBuilder;
        }

        protected DbusEndpointUriComponents()
            : this(new DbusEndpointAbsoluteUriBuilder())
        {
        }

        virtual public System.Uri CreateUri(string relative)
        {
            return this.endpointBuilder.Create(relative);
        }

        #region Factory functions
        static public DbusEndpointUriComponents Create()
        {
            return new DbusEndpointUriComponents();
        }

        static public DbusEndpointUriComponents Create(DbusEndpointAbsoluteUriBuilder endpointBuilder)
        {
            return new DbusEndpointUriComponents(endpointBuilder);
        }

        static public DbusEndpointUriComponents Create(DbusEndpointAbsoluteUriBuilder endpointBuilder, string busname, string path)
        {
            return new DbusEndpointUriComponentsAll(endpointBuilder, busname, path);
        }

        static public DbusEndpointUriComponents CreateForBusname(DbusEndpointAbsoluteUriBuilder endpointBuilder, string busname)
        {
            return new DbusEndpointUriComponentsBusname(endpointBuilder, busname);
        }

        static public DbusEndpointUriComponents CreateForPath(DbusEndpointAbsoluteUriBuilder endpointBuilder, string path)
        {
            return new DbusEndpointUriComponentsPath(endpointBuilder, path);
        }

        static public DbusEndpointUriComponents Create(string busname, string path)
        {
            return new DbusEndpointUriComponentsAll(busname, path);
        }

        static public DbusEndpointUriComponents CreateForBusname(string busname)
        {
            return new DbusEndpointUriComponentsBusname(busname);
        }

        static public DbusEndpointUriComponents CreateForPath(string path)
        {
            return new DbusEndpointUriComponentsPath(path);
        }
        #endregion // Factory functions
    } // Ends class DbusEndpointUriComponents

    internal class DbusEndpointUriComponentsAll : DbusEndpointUriComponents
    {
        string busname;
        string path;

        public DbusEndpointUriComponentsAll(DbusEndpointAbsoluteUriBuilder endpointBuilder, string busname, string path)
            : base(endpointBuilder)
        {
            this.busname = busname;
            this.path = path;
        }

        public DbusEndpointUriComponentsAll(string busname, string path)
            : base()
        {
            this.busname = busname;
            this.path = path;
        }

        public override System.Uri CreateUri(string relative)
        {
            return this.endpointBuilder.Create(relative, this.busname, this.path);
        }
    } // Ends class DbusEndpointUriFull

    internal class DbusEndpointUriComponentsBusname : DbusEndpointUriComponents
    {
        string busname;

        public DbusEndpointUriComponentsBusname(DbusEndpointAbsoluteUriBuilder endpointBuilder, string busname)
            : base(endpointBuilder)
        {
            this.busname = busname;
        }

        public DbusEndpointUriComponentsBusname(string busname)
            : base()
        {
            this.busname = busname;
        }

        public override System.Uri CreateUri(string relative)
        {
            return this.endpointBuilder.CreateForBusname(relative, this.busname);
        }
    } // Ends class DbusEndpointUriBusname

    internal class DbusEndpointUriComponentsPath : DbusEndpointUriComponents
    {
        string path;

        public DbusEndpointUriComponentsPath(DbusEndpointAbsoluteUriBuilder endpointBuilder, string path)
            : base(endpointBuilder)
        {
            this.path = path;
        }

        public DbusEndpointUriComponentsPath(string path)
            : base()
        {
            this.path = path;
        }

        public override System.Uri CreateUri(string relative)
        {
            return this.endpointBuilder.CreateForPath(relative, this.path);
        }
    } // Ends class DbusEndpointUriPath
} // Ends namespace Udbus.WCF.Client
