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
    class WCFHostBuilder : Builder
    {
        protected const string nameWCFServiceParams = "WCFServiceParams";
        protected const string nameWCFServiceParamsArg = "wcfserviceparams";
        protected const string nameUriBaseArg = "uriBase";
        protected const string GetDbusServiceCreationParams = "GetDbusServiceCreationParams";
        protected const string ContainsService = "ContainsService";
        protected const string MakeServiceHostCreationData = "MakeServiceHostCreationData";
        protected const string GetHostDbusServiceRegistrationParams = "GetHostDbusServiceRegistrationParams";
        protected const string ServiceTypesArg = "serviceTypes";

        protected const string CreateServiceName = "CreateService";
        protected const string dbusService = "dbusService";
        protected const string wcfService = "wcfService";

        protected static readonly CodeArgumentReferenceExpression argrefWCFServiceParams = new CodeArgumentReferenceExpression(nameWCFServiceParamsArg);
        protected static readonly CodeVariableReferenceExpression varrefWcfService = new CodeVariableReferenceExpression(wcfService);

        private CodeMemberMethod BuildGetDbusServiceRegistrationParamsMethod(string name)
        {
            CodeMemberMethod methodGetDbusServiceRegistrationParams = new CodeMemberMethod();
            methodGetDbusServiceRegistrationParams.Name = name;
            methodGetDbusServiceRegistrationParams.ReturnType = CodeBuilderCommon.typerefRegistrationParams;

            return methodGetDbusServiceRegistrationParams;
        }

        private CodeTypeDeclaration BuildMakeServiceHostCreationDataImplementation(string ifName,
            string nsWCFContractsName,
            CodeTypeReference typerefWCFService, CodeTypeReference typerefWCFContract,
            CodeTypeReference typerefService)
        {
            string nsWCFHost = CodeBuilderCommon.typeServiceHostCreationData.Namespace;
            string scopedWCFServiceParams = CodeBuilderCommon.GetScopedName(nsWCFHost, nameWCFServiceParams);

            CodeTypeReferenceExpression typerefexprWCFService = new CodeTypeReferenceExpression(typerefWCFService);
            CodeArgumentReferenceExpression argrefUriBase = new CodeArgumentReferenceExpression(nameUriBaseArg);
            CodeTypeReference typerefConstants = new CodeTypeReference(CodeBuilderCommon.GetScopedName(nsWCFContractsName, CodeBuilderCommon.nameConstantsClass));
            CodeTypeReferenceExpression typerefexprConstants = new CodeTypeReferenceExpression(typerefConstants);
            Type typeIMakeServiceHostCreationData = typeof(Udbus.WCF.Service.Host.IMakeServiceHostCreationData);

            CodeTypeDeclaration typeImpl = new CodeTypeDeclaration(string.Format("MakeServiceHostCreationData{0}", ifName));
            typeImpl.Attributes = MemberAttributes.Public;
            typeImpl.BaseTypes.Add(new CodeTypeReference(typeIMakeServiceHostCreationData));
            CodeTypeReference typerefImpl = new CodeTypeReference(typeImpl.Name);

            CodeMethodInvokeExpression invokeGetDbusServiceCreationParams = new CodeMethodInvokeExpression(null, GetDbusServiceCreationParams);
            CodeTypeOfExpression typeofWCFService = new CodeTypeOfExpression(typerefWCFService);

            // CreateService()
            CodeMemberMethod methodCreateService = this.CreateService(typerefWCFService, typerefService);

            CodeMemberMethod methodCreateServiceCreationData = new CodeMemberMethod();
            methodCreateServiceCreationData.Name = "CreateServiceCreationData";
            methodCreateServiceCreationData.Attributes = MemberAttributes.Static;
            CodeTypeReference typerefServiceCreationData = new CodeTypeReference(typeof(Udbus.WCF.Service.Host.WCFServiceCreationData<,>));
            typerefServiceCreationData.TypeArguments.Add(typerefWCFService);
            typerefServiceCreationData.TypeArguments.Add(CodeBuilderCommon.typerefWCFServiceParams);
            methodCreateServiceCreationData.ReturnType = typerefServiceCreationData;
            methodCreateServiceCreationData.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(typerefServiceCreationData
                    // CreateService // Static creation method in this class
                    , new CodeMethodReferenceExpression(null, CreateServiceName)
                    // GetDbusServiceCreationParams()
                    , invokeGetDbusServiceCreationParams
                    //// <wcf_contracts>.Constants.DbusServiceRelativeAddress
                    //, new CodeFieldReferenceExpression(typerefexprConstants, CodeBuilderCommon.nameRelativeAddress)
                    //// typeof(<contract>)
                    //, new CodeTypeOfExpression(typerefWCFContract)
            )));

            //         static Udbus.WCF.Host.DbusServiceCreationParams GetDbusServiceCreationParams()
        //{
        //    return new Udbus.WCF.Host.DbusServiceCreationParams(org.freedesktop.DBus.Properties.wcf.Contracts.Constants.DbusServiceRelativeAddress, typeof(org.freedesktop.DBus.Properties.wcf.Contracts.IProperties));
        //}

            CodeMemberMethod methodGetDbusServiceCreationParams = new CodeMemberMethod();
            methodGetDbusServiceCreationParams.Name = GetDbusServiceCreationParams;
            methodGetDbusServiceCreationParams.Attributes = MemberAttributes.Static;
            methodGetDbusServiceCreationParams.ReturnType = CodeBuilderCommon.typerefDbusServiceCreationParams;
            methodGetDbusServiceCreationParams.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(
                CodeBuilderCommon.typerefDbusServiceCreationParams
                    // <wcf_contracts>.Constants.DbusServiceRelativeAddress
                    , new CodeFieldReferenceExpression(typerefexprConstants, CodeBuilderCommon.nameRelativeAddress)
                    // typeof(<contract>)
                    , new CodeTypeOfExpression(typerefWCFContract)
            )));

            CodeMemberMethod methodContainsService = new CodeMemberMethod();
            methodContainsService.Name = ContainsService;
            methodContainsService.ReturnType = CodeBuilderCommon.typerefBool;
            CodeParameterDeclarationExpression paramdeclServiceTypes = new CodeParameterDeclarationExpression(typeof(Type[]), ServiceTypesArg);
            paramdeclServiceTypes.CustomAttributes.Add(CodeBuilderCommon.attribParams);
            methodContainsService.Parameters.Add(paramdeclServiceTypes);
            methodContainsService.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            methodContainsService.Statements.Add(new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression(ServiceTypesArg),
                    "Contains",
                    typeofWCFService
                )
            ));

            // * public Udbus.WCF.Service.Host.ServiceHostCreationData MakeServiceHostCreationData(Udbus.WCF.Host.WCFServiceParams wcfserviceparams, params System.Uri[] uriBase)
            // * {
            // *     Udbus.WCF.Service.Host.ServiceHostCreationData create = Udbus.WCF.Service.Host.DbusServiceHost.CreateWithData< <wcf_service>, Udbus.WCF.Host.WCFServiceParams>(wcfserviceparams, CreateServiceCreationData, uriBase);
            // *     return create;
            // * }
            CodeMethodInvokeExpression invokeCreateWithData = new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprDbusServiceHost, CodeBuilderCommon.nameDbusServiceHostCreateWithDataMethod
                // wcfserviceparams
                , argrefWCFServiceParams
                // CreateServiceCreationData
                , new CodeMethodReferenceExpression(null, methodCreateServiceCreationData.Name)
                //, uriBase)
                , argrefUriBase
            );
            invokeCreateWithData.Method.TypeArguments.Add(typerefWCFService);
            invokeCreateWithData.Method.TypeArguments.Add(CodeBuilderCommon.typerefWCFServiceParams);

            CodeMemberMethod methodMakeServiceCreationData = new CodeMemberMethod();
            methodMakeServiceCreationData.Name = MakeServiceHostCreationData;
            methodMakeServiceCreationData.ReturnType = CodeBuilderCommon.typerefServiceHostCreationData;
            methodMakeServiceCreationData.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefWCFServiceParams, nameWCFServiceParamsArg));
            CodeParameterDeclarationExpression paramdeclUriBase = new CodeParameterDeclarationExpression(typeof(System.Uri[]), nameUriBaseArg);
            paramdeclUriBase.CustomAttributes.Add(CodeBuilderCommon.attribParams);
            methodMakeServiceCreationData.Parameters.Add(paramdeclUriBase);
            methodMakeServiceCreationData.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            methodMakeServiceCreationData.Statements.Add(
                new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefServiceHostCreationData, "create",
                    invokeCreateWithData
                )
            );
            methodMakeServiceCreationData.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("create")));

            CodeMemberMethod methodGetHostDbusServiceRegistrationParams = BuildGetDbusServiceRegistrationParamsMethod(GetHostDbusServiceRegistrationParams);
            methodGetHostDbusServiceRegistrationParams.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            methodGetHostDbusServiceRegistrationParams.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(
                CodeBuilderCommon.typerefRegistrationParams
                , invokeGetDbusServiceCreationParams
                , typeofWCFService
            )));

            CodeMemberMethod methodGetDbusServiceRegistrationParams = BuildGetDbusServiceRegistrationParamsMethod("GetDbusServiceRegistrationParams");
            methodGetDbusServiceRegistrationParams.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            methodGetDbusServiceRegistrationParams.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, GetHostDbusServiceRegistrationParams)));

            typeImpl.Members.Add(methodCreateService);
            typeImpl.Members.Add(methodCreateServiceCreationData);
            typeImpl.Members.Add(methodGetDbusServiceCreationParams);
            typeImpl.Members.Add(methodGetHostDbusServiceRegistrationParams);
            string regionIMakeServiceHostCreationData = string.Format("{0} functions", typeIMakeServiceHostCreationData.FullName);
            methodContainsService.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, regionIMakeServiceHostCreationData));
            typeImpl.Members.Add(methodContainsService);
            typeImpl.Members.Add(methodGetDbusServiceRegistrationParams);
            typeImpl.Members.Add(methodMakeServiceCreationData);
            methodMakeServiceCreationData.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, regionIMakeServiceHostCreationData));

            return typeImpl;
        }

        protected virtual CodeMemberMethod CreateService(CodeTypeReference typerefWCFService, CodeTypeReference typerefService)
        {
            CodeTypeReferenceExpression typerefexprService = new CodeTypeReferenceExpression(typerefService);
            CodeMethodReferenceExpression methodrefDbusServiceCreate = new CodeMethodReferenceExpression(typerefexprService, "Create");

            CodeMemberMethod methodCreateService = new CodeMemberMethod();
            methodCreateService.Name = CreateServiceName;
            methodCreateService.ReturnType = typerefWCFService;
            methodCreateService.Attributes = MemberAttributes.Static;
            methodCreateService.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefWCFServiceParams, nameWCFServiceParamsArg));
            // * <wcfservice> wcfService = new <wcfservice>(wcfserviceparams);
            methodCreateService.Statements.Add(new CodeVariableDeclarationStatement(typerefWCFService, wcfService
                , new CodeObjectCreateExpression(typerefWCFService, argrefWCFServiceParams)
            ));

            // return wcfService;
            methodCreateService.Statements.Add(new CodeMethodReturnStatement(varrefWcfService));
            return methodCreateService;
        }

        static readonly Type typeServiceConnectionParams = typeof(Udbus.Core.ServiceConnectionParams);
        static readonly Type typeDbusConnectionParams = typeof(Udbus.Serialization.DbusConnectionParameters);

        Type typeRegistryListEntry;
        CodeTypeReference typerefRegistryListEntryBase;
        CodeNamespace nsInit;

        string nsName, ifName;
        string nsContractName;

        string scopedDbusServiceName;
        string scopedWCFContractName;
        string scopedWCFServiceName;

        CodeTypeReference typerefWCFService;
        CodeTypeReference typerefInitService;
        CodeTypeDeclaration typedeclInitHostMakerRegistry;
        CodeMemberField fieldAddInit;

        CodeNamespace nsHost;

        public override void PreProcess()
        {
            this.typeRegistryListEntry = typeof(Udbus.WCF.Service.Host.RegistryListEntry<>);
            this.typerefRegistryListEntryBase = new CodeTypeReference(typeof(Udbus.WCF.Service.Host.RegistryListEntryBase));
            this.nsInit = new CodeNamespace(this.typeRegistryListEntry.Namespace + ".Init");

            // Create IMakeServiceHostCreationDataInterface
            //interface IMakeServiceHostCreationData
            //{
            //    bool ContainsService(Type[] contractTypes);
            //    Udbus.WCF.Host.ServiceHostCreationData MakeServiceHostCreationData(TestDbusIDLWCFService.WCFServiceParams wcfserviceparams, params System.Uri[] uriBase);
            //}
            CodeTypeDeclaration typedeclInterface = new CodeTypeDeclaration("IMakeServiceHostCreationData");
            typedeclInterface.Attributes = MemberAttributes.Public;
            typedeclInterface.IsInterface = true;

            CodeMemberMethod methodContainsService = new CodeMemberMethod();
            methodContainsService.Name = ContainsService;
            methodContainsService.ReturnType = CodeBuilderCommon.typerefServiceHostCreationData;
            CodeParameterDeclarationExpression paramdeclServiceTypes = new CodeParameterDeclarationExpression(typeof(Type[]), ServiceTypesArg);
            paramdeclServiceTypes.CustomAttributes.Add(CodeBuilderCommon.attribParams);
            methodContainsService.Parameters.Add(paramdeclServiceTypes);

            CodeMemberMethod methodMakeServiceCreationData = new CodeMemberMethod();
            methodMakeServiceCreationData.Name = MakeServiceHostCreationData;
            methodMakeServiceCreationData.Parameters.Add(new CodeParameterDeclarationExpression(nameWCFServiceParams, nameWCFServiceParamsArg));
            CodeParameterDeclarationExpression paramdeclUriBase = new CodeParameterDeclarationExpression(typeof(System.Uri[]), nameUriBaseArg);
            paramdeclUriBase.CustomAttributes.Add(CodeBuilderCommon.attribParams);
            methodMakeServiceCreationData.Parameters.Add(paramdeclUriBase);

            typedeclInterface.Members.Add(methodContainsService);
            typedeclInterface.Members.Add(methodMakeServiceCreationData);
        }

        public override void PostProcess()
        {
            // Add Init.
            this.unit.Namespaces.Add(this.nsInit);
        }

        public override void ProcessNamespaces(IDLInterface idlIntf)
        {
            //Get the necessary names
            this.ifName = CodeBuilderCommon.GetName(idlIntf.Name, null);
            this.nsName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new InterfaceVisitor());
            this.nsContractName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new WCFContractVisitor());
            string nsServiceName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new WCFServiceVisitor()); //Local scope, not needed later

            //Get the necessary scoped names
            this.scopedDbusServiceName = CodeBuilderCommon.GetScopedName(this.nsName, CodeBuilderCommon.GetName(idlIntf.Name, new DBUSServiceVisitor()));
            this.scopedWCFContractName = CodeBuilderCommon.GetScopedName(this.nsContractName, CodeBuilderCommon.GetName(idlIntf.Name, new InterfaceVisitor()));
            this.scopedWCFServiceName = CodeBuilderCommon.GetScopedName(nsServiceName, CodeBuilderCommon.GetName(idlIntf.Name, new WCFServiceVisitor()));

            //Get our the Host's namespace and give it all the imports it requires
            this.nsHost = new CodeNamespace(CodeBuilderCommon.GetNamespace(idlIntf.Name, new WCFHostVisitor()));
            CodeBuilderCommon.AddUsingNamespaces(this.nsHost, new WCFHostVisitor());
        }

        public override void DeclareCodeType(IDLInterface idlIntf)
        {
            //Declare code type references
            this.typerefWCFService = new CodeTypeReference(this.scopedWCFServiceName);
            CodeTypeReference typerefConstants = new CodeTypeReference(CodeBuilderCommon.GetScopedName(this.nsName, CodeBuilderCommon.nameConstantsClass));
            CodeTypeReference typerefWCFContract = new CodeTypeReference(this.scopedWCFContractName);
            CodeTypeReference typerefDbusService = new CodeTypeReference(this.scopedDbusServiceName);

            //No idea...
            this.type = BuildMakeServiceHostCreationDataImplementation(this.ifName, this.nsContractName,
                                                                       this.typerefWCFService, typerefWCFContract, typerefDbusService);
            this.nsHost.Types.Add(this.type);
            CodeTypeReference typeref = new CodeTypeReference(CodeBuilderCommon.GetScopedName(nsHost.Name, this.type.Name));

            // Idiotic Partial class for initialisation.
            this.typedeclInitHostMakerRegistry = new CodeTypeDeclaration("InitHostMakerRegistry");
            this.typedeclInitHostMakerRegistry.IsPartial = true;
            this.typedeclInitHostMakerRegistry.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this.typerefInitService = new CodeTypeReference(this.typeRegistryListEntry);
            this.typerefInitService.TypeArguments.Add(typeref);
            this.fieldAddInit = new CodeMemberField(typerefRegistryListEntryBase, "add" + typerefWCFService.BaseType.Replace('.', '_'));
        }

        public override void GenerateMethods(IDLInterface idlIntf)
        {
            if ((idlIntf.Methods != null && idlIntf.Methods.Count > 0) || (idlIntf.Properties != null && idlIntf.Properties.Count > 0) || (idlIntf.Signals != null && idlIntf.Signals.Count > 0))
            {
                this.fieldAddInit.InitExpression = new CodeObjectCreateExpression(this.typerefInitService);
                this.typedeclInitHostMakerRegistry.Members.Add(this.fieldAddInit);
            }
            else
            {
                this.fieldAddInit.InitExpression = new CodePrimitiveExpression(null);
                this.fieldAddInit.Comments.Add(new CodeCommentStatement("You may be expecting a field here, but since there are no methods or properties, there's no WCF interface."));
            }
        }

        public override void PostHandleMembers(IDLInterface idlIntf)
        {
            this.nsInit.Types.Add(this.typedeclInitHostMakerRegistry);

            this.unit.Namespaces.Add(this.nsHost);
        }

    } // Ends class WCFHostBuilder
} // Ends dbusidltocode
