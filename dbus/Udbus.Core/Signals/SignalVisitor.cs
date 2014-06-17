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
    public class SignalVisitor : Udbus.Core.IUdbusMessageVisitor, Udbus.Core.IStoppableUdbusMessageVisitor, Udbus.Core.IRegisterSignalHandlers
        , Udbus.Core.IHandleDbusSignal
    {
        #region Fields
        private readonly IDictionary<Udbus.Core.SignalKey, SignalEntry.HandleSignalMessageDelegate> signals = new Dictionary<Udbus.Core.SignalKey, SignalEntry.HandleSignalMessageDelegate>();
        private bool stop = false;
        #endregion // Fields

        #region SignalHandler Registration
        public void AddSignalHandler(SignalEntry entry)
        {
            SignalEntry.HandleSignalMessageDelegate handler;
            if (this.signals.TryGetValue(entry.key, out handler))
            {
                handler += entry.signalHandler;
                this.signals[entry.key] = handler; // Seriously ? Sigh...
            }
            else
            {
                this.signals[entry.key] = entry.signalHandler;
            }
        }

        public void RemoveSignalHandler(SignalEntry entry)
        {
            SignalEntry.HandleSignalMessageDelegate handler;
            if (this.signals.TryGetValue(entry.key, out handler))
            {
                handler -= entry.signalHandler;
                this.signals[entry.key] = handler; // Seriously ? Sigh...
            }
        }
        #endregion // SignalHandler Registration

        #region Udbus.Core.IUdbusMessageVisitor functions

        public void Reset()
        {
        }

        public bool Stop
        {
            get { return this.stop; }
            set { this.stop = value; }
        }

        public void onDefault(Udbus.Core.UdbusMessagePair messageData)
        {
        }

        public void onError(Udbus.Core.UdbusMessagePair messageData)
        {
        }

        public void onMethod(Udbus.Core.UdbusMessagePair messageData)
        {
        }

        public void onMethodReturn(Udbus.Core.UdbusMessagePair messageData)
        {
        }

        public void onResultError(int error)
        {
        }

        public void onSignal(Udbus.Core.UdbusMessagePair messageData)
        {
            Udbus.Core.SignalKey key = new Udbus.Core.SignalKey(messageData.Data.path, messageData.Data.interface_, messageData.Data.method);

            //Console.WriteLine("Whoop !");
            //Console.WriteLine("Signal Registry...");
            //foreach (Udbus.Core.SignalKey keyIter in this.signals.Keys)
            //{
            //    Console.WriteLine(keyIter.Key);
            //}
            //Console.WriteLine("Message key...");
            //Console.WriteLine(key.Key);

            SignalEntry.HandleSignalMessageDelegate handler;

            if (this.signals.TryGetValue(key, out handler))
            {
                handler(messageData);
            }
        }
        #endregion // Udbus.Core.IUdbusMessageVisitor functions

        #region IHandleDbusSignal Members

        public void HandleSignalMessage(UdbusMessagePair messageData)
        {
            this.onSignal(messageData);
        }

        #endregion // Ends IHandleDbusSignal Members
    } // Ends class SignalVisitor
} // Ends namespace Udbus.Core
