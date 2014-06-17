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

namespace XenClientGuestService
{
    internal static class DbusUtils
    {
        #region Debugging
        internal static void Console_io_debug(IntPtr logpriv, String buf)
        {
            Console.WriteLine("TestDbusWCFConsole Host: {0}", buf);
        }
        internal static void Console_Signal_io_debug(IntPtr logpriv, String buf)
        {
            Console.WriteLine("TestDbusWCFConsole Host Signal: {0}", buf);
        }

        internal static Udbus.Core.IUdbusMessageVisitor DebugVisitor(Udbus.Core.IUdbusMessageVisitor visitor)
        {
            Udbus.Core.UdbusMessageVisitorDumpLog visitorDump = new Udbus.Core.UdbusMessageVisitorDumpLog();
            Udbus.Core.UdbusMessageVisitorMulti visitorMulti = new Udbus.Core.UdbusMessageVisitorMulti(visitorDump, visitor);
            return visitorMulti;
        }
        #endregion // Debugging

        #region Pass Signal Connection as Parameters
        internal struct VisitorThreadInfo
        {
            public Udbus.Serialization.UdbusConnector connector;
            public Udbus.Core.IUdbusMessageVisitor visitor;
            public Udbus.Core.DbusMessageReceiverPool pool;
            public System.Threading.EventWaitHandle stop;
            public System.IO.TextWriter output;
            private static readonly System.IO.TextWriter DefaultTextWriter = Console.Out;
            private static System.Threading.EventWaitHandle DefaultWaitHandle() { return new System.Threading.ManualResetEvent(false); }

            public VisitorThreadInfo(Udbus.Serialization.UdbusConnector connector,
                Udbus.Core.IUdbusMessageVisitor visitor,
                Udbus.Core.DbusMessageReceiverPool pool,
                System.Threading.EventWaitHandle stop,
                System.IO.TextWriter output)
            {
                this.connector = connector;
                this.visitor = visitor;
                this.pool = pool;
                this.stop = stop;
                this.output = output;
            }

            public VisitorThreadInfo(Udbus.Core.ServiceConnectionParams serviceConnectionParams)
                : this(serviceConnectionParams.SignalConnector, serviceConnectionParams.SignalVisitor, serviceConnectionParams.ReceiverPool, DefaultWaitHandle(), DefaultTextWriter)
            {
            }

            public VisitorThreadInfo(Udbus.Core.ServiceConnectionParams serviceConnectionParams,
                System.Threading.EventWaitHandle stop)
                : this(serviceConnectionParams.SignalConnector, serviceConnectionParams.SignalVisitor, serviceConnectionParams.ReceiverPool, stop, DefaultTextWriter)
            {
            }
        }

        internal static void LoopSignals(VisitorThreadInfo threadInfo)
        {
            Udbus.Core.IUdbusMessageVisitor visitorPool = new Udbus.Core.PoolVisitor(threadInfo.pool);
            // We assume that the signalVisitor has been registered with the pool.
            Udbus.Core.IUdbusMessageVisitor visitor = DebugVisitor(visitorPool);
            threadInfo.output.WriteLine("Entering pool loop...");
            threadInfo.pool.LoopMessages(threadInfo.connector, threadInfo.stop);
            //Udbus.Core.UdbusVisitorFunctions.LoopUdbus(threadInfo.connector, visitor, threadInfo.output, threadInfo.stop);
            threadInfo.output.WriteLine("Signal loop thread ending...");
        }


        private static void LoopSignals(object o)
        {
            VisitorThreadInfo threadInfo = (VisitorThreadInfo)o;
            LoopSignals(threadInfo);
        }

        internal static System.Threading.Thread LoopSignalsThread()
        {
            return new System.Threading.Thread(DbusUtils.LoopSignals);
        }
        #endregion // Pass Signal Connection as Parameters


        internal static void RunHosts(IEnumerable<System.ServiceModel.ServiceHost> hosts)
        {
            foreach (System.ServiceModel.ServiceHost host in hosts)
            {
                //host.Description.Behaviors.Remove<System.ServiceModel.Description.ServiceDebugBehavior>();
                System.ServiceModel.Description.ServiceDebugBehavior debugBehavior = host.Description.Behaviors.Find<System.ServiceModel.Description.ServiceDebugBehavior>();
                if (debugBehavior == null)
                {
                    Console.WriteLine("Adding DebugBehavior");
                    debugBehavior = new System.ServiceModel.Description.ServiceDebugBehavior();
                    debugBehavior.IncludeExceptionDetailInFaults = true;
                    host.Description.Behaviors.Add(debugBehavior);
                }
                else
                {
                    Console.WriteLine("IncludeExceptionDetailInFaults={0}", debugBehavior.IncludeExceptionDetailInFaults);
                    debugBehavior.IncludeExceptionDetailInFaults = true;
                }
            }

            try // To open hosts
            {
                foreach (System.ServiceModel.ServiceHost host in hosts)
                {
                    host.Open();
                }
            } // Ends try to open hosts
            finally // Opened hosts
            {
                foreach (System.ServiceModel.ServiceHost host in hosts)
                {
                    Console.WriteLine();
                    if (host.State == System.ServiceModel.CommunicationState.Faulted)
                    {
                        host.Abort();
                    }
                    else
                    {
                        foreach (System.ServiceModel.Description.ServiceEndpoint endpoint in host.Description.Endpoints)
                        {
                            Console.WriteLine("Endpoint: {0}", endpoint.Address);
                        }

                        Console.WriteLine(string.Format("{0} Behaviors", host.Description.Name));
                        foreach (System.ServiceModel.Description.IServiceBehavior behavior in host.Description.Behaviors)
                        {
                            Console.WriteLine(String.Format(" {0}", behavior.ToString()));
                        }
                        Console.WriteLine("Host State: {0}", host.State);
                    }

                } // Ends loop over hosts
            }
        }
    } // Ends internal static class DbusUtils
} // Ends namespace 
