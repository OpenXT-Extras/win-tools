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
using System.IO;

using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using System.ComponentModel;

namespace dbusidltocode
{
    internal class ProxyBuilder : Builder
    {
        const string proxyName = "proxy";
        const string dataName = "data";
        const string resultName = "result";
        string name;
        string genInterfaceName;
        bool bAddNamespace;
        CodeAssignStatement assignProxy;
        CodeTypeDeferredNamespaceDeclarationHolderEvents eventsDeclarationHolder;
        CodeParameterDeclarationExpression paramProxy;
        Udbus.Parsing.ICodeTypeDeclarationHolder contextDeclarationHolder;
        CodeTypeIgnoredNamespaceDeclarationHolderParams declarationHolder;
        CodeFieldReferenceExpression thisProxyFieldRef;
        CodeTypeDeclaration typeProxy;
                
        public override void ProcessNamespaces(IDLInterface idlIntf)
        {
            name = CodeBuilderCommon.GetName(idlIntf.Name, null);
            string nsName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new InterfaceVisitor());
            genInterfaceName = CodeBuilderCommon.GetName(idlIntf.Name, new InterfaceVisitor());

            // Namespace.
            ns = new CodeNamespace(nsName + ".Proxy");
            CodeBuilderCommon.AddUsingNamespaces(ns, new ProxyVisitor());
        }

