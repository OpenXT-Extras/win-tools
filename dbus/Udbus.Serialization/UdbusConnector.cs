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


namespace Udbus.Serialization
{
    //using NMessage = NMessageStruct;
    //using NMessageFunctions = UdbusMsgStructFunctions;

    using NMessage = NMessageHandle;
    using NMessageFunctions = UdbusMsgHandleFunctions;

    public class UdbusConnector
    {
        private static readonly Udbus.Serialization.ReadonlyDbusConnectionParameters defaultConnectionParametersDbus = new Udbus.Serialization.ReadonlyDbusConnectionParameters("org.freedesktop.DBus"
            , "/org/freedesktop/DBus", "org.freedesktop.DBus");
        protected Udbus.Serialization.ManagedDbusIo dbio;
        protected IUdbusTransport transport;
        protected const int DefaultId = 0;
        protected const UInt32 INFINITE = 0xFFFFFFFF;
        protected const UInt32 XHT_DEFAULT_TIMEOUT = INFINITE;//20000;

        public UdbusConnector(IUdbusTransport transport)
        {
            //if (transport.PopulateDbio(out this.dbio) == false) // If failed to populate dbio
            if (transport.PopulateDbio(ref this.dbio) == false) // If failed to populate dbio
            {
                throw Exceptions.TransportFailureException.Create(transport);

            } // Ends if failed to populate dbio

            this.transport = transport;
        }

        /// Implementation.

        /// <summary>
        /// Convert an integer id to its authorisation equivalent.
        /// Converts id to its ascii representation, and then
        /// builds a string consisting of each ascii value
        /// converted to a hexadecimal string.
        /// </summary>
        /// <param name="id">Id to convert.</param>
        /// <returns>Id converted to ascii, and then each ascii character converted to hex string.</returns>
        private static string CreateAuthorisationId(int id)
        {
            // Convert to string.
            string toString = string.Format("{0:d}", id);
            // Get string ASCII bytes.
            byte[] bytesFromString = System.Text.Encoding.ASCII.GetBytes(toString);

            // Build string containing ASCII bytes converted to hex.
            StringBuilder buildAuthorisation = new StringBuilder("EXTERNAL ");

            foreach (byte iterByte in bytesFromString)
            {
                buildAuthorisation.AppendFormat("{0:x}", iterByte);
            }

            return buildAuthorisation.ToString();
        }

        /// Udbus functions.
        public int Authorise()
        {
            // At the moment, the id is hardcoded to 0.
            string id = CreateAuthorisationId(DefaultId);
            int result = Udbus.Serialization.UdbusFunctions.dbus_auth(ref this.dbio, id);
            return result;
        }

        public delegate int HelloRecvDelegate(out string name, UdbusConnector connector, uint serial, Udbus.Serialization.DbusConnectionParameters connectionParametersDbus);
        public int HelloSend(uint serial, out string name, HelloRecvDelegate helloRecv)
        {
            Udbus.Serialization.UdbusMessageBuilder builder = new Udbus.Serialization.UdbusMessageBuilder();
            Udbus.Serialization.NMessageHandle.UdbusMessageHandle msgHandleSend = null;
            int result = -1;
            name = default(string);
            Udbus.Serialization.DbusConnectionParameters connectionParametersDbus = defaultConnectionParametersDbus;
            try
            {
                result = builder.UdbusMethodMessage(serial, connectionParametersDbus, "Hello").Result;
                if ((result == 0))
                {
                    msgHandleSend = builder.Message;
                    result = this.Send(msgHandleSend);
                    if ((result != 0))
                    {
                        throw Udbus.Serialization.Exceptions.UdbusMethodSendException.Create("Hello", result, connectionParametersDbus);
                    }
                }
            }
            finally
            {
                if ((msgHandleSend != null))
                {
                    msgHandleSend.Dispose();
                }
            }

            if ((result == 0))
            {
                result = helloRecv(out name, this, serial, connectionParametersDbus);
            }
            return result;
        }


        //public int Hello(out string name, Udbus.Core.IUdbusSerialNumberManager serialManager, Udbus.Core.DbusMessageReceiverPool receiverPool)
        //{
        //    name = default(string);
        //    uint serial = serialManager.GetNext();
        //    Udbus.Serialization.UdbusMessageBuilder builder = new Udbus.Serialization.UdbusMessageBuilder();
        //    Udbus.Core.NMessageHandle.UdbusMessageHandle msgHandleSend = null;
        //    int result = -1;
        //    Udbus.Serialization.DbusConnectionParameters connectionParametersDbus = defaultConnectionParametersDbus;
        //    try
        //    {
        //        result = builder.UdbusMethodMessage(serial, connectionParametersDbus, "Hello").Result;
        //        if ((result == 0))
        //        {
        //            msgHandleSend = builder.Message;
        //            result = this.Send(msgHandleSend);
        //            if ((result != 0))
        //            {
        //                throw Udbus.Core.Exceptions.UdbusMethodSendException.Create("Hello", result, connectionParametersDbus);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if ((msgHandleSend != null))
        //        {
        //            msgHandleSend.Dispose();
        //        }
        //    }
        //    if ((result == 0))
        //    {
        //        Udbus.Core.UdbusMessagePair msgResp = default(Udbus.Core.UdbusMessagePair);
        //        try
        //        {
        //            msgResp = receiverPool.ReceiveMessageData(this, serial, out result);
        //            if (((result == 0)
        //                        && (msgResp.QuEmpty == false)))
        //            {
        //                if ((msgResp.Data.typefield.type == Udbus.Core.dbus_msg_type.DBUS_TYPE_ERROR))
        //                {
        //                    throw Udbus.Core.Exceptions.UdbusMessageMethodErrorException.Create("Hello", connectionParametersDbus, msgResp.Data);
        //                }
        //                else
        //                {
        //                    Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msgResp.Handle);
        //                    if ((result == 0))
        //                    {
        //                        string nameResult;
        //                        result = Udbus.Serialization.UdbusMessageReader.ReadString(reader, out nameResult);
        //                        if ((result == 0))
        //                        {
        //                            name = nameResult;
        //                        }
        //                        else
        //                        {
        //                            throw Udbus.Core.Exceptions.UdbusMessageMethodArgumentOutException.Create(1, "name", typeof(string), result, "Hello", connectionParametersDbus, msgResp.Data);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        throw Udbus.Core.Exceptions.UdbusMessageMethodArgumentOutException.Create(0, "UnknownParameters", typeof(Udbus.Core.Exceptions.UdbusMessageMethodArgumentException.UnknownParameters), result, "Hello", connectionParametersDbus, msgResp.Data);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                throw Udbus.Core.Exceptions.UdbusMethodReceiveException.Create("Hello", result, connectionParametersDbus);
        //            }
        //        }
        //        finally
        //        {
        //            if ((msgResp.QuEmpty == false))
        //            {
        //                msgResp.Dispose();
        //            }
        //        }
        //    }

