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

namespace dbusidltocode
{
    #region Types for XML projection
    /// <summary>
    /// Provides an object based representation of the XML based IDL interfaces
    /// </summary>
    public class IDLInterface
    {
        public string Name; //Name of the interface
        public string Path; //Path to the interface
        public List<IDLMethod> Methods; //Methods represented by this IDL
        public List<IDLSignal> Signals; //Signals represented by this IDL
        public List<IDLProperty> Properties; //Properties represented by this IDL

        /// <summary>
        /// Dumps a summary of this IDL object
        /// </summary>
        /// <param name="writer">Writer to output on</param>
        /// <param name="pad">Padding level</param>
        public virtual void DumpImpl(TextWriter writer)
        {
            string headerPad = " "; //Padding level for headings
            string callPad = headerPad + " "; //Padding level for next step down
            writer.WriteLine(string.Format("Interface {0}", this.Name));

            //Dump methods
            writer.WriteLine(string.Format("{0}Methods:", headerPad));
            if (this.Methods == null)
            {
                writer.WriteLine(string.Format("{0}No methods.", callPad));
            }
            else
            {
                foreach (IDLMethod method in this.Methods)
                {
                    method.Dump(writer, callPad);
                }
            }

            //Dump signals
            writer.WriteLine(string.Format("{0}Signals:", headerPad));
            if (this.Signals == null)
            {
                writer.WriteLine(string.Format("{0}No signals.", callPad));
            }
            else
            {
                foreach (IDLSignal signal in this.Signals)
                {
                    signal.Dump(writer, callPad);
                }
            }

            //Dump properties
            writer.WriteLine(string.Format("{0}Properties:", headerPad));
            if (this.Properties == null)
            {
                writer.WriteLine(string.Format("{0}No properties.", callPad));
            }
            else
            {
                foreach (IDLProperty property in this.Properties)
                {
                    property.Dump(writer, callPad);
                }
            }
        }
    } // Ends class IDLInterface

    #region Arguments
    /// <summary>
    /// Common base class for IDL arguments
    /// </summary>
    public class IDLArgument
    {
        public string Name;
        public string Type;

        /// <summary>
        /// Dump IDLArgument information
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="pad">Amount of padding to preappend to the line</param>
        public virtual void Dump(TextWriter writer, string pad)
        {
            writer.WriteLine("{0}Arg: {1} {2}", pad, this.Type, this.Name);
        }
    }

    /// <summary>
    /// Signals's argument description:
    ///   * type
    ///   * name
    /// </summary>
    /// <remarks>Everything necessary is implemented via inheritance</remarks>
    public class IDLSignalArgument : IDLArgument
    {
    }

    /// <summary>
    /// Method's argument description:
    ///   * direction
    ///   * type
    ///   * name
    /// </summary>
    public class IDLMethodArgument : IDLArgument
    {
        public string Direction; //Input or return value

        /// <summary>
        /// Dump IDLArgument information
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="pad">Amount of padding to preappend to the line</param>
        new public virtual void Dump(TextWriter writer, string pad)
        {
            writer.WriteLine("{0}Arg: {1} {2} {3}", pad, this.Direction, this.Type, this.Name);
        }
    }
    #endregion Arguments

    /// <summary>
    /// Signal Description
    /// </summary>
    public class IDLSignal
    {
        public string Name;
        public List<IDLSignalArgument> Arguments;

        /// <summary>
        /// Dump signal information
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="pad">Amount of padding to preappend to the line</param>
        public virtual void Dump(TextWriter writer, string pad)
        {
            string headerPad = pad + " "; //Padding level for headings
            string callPad = headerPad + " "; //Padding level for next step down

            writer.WriteLine(string.Format("{0}Signal: {1}", pad, this.Name));
            writer.WriteLine(string.Format("{0}Arguments:", headerPad));

            if (this.Arguments == null)
            {
                writer.WriteLine(string.Format("{0}No arguments", callPad));
            }
            else
            {
                foreach (IDLSignalArgument arg in this.Arguments)
                {
                    arg.Dump(writer, callPad);
                }
            }
        }
    }

    #region Method

    /// <summary>
    /// The method's return type
    /// </summary>
    public class IDLMethodReturn
    {
        public string Type;

        public virtual void Dump(TextWriter writer, string pad)
        {
            writer.WriteLine("{0}Return: {1}", pad, this.Type);
        }
    }

    /// <summary>
    /// Method description:
    /// a method name, a list of arguments and its return value
    /// </summary>
    public class IDLMethod
    {
        public string Name; //Method name
        public List<IDLMethodArgument> Arguments; //List of method arguments
        public IDLMethodReturn Return = null; //TODO This looks like planned/half finished code, return reasonable return types...

        /// <summary>
        /// Dump out method information
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="pad">Amount of indentation to preappend</param>
        public virtual void Dump(TextWriter writer, string pad)
        {
            string headerPad = pad + " "; //Padding level for headings
            string callPad = headerPad + " "; //Padding level for next step down

            writer.WriteLine(string.Format("{0}Method: {1}", pad, this.Name));
            writer.WriteLine(string.Format("{0}Arguments:", headerPad));

            if (this.Arguments == null)
            {
                writer.WriteLine(string.Format("{0}No arguments", callPad));
            }
            else
            {
                foreach (IDLMethodArgument arg in this.Arguments)
                {
                    arg.Dump(writer, callPad);
                }
            }
        }
    }
    #endregion 

    public class IDLProperty
    {
        public string Access; //Property access
        public string Type; //Property type
        public string Name; //Property name
        
        /// <summary>
        /// Arguments are always the same.
        /// They are setted by default and will be used by the code builder
        /// </summary>
        public List<IDLMethodArgument> Arguments {
            get
            {
                return new List<IDLMethodArgument>(new IDLMethodArgument[]
                    {
                        new IDLMethodArgument{ Name="interface_name", Type="s", Direction="in" },
                        new IDLMethodArgument{ Name="property_name", Type="s", Direction="in" },
                        new IDLMethodArgument{ Name="value", Type=this.Type, Direction="out"}
                    }
                );
            }
        }

        /// <summary>
        /// Dumps out a property's information
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="pad">Amount of indentation to preappend</param>
        public virtual void Dump(TextWriter writer, string pad)
        {
            writer.WriteLine("{0}Property: {1} {2} {3}", pad, this.Access, this.Type, this.Name);
        }
    } // Ends class IDLProperty
    
    #endregion // Ends Types for XML projection

}
