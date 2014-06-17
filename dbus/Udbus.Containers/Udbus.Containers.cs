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
using System.Runtime.InteropServices;

namespace Udbus.Containers
{
    public enum StringType
    {
        DBUS_STRING = Udbus.Types.dbus_type.DBUS_STRING,
        DBUS_OBJECTPATH = Udbus.Types.dbus_type.DBUS_OBJECTPATH,
        DBUS_SIGNATURE = Udbus.Types.dbus_type.DBUS_SIGNATURE

    } // Ends enum dbus_string_types

    public enum ArrayType
    {
        DBUS_ARRAY = Udbus.Types.dbus_type.DBUS_ARRAY,
        DBUS_STRUCT_BEGIN = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial class dbus_primitive
    {
        // Managed types need their own offset for the GC to keep track of live objects.
        // Lovely and not documented.
        protected const int UnmanagedOffset = 4;
        protected const int ManagedOffset = 12;

        #region Primitive Fields
        [FieldOffset(0)]
        protected Udbus.Types.dbus_type type;
        [FieldOffset(UnmanagedOffset)]
        private bool dbusBoolean;
        [FieldOffset(UnmanagedOffset)]
        private byte dbusByte;
        [FieldOffset(ManagedOffset)]
        private string dbusString;
        [FieldOffset(UnmanagedOffset)]
        private Int16 dbusInt16;
        [FieldOffset(UnmanagedOffset)]
        private UInt16 dbusUInt16;
        [FieldOffset(UnmanagedOffset)]
        private Int32 dbusInt32;
        [FieldOffset(UnmanagedOffset)]
        private UInt32 dbusUInt32;
        [FieldOffset(UnmanagedOffset)]
        private Int64 dbusInt64;
        [FieldOffset(UnmanagedOffset)]
        private UInt64 dbusUInt64;
        [FieldOffset(UnmanagedOffset)]
        private double dbusDouble;
        [FieldOffset(UnmanagedOffset)]
        private dbus_invalid dbusInvalid;

        //[FieldOffset(ManagedOffset)]
        //private dbus_sig dbusSignature;
        [FieldOffset(ManagedOffset)]
        private Udbus.Types.UdbusObjectPath dbusObjectPath;
        //private string dbusObjectPath;

        // Considering just moving to this...
        [FieldOffset(ManagedOffset)]
        protected object dbusValue;

        #endregion // Primitive Fields

        public readonly static dbus_invalid Invalid = new dbus_invalid();

        #region Properties
        public virtual Udbus.Types.dbus_type Type
        {
            get
            { return this.type; }
            protected set
            {
                this.type = value;
            }
        }
        #endregion // Properties

        #region Constructors
        public dbus_primitive()
        {
            this.Type = Udbus.Types.dbus_type.DBUS_INVALID;
            this.dbusInvalid = Invalid;
            this.dbusValue = Invalid;
        }

        public dbus_primitive(string value)
        {
            this.Type = Udbus.Types.dbus_type.DBUS_STRING;
            this.dbusString = value;
            this.dbusValue = value;
        }

        public dbus_primitive(string value, StringType stringtype)
        {
            switch (stringtype)
            {
                case StringType.DBUS_STRING:
                    this.Type = Udbus.Types.dbus_type.DBUS_STRING;
                    this.dbusString = value;
                    this.dbusValue = value;
                    break;
                case StringType.DBUS_OBJECTPATH:
                    this.Type = Udbus.Types.dbus_type.DBUS_OBJECTPATH;
                    this.dbusObjectPath = new Udbus.Types.UdbusObjectPath(value);
                    //this.dbusObjectPath = value;
                    this.dbusValue = value;
                    break;
                //case StringType.DBUS_SIGNATURE:
                //    this.Type = Udbus.Types.dbus_type.DBUS_SIGNATURE;
                //    this.dbusSignature = value;
                //    this.dbusValue = value;
                //    break;
            }
        }

        //public dbus_primitive(Udbus.Types.UdbusObjectPath value)
        //{
        //    this.Type = Udbus.Types.dbus_type.DBUS_OBJECTPATH;
        //    this.dbusObjectPath = value;
        //}

        //public dbus_primitive(Boolean value)    { this.Type = Udbus.Types.dbus_type.DBUS_BOOLEAN;   this.dbusBoolean = value; }
        //public dbus_primitive(Byte value)       { this.Type = Udbus.Types.dbus_type.DBUS_BYTE;      this.dbusByte = value; }
        //public dbus_primitive(Int16 value)      { this.Type = Udbus.Types.dbus_type.DBUS_INT16;     this.dbusInt16 = value; }
        //public dbus_primitive(UInt16 value)     { this.Type = Udbus.Types.dbus_type.DBUS_UINT16;    this.dbusUInt16 = value; }
        //public dbus_primitive(Int32 value)      { this.Type = Udbus.Types.dbus_type.DBUS_INT32;     this.dbusInt32 = value; }
        //public dbus_primitive(UInt32 value)     { this.Type = Udbus.Types.dbus_type.DBUS_UINT32;    this.dbusUInt32 = value; }
        //public dbus_primitive(Int64 value)      { this.Type = Udbus.Types.dbus_type.DBUS_INT64;     this.dbusInt64 = value; }
        //public dbus_primitive(UInt64 value)     { this.Type = Udbus.Types.dbus_type.DBUS_UINT64;    this.dbusUInt64 = value; }
        //public dbus_primitive(double value)     { this.Type = Udbus.Types.dbus_type.DBUS_DOUBLE;    this.dbusDouble = value; }
        #endregion // Constructors

        #region Conversions
        #endregion // Conversions

        #region Factory functions
        //static public dbus_primitive Create(string value)           { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_STRING,     dbusString = value }; }
        //static public dbus_primitive CreateSignature(string value)  { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_SIGNATURE,  dbusSignature = value }; }
        //static public dbus_primitive CreateObjectPath(string value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_OBJECTPATH, dbusObjectPath = value }; }
        //static public dbus_primitive Create(Boolean value)          { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_BOOLEAN,    dbusBoolean = value }; }
        //static public dbus_primitive Create(Byte value)             { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_BYTE,       dbusByte = value }; }
        //static public dbus_primitive Create(Int16 value)            { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INT16,      dbusInt16 = value }; }
        //static public dbus_primitive Create(UInt16 value)           { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_UINT16,     dbusUInt16 = value }; }
        //static public dbus_primitive Create(Int32 value)            { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INT32,      dbusInt32 = value }; }
        //static public dbus_primitive Create(UInt32 value)           { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_UINT32,     dbusUInt32 = value }; }
        //static public dbus_primitive Create(Int64 value)            { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INT64,      dbusInt64 = value }; }
        //static public dbus_primitive Create(UInt64 value)           { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_UINT64,     dbusUInt64 = value }; }
        //static public dbus_primitive Create(double value)           { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_DOUBLE,     dbusDouble = value }; }

#if _MAGIC
        #region Static creation

        static public dbus_primitive Create(string value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_SIGNATURE, dbusSignature = value }; }
        static public dbus_primitive Create(string value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_OBJECTPATH, dbusObjectPath = value }; }
        static public dbus_primitive Create(Boolean value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_BOOLEAN, dbusBoolean = value }; }
        static public dbus_primitive Create(Byte value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_BYTE, dbusByte = value }; }
        static public dbus_primitive Create(String value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_STRING, dbusString = value }; }
        static public dbus_primitive Create(Int16 value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INT16, dbusInt16 = value }; }
        static public dbus_primitive Create(UInt16 value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_UINT16, dbusUInt16 = value }; }
        static public dbus_primitive Create(Int32 value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INT32, dbusInt32 = value }; }
        static public dbus_primitive Create(UInt32 value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_UINT32, dbusUInt32 = value }; }
        static public dbus_primitive Create(Int64 value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INT64, dbusInt64 = value }; }
        static public dbus_primitive Create(UInt64 value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_UINT64, dbusUInt64 = value }; }
        static public dbus_primitive Create(double value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_DOUBLE, dbusDouble = value }; }
        static public dbus_primitive Create(dbus_union.Invalid value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_INVALID, dbusInvalid = value }; }
        static public dbus_primitive Create(dbus_union[] value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_ARRAY, dbusArray = value }; }
        static public dbus_primitive Create(dbus_union[] value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, dbusStruct = value }; }
        static public dbus_primitive Create(Dictionary<dbus_primitive, dbus_union> value) { return new dbus_primitive { Type = Udbus.Types.dbus_type.DBUS_DICT_BEGIN, dbusDictionary = value }; }
        #endregion // Static creation
