//
// Copyright (c) 2013 Citrix Systems, Inc.
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

namespace XenClientGuestService
{
    internal class XcDiagRunner
    {
        private const string XcInstallRegkey = "Software\\Citrix\\XenTools";
        private const string XcInstallDirName = "Install_Dir";
        private const string XcInstallSubDir = "Citrix\\XenTools";
        private const string ProgramFilesDefault = "C:\\Program Files";
        private const string XcDiagExe = "xcdiag.exe";
        private const string XcDiagOutput = "xcdiag.zip";
        private const string XcDiagArgumentsFormat = "-batch -log -out \"{0}\"";
        private const string XcDiagQuickArgument = "-quick ";
        private const string QuickMode = "quick";

        #region Fields
        private readonly Udbus.Core.Logging.ILog log;
        static private uint XcDiagRequestNum = 0;
        #endregion // Fields

        #region Internal structs/classes
        private class XcDiagProcessInfo
        {
            internal readonly System.Diagnostics.ProcessStartInfo processStartInfo;
            internal readonly string xcDiagOutputPath;

            internal XcDiagProcessInfo(string xcDiagPath, string xcDiagOutputPath, string mode)
            {
                this.xcDiagOutputPath = xcDiagOutputPath;
                this.processStartInfo = new System.Diagnostics.ProcessStartInfo(xcDiagPath, xcDiagOutputPath);
                this.processStartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(xcDiagPath);
                this.processStartInfo.Arguments = string.Format(XcDiagArgumentsFormat, xcDiagOutputPath);
                if (mode == QuickMode)
                {
                    this.processStartInfo.Arguments = XcDiagQuickArgument + this.processStartInfo.Arguments;
                }
            }
        } // Ends class XcDiagProcessInfo

        private struct RunInThreadArgs
        {
            internal readonly string uuid;
            internal readonly string mode;

            internal RunInThreadArgs(string uuid, string mode)
            {
                this.uuid = uuid;
                this.mode = mode;
            }
        } // Ends struct RunInThreadArgs
        #endregion // Internal structs/classes

        #region Constructors
        private XcDiagRunner(Udbus.Core.Logging.ILog log)
        {
            this.log = log;
        }

        internal XcDiagRunner()
            : this(Logging.LogCreation.CreateXcDiagLogger())
        {
        }

        internal XcDiagRunner(System.Diagnostics.EventLog eventLog)
            : this(Logging.LogCreation.CreateXcDiagLogger(eventLog))
        {
        }
        #endregion // Constructors

        #region Guts
        private static string FindXcInstallDir()
        {
            string xcInstallDir = null;
            string programFilesDir = System.Environment.GetEnvironmentVariable("PROGRAMFILES");
            if (!string.IsNullOrEmpty(programFilesDir))
            {
                xcInstallDir = System.IO.Path.Combine(programFilesDir, XcInstallSubDir);
            }
            else
            {
                xcInstallDir = System.IO.Path.Combine(ProgramFilesDefault, XcInstallSubDir);
            }
            Microsoft.Win32.RegistryKey xcInstallKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(XcInstallRegkey);
            if (xcInstallKey != null)
            {
                string xcLookupInstallDir = (string)xcInstallKey.GetValue(XcInstallDirName);

                if (!string.IsNullOrEmpty(xcLookupInstallDir))
                {
                    xcInstallDir = xcLookupInstallDir;
                }
            }

            return xcInstallDir;
        }

        // .Net 3.5, why can't you have thought of this ?
        private static string Combine(string path1, params string[] paths)
        {
            string result = path1;

            if (paths.Length > 0)
            {
                for (int pathIter = 0; pathIter < paths.Length; ++pathIter)
                {
                    result = System.IO.Path.Combine(result, paths[pathIter]);
                }
            }
            return result;
        }

