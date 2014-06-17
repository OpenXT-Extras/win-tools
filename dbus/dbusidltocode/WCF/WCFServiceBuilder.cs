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
using System.CodeDom.Compiler;

namespace dbusidltocode
{
    /// <summary>
    /// WCF Service implementation which holds a registry of DbusServiceKey => DbusService.
    /// Therefore containing assembly will need reference to DbusService assembly.
    /// </summary>
    class WCFServiceBuilder : Builder
    {
        protected const string wcfserviceParamsArgName = "wcfserviceParams";
        protected const string dbusServices = "dbusServices";
        protected static readonly CodePropertyReferenceExpression proprefWCFServiceParams = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "WCFServiceParams");
        static readonly CodeFieldReferenceExpression fieldrefDbusServices = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dbusServices);
        protected static readonly CodeVariableReferenceExpression varrefTarget = new CodeVariableReferenceExpression(CodeBuilderCommon.targetName);
        protected static readonly CodeMethodReturnStatement returnTarget = new CodeMethodReturnStatement(varrefTarget);
        protected static readonly CodeMethodInvokeExpression invokeGetWCFMethodTarget = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.GetWCFMethodTarget);
        protected static readonly CodeParameterDeclarationExpression paramWCFServiceParams = new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefWCFServiceParams, wcfserviceParamsArgName);
        protected static readonly CodeArgumentReferenceExpression argrefWCFSericeParams = new CodeArgumentReferenceExpression(wcfserviceParamsArgName);
        protected static readonly CodeAttributeArgument attribargAddressFilterModePrefix = new CodeAttributeArgument("AddressFilterMode"
                , new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(System.ServiceModel.AddressFilterMode)), "Prefix")
        );
        //[System.ServiceModel.ServiceBehavior(AddressFilterMode=System.ServiceModel.AddressFilterMode.Prefix])]
        protected static readonly CodeAttributeDeclaration attribServiceBehavior = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ServiceModel.ServiceBehaviorAttribute)),
            new CodeAttributeArgument("AddressFilterMode"
                , new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(System.ServiceModel.AddressFilterMode)), "Prefix")
            )
        );
        internal const string callbacks = "callbacks";
        internal const string sender = "sender";
        internal const string eventArgsParamName = "e";
        internal const string addToHost = "addToHost";
        internal const string AddCallback = "AddCallback";
        internal const string callbackEnumerator = "callbackEnumerator";
        internal const string callbackEnumeratorMoveNext = "callbackEnumeratorMoveNext";
        internal readonly static CodeTypeReference typerefObject = new CodeTypeReference(typeof(object));
        internal readonly static CodeParameterDeclarationExpression paramdeclSender = new CodeParameterDeclarationExpression(typerefObject, sender);
        internal readonly static CodeFieldReferenceExpression fieldrefCallbacks = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), callbacks);
        internal readonly static CodeVariableDeclarationStatement vardeclAddToHost = new CodeVariableDeclarationStatement(typeof(bool), addToHost,
            new CodeBinaryOperatorExpression(fieldrefCallbacks, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
        );
        internal readonly static CodeVariableReferenceExpression varrefAddToHost = new CodeVariableReferenceExpression(addToHost);
        internal readonly static CodePropertyReferenceExpression proprefOperationContextCurrent = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(System.ServiceModel.OperationContext)), "Current");

        internal const string RemoveCallback = "RemoveCallback";
        
        private void GenerateMethod(IDLInterface idlIntf, IDLMethod idlMethod
                                   , Udbus.Parsing.ICodeTypeDeclarationHolder contextDeclarationHolder
                                   , CodeTypeReference typerefDbusInterface
                                   , CodeTypeReference typerefDbusMarshal
                                   , CodeTypeDeclaration typeProxy)
        {
            // Straight-forward interface method.
            CodeMemberMethod methodInterface = new CodeMemberMethod();
            CodeExpressionCollection interfaceCallArgs = new CodeExpressionCollection();
            methodInterface.Name = idlMethod.Name;
            methodInterface.Attributes = MemberAttributes.Public;
            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(contextDeclarationHolder);

            #region Methods args
            foreach (IDLMethodArgument idlMethodArg in idlMethod.Arguments)
            {
                CodeCommentStatement commentMethod = new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlMethodArg.Direction, idlMethodArg.Name, idlMethodArg.Type));
                methodInterface.Comments.Add(commentMethod);
                // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
                Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLMethodArgumentTypeNameBuilder(idlIntf, idlMethod);
                ParamCodeTypeFactory paramtypeHolder = new ParamCodeTypeFactory(CodeTypeFactory.Default,
                                                                              idlMethodArg.Direction == "out" ? FieldDirection.Out : FieldDirection.In);
                Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolder, nameBuilder, idlMethodArg.Type, context);
                Udbus.Parsing.ICodeParamType paramtype = paramtypeHolder.paramtype;

                // Arguments.
                CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(paramtype.CodeType, idlMethodArg.Name);
                CodeVariableReferenceExpression varrefMethodArg = new CodeVariableReferenceExpression(idlMethodArg.Name);

                if (idlMethodArg.Direction == "out")
                {
                    // Add to interface parameters.
                    interfaceCallArgs.Add(new CodeDirectionExpression(FieldDirection.Out, varrefMethodArg));
                    // Add parameter to interface method.
                    param.Direction = FieldDirection.Out;
                } else {
                    interfaceCallArgs.Add(varrefMethodArg);
                }
                methodInterface.Parameters.Add(param);
               
            } // Ends loop over method arguments
            #endregion

            methodInterface.Statements.Add(this.DeclareTargetVariable(typerefDbusInterface, typerefDbusMarshal));
            methodInterface.Statements.Add(new CodeMethodInvokeExpression(varrefTarget, idlMethod.Name, interfaceCallArgs.Cast<CodeExpression>().ToArray()));

            //methodInterface.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(thisProxyFieldRef, idlMethod.Name)
            //    , interfaceCallArgs.Cast<CodeExpression>().ToArray()
            //));

            // Finish up.
            typeProxy.Members.Add(methodInterface);
        }

        private void GenerateProperty(IDLInterface idlIntf, IDLProperty idlProperty
                                     , CodeTypeReference typerefDbusInterface
                                     , CodeTypeReference typerefDbusMarshal
                                     , CodeTypeDeclaration typeProxy)
        {
            bool hasGet = CodeBuilderCommon.HasGet(idlProperty);
            bool hasSet = CodeBuilderCommon.HasSet(idlProperty);

            if (hasGet || hasSet)
            {
                CodeMemberProperty property = new CodeMemberProperty();
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                property.Name = CodeBuilderCommon.GetCompilableName(idlProperty.Name);
                property.HasGet = hasGet;
                property.HasSet = hasSet;
                property.Type = CodeBuilderCommon.PropertyType(CodeTypeFactory.DefaultProperty, idlProperty.Type);

                property.Comments.Add(new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlProperty.Access, idlProperty.Name, idlProperty.Type)));
                // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.

                CodeVariableDeclarationStatement vardeclTarget = this.DeclareTargetVariable(typerefDbusInterface, typerefDbusMarshal);
                CodePropertyReferenceExpression proprefTargetProperty = new CodePropertyReferenceExpression(varrefTarget, property.Name);
                if (hasGet)
                {
                    property.GetStatements.Add(vardeclTarget);
                    property.GetStatements.Add(
                        new CodeMethodReturnStatement(
                            proprefTargetProperty
                        )
                    );
                }

                if (hasSet)
                {
                    property.SetStatements.Add(vardeclTarget);
                    property.SetStatements.Add(
                        new CodeAssignStatement(proprefTargetProperty
                            , new CodePropertySetValueReferenceExpression()
                        )
                    );
                }

                // Finish up.
                typeProxy.Members.Add(property);
            }
        }

        private void GenerateSignals(IDLInterface idlIntf, string nsName
                                    , CodeTypeReference typerefDbusInterface
                                    , CodeTypeReference typerefDbusMarshal
                                    , CodeTypeReference typerefWCFCallbackInterface
                                    , CodeTypeDeclaration typeProxy)
        {
            #region list of callbacks
            // Callbacks field.
            // * LinkedList<I<interface>Callback> callbacks = null;
            CodeTypeReference typerefCallbacksLinkedList = new CodeTypeReference(typeof(LinkedList<>));
            typerefCallbacksLinkedList.TypeArguments.Add(typerefWCFCallbackInterface);
            CodeMemberField fieldCallbacks = new CodeMemberField(typerefCallbacksLinkedList, callbacks);
            fieldCallbacks.InitExpression = new CodePrimitiveExpression(null);
            typeProxy.Members.Add(fieldCallbacks);
            #endregion

            #region Method: OnCallBackClosing
            // Write: 
            //   private void OnCallbackClosing(object sender, System.EventArgs e)
            //   {
            //        RemoveCallback(sender, this.callbacks);
            //   }
            CodeMemberMethod methodOnCallbackClosing = new CodeMemberMethod();
            methodOnCallbackClosing.Attributes = MemberAttributes.Private;
            methodOnCallbackClosing.Name = "OnCallbackClosing";
            CodeParameterDeclarationExpression paramdeclSender = new CodeParameterDeclarationExpression(typerefObject, "sender");
            CodeArgumentReferenceExpression argrefSender = new CodeArgumentReferenceExpression(paramdeclSender.Name);
            CodeParameterDeclarationExpression paramdeclEventArgs = new CodeParameterDeclarationExpression(typeof(System.EventArgs), "e");
            CodeArgumentReferenceExpression argrefEventArgs = new CodeArgumentReferenceExpression(paramdeclEventArgs.Name);
            methodOnCallbackClosing.Parameters.Add(paramdeclSender);
            methodOnCallbackClosing.Parameters.Add(paramdeclEventArgs);
            methodOnCallbackClosing.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, RemoveCallback, argrefSender, fieldrefCallbacks)));
            typeProxy.Members.Add(methodOnCallbackClosing);
            #endregion

            #region Method: AddCallBack
            // AddCallback().
            // * private void AddCallback(I<interface>Callback callback)
            CodeParameterDeclarationExpression paramdeclCallback = new CodeParameterDeclarationExpression(typerefWCFCallbackInterface, CodeBuilderCommon.callback);
            CodeArgumentReferenceExpression argrefCallback = new CodeArgumentReferenceExpression(paramdeclCallback.Name);
            CodeMemberMethod methodAddCallback = new CodeMemberMethod();
            methodAddCallback.Attributes = MemberAttributes.Private;
            methodAddCallback.Name = AddCallback;
            methodAddCallback.Parameters.Add(paramdeclCallback);
            CodeArrayCreateExpression arraycreatexprCallbacks = new CodeArrayCreateExpression(typerefWCFCallbackInterface);
            arraycreatexprCallbacks.Initializers.Add(argrefCallback);
            CodeVariableReferenceExpression varrefRegisterComms = new CodeVariableReferenceExpression("registerComms");
            // * bool registerComms = true;
            methodAddCallback.Statements.Add(new CodeVariableDeclarationStatement(typeof(bool)
                                                                                 , varrefRegisterComms.VariableName
                                                                                 , new CodePrimitiveExpression(true)));

            CodeTypeReference typerefCallbackEnumerator = new CodeTypeReference(typeof(System.Collections.Generic.IEnumerator<>));
            typerefCallbackEnumerator.TypeArguments.Add(typerefWCFCallbackInterface);
            CodeVariableReferenceExpression varrefCallbackEnumerator = new CodeVariableReferenceExpression(callbackEnumerator);
            CodeVariableDeclarationStatement vardeclCallbackEnumerator = new CodeVariableDeclarationStatement(typerefCallbackEnumerator, varrefCallbackEnumerator.VariableName,
                new CodeMethodInvokeExpression(fieldrefCallbacks, "GetEnumerator"));
            CodePropertyReferenceExpression proprefCallbackEnumeratorCurrent = new CodePropertyReferenceExpression(varrefCallbackEnumerator, "Current");
            CodeMethodInvokeExpression invokeCallbackEnumeratorMoveNext = new CodeMethodInvokeExpression(varrefCallbackEnumerator, "MoveNext");
            CodeVariableDeclarationStatement vardeclCallbackEnumeratorMoveNext = new CodeVariableDeclarationStatement(typeof(bool), callbackEnumeratorMoveNext, invokeCallbackEnumeratorMoveNext);
            CodeVariableReferenceExpression varrefCallbackEnumeratorMoveNext = new CodeVariableReferenceExpression(vardeclCallbackEnumeratorMoveNext.Name);
            CodeAssignStatement assignCallbackEnumeratorMoveNext = new CodeAssignStatement(varrefCallbackEnumeratorMoveNext, invokeCallbackEnumeratorMoveNext);

            methodAddCallback.Statements.Add(
                // * if (this.callbacks == null)
                new CodeConditionStatement(new CodeBinaryOperatorExpression(fieldrefCallbacks
                                                                           , CodeBinaryOperatorType.IdentityEquality
                                                                           , new CodePrimitiveExpression(null))
                                          , new CodeStatement[]
                    {
                        // * this.callbacks = new LinkedList<I<interface>Callback>(new I<interface>Callback[] { callback });
                        new CodeAssignStatement(fieldrefCallbacks
                                               , new CodeObjectCreateExpression(typerefCallbacksLinkedList
                                                                               , arraycreatexprCallbacks)
                                               )
                    }
                // * else
                                          , new CodeStatement[]
                    {
                        // * IEnumerator<[callback_interface]> callbacksEnumerator = this.callbacks.GetEnumerator();
                        vardeclCallbackEnumerator
                        // * for (bool callbackEnumeratorMoveNext = callbackEnumerator.MoveNext(); callbackEnumeratorMoveNext; callbackEnumeratorMoveNext = callbackEnumerator.MoveNext())
                        // * {
                        , new CodeIterationStatement(vardeclCallbackEnumeratorMoveNext
                                                    , new CodeBinaryOperatorExpression(varrefCallbackEnumeratorMoveNext
                                                                                      , CodeBinaryOperatorType.BooleanAnd
                                                                                      , varrefRegisterComms)
                                                    , assignCallbackEnumeratorMoveNext
                            // * if (callbacksEnumerator.Current.Equals(callback))
                            // * {
                                                    , new CodeConditionStatement(new CodeMethodInvokeExpression(proprefCallbackEnumeratorCurrent
                                                                                                               , "Equals"
                                                                                                               , argrefCallback)
                                                                                , new CodeStatement[] // True
                            {
                                // * registerComms = false;
                                new CodeAssignStatement(varrefRegisterComms, new CodePrimitiveExpression(false))
                            }
                                                                                 )
                                                    )
                        // * if (registerComms)
                        , new CodeConditionStatement(varrefRegisterComms
                            // * this.callbacks.AddLast(callback);
                                                    , new CodeExpressionStatement(new CodeMethodInvokeExpression(fieldrefCallbacks
                                                                                                                , "AddLast"
                                                                                                                , argrefCallback))
                                                    )
                    }
            ));

            // * System.ServiceModel.ICommunicationObject commsCallback = callback as System.ServiceModel.ICommunicationObject;

            // * if (registerComms && (typeof(System.ServiceModel.ICommunicationObject).IsInstanceOfType(callback) != false))
            // * {
            // * commsCallback.Closing += this.OnCallbackClosing;
            // * }
            CodeTypeReference typerefICommunicationObject = new CodeTypeReference(typeof(System.ServiceModel.ICommunicationObject));
            CodeVariableDeclarationStatement vardeclCallbackAsComms = new CodeVariableDeclarationStatement(typerefICommunicationObject
                , "commsCallback"
                , new CodeCastExpression(typerefICommunicationObject, argrefCallback)
            );
            CodeVariableReferenceExpression varrefCallbackAsComms = new CodeVariableReferenceExpression(vardeclCallbackAsComms.Name);
            methodAddCallback.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(
                varrefRegisterComms
                , CodeBinaryOperatorType.BooleanAnd
                , new CodeBinaryOperatorExpression(
                new CodeMethodInvokeExpression(new CodeTypeOfExpression(typerefICommunicationObject), "IsInstanceOfType", argrefCallback)
                    , CodeBinaryOperatorType.IdentityInequality
                    , new CodePrimitiveExpression(false)
                )
            )
            , vardeclCallbackAsComms
            , new CodeAttachEventStatement(varrefCallbackAsComms, "Closing", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), methodOnCallbackClosing.Name))
            ));
            typeProxy.Members.Add(methodAddCallback);
            #endregion

            #region Method: RemoveCallBack
            // RemoveCallback().
            /// * static private void RemoveCallback<T>(object callback, LinkedList<T> callbacks)
            /// *     where T: class
            /// * {
            CodeMemberMethod methodRemoveCallback = new CodeMemberMethod();
            methodRemoveCallback.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            methodRemoveCallback.Name = RemoveCallback;
            CodeTypeParameter typeparamCallback = new CodeTypeParameter("TCallback");
            CodeTypeReference typerefCallbackParam = new CodeTypeReference(typeparamCallback.Name);
            CodeParameterDeclarationExpression paramdeclRemoveCallback = new CodeParameterDeclarationExpression(typerefObject, CodeBuilderCommon.callback);
            CodeTypeReference typerefLinkedListRemoveCallback = new CodeTypeReference(typeof(System.Collections.Generic.LinkedList<>));
            typerefLinkedListRemoveCallback.TypeArguments.Add(typerefCallbackParam);
            CodeTypeReference typerefLinkedListNodeRemoveCallback = new CodeTypeReference(typeof(System.Collections.Generic.LinkedListNode<>));
            typerefLinkedListNodeRemoveCallback.TypeArguments.Add(typerefCallbackParam);

            CodeParameterDeclarationExpression paramdeclRemoveCallbacks = new CodeParameterDeclarationExpression(typerefLinkedListRemoveCallback, callbacks);
            CodeArgumentReferenceExpression argrefCallbacks = new CodeArgumentReferenceExpression(paramdeclRemoveCallbacks.Name);
            typeparamCallback.Constraints.Add(new CodeTypeReference(" class")); // Space before "class" bloody important, stupid CodeDOM and CSharp.
            methodRemoveCallback.TypeParameters.Add(typeparamCallback);
            methodRemoveCallback.Parameters.Add(paramdeclRemoveCallback);
            methodRemoveCallback.Parameters.Add(paramdeclRemoveCallbacks);

            CodeVariableDeclarationStatement vardeclIteratorNode = new CodeVariableDeclarationStatement(
                typerefLinkedListNodeRemoveCallback, "iteratorNode"
                , new CodePropertyReferenceExpression(argrefCallbacks, "First")
            );
            CodeVariableReferenceExpression varrefIteratorNode = new CodeVariableReferenceExpression(vardeclIteratorNode.Name);
            CodeVariableDeclarationStatement vardeclRemoveNode = new CodeVariableDeclarationStatement(
                typerefLinkedListNodeRemoveCallback, "removeNode"
                , varrefIteratorNode
            );
            CodeVariableReferenceExpression varrefRemoveNode = new CodeVariableReferenceExpression(vardeclRemoveNode.Name);
            CodeAssignStatement assignIteratorNode = new CodeAssignStatement(varrefIteratorNode, new CodePropertyReferenceExpression(varrefIteratorNode, "Next"));

            // * for (LinkedListNode<string> n = l.First; n != null; )
            // * {
            methodRemoveCallback.Statements.Add(new CodeIterationStatement(vardeclIteratorNode
                                                                          , new CodeBinaryOperatorExpression(varrefIteratorNode
                                                                                                            , CodeBinaryOperatorType.IdentityInequality
                                                                                                            , new CodePrimitiveExpression(null))
                                                                          , new CodeSnippetStatement(string.Empty) // Don't increment
                // * if (iteratorNode.Value == callback)
                // * {
                                                                          , new CodeConditionStatement(
                                                                                 new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(varrefIteratorNode, "Value")
                                                                                                                 , CodeBinaryOperatorType.IdentityEquality
                                                                                                                 , argrefCallback)
                , new CodeStatement[] // True
                        {
                                // * LinkedListNode<string> removeNode = iteratorNode;
                          vardeclRemoveNode
                                // * iteratorNode = iteratorNode.Next;
                        , assignIteratorNode
                                // * l.Remove(removeNode);
                        , new CodeExpressionStatement (new CodeMethodInvokeExpression(argrefCallbacks, "Remove", varrefRemoveNode))
                            // * }
                        }
                // * else
                // * {
                , new CodeStatement[] // False
                        {
                                // * iteratorNode = iteratorNode.Next;
                          assignIteratorNode
                            // * }
                        }
                // * }
                // * }
            )));
            typeProxy.Members.Add(methodRemoveCallback);
            #endregion

            foreach (IDLSignal idlSignal in idlIntf.Signals)
            {
                #region Write method which will call every registered
                // Add private event handler function which delegates to callback interfaces.
                // * private void On<signal>(object sender, <signal_interface_ns>.<signal>Args e)
                string signalArgsName = CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name);
                string scopedSignalArgsName = CodeBuilderCommon.GetScopedName(nsName, signalArgsName);
                string signalEventMethodName = CodeBuilderCommon.GetSignalCallbackMethodName(idlSignal.Name);
                CodeTypeReference typerefSignalEventArgs = new CodeTypeReference(scopedSignalArgsName);
                CodeParameterDeclarationExpression paramdeclSignalEventArgs = new CodeParameterDeclarationExpression(typerefSignalEventArgs, eventArgsParamName);
                CodeArgumentReferenceExpression argrefSignalEventArgs = new CodeArgumentReferenceExpression(paramdeclSignalEventArgs.Name);
                CodeMemberMethod methodOnSignal = new CodeMemberMethod();

                methodOnSignal.Attributes = MemberAttributes.Private;
                methodOnSignal.Name = signalEventMethodName;
                methodOnSignal.Parameters.Add(paramdeclSender);
                methodOnSignal.Parameters.Add(paramdeclSignalEventArgs);
                methodOnSignal.Statements.Add(new CodeConditionStatement(
                    // * if (this.callbacks != null)
                    new CodeBinaryOperatorExpression(fieldrefCallbacks, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null))
                    // * IEnumerator< <interface>Callback > callbackEnumerator = this.callbacks.GetEnumerator();
                    , vardeclCallbackEnumerator
                    // * for (bool callbackEnumeratorMoveNext = callbackEnumerator.MoveNext(); callbackEnumeratorMoveNext; callbackEnumeratorMoveNext = callbackEnumerator.MoveNext())
                    , new CodeIterationStatement(vardeclCallbackEnumeratorMoveNext, varrefCallbackEnumeratorMoveNext, assignCallbackEnumeratorMoveNext
                        , new CodeExpressionStatement(new CodeMethodInvokeExpression(proprefCallbackEnumeratorCurrent, signalEventMethodName, argrefSignalEventArgs))
                    //// * callbackEnumerator.Current.On<signal>(e);
                    //, new CodeExpressionStatement(new CodeMethodInvokeExpression(arrayindexCallbacks, signalEventMethodName, argrefSignalEventArgs))
                )));
                typeProxy.Members.Add(methodOnSignal);
                #endregion

                #region Write method to register for a signal
                // * public void RegisterFor<signal>()
                CodeMethodReferenceExpression methodrefGetCallbackChannel = new CodeMethodReferenceExpression(proprefOperationContextCurrent, "GetCallbackChannel", typerefWCFCallbackInterface);
                CodeMethodReferenceExpression methodrefOnSignal = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), methodOnSignal.Name);
                CodeMemberMethod methodRegisterForSignal = new CodeMemberMethod();
                methodRegisterForSignal.Attributes = MemberAttributes.Public;
                methodRegisterForSignal.Name = CodeBuilderCommon.GetSignalRegisterFunction(idlSignal.Name);
                // * <interface> target = this.GetWCFMethodTarget();
                methodRegisterForSignal.Statements.Add(this.DeclareTargetVariable(typerefDbusInterface, typerefDbusMarshal));
                // * AddCallback(System.ServiceModel.OperationContext.Current.GetCallbackChannel<I<interface>Callback>());
                methodRegisterForSignal.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodAddCallback.Name
                    , new CodeMethodInvokeExpression(methodrefGetCallbackChannel))
                ));
                CodePropertyReferenceExpression proprefDbusServiceSignal = new CodePropertyReferenceExpression(fieldrefDbusServices, CodeBuilderCommon.GetSignalEventPropertyName(idlSignal.Name));
                CodeAttachEventStatement attacheventSignal = new CodeAttachEventStatement(new CodeEventReferenceExpression(varrefTarget, CodeBuilderCommon.GetSignalEventPropertyName(idlSignal.Name)), methodrefOnSignal);
                methodRegisterForSignal.Statements.Add(attacheventSignal);

                typeProxy.Members.Add(methodRegisterForSignal);
                #endregion

            } // Ends loop over signals
        }

        string nsName, ifName;
        string nsServiceName, service;
        string nsWCFInterfaceName, wcfinterface;

        string genInterfaceName;
        string scopedGenInterfaceName;

        string scopedWCFInterface;
        string dbusservice;
        string scopedDbusServiceName;
        string callbackInterfaceName;
        string scopedCallbackInterfaceName;

        CodeTypeReference typerefDbusInterface;
        CodeTypeReference typerefWCFInterface;
        CodeTypeReference typerefDbusMarshal;
        CodeTypeReferenceExpression typerefexprDbusMarshal;
        CodeTypeReference typerefWCFCallbackInterface;

        CodeMemberMethod methodGetWCFMethodTarget;

        CodeTypeIgnoredNamespaceDeclarationHolderParams contextDeclarationHolder;
        Udbus.Parsing.ICodeTypeDeclarationHolder declarationHolder;
        bool bAddNamespace;

        public void InitializeInterface(IDLInterface idlIntf)
        {
            this.bAddNamespace = false;

            this.ifName = CodeBuilderCommon.GetName(idlIntf.Name, null);
            this.nsName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new InterfaceVisitor());

            this.service = CodeBuilderCommon.GetName(idlIntf.Name, null);
            this.nsServiceName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new WCFServiceVisitor());
            this.nsWCFInterfaceName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new WCFContractVisitor());
            this.wcfinterface = CodeBuilderCommon.GetName(idlIntf.Name, null);

            this.genInterfaceName = CodeBuilderCommon.GetName(idlIntf.Name, new InterfaceVisitor());
            this.scopedGenInterfaceName = CodeBuilderCommon.GetScopedName(nsName, genInterfaceName);

            this.scopedWCFInterface          = CodeBuilderCommon.GetScopedName(this.nsWCFInterfaceName,
                                                                               CodeBuilderCommon.GetName(idlIntf.Name, new InterfaceVisitor()));
            this.dbusservice = CodeBuilderCommon.GetName(idlIntf.Name, new DBUSServiceVisitor());
            this.scopedDbusServiceName       = CodeBuilderCommon.GetScopedName(this.nsName, this.dbusservice);
            this.callbackInterfaceName       = CodeBuilderCommon.GetSignalCallbackInterfaceName(this.wcfinterface);
            this.scopedCallbackInterfaceName = CodeBuilderCommon.GetScopedName(this.nsWCFInterfaceName, this.callbackInterfaceName);

            this.typerefDbusInterface        = new CodeTypeReference(this.scopedGenInterfaceName);
            this.typerefWCFInterface         = new CodeTypeReference(this.scopedWCFInterface);
            this.typerefDbusMarshal          = new CodeTypeReference(this.scopedDbusServiceName);
            this.typerefexprDbusMarshal      = new CodeTypeReferenceExpression(this.typerefDbusMarshal);
            this.typerefWCFCallbackInterface = new CodeTypeReference(this.scopedCallbackInterfaceName);
        }

        public override void ProcessNamespaces(IDLInterface idlIntf)
        {
            this.InitializeInterface(idlIntf); // TODO: SHOULD BE DONE IN THE MAIN GENERATE METHOD

            this.ns = new CodeNamespace(this.nsServiceName);
            CodeBuilderCommon.AddUsingNamespaces(this.ns, new WCFServiceVisitor());
        }

        public override void DeclareCodeType(IDLInterface idlIntf)
        {
            this.type = new CodeTypeDeclaration(CodeBuilderCommon.GetName(idlIntf.Name, new WCFServiceVisitor()));
            this.type.IsClass = true;
            this.type.TypeAttributes = System.Reflection.TypeAttributes.Public;

            this.BaseTypes(typerefWCFInterface, this.type);
            this.type.CustomAttributes.Add(attribServiceBehavior);

            // DbusServices field.
            this.DbusServiceTargetField(this.typerefDbusInterface, this.typerefDbusMarshal, this.type.Members);

            // Constructor.
            this.Constructor(this.type, this.typerefDbusInterface);

            // Target retrieval method.
            this.methodGetWCFMethodTarget = this.TargetRetrievalMethod(this.typerefDbusInterface, this.typerefDbusMarshal, this.typerefexprDbusMarshal);
            this.type.Members.Add(this.methodGetWCFMethodTarget);

            this.contextDeclarationHolder = new CodeTypeIgnoredNamespaceDeclarationHolderParams(idlIntf);
            this.declarationHolder = this.contextDeclarationHolder;
        }

        public override void GenerateMethods(IDLInterface idlIntf)
        {
            if (idlIntf.Methods != null && idlIntf.Methods.Count > 0) // If got methods
            {
                this.bAddNamespace = true;

                foreach (IDLMethod idlMethod in idlIntf.Methods)
                {
                    GenerateMethod(idlIntf, idlMethod
                                  , this.declarationHolder, this.typerefDbusInterface, this.typerefDbusMarshal
                                  , this.type);
                } // Ends loop over methods
            } // Ends if got methods
        }

        public override void GenerateProperties(IDLInterface idlIntf)
        {
            if (idlIntf.Properties != null && idlIntf.Properties.Count > 0) // If got properties
            {
                this.bAddNamespace = true;

                foreach (IDLProperty idlProperty in idlIntf.Properties)
                {
                    GenerateProperty(idlIntf, idlProperty
                                    , this.typerefDbusInterface, this.typerefDbusMarshal
                                    , this.type);
                } // Ends loop over properties
            } // Ends if got properties
        }

        public override void GenerateSignals(IDLInterface idlIntf)
        {
            if (idlIntf.Signals != null && idlIntf.Signals.Count > 0) // If got signals
            {
                this.bAddNamespace = true;

                GenerateSignals(idlIntf, nsName
                               , this.typerefDbusInterface, this.typerefDbusMarshal, this.typerefWCFCallbackInterface
                               , this.type);
            } // Ends if got signals
        }

        public override void PostHandleMembers(IDLInterface idlIntf)
        {
            if (this.bAddNamespace != false)
            {
                // Namespaces.
                if (this.contextDeclarationHolder.ns != null) // If created namespace for parameter types
                {
                    this.unit.Namespaces.Add(this.contextDeclarationHolder.ns);
                    this.ns.Imports.Add(new CodeNamespaceImport(this.contextDeclarationHolder.ns.Name));
                } // Ends if created namespace for parameter types
            }

            //TODO: Understand these bits before deciding what to do with them
            this.ns.Types.Add(this.type);
            //} // Ends if created namespace for parameter types
            this.unit.Namespaces.Add(this.ns);
            //END TODO
        }
        
        protected virtual CodeMemberMethod TargetRetrievalMethod(CodeTypeReference typerefDbusInterface, CodeTypeReference typerefDbusMarshal, CodeTypeReferenceExpression typerefexprDbusMarshal)
        {
            CodeMemberMethod methodGetWCFMethodTarget = CreateTargetRetrievalMethod(typerefDbusMarshal, typerefexprDbusMarshal);
            CodeMethodReferenceExpression methodrefDbusServiceCreate = new CodeMethodReferenceExpression(typerefexprDbusMarshal, "Create");
            methodGetWCFMethodTarget.Statements.Add(
                new CodeVariableDeclarationStatement(typerefDbusMarshal, CodeBuilderCommon.targetName,
                    new CodeMethodInvokeExpression(CodeBuilderCommon.methodrefGetWCFMethodTarget, // * Udbus.WCF.Service.LookupTargetFunctions.GetWCFMethodTarget(
                        proprefWCFServiceParams, // * this.WCFServiceParams,
                        fieldrefDbusServices, // * this.dbusServices,
                        methodrefDbusServiceCreate, // createService1,
                        methodrefDbusServiceCreate, // createService2,
                        new CodePropertyReferenceExpression(typerefexprDbusMarshal, CodeBuilderCommon.DefaultConnectionParameters)
                    )
                )
            );
            methodGetWCFMethodTarget.Statements.Add(returnTarget);
            return methodGetWCFMethodTarget;
        }

        protected virtual void BaseTypes(CodeTypeReference typerefWCFInterface, CodeTypeDeclaration typeProxy)
        {
            typeProxy.BaseTypes.Add(CodeBuilderCommon.typerefDbusServiceBase);
            typeProxy.BaseTypes.Add(typerefWCFInterface);
        }

        protected virtual void DbusServiceTargetField(CodeTypeReference typerefDbusInterface, CodeTypeReference typerefDbusMarshal, CodeTypeMemberCollection members)
        {
            CodeTypeReference typerefDbusServices = new CodeTypeReference(typeof(IDictionary<,>));
            typerefDbusServices.TypeArguments.Add(typeof(Udbus.WCF.Dbus.Details.Service.DbusServiceKey));
            typerefDbusServices.TypeArguments.Add(typerefDbusMarshal);
            CodeTypeReference typerefDbusServicesInit = new CodeTypeReference(typeof(Dictionary<,>));
            typerefDbusServicesInit.TypeArguments.AddRange(typerefDbusServices.TypeArguments);
            CodeMemberField fieldDbusServices = new CodeMemberField(typerefDbusServices, dbusServices);
            fieldDbusServices.InitExpression = new CodeObjectCreateExpression(typerefDbusServicesInit);
            members.Add(fieldDbusServices);
        }

        protected virtual void Constructor(CodeTypeDeclaration typeProxy, CodeTypeReference typerefDbusInterface)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(paramWCFServiceParams);
            constructor.BaseConstructorArgs.Add(argrefWCFSericeParams);
            typeProxy.Members.Add(constructor);
        }

        protected virtual CodeVariableDeclarationStatement DeclareTargetVariable(CodeTypeReference typerefDbusInterface, CodeTypeReference typerefDbusMarshal)
        {
            return new CodeVariableDeclarationStatement(typerefDbusMarshal, CodeBuilderCommon.targetName, invokeGetWCFMethodTarget);
        }

        protected static CodeMemberMethod CreateTargetRetrievalMethod(CodeTypeReference typerefReturn, CodeTypeReferenceExpression typerefexprDbusMarshal)
        {
            CodeMemberMethod methodGetWCFMethodTarget = new CodeMemberMethod();
            methodGetWCFMethodTarget.Attributes = MemberAttributes.Family | MemberAttributes.VTableMask;
            methodGetWCFMethodTarget.Name = CodeBuilderCommon.GetWCFMethodTarget;
            methodGetWCFMethodTarget.ReturnType = typerefReturn;
            return methodGetWCFMethodTarget;
        }

    } // Ends class WCFServiceBuilder
} // Ends dbusidltocode
