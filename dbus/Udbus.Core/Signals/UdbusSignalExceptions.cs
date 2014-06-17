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

namespace Udbus.Core.Exceptions
{
    public class UdbusMessageSignalException : Udbus.Serialization.Exceptions.UdbusMessageException
    {
        protected readonly string signal;
        protected readonly Udbus.Core.DbusSignalParams signalParams;

        public string Signal { get { return this.signal; } }
        public Udbus.Core.DbusSignalParams SignalParams { get { return this.signalParams; } }

        public UdbusMessageSignalException()
            : base()
        { }
        public UdbusMessageSignalException(string message)
            : base(message)
        { }
        public UdbusMessageSignalException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageSignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public UdbusMessageSignalException(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(msgStruct)
        {
            this.signal = signal;
            this.signalParams = signalParams;
        }

        public UdbusMessageSignalException(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
            : base(msgStruct)
        {
            this.signal = signal;
            this.signalParams = signalParams;
        }

        public UdbusMessageSignalException(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, string message, Exception inner)
            : base(msgStruct, message, inner)
        {
            this.signal = signal;
            this.signalParams = signalParams;
        }

        public UdbusMessageSignalException(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(msgStruct, info, context)
        {
            this.signal = signal;
            this.signalParams = signalParams;
        }

        protected static string CreateMessage(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus signal: {0} {1} - '{2}'", msgStruct.ToString(), signalParams.ToString(), signal);
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageSignalException Create(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageSignalException(signal, signalParams, msgStruct, CreateMessage(signal, signalParams, msgStruct));
        }

        public static UdbusMessageSignalException Create(string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct, string message)
        {
            return new UdbusMessageSignalException(signal, signalParams, msgStruct,
                Udbus.Serialization.Exceptions.ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(signal, signalParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageSignalException

    public class UdbusMessageSignalArgumentException : UdbusMessageSignalException
    {
        public sealed class UnknownParameters { }

        protected readonly uint index;
        protected readonly string argument;
        protected readonly Type argumentType;
        protected readonly int result;

        public uint Index { get { return this.index; } }
        public string Argument { get { return this.argument; } }
        public Type ArgumentType { get { return this.argumentType; } }
        public int Result { get { return this.result; } }

        public UdbusMessageSignalArgumentException()
            : base()
        { }
        public UdbusMessageSignalArgumentException(string message)
            : base(message)
        { }
        public UdbusMessageSignalArgumentException(string message, Exception inner)
            : base(message, inner)
        { }
        public UdbusMessageSignalArgumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        //UdbusMessageSignalArgumentException.Create(1, "percent_free", typeof(Int32), this.result, "storage_space_low", messageData.Data, this.signalKey)


        public UdbusMessageSignalArgumentException(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
            : base(signal, signalParams, msgStruct)
        {
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        public UdbusMessageSignalArgumentException(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
            : base(signal, signalParams, msgStruct, message)
        {
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        public UdbusMessageSignalArgumentException(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message, Exception inner)
            : base(signal, signalParams, msgStruct, message, inner)
        {
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        public UdbusMessageSignalArgumentException(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(signal, signalParams, msgStruct, info, context)
        {
            this.index = index;
            this.argument = argument;
            this.argumentType = argumentType;
            this.result = result;
        }

        protected static string CreateMessage(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            string message = string.Format("dbus {0} argument[{1}]: '{2} {3}'. result={4}. {5}. {6}"
                , signal, index, argumentType.Name, argument, result
                , msgStruct.ToString(), signalParams.ToString()
            );
            return message;
        }

        #region Factory functions
        /// Factory functions
        public static UdbusMessageSignalArgumentException Create(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct)
        {
            return new UdbusMessageSignalArgumentException(index, argument, argumentType, result, signal, signalParams, msgStruct,
                CreateMessage(index, argument, argumentType, result, signal, signalParams, msgStruct));
        }

        public static UdbusMessageSignalArgumentException Create(uint index, string argument, Type argumentType, int result,
            string signal, Udbus.Core.DbusSignalParams signalParams,
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle msgStruct,
            string message)
        {
            return new UdbusMessageSignalArgumentException(index, argument, argumentType, result, signal, signalParams, msgStruct,
                Udbus.Serialization.Exceptions.ExceptionFormatter.FormatExceptionMessage(message, CreateMessage(index, argument, argumentType, result, signal, signalParams, msgStruct)));
        }
        #endregion // Factory functions
    } // Ends class UdbusMessageSignalArgumentException
} // Ends namespace Udbus.Core.Exceptions
