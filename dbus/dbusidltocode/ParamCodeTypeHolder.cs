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
    /// Holds onto declarations, and acts as factory for complex types.
    /// </summary>
    //internal class ParamCodeTypeHolder : ParamCodeTypeHolderBase
    internal class ParamCodeTypeFactory : Udbus.Parsing.ParamCodeTypeHolderBase
    {
        public ParamCodeTypeFactory(CodeTypeFactory codetypeFactory, FieldDirection fieldDirection)
            : base(fieldDirection)
        {
            this.codetypeFactory = codetypeFactory;
        }

        protected ParamCodeTypeFactory(CodeTypeFactory codetypeFactory)
            : this(codetypeFactory, FieldDirection.In)
        {}

        private CodeTypeFactory codetypeFactory;
        public CodeTypeFactory CodeTypeFactory { set { this.codetypeFactory = value; } get { return this.codetypeFactory; } }

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
        {
            return this.codetypeFactory.CreateStructCreator(this.codetypeFactory, this);
        }

        public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
        {
            Udbus.Parsing.IDictParamCodeTypeHandler result = null;

            if (this.FieldDirection == FieldDirection.Out)
            {
                result = this.codetypeFactory.CreateDictCreatorOut(this);
            }
            else
            {
                result = this.codetypeFactory.CreateDictCreatorIn(this);
            }

            return result;
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            return new ArrayParamCodeTypeHolder(this, this.codetypeFactory, this.FieldDirection);
        }

    } // Ends class ParamCodeTypeHolder

    /// <summary>
    /// Stores declaration to array.
    /// </summary>
    class ArrayParamCodeTypeHolder : ParamCodeTypeFactory
    {
        Udbus.Parsing.IParamCodeTypeHandler owner;

        public ArrayParamCodeTypeHolder(Udbus.Parsing.IParamCodeTypeHandler owner, CodeTypeFactory codetypeFactory, FieldDirection fieldDirection)
            : base(codetypeFactory, fieldDirection)
        {
            this.owner = owner;
        }

        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            CodeTypeReference typerefArray = new CodeTypeReference(paramtype.CodeType, 1);
            CodeArrayCreateExpression arrayExpr = paramtype.CreateArray();
            base.HandleCodeParamType(new Udbus.Parsing.CodeParamArray(typerefArray, arrayExpr));
            owner.HandleCodeParamType(this.paramtype);
        }
    }
}
