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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dbusidltocode
{
    internal interface IBuilder
    {
        void Generate(IEnumerable<IDLInterface> interfaces, TextWriter writer);
    }

    internal abstract class Builder : IBuilder
    {
        public virtual void PreProcess() { return; }
        public virtual void ProcessNamespaces(IDLInterface idlIntf) { return; }
        public virtual void DeclareCodeType(IDLInterface idlIntf) { return; }
        public virtual void GenerateMethods(IDLInterface idlIntf) { return; }
        public virtual void GenerateProperties(IDLInterface idlIntf) { return; }
        public virtual void GenerateSignals(IDLInterface idlIntf) { return; }
        public virtual void PostHandleMembers(IDLInterface idlIntf) { return; }
        public virtual void PostProcess() { return; }

        //Protected Globals
        protected CodeTypeDeclaration type;
        protected CodeNamespace ns;
        protected CodeCompileUnit unit;
        protected Visitor visitorType;

        public virtual void Generate(IEnumerable<IDLInterface> interfaces, TextWriter writer)
        {
            unit = new CodeCompileUnit(); //Does all the code compilation
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp"); //DOM provider for CodeCompileUnit
            CodeGeneratorOptions genOptions = CodeBuilderHelper.getCodeGeneratorOptions(); //Has all our styling options
            
            this.PreProcess();

            foreach (IDLInterface idlIntf in interfaces)
            {
                //Generate all of the namespace info
                this.ProcessNamespaces(idlIntf);
                //Generate all of the codetype info
                this.DeclareCodeType(idlIntf);
                //Methods
                this.GenerateMethods(idlIntf);
                //Properties
                this.GenerateProperties(idlIntf);
                //Signals
                this.GenerateSignals(idlIntf);
                //All members handled method
                this.PostHandleMembers(idlIntf);
            } // Ends loop over interfaces

            this.PostProcess();

            // Finally do the code generation!
            provider.GenerateCodeFromCompileUnit(unit, writer, genOptions);

        }

    }
}
