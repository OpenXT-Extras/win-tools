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

namespace Udbus.Core
{
    /// <summary>
    /// This mock does the absolute minimum to get by.
    /// </summary>
    public class MockUdbusTransport : Udbus.Serialization.IUdbusTransport
    {
        private int disposed = 0;
        protected const byte newline = (byte)'\n';

        #region Dbus IO functions
        //public virtual int MockIOWrite(IntPtr priv, String buf, System.UInt32 count)
        public virtual int MockIOWrite(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            return 0;
        }

        public virtual int MockIORead(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            if (count > 0)
            {
                buf[0] = newline;
            }
            return checked((int)count);
        }

        public virtual void MockIODebug(IntPtr logpriv, String buf)
        {
            System.Diagnostics.Debug.WriteLine(buf);
        }
        #endregion // Dbus IO functions

        #region IUdbusTransport Members
        /// <summary>
        /// Setup io delegates with member functions.
        /// </summary>
        /// <param name="pdbus_io">Struct containing udbus io function.</param>
        /// <returns>true (success)</returns>
        public bool PopulateDbio(ref Udbus.Serialization.ManagedDbusIo pdbus_io)
        {
            pdbus_io = new Udbus.Serialization.ManagedDbusIo();

            //public delegate int D_io_write(IntPtr priv, String buf, System.UInt32 count);
            pdbus_io.io_write = this.MockIOWrite;
            //public delegate int D_io_read(IntPtr priv, byte[] buf, System.UInt32 count);
            pdbus_io.io_read = this.MockIORead;
            //public delegate void D_io_debug(IntPtr logpriv, String buf);
            pdbus_io.io_debug = this.MockIODebug;

            return true;
        }

        public Udbus.Serialization.WaitForReadResult WaitForRead(uint milliseconds)
        {
            return Udbus.Serialization.WaitForReadResult.Succeeded;
        }

        public bool Cancel()
        {
            return true;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            ++this.disposed;
        }

        #endregion // IUdbusTransport Members
    }  // Ends class MockUdbusTransport

    /// <summary>
    /// Does nothing except return hard-coded read result.
    /// </summary>
    public class MockUdbusTransportReadResult : MockUdbusTransport
    {
        public int ReadResult { get; set; }

        public MockUdbusTransportReadResult(int readResult)
        {
            this.ReadResult = readResult;
        }

        public MockUdbusTransportReadResult()
            : this(-1)
        {
        }

        public override int MockIORead(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            return this.ReadResult;
        }

    } // Ends class MockUdbusTransportReadResult

    /// <summary>
    /// Reads and writes to the same stream, effectively playing messages back.
    /// </summary>
    public class MockStoreUdbusTransport : MockUdbusTransport
    {
        public System.IO.MemoryStream Stream = new System.IO.MemoryStream();
        long posRead = 0;

        public MockStoreUdbusTransport()
        {
            this.Stream.WriteByte(newline);
            this.Stream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        public override int MockIOWrite(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            int nCount = checked((int)count);
            this.Stream.Write(buf, 0, nCount);
            return base.MockIOWrite(priv, buf, count);
        }

        public override int MockIORead(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            long posCurrent = this.Stream.Position;
            this.Stream.Seek(this.posRead, System.IO.SeekOrigin.Begin);
            int nCount = checked((int)count);
            int nRead = this.Stream.Read(buf, 0, nCount);
            this.posRead = this.Stream.Position;
            this.Stream.Seek(posCurrent, System.IO.SeekOrigin.Begin);
            return nRead;
        }

    } // Ends class MockStoreUdbusTransport

    // Sadly there's no easy way to mess with reply_serial.
#if USE_MockStoreUdbusTransportAdjustReply
    /// <summary>
    /// Reads and writes to the same stream, effectively playing messages back.
    /// </summary>
    public class MockStoreUdbusTransportAdjustReply : MockStoreUdbusTransport
    {
        uint lastSerial;