        private static bool FindXcDiagCommandParameters(out string xcdiagPath, out string xcdiagOutputPath)
        {
            xcdiagPath = null;
            xcdiagOutputPath = null;
            string xcInstallDir = FindXcInstallDir();

            if (!string.IsNullOrEmpty(xcInstallDir))
            {
                xcdiagPath = System.IO.Path.Combine(xcInstallDir, XcDiagExe);

                if (!System.IO.File.Exists(xcdiagPath))
                {
#if DEBUG
                    // Have a look in the build directory.
                    string exeDir = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                    string configurationSubDir = System.IO.Path.GetFileName(exeDir);
                    string exeRelativeXcDiagDir = System.IO.Path.GetFullPath(Combine("..", "..", "..", "..", "XenGuestAgent", configurationSubDir));
                    string exeRelativeXcDiagExe = System.IO.Path.Combine(exeRelativeXcDiagDir, "xcdiag.exe");
                    if (System.IO.File.Exists(exeRelativeXcDiagExe))
                    {
                        xcdiagPath = exeRelativeXcDiagExe;
                        xcdiagOutputPath = System.IO.Path.Combine(exeDir, XcDiagOutput);
                    }
#endif // DEBUG
                }
                else
                {
                    xcdiagOutputPath = System.IO.Path.Combine(xcInstallDir, XcDiagOutput);
                }
            }

            return (!string.IsNullOrEmpty(xcdiagOutputPath) && !string.IsNullOrEmpty(xcdiagPath));
        }

        private XcDiagProcessInfo GetXcDiagProcessInfo(string mode)
        {
            XcDiagProcessInfo xcdiagProcessInfo = null;
            string xcDiagPath, xcDiagOutputPath;
            if (FindXcDiagCommandParameters(out xcDiagPath, out xcDiagOutputPath))
            {
                if (!System.IO.File.Exists(xcDiagPath))
                {
                    log.Error("Unable to find xcdiag with path: \"{0}\"", xcDiagPath);
                }
                else
                {
                    xcdiagProcessInfo = new XcDiagProcessInfo(xcDiagPath, xcDiagOutputPath, mode);
                }
            }

            return xcdiagProcessInfo;
        }

        private void Log_Io_Debug(IntPtr logpriv, String buf)
        {
            log.Info(buf);
        }

        private void RunInThread(object args)
        {
            if (args == null)
            {
                throw new System.ArgumentNullException("args cannot be null");
            }
            RunInThreadArgs runArgs = (RunInThreadArgs)args;
            if (args == null)
            {
                throw new ArgumentException("args should be a RunInThreadArgs");
            }

            this.Run(runArgs.uuid, runArgs.mode);
        }

        private void Gather(string uuid, string data)
        {
            ++XcDiagRequestNum;
            string filename = string.Format("xcdiag-{0}-{1}.zip", uuid, XcDiagRequestNum);
            Udbus.Core.ServiceConnectionParams serviceConnectionParams;
            using (com.citrix.xenclient.xenmgr.diag.diagService diag = CreateDbusDiag(out serviceConnectionParams, log, Log_Io_Debug))
            {
                // This file is pretty big, so let's try not to bust dbus.
                diag.BodyLength += checked((uint)(data.Length + filename.Length));
                diag.gather(filename, data);
                log.Info("xenmgr.diag gathered file \"{0}\"", filename);
            }
        }

        #endregion // Guts

        #region Dbus diag creation
        internal static com.citrix.xenclient.xenmgr.diag.diagService CreateDbusDiag(out Udbus.Core.ServiceConnectionParams serviceConnectionParams,
            Udbus.Core.Logging.ILog log, Udbus.Serialization.UdbusDelegates.D_io_debug io_debug)
        {
            // Create a V4V connection.
            Udbus.v4v.v4vConnection connection;
            System.Threading.ManualResetEvent stop = new System.Threading.ManualResetEvent(false);
            DbusHosts.GetV4vConnection(out connection, out serviceConnectionParams,
                io_debug, stop,
                log
            );

            // Use dbus interface to xenmgr.diag.
            Udbus.Serialization.DbusConnectionParameters dbusConnectionParameters = com.citrix.xenclient.xenmgr.diag.diagService.DefaultConnectionParameters;
            dbusConnectionParameters.Destination = "com.citrix.xenclient.xenmgr";
            com.citrix.xenclient.xenmgr.diag.diagService diag = com.citrix.xenclient.xenmgr.diag.diagService.Create(serviceConnectionParams, dbusConnectionParameters);

            return diag;
        }

