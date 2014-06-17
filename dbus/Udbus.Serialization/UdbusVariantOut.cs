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

namespace Udbus.Serialization.Variant
{
    interface IBuildWriteVariantDelegate
    {
        MarshalDelegate<Udbus.Containers.dbus_union> VariantDelegate { get; }

    } // Ends interface IBuildWriteVariantDelegate

    #region Struct Handling
    internal class StructCreatorOut : Udbus.Parsing.StructCreatorBase, IBuildWriteVariantDelegate
    {
        #region StructCreatorOut Internal classes
        public class FieldHandler : UdbusVariantOut
        {
            private StructCreatorOut owner;

            internal FieldHandler(StructCreatorOut owner)
                : base()
            {
                this.owner = owner;
            }

            #region dbus_type handling
            public override void AddType(Udbus.Types.dbus_type type)
            {
                base.AddType(type);
                this.owner.AddType(type);
            }

            public override void AddTypeRange(Udbus.Types.dbus_type[] types)
            {
                base.AddTypeRange(types);
                this.owner.AddTypeRange(types);
            }
            #endregion // dbus_type handling

            #region IParamCodeTypeHandler overrides
            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                owner.HandleField(this);
            }
            #endregion // IParamCodeTypeHandler overrides
        } // Ends class FieldHandler

        #endregion // StructCreatorOut Internal classes

        #region Fields
        private MarshalDelegate<Udbus.Containers.dbus_union> variantDelegate;
        private MarshalDelegate<object> objectDelegate;
        #endregion // Fields

        #region Properties
        public MarshalDelegate<Udbus.Containers.dbus_union> VariantDelegate
        {
            get { return this.WriteStruct; }
        }
        public MarshalDelegate<object> ObjectDelegate
        {
            get { return this.WriteStructObject; }
        }
        #endregion // Properties

        UdbusVariantOut parent;

