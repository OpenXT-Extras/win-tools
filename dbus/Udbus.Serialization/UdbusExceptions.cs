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
using System.Runtime.InteropServices;

namespace Udbus.Serialization.Exceptions
{
    public class ExceptionFormatter
    {
        public static string FormatExceptionMessage(string message, string exception)
        {
            return message + ". " + exception;
        }
    }

    public class TransportFailureException : Exception
    {
        public TransportFailureException() : base()
        {
        }

        public TransportFailureException(string message)
            : base(message)
        { }

        public TransportFailureException(string message, Exception inner)
            : base(message, inner)
        { }

        public TransportFailureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        protected static string CreateMessage(IUdbusTransport transport)
        {
            return string.Format("Error with transport: {0}", transport.ToString());
        }

        #region Factory functions
        /// Factory functions
        public static TransportFailureException Create(IUdbusTransport transport)
        {
            return new TransportFailureException(CreateMessage(transport));
        }

        public static TransportFailureException Create(IUdbusTransport transport, string message)
        {
            return new TransportFailureException(CreateMessage(transport) + ". " + message);
        }
        #endregion // Factory functions
    } // Ends TransportFailureException : Exception

    public class UdbusAuthorisationException : Exception
    {
        readonly int errorcode = 0;
        public UdbusAuthorisationException()
            : base()
        {}

        public UdbusAuthorisationException(string message)
            : base(message)
        { }
        public UdbusAuthorisationException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusAuthorisationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusAuthorisationException(int errorcode)
            : this()
        {
            this.errorcode = errorcode;
        }

        public UdbusAuthorisationException(int errorcode, string message)
            : this(message)
        {
            this.errorcode = errorcode;
        }

        public UdbusAuthorisationException(int errorcode, string message, Exception inner)
            : this(message, inner)
        {
            this.errorcode = errorcode;
        }

