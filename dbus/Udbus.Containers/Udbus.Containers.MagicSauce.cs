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

#define _MAGIC

// Auto-generated, so wouldn't recommending editing by hand...
#if _MAGIC
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Udbus.Containers
{

    //[StructLayout(LayoutKind.Explicit)]
    public partial class dbus_primitive
    {
        #region Properties
        public Udbus.Types.UdbusObjectPath DbusObjectPath
        {
            get
            {
                if (!this.IsDbusObjectPath) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_OBJECTPATH);

                } // Ends if incorrect type
  
                return this.dbusObjectPath;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_OBJECTPATH;
              this.dbusObjectPath = value;
              this.dbusValue = value;
            }
        }

        public bool DbusBoolean
        {
            get
            {
                if (!this.IsDbusBoolean) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_BOOLEAN);

                } // Ends if incorrect type
  
                return this.dbusBoolean;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_BOOLEAN;
              this.dbusBoolean = value;
              this.dbusValue = value;
            }
        }

        public byte DbusByte
        {
            get
            {
                if (!this.IsDbusByte) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_BYTE);

                } // Ends if incorrect type
  
                return this.dbusByte;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_BYTE;
              this.dbusByte = value;
              this.dbusValue = value;
            }
        }

        public string DbusString
        {
            get
            {
                if (!this.IsDbusString) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_STRING);

                } // Ends if incorrect type
  
                return this.dbusString;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_STRING;
              this.dbusString = value;
              this.dbusValue = value;
            }
        }

        public System.Int16 DbusInt16
        {
            get
            {
                if (!this.IsDbusInt16) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_INT16);

                } // Ends if incorrect type
  
                return this.dbusInt16;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_INT16;
              this.dbusInt16 = value;
              this.dbusValue = value;
            }
        }

        public System.UInt16 DbusUInt16
        {
            get
            {
                if (!this.IsDbusUInt16) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_UINT16);

                } // Ends if incorrect type
  
                return this.dbusUInt16;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_UINT16;
              this.dbusUInt16 = value;
              this.dbusValue = value;
            }
        }

        public System.Int32 DbusInt32
        {
            get
            {
                if (!this.IsDbusInt32) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_INT32);

                } // Ends if incorrect type
  
                return this.dbusInt32;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_INT32;
              this.dbusInt32 = value;
              this.dbusValue = value;
            }
        }

        public System.UInt32 DbusUInt32
        {
            get
            {
                if (!this.IsDbusUInt32) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_UINT32);

                } // Ends if incorrect type
  
                return this.dbusUInt32;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_UINT32;
              this.dbusUInt32 = value;
              this.dbusValue = value;
            }
        }

        public System.Int64 DbusInt64
        {
            get
            {
                if (!this.IsDbusInt64) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_INT64);

                } // Ends if incorrect type
  
                return this.dbusInt64;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_INT64;
              this.dbusInt64 = value;
              this.dbusValue = value;
            }
        }

        public System.UInt64 DbusUInt64
        {
            get
            {
                if (!this.IsDbusUInt64) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_UINT64);

                } // Ends if incorrect type
  
                return this.dbusUInt64;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_UINT64;
              this.dbusUInt64 = value;
              this.dbusValue = value;
            }
        }

        public double DbusDouble
        {
            get
            {
                if (!this.IsDbusDouble) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_DOUBLE);

                } // Ends if incorrect type
  
                return this.dbusDouble;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_DOUBLE;
              this.dbusDouble = value;
              this.dbusValue = value;
            }
        }

        public dbus_invalid DbusInvalid
        {
            get
            {
                if (!this.IsDbusInvalid) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_INVALID);

                } // Ends if incorrect type
  
                return this.dbusInvalid;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_INVALID;
              this.dbusInvalid = value;
              this.dbusValue = value;
            }
        }

        #endregion // Properties
        #region Constructors
        protected dbus_primitive(Udbus.Types.UdbusObjectPath value)    { this.DbusObjectPath = value; }
        protected dbus_primitive(bool value)    { this.DbusBoolean = value; }
        protected dbus_primitive(byte value)    { this.DbusByte = value; }
        protected dbus_primitive(System.Int16 value)    { this.DbusInt16 = value; }
        protected dbus_primitive(System.UInt16 value)    { this.DbusUInt16 = value; }
        protected dbus_primitive(System.Int32 value)    { this.DbusInt32 = value; }
        protected dbus_primitive(System.UInt32 value)    { this.DbusUInt32 = value; }
        protected dbus_primitive(System.Int64 value)    { this.DbusInt64 = value; }
        protected dbus_primitive(System.UInt64 value)    { this.DbusUInt64 = value; }
        protected dbus_primitive(double value)    { this.DbusDouble = value; }
        protected dbus_primitive(dbus_invalid value)    { this.DbusInvalid = value; }
        #endregion // Constructors
        #region Static creation
        static public dbus_primitive CreateObjectPath(Udbus.Types.UdbusObjectPath value)  { return new dbus_primitive { DbusObjectPath = value }; }
        static public dbus_primitive Create(bool value)  { return new dbus_primitive { DbusBoolean = value }; }
        static public dbus_primitive Create(byte value)  { return new dbus_primitive { DbusByte = value }; }
        static public dbus_primitive Create(string value)  { return new dbus_primitive { DbusString = value }; }
        static public dbus_primitive Create(System.Int16 value)  { return new dbus_primitive { DbusInt16 = value }; }
        static public dbus_primitive Create(System.UInt16 value)  { return new dbus_primitive { DbusUInt16 = value }; }
        static public dbus_primitive Create(System.Int32 value)  { return new dbus_primitive { DbusInt32 = value }; }
        static public dbus_primitive Create(System.UInt32 value)  { return new dbus_primitive { DbusUInt32 = value }; }
        static public dbus_primitive Create(System.Int64 value)  { return new dbus_primitive { DbusInt64 = value }; }
        static public dbus_primitive Create(System.UInt64 value)  { return new dbus_primitive { DbusUInt64 = value }; }
        static public dbus_primitive Create(double value)  { return new dbus_primitive { DbusDouble = value }; }
        static public dbus_primitive Create(dbus_invalid value)  { return new dbus_primitive { DbusInvalid = value }; }
        #endregion // Static creation
        #region Cast operators
        public static explicit operator Udbus.Types.UdbusObjectPath(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_OBJECTPATH) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_OBJECTPATH);

            } // Ends if wrong type

            return cast.dbusObjectPath;
        }

        public static explicit operator bool(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_BOOLEAN) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_BOOLEAN);

            } // Ends if wrong type

            return cast.dbusBoolean;
        }

        public static explicit operator byte(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_BYTE) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_BYTE);

            } // Ends if wrong type

            return cast.dbusByte;
        }

        public static explicit operator System.Int16(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_INT16) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_INT16);

            } // Ends if wrong type

            return cast.dbusInt16;
        }

        public static explicit operator System.UInt16(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_UINT16) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_UINT16);

            } // Ends if wrong type

            return cast.dbusUInt16;
        }

        public static explicit operator System.Int32(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_INT32) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_INT32);

            } // Ends if wrong type

            return cast.dbusInt32;
        }

        public static explicit operator System.UInt32(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_UINT32) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_UINT32);

            } // Ends if wrong type

            return cast.dbusUInt32;
        }

        public static explicit operator System.Int64(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_INT64) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_INT64);

            } // Ends if wrong type

            return cast.dbusInt64;
        }

        public static explicit operator System.UInt64(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_UINT64) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_UINT64);

            } // Ends if wrong type

            return cast.dbusUInt64;
        }

        public static explicit operator double(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_DOUBLE) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_DOUBLE);

            } // Ends if wrong type

            return cast.dbusDouble;
        }

        public static explicit operator dbus_invalid(dbus_primitive cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_INVALID) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_INVALID);

            } // Ends if wrong type

            return cast.dbusInvalid;
        }

          public static implicit operator string(dbus_primitive cast)
          {
              string result;
  
              if (cast.type == Udbus.Types.dbus_type.DBUS_STRING)
              {
                  result = cast.dbusString;
  
              }
              else if (cast.type == Udbus.Types.dbus_type.DBUS_OBJECTPATH)
              {
                  result = cast.dbusObjectPath.Path;
  
              }
              else // Else wrong type
              {
                  throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_STRING);
  
              } // Ends else wrong type
  
              return cast.dbusString;
          }
  
        #endregion // Cast operators
        #region Type check properties
        public bool IsDbusObjectPath { get { return this.type == Udbus.Types.dbus_type.DBUS_OBJECTPATH; } }
        public bool IsDbusBoolean { get { return this.type == Udbus.Types.dbus_type.DBUS_BOOLEAN; } }
        public bool IsDbusByte { get { return this.type == Udbus.Types.dbus_type.DBUS_BYTE; } }
        public bool IsDbusString { get { return this.type == Udbus.Types.dbus_type.DBUS_STRING; } }
        public bool IsDbusInt16 { get { return this.type == Udbus.Types.dbus_type.DBUS_INT16; } }
        public bool IsDbusUInt16 { get { return this.type == Udbus.Types.dbus_type.DBUS_UINT16; } }
        public bool IsDbusInt32 { get { return this.type == Udbus.Types.dbus_type.DBUS_INT32; } }
        public bool IsDbusUInt32 { get { return this.type == Udbus.Types.dbus_type.DBUS_UINT32; } }
        public bool IsDbusInt64 { get { return this.type == Udbus.Types.dbus_type.DBUS_INT64; } }
        public bool IsDbusUInt64 { get { return this.type == Udbus.Types.dbus_type.DBUS_UINT64; } }
        public bool IsDbusDouble { get { return this.type == Udbus.Types.dbus_type.DBUS_DOUBLE; } }
        public bool IsDbusInvalid { get { return this.type == Udbus.Types.dbus_type.DBUS_INVALID; } }
        #endregion // Type check properties

        public virtual string ValueAsString()
        {
            string value;
            
              switch (this.Type)
              {

                case Udbus.Types.dbus_type.DBUS_OBJECTPATH:
                    value = "'" + this.DbusObjectPath + "'";
                    break;

                case Udbus.Types.dbus_type.DBUS_BOOLEAN:
                    value = this.DbusBoolean.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_BYTE:
                    value = this.DbusByte.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_STRING:
                    value = "'" + this.DbusString + "'";
                    break;

                case Udbus.Types.dbus_type.DBUS_INT16:
                    value = this.DbusInt16.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_UINT16:
                    value = this.DbusUInt16.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_INT32:
                    value = this.DbusInt32.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_UINT32:
                    value = this.DbusUInt32.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_INT64:
                    value = this.DbusInt64.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_UINT64:
                    value = this.DbusUInt64.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_DOUBLE:
                    value = this.DbusDouble.ToString();
                    break;

                case Udbus.Types.dbus_type.DBUS_INVALID:
                    value = "<Invalid>";
                    break;

                  default:
                      value = "<unknown>";
                      break;
              } // Ends switch type
            return value;
        }

    } // Ends class dbus_primitive

    //[StructLayout(LayoutKind.Explicit)]
    public partial class dbus_union : dbus_primitive
    {
        #region Properties
        public Udbus.Types.dbus_sig DbusSignature
        {
            get
            {
                if (!this.IsDbusSignature) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_SIGNATURE);

                } // Ends if incorrect type
  
                return this.dbusSignature;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_SIGNATURE;
              this.dbusSignature = value;
              this.dbusValue = value;
            }
        }

        public Udbus.Containers.dbus_union DbusVariant
        {
            get
            {
                if (!this.IsDbusVariant) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_VARIANT);

                } // Ends if incorrect type
  
                return this.dbusVariant;
            }
  
            set
            {
              this.Type = Udbus.Types.dbus_type.DBUS_VARIANT;
              this.dbusVariant = value;
              this.dbusValue = value;
            }
        }

        public Dictionary<object, object> DbusDictionary
        {
            get
            {
                if (!this.IsDbusDictionary) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_DICT_BEGIN);

                } // Ends if incorrect type
  
                return this.dbusDictionary;
            }
        }

        public object[] DbusObjectArray
        {
            get
            {
                if (!this.IsDbusObjectArray) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_ARRAY);

                } // Ends if incorrect type
  
                return this.dbusObjectArray;
            }
        }

        public object[] DbusObjectStruct
        {
            get
            {
                if (!this.IsDbusObjectStruct) // If incorrect type
                {
                    throw this.CreateCastException(Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN);

                } // Ends if incorrect type
  
                return this.dbusObjectStruct;
            }
        }

        #endregion // Properties
        #region Constructors
        protected dbus_union(Udbus.Types.UdbusObjectPath value)    { this.DbusObjectPath = value; }
        protected dbus_union(bool value)    { this.DbusBoolean = value; }
        protected dbus_union(byte value)    { this.DbusByte = value; }
        protected dbus_union(System.Int16 value)    { this.DbusInt16 = value; }
        protected dbus_union(System.UInt16 value)    { this.DbusUInt16 = value; }
        protected dbus_union(System.Int32 value)    { this.DbusInt32 = value; }
        protected dbus_union(System.UInt32 value)    { this.DbusUInt32 = value; }
        protected dbus_union(System.Int64 value)    { this.DbusInt64 = value; }
        protected dbus_union(System.UInt64 value)    { this.DbusUInt64 = value; }
        protected dbus_union(double value)    { this.DbusDouble = value; }
        protected dbus_union(dbus_invalid value)    { this.DbusInvalid = value; }
        protected dbus_union(Udbus.Types.dbus_sig value)    { this.DbusSignature = value; }
        protected dbus_union(Udbus.Containers.dbus_union value)    { this.DbusVariant = value; }
        #endregion // Constructors
        #region Static creation
        new static public dbus_union CreateObjectPath(Udbus.Types.UdbusObjectPath value)  { return new dbus_union { DbusObjectPath = value }; }
        new static public dbus_union Create(bool value)  { return new dbus_union { DbusBoolean = value }; }
        new static public dbus_union Create(byte value)  { return new dbus_union { DbusByte = value }; }
        new static public dbus_union Create(string value)  { return new dbus_union { DbusString = value }; }
        new static public dbus_union Create(System.Int16 value)  { return new dbus_union { DbusInt16 = value }; }
        new static public dbus_union Create(System.UInt16 value)  { return new dbus_union { DbusUInt16 = value }; }
        new static public dbus_union Create(System.Int32 value)  { return new dbus_union { DbusInt32 = value }; }
        new static public dbus_union Create(System.UInt32 value)  { return new dbus_union { DbusUInt32 = value }; }
        new static public dbus_union Create(System.Int64 value)  { return new dbus_union { DbusInt64 = value }; }
        new static public dbus_union Create(System.UInt64 value)  { return new dbus_union { DbusUInt64 = value }; }
        new static public dbus_union Create(double value)  { return new dbus_union { DbusDouble = value }; }
        new static public dbus_union Create(dbus_invalid value)  { return new dbus_union { DbusInvalid = value }; }
        static public dbus_union Create(Udbus.Containers.dbus_union value)  { return new dbus_union { DbusVariant = value }; }
        #endregion // Static creation
        #region Cast operators
        public static explicit operator Udbus.Types.dbus_sig(dbus_union cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_SIGNATURE) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_SIGNATURE);

            } // Ends if wrong type

            return cast.dbusSignature;
        }

        public static explicit operator Dictionary<object, object>(dbus_union cast)
        {
            if (cast.type != Udbus.Types.dbus_type.DBUS_DICT_BEGIN) // If wrong type
            {
                throw cast.CreateCastException(Udbus.Types.dbus_type.DBUS_DICT_BEGIN);

            } // Ends if wrong type

            return cast.dbusDictionary;
        }

        #endregion // Cast operators
        #region Type check properties
        public bool IsDbusSignature { get { return this.type == Udbus.Types.dbus_type.DBUS_SIGNATURE; } }
        public bool IsDbusObjectArray { get { return this.type == Udbus.Types.dbus_type.DBUS_ARRAY; } }
        public bool IsDbusObjectStruct { get { return this.type == Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN; } }
        public bool IsDbusVariant { get { return this.type == Udbus.Types.dbus_type.DBUS_VARIANT; } }
        #endregion // Type check properties

        public override string ValueAsString()
        {
            string value;
            
            if (this.IsDbusDictionary)
            {
                value = ObjectAsString(this.DbusDictionary);

            }
            else
            {
                switch (this.Type)
                {

                    case Udbus.Types.dbus_type.DBUS_SIGNATURE:
                        value = this.DbusSignature.ToString();
                        break;

                    case Udbus.Types.dbus_type.DBUS_ARRAY:
                        value = ObjectAsString(this.DbusObjectArray);
                        break;

                    case Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN:
                        value = ObjectAsString(this.DbusObjectStruct);
                        break;

                    case Udbus.Types.dbus_type.DBUS_VARIANT:
                        value = this.DbusVariant.ToString();
                        break;

                    default:
                        value = base.ValueAsString();
                        break;
                } // Ends switch type
            } // Ends if not dictionary
            return value;
        }

    } // Ends class dbus_union
} // Ends namespace Udbus.Containers
#endif // _MAGIC
