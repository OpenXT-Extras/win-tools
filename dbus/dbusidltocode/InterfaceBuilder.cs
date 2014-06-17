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

namespace dbusidltocode
{
    internal class InterfaceBuilder : InterfaceBuilderBase
    {
        private CodeTypeDeferredNamespaceDeclarationHolderParams declarationHolder = null;
        internal static readonly CodeTypeReference typerefEventArgs = new CodeTypeReference(typeof(System.EventArgs));

        private CodeTypeDeclaration typeSignalArgs = new CodeTypeDeclaration(); // Reset for each signal
        private CodeConstructor constructorSignalArgs = new CodeConstructor(); // Reset for each signal

        public InterfaceBuilder()
        {
            this.InitFields();
        }

        public override void PreProcess()
        {
            this.visitorType = new InterfaceVisitor();
        }

        #region postHandles

        #region postSignal

        override protected void PostHandleSignal(IDLInterface idlIntf, IDLSignal idlSignal, CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration typeInterface)
        {
            CodeMemberProperty propEventSignal = CodeBuilderCommon.CreateSignalEventProperty(idlSignal);
            typeInterface.Members.Add(propEventSignal);
            propEventSignal.HasGet = true;
            propEventSignal.HasSet = true;

            // * public class <signal_as_type>Args : EventArgs
            this.typeSignalArgs.Name = CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name);
            this.typeSignalArgs.CustomAttributes.Add(CodeBuilderCommon.attribDataContract);
            ns.Types.Add(this.typeSignalArgs);

            // Prepare for next signal.
            this.typeSignalArgs = new CodeTypeDeclaration(CodeBuilderCommon.GetSignalEventTypeName(idlSignal.Name));
            this.constructorSignalArgs = new CodeConstructor();
            this.InitFields();
        }

        protected override ParamCodeTypeFactory HandleSignalArgument(IDLInterface idlIntf, IDLSignal idlSignal, IDLSignalArgument idlSignalArg)
        {
            ParamCodeTypeFactory signalArg = base.HandleSignalArgument(idlIntf, idlSignal, idlSignalArg);
            CodeTypeReference typerefArg = signalArg.paramtype.CodeType;

            string[] components = CodeBuilderCommon.GetSignalComponents(idlSignalArg.Name);
            string first = components[0];
            components = Array.ConvertAll<string, string>(components, System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase);
            components[0] = first;
            string argFieldName = string.Join("", components);
            CodeFieldReferenceExpression fieldrefArg = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), argFieldName);
            CodeMemberField fieldArg = new CodeMemberField(typerefArg, argFieldName);
            fieldArg.CustomAttributes.Add(CodeBuilderCommon.attribDataMember);
            this.typeSignalArgs.Members.Add(fieldArg);

            CodeMemberProperty propArgField = new CodeMemberProperty();
            propArgField.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            propArgField.Type = typerefArg;
            propArgField.Name = argFieldName[0].ToString().ToUpper() + argFieldName.Substring(1);
            propArgField.GetStatements.Add(new CodeMethodReturnStatement(fieldrefArg));
            this.typeSignalArgs.Members.Add(propArgField);

            this.constructorSignalArgs.Parameters.Add(new CodeParameterDeclarationExpression(typerefArg, argFieldName));
            // * this.<signal_arg> = <signal_arg>;
            this.constructorSignalArgs.Statements.Add(new CodeAssignStatement(fieldrefArg, new CodeArgumentReferenceExpression(argFieldName)));
            return signalArg;
        }

        #endregion postSignal

        #region postInterface

        override protected void PostHandleInterface(IDLInterface idlIntf, CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration typeInterface)
        {
            if (this.declarationHolder.ns != null) // If created namespace for parameter types
            {
                ns.Imports.Add(new CodeNamespaceImport(CodeBuilderCommon.nsDbusParams));
                declarationHolder.ns.Imports.Add(new CodeNamespaceImport("System"));
                declarationHolder.ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                unit.Namespaces.Add(declarationHolder.ns);

            } // Ends if created namespace for parameter types
        }

        #endregion postInterface
        #endregion postHandles
        
        override protected Udbus.Parsing.ICodeTypeDeclarationHolder CreateCodeTypeDeclarationHolder(IDLInterface idlIntf)
        {
            return this.declarationHolder = new CodeTypeDeferredNamespaceDeclarationHolderParams(idlIntf);
        }

        override protected ParamCodeTypeFactory CreateParamCodeTypeFactoryForMethod(string argDirection)
        {
            return new ParamCodeTypeFactory(CodeTypeFactory.Default, argDirection == "out" ? FieldDirection.Out : FieldDirection.In);
        }

        override protected ParamCodeTypeFactory CreateParamCodeTypeFactoryForProperty(IDLInterface idlIntf, string propertyName, string argName, string argType, string argDirection)
        {
            return new ParamCodeTypeFactory(CodeTypeFactory.DefaultProperty, argDirection == "out" ? FieldDirection.Out : FieldDirection.In);
        }

        private void InitFields()
        {
            this.typeSignalArgs.BaseTypes.Add(typerefEventArgs);
            this.constructorSignalArgs.Attributes = MemberAttributes.Public;
            this.typeSignalArgs.Members.Add(this.constructorSignalArgs);
        }

    } // Ends class InterfaceBuilder


}