        //    return result;
        //}

        /// Udbus send/receive
        public int Send(NMessage.UdbusMessageHandle msg)
        {
            int result = NMessageFunctions.dbus_msg_send(ref this.dbio, msg);
            return result;
        }

        /// <summary>
        /// Whichever namespace we're using as NMessage, receive one of those.
        /// </summary>
        /// <param name="recv">Received message or initialiser value on failure.</param>
        /// <returns>Zero if successful, otherwise non-zero.</returns>
        public int Receive(out NMessage.UdbusMessageHandle recv)
        {
            int result = -1;
            recv = NMessage.UdbusMessageHandle.Initialiser;

            if (this.transport.WaitForRead(XHT_DEFAULT_TIMEOUT) == WaitForReadResult.Succeeded)
            {
                result = NMessageFunctions.dbus_msg_recv(ref this.dbio, out recv);
            }

            return result;
        }

        public NMessage.UdbusMessageHandle Receive(out int result)
        {
            NMessage.UdbusMessageHandle recv;
            result = this.Receive(out recv);
            return recv;
        }

        /// <summary>
        /// Receive a message handle.
        /// </summary>
        /// <param name="result">Zero if successful, otherwise non-zero.</param>
        /// <returns>Message handle if successful, otherwise null.</returns>
        public NMessageHandle.UdbusMessageHandle ReceiveHandle(out int result)
        {
            result = -1;
            NMessage.UdbusMessageHandle recv = NMessageHandle.UdbusMessageHandle.Initialiser;

            if (this.transport.WaitForRead(XHT_DEFAULT_TIMEOUT) == WaitForReadResult.Succeeded) // If there's something to read
            {
                result = UdbusMsgHandleFunctions.dbus_msg_recv(ref this.dbio, out recv);

            } // Ends if there's something to read

            return recv;
        }

        /// <summary>
        /// Receive a message structure.
        /// </summary>
        /// <param name="recv">Structure being received.</param>
        /// <returns>Zero if successful, otherwise non-zero.</returns>
        public int ReceiveStruct(out NMessageStruct.UdbusMessageHandle recv)
        {
            int result;

            using (NMessageHandle.UdbusMessageHandle recvHandle = this.ReceiveHandle(out result))
            {
                if (result == 0 && !recvHandle.IsInvalid) // If successfully got handle
                {
                    recv = recvHandle.HandleToStructure();

                } // Ends if successfully got handle
                else // Else failed to get handle
                {
                    if (result == 0) // If appeared to get handle
                    {
                        result = -1;

                    } // Ends if appeared to get handle

                    recv = NMessageStruct.UdbusMessageHandle.Initialiser;

                } // Ends else failed to get handle
            } // Ends using handle

            return result;
        }

        public NMessageStruct.UdbusMessageHandle ReceiveStruct(out int result)
        {
            NMessageStruct.UdbusMessageHandle recv;
            result = this.ReceiveStruct(out recv);
            return recv;
        }

        /// Factory functions
        static public UdbusConnector CreateAuthorised(IUdbusTransport transport)
        {
            UdbusConnector connectorCreate = new UdbusConnector(transport);

            int authoriseResult = connectorCreate.Authorise();

            if (authoriseResult != 0) // If error occurred
            {
                throw Exceptions.UdbusAuthorisationException.CreateWithErrorCode();

            } // Ends if error occurred

            return connectorCreate;
        }

        static public UdbusConnector CreateHelloed(IUdbusTransport transport, uint serial, out string name, HelloRecvDelegate helloRecv)
        {
            UdbusConnector connectorCreate = CreateAuthorised(transport);

            // We won't bother to raise an exception since Hello() should do it for us.
            int helloResult = connectorCreate.HelloSend(serial, out name, helloRecv);

            return connectorCreate;
        }

        //static public UdbusConnector CreateHelloed(out string name
        //    , IUdbusTransport transport, Udbus.Core.IUdbusSerialNumberManager serialManager, Udbus.Core.DbusMessageReceiverPool receiverPool)
        //{
        //    UdbusConnector connectorCreate = CreateAuthorised(transport);

        //    // We won't bother to raise an exception since Hello() should do it for us.
        //    int helloResult = connectorCreate.Hello(out name, serialManager, receiverPool);

        //    return connectorCreate;
        //}
    } // class UdbusConnector
} // Ends namespace Udbus.Serialization
