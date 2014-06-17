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
using System.Text;
using System.Linq;

namespace XenClientGuestService.Logging
{
    internal static class LogCreation
    {
        private const System.Diagnostics.SourceLevels defaultSourceLevel = Udbus.Core.Logging.LogCreation.defaultSourceLevel;
        private const System.Diagnostics.SourceLevels defaultRootSourceLevel = System.Diagnostics.SourceLevels.Warning;
        private const System.Diagnostics.SourceLevels defaultIoDebugLevel = System.Diagnostics.SourceLevels.All;
        private const System.Diagnostics.SourceLevels defaultXcDiagLevel = defaultSourceLevel;
        private const System.Diagnostics.SourceLevels defaultWCFServiceLevel = defaultSourceLevel;
        private const string defaultFormat = ">Process {ProcessId} : Thread {ThreadId} : {Name}/{Level} < : {Message}";
        private const string defaultConsoleFormat = ">{Timestamp} : Process {ProcessId} - Thread {ThreadId} : {Name}/{Level} < : {Message}";
        private const string defaultIoDebugFormat = defaultConsoleFormat;
        private const string defaultXcDiagFormat = defaultFormat;
        private const string defaultWCFServiceFormat = defaultFormat;
        internal const string Service = "Service";

        private static System.Diagnostics.TraceListener[] MakeListenersForService(System.ServiceProcess.ServiceBase service)
        {
            System.Diagnostics.EventLogTraceListener eventListener = new System.Diagnostics.EventLogTraceListener(service.EventLog);
            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
            return new System.Diagnostics.TraceListener[] { consoleListener, eventListener };
        }

        private static string assertServiceName(string proposed){
            if (string.IsNullOrEmpty(proposed))
            {
                return Service;
            }
            else
            {
                return proposed;
            }
        }

        #region Root Logger
        internal static Udbus.Core.Logging.ILog CreateRootLogger(System.ServiceProcess.ServiceBase service)
        {
            return Udbus.Core.Logging.LogCreation.CreateFormatLoggerRoot(defaultRootSourceLevel, assertServiceName(service.ServiceName), defaultFormat, MakeListenersForService(service));
        }
        
        internal static Udbus.Core.Logging.ILog ReplaceRootLogger(System.ServiceProcess.ServiceBase service
            , Udbus.Core.Logging.ILog log)
        {
            return Udbus.Core.Logging.LogCreation.ReplaceFormatLoggerRoot(log, defaultRootSourceLevel, assertServiceName(service.ServiceName), defaultFormat, MakeListenersForService(service));
        }
        #endregion // Root Logger

        #region Service Logger
        internal static Udbus.Core.Logging.ILog CreateServiceLogger(System.ServiceProcess.ServiceBase service)
        {
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(defaultSourceLevel, assertServiceName(service.ServiceName), defaultFormat, MakeListenersForService(service));
        }

        internal static Udbus.Core.Logging.ILog ReplaceServiceLogger(System.ServiceProcess.ServiceBase service, Udbus.Core.Logging.ILog log)
        {
            return Udbus.Core.Logging.LogCreation.ReplaceFormatLogger(log, defaultSourceLevel, assertServiceName(service.ServiceName), defaultFormat, MakeListenersForService(service));
        }
        #endregion // Service Logger

        #region Dbus Hosts logger
        const string DbusHostsName = "XenClientGuestService.DbusHosts";
        const string IoDebugName = "XenClientGuestService.IoDebug";
        const string XcDiagName = "XenClientGuestService.xcdiag";
        const string WCFServiceName = "XenClientGuestService.wcf.service";

        internal static Udbus.Core.Logging.ILog CreateDbusHostsLogger(DbusHosts dbusHosts, System.Diagnostics.EventLog eventLog)
        {
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(DbusHostsName
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultSourceLevel, defaultConsoleFormat, new System.Diagnostics.ConsoleTraceListener())
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultSourceLevel, defaultFormat, new System.Diagnostics.EventLogTraceListener(eventLog))
            );
        }

        internal static Udbus.Core.Logging.ILog CreateDbusHostsLogger(DbusHosts dbusHosts)
        {
            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(defaultSourceLevel, DbusHostsName, defaultFormat, consoleListener);
        }

        internal static Udbus.Core.Logging.ILog CreateIoDebugLogger(System.Diagnostics.EventLog eventLog)
        {
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(IoDebugName
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultIoDebugLevel, defaultIoDebugFormat, new System.Diagnostics.ConsoleTraceListener())
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultIoDebugLevel, defaultIoDebugFormat, new System.Diagnostics.EventLogTraceListener(eventLog))
            );
        }

        internal static Udbus.Core.Logging.ILog CreateIoDebugLogger()
        {
            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(defaultIoDebugLevel, IoDebugName, defaultIoDebugFormat, consoleListener);
        }

        internal static Udbus.Core.Logging.ILog CreateXcDiagLogger(System.Diagnostics.EventLog eventLog)
        {
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(XcDiagName
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultXcDiagLevel, defaultXcDiagFormat, new System.Diagnostics.ConsoleTraceListener())
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultXcDiagLevel, defaultXcDiagFormat, new System.Diagnostics.EventLogTraceListener(eventLog))
            );
        }

        internal static Udbus.Core.Logging.ILog CreateXcDiagLogger()
        {
            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(defaultXcDiagLevel, XcDiagName, defaultXcDiagFormat, consoleListener);
        }

        internal static Udbus.Core.Logging.ILog CreateWCFServiceLogger(System.Diagnostics.EventLog eventLog)
        {
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(WCFServiceName
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultWCFServiceLevel, defaultWCFServiceFormat, new System.Diagnostics.ConsoleTraceListener())
                , new Udbus.Core.Logging.LogCreation.LogCreationDatum(defaultWCFServiceLevel, defaultWCFServiceFormat, new System.Diagnostics.EventLogTraceListener(eventLog))
            );
        }

        internal static Udbus.Core.Logging.ILog CreateWCFServiceLogger()
        {
            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
            return Udbus.Core.Logging.LogCreation.CreateFormatLogger(defaultWCFServiceLevel, WCFServiceName, defaultWCFServiceFormat, consoleListener);
        }


        #endregion // Dbus Hosts logger
    } // Ends class LogCreation
}
