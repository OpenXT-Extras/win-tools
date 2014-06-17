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

namespace Udbus.Core.Logging
{
    public static class LogCreation
    {
        public struct LogCreationDatum
        {
            public System.Diagnostics.SourceLevels sourceLevel;
            public string format;
            public IEnumerable<System.Diagnostics.TraceListener> listeners;
            public KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields;

            public LogCreationDatum(System.Diagnostics.SourceLevels sourceLevel,
                string format,
                IEnumerable<System.Diagnostics.TraceListener> listeners,
                params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
            {
                this.sourceLevel = sourceLevel;
                this.format = format;
                this.listeners = listeners;
                this.fields = fields;
            }

            public LogCreationDatum(System.Diagnostics.SourceLevels sourceLevel,
                string format,
                System.Diagnostics.TraceListener listener,
                params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
                : this(sourceLevel, format, new System.Diagnostics.TraceListener[] { listener }, fields)
            {
            }
        } // Ends struct LogCreationDatum

        public const System.Diagnostics.SourceLevels defaultSourceLevel =
#if DEBUG
            System.Diagnostics.SourceLevels.Verbose
#else // !DEBUG
            System.Diagnostics.SourceLevels.Off
#endif // DEBUG
        ;
        public const string defaultFormat = ">{Timestamp} : Thread {ThreadId} : {Name}/{Level} < : {Message}";

        /// <summary>
        /// Create trace sources containing multiple listeners with the multiple formats, source levels and fields.
        /// </summary>
        /// <param name="creationData">Creation data for TraceSources.</param>
        /// <param name="name">TraceSource name.</param>
        /// <returns>TraceSources containing listeners all sharing same format.</returns>
        private static ICollection<System.Diagnostics.TraceSource> CreateTraceSource(string name, IEnumerable<LogCreationDatum> creationData)
        {
            LinkedList<System.Diagnostics.TraceSource> tracesources = new LinkedList<System.Diagnostics.TraceSource>();
            foreach (LogCreationDatum creationDatum in creationData)
            {
                System.Diagnostics.TraceSource ts = new System.Diagnostics.TraceSource(name);
                Udbus.Core.Logging.Formatter formatter = new Udbus.Core.Logging.Formatter(creationDatum.format, creationDatum.fields);
                IEnumerable<System.Diagnostics.TraceListener> listeners = Udbus.Core.Logging.TraceListenerFormat.GenerateTraceListener(formatter, creationDatum.listeners);

                // Remove OutputDebug listener if not debugging.
#if !DEBUG
//#if !TRACE
                ts.Listeners.Remove("Default");
//#endif // !TRACE
#endif // !DEBUG

                // Add listeners.
                ts.Listeners.AddRange(listeners.ToArray());

                ts.Attributes.Add("autoflush", "true");
                ts.Attributes.Add("indentsize", "2");

                // See System.Diagnostics.SourceLevels.
                System.Diagnostics.SourceSwitch sourceSwitch = new System.Diagnostics.SourceSwitch(name, creationDatum.sourceLevel.ToString());
                ts.Switch = sourceSwitch;
                tracesources.AddLast(ts);
            }
            return tracesources;
        }

        /// <summary>
        /// Create trace source containing multiple listeners with the same format.
        /// </summary>
        /// <param name="sourceLevel">Source level for TraceSource.</param>
        /// <param name="name">TraceSource name.</param>
        /// <param name="format">TraceListeners' format.</param>
        /// <param name="listeners">TraceListeners to write to.</param>
        /// <param name="fields">Additional format fields.</param>
        /// <returns>TraceSource containing listeners all sharing same format.</returns>
        private static System.Diagnostics.TraceSource CreateTraceSource(System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , IEnumerable<System.Diagnostics.TraceListener> listeners
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            LogCreationDatum creationDatum = new LogCreationDatum(sourceLevel, format, listeners, fields);
            ICollection<System.Diagnostics.TraceSource> tracesources = CreateTraceSource(name, new LogCreationDatum[] { creationDatum });
            return tracesources.First();
        }

        #region General log creation
        public static Udbus.Core.Logging.ILog CreateFormatLogger(System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , IEnumerable<System.Diagnostics.TraceListener> listeners
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return Udbus.Core.Logging.LogTraceSource.Create(name, CreateTraceSource(sourceLevel, name, format, listeners, fields));
        }

        public static Udbus.Core.Logging.ILog CreateFormatLogger(System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , System.Diagnostics.TraceListener listener
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return CreateFormatLogger(sourceLevel, name, format, new System.Diagnostics.TraceListener[] { listener }, fields);
        }

        public static Udbus.Core.Logging.ILog CreateFormatLoggerRoot(System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , IEnumerable<System.Diagnostics.TraceListener> listeners
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return Udbus.Core.Logging.LogTraceSource.CreateRoot(name, CreateTraceSource(sourceLevel, name, format, listeners, fields));
        }

        public static Udbus.Core.Logging.ILog CreateFormatLoggerRoot(System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , System.Diagnostics.TraceListener listener
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return CreateFormatLoggerRoot(sourceLevel, name, format, new System.Diagnostics.TraceListener[] { listener }, fields);
        }

        public static Udbus.Core.Logging.ILog ReplaceFormatLogger(Udbus.Core.Logging.ILog log
            , System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , IEnumerable<System.Diagnostics.TraceListener> listeners
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return Udbus.Core.Logging.LogTraceSource.Replace(log, name, CreateTraceSource(sourceLevel, name, format, listeners, fields));
        }

        public static Udbus.Core.Logging.ILog ReplaceFormatLogger(Udbus.Core.Logging.ILog log
            , System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , System.Diagnostics.TraceListener listener
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return ReplaceFormatLogger(log, sourceLevel, name, format, new System.Diagnostics.TraceListener[] { listener }, fields);
        }

        public static Udbus.Core.Logging.ILog ReplaceFormatLoggerRoot(Udbus.Core.Logging.ILog log
            , System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , IEnumerable<System.Diagnostics.TraceListener> listeners
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return Udbus.Core.Logging.LogTraceSource.ReplaceRoot(log, name, CreateTraceSource(sourceLevel, name, format, listeners, fields));
        }

        public static Udbus.Core.Logging.ILog ReplaceFormatLoggerRoot(Udbus.Core.Logging.ILog log
            , System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , System.Diagnostics.TraceListener listener
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            return ReplaceFormatLoggerRoot(log, sourceLevel, name, format, new System.Diagnostics.TraceListener[] { listener }, fields);
        }

        #endregion // General log creation

        #region General logs creation
        public static Udbus.Core.Logging.ILog CreateFormatLogger(string name
            , params LogCreationDatum[] creationData)
        {
            return Udbus.Core.Logging.LogTraceSource.Create(name, CreateTraceSource(name, creationData));
        }

        public static Udbus.Core.Logging.ILog CreateFormatLoggerRoot(string name
            , params LogCreationDatum[] creationData)
        {
            return Udbus.Core.Logging.LogTraceSource.CreateRoot(name, CreateTraceSource(name, creationData));
        }

        public static Udbus.Core.Logging.ILog ReplaceFormatLogger(Udbus.Core.Logging.ILog log
            , string name
            , params LogCreationDatum[] creationData)
        {
            return Udbus.Core.Logging.LogTraceSource.Replace(log, name, CreateTraceSource(name, creationData));
        }

        public static Udbus.Core.Logging.ILog ReplaceFormatLoggerRoot(Udbus.Core.Logging.ILog log
            , string name
            , params LogCreationDatum[] creationData)
        {
            return Udbus.Core.Logging.LogTraceSource.ReplaceRoot(log, name, CreateTraceSource(name, creationData));
        }

        #endregion // General logs creation

        #region Udbus log creation implementation
        private static Udbus.Core.Logging.ILog CreateFormatLoggerImpl(System.Diagnostics.SourceLevels sourceLevel
            , string name, string format
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {
            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
            return CreateFormatLogger(sourceLevel, name, format, consoleListener, fields);
        }

        private static Udbus.Core.Logging.ILog CreateFormatLoggerImpl(string name, string format
            , params KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[] fields)
        {

            return CreateFormatLoggerImpl(defaultSourceLevel, name, format, fields);
        }

        private const string PoolFormat = ">{Timestamp} : Thread {ThreadId} : {Name}({Level}). Pool {Pool}< : {Message}";
        private const string ReceiverPool = "ReceiverPool";
        private const string ReceiverPoolMessage = ReceiverPool + "Message";
        private const string Pool = "Pool";

        #endregion // Udbus log creation implementation

        #region Udbus log creation

        internal static Udbus.Core.Logging.ILog CreateReceiverPoolLogger(Udbus.Core.DbusMessageReceiverPool pool)
        {
            return CreateFormatLoggerImpl(System.Diagnostics.SourceLevels.Off//defaultSourceLevel//System.Diagnostics.SourceLevels.Off
                , ReceiverPool, PoolFormat
                , new KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>(Pool, pool.GetPoolId)
            );
        }

        internal static Udbus.Core.Logging.ILog CreateReceiverPoolMessageLogger(Udbus.Core.DbusMessageReceiverPool pool)
        {
            return CreateFormatLoggerImpl(
#if DEBUG
                  System.Diagnostics.SourceLevels.Off//Verbose//Off
#else // !DEBUG
                  System.Diagnostics.SourceLevels.Off
#endif // DEBUG
                , ReceiverPoolMessage, PoolFormat
                , new KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>(Pool, pool.GetPoolId)
            );
        }

        internal static Udbus.Core.Logging.ILog CreateDbusServiceLogger()
        {
            return CreateFormatLoggerImpl("DbusService", defaultFormat);
        }

        internal static Udbus.Core.Logging.ILog CreateRegisterSignalHandlerFunctionsLogger()
        {
            return CreateFormatLoggerImpl("RegisterSignals", defaultFormat);
        }

        internal static Udbus.Core.Logging.ILog CreateMessageDumpLogger()
        {
            return CreateFormatLoggerImpl("MessageDump", defaultFormat);
        }

        #endregion // Udbus log creation

        #region Udbus all log functions

        public static void Bake()
        {
            LogNodeBase.BakeAll();
        }

        public static void DumpLogs(System.IO.TextWriter output)
        {
            LogNodeBase.DumpLogs(output);
        }
        #endregion // Udbus all log functions

        #region Logging for Udbus serialization
        static public Udbus.Serialization.UdbusDelegates.D_io_debug MakeLogIoDebug(Udbus.Core.Logging.ILog log)
        {
            return (logpriv, buf) => log.Info(buf);
        }
        #endregion // Logging for Udbus serialization
    } // Ends class LogCreation
}