        public UdbusAuthorisationException(int errorcode, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : this(info, context)
        {
            this.errorcode = errorcode;
        }

        protected static string CreateMessage(int errorcode)
        {
            System.ComponentModel.Win32Exception temp = new System.ComponentModel.Win32Exception(errorcode);
            return temp.Message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusAuthorisationException CreateWithErrorCode()
        {
            int errorcode = Marshal.GetLastWin32Error();
            return new UdbusAuthorisationException(errorcode, CreateMessage(errorcode));
        }

        public static UdbusAuthorisationException Create(int errorcode)
        {
            return new UdbusAuthorisationException(errorcode, CreateMessage(errorcode));
        }

        public static UdbusAuthorisationException Create(int errorcode, string message)
        {
            return new UdbusAuthorisationException(errorcode, message + ". " + CreateMessage(errorcode));
        }
        #endregion // Factory functions
    } // Ends UdbusAuthorisationException : Exception

    #region Message exceptions
    public class UdbusMessageException : Exception
    {
        protected readonly Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct;

        public Udbus.Serialization.NMessageStruct.UdbusMessageHandle MessageStruct { get { return this.msgStruct; } }

        public UdbusMessageException()
            : base()
        { }
        public UdbusMessageException(string message)
            : base(message)
        { }
        public UdbusMessageException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageException(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : this()
        {
            this.msgStruct = msgStruct;
        }

        public UdbusMessageException(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, string message)
            : this(message)
        {
            this.msgStruct = msgStruct;
        }

        public UdbusMessageException(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message, Exception inner)
            : this(message, inner)
        {
            this.msgStruct = msgStruct;
        }

        public UdbusMessageException(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : this(info, context)
        {
            this.msgStruct = msgStruct;
        }

        protected static string CreateMessage(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus message error: {0}", msgStruct.ToString());
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageException Create(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageException(msgStruct, CreateMessage(msgStruct));
        }

        public static UdbusMessageException Create(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, string message)
        {
            return new UdbusMessageException(msgStruct,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageException

    public class UdbusMessageServiceException : UdbusMessageException
    {
        protected readonly Udbus.Serialization.DbusConnectionParameters connectionParams;

        public Udbus.Serialization.DbusConnectionParameters ConnectionParameters { get { return this.connectionParams; } }

        public UdbusMessageServiceException()
            : base()
        { }
        public UdbusMessageServiceException(string message)
            : base(message)
        { }
        public UdbusMessageServiceException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageServiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageServiceException(Udbus.Serialization.DbusConnectionParameters connectionParams
            , Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(msgStruct)
        {
            this.connectionParams = connectionParams;
        }

        public UdbusMessageServiceException(Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, string message)
            : base(msgStruct, message)
        {
            this.connectionParams = connectionParams;
        }

        public UdbusMessageServiceException(Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, string message, Exception inner)
            : base(msgStruct, message, inner)
        {
            this.connectionParams = connectionParams;
        }

        public UdbusMessageServiceException(Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(msgStruct, info, context)
        {
            this.connectionParams = connectionParams;
        }

        public static string CreateMessage(Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus message error: {0}. {1}", msgStruct.ToString(), connectionParams.ToString());
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageServiceException Create(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            return new UdbusMessageServiceException(connectionParams, msgStruct, CreateMessage(connectionParams, msgStruct));
        }

        public static UdbusMessageServiceException Create(Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
        {
            return new UdbusMessageServiceException(connectionParams, msgStruct,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(connectionParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageServiceException

    public class UdbusMessageMethodException : UdbusMessageServiceException
    {
        protected readonly string method;

        public string Method { get { return this.method; } }

        public UdbusMessageMethodException()
            : base()
        { }
        public UdbusMessageMethodException(string message)
            : base(message)
        { }
        public UdbusMessageMethodException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageMethodException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageMethodException(string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams, Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(connectionParams, msgStruct)
        {
            this.method = method;
        }

        public UdbusMessageMethodException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
            : base(connectionParams, msgStruct, message)
        {
            this.method = method;
        }

        public UdbusMessageMethodException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,string message, Exception inner)
            : base(connectionParams, msgStruct, message, inner)
        {
            this.method = method;
        }

        public UdbusMessageMethodException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(connectionParams, msgStruct, info, context)
        {
            this.method = method;
        }

        protected static string CreateMessage(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus method: {0} {1} - '{2}'", msgStruct.ToString(), connectionParams.ToString(), method);
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageMethodException Create(string method, 
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageMethodException(method, connectionParams, msgStruct, CreateMessage(method, connectionParams, msgStruct));
        }

        public static UdbusMessageMethodException Create(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
        {
            return new UdbusMessageMethodException(method, connectionParams, msgStruct,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(method, connectionParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageMethodException

    public class UdbusMessageMethodErrorException : UdbusMessageMethodException
    {
        public UdbusMessageMethodErrorException()
            : base()
        { }
        public UdbusMessageMethodErrorException(string message)
            : base(message)
        { }
        public UdbusMessageMethodErrorException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageMethodErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageMethodErrorException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(method, connectionParams, msgStruct)
        {
        }

        public UdbusMessageMethodErrorException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
            : base(method, connectionParams, msgStruct, message)
        {
        }

        public UdbusMessageMethodErrorException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message, Exception inner)
            : base(method, connectionParams, msgStruct, message, inner)
        {
        }

        public UdbusMessageMethodErrorException(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(method, connectionParams, msgStruct, info, context)
        {
        }

        protected new static string CreateMessage(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus error: {0}. {1}. {2} - '{3}'", msgStruct.error_name, msgStruct.ToString(), connectionParams.ToString(), method);
            return message;
        }

        #region Factory functions
        /// Factory functions
        public new static UdbusMessageMethodErrorException Create(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageMethodErrorException(method, connectionParams, msgStruct, CreateMessage(method, connectionParams, msgStruct));
        }

        public new static UdbusMessageMethodErrorException Create(string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
        {
            return new UdbusMessageMethodErrorException(method, connectionParams, msgStruct,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(method, connectionParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageMethodErrorException
    #endregion // Message exceptions

    #region Marshalling exceptions
    public class UdbusMarshalException : Exception
    {
        protected readonly int result;
        protected readonly Udbus.Serialization.DbusConnectionParameters connectionParams;

        public int Result { get { return this.result; } }
        public Udbus.Serialization.DbusConnectionParameters ConnectionParams { get { return this.connectionParams; } }

        public UdbusMarshalException()
            : base()
        { }
        public UdbusMarshalException(string message)
            : base(message)
        { }
        public UdbusMarshalException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMarshalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMarshalException(int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
            : this()
        {
            this.result = result;
            this.connectionParams = connectionParams;
        }

        public UdbusMarshalException(int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
            : this(message)
        {
            this.result = result;
            this.connectionParams = connectionParams;
        }

        public UdbusMarshalException(int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message, Exception inner)
            : this(message, inner)
        {
            this.result = result;
            this.connectionParams = connectionParams;
        }

        public UdbusMarshalException(int result, Udbus.Serialization.DbusConnectionParameters connectionParams, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : this(info, context)
        {
            this.result = result;
            this.connectionParams = connectionParams;
        }

        protected static string CreateMessage(int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            string message = string.Format("dbus marshalling error: {0}. {1}", result, connectionParams.ToString());
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMarshalException Create(int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            return new UdbusMarshalException(result, connectionParams, CreateMessage(result, connectionParams));
        }

        public static UdbusMarshalException Create(int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
        {
            return new UdbusMarshalException(result, connectionParams, ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(result, connectionParams)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMarshalException

    public class UdbusMethodSendException : UdbusMarshalException
    {
        protected readonly string method;
        public string Method { get { return this.method; } }

        public UdbusMethodSendException()
            : base()
        { }
        public UdbusMethodSendException(string message)
            : base(message)
        { }
        public UdbusMethodSendException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMethodSendException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMethodSendException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
            : base(result, connectionParams)
        {
            this.method = method;
        }

        public UdbusMethodSendException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
            : base(result, connectionParams, message)
        {
            this.method = method;
        }

        public UdbusMethodSendException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message, Exception inner)
            : base(result, connectionParams, message, inner)
        {
            this.method = method;
        }

        public UdbusMethodSendException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(result, connectionParams, info, context)
        {
            this.method = method;
        }

        protected static string CreateMessage(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            string message = string.Format("dbus method '{0}' send error: {1}. {2}", method, result, connectionParams.ToString());
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMethodSendException Create(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            return new UdbusMethodSendException(method, result, connectionParams, CreateMessage(method, result, connectionParams));
        }

        public static UdbusMethodSendException Create(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
        {
            return new UdbusMethodSendException(method, result, connectionParams, ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(method, result, connectionParams)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMethodSendException

    public class UdbusMessageBuilderException : UdbusMethodSendException
    {
        protected readonly string action;
        protected readonly uint serial;
        public string Action { get { return this.action; } }
        public uint Serial { get { return this.serial; } }

        public UdbusMessageBuilderException()
            : base()
        { }
        public UdbusMessageBuilderException(string message)
            : base(message)
        { }
        public UdbusMessageBuilderException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageBuilderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageBuilderException(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
            : base(method, result, connectionParams)
        {
            this.action = action;
            this.serial = serial;
        }

        public UdbusMessageBuilderException(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
            : base(method, result, connectionParams, message)
        {
            this.action = action;
            this.serial = serial;
        }

        public UdbusMessageBuilderException(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message, Exception inner)
            : base(method, result, connectionParams, message, inner)
        {
            this.action = action;
            this.serial = serial;
        }

        public UdbusMessageBuilderException(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(method, result, connectionParams, info, context)
        {
            this.action = action;
            this.serial = serial;
        }

        protected static string CreateMessage(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            string message = string.Format("dbus method '{0}' error building message: {1}. Result={2}. Serial={3}. {4}", method, action, result, serial, connectionParams.ToString());
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageBuilderException Create(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            return new UdbusMessageBuilderException(action, serial, method, result, connectionParams, CreateMessage(action, serial, method, result, connectionParams));
        }

        public static UdbusMessageBuilderException Create(string action, uint serial, string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
        {
            return new UdbusMessageBuilderException(action, serial, method, result, connectionParams, ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(action, serial, method, result, connectionParams)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageBuilderException

    public class UdbusMethodReceiveException : UdbusMarshalException
    {
        protected readonly string method;
        public string Method { get { return this.method; } }

        public UdbusMethodReceiveException()
            : base()
        { }
        public UdbusMethodReceiveException(string message)
            : base(message)
        { }
        public UdbusMethodReceiveException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMethodReceiveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMethodReceiveException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
            : base(result, connectionParams)
        {
            this.method = method;
        }

        public UdbusMethodReceiveException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
            : base(result, connectionParams, message)
        {
            this.method = method;
        }

        public UdbusMethodReceiveException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message, Exception inner)
            : base(result, connectionParams, message, inner)
        {
            this.method = method;
        }

        public UdbusMethodReceiveException(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(result, connectionParams, info, context)
        {
            this.method = method;
        }

        protected static string CreateMessage(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            string message = string.Format("dbus method '{0}' receive error: {1}. {2}", method, result, connectionParams.ToString());
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMethodReceiveException Create(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            return new UdbusMethodReceiveException(method, result, connectionParams, CreateMessage(method, result, connectionParams));
        }

        public static UdbusMethodReceiveException Create(string method, int result, Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
        {
            return new UdbusMethodReceiveException(method, result, connectionParams, ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(method, result, connectionParams)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMethodReceiveException

    //public class UdbusMarshalArgumentException : UdbusMarshalException
    //{
    //    public enum FieldDirection { In, Out };
    //    readonly FieldDirection direction;
    //    readonly uint index;
    //    readonly string argument;

    //    public FieldDirection Direction { get { return this.direction; } }
    //    public uint Index { get { return this.index; } }
    //    public string Argument { get { return this.argument; } }

    //    public UdbusMarshalArgumentException()
    //        : base()
    //    { }
    //    public UdbusMarshalArgumentException(string message)
    //        : base(message)
    //    { }
    //    public UdbusMarshalArgumentException(string message, Exception inner)
    //        : base(message, inner)
    //    { }
    //    public UdbusMarshalArgumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    //        : base(info, context)
    //    { }

    //    public UdbusMarshalArgumentException(FieldDirection direction, uint index, string argument, int result)
    //        : base(result)
    //    {
    //        this.direction = direction;
    //        this.index = index;
    //        this.argument = argument;
    //    }

    //    public UdbusMarshalArgumentException(FieldDirection direction, uint index, string argument, int result, string message)
    //        : base(result, message)
    //    {
    //        this.direction = direction;
    //        this.index = index;
    //        this.argument = argument;
    //    }

    //    public UdbusMarshalArgumentException(FieldDirection direction, uint index, string argument, int result, string message, Exception inner)
    //        : base(result, message, inner)
    //    {
    //        this.direction = direction;
    //        this.index = index;
    //        this.argument = argument;
    //    }

    //    public UdbusMarshalArgumentException(FieldDirection direction, uint index, string argument, int result, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    //        : base(result, info, context)
    //    {
    //        this.direction = direction;
    //        this.index = index;
    //        this.argument = argument;
    //    }

    //    protected static string CreateMessage(FieldDirection direction, uint index, string argument, int result)
    //    {
    //        string message = string.Format("Error marshalling dbus {0} argument {1} {2}: {3}", direction, index, argument, result);
    //        return message;
    //    }

    //    #region Factory functions
    //    /// Factory functions
    //    public static UdbusMarshalArgumentException Create(FieldDirection direction, uint index, string argument, int result)
    //    {
    //        return new UdbusMarshalArgumentException(direction, index, argument, result, CreateMessage(direction, index, argument, result));
    //    }

    //    public static UdbusMarshalArgumentException Create(FieldDirection direction, uint index, string argument, int result, string message)
    //    {
    //        return new UdbusMarshalArgumentException(direction, index, argument, result,
    //            ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(direction, index, argument, result)));
    //    }
    //    #endregion // Factory functions
    //} // Ends class UdbusMarshalArgumentException

    public class UdbusMessageMethodArgumentException : UdbusMessageMethodException
    {
        public enum FieldDirection { In, Out };
        public sealed class UnknownParameters { }

        protected readonly FieldDirection direction;
        protected readonly uint index;
        protected readonly string argument;
        protected readonly Type argumentType;
        protected readonly int result;

        public FieldDirection Direction { get { return this.direction; } }
        public uint Index { get { return this.index; } }
        public string Argument { get { return this.argument; } }
        public Type ArgumentType { get { return this.argumentType; } }
        public int Result { get { return this.result; } }

        public UdbusMessageMethodArgumentException()
            : base()
        { }
        public UdbusMessageMethodArgumentException(string message)
            : base(message)
        { }
        public UdbusMessageMethodArgumentException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageMethodArgumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageMethodArgumentException(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method, Udbus.Serialization.DbusConnectionParameters connectionParams, Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(method, connectionParams, msgStruct)
        {
            this.direction = direction;
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        public UdbusMessageMethodArgumentException(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
            : base(method, connectionParams, msgStruct, message)
        {
            this.direction = direction;
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        public UdbusMessageMethodArgumentException(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message, Exception inner)
            : base(method, connectionParams, msgStruct, message, inner)
        {
            this.direction = direction;
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        public UdbusMessageMethodArgumentException(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(method, connectionParams, msgStruct, info, context)
        {
            this.direction = direction;
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        protected static string CreateMessage(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method, Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus {0} argument[{1}]: '{2} {3} {4}'. result={5}. {6}. {7}"
                , method, index, direction, argumentType.Name, argument, result
                , msgStruct.ToString(), connectionParams.ToString()
            );
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageMethodArgumentException Create(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method, 
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageMethodArgumentException(direction, index, argument, argumentType, result, method, connectionParams, msgStruct,
                CreateMessage(direction, index, argument, argumentType, result, method, connectionParams, msgStruct));
        }

        public static UdbusMessageMethodArgumentException Create(FieldDirection direction, uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
        {
            return new UdbusMessageMethodArgumentException(direction, index, argument, argumentType, result, method, connectionParams, msgStruct,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(direction, index, argument, argumentType, result, method, connectionParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageMethodArgumentException

    public class UdbusMessageMethodArgumentInException : UdbusMessageMethodArgumentException
    {
        static readonly Udbus.Serialization.NMessageStruct.UdbusMessageHandle DefaultMsgStruct = default(Udbus.Serialization.NMessageStruct.UdbusMessageHandle);

        public UdbusMessageMethodArgumentInException()
            : base()
        { }
        public UdbusMessageMethodArgumentInException(string message)
            : base(message)
        { }
        public UdbusMessageMethodArgumentInException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageMethodArgumentInException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageMethodArgumentInException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams)
            : base(FieldDirection.In, index, argument, argumentType, result, method, connectionParams, DefaultMsgStruct)
        {
        }

        public UdbusMessageMethodArgumentInException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
            : base(FieldDirection.In, index, argument, argumentType, result, method, connectionParams, DefaultMsgStruct, message)
        {
        }

        public UdbusMessageMethodArgumentInException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams, string message, Exception inner)
            : base(FieldDirection.In, index, argument, argumentType, result, method, connectionParams, DefaultMsgStruct, message, inner)
        {
        }

        public UdbusMessageMethodArgumentInException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(FieldDirection.In, index, argument, argumentType, result, method, connectionParams, DefaultMsgStruct, info, context)
        {
        }

        protected static string CreateMessage(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            string message = string.Format("dbus {0} argument[{1}]: '{2} {3} {4}'. result={5}. {6}"
                , method, index, FieldDirection.In, argumentType.Name, argument, result
                , connectionParams.ToString()
            );
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageMethodArgumentInException Create(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams)
        {
            return new UdbusMessageMethodArgumentInException(index, argument, argumentType, result, method, connectionParams,
                CreateMessage(index, argument, argumentType, result, method, connectionParams));
        }

        public static UdbusMessageMethodArgumentInException Create(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams, string message)
        {
            return new UdbusMessageMethodArgumentInException(index, argument, argumentType, result, method, connectionParams,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(index, argument, argumentType, result, method, connectionParams)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageMethodArgumentInException

    public class UdbusMessageMethodArgumentOutException : UdbusMessageMethodArgumentException
    {
        public UdbusMessageMethodArgumentOutException()
            : base()
        { }
        public UdbusMessageMethodArgumentOutException(string message)
            : base(message)
        { }
        public UdbusMessageMethodArgumentOutException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageMethodArgumentOutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageMethodArgumentOutException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(FieldDirection.Out, index, argument, argumentType, result, method, connectionParams, msgStruct)
        {
        }

        public UdbusMessageMethodArgumentOutException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
            : base(FieldDirection.Out, index, argument, argumentType, result, method, connectionParams, msgStruct, message)
        {
        }

        public UdbusMessageMethodArgumentOutException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message, Exception inner)
            : base(FieldDirection.Out, index, argument, argumentType, result, method, connectionParams, msgStruct, message, inner)
        {
        }

        public UdbusMessageMethodArgumentOutException(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(FieldDirection.Out, index, argument, argumentType, result, method, connectionParams, msgStruct, info, context)
        {
        }

        protected static string CreateMessage(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return UdbusMessageMethodArgumentException.CreateMessage(FieldDirection.Out, index, argument, argumentType, result, method,
                connectionParams, msgStruct);
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageMethodArgumentOutException Create(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageMethodArgumentOutException(index, argument, argumentType, result, method, connectionParams, msgStruct,
                CreateMessage(index, argument, argumentType, result, method, connectionParams, msgStruct));
        }

        public static UdbusMessageMethodArgumentOutException Create(uint index, string argument, Type argumentType, int result,
            string method,
            Udbus.Serialization.DbusConnectionParameters connectionParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
        {
            return new UdbusMessageMethodArgumentOutException(index, argument, argumentType, result, method, connectionParams, msgStruct,
                ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(index, argument, argumentType, result, method, connectionParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageMethodArgumentOutException

    #endregion // Marshalling exceptions
} // Ends namespace Udbus.Serialization.Exceptions
