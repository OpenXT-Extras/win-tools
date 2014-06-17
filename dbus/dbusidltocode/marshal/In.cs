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

namespace dbusidltocode.marshal.inward
{
    #region Struct Handling
    class StructCreatorMarshalInFactory : StructCreatorFactory
    {
        internal override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructCreator(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
        {
            return new StructCreatorMarshalIn(codetypeFactory, owner);
        }
    }

    /// <summary>
    /// Creates marshalling code for input argument structs.
    /// </summary>
    class StructCreatorMarshalIn : Udbus.Parsing.StructCreatorBase
    {
        private CodeTypeFactory codetypeFactory;
        private CodeMemberMethod method = null;
        private List<CodeStatement> methodStatements = new List<CodeStatement>();

        internal CodeTypeFactory CodeTypeFactory { get { return this.codetypeFactory; } }

        public StructCreatorMarshalIn(CodeTypeFactory codetypeFactory, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base(owner)
        {
            this.codetypeFactory = codetypeFactory;
        }

        #region IStructBuilder Members
        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateNew()
        {
            //return new StructCreatorMarshalIn(this.owner);
            return this.codetypeFactory.CreateStructCreator(this.codetypeFactory, this.owner);
        }

        public override Udbus.Parsing.BuildContext CreateNestedContext(Udbus.Parsing.BuildContext context)
        {
            return this.CreateNestedContext(context, context.declarationHolder);
        }

        public override void StartStruct(string name, string fullName, Udbus.Parsing.BuildContext context)
        {
            base.StartStruct(name, fullName, context);
            System.Diagnostics.Trace.WriteLine(string.Format("InParam Starting Struct {0}", this.fullname));

            // Going to write a method for handling this struct.
            this.method = new CodeMemberMethod();
            this.method.Name = "BodyAdd_" + this.name;
            this.method.ReturnType = CodeBuilderCommon.typerefReadResult;
            this.method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            this.method.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefMessageBuilder, "builder"));
            this.method.Parameters.Add(new CodeParameterDeclarationExpression(this.fullname, CodeBuilderCommon.nameBuildValue));
        }
        public override void AddField(CodeTypeReference codeTypeField)
        {
            // No op
            System.Diagnostics.Trace.WriteLine(string.Format("InParam Adding Struct {0} Field: {1}", this.name, codeTypeField.BaseType.ToString()));
        }

        internal virtual void HandleCodeParamTypeField(Udbus.Parsing.ICodeParamType paramtype, FieldHandler fieldhandler)
        {
            // Call base class implementation.
            this.HandleCodeParamTypeField(paramtype);
            // Do extra handling.
            CodeMemberField field = this.BuildField(paramtype.CodeType);

            //result = Udbus.Core.UdbusMessageBuilder.BodyAdd_SomeStruct(builder, value.field1);
            this.methodStatements.Add(new CodeAssignStatement(CodeBuilderCommon.varrefResult,
                fieldhandler.BuildWriteExpression(CodeBuilderCommon.argrefBuilder, new CodeFieldReferenceExpression(CodeBuilderCommon.varrefBuildValue, field.Name))
            ));
            //this.method.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(CodeBuilderCommon.varrefReadValue, field.Name),
            //    fieldhandler.BuildWriteExpression(CodeBuilderCommon.argrefReader, new CodeDirectionExpression(FieldDirection.Out, CodeBuilderCommon.argrefResult))
            //));
        }

