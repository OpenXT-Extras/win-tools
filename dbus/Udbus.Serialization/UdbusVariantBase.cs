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

using System.CodeDom;

namespace Udbus.Serialization
{
    public abstract class UdbusVariantBase : Udbus.Parsing.ParamCodeTypeHolderBase
    {
        #region Constructors
        public UdbusVariantBase(FieldDirection fieldDirection)
            : base(fieldDirection)
        {
        }
        #endregion // Constructors

    } // Ends class UdbusVariantBase

    internal abstract class DictCreatorBase : Udbus.Parsing.IDictParamCodeTypeHandler
    {
        #region IDictParamCodeTypeHandler Members

        abstract public Udbus.Parsing.IParamCodeTypeHandler CreateKeyHandler();

        abstract public Udbus.Parsing.IParamCodeTypeHandler CreateValueHandler();

        abstract public void StartDictionary(string name, Udbus.Parsing.BuildContext context);

        abstract public void FinishDictionary(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler);

        #endregion // IDictParamCodeTypeHandler Members
    } // Ends class DictCreatorBase

    namespace Exceptions
    {
        public class InvalidIDLTypeException : System.Exception
        {
            #region Constructors
            public InvalidIDLTypeException()
                : base()
            {
            }
            public InvalidIDLTypeException(string message)
                : base(message)
            {
            }

            public InvalidIDLTypeException(string message, System.Exception innerException)
                : base(message, innerException)
            {
            }

            protected InvalidIDLTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
            }
            #endregion // Constructors

            #region Message functions
            static protected string CreateMessage(string invalidTypeName)
            {
                return string.Format("Invalid type: {0}", invalidTypeName);
            }
            #endregion // Message functions

            #region Creation
            static public InvalidIDLTypeException CreateFromTypeName(string invalidTypeName)
            {
                return new InvalidIDLTypeException(CreateMessage(invalidTypeName));
            }
            #endregion // Creation
        } // Ends class InvalidIDLTypeException

    } // Ends namespace Exceptions
} // Ends namespace Udbus.Serialization
