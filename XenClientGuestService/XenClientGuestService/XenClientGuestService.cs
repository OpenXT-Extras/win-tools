//
// Copyright (c) 2014 Citrix Systems, Inc.
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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace XenClientGuestService
{
    public partial class XenClientGuestService : ServiceBase
    {
        private DbusHosts dbusHosts;
        private Udbus.Core.Logging.ILog log;
        com.citrix.xenclient.xenmgr.diag.diagService diagSignals;
        DbusUtils.VisitorThreadInfo diagsignalsThreadInfo;

        public XenClientGuestService()
        {
            // EventSource needs to be something for logging not to throw exception.
            bool bNoSource = string.IsNullOrEmpty(this.EventLog.Source);
            if (bNoSource) // If no source string setup
            {
                if (!string.IsNullOrEmpty(this.ServiceName))
                {
                    this.EventLog.Source = this.ServiceName;
                }
                else
                {
                    this.EventLog.Source = Logging.LogCreation.Service;
                }

            } // Ends if no source string setup

            log = Logging.LogCreation.CreateServiceLogger(this); // Log pre initialised service.
            Udbus.Core.Logging.ILog logRoot = Logging.LogCreation.CreateRootLogger(this); // Root logger pre initialised service.

            InitializeComponent();

            // Try to ammend event source name to something more meaningful.
            if (bNoSource && !string.IsNullOrEmpty(this.ServiceName)) // If no source string setup
            {
                this.EventLog.Source = this.ServiceName;

            } // Ends if no source string setup

            logRoot = Logging.LogCreation.ReplaceRootLogger(this, logRoot); // Root logger
            log = Logging.LogCreation.ReplaceServiceLogger(this, log); // Log post initialised service.

            // Initialize fields.
            this.dbusHosts = new DbusHosts(this.EventLog);
            Udbus.Core.ServiceConnectionParams serviceConnectionParams;
            this.diagSignals = XcDiagRunner.CreateDbusDiag(out serviceConnectionParams, log, Log_Io_Debug);
            this.diagSignals.GatherRequest += this.OnGatherRequest;
            System.Threading.Thread xcdiagSignalsThread;
            this.diagsignalsThreadInfo = DbusHosts.RunSignalsAsync(out xcdiagSignalsThread, serviceConnectionParams);

            //We're gonna start doing our own thing logs wise...
            if (!EventLog.SourceExists("XenClientGuestService")) EventLog.CreateEventSource("XenClientGuestService", "Application");
            EventLog.WriteEntry("XenClientGuestService", "XenClientGuestService Logs Initialised");
        }

        public void OnServiceStart(string[] args)
        {
            EventLog.WriteEntry("XenClientGuestService", "Starting XenClientGuestService");
            try
            {
                dbusHosts.RunHosts();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("XenClientGuestService", "Unhandled Exception, StackTrace: " + e.StackTrace);
            }
        }

        public void OnServiceStop()
        {
            EventLog.WriteEntry("XenClientGuestService", "Stopping XenClientGuestService");
            this.diagSignals.Dispose();
            this.diagsignalsThreadInfo.stop.Set();
            dbusHosts.StopHosts();
        }

        #region ServiceBase overrides
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            this.OnServiceStart(args);
        }

        protected override void OnStop()
        {
            this.OnServiceStop();
            base.OnStop();
        }
        #endregion ServiceBase overrides

        public void Log_Io_Debug(IntPtr logpriv, String buf)
        {
            log.Info(buf);
        }

        internal void OnGatherRequest(object sender, com.citrix.xenclient.xenmgr.diag.GatherRequestArgs args)
        {
            XcDiagRunner xcdiagRunner = new XcDiagRunner(this.EventLog);
            global::XenClientGuestService.wcf.XenClientGuestWCFService wcfService = new global::XenClientGuestService.wcf.XenClientGuestWCFService();
            xcdiagRunner.RunAsync(wcfService.uuid, args.Mode);
        }
    }
}
