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

#define _NASTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace Udbus.Types
{

    #region Nasty struct crap

#   if _NASTY
    //public enum dbus_type
    //{
    //    DBUS_SIGNATURE,
    //    DBUS_OBJECTPATH,
    //    DBUS_BOOLEAN,
    //    DBUS_BYTE,
    //    DBUS_STRING,
    //    DBUS_INT16,
    //    DBUS_UINT16,
    //    DBUS_INT32,
    //    DBUS_UINT32,
    //    DBUS_INT64,
    //    DBUS_UINT64,
    //    DBUS_DOUBLE,
    //    DBUS_ARRAY,
    //    // The order of these struct/dict enumerations is significant,
    //    // since asking enum to convert to string will produce struct
    //    // names either way, which is what the spec expects.
    //    // http://dbus.freedesktop.org/doc/dbus-specification.html#message-protocol-marshaling
    //    DBUS_STRUCT_BEGIN,
    //    DBUS_DICT_END,
    //    DBUS_DICT_BEGIN = DBUS_STRUCT_BEGIN,
    //    DBUS_STRUCT_END = DBUS_DICT_END,
    //    DBUS_INVALID = -1,
    //};

    //[StructLayout(LayoutKind.Sequential)]
    //public struct dbus_sig
    //{
    //    const int SizeA = 256;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeA)]
    //    private dbus_type [] a_;

    //    public dbus_type[] a
    //    {
    //        get { return this.a_; }
    //        set
    //        {
    //            if (this.a_ == null) // If haven't initialised yet
    //            {
    //                this.a_ = new dbus_type[SizeA];

    //            } // Ends if haven't initialised yet

    //            if (value == null) // If no value
    //            {
    //                this.a_[0] = dbus_type.DBUS_INVALID;

    //            } // Ends if no value
    //            else if (value.Length == 0) // Else if empty value
    //            {
    //                this.a_[0] = dbus_type.DBUS_INVALID;

    //            } // Ends else if empty value
    //            else // Else value ok
    //            {
    //                value.CopyTo(this.a_, 0);

    //                if (value.Length < SizeA && value[value.Length - 1] != dbus_type.DBUS_INVALID) // If not terminated
    //                {
    //                    this.a_[value.Length - 1] = dbus_type.DBUS_INVALID;

    //                } // Ends if not terminated
    //            } // Ends else value ok
    //        }
    //    }
    //    static public dbus_sig Initialiser
    //    {
    //        get
    //        {
    //            dbus_sig init = default(dbus_sig);
    //            //System.Reflection.MemberInfo infoA = typeof(dbus_sig).GetMember("a", System.Reflection.BindingFlags.ExactBinding)[0];
    //            //MarshalAsAttribute attributeA = (MarshalAsAttribute)MarshalAsAttribute.GetCustomAttribute(infoA, typeof(MarshalAsAttribute), false);
    //            //init.a = new dbus_type[attributeA.SizeConst];

    //            init.a_ = new dbus_type[SizeA];
    //            init.a_[0] = dbus_type.DBUS_INVALID;
    //            init.a_[1] = dbus_type.DBUS_INVALID;
    //            return init;
    //        }
    //    }

    //    static public string ToString(dbus_type[] types)
    //    {
    //        string result;
    //        if (types == null || types.Length == 0 || types[0] == dbus_type.DBUS_INVALID) // If got no entries
    //        {
    //            result = "Length=0";

    //        } // Ends if got no entries
    //        else // Else got entries
    //        {
    //            StringBuilder content = new StringBuilder();
    //            content.Append(types[0].ToString());
    //            uint counter = 1;

    //            if (types[1] != dbus_type.DBUS_INVALID) // If more than one entry
    //            {
    //                for (; counter < types.Length && types[counter] != dbus_type.DBUS_INVALID; ++counter)
    //                {
    //                    content.AppendFormat(", {0}", types[counter].ToString());

    //                }
    //            } // Ends if more than one entry

    //            result = string.Format("Length={0} [{1}]", counter, content.ToString());

    //        } // Ends else got entries

    //        return result;
    //    }

    //    static public dbus_type[] TruncatedCopy(dbus_type[] types)
    //    {
    //        dbus_type[] result = types;

    //        if (types != null && types.Length > 0) // If got some types
    //        {
    //            int counter = 0;

    //            for (; counter < types.Length; ++counter)
    //            {
    //                if (types[counter] == dbus_type.DBUS_INVALID) // If found truncating entry
    //                {
    //                    break;

    //                } // Ends if found truncating entry

    //            } // Ends loop over original array

    //            if (counter != types.Length) // If original array is terminated
    //            {
    //                ++counter; // Include the terminator.
    //                result = new dbus_type[counter];
    //                Array.Copy(types, result, counter);

    //            } // Ends if original array is terminated
    //        } // Ends if got some types


    //        return result;
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder result = new StringBuilder(base.ToString());
    //        result.Append(" ");
    //        result.Append(ToString(this.a_));
    //        return result.ToString();
    //    }
    //} // Ends struct dbus_sig

    // Not actually referenced in code, so here for completeness.
    public enum dbus_field_type
    {
        DBUS_FIELD_INVALID = 0,
        DBUS_FIELD_PATH = 1,
        DBUS_FIELD_INTERFACE = 2,
        DBUS_FIELD_MEMBER = 3,
        DBUS_FIELD_ERROR_NAME = 4,
        DBUS_FIELD_REPLY_SERIAL = 5,
        DBUS_FIELD_DESTINATION = 6,
        DBUS_FIELD_SENDER = 7,
        DBUS_FIELD_SIGNATURE = 8,
        DBUS_FIELD_UNIX_FDS = 9,
    };

    public enum dbus_msg_type : byte
    {
        DBUS_TYPE_INVALID = 0,
        DBUS_TYPE_METHOD_CALL = 1,
        DBUS_TYPE_METHOD_RETURN = 2,
        DBUS_TYPE_ERROR = 3,
        DBUS_TYPE_SIGNAL = 4,
    } ;

#   endif // _NASTY
    #endregion // Nasty struct crap

} // Ends namespace Udbus.Serialization
