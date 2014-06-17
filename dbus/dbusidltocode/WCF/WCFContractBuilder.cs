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

namespace dbusidltocode
{
    internal class WCFContractBuilder : InterfaceBuilderBase
    {
        private bool gotVariant = false;

        public override void PreProcess()
        {
            this.visitorType = new WCFContractVisitor();
        }

        #region postHandles
        #region postMethod

        protected override CodeMemberMethod HandleMethod(IDLInterface idlIntf, IDLMethod idlMethod)
        {
            CodeMemberMethod method = base.HandleMethod(idlIntf, idlMethod);

            // [System.ServiceModel.OperationContractAttribute()]
            // Indicates that a method defines an operation that is part of a service contract in a Windows Communication Foundation (WCF) application
            method.CustomAttributes.Add(CodeBuilderCommon.attribOperationContract);

            if (this.gotVariant)
            {
                this.gotVariant = false; //Reinitialise for next loop

                // [Udbus.WCF.Contracts.DbusContainerAttribute()]
                method.CustomAttributes.Add(CodeBuilderCommon.attribDbusContainer);
            }

            return method;
        }

        protected override CodeParameterDeclarationExpression HandleMethodArgument(IDLMethodArgument idlMethodArg, ParamCodeTypeFactory paramtypeHolder)
        {
            CodeParameterDeclarationExpression toReturn = base.HandleMethodArgument(idlMethodArg, paramtypeHolder);
            if (idlMethodArg.Type.IndexOf('v') != -1) this.gotVariant = true;
            return toReturn;
        }

        #endregion postMethod

        public override CodeTypeMember HandleProperty(IDLInterface idlIntf, IDLProperty idlProperty)
        {
            CodeMemberProperty property = (CodeMemberProperty)base.HandleProperty(idlIntf, idlProperty);

            if (property.HasGet || property.HasSet)
            {
                // Generate the property out of context.
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CodeGeneratorOptions genOptions = CodeBuilderHelper.getCodeGeneratorOptions();
                StringWriter temp = new StringWriter();
                provider.GenerateCodeFromMember(property, temp, genOptions);
                string propertyText = temp.ToString();
                propertyText = propertyText.TrimStart();

                //Get ready to store all the output
                StringBuilder result = new StringBuilder();

                // Figure out how much comments exist before doing the real work
                int posSkipStatements = 0;
                while (posSkipStatements + 1 < propertyText.Length && propertyText[posSkipStatements] == '/' && propertyText[posSkipStatements + 1] == '/')
                {
                    posSkipStatements = propertyText.IndexOf(temp.NewLine, posSkipStatements);
                    posSkipStatements += temp.NewLine.Length;
                }

                //Insert comments into output
                if (posSkipStatements > 0)
                {
                    result.Append(propertyText.Substring(0, posSkipStatements));
                    propertyText = propertyText.Substring(posSkipStatements);
                }

                //Remove abstract modifiers
                const string abstractName = "abstract ";
                if (propertyText.StartsWith(abstractName))
                {
                    propertyText = propertyText.Substring(abstractName.Length);
                }
                
                // Hacky rewrite of the getter/setter for CSharp.
                if (property.HasGet)
                {
                    propertyText = AddOperationContractToProperty(propertyText, "get;", temp.NewLine);
                }
                if (property.HasSet)
                {
                    propertyText = AddOperationContractToProperty(propertyText, "set;", temp.NewLine);
                }

                // Add the altered text.
                result.Append(propertyText);

                // Mess around with padding.
                string resultText = result.ToString();
                resultText = resultText.Replace(temp.NewLine, temp.NewLine + "        ");
                resultText = "        " + resultText;

                // Add the snippet.
                CodeSnippetTypeMember snipProperty = new CodeSnippetTypeMember(resultText);
                snipProperty.Name = property.Name;
                return snipProperty;
            }
            else
            {
                return property;
            }
        }

        #region postSignal