        #region Constructors
        public StructCreatorOut(UdbusVariantOut parent, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base (owner)
        {
            this.parent = parent;
        }
        #endregion Constructors

        #region IStructParamCodeTypeHandler members
        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateNew()
        {
            return new StructCreatorOut(this.parent, this.owner);
        }

        public override void EndStruct(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.EndStruct(context, paramtypeHandler);
            this.parent.VariantDelegate += this.VariantDelegate;
            this.parent.ObjectDelegate += this.ObjectDelegate;
            this.parent.HandleStruct(null, null, null);
        }
        #endregion // IStructParamCodeTypeHandler

        public void HandleField(FieldHandler fieldHandler)
        {
            this.variantDelegate += fieldHandler.VariantDelegate;
            this.objectDelegate += fieldHandler.ObjectDelegate;
        }

        internal void AddType(Udbus.Types.dbus_type type)
        {
            // Tell the outside object about the type field.
            this.parent.AddType(type);
        }

        internal void AddTypeRange(Udbus.Types.dbus_type[] types)
        {
            // Tell the outside object about the type fields.
            this.parent.AddTypeRange(types);
        }

        private int WriteStruct(Udbus.Serialization.UdbusMessageBuilder builder, object[] structFields)
        {
            int result = 0;

            if (this.objectDelegate == null) // If no fields
            {
                if (structFields != null && structFields.Length > 0) // If there are fields to write
                {
                    result = -1;
                    throw new ApplicationException("Struct has no delegates to write struct fields with");

                } // Ends if there are fields to write

            } // Ends if no fields
            else // Else fields
            {
                result = builder.BodyAdd_Structure().Result;

                if (result == 0) // If started struct
                {
                    // Yay casting something to what we already know it is because, yeah, that's how this works...
                    System.Delegate[] delegates = this.objectDelegate.GetInvocationList();
                    int i = 0;

                    foreach (MarshalDelegate<object> fn in delegates)
                    {
                        result = fn(builder, structFields[i]);

                        if (result != 0) // If failed to read result
                        {
                            break;

                        } // Ends if failed to read result

                        ++i;

                    } // Ends loop over delegates

                    if (result == 0) // If read fields
                    {
                        result = builder.BodyAdd_StructureEnd().Result;

                    } // Ends if read read fields
                    else // Else failed to read fields
                    {

                    } // Ends if failed to read fields
                } // Ends if started struct
                else // Else failed to start struct
                {

                } // Ends else failed to start struct

            } // Ends else fields

            return result;
        }

        private int WriteStruct(Udbus.Serialization.UdbusMessageBuilder builder, Udbus.Containers.dbus_union variantStruct)
        {
            return this.WriteStruct(builder, variantStruct.DbusObjectStruct);
        }

        // The object returned from a struct read is always a variant...
        private int WriteStructObject(Udbus.Serialization.UdbusMessageBuilder builder, object objStruct)
        {
            int result;

            if (objStruct is Udbus.Containers.dbus_union) // If it's a struct
            {
                Udbus.Containers.dbus_union variantStruct = (Udbus.Containers.dbus_union)objStruct;
                result = this.WriteStruct(builder, variantStruct);

            } // Ends if it's a struct
            else if (objStruct is object[]) // Else if it's an array
            {
                object[] structFields = (object[])objStruct;
                result = this.WriteStruct(builder, structFields);

            } // Ends if it's an array
            else // Else wrong type
            {
                result = -1;
                throw new System.InvalidCastException("object is not a dbus_union or object[]");

            } // Ends else wrong type

            return result;
        }

        #region StructCreatorBase overrides
        public override void AddField(CodeTypeReference codeTypeField)
        {
            // No op
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateFieldHandler()
        {
            return new FieldHandler(this);
        }
        #endregion // StructCreatorBase overrides

    } // Ends class StructCreatorOut

    #endregion // Struct Handling

    #region Dictionary Handling


    internal class DictCreatorOut : DictCreatorBase, IBuildWriteVariantDelegate
    {
        #region Handlers

        internal class KeyHandler : UdbusPrimitiveOut
        {
            #region Fields
            DictCreatorOut owner;
            #endregion // Fields

            #region Constructors
            internal KeyHandler(DictCreatorOut owner)
                : base()
            {
                this.owner = owner;
            }
            #endregion // Constructors

            #region IParamCodeTypeHandler overrides
            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleKey(this, paramtype);
            }
            #endregion // IParamCodeTypeHandler overrides

            #region dbus_type handling
            protected override void AddType(Udbus.Types.dbus_type type)
            {
                base.AddType(type);
                this.owner.AddType(type);
            }
            #endregion // dbus_type handling

        } // Ends class KeyHandler

        internal class ValueHandler : UdbusVariantOut
        {
            #region Fields
            DictCreatorOut owner;
            #endregion // Fields

            #region Constructors
            internal ValueHandler(DictCreatorOut owner)
                : base()
            {
                this.owner = owner;
            }
            #endregion // Constructors

            #region IParamCodeTypeHandler overrides
            public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
            {
                base.HandleCodeParamType(paramtype);
                this.owner.HandleValue(this, paramtype);
            }
            #endregion // IParamCodeTypeHandler overrides

            #region dbus_type handling
            public override void AddType(Udbus.Types.dbus_type type)
            {
                base.AddType(type);
                this.owner.AddType(type);
            }

            public override void AddTypeRange(Udbus.Types.dbus_type[] types)
            {
                base.AddTypeRange(types);
                this.owner.AddTypeRange(types);
            }
            #endregion // dbus_type handling
        } // Ends class ValueHandler

        #endregion // Handlers

        #region  Fields

        private UdbusVariantOut owner;
        protected MarshalDelegate<object>ObjectDelegateKey;
        protected MarshalDelegate<object> ObjectDelegateValue;

        #endregion  //Fields

        public MarshalDelegate<Udbus.Containers.dbus_union> VariantDelegate { get { return this.WriteDictionary; } }
        public MarshalDelegate<object> ObjectDelegate { get { return this.WriteDictionaryObject; } }

        #region Constructors
        internal DictCreatorOut(UdbusVariantOut owner)
        {
            this.owner = owner;
        }
        #endregion // Constructors

        private int WriteDictionary(Udbus.Serialization.UdbusMessageBuilder builder, Udbus.Containers.dbus_union variantDict)
        {
            int result;
            if (variantDict.IsDbusDictionary == false) // If not a dictionary
            {
                result = -1;
                throw new System.InvalidCastException("object is a dbus_union, but not a dbus dictionary");

            } // Ends if not a dictionary
            else // Else a dictionary
            {
                result = builder.MarshalDict(variantDict.DbusDictionary, this.ObjectDelegateKey, this.ObjectDelegateValue);

            } // Ends else a dictionary

            return result;
        }

        private int WriteDictionaryObject(Udbus.Serialization.UdbusMessageBuilder builder, object objDict)
        {
            int result;
            if (objDict is Udbus.Containers.dbus_union == false)
            {
                result = -1;
                throw new System.InvalidCastException("object is not a dbus_union");
            }
            else
            {
                Udbus.Containers.dbus_union variantDict = (Udbus.Containers.dbus_union)objDict;

                result = this.WriteDictionary(builder, variantDict);
            }
            return result;
        }

        #region dbus_type handling
        internal void AddType(Udbus.Types.dbus_type type)
        {
            // Tell the outside object about the type field.
            this.owner.AddType(type);
        }

        internal void AddTypeRange(Udbus.Types.dbus_type[] types)
        {
            this.owner.AddTypeRange(types);
        }
        #endregion // dbus_type handling

        #region IDictParamCodeTypeHandler Members

        public override Udbus.Parsing.IParamCodeTypeHandler CreateKeyHandler()
        {
            return new KeyHandler(this);//this.variantKey, this.Builder, this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateValueHandler()
        {
            return new ValueHandler(this);
        }
        #endregion IDictParamCodeTypeHandler Members

        internal void HandleKey(KeyHandler handler, Udbus.Parsing.ICodeParamType paramtype)
        {
            this.ObjectDelegateKey += handler.ObjectDelegate;
        }

        internal void HandleValue(ValueHandler handler, Udbus.Parsing.ICodeParamType paramtype)
        {
            this.ObjectDelegateValue += handler.ObjectDelegate;
        }

        public override void StartDictionary(string name, Udbus.Parsing.BuildContext context)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Starting dictionary {0}", name));
        }

        public override void FinishDictionary(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            System.Diagnostics.Debug.WriteLine("Finishing dictionary");
            owner.VariantDelegate += this.VariantDelegate;
            owner.ObjectDelegate += this.ObjectDelegate;
            this.owner.HandleDictionary(null, null, null);
        }
    } // Ends class DictCreatorOut