        static private UInt32 GetSerial(byte[] bytes)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes, 8, 4))
            {
                System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

                return reader.ReadUInt32();
            }
        }

        public override int MockIOWrite(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            this.lastSerial = GetSerial(buf);
            return base.MockIOWrite(priv, buf, count);
        }

        public override int MockIORead(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            int nRead = base.MockIORead(priv, buf, count);

            if (nRead == count) // If read it all back
            {
                MockStoreUdbusTransport tempTransport = new MockStoreUdbusTransport();
                tempTransport.Stream = new System.IO.MemoryStream(buf);
                tempTransport.Stream.Seek(0, System.IO.SeekOrigin.Begin
                ManagedDbusIo dbus_io = new ManagedDbusIo();
                tempTransport.PopulateDbio(ref dbus_io);
                NMessageHandle.UdbusMessageHandle msg;
                UdbusMsgHandleFunctions.dbus_msg_recv(ref dbus_io, out msg);
                if (msg.HandleToStructure().reply_serial == this.lastSerial)
                {
                    UdbusMessageBuilder builder = new UdbusMessageBuilder();
                    // No can do since no accessors.
                    // Also, how would we then turn this back into binary format ??
                    builder.SetReplySerial(this.lastSerial);
                }

            } // Ends if read it all back

            return nRead;
        }

    } // Ends class MockStoreUdbusTransportAdjustReply
