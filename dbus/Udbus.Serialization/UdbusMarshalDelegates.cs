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
    public delegate int MarshalDelegate<T>(Udbus.Serialization.UdbusMessageBuilder builder, T t);
    public delegate void MarshalResultDelegate<T>(Udbus.Serialization.UdbusMessageBuilder builder, T t, out int result);

    // result = UdbusMessageReader.ReadFoo(reader, out Foo.X)
    public delegate int MarshalReadDelegate<T>(Udbus.Serialization.UdbusMessageReader reader, out T t); // This would actually probably be better, despite sample code.
    public delegate T MarshalReadResultDelegate<T>(Udbus.Serialization.UdbusMessageReader reader, out int result);
} // Ends namespace Udbus.Serialization
