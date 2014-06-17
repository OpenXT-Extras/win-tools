//
// Copyright (c) 2013 Citrix Systems, Inc.
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
    internal class DbusHosts
    {
        const int ConnectionWaitMilliseconds = 5000;

        Udbus.v4v.v4vConnection connection = null;
        Udbus.Core.ServiceConnectionParams serviceConnectionParams = null;
        DbusUtils.VisitorThreadInfo threadInfo = default(DbusUtils.VisitorThreadInfo);
        System.Threading.Thread threadSignalLoop = null;
        IEnumerable<System.ServiceModel.ServiceHost> hosts = null;
        private Udbus.Core.Logging.ILog log;
        private Udbus.Core.Logging.ILog logIoDebug;
        private System.Threading.ManualResetEvent eventStop = new System.Threading.ManualResetEvent(false);

        public DbusHosts()
        {
            this.log = Logging.LogCreation.CreateDbusHostsLogger(this);
            this.logIoDebug = Logging.LogCreation.CreateIoDebugLogger();
        }

        public DbusHosts(System.Diagnostics.EventLog eventLog)
        {
            this.log = Logging.LogCreation.CreateDbusHostsLogger(this, eventLog);
            this.logIoDebug = Logging.LogCreation.CreateIoDebugLogger(eventLog);
        }

        private void InitFields()
        {
            this.connection = null;
            this.serviceConnectionParams = null;
            this.threadInfo = default(DbusUtils.VisitorThreadInfo);
            this.threadSignalLoop = null;
            this.hosts = null;
        }

        void Log_io_debug(IntPtr logpriv, String buf)
        {
            this.logIoDebug.Info(buf);
        }

        static private Udbus.v4v.v4vConnection KeepTryingToConnectToV4V(Udbus.Serialization.UdbusDelegates.D_io_debug io_debug, System.Threading.ManualResetEvent stop, Udbus.Core.Logging.ILog log)
        {
            Udbus.v4v.v4vConnection connection = TryV4VConnection(io_debug, log);

            while (connection == null && stop.WaitOne(0) == false)
            {
                System.Threading.Thread.Sleep(ConnectionWaitMilliseconds);
                connection = TryV4VConnection(io_debug, log);

            } // Ends while failed to connect to V4V

            return connection;
        }

        private Udbus.v4v.v4vConnection KeepTryingToConnectToV4V(Udbus.Serialization.UdbusDelegates.D_io_debug io_debug, System.Threading.ManualResetEvent stop)
        {
            return KeepTryingToConnectToV4V(io_debug, stop, this.log);
        }

        static private Udbus.v4v.v4vConnection TryV4VConnection(Udbus.Serialization.UdbusDelegates.D_io_debug io_debug, Udbus.Core.Logging.ILog log)
        {
            Udbus.v4v.v4vConnection result = null;

            try
            {
                Udbus.v4v.v4vConnection create = new Udbus.v4v.v4vConnection(io_debug);
                result = create;
            }
            catch (Udbus.Serialization.Exceptions.TransportFailureException transportEx)
            {
                log.Exception("Failed to create V4V transport. {0}", transportEx);
            }
            catch (Exception ex)
            {
                log.Exception("Error creating V4V transport. {0}", ex);
            }

            return result;
        }

        private Udbus.v4v.v4vConnection TryV4VConnection(Udbus.Serialization.UdbusDelegates.D_io_debug io_debug)
        {
            return TryV4VConnection(io_debug, this.log);
        }

        static private Udbus.Core.ServiceConnectionParams TryServiceConnectionParams(Udbus.v4v.v4vConnection connection, Udbus.Core.Logging.ILog log)
        {
            Udbus.Core.ServiceConnectionParams result = null;

            try
            {
                Udbus.Core.ServiceConnectionParams create = new Udbus.Core.ServiceConnectionParams(connection);
                result = create;
            }
            catch (Udbus.Serialization.Exceptions.UdbusAuthorisationException authex)
            {
                log.Exception("Authorisation Error creating ServiceConnectionParams. {0}", authex);
            }
            catch (Exception ex)
            {
                log.Exception("Error creating ServiceConnectionParams. {0}", ex);
            }
            return result;
        }

        private Udbus.Core.ServiceConnectionParams TryServiceConnectionParams(Udbus.v4v.v4vConnection connection)
        {
            return TryServiceConnectionParams(connection, this.log);
        }

        public void RunHosts()
        {
            // You get one shot at startup.
            Udbus.v4v.v4vConnection connection = TryV4VConnection(Log_io_debug);

            if (connection == null) // If failed to connect to V4V
            {
                // Continue attempting to setup in another thread.
                this.log.Warning("Failed to connect to V4V. Continuing to establish V4V connection in another thread");
                RunHostsInAnotherThread();

            } // Ends if failed to connect to V4V
            else // Else connected to V4V
            {
                // Try setting up the service connection.
                Udbus.Core.ServiceConnectionParams serviceConnectionParams = TryServiceConnectionParams(connection);

                if (serviceConnectionParams == null) // If failed to set up service connection
                {
                    RunHostsInAnotherThread();

                } // Ends if failed to set up service connection
                else // Else setup service connection
                {
                    FinishRunHosts(connection, serviceConnectionParams);

                } // Ends else setup service connection
            } // Ends else connected to V4V
        }

        private void RunHostsInAnotherThread()
        {
            System.Threading.Thread runhostsThread = new System.Threading.Thread(this.RunHostsThread);
            runhostsThread.Start();
        }

        public static void GetV4vConnection(out Udbus.v4v.v4vConnection connection, out Udbus.Core.ServiceConnectionParams serviceConnectionParams,
            Udbus.Serialization.UdbusDelegates.D_io_debug io_debug, System.Threading.ManualResetEvent stop,
            Udbus.Core.Logging.ILog log)
        {
            connection = null;
            serviceConnectionParams = null;

            while (serviceConnectionParams == null)
            {
                using (Udbus.v4v.v4vConnection connectionTemp = KeepTryingToConnectToV4V(io_debug, stop, log))
                {
                    if (connectionTemp != null) // If got v4v connection
                    {
                        Udbus.v4v.v4vConnection connectionTemp2 = connectionTemp.Release();
                        try
                        {
                            Udbus.Core.ServiceConnectionParams serviceConnectionParamsTemp = TryServiceConnectionParams(connectionTemp2, log);

                            if (serviceConnectionParamsTemp == null) // If failed to create service connection
                            {
                                connectionTemp.Swap(connectionTemp2);

                            } // Ends if failed to create service connection
                            else // Else created service connection
                            {
                                serviceConnectionParams = serviceConnectionParamsTemp;
                                connection = connectionTemp2;

                            } // Ends else created service connection
                        }
                        catch (Exception /*ex*/)
                        {
                            connectionTemp.Swap(connectionTemp2);
                            throw;
                        }

                    } // Ends if got v4v connection
                } // Ends using v4vConnection

                if (serviceConnectionParams == null) // If failed to create connection
                {
                    // Wait a while.
                    System.Threading.Thread.Sleep(ConnectionWaitMilliseconds);

                } // Ends if failed to create connection
            } // Ends while trying to setup service connection
        }

        private void RunHostsThread()
        {
            log.Info("Setting up running hosts in another thread");
            Udbus.v4v.v4vConnection connection = null;
            Udbus.Core.ServiceConnectionParams serviceConnectionParams = null;
            DbusHosts.GetV4vConnection(out connection, out serviceConnectionParams, Log_io_debug, this.eventStop, log);

            log.Info("Established service connection");
            FinishRunHosts(connection, serviceConnectionParams);
            log.Info("Finished setting up running hosts in another thread");
        }

        private void FinishRunHosts(Udbus.v4v.v4vConnection connection, Udbus.Core.ServiceConnectionParams serviceConnectionParams)
        {
            this.connection = connection;
            this.serviceConnectionParams = serviceConnectionParams;

            log.Info("Connection helloed with name {0} and signalName {1}", serviceConnectionParams.Name, serviceConnectionParams.SignalName);
            System.Uri uriBase = new System.Uri(Udbus.WCF.Constants.BaseAddress);

            new Udbus.WCF.Service.Host.Init.InitHostMakerRegistry();
            new global::XenClientGuestService.wcf.Hosts.Init.InitHostMakerRegistry();
            Udbus.WCF.Service.Host.RegistryDictionaryHolder registry = Udbus.WCF.Service.Host.HostMakerRegistry.RegistryDictionaryClone;

            log.Info("Launching hosts...");

            // Build all hosts including "special" properties host.
            ICollection<Udbus.WCF.Service.Host.ServiceHostCreationData> hostdata = Udbus.WCF.Service.Host.HostMakerRegistry.BuildAllHosts(
                registry.Values
                , new Udbus.WCF.Host.WCFServiceParams(serviceConnectionParams)
                , new Uri[] { uriBase }
            );

            System.ServiceModel.NetNamedPipeBinding namedpipeBinding = new System.ServiceModel.NetNamedPipeBinding();
            System.ServiceModel.Channels.Binding binding = namedpipeBinding;
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            foreach (Udbus.WCF.Service.Host.ServiceHostCreationData host in hostdata)
            {
                host.Host.AddServiceEndpoint(host.CreationParams.ContractType, binding, host.CreationParams.RelativeName);
            }

            this.hosts = hostdata.Select<Udbus.WCF.Service.Host.ServiceHostCreationData, System.ServiceModel.ServiceHost>(Udbus.WCF.Service.Host.ServiceHostCreationData.DataToHost);
            DbusUtils.RunHosts(hosts);

            this.threadInfo = RunSignalsAsync(out this.threadSignalLoop, serviceConnectionParams);
        }

        public static DbusUtils.VisitorThreadInfo RunSignalsAsync(out System.Threading.Thread threadSignalLoop, Udbus.Core.ServiceConnectionParams serviceConnectionParams)
        {
            DbusUtils.VisitorThreadInfo threadInfo = new DbusUtils.VisitorThreadInfo(serviceConnectionParams);
            threadSignalLoop = DbusUtils.LoopSignalsThread();
            threadSignalLoop.IsBackground = true;
            threadSignalLoop.Start(threadInfo);
            return threadInfo;
        }

        public void StopHosts()
        {
            try
            {
                log.Info("Stopping signals thread...");
                if (this.threadInfo.stop != null)
                {
                    this.threadInfo.stop.Set();
                }

                if (this.serviceConnectionParams != null)
                {
                    Udbus.Core.IStoppable stoppableSignalVisitor = this.serviceConnectionParams.SignalVisitor;
                    stoppableSignalVisitor.Stop = true;
                    this.serviceConnectionParams.SignalConnection.Cancel();
                }

                log.Info("Stopping signals");
                if (this.threadSignalLoop != null)
                {
                    this.threadSignalLoop.Join();
                }
                log.Info("Stopped signals");

                log.Info("Closing hosts...");
                if (this.hosts != null)
                {
                    foreach (System.ServiceModel.ServiceHost host in hosts)
                    {
                        host.Close();
                    }
                }
                log.Info("Disposing hosts...");
            }
            finally
            {
                Udbus.v4v.v4vConnection connectionDispose = this.connection;
                this.InitFields();
                if (connectionDispose != null)
                {
                    log.Info("Disposing connection");
                    connectionDispose.Dispose();
                    log.Info("Disposed connection");
                }
                else
                {
                    log.Info("No connection to dispose");
                }
            }
        }
    } // Ends class DbusHosts
}
