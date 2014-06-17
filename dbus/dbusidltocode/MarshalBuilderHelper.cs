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
    abstract class MarshalBuilderHelper
    {
        protected static string PropertyNameFromFieldName(string fieldName)
        {
            return fieldName[0].ToString().ToUpper() + fieldName.Substring(1);
        }

        const string SendHandleName = "msgHandleSend";
        const string ResponseDataName = "msgResp";
        const string MessageBuilderName = "builder";
        const string nameMessageReader = "reader";

        static protected readonly CodeVariableReferenceExpression varrefSig = new CodeVariableReferenceExpression("sig");
        static protected readonly CodeTypeReference typerefSig = new CodeTypeReference(typeof(Udbus.Types.dbus_sig));
        static protected readonly CodeFieldReferenceExpression fieldrefSigA = new CodeFieldReferenceExpression(varrefSig, "a");
        static protected readonly CodeTypeReferenceExpression typerefexprDbusType = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Types.dbus_type)));
        static protected readonly CodeVariableReferenceExpression varrefReader = new CodeVariableReferenceExpression(nameMessageReader);
        static protected readonly CodeVariableReferenceExpression varrefBuilder = new CodeVariableReferenceExpression(MessageBuilderName);
        static protected readonly CodeFieldReferenceExpression fieldrefResult = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.nameResult);
        static protected readonly CodeExpression exprResultOk = new CodeBinaryOperatorExpression(fieldrefResult, CodeBinaryOperatorType.IdentityEquality, CodeBuilderCommon.exprBuildResultSuccessValue);
        static protected readonly CodeExpression exprResultNotOk = new CodeBinaryOperatorExpression(fieldrefResult, CodeBinaryOperatorType.IdentityInequality, CodeBuilderCommon.exprBuildResultSuccessValue);
        static protected readonly CodeFieldReferenceExpression fieldrefConnectionParameters = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CodeBuilderCommon.nameConnectionParameters);
        static protected readonly CodePropertyReferenceExpression proprefConnectionParameters = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(),
                                                                                                                                    PropertyNameFromFieldName(CodeBuilderCommon.nameConnectionParameters));

        static protected readonly CodeTypeReference typerefMessagePair = new CodeTypeReference(typeof(Udbus.Core.UdbusMessagePair));
        static protected readonly CodeVariableReferenceExpression varrefResponseMessagePair = new CodeVariableReferenceExpression(ResponseDataName);
        static public readonly CodeParameterDeclarationExpression paramdeclMessageResponse = new CodeParameterDeclarationExpression(typerefMessagePair, ResponseDataName);

        static protected readonly CodePropertyReferenceExpression proprefResponseData = new CodePropertyReferenceExpression(varrefResponseMessagePair, "Data");
        static protected readonly CodeBinaryOperatorExpression compResponseOk = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(varrefResponseMessagePair, "QuEmpty"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(false));

        public readonly static CodeTypeReferenceExpression typerefexprArgumentInException = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMessageMethodArgumentInException)));
        public readonly static CodeTypeReferenceExpression typerefexprArgumentOutException = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMessageMethodArgumentOutException)));
        public readonly static CodeTypeReferenceExpression typerefexprSignalArgumentException = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Core.Exceptions.UdbusMessageSignalArgumentException)));
        public readonly static CodeTypeReferenceExpression typerefexprMethodError = new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Udbus.Serialization.Exceptions.UdbusMessageMethodErrorException)));

        public virtual void HandleResponse(string idlMethodName, CodeStatementCollection statementsTryRecv, List<CodeStatement> statementsReponse)
        {
            // Create message reader.
            // * Udbus.Core.UdbusMessageReader reader = new Udbus.Core.UdbusMessageReader(msgHandleResp);
            statementsTryRecv.Insert(0, this.CreateMessageReader());

            // * Udbus.Core.NMessageHandle.UdbusMessageHandle msgHandleResp = null;
            statementsReponse.Add(this.DeclareResponseVariable());
            CodeTryCatchFinallyStatement trycatchReceive = this.ReceiveMessage(idlMethodName, statementsTryRecv);
            statementsReponse.Add(trycatchReceive);

        }


        public virtual CodeVariableDeclarationStatement DeclareResponseVariable()
        {
            // * Udbus.Core.NMessageHandle.UdbusMessageHandle msgHandleResp = null;
            return new CodeVariableDeclarationStatement(typerefMessagePair, ResponseDataName
                , new CodeDefaultValueExpression(typerefMessagePair)
            );
        }

        public virtual CodeTryCatchFinallyStatement ReceiveMessage(string methodName, CodeStatementCollection statementsTryRecv)
        {
            CodeTryCatchFinallyStatement trycatchReceive = new CodeTryCatchFinallyStatement(
                new CodeStatement[]
                {
                    // ReceiveMessage
                    this.ReceiveMessage(),
                    // * if (((this.result == 0) && (msgHandleResp != null))) // If received message ok
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            exprResultOk,
                            CodeBinaryOperatorType.BooleanAnd,
                            this.IfResponseHandleNotNull()
                        )
                        ,new CodeStatement[] // True statements
                        {
                            // * if ((msgResp.Data.typefield.type == Udbus.Core.dbus_msg_type.DBUS_TYPE_ERROR))
                            new CodeConditionStatement(
                                this.IfResponseIsDbusError()
                                ,new CodeStatement[]
                                {
                                    // * throw Udbus.Core.Exceptions.UdbusMessageMethodErrorException.Create("<method_name>", this.ConnectionParameters, msgStructResp);
                                    this.ThrowMethodErrorException(methodName)
                                }
                                ,statementsTryRecv.Cast<CodeStatement>().ToArray() // False statements
                            )
                        }
                        ,new CodeStatement[] // False statements
                        {
                            // * throw Udbus.Core.Exceptions.UdbusMethodReceiveException.Create("<method_name>", this.result, this.ConnectionParameters);
                            this.ThrowMethodReceiveException(methodName)
                        }
                    )

                },
                new CodeCatchClause[] { },
                new CodeStatement[] // Finally
                {
                    new CodeConditionStatement(
                        compResponseOk, new CodeExpressionStatement(new CodeMethodInvokeExpression(varrefResponseMessagePair, "Dispose"))
                    )
                }
            );
            return trycatchReceive;
        }

        public virtual CodeVariableDeclarationStatement CreateMessageReader()
        {
            // * Udbus.Core.UdbusMessageReader reader = new Udbus.Core.UdbusMessageReader(msgHandleResp);
            return new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefMessageReader, nameMessageReader,
                            new CodeObjectCreateExpression(CodeBuilderCommon.typerefMessageReader, new CodePropertyReferenceExpression(varrefResponseMessagePair, "Handle"))
                        );
        }

        public virtual CodeConditionStatement FinishInArguments(string idlMethodName, MarshalBuilderHelper codebuilder, CodeConditionStatement condMethodSignature
            , CodeConditionStatement condIn, CodeConditionStatement condInIter, CodeThrowExceptionStatement throwargInPrev)
        {
            if (condIn != null) // If got arguments to add
            {
                if (condInIter != null && condInIter != condIn) // If multiple arguments
                {
                    // Check the final argument is ok.
                    // * if (this.result != 0) { throw UdbusMessageMethodArgumentInException.Create(...); }
                    condInIter.TrueStatements.Add(codebuilder.ThrowArgInException(throwargInPrev));

                } // Ends if multiple arguments

                condMethodSignature.TrueStatements.Add(condIn);
                condIn = condMethodSignature;

                CodeConditionStatement condMethodResult = codebuilder.AddBodyLength(idlMethodName);
                condMethodResult.TrueStatements.Add(condIn);
                condIn = condMethodResult;

            } // Ends if got arguments to add
            return condIn;
        }

        public virtual CodeAssignStatement SetSignature()
        {
            // * this.result = builder.SetSignature(ref sig).Result;
            return new CodeAssignStatement(fieldrefResult
                , new CodeFieldReferenceExpression(new CodeMethodInvokeExpression(varrefBuilder, "SetSignature", new CodeDirectionExpression(FieldDirection.Ref, varrefSig)), "Result"));
        }

        public virtual CodeThrowExceptionStatement ThrowMethodReceiveException(string methodName)
        {
            return new CodeThrowExceptionStatement(
                                            new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprReceiveException, "Create",
                                                new CodePrimitiveExpression(methodName),
                                                fieldrefResult,
                                                proprefConnectionParameters
                                            )
                                        )
            ;
        }

        public virtual CodeThrowExceptionStatement ThrowMethodErrorException(string methodName)
        {
            return new CodeThrowExceptionStatement( // True
                                                    new CodeMethodInvokeExpression(MarshalBuilderHelper.typerefexprMethodError, "Create",
                                                        new CodePrimitiveExpression(methodName),
                                                        //varrefRecvStruct,
                                                        proprefConnectionParameters,
                                                        proprefResponseData
                                                    )
                                                )
            ;
        }

        public virtual CodeBinaryOperatorExpression IfResponseIsDbusError()
        {
            // * if ((msgResp.Data.typefield.type == Udbus.Core.dbus_msg_type.DBUS_TYPE_ERROR))
            return  new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(proprefResponseData, "typefield"), "type")
                        , CodeBinaryOperatorType.IdentityEquality
                        , CodeBuilderCommon.proprefMessageTypeError
                    );
        }

        public virtual CodeBinaryOperatorExpression IfResponseHandleNotNull()
        {
            // * msgResp.QuEmpty != false
            return compResponseOk;
        }

        public virtual CodeAssignStatement ReceiveMessage()
        {
            // * msgResp = this.ReceiverPool.ReceiveMessageData(this.Connector, serial, out this.result);
            return new CodeAssignStatement(varrefResponseMessagePair,
                new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), PropertyNameFromFieldName(CodeBuilderCommon.nameReceiverPool)),
                                                "ReceiveMessageData",
                                                this.GetReceiveConnector(),
                                                CodeBuilderCommon.varrefSerial,
                    new CodeDirectionExpression(FieldDirection.Out, fieldrefResult)
            ));
        }

        public virtual void TryCatchSend(CodeStatementCollection statements, CodeVariableReferenceExpression varrefSendHandle, CodeStatementCollection statementsTrySend)
        {
            CodeTryCatchFinallyStatement trycatchSend = new CodeTryCatchFinallyStatement(
                statementsTrySend.Cast<CodeStatement>().ToArray(),
                new CodeCatchClause[] { },
                new CodeStatement[] // Finally
                {
                    // Dispose in finally block.
                    new CodeConditionStatement(new CodeBinaryOperatorExpression(varrefSendHandle, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(varrefSendHandle, "Dispose"))
                    )
                }
            );
            statements.Add(trycatchSend);
        }

        protected virtual CodePropertyReferenceExpression GetSendConnector()
        {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Connector");
        }

        protected virtual CodePropertyReferenceExpression GetReceiveConnector()
        {
            return this.GetSendConnector();
        }

        public virtual void CallSend(string methodName, CodeVariableReferenceExpression varrefSendHandle, CodeStatementCollection statementsTrySend)
        {
            // Call "Send".
            // * if ((this.result == 0))
            // * {
            // *     msgHandleSend = builder.Message;
            // *     this.result = this.connector.Send(msgHandleSend);
            // * }
            statementsTrySend.Add(new CodeConditionStatement(
                exprResultOk,
                new CodeAssignStatement(varrefSendHandle, new CodeFieldReferenceExpression(
#if USE_FLUID_MESSAGE_BUILDER
                            // Single statement constructing message.
                            invokeBuild,
#else // !USE_FLUID_MESSAGE_BUILDER
                // Builder containing completed message.
                varrefBuilder,
#endif // USE_FLUID_MESSAGE_BUILDER
                "Message")
                ),
                new CodeAssignStatement(fieldrefResult,
                    new CodeMethodInvokeExpression(this.GetSendConnector(), "Send", varrefSendHandle)
                ),
                // * if (this.result != 0)
                new CodeConditionStatement(exprResultNotOk,
                    // * throw Udbus.Core.Exceptions.UdbusMethodSendException.Create("<method_name>", this.result, this.ConnectionParameters);
                    new CodeThrowExceptionStatement(
                        new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprSendException, "Create",
                            new CodePrimitiveExpression(methodName),
                            fieldrefResult,
                            proprefConnectionParameters
                        )
                    )
                )
            ));
        }

        public virtual CodeVariableReferenceExpression DeclareSendHandle()
        {
            // * Udbus.Core.NMessageHandle.UdbusMessageHandle msgHandleSend = null;
            CodeVariableReferenceExpression varrefSendHandle = new CodeVariableReferenceExpression(SendHandleName);
            return varrefSendHandle;
        }

        public virtual CodeConditionStatement AddBodyLength(string name)
        {
            CodeConditionStatement condMethodResult = new CodeConditionStatement(
                // * if (this.result == 0)
                exprResultOk
                , new CodeStatement[] // True statements
                {
                    // * this.result = builder.BodyAdd(BodyLength).Result;
                    new CodeAssignStatement(fieldrefResult,
                        new CodeFieldReferenceExpression(new CodeMethodInvokeExpression(varrefBuilder, "BodyAdd", new CodeFieldReferenceExpression(null, "BodyLength")), "Result"))
                }
                , new CodeStatement[] // False statements
                {
                    // Unknown stuff
                    this.ThrowMessageBuilderException(name, "UdbusMethodMessage")
                }
            );
            return condMethodResult;
        }

        public virtual CodeConditionStatement ThrowArgInException(CodeThrowExceptionStatement throwargInPrev)
        {
            return new CodeConditionStatement(exprResultNotOk, throwargInPrev);
        }

        abstract public CodeStatement InvokeBuild(CodeStatementCollection statements, string idlMethodName);

        protected CodeStatement InvokeBuild(CodeStatementCollection statements, string idlMethodName, CodeExpression exprConnectionParameter)
        {
            // * this.result = builder.UdbusMethodMessage(serial, connectionParams, "<method_name>").Result;
            CodeMethodInvokeExpression invokeBuild = new CodeMethodInvokeExpression(varrefBuilder,
               "UdbusMethodMessage",
               CodeBuilderCommon.varrefSerial,
               exprConnectionParameter,
               new CodePrimitiveExpression(idlMethodName)
            );
            CodeStatement stmtBuildMethod = new CodeAssignStatement(
                fieldrefResult, new CodeFieldReferenceExpression(invokeBuild, "Result")
            );
            return stmtBuildMethod;
        }

        public virtual void DeclareMessageHandle(CodeStatementCollection statements)
        {
            // Declare message handle.
            // * Udbus.Core.NMessageHandle.UdbusMessageHandle msgHandleSend = null;
            statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Udbus.Serialization.NMessageHandle.UdbusMessageHandle)), SendHandleName, new CodePrimitiveExpression(null)));
        }

        public virtual void InitialiseMessageBuilder(CodeStatementCollection statements)
        {
            // Add a message builder.
            // * Udbus.Core.UdbusMessageBuilder builder = new Udbus.Core.UdbusMessageBuilder();
            statements.Add(new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefMessageBuilder, MessageBuilderName
                , new CodeObjectCreateExpression(CodeBuilderCommon.typerefMessageBuilder)
            ));
        }

        public virtual void AddSerialNumber(CodeStatementCollection statements)
        {
            // Add serial number.
            // * uint serial = this.serialManager.GetNext();
            statements.Add(new CodeVariableDeclarationStatement(typeof(System.UInt32), CodeBuilderCommon.nameSerialNumber
                , new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), PropertyNameFromFieldName("serialManager"))
                , "GetNext"
                )
            ));
        }

        public virtual void TerminateSignature(CodeStatementCollection statements, bool bInParameters, int nInArgSigCounter)
        {
            if (bInParameters) // If got input parameters
            {
                // Add signature terminator.
                // * sig.a[<X>] = Udbus.Types.dbus_type.DBUS_INVALID;
                statements.Add(new CodeAssignStatement(
                     new CodeArrayIndexerExpression(fieldrefSigA, new CodePrimitiveExpression(nInArgSigCounter))
                    , new CodeFieldReferenceExpression(typerefexprDbusType, Enum.GetName(typeof(Udbus.Types.dbus_type), Udbus.Types.dbus_type.DBUS_INVALID))
                ));

            } // Ends if got input parameters
        }

        public virtual void AssignResults(CodeStatementCollection statementsTryRecv
            , CodeConditionStatement condOut, CodeConditionStatement condOutIter
            , CodeStatementCollection stmtsFinishResult, CodeThrowExceptionStatement throwargOutPrev

            , string idlMethodName, ref int nOutArgCounter
            )
        {
            if (condOut != null) // If there are out parameters
            {
                if (stmtsFinishResult.Count > 0) // If there's any out result parameters
                {
                    // Assign if successful.
                    condOutIter.TrueStatements.Add(new CodeConditionStatement(
                        // * if (this.result == 0)
                        exprResultOk,
                        stmtsFinishResult.Cast<CodeStatement>().ToArray() // True statements
                        , new CodeStatement[] // False statements
                        {
                            throwargOutPrev
                        }
                    ));

                } // Ends if there's any out result parameters

                // Add the root condition variable.
                statementsTryRecv.Add(condOut);

            } // Ends if there are out parameters
        }

        public virtual void PrefixOutParams(ref CodeConditionStatement condOut, ref CodeConditionStatement condOutIter, string idlMethodName, ref int nOutArgCounter, ref CodeThrowExceptionStatement throwargOutPrev)
        {
        }

        public virtual CodeParameterDeclarationExpression AddParameter(CodeParameterDeclarationExpressionCollection parameters
            , string idlArgName, string idlArgDirection, CodeTypeReference typerefParam)
        {
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(typerefParam, idlArgName);

            if (idlArgDirection == "out") // If output parameter
            {
                param.Direction = FieldDirection.Out;

            } // Ends if output parameter

            parameters.Add(param);
            return param;
        }

        public virtual int DeclareSignature(CodeStatementCollection statements, int nInArgSigCounter, string idlType)
        {
            Udbus.Types.dbus_sig sig = Udbus.Types.dbus_sig.Initialiser;
            int sigLength = DbusSignatureFromIDLArgumentType(ref sig, idlType);
            for (int sigIndex = 0; sigIndex < sigLength; ++sigIndex)
            {
                statements.Add(new CodeAssignStatement(
                        new CodeArrayIndexerExpression(fieldrefSigA, new CodePrimitiveExpression(nInArgSigCounter))
                    , new CodeFieldReferenceExpression(typerefexprDbusType, Enum.GetName(typeof(Udbus.Types.dbus_type), sig.a[sigIndex]))
                ));

                ++nInArgSigCounter;

            } // Ends loop populating sig
            return nInArgSigCounter;
        }

        public virtual void StoreCondIterator(ref CodeConditionStatement cond, ref CodeConditionStatement condIter, CodeConditionStatement condVarResult)
        {
            if (cond == null)
            {
                cond = condVarResult;
                condIter = cond;

            } // Ends if first in result
            else
            {
                condIter.TrueStatements.Add(condVarResult);
                condIter = condVarResult;
            }
        }

        public virtual CodeThrowExceptionStatement ThrowMessageBuilderException(string methodName, string actionName)
        {
            CodeThrowExceptionStatement throwEx = new CodeThrowExceptionStatement(
                new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprBuildException, "Create",
                    new CodePrimitiveExpression(actionName),
                    CodeBuilderCommon.varrefSerial,
                    new CodePrimitiveExpression(methodName),
                    fieldrefResult,
                    proprefConnectionParameters
                )
            );
            return throwEx;
        }

        public virtual bool InitialiseSignature(IEnumerable<IDLMethodArgument> arguments, CodeStatementCollection statements)
        {
            bool bInParameters = false;
            foreach (IDLMethodArgument idlMethodArg in arguments)
            {
                if (idlMethodArg.Direction != "out")
                {
                    // Add a signature
                    statements.Add(
                        new CodeVariableDeclarationStatement(typerefSig, varrefSig.VariableName,
                            new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typerefSig), "Initialiser")
                    ));
                    bInParameters = true;
                    break;
                }
            } // Ends loop checking for in parameters
            return bInParameters;
        }

        public virtual void MakeOutArgument(CodeStatementCollection statements
            , CodeStatementCollection stmtsFinishResult
            , string idlMethodName
            , CodeTypeFactory codetypeFactoryOut
            , ref int nOutArgCounter
            , Udbus.Parsing.BuildContext context
            , ref CodeThrowExceptionStatement throwargOutPrev
            , IDLArgument idlMethodArg
            //, IDLMethodArgument idlMethodArg
            , Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder
            , ref ParamCodeTypeHolderMarshalBase paramtypeHolder
            , ref CodeTypeReference typerefParam
            , out CodeConditionStatement condVarResult)
        {
            marshal.outward.ParamCodeTypeHolderMarshalOut paramtypeHolderOut = new marshal.outward.ParamCodeTypeHolderMarshalOut(codetypeFactoryOut);
            Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolderOut, nameBuilder, idlMethodArg.Type, context);
            paramtypeHolder = paramtypeHolderOut;
            typerefParam = paramtypeHolder.paramtype.CodeType;

            // Initialise out parameter.
            this.AssignOutParamDefault(statements, idlMethodArg, typerefParam);

            // Read out parameter from message into temporary.
            string argResultVar = idlMethodArg.Name + "Result";
            CodeVariableReferenceExpression varrefResultVar = new CodeVariableReferenceExpression(argResultVar);

            condVarResult = new CodeConditionStatement(
                // * if (this.result == 0)
                exprResultOk,
                new CodeStatement[] // True statements
                {
                    // * <param_type> <arg_name>Result;
                    new CodeVariableDeclarationStatement(typerefParam, argResultVar),
                    // * this.result = reader.Marshal<type>(<marshal_function>, out <arg_name>Result);
                    new CodeAssignStatement(fieldrefResult,
                        paramtypeHolderOut.BuildReadExpression(varrefReader,
                            new CodeDirectionExpression(FieldDirection.Out, varrefResultVar)))
                }
                , new CodeStatement[] // False statements
                {
                    throwargOutPrev
                }
                );

            throwargOutPrev = this.ThrowArgOutException(idlMethodName, nOutArgCounter, idlMethodArg.Name, typerefParam);

            // * <arg_name> = <arg_name>Result;
            this.FinishArgument(idlMethodName, stmtsFinishResult, idlMethodArg, varrefResultVar);
            ++nOutArgCounter;
        }

        public virtual void FinishArgument(string idlMethodName, CodeStatementCollection stmtsFinishResult, IDLArgument idlMethodArg, CodeVariableReferenceExpression varrefResultVar)
        {
            stmtsFinishResult.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression(idlMethodArg.Name), varrefResultVar));
        }

        protected virtual CodeTypeReferenceExpression GetOutArgumentExceptionType()
        {
            return MarshalBuilderHelper.typerefexprArgumentOutException;
        }

        internal static CodeThrowExceptionStatement ThrowArgOutExceptionImpl(string idlMethodName, int nOutArgCounter, string idlMethodArgName, CodeTypeReference typerefParam,
            CodeTypeReferenceExpression typerefexprArgumentOut)
        {
            CodeThrowExceptionStatement throwargOutPrev = new CodeThrowExceptionStatement(
                new CodeMethodInvokeExpression(typerefexprArgumentOut, "Create",
                    new CodePrimitiveExpression(nOutArgCounter),
                    new CodePrimitiveExpression(idlMethodArgName),
                    new CodeTypeOfExpression(typerefParam),
                    fieldrefResult,
                    new CodePrimitiveExpression(idlMethodName),
                    fieldrefConnectionParameters,
                    proprefResponseData
                )
            );
            return throwargOutPrev;
        }

        public virtual CodeThrowExceptionStatement CreateArgumentOutException(string name)
        {
            return ThrowArgOutExceptionImpl(name, 0, "UnknownParameters", CodeBuilderCommon.typerefUnknownParameters, this.GetOutArgumentExceptionType());
        }

        public virtual CodeThrowExceptionStatement ThrowArgOutException(string idlMethodName, int nOutArgCounter, string idlMethodArgName, CodeTypeReference typerefParam)
        {
            return ThrowArgOutExceptionImpl(idlMethodName, nOutArgCounter + 1, idlMethodArgName, typerefParam, this.GetOutArgumentExceptionType());
        }

        public virtual void AssignOutParamDefault(CodeStatementCollection statements
            , IDLArgument idlMethodArg
            , CodeTypeReference typerefParam)
        {
            CodeAssignStatement assignOut = new CodeAssignStatement(new CodeArgumentReferenceExpression(idlMethodArg.Name),
                new CodeDefaultValueExpression(typerefParam));
            statements.Add(assignOut);
        }

        public virtual void MakeInArgument(CodeTypeFactory codetypefactoryIn
            , string idlMethodName
            , ref int nInArgCounter
            , Udbus.Parsing.BuildContext context
            , ref CodeThrowExceptionStatement throwargInPrev
            , IDLMethodArgument idlMethodArg
            , Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder
            , ref ParamCodeTypeHolderMarshalBase paramtypeHolder
            , ref CodeTypeReference typerefParam
            , out CodeConditionStatement condVarResult)
        {
            marshal.inward.ParamCodeTypeHolderMarshalIn paramtypeHolderIn = new marshal.inward.ParamCodeTypeHolderMarshalIn(codetypefactoryIn);
            Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(paramtypeHolderIn, nameBuilder, idlMethodArg.Type, context);
            paramtypeHolder = paramtypeHolderIn;
            /*typerefParamIter = */
            typerefParam = paramtypeHolder.paramtype.CodeType;

            condVarResult = new CodeConditionStatement(
                // * if (this.result == 0)
                exprResultOk
                , new CodeStatement[] // True statements
                {
                    // * this.result = builder.Marshal<arg_type>(<marshal_function>, <arg_name>);
                    new CodeAssignStatement(fieldrefResult,
                        paramtypeHolderIn.BuildWriteExpression(varrefBuilder,
                            MarshalArgument(idlMethodName, idlMethodArg)))
                }
                , new CodeStatement[] // False statements
                {
                    throwargInPrev
                }
                );

            throwargInPrev = new CodeThrowExceptionStatement(
                new CodeMethodInvokeExpression(MarshalBuilderHelper.typerefexprArgumentInException, "Create",
                    new CodePrimitiveExpression(nInArgCounter + 1),
                    new CodePrimitiveExpression(idlMethodArg.Name),
                    new CodeTypeOfExpression(typerefParam),
                    fieldrefResult,
                    new CodePrimitiveExpression(idlMethodName),
                    fieldrefConnectionParameters
                )
            );

            ++nInArgCounter;
        }

        public virtual CodeExpression MarshalArgument(string idlMethodName, IDLMethodArgument idlMethodArg)
        {
            return new CodeArgumentReferenceExpression(idlMethodArg.Name);
        }

        static private int DbusSignatureFromIDLArgumentType(ref Udbus.Types.dbus_sig sig, string idlType)
        {
            int nSigIndex = 0;
            foreach (char typeChar in idlType)
            {
                switch (typeChar)
                {
                    case 'g':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_SIGNATURE;
                        break;
                    case 'o':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_OBJECTPATH;
                        break;
                    case 'b':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_BOOLEAN;
                        break;
                    case 's':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_STRING;
                        break;
                    case 'n':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_INT16;
                        break;
                    case 'q':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_UINT16;
                        break;
                    case 'i':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_INT32;
                        break;
                    case 'u':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_UINT32;
                        break;
                    case 'x':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_INT64;
                        break;
                    case 't':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_UINT64;
                        break;
                    case 'd':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_DOUBLE;
                        break;
                    case 'a':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_ARRAY;
                        break;
                    case '(':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN;
                        break;
                    case ')':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_STRUCT_END;
                        break;
                    case '{':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_DICT_BEGIN;
                        break;
                    case '}':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_DICT_END;
                        break;
                    case 'v':
                        sig.a[nSigIndex] = Udbus.Types.dbus_type.DBUS_VARIANT;
                        break;
                    default:
                        throw new Exception(string.Format("Unknown IDL Type character: '{0}'", typeChar));
                    //break;

                } // Ends switch idl type character
                ++nSigIndex;
            } // Ends loop over IDL type characters

            return nSigIndex;
        }

    } // Ends class MarshalBuilderHelper

    class MarshalBuilderHelperMethod : MarshalBuilderHelper
    {
        public override CodeStatement InvokeBuild(CodeStatementCollection statements, string idlMethodName)
        {
            return this.InvokeBuild(statements, idlMethodName, proprefConnectionParameters);
        }
    } // Ends class MarshalBuilderHelperMethod

    class MarshalBuilderHelperSignalAddMatchMethod : MarshalBuilderHelperMethod
    {
        protected override CodePropertyReferenceExpression GetSendConnector()
        {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "SignalConnector");
        }
    
        public override CodeStatement InvokeBuild(CodeStatementCollection statements, string idlMethodName)
        {
            // * Udbus.Core.DbusConnectionParameters connectionParams = new Udbus.Core.DbusConnectionParameters("org.freedesktop.DBus", "/org/freedesktop/DBus", "org.freedesktop.DBus");
            statements.Add(new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefConnectionParameters, CodeBuilderCommon.nameConnectionParameters,
                new CodeObjectCreateExpression(CodeBuilderCommon.typerefConnectionParameters
                    , new CodePrimitiveExpression("org.freedesktop.DBus")
                    , new CodePrimitiveExpression("/org/freedesktop/DBus")
                    , new CodePrimitiveExpression("org.freedesktop.DBus")
                )
            ));
            return this.InvokeBuild(statements, idlMethodName, CodeBuilderCommon.varrefConnectionParameters);
        }

    } // Ends class MarshalBuilderHelperSignalAddMatchMethod

    class MarshalBuilderHelperProperty : MarshalBuilderHelper
    {
        public override CodeStatement InvokeBuild(CodeStatementCollection statements, string idlMethodName)
        {
            // * Udbus.Core.DbusConnectionParameters connectionParams = new Udbus.Core.DbusConnectionParameters(this.ConnectionParameters);
            statements.Add(new CodeVariableDeclarationStatement(CodeBuilderCommon.typerefConnectionParameters, CodeBuilderCommon.nameConnectionParameters,
                new CodeObjectCreateExpression(CodeBuilderCommon.typerefConnectionParameters, proprefConnectionParameters)
            ));
            // * connectionParams.Interface = "org.freedesktop.DBus.Properties";
            statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(CodeBuilderCommon.varrefConnectionParameters, "Interface"), new CodePrimitiveExpression("org.freedesktop.DBus.Properties")));
            return this.InvokeBuild(statements, idlMethodName, CodeBuilderCommon.varrefConnectionParameters);
        }

        public override void AssignOutParamDefault(CodeStatementCollection statements
            , IDLArgument idlMethodArg
            , CodeTypeReference typerefParam)
        {
            // Properties don't take parameters, so have to declare as well as assign.
            CodeVariableDeclarationStatement vardeclParam = new CodeVariableDeclarationStatement(
                typerefParam, CodeBuilderCommon.nameReadValue, new CodeDefaultValueExpression(typerefParam)
            );
            statements.Add(vardeclParam);
        }

        public override CodeExpression MarshalArgument(string idlMethodName, IDLMethodArgument idlMethodArg)
        {
            // Marshal value as normal, but interface_name as connection parameter property, and property_name as primitive string.
            CodeExpression exprResult;
            switch (idlMethodArg.Name )
            {
                case "value":
                    exprResult = base.MarshalArgument(idlMethodName, idlMethodArg);
                    break;
                case "interface_name":
                    exprResult = new CodePropertyReferenceExpression(proprefConnectionParameters, "Interface");
                    break;
                case "property_name":
                    exprResult = new CodePrimitiveExpression(idlMethodName);
                    break;
                default:
                    exprResult = base.MarshalArgument(idlMethodName, idlMethodArg);
                    break;
            }
            return exprResult;
        }

        public override void PrefixOutParams(ref CodeConditionStatement condOut, ref CodeConditionStatement condOutIter, string idlMethodName, ref int nOutArgCounter, ref CodeThrowExceptionStatement throwargOutPrev)
        {
            CodeVariableReferenceExpression varrefVariantSignature = new CodeVariableReferenceExpression("variantSignature");
            CodeConditionStatement condVariantSignature = new CodeConditionStatement(
                // * if (this.result == 0)
                exprResultOk,
                new CodeStatement[] // True statements
                    {
                        // * dbus_sig variantSignature;
                        new CodeVariableDeclarationStatement(typerefSig, "variantSignature")
                        // * this.result = Udbus.Core.UdbusMessageReader.MarshalSignature(reader, out variantSignature);
                        , new CodeAssignStatement(fieldrefResult,
                            new CodeMethodInvokeExpression(CodeBuilderCommon.typerefexprMessageReader, "ReadSignature", varrefReader, 
                                new CodeDirectionExpression(FieldDirection.Out, varrefVariantSignature)))
                    }
                , new CodeStatement[] // False statements
                {
                    throwargOutPrev
                }
            );
            throwargOutPrev = ThrowArgOutException(idlMethodName, nOutArgCounter, "variantSignature", typerefSig);

            this.StoreCondIterator(ref condOut, ref condOutIter, condVariantSignature);
            ++nOutArgCounter;
        }

    } // Ends class MarshalBuilderHelperProperty

    class MarshalBuilderHelperSignal : MarshalBuilderHelper
    {
        public override CodeStatement InvokeBuild(CodeStatementCollection statements, string idlMethodName)
        {
            return this.InvokeBuild(statements, idlMethodName, proprefConnectionParameters);
        }

        protected const string SignalEventArgName = "args";

        internal static new CodeThrowExceptionStatement ThrowArgOutExceptionImpl(string idlMethodName, int nOutArgCounter, string idlMethodArgName, CodeTypeReference typerefParam,
            CodeTypeReferenceExpression typerefexprArgumentOut)
        {
            CodeThrowExceptionStatement throwargOutPrev = new CodeThrowExceptionStatement( // * throw Udbus.Core.Exceptions.UdbusMessageSignalArgumentException.Create(
                new CodeMethodInvokeExpression(typerefexprArgumentOut, "Create",
                    new CodePrimitiveExpression(nOutArgCounter), // * <nOutArg>,
                    new CodePrimitiveExpression(idlMethodArgName), // * "<signal_arg>",
                    new CodeTypeOfExpression(typerefParam), // * typeof(<signal_type>),
                    fieldrefResult, // * this.result,
                    new CodePrimitiveExpression(idlMethodName), // * "<signal>",
                    new CodeObjectCreateExpression( new CodeTypeReference(typeof(Udbus.Core.DbusSignalParams)),
                                                    new CodePropertyReferenceExpression(fieldrefConnectionParameters, "Interface"),
                                                    new CodePropertyReferenceExpression(fieldrefConnectionParameters, "Path")), // * new Udbus.DbusSignalParams(this.connectionParameters.Path, this.connectionParameters.Interface),
                    proprefResponseData // * messageData.Data);
                )
            );
            return throwargOutPrev;
        }

        public override CodeThrowExceptionStatement ThrowArgOutException(string idlMethodName, int nOutArgCounter, string idlMethodArgName, CodeTypeReference typerefParam)
        {
            return ThrowArgOutExceptionImpl(idlMethodName, nOutArgCounter, idlMethodArgName, typerefParam, this.GetOutArgumentExceptionType());
        }

        public override CodeThrowExceptionStatement CreateArgumentOutException(string name)
        {
            return ThrowArgOutExceptionImpl(name, 0, "UnknownParameters", CodeBuilderCommon.typerefUnknownParameters, this.GetOutArgumentExceptionType());
        }

        protected override CodeTypeReferenceExpression GetOutArgumentExceptionType()
        {
            return MarshalBuilderHelper.typerefexprSignalArgumentException;
        }

        public override void AssignOutParamDefault(CodeStatementCollection statements
            , IDLArgument idlMethodArg
            , CodeTypeReference typerefParam)
        {
            // No-op since signals don't have an out param.
        }

        public override void FinishArgument(string idlMethodName, CodeStatementCollection stmtsFinishResult, IDLArgument idlMethodArg, CodeVariableReferenceExpression varrefResultVar)
        {
            // Peek inside the stmsFinishResult and assume this function is the only one messing with the first statement...
            CodeObjectCreateExpression createArgs;

            if (stmtsFinishResult.Count == 0) // If no statements yet
            {
                // Setup everything.
                string signalEventTypeName = CodeBuilderCommon.GetSignalEventTypeName(idlMethodName);
                string signalEventName = CodeBuilderCommon.GetSignalEventName(idlMethodName);
                createArgs = new CodeObjectCreateExpression(signalEventTypeName);
                CodeVariableDeclarationStatement vardeclEventArgs = new CodeVariableDeclarationStatement(
                    signalEventTypeName
                    , SignalEventArgName
                    , createArgs
                );
                // * if (this.<signal>Event != null)
                CodeConditionStatement condGotEvent = new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeEventReferenceExpression(new CodeThisReferenceExpression()
                            , signalEventName
                        )
                        ,CodeBinaryOperatorType.IdentityInequality
                        , new CodePrimitiveExpression(null)
                    )
                    // * <signal>Args args = new <signal>Args(...);
                    , new CodeVariableDeclarationStatement(
                    signalEventTypeName
                        , SignalEventArgName
                        , createArgs
                    )
                    // * this.<signal>Event(this, args);
                    , new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), signalEventName
                        , new CodeThisReferenceExpression()
                        , new CodeVariableReferenceExpression(SignalEventArgName)
                    ))
                );

                stmtsFinishResult.Add(condGotEvent);

            } // Ends if no statements yet
            else // Else got statements
            {
                // Find the initialiser for the Args creation.
                CodeConditionStatement condGotEvent = stmtsFinishResult[0] as CodeConditionStatement;
                CodeVariableDeclarationStatement vardeclEventArgs = condGotEvent.TrueStatements[0] as CodeVariableDeclarationStatement;
                createArgs = vardeclEventArgs.InitExpression as CodeObjectCreateExpression;

            } // Ends else got statements

            // Add result variable to list of parameters for argument.
            createArgs.Parameters.Add(varrefResultVar);

        }
    } // Ends class MarshalBuilderHelperSignal

}
