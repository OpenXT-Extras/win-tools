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

//#define USE_FLUID_MESSAGE_BUILDER
#define MAKEPROPERTYGET

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
    internal class MarshalBuilder : Builder
    {
        private static string PropertyNameFromFieldName(string fieldName)
        {
            return fieldName[0].ToString().ToUpper() + fieldName.Substring(1);
        }

        const string constructionRegion = "Construction/Destruction";
        const string createRegion = "Static Creation";
        const string propertiesRegion = "Properties";
        const string fieldsRegion = "Fields";

        const string SerialManagerName = "serialManager";
        const string DefaultConnectionParametersName = "connectionParametersDefault";
        const string ResponseHandleName = "msgHandleResp";

        static readonly CodeFieldReferenceExpression fieldrefResult = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.nameResult);
        static readonly CodeExpression exprResultOk = new CodeBinaryOperatorExpression(fieldrefResult, CodeBinaryOperatorType.IdentityEquality, CodeBuilderCommon.exprBuildResultSuccessValue);
        static readonly CodeFieldReferenceExpression fieldrefConnectionParameters = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.nameConnectionParameters);
        static readonly string receiverpoolPropName = PropertyNameFromFieldName(CodeBuilderCommon.nameReceiverPool);
        static readonly string connectionParametersPropName = PropertyNameFromFieldName(CodeBuilderCommon.nameConnectionParameters);
        
        //PreProcess globals
        Udbus.Parsing.CodeTypeDeferredNamespaceDeclarationHolder declarationHolderDictionaryMarshalNs;
        Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder;
        Udbus.Parsing.CodeTypeDeferredNamespaceDeclarationHolder declarationHolderCustomTypesNs;
        CodeTypeFactory codetypefactoryIn;
        CodeTypeFactory codetypefactoryOut;
        CodeFieldReferenceExpression fieldrefDefaultConnectionParams;
        CodeStatement stmtAssignConnectionParameters;
        CodeStatement stmtAssignServiceConnectionParameters;

        public override void PreProcess()
        {
            DictCreatorFactory dictfactoryIn = new marshal.inward.DictCreatorMarshalInFactory();
            DictCreatorFactory dictfactoryOut = new marshal.outward.DictCreatorMarshalOutFactory();

            codetypefactoryOut = new CodeTypeFactory(new marshal.outward.StructCreatorMarshalOutFactory(), dictfactoryOut, dictfactoryOut);
            codetypefactoryIn = new CodeTypeFactory(new marshal.inward.StructCreatorMarshalInFactory(), dictfactoryIn, dictfactoryIn);

            stmtAssignConnectionParameters = new CodeAssignStatement(fieldrefConnectionParameters, CodeBuilderCommon.argrefConnectionParameters);
            fieldrefDefaultConnectionParams = new CodeFieldReferenceExpression(null, DefaultConnectionParametersName);
            stmtAssignServiceConnectionParameters = new CodeAssignStatement(CodeBuilderCommon.fieldrefServiceConnectionParameters, CodeBuilderCommon.argrefServiceConnectionParameters);

            declarationHolderCustomTypesNs = new Udbus.Parsing.CodeTypeDeferredNamespaceDeclarationHolder(CodeBuilderCommon.nsDbusMarshalCustom);
            // If utilised, this namespace is nested in service's namespace.
            declarationHolderDictionaryMarshalNs = new Udbus.Parsing.CodeTypeDeferredNamespaceDeclarationHolder(CodeBuilderCommon.nsDbusMarshalNested);

            declarationHolder = new Udbus.Parsing.CodeMemberDeferredClassHolder(declarationHolderCustomTypesNs
                , "Marshal"
                , declarationHolderDictionaryMarshalNs
                , CodeBuilderCommon.GetNameMarshalDictionary()
            );
        }

        CodeCommentStatement DestinationOverride;
        CodePrimitiveExpression exprDestinationName;
        CodePrimitiveExpression exprInterfaceName;
        string nsName, ifName, ifPath;

        public override void ProcessNamespaces(IDLInterface idlIntf)
        {
            ifPath = idlIntf.Path;

            nsName = CodeBuilderCommon.GetNamespace(idlIntf.Name, new InterfaceVisitor());
            ifName = CodeBuilderCommon.GetName(idlIntf.Name, new InterfaceVisitor());

            exprInterfaceName = new CodePrimitiveExpression(idlIntf.Name);
            exprDestinationName = generateDbusDestinationName(idlIntf, out DestinationOverride);
            // Setup namespaces.
            if (declarationHolderDictionaryMarshalNs.ns != null) // If dictionary marshalling namespace
            {
                // Reset.
                declarationHolderDictionaryMarshalNs = new Udbus.Parsing.CodeTypeDeferredNamespaceDeclarationHolder(CodeBuilderCommon.nsDbusMarshalNested);
                declarationHolder.ResetDictionary(declarationHolderDictionaryMarshalNs);

            } // Ends if dictionary marshalling namespace

            // Namespace.
            ns = new CodeNamespace(nsName);
            CodeBuilderCommon.AddUsingNamespaces(ns, new MarshalVisitor());
        }

        public override void DeclareCodeType(IDLInterface idlIntf)
        {
            // Marshal class.
            type = new CodeTypeDeclaration(CodeBuilderCommon.GetName(idlIntf.Name, new DBUSServiceVisitor()));
            type.IsClass = true;
            type.TypeAttributes = TypeAttributes.Public;
            type.BaseTypes.Add(new CodeTypeReference(typeof(Udbus.Core.ServiceBase)));
            CodeTypeReference typerefMarshal = new CodeTypeReference(type.Name);

            // Fields...
            // Serial number manager.
            CodeMemberField fieldSerialManager = new CodeMemberField(typeof(Udbus.Core.UdbusSerialNumberManagerThreadsafe), SerialManagerName);
            fieldSerialManager.InitExpression = new CodeObjectCreateExpression(typeof(Udbus.Core.UdbusSerialNumberManagerThreadsafe));
            // Connection parameters.
            CodeMemberField fieldConnectionParameters = new CodeMemberField(CodeBuilderCommon.typerefConnectionParameters, CodeBuilderCommon.nameConnectionParameters);
            // Connection parameter defaults.
            CodeTypeReference typerefReadonlyConnectionParameters = CodeBuilderCommon.GetReadOnlyCodeReference(CodeBuilderCommon.typerefReadonlyConnectionParameters);
            CodeMemberField fieldConnectionParametersDefault = new CodeMemberField(typerefReadonlyConnectionParameters, DefaultConnectionParametersName);
            fieldConnectionParametersDefault.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            fieldConnectionParametersDefault.InitExpression = new CodeObjectCreateExpression(CodeBuilderCommon.typerefReadonlyConnectionParameters,
                exprDestinationName
                , new CodePrimitiveExpression(ifPath)
                , exprInterfaceName
            );

            //If the destination was overridden, insert comment for informational purposes
            if (DestinationOverride != null)
            {
                fieldConnectionParametersDefault.Comments.Add(DestinationOverride);
            }

            // Service Connection parameters.
            CodeMemberField fieldServiceConnectionParameters = new CodeMemberField(CodeBuilderCommon.typerefServiceConnectionParameters,
                CodeBuilderCommon.nameServiceConnectionParameters);

            fieldSerialManager.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, fieldsRegion));
            type.Members.Add(fieldSerialManager);
            type.Members.Add(fieldConnectionParameters);
            type.Members.Add(fieldServiceConnectionParameters);
            type.Members.Add(fieldConnectionParametersDefault);
            fieldConnectionParametersDefault.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, fieldsRegion));

            // Properties...
            CodeMemberProperty propConnectionParameters = new CodeMemberProperty();
            propConnectionParameters.Name = connectionParametersPropName;
            propConnectionParameters.Type = CodeBuilderCommon.typerefConnectionParameters;
            propConnectionParameters.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            propConnectionParameters.GetStatements.Add(new CodeMethodReturnStatement(fieldrefConnectionParameters));
            propConnectionParameters.SetStatements.Add(new CodeAssignStatement(fieldrefConnectionParameters, new CodePropertySetValueReferenceExpression()));

            CodeMemberProperty propDefaultConnectionParameters = new CodeMemberProperty();
            propDefaultConnectionParameters.Name = CodeBuilderCommon.DefaultConnectionParameters;
            propDefaultConnectionParameters.Type = CodeBuilderCommon.typerefReadonlyConnectionParameters;
            propDefaultConnectionParameters.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
            propDefaultConnectionParameters.GetStatements.Add(new CodeMethodReturnStatement(fieldrefDefaultConnectionParams));

            CodeMemberProperty propServiceConnectionParameters = new CodeMemberProperty();
            propServiceConnectionParameters.Name = PropertyNameFromFieldName(CodeBuilderCommon.nameServiceConnectionParameters);
            propServiceConnectionParameters.Type = CodeBuilderCommon.typerefServiceConnectionParameters;
            propServiceConnectionParameters.Attributes = MemberAttributes.Family | MemberAttributes.Final; // Family means Protected (wtf?)
            propServiceConnectionParameters.GetStatements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.fieldrefServiceConnectionParameters));
            CodePropertyReferenceExpression proprefServiceConnectionParameters = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propServiceConnectionParameters.Name);

            CodeMemberProperty propSerialManager = new CodeMemberProperty();
            propSerialManager.Name = PropertyNameFromFieldName(SerialManagerName);
            propSerialManager.Type = new CodeTypeReference(typeof(Udbus.Core.IUdbusSerialNumberManager));
            propSerialManager.Attributes = MemberAttributes.Family | MemberAttributes.Final; // Family means Protected (wtf?)
            propSerialManager.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(CodeBuilderCommon.fieldrefServiceConnectionParameters, propSerialManager.Name)));

            CodeMemberProperty propReceiverPool = new CodeMemberProperty();
            propReceiverPool.Name = PropertyNameFromFieldName(receiverpoolPropName);
            propReceiverPool.Type = new CodeTypeReference(typeof(Udbus.Core.DbusMessageReceiverPool));
            propReceiverPool.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            // TODO - this should reference a field in the service connection parameters.
            //propReceiverPool.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.nameReceiverPool)));
            propReceiverPool.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(proprefServiceConnectionParameters, "ReceiverPool")));

            propConnectionParameters.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, propertiesRegion));
            type.Members.Add(propConnectionParameters);
            type.Members.Add(propDefaultConnectionParameters);
            type.Members.Add(propServiceConnectionParameters);
            type.Members.Add(propSerialManager);
            type.Members.Add(propReceiverPool);
            propReceiverPool.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, propertiesRegion));

            // Constructors...
            int firstConstructorIndex = type.Members.Count;

            // Constructors without ConnectionParameters.
            // * public configService() :
            // *        this(new Udbus.Core.ServiceConnectionParams(), connectionParametersDefault)
            CodeConstructor constructorNoParams = new CodeConstructor();
            constructorNoParams.Attributes = MemberAttributes.Public;
            constructorNoParams.ChainedConstructorArgs.Add(new CodeObjectCreateExpression(CodeBuilderCommon.typerefServiceConnectionParameters));
            constructorNoParams.ChainedConstructorArgs.Add(fieldrefDefaultConnectionParams);

            // * public configService(Udbus.Core.DbusConnectionParameters connectionParameters)
            CodeConstructor constructorParams = new CodeConstructor();
            constructorParams.Attributes = MemberAttributes.Public;
            constructorParams.Parameters.Add(CodeBuilderCommon.paramdeclConnectionParams);
            constructorParams.Statements.Add(stmtAssignConnectionParameters);

            // Constructor with ServiceConnectionParams and DbusConnectionParams
            // * public configService(Udbus.Core.ServiceConnectionParams serviceConnectionParameters, Udbus.Core.DbusConnectionParameters connectionParameters) :
            // *         base(serviceConnectionParameters)
            CodeConstructor constructorServiceDbusParams = MakeServiceConnectionAndDbusParamsConstructor(stmtAssignConnectionParameters, stmtAssignServiceConnectionParameters);

            // Constructor with ServiceConnectionParams
            // * public configService(Udbus.Core.ServiceConnectionParams serviceConnectionParameters) :
            // *         this(serviceConnectionParameters, connectionParametersDefault)
            CodeConstructor constructorServiceParams = new CodeConstructor();
            constructorServiceParams.Attributes = MemberAttributes.Public;
            constructorServiceParams.Parameters.Add(CodeBuilderCommon.paramdeclServiceConnectionParams);
            constructorServiceParams.ChainedConstructorArgs.Add(CodeBuilderCommon.argrefServiceConnectionParameters);
            constructorServiceParams.ChainedConstructorArgs.Add(fieldrefDefaultConnectionParams);

            CodeConstructor[] constructors = { constructorServiceDbusParams, constructorServiceParams, constructorParams, constructorNoParams };
            constructorServiceDbusParams.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, constructionRegion));
            type.Members.Add(constructorServiceDbusParams);
            type.Members.Add(constructorServiceParams);
            type.Members.Add(constructorParams);
            type.Members.Add(constructorNoParams);
            constructorNoParams.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, constructionRegion));

            // Static creation functions.
            int firstStaticCreationMethodIndex = type.Members.Count;
            for (int nConstructorIter = firstConstructorIndex; nConstructorIter < firstStaticCreationMethodIndex; ++nConstructorIter)
            {
                CodeConstructor iterConstructor = constructors[nConstructorIter - firstConstructorIndex];
                CodeMemberMethod methodStaticCreate = new CodeMemberMethod();
                methodStaticCreate.Name = "Create";
                methodStaticCreate.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                methodStaticCreate.ReturnType = typerefMarshal;
                methodStaticCreate.Parameters.AddRange(iterConstructor.Parameters);
                CodeParameterDeclarationExpression[] paramdeclsConstructor = new CodeParameterDeclarationExpression[iterConstructor.Parameters.Count];
                iterConstructor.Parameters.CopyTo(paramdeclsConstructor, 0);
                CodeArgumentReferenceExpression[] argrefsConstructor = (from p in paramdeclsConstructor
                                                                        select new CodeArgumentReferenceExpression(p.Name)).ToArray();
                CodeObjectCreateExpression createExpression = new CodeObjectCreateExpression(typerefMarshal, argrefsConstructor);
                CodeMethodReturnStatement retstmtCreate = new CodeMethodReturnStatement(createExpression);
                methodStaticCreate.Statements.Add(retstmtCreate);
                type.Members.Add(methodStaticCreate);
            }

            if (type.Members.Count != firstStaticCreationMethodIndex) // If added static create methods
            {
                type.Members[firstStaticCreationMethodIndex].StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, createRegion));
                type.Members[type.Members.Count - 1].EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, createRegion));

            } // Ends if added static create methods

            // Make class implement interface.
            type.BaseTypes.Add(ifName);
        }

        public override void GenerateMethods(IDLInterface idlIntf)
        {
            // Methods.
            if (idlIntf.Methods != null && idlIntf.Methods.Count > 0) // If got methods
            {
                int firstMethodIndex = type.Members.Count;

                foreach (IDLMethod idlMethod in idlIntf.Methods)
                {
                    CodeMemberMethod method = MakeMethod(codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, idlMethod);
                    type.Members.Add(method);

                } // Ends loop over methods

                string methodsRegion = string.Format("{0} methods implementation", ifName);
                type.Members[firstMethodIndex].StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, methodsRegion));
                type.Members[type.Members.Count - 1].EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, methodsRegion));

            } // Ends if got methods
        }


        public override void GenerateProperties(IDLInterface idlIntf)
        {
            // Properties.
            if (idlIntf.Properties != null && idlIntf.Properties.Count > 0) // If got properties
            {
                int firstPropertyIndex = type.Members.Count - 1;
                Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);

                DictCreatorFactory dictfactoryPropertyOut = new marshal.outward.DictCreatorMarshalOutPropertyFactory();
                CodeTypeFactory codetypefactoryPropertyOut = new CodeTypeFactory(new marshal.outward.StructCreatorMarshalOutFactory(), dictfactoryPropertyOut, dictfactoryPropertyOut);

                foreach (IDLProperty idlProperty in idlIntf.Properties)
                {
                    bool hasGet = CodeBuilderCommon.HasGet(idlProperty);
                    bool hasSet = CodeBuilderCommon.HasSet(idlProperty);

                    if (!hasGet && !hasSet)
                    {
                        continue;
                    }

                    MakeProperty(codetypefactoryPropertyOut, codetypefactoryIn, declarationHolder, idlIntf, type, idlProperty, hasGet, hasSet, context);

                } // Ends loop over properties
            } // Ends if got properties
        }


        public override void GenerateSignals(IDLInterface idlIntf)
        {
            // Signals.
            if (idlIntf.Signals != null && idlIntf.Signals.Count > 0) // If got signals
            {
                int firstSignalIndex = type.Members.Count;

                // * private void AddMatchInternal(string matchrule)
                // * protected void RegisterForSignal(string matchrule, Udbus.Core.SignalEntry signalEntry, params Udbus.Core.IRegisterSignalHandlers[] registers)
                PreMakeSignals(type, codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf);

                foreach (IDLSignal idlSignal in idlIntf.Signals)
                {
                    CodeMemberMethod method = MakeSignal(ns, type, codetypefactoryOut, declarationHolder, idlIntf, idlSignal);
                    type.Members.Add(method);
                } // Ends loop over methods

                string signalsRegion = string.Format("{0} signals implementation", ifName);
                type.Members[firstSignalIndex].StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, signalsRegion));
                type.Members[type.Members.Count - 1].EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, signalsRegion));

            } // Ends if got signals
        }


        public override void PostHandleMembers(IDLInterface idlIntf)
        {
            ns.Types.Add(type);
            unit.Namespaces.Add(ns);

            if (declarationHolderCustomTypesNs.ns != null) // If custom types namespace
            {
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.nsDbusMarshalCustom));

            } // Ends if custom types namespace

            if (declarationHolderDictionaryMarshalNs.ns != null) // If dictionary marshalling namespace
            {
                declarationHolderDictionaryMarshalNs.ns.Imports.Add(new CodeNamespaceImport("System"));
                declarationHolderDictionaryMarshalNs.ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

                // Dictionary marshalling namespace is nested inside service namespace.
                declarationHolderDictionaryMarshalNs.ns.Name = CodeBuilderCommon.GetScopedName(ns.Name, declarationHolderDictionaryMarshalNs.ns.Name);
                unit.Namespaces.Add(declarationHolderDictionaryMarshalNs.ns);

            } // Ends if dictionary marshalling namespace
        }

        public override void PostProcess()
        {
            if (declarationHolder.typedecl != null) // If custom types class
            {
                declarationHolder.typedecl.IsPartial = true;

            } // Ends if custom types class

            if (declarationHolderCustomTypesNs.ns != null) // If custom types namespace
            {
                declarationHolderCustomTypesNs.ns.Imports.Add(new CodeNamespaceImport("System"));
                declarationHolderCustomTypesNs.ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                unit.Namespaces.Add(declarationHolderCustomTypesNs.ns);

            } // Ends if custom types namespace
        }

        /*public override void Generate(IEnumerable<IDLInterface> interfaces, TextWriter writer)
        {
            unit = new CodeCompileUnit();
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions genOptions = CodeBuilderHelper.getCodeGeneratorOptions();

            this.PreProcess();

            foreach (IDLInterface idlIntf in interfaces)
            {
                this.ProcessNamespaces(idlIntf);
                this.DeclareCodeType(idlIntf);
                this.GenerateMethods(idlIntf);
                this.GenerateProperties(idlIntf);
                this.GenerateSignals(idlIntf);
                this.PostHandleMembers(idlIntf);
            } // Ends loop over interfaces

            this.PostProcess();
            // Finally generate the code
            provider.GenerateCodeFromCompileUnit(unit, writer, genOptions);
        }*/

        /// <summary>
        /// Could do this through a config file, but really, who gives a shit.
        /// </summary>
        /// <param name="idlIntf"></param>
        /// <returns></returns>
        private static CodePrimitiveExpression generateDbusDestinationName(IDLInterface idlIntf, out CodeCommentStatement comment)
        {
            string destination;
            bool bHacked = true;

            switch (idlIntf.Name)
            {
                //Insert any case statements here for when you might want to over-ride destination names for example:
                //case "com.citrix.xenclient.xenmgr.config":
                //    destination = "com.citrix.xenclient.xenmgr";
                //    break;

                default:
                    destination = idlIntf.Name;
                    bHacked = false;
                    break;
            }

            CodePrimitiveExpression exprDestinationName = new CodePrimitiveExpression(destination);

            if (bHacked)
            {
                comment = new CodeCommentStatement(string.Format("Overrode destination '{0}' to '{1}'", idlIntf.Name, destination));
            }
            else
            {
                comment = null;
            }
            return exprDestinationName;
        }

        private static void MakeProperty(CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn, Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf, CodeTypeDeclaration typeMarshal, IDLProperty idlProperty, bool hasGet, bool hasSet,
            Udbus.Parsing.BuildContext context)
        {
            CodeMemberProperty property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            property.Name = CodeBuilderCommon.GetCompilableName(idlProperty.Name);
            property.HasGet = hasGet;
            property.HasSet = hasSet;

            property.Comments.Add(new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlProperty.Access, idlProperty.Name, idlProperty.Type)));
            // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
            // We use the out factory because its out types we have to return and they tend to be less forgiving than in types.
            property.Type = CodeBuilderCommon.PropertyType(codetypefactoryOut, idlProperty.Type);

            if (hasGet) // If gettable property
            {
                MakePropertyGet(property, codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, idlProperty, context);

            } // Ends if gettable property

            if (hasSet) // If settable property
            {
                MakePropertySet(property, codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, idlProperty, context);

            } // Ends if settable property

            typeMarshal.Members.Add(property);
        }

        private static CodeConstructor MakeServiceConnectionAndDbusParamsConstructor(CodeStatement stmtAssignConnectionParameters, CodeStatement stmtAssignServiceConnectionParameters)
        {
            CodeConstructor constructorServiceDbusParams = new CodeConstructor();
            constructorServiceDbusParams.Attributes = MemberAttributes.Public;
            constructorServiceDbusParams.Parameters.Add(CodeBuilderCommon.paramdeclServiceConnectionParams);
            constructorServiceDbusParams.Parameters.Add(CodeBuilderCommon.paramdeclConnectionParams);
            constructorServiceDbusParams.BaseConstructorArgs.Add(CodeBuilderCommon.argrefServiceConnectionParameters);
            constructorServiceDbusParams.Statements.Add(stmtAssignConnectionParameters);
            constructorServiceDbusParams.Statements.Add(stmtAssignServiceConnectionParameters);
            return constructorServiceDbusParams;
        }

        private static CodeMemberProperty MakePropertyGet(CodeMemberProperty property, CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLProperty idlProperty,
            Udbus.Parsing.BuildContext context)
        {
            return MakePropertyGet(property, codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, idlProperty, context, new MarshalBuilderHelperProperty());
        }

        private static CodeMemberProperty MakePropertyGet(CodeMemberProperty property, CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLProperty idlProperty,
            Udbus.Parsing.BuildContext context,
            MarshalBuilderHelper codebuilder)
        {
            List < IDLMethodArgument > arguments = new List<IDLMethodArgument>(new IDLMethodArgument[]
            {
                //<arg name="interface_name" type="s"/>
                //<arg name="property_name" type="s"/>
                new IDLMethodArgument{ Name="interface_name", Type="s", Direction="in" },
                new IDLMethodArgument{ Name="property_name", Type="s", Direction="in" },
                //<arg name="value" type="<PropertyType>" direction="out"/>
                new IDLMethodArgument{ Name="value", Type=idlProperty.Type, Direction="out"}
            }
            );

            MakeMethodParameters(codetypefactoryOut, codetypefactoryIn,
                declarationHolder,
                idlIntf,
                "Get",
                idlProperty.Name,
                arguments,
                new CodeParameterDeclarationExpressionCollection(),
                property.GetStatements,
                context,
                codebuilder
            );

            property.GetStatements.Add(new CodeMethodReturnStatement(CodeBuilderCommon.varrefReadValue));
            return property;
        }

        private static CodeMemberProperty MakePropertySet(CodeMemberProperty property, CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLProperty idlProperty,
            Udbus.Parsing.BuildContext context)
        {
            return MakePropertySet(property, codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, idlProperty, context, new MarshalBuilderHelperProperty());
        }

        private static CodeMemberProperty MakePropertySet(CodeMemberProperty property, CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLProperty idlProperty,
            Udbus.Parsing.BuildContext context,
            MarshalBuilderHelper codebuilder)
        {
            List<IDLMethodArgument> arguments = new List<IDLMethodArgument>(new IDLMethodArgument[]
            {
                //<arg name="interface_name" type="s"/>
                //<arg name="property_name" type="s"/>
                new IDLMethodArgument{ Name="interface_name", Type="s", Direction="in" },
                new IDLMethodArgument{ Name="property_name", Type="s", Direction="in" },
                //<arg name="value" type="<PropertyType>"/>
                new IDLMethodArgument{ Name="value", Type=idlProperty.Type, Direction="in"}
            }
            );
            MakeMethodParameters(codetypefactoryOut, codetypefactoryIn,
                declarationHolder,
                idlIntf,
                "Set",
                idlProperty.Name,
                arguments,
                new CodeParameterDeclarationExpressionCollection(),
                property.SetStatements,
                context,
                codebuilder
            );
            return property;
        }


        #region Signals
        static public CodeMemberMethod MakeSignal(CodeNamespace  ns,
            CodeTypeDeclaration typeMarshal,
            CodeTypeFactory codetypefactoryOut,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLSignal idlMethod)
        {
            return MakeSignal(ns, typeMarshal, codetypefactoryOut, declarationHolder, idlIntf, idlMethod, new MarshalBuilderHelperSignal());
        }

        // WAXME
        //internal static readonly CodeTypeReference typerefSignalEventArgs = new CodeTypeReference(typeof(System.EventArgs));
        internal static readonly CodeTypeReference typerefSignalKey = new CodeTypeReference(typeof(Udbus.Core.SignalKey));
        internal const string SignalKeyDefault = "SignalKeyDefault";
        internal static readonly IDLMethod AddMatchInternal = new IDLMethod
        {
            Name = "AddMatch"
            , Arguments = new List<IDLMethodArgument>()
            {
                new IDLMethodArgument { Direction="in", Name="matchrule", Type="s" }
            }
        };
        internal const string AddMatchInternalName = "AddMatchInternal";

        internal static readonly CodeTypeReference typerefIRegisterSignalHandlers = new CodeTypeReference(typeof(Udbus.Core.IRegisterSignalHandlers));
        internal static readonly CodeTypeReference typerefIRegisterSignalHandlersArray = new CodeTypeReference(typeof(Udbus.Core.IRegisterSignalHandlers[]));
        internal const string register = "register";
        internal static readonly CodeParameterDeclarationExpression paramdeclRegister = new CodeParameterDeclarationExpression(typerefIRegisterSignalHandlers, register);
        internal static readonly CodeArgumentReferenceExpression argrefRegister = new CodeArgumentReferenceExpression(register);
        internal const string AddSignalHandler = "AddSignalHandler";
        internal static readonly CodeTypeReference typerefSignalEntry = new CodeTypeReference(typeof(Udbus.Core.SignalEntry));

        internal const string RegisterSignalHandlers = "RegisterSignalHandlers";
        internal const string RegisterForSignal = "RegisterForSignal";
        internal const string registers = "registers";
        internal const string matchrule = "matchrule";
        internal static readonly CodePropertyReferenceExpression proprefRegisterSignalHandlers = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), RegisterSignalHandlers);
        internal static readonly CodePropertyReferenceExpression proprefServiceParamsRegisterSignalHandlers = new CodePropertyReferenceExpression(CodeBuilderCommon.fieldrefServiceConnectionParameters, RegisterSignalHandlers);
        internal static readonly CodeMethodReturnStatement returnServiceParamsRegisterSignalHandlers = new CodeMethodReturnStatement(proprefServiceParamsRegisterSignalHandlers);
        internal static readonly CodeParameterDeclarationExpression paramdeclMatchRule = new CodeParameterDeclarationExpression(typeof(string), matchrule);
        internal static readonly CodeParameterDeclarationExpression paramdeclSignalEntry = new CodeParameterDeclarationExpression(typerefSignalEntry, "signalEntry");
        internal static readonly CodeArgumentReferenceExpression argrefMatchrule = new CodeArgumentReferenceExpression(paramdeclMatchRule.Name);
        internal static readonly CodeArgumentReferenceExpression argrefSignalEntry = new CodeArgumentReferenceExpression(paramdeclSignalEntry.Name);
        internal static readonly CodeArgumentReferenceExpression argrefRegisters = new CodeArgumentReferenceExpression(registers);
        internal static readonly CodeMethodInvokeExpression invokeAddMatchInternal = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), AddMatchInternalName, argrefMatchrule);
        internal static readonly CodeTypeReference typerefRegisterSignalHandlerFunctions = new CodeTypeReference(typeof(Udbus.Core.RegisterSignalHandlerFunctions));
        internal static readonly CodeTypeReferenceExpression typerefexprRegisterSignalHandlerFunctions = new CodeTypeReferenceExpression(typerefRegisterSignalHandlerFunctions);
        internal static readonly CodeMethodInvokeExpression invokeRegisterForSignal = new CodeMethodInvokeExpression(typerefexprRegisterSignalHandlerFunctions, RegisterForSignal, argrefSignalEntry, argrefRegisters);
        internal static readonly CodePropertyReferenceExpression proprefConnectionParameters = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), connectionParametersPropName);
        internal static readonly CodePropertyReferenceExpression proprefConnectionParametersPath = new CodePropertyReferenceExpression(proprefConnectionParameters, "Path");
        internal static readonly CodePropertyReferenceExpression proprefConnectionParametersInterface = new CodePropertyReferenceExpression(proprefConnectionParameters, "Interface");

        static public CodeMemberMethod MakeSignal(CodeNamespace ns, CodeTypeDeclaration typeMarshal, CodeTypeFactory codetypefactoryOut,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLSignal idlSignal,
            MarshalBuilderHelper codebuilder)
        {
            string signalMethodName = "Handle" + CodeBuilderCommon.GetSignalTypeName(idlSignal.Name);

            //// * public class <signal_as_type>Args : EventArgs
            CodeTypeDeclaration typeArgs = new CodeTypeDeclaration(CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name));
            //typeArgs.BaseTypes.Add(typerefSignalEventArgs);

            // * const string StorageSpaceLowMatchRule = "type=\'signal\',interface=\'<interface>\',member=\'<signal>'";
            CodeMemberField fieldMatchRule = new CodeMemberField(typeof(string), CodeBuilderCommon.GetSignalCompilableName(idlSignal.Name) + "MatchRule");
            fieldMatchRule.InitExpression = new CodePrimitiveExpression(string.Format("type='signal',interface='{0}',member='{1}'", idlIntf.Name, idlSignal.Name));
            fieldMatchRule.Attributes = MemberAttributes.Private | MemberAttributes.Const;
            typeMarshal.Members.Add(fieldMatchRule);
            CodeFieldReferenceExpression fieldrefMatchrule = new CodeFieldReferenceExpression(null, fieldMatchRule.Name);

            // * private Udbus.Core.SignalKey StorageSpaceLowSignalKey { get { return new Udbus.Core.SignalKey(this.ConnectionParameters.Path, this.ConnectionParameters.Interface, "storage_space_low"); } }
            // * private Udbus.Core.SignalEntry StorageSpaceLowSignalEntry { get { return new Udbus.Core.SignalEntry(this.StorageSpaceLowSignalKey, this.HandleStorageSpaceLow); } }
            CodeMethodReferenceExpression methodrefSignal = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), signalMethodName);
            CodeMemberProperty propSignalKey = new CodeMemberProperty();
            propSignalKey.Name = CodeBuilderCommon.GetSignalCompilableName(idlSignal.Name) + "Key";
            propSignalKey.Type = typerefSignalKey;
            propSignalKey.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            propSignalKey.GetStatements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(typerefSignalKey
                , proprefConnectionParametersPath, proprefConnectionParametersInterface, new CodePrimitiveExpression(idlSignal.Name)
            )));
            CodeMemberProperty propSignalEntry = new CodeMemberProperty();
            propSignalEntry.Name = CodeBuilderCommon.GetSignalCompilableName(idlSignal.Name) + "Entry";
            propSignalEntry.Type = typerefSignalEntry;
            propSignalEntry.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            propSignalEntry.GetStatements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(typerefSignalEntry
                , new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propSignalKey.Name)
                , methodrefSignal
            )));
            typeMarshal.Members.Add(propSignalKey);
            typeMarshal.Members.Add(propSignalEntry);

            // * public virtual void RegisterForStorageSpaceLow()
            // * {
            // *     this.RegisterForSignal(StorageSpaceLowMatchRule, this.StorageSpaceLowSignalEntry, this.RegisterSignalHandlers);
            // * }
            CodeMemberMethod methodRegisterForSpecificSignal = new CodeMemberMethod();
            methodRegisterForSpecificSignal.Name = CodeBuilderCommon.GetSignalRegisterFunction(idlSignal.Name);
            methodRegisterForSpecificSignal.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), RegisterForSignal
                , fieldrefMatchrule
                , new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propSignalEntry.Name)
                , new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), proprefRegisterSignalHandlers.PropertyName)
            ));
            typeMarshal.Members.Add(methodRegisterForSpecificSignal);

            CodeMemberEvent eventSignal = CodeBuilderCommon.CreateSignalEvent(idlSignal);
            CodeVariableReferenceExpression varrefSignalHandler = new CodeVariableReferenceExpression("signalHandler");
            CodeEventReferenceExpression eventrefSignalHandlerSignal = new CodeEventReferenceExpression(varrefSignalHandler, eventSignal.Name);
            CodeMethodInvokeExpression invokeGetSignalHandler = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), GetSignalHandler);
            CodeTypeReference typerefMarshal = new CodeTypeReference(typeMarshal.Name);
            CodeMemberProperty propEventSignal = new CodeMemberProperty();
            propEventSignal.Name = CodeBuilderCommon.GetSignalEventPropertyName(idlSignal.Name);
            propEventSignal.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            propEventSignal.Type = eventSignal.Type;
            // * return this.GetSignalHandler().storageSpaceLowEvent;
            propEventSignal.GetStatements.Add(new CodeMethodReturnStatement(new CodeEventReferenceExpression(invokeGetSignalHandler, eventSignal.Name)));

            // * <service> signalHandler = this.GetSignalHandler();
            propEventSignal.SetStatements.Add(new CodeVariableDeclarationStatement(typerefMarshal, varrefSignalHandler.VariableName, invokeGetSignalHandler));
            // * if (signalHandler.<signal>Event == null)
            propEventSignal.SetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(eventrefSignalHandlerSignal, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
                // *     signalHandler.RegisterFor<signal>();
                , new CodeExpressionStatement(new CodeMethodInvokeExpression(varrefSignalHandler, methodRegisterForSpecificSignal.Name))
            ));
            // * signalHandler.<signal>Event = value;
            propEventSignal.SetStatements.Add(
                new CodeAssignStatement(eventrefSignalHandlerSignal, new CodePropertySetValueReferenceExpression())
            );
            // * private event System.EventHandler< <signal>Args > <signal>Event { ... }
            // * public System.EventHandler< <signal>Args > <signal>;
            typeMarshal.Members.Add(eventSignal);
            typeMarshal.Members.Add(propEventSignal);

            // * public event System.EventHandler< <signal>Args > <signal>;
            //string eventPropertyName = CodeBuilderCommon.GetSignalEventName(idlSignal.Name);
            //CodeMemberEvent eventSignal = new CodeMemberEvent();
            //eventSignal.Attributes = MemberAttributes.Public;
            //eventSignal.Name = CodeBuilderCommon.GetSignalEventName(idlSignal.Name);
            //CodeTypeReference typerefEvent = new CodeTypeReference(typeof(System.EventHandler<>));
            //typerefEvent.TypeArguments.Add(new CodeTypeReference(CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name)));
            //eventSignal.Type = typerefEvent;
            //typeMarshal.Members.Add(eventSignal);

            //// * public void RegisterFor<signal_compilable>()
            //// * {
            //// *     this.AddMatchInternal("type='signal',interface='<interface>',member='<signal>'");
            //// * }
            //CodeMemberMethod methodRegister = new CodeMemberMethod();
            //methodRegister.Attributes = MemberAttributes.Public;
            //methodRegister.Name = GetSignalRegisterFunctionName(idlSignal.Name);
            //methodRegister.Statements.Add(
            //    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), AddMatchInternal.Name
            //        ,new CodePrimitiveExpression(string.Format(
            //            "type='signal',interface='{0}',member='{1}'", idlIntf.Name, idlSignal.Name
            //        ))
            //    )
            //);
            //typeMarshal.Members.Add(methodRegister);

            //CodeMemberMethod methodRegisterWithInterface = new CodeMemberMethod();
            //methodRegisterWithInterface.Attributes = MemberAttributes.Public;
            //methodRegisterWithInterface.Name = methodRegister.Name;
            //methodRegisterWithInterface.Parameters.Add(new CodeParameterDeclarationExpression(typerefIRegisterSignalHandlers, register));
            //methodRegisterWithInterface.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodRegister.Name));
            //// * new Udbus.Core.SignalEntry(new Udbus.Core.SignalKey(this.ConnectionParameters.Path, this.ConnectionParameters.Interface, "storage_space_low")
            //methodRegisterWithInterface.Statements.Add(new CodeMethodInvokeExpression(argrefRegister, AddSignalHandler
            //    , new CodeObjectCreateExpression(typerefSignalEntry
            //        , new CodeObjectCreateExpression(typerefSignalKey
            //            , MarshalBuilderHelper.proprefThisConnectionParametersPath
            //            , MarshalBuilderHelper.proprefThisConnectionParametersInterface
            //            , new CodePrimitiveExpression(idlSignal.Name)
            //        )
            //        , methodrefSignal
            //    )
            //));
            //typeMarshal.Members.Add(methodRegisterWithInterface);

            // * Handle<signal>(<signal>Args args)
            CodeMemberMethod method = new CodeMemberMethod();
            CodeComment commentMethod = new CodeComment(idlSignal.Name);
            method.Comments.Add(new CodeCommentStatement(idlSignal.Name));
            method.Name = signalMethodName;
            method.Attributes = MemberAttributes.Public;
            method.Parameters.Add(MarshalBuilderHelper.paramdeclMessageResponse);

            foreach (IDLSignalArgument idlSignalArg in idlSignal.Arguments)
            {
                method.Comments.Add(new CodeCommentStatement(string.Format(" {0} \"{1}\"", idlSignalArg.Name, idlSignalArg.Type)));
            }

            // Context used for all arguments in method.
            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);
            MakeSignalParameters(typeArgs, codetypefactoryOut,
                declarationHolder,
                idlIntf,
                method.Name,
                idlSignal.Name,
                idlSignal.Arguments,
                method.Parameters,
                method.Statements,
                context,
                codebuilder
            );

            //ns.Types.Add(typeArgs);
            return method;
        }

        private const string GetSignalHandler = "GetSignalHandler";

        static public void PreMakeSignals(CodeTypeDeclaration typeMarshal,
            CodeTypeFactory codetypefactoryOut,
            CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf
            )
        {
            // * private void AddMatchInternal(string matchrule)
            CodeMemberMethod methodAddMatch = MakeMethod(codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, AddMatchInternal, new MarshalBuilderHelperSignalAddMatchMethod());
            methodAddMatch.Name = AddMatchInternalName; // Change the name to prevent name collision.
            methodAddMatch.Attributes = MemberAttributes.Private;
            typeMarshal.Members.Add(methodAddMatch);

            // * protected void RegisterForSignal(string matchrule, Udbus.Core.SignalEntry signalEntry, params Udbus.Core.IRegisterSignalHandlers[] registers)
            // * {
            // *     this.AddMatchInternal(matchrule);
            // *     Udbus.Core.RegisterSignalHandlerFunctions.RegisterForSignal(signalEntry, registers);
            // * }
            CodeMemberMethod methodRegisterForSignal = new CodeMemberMethod();
            methodRegisterForSignal.Name = RegisterForSignal;
            CodeParameterDeclarationExpression paramdeclRegisters = new CodeParameterDeclarationExpression(typerefIRegisterSignalHandlersArray, registers);
            paramdeclRegisters.CustomAttributes.Add(CodeBuilderCommon.attribParams);
            methodRegisterForSignal.Parameters.Add(paramdeclMatchRule);
            methodRegisterForSignal.Parameters.Add(paramdeclSignalEntry);
            methodRegisterForSignal.Parameters.Add(paramdeclRegisters);
            methodRegisterForSignal.Statements.Add(invokeAddMatchInternal);
            methodRegisterForSignal.Statements.Add(invokeRegisterForSignal);
            typeMarshal.Members.Add(methodRegisterForSignal);

            // * protected Udbus.Core.IRegisterSignalHandlers RegisterSignalHandlers { get { return this.serviceConnectionParameters.RegisterSignalHandlers; } }
            CodeMemberProperty propRegisterSignalHandlers = new CodeMemberProperty();
            propRegisterSignalHandlers.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            propRegisterSignalHandlers.Name = proprefRegisterSignalHandlers.PropertyName;

            propRegisterSignalHandlers.Type = typerefIRegisterSignalHandlers;
            propRegisterSignalHandlers.GetStatements.Add(returnServiceParamsRegisterSignalHandlers);
            typeMarshal.Members.Add(propRegisterSignalHandlers);

            // * private static object lockGlobalSignalHandlers = new object();
            CodeMemberField fieldLockGlobalSignalHandlers = new CodeMemberField(typeof(object), "lockGlobalSignalHandlers");
            fieldLockGlobalSignalHandlers.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            fieldLockGlobalSignalHandlers.InitExpression = new CodeObjectCreateExpression(fieldLockGlobalSignalHandlers.Type);
            typeMarshal.Members.Add(fieldLockGlobalSignalHandlers);
            // * private static System.Collections.Generic.Dictionary<Udbus.Serialization.DbusConnectionParameters, <interface>Service> globalSignalHandlers = new System.Collections.Generic.Dictionary<Udbus.Serialization.DbusConnectionParameters, <interface>Service);
            CodeTypeReference typerefGlobalSignalHandlers = new CodeTypeReference(typeof(Dictionary<,>));
            CodeTypeReference typerefMarshal = new CodeTypeReference(typeMarshal.Name);
            typerefGlobalSignalHandlers.TypeArguments.Add(CodeBuilderCommon.typerefConnectionParameters);
            typerefGlobalSignalHandlers.TypeArguments.Add(typerefMarshal);
            CodeMemberField fieldGlobalSignalHandlers = new CodeMemberField(typerefGlobalSignalHandlers, "globalSignalHandlers");
            fieldGlobalSignalHandlers.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            fieldGlobalSignalHandlers.InitExpression = new CodeObjectCreateExpression(fieldGlobalSignalHandlers.Type);
            typeMarshal.Members.Add(fieldGlobalSignalHandlers);

            CodeMemberMethod methodGetSignalHandler = new CodeMemberMethod();
            methodGetSignalHandler.Name = GetSignalHandler;
            methodGetSignalHandler.Attributes = MemberAttributes.Private;
            methodGetSignalHandler.ReturnType = typerefMarshal;
            CodeVariableReferenceExpression varrefLocalSignalHandler = new CodeVariableReferenceExpression("localSignalHandler");
            CodeVariableReferenceExpression varrefLookupSignalHandler = new CodeVariableReferenceExpression("lookupSignalHandler");
            CodeFieldReferenceExpression fieldrefLockGlobalSignalHandlers = new CodeFieldReferenceExpression(null, fieldLockGlobalSignalHandlers.Name);
            CodeFieldReferenceExpression fieldrefGlobalSignalHandlers = new CodeFieldReferenceExpression(null,  fieldGlobalSignalHandlers.Name);

            methodGetSignalHandler.Statements.Add(new CodeVariableDeclarationStatement(typerefMarshal, varrefLocalSignalHandler.VariableName, new CodePrimitiveExpression(null)));
            methodGetSignalHandler.Statements.Add(new CodeVariableDeclarationStatement(typerefMarshal, varrefLookupSignalHandler.VariableName));
            CodeTypeReferenceExpression typerefexprMonitor = new CodeTypeReferenceExpression(typeof(System.Threading.Monitor));
            // * System.Threading.Monitor.Enter(lockGlobalSignalHandlers);
            methodGetSignalHandler.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(typerefexprMonitor, "Enter", fieldrefLockGlobalSignalHandlers)));

            methodGetSignalHandler.Statements.Add(new CodeTryCatchFinallyStatement(
                new CodeStatement[] // * try {
                {
                    // * if (globalSignalHandlers.TryGetValue(this.ConnectionParameters, out lookupSignalHandler) == false)
                    // * {
                    new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(fieldrefGlobalSignalHandlers, "TryGetValue"
                        , proprefConnectionParameters, new CodeDirectionExpression(FieldDirection.Out, varrefLookupSignalHandler)
                        )
                        , CodeBinaryOperatorType.IdentityEquality
                        , new CodePrimitiveExpression(false)
                        )
                        , new CodeStatement[]
                        {
                            // * globalSignalHandlers[this.ConnectionParameters] = this;
                            new CodeAssignStatement(new CodeIndexerExpression(fieldrefGlobalSignalHandlers, proprefConnectionParameters), new CodeThisReferenceExpression())
                        }
                        , new CodeStatement[] // else
                        {
                            // * localSignalHandler = lookupSignalHandler;
                            new CodeAssignStatement(varrefLocalSignalHandler, varrefLookupSignalHandler)
                        }
                    // * }
                    )
                }
                ,new CodeCatchClause[]{} // * } catch {
                ,new CodeStatement[] // * } finally {
                {
                    // * System.Threading.Monitor.Exit(lockGlobalSignalHandlers);
                    new CodeExpressionStatement(new CodeMethodInvokeExpression(typerefexprMonitor, "Exit", fieldrefLockGlobalSignalHandlers))
                } // * }
            ));

            // * if (localSignalHandler == null)
            methodGetSignalHandler.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(varrefLocalSignalHandler, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
                // * localSignalHandler = this;
                , new CodeAssignStatement(varrefLocalSignalHandler, new CodeThisReferenceExpression())
            ));
            // * return localSignalHandler;
            methodGetSignalHandler.Statements.Add(new CodeMethodReturnStatement(varrefLocalSignalHandler));
            typeMarshal.Members.Add(methodGetSignalHandler);
        }

        static public void MakeSignalParameters(CodeTypeDeclaration typedeclArgs,
            CodeTypeFactory codetypefactoryOut,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            string methodName,
            string idlSignalName,
            IList<IDLSignalArgument> arguments,
            CodeParameterDeclarationExpressionCollection parameters,
            CodeStatementCollection statements,
            Udbus.Parsing.BuildContext context,
            MarshalBuilderHelper codebuilder)
        {
            CodeStatementCollection statementsTryRecv = new CodeStatementCollection();
            int nOutArgCounter = 0;
            List<CodeMethodInvokeExpression> invokemethodsBuild = new List<CodeMethodInvokeExpression>();
            CodeConditionStatement condOut = null; // Root if statement for out parameters.
            CodeConditionStatement condOutIter = null; // Most nested if statement for out parameters.
            CodeStatementCollection stmtsFinishResult = new CodeStatementCollection();
            CodeTypeReference typerefParamIter = CodeBuilderCommon.typerefUnknownParameters;
            string argNameIter = arguments != null && arguments.Count > 0 ? arguments[0].Name : "UnknownParameters";

            CodeThrowExceptionStatement throwargOutPrev = codebuilder.CreateArgumentOutException(idlSignalName);

            // WAXME
            //CodeConstructor constructorArgs = new CodeConstructor();
            //constructorArgs.Attributes = MemberAttributes.Public;
            foreach (IDLSignalArgument idlSignalArg in arguments)
            {
                argNameIter = idlSignalArg.Name;

                // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
                //Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLMethodArgumentTypeNameBuilder(idlIntf, idlMethod, idlMethodArg);
                Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLArgumentTypeNameBuilder(idlIntf, methodName);
                ParamCodeTypeHolderMarshalBase paramtypeHolder = null;
                CodeTypeReference typerefParam = null;
                if (condOut == null)
                {
                    codebuilder.PrefixOutParams(ref condOut, ref condOutIter, idlSignalName, ref nOutArgCounter, ref throwargOutPrev);
                }

                // Handle the signal argument in the message.
                CodeConditionStatement condVarResult;
                codebuilder.MakeOutArgument(statements
                    , stmtsFinishResult
                    , idlSignalName
                    , codetypefactoryOut // Yeah I messed up the naming
                    , ref nOutArgCounter
                    , context
                    , ref throwargOutPrev
                    , idlSignalArg
                    , nameBuilder
                    , ref paramtypeHolder
                    , ref typerefParam
                    , out condVarResult
                );

                codebuilder.StoreCondIterator(ref condOut, ref condOutIter, condVarResult);

                // WAXME
                // Add a field to the <signal>Args class.
                //string argFieldName = CodeBuilderCommon.GetSignalArgFieldName(idlSignalArg.Name);
                //CodeFieldReferenceExpression fielrefdRefArgField = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), argFieldName);
                //typedeclArgs.Members.Add(new CodeMemberField(paramtypeHolder.paramtype.CodeType, argFieldName));
                //CodeMemberProperty propArgField = new CodeMemberProperty();
                //propArgField.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                //propArgField.Type = paramtypeHolder.paramtype.CodeType;
                //propArgField.Name = PropertyNameFromFieldName(argFieldName);
                //propArgField.GetStatements.Add(new CodeMethodReturnStatement(fielrefdRefArgField));
                //typedeclArgs.Members.Add(propArgField);
                //constructorArgs.Parameters.Add(new CodeParameterDeclarationExpression(paramtypeHolder.paramtype.CodeType, argFieldName));
                //// * this.<signal_arg> = <signal_arg>;
                //constructorArgs.Statements.Add(new CodeAssignStatement(fielrefdRefArgField, new CodeArgumentReferenceExpression(argFieldName)));

            } // Ends loop over arguments

            //typedeclArgs.Members.Add(constructorArgs);

            codebuilder.AssignResults(statementsTryRecv, condOut, condOutIter, stmtsFinishResult, throwargOutPrev
                , idlSignalName, ref nOutArgCounter
            );
            List<CodeStatement> statementsReponse = new List<CodeStatement>();

            // Now receive the response.
            // Create message reader.
            // * Udbus.Core.UdbusMessageReader reader = new Udbus.Core.UdbusMessageReader(msgHandleResp);
            statementsTryRecv.Insert(0, codebuilder.CreateMessageReader());
            statementsReponse.AddRange(statementsTryRecv.Cast<CodeStatement>());
            statements.Add(new CodeConditionStatement(exprResultOk, statementsReponse.ToArray()));
        }
        #endregion // Signals

        static public CodeMemberMethod MakeMethod(CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLMethod idlMethod)
        {
            return MakeMethod(codetypefactoryOut, codetypefactoryIn, declarationHolder, idlIntf, idlMethod, new MarshalBuilderHelperMethod());
        }

        static public CodeMemberMethod MakeMethod(CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            IDLMethod idlMethod,
            MarshalBuilderHelper codebuilder)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            CodeComment commentMethod = new CodeComment(idlMethod.Name);
            method.Comments.Add(new CodeCommentStatement(idlMethod.Name));
            method.Name = idlMethod.Name;
            method.Attributes = MemberAttributes.Public;

            foreach (IDLMethodArgument idlMethodArg in idlMethod.Arguments)
            {
                method.Comments.Add(new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlMethodArg.Direction, idlMethodArg.Name, idlMethodArg.Type)));
            }

            // Context used for all arguments in method.
            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);
            MakeMethodParameters(codetypefactoryOut, codetypefactoryIn,
                declarationHolder,
                idlIntf,
                method.Name,
                idlMethod.Name,
                idlMethod.Arguments,
                method.Parameters,
                method.Statements,
                context,
                codebuilder
            );

            return method;
        }

        static public void MakeMethodParameters(CodeTypeFactory codetypefactoryOut, CodeTypeFactory codetypefactoryIn,
            Udbus.Parsing.CodeMemberDeferredClassHolder declarationHolder,
            IDLInterface idlIntf,
            string methodName,
            string idlMethodName,
            IList<IDLMethodArgument> arguments,
            CodeParameterDeclarationExpressionCollection parameters,
            CodeStatementCollection statements,
            Udbus.Parsing.BuildContext context,
            MarshalBuilderHelper codebuilder)
        {
            CodeStatementCollection statementsTryRecv = new CodeStatementCollection();
            CodeConditionStatement condMethodSignature = new CodeConditionStatement(
                exprResultOk
                , new CodeStatement[]
                {
                    codebuilder.SetSignature() 
                }
                , new CodeStatement[]
                {
                    // * throw Udbus.Core.Exceptions.UdbusMessageBuilderException.Create("BodyAdd", serial, "<method_name>", this.result, this.ConnectionParameters);
                    codebuilder.ThrowMessageBuilderException(idlMethodName, "BodyAdd")
                }
            );

            // Check for in parameters.
            bool bInParameters = codebuilder.InitialiseSignature(arguments, statements);

            int nInArgSigCounter = 0;
            int nInArgCounter = 0;
            int nOutArgCounter = 0;
            List<CodeMethodInvokeExpression> invokemethodsBuild = new List<CodeMethodInvokeExpression>();
            CodeConditionStatement condOut = null; // Root if statement for out parameters.
            CodeConditionStatement condOutIter = null; // Most nested if statement for out parameters.
            CodeConditionStatement condIn = null; // Root if statement for in parameters.
            CodeConditionStatement condInIter = null; // Most nested if statement for in parameters.
            CodeStatementCollection stmtsFinishResult = new CodeStatementCollection();
            //Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);
            CodeTypeReference typerefParamIter = CodeBuilderCommon.typerefUnknownParameters;
            string argNameIter = arguments != null && arguments.Count > 0 ? arguments[0].Name : "UnknownParameters";

            CodeThrowExceptionStatement throwargOutPrev = codebuilder.CreateArgumentOutException(idlMethodName);

            CodeThrowExceptionStatement throwargInPrev = codebuilder.ThrowMessageBuilderException(idlMethodName, "SetSignature");

            foreach (IDLMethodArgument idlMethodArg in arguments)
            {
                argNameIter = idlMethodArg.Name;

                // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
                //Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLMethodArgumentTypeNameBuilder(idlIntf, idlMethod, idlMethodArg);
                Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLArgumentTypeNameBuilder(idlIntf, methodName);
                ParamCodeTypeHolderMarshalBase paramtypeHolder = null;
                CodeParameterDeclarationExpression param = null;
                CodeTypeReference typerefParam = null;
                if (idlMethodArg.Direction == "out") // If output parameter
                {
                    if (condOut == null)
                    {
                        codebuilder.PrefixOutParams(ref condOut, ref condOutIter, idlMethodName, ref nOutArgCounter, ref throwargOutPrev);
                    }

                    CodeConditionStatement condVarResult;
                    codebuilder.MakeOutArgument(statements
                        , stmtsFinishResult
                        , idlMethodName
                        , codetypefactoryOut // Yeah I messed up the naming
                        , ref nOutArgCounter
                        , context
                        , ref throwargOutPrev
                        , idlMethodArg
                        , nameBuilder
                        , ref paramtypeHolder
                        , ref typerefParam
                        , out condVarResult
                    );

                    codebuilder.StoreCondIterator(ref condOut, ref condOutIter, condVarResult);

                } // Ends if output parameter
                else // Else not output parameter
                {
                    // Add signature for argument.
                    nInArgSigCounter = codebuilder.DeclareSignature(statements, nInArgSigCounter, idlMethodArg.Type);

                    CodeConditionStatement condVarResult;
                    codebuilder.MakeInArgument(codetypefactoryIn // Yeah I messed up the naming
                        //, method.Statements
                        , idlMethodName
                        , ref nInArgCounter
                        , context
                        , ref throwargInPrev
                        , idlMethodArg
                        , nameBuilder
                        , ref paramtypeHolder
                        , ref typerefParam
                        , out condVarResult
                    );

                    codebuilder.StoreCondIterator(ref condIn, ref condInIter, condVarResult);

                } // Ends else not output parameter

                param = codebuilder.AddParameter(parameters, idlMethodArg.Name, idlMethodArg.Direction, typerefParam);

            } // Ends loop over arguments

            codebuilder.AssignResults(statementsTryRecv, condOut, condOutIter, stmtsFinishResult, throwargOutPrev
                ,idlMethodName, ref nOutArgCounter
            );
            codebuilder.TerminateSignature(statements, bInParameters, nInArgSigCounter);
            codebuilder.AddSerialNumber(statements);
            codebuilder.InitialiseMessageBuilder(statements);
            codebuilder.DeclareMessageHandle(statements);

            CodeStatement stmtBuildMethod = codebuilder.InvokeBuild(statements, methodName);

            condIn = codebuilder.FinishInArguments(idlMethodName, codebuilder, condMethodSignature, condIn, condInIter, throwargInPrev);

            // Add a using statement ??? Nope. Add a try/finally statement. Ahhh CodeDOM is there no construct you can't mangle ?
            CodeVariableReferenceExpression varrefSendHandle = codebuilder.DeclareSendHandle();
            CodeStatementCollection statementsTrySend = new CodeStatementCollection();
#if !USE_FLUID_MESSAGE_BUILDER
            // Use individual statements to build message.
            statementsTrySend.Add(stmtBuildMethod);

            if (condIn != null)
            {
                statementsTrySend.Add(condIn);
            }
            else
            {
                // Need to set the signature even for methods with no parameters (?).
                //statementsTrySend.Add(condMethodSignature);
            }
#endif // !USE_FLUID_MESSAGE_BUILDER


            codebuilder.CallSend(idlMethodName, varrefSendHandle, statementsTrySend);

            codebuilder.TryCatchSend(statements, varrefSendHandle, statementsTrySend);

            List<CodeStatement> statementsReponse = new List<CodeStatement>();

            // Now receive the response.
            codebuilder.HandleResponse(idlMethodName, statementsTryRecv, statementsReponse);

            statements.Add(new CodeConditionStatement(exprResultOk,
                statementsReponse.ToArray()
            ));
        }

    } // Ends class MarshalBuilder
}
