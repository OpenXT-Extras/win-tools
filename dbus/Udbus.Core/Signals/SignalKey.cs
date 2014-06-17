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

namespace Udbus.Core
{
    public struct DbusSignalParams
    {
        private readonly string path;
        private readonly string intf;

        public string Path { get { return this.path; } }
        public string Interface { get { return this.intf; } }

        public DbusSignalParams(string path, string intf)
        {
            this.path = path;
            this.intf = intf;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(". Path='{0}'. Interface='{1}'", this.Path, this.Interface);
        }
    }

    public struct SignalKey
    {
        DbusSignalParams signalParams;
        //private readonly string path;
        //private readonly string intf;
        private readonly string signal;
        private readonly string key;
        private readonly int hash;
        const string keyFormat = "{0}:{1}:{2}";

        public Udbus.Core.DbusSignalParams SignalParams { get { return this.signalParams; } }
        public string Path { get { return this.signalParams.Path; } }
        public string Interface { get { return this.signalParams.Interface; } }
        public string Signal { get { return this.Signal; } }
        public string Key { get { return this.key; } }

        static private string BuildKey(string path, string intf, string signal)
        {
            return string.Format(keyFormat, path, intf, signal);
        }
        public SignalKey(string path, string intf, string signal)
        {
            this.signalParams = new DbusSignalParams(path, intf);
            //this.path = path;
            //this.intf = intf;
            this.signal = signal;
            this.key = BuildKey(path, intf, signal);
            this.hash = this.key.GetHashCode();
        }

        public SignalKey(SignalKey source)
        {
            this.signalParams = source.signalParams;
            //this.path = source.path;
            //this.intf = source.intf;
            this.signal = source.signal;
            this.key = source.key;
            this.hash = source.hash;
        }

        public override int GetHashCode()
        {
            return this.hash;
        }

        public override bool Equals(object obj)
        {
            bool equal = obj is SignalKey;
            if (equal)
            {
                SignalKey other = (SignalKey)obj;
                equal = other.key == this.key;
            }

            return equal;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(". key={0}", this.key);
        }
    } // Ends SignalKey
} // Ends namespace Udbus.Core
