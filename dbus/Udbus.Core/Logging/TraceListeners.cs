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


namespace Udbus.Core.Logging
{
    /// <summary>
    /// Holds extra trace data for logging, provided at point of trace rather than logger initialisation.
    /// </summary>
    public struct TraceData
    {
        internal readonly string source;
        internal readonly string level;
        public TraceData(string source, string level)
        {
            this.source = source;
            this.level = level;
        }
        internal string GetSource() { return this.source; }
        internal string GetLevel() { return this.level; }
        internal System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>> StashEntries
        {
            get
            {
                return new System.Collections.Generic.KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[]
                {
                    new System.Collections.Generic.KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>("Name", this.GetSource)
                    ,new System.Collections.Generic.KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>("Level", this.GetLevel)
                };
            }
        }
    }

    /// <summary>
    /// Passes through all the important function calls to a target object.
    /// </summary>
    public class TraceListenerPassThru : System.Diagnostics.TraceListener
    {
        protected System.Diagnostics.TraceListener target;

        public TraceListenerPassThru(System.Diagnostics.TraceListener target)
        {
            this.target = target;
            this.Attributes.Clear();
            foreach (System.Collections.DictionaryEntry attribEntry in this.target.Attributes)
            {
                this.Attributes.Add(attribEntry.Key as string, attribEntry.Value as string);
            }
            this.Filter = target.Filter;
            this.IndentLevel = target.IndentLevel;
            this.IndentSize = target.IndentSize;
            this.Name = target.Name;
            this.TraceOutputOptions = target.TraceOutputOptions;
        }

        #region System.Diagnostics.TraceListener overrides
        public override void Write(string message)
        {
            this.target.Write(message);
        }

        public override void WriteLine(string message)
        {
            this.target.WriteLine(message);
        }

