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

namespace Udbus.Core.Logging
{
    public interface ILog
    {
        void Exception(string message, params object[] args);
        void Error(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Info(string message, params object[] args);
        void Debug(string message, params object[] args);
    } // Ends interface ILog

    public interface ILogNode : IEquatable<ILogNode>
    {
        /// <summary>
        /// Check whether this log is the parent of the specified log.
        /// </summary>
        /// <param name="log">Potential child log.</param>
        /// <returns>True if this log is parent of child.</returns>
        bool IsParent(ILogNode potentialChild);

        IEnumerable<ILog> ParentLogs { get; }
        string Name { get; }
        ILog Log { get; }

        void Construct();
        void Replace(ILog logOld);
        void Bake();
    } // Ends interface ILogNode

    #region LogAdmin
    public interface ILogAdmin
    {
        // Thread safe.
        void AddLog(ILogNode log);
        void ReplaceLog(ILog oldLog, ILogNode newNode);
        void Bake();

        // No thread safety here, so no more AddLog(), ok ?.
        IEnumerable<ILogNode> Logs { get; }

        void DumpLogs(System.IO.TextWriter output);
    } // Ends interface ILogAdmin

    public class LogAdmin : ILogAdmin
    {
        LinkedList<ILogNode> logs = new LinkedList<ILogNode>();

        static LogAdmin instance = new LogAdmin();

        public static LogAdmin Instance { get { return instance; } }

        private void BakeImpl()
        {
            foreach (ILogNode logNode in logs)
            {
                logNode.Bake();
            }
        }

        #region ILogAdmin Members

        public IEnumerable<ILogNode> Logs
        {
            get
            {
                return this.logs;
            }
        }

        public void AddLog(ILogNode log)
        {
            lock (this.logs)
            {
                this.logs.AddLast(log);
                this.BakeImpl();
            }
        }

        public void ReplaceLog(ILog oldLog, ILogNode newNode)
        {
            lock (this.logs)
            {
                ILogNode oldNode = null;

                // Find the ILogNode for the old log.
                foreach (ILogNode logNode in this.logs)
                {
                    if (ReferenceEquals(logNode.Log, oldLog))
                    {
                        oldNode = logNode;
                        break;
                    }
                }

                if (oldNode != null)
                {
                    // Change all nodes matching the old ILogNode to use the new node.
                    for (LinkedListNode<ILogNode> node = this.logs.First; node != null; node = node.Next)
                    {
                        if (node.Value.Equals(oldNode))
                        {
                            node.Value = newNode;
                        }
                    }
                }
                this.BakeImpl();
            }
        }

        public void Bake()
        {
            lock (this.logs)
            {
                this.BakeImpl();
            }
        }

        public void DumpLogs(System.IO.TextWriter output)
        {
            output.WriteLine("Begin log dump...");
            foreach (ILogNode logNode in logs)
            {
                output.WriteLine(string.Format("{0}:", logNode.Name));
                foreach (ILogNode logNodeTest in logs)
                {
                    if (!ReferenceEquals(logNode, logNodeTest))
                    {
                        output.WriteLine(string.Format("  IsParent({0}): {1}", logNodeTest.Name, logNode.IsParent(logNodeTest)));
                    }
                }

#if DEBUG
                LogNodeBase nodeBase = logNode as LogNodeBase;
                if (nodeBase != null)
                {
                    int counter = 0;
                    foreach (ILogNode logNodeParent in nodeBase.Parents)
                    {
                        string name = logNodeParent.Name;
                        if (logNodeParent is LogNodeNameHierarchyRoot)
                        {
                            name += " {root}";
                        }
                        output.WriteLine(string.Format(" Parent[{0}]: {1}", counter, name));
                        ++counter;
                    }
                }
#endif // DEBUG
            }

            output.WriteLine("...End log dump");
            output.WriteLine();

        }
        #endregion // LogAdmin Members

    } // Ends class LogAdmin
    #endregion // LogAdmin

    #region LogNode implementations
    static internal class LogNodeFunctions
    {
        internal static ILog GetLog(ILogNode logNode)
        {
            return logNode.Log;
        }

    } // Ends class LogNodeFunctions