        public override void DeclareCodeType(IDLInterface idlIntf)
        {
            // Proxy class.
            typeProxy = new CodeTypeDeclaration(name + "Proxy");
            typeProxy.IsClass = true;
            typeProxy.TypeAttributes = TypeAttributes.Public;
            eventsDeclarationHolder = new CodeTypeDeferredNamespaceDeclarationHolderEvents(idlIntf);
            typeProxy.BaseTypes.Add(genInterfaceName);

            // Interface field.
            CodeMemberField memberProxy = new CodeMemberField(genInterfaceName, proxyName);
            memberProxy.Attributes = MemberAttributes.Private;
            typeProxy.Members.Add(memberProxy); // TODO: Going to need a using or a fully qualified name.

            // Constructor.
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            // TODO - use the actual interface type rather than a string.
            paramProxy = new CodeParameterDeclarationExpression(genInterfaceName, proxyName);
            constructor.Parameters.Add(paramProxy);
            thisProxyFieldRef = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), proxyName
            );
            assignProxy = new CodeAssignStatement(thisProxyFieldRef,
                new CodeArgumentReferenceExpression(proxyName));
            constructor.Statements.Add(assignProxy);
            typeProxy.Members.Add(constructor);

            declarationHolder = new CodeTypeIgnoredNamespaceDeclarationHolderParams(idlIntf);
            contextDeclarationHolder = declarationHolder;

            bAddNamespace = false;
        }

        public override void GenerateMethods(IDLInterface idlIntf)
        {
            if (idlIntf.Methods != null) // If got methods
            {
                bAddNamespace = idlIntf.Methods.Count > 0;

                foreach (IDLMethod idlMethod in idlIntf.Methods)
                {
                    // Structs for capturing input/output from method.
                    Udbus.Parsing.CodeTypeDeferredStructHolder paramsHolder = new Udbus.Parsing.CodeTypeDeferredStructHolder(idlMethod.Name + "AsyncParams");
                    Udbus.Parsing.CodeTypeDeferredStructHolder resultHolder = new Udbus.Parsing.CodeTypeDeferredStructHolder(idlMethod.Name + "AsyncResult");
                    CodeConstructor constructorResults = new CodeConstructor();
                    CodeConstructor constructorParams = new CodeConstructor();
                    constructorResults.Attributes = constructorParams.Attributes = MemberAttributes.Public;

                    // Create Call method.
                    CodeMemberMethod methodCall = new CodeMemberMethod();
                    methodCall.Name = "Call" + idlMethod.Name;
                    // Actual call to proxy arguments will be added to as we parse IDL arguments.
                    CodeExpressionCollection callParameters = new CodeExpressionCollection();
                    CodeExpressionCollection callResultArgs = new CodeExpressionCollection();
                    CodeExpressionCollection interfaceCallArgs = new CodeExpressionCollection();
                    CodeArgumentReferenceExpression argrefCallData = new CodeArgumentReferenceExpression(dataName);

                    // Event Args
                    string eventName = idlMethod.Name + "EventArgs";
                    CodeTypeDeclaration typeEvent = new CodeTypeDeclaration(eventName);
                    typeEvent.IsClass = true;
                    typeEvent.TypeAttributes = TypeAttributes.Public;
                    typeEvent.BaseTypes.Add(new CodeTypeReference(typeof(AsyncCompletedEventArgs)));
                    CodeConstructor constructorEventArgs = new CodeConstructor();
                    eventsDeclarationHolder.Add(typeEvent);
                    CodeExpressionCollection makeEventArgs = new CodeExpressionCollection();

                    // Event Raiser...
                    string raiserName = "raiser" + idlMethod.Name;
                    CodeTypeReference raiserRef = new CodeTypeReference("AsyncOperationEventRaiser", new CodeTypeReference(eventName));
                    CodeMemberField fieldRaiser = new CodeMemberField(raiserRef, raiserName);
                    fieldRaiser.InitExpression = new CodeObjectCreateExpression(raiserRef);
                    fieldRaiser.Attributes = MemberAttributes.Private;
                    typeProxy.Members.Add(fieldRaiser);

                    // Event raiser EventCompleted Property - unfortunately CodeMemberEvent is useless here since can't add custom add/remove.
                    // So anything other than CSharp is now a bust. Brilliant.
                    CodeSnippetTypeMember eventRaiser = new CodeSnippetTypeMember();
                    eventRaiser.Text = string.Format(@"        public event System.EventHandler<{0}> {1}Completed
        {{
            add {{ this.{2}.CompletedEvent += value; }}
            remove {{ this.{2}.CompletedEvent -= value; }}
        }}
", eventName, idlMethod.Name, raiserName
                        ); // Adding text here. 

                    //CodeMemberEvent eventRaiser = new CodeMemberEvent();
                    //eventRaiser.Attributes = MemberAttributes.Public;
                    //CodeParamDeclaredType declaredType = new CodeParamDeclaredType(typeEvent);
                    //eventRaiser.Type = new CodeTypeReference(CodeBuilderCommon.EventHandlerType.Name, declaredType.CodeType);
                    //eventRaiser.Name = idlMethod.Name + "Completed";

                    typeProxy.Members.Add(eventRaiser);

                    // Async method.
                    CodeMemberMethod methodAsync = new CodeMemberMethod();
                    methodAsync.Name = idlMethod.Name + "Async";
                    methodAsync.Attributes = MemberAttributes.Public;

                    // Straight-forward interface method.
                    CodeMemberMethod methodInterface = new CodeMemberMethod();
                    methodInterface.Name = idlMethod.Name;
                    methodInterface.Attributes = MemberAttributes.Public;

                    //        CodeComment commentMethod = new CodeComment(idlMethod.Name);
                    //        method.Comments.Add(new CodeCommentStatement(idlMethod.Name));
                    CodeExpressionCollection asyncArgs = new CodeExpressionCollection();
                    Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(contextDeclarationHolder);

                    foreach (IDLMethodArgument idlMethodArg in idlMethod.Arguments)
                    {
                        CodeCommentStatement commentMethod = new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlMethodArg.Direction, idlMethodArg.Name, idlMethodArg.Type));
                        methodAsync.Comments.Add(commentMethod);
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

                        if (idlMethodArg.Direction == "out") // If out argument
                        {
                            // Add to interface parameters.
                            interfaceCallArgs.Add(new CodeDirectionExpression(FieldDirection.Out, varrefMethodArg));

                            // Put into result holding struct...
                            param.Direction = FieldDirection.Out;
                            methodInterface.Parameters.Add(param);

                            CodeTypeReference argType = CodeBuilderCommon.GetReadOnlyCodeReference(paramtypeHolder.paramtype.CodeType);
                            CodeMemberField fieldResultArg = new CodeMemberField(argType, idlMethodArg.Name);
                            fieldResultArg.Attributes = MemberAttributes.Public;
                            resultHolder.CodeType.Members.Add(fieldResultArg);

                            constructorResults.Parameters.Add(new CodeParameterDeclarationExpression(paramtype.CodeType, idlMethodArg.Name));
                            CodeFieldReferenceExpression thisArg = new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), idlMethodArg.Name
                            );
                            constructorResults.Statements.Add(new CodeAssignStatement(thisArg,
                                new CodeArgumentReferenceExpression(idlMethodArg.Name)));

                            // Add placeholder variables to call method.
                            methodCall.Statements.Add(new CodeVariableDeclarationStatement(paramtype.CodeType, idlMethodArg.Name));

                            // Add appropriate parameter in actual call to interface.
                            callParameters.Add(new CodeDirectionExpression(FieldDirection.Out, varrefMethodArg));

                            // Add appropriate parameter in call to result constructor.
                            callResultArgs.Add(new CodeArgumentReferenceExpression(idlMethodArg.Name));

                            // Put into event args class...
                            string fieldEventArgName = idlMethodArg.Name + "_";
                            CodeMemberField fieldEventArg = new CodeMemberField(paramtype.CodeType, fieldEventArgName);
                            fieldEventArg.Attributes = MemberAttributes.Private;
                            typeEvent.Members.Add(fieldEventArg);

                            // Add argument property to EventArgs.
                            CodeMemberProperty propEventArg = new CodeMemberProperty();
                            propEventArg.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                            propEventArg.Name = idlMethodArg.Name;
                            propEventArg.HasGet = true;
                            propEventArg.Type = paramtype.CodeType;
                            propEventArg.GetStatements.Add(new CodeMethodInvokeExpression(null, "RaiseExceptionIfNecessary"));
                            propEventArg.GetStatements.Add(new CodeMethodReturnStatement(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), fieldEventArgName)));
                            typeEvent.Members.Add(propEventArg);

                            // Add constructor parameter and statement to EventArgs.
                            constructorEventArgs.Parameters.Add(new CodeParameterDeclarationExpression(paramtype.CodeType, idlMethodArg.Name));
                            constructorEventArgs.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldEventArgName),
                                new CodeArgumentReferenceExpression(idlMethodArg.Name)
                            ));

                            // Add as parameter in call to EventArgs constructor.
                            makeEventArgs.Add(new CodeFieldReferenceExpression(new CodeArgumentReferenceExpression(resultName), idlMethodArg.Name));

                        } // Ends if out argument
                        else // Else in argument
                        {
                            // Add to interface parameters.
                            interfaceCallArgs.Add(varrefMethodArg);
                            // Put into parameter holding struct.
                            CodeMemberField fieldArg = new CodeMemberField(CodeBuilderCommon.GetReadOnlyCodeReference(paramtype.CodeType), idlMethodArg.Name);
                            fieldArg.Attributes = MemberAttributes.Public;
                            paramsHolder.CodeType.Members.Add(fieldArg);

                            constructorParams.Parameters.Add(new CodeParameterDeclarationExpression(paramtype.CodeType, idlMethodArg.Name));
                            CodeFieldReferenceExpression thisArg = new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), idlMethodArg.Name
                            );
                            constructorParams.Statements.Add(new CodeAssignStatement(thisArg,
                                new CodeArgumentReferenceExpression(idlMethodArg.Name)));

                            // Add appropriate parameter in actual call to interface.
                            callParameters.Add(new CodeFieldReferenceExpression(argrefCallData, idlMethodArg.Name));

                            // Add appropriate parameter to async method parameters constructor call.
                            asyncArgs.Add(new CodeArgumentReferenceExpression(idlMethodArg.Name));

                            // Add parameter to async method.
                            methodAsync.Parameters.Add(param);
                            // Add parameter to interface method.
                            methodInterface.Parameters.Add(param);

                        } // Ends else in argument

                    } // Ends loop over method arguments

                    methodAsync.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "userState"));

                    // Add method body, then sort out arguments code...
                    CodeFieldReferenceExpression thisRaiser = new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), raiserName
                    );
                    CodeTypeReference typerefResult = null;
                    if (idlMethod.Return != null) // If method has return type
                    {
                        // Add result to Result type.
                        // TODO replace typeof(string) with actual return type.
                        typerefResult = new CodeTypeReference("???");
                    }

                    CodeTypeReference typerefResults = null;
                    CodeTypeReference typerefParams = null;
                    CodeExpression exprAsyncParamValue = null;
                    string asyncMethodName = "AsyncFuncImpl"; // Default is assuming that function returns something.

                    if (paramsHolder.QuType()) // If got params type
                    {
                        typerefParams = new CodeTypeReference(paramsHolder.CodeType.Name);

                        // Add proxy field to params struct.
                        CodeMemberField memberParamsProxy = new CodeMemberField(new CodeTypeReference("readonly " + genInterfaceName), proxyName);
                        memberParamsProxy.Attributes = MemberAttributes.Public;
                        paramsHolder.CodeType.Members.Insert(0, memberParamsProxy); // TODO: Going to need a using or a fully qualified name.

                        // Add initialisation to constructor
                        constructorParams.Parameters.Insert(0, paramProxy);
                        // Constructor will take proxy as first argument.
                        constructorParams.Statements.Insert(0, assignProxy);


                        paramsHolder.CodeType.TypeAttributes = TypeAttributes.NestedPrivate;
                        paramsHolder.CodeType.IsStruct = true;
                        constructorParams.Attributes = MemberAttributes.Public;
                        paramsHolder.CodeType.Members.Add(constructorParams);
                        typeProxy.Members.Add(paramsHolder.CodeType);

                        asyncArgs.Insert(0, thisProxyFieldRef);
                        exprAsyncParamValue = new CodeObjectCreateExpression(typerefParams, asyncArgs.Cast<CodeExpression>().ToArray());
                    } // Ends if got params type

                    if (resultHolder.QuType()) // If got results type
                    {
                        typerefResults = new CodeTypeReference(resultHolder.CodeType.Name);

                        methodCall.ReturnType = typerefResults;

                        // Setup call method parameters.
                        if (idlMethod.Return != null)
                        {
                            // Add result field to EventArgs.
                            CodeMemberField fieldResult = new CodeMemberField(typerefResult, resultName);
                            fieldResult.Attributes = MemberAttributes.Private;
                            typeEvent.Members.Insert(0, fieldResult);

                            // Add result property to EventArgs.
                            CodeMemberProperty propResult = new CodeMemberProperty();
                            propResult.Attributes = MemberAttributes.Public;
                            propResult.Name = resultName;
                            propResult.HasGet = true;
                            propResult.Type = typerefResult;
                            propResult.GetStatements.Add(new CodeMethodInvokeExpression(null, "RaiseExceptionIfNecessary"));
                            propResult.GetStatements.Add(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), resultName));
                            typeEvent.Members.Add(propResult);

                            // Add suitable parameter as first EventArgs constructor argument.
                            constructorEventArgs.Parameters.Insert(0, new CodeParameterDeclarationExpression(typerefResult, resultName));
                            // Add statement to initialise result member.
                            CodeFieldReferenceExpression thisArg = new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), resultName
                            );

                            // Include result in parameters to EventArgs in MakeEventArgs statements.
                            makeEventArgs.Insert(0, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), resultName));

                            // Add result to Result type.
                            CodeTypeReference typerefReadOnlyResult = CodeBuilderCommon.GetReadOnlyCodeReference(typerefResult);
                            CodeMemberField fieldReadOnlyResult = new CodeMemberField(typerefReadOnlyResult, resultName);
                            fieldResult.Attributes = MemberAttributes.Public;
                            resultHolder.CodeType.Members.Insert(0, fieldReadOnlyResult);

                            // Add suitable parameter as first constructor argument.
                            constructorResults.Parameters.Insert(0, new CodeParameterDeclarationExpression(typerefResult, resultName));
                            // Add statement to initialise result member.
                            constructorResults.Statements.Insert(0, new CodeAssignStatement(thisArg,
                                new CodeArgumentReferenceExpression(resultName)));

                        } // End if there is a return statement

                        resultHolder.CodeType.TypeAttributes = TypeAttributes.NestedPrivate;
                        resultHolder.CodeType.IsStruct = true;

                        resultHolder.CodeType.Members.Add(constructorResults);
                        typeProxy.Members.Add(resultHolder.CodeType);

                    } // Ends if got results type
                    else // Else no results type
                    {
                        // Absolutely no results (including out parameters) means it's an Action.
                        asyncMethodName = "AsyncActionImpl";

                    } // Ends else no results type

                    // Time to bite the bullet.
                    CodeTypeReference typerefEvent = new CodeTypeReference(typeEvent.Name);
                    CodeMemberMethod methodMakeEventArgs = new CodeMemberMethod();
                    methodMakeEventArgs.Name = "Make" + idlMethod.Name + "AsyncEventArgs";

                    // Add missing parameters to EventArgs...
                    constructorEventArgs.Parameters.Add(CodeBuilderCommon.paramdeclException);
                    constructorEventArgs.Parameters.Add(CodeBuilderCommon.paramdeclCancelled);
                    constructorEventArgs.Parameters.Add(CodeBuilderCommon.paramdeclUserState);
                    // Pass through to base class.
                    constructorEventArgs.BaseConstructorArgs.Add(CodeBuilderCommon.argrefException);
                    constructorEventArgs.BaseConstructorArgs.Add(CodeBuilderCommon.argrefCancelled);
                    constructorEventArgs.BaseConstructorArgs.Add(CodeBuilderCommon.argrefUserState);

                    // Add MakeEventArgs method...
                    // Create MakeEventArgs method.
                    methodMakeEventArgs.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                    if (typerefParams != null) // If got params type
                    {
                        methodMakeEventArgs.Parameters.Add(new CodeParameterDeclarationExpression(typerefParams, dataName));
                    }
                    else // Else no params type
                    {
                        // The interface will be passed as the first parameter.
                        methodMakeEventArgs.Parameters.Add(paramProxy);

                    } // Ends else no params type

                    if (typerefResults != null) // If got results type
                    {
                        methodMakeEventArgs.Parameters.Add(new CodeParameterDeclarationExpression(typerefResults, resultName));
                    }
                    methodMakeEventArgs.Parameters.Add(CodeBuilderCommon.paramdeclException);
                    methodMakeEventArgs.Parameters.Add(CodeBuilderCommon.paramdeclCancelled);
                    methodMakeEventArgs.Parameters.Add(CodeBuilderCommon.paramdeclUserState);
                    makeEventArgs.Add(CodeBuilderCommon.argrefException);
                    makeEventArgs.Add(CodeBuilderCommon.argrefCancelled);
                    makeEventArgs.Add(CodeBuilderCommon.argrefUserState);
                    methodMakeEventArgs.Statements.Add(new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(
                            typerefEvent, makeEventArgs.Cast<CodeExpression>().ToArray()
                        )
                    ));
                    methodMakeEventArgs.ReturnType = typerefEvent;
                    typeProxy.Members.Add(methodMakeEventArgs);

                    // Setup call method parameters.
                    methodCall.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                    CodeExpression exprProxyRef;
                    //methodCall.ReturnType = new CodeTypeReference(resultHolder.CodeType.Name);
                    if (typerefParams != null) // If passing parameters
                    {
                        methodCall.Parameters.Add(new CodeParameterDeclarationExpression(typerefParams, dataName));
                        exprProxyRef = new CodeFieldReferenceExpression(argrefCallData, proxyName);

                    } // Ends if passing parameters
                    else // Else just passing proxy
                    {
                        methodCall.Parameters.Add(new CodeParameterDeclarationExpression(genInterfaceName, proxyName));
                        exprProxyRef = new CodeArgumentReferenceExpression(proxyName);

                    } // Ends else just passing proxy

                    // Add call method body...
                    // Invocation to proxy.
                    CodeMethodReferenceExpression methodProxy = new CodeMethodReferenceExpression(exprProxyRef, idlMethod.Name);
                    CodeMethodInvokeExpression invokeProxy = new CodeMethodInvokeExpression(methodProxy, callParameters.Cast<CodeExpression>().ToArray());
                    if (idlMethod.Return != null) // If got return type
                    {
                        // TODO - return values.
                        // Add invocation of actual interface method and store result.
                        CodeVariableDeclarationStatement exprResult = new CodeVariableDeclarationStatement(typerefResult, resultName, invokeProxy);
                        methodCall.Statements.Add(exprResult);

                        // Initialise result parameter in Result struct.
                        callResultArgs.Insert(0, new CodeArgumentReferenceExpression(resultName));

                    } // Ends if got return type
                    else // Else no return type
                    {
                        // Add invocation of actual interface method and ignore result.
                        methodCall.Statements.Add(invokeProxy);

                    } // Ends else no return type

                    if (typerefResults != null) // If got result type
                    {
                        // Create Call return statement.
                        CodeMethodReturnStatement retCall = new CodeMethodReturnStatement(
                            new CodeObjectCreateExpression(typerefResults, callResultArgs.Cast<CodeExpression>().ToArray())
                        );
                        methodCall.Statements.Add(retCall);

                    } // Ends if got result type

                    // Add call method to type.
                    typeProxy.Members.Add(methodCall);

                    // Add async method implementation...
                    if (exprAsyncParamValue == null) // If no params
                    {
                        // Just pass the proxy.
                        exprAsyncParamValue = thisProxyFieldRef;

                    } // Ends if no params

                    methodAsync.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(thisRaiser, asyncMethodName)
                        , new CodeArgumentReferenceExpression("userState")
                        , exprAsyncParamValue
                        , new CodeMethodReferenceExpression(null, methodCall.Name)
                        , new CodeMethodReferenceExpression(null, methodMakeEventArgs.Name)
                    ));

                    methodInterface.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(thisProxyFieldRef, idlMethod.Name)
                        , interfaceCallArgs.Cast<CodeExpression>().ToArray()
                    ));
                    // Finish up.
                    typeEvent.Members.Add(constructorEventArgs);
                    constructorEventArgs.Attributes = MemberAttributes.Public;
                    ns.Types.Add(typeEvent);
                    typeProxy.Members.Add(methodAsync);
                    typeProxy.Members.Add(methodInterface);

                } // Ends loop over methods
            } // Ends if got methods
        }

        public override void GenerateProperties(IDLInterface idlIntf)
        {
            if (idlIntf.Properties != null && idlIntf.Properties.Count > 0) // If got properties
            {
                foreach (IDLProperty idlProperty in idlIntf.Properties)
                {
                    bool hasGet = CodeBuilderCommon.HasGet(idlProperty);
                    bool hasSet = CodeBuilderCommon.HasSet(idlProperty);

                    if (!hasGet && !hasSet)
                    {
                        continue;
                    }

                    CodeMemberProperty property = new CodeMemberProperty();
                    property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    property.Name = CodeBuilderCommon.GetCompilableName(idlProperty.Name);
                    property.HasGet = hasGet;
                    property.HasSet = hasSet;
                    property.Type = CodeBuilderCommon.PropertyType(CodeTypeFactory.Default, idlProperty.Type);

                    property.Comments.Add(new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlProperty.Access, idlProperty.Name, idlProperty.Type)));
                    // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.

                    CodePropertyReferenceExpression proprefTargetProperty = new CodePropertyReferenceExpression(thisProxyFieldRef, property.Name);
                    if (hasGet)
                    {
                        property.GetStatements.Add(
                            new CodeMethodReturnStatement(
                                proprefTargetProperty
                            )
                        );
                    }

                    if (hasSet)
                    {
                        property.SetStatements.Add(
                            new CodeAssignStatement(proprefTargetProperty
                                , new CodePropertySetValueReferenceExpression()
                            )
                        );
                    }

                    // Finish up.
                    typeProxy.Members.Add(property);
                } // Ends loop over properties
            } // Ends if got properties
        }

        public override void GenerateSignals(IDLInterface idlIntf)
        {
            if (idlIntf.Signals != null && idlIntf.Signals.Count > 0) // If got signals
            {
                bAddNamespace = true;
                foreach (IDLSignal idlSignal in idlIntf.Signals)
                {
                    // Signal.
                    // * public System.EventHandler< <signal>Args > <signal>
                    // * {
                    // *     get { return this.proxy.<signal>; }
                    // *     set { this.proxy.<signal> = value; }
                    // * }
                    CodeMemberProperty propSignal = CodeBuilderCommon.CreateSignalEventProperty(idlSignal);
                    propSignal.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    CodePropertyReferenceExpression proprefProxySignal = new CodePropertyReferenceExpression(thisProxyFieldRef, propSignal.Name);
                    propSignal.GetStatements.Add(new CodeMethodReturnStatement(proprefProxySignal));
                    propSignal.SetStatements.Add(new CodeAssignStatement(proprefProxySignal, new CodePropertySetValueReferenceExpression()));
                    typeProxy.Members.Add(propSignal);

                } // Ends loop over signal
            } // Ends if got signals
        }

        public override void PostHandleMembers(IDLInterface idlIntf)
        {
            if (bAddNamespace != false)
            {
                // Namespaces.
                if (declarationHolder.ns != null) // If created namespace for parameter types
                {
                    unit.Namespaces.Add(declarationHolder.ns);
                    ns.Imports.Add(new CodeNamespaceImport(declarationHolder.ns.Name));

                } // Ends if created namespace for parameter types

                ns.Types.Add(typeProxy);
                unit.Namespaces.Add(ns);
            }
        }

    } // Ends class ProxyBuilder
}