        public override void EndStruct(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.EndStruct(context, paramtypeHandler);
            //CodeStatement
            if (this.methodStatements.Count > 0) // If got field statements
            {
                this.method.Statements.Add(new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefResult, CodeBuilderCommon.nameResult));
                // Got all the field assignment statements. Now put them in the method with result tests.
                CodeExpression exprTest = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(CodeBuilderCommon.nameResult)
                    , CodeBinaryOperatorType.IdentityEquality, CodeBuilderCommon.exprReadResultSuccessValue
                );
                CodeStatement[] assignFields = CodeBuilderCommon.NestedExpression(exprTest, this.methodStatements).ToArray();
                this.method.Statements.AddRange(assignFields);
                this.method.Statements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.varrefResult));

            } // Ends if got field statements
            else // Else no fields
            {
                // No statements ? No implementation.
                // return 0;
                this.method.Statements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.exprBuildResultSuccessValue));

            } // Ends else no fields

            // Marshalling code needs the name, nothing else.
            paramtypeHandler.HandleStruct(new Udbus.Parsing.CodeParamType(new CodeTypeReference(this.fullname)),
                this.name,
                CodeBuilderCommon.GetScopedName(CodeBuilderCommon.nsDbusMarshalCustom, context.declarationHolder.Name)
            );

            System.Diagnostics.Trace.WriteLine(string.Format("InParam Ending Struct {0}", this.name));
            context.declarationHolder.Add(this.method);
        }

        internal class FieldHandler : ParamCodeTypeHolderMarshalIn
        {
            StructCreatorMarshalIn owner;

            internal FieldHandler(StructCreatorMarshalIn owner)
                : base(owner.CodeTypeFactory)
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

            #endregion // Ends IParamCodeTypeHandler Members

            internal CodeExpression BuildWriteExpression(CodeArgumentReferenceExpression codeArgumentReferenceExpression, CodeDirectionExpression codeDirectionExpression)
            {
                //throw new System.NotImplementedException();
                CodeExpression expr = base.BuildWriteExpression(codeArgumentReferenceExpression, codeDirectionExpression);
                return expr;
            }

        } // Ends class FieldHandler

        public override Udbus.Parsing.IParamCodeTypeHandler CreateFieldHandler()
        {
            return new FieldHandler(this);
        }
        #endregion // Ends IStructBuilder Members

    } // Ends class StructCreatorMarshalIn
    #endregion // Ends Struct Handling

    #region Dictionary Handling
    class DictCreatorMarshalInFactory : DictCreatorFactory
    {
        internal override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictCreator(ParamCodeTypeFactory paramCodeTypeHolder)
        {
            return new DictCreatorMarshalIn(paramCodeTypeHolder.CodeTypeFactory);
        }
    }

    class DictCreatorMarshalIn : DictCreatorIn
    {
        CodeExpression exprKey = null;
        string nameKey = null;
        ValueHandler valuehandler = null;

        public DictCreatorMarshalIn(CodeTypeFactory codetypeFactory)
            : base(codetypeFactory)
        {
        }

        internal new class ValueHandler : ParamCodeTypeHolderMarshalIn, IWriteExpressionBuilder
        {
            private class ValueArrayParamCodeTypeHolderMarshalIn : ArrayParamCodeTypeHolderMarshalIn
            {
                public ValueArrayParamCodeTypeHolderMarshalIn(ParamCodeTypeHolderMarshalIn owner)
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
                private class StructExpressionBuilder : IWriteExpressionBuilder
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

                    public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
                    {
                        CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageBuilder, "StructMarshaller",
                            new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(this.nameScope), "BodyAdd_" + this.nameStruct)
                        );
                        methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                        CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageBuilder, "ArrayMarshallerStruct",
                            methodStructMarshaller
                        );
                        return exprMarshal;
                    }
                } // Ends class StructExpressionBuilder

                /// <summary>
                /// Sub-arrays for dictionary values expression builder.
                /// </summary>
                private class ArrayExpressionInnerBuilder : IWriteExpressionBuilder
                {
                    public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
                    {
                        CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameBodyMethod, info.MethodTypes);
                        return new CodeMethodInvokeExpression(methodrefMarshal, args);
                    }
                } // Ends class ArrayExpressionInnerBuilder

                /// <summary>
                /// Same as outer ArrayParamCodeTypeHolderMarshalIn class but builds array expressions differently.
                /// i.e. acts as a class factory for array expression builder differently.
                /// </summary>
                private class NestedArrayParamCodeTypeHolderMarshalIn : ValueArrayParamCodeTypeHolderMarshalIn
                {
                    internal NestedArrayParamCodeTypeHolderMarshalIn(ArrayParamCodeTypeHolderMarshalIn owner)
                        : base(owner)
                    {
                    }
                    /// <summary>
                    /// Factory for write expression builder.
                    /// </summary>
                    /// <returns>IWriteExpressionBuilder implementation.</returns>
                    protected override IWriteExpressionBuilder CreateArrayExpressionBuilder()
                    {
                        return new ArrayExpressionInnerBuilder();
                    }

                } // Ends NestedArrayParamCodeTypeHolderMarshalIn

                protected override ArrayParamCodeTypeHolderMarshalIn CreateNestedArrayHandler()
                {
                    return new NestedArrayParamCodeTypeHolderMarshalIn(this);
                }

                /// <summary>
                /// ArrayExpressionBuilder for simple dictionary value arrays.
                /// </summary>
                private class ArrayExpressionBuilder : IWriteExpressionBuilder
                {

                    internal ArrayExpressionBuilder() { }

                    public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
                    {
                        CodeMethodReferenceExpression methodArrayValueMarshaller = new CodeMethodReferenceExpression(info.MethodExpression, info.NameBodyMethod);

                        CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageBuilder,
                            info.NameEnumerableMethod,
                            methodArrayValueMarshaller
                        );

                        return exprMarshal;
                    }
                } // Ends class ArrayExpressionBuilder

                /// <summary>
                /// Factory for write expression builder.
                /// </summary>
                /// <returns>IWriteExpressionBuilder implementation.</returns>
                protected override IWriteExpressionBuilder CreateArrayExpressionBuilder()
                {
                    IWriteExpressionBuilder writeBuilder = null;
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

                protected override IWriteExpressionBuilder CreateStructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
                {
                    return new StructExpressionBuilder(typerefStruct, nameStruct, nameScope);
                }

                protected override CodeExpression BuildMarshalEnumerableIn(CodeExpression exprArray, params CodeExpression[] args)
                {
                    return new CodeMethodInvokeExpression(this.MethodExpression, this.NameBodyMethod, exprArray);
                }

                public override void HandleSubArray()
                {
                    base.HandleSubArray(); // Do the default behaviour

                    if (this.NameBodyMethod == CodeBuilderCommon.nameDefaultEnumerableMarshallerIn) // If standard marshaller in use
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
                protected override void BuildMethodInvokeMethodType(CodeMethodInvokeExpression exprInvoke, CodeTypeReference arrayElementType)
                {
                    // Hacky question to ask, but basically if we've got a sub-array which is actually just the referenced type...
                    if (this.QuSubArray && !this.QuSubArrayIsArray) // If got sub-array which holds a value
                    {
                        base.BuildMethodInvokeMethodType(exprInvoke, arrayElementType);

                    } // Ends if got sub-array which holds a value
                }

                protected override void BuildMethodInvokeMethodTypes(CodeMethodInvokeExpression exprInvoke, CodeTypeReferenceCollection elementTypes)
                {
                    if (this.QuSubArray && !this.QuSubArrayIsArray) // If got sub-array which holds a value
                    {
                        base.BuildMethodInvokeMethodTypes(exprInvoke, elementTypes);

                    } // Ends if got sub-array which holds a value
                }

            } // Ends ValueArrayParamCodeTypeHolderMarshalIn

            public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
            {
                return new ValueArrayParamCodeTypeHolderMarshalIn(this);
            }

            // TODO Create ArrayHandler with own write method.
            DictCreatorMarshalIn owner;
            IWriteExpressionBuilder exprbuilderValue = null;

            internal new string NameBodyMethod { get { return base.NameBodyMethod; } }
            internal new CodeExpression MethodExpression { get { return base.MethodExpression; } }

            internal ValueHandler(DictCreatorMarshalIn owner)
                : base(owner.CodeTypeFactory)
            {
                this.owner = owner;
            }
            
            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeValue(paramtype, this);
            }

            public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
            {
                base.HandleStruct(paramtype, nameStruct, nameScope);
                IWriteExpressionBuilder exprbuilder = new StructExpressionBuilder(paramtype.CodeType, nameStruct, nameScope);
                this.exprbuilderValue = exprbuilder;
                SetExpressionBuilder(this, exprbuilder);
            }

            protected override CodeExpression HandleBuildWriteExpression(params CodeExpression[] args)
            {
                return new CodeMethodReferenceExpression(this.MethodExpression, this.NameBodyMethod);
            }

            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeExpression result;
                if (this.exprbuilderValue != null) // If got expression builder
                {
                    result = this.exprbuilderValue.Build(info, args);

                } // Ends if got expression builder
                else
                {
                    result = this.HandleBuildWriteExpression(args);
                }

                return result;
            }
        } // Ends class ValueHandler

        internal new class KeyHandler : ParamCodeTypeHolderMarshalIn
        {
            DictCreatorMarshalIn owner;

            internal new string NameBodyMethod { get { return base.NameBodyMethod; } }
            internal new CodeExpression MethodExpression { get { return base.MethodExpression; } }

            internal KeyHandler(DictCreatorMarshalIn owner)
                : base(owner.CodeTypeFactory)
            {
                this.owner = owner;
            }

            #region IParamCodeTypeHandler Members

            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleCodeParamTypeKey(paramtype, this);
            }

            #endregion // IParamCodeTypeHandler Members
        } // Ends class KeyHandler

        // For dictionary.
        // TODO Identical to array nested struct builder. Refactor code.
        private class StructExpressionBuilder : IWriteExpressionBuilder
        {
            private string nameStruct;
            private CodeTypeReference typerefStruct;

            internal StructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
            {
                this.typerefStruct = typerefStruct;
                this.nameStruct = nameStruct;
            }

            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageBuilder, "StructMarshaller",
                    new CodeMethodReferenceExpression(null, "BodyAdd_" + this.nameStruct)
                );
                methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                return methodStructMarshaller;
            }

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
            this.nameKey = keyhandler.NameBodyMethod;
        }

        public virtual void HandleCodeParamTypeValue(Udbus.Parsing.ICodeParamType paramtype, ValueHandler valuehandler)
        {
            this.HandleCodeParamTypeValue(paramtype);
            this.valuehandler = valuehandler;
        }


        public override void FinishDictionary(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.FinishDictionary(context, paramtypeHandler);
            CodeMemberMethod method = new CodeMemberMethod();
            
            method.Name = "BodyAdd_" + this.name + CodeBuilderCommon.DictionaryFunctionSuffix;
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefMessageBuilder, CodeBuilderCommon.nameBuilder));
            method.Parameters.Add(new CodeParameterDeclarationExpression(this.DictTypeRef, CodeBuilderCommon.nameBuildValue));
            method.ReturnType = CodeBuilderCommon.typerefResult;
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("builder"),
                    "MarshalDict",
                    new CodeArgumentReferenceExpression(CodeBuilderCommon.nameBuildValue),
                    new CodeMethodReferenceExpression(this.exprKey, this.nameKey),
                    this.valuehandler.BuildWriteExpression()
                )
            ));
            context.declarationHolder.AddDictionary(method);
            System.Diagnostics.Trace.WriteLine(string.Format("InParam Ending Dictionary {0}", this.name));

        }

    } // Ends class DictCreatorMarshal
    #endregion // Ends Dictionary Handling

    /// <summary>
    /// Stores declaration to array in parameter.
    /// </summary>
    class ArrayParamCodeTypeHolderMarshalIn : ParamCodeTypeHolderMarshalIn, IWriteExpressionBuilder
    {
        /// <summary>
        /// Same as outer ArrayParamCodeTypeHolderMarshalIn class but builds array expressions differently.
        /// i.e. acts as a class factory for array expression builder differently.
        /// </summary>
        private class NestedArrayParamCodeTypeHolderMarshalIn : ArrayParamCodeTypeHolderMarshalIn
        {
            internal NestedArrayParamCodeTypeHolderMarshalIn(ArrayParamCodeTypeHolderMarshalIn owner)
                : base(owner)
            {
            }
            /// <summary>
            /// Factory for write expression builder.
            /// </summary>
            /// <returns>IWriteExpressionBuilder implementation.</returns>
            protected override IWriteExpressionBuilder CreateArrayExpressionBuilder()
            {
                return new ArrayExpressionInnerBuilder();
            }

        } // Ends NestedArrayParamCodeTypeHolderMarshalIn

        // For array
        private class StructExpressionBuilder : IWriteExpressionBuilder
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

            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodInvokeExpression methodStructMarshaller = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageBuilder, "StructMarshaller",
                    new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(this.nameScope), "BodyAdd_" + this.nameStruct)
                );
                methodStructMarshaller.Method.TypeArguments.Add(this.typerefStruct);
                CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalEnumerableStructIn(methodStructMarshaller, args);
                return exprMarshal;
            }

        } // Ends class StructExpressionBuilder

        /// <summary>
        /// Dictionary expression builder for array in arguments.
        /// </summary>
        private class DictionaryExpressionBuilder : IWriteExpressionBuilder
        {

            // TODO Sort out that namespacing - update Build to use the appropriate namespace and generate the rest from the sictionary name.
            internal DictionaryExpressionBuilder(CodeTypeReference typerefDictionary, string nameDictionary, string nameScope)
            {}

            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodMarshalDictionary = new CodeMethodReferenceExpression(info.MethodExpression, info.NameBodyMethod);

                if (info.MethodTypes != null && info.MethodTypes.Length > 0)
                {
                    methodMarshalDictionary.TypeArguments.AddRange(info.MethodTypes);

                }

                CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalEnumerableIn(methodMarshalDictionary, args);
                return exprMarshal;
            }
        } // Ends class DictionaryExpressionBuilder

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
                CodeMethodReferenceExpression methodrefMarshal = new CodeMethodReferenceExpression(exprMethod, name);

                if (methodTypes != null)
                {
                    methodrefMarshal.TypeArguments.AddRange(methodTypes);
                }

                return methodrefMarshal;
            }
        } // Ends class ArrayExpressionCommon

        private class ArrayExpressionInnerBuilder : IWriteExpressionBuilder
        {
            /// <summary>
            /// Create an invocation of the inner method.
            /// </summary>
            /// <param name="exprMethod">Type to call method on.</param>
            /// <param name="name">Method to call.</param>
            /// <param name="methodTypes">Generic method types.</param>
            /// <param name="args">Method arguments.</param>
            /// <returns>Invocation of method with arguments.</returns>
            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameBodyMethod, info.MethodTypes);

                return new CodeMethodInvokeExpression(methodrefMarshal, args);
            }
        } // Ends class ArrayExpressionInnerBuilder
        private class ArrayExpressionOuterBuilder : IWriteExpressionBuilder
        {
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
            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                // Create the common method reference.
                CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameBodyMethod, info.MethodTypes);
                // Invoke the common method reference.
                CodeMethodInvokeExpression methodinvokeMarshalParam = new CodeMethodInvokeExpression(methodrefMarshal, args);
                
                CodeExpression[] adjustedArgs = new CodeExpression[args.Length + 2];
                args.CopyTo(adjustedArgs, 1);
                adjustedArgs[0] = info.MethodExpression;
                adjustedArgs[args.Length+1] = methodrefMarshal;
                return CodeBuilderCommon.BuildMarshalEnumerableIn(info.MethodExpression, adjustedArgs);
            }
        } // Ends class ArrayExpressionOuterBuilder

        private class ArrayExpressionBuilder : IWriteExpressionBuilder
        {
            internal ArrayExpressionBuilder() { }

            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodrefMarshal = ArrayExpressionCommon.BuildCommonImpl(info.MethodExpression, info.NameBodyMethod, info.MethodTypes);
                return CodeBuilderCommon.BuildMarshalEnumerableIn(methodrefMarshal, args);
            }
        } // Ends class ArrayExpressionBuilder

        ParamCodeTypeHolderMarshalIn owner;
        ArrayParamCodeTypeHolderMarshalIn sub = null;
        private bool bGenericRequired = false;

        public ArrayParamCodeTypeHolderMarshalIn(ParamCodeTypeHolderMarshalIn owner)
            : base (owner.CodeTypeFactory)
        {
            this.owner = owner;
        }

        protected bool QuSubArray { get { return this.sub != null; } }
        protected bool QuSubArrayIsArray { get { return this.sub != null && this.sub.sub != null; } }
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
            }
        }

        protected override IWriteExpressionBuilder CreateStructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
        {
            return new StructExpressionBuilder(typerefStruct, nameStruct, nameScope);
        }

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
        
        protected virtual ArrayParamCodeTypeHolderMarshalIn CreateNestedArrayHandler()
        {
            return new NestedArrayParamCodeTypeHolderMarshalIn(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateArrayHandler()
        {
            if (this.QuSubArray)
            {
                // Runtime exception
            }
            // This array contains an array. Bam.
            this.sub = this.CreateNestedArrayHandler();
            Udbus.Parsing.IParamCodeTypeHandler handler = this.sub;
            this.HandleSubArray();
            return handler;
        }

        /// <summary>
        /// Factory for write expression builder.
        /// </summary>
        /// <returns>IWriteExpressionBuilder implementation.</returns>
        protected virtual IWriteExpressionBuilder CreateArrayExpressionBuilder()
        {
            IWriteExpressionBuilder writeBuilder = null;
            if (this.QuSubArray) // If this is an outer array
            {
                writeBuilder = new ArrayExpressionOuterBuilder();
            }
            else
            {
                writeBuilder = new ArrayExpressionBuilder();
            }
            return writeBuilder;
        }

        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            CodeArrayCreateExpression arrayExpr = paramtype.CreateArray();
            Udbus.Parsing.CodeParamArray arraytype = new Udbus.Parsing.CodeParamArray(arrayExpr);
            base.HandleCodeParamType(arraytype);

            if (this.QuExpressionBuilder == false)
            {
                IWriteExpressionBuilder exprbuilder = this.CreateArrayExpressionBuilder();

                // Use the array expression builder.
                SetExpressionBuilder(this, exprbuilder);
            }

            // Not quite sure about this.
            // Need to set owner's expression builder first, but then how do we know level of nesting ?
            if (owner.QuExpressionBuilder == false)
            {
                // Use this Array holder as owner's expression builder
                // (which will in turn delegate to it's expression builder).
                SetExpressionBuilder(owner, this);
            }

            owner.HandleCodeParamType(arraytype);
        }

        protected virtual CodeExpression BuildMarshalEnumerableIn(CodeExpression exprArray, params CodeExpression[] args)
        {
            return CodeBuilderCommon.BuildMarshalEnumerableIn(exprArray, args);
        }

        public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
        {
            CodeExpression exprResult = null;
            if (this.QuSubArray) // If there is a sub-array
            {
                // Ignore calling parameters because it's this array object that knows what's what.
                exprResult = this.BuildArray();
                exprResult = this.BuildMarshalEnumerableIn(exprResult, args);
            } // Ends if there is a sub-array
            else
            {
                // Ignore calling parameters because it's this array object that knows what's what.
                exprResult = this.BuildWriteExpression(args);
            }
            return exprResult;
        }

        /// <summary>
        /// For the marshalling method to be invoked, create the appropriate method type (generic type).
        /// </summary>
        /// <param name="exprInvoke">Marshalling method being created.</param>
        /// <param name="arrayElementType">Array type of marshalling method.</param>
        protected virtual void BuildMethodInvokeMethodType(CodeMethodInvokeExpression exprInvoke, CodeTypeReference arrayElementType)
        {
            if (arrayElementType != null)
            {
                CodeTypeReference typerefEnumerable = CodeBuilderCommon.ArrayTypeToEnumerableType(this.sub.paramtype.CodeType.ArrayElementType);//new CodeTypeReference(typeof(IEnumerable<>).Name, new CodeTypeReference[] { sub.paramtype.CodeType.ArrayElementType });
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

            if (this.QuSubArray) // If got sub array
            {
                // We need to get the param type, dereference array rank by 1 (e.g. Int32[] => Int32) for marshal param.
                // Only at this point have all the types been established.
                // TODO - We don't really need to store this, because it's only used a few lines lower down, and nowhere else (?).
                SetMethodType(this, this.sub.paramtype.CodeType.ArrayElementType);

                // Nested array with a sub-array. Produce UdbusMessageReader.EnumerableMarshaller(<sub.BuildArray()>)
                CodeExpression exprSub = this.sub.BuildArray();
                CodeMethodInvokeExpression exprInvoke = new CodeMethodInvokeExpression(this.MethodExpression, this.sub.NameEnumerableMethod, exprSub);

                this.BuildMethodInvokeMethodTypes(exprInvoke, this.sub.OptionalGenericTypes);

                exprResult = exprInvoke;

            } // Ends if got sub array
            else // Else no sub array
            {
                // Leaf nested array. Produce the method reference for the actual type.
                exprResult = new CodeMethodReferenceExpression(this.MethodExpression, this.NameBodyMethod);

            } // Ends else no sub array

            return exprResult;
        }
    } // Ends class ArrayParamCodeTypeHolderMarshalIn

    /// <summary>
    /// Information required for build method.
    /// </summary>
    internal interface IWriteBuildInfo
    {
        string NameBodyMethod { get; }
        string NameEnumerableMethod { get; }
        CodeExpression MethodExpression { get; }
        CodeTypeReference[] MethodTypes { get; }

    } // Ends interface IWriteBuildInfo

    /// <summary>
    /// Simple struct to hold build parameters.
    /// </summary>
    struct WriteBuildInfo : IWriteBuildInfo
    {
        #region IWriteBuildInfo Members

        string nameBodyMethod;
        string nameEnumerableMethod;
        CodeExpression methodExpression;
        CodeTypeReference[] methodTypes;

        public WriteBuildInfo(  string nameBodyMethod,
                                string nameEnumerableMethod,
                                CodeExpression methodExpression,
                                CodeTypeReference[] methodTypes)
        {
            this.nameBodyMethod = nameBodyMethod;
            this.nameEnumerableMethod = nameEnumerableMethod;
            this.methodExpression = methodExpression;
            this.methodTypes = methodTypes;
        }

        #region IWriteBuildInfo properties
        public string NameBodyMethod
        {
            get { return this.nameBodyMethod; }
            set { this.nameBodyMethod = value; }
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
        #endregion // IWriteBuildInfo properties

        #endregion
    } // Ends struct WriteBuildInfo

    internal interface IWriteExpressionBuilder
    {
        CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args);
        //CodeExpression Build(CodeExpression exprMethod, string name, CodeTypeReference[] methodTypes, params CodeExpression[] args);
        //CodeExpression Build(CodeExpression exprMethod, string name, params CodeExpression[] args);
    } // Ends interface IWriteExpressionBuilder

    internal class ParamCodeTypeHolderMarshalIn : ParamCodeTypeHolderMarshalBase, IWriteBuildInfo
    {
        private string nameBodyMethod = null;
        private string nameEnumerableMethod = null;
        private CodeExpression typerefexprMethod = null;
        private IWriteExpressionBuilder exprbuilder = null;
        private CodeTypeReference[] methodTypes = null;

        public string NameBodyMethod { get { return this.nameBodyMethod; } }
        public virtual string NameEnumerableMethod { get { return this.nameEnumerableMethod; } }
        public CodeExpression MethodExpression { get { return this.typerefexprMethod; } }
        public CodeTypeReference[] MethodTypes { get { return this.methodTypes; } }

        public ParamCodeTypeHolderMarshalIn(CodeTypeFactory codetypeFactory)
            : base(codetypeFactory, FieldDirection.In)
        {
        }

        protected virtual CodeExpression HandleBuildWriteExpression(params CodeExpression[] args)
        {
            CodeMethodInvokeExpression invokeResult = new CodeMethodInvokeExpression(this.typerefexprMethod, this.nameBodyMethod, args);

            if (this.methodTypes != null)
            {
                invokeResult.Method.TypeArguments.AddRange(this.methodTypes);
            }
            return invokeResult;
        }

        public virtual CodeExpression BuildWriteExpression(params CodeExpression[] args)
        {
            CodeExpression result;
            if (this.exprbuilder == null)
            {
                result = this.HandleBuildWriteExpression(args);
            }
            else
            {
                result = this.exprbuilder.Build(this, args);
            }
            return result;
        }

        private CodeTypeReferenceExpression MessageBuilderTypeRef { get { return CodeBuilderCommon.typerefexprMessageBuilder; } }

        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            base.HandleCodeParamType(paramtype);
        }

        #region Type Handling
        public override void HandleByte()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Byte";
            this.nameEnumerableMethod = "EnumerableMarshallerByte";
            base.HandleByte();
        }

        public override void HandleBoolean()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Boolean";
            this.nameEnumerableMethod = "EnumerableMarshallerBoolean";
            base.HandleBoolean();
        }

        public override void HandleInt16()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Int16";
            this.nameEnumerableMethod = "EnumerableMarshallerInt16";
            base.HandleInt16();
        }

        public override void HandleUInt16()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_UInt16";
            this.nameEnumerableMethod = "EnumerableMarshallerUInt16";
            base.HandleUInt16();
        }

        public override void HandleInt32()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Int32";
            this.nameEnumerableMethod = "EnumerableMarshallerInt32";
            base.HandleInt32();
        }

        public override void HandleUInt32()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_UInt32";
            this.nameEnumerableMethod = "EnumerableMarshallerUInt32";
            base.HandleUInt32();
        }

        public override void HandleInt64()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Int64";
            this.nameEnumerableMethod = "EnumerableMarshallerInt64";
            base.HandleInt64();
        }

        public override void HandleUInt64()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_UInt64";
            this.nameEnumerableMethod = "EnumerableMarshallerUInt64";
            base.HandleUInt64();
        }

        public override void HandleDouble()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Double";
            this.nameEnumerableMethod = "EnumerableMarshallerDouble";
            base.HandleDouble();
        }

        public override void HandleString()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_String";
            this.nameEnumerableMethod = "EnumerableMarshallerString";
            base.HandleString();
        }

        public override void HandleVariant()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_Variant";
            this.nameEnumerableMethod = "EnumerableMarshallerVariant";
            base.HandleVariant();
        }

        public override void HandleObjectPath()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_ObjectPath";
            this.nameEnumerableMethod = "EnumerableMarshallerObjectPath";
            base.HandleObjectPath();
        }

        public override void HandleSignature()
        {
            this.typerefexprMethod = this.MessageBuilderTypeRef;
            this.nameBodyMethod = "BodyAdd_String";
            this.nameEnumerableMethod = "EnumerableMarshallerString";
            base.HandleSignature();
        }

        /// <summary>
        /// Struct expression builder for in arguments.
        /// </summary>
        private class StructExpressionBuilder : IWriteExpressionBuilder
        {
            internal StructExpressionBuilder()
            { }

            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodMarshalStruct = new CodeMethodReferenceExpression(info.MethodExpression, info.NameBodyMethod);
                CodeMethodInvokeExpression exprMarshal = CodeBuilderCommon.BuildMarshalStructIn(methodMarshalStruct, args);

                return exprMarshal;
            }
        } // Ends class StructExpressionBuilder

        protected virtual IWriteExpressionBuilder CreateStructExpressionBuilder(CodeTypeReference typerefStruct, string nameStruct, string nameScope)
        {
            return new StructExpressionBuilder();
        }

        /// <summary>
        /// Handle a discovered struct type.
        /// </summary>
        /// <param name="paramtype">The struct that's been discovered.</param>
        /// <param name="nameStruct">The plain name of the discovered struct.</param>
        /// <param name="nameScope">Scope to create new code in (i.e. struct handling functions).</param>
        public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
        {
            this.typerefexprMethod = new CodeTypeReferenceExpression(nameScope); // TODO Not ideal, but not sure how to get at this otherwise.
            this.nameBodyMethod = "BodyAdd_" + nameStruct;
            this.nameEnumerableMethod = "EnumerableMarshallerStruct";

            SetExpressionBuilder(this, this.CreateStructExpressionBuilder(paramtype.CodeType, nameStruct, nameScope));
            base.HandleStruct(paramtype, nameStruct, nameScope);
        }

        /// <summary>
        /// Dictionary expression builder for in arguments.
        /// </summary>
        private class DictionaryExpressionBuilder : IWriteExpressionBuilder
        {
            public CodeExpression Build(IWriteBuildInfo info, params CodeExpression[] args)
            {
                CodeMethodReferenceExpression methodMarshalDictionary = new CodeMethodReferenceExpression(info.MethodExpression, info.NameBodyMethod);

                if (info.MethodTypes != null && info.MethodTypes.Length > 0)
                {
                    methodMarshalDictionary.TypeArguments.AddRange(info.MethodTypes);
                }

                CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(methodMarshalDictionary, args);
                return exprMarshal;
            }

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
            this.nameBodyMethod = "BodyAdd_" + nameDictionary + CodeBuilderCommon.DictionaryFunctionSuffix;
            this.nameEnumerableMethod = "EnumerableMarshallerDictionary";
            SetExpressionBuilder(this, new DictionaryExpressionBuilder());
            base.HandleDictionary(paramtype, nameDictionary, nameScope);
        }

        /// <summary>
        /// Implementation for handling sub-array.
        /// Not particularly pretty way of doing things, but at least it's explicit.
        /// </summary>
        /// <param name="typerefexprMethod">Type on which to call method.</param>
        /// <param name="nameBodyMethod">Method name.</param>
        protected virtual void HandleSubArray(CodeExpression typerefexprMethod, string nameBodyMethod)
        {
            this.typerefexprMethod = typerefexprMethod;
            this.nameBodyMethod = nameBodyMethod;
        }

        // Well this is new. We don't call a base function because a) there isn't one, and b) we're an intermediary placeholding type,
        // and the base class functions tend to be something more concrete and dbus-y,
        // plus we have yet to workout what the concrete type we're actually going to be marshalling is.
        public virtual void HandleSubArray()
        {
            this.HandleSubArray(this.MessageBuilderTypeRef, CodeBuilderCommon.nameDefaultEnumerableMarshallerIn);
            this.nameEnumerableMethod = "EnumerableMarshallerArray";
        }

        #endregion // Ends Type Handling

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
        {
            return this.CodeTypeFactory.CreateStructCreator(this.CodeTypeFactory, this);
        }

        public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
        {
            return this.CodeTypeFactory.CreateDictCreatorIn(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            return new ArrayParamCodeTypeHolderMarshalIn(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateArrayHandler()
        {
            return this.CreateNewArrayHandler();
        }

        internal bool QuExpressionBuilder { get { return this.exprbuilder != null; } }
        static protected void SetExpressionBuilder(ParamCodeTypeHolderMarshalIn owner, IWriteExpressionBuilder exprbuilder)
        {
            owner.exprbuilder = exprbuilder;
        }

        /// <summary>
        /// Set the reference type for the marshalling method.
        /// </summary>
        /// <param name="typeMethod">Type being marshalled</param>
        static protected void SetMethodType(ParamCodeTypeHolderMarshalIn owner, CodeTypeReference typeMethod)
        {
            if (owner.methodTypes != null)
            {
                // Runtime error.
            }

            owner.methodTypes = new CodeTypeReference[] { typeMethod };
        }

        /// <summary>
        /// Remove reference type for the marshalling method.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="typeMethod"></param>
        static protected void ClearMethodType(ParamCodeTypeHolderMarshalIn owner)
        {
            owner.methodTypes = null;
        }
    } // Ends class ParamCodeTypeHolderMarshalIn
} // Ends namespace dbusidltocode.marshal.inward
