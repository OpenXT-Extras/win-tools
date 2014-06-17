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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dbusidltocode
{
    internal abstract class InterfaceBuilderBase : Builder
    {
        protected abstract void PostHandleSignal(IDLInterface idlIntf, IDLSignal idlSignal, CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration typeInterface);
        
        protected abstract void PostHandleInterface(IDLInterface idlIntf, CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration typeInterface);

        protected abstract Udbus.Parsing.ICodeTypeDeclarationHolder CreateCodeTypeDeclarationHolder(IDLInterface idlIntf);

        protected abstract ParamCodeTypeFactory CreateParamCodeTypeFactoryForMethod(string argDirection);

        protected abstract ParamCodeTypeFactory CreateParamCodeTypeFactoryForProperty(IDLInterface idlIntf, string propertyName, string argName, string argType, string argDirection);

        string ifName;
        Udbus.Parsing.ICodeTypeDeclarationHolder declarationHolder;

        public override void ProcessNamespaces(IDLInterface idlIntf)
        {
            string nsName = CodeBuilderCommon.GetNamespace(idlIntf.Name, this.visitorType);
            this.ifName = CodeBuilderCommon.GetName(idlIntf.Name, new InterfaceVisitor());
            ns = CodeBuilderCommon.AddUsingNamespaces(new CodeNamespace(nsName), this.visitorType);
        }

        public override void DeclareCodeType(IDLInterface idlIntf)
        {
            // Interface.
            type =  new CodeTypeDeclaration(ifName);
            type.IsInterface = true;
            declarationHolder = this.CreateCodeTypeDeclarationHolder(idlIntf); //Implemented by subclasses
        }

        public override void PostHandleMembers(IDLInterface idlIntf)
        {
            this.PostHandleInterface(idlIntf, unit, ns, type);

            //TODO: Understand these bits before deciding what to do with them
            ns.Types.Add(type);
            //} // Ends if created namespace for parameter types
            unit.Namespaces.Add(ns);
            //END TODO
        }

        /// <summary>
        /// Interface defined requisite. Generates everything needed for methods
        /// </summary>
        /// <param name="idlIntf">The IDL defined interface</param>
        public override void GenerateMethods(IDLInterface idlIntf)
        {
            if (idlIntf.Methods != null) // If got methods
            {
                foreach (IDLMethod idlMethod in idlIntf.Methods)
                {
                    type.Members.Add(HandleMethod(idlIntf, idlMethod));
                } // Ends loop over methods

            } // Ends if got methods
        }

        /// <summary>
        /// Given an IDL Method, generate the necessary bits
        /// </summary>
        /// <remarks>Override to expand functionality or replace it</remarks>
        /// <param name="idlIntf"></param>
        /// <param name="idlMethod"></param>
        /// <returns></returns>
        protected virtual CodeMemberMethod HandleMethod(IDLInterface idlIntf, IDLMethod idlMethod)
        {
            // Method.
            CodeMemberMethod method = new CodeMemberMethod();
            method.Comments.Add(new CodeCommentStatement(idlMethod.Name));
            method.Name = idlMethod.Name;

            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);

            foreach (IDLMethodArgument idlMethodArg in idlMethod.Arguments)
            {
                method.Comments.Add(new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlMethodArg.Direction, idlMethodArg.Name, idlMethodArg.Type)));
                // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
                Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLMethodArgumentTypeNameBuilder(idlIntf, idlMethod);
                ParamCodeTypeFactory paramtypeHolder = this.CreateParamCodeTypeFactoryForMethod(idlMethodArg.Direction);
                Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolder, nameBuilder, idlMethodArg.Type, context);
                Console.WriteLine(idlMethodArg.Type);
                // Arguments.
                method.Parameters.Add(HandleMethodArgument(idlMethodArg, paramtypeHolder));
            } // Ends loop over method arguments
            return method;
        }

        /// <summary>
        /// Given an IDLMethodArgument, generate the necessary bits
        /// </summary>
        /// <remarks>Override to expand functionality or replace it</remarks>
        /// <param name="idlMethodArg"></param>
        /// <param name="paramtypeHolder"></param>
        /// <returns></returns>
        protected virtual CodeParameterDeclarationExpression HandleMethodArgument(IDLMethodArgument idlMethodArg, ParamCodeTypeFactory paramtypeHolder)
        {
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(paramtypeHolder.paramtype.CodeType, idlMethodArg.Name);
            if (idlMethodArg.Direction == "out")
            {
                param.Direction = FieldDirection.Out;
            }

            return param;
        }

        protected virtual ParamCodeTypeFactory HandleSignalArgument(IDLInterface idlIntf, IDLSignal idlSignal, IDLSignalArgument idlSignalArg)
        {
            // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
            Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLSignalArgumentTypeNameBuilder(idlIntf, idlSignal);
            ParamCodeTypeFactory paramtypeHolder = this.CreateParamCodeTypeFactoryForMethod("in");
            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);
            Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolder, nameBuilder, idlSignalArg.Type, context);

            // Arguments
            return paramtypeHolder;
        }

        /// <summary>
        /// Interface defined requisite. Generates everything needed for properties
        /// </summary>
        /// <param name="idlIntf"></param>
        public override void GenerateProperties(IDLInterface idlIntf)
        {
            if (idlIntf.Properties != null) // If got properties
            {
                foreach (IDLProperty idlProperty in idlIntf.Properties)
                {
                    type.Members.Add(HandleProperty(idlIntf, idlProperty));
                } // Ends loop over properties
            } // Ends if got properties
        }

        /// <summary>
        /// Given an IDLProperty, generate all the necessaries
        /// </summary>
        /// <remarks>Override to expand functionality or replace it</remarks>
        /// <param name="idlIntf"></param>
        /// <param name="idlProperty"></param>
        /// <returns></returns>
        public virtual CodeTypeMember HandleProperty(IDLInterface idlIntf, IDLProperty idlProperty)
        {
            CodeMemberProperty property = new CodeMemberProperty();
            property.Comments.Add(new CodeCommentStatement(idlProperty.Name));
            property.Name = CodeBuilderCommon.GetCompilableName(idlProperty.Name);
            property.Attributes = MemberAttributes.Abstract;
            IDLMethodArgument idlMethodArgGet = new IDLMethodArgument { Direction = "out", Name = "value", Type = idlProperty.Type };
            IDLMethod idlMethodGet = new IDLMethod
            {
                Arguments = new List<IDLMethodArgument>(new IDLMethodArgument[] { idlMethodArgGet }),
                Name = "get",
            };
            Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new IDLMethodArgumentTypeNameBuilder(idlIntf, idlMethodGet);
            property.Comments.Add(new CodeCommentStatement(string.Format("{0} {1} \"{2}\"", idlMethodArgGet.Direction, idlMethodArgGet.Name, idlMethodArgGet.Type)));
            // Parse the type string for the argument, creating required structs as we go, and returning a type for the argument.
            ParamCodeTypeFactory paramtypeHolder = this.CreateParamCodeTypeFactoryForProperty(idlIntf, idlProperty.Name, idlMethodArgGet.Name, idlMethodArgGet.Type, idlMethodArgGet.Direction);
            Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(declarationHolder);
            Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolder, nameBuilder, idlMethodArgGet.Type, context);

            // Arguments.
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(paramtypeHolder.paramtype.CodeType, idlMethodArgGet.Name);
            property.Type = paramtypeHolder.paramtype.CodeType;
            property.HasGet = CodeBuilderCommon.HasGet(idlProperty);
            property.HasSet = CodeBuilderCommon.HasSet(idlProperty);
            property.Parameters.Add(param);
                        
            return property;
        }

        //TODO - Figure this nasty bit of work out...
        //this.PostHandleSignalArgument(idlIntf, idlSignal, idlSignalArg, paramtypeHolder.paramtype.CodeType); is low hanging fruit
        public override void GenerateSignals(IDLInterface idlIntf)
        {
            if (idlIntf.Signals != null && idlIntf.Signals.Count > 0) // If got signals
            {
                foreach (IDLSignal idlSignal in idlIntf.Signals)
                {
                    foreach (IDLSignalArgument idlSignalArg in idlSignal.Arguments)
                    {
                        HandleSignalArgument(idlIntf, idlSignal, idlSignalArg);
                    } // Ends loop over signal arguments

                    this.PostHandleSignal(idlIntf, idlSignal, unit, ns, type);
                } // Ends loop over signal

            } // Ends if got signals
        }

    } // Ends class InterfaceBuilderBase
}
