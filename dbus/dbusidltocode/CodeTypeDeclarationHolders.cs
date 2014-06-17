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
using System.CodeDom;

namespace dbusidltocode
{
    /// <summary>
    /// Base class for holders which create objects only when necessary.
    /// </summary>
    abstract class CodeTypeDeferredNamespaceDeclarationHolderBase : Udbus.Parsing.ICodeTypeDeclarationHolder
    {
        public CodeNamespace ns;
        protected IDLInterface intf;

        public CodeTypeDeferredNamespaceDeclarationHolderBase(IDLInterface intf)
        {
            this.intf = intf;
        }

        public CodeTypeDeferredNamespaceDeclarationHolderBase(IDLInterface intf, CodeNamespace ns)
            : this(intf)
        {
            this.ns = ns;
        }

        protected abstract CodeNamespace CreateNamespace();

        #region ICodeTypeDeclarationHolder Members

        public virtual void Add(CodeTypeMember refAdd)
        {
            CodeTypeDeclaration typedeclAdd = refAdd as CodeTypeDeclaration;
            if (this.ns == null)
            {
                this.ns = CreateNamespace();
            }

            this.ns.Types.Add(typedeclAdd);
        }

        public virtual void AddDictionary(CodeTypeMember refAdd)
        {
            this.Add(refAdd);
        }

        public string Name { get { return this.ns == null ? null : this.ns.Name; } }
        #endregion
    } // Ends CodeTypeDeferredNamespaceDeclarationHolder

    /// <summary>
    /// Adds declarations to a "Params" namespace's types.
    /// </summary>
    class CodeTypeDeferredNamespaceDeclarationHolderParams : CodeTypeDeferredNamespaceDeclarationHolderBase
    {
        //Simply wrap base constructors
        public CodeTypeDeferredNamespaceDeclarationHolderParams(IDLInterface intf) : base(intf) {}
        public CodeTypeDeferredNamespaceDeclarationHolderParams(IDLInterface intf, CodeNamespace ns) : base(intf, ns) { }

        protected override CodeNamespace CreateNamespace()
        {
            return new CodeNamespace(CodeBuilderCommon.GetParamsNamespaceName(this.intf));
        }
    } // Ends CodeTypeDeferredNamespaceDeclarationHolderParams

    /// <summary>
    /// Creates a Params namespace if any types are added, but doesn't add the types.
    /// Used to detect when a Params namespace is used.
    /// </summary>
    class CodeTypeIgnoredNamespaceDeclarationHolderParams : CodeTypeDeferredNamespaceDeclarationHolderParams
    {
        //Simply wrap base constructors
        public CodeTypeIgnoredNamespaceDeclarationHolderParams(IDLInterface intf) : base(intf) { }
        public CodeTypeIgnoredNamespaceDeclarationHolderParams(IDLInterface intf, CodeNamespace ns) : base(intf, ns) { }

        public override void Add(CodeTypeMember refAdd)
        {
            if (this.ns == null)
            {
                this.ns = CreateNamespace();
            }
        }

        public override void AddDictionary(CodeTypeMember refAdd)
        {
            this.Add(refAdd);
        }

    } // Ends CodeTypeDeferredNamespaceDeclarationHolderParams

    /// <summary>
    /// Adds declarations to an "Events" namespace's types.
    /// </summary>
    class CodeTypeDeferredNamespaceDeclarationHolderEvents : CodeTypeDeferredNamespaceDeclarationHolderBase
    {
        //Simply wrap base constructors
        public CodeTypeDeferredNamespaceDeclarationHolderEvents(IDLInterface intf) : base(intf) { }
        public CodeTypeDeferredNamespaceDeclarationHolderEvents(IDLInterface intf, CodeNamespace ns) : base(intf, ns) { }

        protected override CodeNamespace CreateNamespace()
        {
            return new CodeNamespace(this.intf.Name + ".Events");
        }
    } // Ends CodeTypeDeferredNamespaceDeclarationHolderEvents

}
