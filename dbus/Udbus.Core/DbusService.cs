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

namespace Udbus.Core
{
    /// <summary>
    /// Base class for connection parameters for a dbus service.
    /// </summary>
    public class ServiceBaseConnectionParams : IDisposable
    {
        #region Static Functions
        /// <summary>
        /// Log debug output.
        /// </summary>
        /// <param name="logpriv">Reserved.</param>
        /// <param name="buf">String to log.</param>
        protected static void Service_io_debug(IntPtr logpriv, String buf)
        {
            Udbus.Core.Logging.ILog log = Udbus.Core.Logging.LogCreation.CreateDbusServiceLogger();
            log.Info(buf);
        }
        #endregion // Ends static Functions

        #region Member variables
        /// <summary>
        /// Connection for the service.
        /// </summary>
        public Udbus.Serialization.IUdbusTransport Connection;

        /// <summary>
        /// Connector used to send and receive MiniDbus messages.
        /// </summary>
        public Udbus.Serialization.UdbusConnector Connector;

        private Udbus.Serialization.IUdbusTransport signalConnection = null;
        private Udbus.Serialization.UdbusConnector signalConnector = null;

        #endregion // Ends member variables

        #region Properties
        public Udbus.Serialization.IUdbusTransport SignalConnection
        {
            get { return this.signalConnection ?? this.Connection; }
        }
        public Udbus.Serialization.UdbusConnector SignalConnector
        {
            get { return this.signalConnector ?? this.Connector; }
        }
        #endregion // Properties

        #region Initialization functions
        static private Udbus.Serialization.UdbusConnector InitializeConnectorFromConnection(Udbus.Serialization.IUdbusTransport connection)
        {
            return Udbus.Serialization.UdbusConnector.CreateAuthorised(connection);
        }

        static private Udbus.Serialization.UdbusConnector InitializeSignalConnector(Udbus.Serialization.IUdbusTransport connection, Udbus.Serialization.UdbusConnector connector,
            Udbus.Serialization.IUdbusTransport connectionSignal)
        {
            Udbus.Serialization.UdbusConnector connectorSignal;
            if (object.ReferenceEquals(connection, connectionSignal))
            {
                connectorSignal = connector;
            }
            else
            {
                connectorSignal = InitializeConnectorFromConnection(connectionSignal);
            }

            return connectorSignal;
        }
        static internal protected Udbus.Serialization.IUdbusTransport DefaultConnection()
        {
            return new Udbus.v4v.v4vConnection(Service_io_debug);
        }
        #endregion // Initialization functions

        #region Constructors
        public ServiceBaseConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.UdbusConnector connector,
            Udbus.Serialization.IUdbusTransport signalConnection,
            Udbus.Serialization.UdbusConnector signalConnector)
        {
            this.Connection = connection;
            this.Connector = connector;
            this.signalConnection = signalConnection;
            this.signalConnector = signalConnector;
        }

