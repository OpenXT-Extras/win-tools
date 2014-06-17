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

using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace dbusidltocode.marshal.outward
{
    internal class StructCreatorMarshalOutFactory : StructCreatorFactory
    {
        internal override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructCreator(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
        {
            return new StructCreatorMarshalOut(codetypeFactory, owner);
        }
    }

    #region Struct Handling
    class StructCreatorMarshalOut : Udbus.Parsing.StructCreatorBase
    {
        private CodeMemberMethod method = null;
        private List<CodeStatement> methodStatements = new List<CodeStatement>();
        CodeTypeFactory codetypeFactory;
//        ParamCodeTypeHolderMarshalOut outOwner;

        //public StructCreatorMarshalOut(ParamCodeTypeHolderMarshalOut owner)
        public StructCreatorMarshalOut(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base(owner)
        {
            this.codetypeFactory = codetypeFactory;
            //this.outOwner = owner;
        }

        #region IStructBuilder Members
        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateNew()
        {
            return this.codetypeFactory.CreateStructCreator(this.codetypeFactory, this.owner);//new StructCreatorMarshalOut(this.codetypeFactory, this.owner);
        }

        public override Udbus.Parsing.BuildContext CreateNestedContext(Udbus.Parsing.BuildContext context)
        {
            return this.CreateNestedContext(context, context.declarationHolder);
        }

        public override void StartStruct(string name, string fullName, Udbus.Parsing.BuildContext context)
        {
            base.StartStruct(name, fullName, context);
            System.Diagnostics.Trace.WriteLine(string.Format("OutParam Starting Struct {0}", this.fullname));

            // Going to write a method for handling this struct.
            //public static int ReadFoo(UdbusMessageReader reader, out Foo value)
            //this.method.Statements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.varrefReadValue));
            //this.method.ReturnType = typerefStruct;
            CodeTypeReference typerefStruct = new CodeTypeReference(this.BuildNestedScopedName(context));
            this.method = new CodeMemberMethod();
            this.method.Name = "Read" + this.name + CodeBuilderCommon.ReadSuffix;
            this.method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.method.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typeMessageReader, CodeBuilderCommon.nameReader));
            CodeParameterDeclarationExpression paramdeclResult = new CodeParameterDeclarationExpression(typerefStruct, CodeBuilderCommon.nameReadValue);
            paramdeclResult.Direction = FieldDirection.Out;
            this.method.Parameters.Add(paramdeclResult);
            this.method.ReturnType = CodeBuilderCommon.typerefReadResult;

        }
        public override void AddField(CodeTypeReference codeTypeField)
        {
            // No op
            System.Diagnostics.Trace.WriteLine(string.Format("OutParam Adding Struct {0} Field: {1}", this.name, codeTypeField.BaseType.ToString()));
        }

        internal virtual void HandleCodeParamTypeField(Udbus.Parsing.ICodeParamType paramtype, FieldHandler fieldhandler)
        {
            // Call base class implementation.
            this.HandleCodeParamTypeField(paramtype);
            // Do extra handling.
            CodeMemberField field = this.BuildField(paramtype.CodeType);
            //read.Field1 = Udbus.Core.UdbusMessageReader.ReadStringValue(reader, out result);
            //this.result = Udbus.Core.UdbusMessageReader.ReadBoolean(reader, out a2Result);
            //ArrayStructInOutParam a2Result;
            //this.result = ReadArrayStructInOutParam(reader, out a2Result);
            //this.methodStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(CodeBuilderCommon.varrefReadValue, field.Name),
            //    fieldhandler.BuildReadExpression(CodeBuilderCommon.argrefReader, new CodeDirectionExpression(FieldDirection.Out, CodeBuilderCommon.argrefResult))
            //));
            this.methodStatements.Add(new CodeAssignStatement(CodeBuilderCommon.varrefResult,
                fieldhandler.BuildReadExpression(CodeBuilderCommon.argrefReader, new CodeDirectionExpression(FieldDirection.Out, new CodeFieldReferenceExpression(CodeBuilderCommon.varrefReadValue, field.Name)))
            ));
        }

        public override void EndStruct(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.EndStruct(context, paramtypeHandler);
            //this.outOwner.HandleStruct(this.name); // KILL IT

            CodeTypeReference typerefStruct = new CodeTypeReference(this.BuildNestedScopedName(context));

            // Initialise struct out parameter.
            this.method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(CodeBuilderCommon.nameReadValue),
                new CodeObjectCreateExpression(typerefStruct)
            ));

            if (this.methodStatements.Count > 0)
            {
                // Declare result variable.
                // int result;
                this.method.Statements.Add(new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefReadResult, CodeBuilderCommon.nameResult));

                // Marshal the struct fields in the struct-specific method.
                CodeExpression exprTest = new CodeBinaryOperatorExpression(CodeBuilderCommon.varrefResult
                    , CodeBinaryOperatorType.IdentityEquality, CodeBuilderCommon.exprReadResultSuccessValue
                );

                CodeStatement[] assignFields = CodeBuilderCommon.NestedExpression(exprTest, this.methodStatements).ToArray();
                this.method.Statements.AddRange(assignFields);

                // Return statement.
                this.method.Statements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.varrefResult));
            }
            else
            {
                // No statements ? No implementation.
                // return 0;
                this.method.Statements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.exprReadResultSuccessValue));
            }


            // Tell whomever's interested about the struct.
            paramtypeHandler.HandleStruct(new Udbus.Parsing.CodeParamType(typerefStruct), this.name,
                CodeBuilderCommon.GetScopedName(CodeBuilderCommon.nsDbusMarshalCustom, context.declarationHolder.Name)
            ); // FIX IT
            System.Diagnostics.Trace.WriteLine(string.Format("OutParam Ending Struct {0}", this.name));
            // Add the struct-specific method to the local context holder (typically, a class for holding methods).
            context.declarationHolder.Add(this.method);
        }

        internal class FieldHandler : ParamCodeTypeHolderMarshalOut
        {
            StructCreatorMarshalOut owner;

            internal FieldHandler(CodeTypeFactory codetypeFactory, StructCreatorMarshalOut owner)
                : base(codetypeFactory)
            {
                this.owner = owner;
            }

            #region Udbus.Parsing.IParamCodeTypeHandler Members

            public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
            {
                return owner.CreateNew();
            }

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeField(paramtype, this);
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
                // Asking a field to create an array is asking for a *new* array.
                return this.CreateNewArrayHandler();
            }

            #endregion
        } // Ends class FieldHandler

        public override Udbus.Parsing.IParamCodeTypeHandler CreateFieldHandler()
        {
            return new FieldHandler(this.codetypeFactory, this);
        }
        #endregion // Ends IStructBuilder Members

    } // Ends class StructCreatorMarshalOut
    #endregion // Ends Struct Handling

    #region Dictionary Handling
    internal class DictCreatorMarshalOutFactory : DictCreatorFactory
    {
        internal override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictCreator(ParamCodeTypeFactory paramCodeTypeHolder)
        {
            return new DictCreatorMarshalOut(paramCodeTypeHolder.CodeTypeFactory, paramCodeTypeHolder);
        }
    }

    class DictCreatorMarshalOut : DictCreatorOut
    {
        Udbus.Parsing.IParamCodeTypeHandler owner;
        CodeExpression exprKey = null;
        CodeExpression exprValue = null;
        string nameKey = null;
        string nameValue = null;
        //private CodeMemberMethod method = null;
        ValueHandler valuehandler = null;
        IReadExpressionBuilder exprbuilderValue = null;

        //ICodeParamType paramtypeKey;
        //ICodeParamType paramtypeValue;
        public DictCreatorMarshalOut(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base(codetypeFactory)
        {
            this.owner = owner;
        }

        internal new class ValueHandler : ParamCodeTypeHolderMarshalOut, IReadExpressionBuilder
        {
            private class ValueArrayParamCodeTypeHolderMarshalOut : ArrayParamCodeTypeHolderMarshalOut
            {
                public ValueArrayParamCodeTypeHolderMarshalOut(ParamCodeTypeHolderMarshalOut owner)
                    : base(owner)
                {
                }

                public override string NameEnumerableMethod
                {
                    get
                    {
                        string result = base.NameEnumerableMethod;
                        // What hackery is this ?
                        // Dictionary values aren't covariant, so we stick to arrays throughout the magical kingdom of blerg.
                        // Blerg.
                        if (result.StartsWith(CodeBuilderCommon.nameEnumerableStem))
                        {
                            // Replace "Enumberable" function stem with "Array" function stem.
                            result = CodeBuilderCommon.nameDictionaryValueArrayStem + result.Substring(CodeBuilderCommon.lengthEnumerableStemIn);
                        }
                        return result;
                    }
                }

                // Value handler array
                private class StructExpressionBuilder : IReadExpressionBuilder
                {
                    private string nameStruct;
                    private string nameScope;
                    private CodeTypeReference typerefStruct;

                    internal StructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
                    {
                        this.typerefStruct = typerefStruct;
                        this.nameStruct = nameStruct;
                        this.nameScope = nameScope;
                    }

                    #region IReadExpressionBuilder functions
                    //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
                    public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
                    {
                        CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader,
                            "StructMarshaller",
                            new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(nameScope), "Read" + this.nameStruct)
                        );
                        methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                        CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader, "ArrayMarshallerStruct",
                            methodStructMarshaller
                        );
                        return exprMarshal;
                    }
                    #endregion // Ends IReadExpressionBuilder functions

                } // Ends class StructExpressionBuilder

                /// <summary>
                /// Sub-arrays for dictionary values expression builder.
                /// </summary>
                private class ArrayExpressionInnerBuilder : IReadExpressionBuilder
                {
                    #region IReadExpressionBuilder functions
                    /// <summary>
                    /// Create an invocation of the inner method.
                    /// </summary>
                    ///// <param name="exprMethod">Type to call method on.</param>
                    ///// <param name="name">Method to call.</param>
                    ///// <param name="methodTypes">Generic method types.</param>
                    /// <param name="info">Build information.</param>
                    /// <param name="args">Method arguments.</param>
                    /// <returns>Invocation of method with arguments.</returns>
                    //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
                    public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
                    {
                        CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression,
                            info.NameReadMethod,
                            info.MethodTypes);

                        return new CodeMethodInvokeExpression(methodrefMarshal, args);
                    }
                    #endregion // Ends IReadExpressionBuilder functions
                } // Ends class ArrayExpressionInnerBuilder

                /// <summary>
                /// Same as outer ArrayParamCodeTypeHolderMarshalOut class but builds array expressions differently.
                /// i.e. acts as a class factory for array expression builder differently.
                /// </summary>
                private class NestedArrayParamCodeTypeHolderMarshalOut : ValueArrayParamCodeTypeHolderMarshalOut
                {
                    internal NestedArrayParamCodeTypeHolderMarshalOut(ArrayParamCodeTypeHolderMarshalOut owner)
                        : base(owner)
                    {
                    }
                    /// <summary>
                    /// Factory for write expression builder.
                    /// </summary>
                    /// <returns>IReadExpressionBuilder implementation.</returns>
                    protected override IReadExpressionBuilder CreateArrayExpressionBuilder()
                    {
                        return new ArrayExpressionInnerBuilder();
                    }

                } // Ends NestedArrayParamCodeTypeHolderMarshalOut

                protected override ArrayParamCodeTypeHolderMarshalOut CreateNestedArrayHandler()
                {
                    return new NestedArrayParamCodeTypeHolderMarshalOut(this);
                }

                /// <summary>
                /// ArrayExpressionBuilder for simple dictionary value arrays.
                /// </summary>
                private class ArrayExpressionBuilder : IReadExpressionBuilder
                {
                    //CodeExpression exprArrayMethodTarget;
                    //string nameArrayMethod;

                    internal ArrayExpressionBuilder() { }
                    internal ArrayExpressionBuilder(CodeExpression exprArrayMethodTarget, string nameArrayMethod)
                    {
                        //this.exprArrayMethodTarget = exprArrayMethodTarget;
                        //this.nameArrayMethod = nameArrayMethod;
                    }

                    #region IReadExpressionBuilder functions
                    //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
                    public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
                    {
                        //CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageBuilder, "StructMarshaller",
                        //    new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(this.nameScope), "BodyAdd_" + this.nameStruct)
                        //);
                        //methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                        CodeMethodReferenceExpression methodArrayValueMarshaller = new CodeMethodReferenceExpression(info.MethodExpression, info.NameReadMethod);

                        CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader,
                            info.NameEnumerableMethod,
                            methodArrayValueMarshaller
                        );

                        //// Assume method type applies to ArrayMarshaller, and not marshalled delegate method (which seems fair really).
                        //if (info.MethodTypes != null && info.MethodTypes.Length > 0)
                        //{
                        //    exprMarshal.Method.TypeArguments.AddRange(info.MethodTypes);
                        //}

                        return exprMarshal;
                    }
                    #endregion // Ends IReadExpressionBuilder functions
                } // Ends class ArrayExpressionBuilder

                /// <summary>
                /// Factory for write expression builder.
                /// </summary>
                /// <returns>IReadExpressionBuilder implementation.</returns>
                protected override IReadExpressionBuilder CreateArrayExpressionBuilder()
                {
                    IReadExpressionBuilder writeBuilder = null;
                    if (!this.QuSubArray) // If this is an inner array
                    {
                        // For some (crappy) reason, outer ArrayMarshaller can deduce its generic type,
                        // but innermost (e.g. ArrayMarshaller<string>(Body_AddString)) needs the type specified.
                        // Yeck.
                        SetMethodType(this, this.paramtype.CodeType.ArrayElementType);

                        // Override behaviour with special array expression builder for dictionary values.
                        writeBuilder = new ArrayExpressionBuilder();

                    } // Ends if this is an inner array
                    else // Else not inner array
                    {
                        // Just do the standard action.
                        writeBuilder = base.CreateArrayExpressionBuilder();

                    } // Ends else not inner array

                    return writeBuilder;
                }

                protected override IReadExpressionBuilder CreateStructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
                {
                    return new StructExpressionBuilder(typerefStruct, nameStruct, nameScope);
                }

                protected override CodeExpression BuildMarshalEnumerableOut(CodeExpression exprArray, params CodeExpression[] args)
                {
                    //CodeMethodReferenceExpression exprMethod = new CodeMethodReferenceExpression(
                    //return new CodeMethodInvokeExpression(null,"EnumerableMarshaller", exprArray);
                    return new CodeMethodInvokeExpression(this.MethodExpression, this.NameReadMethod, exprArray);
                }

                public override void HandleSubArray()
                {
                    base.HandleSubArray(); // Do the default behaviour

                    if (this.NameReadMethod == CodeBuilderCommon.nameDefaultEnumerableMarshallerOut) // If standard marshaller in use
                    {
                        // Switch to whatever we use to marshal dictionary array values.
                        this.HandleSubArray(this.MethodExpression, CodeBuilderCommon.nameDictionaryValueArrayMarshaller);

                        if (this.QuSubArray) // If got sub-array
                        {
                            ClearMethodType(this);

                        } // Ends if got sub-array
                    } // Ends if standard marshaller in use
                }

                /// <summary>
                /// For the marshalling method to be invoked, create the appropriate method type (generic type).
                /// This override does not include an invoke type because we don't use the IEnumerable&gt;T&lt; in marshalling dictionary value.
                /// </summary>
                /// <param name="exprInvoke">Marshalling method being created.</param>
                /// <param name="arrayElementType">Array type of marshalling method.</param>
                //protected override void BuildMethodInvokeMethodType(CodeMethodInvokeExpression exprInvoke, CodeTypeReference arrayElementType)
                //{
                //    // Hacky question to ask, but basically if we've got a sub-array which is actually just the referenced type...
                //    if (this.QuSubArray && !this.QuSubArrayIsArray) // If got sub-array which holds a value
                //    {
                //        base.BuildMethodInvokeMethodType(exprInvoke, arrayElementType);

                //    } // Ends if got sub-array which holds a value
                //}

                protected override void BuildMethodInvokeMethodTypes(CodeMethodInvokeExpression exprInvoke, CodeTypeReferenceCollection elementTypes)
                {
                    // Hacky question to ask, but basically if we've got a sub-array which is actually just the referenced type...
                    if (this.QuSubArray && !this.QuSubArrayIsArray) // If got sub-array which holds a value
                    {
                        base.BuildMethodInvokeMethodTypes(exprInvoke, elementTypes);

                    } // Ends if got sub-array which holds a value
                }

            } // Ends ValueArrayParamCodeTypeHolderMarshalOut

            public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
            {
                return new ValueArrayParamCodeTypeHolderMarshalOut(this);
            }

            // TODO Create ArrayHandler with own write method.
            DictCreatorMarshalOut owner;
            IReadExpressionBuilder exprbuilderValue = null;

            internal new string NameReadMethod { get { return base.NameReadMethod; } }
            internal new CodeExpression MethodExpression { get { return base.MethodExpression; } }

            internal ValueHandler(DictCreatorMarshalOut owner)
                : base(owner.CodeTypeFactory)
            {
                this.owner = owner;
            }

            #region Udbus.Parsing.IParamCodeTypeHandler Members

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeValue(paramtype, this);
            }

            public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
            {
                base.HandleStruct(paramtype, nameStruct, nameScope);
                IReadExpressionBuilder exprbuilder = new StructExpressionBuilder(paramtype.CodeType, nameStruct, nameScope);
                this.exprbuilderValue = exprbuilder;
                SetExpressionBuilder(this, exprbuilder);
            }
            #endregion // Udbus.Parsing.IParamCodeTypeHandler Members

            protected override CodeExpression HandleBuildReadExpression(params CodeExpression[] args)
            {
                return new CodeMethodReferenceExpression(this.MethodExpression, this.NameReadMethod);
            }

            #region IReadExpressionBuilder Members
            // Waxme
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                CodeExpression result;
                if (this.exprbuilderValue != null) // If got expression builder
                {
                    result = this.exprbuilderValue.Build(info, args);

                } // Ends if got expression builder
                else
                {
                    result = this.HandleBuildReadExpression(args);
                }

                return result;
            }
            #endregion // IReadExpressionBuilder Members
        } // Ends class ValueHandler

        internal new class KeyHandler : ParamCodeTypeHolderMarshalOut
        {
            DictCreatorMarshalOut owner;

            internal KeyHandler(DictCreatorMarshalOut owner)
                : base(owner.CodeTypeFactory)
            {
                this.owner = owner;
            }

            #region Udbus.Parsing.IParamCodeTypeHandler Members

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeKey(paramtype, this);
            }

            #endregion
        } // Ends class KeyHandler

        // For dictionary.
        // TODO Identical to array nested struct builder. Refactor code.
        private class StructExpressionBuilder : IReadExpressionBuilder
        {
            private string nameStruct;
            private string nameScope;
            private CodeTypeReference typerefStruct;

            internal StructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
            {
                this.typerefStruct = typerefStruct;
                this.nameStruct = nameStruct;
                this.nameScope = nameScope;
            }

            #region IReadExpressionBuilder functions
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                //throw new System.NotImplementedException("How did I get here ? This doesn't look right");
                // TODO - Values that are arrays need to be cast from Dictionary<Key,Array[]> to IDictionary<Key,IEnumerator<Array>> which sucks.
                // Either do it, or add an array marshaller...
                // ENDS TODO - THAT IS THE NEXT THING
                // Build marshaller for struct.
                // return builder.MarshalDict(value, Udbus.Core.UdbusMessageBuilder.BodyAdd_Int32,
                //    Udbus.Core.UdbusMessageReader.StructMarshaller<DictyTestParam>(ReadDictyTestParam)); // <-- This bit
                CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader, "StructMarshaller",
                    new CodeMethodReferenceExpression(null, "Read" + this.nameStruct)
                );
                methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                return methodStructMarshaller;
            }
            #endregion // Ends IReadExpressionBuilder functions

        } // Ends class StructExpressionBuilder (Dictionary)

        public override Udbus.Parsing.IParamCodeTypeHandler CreateKeyHandler()
        {
            return new KeyHandler(this);
        }
        public override Udbus.Parsing.IParamCodeTypeHandler CreateValueHandler()
        {
            return new ValueHandler(this);
        }
        public virtual void HandleCodeParamTypeKey(Udbus.Parsing.ICodeParamType paramtype, KeyHandler keyhandler)
        {
            this.HandleCodeParamTypeKey(paramtype);
            this.exprKey = keyhandler.MethodExpression;
            this.nameKey = keyhandler.NameReadMethod;
        }

        public virtual void HandleCodeParamTypeValue(Udbus.Parsing.ICodeParamType paramtype, ValueHandler valuehandler)
        {
            this.HandleCodeParamTypeValue(paramtype);
            this.exprbuilderValue = valuehandler;
            this.valuehandler = valuehandler;
            this.exprValue = valuehandler.MethodExpression;
            this.nameValue = valuehandler.NameReadMethod;
        }

        public override void FinishDictionary(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            //this.owner.HandleCodeParamType(new CodeParamType(new CodeTypeReference(CodeBuilderCommon.DictionaryType.Name,
            //    new CodeTypeReference[]
            //    {
            //        this.paramtypeKey.CodeType,
            //        this.paramtypeValue.CodeType
            //    }
            //)));
            base.FinishDictionary(context, paramtypeHandler);
            CodeMemberMethod method = new CodeMemberMethod();
            
            method.Name = "Read" + this.name + CodeBuilderCommon.DictionaryFunctionSuffix;
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefMessageReader, CodeBuilderCommon.nameReader));

            CodeParameterDeclarationExpression paramdeclResult = new CodeParameterDeclarationExpression(this.DictTypeRef, CodeBuilderCommon.nameReadValue);
            paramdeclResult.Direction = FieldDirection.Out;

            method.Parameters.Add(paramdeclResult);
            method.ReturnType = CodeBuilderCommon.typerefResult;
            // TODO - fix value handler write expression for array so that it doesn't try to access the args...
            // Try subclassing the normal dude to overwrite its Write method ?
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"),
                    "MarshalDict",
                    new CodeMethodReferenceExpression(this.exprKey, this.nameKey),
                    this.valuehandler.BuildReadExpression(),
                    new CodeDirectionExpression(FieldDirection.Out, new CodeArgumentReferenceExpression(CodeBuilderCommon.nameReadValue))
                    //this.exprbuilderValue.Build(this.exprValue, this.nameValue, null)
                    //new CodeMethodReferenceExpression(this.exprValue, this.nameValue)
                )
            ));
            context.declarationHolder.AddDictionary(method);

            System.Diagnostics.Trace.WriteLine(string.Format("OutParam Ending Dictionary {0}", this.name));
        }

    } // Ends class DictCreatorMarshal
    #endregion // Ends Dictionary Handling

    /// <summary>
    /// Stores declaration to array out parameter.
    /// </summary>
    class ArrayParamCodeTypeHolderMarshalOut : ParamCodeTypeHolderMarshalOut, IReadExpressionBuilder
    {
        /// <summary>
        /// Same as outer ArrayParamCodeTypeHolderMarshalOut class but builds array expressions differently.
        /// i.e. acts as a class factory for array expression builder differently.
        /// </summary>
        private class NestedArrayParamCodeTypeHolderMarshalOut : ArrayParamCodeTypeHolderMarshalOut
        {
            internal NestedArrayParamCodeTypeHolderMarshalOut(ArrayParamCodeTypeHolderMarshalOut owner)
                : base(owner)
            {
            }
            /// <summary>
            /// Factory for write expression builder.
            /// </summary>
            /// <returns>IReadExpressionBuilder implementation.</returns>
            protected override IReadExpressionBuilder CreateArrayExpressionBuilder()
            {
                return new ArrayExpressionInnerBuilder();
            }

        } // Ends NestedArrayParamCodeTypeHolderMarshalOut

        #region Struct expression building
        /// <summary>
        /// Struct expression builder for out array arguments.
        /// </summary>
        private class StructExpressionBuilder : IReadExpressionBuilder
        {
            private string nameStruct;
            private string nameScope;
            private CodeTypeReference typerefStruct;

            internal StructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
            {
                this.typerefStruct = typerefStruct;
                this.nameStruct = nameStruct;
                this.nameScope = nameScope;
            }

            #region IReadExpressionBuilder functions
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                //result = reader.MarshalEnumerable<TrickyDictParam.NestedI3>(
                //    Udbus.Core.UdbusMessageReader.StructMarshaller<TrickyDictParam.NestedI3>(ReadTrickyDictNestedI3),
                //    out valueTemp.Field4
                //);
                //CodeExpression[] argsRemainder = new CodeExpression[args.Length - 1];
                //for (int nArgs = 1; nArgs < args.Length; ++nArgs)
                //{
                //    argsRemainder[nArgs - 1] = args[nArgs];
                //}
                CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader, "StructMarshaller",
                    new CodeMethodReferenceExpression(info.MethodExpression, info.NameReadMethod)
                );
                methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalEnumerableStructOut(methodStructMarshaller, args);
                return exprMarshal;
            }
            #endregion // Ends IReadExpressionBuilder functions

        } // Ends class StructExpressionBuilder
        #endregion // Struct expression building

        /// <summary>
        /// Dictionary expression builder for array out arguments.
        /// </summary>
        private class DictionaryExpressionBuilder : IReadExpressionBuilder
        {
            private string nameDictionary;
            private CodeTypeReference typerefDictionary;
            private string nameScope;

            // TODO Sort out that namespacing - update Build to use the appropriate namespace and generate the rest from the sictionary name.
            internal DictionaryExpressionBuilder(CodeTypeReference typerefDictionary, string nameDictionary, string nameScope)
            {
                this.typerefDictionary = typerefDictionary;
                this.nameDictionary = nameDictionary;
                this.nameScope = nameScope;
            }

            #region IReadExpressionBuilder functions
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodMarshalDictionary = new CodeMethodReferenceExpression(info.MethodExpression, info.NameReadMethod);

                if (info.MethodTypes != null && info.MethodTypes.Length > 0)
                {
                    methodMarshalDictionary.TypeArguments.AddRange(info.MethodTypes);

                }

                CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalEnumerableOut(methodMarshalDictionary, args);
                return exprMarshal;
            }
            #endregion // Ends IReadExpressionBuilder functions

        } // Ends class DictionaryExpressionBuilder

        #region Array expression building
        protected class ArrayExpressionCommon
        {
            /// <summary>
            /// Common implementation for array marshalling expressions.
            /// </summary>
            /// <param name="exprMethod">Type containing method.</param>
            /// <param name="name">Method name to call.</param>
            /// <param name="methodTypes">Method generic types.</param>
            /// <returns>Method reference.</returns>
            static internal CodeMethodReferenceExpression BuildCommonImpl(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes)
            {
                // Parameters ignored because array knows what the type is, not the thing that created array.
                // Bit hacky...
                //CodeMethodReferenceExpression methodrefMarshal = new CodeMethodReferenceExpression(this.exprArrayMethodTarget, this.nameArrayMethod);
                CodeMethodReferenceExpression methodrefMarshal = new CodeMethodReferenceExpression(exprMethod, name);

                if (methodTypes != null)
                {
                    methodrefMarshal.TypeArguments.AddRange(methodTypes);
                }

                return methodrefMarshal;
            }
        } // Ends class ArrayExpressionCommon

        /// <summary>
        /// BROKEN. TODO WAXME ?
        /// Builds an inner array marshaller but a) Includes arguments (wrong), and b) Has no sense of further nested arrays (stupid).
        /// </summary>
        private class ArrayExpressionInnerBuilder : IReadExpressionBuilder
        {
            // THIS IS A MISTAKE BECAUSE WE INVOKE THE METHOD WITH THE ARGS.
            // THE INVOCATION SHOULD HAPPEN AT THE OUTERMOST LAYER, NOT HERE AT THE INNERMOST.
            // TODO WAXME ?
            #region IReadExpressionBuilder functions
            /// <summary>
            /// Create an invocation of the inner method.
            /// </summary>
            /// <param name="exprMethod">Type to call method on.</param>
            /// <param name="name">Method to call.</param>
            /// <param name="methodTypes">Generic method types.</param>
            /// <param name="args">Method arguments.</param>
            /// <returns>Invocation of method with arguments.</returns>
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameReadMethod, info.MethodTypes);

                return new CodeMethodInvokeExpression(methodrefMarshal, args);
            }
            #endregion // Ends IReadExpressionBuilder functions
        } // Ends class ArrayExpressionInnerBuilder

        /// <summary>
        /// BROKEN. TODO WAXME ?
        /// Builds an outer array marshalling expression but a) Includes arguments (wrong), and b) Has no sense of further nested arrays (stupid).
        /// </summary>
        private class ArrayExpressionOuterBuilder : IReadExpressionBuilder
        {
            #region IReadExpressionBuilder functions
            /// <summary>
            /// Create an invocation of the outer method.
            /// </summary>
            /// <param name="exprMethod">Type to call method on.</param>
            /// <param name="name">Method to call.</param>
            /// <param name="methodTypes">Generic method types.</param>
            /// <param name="args">Method arguments.</param>
            /// <returns>Invocation of method with arguments.
            /// Typically this is an invocation taking another (inner) method invocation as a parameter, and returning a delegate to pass to an outer invocation.
            /// </returns>
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                // Create the common method reference.
                CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameReadMethod, info.MethodTypes);
                // Invoke the common method reference.
                CodeMethodInvokeExpression methodinvokeMarshalParam = new CodeMethodInvokeExpression(methodrefMarshal, args);
                return CodeBuilderCommon.BuildMarshalEnumerableOut(info.MethodExpression, args);
                // Use the common method reference invocation as the parameter to MarshalEnumerable.
                //CodeMethodInvokeExpression methodinvokeMarshal = new CodeMethodInvokeExpression(exprMethod, "MarshalEnumerable", methodinvokeMarshalParam);
                //return methodinvokeMarshal;

                //CodeExpression[] adjustedArgs = new CodeExpression[args.Length + 2];
                //args.CopyTo(adjustedArgs, 1);
                //adjustedArgs[0] = exprMethod;
                //adjustedArgs[args.Length + 1] = methodrefMarshal;
                //return CodeBuilderCommon.BuildMarshalEnumerableOut(exprMethod, adjustedArgs);

            }
            #endregion // Ends IReadExpressionBuilder functions
        } // Ends class ArrayExpressionOuterBuilder

        /// <summary>
        /// BROKEN. TODO WAXME ?
        /// Builds an array marshaller expression but a) Includes arguments (wrong), and b) Has no sense of further nested arrays (stupid).
        /// </summary>
        private class ArrayExpressionBuilder : IReadExpressionBuilder
        {
            CodeExpression exprArrayMethodTarget;
            string nameArrayMethod;

            internal ArrayExpressionBuilder() { }
            internal ArrayExpressionBuilder(CodeExpression exprArrayMethodTarget, string nameArrayMethod)
            {
                this.exprArrayMethodTarget = exprArrayMethodTarget;
                this.nameArrayMethod = nameArrayMethod;
            }

            #region IReadExpressionBuilder functions
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                // Parameters ignored because array knows what the type is, not the thing that created array.
                // Bit hacky...
                //CodeMethodReferenceExpression methodrefMarshal = new CodeMethodReferenceExpression(this.exprArrayMethodTarget, this.nameArrayMethod);
                CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameReadMethod, info.MethodTypes);
                return CodeBuilderCommon.BuildMarshalEnumerableOut(methodrefMarshal, args);
            }
            #endregion // Ends IReadExpressionBuilder functions
        } // Ends class ArrayExpressionBuilder
        #endregion // Array expression building

        ParamCodeTypeHolderMarshalOut owner;
        ArrayParamCodeTypeHolderMarshalOut sub = null;
        private bool bGenericRequired = false;

        public ArrayParamCodeTypeHolderMarshalOut(ParamCodeTypeHolderMarshalOut owner)
            : base(owner.CodeTypeFactory)
        {
            this.owner = owner;
        }
        #region Udbus.Parsing.IParamCodeTypeHandler Members

        protected bool QuSubArray { get { return this.sub != null; } }
        protected bool QuSubArrayIsArray { get { return this.sub != null && this.sub.sub != null; } }
        protected virtual CodeTypeReference OptionalGenericType { get { return this.bGenericRequired ? this.paramtype.CodeType.ArrayElementType : null; } }
        protected virtual CodeTypeReferenceCollection OptionalGenericTypes
        {
            get
            {
                CodeTypeReferenceCollection result = null;

                if (this.bGenericRequired) // If going to be using optional generic
                {
                    // As it happens, every situation where we require these generics is based on the sub element array type information.
                    // The hack here is that if the array type information has generic types, we supply those (i.e. dictionary).
                    // Otherwise we supply the array type.
                    if (this.paramtype.CodeType.ArrayElementType.TypeArguments != null
                        && this.paramtype.CodeType.ArrayElementType.TypeArguments.Count > 0)
                    {
                        result = this.paramtype.CodeType.ArrayElementType.TypeArguments;
                    }
                    else
                    {
                        result = new CodeTypeReferenceCollection();
                        result.Add(this.paramtype.CodeType.ArrayElementType);
                    }
                }
                return result;
                //return this.bGenericRequired ? new CodeTypeReference[] { this.paramtype.CodeType.ArrayElementType } : null;
                //return this.bGenericRequired ? this.paramtype.CodeType.ArrayElementType : null;
            }
        }

        #region Type Handling

        public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
        {
            this.bGenericRequired = true;
            base.HandleStruct(paramtype, nameStruct, nameScope);
            SetExpressionBuilder(this, this.CreateStructExpressionBuilder(paramtype.CodeType, nameStruct, nameScope));
        }

        public override void HandleDictionary(Udbus.Parsing.ICodeParamType paramtype, string nameDictionary, string nameScope)
        {
            this.bGenericRequired = true;
            base.HandleDictionary(paramtype, nameDictionary, nameScope);
            SetExpressionBuilder(this, new DictionaryExpressionBuilder(paramtype.CodeType, nameDictionary, nameScope));
        }

        #endregion // Type Handling

        protected virtual ArrayParamCodeTypeHolderMarshalOut CreateNestedArrayHandler()
        {
            return new NestedArrayParamCodeTypeHolderMarshalOut(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateArrayHandler()
        {
            if (this.sub != null)
            {
                throw new dbusidltocode.RuntimeGeneratorException("CreateArrayHandler() has no sub array");
            }

            // This array contains an array. Bam.
            this.sub = new NestedArrayParamCodeTypeHolderMarshalOut(this);// this.CreateArrayHandlerImpl();
            Udbus.Parsing.IParamCodeTypeHandler handler = this.sub;
            this.HandleSubArray();
            return handler;
        }

        /// <summary>
        /// Factory for read expression builder.
        /// </summary>
        /// <returns>IReadExpressionBuilder implementation.</returns>
        protected virtual IReadExpressionBuilder CreateArrayExpressionBuilder()
        {
            IReadExpressionBuilder readBuilder = null;
            if (this.sub != null) // If this is an outer array
            {
                readBuilder = new ArrayExpressionOuterBuilder();
            }
            else
            {
                readBuilder = new ArrayExpressionBuilder();
            }
            return readBuilder;
        }

        //public override CodeExpression BuildReadExpression(params CodeExpression[] args)
        //{
        //    CodeExpression result;
        //    if (this.exprbuilder == null)
        //    {
        //        //result = base.BuildReadExpression(args);
        //        CodeMethodReferenceExpression methodrefMarshal = new CodeMethodReferenceExpression(this.Method, this.NameReadMethod);
        //        result = CodeBuilderCommon.BuildMarshalEnumerableOut(methodrefMarshal, args);
        //    }
        //    else
        //    {
        //        result = this.exprbuilder.Build(this.Method, this.NameReadMethod, args);
        //    }
        //    return result;
        //    // TODO - work out the method for reading out.
        //    //return new CodeMethodInvokeExpression(this.typerefexprMethod, this.nameReadMethod, args);
        //}


        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            CodeArrayCreateExpression arrayExpr = paramtype.CreateArray();
            Udbus.Parsing.CodeParamArray arraytype = new Udbus.Parsing.CodeParamArray(arrayExpr);
            // FIX IT and then do dictionaries.
            //result = reader.MarshalEnumerable<TrickyDictParam.NestedI3>(
            //    Udbus.Core.UdbusMessageReader.StructMarshaller<TrickyDictParam.NestedI3>(ReadTrickyDictNestedI3),
            //    out valueTemp.Field4
            //);
            base.HandleCodeParamType(arraytype);

            if (this.QuExpressionBuilder == false)
            {
                IReadExpressionBuilder exprbuilder = this.CreateArrayExpressionBuilder();

                // Use the array expression builder.
                SetExpressionBuilder(this, exprbuilder);
            }

            if (owner.QuExpressionBuilder == false)
            {
                // Use this Array holder as owner's expression builder
                // (which will in turn delegate to it's expression builder).
                SetExpressionBuilder(owner, this);
            }

            owner.HandleCodeParamType(arraytype);
        }

        #endregion // Ends Udbus.Parsing.IParamCodeTypeHandler Members

        protected virtual CodeExpression BuildMarshalEnumerableOut(CodeExpression exprArray, params CodeExpression[] args)
        {
            return CodeBuilderCommon.BuildMarshalEnumerableOut(exprArray, args);
        }

        #region IReadExpressionBuilder Members

        //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
        public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
        {
            CodeExpression exprResult = null;
            if (this.QuSubArray) // If there is a sub-array
            {
                // Ignore calling parameters because it's this array object that knows what's what.
                exprResult = this.BuildArray();
                exprResult = this.BuildMarshalEnumerableOut(exprResult, args);

            } // Ends if there is a sub-array
            else
            {
                // Ignore calling parameters because it's this array object that knows what's what.
                exprResult = this.BuildReadExpression(args);
            }
            return exprResult;
        }

        #endregion //IReadExpressionBuilder Members

        /// <summary>
        /// For the marshalling method to be invoked, create the appropriate method type (generic type).
        /// </summary>
        /// <param name="exprInvoke">Marshalling method being created.</param>
        /// <param name="arrayElementType">Array type of marshalling method.</param>
        protected virtual void BuildMethodInvokeMethodType(CodeMethodInvokeExpression exprInvoke, CodeTypeReference arrayElementType)
        {
            if (arrayElementType != null)
            {
                CodeTypeReference typerefEnumerable = CodeBuilderCommon.ArrayTypeToEnumerableType(sub.paramtype.CodeType.ArrayElementType);//new CodeTypeReference(typeof(IEnumerable<>).Name, new CodeTypeReference[] { sub.paramtype.CodeType.ArrayElementType });
                exprInvoke.Method.TypeArguments.Add(typerefEnumerable);
            }
        }

        /// <summary>
        /// For the marshalling method to be invoked, create the appropriate method type (generic type).
        /// </summary>
        /// <param name="exprInvoke">Marshalling method being created.</param>
        /// <param name="elementTypes">Array types of marshalling method.</param>
        protected virtual void BuildMethodInvokeMethodTypes(CodeMethodInvokeExpression exprInvoke, CodeTypeReferenceCollection elementTypes)
        {
            if (elementTypes != null && elementTypes.Count > 0)
            {
                // Bit of a hack, but essentially in every case (the most annoying being dictionary), the last type is the one that needs fixing up.
                CodeTypeReference[] arrayAdjustedElementTypes = new CodeTypeReference[elementTypes.Count];
                int iter = 0;

                for (; iter < elementTypes.Count - 1; ++iter)
                {
                    arrayAdjustedElementTypes[iter] = elementTypes[iter];

                } // Ends loop copying all but last element

                // Since dictionary values support covariance about as well as Republicans support healthcare,
                // we have to stick with arrays as part of the interface.
                arrayAdjustedElementTypes[iter] = elementTypes[iter];// CodeBuilderCommon.ArrayTypeToEnumerableType(elementTypes[iter]);
                exprInvoke.Method.TypeArguments.AddRange(arrayAdjustedElementTypes);
            }
        }

        /// <summary>
        /// Called when an array is nested inside another array. E.g. "aai".
        /// </summary>
        /// <returns>Delegate (potentially composed from other nested delegates) to pass to enumerable marshaller.</returns>
        protected virtual CodeExpression BuildArray()
        {
            CodeExpression exprResult = null;

            if (this.sub != null) // If got sub array
            {
                // We need to get the param type, dereference array rank by 1 (e.g. Int32[] => Int32) for marshal param.
                // Only at this point have all the types been established.
                // TODO - We don't really need to store this, because it's only used a few lines lower down, and nowhere else (?).
                SetMethodType(this, this.sub.paramtype.CodeType.ArrayElementType);

                // Nested array with a sub-array. Produce UdbusMessageReader.ArrayMarshaller(<sub.BuildArray()>)
                CodeExpression exprSub = this.sub.BuildArray();
                //CodeMethodInvokeExpression exprInvoke = new CodeMethodInvokeExpression(this.MethodExpression, this.NameReadMethod, exprSub);
                CodeMethodInvokeExpression exprInvoke = new CodeMethodInvokeExpression(this.MethodExpression, this.sub.NameEnumerableMethod, exprSub);

                this.BuildMethodInvokeMethodTypes(exprInvoke, this.sub.OptionalGenericTypes);

                //if (this.MethodTypes != null) // If got method types
                //{
                //    exprInvoke.Method.TypeArguments.Add(sub.paramtype.CodeType.ArrayElementType);

                //} // Ends if got method types

                exprResult = exprInvoke;

            } // Ends if got sub array
            else // Else no sub array
            {
                // Leaf nested array. Produce the method reference for the actual type.
                exprResult = new CodeMethodReferenceExpression(this.MethodExpression, this.NameReadMethod);

            } // Ends else no sub array

            return exprResult;
        }

    } // Ends class ArrayParamCodeTypeHolderMarshalOut

    #region Out Param Handler
    internal interface IReadBuildInfo
    {
        string NameReadMethod { get; }
        string NameEnumerableMethod { get; }
        CodeExpression MethodExpression { get; }
        CodeTypeReference[] MethodTypes { get; }

    } // Ends interface IReadBuildInfo

    /// <summary>
    /// Simple struct to hold build parameters.
    /// </summary>
    struct ReadBuildInfo : IReadBuildInfo
    {
        #region IReadBuildInfo Members

        string nameReadMethod;
        string nameEnumerableMethod;
        CodeExpression methodExpression;
        CodeTypeReference[] methodTypes;

        public ReadBuildInfo(string nameReadMethod,
                                string nameEnumerableMethod,
                                CodeExpression methodExpression,
                                CodeTypeReference[] methodTypes)
        {
            this.nameReadMethod = nameReadMethod;
            this.nameEnumerableMethod = nameEnumerableMethod;
            this.methodExpression = methodExpression;
            this.methodTypes = methodTypes;
        }

        #region IReadBuildInfo properties
        public string NameReadMethod
        {
            get { return this.nameReadMethod; }
            set { this.nameReadMethod = value; }
        }

        public string NameEnumerableMethod
        {
            get { return this.nameEnumerableMethod; }
            set { this.nameEnumerableMethod = value; }
        }

        public CodeExpression MethodExpression
        {
            get { return this.methodExpression; }
            set { this.methodExpression = value; }
        }

        public CodeTypeReference[] MethodTypes
        {
            get { return this.MethodTypes; }
            set { this.methodTypes = value; }
        }
        #endregion // IReadBuildInfo properties

        #endregion
    } // Ends struct ReadBuildInfo

    internal interface IReadExpressionBuilder
    {
        CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args);
        //CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args);
    }

    internal class ParamCodeTypeHolderMarshalOut : ParamCodeTypeHolderMarshalBase, IReadBuildInfo
    {
        private CodeExpression typerefexprMethod = new CodeTypeReferenceExpression("TODO.Foo.Out"); //null;
        //private CodeTypeReferenceExpression typerefexprMethod = new CodeTypeReferenceExpression("TODO.Foo"); //null;
        private string nameReadMethod = "TodoMethodOut"; // null;
        private string nameEnumerableMethod = null;
        private IReadExpressionBuilder exprbuilder = null; // Can be overwritten by sub-classes. Not sure if that's a good idea.
        private CodeTypeReference[] methodTypes = null;

        //internal string NameReadMethod { get { return this.nameReadMethod; } }
        //internal CodeExpression MethodExpression { get { return this.typerefexprMethod; } }
        //protected CodeTypeReference[] MethodTypes { get { return this.methodTypes; } }
        public string NameReadMethod { get { return this.nameReadMethod; } }
        public virtual string NameEnumerableMethod { get { return this.nameEnumerableMethod; } }
        public CodeExpression MethodExpression { get { return this.typerefexprMethod; } }
        public CodeTypeReference[] MethodTypes { get { return this.methodTypes; } }

        public ParamCodeTypeHolderMarshalOut(CodeTypeFactory codetypeFactory)
            : base (codetypeFactory, FieldDirection.Out)
        {
        }

        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            base.HandleCodeParamType(paramtype);
        }

        protected virtual CodeExpression HandleBuildReadExpression(params CodeExpression[] args)
        {
            CodeMethodInvokeExpression invokeResult = new CodeMethodInvokeExpression(this.typerefexprMethod, this.nameReadMethod, args);

            if (this.methodTypes != null)
            {
                invokeResult.Method.TypeArguments.AddRange(this.methodTypes);
            }
            return invokeResult;
        }

        public virtual CodeExpression BuildReadExpression(params CodeExpression[] args)
        {
            CodeExpression result;
            if (this.exprbuilder == null)
            {
                result = this.HandleBuildReadExpression(args);
            }
            else
            {
                result = this.exprbuilder.Build(this, args);
            }
            return result;
        }

        //public CodeExpression BuildReadExpression(CodeVariableReferenceExpression varrefReader, params CodeExpression[] args)
        //{
        //    // TODO - work out the method for reading out.
        //    CodeExpression[] argsWithReader = args;
        //    if (false)
        //    {
        //        argsWithReader = new CodeExpression[args.Length + 1];
        //        args.CopyTo(argsWithReader, 1);
        //        argsWithReader[0] = varrefReader;
        //    }
        //    return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("TODO.Foo"), "TodoMethod", argsWithReader);
        //}

        private CodeTypeReferenceExpression MessageReaderTypeRef { get { return CodeBuilderCommon.typerefexprMessageReader; } }

        #region Additional type handling
        // WAXME
        //public virtual void HandleStruct(string nameStruct)
        //{
        //    this.typerefexprMethod = null; // Calling static method on this class.
        //    this.nameReadMethod = "Read" + nameStruct + "Value";
        //}

        #endregion // Ends Additional type handling

        #region Udbus.Parsing.IParamCodeTypeHandler functions
        #region Type Handling
        public override void HandleByte()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadByte" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerByte";
            base.HandleByte();
        }

        public override void HandleBoolean()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadBoolean" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerBoolean";
            base.HandleBoolean();
        }

        public override void HandleInt16()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadInt16" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerInt16";
            base.HandleInt16();
        }

        public override void HandleUInt16()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadUInt16" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerUInt16";
            base.HandleUInt16();
        }

        public override void HandleInt32()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadInt32" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerInt32";
            base.HandleInt32();
        }

        public override void HandleUInt32()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadUInt32" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerUInt32";
            base.HandleUInt32();
        }

        public override void HandleInt64()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadInt64" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerInt64";
            base.HandleInt64();
        }

        public override void HandleUInt64()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadUInt64" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerUInt64";
            base.HandleUInt64();
        }

        public override void HandleDouble()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadDouble" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerDouble";
            base.HandleDouble();
        }

        public override void HandleString()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadString" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerString";
            base.HandleString();
        }

        public override void HandleVariant()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadVariant" + CodeBuilderCommon.ReadSuffix;
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerVariant";
            base.HandleVariant();
        }

        public override void HandleObjectPath()
        {
            this.typerefexprMethod = this.MessageReaderTypeRef;
            this.nameReadMethod = "ReadObjectPath" + CodeBuilderCommon.ReadSuffix; // ?
            // Sadly we use arrays not enumerables. We could fix this up at least for single depth arrays...
            this.nameEnumerableMethod = "ArrayMarshallerObjectPath"; // ?
            base.HandleObjectPath();
        }

        public override void HandleSignature()
        {
            base.HandleSignature();
            throw new System.NotImplementedException("Can't read a signature");
        }

        /// <summary>
        /// Struct expression builder for out arguments.
        /// </summary>
        private class StructExpressionBuilder : IReadExpressionBuilder
        {
            private string nameStruct;
            private string nameScope;
            private CodeTypeReference typerefStruct;

            internal StructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
            {
                this.typerefStruct = typerefStruct;
                this.nameStruct = nameStruct;
                this.nameScope = nameScope;
            }

            #region IReadExpressionBuilder functions
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                //result = reader.MarshalEnumerable<TrickyDictParam.NestedI3>(
                //    Udbus.Core.UdbusMessageReader.StructMarshaller<TrickyDictParam.NestedI3>(ReadTrickyDictNestedI3),
                //    out valueTemp.Field4
                //);
                //CodeExpression[] argsRemainder = new CodeExpression[args.Length - 1];
                //for (int nArgs = 1; nArgs < args.Length; ++nArgs)
                //{
                //    argsRemainder[nArgs - 1] = args[nArgs];
                //}
                //result = builder.MarshalStruct(param.Field2, BodyAdd_TrickyDictNested);

                //CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader, "StructMarshaller",
                //    new CodeMethodReferenceExpression(exprMethod, name)
                //);
                //methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                //CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalEnumerableOut(methodStructMarshaller, args);
                // TODO - sort out namespace of referenced method.
                CodeMethodReferenceExpression methodMarshalStruct = new CodeMethodReferenceExpression(info.MethodExpression, info.NameReadMethod);
                CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalStructOut(methodMarshalStruct, args);

                return exprMarshal;
            }
            #endregion // Ends IReadExpressionBuilder functions

        } // Ends class StructExpressionBuilder

        protected virtual IReadExpressionBuilder CreateStructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
        {
            return new StructExpressionBuilder(typerefStruct, nameStruct, nameScope);
        }

        public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
        {
            this.typerefexprMethod = new CodeTypeReferenceExpression(nameScope);
            this.nameReadMethod = "Read" + nameStruct + CodeBuilderCommon.ReadSuffix;
            //this.nameEnumerableMethod = "EnumerableMarshallerStruct";
            this.nameEnumerableMethod = "ArrayMarshallerStruct";

            SetExpressionBuilder(this, new StructExpressionBuilder(paramtype.CodeType, nameStruct, nameScope));
            base.HandleStruct(paramtype, nameStruct, nameScope);
        }

        /// <summary>
        /// Dictionary expression builder for out arguments.
        /// </summary>
        private class DictionaryExpressionBuilder : IReadExpressionBuilder
        {
            private string nameDictionary;
            private CodeTypeReference typerefDictionary;
            private string nameScope;

            // TODO Sort out that namespacing - update Build to use the appropriate namespace and generate the rest from the sictionary name.
            internal DictionaryExpressionBuilder(CodeTypeReference typerefDictionary, string nameDictionary, string nameScope)
            {
                this.typerefDictionary = typerefDictionary;
                this.nameDictionary = nameDictionary;
                this.nameScope = nameScope;
            }

            #region IReadExpressionBuilder functions
            //public CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args)
            public CodeExpression Build(IReadBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodMarshalDictionary = new CodeMethodReferenceExpression(info.MethodExpression, info.NameReadMethod);

                if (info.MethodTypes != null && info.MethodTypes.Length > 0)
                {
                    methodMarshalDictionary.TypeArguments.AddRange(info.MethodTypes);

                }

                CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(methodMarshalDictionary, args);

                return exprMarshal;
            }
            #endregion // Ends IReadExpressionBuilder functions

        } // Ends class DictionaryExpressionBuilder

        /// <summary>
        /// Handle a discovered dictionary type.
        /// </summary>
        /// <param name="paramtype">The dictionary that's been discovered.</param>
        /// <param name="nameDictionary">The plain name of the discovered dictionary.</param>
        /// <param name="nameScope">Scope to create new code in (i.e. dictionary handling functions).</param>
        public override void HandleDictionary(Udbus.Parsing.ICodeParamType paramtype, string nameDictionary, string nameScope)
        {
            this.typerefexprMethod = new CodeTypeReferenceExpression(nameScope);
            this.nameReadMethod = "Read" + nameDictionary + CodeBuilderCommon.DictionaryFunctionSuffix;
            // Since our out parameters are arrays, we have to marshal arrays.
            //this.nameEnumerableMethod = "EnumerableMarshallerDictionary";
            this.nameEnumerableMethod = "ArrayMarshallerDictionary";

            SetExpressionBuilder(this, new DictionaryExpressionBuilder(paramtype.CodeType, nameDictionary, nameScope));
#if !INHERIT_FROM_INTERFACE
            base.HandleDictionary(paramtype, nameDictionary, nameScope);
#endif // !INHERIT_FROM_INTERFACE
        }

        /// <summary>
        /// Implementation for handling sub-array.
        /// Not particularly pretty way of doing things, but at least it's explicit.
        /// </summary>
        /// <param name="typerefexprMethod">Type on which to call method.</param>
        /// <param name="nameReadMethod">Method name.</param>
        protected virtual void HandleSubArray(CodeExpression typerefexprMethod, string nameReadMethod)
        {
            this.typerefexprMethod = typerefexprMethod;
            this.nameReadMethod = nameReadMethod;
        }

        // Well this is new. We don't call a base function because a) there isn't one, and b) we're an intermediary placeholding type,
        // and the base class functions tend to be something more concrete and dbus-y,
        // plus we have yet to workout what the concrete type we're actually going to be marshalling is.
        public virtual void HandleSubArray()
        {
            this.HandleSubArray(this.MessageReaderTypeRef, CodeBuilderCommon.nameDefaultEnumerableMarshallerOut);
            // Since our out parameters are arrays, we have to marshal arrays.
            //this.nameEnumerableMethod = "EnumerableMarshallerArray";
            this.nameEnumerableMethod = "ArrayMarshallerArray";
        }
        #endregion // Ends Type Handling

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
        {
            return this.CodeTypeFactory.CreateStructCreator(this.CodeTypeFactory, this);// new StructCreatorMarshalOut(this);
        }

        public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
        {
            return this.CodeTypeFactory.CreateDictCreatorOut(this);//new DictCreatorMarshalOut(this);
        }

        protected ArrayParamCodeTypeHolderMarshalOut CreateArrayHandlerImpl()
        {
            return new ArrayParamCodeTypeHolderMarshalOut(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            return this.CreateArrayHandlerImpl();
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateArrayHandler()
        {
            return this.CreateNewArrayHandler();
        }

        internal bool QuExpressionBuilder { get { return this.exprbuilder != null; } }
        static protected void SetExpressionBuilder(ParamCodeTypeHolderMarshalOut owner, IReadExpressionBuilder exprbuilder)
        {
            owner.exprbuilder = exprbuilder;
        }

        static string StringConverter<T>(T t) { return t.ToString(); }
        /// <summary>
        /// Set the reference type for the marshalling method.
        /// </summary>
        /// <param name="typeMethod">Type being marshalled</param>
        static protected void SetMethodType(ParamCodeTypeHolderMarshalOut owner, CodeTypeReference typeMethod)
        {
            if (owner.methodTypes != null)
            {
                string[] foo = { "a", "b" };
                string[] bar = { "c", "d" };
                List<string> description = new List<string>(new string[] {"SetMethodType already has method types. Existing types:"});
                description.AddRange(System.Array.ConvertAll(owner.MethodTypes, new System.Converter<CodeTypeReference, string>(StringConverter)));
                description.Add("New type:");
                description.Add(StringConverter(typeMethod));
                throw new dbusidltocode.RuntimeGeneratorException(string.Join(" ", description.ToArray()));
            }

            owner.methodTypes = new CodeTypeReference[] { typeMethod };
        }
        #endregion // Ends Udbus.Parsing.IParamCodeTypeHandler functions

        /// <summary>
        /// Remove reference type for the marshalling method.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="typeMethod"></param>
        static protected void ClearMethodType(ParamCodeTypeHolderMarshalOut owner)
        {
            owner.methodTypes = null;
        }
    } // Ends class ParamCodeTypeHolderMarshalOut
    #endregion // Ends Out Param Handler
} // Ends namespace dbusidltocode.marshal.outward
