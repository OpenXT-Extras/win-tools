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
using System.IO;
using System.Linq;
//using System.Text;

using System.CodeDom;
//using System.Reflection;
//using System.CodeDom.Compiler;

namespace dbusidltocode
{
    internal static class CodeBuilderCommon
    {
        public readonly static Type DictionaryType = typeof(Dictionary<,>);

        #region Proxy
        public readonly static CodeParameterDeclarationExpression paramdeclException = new CodeParameterDeclarationExpression(typeof(System.Exception), "e");
        public readonly static CodeParameterDeclarationExpression paramdeclCancelled = new CodeParameterDeclarationExpression(typeof(bool), "cancelled");
        public readonly static CodeParameterDeclarationExpression paramdeclUserState = new CodeParameterDeclarationExpression(typeof(object), "userStateObj");
        public readonly static CodeArgumentReferenceExpression argrefException = new CodeArgumentReferenceExpression("e");
        public readonly static CodeArgumentReferenceExpression argrefCancelled = new CodeArgumentReferenceExpression("cancelled");
        public readonly static CodeArgumentReferenceExpression argrefUserState = new CodeArgumentReferenceExpression("userStateObj");
        #endregion // Ends Proxy

        #region Marshal
        internal const string nsDbusParams = "Params";
        internal const string nsDbusMarshalCustom = "DbusService.Marshal.CustomTypes";
        internal const string nsDbusMarshalNested = "MarshalFred"; //Why is this Fred? Seriously?

        public const string nameConnectionParameters = "connectionParameters";
        public const string nameServiceConnectionParameters = "serviceConnectionParameters";
        public const string nameSerialNumber = "serial";
        public const string nameReceiverPool = "receiverPool";
        public const string nameReadValue = "value";
        public const string nameBuildValue = "value";
        public const string nameBuilder = "builder";
        public const string nameReader = "reader";
        public const string nameResult = "result";
        public const string ReadSuffix = "";//"Value";
        public const string DictionaryFunctionSuffix = "Dictionary";
        public const string nameDefaultEnumerableMarshallerIn = "EnumerableMarshallerArray";
        public const string nameDefaultEnumerableMarshallerOut = "ArrayMarshallerArray";
        public const string nameDictionaryValueArrayMarshaller = "ArrayMarshallerArray";
        public const string nameEnumerableStem = "Enumerable";
        public const string nameDictionaryValueArrayStem = "Array";
        public const string DefaultConnectionParameters = "DefaultConnectionParameters";
        public readonly static int lengthEnumerableStemIn = nameEnumerableStem.Length;
        public readonly static CodeTypeReference typerefConnectionParameters = new CodeTypeReference(typeof(Udbus.Serialization.DbusConnectionParameters));
        public readonly static CodeTypeReference typerefReadonlyConnectionParameters = new CodeTypeReference(typeof(Udbus.Serialization.ReadonlyDbusConnectionParameters));
        public readonly static CodeTypeReference typerefServiceConnectionParameters = new CodeTypeReference(typeof(Udbus.Core.ServiceConnectionParams));

        
        public readonly static CodeParameterDeclarationExpression paramdeclConnectionParams = new CodeParameterDeclarationExpression(typerefConnectionParameters, nameConnectionParameters);
        public readonly static CodeParameterDeclarationExpression paramdeclServiceConnectionParams = new CodeParameterDeclarationExpression(typerefServiceConnectionParameters, nameServiceConnectionParameters);
        public readonly static CodeFieldReferenceExpression fieldrefServiceConnectionParameters = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.nameServiceConnectionParameters);
        public readonly static CodeArgumentReferenceExpression argrefConnectionParameters = new CodeArgumentReferenceExpression(nameConnectionParameters);
        public readonly static CodeArgumentReferenceExpression argrefServiceConnectionParameters = new CodeArgumentReferenceExpression(nameServiceConnectionParameters);
        public readonly static CodeVariableReferenceExpression varrefConnectionParameters = new CodeVariableReferenceExpression(nameConnectionParameters);
        public readonly static CodeTypeReference typerefMessageBuilder = new CodeTypeReference(typeof(Udbus.Serialization.UdbusMessageBuilder));
        public readonly static CodeTypeReferenceExpression typerefexprMessageBuilder = new CodeTypeReferenceExpression(typerefMessageBuilder);
        public readonly static Type typeMessageReader = typeof(Udbus.Serialization.UdbusMessageReader);
        public readonly static CodeTypeReference typerefMessageReader = new CodeTypeReference(typeMessageReader);
        public readonly static CodeTypeReferenceExpression typerefexprMessageReader = new CodeTypeReferenceExpression(typerefMessageReader);
        public readonly static CodeTypeReference typerefResult = new CodeTypeReference(typeof(int));
        public readonly static CodeTypeReference typerefReadResult = new CodeTypeReference(typeof(int));
        public readonly static CodePrimitiveExpression exprReadResultSuccessValue = new CodePrimitiveExpression(0);
        public readonly static CodePrimitiveExpression exprBuildResultSuccessValue = new CodePrimitiveExpression(0);
        public readonly static CodeVariableReferenceExpression varrefSerial = new CodeVariableReferenceExpression(nameSerialNumber);
        public readonly static CodeVariableReferenceExpression varrefReadValue = new CodeVariableReferenceExpression(nameReadValue);
        public readonly static CodeVariableReferenceExpression varrefBuildValue = new CodeVariableReferenceExpression(nameBuildValue);
        public readonly static CodeArgumentReferenceExpression argrefReader = new CodeArgumentReferenceExpression(nameReader);
        public readonly static CodeArgumentReferenceExpression argrefBuilder = new CodeArgumentReferenceExpression(nameBuilder);
        public readonly static CodeVariableReferenceExpression varrefResult = new CodeVariableReferenceExpression(nameResult);