    [System.Diagnostics.DebuggerDisplay("{Name} {bakedParentLogs == null ? null : (int?)System.Linq.Enumerable.ToArray(bakedParentLogs).Length}")]
    internal abstract class LogNodeBase : ILogNode
    {
        private ILog log;
        private IEnumerable<ILog> bakedParentLogs = null;
        private static ILogAdmin logAdmin = LogAdmin.Instance;

        internal protected LogNodeBase(ILog log)
        {
            this.log = log;
        }

        internal protected LogNodeBase()
            : this(null)
        {
        }

        #region ILogNode Members

        abstract public bool IsParent(ILogNode potentialChild);

        public ILog Log
        {
            get { return this.log; }
        }

        public virtual string Name { get { return string.Empty; } }

        public void Construct()
        {
            logAdmin.AddLog(this);
        }

        //public void Replace(ILogNode newNode)
        //{
        //    logAdmin.ReplaceLog(this, newNode);
        //}
        public void Replace(ILog logOld)
        {
            logAdmin.ReplaceLog(logOld, this);
        }

        public void Bake()
        {
            this.bakedParentLogs = GetParentLogs(this).ToArray();
        }

        #endregion // ILogNode Members

        private bool NodeIsParent(ILogNode logNode) { return logNode.IsParent(this); }

        private static IEnumerable<ILogNode> GetParents(LogNodeBase logNode)
        {
            return logAdmin.Logs.Where(logNode.NodeIsParent);
        }

        private static IEnumerable<ILog> GetParentLogs(LogNodeBase logNode)
        {
            return GetParents(logNode).Select<ILogNode, ILog>(LogNodeFunctions.GetLog);
        }

#if DEBUG
        // For debugging only
        internal IEnumerable<ILogNode> Parents
        {
            get { return GetParents(this); }
        }
#endif // DEBUG

        public IEnumerable<ILog> ParentLogs
        {
            get
            {
                //if (this.bakedParentLogs == null)
                //{
                //    this.bakedParentLogs = GetParentLogs(this);
                //}
                return this.bakedParentLogs;
            }
        }

        static public void BakeAll()
        {
            logAdmin.Bake();
        }

        internal static void DumpLogs(System.IO.TextWriter output)
        {
            logAdmin.DumpLogs(output);
        }
    
        #region IEquatable<ILogNode> Members

        protected internal static bool Equals(ILogNode self, ILogNode other)
        {
            return self.Name.Equals(other.Name);
        }

        public virtual bool Equals(ILogNode other)
        {
            return Equals(this, other);
        }

        #endregion // IEquatable<ILogNode> Members
    } // Ends class LogNodeBase

    internal class LogNodeLeaf : LogNodeBase
    {
        internal LogNodeLeaf(ILog log)
            : base(log)
        {
        }

        #region ILogNode Members
        public override bool IsParent(ILogNode potentialChild)
        {
            return false;
        }
        #endregion // ILogNode Members
    } // Ends class LogNodeLeaf

    /// <summary>
    /// LogNodes organised by name ("." to indicate lineage).
    /// </summary>
    internal class LogNodeNameHierarchy : LogNodeBase
    {
        protected readonly string name;

        internal LogNodeNameHierarchy(string name, ILog log)
            : base(log)
        {
            this.name = name;
        }

        static protected internal bool IsChildName(string name, string childName)
        {
            return childName.StartsWith(name)
                && childName.Length > name.Length
                && childName[name.Length] == '.'
                ;
        }

        protected virtual bool CheckForChild(string childName)
        {
            return IsChildName(this.name, childName);
        }

        #region ILogNode Members
        public override string Name { get { return this.name; } }

        public override bool IsParent(ILogNode potentialChild)
        {
            bool isParent = false;

            if (!ReferenceEquals(this, potentialChild)) // If not same object
            {
                string childName = potentialChild.Name;

                if (!string.IsNullOrEmpty(childName)) // If child has a name
                {
                    isParent = this.CheckForChild(childName);

                } // Ends if child has a name
            } // Ends if not same object

            return isParent;
        }
        #endregion // ILogNode Members

