//
// Copyright (c) 2012 Citrix Systems, Inc.
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
    /// WCF Service implementation which holds onto a Dbus interface and delegates all calls through it.
    /// </summary>
    class WCFPassthruServiceBuilder : WCFServiceBuilder
    {
        static readonly CodeFieldReferenceExpression thisProxyFieldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.targetName);

        protected override void BaseTypes(CodeTypeReference typerefWCFInterface, CodeTypeDeclaration typeProxy)
        {
            typeProxy.BaseTypes.Add(typerefWCFInterface);
        }

        protected override void DbusServiceTargetField(CodeTypeReference typerefDbusInterface, CodeTypeReference typerefDbusMarshal, CodeTypeMemberCollection members)
        {
            CodeMemberField memberProxy = new CodeMemberField(typerefDbusInterface, CodeBuilderCommon.targetName);
            memberProxy.Attributes = MemberAttributes.Private;
            members.Add(memberProxy);
        }

        protected override void Constructor(CodeTypeDeclaration typeProxy, CodeTypeReference typerefDbusInterface)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            CodeParameterDeclarationExpression paramProxy = new CodeParameterDeclarationExpression(typerefDbusInterface, CodeBuilderCommon.targetName);
            constructor.Parameters.Add(paramProxy);
            CodeFieldReferenceExpression thisProxyFieldRef = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), CodeBuilderCommon.targetName
            );
            CodeAssignStatement assignProxy = new CodeAssignStatement(thisProxyFieldRef,
                new CodeArgumentReferenceExpression(CodeBuilderCommon.targetName));
            constructor.Statements.Add(assignProxy);
            typeProxy.Members.Add(constructor);
        }

        protected override CodeMemberMethod TargetRetrievalMethod(CodeTypeReference typerefDbusInterface, CodeTypeReference typerefDbusMarshal, CodeTypeReferenceExpression typerefexprDbusMarshal)
        {
            CodeMemberMethod methodGetWCFMethodTarget = CreateTargetRetrievalMethod(typerefDbusInterface, typerefexprDbusMarshal);
            methodGetWCFMethodTarget.Statements.Add(new CodeMethodReturnStatement(thisProxyFieldRef));
            return methodGetWCFMethodTarget;
        }

        protected override CodeVariableDeclarationStatement DeclareTargetVariable(CodeTypeReference typerefDbusInterface, CodeTypeReference typerefDbusMarshal)
        {
            return new CodeVariableDeclarationStatement(typerefDbusInterface, CodeBuilderCommon.targetName, invokeGetWCFMethodTarget);
        }
    } // Ends class WCFPassthruServiceBuilder
} // Ends dbusidltocode
