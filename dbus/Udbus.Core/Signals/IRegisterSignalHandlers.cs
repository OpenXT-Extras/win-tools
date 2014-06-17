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
    public struct SignalEntry
    {
        public delegate void HandleSignalMessageDelegate(Udbus.Core.UdbusMessagePair messageData);

        public Udbus.Core.SignalKey key;
        public HandleSignalMessageDelegate signalHandler;

        public SignalEntry(Udbus.Core.SignalKey key, HandleSignalMessageDelegate signalHandler)
        {
            this.key = key;
            this.signalHandler = signalHandler;
        }

        public SignalEntry(Udbus.Core.SignalKey key, Udbus.Core.IHandleDbusSignal signalHandler)
            : this(key, signalHandler.HandleSignalMessage)
        {
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(". key='{0}'. handle='{1}'", this.key.ToString(), this.signalHandler);
        }
    } // Ends SignalEntry

    public interface IRegisterSignalHandlers
    {
        void AddSignalHandler(Udbus.Core.SignalEntry entry);
        void RemoveSignalHandler(Udbus.Core.SignalEntry entry);
    }

    static public class RegisterSignalHandlerFunctions
    {
        static public void RegisterForSignal(Udbus.Core.SignalEntry signalEntry, params Udbus.Core.IRegisterSignalHandlers[] registers)
        {
            Udbus.Core.Logging.ILog log = Udbus.Core.Logging.LogCreation.CreateRegisterSignalHandlerFunctionsLogger();
            log.Info("Registering signal: {0}", signalEntry.ToString());
            foreach (Udbus.Core.IRegisterSignalHandlers register in registers)
            {
                log.Info("Registering signal with {0}", register);
                register.AddSignalHandler(signalEntry);
            }
        }
    } // Ends class RegisterSignalHandlerFunctions
} // Ends namespace Udbus.Core
