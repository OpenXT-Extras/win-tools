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

//#define _LISTNAMES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace UdbusCSharpConsole
{
    class UdbusCSharpConsole
    {
        static void Console_io_debug(IntPtr logpriv, String buf)
        {
            Console.WriteLine("CSharpConsole: {0}", buf);
        }

        static void WriteStructSizes(string[] args)
        {
            //Udbus.Serialization.NMessageStruct.dbus_msg testmsg = new Udbus.Core.dbus_msg();
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle testmsg = new Udbus.Serialization.NMessageStruct.UdbusMessageHandle();
            testmsg.destination = "test destination";
            testmsg.error_name = "test error_name";
            testmsg.interface_ = "test interface";
            testmsg.method = "test method";
            testmsg.path = "test path";
            testmsg.sender = "test sender";

            Console.WriteLine("Size of dbus_msg type: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.NMessageStruct.UdbusMessageHandle)));
            Console.WriteLine("Size of dbus_msg: {0}", System.Runtime.InteropServices.Marshal.SizeOf(testmsg));
            Console.WriteLine("Size of dbus_sig: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Types.dbus_sig)));
            Console.WriteLine("Size of dbus_reader: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.dbus_reader)));
            Console.WriteLine("Size of dbus_writer: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.dbus_writer)));
            Console.WriteLine("Size of dbus_array_writer: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.dbus_array_writer)));
            Console.WriteLine("Size of : dbus_array_reader{0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.dbus_array_reader)));
            Console.WriteLine("Size of dbus_msg::typefield: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.NMessageStruct.UdbusMessageHandle.Stypefield)));
            Console.WriteLine("Size of dbus_msg::comms: {0}", System.Runtime.InteropServices.Marshal.SizeOf(typeof(Udbus.Serialization.NMessageStruct.UdbusMessageHandle.Scomms)));
            //Console.WriteLine("Size of dbus_msg: {0}", System.Runtime.InteropServices.Marshal.SizeOf(testmsg.destination));
        }

        delegate void PreNextActionDelegate(string description);
        private static void NoOp(string ignored) { }
        private static void Announce(string action) { Console.WriteLine();  Console.WriteLine("==={0}===", action); }
        static void TestDbusCalls(Udbus.Serialization.IUdbusTransport connection)
        {
            TestDbusCalls(connection, Announce);
        }
        static void TestDbusGetProperty(Udbus.Serialization.IUdbusTransport connection)
        {
            TestDbusGetProperty(connection, Announce);
        }

        static void TestDbusHelloImpl(Udbus.Serialization.UdbusMessageBuilderTracker builder, Udbus.Core.IUdbusMessageVisitor visitor, Udbus.Serialization.UdbusConnector connector,
            System.Threading.ManualResetEvent stop, PreNextActionDelegate preNextAction)
        {
            int result = -1;
            //Udbus.v4v.v4vConnection connection = new Udbus.v4v.v4vConnection(Console_io_debug);
            //Udbus.Serialization.ManagedDbusIo dbio;
            //connection.PopulateDbio(ref dbio);

            //Udbus.Core.UdbusFunctions.dbus_auth(ref dbio, "Dude");
            //Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(connection);

            // Visitors
            preNextAction("Hello");

            // Hello.
            using (var msg = builder.UdbusMethodMessage(
                                "org.freedesktop.DBus", "/org/freedesktop/DBus",
                                "org.freedesktop.DBus", "Hello").Message)
            {
                result = connector.Send(msg);
                Console.WriteLine("Hello send result: {0}", result);
                Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, new Udbus.Core.UdbusMessageVisitorDumpConsole());
            }
            //var recv = connector.Receive(out result);
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle recv;
            result = connector.ReceiveStruct(out recv);
            Console.WriteLine("Hello recv result: {0}. msg: {1}", result, recv);
        }

        static void TestDbusGetPropertyImpl(Udbus.Serialization.UdbusMessageBuilderTracker builder, Udbus.Core.IUdbusMessageVisitor visitor, Udbus.Serialization.UdbusConnector connector,
            System.Threading.ManualResetEvent stop, PreNextActionDelegate preNextAction)
        {
            preNextAction("GetProperty");
            int result;
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle recv;

            using (var msg = builder.UdbusMethodMessage(
                "com.citrix.xenclient.xenmgr", "/",
                "org.freedesktop.DBus.Properties", "Get").Message)
            {
                Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                signature.a[0] = Udbus.Types.dbus_type.DBUS_STRING;
                signature.a[1] = Udbus.Types.dbus_type.DBUS_STRING;
                signature.a[2] = Udbus.Types.dbus_type.DBUS_INVALID;
                builder.SetSignature(ref signature)
                    .BodyAdd(4096)
                    .BodyAdd_String("com.citrix.xenclient.xenmgr.config")
                    .BodyAdd_String("iso-path")
                ;
                result = connector.Send(msg);
                Console.WriteLine("GetProperty send result: {0}", result);
                Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, new Udbus.Core.UdbusMessageVisitorDumpConsole());
            }
            try
            {
                Udbus.Core.UdbusMessagePair recvData = Udbus.Core.UdbusVisitorFunctions.LoopUdbusFind(connector, visitor, Console.Out, stop);
                if (!recvData.QuEmpty)
                {
                    recv = recvData.Data;
                    if (recv.typefield.type != Udbus.Types.dbus_msg_type.DBUS_TYPE_ERROR)
                    {
                        Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(recvData.Handle);
                        do
                        {
#if !READASVARIANT
                            var variant = reader.ReadVariantValue(out result);
                            Console.WriteLine("iso_path: {0}", variant.ToString());
                            //Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(new Udbus.Parsing.CodeTypeNoOpHolder());
                            //Udbus.Serialization.Variant.UdbusVariantIn variantIn = new Udbus.Serialization.Variant.UdbusVariantIn(reader);
                            //Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new Udbus.Parsing.IDLArgumentTypeNameBuilderNoOp();
                            //Udbus.Types.dbus_sig sig = Udbus.Types.dbus_sig.Initialiser;
                            //result = reader.ReadSignature(ref sig);
                            //Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(variantIn, nameBuilder, sig.a, context);
                            //Console.WriteLine("iso_path: {0}", variantIn.Variant.ToString());

#else // !READASVARIANT
                        Udbus.Types.dbus_sig sig = Udbus.Types.dbus_sig.Initialiser;
                        result = reader.ReadSignature(ref sig);
                        if (result != 0)
                        {
                            Console.WriteLine("Error reading property variant signature: {0}", result);
                            break;
                        }
                        string isopath;
                        result = reader.ReadString(out isopath);
                        if (result != 0)
                        {
                            Console.WriteLine("Error reading property variant string: {0}", result);
                            break;
                        }

                        Console.WriteLine("iso_path: {0}", isopath);
#endif // READASVARIANT
                        } while (false);
                    }
                }
                else
                {
                    Console.WriteLine("Getting the property failed. Boooooo ");
                }
            }
            catch (System.Runtime.InteropServices.SEHException seh)
            {
                Console.WriteLine("Error: " + seh.ToString());
            }
        }

        static Udbus.Serialization.UdbusConnector TestDbusGetProperty(Udbus.Serialization.IUdbusTransport connection, PreNextActionDelegate preNextAction)
        {
            System.Threading.ManualResetEvent stop = new System.Threading.ManualResetEvent(false);
            Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(connection);
            Udbus.Serialization.UdbusMessageBuilderTracker builder = new Udbus.Serialization.UdbusMessageBuilderTracker();

            // Visitors
            Udbus.Core.IUdbusMessageVisitor visitor = new Udbus.Core.UdbusMessageVisitorDumpXen();
            Udbus.Core.IUdbusMessageVisitor visitorTrack = new Udbus.Core.UdbusMessageVisitorFind(builder);
            Udbus.Core.IUdbusMessageVisitor visitors = new Udbus.Core.UdbusMessageVisitorMulti(visitor, visitorTrack);

            TestDbusHelloImpl(builder, visitors, connector, stop, preNextAction);

            // Get property.
            TestDbusGetPropertyImpl(builder, visitors, connector, stop, preNextAction);

            return connector;
        }

        static void TestDbusReadIcon(Udbus.Serialization.UdbusMessageBuilderTracker builder, Udbus.Core.IUdbusMessageVisitor visitor, Udbus.Serialization.UdbusConnector connector,
            System.Threading.ManualResetEvent stop, PreNextActionDelegate preNextAction)
        {
            int result = -1;
            //Udbus.v4v.v4vConnection connection = new Udbus.v4v.v4vConnection(Console_io_debug);
            //Udbus.Serialization.ManagedDbusIo dbio;
            //connection.PopulateDbio(ref dbio);

            //Udbus.Core.UdbusFunctions.dbus_auth(ref dbio, "Dude");
            //Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(connection);

            // Visitors
            preNextAction("readicon");

            // list_vms.
            using (var msg = builder.UdbusMethodMessage(
                "com.citrix.xenclient.xenmgr", "/",
                "com.citrix.xenclient.xenmgr",
                "list_vms").Message)
            {
                result = connector.Send(msg);
                Console.WriteLine("list_vms send result: {0}", result);
            }
            Udbus.Serialization.NMessageStruct.UdbusMessageHandle recv;
            using (var msg = Udbus.Core.UdbusVisitorFunctions.LoopUdbusFind(connector, visitor, Console.Out, stop).Handle)
            {
                msg.HandleToStructure(out recv);
                Console.WriteLine("list_vms recv result: {0}. msg: {1}", result, recv);

                Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);
                uint counter = 0;

                foreach (Udbus.Serialization.UdbusMessageReader subreader in reader.ArrayReader(Udbus.Types.dbus_type.DBUS_OBJECTPATH))
                {
                    Udbus.Types.UdbusObjectPath op;
                    result = subreader.ReadObjectPath(out op);
                    if (result != 0)
                    {
                        Console.WriteLine("Error ! {0:d}/0x{0:x8}", result);
                    }
                    else
                    {
                        Console.WriteLine(" {0}", op);

                        using (var msgVm = builder.UdbusMethodMessage(
                            "com.citrix.xenclient.xenmgr", op.Path,
                            "com.citrix.xenclient.xenmgr.vm",
                            "read_icon").Message)
                        {
                            result = connector.Send(msgVm);
                            Console.WriteLine("read_icon send result: {0}", result);

                        }
                        if (result == 0)
                        {
                            Udbus.Serialization.NMessageStruct.UdbusMessageHandle recvVm;
                            using (var msgVm = Udbus.Core.UdbusVisitorFunctions.LoopUdbusFind(connector, visitor, Console.Out, stop).Handle)
                            {
                                if (msgVm == null)
                                {
                                    Console.WriteLine("read_icon failed");
                                }
                                else
                                {
                                    msgVm.HandleToStructure(out recvVm);
                                    Console.WriteLine("read_icon result: {0}. msg: {1}", result, recvVm);

                                    Udbus.Serialization.UdbusMessageReader readerVm = new Udbus.Serialization.UdbusMessageReader(msgVm);
                                    Console.WriteLine("Icon bytes:");
                                    uint byteCounter = 0;

                                    foreach (Udbus.Serialization.UdbusMessageReader subreaderIcon in readerVm.ArrayReader(Udbus.Types.dbus_type.DBUS_BYTE))
                                    {
                                        byte b;
                                        result = subreaderIcon.ReadByte(out b);
                                        if (result != 0)
                                        {
                                            Console.WriteLine("Error ! {0:d}/0x{0:x8}", result);
                                            break;
                                        }
                                        else
                                        {
                                            //Console.Write(b);
                                            ++byteCounter;
                                        }
                                    }
                                    Console.WriteLine("Total: {0} bytes", byteCounter);
                                    Console.WriteLine();

                                } // Ends else read_icon succeeded
                            } // Ends read_icon response
                        }
                    }
                    Console.WriteLine("Entry {0:d2}: {1}", counter++, op);
                }
            } // Ends using ListVms response

        }

        static Udbus.Serialization.UdbusConnector TestDbusCalls(Udbus.Serialization.IUdbusTransport connection, PreNextActionDelegate preNextAction)
        {
            //int result = -1;
            System.Threading.ManualResetEvent stop = new System.Threading.ManualResetEvent(false);
            //Udbus.v4v.v4vConnection connection = new Udbus.v4v.v4vConnection(Console_io_debug);
            //Udbus.Serialization.ManagedDbusIo dbio;
            //connection.PopulateDbio(ref dbio);

            //Udbus.Core.UdbusFunctions.dbus_auth(ref dbio, "Dude");
            Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(connection);

            Udbus.Serialization.UdbusMessageBuilderTracker builder = new Udbus.Serialization.UdbusMessageBuilderTracker();

            // Visitors
            Udbus.Core.IUdbusMessageVisitor visitor = new Udbus.Core.UdbusMessageVisitorDumpXen();
            Udbus.Core.IUdbusMessageVisitor visitorTrack = new Udbus.Core.UdbusMessageVisitorFind(builder);
            Udbus.Core.IUdbusMessageVisitor visitors = new Udbus.Core.UdbusMessageVisitorMulti(visitor, visitorTrack);

            // Udbus.Serialization.NMessageStruct.UdbusMessageHandle recv;

            //preNextAction("Hello");

            //// Hello.
            //using (var msg = builder.UdbusMethodMessage(
            //                    "org.freedesktop.DBus", "/org/freedesktop/DBus",
            //                    "org.freedesktop.DBus", "Hello").Message)
            //{
            //    result = connector.Send(msg);
            //    Console.WriteLine("Hello send result: {0}", result);
            //}
            ////var recv = connector.Receive(out result);
            //result = connector.ReceiveStruct(out recv);
            //Console.WriteLine("Hello recv result: {0}. msg: {1}", result, recv);
            TestDbusHelloImpl(builder, visitors, connector, stop, preNextAction);

            // Get property.
            //TestDbusGetPropertyImpl(builder, visitors, connector, stop, preNextAction);

#if _LISTVMS
            preNextAction("List VMs");

            using (var msg = builder.UdbusMethodMessage(
                "com.citrix.xenclient.xenmgr", "/",
                "com.citrix.xenclient.xenmgr",
                "list_vms").Message)
            {
                result = connector.Send(msg);
                Console.WriteLine("list_vms send result: {0}", result);
            }
            using (var msg = Udbus.Core.UdbusVisitorFunctions.LoopUdbusFind(connector, visitors, Console.Out, stop).Handle)
            {
                msg.HandleToStructure(out recv);
                Console.WriteLine("list_vms recv result: {0}. msg: {1}", result, recv);

                Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);
                uint counter = 0;

                foreach (Udbus.Serialization.UdbusMessageReader subreader in reader.ArrayReader(Udbus.Types.dbus_type.DBUS_OBJECTPATH))
                {
                    Udbus.Types.UdbusObjectPath op;
                    result = subreader.ReadObjectPath(out op);
                    if (result != 0)
                    {
                        Console.WriteLine("Error ! {0:d}/0x{0:x8}", result);
                    }
                    Console.WriteLine("Entry {0:d2}: {1}", counter++, op);
                }
            } // Ends using ListNames response


#endif // _LISTVMS

// These days GetAllProperties tends to be blocked by firewall
#if _GETALLPROPERTIES
            preNextAction("GetAllProperties");
            using (var msg = builder.UdbusMethodMessage(
                "com.citrix.xenclient.xenmgr", "/",
                "org.freedesktop.DBus.Properties", "GetAll").Message)
            {
                Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                signature.a[0] = Udbus.Types.dbus_type.DBUS_STRING;
                signature.a[1] = Udbus.Types.dbus_type.DBUS_INVALID;
                builder.SetSignature(ref signature)
                    .BodyAdd(4096)
                    .BodyAdd_String("com.citrix.xenclient.xenmgr.config")
                ;
                result = connector.Send(msg);
                Console.WriteLine("GetAllProperties send result: {0}", result);
            }
            try
            {
                Udbus.Core.UdbusMessagePair recvData = Udbus.Core.UdbusVisitorFunctions.LoopUdbusFind(connector, visitors, Console.Out, stop);
                recv = recvData.Data;
                if (recv.typefield.type != Udbus.Types.dbus_msg_type.DBUS_TYPE_ERROR)
                {
                    Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(recvData.Handle);
                    do
                    {
#if !READASVARIANT
                        Udbus.Parsing.BuildContext context = new Udbus.Parsing.BuildContext(new Udbus.Parsing.CodeTypeNoOpHolder());
                        Udbus.Serialization.Variant.UdbusVariantIn variantIn = new Udbus.Serialization.Variant.UdbusVariantIn();
                        Udbus.Parsing.IDLArgumentTypeNameBuilderBase nameBuilder = new Udbus.Parsing.IDLArgumentTypeNameBuilderNoOp();
                        // It's a dictionary of strings -> variants.
                        //IDictionary<string, Udbus.Containers.dbus_union> properties;
                        IEnumerable<KeyValuePair<string, Udbus.Containers.dbus_union>> properties;
                        reader.MarshalDict(Udbus.Serialization.UdbusMessageReader.ReadString,
                            Udbus.Serialization.UdbusMessageReader.ReadVariant,
                            out properties);
                        //Udbus.Types.dbus_sig sig = Udbus.Types.dbus_sig.Initialiser;
                        //result = reader.ReadSignature(ref sig);
                        //Udbus.Parsing.CodeBuilderHelper.BuildCodeParamType(variantIn, nameBuilder, sig.a, context);
                        foreach (KeyValuePair<string, Udbus.Containers.dbus_union> entry in properties)
                        {
                            Console.WriteLine("{0} => {1}", entry.Key, entry.Value.ToString());
                        }

#else // !READASVARIANT
                        Udbus.Types.dbus_sig sig = Udbus.Types.dbus_sig.Initialiser;
                        result = reader.ReadSignature(ref sig);
                        if (result != 0)
                        {
                            Console.WriteLine("Error reading property variant signature: {0}", result);
                            break;
                        }
                        string isopath;
                        result = reader.ReadString(out isopath);
                        if (result != 0)
                        {
                            Console.WriteLine("Error reading property variant string: {0}", result);
                            break;
                        }

                        Console.WriteLine("iso_path: {0}", isopath);
#endif // READASVARIANT
                    } while (false);
                }
                else
                {
                    Console.WriteLine("Getting the property failed. Boooooo ");
                }
            }
            catch (System.Runtime.InteropServices.SEHException seh)
            {
                Console.WriteLine("Error: " + seh.ToString());
            }
#endif //_GETALLPROPERTIES

#if _LISTNAMES
            preNextAction("ListNames");

            // List Names.
            using (var msg = builder.UdbusMethodMessage(
                "org.freedesktop.DBus", "/org/freedesktop/DBus",
                "org.freedesktop.DBus", "ListNames").Message)
            {
                result = connector.Send(msg);
                Console.WriteLine("ListNames send result: {0}", result);
            }

            //result = connector.ReceiveStruct(out recv);
            //Console.WriteLine("ListNames recv result: {0}. msg: {1}", result, recv);

            //using (var msg = connector.ReceiveHandle(out result))
            using(var msg = Udbus.Core.UdbusVisitorFunctions.LoopUdbusFind(connector, visitors, Console.Out, stop).Handle)
            {
                //result = connector.ReceiveStruct(out recv);
                msg.HandleToStructure(out recv);
                Console.WriteLine("ListNames recv result: {0}. msg: {1}", result, recv);

                Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);
                uint counter = 0;

                foreach (Udbus.Serialization.UdbusMessageReader subreader in reader.ArrayReader(Udbus.Types.dbus_type.DBUS_STRING))
                {
                    string name = subreader.ReadStringValue(out result);
                    if (result != 0)
                    {
                        Console.WriteLine("Error ! {0:d}/0x{0:x8}", result);
                    }
                    Console.WriteLine("Entry {0:d2}: {1}", counter++, name);
                }
            } // Ends using ListNames response
#endif // _LISTNAMES

            // read_icon
            TestDbusReadIcon(builder, visitors, connector, stop, preNextAction);

#if _GETSIGNAL
            preNextAction("AddMatch");

            // AddMatch for signal.
            using (var msg = builder.UdbusMethodMessage(
                "org.freedesktop.DBus", "/org/freedesktop/DBus",
                "org.freedesktop.DBus", "AddMatch").Message)
            {
                Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                signature.a[0] = Udbus.Types.dbus_type.DBUS_STRING;
                signature.a[1] = Udbus.Types.dbus_type.DBUS_INVALID;
                builder.SetSignature(ref signature)
                    .BodyAdd(4096)
                    .BodyAdd_String("type='signal',interface='com.citrix.xenclient.xenmgr.host',member='storage_space_low'")
                    //.BodyAdd_String("type='signal',interface='com.citrix.xenclient.xenmgr.host'")
                ;
                result = connector.Send(msg);
                Console.WriteLine("AddMatch send result: {0}", result);

                    
            }

            preNextAction("LoopSignals");

            // Handle signals and other bits of magic.
            //Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += delegate(Object sender, ConsoleCancelEventArgs consoleargs)
            {
                consoleargs.Cancel = true;
                Console.WriteLine("Setting stop event...");
                stop.Set();
                connection.Cancel();
            };

            try
            {
                Udbus.Core.UdbusVisitorFunctions.LoopUdbus(connector, visitor, Console.Out, stop);
            }
            catch (System.Runtime.InteropServices.SEHException seh)
            {
                Console.WriteLine("Error: " + seh.ToString());
            }
#endif // _GETSIGNAL

#if HAVESOMECCODE
	r = 0;
	msg = NULL;
	msg = dbus_msg_new_method_call(serial++,
		"org.freedesktop.DBus", "/org/freedesktop/DBus",
		"org.freedesktop.DBus", "AddMatch");
	if (!msg) {
		dio.io_debug(dio.logpriv, "Unable to create method message for AddMatch\n");
		exit(1);
	}
	MessageInfo::dumpMethodSend(&io_debug, msg);
	dbus_sig signature;
	signature.a[0] = DBUS_STRING;
	signature.a[1] = DBUS_INVALID;
	dbus_msg_set_signature(msg, &signature);
	dbus_msg_body_add(msg, 4096);
	//r |= dbus_msg_body_add_string(msg, "type='signal',interface='com.citrix.xenclient.xenmgr.host'");
	//r |= dbus_msg_body_add_string(msg, "type='method_call'");
	r |= dbus_msg_body_add_string(msg, "type='signal',interface='com.citrix.xenclient.xenmgr.host'");
	r |= dbus_msg_send(&dio, msg);

	loop_dbus(dio, visitor);
#endif // HAVESOMECCODE
            Console.WriteLine("Press <ENTER> to end connection");
            Console.ReadLine();

            return connector;
        }

        class SampleDbusMessages
        {
            // hexbytes c:\code\work\xen\win-tools\dbus\MessageData\DbusMessageData1.bin
            public static readonly byte[] Authorise = {
                0x4f, 0x4b, 0x20, 0x37, 0x38, 0x36, 0x61, 0x61, 0x34, 0x61, 0x62, 0x65, 0x62
                , 0x33, 0x65, 0x37, 0x39, 0x65, 0x36, 0x33, 0x36, 0x62, 0x65, 0x66, 0x35, 0x33, 0x39
                , 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x61, 0x0d, 0x0a
            };
            public static readonly byte[] Hello = {
                0x6c, 0x02, 0x01, 0x01, 0x0b, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x3d, 
                0x00, 0x00, 0x00, 0x06, 0x01, 0x73, 0x00, 0x06, 0x00, 0x00, 0x00, 0x3a, 
                0x31, 0x2e, 0x34, 0x35, 0x37, 0x00, 0x00, 0x05, 0x01, 0x75, 0x00, 0x01, 0x00, 
                0x00, 0x00, 0x08, 0x01, 0x67, 0x00, 0x01, 0x73, 0x00, 0x00, 0x07, 0x01, 0x73, 
                0x00, 0x14, 0x00, 0x00, 0x00, 0x6f, 0x72, 0x67, 0x2e, 0x66, 0x72, 0x65, 0x65, 
                0x64, 0x65, 0x73, 0x6b, 0x74, 0x6f, 0x70, 0x2e, 0x44, 0x42, 0x75, 0x73, 0x00, 
                0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x3a, 0x31, 0x2e, 
                0x34, 0x35, 0x37, 0x00
            };
            public static readonly byte[] GetPropertyIsoPath = {
                0x6c, 0x04, 0x01, 0x01, 0x0b, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x8d, 
                0x00, 0x00, 0x00, 0x01, 0x01, 0x6f, 0x00, 0x15, 0x00, 0x00, 0x00, 0x2f, 
                0x6f, 0x72, 0x67, 0x2f, 0x66, 0x72, 0x65, 0x65, 0x64, 0x65, 0x73, 0x6b, 0x74, 
                0x6f, 0x70, 0x2f, 0x44, 0x42, 0x75, 0x73, 0x00, 0x00, 0x00, 0x02, 0x01, 0x73, 
                0x00, 0x14, 0x00, 0x00, 0x00, 0x6f, 0x72, 0x67, 0x2e, 0x66, 0x72, 0x65, 0x65, 
                0x64, 0x65, 0x73, 0x6b, 0x74, 0x6f, 0x70, 0x2e, 0x44, 0x42, 0x75, 0x73, 0x00, 
                0x00, 0x00, 0x00, 0x03, 0x01, 0x73, 0x00, 0x0c, 0x00, 0x00, 0x00, 0x4e, 0x61, 
                0x6d, 0x65, 0x41, 0x63, 0x71, 0x75, 0x69, 0x72, 0x65, 0x64, 0x00, 0x00, 0x00, 
                0x00, 0x06, 0x01, 0x73, 0x00, 0x06, 0x00, 0x00, 0x00, 0x3a, 0x31, 0x2e, 0x34, 
                0x35, 0x38, 0x00, 0x00, 0x08, 0x01, 0x67, 0x00, 0x01, 0x73, 0x00, 0x00, 0x07, 
                0x01, 0x73, 0x00, 0x14, 0x00, 0x00, 0x00, 0x6f, 0x72, 0x67, 0x2e, 0x66, 0x72, 
                0x65, 0x65, 0x64, 0x65, 0x73, 0x6b, 0x74, 0x6f, 0x70, 0x2e, 0x44, 0x42, 0x75, 
                0x73, 0x00, 0x00, 0x00, 0x00, 0x06, 
                0x00, 0x00, 0x00, 0x3a, 0x31, 0x2e, 0x34, 0x35, 0x38, 0x00, 0x6c, 0x02, 
                0x03, 0x01, 0x16, 0x00, 0x00, 0x00, 0xdd, 0x33, 0x00, 0x00, 0x2e, 0x00, 0x00, 
                0x00, 0x08, 0x01, 0x67, 0x00, 0x01, 0x76, 0x00, 0x00, 0x05, 0x01, 0x75, 
                0x00, 0x02, 0x00, 0x00, 0x00, 0x06, 0x01, 0x73, 0x00, 0x06, 0x00, 0x00, 0x00, 
                0x3a, 0x31, 0x2e, 0x34, 0x35, 0x38, 0x00, 0x00, 0x07, 0x01, 0x73, 0x00, 0x05, 
                0x00, 0x00, 0x00, 0x3a, 0x31, 0x2e, 0x32, 0x35, 0x00, 0x00, 0x00, 0x01, 0x73, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0x2f, 0x73, 0x74, 0x6f, 0x72, 
                0x61, 0x67, 0x65, 0x2f, 0x69, 0x73, 0x6f, 0x73, 0x00
            };
        } // Ends class SampleDbusMessages

        static void Testv4v()
        {
            using (Udbus.v4v.v4vConnection connection = new Udbus.v4v.v4vConnection(Console_io_debug))
            {
                TestDbusCalls(connection);
                Console.WriteLine("About to Dispose");
            }
        }

        static void Testv4vDump()
        {
            using (Udbus.Core.TransportDumper connection = new Udbus.Core.TransportDumper(new Udbus.v4v.v4vConnection(Console_io_debug)))
            {
                TestDbusCalls(connection, connection.NextAction);
                Console.WriteLine("About to Dispose");
            }
        }

        static void Testv4vGetPropertyDump()
        {
            using (Udbus.Core.TransportDumper connection = new Udbus.Core.TransportDumper(new Udbus.v4v.v4vConnection(Console_io_debug)))
            {
                TestDbusGetProperty(connection, connection.NextAction);
                Console.WriteLine("About to Dispose");
            }
            Console.WriteLine("Press <ENTER> to end connection");
            Console.ReadLine();
        }

        static void Testv4vGetProperty()
        {
            using (Udbus.v4v.v4vConnection connection = new Udbus.v4v.v4vConnection(Console_io_debug))
            {
                TestDbusGetProperty(connection);
                Console.WriteLine("About to Dispose");
            }
            Console.WriteLine("Press <ENTER> to end connection");
            Console.ReadLine();
        }


        static void TestMockGetProperty()
        {
            using (Udbus.Core.MockPlaybackUdbusTransport connection = new Udbus.Core.MockPlaybackUdbusTransport())
            {
                connection.AddBytes(SampleDbusMessages.Authorise);
                connection.AddBytes(SampleDbusMessages.Hello);
                connection.AddBytes(SampleDbusMessages.GetPropertyIsoPath);

                TestDbusGetProperty(connection);
            }
            Console.WriteLine("Press <ENTER> to end connection");
            Console.ReadLine();
        }

        class MarshalEventTest
        {
            public event Udbus.Serialization.MarshalReadDelegate<Udbus.Containers.dbus_union> foo;
            public Udbus.Serialization.MarshalReadDelegate<Udbus.Containers.dbus_union> Dude { get { return foo; } }
        }
        static void Foo()
        {
    //public delegate int MarshalReadDelegate<T>(Udbus.Serialization.UdbusMessageReader reader, out T t); // This would actually probably be better, despite sample code.
    //public delegate T MarshalReadResultDelegate<T>(Udbus.Serialization.UdbusMessageReader reader, out int result);
        //public static int ReadString(UdbusMessageReader reader, out string value)
            Udbus.Serialization.MarshalReadDelegate<string> ds = Udbus.Serialization.UdbusMessageReader.ReadString;
            Udbus.Serialization.MarshalReadDelegate<Udbus.Containers.dbus_union> dvbool = Udbus.Serialization.UdbusMessageReader.ReadVariantBoolean;
            Udbus.Serialization.MarshalReadDelegate<Udbus.Containers.dbus_union> dvbyte = Udbus.Serialization.UdbusMessageReader.ReadVariantByte;
            //Udbus.Core.MarshalReadDelegate<Udbus.Containers.dbus_union> dvmulti = new Udbus.Core.MarshalReadDelegate<Udbus.Containers.dbus_union>();
            //dvmulti += dvbool;
            //dvmulti += dvbyte;

            Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(null);
            Udbus.Containers.dbus_union u1 = new Udbus.Containers.dbus_union();
            reader.ReadVariantByte(u1);
            reader.ReadVariantByte(out u1);

            MarshalEventTest met = new MarshalEventTest();
            met.foo += dvbool;
            met.foo += dvbyte;
            met.Dude(reader, out u1);
        
        
        }

        static void TestVariantSubArrays()
        {
            //Udbus.Containers.dbus_union uTop = null;

            Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(null);
            //object o;
            //reader.ReadString(out o);
        }

        static void TestReadWriteString()
        {
            using (Udbus.Core.MockStoreUdbusTransport transport = new Udbus.Core.MockStoreUdbusTransport())
            {
                //Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(transport);
                Udbus.Serialization.UdbusConnector connector = new Udbus.Serialization.UdbusConnector(transport);
                Udbus.Serialization.UdbusMessageBuilderTracker builder = new Udbus.Serialization.UdbusMessageBuilderTracker();

                Udbus.Core.IUdbusMessageVisitor visitor = new Udbus.Core.UdbusMessageVisitorDumpXen();
                Udbus.Core.IUdbusMessageVisitor visitorTrack = new Udbus.Core.UdbusMessageVisitorFind(builder);
                Udbus.Core.IUdbusMessageVisitor visitors = new Udbus.Core.UdbusMessageVisitorMulti(visitor, visitorTrack);

                int result;
                using (var msg = builder.UdbusMethodMessage(
                                    "org.freedesktop.DBus", "/org/freedesktop/DBus",
                                    "org.freedesktop.DBus", "NotARealMessage").Message)
                {
                    Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                    signature.a[0] = Udbus.Types.dbus_type.DBUS_STRING;
                    signature.a[1] = Udbus.Types.dbus_type.DBUS_INVALID;
                    builder.SetSignature(ref signature)
                        .BodyAdd(4096)
                        .BodyAdd_String("param1")
                    ;
                    result = connector.Send(msg);
                    Console.WriteLine("NotARealMessagesend result: {0}", result);
                }

                using (var msg = connector.ReceiveHandle(out result))
                {
                    Console.WriteLine("NotARealMessage recv result: {0}. msg: {1}", result, msg);
                    if (result == 0) // If got message ok
                    {
                        Udbus.Core.UdbusMessagePair messageData = new Udbus.Core.UdbusMessagePair(msg);
                        Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);

                        Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, visitor);
                        //Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, visitor);
                        string param1 = reader.ReadString();
                        Console.WriteLine(string.Format("Param1: {0}", param1));

                    } // Ends if message ok
                } // Ends using receive message
            } // Ends using transport
        }

        static void TestReadWriteVariantStruct()
        {
            using (Udbus.Core.MockStoreUdbusTransport transport = new Udbus.Core.MockStoreUdbusTransport())
            {
                //Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(transport);
                Udbus.Serialization.UdbusConnector connector = new Udbus.Serialization.UdbusConnector(transport);
                Udbus.Serialization.UdbusMessageBuilderTracker builder = new Udbus.Serialization.UdbusMessageBuilderTracker();

                Udbus.Core.IUdbusMessageVisitor visitor = new Udbus.Core.UdbusMessageVisitorDumpXen();
                Udbus.Core.IUdbusMessageVisitor visitorTrack = new Udbus.Core.UdbusMessageVisitorFind(builder);
                Udbus.Core.IUdbusMessageVisitor visitors = new Udbus.Core.UdbusMessageVisitorMulti(visitor, visitorTrack);

                int result;
                using (var msg = builder.UdbusMethodMessage(
                                    "org.freedesktop.DBus", "/org/freedesktop/DBus",
                                    "org.freedesktop.DBus", "VariantStructMessage").Message)
                {
                    Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                    signature.a[0] = Udbus.Types.dbus_type.DBUS_SIGNATURE;
                    signature.a[1] = Udbus.Types.dbus_type.DBUS_INVALID;
                    // As it happens these struct fields will double as dictionary entries...
                    object[] structFields = new object[] { 1, new object[] {"one", true } };

                    Udbus.Containers.dbus_union variantStruct = Udbus.Containers.dbus_union.CreateStruct(structFields,
                        new Udbus.Types.dbus_type[] {
                            Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN,
                            Udbus.Types.dbus_type.DBUS_INT32,
                            Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN,
                            Udbus.Types.dbus_type.DBUS_STRING,
                            Udbus.Types.dbus_type.DBUS_BOOLEAN,
                            Udbus.Types.dbus_type.DBUS_STRUCT_END,
                            Udbus.Types.dbus_type.DBUS_STRUCT_END,
                            Udbus.Types.dbus_type.DBUS_INVALID
                        }
                    );

                    builder.SetSignature(ref signature)
                        .BodyAdd(4096);
                    builder
                        .BodyAdd_Variant(variantStruct)
                    ;
                    result = connector.Send(msg);
                    Console.WriteLine("VariantStructMessage send result: {0}", result);
                }

                using (var msg = connector.ReceiveHandle(out result))
                {
                    Console.WriteLine("VariantStructMessage recv result: {0}. msg: {1}", result, msg);
                    if (result == 0) // If got message ok
                    {
                        Udbus.Core.UdbusMessagePair messageData = new Udbus.Core.UdbusMessagePair(msg);
                        Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);

                        Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, visitor);
                        Udbus.Containers.dbus_union variantReadStruct = reader.ReadVariant();
                        Console.WriteLine(string.Format("variantReadStruct: {0}", variantReadStruct.ToString()));

                    } // Ends if message ok
                } // Ends using receive message
            } // Ends using transport
        }

        static void TestReadWriteVariantDict()
        {
            using (Udbus.Core.MockStoreUdbusTransport transport = new Udbus.Core.MockStoreUdbusTransport())
            {
                //Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(transport);
                Udbus.Serialization.UdbusConnector connector = new Udbus.Serialization.UdbusConnector(transport);
                Udbus.Serialization.UdbusMessageBuilderTracker builder = new Udbus.Serialization.UdbusMessageBuilderTracker();

                Udbus.Core.IUdbusMessageVisitor visitor = new Udbus.Core.UdbusMessageVisitorDumpXen();
                Udbus.Core.IUdbusMessageVisitor visitorTrack = new Udbus.Core.UdbusMessageVisitorFind(builder);
                Udbus.Core.IUdbusMessageVisitor visitors = new Udbus.Core.UdbusMessageVisitorMulti(visitor, visitorTrack);

                int result;
                using (var msg = builder.UdbusMethodMessage(
                                    "org.freedesktop.DBus", "/org/freedesktop/DBus",
                                    "org.freedesktop.DBus", "VariantDictMessage").Message)
                {
                    Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                    signature.a[0] = Udbus.Types.dbus_type.DBUS_SIGNATURE;
                    signature.a[1] = Udbus.Types.dbus_type.DBUS_INVALID;
                    // As it happens these struct fields will double as dictionary entries...
                    object[] structFields = new object[] { 1, new object[] { "one", true } };

                    object[] dictFields = new object[] {
                        structFields, // dict entry 1
                        new object[] { 2, new object[] { "two", false } }, // dict entry 2
                    };

                    System.Collections.Generic.Dictionary<object, object> dict = new System.Collections.Generic.Dictionary<object, object>();
                    dict[1] = new object[] { "one", true };
                    dict[2] = new object[] { "two", false };

                    Udbus.Containers.dbus_union variantDict = Udbus.Containers.dbus_union.Create(dict,
                        new Udbus.Types.dbus_type[] {
                            Udbus.Types.dbus_type.DBUS_ARRAY,
                            Udbus.Types.dbus_type.DBUS_DICT_BEGIN,
                            Udbus.Types.dbus_type.DBUS_INT32,
                            Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN,
                            Udbus.Types.dbus_type.DBUS_STRING,
                            Udbus.Types.dbus_type.DBUS_BOOLEAN,
                            Udbus.Types.dbus_type.DBUS_STRUCT_END,
                            Udbus.Types.dbus_type.DBUS_DICT_END,
                            Udbus.Types.dbus_type.DBUS_INVALID
                        }
                    );

                    builder.SetSignature(ref signature)
                        .BodyAdd(4096);
                    builder
                        .BodyAdd_Variant(variantDict)
                    ;
                    result = connector.Send(msg);
                    Console.WriteLine("VariantDictMessage send result: {0}", result);
                }

                using (var msg = connector.ReceiveHandle(out result))
                {
                    Console.WriteLine("VariantDictMessage recv result: {0}. msg: {1}", result, msg);
                    if (result == 0) // If got message ok
                    {
                        Udbus.Core.UdbusMessagePair messageData = new Udbus.Core.UdbusMessagePair(msg);
                        Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);

                        Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, visitor);

                        Udbus.Containers.dbus_union variantReadDict = reader.ReadVariant();
                        Console.WriteLine(string.Format("variantReadDict: {0}", variantReadDict.ToString()));

                    } // Ends if message ok
                } // Ends using receive message
            } // Ends using transport
        }

        static void TestReadWriteVariantManuallyConstructedDict()
        {
            using (Udbus.Core.MockStoreUdbusTransport transport = new Udbus.Core.MockStoreUdbusTransport())
            {
                //Udbus.Serialization.UdbusConnector connector = Udbus.Serialization.UdbusConnector.CreateAuthorised(transport);
                Udbus.Serialization.UdbusConnector connector = new Udbus.Serialization.UdbusConnector(transport);
                Udbus.Serialization.UdbusMessageBuilderTracker builder = new Udbus.Serialization.UdbusMessageBuilderTracker();

                Udbus.Core.IUdbusMessageVisitor visitor = new Udbus.Core.UdbusMessageVisitorDumpXen();
                Udbus.Core.IUdbusMessageVisitor visitorTrack = new Udbus.Core.UdbusMessageVisitorFind(builder);
                Udbus.Core.IUdbusMessageVisitor visitors = new Udbus.Core.UdbusMessageVisitorMulti(visitor, visitorTrack);

                int result;
                using (var msg = builder.UdbusMethodMessage(
                                    "org.freedesktop.DBus", "/org/freedesktop/DBus",
                                    "org.freedesktop.DBus", "VariantDictMessage").Message)
                {
                    Udbus.Types.dbus_sig signature = Udbus.Types.dbus_sig.Initialiser;
                    signature.a[0] = Udbus.Types.dbus_type.DBUS_SIGNATURE;
                    signature.a[1] = Udbus.Types.dbus_type.DBUS_INVALID;
                    // As it happens these struct fields will double as dictionary entries...
                    object[] structFields = new object[] { 1, new object[] { "one", true } };

                    object[] dictFields = new object[] {
                        structFields, // dict entry 1
                        new object[] { 2, new object[] { "two", false } }, // dict entry 2
                    };

                    Udbus.Containers.dbus_union variantDict = Udbus.Containers.dbus_union.Create(dictFields,
                        new Udbus.Types.dbus_type[] {
                            Udbus.Types.dbus_type.DBUS_ARRAY,
                            Udbus.Types.dbus_type.DBUS_DICT_BEGIN,
                            Udbus.Types.dbus_type.DBUS_INT32,
                            Udbus.Types.dbus_type.DBUS_STRUCT_BEGIN,
                            Udbus.Types.dbus_type.DBUS_STRING,
                            Udbus.Types.dbus_type.DBUS_BOOLEAN,
                            Udbus.Types.dbus_type.DBUS_STRUCT_END,
                            Udbus.Types.dbus_type.DBUS_DICT_END,
                            Udbus.Types.dbus_type.DBUS_INVALID
                        }
                    );

                    builder.SetSignature(ref signature)
                        .BodyAdd(4096)
                        .BodyAdd_Variant(variantDict)
                    ;
                    result = connector.Send(msg);
                    Console.WriteLine("VariantDictMessage send result: {0}", result);
                }

                using (var msg = connector.ReceiveHandle(out result))
                {
                    Console.WriteLine("VariantDictMessage recv result: {0}. msg: {1}", result, msg);
                    if (result == 0) // If got message ok
                    {
                        Udbus.Core.UdbusMessagePair messageData = new Udbus.Core.UdbusMessagePair(msg);
                        Udbus.Serialization.UdbusMessageReader reader = new Udbus.Serialization.UdbusMessageReader(msg);

                        Udbus.Core.UdbusVisitorFunctions.Visit(result, msg, visitor);

                        Udbus.Containers.dbus_union variantReadDict = reader.ReadVariant();
                        Console.WriteLine(string.Format("variantReadDict: {0}", variantReadDict.ToString()));

                    } // Ends if message ok
                } // Ends using receive message
            } // Ends using transport
        }


        static void MarshalNull()
        {
            string zero = Marshal.PtrToStringAnsi(IntPtr.Zero);
            Console.WriteLine(string.Format("zero is: {0}", zero));
        }

        static void TestCreateHandleFromStructure()
        {
            uint serial = 1;

            Udbus.Serialization.NMessageStruct.UdbusMessageHandle structMessage = new Udbus.Serialization.NMessageStruct.UdbusMessageHandle();
            structMessage.error_name = null;
            structMessage.interface_ = null;
            structMessage.method = null;
            structMessage.path = null;
            structMessage.sender = "org.freedesktop.DBus";
            structMessage.signature = Udbus.Types.dbus_sig.Initialiser;
            structMessage.typefield.type = Udbus.Types.dbus_msg_type.DBUS_TYPE_METHOD_RETURN;
            structMessage.destination = ":1.123";
            structMessage.reply_serial = serial;
            structMessage.body = IntPtr.Zero;
            Udbus.Serialization.NMessageHandle.UdbusMessageHandle message = Udbus.Serialization.NMessageHandle.UdbusMessageHandle.StructureToHandle(structMessage);

            Udbus.Serialization.NMessageStruct.UdbusMessageHandle structMessageUnserialize = message.HandleToStructure();
            Console.WriteLine(structMessageUnserialize);


        }

        // TODO - start implementing functions.
        // Will we need a separate assembly to tie up C++ and .NET ?
        static void Main(string[] args)
        {
            WriteStructSizes(args);
            //return;
            Console.WriteLine("CSharpConsole pre changing directory: {0}", System.IO.Directory.GetCurrentDirectory());
            try
            {
                // Since setting working directory on remote build seems to do FA
                System.IO.Directory.SetCurrentDirectory("..\\..\\..\\..\\linksVS2010");
                Console.WriteLine("CSharpConsole: {0}", System.IO.Directory.GetCurrentDirectory());
            }
            catch (System.IO.DirectoryNotFoundException)
            {
            }
            Testv4v();
            //Testv4vDump();
            //TestMockGetProperty();
            //Testv4vGetProperty();
            //Testv4vGetPropertyDump();
            //TestReadWriteString();
            //TestReadWriteVariantStruct();
            //TestReadWriteVariantDict();
            //MarshalNull();
            //TestCreateHandleFromStructure();

            Console.WriteLine("Disposed");

        }
    }
}
