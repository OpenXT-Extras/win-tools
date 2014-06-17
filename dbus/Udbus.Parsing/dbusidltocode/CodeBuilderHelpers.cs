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

namespace Udbus.Parsing
{
    #region Struct Handling
    /// <summary>
    /// Declares a struct and adds fields to it.
    /// </summary>
    public abstract class StructCreatorBase : IStructParamCodeTypeHandler
    {
        private uint fieldCounter = 0;
        private uint structIndex = 1;
        protected string name = null;
        protected string fullname = null;
        protected IParamCodeTypeHandler owner;

        public StructCreatorBase(IParamCodeTypeHandler owner)
        {
            this.owner = owner;
        }

        public abstract IStructParamCodeTypeHandler CreateNew();

        /// <summary>
        /// Build the nested struct name.
        /// </summary>
        /// <param name="name">Name of struct being built.</param>
        /// <param name="context">Context, which contains parent struct names.</param>
        /// <returns>Full nested struct name.</returns>
        protected static string BuildNestedName(string name, BuildContext context)
        {
            string nestedName = name;

            if (context.StructNames != null)
            {
                nestedName = string.Join(".", context.StructNames);
                nestedName += "." + name;
            }

            return nestedName;
        }

        /// <summary>
        /// Build the nested struct name.
        /// </summary>
        /// <param name="context">Context, which contains parent struct names.</param>
        /// <returns>Full nested struct name.</returns>
        protected string BuildNestedName(BuildContext context)
        {
            return BuildNestedName(this.name, context);
        }

        /// <summary>
        /// Build the nested scoped struct name.
        /// </summary>
        /// <param name="context">Context, which contains parent struct names.</param>
        /// <returns>Full nested scoped struct name.</returns>
        protected string BuildNestedScopedName(BuildContext context)
        {
            return BuildNestedName(this.fullname, context);
        }

        #region IStructBuilder Members

        public abstract void AddField(CodeTypeReference codeTypeField);

        public virtual void StartStruct(string name, string fullName, BuildContext context)
        {
            this.fieldCounter = 0;
            this.structIndex = context.StructIndex; // Holds onto struct index.
            ++context.StructDepth;
            this.name = name;
            this.fullname = fullName;
        }

        protected CodeMemberField BuildField(CodeTypeReference codeTypeField)
        {
            CodeMemberField field = new CodeMemberField();
            field.Attributes = MemberAttributes.Public;
            field.Name = string.Format("Field{0}", ++fieldCounter);
            field.Type = codeTypeField;
            return field;
        }

        public virtual void EndStruct(BuildContext context, IParamCodeTypeHandler paramtypeHandler)
        {
            ++context.StructIndex;
            --context.StructDepth;
        }

        protected BuildContext CreateNestedContext(BuildContext context, ICodeTypeDeclarationHolder declarationHolder)
        {
            BuildContext contextNested = new BuildContext(declarationHolder);
            contextNested.StructIndex = this.structIndex;
            contextNested.StructDepth = context.StructDepth;
            int StructNamesLength = context.StructNames == null ? 1 : context.StructNames.Length + 1;
            contextNested.StructNames = new string[StructNamesLength];
            if (context.StructNames != null)
            {
                context.StructNames.CopyTo(contextNested.StructNames, 0);
            }
            contextNested.StructNames[StructNamesLength - 1] = this.name;
            return contextNested;
        }

        public virtual BuildContext CreateNestedContext(BuildContext context)
        {
            return this.CreateNestedContext(context, new CodeTypeNoOpHolder());
        }

        public virtual void CloseNestedContext(BuildContext contextNested)
        {
            this.structIndex = contextNested.StructIndex; // Keep a record of struct index as we loop.
        }

        // Pass-through factory methods delegated to by FieldHandler.
        public virtual IDictParamCodeTypeHandler CreateDictHandler()
        {
            return owner.CreateDictHandler();
        }

        public virtual IParamCodeTypeHandler CreateNewArrayHandler()
        {
            return owner.CreateNewArrayHandler();
        }

        public virtual IParamCodeTypeHandler CreateArrayHandler()
        {
            // Note: Asking a struct to create an array, is really asking a struct to create a *new* array.
            return owner.CreateNewArrayHandler();
        }

        abstract public IParamCodeTypeHandler CreateFieldHandler();
        #endregion // Ends IStructBuilder Members

        public virtual void HandleCodeParamTypeField(ICodeParamType paramtype)
        {
            this.AddField(paramtype.CodeType);
        }
    } // Ends class StructCreatorBase

    #endregion // Struct Handling

    #region Dictionary Handling
    abstract public class DictionaryCreatorBase : IDictParamCodeTypeHandler
    {
        #region IDictParamCodeTypeHandler Members
        abstract public IParamCodeTypeHandler CreateKeyHandler();
        abstract public IParamCodeTypeHandler CreateValueHandler();

        uint dictionaryIndex = 0;
        public virtual void StartDictionary(string name, BuildContext context)
        {
            this.dictionaryIndex = context.DictionaryIndex;
        }

        public virtual void FinishDictionary(BuildContext context, IParamCodeTypeHandler paramtypeHandler)
        {
            ++context.DictionaryIndex;
        }
        #endregion // IDictParamCodeTypeHandler Members
    } // Ends class DictionaryCreatorBase

    #endregion // Dictionary Handling

    #region IDLTypes
    public class IDLObjectPath //: string
    {
        private string path;
        public IDLObjectPath(string path)
        {
            this.path = path;
        }

        public override string ToString()
        {
            return this.path;
        }

        public string Path { get { return this.path; } }

    } // Ends class IDLObjectPath

    public class IDLSignature //: string
    {
        private string signature;
        public IDLSignature(string signature)
        {
            this.signature = signature;
        }

        public override string ToString()
        {
            return this.signature;
        }

        public string Signature { get { return this.signature; } }

    } // Ends class IDLSignature
    #endregion // Ends IDLTypes

    #region Declaration Holders
    /// <summary>
    /// Tired of covariance and generics making your life easy ? YES ?
    /// Then how about some half arsed non-sensical interfaces.
    /// Go team awesome.
    /// </summary>
    public interface ICodeTypeDeclarationHolder
    {
        void Add(CodeTypeMember refAdd);
        void AddDictionary(CodeTypeMember refAdd);
        string Name { get; }

    } // Ends interface ICodeTypeDeclarationHolder

    /// <summary>
    /// Add to a collection of CodeTypeDeclaration.
    /// </summary>
    public class CodeTypeDeclarationHolder : ICodeTypeDeclarationHolder
    {
        CodeTypeDeclarationCollection collection;
        string name;
        public CodeTypeDeclarationHolder(CodeTypeDeclarationCollection collection, string name)
        {
            this.collection = collection;
            this.name = name;
        }

        public void Add(CodeTypeMember refAdd)
        {
            CodeTypeDeclaration typedeclAdd = refAdd as CodeTypeDeclaration;
            this.collection.Add(typedeclAdd);
        }

        public void AddDictionary(CodeTypeMember refAdd)
        {
            this.Add(refAdd);
        }

