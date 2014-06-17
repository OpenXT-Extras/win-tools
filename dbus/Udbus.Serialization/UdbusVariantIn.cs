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
    interface IBuildReadVariantDelegate
    {
        Udbus.Serialization.MarshalReadDelegate<Udbus.Containers.dbus_union> VariantDelegate { get; }

    } // Ends interface IBuildReadVariantDelegate

    interface IDbusTypeHolder
    {
        void AddType(Udbus.Types.dbus_type type);
        void AddTypeRange(Udbus.Types.dbus_type[] types);

    } // Ends interface IDbusTypeHolder
    #region Struct Handling

    internal class StructCreatorIn : Udbus.Parsing.StructCreatorBase
    {
        #region StructCreatorIn Internal classes
        public class FieldHandler : UdbusVariantIn
        {
            private StructCreatorIn owner;

            internal FieldHandler(StructCreatorIn owner)
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

        #endregion // StructCreatorIn Internal classes

        #region Fields
        private MarshalReadDelegate<Udbus.Containers.dbus_union> variantDelegate;
        private MarshalReadDelegate<object> objectDelegate;
        #endregion // Fields

        #region Properties
        public MarshalReadDelegate<Udbus.Containers.dbus_union> VariantDelegate
        {
            get { return this.ReadStruct; }
        }
        public MarshalReadDelegate<object> ObjectDelegate
        {
            get { return this.ReadStructObject; }
        }
        #endregion // Properties

        UdbusVariantIn parent;

        #region Constructors
        public StructCreatorIn(UdbusVariantIn parent, Udbus.Parsing.IParamCodeTypeHandler owner)
            : base (owner)
        {
            this.parent = parent;
        }
        #endregion Constructors

        #region IStructParamCodeTypeHandler members
        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateNew()
        {
            return new StructCreatorIn(this.parent, this.owner);
        }

        public override void EndStruct(Udbus.Parsing.BuildContext context, Udbus.Parsing.IParamCodeTypeHandler paramtypeHandler)
        {
            base.EndStruct(context, paramtypeHandler);
            this.parent.VariantDelegate += this.VariantDelegate;
            this.parent.ObjectDelegate += this.ObjectDelegate;
            this.parent.HandleStruct(null, null, null);
        }
        #endregion // IStructParamCodeTypeHandler

        #region dbus_type handling
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
        #endregion // dbus_type handling

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

        public void HandleField(FieldHandler fieldHandler)
        {
            this.variantDelegate += fieldHandler.VariantDelegate;
            this.objectDelegate += fieldHandler.ObjectDelegate;
        }

        #region Struct reading implementations
        private int ReadStruct(Udbus.Serialization.UdbusMessageReader reader, out object[] structFields)
        {
            int result = 0;

            if (this.objectDelegate == null) // If no fields
            {
                structFields = null;

            } // Ends if no fields
            else // Else fields
            {
                result = reader.ReadStructureStart();

                if (result == 0) // If started struct
                {
                    // Yay casting something to what we already know it is because, yeah, that's how this works...
                    System.Delegate[] delegates = this.objectDelegate.GetInvocationList();
                    object[] fields = new object[delegates.Length];

                    int i = 0;
                    foreach (MarshalReadDelegate<object> fn in delegates)
                    {
                        result = fn(reader, out fields[i]);

                        if (result != 0) // If failed to read result
                        {
                            break;

                        } // Ends if failed to read result

                        ++i;

                    } // Ends loop over delegates

                    if (result == 0) // If read fields
                    {
                        result = reader.ReadStructureEnd();

                        if (result != 0) // If struct end failed
                        {
                            structFields = null;

                        } // Ends if struct end failed
                        else // Else struct end ok
                        {
                            structFields = fields;

                        } // Ends else struct end ok
                    } // Ends if read read fields
                    else // Else failed to read fields
                    {
                        structFields = null;

                    } // Ends if failed to read fields
                } // Ends if started struct
                else // Else failed to start struct
                {
                    structFields = null;

                } // Ends else failed to start struct

            } // Ends else fields

            return result;
        }

        private int ReadStruct(Udbus.Serialization.UdbusMessageReader reader, out Udbus.Containers.dbus_union variantStruct)
        {
            int result = 0;

            if (this.objectDelegate == null) // If no fields
            {
                variantStruct = null;

            } // Ends if no fields
            else // Else fields
            {
                object[] structFields;
                result = this.ReadStruct(reader, out structFields);

                if (result == 0) // If started struct
                {
                    // Todo - instantiate the thing which holds onto the field types.
                    // Basically this is our existing container but with object[] instead of dbus_union[]
                    variantStruct = Udbus.Containers.dbus_union.CreateStruct(structFields, this.parent.Types.ToArray());

                } // Ends if started struct
                else // Else failed to start struct
                {
                    variantStruct = null;

                } // Ends else failed to start struct

            } // Ends else fields

            return result;
        }

        // The object returned from a struct read is always a variant...
        private int ReadStructObject(Udbus.Serialization.UdbusMessageReader reader, out object objStruct)
        {
            //Udbus.Containers.dbus_union variantStruct;
            //int result = this.ReadStruct(reader, out variantStruct);
            object[] structFields;
            int result = this.ReadStruct(reader, out structFields);
            if (result != 0) // If failed to read struct
            {
                objStruct = null;

            } // Ends if failed to read struct
            else // Else read struct
            {
                //objStruct = variantStruct;
                objStruct = structFields;

            } // Ends else read struct

            return result;
        }
        #endregion // Struct reading implementations

    } // Ends class StructCreatorIn

    #endregion // Struct Handling

    #region Dictionary Handling

    internal class DictCreatorIn : DictCreatorBase, IBuildReadVariantDelegate
    {
        #region Handlers

        internal class KeyHandler : UdbusPrimitiveIn
        {
            #region Fields
            DictCreatorIn owner;
            #endregion // Fields

            #region Constructors
            internal KeyHandler(DictCreatorIn owner)
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

        internal class ValueHandler : UdbusVariantIn
        {
            #region Fields
            DictCreatorIn owner;
            #endregion // Fields

            #region Constructors
            internal ValueHandler(DictCreatorIn owner)
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

        private UdbusVariantIn owner;
        protected MarshalReadDelegate<object> ObjectDelegateKey;
        protected MarshalReadDelegate<object> ObjectDelegateValue;

        #endregion  //Fields

        #region Properties

        public MarshalReadDelegate<Udbus.Containers.dbus_union> VariantDelegate { get { return this.ReadDictionary; } }
        public MarshalReadDelegate<object> ObjectDelegate { get { return this.ReadDictionaryObject; } }

        #endregion // Properties

        #region Constructors
        internal DictCreatorIn(UdbusVariantIn owner)
        {
            this.owner = owner;
        }
        #endregion // Constructors

        #region Dictionary reading implementations
        private int ReadDictionary(Udbus.Serialization.UdbusMessageReader reader, out Dictionary<object, object> dict)
        {
            IDictionary<object, object> idict;
            int result = reader.MarshalDict(this.ObjectDelegateKey, this.ObjectDelegateValue, out idict);
            dict = new Dictionary<object, object>(idict);
            return result;
        }

        private int ReadDictionary(Udbus.Serialization.UdbusMessageReader reader, out Udbus.Containers.dbus_union variantDict)
        {
            Dictionary<object, object> dict;
            int result = this.ReadDictionary(reader, out dict);
            variantDict = Udbus.Containers.dbus_union.Create(dict, this.owner.Types.ToArray());
            return result;
        }

        private int ReadDictionaryObject(Udbus.Serialization.UdbusMessageReader reader, out object objDict)
        {
            Dictionary<object, object> dict;
            int result = this.ReadDictionary(reader, out dict);
            objDict = dict;
            return result;
        }
        #endregion Dictionary reading implementations

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
            return new KeyHandler(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateValueHandler()
        {
            return new ValueHandler(this);
        }
        #endregion IDictParamCodeTypeHandler Members

        internal void HandleKey(KeyHandler handler, Udbus.Parsing.ICodeParamType paramtype)
        {
            this.ObjectDelegateKey = handler.ObjectDelegate;
        }

        internal void HandleValue(ValueHandler handler, Udbus.Parsing.ICodeParamType paramtype)
        {
            this.ObjectDelegateValue = handler.ObjectDelegate;
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
    } // Ends class DictCreatorIn

    #endregion // Dictionary Handling

    internal class UdbusPrimitiveIn : Udbus.Parsing.ParamCodeTypeHolderBase
    {
        private Udbus.Serialization.MarshalReadDelegate<object> objectDelegate;
        public Udbus.Serialization.MarshalReadDelegate<object> ObjectDelegate { get { return this.objectDelegate; } }
        private Udbus.Types.dbus_type type = Udbus.Types.dbus_type.DBUS_INVALID;

        #region Properties
        public Udbus.Types.dbus_type Type { get { return this.type; } }
        #endregion // Properties

        protected virtual void AddType(Udbus.Types.dbus_type type)
        {
            this.type = type;
        }

        #region ParamCodeTypeHolderBase functions
        #region Type Handling
        public override void HandleByte()
        {
            this.objectDelegate = UdbusMessageReader.ReadByteObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BYTE);
            base.HandleByte();
        }

        public override void HandleBoolean()
        {
            this.objectDelegate = UdbusMessageReader.ReadBooleanObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BOOLEAN);
            base.HandleBoolean();
        }

        public override void HandleInt16()
        {
            this.objectDelegate = UdbusMessageReader.ReadInt16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT16);
            base.HandleInt16();
        }

        public override void HandleUInt16()
        {
            this.objectDelegate = UdbusMessageReader.ReadUInt16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT16);
            base.HandleUInt16();
        }

        public override void HandleInt32()
        {
            this.objectDelegate = UdbusMessageReader.ReadInt32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT32);
            base.HandleInt32();
        }

        public override void HandleUInt32()
        {
            this.objectDelegate = UdbusMessageReader.ReadUInt32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT32);
            base.HandleUInt32();
        }

        public override void HandleInt64()
        {
            this.objectDelegate = UdbusMessageReader.ReadInt64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT64);
            base.HandleInt64();
        }

        public override void HandleUInt64()
        {
            this.objectDelegate = UdbusMessageReader.ReadUInt64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT64);
            base.HandleUInt64();
        }

        public override void HandleDouble()
        {
            this.objectDelegate = UdbusMessageReader.ReadDoubleObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_DOUBLE);
            base.HandleDouble();
        }

        public override void HandleString()
        {
            this.objectDelegate = UdbusMessageReader.ReadStringObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_STRING);
            base.HandleString();
        }

        public override void HandleObjectPath()
        {
            this.objectDelegate = UdbusMessageReader.ReadObjectPathObject;
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
            this.objectDelegate = UdbusMessageReader.ReadVariantObject;
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
    } // Ends class UdbusPrimitiveIn

    public class UdbusVariantIn : Udbus.Serialization.UdbusVariantBase
        , IDbusTypeHolder
    {
        #region Fields
        List<Udbus.Types.dbus_type> types = new List<Udbus.Types.dbus_type>(new Udbus.Types.dbus_type[] { Udbus.Types.dbus_type.DBUS_INVALID });
        #endregion // Fields

        #region Properties
        MarshalReadDelegate<Udbus.Containers.dbus_union> variantDelegate;
        public MarshalReadDelegate<Udbus.Containers.dbus_union> VariantDelegate
        {
            get { return this.variantDelegate; }
            set { this.variantDelegate = value; }
        }

        MarshalReadDelegate<object> objectDelegate;
        public virtual MarshalReadDelegate<object> ObjectDelegate
        {
            get { return this.objectDelegate; }
            set { this.objectDelegate = value; }
        }

        public List<Udbus.Types.dbus_type> Types { get { return this.types; } }
        #endregion // Properties

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

        public Udbus.Types.dbus_type Type { get { return this.types[0]; } }

        #region Constructors
        public UdbusVariantIn()
            : base(FieldDirection.Out)
        {
        }
        #endregion // Constructors

        #region ParamCodeTypeHolderBase functions
        #region Type Handling
        public override void HandleByte()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantByte;
            this.ObjectDelegate = UdbusMessageReader.ReadByteObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BYTE);
            base.HandleByte();
        }

        public override void HandleBoolean()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantBoolean;
            this.ObjectDelegate = UdbusMessageReader.ReadBooleanObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_BOOLEAN);
            base.HandleBoolean();
        }

        public override void HandleInt16()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantInt16;
            this.ObjectDelegate = UdbusMessageReader.ReadInt16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT16);
            base.HandleInt16();
        }

        public override void HandleUInt16()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantUInt16;
            this.ObjectDelegate = UdbusMessageReader.ReadUInt16Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT16);
            base.HandleUInt16();
        }

        public override void HandleInt32()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantInt32;
            this.ObjectDelegate = UdbusMessageReader.ReadInt32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT32);
            base.HandleInt32();
        }

        public override void HandleUInt32()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantUInt32;
            this.ObjectDelegate = UdbusMessageReader.ReadUInt32Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT32);
            base.HandleUInt32();
        }

        public override void HandleInt64()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantInt64;
            this.ObjectDelegate = UdbusMessageReader.ReadInt64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_INT64);
            base.HandleInt64();
        }

        public override void HandleUInt64()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantUInt64;
            this.ObjectDelegate = UdbusMessageReader.ReadUInt64Object;
            this.AddType(Udbus.Types.dbus_type.DBUS_UINT64);
            base.HandleUInt64();
        }

        public override void HandleDouble()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantDouble;
            this.ObjectDelegate = UdbusMessageReader.ReadDoubleObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_DOUBLE);
            base.HandleDouble();
        }

        public override void HandleString()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantString;
            this.ObjectDelegate = UdbusMessageReader.ReadStringObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_STRING);
            base.HandleString();
        }

        public override void HandleObjectPath()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariantObjectPath;
            this.ObjectDelegate = UdbusMessageReader.ReadObjectPathObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_OBJECTPATH);
            base.HandleObjectPath();
        }

        public override void HandleSignature()
        {
            this.VariantDelegate = UdbusMessageReader.ReadVariant;
            this.ObjectDelegate = UdbusMessageReader.ReadSignatureObject;
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
            this.VariantDelegate = UdbusMessageReader.ReadVariantVariant;
            this.ObjectDelegate = UdbusMessageReader.ReadVariantObject;
            this.AddType(Udbus.Types.dbus_type.DBUS_VARIANT);
            base.HandleVariant();
        }
        #endregion // Ends Type Handling

        public override Udbus.Parsing.IStructParamCodeTypeHandler CreateStructHandler()
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN);
            return new StructCreatorIn(this, this);
        }

        public override Udbus.Parsing.IDictParamCodeTypeHandler CreateDictHandler()
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_ARRAY);
            this.AddType(Udbus.Types.dbus_type.DBUS_DICT_BEGIN);
            return new DictCreatorIn(this);
        }

        public override Udbus.Parsing.IParamCodeTypeHandler CreateNewArrayHandler()
        {
            this.AddType(Udbus.Types.dbus_type.DBUS_ARRAY);
            return new ArrayHandlerIn(this);
        }
        #endregion // ParamCodeTypeHolderBase functions
    } // Ends class UdbusVariantIn

    class ArrayHandlerIn : UdbusVariantIn
    {
        UdbusVariantIn owner;

        internal ArrayHandlerIn(UdbusVariantIn owner)
        {
            this.owner = owner;
        }

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
            this.owner.VariantDelegate += this.ReadArrayObject;
        }
        #endregion // IParamCodeTypeHandler overrides

        private int ReadArrayObject(Udbus.Serialization.UdbusMessageReader reader, out Udbus.Containers.dbus_union variantArray)
        {
            object[] output;
            int result = reader.MarshalEnumerable(this.ObjectDelegate, this.Type, out output);
            variantArray = Udbus.Containers.dbus_union.Create(output, this.owner.Types.ToArray());
            return result;
        }

        private int ReadArrayObject(Udbus.Serialization.UdbusMessageReader reader, out object objDict)
        {
            object[] output;
            int result = reader.MarshalEnumerable(this.ObjectDelegate, this.Type, out output);
            objDict = output;
            return result;
        }
    } // Ends class ArrayHandlerIn
} // Ends namespace Udbus.Serialization.Variant
