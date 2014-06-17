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
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.IO;

using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;

namespace dbusidltocode
{
    class ArgHolder
    {
        private string _arg;
        public string Arg
        {
            get
            {
                return this._arg;
            }
            set
            {
                this._arg = value;
            }
        }

        public ArgHolder()
            : this(null)
        {
        }

        public ArgHolder(string arg)
        {
            this.Arg = arg;
        }
    } // Ends struct ArgHolder

    public struct TextWriters
    {
        public TextWriter writerInterface;
        public TextWriter writerService;
        public TextWriter writerProxy;
        public TextWriter writerWCFContracts;
        public TextWriter writerWCFService;
        public TextWriter writerWCFHost;

        /// <summary>
        /// Generate the text writers provided to this collection struct.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TextWriter> GenTextWriters()
        {
            if (this.writerInterface != null) yield return this.writerInterface;
            if (this.writerService != null) yield return this.writerService;
            if (this.writerProxy != null) yield return this.writerProxy;
            if (this.writerWCFContracts != null) yield return this.writerWCFContracts;
            if (this.writerWCFService != null) yield return this.writerWCFService;
            if (this.writerWCFHost != null) yield return this.writerWCFHost;
        }

        public void Close()
        {
            foreach (TextWriter textwriter in this.GenTextWriters())
            {
                textwriter.Close();
            }
        }
    } // Ends struct TextWriters

    /// <summary>
    /// Class to process IDL objects and drive code generation
    /// </summary>
    internal class IDLProcessor
    {
        /// <summary>
        /// Do the actual transformation XML to useful classes.
        /// </summary>
        /// <param name="xInterfaces">Interface nodes in XML document.</param>
        /// <returns>Fully populated interface instances.</returns>
        public static IEnumerable<IDLInterface> Transform(XElement xInterfaces)
        {
            int nullCount = 0;
            var vCollection =
                (from i in xInterfaces.Elements("interface") //For every <interface> under <node>
                 select new IDLInterface
                 {
                     Name = (string)i.Attribute("name"), //<interface name="">
                     Path = (string)xInterfaces.Attribute("name") ?? ((string)i.Attribute("name")).Replace('.', '/'), //<node name=""> or if NULL <node><interface name"">.Replace('.', '/')
                     Methods =
                         (from m in i.Elements("method")//For every <method> under <interface>
                          select new IDLMethod
                          {
                              Name = (string)m.Attribute("name"),//<method name="">
                              Arguments =
                                  (from a in m.Elements("arg")//For every <arg> in <method>
                                   select new IDLMethodArgument
                                   {
                                       Name = (string)a.Attribute("name") == string.Empty && (string)a.Attribute("direction") == "out" ?
                                               String.Format("result_{0}", nullCount++) : // Presume that empty out params are results, hence format them as "result_{nullCount}"
                                               (string)a.Attribute("name"), //<arg name="">
                                       Direction = (string)a.Attribute("direction"),//<arg direction="">
                                       Type = (string)a.Attribute("type") //<arg type="">
                                   }
                                  ).ToList() //End of for <arg> in <method>
                          }).ToList(), //End of for <method> in <interface>
                     Signals =
                         (from s in i.Elements("signal")//For every <signal> in <interface>
                          select new IDLSignal
                          {
                              Name = (string)s.Attribute("name"),//<signal name="">
                              Arguments =
                                  (from a in s.Elements("arg") //For every <arg> in <signal>
                                   select new IDLSignalArgument
                                   {
                                       Name = (string)a.Attribute("name"), //<arg name="">
                                       Type = (string)a.Attribute("type") //<arg type="">
                                   }
                                   ).ToList() //End of for every <arg> in <signal>
                          }).ToList(), //End of for every <signal> in <arg>
                     Properties =
                         (from p in i.Elements("property")//For every <property> in <interface>
                          select new IDLProperty
                          {
                              Name = (string)p.Attribute("name"),//<property name="">
                              Type = (string)p.Attribute("type"),//<property type="">
                              Access = (string)p.Attribute("access")//<property access="">
                          }).ToList() //End of for every <property> in <interface>
                 }).ToList();//End of <interface>
            return vCollection;
        }

