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
using System.Collections.Generic;

namespace dbusidltocode
{
    #region Struct Handling

    /// <summary>
    /// Declares a struct and adds fields to it.
    /// </summary>
    //class StructCreator : StructCreatorBase
    class StructCreator : Udbus.Parsing.StructCreatorBase
    {
        private CodeTypeDeclaration declaration;
        private CodeTypeFactory codetypeFactory;

        public virtual CodeTypeDeclaration Declaration { get { return this.declaration; } }
        public CodeTypeFactory CodeTypeFactory { get { return this.codetypeFactory; } }

        //public StructCreator(IParamCodeTypeHandler owner)
        public StructCreator(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base (owner)
        {
            this.codetypeFactory = codetypeFactory;
        }
        #region IStructBuilder Members

        // Pass-through factory methods delegated to by FieldHandler.
        class FieldHandler : ParamCodeTypeFactory
        {
            Udbus.Parsing.StructCreatorBase owner;

            internal FieldHandler(CodeTypeFactory codetypeFactory, Udbus.Parsing.StructCreatorBase owner)
                : base(codetypeFactory)
            {
                this.owner = owner;
            }

            #region IParamCodeTypeHandler Members

            public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
            {
                return owner.CreateNew();
            }

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeField(paramtype);
            }

            public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
            {
                return owner.CreateDictHandler();
            }

            public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
            {
                return base.CreateNewArrayHandler();
            }

            public override Udbus.Parsing.IParamCodeTypeHandler CreateArrayHandler()
            {
                // Asking a field to create an array is asking a field to create a *new* array.
                return this.CreateNewArrayHandler();
            }

            #endregion
        } // Ends class FieldHandler

        public override Udbus.Parsing.IParamCodeTypeHandler CreateFieldHandler()
        {
            return new FieldHandler(this.codetypeFactory, this);
        }

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateNew()
        {
            return new StructCreator(this.codetypeFactory, this.owner);
        }

        public override void StartStruct(string name, string fullName, Udbus.Parsing.BuildContext context)
        {
            base.StartStruct(name, fullName, context);
            this.declaration = new CodeTypeDeclaration(name);
        }

        public override void AddField(CodeTypeReference codeTypeField)
        {
            CodeMemberField field = base.BuildField(codeTypeField);
            this.declaration.Members.Add(field);
        }

        public override void EndStruct(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.EndStruct(context, paramtypeHandler);
            context.declarationHolder.Add(this.declaration);
            paramtypeHandler.HandleStruct(new Udbus.Parsing.CodeParamDeclaredType(this.declaration), this.name,
                CodeBuilderCommon.GetScopedName(CodeBuilderCommon.nsDbusMarshalCustom, context.declarationHolder.Name)
            );
        }

        public override Udbus.Parsing.BuildContext CreateNestedContext(Udbus.Parsing.BuildContext context)
        {
            return this.CreateNestedContext(context, new Udbus.Parsing.CodeTypeMemberHolder(this.declaration.Members, this.name));
        }

        #endregion // Ends IStructBuilder Members

    } // Ends class StructCreator

    class StructCreatorFullName : StructCreator
    {
        public StructCreatorFullName(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base (codetypeFactory, owner)
        {
        }

        public override void StartStruct(string name, string fullName, Udbus.Parsing.BuildContext context)
        {
            base.StartStruct(name, fullName, context);
            this.Declaration.Name = fullname;
        }

    } // Ends class StructCreatorFullName
    #endregion // Ends Struct Handling

    #region Dictionary Handling
    abstract class DictCreator : Udbus.Parsing.DictionaryCreatorBase
    {
        CodeTypeFactory codetypeFactory;
        Udbus.Parsing.ICodeParamType paramtypeKey;
        Udbus.Parsing.ICodeParamType paramtypeValue;
        protected string name;

        public CodeTypeFactory CodeTypeFactory { get { return this.codetypeFactory; } }

        public DictCreator(CodeTypeFactory codetypeFactory)
        {
            this.codetypeFactory = codetypeFactory;
        }

        internal class ValueHandler : ParamCodeTypeFactory
        {
            DictCreator owner;

            internal ValueHandler(DictCreator owner)
                : base(owner.CodeTypeFactory)
            {
                this.owner = owner;
            }

            #region IParamCodeTypeHandler Members

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeValue(paramtype);
            }

            #endregion
        } // Ends class ValueHandler

        internal class KeyHandler : ParamCodeTypeFactory
        {
            DictCreator owner;

            internal KeyHandler(DictCreator owner)
                : base(owner.CodeTypeFactory)
            {
                this.owner = owner;
            }

            #region IParamCodeTypeHandler Members

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeKey(paramtype);
            }

            #endregion // IParamCodeTypeHandler Members
        } // Ends class KeyHandler

        public override Udbus.Parsing.IParamCodeTypeHandler CreateKeyHandler()
        {
            return new KeyHandler(this);
        }
        public override Udbus.Parsing.IParamCodeTypeHandler CreateValueHandler()
        {
            return new ValueHandler(this);
        }
        public virtual void HandleCodeParamTypeKey(Udbus.Parsing.ICodeParamType paramtype)
        {
            this.paramtypeKey = paramtype;
        }

        public virtual void HandleCodeParamTypeValue(Udbus.Parsing.ICodeParamType paramtype)
        {
            this.paramtypeValue = paramtype;
        }

        private CodeTypeReference typerefDict = null;
        protected CodeTypeReference DictTypeRef { get { return this.typerefDict; } }
        // Default dictionary type is Dictionary (!).
        protected abstract Type DictionaryType { get; }// { get { return CodeBuilderCommon.DictionaryType; } }

        public override void StartDictionary(string name, Udbus.Parsing.BuildContext context)
        {
            base.StartDictionary(name, context);
            this.name = name;
        }

        public override void FinishDictionary(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.FinishDictionary(context, paramtypeHandler);
            this.typerefDict = new CodeTypeReference(this.DictionaryType.Name,
                new CodeTypeReference[]
                {
                    this.paramtypeKey.CodeType,
                    this.paramtypeValue.CodeType
                }
            );
            paramtypeHandler.HandleDictionary(new Udbus.Parsing.CodeParamType(this.typerefDict), this.name, CodeBuilderCommon.GetMarshalDictionaryScope());
        }

    } // Ends class DictCreator

    class DictCreatorIn : DictCreator
    {
        public DictCreatorIn(CodeTypeFactory codetypeFactory)
            : base(codetypeFactory)
        {
        }
        // Dictionary type is Dictionary.
        protected override System.Type DictionaryType { get { return CodeBuilderCommon.DictionaryType; } }

    } // Ends class DictCreatorOut

    class DictCreatorOut : DictCreator
    {
        public DictCreatorOut(CodeTypeFactory codetypeFactory)
            : base(codetypeFactory)
        {
        }
        // Dictionary type is IDictionary.
        protected override System.Type DictionaryType { get { return typeof(IDictionary<,>); } }

    } // Ends class DictCreatorOut

    class DictCreatorOutProperty : DictCreatorOut
    {
        public DictCreatorOutProperty(CodeTypeFactory codetypeFactory)
            : base(codetypeFactory)
        {
        }
        // Dictionary type is IDictionary.
        protected override System.Type DictionaryType { get { return CodeBuilderCommon.DictionaryType; } }

    } // Ends class DictCreatorOut


    #endregion // Ends Dictionary Handling
}
