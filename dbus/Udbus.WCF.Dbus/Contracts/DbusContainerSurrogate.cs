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
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Udbus.WCF.Dbus.Contracts
{
    #region Surrogate code
    #region Surrogate class
    [DataContract(Name = "dbus_union", Namespace = Constants.UnionNamespace)]
    [KnownType(typeof(Dictionary<object, object>))]
    [KnownType(typeof(object[]))]
    public class DbusUnionSurrogate
    {
        [DataMember]
        public Udbus.Types.dbus_type[] unionTypes;

        [DataMember]
        public object data;

    } // Ends class DbusUnionSurrogate
    #endregion // Surrogate class

    #region Surrogate substitution class
    public class DbusContainerSurrogate : IDataContractSurrogate
    {
        #region IDataContractSurrogate Members

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            // Not used.
            return null;
        }

        public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
        {
            // Not used.
            return null;
        }

        public Type GetDataContractType(Type type)
        {
            Type result;
            if (typeof(Udbus.Containers.dbus_union).IsAssignableFrom(type))
            {
                result = typeof(DbusUnionSurrogate);
            }
            else
            {
                result = type;
            }

            return result;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            object result;
            if (obj is DbusUnionSurrogate)
            {
                DbusUnionSurrogate unionSurrogate = (DbusUnionSurrogate)obj;
                Udbus.Containers.dbus_union union = new Udbus.Containers.dbus_union();
                switch (unionSurrogate.unionTypes[0])
                {
                    case Udbus.Types.dbus_type.DBUS_BOOLEAN:
                        union.DbusBoolean = (bool)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_BYTE:
                        union.DbusByte = (byte)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_OBJECTPATH:
                        union.DbusObjectPath = new Udbus.Types.UdbusObjectPath((string)unionSurrogate.data);
                        break;

                    case Udbus.Types.dbus_type.DBUS_SIGNATURE:
                        union.DbusSignature = (Udbus.Types.dbus_sig)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_STRING:
                        union.DbusString = (string)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INT16:
                        union.DbusInt16 = (Int16)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_UINT16:
                        union.DbusUInt16 = (UInt16)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INT32:
                        union.DbusInt32 = (Int32)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_UINT32:
                        union.DbusUInt32 = (UInt32)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INT64:
                        union.DbusInt64 = (Int64)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_UINT64:
                        union.DbusUInt64 = (UInt64)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_DOUBLE:
                        union.DbusDouble = (double)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_ARRAY:
                        union.SetDbusObjectArray((object[])unionSurrogate.data, unionSurrogate.unionTypes);
                        break;

                    case Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN:
                        union.SetDbusObjectStruct((object[])unionSurrogate.data, unionSurrogate.unionTypes);
                        break;

                    case Udbus.Types.dbus_type.DBUS_VARIANT:
                        union.DbusVariant = (Udbus.Containers.dbus_union)unionSurrogate.data;
                        break;

                    case Udbus.Types.dbus_type.DBUS_DICT_BEGIN:
                        union.SetDbusDictionary((Dictionary<object, object>)unionSurrogate.data, unionSurrogate.unionTypes);
                        break;

                    case Udbus.Types.dbus_type.DBUS_INVALID:
                        break;

                    default:
                        throw new Exception(string.Format("Unkonwn dbus_type: {0}", unionSurrogate.unionTypes.ToString()));
                }
                result = union;
            }
            else
            {
                result = obj;
            }
            return result;
        }

        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
        {
            // Not used.
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            object result;
            if (obj is Udbus.Containers.dbus_union)
            {
                Udbus.Containers.dbus_union union = (Udbus.Containers.dbus_union)obj;
                DbusUnionSurrogate unionSurrogate = new DbusUnionSurrogate();
                unionSurrogate.unionTypes = Udbus.Types.dbus_sig.TruncatedCopy(union.Types);
                switch (union.Type)
                {
                    case Udbus.Types.dbus_type.DBUS_BOOLEAN:
                        unionSurrogate.data = union.DbusBoolean;
                        break;

                    case Udbus.Types.dbus_type.DBUS_BYTE:
                        unionSurrogate.data = union.DbusByte;
                        break;

                    case Udbus.Types.dbus_type.DBUS_OBJECTPATH:
                        unionSurrogate.data = union.DbusObjectPath.Path;
                        break;

                    case Udbus.Types.dbus_type.DBUS_SIGNATURE:
                        unionSurrogate.data = union.DbusSignature;
                        break;

                    case Udbus.Types.dbus_type.DBUS_STRING:
                        unionSurrogate.data = union.DbusString;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INT16:
                        unionSurrogate.data = union.DbusInt16;
                        break;

                    case Udbus.Types.dbus_type.DBUS_UINT16:
                        unionSurrogate.data = union.DbusUInt16;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INT32:
                        unionSurrogate.data = union.DbusInt32;
                        break;

                    case Udbus.Types.dbus_type.DBUS_UINT32:
                        unionSurrogate.data = union.DbusUInt32;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INT64:
                        unionSurrogate.data = union.DbusInt64;
                        break;

                    case Udbus.Types.dbus_type.DBUS_UINT64:
                        unionSurrogate.data = union.DbusUInt64;
                        break;

                    case Udbus.Types.dbus_type.DBUS_DOUBLE:
                        unionSurrogate.data = union.DbusDouble;
                        break;

                    case Udbus.Types.dbus_type.DBUS_ARRAY:
                        unionSurrogate.data = union.DbusObjectArray;
                        break;

                    case Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN:
                        unionSurrogate.data = union.DbusObjectStruct;
                        break;

                    case Udbus.Types.dbus_type.DBUS_VARIANT:
                        unionSurrogate.data = union.DbusVariant;
                        break;

                    case Udbus.Types.dbus_type.DBUS_DICT_BEGIN:
                        unionSurrogate.data = union.DbusDictionary;
                        break;

                    case Udbus.Types.dbus_type.DBUS_INVALID:
                        break;

                    default:
                        throw new Exception(string.Format("Unkonwn dbus_type for surrogate: {0}", union.Type.ToString()));
                }
                result = unionSurrogate;
            }
            else
            {
                result = obj;
            }
            return result;
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            Type result = null;

            if (typeNamespace.Equals(Constants.UnionNamespace))
            {
                if (typeName.Equals("DbusUnionSurrogate"))
                {
                    result = typeof(DbusUnionSurrogate);
                }
            }

            return result;
        }

        public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
        {
            // Not used.
            return typeDeclaration;
        }

        #endregion
    } // Ends class DbusContainerSurrogate
    #endregion // Surrogate substitution class

    #endregion // Surrogate code
} // Ends namespace Udbus.WCF.Dbus.Contracts