        /// <summary>
        /// Accepts IDL input and outputs code through the given text writers
        /// </summary>
        /// <param name="interfaces">Input IDL</param>
        /// <param name="textwriters">TextWriters struct for output</param>
        public static void Generate(IEnumerable<IDLInterface> interfaces, TextWriters textwriters)
        {
            if (textwriters.writerInterface != null)
            {
                new InterfaceBuilder().Generate(interfaces, textwriters.writerInterface);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Interface code generated, press enter to continue...");
                    Console.ReadLine();
                }
#endif // DEBUG
            }
            if (textwriters.writerProxy != null)
            {
                new ProxyBuilder().Generate(interfaces, textwriters.writerProxy);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Proxy code generated, press enter to continue...");
                    Console.ReadLine();
                }
#endif // DEBUG
            }
            if (textwriters.writerService != null)
            {
                new MarshalBuilder().Generate(interfaces, textwriters.writerService);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Marshal code generated, press enter to continue...");
                    Console.ReadLine();
                }
#endif // DEBUG
            }
            if (textwriters.writerWCFContracts != null)
            {
                new WCFContractBuilder().Generate(interfaces, textwriters.writerWCFContracts);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("WCF Contracts code generated, press enter to continue...");
                    Console.ReadLine();
                }
#endif // DEBUG
            }
            if (textwriters.writerWCFService != null)
            {
                // Non-passthrough version creates dbus instance at method invocation time
                //new WCFServiceBuilder().Generate(interfaces, textwriters.writerWCFService);
                new WCFPassthruServiceBuilder().Generate(interfaces, textwriters.writerWCFService);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("WCF Service code generated, press enter to continue...");
                    Console.ReadLine();
                }
#endif // DEBUG
            }
            if (textwriters.writerWCFHost != null)
            {
                // Non-passthrough version creates dbus instance at method invocation time
                //new WCFHostBuilder().Generate(interfaces, textwriters.writerWCFHost);
                new WCFPassthruHostBuilder().Generate(interfaces, textwriters.writerWCFHost);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("WCF Host code generated, press enter to continue...");
                    Console.ReadLine();
                }
