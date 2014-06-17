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
    public struct UdbusMessagePair
        : IDisposable
    {
        /// <summary>
        /// Message handle. Must always be set.
        /// </summary>
        public readonly Udbus.Serialization.NMessageHandle.UdbusMessageHandle Handle;

        /// <summary>
        /// Message structure deserialised from handle.
        /// </summary>
        private Udbus.Serialization.NMessageStruct.UdbusMessageHandle? data;

        #region Constructors
        public UdbusMessagePair(Udbus.Serialization.NMessageHandle.UdbusMessageHandle handle)
            : this(handle, null)
        {
        }

        private UdbusMessagePair(Udbus.Serialization.NMessageHandle.UdbusMessageHandle handle, Udbus.Serialization.NMessageStruct.UdbusMessageHandle? data)
        {
            this.Handle = handle;
            this.data = data;
        }

        private UdbusMessagePair(UdbusMessagePair source)
            : this(source.Handle, source.data)
        {
        }
        #endregion // Constructors

        #region Properties
        public Udbus.Serialization.NMessageStruct.UdbusMessageHandle Data
        {
            get
            {
                Udbus.Serialization.NMessageStruct.UdbusMessageHandle value;

                if (!this.data.HasValue)
                {
                    if (this.Handle != null)
                    {
                        this.data = this.Handle.HandleToStructure();
                        value = this.data.Value;
                    }
                    else
                    {
                        value = default(Udbus.Serialization.NMessageStruct.UdbusMessageHandle);
                    }
                }
                else
                {
                    value = this.data.Value;
                }

                return value;
            }
        }

        public bool QuEmpty
        {
            get
            {
                return this.Handle == Udbus.Serialization.NMessageHandle.UdbusMessageHandle.EmptyHandle;
            }
        }
        #endregion // Properties

        public UdbusMessagePair Clone()
        {
            UdbusMessagePair clone = new UdbusMessagePair(this);
            return clone;
        }

        public UdbusMessagePair Release()
        {
            UdbusMessagePair result = new UdbusMessagePair(this.Handle.Release(), this.data);
            return result;
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (this.Handle != null)
            {
                this.Handle.Dispose();
            }
        }

        #endregion
    } // Ends UdbusMessagePair

    public interface IUdbusMessageVisitor
    {
        void Reset();

        void onMethod(UdbusMessagePair messageData);

        void onSignal(UdbusMessagePair messageData);

        void onMethodReturn(UdbusMessagePair messageData);

        void onError(UdbusMessagePair messageData);

        void onDefault(UdbusMessagePair messageData);

        void onResultError(int error);

        bool Stop { get; }

    } // Ends interface IUdbusMessageVisitor

    public interface IStoppable
    {
        bool Stop { set; }
    }

    public interface IStoppableUdbusMessageVisitor : IStoppable, IUdbusMessageVisitor
    {
    }

    #region Visitor Factories
    /// <summary>
    /// Interface for knocking out visitors on the fly.
    /// </summary>
    public interface IUdbusMessageVisitorFactory
    {
        IUdbusMessageVisitor CreateVisitor();

    } // Ends interface IUdbusMessageVisitorFactory

    /// <summary>
    /// Interface for knocking out visitors on the fly.
    /// </summary>
    /// <typeparam name="TVisitor">Type of visitor to create.</typeparam>
    public interface IUdbusMessageTVisitorFactory<TVisitor> : IUdbusMessageVisitorFactory
        where TVisitor : IUdbusMessageVisitor
    {
        TVisitor CreateTVisitor();

    } // Ends interface IUdbusMessageVisitorFactory

    /// <summary>
    /// Factory implementation returning new visitor instances which have a default constructor.
    /// </summary>
    /// <typeparam name="TVisitor">Type of visitor with default constructor to create.</typeparam>
    public class VisitorFactory<TVisitor> : IUdbusMessageTVisitorFactory<TVisitor>
        where TVisitor : IUdbusMessageVisitor, new()
    {
        #region IUdbusMessageVisitorFactory<TVisitor> Members

        public TVisitor CreateTVisitor()
        {
            return new TVisitor();
        }

        public IUdbusMessageVisitor CreateVisitor()
        {
            return this.CreateTVisitor();
        }

        #endregion // Ends IUdbusMessageVisitorFactory<TVisitor> Members

    } // Ends class VisitorFactory

    /// <summary>
    /// Factory implementation returning one visitor instance which has a default constructor.
    /// </summary>
    /// <typeparam name="TVisitor">Type of visitor with default constructor to create.</typeparam>
    public class VisitorSingletonFactory<TVisitor> : IUdbusMessageTVisitorFactory<TVisitor>
        where TVisitor : IUdbusMessageVisitor, new()
    {
        static private readonly TVisitor visitorSingleton = new TVisitor();

        #region IUdbusMessageVisitorFactory<TVisitor> Members

        public TVisitor CreateTVisitor()
        {
            return visitorSingleton;
        }

        public IUdbusMessageVisitor CreateVisitor()
        {
            return this.CreateTVisitor();
        }

        #endregion // Ends IUdbusMessageVisitorFactory<TVisitor> Members

    } // Ends class VisitorSingletonFactory

    #endregion // Visitor Factories

    public static class UdbusVisitorFunctions
    {
        private static void HandleMessageError(int result, IUdbusMessageVisitor visitor)
        {
            visitor.onResultError(result);
        }

        private static void HandleMessageError(int result, IUdbusMessageVisitor visitor, out UdbusMessagePair releaseMessageData)
        {
            releaseMessageData = default(UdbusMessagePair);
            HandleMessageError(result, visitor);
        }

        /// <summary>
        /// Call into a visitor, and pass back message data if visitor stops.
        /// </summary>
        /// <remarks>
        /// Note that it is up to calling code to dispose of the message handle,
        /// and that if the visitor Stop flag is set the message handle is moved into the out parameter
        /// evading disposal.
        /// </remarks>
        /// <param name="result">Receive result.</param>
        /// <param name="messageData">Received message data.</param>
        /// <param name="visitor">Visitor to handle message handle.</param>
        /// <param name="releaseMessageData">Message data if visitor sets Stop flag.</param>
        /// <returns>Receive result.</returns>
        public static int Visit(int result, UdbusMessagePair messageData, IUdbusMessageVisitor visitor, out UdbusMessagePair releaseMessageData)
        {
            releaseMessageData = default(UdbusMessagePair);

            if (result == 0) // If got message ok
            {
                switch ((Udbus.Types.dbus_msg_type)messageData.Data.typefield.type)
                {
                    case Udbus.Types.dbus_msg_type.DBUS_TYPE_METHOD_CALL:
                        visitor.onMethod(messageData);
                        break;

                    case Udbus.Types.dbus_msg_type.DBUS_TYPE_SIGNAL:
                        visitor.onSignal(messageData);
                        break;

                    case Udbus.Types.dbus_msg_type.DBUS_TYPE_METHOD_RETURN:
                        visitor.onMethodReturn(messageData);
                        break;

                    case Udbus.Types.dbus_msg_type.DBUS_TYPE_ERROR:
                        visitor.onError(messageData);
                        break;

                    default:
                        visitor.onDefault(messageData);
                        break;

                } // Ends switch received message type

                if (visitor.Stop) // If stopping
                {
                    // No disposing thanks very much.
                    //UdbusMessagePair releaseMessageData = messageData.Release();
                    releaseMessageData = messageData.Release();
                    //msg.Release();

                } // Ends if stopping
            } // Ends if got message ok
            else // Else failed to get message
            {
                HandleMessageError(result, visitor);

            } // Ends else failed to get message

            return result;
        }

        public static int Visit(int result, UdbusMessagePair messageData, IUdbusMessageVisitor visitor)
        {
            UdbusMessagePair releaseMessageData;
            int visitResult = Visit(result, messageData, visitor, out releaseMessageData);
            //releaseMessageData.Dispose();
            return visitResult;
        }

        internal static int Visit(int result, Udbus.Serialization.NMessageHandle.UdbusMessageHandle msg, IUdbusMessageVisitor visitor, out UdbusMessagePair releaseMessageData)
        {
            if (result == 0)
            {
                UdbusMessagePair messageData = new UdbusMessagePair(msg);
                result = Visit(result, messageData, visitor, out releaseMessageData);
            }
            else
            {
                HandleMessageError(result, visitor, out releaseMessageData);
            }

            return result;
        }

        public static int Visit(int result, Udbus.Serialization.NMessageHandle.UdbusMessageHandle msg, IUdbusMessageVisitor visitor)
        {
            UdbusMessagePair releaseMessageData;
            int visitResult = Visit(result, msg, visitor, out releaseMessageData);
            //releaseMessageData.Dispose();
            return visitResult;
        }

        public static void LoopUdbus(Udbus.Serialization.UdbusConnector connector, IUdbusMessageVisitor visitor, System.IO.TextWriter output, System.Threading.WaitHandle stop)
        {
            output.WriteLine("Looping...");
            int r = 0;

            while (r == 0 && stop.WaitOne(0) == false)
            {
                using (var msg = connector.ReceiveHandle(out r))
                {
                    Visit(r, msg, visitor);

                } // Ends using received message
            } // Ends loop while getting results and not told to stop

            visitor.Reset();

            output.WriteLine("Finished looping...");
        }

        public static UdbusMessagePair LoopUdbusFind(Udbus.Serialization.UdbusConnector connector, IUdbusMessageVisitor visitor, System.IO.TextWriter output, System.Threading.WaitHandle stop)
        {
            output.WriteLine("Looping...");
            int r = 0;
            UdbusMessagePair result = default(UdbusMessagePair);

            while (r == 0 && visitor.Stop == false && stop.WaitOne(0) == false)
            {
                using (var msg = connector.ReceiveHandle(out r))
                {
                    Visit(r, msg, visitor, out result);

                } // Ends using received message
            } // Ends loop while getting results and not told to stop

            visitor.Reset();

            output.WriteLine("Finished looping...");

            return result;
        }
    } // Ends class UdbusVisitorFunctions

    public class UdbusMessageVisitorMulti : IUdbusMessageVisitor
    {
        IEnumerable<IUdbusMessageVisitor> visitors;

        public UdbusMessageVisitorMulti(IEnumerable<IUdbusMessageVisitor> visitors)
        {
            this.visitors = visitors;
        }

        public UdbusMessageVisitorMulti(params IUdbusMessageVisitor[] visitors)
        {
            this.visitors = visitors;
        }

        #region IUdbusMessageVisitor Members

        public void Reset()
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.Reset();

            } // Ends loop over visitors
        }

        public void onMethod(UdbusMessagePair messageData)
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.onMethod(messageData);

            } // Ends loop over visitors
        }

        public void onSignal(UdbusMessagePair messageData)
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.onSignal(messageData);

            } // Ends loop over visitors
        }

        public void onMethodReturn(UdbusMessagePair messageData)
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.onMethodReturn(messageData);

            } // Ends loop over visitors
        }

        public void onError(UdbusMessagePair messageData)
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.onError(messageData);
            }
        }

        public void onDefault(UdbusMessagePair messageData)
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.onDefault(messageData);

            } // Ends loop over visitors
        }

        public void onResultError(int error)
        {
            foreach (IUdbusMessageVisitor visitor in this.visitors)
            {
                visitor.onResultError(error);

            } // Ends loop over visitors
        }

        public bool Stop
        {
            get
            {
                bool bStop = false;

                foreach (IUdbusMessageVisitor visitor in this.visitors)
                {
                    if (visitor.Stop) // If one visitor wants to stop
                    {
                        bStop = true;
                        break;

                    } // Ends if one visitor wants to stop
                } // Ends loop over visitors

                return bStop;
            }
        }

        #endregion // Ends IUdbusMessageVisitor Members
    } // Ends class UdbusMessageVisitorNoOp

    public class UdbusMessageVisitorNoOp : IUdbusMessageVisitor
    {
        #region IUdbusMessageVisitor Members

        public void Reset()
        {
        }

        public void onMethod(UdbusMessagePair messageData)
        {
        }

        public void onSignal(UdbusMessagePair messageData)
        {
        }

        public void onMethodReturn(UdbusMessagePair messageData)
        {
        }

        public void onError(UdbusMessagePair messageData)
        {
        }

        public void onDefault(UdbusMessagePair messageData)
        {
        }

        public void onResultError(int error)
        {
        }

        public bool Stop { get { return false; } }

        #endregion // Ends IUdbusMessageVisitor Members
    } // Ends class UdbusMessageVisitorNoOp

    abstract public class UdbusMessageVisitorHandlerBase : IUdbusMessageVisitor
    {
        abstract protected void HandleMessage(UdbusMessagePair messageData);

        #region IUdbusMessageVisitor Members
        public virtual void Reset()
        {
        }

        public virtual void onMethod(UdbusMessagePair messageData)
        {
            this.HandleMessage(messageData);
        }

        public virtual void onSignal(UdbusMessagePair messageData)
        {
            this.HandleMessage(messageData);
        }

        public virtual void onMethodReturn(UdbusMessagePair messageData)
        {
            this.HandleMessage(messageData);
        }

        public void onError(UdbusMessagePair messageData)
        {
            this.HandleMessage(messageData);
        }

        public virtual void onDefault(UdbusMessagePair messageData)
        {
            this.HandleMessage(messageData);
        }

        public void onResultError(int error)
        {
        }

        public virtual bool Stop { get { return false; } }
        #endregion // Ends IUdbusMessageVisitor Members
    } // Ends class UdbusMessageVisitorHandlerBase

    public class UdbusMessageVisitorFind : UdbusMessageVisitorHandlerBase
    {
        #region SerialHolders
        interface ISerialHolder
        {
            uint Serial { get; }

        } // Ends interface ISerialHolder

        class SerialHolder : ISerialHolder
        {
            private readonly uint serial;

            public SerialHolder(uint serial)
            {
                this.serial = serial;
            }

            #region ISerialHolder Members

            public uint Serial
            {
                get { return this.serial; }
            }

            #endregion // ISerialHolder Members
        } // Ends class SerialHolder

        class SerialHolderMessageBuilderTracker : ISerialHolder
        {
            private readonly Udbus.Serialization.UdbusMessageBuilderTracker builder;

            public SerialHolderMessageBuilderTracker(Udbus.Serialization.UdbusMessageBuilderTracker builder)
            {
                this.builder = builder;
            }

            #region ISerialHolder Members

            public uint Serial
            {
                get { return this.builder.MostRecentSerialNumber; }
            }

            #endregion // ISerialHolder Members

        } // Ends SerialHolderMessageBuilderTracker
        #endregion // SerialHolders

        #region Fields
        //uint serial;
        ISerialHolder serialholder;
        bool bStop = false;
        #endregion // Fields

        #region Constructors
        public UdbusMessageVisitorFind(uint serial)
        {
            this.serialholder = new SerialHolder(serial);
        }

        public UdbusMessageVisitorFind(Udbus.Serialization.NMessageStruct.UdbusMessageHandle handle)
            : this(handle.serial)
        {
        }

        public UdbusMessageVisitorFind(Udbus.Serialization.UdbusMessageBuilderTracker builder)
        {
            this.serialholder = new SerialHolderMessageBuilderTracker(builder);
        }
        #endregion // Constructors

        #region IUdbusMessageVisitor Members
        public override bool Stop { get { return this.bStop; } }

        public override void Reset()
        {
            this.bStop = false;
        }
        #endregion // IUdbusMessageVisitor Members

        #region UdbusMessageVisitorFind overrides
        protected override void HandleMessage(UdbusMessagePair messageData)
        {
            if (messageData.Data.reply_serial == this.serialholder.Serial)
            {
                this.bStop = true;
            }
        }
        #endregion // UdbusMessageVisitorFind overrides

    } // Ends class UdbusMessageVisitorFind

    abstract public class UdbusMessageVisitorDumpBase : IUdbusMessageVisitor
    {
        public delegate void DDumpMessage(UdbusMessagePair messageData, string description);

        protected DDumpMessage dDump;
        protected DDumpMessage dDumpError;

        virtual protected void dumpStuff(UdbusMessagePair messageData, string description)
        {
            this.dDump(messageData, description);
        }

        virtual protected void dumpError(UdbusMessagePair messageData, string description)
        {
            this.dDumpError(messageData, "Error: " + description);
        }

        protected UdbusMessageVisitorDumpBase(DDumpMessage dDump, DDumpMessage dDumpError)
        {
            this.dDump = dDump;
            this.dDumpError = dDumpError;
        }

        #region IUdbusMessageVisitor Members
        public virtual void Reset()
        {
        }

        public virtual void onMethod(UdbusMessagePair messageData)
        {
            this.dumpStuff(messageData, "Method");
        }

        public virtual void onSignal(UdbusMessagePair messageData)
        {
            this.dumpStuff(messageData, "Signal");
        }

        public virtual void onMethodReturn(UdbusMessagePair messageData)
        {
            this.dumpStuff(messageData, "Method Return");
        }

        public void onError(UdbusMessagePair messageData)
        {
            this.dumpStuff(messageData, "Error message");
        }

        public virtual void onDefault(UdbusMessagePair messageData)
        {
            this.dumpStuff(messageData, "Dunno");
        }

        public void onResultError(int error)
        {
            UdbusMessagePair errordata = new UdbusMessagePair();
            this.dDumpError(errordata, string.Format("Error result: {0}", error));
        }

        public bool Stop { get { return false; } }
        #endregion // Ends IUdbusMessageVisitor Members
    } // Ends class UdbusMessageVisitorDumpBase


    public class UdbusMessageVisitorDumpConsole : UdbusMessageVisitorDumpBase
    {
        public static DDumpMessage CreateMessageDumper(System.IO.TextWriter writer)
        {
            DDumpMessage result = delegate(UdbusMessagePair messageData, string description)
            {
                if (!messageData.QuEmpty)
                {
                    writer.WriteLine("{0}. {1}", description, messageData.Data.ToString());
                }
                else
                {
                    writer.WriteLine("No message. {0}", description);
                }
            };
            return result;
        }

        public UdbusMessageVisitorDumpConsole()
            : base(CreateMessageDumper(Console.Out), CreateMessageDumper(Console.Error))
        {
        }

    } // Ends class UdbusMessageVisitorDumpConsole

    public class UdbusMessageVisitorDumpLog : UdbusMessageVisitorDumpBase
    {
        public static DDumpMessage CreateMessageDumper(Udbus.Core.Logging.ILog log)
        {
            DDumpMessage result = delegate(UdbusMessagePair messageData, string description)
            {
                if (!messageData.QuEmpty)
                {
                    log.Info("{0}. {1}", description, messageData.Data.ToString());
                }
                else
                {
                    log.Info("No message. {0}", description);
                }
            };
            return result;
        }

        public UdbusMessageVisitorDumpLog(Udbus.Core.Logging.ILog log)
            : base(CreateMessageDumper(log), CreateMessageDumper(log))
        {
        }

        public UdbusMessageVisitorDumpLog()
            : this(Udbus.Core.Logging.LogCreation.CreateMessageDumpLogger())
        {
        }
        
    } // Ends class UdbusMessageVisitorDumpLog

    public class UdbusMessageVisitorDumpXen : UdbusMessageVisitorDumpLog
    {
        public UdbusMessageVisitorDumpXen()
            : base()
        { }

        public override void onSignal(UdbusMessagePair messageData)
        {
            const string StorageSpaceLow = "storage_space_low";
            if (messageData.Data.method.StartsWith(StorageSpaceLow))
            {
                Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(messageData.Handle);
                Int32 percentage = 0;
                int result = reader.ReadInt32(out percentage);
                if (result != 0)
                {
                    this.dumpError(messageData, "Reading percentage from storage space low signal");
                }
                else
                {
                    this.dumpStuff(messageData, string.Format("Signal. Left {0}%", percentage));
                }
            }
            else
            {
                base.onSignal(messageData);
            }
        }

    } // Ends class UdbusMessageVisitorDumpXen

    public class UdbusMessageVisitorStopper : UdbusMessageVisitorHandlerBase
    {
        private bool stop = false;

        public override bool Stop { get { return this.stop; } }

        protected override void HandleMessage(UdbusMessagePair messageData)
        {
            this.stop = true;
        }

        public override void Reset()
        {
            base.Reset();
            this.stop = false;
        }

    } // Ends class UdbusMessageVisitorStopper

    public class UdbusMessageVisitorDumpStop : UdbusMessageVisitorMulti
    {
        public UdbusMessageVisitorDumpStop()
            : base(new UdbusMessageVisitorDumpLog(), new UdbusMessageVisitorStopper())
        { }
    } // Ends class UdbusMessageVisitorDumpStop
}
