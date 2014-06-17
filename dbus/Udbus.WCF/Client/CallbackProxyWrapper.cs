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
    /// Helpful for sticking methods and calls in one place to simplify access.
    /// </summary>
    /// <typeparam name="TProxyInterface_"></typeparam>
    /// <typeparam name="TCallback_"></typeparam>
    /// <typeparam name="TCallbackProxy_"></typeparam>
    public class CallbackProxyWrapper<TProxyInterface_, TCallback_, TCallbackProxy_>
        where TCallbackProxy_ : Udbus.WCF.Client.CallbackProxyBase<TProxyInterface_, TCallback_>
        where TProxyInterface_ : class
        where TCallback_ : class
    {
        TCallbackProxy_ callbackProxy;

        public CallbackProxyWrapper(TCallbackProxy_ callbackProxy)
        {
            this.callbackProxy = callbackProxy;
        }

        public TProxyInterface_ ProxyInterface { get { return this.callbackProxy.ProxyInterface; } }
        public TCallbackProxy_ Callback { get { return this.callbackProxy; } }

    } // Ends class CallbackProxyWrapper
}