#endif // DEBUG
           }
        }
    } // Ends class IDLProcessor

    /// <summary>
    /// The actual main program
    /// </summary>
    public class Program
    {
        //Default output file names
        const string DefaultInterfaceFile = "DbusIDLInterface.cs";
        const string DefaultProxyFile = "DbusIDLClientProxy.cs";
        const string DefaultImplementationFile = "DbusIDLImpl.cs";
        const string DefaultContractFile = "DbusWCFContract.cs";
        const string DefaultWCFServiceFile = "DbusWCFService.cs";
        const string DefaultHostFile = "DbusWCFHost.cs";

        /// <summary>
        /// Given an XML reader and a collection of text writers, generate the desired source
        /// </summary>
        /// <param name="readerIDL">Reader of input XML</param>
        /// <param name="textwriters">Writers for output</param>
        public static void BuildCodeFromDbusIDL(XmlReader readerIDL, TextWriters textwriters)
        {
            XDocument doc = XDocument.Load(readerIDL);
            IEnumerable<IDLInterface> idlInterfaces = IDLProcessor.Transform(doc.Element("node")); //Parse the XML into lists of objects
#if DEBUG
            foreach (IDLInterface IDLint in idlInterfaces)
            {
                IDLint.DumpImpl(Console.Out);
            }
#endif
            IDLProcessor.Generate(idlInterfaces, textwriters); //Output objects as source
        }

        /// <summary>
        /// Print out the usage information of dbusidltocode
        /// </summary>
        /// <remarks>
        /// Calls to this function will cause the application to terminate execution
        /// </remarks>
        static void Usage()
        {
            Console.WriteLine(string.Format(@"{0} - create code from DBUS IDL. Languages supported: C#.
If no output paths are specified, the defaults DbusIDLInterface.cs, DbusIDLClientProxy.cs, 
DbusIDLImpl.cs, DbusWCFContract.cs, DbusWCFService.cs and DbusWCFHost.cs will be used.
If a sub-set of output options are specified however, only these paths will be generated.

Parameters:
    /?                                  Show this help screen
    --path|-p <input_path>              Path to read Dbus IDL from. Typically an xml file.
    --interface|-i <interface_path>     Path to write interface write to.
    --service|-s <service_path>         Path to write service to.
    --proxy|-x <proxy_path>             Path to write client proxy to.
    --contract|-c <contract_path>       Path to write WCF contract to.
    --wcfservice|-w <wcfservice_path>   Path to write WCF service to.
    --host|-h <host_path>               Path to write WCF host to.
    --verbose|-v                        Show additional output information.

E.g.
    {0} --verbose --path D:\code\xsl\tests\test_TransformDBusIDL\dbus.xml
", System.AppDomain.CurrentDomain.FriendlyName
));
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            //Holds all of the argument paths we may get given
            ArgHolder pathArg = new ArgHolder();
            ArgHolder interfaceArg = new ArgHolder();
            ArgHolder serviceArg = new ArgHolder();
            ArgHolder proxyArg = new ArgHolder();
            ArgHolder contractArg = new ArgHolder();
            ArgHolder wcfserviceArg = new ArgHolder();
            ArgHolder hostArg = new ArgHolder();

            bool bVerbose = false; //Verbosity level
            bool bOutputSpecified = false; //Whether any output values have been set

            #region argParsing
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                ArgHolder holder = null;

                if (arg.StartsWith("--")) // If long param
                {
                    switch(arg)
                    {
                        case "--path":
                            holder = pathArg;
                            break;
                        case "--interface":
                            holder = interfaceArg;
                            break;
                        case "--service":
                            holder = serviceArg;
                            break;
                        case "--proxy":
                            holder = proxyArg;
                            break;
                        case "--contract":
                            holder = contractArg;
                            break;
                        case "--wcfservice":
                            holder = wcfserviceArg;
                            break;
                        case "--host":
                            holder = hostArg;
                            break;
                        case "--verbose":
                            bVerbose = true;
                            break;
                        default:
                            Console.Error.WriteLine(args[i] + " not a valid argument");
                            break;
                    }
                } // Ends if long param
                else if (arg.StartsWith("-")) // Else if short param
                {
                    switch(arg)
                    {
                        case "-p":
                            holder = pathArg;
                            break;
                        case "-i":
                            holder = interfaceArg;
                            break;
                        case "-s":
                            holder = serviceArg;
                            break;
                        case "-x":
                            holder = proxyArg;
                            break;
                        case "-c":
                            holder = contractArg;
                            break;
                        case "-w":
                            holder = wcfserviceArg;
                            break;
                        case "-h":
                            holder = hostArg;
                            break;
                        case "-v":
                            bVerbose = true;
                            break;
                        default:
                            Console.Error.WriteLine(args[i] + " not a valid argument");
                            break;
                    }
                } // Ends else if short param
                else
                {
                    if (arg == "/?")
                    {
                        Usage();
                    }
                    else
                    {
                        Console.Error.WriteLine(args[i] + " not a valid argument");
                    }
                }

                //If given a parameter that comes as two args, i.e. arg & path, consume an extra arg this loop
                if (holder != null)
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine(string.Format("Insufficient number of arguments provided."));
                        Usage();
                    }
                    holder.Arg = args[i];
                    if (holder != pathArg)
                    {
                        bOutputSpecified = true;
                    }
                }
            } // Ends loop over args
            #endregion argParsing

            // Check we were given an input file at least
            if (pathArg.Arg == null)
            {
                System.Console.Error.WriteLine("No input path specified");
                Usage();
            }

            // If no output parameters specified, just spit out all code to default files
            if (!bOutputSpecified)
            {
                interfaceArg.Arg = DefaultInterfaceFile;
                serviceArg.Arg = DefaultImplementationFile;
                proxyArg.Arg = DefaultProxyFile;
                contractArg.Arg = DefaultContractFile;
                wcfserviceArg.Arg = DefaultWCFServiceFile;
                hostArg.Arg = DefaultHostFile;
            } // Ends if no output parameters specified

            #region outputWriters
            // Initialise all writers to null
            TextWriter writerInterface = null;
            TextWriter writerService = null;
            TextWriter writerProxy = null;
            TextWriter writerContract = null;
            TextWriter writerWCFService = null;
            TextWriter writerHost = null;

            //Lets us print out info in exceptions if anything goes wrong
            string exceptionInfoString = "";
            string givenArg = "";
            //try-catch-bail
            try
            {
                //try-catch-info-throw
                try
                {
                    //Only open the files we've been told to care about
                    //i.e. We need to prevent any ArgumentNullExceptions as it's a valid use case
                    exceptionInfoString = "Service Interface";
                    givenArg = interfaceArg.Arg;
                    if (givenArg != null) writerInterface = new StreamWriter(givenArg);

                    exceptionInfoString = "Service Implementation";
                    givenArg = serviceArg.Arg;
                    if (givenArg != null) writerService = new StreamWriter(givenArg);

                    exceptionInfoString = "Client Proxy";
                    givenArg = proxyArg.Arg;
                    if (givenArg != null) writerProxy = new StreamWriter(givenArg);

                    exceptionInfoString = "WCF Contract";
                    givenArg = contractArg.Arg;
                    if (givenArg != null) writerContract = new StreamWriter(givenArg);

                    exceptionInfoString = "WCF Service";
                    givenArg = wcfserviceArg.Arg;
                    if (givenArg != null) writerWCFService = new StreamWriter(givenArg);

                    exceptionInfoString = "WCF Host";
                    givenArg = hostArg.Arg;
                    if (givenArg != null) writerHost = new StreamWriter(givenArg);
                }
                catch (System.ArgumentException sae)
                {
                    System.Console.Error.WriteLine("Bad output path {0} given for {1}", givenArg, exceptionInfoString);
                    throw sae;
                }
                catch (System.UnauthorizedAccessException uae)
                {
                    System.Console.Error.WriteLine("Unauthorised access to {0} given for {1}", givenArg, exceptionInfoString);
                    throw uae;
                }
                catch (System.IO.DirectoryNotFoundException dnfe)
                {
                    System.Console.Error.WriteLine("Invalid directory in path {0} given for {1}", givenArg, exceptionInfoString);
                    throw dnfe;
                }
                catch (System.IO.PathTooLongException ptle)
                {
                    System.Console.Error.WriteLine("The specified path, file name, or both in {0} given for {1}, exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.", givenArg, exceptionInfoString);
                    throw ptle;
                }
                catch (Exception ex)
                {
                    System.Console.Error.Write(ex.ToString()); //Cover any missed bases
                    throw ex;
                }//End try-catch-info-throw
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }//End try-catch-bail

            //May be that we're being watched by a debugger and need to output in two directions at once
            if (System.Diagnostics.Debugger.IsAttached)
            {
                TextWriter writerDebug = new TextWriterDebug();
                writerInterface = writerInterface != null ? new TextWriterMulti(writerInterface, writerDebug) : null;
                writerService = writerService != null ? new TextWriterMulti(writerService, writerDebug) : null;
                writerProxy = writerProxy != null ? new TextWriterMulti(writerProxy, writerDebug) : null;
                writerContract = writerContract != null ? new TextWriterMulti(writerContract, writerDebug) : null;
                writerWCFService = writerWCFService != null ? new TextWriterMulti(writerWCFService, writerDebug) : null;
                writerHost = writerHost != null ? new TextWriterMulti(writerHost, writerDebug) : null;
            }
            
            TextWriters textwriters = new TextWriters
            {
                writerInterface = writerInterface,
                writerService = writerService,
                writerProxy = writerProxy,
                writerWCFContracts = writerContract,
                writerWCFService = writerWCFService,
                writerWCFHost = writerHost
            };
            #endregion outputWriters

            //Output info if in verbose
            if (bVerbose)
            {
                System.Console.WriteLine(string.Format("// IDL input path: \"{0}\"", pathArg.Arg));
                System.Diagnostics.Debug.WriteLine(string.Format("// IDL input path: \"{0}\"", pathArg.Arg));
                
                foreach (TextWriter textwriter in textwriters.GenTextWriters())
                {
                    if(textwriter != null)
                    {
                        System.Console.WriteLine(string.Format("// Output path: \"{0}\"", ((FileStream)((StreamWriter)textwriter).BaseStream).Name));
                        System.Diagnostics.Debug.WriteLine(string.Format("// Output path: \"{0}\"", ((FileStream)((StreamWriter)textwriter).BaseStream).Name));
                    }
                }
            }

            
            System.Xml.XmlReaderSettings readerSettings = new System.Xml.XmlReaderSettings();
            readerSettings.DtdProcessing = System.Xml.DtdProcessing.Ignore;
            readerSettings.ValidationType = ValidationType.None;
            readerSettings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
            readerSettings.CloseInput = true;

            //Try to open the input xml file and generate the output code
            try
            {
                try
                {
                    if (pathArg.Arg != null) BuildCodeFromDbusIDL(XmlReader.Create(pathArg.Arg, readerSettings), textwriters);
                }
                catch (System.IO.FileNotFoundException fne)
                {
                    System.Console.Error.WriteLine("Could not find the input file {0}", pathArg.Arg);
                    throw fne;
                }
                catch (System.UriFormatException ufe)
                {
                    System.Console.Error.WriteLine("URI Formatting exception for input file {0}", pathArg.Arg);
                    throw ufe;
                }
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }

            //Close everything
            textwriters.Close();

            //If debugger attached, wait for them to tell us to quit
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press a key to continue...");
                Console.ReadKey(true);
            }
        }
    } // Ends class Program
}