        internal com.citrix.xenclient.xenmgr.diag.diagService CreateDbusDiag(out Udbus.Core.ServiceConnectionParams serviceConnectionParams)
        {
            return CreateDbusDiag(out serviceConnectionParams, this.log, this.Log_Io_Debug);
        }
        #endregion // Dbus diag creation

        #region Run
        internal System.Threading.Thread RunAsync(string uuid, string mode)
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RunInThread));
            RunInThreadArgs runArgs = new RunInThreadArgs(uuid, mode);
            thread.Start(runArgs);
            return thread;
        }

        internal bool Run(string uuid, string mode)
        {
            bool run = false;
            XcDiagProcessInfo xcdiagProcessInfo = GetXcDiagProcessInfo(mode);

            if (xcdiagProcessInfo != null) // If got xcdiag process info
            {
                this.log.Info("Received xcdiag request");
                xcdiagProcessInfo.processStartInfo.CreateNoWindow = true;
                xcdiagProcessInfo.processStartInfo.ErrorDialog = false;
                xcdiagProcessInfo.processStartInfo.RedirectStandardError = true;
                xcdiagProcessInfo.processStartInfo.RedirectStandardOutput = true;
                xcdiagProcessInfo.processStartInfo.UseShellExecute = false;
                xcdiagProcessInfo.processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                System.Diagnostics.Process xcdiagProcess = null;
                try
                {
                    xcdiagProcess = System.Diagnostics.Process.Start(xcdiagProcessInfo.processStartInfo);
                    if (xcdiagProcess == null)
                    {
                        log.Info("xcdiag process already running in: \"{0}\"", xcdiagProcessInfo.processStartInfo.FileName);
                    }
                }
                catch (System.IO.FileNotFoundException filenotfoundEx)
                {
                    log.Exception("Cannot launch xcdiag (file not found)", filenotfoundEx);
                }
                catch (System.ObjectDisposedException objdisposedEx)
                {
                    log.Exception("Cannot launch xcdiag (object disposed)", objdisposedEx);
                }
                catch (System.InvalidOperationException invalidopEx)
                {
                    log.Exception("Cannot launch xcdiag (invalid operation)", invalidopEx);
                }
                catch (System.ComponentModel.Win32Exception win32Ex)
                {
                    log.Exception("Cannot launch xcdiag (win32 error)", win32Ex);
                }
                catch (System.Exception ex)
                {
                    log.Exception("Cannot launch xcdiag", ex);
                }

                if (xcdiagProcess != null) // If launched xcdiag
                {
                    // Tends to block until process finishes.
                    string xcdiagError = xcdiagProcess.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(xcdiagError))
                    {
                        log.Error("xcdiag produced error output: {0}", xcdiagError);
                    }

                    xcdiagProcess.WaitForExit(); // Oooooh.

                    if (xcdiagProcess.ExitCode != 0) // If xcdiag reported an error
                    {
                        log.Error("xcdiag exited with error code '{0}'", xcdiagProcess.ExitCode);
    
                    } // Ends if xcdiag reported an error
                    else // else xcdiag ran ok
                    {
                        System.Text.StringBuilder dataBuffer = null;
                        // Make a big ol' string containing the data, encoding each byte as 2-digit ASCII hex.
#if MEMORYMAPFILE_SUPPORTED
                        using(System.IO.MemoryMappedFiles.MemoryMappedFile memmapXcDiag = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(xcdiagProcessInfo.xcDiagOutputPath))
#else // !MEMORYMAPFILE_SUPPORTED
                        //// Create a memory mapped view of the zip file
                        //hFile = CreateFileA(szXcDiagOut, 
                        //    GENERIC_READ | GENERIC_WRITE,
                        //    0, 
                        //    NULL,
                        //    OPEN_EXISTING, 
                        //    FILE_ATTRIBUTE_NORMAL, 
                        //    NULL);
                        using (Microsoft.Win32.SafeHandles.SafeFileHandle mapHandle = wcf.Native.FileFunctions.CreateFile(xcdiagProcessInfo.xcDiagOutputPath
                            , global::XenClientGuestService.Native.EFileAccess.FILE_GENERIC_READ | global::XenClientGuestService.Native.EFileAccess.FILE_GENERIC_WRITE
                            , System.IO.FileShare.None
                            , IntPtr.Zero
                            , System.IO.FileMode.Open
                            , System.IO.FileAttributes.Normal
                            , IntPtr.Zero))
#endif // MEMORYMAPFILE_SUPPORTED
                        {
                            // Finish creating the native mapping, and then work out how to read bytes...
#if MEMORYMAPFILE_SUPPORTED
                            using (System.IO.MemoryMappedFiles.MemoryMappedViewStream streamXcDiag = memmapXcDiag.CreateViewStream())
#else // !MEMORYMAPFILE_SUPPORTED
                            long fileSize = new System.IO.FileInfo(xcdiagProcessInfo.xcDiagOutputPath).Length;
                            //hMapFile = CreateFileMapping(
                            //    hFile,          // current file handle
                            //    NULL,           // default security
                            //    PAGE_READWRITE, // read/write permission
                            //    0,              // size of mapping object, high
                            //    filesize,       // size of mapping object, low
                            //    NULL);          // name of mapping object
                            using (Microsoft.Win32.SafeHandles.SafeFileHandle xcDiagOutputMappingHandle = wcf.Native.MappingFunctions.CreateFileMapping(mapHandle
                                    , IntPtr.Zero
                                    , global::XenClientGuestService.Native.FileMapProtection.PageReadWrite
                                    , 0
                                    , checked((uint)fileSize)
                                    , null))
#endif // MEMORYMAPFILE_SUPPORTED
                            {
#if MEMORYMAPFILE_SUPPORTED
                                long fileSize = streamXcDiag.Length;
#else // !MEMORYMAPFILE_SUPPORTED
                                //// Map the view
                                //lpMapAddress = (unsigned char *)MapViewOfFile(
                                //    hMapFile,			 // handle to mapping object
                                //    FILE_MAP_ALL_ACCESS, // read/write 
                                //    0,                   // high-order 32 bits of file offset
                                //    0,                   // low-order 32 bits of file offset
                                //    filesize);           // number of bytes to map
                                using (wcf.Native.FileMappingViewHandle mappingViewHandler = wcf.Native.MappingFunctions.MapViewOfFile(xcDiagOutputMappingHandle
                                    , global::XenClientGuestService.Native.FileMapAccess.FileMapAllAccess
                                    , 0
                                    , 0
                                    , (checked((UInt32)fileSize))))
#endif // !MEMORYMAPFILE_SUPPORTED
                                {
#if MEMORYMAPFILE_SUPPORTED
                                    System.IO.BinaryReader reader = new System.IO.BinaryReader(streamXcDiag);
#else // !MEMORYMAPFILE_SUPPORTED
                                    wcf.Native.FileMappingViewHandle.ViewReader reader = mappingViewHandler.CreateViewReader();
#endif // !MEMORYMAPFILE_SUPPORTED
                                    int resultSize = checked((int)(fileSize * 2));
                                    dataBuffer = new StringBuilder(resultSize, resultSize);
                                    for (int byteIter = 0; byteIter < fileSize; ++byteIter)
                                    {
                                        byte b = reader.ReadByte();
                                        dataBuffer.Append(b.ToString("x2"));

                                    } // Ends loop over bytes
                                } // Ends using map view handle
                            } // Ends using Memory Map view
                        } // Ends using Memory Map file

                        string data = dataBuffer.ToString();
                        byte[] stringBytes = System.Text.Encoding.Default.GetBytes(data);
                        byte[] asciiBytes = System.Text.ASCIIEncoding.Convert(Encoding.Default, Encoding.ASCII, stringBytes);
                        data = System.Text.Encoding.ASCII.GetString(asciiBytes);
                        Gather(uuid, data);

                    } // Ends else xcdiag ran ok
                } // Ends if launched xcdiag
            } // Ends if got xcdiag process info

            return run;
        }
        #endregion // Run

    } // Ends class XcDiagRunner
}
