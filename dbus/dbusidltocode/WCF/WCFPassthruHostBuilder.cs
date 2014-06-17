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
    class WCFPassthruHostBuilder : WCFHostBuilder
    {
        protected static readonly CodeVariableReferenceExpression varrefDbusService = new CodeVariableReferenceExpression(dbusService);

        protected override CodeMemberMethod CreateService(CodeTypeReference typerefWCFService, CodeTypeReference typerefService)
        {
            CodeTypeReferenceExpression typerefexprService = new CodeTypeReferenceExpression(typerefService);
            CodeMethodReferenceExpression methodrefDbusServiceCreate = new CodeMethodReferenceExpression(typerefexprService, "Create");

            CodeMemberMethod methodCreateService = new CodeMemberMethod();
            methodCreateService.Name = CreateServiceName;
            methodCreateService.ReturnType = typerefWCFService;
            methodCreateService.Attributes = MemberAttributes.Static;
            methodCreateService.Parameters.Add(new CodeParameterDeclarationExpression(CodeBuilderCommon.typerefWCFServiceParams, nameWCFServiceParamsArg));
            CodeMethodReferenceExpression methodrefCreateDbusService = new CodeMethodReferenceExpression(CodeBuilderCommon.typerefexprLookupTargetFunctions, CodeBuilderCommon.CreateDbusService);
            methodrefCreateDbusService.TypeArguments.Add(typerefService);

            methodCreateService.Statements.Add(new CodeVariableDeclarationStatement(typerefService, dbusService, // * <dbus_service> dbusService = 
                new CodeMethodInvokeExpression(methodrefCreateDbusService // * Udbus.WCF.Dbus.Service.LookupTargetFunctions.CreateDbusService(
                    , argrefWCFServiceParams // * wcfserviceparams
                    , methodrefDbusServiceCreate // * , <dbus_service>.Create // createService1
                    , methodrefDbusServiceCreate // * , <dbus_service>.Create // createService2
                    , new CodePropertyReferenceExpression(typerefexprService, CodeBuilderCommon.DefaultConnectionParameters) // * <dbus_service>.DefaultConnectionParameters);
                )
            ));

            // * <wcfservice> wcfService = new <wcfservice>(dbusservice);
            methodCreateService.Statements.Add(new CodeVariableDeclarationStatement(typerefWCFService, wcfService
                , new CodeObjectCreateExpression(typerefWCFService, varrefDbusService)
            ));

            // return wcfService;
            methodCreateService.Statements.Add(new CodeMethodReturnStatement(varrefWcfService));
            return methodCreateService;
        }

    } // Ends class WCFPassthruHostBuilder
} // Ends dbusidltocode