    #endregion // Dictionary Handling

    internal class UdbusPrimitiveOut : Udbus.Parsing.ParamCodeTypeHolderBase
    {
        #region Fields
        private MarshalDelegate<object> objectDelegate;
        private Udbus.Types.dbus_type type = Udbus.Types.dbus_type.DBUS_INVALID;
        #endregion // Fields

        #region Properties
        public MarshalDelegate<object> ObjectDelegate { get { return this.objectDelegate; } }
        public Udbus.Types.dbus_type Type { get { return this.type; } }
        #endregion // Properties

        #region dbus_type handling
        protected virtual void AddType(Udbus.Types.dbus_type type)
        {
            this.type = type;
        }
        #endregion // dbus_type handling

        #region ParamCodeTypeHolderBase functions
        #region Type Handling
        public override void HandleByte()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_ByteObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BYTE);
            base.HandleByte();
        }

        public override void HandleBoolean()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_BooleanObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BOOLEAN);
            base.HandleBoolean();
        }

        public override void HandleInt16()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_Int16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT16);
            base.HandleInt16();
        }

        public override void HandleUInt16()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_UInt16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT16);
            base.HandleUInt16();
        }

        public override void HandleInt32()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_Int32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT32);
            base.HandleInt32();
        }

        public override void HandleUInt32()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_UInt32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT32);
            base.HandleUInt32();
        }

        public override void HandleInt64()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_Int64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT64);
            base.HandleInt64();
        }

        public override void HandleUInt64()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_UInt64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT64);
            base.HandleUInt64();
        }

        public override void HandleDouble()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_DoubleObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_DOUBLE);
            base.HandleDouble();
        }

        public override void HandleString()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_StringObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_STRING);
            base.HandleString();
        }

        public override void HandleObjectPath()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_ObjectPathObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_OBJECTPATH);
            base.HandleObjectPath();
        }

        public override void HandleSignature()
        {
            base.HandleSignature();
            throw Exceptions.InvalidIDLTypeException.CreateFromTypeName("Signature");
        }

        public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
        {
            base.HandleStruct(paramtype, nameStruct, nameScope);
            // Ummm all done already ?
        }

        public override void HandleDictionary(Udbus.Parsing.ICodeParamType paramtype, string nameDictionary, string nameScope)
        {
            base.HandleDictionary(paramtype, nameDictionary, nameScope);
            // Ummm all done already ?
        }

        public override void HandleVariant()
        {
            this.objectDelegate = UdbusMessageBuilder.BodyAdd_VariantObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_VARIANT);
            base.HandleVariant();
        }

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
        {
            throw Exceptions.InvalidIDLTypeException.CreateFromTypeName("Struct");
        }

        public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
        {
            throw Exceptions.InvalidIDLTypeException.CreateFromTypeName("Dictionary");
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            throw Exceptions.InvalidIDLTypeException.CreateFromTypeName("Array");
        }
        #endregion // Ends Type Handling
        #endregion // ParamCodeTypeHolderBase functions
    } // Ends class UdbusPrimitiveOut

    public class UdbusVariantOut : UdbusVariantBase
        , IDbusTypeHolder
    {
        #region Fields
        List<Udbus.Types.dbus_type> types = new List<Udbus.Types.dbus_type>(new Udbus.Types.dbus_type[] { Udbus.Types.dbus_type.DBUS_INVALID });
        #endregion // Fields

        #region Properties
        MarshalDelegate<Udbus.Containers.dbus_union> variantDelegate;
        public MarshalDelegate<Udbus.Containers.dbus_union> VariantDelegate
        {
            get { return this.variantDelegate; }
            set { this.variantDelegate = value; }
        }

        MarshalDelegate<object> objectDelegate;
        public MarshalDelegate<object> ObjectDelegate
        {
            get { return this.objectDelegate; }
            set { this.objectDelegate = value; }
        }
        public Udbus.Types.dbus_type Type { get { return this.types[0]; } }
        public List<Udbus.Types.dbus_type> Types { get { return this.types; } }
        #endregion // Properties

        #region dbus_type handling
        public virtual void AddType(Udbus.Types.dbus_type type)
        {
            // Add and terminate with DBUS_INVALID.
            this.types[this.types.Count - 1] = type;
            this.types.Add(Udbus.Types.dbus_type.DBUS_INVALID);
        }

        public virtual void AddTypeRange(Udbus.Types.dbus_type[] types)
        {
            // Pop DBUS_INVALID.
            this.types.RemoveAt(this.types.Count - 1);
            // Add and terminate with DBUS_INVALID.
            this.types.AddRange(types);
            this.types.Add(Udbus.Types.dbus_type.DBUS_INVALID);
        }
        #endregion // dbus_type handling

        #region Constructors
        public UdbusVariantOut()
            : base(FieldDirection.In)
        {
        }
        #endregion // Constructors

        #region ParamCodeTypeHolderBase functions
        #region Type Handling
        public override void HandleByte()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantByte;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_ByteObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BYTE);
            base.HandleByte();
        }

        public override void HandleBoolean()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantBoolean;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_BooleanObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BOOLEAN);
            base.HandleBoolean();
        }

        public override void HandleInt16()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantInt16;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_Int16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT16);
            base.HandleInt16();
        }

        public override void HandleUInt16()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantUInt16;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_UInt16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT16);
            base.HandleUInt16();
        }

        public override void HandleInt32()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantInt32;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_Int32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT32);
            base.HandleInt32();
        }

        public override void HandleUInt32()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantUInt32;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_UInt32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT32);
            base.HandleUInt32();
        }

        public override void HandleInt64()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantInt64;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_Int64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT64);
            base.HandleInt64();
        }

        public override void HandleUInt64()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantUInt64;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_UInt64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT64);
            base.HandleUInt64();
        }

        public override void HandleDouble()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantDouble;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_DoubleObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_DOUBLE);
            base.HandleDouble();
        }

        public override void HandleString()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantString;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_StringObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_STRING);
            base.HandleString();
        }

        public override void HandleObjectPath()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantObjectPath;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_ObjectPathObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_OBJECTPATH);
            base.HandleObjectPath();
        }

        public override void HandleSignature()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_Variant;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_SignatureObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_SIGNATURE);
            base.HandleSignature();
        }

        public override void HandleStruct(Udbus.Parsing.ICodeParamType paramtype, string nameStruct, string nameScope)
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_STRUCT_END);
            base.HandleStruct(paramtype, nameStruct, nameScope);
        }

        public override void HandleDictionary(Udbus.Parsing.ICodeParamType paramtype, string nameDictionary, string nameScope)
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_DICT_END);
            base.HandleDictionary(paramtype, nameDictionary, nameScope);
        }

        public override void HandleVariant()
        {
            this.VariantDelegate = UdbusMessageBuilder.BodyAdd_VariantVariant;
            this.ObjectDelegate = UdbusMessageBuilder.BodyAdd_VariantObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_VARIANT);
            base.HandleVariant();
        }

        #endregion // Ends Type Handling

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN);
            return new StructCreatorOut(this, this);
        }

        public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_ARRAY);
            this.AddType(Udbus.Types.dbus_type.DBUS_DICT_BEGIN);
            return new DictCreatorOut(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_ARRAY);
            return new ArrayHandlerOut(this);
        }
        #endregion // ParamCodeTypeHolderBase functions
    } // Ends class UdbusVariantOut

    class ArrayHandlerOut : UdbusVariantOut
    {
        #region Fields
        UdbusVariantOut owner;
        #endregion // Fields

        #region Constructors
        internal ArrayHandlerOut(UdbusVariantOut owner)
            :base()
        {
            this.owner = owner;
        }
        #endregion // Constructors

        #region dbus_type handling
        public override void AddType(Udbus.Types.dbus_type type)
        {
            base.AddType(type);
            owner.AddType(type);
        }

        public override void AddTypeRange(Udbus.Types.dbus_type[] types)
        {
            base.AddTypeRange(types);
            this.owner.AddTypeRange(types);
        }
        #endregion // dbus_type handling

        #region IParamCodeTypeHandler overrides
        public override void HandleCodeParamType(Udbus.Parsing.ICodeParamType paramtype)
        {
            base.HandleCodeParamType(paramtype);
            this.owner.VariantDelegate += this.WriteArrayObject;
            this.owner.ObjectDelegate += this.WriteArrayObject;
        }
        #endregion // IParamCodeTypeHandler overrides

        #region Array writing implementations
        private int WriteArrayObject(Udbus.Serialization.UdbusMessageBuilder builder, Udbus.Containers.dbus_union variantArray)
        {
            int result = builder.MarshalEnumerable(variantArray.DbusObjectArray, this.Type, this.ObjectDelegate);
            return result;
        }

        private int WriteArrayObject(Udbus.Serialization.UdbusMessageBuilder builder, object objArray)
        {
            int result;
            if (objArray is Array == false) // If not array
            {
                result = -1;
                throw new System.InvalidCastException("object is not an array");

            } // Ends if not array
            else // Else array
            {
                object[] output = (object[]) objArray;
                result = builder.MarshalEnumerable(output, this.Type, this.ObjectDelegate);

            } // Ends else array

            return result;
        }
        #endregion // Array writing implementations
    } // Ends class ArrayHandlerOut
} // Ends namespace Udbus.Serialization.Variant