#endif //!_MAGIC

        #endregion // Factory functions

        protected string BaseToString()
        {
            return base.ToString();
        }

        public override string ToString()
        {
            return string.Join("", new string[] { this.BaseToString(), " type=", this.Type.ToString(), ": ", this.ValueAsString() });
        }

        #region Casting
        //public static implicit operator Int32(dbus_primitive primitive)
        //{
        //    if (primitive.Type != Udbus.Types.dbus_type.DBUS_INT32)
        //    {
        //        throw primitive.CreateCastException(dbus_type.DBUS_INT32);
        //    }
        //    return primitive.dbusInt32;
        //}
        #endregion // Casting

        #region Exceptions
        protected Exceptions.InvalidVariantCastException CreateCastException(Udbus.Types.dbus_type requestedType)
        {
            return Exceptions.InvalidVariantCastException.Create(this.Type, this.ValueAsString(), requestedType);
        }
        #endregion // Exceptions

        #region Helpers
        protected static string ObjectAsString(object o)
        {
            StringBuilder result = new StringBuilder();
            if (o is string)
            {
                result.AppendFormat("'{0}'", o);
            }
            else if (o is System.Collections.DictionaryEntry)
            {
                System.Collections.DictionaryEntry de = (System.Collections.DictionaryEntry)o;
                result.AppendFormat(" {0} => {1}", ObjectAsString(de.Key), ObjectAsString(de.Value));
            }
            else if (o is Dictionary<object, object>)
            {
                Dictionary<object, object> d = (Dictionary<object, object>)o;
                result.Append("{");
                foreach (KeyValuePair<object, object> pair in d)
                {
                    result.AppendFormat(" {0} => {1};", ObjectAsString(pair.Key), ObjectAsString(pair.Value));
                }
                result.Append("}");
            }
            else if (o is System.Collections.IEnumerable)
            {
                result.Append("[");
                System.Collections.IEnumerable e = (System.Collections.IEnumerable)o;
                System.Collections.IEnumerator et = e.GetEnumerator();
                if (et.MoveNext())
                {
                    result.Append(ObjectAsString(et.Current));

                    while (et.MoveNext())
                    {
                        result.Append(", ");
                        result.Append(ObjectAsString(et.Current));
                    }
                }
                result.Append("]");
            }
            else
            {
                result.Append(o.ToString());
            }
            return result.ToString();
        }
        #endregion // Helpers
    } // Ends public class dbus_primitive

    [StructLayout(LayoutKind.Explicit)]
    public partial class dbus_union : dbus_primitive
    {
        const int TypesOffset = ManagedOffset + 4;
        //[FieldOffset(4)]
        //private string dbusSignature;
        //[FieldOffset(4)]
        //private string dbusObjectpath;
        //[FieldOffset(4)]
        //private bool dbusBoolean;
        //[FieldOffset(4)]
        //private byte dbusByte;
        //[FieldOffset(4)]
        //private string dbusString;
        //[FieldOffset(4)]
        //private Int16 dbusInt16;
        //[FieldOffset(4)]
        //private UInt16 dbusUInt16;
        //[FieldOffset(4)]
        //private Int32 dbusInt32;
        //[FieldOffset(4)]
        //private UInt32 dbusUInt32;
        //[FieldOffset(4)]
        //private Int64 dbusInt64;
        //[FieldOffset(4)]
        //private UInt64 dbusUInt64;
        //[FieldOffset(4)]
        //private double dbusDouble;
        [FieldOffset(ManagedOffset)]
        private Udbus.Types.dbus_sig dbusSignature;
        [FieldOffset(ManagedOffset)]
        private dbus_union[] dbusArray;//dbus_array dbusArray;
        [FieldOffset(ManagedOffset)]
        private object[] dbusObjectArray;
        [FieldOffset(ManagedOffset)]
        private dbus_union[] dbusStruct;
        [FieldOffset(ManagedOffset)]
        private object[] dbusObjectStruct;
        //private dbus_structure dbusStruct;
        [FieldOffset(ManagedOffset)]
        //private dbus_dictionary dbusDict;
        private Dictionary<object, object> dbusDictionary;
        [FieldOffset(ManagedOffset)]
        private dbus_union dbusVariant;

        #region dbus_union Fields
        [FieldOffset(TypesOffset)]
        //dbus_type[] types;
        Udbus.Types.dbus_sig sig;

        static readonly private Udbus.Types.dbus_type[] InvalidTypes = { Udbus.Types.dbus_type.DBUS_INVALID };
        #endregion // dbus_union Fields

        #region dbus_union Properties
        public override Udbus.Types.dbus_type Type
        {
            get
            {
                return base.Type;
            }
            protected set
            {
                base.Type = value;

                if (value == Udbus.Types.dbus_type.DBUS_INVALID)
                {
                    this.sig.a = InvalidTypes;
                }
                else
                {
                    this.sig.a = new Udbus.Types.dbus_type[] { value, Udbus.Types.dbus_type.DBUS_INVALID };
                }
            }
        }

        public Udbus.Types.dbus_type[] Types
        {
            get { return this.sig.a; }
            protected set
            {
                // Sort out the type.
                if (value == null)
                {
                    this.Type = Udbus.Types.dbus_type.DBUS_INVALID;
                    this.sig.a = value;
                }
                else if (value.Length == 0)
                {
                    this.Type = Udbus.Types.dbus_type.DBUS_INVALID;
                }
                else
                {
                    base.Type = value[0]; // Yes base, not this.
                    this.sig.a = value;
                }
            }
        }

        public Udbus.Types.dbus_sig Signature { get { return this.sig; } }
        #endregion // dbus_union Properties

        #region dbus_union Constructors
        public dbus_union(Udbus.Types.dbus_type[] types)
        {
            this.Types = types;
        }

        public dbus_union(Udbus.Types.dbus_type type)
            : this(new Udbus.Types.dbus_type[] { type, Udbus.Types.dbus_type.DBUS_INVALID })
        {
        }

        public dbus_union(object[] dbusArray, ArrayType arrayType, Udbus.Types.dbus_type[] types)
            : base()
        {
            switch (arrayType)
            {
                case ArrayType.DBUS_ARRAY:
                    CheckTypesNotEmpty(types);
                    CheckTypesForSetStart(dbusArray, Udbus.Types.dbus_type.DBUS_ARRAY, types);
                    //dbus_type arrayTypes = new Udbus.Types.dbus_type[types.Length + 1];
                    //arrayTypes[0] = Udbus.Types.dbus_type.DBUS_ARRAY;
                    //types.CopyTo(arrayTypes, 1);
                    this.dbusObjectArray = dbusArray;
                    this.dbusValue = dbusArray;
                    this.Types = types;
                    break;

                case ArrayType.DBUS_STRUCT_BEGIN:
                    CheckTypesNotEmpty(types);
                    CheckTypesForSetStart(dbusArray, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, types);
                    CheckTypesForSetEnd(dbusArray, Udbus.Types.dbus_type.DBUS_STRUCT_END, types);
                    //this.Type = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN;
                    this.dbusObjectStruct = dbusArray;
                    this.dbusValue = dbusArray;
                    this.Types = types;
                    break;
            }
        }

        public dbus_union(Dictionary<object, object> dbusDictionary, Udbus.Types.dbus_type[] types)
        {
            CheckTypesNotEmpty(types);
            CheckTypesForSetStart(dbusArray, Udbus.Types.dbus_type.DBUS_ARRAY, types);
            CheckTypesForSet(dbusArray, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, types, 1);
            CheckTypesForSetEnd(dbusArray, Udbus.Types.dbus_type.DBUS_DICT_END, types);
            this.dbusDictionary = dbusDictionary;
            this.Types = types;
        }

        public dbus_union()
            : base()
        {
        }

        //public dbus_union(dbus_union[] dbusArray, ArrayType arrayType)
        //    : base()
        //{
        //    switch (arrayType)
        //    {
        //        case ArrayType.DBUS_ARRAY:
        //            this.Type = Udbus.Types.dbus_type.DBUS_ARRAY;
        //            this.DbusArray = dbusArray;
        //            this.dbusValue = dbusArray;
        //            break;

        //        case ArrayType.DBUS_STRUCT_BEGIN:
        //            this.Type = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN;
        //            this.DbusStruct = dbusArray;
        //            this.dbusValue = dbusArray;
        //            break;
        //    }
        //}
        #endregion // Constructors

        #region Type check properties
        public bool IsDbusDictionary { get { return this.Types != null && this.Types.Length > 1 && this.Types[0] == Udbus.Types.dbus_type.DBUS_ARRAY && this.Types[1] == Udbus.Types.dbus_type.DBUS_DICT_BEGIN; } }
        #endregion // Type check properties

        public override string ToString()
        {
            return string.Join("", new string[] { this.BaseToString(), " types: ", Udbus.Types.dbus_sig.ToString(this.Types), ": ", this.ValueAsString() });
        }

        #region Static creation functions
        static public dbus_union Create(Dictionary<object, object> value, Udbus.Types.dbus_type[] types)
        {
            return new dbus_union(value, types);
            //return new dbus_union { DbusDictionary = value };
        }
        static public dbus_union Create(object[] value, Udbus.Types.dbus_type[] types)
        {
            return new dbus_union(value, ArrayType.DBUS_ARRAY, types);
            //return new dbus_union { DbusObjectArray = value };
        }
        static public dbus_union CreateStruct(object[] value, Udbus.Types.dbus_type[] types)
        {
            return new dbus_union(value, ArrayType.DBUS_STRUCT_BEGIN, types);
            //return new dbus_union { DbusObjectStruct = value };
        }
        #endregion // Ends Static creation functions

        #region dbus_union Accessors
        public void SetDbusObjectArray(object[] dbusObjectArray, Udbus.Types.dbus_type[] types)
        {
            CheckTypesForSetStart(dbusArray, Udbus.Types.dbus_type.DBUS_ARRAY, types);
            this.dbusObjectArray = dbusObjectArray;
            this.Types = types;
        }

        public void SetDbusObjectStruct(object[] dbusObjectStruct, Udbus.Types.dbus_type[] types)
        {
            CheckTypesForSetStart(dbusArray, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, types);
            CheckTypesForSetEnd(dbusArray, Udbus.Types.dbus_type.DBUS_STRUCT_END, types);
            this.dbusObjectStruct = dbusObjectStruct;
            this.Types = types;
        }

        public void SetDbusDictionary(Dictionary<object, object> dbusDictionary, Udbus.Types.dbus_type[] types)
        {
            CheckTypesForSetStart(dbusArray, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, types);
            CheckTypesForSetEnd(dbusArray, Udbus.Types.dbus_type.DBUS_DICT_END, types);
            this.dbusDictionary = dbusDictionary;
            this.Types = types;
        }

        #endregion // dbus_union Accessors
        #region NewStuff

        #endregion // NewStuff

        #region Internal Validation
        /// <summary>
        /// Throws an exception if types empty.
        /// </summary>
        /// <param name="types">Requested types.</param>
        static private void CheckTypesNotEmpty(Udbus.Types.dbus_type[] types)
        {
            if (types == null) // If null types
            {
                throw new ArgumentNullException("types");

            } // Ends if null types
            else if (types.Length == 0) // Else if no types
            {
                throw new ArgumentException("Empty types array is invalid", "types");

            } // Ends else if no types
        }

        /// <summary>
        /// Throws an exception if types invalid.
        /// </summary>
        /// <param name="value">Value being set.</param>
        /// <param name="requestedType">Type being requested.</param>
        /// <param name="types">Requested types for value.</param>
        /// <param name="index">Index of type to check.</param>
        static private void CheckTypesForSet(object value, Udbus.Types.dbus_type requestedType, Udbus.Types.dbus_type[] types, int index)
        {
            if (types.Length <= index) // Else if index too large
            {
                throw new IndexOutOfRangeException(string.Format("Types index out of range. Requested index {0}, Types length {1}",
                    index, types.Length));

            } // Ends else if index too large
            else if (types[index] != requestedType) // Else if not correct type
            {
                throw new Exceptions.IncorrectTypeException(requestedType, value, types);

            } // Ends else if not correct type
        }

        /// <summary>
        /// Check that first type is as requested.
        /// </summary>
        /// <param name="value">Value being set.</param>
        /// <param name="requestedType">Type being requested.</param>
        /// <param name="types">Requested types for value.</param>
        static private void CheckTypesForSetStart(object value, Udbus.Types.dbus_type requestedType, Udbus.Types.dbus_type[] types)
        {
            CheckTypesForSet(value, requestedType, types, 0);
        }

        /// <summary>
        /// Check that last type is as requested.
        /// </summary>
        /// <param name="value">Value being set.</param>
        /// <param name="requestedType">Type being requested.</param>
        /// <param name="types">Requested types for value.</param>
        static private void CheckTypesForSetEnd(object value, Udbus.Types.dbus_type requestedType, Udbus.Types.dbus_type[] types)
        {
            CheckTypesForSet(value, requestedType, types, types.Length - 1);
        }
        #endregion // Internal Validation
    } // Ends class dbus_union

    public struct dbus_structure
    {
        public dbus_union[] Fields;

    } // Ends struct dbus_structure

    public class dbus_dictionary : Dictionary<dbus_primitive, dbus_union>
    {
        //public Dictionary<dbus_primitive, dbus_union>

    } // Ends struct dbus_dictionary

    public struct dbus_invalid
    {

    } // Ends struct dbus_invalid

    class Example
    {
        static void UseFactory()
        {
            Int16 i16 = 1;
            Int32 i32 = 1;
            dbus_primitive p1 = dbus_primitive.Create(i16);
            dbus_primitive p2 = dbus_primitive.Create(i32);
            dbus_primitive p3 = dbus_primitive.Create(1);

        }

        static void CastToContainer()
        {
            dbus_primitive dpi32 = dbus_primitive.Create(1);
            Int32 i32 = dpi32.DbusInt32;
        }

        static void WriteSnippets()
        {
            //string structSig = "(bs)";
            Udbus.Types.dbus_type[] terminator = new Udbus.Types.dbus_type[] { Udbus.Types.dbus_type.DBUS_INVALID };
            Udbus.Types.dbus_type[] structSig = new Udbus.Types.dbus_type[] { Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, Udbus.Types.dbus_type.DBUS_BOOLEAN, Udbus.Types.dbus_type.DBUS_STRING, Udbus.Types.dbus_type.DBUS_STRUCT_END };
            Udbus.Types.dbus_type[] structTypes = structSig.Concat(terminator).ToArray();
            //object[] astruct = new object[] { dbus_union.CreateStruct(new object[] { true, "One" }, structTypes),
            //    dbus_union.CreateStruct(new object[] { false, "Two" }, structTypes)
            //};
            object[] astruct = new object[] { new object[] { true, "One" },
                new object[] { false, "Two" }
            };
            object[] adict = new object[] {
                new object[] { 1, new object[] { true, "One" }},
                new object[] { 2, new object[] { false, "Two" }}
            };
            Udbus.Types.dbus_type[] dictTypes = new Udbus.Types.dbus_type[] { Udbus.Types.dbus_type.DBUS_ARRAY, Udbus.Types.dbus_type.DBUS_DICT_BEGIN, Udbus.Types.dbus_type.DBUS_INT32 }
                .Concat(structSig)
                .Concat(new Udbus.Types.dbus_type[] { Udbus.Types.dbus_type.DBUS_DICT_END })
                .Concat(terminator)
                .ToArray();
            dbus_union s = dbus_union.CreateStruct(astruct, structTypes);
            dbus_union d = dbus_union.Create(adict, dictTypes);

            //UdbusMessageBuilder builder = new UdbusMessageBuilder();
            //MarshalWriteObjectDelegate del = UdbusMessageBuilder.BodyAdd_String;
        }

        static void ManuallyBuildType()
        {
            // Array of dictionary of int => array of structs(array of strings, double)
            // "aa{ia(asd)}"
            dbus_union root = dbus_union.Create(
                new Dictionary<object, object>
                {
                    {
                        1, // Key
                        new object[] { // Value (array of structs)
                            new object[] {
                                new string[] { "a1" },
                                1.1
                            }, // Ends struct 1 fields
                            new object[] {
                                new string[] { "a1a" },
                                1.2
                            } // Ends struct 2 fields
                        } // Ends array of structs (value)
                    }, // Ends dictionary entry 1
                    {
                        2, // Key
                        new object[] { // Value (array of structs)
                            new object[] {
                                new string[] { "a2" },
                                2.1
                            }, // Ends struct 1 fields
                            new object[] {
                                new string[] { "a2a" },
                                2.2
                            } // Ends struct 2 fields
                        } // Ends array of structs (value)
                    } // Ends dictionary entry 1
                },
                new Udbus.Types.dbus_type[] {
                    Udbus.Types.dbus_type.DBUS_INT32, Udbus.Types.dbus_type.DBUS_ARRAY, Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN,
                        Udbus.Types.dbus_type.DBUS_ARRAY, Udbus.Types.dbus_type.DBUS_STRING, Udbus.Types.dbus_type.DBUS_DOUBLE,
                    Udbus.Types.dbus_type.DBUS_STRUCT_END
                }
            );

#if MANUALLABOUR1 || MANUALLABOUR2
            dbus_union root = new dbus_union
            {
#if MANUALLABOUR1
                //Type = Udbus.Types.dbus_type.DBUS_ARRAY,
                DbusObjectArray = new dbus_union[]
                {
                    new dbus_union // root.DbusArray[0]
                    {
                        //Type = Udbus.Types.dbus_type.DBUS_DICT_BEGIN,
                        DbusDictionary = new Dictionary<object,object>
                        {  // root.DbusArray[0] Dictionary
                            { // root.DbusArray[0].DbusDictionary Entry 1
                                new dbus_union { /*Type = Udbus.Types.dbus_type.DBUS_INT32, */DbusInt32 = 1 }, // root.DbusArray[0].Key
                                new dbus_union // root.DbusArray[0].Value
                                {   //Type = Udbus.Types.dbus_type.DBUS_ARRAY, // root.DbusArray[0].Value Array
                                    DbusObjectArray = new dbus_union[]
                                    {
                                        new dbus_union
                                        {   //Type = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, // root.DbusArray[0].Value Array[0].Struct
                                            DbusObjectStruct = new dbus_union[] // root.DbusArray[0].Value Array[0].Struct.Fields
                                            {
                                                new dbus_union // root.DbusArray[0].Value Array[0].Struct[0]
                                                {
                                                    //Type = Udbus.Types.dbus_type.DBUS_ARRAY,
                                                    DbusObjectArray = new dbus_union[] // root.DbusArray[0].Value Array[0].Struct[0].Array
                                                    {
                                                        new dbus_union { /*Type = Udbus.Types.dbus_type.DBUS_STRING, */DbusString = "One" },
                                                        new dbus_union { /*Type = Udbus.Types.dbus_type.DBUS_STRING, */DbusString = "Two" }
                                                    } // root.DbusArray[0].Value Array[0].Struct[0].Array
                                                }, // Ends root.DbusArray[0].Value Array[0].Struct[0]
                                                new dbus_union // root.DbusArray[0].Value Array[0].Struct[1]
                                                {
                                                    //Type = Udbus.Types.dbus_type.DBUS_DOUBLE,
                                                    DbusDouble = 1.2
                                                } // Ends root.DbusArray[0].Value Array[0].Struct[1]
                                            } // Ends root.DbusArray[0].Value Array[0].Struct.Fields
                                        } // Ends root.DbusArray[0].Value Array[0].Struct
                                    } // // root.DbusArray[0].Value Array
                                } // root.DbusArray[0].Value
                            } // Ends root.DbusArray[0].DbusDictionary Entry 1
                        } // Ends root.DbusArray[0] Dictionary
                    } // Ends root.DbusArray[0]
                } // Ends DbusArray

#elif MANUALLABOUR2
                //Type = Udbus.Types.dbus_type.DBUS_ARRAY,
                DbusArray = new dbus_union[]
                {
                    new dbus_union // root.DbusArray[0]
                    {
                        //Type = Udbus.Types.dbus_type.DBUS_DICT_BEGIN,
                        DbusDictionary = new Dictionary<dbus_primitive,dbus_union>
                        {  // root.DbusArray[0] Dictionary
                            { // root.DbusArray[0].DbusDictionary Entry 1
                                new dbus_union { /*Type = Udbus.Types.dbus_type.DBUS_INT32, */DbusInt32 = 1 }, // root.DbusArray[0].Key
                                new dbus_union // root.DbusArray[0].Value
                                {   //Type = Udbus.Types.dbus_type.DBUS_ARRAY, // root.DbusArray[0].Value Array
                                    DbusArray = new dbus_union[]
                                    {
                                        new dbus_union
                                        {   //Type = Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN, // root.DbusArray[0].Value Array[0].Struct
                                            DbusStruct = new dbus_union[] // root.DbusArray[0].Value Array[0].Struct.Fields
                                            {
                                                new dbus_union // root.DbusArray[0].Value Array[0].Struct[0]
                                                {
                                                    //Type = Udbus.Types.dbus_type.DBUS_ARRAY,
                                                    DbusArray = new dbus_union[] // root.DbusArray[0].Value Array[0].Struct[0].Array
                                                    {
                                                        new dbus_union { /*Type = Udbus.Types.dbus_type.DBUS_STRING, */DbusString = "One" },
                                                        new dbus_union { /*Type = Udbus.Types.dbus_type.DBUS_STRING, */DbusString = "Two" }
                                                    } // root.DbusArray[0].Value Array[0].Struct[0].Array
                                                }, // Ends root.DbusArray[0].Value Array[0].Struct[0]
                                                new dbus_union // root.DbusArray[0].Value Array[0].Struct[1]
                                                {
                                                    //Type = Udbus.Types.dbus_type.DBUS_DOUBLE,
                                                    DbusDouble = 1.2
                                                } // Ends root.DbusArray[0].Value Array[0].Struct[1]
                                            } // Ends root.DbusArray[0].Value Array[0].Struct.Fields
                                        } // Ends root.DbusArray[0].Value Array[0].Struct
                                    } // // root.DbusArray[0].Value Array
                                } // root.DbusArray[0].Value
                            } // Ends root.DbusArray[0].DbusDictionary Entry 1
                        } // Ends root.DbusArray[0] Dictionary
                    } // Ends root.DbusArray[0]
                } // Ends DbusArray
#endif // MANUALLABOUR2
            }; // Ends root
#endif // MANUALLABOUR1 || MANUALLABOUR2
        }
    } // Ends class Example
} // Ends Udbus.Containers
