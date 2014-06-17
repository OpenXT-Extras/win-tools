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

namespace Udbus.Parsing
{
    #region IDL Exceptions
    class IDLTypeException : System.Exception
    {
        #region Fields
        private string IDLType_;
        int index;
        #endregion // Ends Fields

        #region Properties
        public string IDLType { get { return this.IDLType_; } }
        public int Index { get { return this.index; } }
        #endregion // Ends Properties

        IDLTypeException(string message)
            : base(message)
        {
        }

        public static IDLTypeException CreateFromFormat(string format, string IDLType, int index)
        {
            string message = string.Format(format, IDLType, index);
            return Create(message, IDLType, index);
        }

        public static IDLTypeException CreateWithData(string message, string IDLType, int index)
        {
            string messageEx = string.Format("{0}. Type=\"{1}\". Index={2}", message, IDLType, index);
            return Create(messageEx, IDLType, index);
        }

        public static IDLTypeException Create(string message, string IDLType, int index)
        {
            IDLTypeException ex = new IDLTypeException(message);
            ex.index = index;
            ex.IDLType_ = IDLType;
            return ex;
        }
    } // Ends class IDLTypeException
    #endregion // Ends IDL Exceptions

    public abstract class DbusIDLToCodeException : System.Exception
    {
        #region Construction
        public DbusIDLToCodeException()
            : base()
        {
        }

        public DbusIDLToCodeException(string message)
            : base(message)
        {
        }

        public DbusIDLToCodeException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected DbusIDLToCodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
        #endregion // Construction

    } // Ends class DbusIDLToCodeException

    public class RuntimeGeneratorException : DbusIDLToCodeException
    {

        #region Construction
        public RuntimeGeneratorException()
            : base()
        {
        }

        public RuntimeGeneratorException(string message)
            : base(message)
        {
        }

        public RuntimeGeneratorException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion // Construction

    } // Ends class RuntimeGeneratorException
} // Ends namespace Udbus.Parsing
