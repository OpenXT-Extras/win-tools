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

namespace Udbus.Containers.Exceptions
{
    public class InvalidVariantCastException : InvalidCastException
    {
        private Udbus.Types.dbus_type underlyingType;
        private string underlyingValue;
        private Udbus.Types.dbus_type requestedType;

        public Udbus.Types.dbus_type UnderlyingType { get { return this.underlyingType; } }
        public Udbus.Types.dbus_type RequestedType { get { return this.requestedType; } }
        public string UnderlyingValue { get { return this.underlyingValue; } }

        #region Constructors
        public InvalidVariantCastException()
            : base()
        {
        }

        public InvalidVariantCastException(string message)
            : base(message)
        { }

        public InvalidVariantCastException(string message, Exception inner)
            : base(message, inner)
        { }

        public InvalidVariantCastException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public InvalidVariantCastException(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType)
            : base(CreateMessage(underlyingType, underlyingValue, requestedType))
        {
            this.underlyingType = underlyingType;
            this.underlyingValue = underlyingValue;
            this.requestedType = requestedType;
        }

        public InvalidVariantCastException(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType, string message)
            : base(message)
        {
            this.underlyingType = underlyingType;
            this.underlyingValue = underlyingValue;
            this.requestedType = requestedType;
        }

        public InvalidVariantCastException(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType, string message,
            Exception inner)
            : base(message, inner)
        {
            this.underlyingType = underlyingType;
            this.underlyingValue = underlyingValue;
            this.requestedType = requestedType;
        }

        public InvalidVariantCastException(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            this.underlyingType = underlyingType;
            this.underlyingValue = underlyingValue;
            this.requestedType = requestedType;
        }
        #endregion //Constructors

        #region Message functions
        public static string CreateMessage(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType)
        {
            return string.Format("Trying to retrieve {0} (value={1}) as {2}", underlyingType, underlyingValue, requestedType);
        }
        #endregion // Message functions

        #region Factory Functions
        public static InvalidVariantCastException Create(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType,
            string message)
        {
            return new InvalidVariantCastException(underlyingType, underlyingValue, requestedType, message);
        }

        public static InvalidVariantCastException Create(Udbus.Types.dbus_type underlyingType, string underlyingValue, Udbus.Types.dbus_type requestedType)
        {
            return Create(underlyingType, underlyingValue, requestedType,
                    CreateMessage(underlyingType, underlyingValue, requestedType));
        }
        #endregion // Factory Functions
    } // Ends class InvalidVariantCastException


    public class IncorrectTypeException : FormatException
    {
        private Udbus.Types.dbus_type expectedType;
        private object providedValue;
        private Udbus.Types.dbus_type[] providedTypes;

        #region Constructors
        public IncorrectTypeException()
            : base()
        {
        }

        public IncorrectTypeException(string message)
            : base(message)
        { }

        public IncorrectTypeException(string message, Exception inner)
            : base(message, inner)
        { }

        public IncorrectTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public IncorrectTypeException(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes)
            : base(CreateMessage(expectedType, providedValue, providedTypes))
        {
            this.expectedType = expectedType;
            this.providedValue = providedValue;
            this.providedTypes = providedTypes;
        }

        public IncorrectTypeException(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes, string message)
            : base(message)
        {
            this.expectedType = expectedType;
            this.providedValue = providedValue;
            this.providedTypes = providedTypes;
        }

        public IncorrectTypeException(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes, string message,
            Exception inner)
            : base(message, inner)
        {
            this.expectedType = expectedType;
            this.providedValue = providedValue;
            this.providedTypes = providedTypes;
        }

        public IncorrectTypeException(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes,
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            this.expectedType = expectedType;
            this.providedValue = providedValue;
            this.providedTypes = providedTypes;
        }
        #endregion //Constructors

        #region Message functions
        public static string CreateMessage(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes)
        {
            return string.Format("Type specified as '{0}' (value={1}) but expected '{2}'", expectedType, providedValue, Udbus.Types.dbus_sig.ToString(providedTypes));
        }
        #endregion // Message functions

        #region Factory Functions
        public static IncorrectTypeException Create(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes,
            string message)
        {
            return new IncorrectTypeException(expectedType, providedValue, providedTypes, message);
        }

        public static IncorrectTypeException Create(Udbus.Types.dbus_type expectedType, object providedValue, Udbus.Types.dbus_type[] providedTypes)
        {
            return Create(expectedType, providedValue, providedTypes,
                    CreateMessage(expectedType, providedValue, providedTypes));
        }
        #endregion // Factory Functions
    } // Ends class IncorrectTypeException
} // Ends namespace Udbus.Containers.Exceptions