        override protected void PostHandleSignal(IDLInterface idlIntf, IDLSignal idlSignal, CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration typeInterface)
        {
            string nsName, intf;
            int lastDot = idlIntf.Name.LastIndexOf('.');
            intf = idlIntf.Name.Substring(lastDot + 1);
            nsName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new InterfaceVisitor());
            CodeFieldReferenceExpression fieldrefCallback = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.callback);
            CodeFieldReferenceExpression fieldrefTarget = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.targetName);
            CodeNamespace nsClient = this.GetClientNamespace(unit, idlIntf.Name);
            CodeNamespace nsClientTypes = nsClient;

            if (this.typedeclCallbackInterface == null) // If no callback interface yet
            {
                CodeTypeReference typerefInterface = new CodeTypeReference(typeInterface.Name);

                // Declare callback interface and hold onto it in field.
                // * public interface I<interface>Callback
                // * {
                string nsWCFName, intfWCF;
                nsWCFName = CodeBuilderCommon.GetNamespace(idlIntf.Name, this.visitorType);
                intfWCF = CodeBuilderCommon.GetName(idlIntf.Name, null);
                string callbackInterfaceName = CodeBuilderCommon.GetSignalCallbackInterfaceName(intfWCF);
                typedeclCallbackInterface = new CodeTypeDeclaration(callbackInterfaceName);
                typedeclCallbackInterface.Attributes = MemberAttributes.Public;
                typedeclCallbackInterface.IsInterface = true;
                ns.Types.Add(typedeclCallbackInterface);

                // * class <interface>CallbackClient : I<interface>Callback
                string wcfCallbackClientName = CodeBuilderCommon.GetSignalCompilableName(intfWCF) + "CallbackClient";
                CodeTypeReference typerefCallbackInterface = new CodeTypeReference(typedeclCallbackInterface.Name);

                CodeAttributeDeclaration attribdeclServiceContract = new CodeAttributeDeclaration(CodeBuilderCommon.typerefServiceContract,
                    new CodeAttributeArgument("CallbackContract", new CodeTypeOfExpression(typerefCallbackInterface))
                );
                typeInterface.CustomAttributes.Add(attribdeclServiceContract);

                //string scopedCallbackInterfaceName = CodeBuilderCommon.GetScopedName(nsWCFName, proxyInterfaceName);
                //typedeclCallbackClient = new CodeTypeDeclaration(CodeBuilderCommon.GetSignalCallbackName(intf));
                typedeclCallbackClient = new CodeTypeDeclaration(wcfCallbackClientName);
                CodeTypeReference typerefWCFCallbackInterface = new CodeTypeReference(typedeclCallbackInterface.Name);
                typedeclCallbackClient.BaseTypes.Add(typerefWCFCallbackInterface);

                nsClientTypes.Types.Add(typedeclCallbackClient);

                // * public class <interface>Proxy : Udbus.WCF.Client.CallbackProxy< <wcf_contracts.interface>, <interface>CallbackClient >
                this.typedeclProxy = new CodeTypeDeclaration(CodeBuilderCommon.GetSignalProxyName(intfWCF));
                CodeTypeReference typerefCallbackProxy = new CodeTypeReference(typeof(Udbus.WCF.Client.CallbackProxy<,>));
                CodeTypeReference typerefCallbackClient = new CodeTypeReference(typedeclCallbackClient.Name);
                typerefCallbackProxy.TypeArguments.Add(typerefInterface);
                typerefCallbackProxy.TypeArguments.Add(typerefCallbackClient);
                this.typedeclProxy.BaseTypes.Add(typerefCallbackProxy);

                AddProxyConstructors(this.typedeclProxy);

                nsClientTypes.Types.Add(this.typedeclProxy);

            } // Ends if no callback interface yet

            // Add signal property to Proxy.
            CodeMemberProperty propProxySignal = CodeBuilderCommon.CreateSignalEventProperty(idlSignal);
            propProxySignal.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodePropertyReferenceExpression proprefProxyInterface = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ProxyInterface");
            CodePropertyReferenceExpression proprefProxyCallback = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Callback");
            CodePropertyReferenceExpression proprefProxyCallbackEvent = new CodePropertyReferenceExpression(proprefProxyCallback, propProxySignal.Name);
            propProxySignal.GetStatements.Add(new CodeMethodReturnStatement(proprefProxyCallbackEvent));
            propProxySignal.SetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(proprefProxyCallbackEvent, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
                , new CodeExpressionStatement(new CodeMethodInvokeExpression(proprefProxyInterface, CodeBuilderCommon.GetSignalRegisterFunction(idlSignal.Name)))
            ));
            propProxySignal.SetStatements.Add(new CodeAssignStatement(proprefProxyCallbackEvent, new CodePropertySetValueReferenceExpression()));
            this.typedeclProxy.Members.Add(propProxySignal);

            // Add callback method to callback interface.
            // * [System.ServiceModel.OperationContract(IsOneWay=true)]
            // * void On<signal>(<interface_ns>.<signal>Args args);
            CodeMemberMethod methodOnSignal = new CodeMemberMethod();
            methodOnSignal.Name = CodeBuilderCommon.GetSignalCallbackMethodName(idlSignal.Name);
            string signalArgsTypeName = CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name);
            string scopedSignalArgsTypeName = CodeBuilderCommon.GetScopedName(nsName, signalArgsTypeName);
            CodeParameterDeclarationExpression paramdeclSignalArgs = new CodeParameterDeclarationExpression(scopedSignalArgsTypeName,
                CodeBuilderCommon.SignalArgsName);
            methodOnSignal.Parameters.Add(paramdeclSignalArgs);
            methodOnSignal.CustomAttributes.Add(CodeBuilderCommon.attribOperationContractOneWay);
            typedeclCallbackInterface.Members.Add(methodOnSignal);

            // Add registration method to wcf interface.
            // * [System.ServiceModel.OperationContract]
            // * void RegisterForStorageSpaceLow();
            CodeMemberMethod methodRegister = new CodeMemberMethod();
            methodRegister.Name = CodeBuilderCommon.GetSignalRegisterFunction(idlSignal.Name);
            methodRegister.CustomAttributes.Add(CodeBuilderCommon.attribOperationContract);
            typeInterface.Members.Add(methodRegister);

            // Add event to callback client implementation.

            // * private event System.EventHandler< <signal>Args > <signal>Event;
            CodeMemberEvent eventSignal = CodeBuilderCommon.CreateSignalEvent(idlSignal);
            typedeclCallbackClient.Members.Add(eventSignal);

            // * public virtual System.EventHandler< <signal>Args > <signal>
            // * {
            // *     get { return this.<signal>Event; }
            // *     set { this.<signal>Event = value; }
            // * }
            CodeMemberProperty propSignal = CodeBuilderCommon.CreateSignalEventProperty(idlSignal);
            propSignal.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeEventReferenceExpression eventrefSignal = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), eventSignal.Name);
            propSignal.GetStatements.Add(new CodeMethodReturnStatement(eventrefSignal));
            propSignal.SetStatements.Add(new CodeAssignStatement(eventrefSignal, new CodePropertySetValueReferenceExpression()));
            typedeclCallbackClient.Members.Add(propSignal);

            // * public void On<signal>(<ns>.<signal>Args args)
            // * {
            // *     if (this.<signal> != null)
            // *     {
            // *         this.<signal>(this, args);
            // *     }
            //}
            CodeMemberMethod methodSignal = new CodeMemberMethod();
            methodSignal.Name = CodeBuilderCommon.GetSignalCallbackMethodName(idlSignal.Name);
            methodSignal.Attributes = MemberAttributes.Public;
            methodSignal.Parameters.Add(paramdeclSignalArgs);
            CodePropertyReferenceExpression proprefSignal = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propSignal.Name);
            methodSignal.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(eventrefSignal, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null))
                , new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), eventrefSignal.EventName, new CodeThisReferenceExpression(), new CodeArgumentReferenceExpression(paramdeclSignalArgs.Name)))
            ));
            typedeclCallbackClient.Members.Add(methodSignal);
        }
        
        #endregion postSignal

        #region postInterface
        override protected void PostHandleInterface(IDLInterface idlIntf, CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration typeInterface)
        {
            if (idlIntf.Signals == null || idlIntf.Signals.Count == 0) // If no signals
            {
                typeInterface.CustomAttributes.Add(CodeBuilderCommon.attribServiceContract);

            } // Ends if no signals

            // Add DbusServiceRelativeAddress constant to Constants class.
            // * public partial class Constants {
            CodeTypeDeclaration typedeclConstants = new CodeTypeDeclaration(CodeBuilderCommon.nameConstantsClass);
            typedeclConstants.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            typedeclConstants.IsPartial = true;
            // *    public const string DbusServiceRelativeAddress = "<contract_name>";
            // * }

            CodeMemberField fieldRelativeAddress = new CodeMemberField(typeof(string), CodeBuilderCommon.nameRelativeAddress);
            fieldRelativeAddress.Attributes = MemberAttributes.Const | MemberAttributes.Public;
            fieldRelativeAddress.InitExpression = new CodePrimitiveExpression(idlIntf.Name.Replace('.', '/'));
            typedeclConstants.Members.Add(fieldRelativeAddress);

            ns.Types.Add(typedeclConstants);

            CodeTypeReference typerefInterface = new CodeTypeReference(CodeBuilderCommon.GetScopedName(ns.Name, typeInterface.Name));
            CodeTypeReference typerefInterfaceProxy = new CodeTypeReference(typeof(Udbus.WCF.Client.Proxy<>));
            typerefInterfaceProxy.TypeArguments.Add(typerefInterface);

            CodeTypeReference typerefProxy; // Worked out in following code...

            if (this.typedeclProxy == null) // If no proxy type defined yet
            {
                // Just knock up a nice vanilla one for consistency's sake.
                string nsWCFName, intfWCF;
                nsWCFName = CodeBuilderCommon.GetNamespace(idlIntf.Name, this.visitorType);
                intfWCF = CodeBuilderCommon.GetName(idlIntf.Name, null);

                CodeTypeDeclaration typedeclProxy = new CodeTypeDeclaration(CodeBuilderCommon.GetSignalProxyName(intfWCF));
                typedeclProxy.BaseTypes.Add(typerefInterfaceProxy);

                AddProxyConstructors(typedeclProxy);

                CodeNamespace nsClient = this.GetClientNamespace(unit, idlIntf.Name);
                nsClient.Types.Add(typedeclProxy);

                typerefProxy = new CodeTypeReference(CodeBuilderCommon.GetScopedName(nsClient.Name, typedeclProxy.Name));

            } // Ends if no proxy type defined yet
            else // Else added proxy type
            {
                // Grab holder of scoped type reference.
                string nsWCFClient = CodeBuilderCommon.GetNamespace(idlIntf.Name, new WCFClientVisitor());
                typerefProxy = new CodeTypeReference(CodeBuilderCommon.GetScopedName(nsWCFClient, this.typedeclProxy.Name));

            } // Ends else added proxy type

            // Reset helper types for next time.
            this.typedeclCallbackInterface = null;
            this.typedeclCallbackClient = null;
            this.typedeclProxy = null;

            // Add Proxy creation function to ProxyBuilder class via extension method.
            CodeTypeReference typerefEndpointComponents = new CodeTypeReference(typeof(Udbus.WCF.Client.DbusEndpointUriComponents));
            CodeTypeReferenceExpression typerefexprEndpointComponents = new CodeTypeReferenceExpression(typerefEndpointComponents);
            CodeTypeReference typerefProxyManager = new CodeTypeReference(typeof(Udbus.WCF.Client.ProxyManager));
            CodeTypeReferenceExpression typerefexprProxyManager = new CodeTypeReferenceExpression(typerefProxyManager);
            CodeTypeReference typerefProxyBuilder = new CodeTypeReference(typeof(Udbus.WCF.Client.ProxyBuilder));
            CodeTypeReference typerefProxyBuilderExtension = new CodeTypeReference("this " + typerefProxyBuilder.BaseType); // No CodeDom support for extension methods. Awesome.
            CodeTypeReferenceExpression typerefexprProxyBuilder = new CodeTypeReferenceExpression(typerefProxyBuilder);

            CodeTypeReferenceExpression typerefexprConstants = new CodeTypeReferenceExpression(CodeBuilderCommon.GetScopedName(ns.Name, typedeclConstants.Name));
            CodeParameterDeclarationExpression paramdeclInterfaceProxy = new CodeParameterDeclarationExpression(typerefInterfaceProxy, proxy);
            CodeParameterDeclarationExpression paramdeclProxyManagerExtension = new CodeParameterDeclarationExpression(typerefProxyBuilderExtension, proxyBuilder);
            CodeParameterDeclarationExpression paramdeclEndpointUri = new CodeParameterDeclarationExpression(typerefEndpointComponents, dbusEndpointUri);
            CodeArgumentReferenceExpression argrefProxy = new CodeArgumentReferenceExpression(proxy);
            CodeArgumentReferenceExpression argrefEndpointUri = new CodeArgumentReferenceExpression(paramdeclEndpointUri.Name);
            CodeExpression exprRelativeAddress = new CodeFieldReferenceExpression(typerefexprConstants, fieldRelativeAddress.Name);
            CodeArgumentReferenceExpression argrefProxyBuilder = new CodeArgumentReferenceExpression(proxyBuilder);
            CodePropertyReferenceExpression proprefBindingFactory = new CodePropertyReferenceExpression(argrefProxyBuilder, BindingFactory);

            // * static public partial class ProxyBuilderExtensions
            // * {
            CodeTypeDeclaration typedeclProxyBuilderExtensions = new CodeTypeDeclaration(CodeBuilderCommon.nameProxyBuilderExtensionsClass);
            typedeclProxyBuilderExtensions.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            typedeclProxyBuilderExtensions.IsPartial = true;
            // http://stackoverflow.com/questions/6308310/creating-extension-method-using-codedom
            typedeclProxyBuilderExtensions.Attributes = MemberAttributes.Public;
            typedeclProxyBuilderExtensions.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, Environment.NewLine + "\tstatic"));
            typedeclProxyBuilderExtensions.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));

            // *     static public void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out Udbus.WCF.Client.Proxy< <wcf_contract> > proxy
            // *         , Udbus.WCF.Client.DbusEndpointUriComponents dbusEndpointUri)
            // *     {
            // *         proxy = Udbus.WCF.Client.ProxyManager.Create< <wcf_contract> >(
            // *             dbusEndpointUri.CreateUri(<wcf_namespace>.wcf.Contracts.Constants.DbusServiceRelativeAddress)
            // *             , proxyBuilder.BindingFactory
            // *         );
            // *     }
            CodeMemberMethod methodCreateInterfaceProxyWithEndpoint = new CodeMemberMethod();
            methodCreateInterfaceProxyWithEndpoint.Name = "Create";
            methodCreateInterfaceProxyWithEndpoint.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            paramdeclInterfaceProxy.Direction = FieldDirection.Out;
            methodCreateInterfaceProxyWithEndpoint.Parameters.Add(paramdeclProxyManagerExtension);
            methodCreateInterfaceProxyWithEndpoint.Parameters.Add(paramdeclInterfaceProxy);
            methodCreateInterfaceProxyWithEndpoint.Parameters.Add(paramdeclEndpointUri);
            // * dbusEndpointUri.CreateUri(<ns>.wcf.Contracts.Constants.DbusServiceRelativeAddress)
            CodeMethodInvokeExpression invokeCreateUri = new CodeMethodInvokeExpression(argrefEndpointUri, CreateUri, exprRelativeAddress);
            CodeMethodInvokeExpression invokeProxyManagerCreate = new CodeMethodInvokeExpression(typerefexprProxyManager, "Create", invokeCreateUri, proprefBindingFactory);
            invokeProxyManagerCreate.Method.TypeArguments.Add(typerefInterface);
            methodCreateInterfaceProxyWithEndpoint.Statements.Add(
                new CodeAssignStatement(
                    argrefProxy
                    , invokeProxyManagerCreate
                )
            );
            typedeclProxyBuilderExtensions.Members.Add(methodCreateInterfaceProxyWithEndpoint);

            // *     static public void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out Udbus.WCF.Client.Proxy< <wcf_contract> > proxy)
            // *     {
            // *         proxyBuilder.Create(out proxy, Udbus.WCF.Client.DbusEndpointUriComponents.Create(proxyBuilder.AbsoluteUribuilder));
            // *     }
            CodeMemberMethod methodCreateInterfaceProxy = new CodeMemberMethod();
            methodCreateInterfaceProxy.Name = "Create";
            methodCreateInterfaceProxy.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            methodCreateInterfaceProxy.Parameters.Add(paramdeclProxyManagerExtension);
            methodCreateInterfaceProxy.Parameters.Add(paramdeclInterfaceProxy);
            CodeMethodInvokeExpression invokeProxyManagerExtensionCreate = new CodeMethodInvokeExpression(
                argrefProxyBuilder
                , "Create"
                , new CodeDirectionExpression(FieldDirection.Out, argrefProxy)
                , new CodeMethodInvokeExpression(
                    typerefexprEndpointComponents
                    , "Create"
                    , new CodePropertyReferenceExpression(argrefProxyBuilder, AbsoluteUribuilder)
                )
            );
            methodCreateInterfaceProxy.Statements.Add(invokeProxyManagerExtensionCreate);
            typedeclProxyBuilderExtensions.Members.Add(methodCreateInterfaceProxy);

            CodeParameterDeclarationExpression paramdeclProxy = new CodeParameterDeclarationExpression(typerefProxy, proxy);
            paramdeclProxy.Direction = FieldDirection.Out;

            // * public static void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out <namespace>.Contracts.Clients.<interface>Proxy proxy, Udbus.WCF.Client.DbusEndpointUriComponents dbusEndpointUri)
            // * {
            // *     proxy = new <wcf_namespace>.Contracts.Clients.<interface>Proxy(proxyBuilder.BindingFactory, dbusEndpointUri.CreateUri(<namespace>.wcf.Contracts.Constants.DbusServiceRelativeAddress));
            // * }
            CodeMemberMethod methodCreateProxyWithEndpoint = new CodeMemberMethod();
            methodCreateProxyWithEndpoint.Name = "Create";
            methodCreateProxyWithEndpoint.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            methodCreateProxyWithEndpoint.Parameters.Add(paramdeclProxyManagerExtension);
            methodCreateProxyWithEndpoint.Parameters.Add(paramdeclProxy);
            methodCreateProxyWithEndpoint.Parameters.Add(paramdeclEndpointUri);
            methodCreateProxyWithEndpoint.Statements.Add(new CodeAssignStatement(
                argrefProxy
                , new CodeObjectCreateExpression(typerefProxy
                    , proprefBindingFactory
                    , invokeCreateUri
                )
            ));
            typedeclProxyBuilderExtensions.Members.Add(methodCreateProxyWithEndpoint);

            // * public static void Create(this Udbus.WCF.Client.ProxyBuilder proxyBuilder, out <namespace>.Contracts.Clients.<interface>Proxy proxy)
            // * {
            // *     proxyBuilder.Create(out proxy, Udbus.WCF.Client.DbusEndpointUriComponents.Create(proxyBuilder.AbsoluteUribuilder));
            // * }
            CodeMemberMethod methodCreateProxy = new CodeMemberMethod();
            methodCreateProxy.Name = "Create";
            methodCreateProxy.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            methodCreateProxy.Parameters.Add(paramdeclProxyManagerExtension);
            methodCreateProxy.Parameters.Add(paramdeclProxy);
            methodCreateProxy.Statements.Add(invokeProxyManagerExtensionCreate);
            typedeclProxyBuilderExtensions.Members.Add(methodCreateProxy);

            CodeNamespace nsClientExtensions = new CodeNamespace(CodeBuilderCommon.nsClientExtensions);
            nsClientExtensions.Types.Add(typedeclProxyBuilderExtensions);
            unit.Namespaces.Add(nsClientExtensions);
            // * }

        }
        #endregion postInterface
        #endregion postHandles

        private static int AddOperationContractToProperty(string propertyText, string newline, string propertyMethod, int posStart, StringBuilder result)
        {
            int posEnd = -1;
            if (posStart > 0)
            {
                propertyText = propertyText.Substring(posStart);
            }
            int pos = propertyText.IndexOf(propertyMethod);
            if (pos != -1)
            {
                int posNewLine = propertyText.LastIndexOf(newline, pos);
                if (posNewLine != -1)
                {
                    result.Append(propertyText.Substring(0, posNewLine + newline.Length));
                    result.AppendLine("    [OperationContract]");
                    result.AppendLine("    " + propertyMethod);
                    posEnd = posStart + pos + propertyMethod.Length + newline.Length;
                }
                else
                {
                    result.Append(propertyText.Substring(0, pos));
                    result.AppendLine("[OperationContract]");
                    result.Append("    " + propertyMethod);
                    posEnd = posStart + pos + propertyMethod.Length;
                }
            }

            return posEnd;
        }

        private static string AddOperationContractToProperty(string propertyText, string propertyMethod, string newline)
        {
            return propertyText.Replace(propertyMethod, "[OperationContract]" + newline + "    " + propertyMethod);
        }   

        static readonly CodeTypeFactory WCFContractTypeFactoryMethod = new CodeTypeFactory(new StructCreatorFullNameFactory(), new DictCreatorFactoryIn(), new DictCreatorFactoryOut());
        static readonly CodeTypeFactory WCFContractTypeFactoryProperty = new CodeTypeFactory(new StructCreatorFullNameFactory(), new DictCreatorFactoryIn(), new DictCreatorFactoryOutProperty());

        protected override ParamCodeTypeFactory CreateParamCodeTypeFactoryForMethod(string argDirection)
        {
            return new ParamCodeTypeFactory(WCFContractTypeFactoryMethod, argDirection == "out" ? FieldDirection.Out : FieldDirection.In);
        }

        protected override ParamCodeTypeFactory CreateParamCodeTypeFactoryForProperty(IDLInterface idlIntf, string propertyName, string argName, string argType, string argDirection)
        {
            return new ParamCodeTypeFactory(WCFContractTypeFactoryProperty, argDirection == "out" ? FieldDirection.Out : FieldDirection.In);
        }

        protected override Udbus.Parsing.ICodeTypeDeclarationHolder CreateCodeTypeDeclarationHolder(IDLInterface idlIntf)
        {
            return new Udbus.Parsing.CodeTypeNoOpHolder();
        }

        #region Signals
        //private CodeTypeDeclaration typeCallback = new CodeTypeDeclaration(); // Reset for each signal

        private void InitFields()
        {
            //this.typeCallback.Attributes = MemberAttributes.Public;
        }

        CodeTypeDeclaration typedeclCallbackInterface = null;
        CodeTypeDeclaration typedeclCallbackClient = null;
        CodeTypeDeclaration typedeclProxy = null;
        Dictionary<string, CodeNamespace> dictNSClient = new Dictionary<string, CodeNamespace>();
        const string args = "args";

        private CodeNamespace GetClientNamespace(CodeCompileUnit unit, string idlIntfName)
        {
            CodeNamespace nsClient;
            string nsClientName = CodeBuilderCommon.GetNamespace(idlIntfName, new WCFClientVisitor());
            if (this.dictNSClient.TryGetValue(nsClientName, out nsClient) == false)
            {
                nsClient = new CodeNamespace(nsClientName);
                this.dictNSClient[nsClientName] = nsClient;
                unit.Namespaces.Add(nsClient);
            }
            return nsClient;
        }

        static private void AddProxyConstructors(CodeTypeDeclaration typedeclProxy)
        {
            // * public <interface>Proxy(Udbus.WCF.Client.IBindingFactory bindingFactory, System.Uri uriEndpoint)
            // *     : base(bindingFactory, uriEndpoint)
            // * {}
            CodeParameterDeclarationExpression paramdeclFactory = new CodeParameterDeclarationExpression(typeof(Udbus.WCF.Client.IBindingFactory), "bindingFactory");
            CodeParameterDeclarationExpression paramdeclUri = new CodeParameterDeclarationExpression(typeof(System.Uri), "uriEndpoint");
            CodeArgumentReferenceExpression argrefFactory = new CodeArgumentReferenceExpression(paramdeclFactory.Name);
            CodeArgumentReferenceExpression argrefUri = new CodeArgumentReferenceExpression(paramdeclUri.Name);
            CodeConstructor proxyConstructorFactoryUri = new CodeConstructor();
            proxyConstructorFactoryUri.Attributes = MemberAttributes.Public;
            proxyConstructorFactoryUri.Parameters.Add(paramdeclFactory);
            proxyConstructorFactoryUri.Parameters.Add(paramdeclUri);
            proxyConstructorFactoryUri.BaseConstructorArgs.Add(argrefFactory);
            proxyConstructorFactoryUri.BaseConstructorArgs.Add(argrefUri);
            typedeclProxy.Members.Add(proxyConstructorFactoryUri);

            // * public <interface>Proxy(System.Uri uriEndpoint)
            // *     : base(uriEndpoint)
            // * {}
            CodeConstructor proxyConstructorUri = new CodeConstructor();
            proxyConstructorUri.Attributes = MemberAttributes.Public;
            proxyConstructorUri.Parameters.Add(paramdeclUri);
            proxyConstructorUri.BaseConstructorArgs.Add(argrefUri);
            typedeclProxy.Members.Add(proxyConstructorUri);
        }

        #endregion // Signals

        const string proxy = "proxy";
        const string dbusEndpointUri = "dbusEndpointUri";
        const string CreateUri = "CreateUri";
        const string proxyBuilder = "proxyBuilder";
        const string AbsoluteUribuilder = "AbsoluteUribuilder";
        const string BindingFactory = "BindingFactory";


    } // Ends class WCFContractBuilder

}