        public override bool Equals(ILogNode other)
        {
            // ARSE. Replacing a root should only be possible with another root.
            // As usual, inheritance was the wrong way to go.
            // Replacing a normal with a normal, fine.
            // Replacing a root with a normal, no - reject.
            // Replacing a normal with a root, hmmmm, for now let's say nope.
            bool equal = !(other is LogNodeNameHierarchyRoot);

            if (equal)
            {
                equal = base.Equals(other);
            }

            return equal;
        }

    } // Ends class LogNodeNameHierarchy

    internal class LogNodeNameHierarchyRoot : LogNodeNameHierarchy
    {
        internal LogNodeNameHierarchyRoot(string name, ILog log)
            : base(name, log)
        {
        }

        #region LogNodeNameHierarchy Members

        protected override bool CheckForChild(string childName)
        {
            // this = root.toot
            // childName = root
            // Child has no hierarchy, so the root is its parent.
            bool isChild = !childName.Contains('.');

            if (isChild && !string.IsNullOrEmpty(this.name)) // If rootless
            {
                // Check whether this root childName is in fact OUR root...
                isChild = !IsChildName(childName, this.name);

            } // Ends if rootless

            return isChild;
        }

        #region IEquatable<ILogNode> Members

        public override bool Equals(ILogNode other)
        {
            // ARSE. Replacing a root should only be possible with another root.
            // As usual, inheritance was the wrong way to go.
            // Replacing a normal with a normal, fine.
            // Replacing a root with a normal, no - reject.
            // Replacing a normal with a root, hmmmm, for now let's say nope.
            // i.e. normals and root's don't interact.
            // Sadly, this is going to be busted working the other way around without implementing Equals in both classes. C'est la vie.
            bool equal = other is LogNodeNameHierarchyRoot;

            if (equal)
            {
                equal = Equals(this, other);
            }

            return equal;
        }

        #endregion // IEquatable<ILogNode> Members
        #endregion // LogNodeNameHierarchy Members
    } // Ends class LogNodeNameHierarchyRoot
    #endregion // LogNode implementations

    #region Log implementations

    [System.Diagnostics.DebuggerDisplay("{logNode == null ? null : logNode.Name}")]
    internal abstract class LogBase : ILog
    {
        private static ILogAdmin logAdmin = LogAdmin.Instance;

        private ILogNode logNode = null;
        private IEnumerable<ILog> Parents
        {
            get
            {
                return this.logNode.ParentLogs;
            }
        }

        protected static void Construct(LogBase log, ILogNode logNode)
        {
            log.logNode = logNode;
            logNode.Construct();
        }

        protected static void Replace(ILog logOld, LogBase log, ILogNode logNode)
        {
            log.logNode = logNode;
            logNode.Replace(logOld);
        }

        protected static void DefaultConstruct(LogBase log)
        {
            Construct(log, new LogNodeLeaf(log));
        }

        // It's up to the sub-classes to call Construct with thes constructors...
        protected LogBase(ILogNode logNode)
        {
            this.logNode = logNode;
        }

        protected LogBase()
            : this(null)
        {
        }

        #region ILog Members

        public virtual void Exception(string message, params object[] args)
        {
            foreach (ILog log in Parents)
            {
                log.Exception(message, args);
            }
        }

        public virtual void Error(string message, params object[] args)
        {
            foreach (ILog log in Parents)
            {
                log.Error(message, args);
            }
        }

        public virtual void Warning(string message, params object[] args)
        {
            foreach (ILog log in Parents)
            {
                log.Warning(message, args);
            }
        }

        public virtual void Info(string message, params object[] args)
        {
            foreach (ILog log in Parents)
            {
                log.Info(message, args);
            }
        }

        public virtual void Debug(string message, params object[] args)
        {
            foreach (ILog log in Parents)
            {
                log.Debug(message, args);
            }
        }

        #endregion // ILog Members
    } // Ends class LogBase