        public ServiceBaseConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.UdbusConnector connector)
            : this(connection, connector, null, null)
        {
            //this.Connection = connection;
            //this.Connector = connector;
        }

        public ServiceBaseConnectionParams(Udbus.Serialization.IUdbusTransport connection)
            : this(connection, InitializeConnectorFromConnection(connection))
        {
        }

        private ServiceBaseConnectionParams(Udbus.Serialization.IUdbusTransport connection
            , Udbus.Serialization.UdbusConnector connector
            , Udbus.Serialization.IUdbusTransport signalConnection
        )
            : this(connection, connector
            , signalConnection, InitializeSignalConnector(connection, connector, signalConnection))
        {
        }


        public ServiceBaseConnectionParams(Udbus.Serialization.IUdbusTransport connection
            , Udbus.Serialization.IUdbusTransport signalConnection
        )
            : this(connection, InitializeConnectorFromConnection(connection)
            , signalConnection)
        {
        }

        public ServiceBaseConnectionParams()
            : this(DefaultConnection())
        {
        }

        #endregion // Constructors

        #region IDisposable methods
        public void Dispose()
        {
            if (this.signalConnection != null) // If got a connection for signals
            {
                this.signalConnection.Dispose();

            } // Ends if got a signal connection

            this.Connection.Dispose();
        }
        #endregion // IDisposable methods
    } // Ends class ServiceBaseConnectionParams

    /// <summary>
    /// Connection parameters for a Dbus Service.
    /// </summary>
    public class ServiceConnectionParams : ServiceBaseConnectionParams
    {
        #region Member variables
        /// <summary>
        /// Manages serial numbers for dbus messages.
        /// </summary>
        public readonly Udbus.Core.IRegisterSignalHandlers RegisterSignalHandlers;
        public readonly Udbus.Core.IStoppableUdbusMessageVisitor SignalVisitor;
        public readonly Udbus.Core.DbusMessageReceiverPool ReceiverPool;
        public readonly Udbus.Core.IUdbusSerialNumberManager SerialManager;
        public readonly string Name;
        public readonly string SignalName;
        #endregion // Ends member variables

        private static Udbus.Core.IUdbusSerialNumberManager DefaultSerialManager()
        {
            return new Udbus.Core.UdbusSerialNumberManagerThreadsafe();
        }

        private static Udbus.Core.SignalVisitor DefaultRegisterSignalHandlers()
        {
            return new Udbus.Core.SignalVisitor();
        }

        private static Udbus.Core.DbusMessageReceiverPool DefaultReceiverPool(IHandleDbusSignal handleSignals)
        {
            return new Udbus.Core.DbusMessageReceiverPool(handleSignals);
        }

        #region Constructor Idiocy
        private struct ConstructorPoolArgs
        {
            internal Udbus.Core.DbusMessageReceiverPool receiverPool;
            internal Udbus.Core.SignalVisitor signalVisitor;
            internal ConstructorPoolArgs(Udbus.Core.DbusMessageReceiverPool receiverPool, Udbus.Core.SignalVisitor signalVisitor)
            {
                this.receiverPool = receiverPool;
                this.signalVisitor = signalVisitor;
            }
        }

        private static ConstructorPoolArgs DefaultPoolArgs()
        {
            Udbus.Core.SignalVisitor signalVisitor = DefaultRegisterSignalHandlers();
            Udbus.Core.DbusMessageReceiverPool pool = DefaultReceiverPool(signalVisitor);
            return new ConstructorPoolArgs(pool, signalVisitor);
        }

        private struct ConstructorConnectorArgs
        {
            internal Udbus.Serialization.UdbusConnector connector;
            internal string name;
            internal ConstructorConnectorArgs(Udbus.Serialization.UdbusConnector connector, string name)
            {
                this.connector = connector;
                this.name = name;
            }
        }
        #endregion // Constructor Idiocy

        #region Initialization functions
        //public delegate int HelloRecvDelegate(out string name, uint serial, Udbus.Serialization.DbusConnectionParameters connectionParametersDbus);

        public static Udbus.Serialization.UdbusConnector.HelloRecvDelegate MakeHelloRecvDelegate(DbusMessageReceiverPool receiverPool)
        {
            return (out string name, Udbus.Serialization.UdbusConnector connector, uint serial, Udbus.Serialization.DbusConnectionParameters connectionParametersDbus) =>
            {
                name = default(string);

                int result;
                Udbus.Core.UdbusMessagePair msgResp = default(Udbus.Core.UdbusMessagePair);
                try
                {
                    msgResp = receiverPool.ReceiveMessageData(connector, serial, out result);
                    if (((result == 0)
                                && (msgResp.QuEmpty == false)))
                    {
                        if ((msgResp.Data.typefield.type == Udbus.Types.dbus_msg_type.DBUS_TYPE_ERROR))
                        {
                            throw Udbus.Serialization.Exceptions.UdbusMessageMethodErrorException.Create("Hello", connectionParametersDbus, msgResp.Data);
                        }
                        else
                        {
                            Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msgResp.Handle);
                            if ((result == 0))
                            {
                                string nameResult;
                                result = Udbus.Serialization.UdbusMessageReader.ReadString(reader, out nameResult);
                                if ((result == 0))
                                {
                                    name = nameResult;
                                }
                                else
                                {
                                    throw Udbus.Serialization.Exceptions.UdbusMessageMethodArgumentOutException.Create(1, "name", typeof(string), result, "Hello", connectionParametersDbus, msgResp.Data);
                                }
                            }
                            else
                            {
                                throw Udbus.Serialization.Exceptions.UdbusMessageMethodArgumentOutException.Create(0, "UnknownParameters", typeof(Udbus.Serialization.Exceptions.UdbusMessageMethodArgumentException.UnknownParameters), result, "Hello", connectionParametersDbus, msgResp.Data);
                            }
                        }
                    }
                    else
                    {
                        throw Udbus.Serialization.Exceptions.UdbusMethodReceiveException.Create("Hello", result, connectionParametersDbus);
                    }
                }
                finally
                {
                    if ((msgResp.QuEmpty == false))
                    {
                        msgResp.Dispose();
                    }
                }

                return result;
            };
        }

        static private ConstructorConnectorArgs InitializeConnectorFromConnection(Udbus.Serialization.IUdbusTransport connection
            , Udbus.Core.IUdbusSerialNumberManager serialManager
            , Udbus.Core.DbusMessageReceiverPool receiverPool)
        {
            // We're going to throw away the name.
            string name;
            Udbus.Serialization.UdbusConnector createConnector = Udbus.Serialization.UdbusConnector.CreateHelloed(connection, serialManager.GetNext(), out name,
                MakeHelloRecvDelegate(receiverPool)
            );
            //Udbus.Serialization.UdbusConnector createConnector = Udbus.Serialization.UdbusConnector.CreateHelloed(out name, connection, serialManager, receiverPool);
            return new ConstructorConnectorArgs(createConnector, name);
        }

        static private ConstructorConnectorArgs InitializeSignalConnector(Udbus.Serialization.IUdbusTransport connection, ConstructorConnectorArgs args,
            Udbus.Serialization.IUdbusTransport connectionSignal
            , Udbus.Core.IUdbusSerialNumberManager serialManager
            , Udbus.Core.DbusMessageReceiverPool receiverPool)
        {
            ConstructorConnectorArgs signalArgs;
            if (object.ReferenceEquals(connection, connectionSignal))
            {
                signalArgs = args;
            }
            else
            {
                signalArgs = InitializeConnectorFromConnection(connectionSignal, serialManager, receiverPool);
            }

            return signalArgs;
        }
        #endregion // Initialization functions

        #region Constructors
        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.UdbusConnector connector,
            string name,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : base(connection, connector)
        {
            this.Name = name;
            this.SignalName = this.Name;
            this.RegisterSignalHandlers = registerSignalHandlers;
            this.SignalVisitor = signalVisitor;
            this.ReceiverPool = receiverPool;
            this.SerialManager = serialManager;
        }

        private ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.UdbusConnector connector,
            string name,
            ConstructorPoolArgs poolArgs,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, connector, name, poolArgs.signalVisitor, poolArgs.signalVisitor, poolArgs.receiverPool, serialManager)
        {
        }

        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.UdbusConnector connector,
            string name)
            : this(connection, connector, name, DefaultPoolArgs(), DefaultSerialManager())
        {
        }

        private ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            ConstructorConnectorArgs connectorArgs,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : base(connection, connectorArgs.connector)
        {
            this.Name = connectorArgs.name;
            this.SignalName = this.Name;
            this.RegisterSignalHandlers = registerSignalHandlers;
            this.SignalVisitor = signalVisitor;
            this.ReceiverPool = receiverPool;
            this.SerialManager = serialManager;
        }

        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, InitializeConnectorFromConnection(connection, serialManager, receiverPool), registerSignalHandlers, signalVisitor, receiverPool, serialManager)
        {
        }

        private ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            ConstructorPoolArgs poolArgs,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, poolArgs.signalVisitor, poolArgs.signalVisitor, poolArgs.receiverPool, serialManager)
        {
        }

        //public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
        //    Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
        //    Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor)
        //    : this(connection, registerSignalHandlers, signalVisitor, DefaultReceiverPool(), DefaultSerialManager())
        //{
        //}

        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection)
            : this(connection, DefaultPoolArgs(), DefaultSerialManager())
        {
        }

        public ServiceConnectionParams(Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(DefaultConnection(), registerSignalHandlers, signalVisitor, receiverPool, serialManager)
        {
        }

        private ServiceConnectionParams(ConstructorPoolArgs poolArgs,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(poolArgs.signalVisitor, poolArgs.signalVisitor, poolArgs.receiverPool, serialManager)
        {
        }

        public ServiceConnectionParams()
            : this(DefaultPoolArgs(), DefaultSerialManager())
        {
        }

        private ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            ConstructorConnectorArgs connectorArgs,
            Udbus.Serialization.IUdbusTransport signalConnection,
            ConstructorConnectorArgs signalConnectorArgs,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : base(connection, connectorArgs.connector, signalConnection, signalConnectorArgs.connector)
        {
            this.Name = connectorArgs.name;
            this.SignalName = signalConnectorArgs.name;
            this.RegisterSignalHandlers = registerSignalHandlers;
            this.SignalVisitor = signalVisitor;
            this.ReceiverPool = receiverPool;
            this.SerialManager = serialManager;
        }

        private ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            ConstructorConnectorArgs connectorArgs,
            Udbus.Serialization.IUdbusTransport signalConnection,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, connectorArgs, signalConnection, InitializeSignalConnector(connection, connectorArgs, signalConnection, serialManager, receiverPool)
                , registerSignalHandlers, signalVisitor, receiverPool, serialManager)
        {
        }

        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.UdbusConnector connector,
            string name,
            Udbus.Serialization.IUdbusTransport signalConnection,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, new ConstructorConnectorArgs(connector, name)
                , signalConnection, registerSignalHandlers, signalVisitor, receiverPool, serialManager)
        {
        }

        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.IUdbusTransport signalConnection,
            Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
            Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor,
            Udbus.Core.DbusMessageReceiverPool receiverPool,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, InitializeConnectorFromConnection(connection, serialManager, receiverPool), signalConnection
                , registerSignalHandlers, signalVisitor, receiverPool, serialManager)
        {
        }

        private ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.IUdbusTransport signalConnection,
            ConstructorPoolArgs poolArgs,
            Udbus.Core.IUdbusSerialNumberManager serialManager)
            : this(connection, signalConnection, poolArgs.signalVisitor, poolArgs.signalVisitor, poolArgs.receiverPool, serialManager)
        {
        }

        //public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
        //    Udbus.Serialization.IUdbusTransport signalConnection,
        //    Udbus.Core.IRegisterSignalHandlers registerSignalHandlers,
        //    Udbus.Core.IStoppableUdbusMessageVisitor signalVisitor)
        //    : this(connection, signalConnection, registerSignalHandlers, signalVisitor, DefaultReceiverPool(), DefaultSerialManager())
        //{
        //}

        public ServiceConnectionParams(Udbus.Serialization.IUdbusTransport connection,
            Udbus.Serialization.IUdbusTransport signalConnection)
            : this(connection, signalConnection, DefaultPoolArgs(), DefaultSerialManager())
        {
        }
        #endregion // Constructors

        #region Static Creation
        /// <summary>
        /// Create connection parameters servicing a signal connection, based off an existing serviceConnectionParams.
        /// </summary>
        /// <param name="serviceConnectionParams">Source service connection parameters.</param>
        /// <returns>Service connection parameters for communicating with signal.</returns>
        public static ServiceConnectionParams SignalOnlyConnectionParams(ServiceConnectionParams serviceConnectionParams)
        {
            ServiceConnectionParams signalConnectionParams = new ServiceConnectionParams(
                serviceConnectionParams.SignalConnection
                , serviceConnectionParams.SignalConnector
                , serviceConnectionParams.SignalName
                , serviceConnectionParams.RegisterSignalHandlers
                , serviceConnectionParams.SignalVisitor
                , serviceConnectionParams.ReceiverPool
                , serviceConnectionParams.SerialManager
            );
            return signalConnectionParams;
        }
        #endregion Static Creation
    } // Ends class ServiceConnectionParams

    /// <summary>
    /// Base class for Dbus services.
    /// </summary>
    public class ServiceBase : IDisposable
    {
        protected const uint DefaultBodyLength = 4096; // HACK. If we bust this, we have an issue.
        private uint bodyLength = DefaultBodyLength;
        #region Member variables

        /// <summary>
        /// Connection parameters for this service.
        /// </summary>
        protected ServiceBaseConnectionParams serviceConnectionParams;

        /// <summary>
        /// Pool of received messages.
        /// </summary>
        //protected Udbus.Core.DbusMessageReceiverPool receiverPool = new Udbus.Core.DbusMessageReceiverPool();

        /// <summary>
        /// Result of most recent dbus call.
        /// </summary>
        protected int result;
        #endregion // Ends member variables

        #region Properties
        protected Udbus.Serialization.IUdbusTransport Connection
        {
            get { return this.serviceConnectionParams.Connection; }
        }
        protected Udbus.Serialization.UdbusConnector Connector
        {
            get { return this.serviceConnectionParams.Connector; }
        }
        protected Udbus.Serialization.IUdbusTransport SignalConnection
        {
            get { return this.serviceConnectionParams.SignalConnection; }
        }
        protected Udbus.Serialization.UdbusConnector SignalConnector
        {
            get { return this.serviceConnectionParams.SignalConnector; }
        }
        public uint BodyLength
        {
            get { return this.bodyLength; }
            set { this.bodyLength = value; }
        }
        #endregion // Properties

        #region Creation / Destruction

        // WAXME
        public ServiceBase(Udbus.Serialization.IUdbusTransport connection, Udbus.Serialization.UdbusConnector connector)
            : this(new ServiceBaseConnectionParams(connection, connector))
        {
        }

        // WAXME
        public ServiceBase(Udbus.Serialization.IUdbusTransport connection)
            : this(new ServiceBaseConnectionParams(connection))
        {
        }

        public ServiceBase(ServiceBaseConnectionParams serviceConnectionParams)
        {
            this.serviceConnectionParams = serviceConnectionParams;
        }

        public ServiceBase()
            : this(new ServiceBaseConnectionParams())
        {
        }

        #endregion // Ends Creation / Destruction

        #region IDisposable Members
        /// <summary>
        /// Dispose of this service.
        /// </summary>
        public void Dispose()
        {
            // Clear up the V4V connection.
            this.serviceConnectionParams.Dispose();
        }
        #endregion

        #region Message Receiving
#if GOTRECEIVERPOOL
        /// <summary>
        /// Receive a dbus message.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <returns>Received message.</returns>
        public UdbusMessagePair ReceiveMessage(uint id, out int result)
        {
            UdbusMessagePair resultData = this.receiverPool.ReceiveMessageData(this.serviceConnectionParams.Connector, id, out result);
            this.receiverPool.TracePool();
            return resultData;
        }
        //public Udbus.Core.NMessageHandle.UdbusMessageHandle ReceiveMessage(uint id, out int result)
        //{
        //    Udbus.Core.NMessageHandle.UdbusMessageHandle resultHandle = this.receiverPool.ReceiveMessage(this.serviceConnectionParams.Connector, id, out result);
        //    this.receiverPool.TracePool();
        //    return resultHandle;
        //}

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="messages">Collection of received messages.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>True if received any messages, otherwise false.</returns>
        public int ReceiveMessages(uint id, ICollection<UdbusMessagePair> messages, uint count)
        {
            int result = this.receiverPool.ReceiveMessages(this.serviceConnectionParams.Connector, id, messages, count);
            this.receiverPool.TracePool();
            return result;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="messages">Collection of received messages.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>True if received any messages, otherwise false.</returns>
        public int ReceiveMessageHandles(uint id, ICollection<Udbus.Core.NMessageHandle.UdbusMessageHandle> messages, uint count)
        {
            int result = this.receiverPool.ReceiveMessageHandles(this.serviceConnectionParams.Connector, id, messages, count);
            this.receiverPool.TracePool();
            return result;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>Collection of received messages.</returns>
        public ICollection<UdbusMessagePair> ReceiveMessages(uint id, uint count, out int result)
        {
            ICollection<UdbusMessagePair> messages = this.receiverPool.ReceiveMessages(this.serviceConnectionParams.Connector, id, count, out result);
            this.receiverPool.TracePool();
            return messages;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>Collection of received messages.</returns>
        public ICollection<Udbus.Core.NMessageHandle.UdbusMessageHandle> ReceiveMessageHandles(uint id, uint count, out int result)
        {
            ICollection<Udbus.Core.NMessageHandle.UdbusMessageHandle> messages = this.receiverPool.ReceiveMessageHandles(this.serviceConnectionParams.Connector, id, count, out result);
            this.receiverPool.TracePool();
            return messages;
        }
#endif // GOTRECEIVERPOOL

        #endregion // Ends Message Receiving

    } // Ends class ServiceBase

    //public class DbusServiceBase : ServiceBase
    //{
    //    new private ServiceConnectionParams serviceConnectionParams;
    //    #region Creation / Destruction

    //    //public DbusServiceBase(Udbus.Serialization.IUdbusTransport connection, Udbus.Serialization.UdbusConnector connector)
    //    //    : base(connection, connector)
    //    //{
    //    //}

    //    //public DbusServiceBase(Udbus.Serialization.IUdbusTransport connection)
    //    //    : base(connection)
    //    //{
    //    //}

    //    public DbusServiceBase(ServiceConnectionParams serviceConnectionParams)
    //        : base(serviceConnectionParams)
    //    {
    //        this.serviceConnectionParams = serviceConnectionParams;
    //    }

    //    public DbusServiceBase()
    //        : base()
    //    {
    //    }

    //    #endregion // Ends Creation / Destruction
    //} // Ends class DbusServiceBase
} // Ends namespace Udbus.Core
