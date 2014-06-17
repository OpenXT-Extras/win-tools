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
using System.ServiceModel;

namespace UdbusWCFService
{
    public class ServiceBase : IDisposable
    {
        protected const uint DefaultBodyLength = 4096; // HACK. If we bust this, we have an issue.

        #region Static Functions
        /// <summary>
        /// Log debug output.
        /// </summary>
        /// <param name="logpriv">Reserved.</param>
        /// <param name="buf">String to log.</param>
        protected static void Service_io_debug(IntPtr logpriv, String buf)
        {
            Console.WriteLine("UdbusWCFService: {0}", buf);
        }
        #endregion // Ends static Functions

        #region Member variables
        /// <summary>
        /// V4V connection for this service.
        /// </summary>
        protected Udbus.Core.v4v.v4vConnection connection;

        /// <summary>
        /// V4V connector used to send and receive MiniDbus messages.
        /// </summary>
        protected Udbus.Core.UdbusConnector connector;

        /// <summary>
        /// Pool of received messages.
        /// </summary>
        protected Udbus.Core.DbusMessageReceiverPool receiverPool = new Udbus.Core.DbusMessageReceiverPool();

        /// <summary>
        /// Result of most recent V4V call.
        /// </summary>
        protected int result;
        #endregion // Ends member variables

        #region Creation / Destruction
        public ServiceBase(Udbus.Core.v4v.v4vConnection connection, Udbus.Core.UdbusConnector connector)
        {
            this.connection = connection;
            this.connector = connector;
        }

        public ServiceBase(Udbus.Core.v4v.v4vConnection connection)
            :this(connection, Udbus.Core.UdbusConnector.CreateAuthorised(connection))
        {
        }

        public ServiceBase()
            : this(new Udbus.Core.v4v.v4vConnection(Service_io_debug))
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
            this.connection.Dispose();
        }
        #endregion

        #region Message Receiving

        /// <summary>
        /// Receive a dbus message.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <returns>Received message.</returns>
        public Udbus.Core.NMessageHandle.UdbusMessageHandle ReceiveMessage(uint id, out int result)
        {
            Udbus.Core.NMessageHandle.UdbusMessageHandle resultHandle = this.receiverPool.ReceiveMessage(this.connector, id, out result);
            this.receiverPool.TracePool();
            return resultHandle;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="messages">Collection of received messages.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>True if received any messages, otherwise false.</returns>
        public int ReceiveMessages(uint id, ICollection<Udbus.Core.NMessageHandle.UdbusMessageHandle> messages, uint count)
        {
            int result = this.receiverPool.ReceiveMessages(this.connector, id, messages, count);
            this.receiverPool.TracePool();
            return result;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>Collection of received messages.</returns>
        public ICollection<Udbus.Core.NMessageHandle.UdbusMessageHandle> ReceiveMessages(uint id, uint count, out int result)
        {
            ICollection <Udbus.Core.NMessageHandle.UdbusMessageHandle> messages = this.receiverPool.ReceiveMessages(this.connector, id, count, out result);
            this.receiverPool.TracePool();
            return messages;
        }

        #endregion // Ends Message Receiving

    } // Ends class Service
}