        public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, object data)
        {
            this.target.TraceData(eventCache, source, eventType, id, data);
        }
        public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, params object[] data)
        {
            this.target.TraceData(eventCache, source, eventType, id, data);
        }
        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id)
        {
            this.target.TraceEvent(eventCache, source, eventType, id);
        }
        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
        {
            this.target.TraceEvent(eventCache, source, eventType, id, format, args);
        }
        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string message)
        {
            this.target.TraceEvent(eventCache, source, eventType, id, message);
        }
        public override void TraceTransfer(System.Diagnostics.TraceEventCache eventCache, string source, int id, string message, System.Guid relatedActivityId)
        {
            this.target.TraceTransfer(eventCache, source, id, message, relatedActivityId);
        }

        // Other stuff...
        public override void Close()
        {
            this.target.Close();
            base.Close();
        }
        public override System.Runtime.Remoting.ObjRef CreateObjRef(System.Type requestedType)
        {
            return this.target.CreateObjRef(requestedType);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                System.IDisposable disposableTarget = this.target;
                disposableTarget.Dispose();
            }
        }
        public override bool Equals(object obj)
        {
            return this.target.Equals(obj);
        }
        public override void Fail(string message)
        {
            this.target.Fail(message);
        }
        public override void Fail(string message, string detailMessage)
        {
            this.target.Fail(message, detailMessage);
        }
        public override void Flush()
        {
            this.target.Flush();
        }
        public override int GetHashCode()
        {
            return this.target.GetHashCode();
        }
        protected override string[] GetSupportedAttributes()
        {
            return base.GetSupportedAttributes();
        }
        public override object InitializeLifetimeService()
        {
            return this.target.InitializeLifetimeService();
        }
        public override bool IsThreadSafe
        {
            get
            {
                return this.target.IsThreadSafe;
            }
        }
        public override string Name
        {
            get
            {
                return this.target.Name;
            }
            set
            {
                this.target.Name = value;
            }
        }
        public override string ToString()
        {
            return this.target.ToString();
        }
        public override void Write(object o)
        {
            this.target.Write(o);
        }
        public override void Write(object o, string category)
        {
            this.target.Write(o, category);
        }
        public override void Write(string message, string category)
        {
            this.target.Write(message, category);
        }
        protected override void WriteIndent()
        {
            this.IndentLevel = this.target.IndentLevel;
            base.WriteIndent();
        }
        public override void WriteLine(object o)
        {
            this.target.WriteLine(o);
        }
        public override void WriteLine(object o, string category)
        {
            this.target.WriteLine(o, category);
        }
        public override void WriteLine(string message, string category)
        {
            this.target.WriteLine(message, category);
        }
        #endregion // System.Diagnostics.TraceListener overrides
    } // Ends TraceListenerPassThru

    /// <summary>
    /// Passes through writing, but doesn't bother with header/footer.
    /// </summary>
    public class TraceListenerBare : TraceListenerPassThru
    {
        public TraceListenerBare(System.Diagnostics.TraceListener target)
            : base(target)
        {
        }

        #region Extension points for subclasses
        protected virtual string FormatMessage(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Formatter.StashDelegate>> stashEntries
            , string message, params object[] args)
        {
            return message;
        }

        protected virtual string FormatMessage(string message, params object[] args)
        {
            return message;
        }
        #endregion // Extension points for subclasses

        public void WriteFormattedLine(TraceData traceData, string message, params object[] args)
        {
            message = this.FormatMessage(traceData.StashEntries, message, args);
            this.WriteLine(message);
        }

        public void WriteFormatted(TraceData traceData, string message, params object[] args)
        {
            message = this.FormatMessage(traceData.StashEntries, message, args);
            this.Write(message);
        }

        public void WriteFormattedLine(string message, params object[] args)
        {
            message = this.FormatMessage(message, args);
            this.WriteLine(message);
        }

        public void WriteFormatted(string message, params object[] args)
        {
            message = this.FormatMessage(message, args);
            this.Write(message);
        }

        #region System.Diagnostics.TraceListener overrides
        /// <remarks>Similar to other trace listeners but with no header/footer.</remarks>
        // ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1)

        public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, object data)
        {
            if (this.target.Filter != null && !this.target.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                return;
            }
            string message = string.Empty;
            if (data != null)
            {
                message = data.ToString();
            }

            this.WriteFormattedLine(new TraceData(source, eventType.ToString()), message);
        }

        public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, params object[] data)
        {
            if (this.target.Filter != null && !this.target.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                return;
            }
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i != 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    if (data[i] != null)
                    {
                        stringBuilder.Append(data[i].ToString());
                    }
                }
            }
            this.WriteFormattedLine(new TraceData(source, eventType.ToString()), stringBuilder.ToString());
        }

        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.target.Filter != null && !this.target.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                return;
            }
            if (args != null)
            {
                this.WriteFormattedLine(new TraceData(source, eventType.ToString()), format, args);
            }
            else
            {
                this.WriteFormattedLine(new TraceData(source, eventType.ToString()), format);
            }
        }

        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string message)
        {
            if (this.target.Filter != null && !this.target.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                return;
            }
            this.WriteFormattedLine(new TraceData(source, eventType.ToString()), message);
        }
        #endregion // System.Diagnostics.TraceListener overrides
    } // Ends class TraceListenerBare

    public class TraceListenerFormat : TraceListenerBare
    {
        public TraceListenerFormat(System.Diagnostics.TraceListener target, Formatter formatter)
            : base(target)
        {
            this.Formatter = formatter;
        }
        public Formatter Formatter { get; set; }

        public static TraceListenerFormat Create(System.Diagnostics.TraceListener target, Formatter formatter)
        {
            return new TraceListenerFormat(target, formatter);
        }
        public static System.Collections.Generic.IEnumerable<TraceListenerFormat> GenerateTraceListenerFormat(Formatter formatter,
            System.Collections.Generic.IEnumerable<System.Diagnostics.TraceListener> listeners)
        {
            foreach (System.Diagnostics.TraceListener listener in listeners)
            {
                yield return Create(listener, formatter);
            }
        }
        public static System.Collections.Generic.IEnumerable<System.Diagnostics.TraceListener> GenerateTraceListener(Formatter formatter,
            System.Collections.Generic.IEnumerable<System.Diagnostics.TraceListener> listeners)
        {
            foreach (System.Diagnostics.TraceListener listener in GenerateTraceListenerFormat(formatter, listeners))
            {
                yield return listener;
            }
        }

        protected override string FormatMessage(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Formatter.StashDelegate>> stashEntries, string message, params object[] args)
        {
            message = base.FormatMessage(stashEntries, message, args);

            Formatter formatter = this.Formatter;

            if (formatter != null)
            {
                message = formatter.Format(stashEntries, message, args);
            }
            return message;
        }

        protected override string FormatMessage(string message, params object[] args)
        {
            message = base.FormatMessage(message, args);

            Formatter formatter = this.Formatter;

            if (formatter != null)
            {
                message = formatter.Format(message, args);
            }
            return message;
        }
    } // Ends class TraceListenerFormat
} // Ends namespace Udbus.Core.Logging