        public string Name
        { 
            get 
            { 
                return this.name; 
            }
        }
    } // Ends CodeTypeDeclarationHolder

    /// <summary>
    /// Add to a collection of CodeTypeMember.
    /// </summary>
    public class CodeTypeMemberHolder : ICodeTypeDeclarationHolder
    {
        CodeTypeMemberCollection collection;
        string name;

        public CodeTypeMemberHolder(CodeTypeMemberCollection collection, string name)
        {
            this.collection = collection;
            this.name = name;
        }

        public void Add(CodeTypeMember refAdd)
        {
            CodeTypeDeclaration typedeclAdd = refAdd as CodeTypeDeclaration;
            this.collection.Add(typedeclAdd);
        }

        public void AddDictionary(CodeTypeMember refAdd)
        {
            this.Add(refAdd);
        }

        public string Name 
        {
            get
            { 
                return this.name; 
            } 
        }
    } // Ends class CodeTypeMemberHolder

    /// <summary>
    /// Do nothing (doesn't add).
    /// </summary>
    public class CodeTypeNoOpHolder : ICodeTypeDeclarationHolder
    {
        public void Add(CodeTypeMember refAdd)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Not adding type member {0}", refAdd.Name));
        }

        public void AddDictionary(CodeTypeMember refAdd)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Not adding dictionary type member {0}", refAdd.Name));
        }

        public string Name {
            get
            { 
                return string.Empty; 
            } 
        }

    } // Ends class CodeTypeNoOpHolder

    /// <summary>
    /// Creates a namespace with the specified name on demand.
    /// </summary>
    public class CodeTypeDeferredNamespaceDeclarationHolder : ICodeTypeDeclarationHolder
    {
        public CodeNamespace ns;

        private string name;

        public CodeTypeDeferredNamespaceDeclarationHolder(string name)
        {
            this.name = name;
        }

        #region ICodeTypeDeclarationHolder Members

        public void Add(CodeTypeMember refAdd)
        {
            CodeTypeDeclaration typedeclAdd = refAdd as CodeTypeDeclaration;
            if (this.ns == null)
            {
                this.ns = new CodeNamespace(this.name);
            }

            this.ns.Types.Add(typedeclAdd);
        }

        public void AddDictionary(CodeTypeMember refAdd)
        {
            this.Add(refAdd);
        }


        public string Name { get { return this.name; } }
        #endregion
    } // Ends CodeTypeDeferredNamespaceDeclarationHolder

    /// <summary>
    /// Creates a class with the specified name on demand.
    /// </summary>
    public class CodeMemberDeferredClassHolder : ICodeTypeDeclarationHolder
    {
        public CodeTypeDeclaration typedecl = null;
        private ICodeTypeDeclarationHolder typeholder;
        public CodeTypeDeclaration typedeclDictionary = null;
        private ICodeTypeDeclarationHolder typeholderDictionary;
        private string name;
        private string nameDictionary;

        public ICodeTypeDeclarationHolder TypeHolderDictionary { set { this.typeholderDictionary = value; } }

        public CodeMemberDeferredClassHolder(ICodeTypeDeclarationHolder typeholder, string name,
            ICodeTypeDeclarationHolder typeholderDictionary, string nameDictionary)
        {
            this.typeholder = typeholder;
            this.name = name;
            this.typeholderDictionary = typeholderDictionary;
            this.nameDictionary = nameDictionary;
        }

        public CodeMemberDeferredClassHolder(ICodeTypeDeclarationHolder typeholder, string name,
            string nameDictionary)
            : this(typeholder, name, null, nameDictionary)
        {
        }

        #region ICodeTypeDeclarationHolder Members

        /// <summary>
        /// Add the type member as a method to a class.
        /// </summary>
        /// <param name="refAdd">Method to add.</param>
        public void Add(CodeTypeMember refAdd)
        {
            CodeMemberMethod methodAdd = refAdd as CodeMemberMethod;

            if (this.typedecl == null)
            {
                this.typedecl = new CodeTypeDeclaration(this.name);
                this.typeholder.Add(this.typedecl);
            }

            this.typedecl.Members.Add(methodAdd);
        }

        public void AddDictionary(CodeTypeMember refAdd)
        {
            CodeMemberMethod methodAdd = refAdd as CodeMemberMethod;

            if (this.typedeclDictionary == null)
            {
                this.typedeclDictionary = new CodeTypeDeclaration(this.nameDictionary);
                this.typeholderDictionary.Add(this.typedeclDictionary);
            }

            this.typedeclDictionary.Members.Add(methodAdd);
        }

        public void ResetDictionary(ICodeTypeDeclarationHolder typeholderDictionary)
        {
            this.TypeHolderDictionary = typeholderDictionary;
            this.typedeclDictionary = null;
        }

        public string Name { get { return this.name; } }
        #endregion
    } // Ends CodeTypeDeferredNamespaceDeclarationHolder

    /// <summary>
    /// Provides CodeTypeDeclaration.
    /// </summary>
    internal interface IDeferredCodeTypeDeclarationHolder
    {
        CodeTypeDeclaration CodeType { get; }
        bool QuType();

    } // Ends interface ICodeTypeReferenceCollection

    /// <summary>
    /// Defers CodeTypeDeclaration creation for a struct.
    /// </summary>
    public class CodeTypeDeferredStructHolder : IDeferredCodeTypeDeclarationHolder
    {
        private CodeTypeDeclaration codeType;
        private string structName;

        public CodeTypeDeferredStructHolder(string structName)
        {
            this.structName = structName;
        }

        #region IDeferredCodeTypeDeclarationHolder Members

        public CodeTypeDeclaration CodeType
        {
            get
            {
                if (this.codeType == null)
                {
                    this.codeType = new CodeTypeDeclaration(this.structName);
                }
                return this.codeType;
            }
        }

        public bool QuType()
        {
            return this.codeType != null;
        }

        #endregion
    } // Ends class CodeTypeDeferredStructHolder 
    #endregion // Ends Declaration Holders

    #region CodeParamTypes
    /// <summary>
    /// Helps solve the "can't have a reference to a declaration" issue.
    /// </summary>
    //protected 
    public interface ICodeParamType
    {
        CodeTypeReference CodeType { get; }
        CodeArrayCreateExpression CreateArray();

    } // Ends interface ICodeParamType

    /// <summary>
    /// Some common implementation.
    /// </summary>
    static class CodeParamImpl
    {
        internal static string ParamToString(ICodeParamType paramtype, string baseimpl)
        {
            string result = baseimpl;
            CodeTypeReference coderef = paramtype.CodeType;
            if (coderef != null)
            {
                result += ". " + coderef.BaseType.ToString();
            }
            return result;
        }
    }

    /// <summary>
    /// Holds a CodeTypeReference.
    /// </summary>
    //protected 
    public struct CodeParamType : ICodeParamType
    {
        private CodeTypeReference type;

        public CodeParamType(Type type)
            : this(new CodeTypeReference(type))
        {
        }

        public CodeParamType(CodeTypeReference type)
        {
            this.type = type;
        }


        public CodeTypeReference CodeType
        {
            get
            {
                return this.type;
            }
        }

        public CodeArrayCreateExpression CreateArray()
        {
            CodeArrayCreateExpression result = new CodeArrayCreateExpression(this.CodeType);
            return result;
        }

        public override string ToString()
        {
            return CodeParamImpl.ParamToString(this, base.ToString());
        }
    } // Ends CodeParamType

    /// <summary>
    /// Holds a CodeArrayCreateExpression for a type.
    /// </summary>
    //protected
    public struct CodeParamArray : ICodeParamType
    {
        CodeArrayCreateExpression arrayExpr;
        CodeTypeReference typeArray;

        public CodeParamArray(CodeTypeReference typeArray, CodeArrayCreateExpression arrayExpr)
        {
            this.typeArray = typeArray;
            this.arrayExpr = arrayExpr;
        }


        public CodeParamArray(CodeArrayCreateExpression arrayExpr)
            :this(new CodeTypeReference(arrayExpr.CreateType, 1), arrayExpr)
        {
        }

        public CodeTypeReference CodeType
        {
            get
            {
                return this.typeArray;
            }
        }

        public CodeArrayCreateExpression CreateArray()
        {
            CodeArrayCreateExpression result = new CodeArrayCreateExpression(this.CodeType);
            return result;
        }

        public override string ToString()
        {
            return CodeParamImpl.ParamToString(this, base.ToString());
        }
    } // Ends CodeParamArray

    /// <summary>
    /// Holds a CodeTypeDeclaration and provides the equivalent CodeTypeReference.
    /// </summary>
    //protected 
    public struct CodeParamDeclaredType : ICodeParamType
    {
        CodeTypeDeclaration declaration;

        public CodeParamDeclaredType(CodeTypeDeclaration declaration)
        {
            this.declaration = declaration;
        }

        public CodeTypeReference CodeType
        {
            get
            {
                // Here's hoping this works.
                return new CodeTypeReference(declaration.Name);
            }
        }

        public CodeArrayCreateExpression CreateArray()
        {
            CodeArrayCreateExpression result = new CodeArrayCreateExpression(this.CodeType);
            return result;
        }

        public override string ToString()
        {
            return CodeParamImpl.ParamToString(this, base.ToString());
        }
    } // Ends CodeParamDeclaredType
    #endregion // CodeParamTypes

    /// <summary>
    /// Helper for implementing code generation and traversing the argument type string.
    /// </summary>
    public class BuildContext
    {
        /// <summary>
        /// Counter for number of nested structs at a given level.
        /// </summary>
        public uint StructIndex = 1;

        /// <summary>
        /// Counter for depth of nested structs.
        /// </summary>
        public int StructDepth = 0;

        /// <summary>
        /// Counter for number of dictionaries at a given level.
        /// </summary>
        public uint DictionaryIndex = 1;

        /// <summary>
        /// Names of parent structs.
        /// </summary>
        //public List<string> StructNames = new List<string>();
        public string[] StructNames = null;

        /// <summary>
        /// Newly declared types (typically structs) are added via this interface.
        /// </summary>
        public ICodeTypeDeclarationHolder declarationHolder;

        public BuildContext(ICodeTypeDeclarationHolder declarationHolder)
        {
            this.declarationHolder = declarationHolder;
        }
    }

    #region Name Builders
    /// <summary>
    /// Base class for classes which build names for structs.
    /// </summary>
    public abstract class IDLArgumentTypeNameBuilderBase
    {
        abstract public string getNames(BuildContext context, out string fullName);
        abstract public string getScopedNames(BuildContext context, out string fullName);
        abstract public string getScopedName(string name);
        abstract public string getParamScopedName(string name);

        abstract public string getDictionaryNames(BuildContext context, out string fullName);
        abstract public string getDictionaryScopedNames(BuildContext context, out string fullName);
        abstract public string getDictionaryScopedName(string name);
        abstract public string getDictionaryParamScopedName(string name);

        static protected string getOuterNameImpl(BuildContext context, string methodName, uint index)
        {
            string name = methodName + "Param";
            if (index > 1)
            {
                name += index.ToString();
            }

            return name;
        }

        static protected string getOuterNameImpl(BuildContext context, string methodName)
        {
            return getOuterNameImpl(context, methodName, context.StructIndex);
            //string name = methodName + "Param";
            //if (context.StructIndex > 1)
            //{
            //    name += context.StructIndex.ToString();
            //}

            //return name;
        }

        static protected string getNameImpl(BuildContext context, string methodName, out string fullName)
        {
            string name;
            string outerName = getOuterNameImpl(context, methodName);

            if (context.StructDepth > 0)
            {
                //// We're in a struct.
                //// First struct - no decoration.
                //// Structs nested in structs get D<N> appended, where <N> is depth.
                //// This is because a nested type can't contain a nested type with the same name which is simply awesome.
                //// Structs after the first struct at a given level get I<N> appended, where <N> is index.
                name = "Nested" +
                    (context.StructDepth == 1 ? "" : "D" + context.StructDepth.ToString()) + // Depth
                    (context.StructIndex == 1 ? "" : "I" + context.StructIndex.ToString()); // Index
                // Can't deduce context so have to check on struct name stack.
                fullName = string.Join(".", context.StructNames) + "." + name;
            }
            else
            {
                // We're not in a struct. We're a param type. Make up something param-y.
                name = outerName;
                fullName = name;

            }
            return name;
        }

        static protected string getDictionaryNameImpl(BuildContext context, string methodName, out string fullName)
        {
            string outerName = getOuterNameImpl(context, methodName, context.DictionaryIndex);
            string name = outerName;
            fullName = name;
            return name;
        }
//        static internal string getNameImpl(BuildContext context, string methodName, out string fullName)
//        {
//            string name;
//            string outerName = getOuterNameImpl(context, methodName);

//            if (context.StructDepth > 0)
//            {
//#if !NEWIMPL
//                fullName = outerName; // Start the full name
                
//                // YEAH I AM STARTING TO DOUBT THIS SINCE IS INDEX ALWAYS A MATCH FOR DEPTH ?
//                // Can this distinguish NestedD2.NestedD3I2 from NestedD2I2.NestedD3I2 ?
//                // First build up the depth structs...
//                if (context.StructDepth > 1) // If at least one depth struct
//                {
//                    fullName += ".Nested";

//                    for (int structDepthIter = 2; structDepthIter < context.StructDepth; ++structDepthIter)
//                    {
//                        string nestedPredecessor = ".NestedD" + structDepthIter.ToString(); // Depth
//                        fullName += nestedPredecessor;

//                    } // Ends loop adding nested structs

//                } // Ends if at least one depth struct

//                name = "Nested" +
//                    (context.StructDepth == 1 ? "" : "D" + context.StructDepth.ToString()) + // Depth
//                    (context.StructIndex == 1 ? "" : "I" + context.StructIndex.ToString()); // Index
//                fullName += "." + name;
//                //string nameIter = outerName;
//                //int structDepthIter = 1;
//                //int structIndexIter = 1;
//                //// "(((i)(b))))"
//                //// Foo.Nested.NestedD2.NestedD2.NestedD2.Nested & 
//                //// Foo.Nested.NestedD2.NestedD2.NestedD2.NestedD2I2
//                //do
//                //{
//                //    do
//                //    {
//                //        // We're in a struct.
//                //        // First struct - no decoration.
//                //        // Structs nested in structs get D<N> appended, where <N> is depth.
//                //        // This is because a nested type can't contain a nested type with the same name which is simply awesome.
//                //        // Structs after the first struct at a given level get I<N> appended, where <N> is index.
//                //        name = "Nested" +
//                //            (structDepthIter == 1 ? "" : "D" + structDepthIter.ToString()) + // Depth
//                //            (structIndexIter == 1 ? "" : "I" + structIndexIter.ToString()); // Index
//                //        nameIter += "." + name;
//                //    } while (++structIndexIter <= context.StructIndex);
//                //} while (++structDepthIter <= context.StructDepth);

//                //fullName = nameIter;

//#else // !NEWIMPL
//                //// We're in a struct.
//                //// First struct - no decoration.
//                //// Structs nested in structs get D<N> appended, where <N> is depth.
//                //// This is because a nested type can't contain a nested type with the same name which is simply awesome.
//                //// Structs after the first struct at a given level get I<N> appended, where <N> is index.
//                name = "Nested" +
//                    (context.StructDepth == 1 ? "" : "D" + context.StructDepth.ToString()) + // Depth
//                    (context.StructIndex == 1 ? "" : "I" + context.StructIndex.ToString()); // Index
//                fullName = outerName + "." + name;
//#endif // NEWIMPL
//            }
//            else
//            {
//                // We're not in a struct. We're a param type. Make up something param-y.
//                name = outerName;
//                fullName = name;

//            }
//            return name;
//        }
    } // Ends class IDLArgumentTypeNameBuilderBase

    public class IDLArgumentTypeNameBuilderNoOp : IDLArgumentTypeNameBuilderBase
    {
        public override string getNames(BuildContext context, out string fullName)
        {
            fullName = string.Empty;
            return string.Empty;
        }

        public override string getScopedNames(BuildContext context, out string fullName)
        {
            fullName = string.Empty;
            return string.Empty;
        }

        public override string getScopedName(string name)
        {
            return name;
        }

        public override string getParamScopedName(string name)
        {
            return name;
        }

        public override string getDictionaryNames(BuildContext context, out string fullName)
        {
            fullName = string.Empty;
            return string.Empty;
        }

        public override string getDictionaryScopedNames(BuildContext context, out string fullName)
        {
            fullName = string.Empty;
            return string.Empty;
        }

        public override string getDictionaryScopedName(string name)
        {
            return name;
        }

        public override string getDictionaryParamScopedName(string name)
        {
            return name;
        }
    } // Ends class IDLArgumentTypeNameBuilderNoOp

    #endregion // Ends Name Builders

    /// <summary>
    /// Interface for building struct code.
    /// </summary>
    public interface IStructParamCodeTypeHandler
    {
        void StartStruct(string name, string fullName, BuildContext context);
        void AddField(CodeTypeReference codeTypeField);
        void EndStruct(BuildContext context, IParamCodeTypeHandler paramtypeHandler);
        BuildContext CreateNestedContext(BuildContext context);
        void CloseNestedContext(BuildContext contextNested);
        IParamCodeTypeHandler CreateFieldHandler();

    } // Ends interface IStructBuilder

    /// <summary>
    /// Interface for building dictionary code.
    /// </summary>
    public interface IDictParamCodeTypeHandler
    {
        IParamCodeTypeHandler CreateKeyHandler();
        IParamCodeTypeHandler CreateValueHandler();

        void StartDictionary(string name, BuildContext context);
        void FinishDictionary(BuildContext context, IParamCodeTypeHandler paramtypeHandler);
    }

    /// <summary>
    /// Handles method parameters.
    /// </summary>
    public interface IParamCodeTypeHandler
    {
        void HandleCodeParamType(ICodeParamType paramtype);
        #region Type Handling
        void HandleByte();
        void HandleBoolean();
        void HandleInt16();
        void HandleUInt16();
        void HandleInt32();
        void HandleUInt32();
        void HandleInt64();
        void HandleUInt64();
        void HandleDouble();
        void HandleString();
        void HandleObjectPath();
        void HandleSignature();
        void HandleStruct(ICodeParamType paramtype, string nameStruct, string nameScope);
        void HandleDictionary(ICodeParamType paramtype, string nameDictionary, string nameScope);
        void HandleVariant();
        #endregion // Ends Type Handling

        IStructParamCodeTypeHandler CreateStructHandler();
        IDictParamCodeTypeHandler CreateDictHandler();
        IParamCodeTypeHandler CreateNewArrayHandler();
        IParamCodeTypeHandler CreateArrayHandler();

    } // Ends interface IParamCodeTypeHandler

    #region Type Creation
    /// <summary>
    /// Base class for objects holding type info. Not strictly used at moment,
    /// and would need to refactor factory methods since at the moment rely on inheritance
    /// for implementation classes to get functionality they need.
    /// </summary>
    abstract public class ParamCodeTypeHolderBase : IParamCodeTypeHandler
    {
        public ICodeParamType paramtype;
        private readonly FieldDirection fieldDirection;

        #region Properties

        protected FieldDirection FieldDirection { get { return this.fieldDirection; } }

        #endregion // Properties

        public ParamCodeTypeHolderBase(FieldDirection fieldDirection)
        {
            this.fieldDirection = fieldDirection;
        }

        protected ParamCodeTypeHolderBase()
            : this(FieldDirection.In)
        {
        }

        public virtual void HandleCodeParamType(ICodeParamType paramtype)
        {
            this.paramtype = paramtype;
        }

        #region Type Handling
        public virtual void HandleByte()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(byte)));
        }
        public virtual void HandleBoolean()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(bool)));
        }

        public virtual void HandleInt16()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(Int16)));
        }

        public virtual void HandleUInt16()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(UInt16)));
        }

        public virtual void HandleInt32()
        {
            // Use string because using type Int32 => int which seems wrong.
            this.HandleCodeParamType(new CodeParamType(new CodeTypeReference("Int32")//typeof(Int32)
                ));
        }

        public virtual void HandleUInt32()
        {
            // Use string because using type UInt32 => uint which seems wrong.
            this.HandleCodeParamType(new CodeParamType(new CodeTypeReference("UInt32")//typeof(UInt32)
                ));
        }

        public virtual void HandleInt64()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(Int64)));
        }

        public virtual void HandleUInt64()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(UInt64)));
        }

        public virtual void HandleDouble()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(double)));
        }

        public virtual void HandleString()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(string)));
        }

        public virtual void HandleObjectPath()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(Udbus.Types.UdbusObjectPath)));
        }

        public virtual void HandleSignature()
        {
            this.HandleCodeParamType(new CodeParamType(typeof(IDLSignature)));
        }

        public virtual void HandleStruct(ICodeParamType paramtype, string nameStruct, string nameScope)
        {
            this.HandleCodeParamType(paramtype);
        }

        public virtual void HandleDictionary(ICodeParamType paramtype, string nameDictionary, string nameScope)
        {
            this.HandleCodeParamType(paramtype);
        }

        public virtual void HandleVariant()
        {
            this.HandleCodeParamType(new CodeParamType(new CodeTypeReference("Udbus.Containers.dbus_union")));
        }
        #endregion // Ends Type Handling

        public abstract IStructParamCodeTypeHandler CreateStructHandler();

        public abstract IDictParamCodeTypeHandler CreateDictHandler();

        public abstract IParamCodeTypeHandler CreateNewArrayHandler();

        public virtual IParamCodeTypeHandler CreateArrayHandler()
        {
            return this.CreateNewArrayHandler();
        }
    } // Ends class ParamCodeTypeHolderBase

    #endregion // Ends Type Creation

    /// <summary>
    /// Helper functions for building code.
    /// </summary>
    public class CodeBuilderHelper
    {
#if GOTSTRINGTYPE
        #region string IDLType
        static internal void BuildStruct(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, string IDLType, ref int index, BuildContext context)
        {
            // E.g. Foo(arg1 type="(s(i(u)i)(d)a(b)s)")
            // While (!")") recurse call
            //  "s" -> call => string
            //  "(" -> call => recurse
            //   "i" -> call => int
            //    "(" -> call => recurse
            //     "u" -> call => uint
            //    ")" -> Declare struct "Nested" containing types: u. Return struct expression.
            //   "i" -> call => int
            //  ")" -> Declare struct "Nested" containing types: int,<Nested>,int. Return struct expression.
            //  "(" -> call => recurse
            //    "d" -> call => double
            //  ")" -> Declare struct "NestedI2" containing types: double. Return struct expression.
            //  "a" -> recurse.
            //   "(" -> call => recurse
            //     "b" -> call => bool
            //   ")" -> Declare struct "NestedI3" containing types: bool. Return struct expression.
            //  Return array type of struct result.
            //  "s" -> call => string
            // ")" -> Declare struct "Nested" containing types: string,Nested1,Nested2,string. Return struct expression.
            IStructParamCodeTypeHandler structBuilder = paramtypeHandler.CreateStructHandler();
            string fullName;
            string structName = nameBuilder.getNames(context, out fullName);
            structBuilder.StartStruct(structName, fullName, context);

            while (IDLType[index] != ')')
            {
                if (index == IDLType.Length)
                {
                    throw IDLTypeException.Create("No \")\" marking end of struct found", IDLType, index);
                }

                // Prepare the context.
                BuildContext contextNested = structBuilder.CreateNestedContext(context);

                // Build the param.
                //ICodeParamType paramtypeStructMember;
                IParamCodeTypeHandler paramtypeStructMemberHandler = structBuilder.CreateFieldHandler();
                BuildCodeParamType(paramtypeStructMemberHandler, nameBuilder, IDLType, ref index, contextNested); // Increments index
                structBuilder.CloseNestedContext(contextNested);

            } // Ends looping to end of struct

            structBuilder.EndStruct(context, paramtypeHandler);
            ++index; // Move past the ')'
        }

        static private void BuildDictionary(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, string IDLType, ref int index, BuildContext context)
        {
            int indexKey = index;
            IDictParamCodeTypeHandler dictparamtypeHandler = paramtypeHandler.CreateDictHandler();
            string fullName;
            string dictName = nameBuilder.getNames(context, out fullName);
            dictparamtypeHandler.StartDictionary(dictName, context);
            IParamCodeTypeHandler codetypeHolderKey = dictparamtypeHandler.CreateKeyHandler();
            if (!BuildBasicCodeParamType(codetypeHolderKey, IDLType, ref indexKey, context))
            {
                throw IDLTypeException.CreateWithData(string.Format("Invalid dictionary key type: \"{0}\"", IDLType.Substring(index)), IDLType, index);
            }
            else
            {
                index = indexKey;
                IParamCodeTypeHandler codetypeHolderValue = dictparamtypeHandler.CreateValueHandler();

                BuildContext contextDict = new BuildContext(context.declarationHolder);
                BuildCodeParamType(codetypeHolderValue, nameBuilder, IDLType, ref indexKey, contextDict); // Increments index
                if (IDLType[indexKey] != '}') // If haven't got to end of dictionary
                {
                    throw IDLTypeException.CreateWithData(string.Format("Dictionary value type invalid. Expected single type, then \"}}\" : {0}", IDLType.Substring(index)),
                        IDLType, index);
                }
                else
                {
                    index = indexKey + 1;
                    dictparamtypeHandler.FinishDictionary(context, paramtypeHandler);
                }
            }
        }

        //static private void BuildDictionary(out CodeParamType paramtypeDictionary, IDLArgumentTypeNameBuilderBase nameBuilder, string IDLType, ref int index, BuildContext context)
        //{
        //    CodeParamType paramtypeKey = new CodeParamType();
        //    int indexKey = index;
        //    if (!BuildBasicCodeParamType(ref paramtypeKey, IDLType, ref indexKey, context))
        //    {
        //        throw IDLTypeException.CreateWithData(string.Format("Invalid dictionary key type: \"{0}\"", IDLType.Substring(index)), IDLType, index);
        //    }
        //    else
        //    {
        //        index = indexKey;
        //        ICodeParamType paramtypeDictValue;
        //        BuildContext contextDict = new BuildContext(context.declarationHolder, context.structBuilder);
        //        BuildCodeParamType(out paramtypeDictValue, nameBuilder, IDLType, ref indexKey, contextDict); // Increments index
        //        if (IDLType[indexKey] != '}') // If haven't got to end of dictionary
        //        {
        //            throw IDLTypeException.CreateWithData(string.Format("Dictionary value type invalid. Expected single type, then \"}}\" : {0}", IDLType.Substring(index)),
        //                IDLType, index);
        //        }
        //        else
        //        {
        //            index = indexKey;
        //            paramtypeDictionary = new CodeParamType(new CodeTypeReference(CodeBuilderCommon.DictionaryType.Name,
        //                new CodeTypeReference[]
        //                {
        //                    paramtypeKey.CodeType,
        //                    paramtypeDictValue.CodeType
        //                }
        //            ));
        //        }
        //    }
        //}

        static protected bool BuildBasicCodeParamType(IParamCodeTypeHandler paramtypeHandler, string IDLType, ref int index, BuildContext context)
        {
            bool bFound = true;
            switch (IDLType[index])
            {
                case 'y':
                    paramtypeHandler.HandleByte();
                    ++index;
                    break;
                case 'b':
                    paramtypeHandler.HandleBoolean();
                    ++index;
                    break;
                case 'n':
                    paramtypeHandler.HandleInt16();
                    ++index;
                    break;
                case 'q':
                    paramtypeHandler.HandleUInt16();
                    ++index;
                    break;
                case 'i':
                    paramtypeHandler.HandleInt32();
                    ++index;
                    break;
                case 'u':
                    paramtypeHandler.HandleUInt32();
                    ++index;
                    break;
                case 'x':
                    paramtypeHandler.HandleInt64();
                    ++index;
                    break;
                case 't':
                    paramtypeHandler.HandleUInt64();
                    ++index;
                    break;
                case 'd':
                    paramtypeHandler.HandleDouble();
                    ++index;
                    break;
                case 's':
                    paramtypeHandler.HandleString();
                    ++index;
                    break;
                default:
                    bFound = false;
                    break;
            }
            return bFound;
        }

        static protected void BuildCodeParamType(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, string IDLType, ref int index, BuildContext context)
        {
            if (BuildBasicCodeParamType(paramtypeHandler, IDLType, ref index, context))
            {
            }
            else
            {
                switch (IDLType[index])
                {
                    case 'o':
                        paramtypeHandler.HandleObjectPath();
                        ++index;
                        break;
                    case 'g':
                        paramtypeHandler.HandleSignature();
                        ++index;
                        break;
                    case 'a':
                    {
                        ++index;
                        if (index == IDLType.Length) // If reached end of type
                        {
                            throw new Exception("Array description truncated by end of type string");
                        }
                        else if (IDLType[index] == '{') // Else if dictionary
                        {
                            ++index;
                            BuildDictionary(paramtypeHandler, nameBuilder, IDLType, ref index, context);
                        }
                        else // Else not a dictionary
                        {
                            IParamCodeTypeHandler arrayHandler = paramtypeHandler.CreateArrayHandler();

                            BuildCodeParamType(arrayHandler, nameBuilder, IDLType, ref index, context);
                        } // Ends else not a dictionary
                        break;
                    }
                    case 'r':
                        throw IDLTypeException.Create("Type \"r\" is not supposed to appear in DBUS signatures. See http://dbus.freedesktop.org/doc/dbus-specification.html#message-protocol-signatures",
                            IDLType, index);
                    case '(':
                        ++index;
                        BuildStruct(paramtypeHandler, nameBuilder, IDLType, ref index, context);
                        break;
                    case ')':
                        throw IDLTypeException.CreateWithData("Unmatched \")\" character. Where did this struct begin ?", IDLType, index);
                    case 'e':
                        throw IDLTypeException.CreateWithData("Type \"e\" is not supposed to appear in DBUS signatures. . See http://dbus.freedesktop.org/doc/dbus-specification.html#message-protocol-signatures",
                            IDLType, index);
                    case '{':
                        throw IDLTypeException.CreateWithData("\"{\" not preceded by array character \"a\"", IDLType, index);
                    case '}':
                        throw IDLTypeException.CreateWithData("Unmatched \"}\" character. Where did this dictionary begin ?", IDLType, index);
                    default:
                        throw IDLTypeException.CreateWithData("Unknown IDL type", IDLType, index);
                } // Ends switch first character
            }
        }

        internal static void BuildCodeParamType(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, string IDLType, BuildContext context)
        {
            int index = 0;

            BuildCodeParamType(paramtypeHandler, nameBuilder, IDLType, ref index, context);
        }

        #endregion // string IDLType
#endif // GOTSTRINGTYPE

#if !GotType
        internal static Udbus.Types.dbus_type TypeFromChar(char IDLType)
        {
            Udbus.Types.dbus_type type;

            switch (IDLType)
            {
                case 'y':
                    type = Udbus.Types.dbus_type.DBUS_BYTE;
                    break;
                case 'b':
                    type = Udbus.Types.dbus_type.DBUS_BOOLEAN;
                    break;
                case 'n':
                    type = Udbus.Types.dbus_type.DBUS_INT16;
                    break;
                case 'q':
                    type = Udbus.Types.dbus_type.DBUS_UINT16;
                    break;
                case 'i':
                    type = Udbus.Types.dbus_type.DBUS_INT32;
                    break;
                case 'u':
                    type = Udbus.Types.dbus_type.DBUS_UINT32;
                    break;
                case 'x':
                    type = Udbus.Types.dbus_type.DBUS_INT64;
                    break;
                case 't':
                    type = Udbus.Types.dbus_type.DBUS_UINT64;
                    break;
                case 'd':
                    type = Udbus.Types.dbus_type.DBUS_DOUBLE;
                    break;
                case 's':
                    type = Udbus.Types.dbus_type.DBUS_STRING;
                    break;
                case 'o':
                    type = Udbus.Types.dbus_type.DBUS_OBJECTPATH;
                    break;
                case 'g':
                    type = Udbus.Types.dbus_type.DBUS_SIGNATURE;
                    break;
                case 'a':
                    type = Udbus.Types.dbus_type.DBUS_ARRAY;
                    break;
                case '{':
                    type = Udbus.Types.dbus_type.DBUS_DICT_BEGIN;
                    break;
                case '}':
                    type = Udbus.Types.dbus_type.DBUS_DICT_END;
                    break;
                case '(':
                    type = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN;
                    break;
                case ')':
                    type = Udbus.Types.dbus_type.DBUS_STRUCT_END;
                    break;
                case 'v':
                    type = Udbus.Types.dbus_type.DBUS_VARIANT;
                    break;
                case 'r':
                    throw IDLTypeException.Create("Type \"r\" is not supposed to appear in DBUS signatures. See http://dbus.freedesktop.org/doc/dbus-specification.html#message-protocol-signatures",
                        IDLType.ToString(), 0);
                case 'e':
                    throw IDLTypeException.CreateWithData("Type \"e\" is not supposed to appear in DBUS signatures. See http://dbus.freedesktop.org/doc/dbus-specification.html#message-protocol-signatures",
                        IDLType.ToString(), 0);
                default:
                    throw IDLTypeException.CreateWithData("Unknown IDL type", IDLType.ToString(), 0);
            } // Ends switch (IDLType)

            return type;
        }

        internal static char CharFromType(Udbus.Types.dbus_type IDLType)
        {
            char type;

            switch (IDLType)
            {
                case Udbus.Types.dbus_type.DBUS_BYTE:
                    type = 'y';
                    break;
                case Udbus.Types.dbus_type.DBUS_BOOLEAN:
                    type = 'b';
                    break;
                case Udbus.Types.dbus_type.DBUS_INT16:
                    type = 'n';
                    break;
                case Udbus.Types.dbus_type.DBUS_UINT16:
                    type = 'q';
                    break;
                case Udbus.Types.dbus_type.DBUS_INT32:
                    type = 'i';
                    break;
                case Udbus.Types.dbus_type.DBUS_UINT32:
                    type = 'u';
                    break;
                case Udbus.Types.dbus_type.DBUS_INT64:
                    type = 'x';
                    break;
                case Udbus.Types.dbus_type.DBUS_UINT64:
                    type = 't';
                    break;
                case Udbus.Types.dbus_type.DBUS_DOUBLE:
                    type = 'd';
                    break;
                case Udbus.Types.dbus_type.DBUS_STRING:
                    type = 's';
                    break;
                case Udbus.Types.dbus_type.DBUS_OBJECTPATH:
                    type = 'o';
                    break;
                case Udbus.Types.dbus_type.DBUS_SIGNATURE:
                    type = 'g';
                    break;
                case Udbus.Types.dbus_type.DBUS_ARRAY:
                    type = 'a';
                    break;
                case Udbus.Types.dbus_type.DBUS_DICT_BEGIN:
                    type = '{';
                    break;
                case Udbus.Types.dbus_type.DBUS_DICT_END:
                    type = '}';
                    break;
                case Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN:
                    type = '(';
                    break;
                case Udbus.Types.dbus_type.DBUS_STRUCT_END:
                    type = ')';
                    break;
                case Udbus.Types.dbus_type.DBUS_VARIANT:
                    type = 'v';
                    break;
                default:
                    throw IDLTypeException.CreateWithData("Unknown IDL type", IDLType.ToString(), 0);
            } // Ends switch (IDLType)

            return type;
        }

        IEnumerable<Udbus.Types.dbus_type> DbusTypeIterator(string IDLTypes)
        {
            foreach (char IDLType in IDLTypes)
            {
                yield return TypeFromChar(IDLType);
            }
        }

        #region // dbus_type IDLType
        //using Udbus.Core;
        static public string ToString(IList<Udbus.Types.dbus_type> types)
        {
            StringBuilder result = new StringBuilder();
            if (types.Count == 0 || types[0] == Udbus.Types.dbus_type.DBUS_INVALID) // If got no entries
            {
                result.Append("Length=0");

            } // Ends if got no entries
            else // Else got entries
            {
                StringBuilder content = new StringBuilder();
                content.Append(types[0].ToString());
                int counter = 1;
                if (types.Count > 1 && types[1] != Udbus.Types.dbus_type.DBUS_INVALID) // If more than one entry
                {
                    for (; counter < types.Count && types[counter] != Udbus.Types.dbus_type.DBUS_INVALID; ++counter)
                    {
                        content.AppendFormat(", {0}", types[counter].ToString());

                    }
                } // Ends if more than one entry
                result.AppendFormat("Length={0} [{1}]", counter, content.ToString());
            } // Ends else got entries
            return result.ToString();
        }


        static internal void BuildStruct(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, IList<Udbus.Types.dbus_type> IDLType, ref int index, BuildContext context)
        {
            // E.g. Foo(arg1 type="(s(i(u)i)(d)a(b)s)")
            // While (!")") recurse call
            //  "s" -> call => string
            //  "(" -> call => recurse
            //   "i" -> call => int
            //    "(" -> call => recurse
            //     "u" -> call => uint
            //    ")" -> Declare struct "Nested" containing types: u. Return struct expression.
            //   "i" -> call => int
            //  ")" -> Declare struct "Nested" containing types: int,<Nested>,int. Return struct expression.
            //  "(" -> call => recurse
            //    "d" -> call => double
            //  ")" -> Declare struct "NestedI2" containing types: double. Return struct expression.
            //  "a" -> recurse.
            //   "(" -> call => recurse
            //     "b" -> call => bool
            //   ")" -> Declare struct "NestedI3" containing types: bool. Return struct expression.
            //  Return array type of struct result.
            //  "s" -> call => string
            // ")" -> Declare struct "Nested" containing types: string,Nested1,Nested2,string. Return struct expression.
            IStructParamCodeTypeHandler structBuilder = paramtypeHandler.CreateStructHandler();
            string fullName;
            string structName = nameBuilder.getNames(context, out fullName);
            fullName = nameBuilder.getParamScopedName(fullName);
            structBuilder.StartStruct(structName, fullName, context);

            while (IDLType[index] != Udbus.Types.dbus_type.DBUS_STRUCT_END)
            {
                if (index == IDLType.Count)
                {
                    throw IDLTypeException.Create("No \")\" marking end of struct found", ToString(IDLType), index);
                }

                // Prepare the context.
                BuildContext contextNested = structBuilder.CreateNestedContext(context);

                // Build the param.
                //ICodeParamType paramtypeStructMember;
                IParamCodeTypeHandler paramtypeStructMemberHandler = structBuilder.CreateFieldHandler();
                BuildCodeParamType(paramtypeStructMemberHandler, nameBuilder, IDLType, ref index, contextNested); // Increments index
                structBuilder.CloseNestedContext(contextNested);

            } // Ends looping to end of struct

            structBuilder.EndStruct(context, paramtypeHandler);
            ++index; // Move past the ')'
        }

        static private void BuildDictionary(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, IList<Udbus.Types.dbus_type> IDLType, ref int index, BuildContext context)
        {
            int indexKey = index;
            IDictParamCodeTypeHandler dictparamtypeHandler = paramtypeHandler.CreateDictHandler();
            string fullName;
            string dictName = nameBuilder.getDictionaryNames(context, out fullName);
            dictparamtypeHandler.StartDictionary(dictName, context);
            IParamCodeTypeHandler codetypeHolderKey = dictparamtypeHandler.CreateKeyHandler();
            if (!BuildBasicCodeParamType(codetypeHolderKey, IDLType, ref indexKey))
            {
                string IDLTypeString = ToString(IDLType);
                throw IDLTypeException.CreateWithData(string.Format("Invalid dictionary key type: \"{0}\"", IDLTypeString.Substring(index)), IDLTypeString, index);
            }
            else
            {
                index = indexKey;
                IParamCodeTypeHandler codetypeHolderValue = dictparamtypeHandler.CreateValueHandler();

                BuildContext contextDict = new BuildContext(context.declarationHolder);
                BuildCodeParamType(codetypeHolderValue, nameBuilder, IDLType, ref indexKey, contextDict); // Increments index
                if (IDLType[indexKey] != Udbus.Types.dbus_type.DBUS_DICT_END) // If haven't got to end of dictionary
                {
                    string IDLTypeString = ToString(IDLType);
                    throw IDLTypeException.CreateWithData(string.Format("Dictionary value type invalid. Expected single type, then \"}}\" : {0}", IDLTypeString.Substring(index)),
                        IDLTypeString, index);
                }
                else
                {
                    index = indexKey + 1;
                    dictparamtypeHandler.FinishDictionary(context, paramtypeHandler);
                }
            }
        }

        /// <summary>
        /// Appears to be used to handle basic parameters, ints and so on...
        /// </summary>
        /// <param name="paramtypeHandler"></param>
        /// <param name="IDLType"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static protected bool BuildBasicCodeParamType(IParamCodeTypeHandler paramtypeHandler, IList<Udbus.Types.dbus_type> IDLType, ref int index)
        {
            bool bFound = true;
            switch (IDLType[index])
            {
                case Udbus.Types.dbus_type.DBUS_BYTE:
                    paramtypeHandler.HandleByte();
                    break;
                case Udbus.Types.dbus_type.DBUS_BOOLEAN:
                    paramtypeHandler.HandleBoolean();
                    break;
                case Udbus.Types.dbus_type.DBUS_INT16:
                    paramtypeHandler.HandleInt16();
                    break;
                case Udbus.Types.dbus_type.DBUS_UINT16:
                    paramtypeHandler.HandleUInt16();
                    break;
                case Udbus.Types.dbus_type.DBUS_INT32:
                    paramtypeHandler.HandleInt32();
                    break;
                case Udbus.Types.dbus_type.DBUS_UINT32:
                    paramtypeHandler.HandleUInt32();
                    break;
                case Udbus.Types.dbus_type.DBUS_INT64:
                    paramtypeHandler.HandleInt64();
                    break;
                case Udbus.Types.dbus_type.DBUS_UINT64:
                    paramtypeHandler.HandleUInt64();
                    break;
                case Udbus.Types.dbus_type.DBUS_DOUBLE:
                    paramtypeHandler.HandleDouble();
                    break;
                case Udbus.Types.dbus_type.DBUS_STRING:
                    paramtypeHandler.HandleString();
                    break;
                default:
                    bFound = false;
                    break;
            }

            //If param handled, increment index
            if (bFound) index++;

            return bFound;
        }

        /// <summary>
        /// Simplest way to create a parameter
        /// </summary>
        /// <param name="paramtypeHandler"></param>
        /// <param name="nameBuilder"></param>
        /// <param name="IDLType"></param>
        /// <param name="context"></param>
        public static void BuildCodeParamType(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, string IDLType, BuildContext context)
        {
            DbusTypeCollection dbus_types = new DbusTypeCollection(IDLType);
            BuildCodeParamType(paramtypeHandler, nameBuilder, dbus_types, context);
        }

        /// <summary>
        /// Build a code parameter while providing your own type list
        /// </summary>
        /// <param name="paramtypeHandler"></param>
        /// <param name="nameBuilder"></param>
        /// <param name="IDLType"></param>
        /// <param name="context"></param>
        public static void BuildCodeParamType(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, IList<Udbus.Types.dbus_type> IDLType, BuildContext context)
        {
            int index = 0;
            BuildCodeParamType(paramtypeHandler, nameBuilder, IDLType, ref index, context);
        }

        /// <summary>
        /// Build a code parameter
        /// </summary>
        /// <param name="paramtypeHandler">Holds onto declarations, and acts as factory for complex types?</param>
        /// <param name="nameBuilder">Factory for names?</param>
        /// <param name="IDLType">List of DBus types</param>
        /// <param name="index">Element within IDLType to build</param>
        /// <param name="context">???</param>
        static void BuildCodeParamType(IParamCodeTypeHandler paramtypeHandler, IDLArgumentTypeNameBuilderBase nameBuilder, IList<Udbus.Types.dbus_type> IDLType, ref int index, BuildContext context)
        {
            if (!BuildBasicCodeParamType(paramtypeHandler, IDLType, ref index))
            {
                switch (IDLType[index])
                {
                    case Udbus.Types.dbus_type.DBUS_OBJECTPATH:
                        paramtypeHandler.HandleObjectPath();
                        ++index;
                        break;
                    case Udbus.Types.dbus_type.DBUS_SIGNATURE:
                        paramtypeHandler.HandleSignature();
                        ++index;
                        break;
                    case Udbus.Types.dbus_type.DBUS_ARRAY:
                    {
                        ++index;
                        if (index == IDLType.Count) // If reached end of type
                        {
                            throw new Exception("Array description truncated by end of type string");
                        }
                        else if (IDLType[index] == Udbus.Types.dbus_type.DBUS_DICT_BEGIN) // Else if dictionary
                        {
                            ++index;
                            BuildDictionary(paramtypeHandler, nameBuilder, IDLType, ref index, context);
                        }
                        else // Else not a dictionary
                        {
                            IParamCodeTypeHandler arrayHandler = paramtypeHandler.CreateArrayHandler();
                            BuildCodeParamType(arrayHandler, nameBuilder, IDLType, ref index, context);
                        } // Ends else not a dictionary
                        break;
                    }
                    case Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN:
                        ++index;
                        BuildStruct(paramtypeHandler, nameBuilder, IDLType, ref index, context);
                        break;
                    case Udbus.Types.dbus_type.DBUS_VARIANT:
                        paramtypeHandler.HandleVariant();
                        ++index;
                        break;
                    case Udbus.Types.dbus_type.DBUS_STRUCT_END:
                        throw IDLTypeException.CreateWithData("Unmatched \")\" character. Where did this struct begin ?", ToString(IDLType), index);
                    case Udbus.Types.dbus_type.DBUS_DICT_END:
                        throw IDLTypeException.CreateWithData("Unmatched \"}\" character. Where did this dictionary begin ?", ToString(IDLType), index);
                    case Udbus.Types.dbus_type.DBUS_INVALID:
                        // Signifies the last entry
                        break;
                    default:
                        throw IDLTypeException.CreateWithData("Unknown IDL type", ToString(IDLType), index);
                } // Ends switch first character
            }
        }

        #endregion // dbus_type IDLType
#endif //GotType

    } // Ends CodeBuilderHelper

    /// <summary>
    /// Converts a string dbus signature into a dbus_type collection.
    /// </summary>
    class DbusTypeCollection : IList<Udbus.Types.dbus_type>
    {
        #region Fields
        private IList<char> types;
        #endregion // Fields

        #region Constructors
        public DbusTypeCollection(string types)
        {
            this.types = types.ToList();
        }
        #endregion // Constructors

        #region IList<dbus_type> Members

        public int IndexOf(Udbus.Types.dbus_type item)
        {
            return this.types.IndexOf(CodeBuilderHelper.CharFromType(item));
        }

        public void Insert(int index, Udbus.Types.dbus_type item)
        {
            char ch = CodeBuilderHelper.CharFromType(item);
            this.types.Insert(index, CodeBuilderHelper.CharFromType(item));
        }

        public void RemoveAt(int index)
        {
            this.types.RemoveAt(index);
        }

        public Udbus.Types.dbus_type this[int index]
        {
            get
            {
                return CodeBuilderHelper.TypeFromChar(this.types[index]);
            }
            set
            {
                this.types[index] = CodeBuilderHelper.CharFromType(value);
            }
        }

        #endregion // Ends IList<dbus_type> Members

        #region ICollection<dbus_type> Members

        public void Add(Udbus.Types.dbus_type item)
        {
            this.types.Add(CodeBuilderHelper.CharFromType(item));
        }

        public void Clear()
        {
            this.types.Clear();
        }

        public bool Contains(Udbus.Types.dbus_type item)
        {
            return this.types.Contains(CodeBuilderHelper.CharFromType(item));
        }

        public void CopyTo(Udbus.Types.dbus_type[] array, int arrayIndex)
        {
            for (int iter = arrayIndex; iter < array.Length; ++iter)
            {
                array[iter] = CodeBuilderHelper.TypeFromChar(this.types[iter - arrayIndex]);
            }
        }

        public int Count
        {
            get { return this.types.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.types.IsReadOnly; }
        }

        public bool Remove(Udbus.Types.dbus_type item)
        {
            return this.types.Remove(CodeBuilderHelper.CharFromType(item));
        }

        #endregion // Ends ICollection<dbus_type> Members

        #region IEnumerable<dbus_type> Members

        public IEnumerator<Udbus.Types.dbus_type> GetEnumerator()
        {
            foreach (char ch in this.types)
            {
                yield return CodeBuilderHelper.TypeFromChar(ch);
            }
        }

        #endregion // Ends IEnumerable<dbus_type> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (char ch in this.types)
            {
                yield return CodeBuilderHelper.TypeFromChar(ch);
            }
        }

        #endregion // Ends IEnumerable Members
    } // Ends class DbusTypeCollection
} // Ends namespace Udbus.Parsing
