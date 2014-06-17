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

namespace dbusidltocode.marshal.inward
{
    internal class ParamCodeTypeHolderProperty : ParamCodeTypeFactory
    {
        #region Fields
        private CodePropertyReferenceExpression proprefContainerField = null;
        private CodeTypeReference typerefCastType = null;
        #endregion // Fields

        #region Properties
        public CodeExpression ContainerFieldExpression { get { return this.proprefContainerField; } }
        public CodeTypeReference CastType { get { return this.typerefCastType; } }
        #endregion // Properties

        #region Constructors
        public ParamCodeTypeHolderProperty(CodeTypeFactory codetypeFactory, FieldDirection fieldDirection)
            : base(codetypeFactory, fieldDirection)
        {
        }
        #endregion // Constructors

        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
#if !INHERIT_FROM_INTERFACE
            base.HandleCodeParamType(paramtype);
#endif // !INHERIT_FROM_INTERFACE
        }

        #region Type Handling
        public override void HandleByte()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusByte");
#if !INHERIT_FROM_INTERFACE
            base.HandleByte();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleBoolean()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusBoolean");
#if !INHERIT_FROM_INTERFACE
            base.HandleBoolean();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleInt16()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusInt16");
#if !INHERIT_FROM_INTERFACE
            base.HandleInt16();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleUInt16()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusUInt16");
#if !INHERIT_FROM_INTERFACE
            base.HandleUInt16();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleInt32()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusInt32");
#if !INHERIT_FROM_INTERFACE
            base.HandleInt32();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleUInt32()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusUInt32");
#if !INHERIT_FROM_INTERFACE
            base.HandleUInt32();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleInt64()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusInt64");
#if !INHERIT_FROM_INTERFACE
            base.HandleInt64();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleUInt64()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusUInt64");
#if !INHERIT_FROM_INTERFACE
            base.HandleUInt64();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleDouble()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusDouble");
#if !INHERIT_FROM_INTERFACE
            base.HandleDouble();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleString()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusString");
#if !INHERIT_FROM_INTERFACE
            base.HandleString();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleVariant()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusVariant");
#if !INHERIT_FROM_INTERFACE
            base.HandleVariant();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleObjectPath()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusObjectPath");
#if !INHERIT_FROM_INTERFACE
            base.HandleObjectPath();
#endif // !INHERIT_FROM_INTERFACE
        }

        public override void HandleSignature()
        {
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusSignature");
#if !INHERIT_FROM_INTERFACE
            base.HandleSignature();
#endif // !INHERIT_FROM_INTERFACE
        }
        #endregion // Type Handling

        public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
        {
#if !INHERIT_FROM_INTERFACE
            base.HandleStruct(paramtype, nameStruct, nameScope);
#endif // !INHERIT_FROM_INTERFACE
            this.typerefCastType = paramtype.CodeType;
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusObjectStruct");
        }

        public override void HandleDictionary(Udbus.Parsing.ICodeParamType paramtype, string nameDictionary, string nameScope)
        {
#if !INHERIT_FROM_INTERFACE
            base.HandleDictionary(paramtype, nameDictionary, nameScope);
#endif // !INHERIT_FROM_INTERFACE
            this.typerefCastType = paramtype.CodeType;
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusDictionary");
        }

        public void HandleArray(Udbus.Parsing.ICodeParamType paramtype)
        {
            this.typerefCastType = paramtype.CodeType;
            this.proprefContainerField = new CodePropertyReferenceExpression(CodeBuilderCommon.varrefReadValue, "DbusObjectArray");
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            return new ArrayParamCodeTypeHolderProperty(this, this.CodeTypeFactory, this.FieldDirection);
        }
    } // Ends class ParamCodeTypeHolderProperty

    /// <summary>
    /// Stores declaration to array for property.
    /// </summary>
    class ArrayParamCodeTypeHolderProperty : ArrayParamCodeTypeHolder
    {
        private ParamCodeTypeHolderProperty owner;

        public ArrayParamCodeTypeHolderProperty(ParamCodeTypeHolderProperty owner, CodeTypeFactory codetypeFactory, FieldDirection fieldDirection)
            : base(owner, codetypeFactory, fieldDirection)
        {
            this.owner = owner;
        }
        #region IParamCodeTypeHandler Members

        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            base.HandleCodeParamType(paramtype);
            owner.HandleArray(this.paramtype);
        }

        #endregion // Ends IParamCodeTypeHandler Members
    }
} // Ends namespace dbusidltocode.marshal.inward