#endif // USE_MockStoreUdbusTransportAdjustReply
    /// <summary>
    /// Playsback previously stored messages.
    /// </summary>
    public class MockPlaybackUdbusTransport : MockUdbusTransport
    {
        public System.IO.MemoryStream Stream = new System.IO.MemoryStream();
        long posRead = 0;

        static UInt32 GetSerial(byte[] bytes)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes, 8, 4))
            {
                System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

                return reader.ReadUInt32();
            }
        }

        static void SetSerial(byte[] bytes, UInt32 serial)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes, 8, 4))
            {
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);

                writer.Write(serial);
            }
        }

        public MockPlaybackUdbusTransport()
        {
        }

        public void AddAuthorise()
        {
            this.Stream.WriteByte(newline);
        }

        public void AddBytes(byte[] bytes)
        {
            this.Stream.Write(bytes, 0, bytes.Length);
        }

        public void AddMessageReply(byte[] bytes, uint serial)
        {
            uint oldSerial = GetSerial(bytes);
            if (GetSerial(bytes) != serial)
            {
                byte[] bytesAdjusted = new byte[bytes.Length];
                bytes.CopyTo(bytesAdjusted, 0);
                SetSerial(bytesAdjusted, serial);
                bytes = bytesAdjusted;

            }
            this.Stream.Write(bytes, 0, bytes.Length);
        }

        public override int MockIOWrite(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            // Don't do any writing...
            return 0;
        }

        public override int MockIORead(IntPtr priv, byte[] buf, System.UInt32 count)
        {
            // Read back the next set of bytes...
            long posCurrent = this.Stream.Position;
            this.Stream.Seek(this.posRead, System.IO.SeekOrigin.Begin);
            int nCount = checked((int)count);
            int nRead = this.Stream.Read(buf, 0, nCount);
            this.posRead = this.Stream.Position;
            this.Stream.Seek(posCurrent, System.IO.SeekOrigin.Begin);
            return nRead;
        }

    } // Ends class MockPlaybackUdbusTransport


    public class TransportDumper : Udbus.Serialization.IUdbusTransport
    {
        Udbus.Serialization.ManagedDbusIo dbusioOriginal;
        Udbus.Serialization.IUdbusTransport transport;

        public TransportDumper(Udbus.Serialization.IUdbusTransport transport)
        {
            this.transport = transport;
            this.NextAction("Start");
        }

        const string formatLast = "0x{0:x2}";
        const string formatMid = "0x{0:x2}, ";
        const string pad = "    ";
        const int columns = 80;
        const string InFileFormat = "DbusMessageReadData{0}_{1}.bin";
        const string OutFileFormat = "DbusMessageWriteData{0}_{1}.bin";
        const string InHexFileFormat = "HexDbusMessageReadData{0}_{1}.txt";
        const string OutHexFileFormat = "HexDbusMessageWriteData{0}_{1}.txt";
        string inFileName;
        string outFileName;
        string inHexFileName;
        string outHexFileName;
        long posInHexFile = 0;
        long posOutHexFile = 0;
        int inFiles = 0;
        int outFiles = 0;

        /// <summary>
        /// Turn bytes into comma-delimited hex strings.
        /// </summary>
        /// <param name="buffer">bytes to write</param>
        /// <param name="streamOut">Where to write hex strings.</param>
        /// <param name="column">Max column width in characters.</param>
        /// <param name="pad">Indentation string.</param>
        static long BufferToCharBytes(IEnumerable<byte> buffer, System.IO.TextWriter streamOut, int column, string pad, long posStart)
        {
            IEnumerator<byte> bytes = buffer.GetEnumerator();
            string lastline = System.Environment.NewLine;
            string nextline = System.Environment.NewLine + ", ";
            int padLength = pad.Length;
            long total = 0;

            if (bytes.MoveNext()) // If got first
            {
                byte byteIter = bytes.Current;
                long written = padLength;

                string output;// = string.Format(formatMid, byteIter);

                bool bContinue = bytes.MoveNext();

                if (bContinue) // If not last byte
                {
                    output = string.Format(formatMid, byteIter);

                } // Ends if not last byte
                else // Else last byte
                {
                    output = string.Format(formatLast, byteIter);

                } // Ends else last byte

                if (posStart == 0) // If writing at start
                {
                    streamOut.Write(pad);
                    total += padLength;

                } // Ends if writing at start
                else // Else writing picking up from wherever we were
                {
                    written = posStart % column;
                    string separator = ", ";
                    streamOut.Write(separator);
                    written += separator.Length;
                    total += separator.Length;

                } // Ends else writing picking up from wherever we were

                while (bContinue)
                {
                    if (written + output.Length >= column) // If over the column length
                    {
                        streamOut.WriteLine(output);
                        written = padLength;
                        streamOut.Write(pad);
                        total += output.Length + padLength + streamOut.NewLine.Length;

                    } // Ends if over the column length
                    else // Else below column length
                    {
                        streamOut.Write(output);
                        written += output.Length;
                        total += output.Length;

                    } // Ends else below column length

                    byteIter = bytes.Current;
                    output = string.Format(formatMid, byteIter);
                    bContinue = bytes.MoveNext();

                } // Ends loop iterating over remaining bytes

                // Write the last.
                output = string.Format(formatLast, byteIter);

                if (written + output.Length >= column) // If over the column length
                {
                    streamOut.WriteLine(output);
                    written = padLength;
                    total += output.Length;

                }// Ends if over the column length
                else // Else below column length
                {
                    streamOut.Write(output);
                    written += output.Length;
                    total += output.Length;

                } // Ends else below column length
            } // Ends if got first

            return total;
        }

        /// <summary>
        /// Turn bytes into comma-delimited hex strings (no columns).
        /// </summary>
        /// <param name="buffer">bytes to write</param>
        /// <param name="streamOut">Where to write hex strings.</param>
        /// <param name="pad">Indentation string.</param>
        static long BufferToCharBytes(IEnumerable<byte> buffer, System.IO.TextWriter streamOut, string pad, long posStart)
        {
            IEnumerator<byte> bytes = buffer.GetEnumerator();
            string lastline = System.Environment.NewLine;
            string nextline = System.Environment.NewLine + ", ";
            long written = 0;

            if (bytes.MoveNext()) // If got first
            {
                byte byteIter = bytes.Current;

                string output;// = string.Format(formatMid, byteIter);

                bool bContinue = bytes.MoveNext();

                if (bContinue) // If not last byte
                {
                    output = string.Format(formatMid, byteIter);

                } // Ends if not last byte
                else // Else last byte
                {
                    output = string.Format(formatLast, byteIter);

                } // Ends else last byte

                if (posStart == 0) // If writing at start
                {
                    streamOut.Write(pad);
                    written += pad.Length;

                } // Ends if writing at start

                while (bContinue)
                {
                    streamOut.Write(output);
                    written += output.Length;
                    byteIter = bytes.Current;
                    output = string.Format(formatMid, byteIter);
                    bContinue = bytes.MoveNext();

                } // Ends loop iterating over remaining bytes

                // Write the last.
                output = string.Format(formatLast, byteIter);
                streamOut.Write(output);
                written += output.Length;

            } // Ends if got first

            return written;
        }

        /// <summary>
        /// Turn bytes into comma-delimited hex strings (columns, no padding).
        /// </summary>
        /// <param name="buffer">bytes to write</param>
        /// <param name="streamOut">Where to write hex strings.</param>
        /// <param name="column">Max column width in characters.</param>
        static void BufferToCharBytes(IEnumerable<byte> buffer, System.IO.TextWriter streamOut, int column, long posStart)
        {
            BufferToCharBytes(buffer, streamOut, column, string.Empty, posStart);
        }

        /// <summary>
        /// Turn bytes into comma-delimited hex strings (no columns, no padding).
        /// </summary>
        /// <param name="buffer">bytes to write</param>
        /// <param name="streamOut">Where to write hex strings.</param>
        static void BufferToCharBytes(IEnumerable<byte> buffer, System.IO.TextWriter streamOut, long posStart)
        {
            BufferToCharBytes(buffer, streamOut, string.Empty, posStart);
        }

        public void NextAction(string description)
        {
            ++this.inFiles;
            this.inFileName = string.Format(InFileFormat, this.inFiles, description);
            ++this.outFiles;
            this.outFileName = string.Format(OutFileFormat, this.outFiles, description);

            this.inHexFileName = string.Format(InHexFileFormat, this.inFiles, description);
            this.outHexFileName = string.Format(OutHexFileFormat, this.inFiles, description);

            this.posInHexFile = 0;
            this.posOutHexFile = 0;

            foreach (string fileName in new string[] { this.inFileName, this.outFileName, this.inHexFileName, this.outHexFileName })
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
            }
        }

        #region dbus_io functions
        ///* return 0 if succeed to write count bytes, non-0 otherwise */
        //public int io_write(IntPtr priv, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In]byte[] buf, System.UInt32 count);
        public int io_write(IntPtr priv, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray, SizeParamIndex = 2)][System.Runtime.InteropServices.In]byte[] buf, System.UInt32 count)
        {
            //int i = 1;
            //string outFileName = string.Format(OutFileFormat, i);

            //while (System.IO.File.Exists(outFileName))
            //{
            //    ++i;
            //    outFileName = string.Format(OutFileFormat, i);
            //}


            //using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(outFileName)))
            //{
            //    writer.Write(buf);
            //}
            //BundleData(OutFileFormat, buf, count);

            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(this.outFileName, System.IO.FileMode.Append)))
            {
                writer.Write(buf);
            }
            bool bHexExists = System.IO.File.Exists(this.outHexFileName);
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.File.Open(this.outHexFileName, System.IO.FileMode.Append)))
            {
                this.posOutHexFile += BufferToCharBytes(buf, writer, columns, pad, this.posOutHexFile);
            }
            int result = this.dbusioOriginal.io_write(priv, buf, count);

            return result;
        }

        //public delegate int D_io_write(IntPtr priv, String buf, System.UInt32 count);
        /* return 0 if succeed to read count bytes, non-0 otherwise */
        //public int io_read(IntPtr priv, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][Out]byte[] buf, System.UInt32 count);
        public int io_read(IntPtr priv, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray, SizeParamIndex = 2)][System.Runtime.InteropServices.Out]byte[] buf, System.UInt32 count)
        {
            int result = this.dbusioOriginal.io_read(priv, buf, count);
            //BundleData(InFileFormat, buf, count);
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(this.inFileName, System.IO.FileMode.Append)))
            {
                writer.Write(buf);
            }

            bool bHexExists = System.IO.File.Exists(this.inHexFileName);
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.File.Open(this.inHexFileName, System.IO.FileMode.Append)))
            {
                this.posInHexFile  += BufferToCharBytes(buf, writer, columns, pad, this.posInHexFile);
            }

            return result;
        }

        #endregion // dbus_io functions

        #region IUdbusTransport functions
        //public override bool PopulateDbio(out Udbus.Serialization.ManagedDbusIo pdbus_io)
        public bool PopulateDbio(ref Udbus.Serialization.ManagedDbusIo pdbus_io)
        {
            //bool result = base.PopulateDbio(ref pdbus_io);
            ///pdbus_io = pdbus_io;// default(Udbus.Serialization.ManagedDbusIo);
            //this.dbusioOriginal = new Udbus.Serialization.ManagedDbusIo(pdbus_io);
            bool result = this.transport.PopulateDbio(ref this.dbusioOriginal);
            ////base.PopulateDbio(ref this.dbusioOriginal);

            if (result) // If populated base ok
            {
                this.transport.PopulateDbio(ref pdbus_io);
                //pdbus_io = new Udbus.Serialization.ManagedDbusIo();
                pdbus_io.io_write = this.io_write;
                pdbus_io.io_read = this.io_read;
                pdbus_io.io_debug = this.dbusioOriginal.io_debug;

            } // Ends if populated base ok
            ////else
            ////{
            ////    pdbus_io = this.dbusioOriginal;
            ////}

            return result;
        }

        public Udbus.Serialization.WaitForReadResult WaitForRead(uint milliseconds)
        {
            return this.transport.WaitForRead(milliseconds);
        }

        public bool Cancel()
        {
            return this.transport.Cancel();
        }

        #endregion // IUdbusTransport functions

        #region IDisposable Members

        public void Dispose()
        {
            this.transport.Dispose();
        }

        #endregion
    } // Ends class TransportDumper
} // Ends namespace Udbus.Core
