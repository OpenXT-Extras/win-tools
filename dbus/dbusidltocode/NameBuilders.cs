//
// Copyright (c) 2014 Citrix Systems, Inc.
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

namespace dbusidltocode
{

    internal class IDLArgumentTypeNameBuilder : Udbus.Parsing.IDLArgumentTypeNameBuilderBase
    {
        IDLInterface intf;
        string methodName;

        public IDLArgumentTypeNameBuilder(IDLInterface intf, string methodName)
        {
            this.intf = intf;
            this.methodName = methodName;
        }

        public override string getNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            return Udbus.Parsing.IDLArgumentTypeNameBuilderBase.getNameImpl(context, this.methodName, out fullName);
        }

        public override string getScopedNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            string result = this.getNames(context, out fullName);

            return getScopedNamesResult(ref fullName, result);
        }

        public override string getScopedName(string name)
        {
            return getScopedNameImpl(name);
        }

        public override string getParamScopedName(string name)
        {
            return getParamScopedNameImpl(name);
        }

        public override string getDictionaryNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            return Udbus.Parsing.IDLArgumentTypeNameBuilderBase.getDictionaryNameImpl(context, this.methodName, out fullName);
        }

        public override string getDictionaryScopedNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            string result = this.getNames(context, out fullName);

            return getScopedNamesResult(ref fullName, result);
        }

        public override string getDictionaryScopedName(string name)
        {
            return getScopedNameImpl(name);
        }

        public override string getDictionaryParamScopedName(string name)
        {
            return getParamScopedNameImpl(name);
        }

        private string getScopedNamesResult(ref string fullName, string result)
        {
            result = CodeBuilderCommon.GetScopedName(this.intf.Name, result);
            fullName = CodeBuilderCommon.GetScopedName(this.intf.Name, result);

            return result;
        }

        private string getScopedNameImpl(string name)
        {
            return CodeBuilderCommon.GetScopedName(this.intf.Name, name);
        }

        private string getParamScopedNameImpl(string name)
        {
            return CodeBuilderCommon.GetScopedName(CodeBuilderCommon.GetParamsNamespaceName(this.intf.Name), name);
        }

    } // Ends class IDLMethodArgumentTypeNameBuilder

    //}
    /// <summary>
    /// You want names ? So many names will I give you.
    /// </summary>
    internal class IDLMethodArgumentTypeNameBuilder : Udbus.Parsing.IDLArgumentTypeNameBuilderBase
    {
        IDLInterface intf;
        IDLMethod method;

        public IDLMethodArgumentTypeNameBuilder(IDLInterface intf, IDLMethod method)
        {
            this.intf = intf;
            this.method = method;
        }

        public override string getNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            return Udbus.Parsing.IDLArgumentTypeNameBuilderBase.getNameImpl(context, this.method.Name, out fullName);
        }

        public override string getScopedNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            string result = this.getNames(context, out fullName);

            return getScopedNamesResult(ref fullName, result);
        }

        public override string getScopedName(string name)
        {
            return getScopedNameImpl(name);
        }

        public override string getParamScopedName(string name)
        {
            return getParamScopedNameImpl(name);
        }

        public override string getDictionaryNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            return Udbus.Parsing.IDLArgumentTypeNameBuilderBase.getDictionaryNameImpl(context, this.method.Name, out fullName);
        }

        public override string getDictionaryScopedNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            string result = this.getNames(context, out fullName);

            return getScopedNamesResult(ref fullName, result);
        }

        public override string getDictionaryScopedName(string name)
        {
            return getScopedNameImpl(name);
        }

        public override string getDictionaryParamScopedName(string name)
        {
            return getParamScopedNameImpl(name);
        }

        private string getScopedNamesResult(ref string fullName, string result)
        {
            result = CodeBuilderCommon.GetScopedName(this.intf.Name, result);
            fullName = CodeBuilderCommon.GetScopedName(this.intf.Name, result);

            return result;
        }

        private string getScopedNameImpl(string name)
        {
            return CodeBuilderCommon.GetScopedName(this.intf.Name, name);
        }

        private string getParamScopedNameImpl(string name)
        {
            return CodeBuilderCommon.GetScopedName(CodeBuilderCommon.GetParamsNamespaceName(this.intf.Name), name);
        }
    } // Ends class IDLMethodArgumentTypeNameBuilder

    /// <summary>
    /// These names are more signally. No I don't know what that really means either.
    /// </summary>
    internal class IDLSignalArgumentTypeNameBuilder : Udbus.Parsing.IDLArgumentTypeNameBuilderBase
    {
        IDLInterface intf;
        IDLSignal signal;

        public IDLSignalArgumentTypeNameBuilder(IDLInterface intf, IDLSignal signal)
        {
            this.intf = intf;
            this.signal = signal;
        }

        public override string getNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            return Udbus.Parsing.IDLArgumentTypeNameBuilderBase.getNameImpl(context, this.signal.Name, out fullName);
        }

        public override string getScopedNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            string result = this.getNames(context, out fullName);

            return getScopedNamesResult(ref fullName, result);
        }

        public override string getScopedName(string name)
        {
            return getScopedNameImpl(name);
        }

        public override string getParamScopedName(string name)
        {
            return getParamScopedNameImpl(name);
        }

        public override string getDictionaryNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            return Udbus.Parsing.IDLArgumentTypeNameBuilderBase.getDictionaryNameImpl(context, this.signal.Name, out fullName);
        }

        public override string getDictionaryScopedNames(Udbus.Parsing.BuildContext context, out string fullName)
        {
            string result = this.getNames(context, out fullName);

            return getScopedNamesResult(ref fullName, result);
        }

        public override string getDictionaryScopedName(string name)
        {
            return getScopedNameImpl(name);
        }

        public override string getDictionaryParamScopedName(string name)
        {
            return getParamScopedNameImpl(name);
        }

        private string getScopedNamesResult(ref string fullName, string result)
        {
            result = CodeBuilderCommon.GetScopedName(this.intf.Name, result);
            fullName = CodeBuilderCommon.GetScopedName(this.intf.Name, result);

            return result;
        }

        private string getScopedNameImpl(string name)
        {
            return CodeBuilderCommon.GetScopedName(this.intf.Name, name);
        }

        private string getParamScopedNameImpl(string name)
        {
            return CodeBuilderCommon.GetScopedName(CodeBuilderCommon.GetParamsNamespaceName(this.intf.Name), name);
        }
    }
}
