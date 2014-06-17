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
    public interface IHandleDbusSignal
    {
        void HandleSignalMessage(UdbusMessagePair messageData);

    } // Ends interface IHandleDbusSignal

    public class HandleDbusSignalNoOp : IHandleDbusSignal
    {
        public void HandleSignalMessage(UdbusMessagePair messageData)
        {
        }

    } // Ends class HandleDbusSignalNoOp

    public class HandleDbusSignalMutli : IHandleDbusSignal
    {
        private IEnumerable<IHandleDbusSignal> handlers;

        public HandleDbusSignalMutli(IEnumerable<IHandleDbusSignal> handlers)
        {
            this.handlers = handlers;
        }

        public HandleDbusSignalMutli(params IHandleDbusSignal[] handlers)
        {
            this.handlers = handlers;
        }

        public void HandleSignalMessage(UdbusMessagePair messageData)
        {
            foreach (IHandleDbusSignal handler in this.handlers)
            {
                handler.HandleSignalMessage(messageData);
            }
        }
    } // Ends class HandleDbusSignalMutli

    public class HandleDbusSignalDump : IHandleDbusSignal
    {
        public delegate void WriteMessageDelegate(string text);
        private WriteMessageDelegate writemessage;

        public HandleDbusSignalDump(WriteMessageDelegate writemessage)
        {
            this.writemessage = writemessage;
        }

        public void HandleSignalMessage(UdbusMessagePair messageData)
        {
            this.writemessage(messageData.Data.ToString());
        }
    } // Ends class HandleDbusSignalDump

} // Ends namespace Udbus.Core