    internal class LogTraceSource : LogBase
    {
        //private System.Diagnostics.TraceSource ts;
        private IEnumerable<System.Diagnostics.TraceSource> tracesources;
        protected LogTraceSource(ILogNode logNode, IEnumerable<System.Diagnostics.TraceSource> tracesources)
            : base(logNode)
        {
            this.tracesources = tracesources;
        }
        protected LogTraceSource(ILogNode logNode, params System.Diagnostics.TraceSource[] tracesources)
            : base(logNode)
        {
            this.tracesources = tracesources;
        }

        protected LogTraceSource(IEnumerable<System.Diagnostics.TraceSource> tracesources)
            : this(null, tracesources)
        {
        }

        protected LogTraceSource(params System.Diagnostics.TraceSource[] tracesources)
            : this(null, tracesources)
        {
        }

        static public LogTraceSource Create(string name, IEnumerable<System.Diagnostics.TraceSource> tracesources)
        {
            LogTraceSource create = new LogTraceSource(tracesources);
            ILogNode logNode = new LogNodeNameHierarchy(name, create);
            Construct(create, logNode);
            return create;
        }

        static public LogTraceSource CreateRoot(string name, IEnumerable<System.Diagnostics.TraceSource> tracesources)
        {
            LogTraceSource create = new LogTraceSource(tracesources);
            ILogNode logNodeRoot = new LogNodeNameHierarchyRoot(name, create);
            Construct(create, logNodeRoot);
            return create;
        }

        static public LogTraceSource Replace(ILog logOld, string name, IEnumerable<System.Diagnostics.TraceSource> tracesources)
        {
            LogTraceSource create = new LogTraceSource(tracesources);
            ILogNode logNode = new LogNodeNameHierarchy(name, create);
            Replace(logOld, create, logNode);
            return create;
        }

        static public LogTraceSource ReplaceRoot(Udbus.Core.Logging.ILog logOld, string name, IEnumerable<System.Diagnostics.TraceSource> tracesources)
        {
            LogTraceSource create = new LogTraceSource(tracesources);
            ILogNode logNodeRoot = new LogNodeNameHierarchyRoot(name, create);
            Replace(logOld, create, logNodeRoot);
            return create;
        }

        static public LogTraceSource Create(string name, params System.Diagnostics.TraceSource[] tracesources)
        {
            return Create(name, tracesources as IEnumerable<System.Diagnostics.TraceSource>);
        }

        static public LogTraceSource CreateRoot(string name, params System.Diagnostics.TraceSource[] tracesources)
        {
            return CreateRoot(name, tracesources as IEnumerable<System.Diagnostics.TraceSource>);
        }

        static public LogTraceSource Replace(ILog logOld, string name, params System.Diagnostics.TraceSource[] tracesources)
        {
            return Replace(logOld, name, tracesources as IEnumerable<System.Diagnostics.TraceSource>);
        }

        static public LogTraceSource ReplaceRoot(Udbus.Core.Logging.ILog logOld, string name, params System.Diagnostics.TraceSource[] tracesources)
        {
            return ReplaceRoot(logOld, name, tracesources as IEnumerable<System.Diagnostics.TraceSource>);
        }

        #region ILog Members
        public override void Exception(string message, params object[] args)
        {
            base.Exception(message, args);
            foreach (System.Diagnostics.TraceSource ts in this.tracesources)
            {
                ts.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, message, args);
            }
        }

        public override void Error(string message, params object[] args)
        {
            base.Error(message, args);
            foreach (System.Diagnostics.TraceSource ts in this.tracesources)
            {
                ts.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, message, args);
            }
        }

        public override void Warning(string message, params object[] args)
        {
            base.Warning(message, args);
            foreach (System.Diagnostics.TraceSource ts in this.tracesources)
            {
                ts.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, message, args);
            }
        }

        public override void Info(string message, params object[] args)
        {
            base.Info(message, args);
            foreach (System.Diagnostics.TraceSource ts in this.tracesources)
            {
                ts.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, message, args);
            }
        }

        public override void Debug(string message, params object[] args)
        {
            base.Debug(message, args);
            foreach (System.Diagnostics.TraceSource ts in this.tracesources)
            {
                ts.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, message, args);
            }
        }
        #endregion // ILog Members
    } // Ends class LogTraceSource
    #endregion // Log implementations
}
