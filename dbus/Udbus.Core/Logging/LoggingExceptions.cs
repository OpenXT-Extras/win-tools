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

namespace Udbus.Core.Logging.Exceptions
{
    public abstract class FormatterException : Exception
    {
        public FormatterException()
            : base()
        {
        }
        public FormatterException(string message)
            : base(message)
        {
        }
        [System.Security.SecuritySafeCritical]
        protected FormatterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
        public FormatterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    } // Ends class FormatterException

    public class FormatterStashKeyException : FormatterException
    {
        public FormatterStashKeyException()
            : base()
        {
        }
        public FormatterStashKeyException(string message)
            : base(message)
        {
        }
        [System.Security.SecuritySafeCritical]
        protected FormatterStashKeyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
        public FormatterStashKeyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        static public string CreateNumericMessage(string numerals)
        {
            return string.Format("Cannot use numeric string as stash key ('{0}')", numerals);
        }

        static public FormatterStashKeyException CreateNumeric(string numerals)
        {
            return new FormatterStashKeyException(CreateNumericMessage(numerals));
        }
    } // Ends class FormatterStashKeyException
} // Ends namespace Udbus.Core.Logging.Exceptions