        public readonly static CodeTypeReference typerefUnknownParameters = new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMessageMethodArgumentException.UnknownParameters));
        public readonly static CodePropertyReferenceExpression proprefMessageTypeError = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Types.dbus_msg_type))), "DBUS_TYPE_ERROR");

        public readonly static CodeTypeReferenceExpression typerefexprSendException = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMethodSendException)));
        public readonly static CodeTypeReferenceExpression typerefexprReceiveException = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMethodReceiveException)));
        public readonly static CodeTypeReferenceExpression typerefexprBuildException = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMessageBuilderException)));
        #endregion Marshal

        #region WCF Contracts
        public readonly static CodeTypeReference typerefServiceContract = new CodeTypeReference(typeof(System.ServiceModel.ServiceContractAttribute));

        public readonly static CodeAttributeDeclaration attribServiceContract = new CodeAttributeDeclaration(typerefServiceContract);
        public readonly static CodeAttributeDeclaration attribOperationContract = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ServiceModel.OperationContractAttribute)));
        public readonly static CodeAttributeDeclaration attribOperationContractOneWay = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ServiceModel.OperationContractAttribute)), 
                                                                    new CodeAttributeArgument("IsOneWay", new CodePrimitiveExpression(true)));
        internal readonly static CodeAttributeDeclaration attribDataMember = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.Runtime.Serialization.DataMemberAttribute)));
        internal readonly static CodeAttributeDeclaration attribDataContract = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.Runtime.Serialization.DataContractAttribute)));
        public readonly static CodeAttributeDeclaration attribDbusContainer = new CodeAttributeDeclaration(new CodeTypeReference(typeof(Udbus.WCF.Dbus.Contracts.DbusContainerAttribute)));

        public const string nameConstantsClass = "Constants";
        public const string nameProxyBuilderExtensionsClass = "ProxyBuilderExtensions";
        public const string nameRelativeAddress = "DbusServiceRelativeAddress";
        public const string nsClientExtensions = "Udbus.WCF.Client.Extensions";
        #endregion // WCF Contracts

        #region WCF Services
        public const string nameDbusServiceHostCreateWithDataMethod = "CreateWithData";
        public readonly static CodeTypeReferenceExpression typerefexprDbusServiceHost = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.WCF.Service.Host.DbusServiceHost)));
        public readonly static Type typeServiceHostCreationData = typeof(Udbus.WCF.Service.Host.ServiceHostCreationData);
        public readonly static CodeTypeReference typerefServiceHostCreationData = new CodeTypeReference(typeServiceHostCreationData);

        public readonly static CodeTypeReference typerefDbusServiceBase = new CodeTypeReference(typeof(Udbus.WCF.Dbus.Details.Service.WCFDbusServiceBase));

        public const string GetWCFMethodTarget = "GetWCFMethodTarget";
        public const string CreateDbusService = "CreateDbusService";
        public static readonly CodeTypeReferenceExpression typerefexprLookupTargetFunctions = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.WCF.Dbus.Details.Service.LookupTargetFunctions)));
        public static readonly CodeMethodReferenceExpression methodrefGetWCFMethodTarget = new CodeMethodReferenceExpression(typerefexprLookupTargetFunctions, GetWCFMethodTarget);
        #endregion // WCF Services

        #region WCF Host
        public readonly static CodeTypeReference typerefWCFServiceParams = new CodeTypeReference(typeof(Udbus.WCF.Host.WCFServiceParams));
        public readonly static CodeTypeReference typerefRegistrationParams = new CodeTypeReference(typeof(Udbus.WCF.Service.Host.DbusServiceRegistrationParams));
        public readonly static CodeTypeReference typerefDbusServiceCreationParams = new CodeTypeReference(typeof(Udbus.WCF.Service.Host.DbusServiceCreationParams));
        #endregion // WCF Host

        public static readonly CodeTypeReference typerefBool = new CodeTypeReference(typeof(bool));
        public readonly static CodeAttributeDeclaration attribParams = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ParamArrayAttribute)));

        internal const string SignalArgsName = "args";
        internal const string callback = "callback";
        internal const string targetName = "target";
        
        public static string GetNamespace(string idlInterface, Visitor reflection)
        {
            int lastDot = idlInterface.LastIndexOf('.');

            if (reflection is InterfaceVisitor)
            {
                return idlInterface;
            }
            else if (reflection is WCFContractVisitor)
            {
                return idlInterface + ".wcf.Contracts";
            }
            else if (reflection is WCFClientVisitor)
            {
                return idlInterface + ".wcf.Contracts.Clients";
            }
            else if (reflection is WCFServiceVisitor)
            {
                return idlInterface + ".wcf.Services";
            }
            else if (reflection is WCFHostVisitor)
            {
                return idlInterface + ".wcf.Hosts";
            }
            else
            {
                return null;
            }
        }

        public static CodeNamespace AddUsingNamespaces(CodeNamespace ns, Visitor reflection)
        {
            //Safety catch
            if (ns == null) return ns;

            if (reflection is InterfaceVisitor)
            {
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.DictionaryType.Namespace));

            }
            else if (reflection is WCFContractVisitor)
            {
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.ServiceModel"));
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.DictionaryType.Namespace));
            }
            else if (reflection is WCFServiceVisitor)
            {
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.ServiceModel"));
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.DictionaryType.Namespace));
            }
            else if (reflection is WCFHostVisitor)
            {
                ns.Imports.Add(new CodeNamespaceImport("System.Linq"));
                ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            }
            else if (reflection is ProxyVisitor)
            {
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.DictionaryType.Namespace));
            }
            else if (reflection is MarshalVisitor)
            {
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.DictionaryType.Namespace));
            }

            return ns;
        }

        public static string GetName(string idlInterface, Visitor reflection)
        {
            int lastDot = idlInterface.LastIndexOf('.');

            if (lastDot == -1) return null;

            if (reflection == null)
            {
                return idlInterface.Substring(lastDot + 1);
            }
            else if (reflection is InterfaceVisitor)
            {
                return "I" + idlInterface.Substring(lastDot + 1);
            }
            else if (reflection is DBUSServiceVisitor)
            {
                return idlInterface.Substring(lastDot + 1) + "Service";
            }
            else if (reflection is WCFServiceVisitor)
            {
                return idlInterface.Substring(lastDot + 1) + "Service";
            }

            return null;
        }

        public static string GetCompilableName(string name)
        {
            return name.Replace('-', '_');
        }

        private static CodeTypeReference RecursiveCopyTypeReference(CodeTypeReference source)
        {
            CodeTypeReference result = null;

            if (source.ArrayElementType != null) // If got subarray
            {
                CodeTypeReference subarray = RecursiveCopyTypeReference(source.ArrayElementType);
                result = new CodeTypeReference(subarray, source.ArrayRank);

            } // Ends if got subarray
            else // Else normal
            {
                CodeTypeReference[] refs = new CodeTypeReference[source.TypeArguments.Count];
                source.TypeArguments.CopyTo(refs, 0);
                result = new CodeTypeReference(source.BaseType, refs);

            } // Ends else normal

            return result;
        }

        public static CodeTypeReference GetReadOnlyCodeReference(CodeTypeReference coderef)
        {
            // Make it readonly by putting the string readonly on the front.
            // CodeDom, for all your coding needs. Apart from when it isn't.
            CodeTypeReference codetypeReadOnly = RecursiveCopyTypeReference(coderef);
            codetypeReadOnly.BaseType = "readonly " + codetypeReadOnly.BaseType;
            return codetypeReadOnly;
        }

        /// <summary>
        /// Convert an array type to the equivalent enumerable type.
        /// E.g. int[][] => IEnumerable&lt;IEnumerable&lt;int&gt;&gt;
        /// </summary>
        /// <param name="source">Array type to convert.</param>
        /// <returns>Equivalent IEnumerable type.</returns>
        private static CodeTypeReference RecursiveArrayTypeToEnumerableType(CodeTypeReference source)
        {
            CodeTypeReference result = source;

            if (source.ArrayElementType != null) // If got subarray
            {
                CodeTypeReference subarray = RecursiveArrayTypeToEnumerableType(source.ArrayElementType);
                result = new CodeTypeReference(typeof(IEnumerable<>).Name, new CodeTypeReference[] { subarray });
            } // Ends if got subarray

            return result;
        }
        
        public static CodeTypeReference ArrayTypeToEnumerableType(CodeTypeReference source)
        {
            return RecursiveArrayTypeToEnumerableType(source);
        }

        /// <summary>
        /// Get the fully scoped name.
        /// </summary>
        /// <param name="scope">Enclosing scope.</param>
        /// <param name="name">Name to scope.</param>
        /// <returns></returns>
        public static string GetScopedName(params string[] names)
        {
            return string.Join(".", names);
        }

        public static string GetParamsNamespaceName(string interfaceName)
        {
            return GetScopedName(interfaceName, nsDbusParams);
        }

        public static string GetParamsNamespaceName(IDLInterface idlInterface)
        {
            return GetParamsNamespaceName(idlInterface.Name);
        }

        public static string GetNameMarshalDictionary()
        {
            return "MarshalBert"; //And why Bert?
        }

        private static CodeMethodInvokeExpression BuildMethodInvocationArgSwappedEnd(string nameMethod, CodeExpression exprReplaceArgEnd, params CodeExpression[] args)
        {
            // We mess with the first argument, because that's where we pass in the builder.
            // However in the resulting method call, it's the last argument so we also need to do a bit of a bump down first.
            CodeExpression exprBuilder = args[0];
            int nLastArg = args.Length - 1;
            for (int nArgIter = 0; nArgIter < nLastArg; ++nArgIter)
            {
                args[nArgIter] = args[nArgIter + 1];

            } // Ends loop bumping down args

            args[nLastArg] = exprReplaceArgEnd;
            CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(
                exprBuilder, nameMethod, args
            );
            return exprMarshal;
        }

        private static CodeMethodInvokeExpression BuildMethodInvocationArgSwappedBeginning(string nameMethod, CodeExpression exprReplaceArg0, params CodeExpression[] args)
        {
            CodeExpression exprReader = args[0];
            args[0] = exprReplaceArg0;
            CodeMethodInvokeExpression exprMarshal = new CodeMethodInvokeExpression(
                exprReader, nameMethod, args
            );
            return exprMarshal;
        }

        public static CodeMethodInvokeExpression BuildMarshalEnumerableIn(string nameMarshalFunction, CodeExpression exprReplaceArgEnd, params CodeExpression[] args)
        {
            return BuildMethodInvocationArgSwappedEnd(nameMarshalFunction, exprReplaceArgEnd, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalEnumerableIn(CodeExpression exprReplaceArgEnd, params CodeExpression[] args)
        {
            return BuildMarshalEnumerableIn("MarshalEnumerable", exprReplaceArgEnd, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalEnumerableStructIn(CodeExpression exprReplaceArgEnd, params CodeExpression[] args)
        {
            return BuildMarshalEnumerableIn("MarshalEnumerableStruct", exprReplaceArgEnd, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalEnumerableOut(string nameMarshalFunction, CodeExpression exprReplaceArg0, params CodeExpression[] args)
        {
            return BuildMethodInvocationArgSwappedBeginning(nameMarshalFunction, exprReplaceArg0, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalEnumerableOut(CodeExpression exprReplaceArg0, params CodeExpression[] args)
        {
            return BuildMarshalEnumerableOut("MarshalEnumerable", exprReplaceArg0, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalEnumerableStructOut(CodeExpression exprReplaceArg0, params CodeExpression[] args)
        {
            return BuildMarshalEnumerableOut("MarshalEnumerableStruct", exprReplaceArg0, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalStructIn(CodeExpression exprReplaceArgEnd, params CodeExpression[] args)
        {
            return BuildMethodInvocationArgSwappedEnd("MarshalStruct", exprReplaceArgEnd, args);
        }

        public static CodeMethodInvokeExpression BuildMarshalStructOut(CodeExpression exprReplaceArg0, params CodeExpression[] args)
        {
            return BuildMethodInvocationArgSwappedBeginning("MarshalStruct", exprReplaceArg0, args);
        }

        public static string GetMarshalDictionaryScope()
        {
            return GetScopedName(nsDbusMarshalNested, GetNameMarshalDictionary());
        }

        public static string GetMarshalDictionaryScopedName(string dictionaryName)
        {
            return GetScopedName(GetMarshalDictionaryScope(), dictionaryName);
        }

        #region Nested Expressions
        /// <summary>
        /// Nest a collection of statements based on a test expression.
        /// </summary>
        /// <param name="exprTest">Test for each set of statements.</param>
        /// <param name="nest">Collection of set of statements to execute each time test is evaluated.</param>
        /// <returns>Top level statement containing nested statements.</returns>
        public static IEnumerable<CodeStatement> NestedExpression(out bool bGotConditional, CodeExpression exprTest, IEnumerable<IEnumerable<CodeStatement>> nest)
        {
            IEnumerable<IEnumerable<CodeStatement>> iterableStatements = nest;
            IEnumerator<IEnumerable<CodeStatement>> iterStatements = iterableStatements.Reverse().GetEnumerator();
            IEnumerable<CodeStatement> result = null;
            bGotConditional = false;

            if (iterStatements.MoveNext()) // If got statements
            {
                // This is the last statement.
                result = iterStatements.Current;

                while (iterStatements.MoveNext()) // If got more statements
                {
                    bGotConditional = true;

                    // Execute the old statements inside a conditional, which is executed after the current set of statements.
                    CodeStatement[] arrayOldCurrent = iterStatements.Current.ToArray();
                    CodeStatement[] arrayNewCurrent = new CodeStatement[arrayOldCurrent.Length + 1];
                    arrayOldCurrent.CopyTo(arrayNewCurrent, 0);
                    // This is the last conditional statement.
                    arrayNewCurrent[arrayOldCurrent.Length] = new CodeConditionStatement(
                        exprTest,
                        result.ToArray()
                    );
                    result = arrayNewCurrent;

                } // Ends if got more statements
            } // Ends if got statements

            return result;
        }

        /// <summary>
        /// Turn "an enumerable of T" into "an enumerable of enumerables of T".
        /// </summary>
        /// <typeparam name="T">Type to nest.</typeparam>
        /// <param name="e">Enumerable of T</param>
        /// <returns>Enumerable of each element in e, nested in its own enumerable.</returns>
        public static IEnumerable<IEnumerable<T>> NestEnumerable<T>(IEnumerable<T> e)
        {
            foreach (T iter in e)
            {
                yield return new T[] { iter };
            }
        }

        /// <summary>
        /// Nest a collection of statements based on a test expression (overload for enumerable of single statements).
        /// </summary>
        /// <param name="exprTest">Test for each set of statements.</param>
        /// <param name="nest">Collection of set of statements to execute each time test is evaluated.</param>
        /// <returns>Top level statement containing nested statements.</returns>
        public static IEnumerable<CodeStatement> NestedExpression(out bool bGotConditional, CodeExpression exprTest, IEnumerable<CodeStatement> nest)
        {
            return NestedExpression(out bGotConditional, exprTest, NestEnumerable(nest));
        }
        
        public static IEnumerable<CodeStatement> NestedExpression(CodeExpression exprTest, IEnumerable<CodeStatement> nest)
        {
            bool bGotConditional;
            return NestedExpression(out bGotConditional, exprTest, nest);
        }
        #endregion // Nested Expressions

        #region Properties
        public static bool HasGet(IDLProperty idlProperty)
        {
            return idlProperty.Access == "readwrite" || idlProperty.Access == "read";
        }

        public static bool HasSet(IDLProperty idlProperty)
        {
            return idlProperty.Access == "readwrite" || idlProperty.Access == "write";
        }

        public static CodeTypeReference PropertyType(CodeTypeFactory codetypefactoryOut, string idlPropertyType)
        {
            Udbus.Parsing.ICodeTypeDeclarationHolder declarationHolderProperty = new Udbus.Parsing.CodeTypeNoOpHolder();
            Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new Udbus.Parsing.IDLArgumentTypeNameBuilderNoOp();
            marshal.outward.ParamCodeTypeHolderProperty paramtypeHolder = new marshal.outward.ParamCodeTypeHolderProperty(codetypefactoryOut, FieldDirection.Out);
            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolderProperty);
            Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolder, nameBuilder, idlPropertyType, context);
            return paramtypeHolder.paramtype.CodeType;
        }
        #endregion // Properties

        internal static CodeMemberEvent CreateSignalEvent(IDLSignal idlSignal)
        {
            string eventPropertyName = CodeBuilderCommon.GetSignalEventName(idlSignal.Name);
            CodeMemberEvent eventSignal = new CodeMemberEvent();
            eventSignal.Attributes = MemberAttributes.Private;
            eventSignal.Name = CodeBuilderCommon.GetSignalEventName(idlSignal.Name);
            CodeTypeReference typerefEvent = new CodeTypeReference(typeof(System.EventHandler<>));
            typerefEvent.TypeArguments.Add(new CodeTypeReference(CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name)));
            eventSignal.Type = typerefEvent;
            return eventSignal;
        }

        internal static CodeMemberProperty CreateSignalEventProperty(IDLSignal idlSignal)
        {
            CodeTypeReference typerefEvent = new CodeTypeReference(typeof(System.EventHandler<>));
            typerefEvent.TypeArguments.Add(new CodeTypeReference(CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name)));
            CodeMemberProperty propEventSignal = new CodeMemberProperty();
            propEventSignal.Name = CodeBuilderCommon.GetSignalEventPropertyName(idlSignal.Name);
            propEventSignal.Type = typerefEvent;
            return propEventSignal;
        }
        
        internal static string[] GetSignalComponents(string idlSignalName)
        {
            string[] components = idlSignalName.Split('_');
            if (components.Length == 1) // If no underscores
            {
                components = idlSignalName.Split('-');

            } // Ends if no underscores

            return components;
        }

        internal static string GetSignalCompilableName(string idlSignalName)
        {
            string[] components = GetSignalComponents(idlSignalName);
            components = Array.ConvertAll<string,string>(components, System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase);
            return string.Join("", components);
        }

        internal static string GetSignalTypeName(string idlSignalName)
        {
            return GetSignalCompilableName(idlSignalName);
        }

        internal static string GetSignalEventTypeName(string idlSignalName)
        {
            return GetSignalTypeName(idlSignalName) + "Args";
        }
    
        internal static string GetSignalEventName(string idlSignalName)
        {
            string signalTypeName = GetSignalTypeName(idlSignalName);
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToLower(signalTypeName[0]) + signalTypeName.Substring(1) + "Event";
        }

        internal static string GetSignalEventPropertyName(string idlSignalName)
        {
            return GetSignalTypeName(idlSignalName);
        }
        
        internal static string GetSignalRegisterFunction(string idlSignalName)
        {
            return "RegisterFor" + GetSignalCompilableName(idlSignalName);
        }

        internal static string GetSignalCallbackInterfaceName(string idlInterfaceName)
        {
            return "I" + idlInterfaceName + "Callback";
        }

        internal static string GetSignalProxyName(string idlInterfaceName)
        {
            return CodeBuilderCommon.GetSignalCompilableName(idlInterfaceName) + "Proxy";
        }

        internal static string GetSignalCallbackMethodName(string idlSignalName)
        {
            return "On" + CodeBuilderCommon.GetSignalCompilableName(idlSignalName);
        }
    } // Ends static class CodeBuilderCommon
}
