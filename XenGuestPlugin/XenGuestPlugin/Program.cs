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
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;
using XenGuestPlugin.Properties;
using XenGuestPlugin.Services;
using XenGuestPlugin.Services.Development;
using XenGuestPlugin.Services.XenClientGuest;
using XenGuestPlugin.Types;

namespace XenGuestPlugin
{
    static class Program
    {
        private static log4net.ILog log;
        private static System.Threading.Mutex exclusiveAppMtx;
        private static TrayIcon _systray;
        private static List<Alert> alertList = new List<Alert>();
        private static Assembly _assembly;
        private static StreamReader alertReader;
        private static RegistryMonitor monitor = new RegistryMonitor(RegistryHive.CurrentUser, "Control Panel\\Mouse");
        private static IHostService hostService = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool ok;

            // Only allow one instance of the switcher to start
            exclusiveAppMtx = new System.Threading.Mutex(true, "{D39FAB11-ED23-478d-AF57-C638F79A7BA8}", out ok);
            if (!ok)
            {
                return;
            }

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(Assembly.GetExecutingAssembly().Location + ".config"));
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Info("Application Started");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            Application.ApplicationExit += Application_ApplicationExit;
            monitor.RegChanged += new EventHandler(OnRegChanged);
            monitor.Error += new System.IO.ErrorEventHandler(OnRegMonError);

            try
            {
                log.Info("Starting plugin...");

                //Populate notifications from XML file 
                _assembly = Assembly.GetExecutingAssembly();
                alertReader = new StreamReader(_assembly.GetManifestResourceStream(Resources.ResourceManager.GetString("strings.xml")));
                XmlSerializer deserializer = new XmlSerializer(typeof(List<Alert>));
                TextReader textReader = alertReader;
                alertList = (List<Alert>)deserializer.Deserialize(textReader);
                textReader.Close();
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                MessageBox.Show(Resources.ResourceManager.GetString("Error loading"));
            }

            if (SetupPlugin())
            {
                OnRegChanged(null, EventArgs.Empty);                
                _systray = new TrayIcon();
                Application.Run();
            }
        }

        private static void OnRegMonError(object sender, ErrorEventArgs e)
        {
            log.Debug("Registry monitoring error: ", e.GetException());
            StopRegistryMonitor();
        }

        private static void OnRegChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser;
            object accelVal = null;
            log.Debug("HKCU\\Control Panel\\Mouse key change detected");

            try
            {
                key = key.OpenSubKey("Control Panel\\Mouse", false);
                if (key != null)
                {
                    accelVal = key.GetValue("MouseSensitivity");
                    log.Debug("MouseSensitivity: " + accelVal.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception reading Mouse key", ex);
            }

            try
            {
                if (accelVal != null)
                    hostService.SetAcceleration(Convert.ToInt32(accelVal.ToString()));
                
            }
            catch (Exception ex)
            {
                log.Error("Failed to set accleration: ", ex);
            }
        }

        private static void StopRegistryMonitor()
        {
            if (monitor != null)
            {
                monitor.Stop();
                monitor.RegChanged -= new EventHandler(OnRegChanged);
                monitor.Error -= new System.IO.ErrorEventHandler(OnRegMonError);
                monitor = null;
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {         
            StopRegistryMonitor();

            if (hostService != null)
            {
                hostService.Dispose();
                hostService = null;
            }
        }

        private static bool SetupPlugin()
        {
            string hostServiceSetting = ConfigurationManager.AppSettings["hostService"];
            if (hostServiceSetting == null)
            {
                hostServiceSetting = "XenClientGuest";
            }

            try
            {
                switch (hostServiceSetting)
                {
                    case "Development":
                        {
                            hostService = new DevelopmentHostService();
                            break;
                        }
                    case "XenGuestAgent":
                        {
                            hostService = new XenGuestAgentHostService();
                            break;
                        }
                    case "XenClientGuest":
                        {
                            try
                            {
                                hostService = new XenClientGuestHostService();
                            }
                            catch (Exception e)
                            {
                                log.Info(e);
#if DEBUG
                                System.Windows.Forms.MessageBox.Show(string.Format("Using XenGuestAgent, error encountered:\n\n{0}", e.Message));
#endif
                                hostService = new XenGuestAgentHostService();
                            }
                            break;
                        }
                }

                monitor.Start();
                hostService.NewAlert += hostService_NewAlert;
                log.Info("Plugin setup!");
                return true;
            }
            catch (Exception e)
            {
                log.Debug("Failed to initialize", e);
                MessageBox.Show(Resources.ResourceManager.GetString("Failed to initialize"), Resources.ResourceManager.GetString("XenClient Guest Notifications Plug-in"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            return false;
        }

        static void hostService_NewAlert(string iface, string member, string[] args)
        {
            Alert alert = LookupAlertCode(iface, member);
            if (alert != null)
            {
                _systray.showAlert(alert, args);
            }
            else
            {
                log.Debug("Failed to find alert");
            }
        }

        private static Alert LookupAlertCode(string aInterface, string aMember)
        {
            return alertList.Find(delegate(Alert a) { return ((a.Interface == aInterface) && (a.Member == aMember)); });
        }

        public static IHostService GetHostService()
        {
            return hostService;
        }

        public static List<Alert> GetAlertList()
        {
            return alertList;
        }

        #region Logging Rubbish

        public static string GetLogFile()
        {
            foreach (log4net.Appender.IAppender appender in log.Logger.Repository.GetAppenders())
            {
                if (appender is log4net.Appender.FileAppender)
                {
                    // Assume that the first FileAppender is the primary log file.
                    return ((log4net.Appender.FileAppender)appender).File;
                }
            }

            return "MISSING LOG FILE!";
        }

        private static void HandleException(Exception e)
        {
            try
            {
                if (e != null)
                {
                    log.Fatal("Uncaught exception", e);
                }
                else
                {
                    log.Fatal("Fatal error");
                }

            }
            catch (Exception exception)
            {
                try
                {
                    log.Fatal("Fatal error while handling fatal error!", exception);
                }
                catch
                {
                }
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal("Unhandeled exception");
            HandleException(e.ExceptionObject as Exception);
        }

        internal static void AssertOffEventThread()
        {
            if (Program._systray != null && false)
            {
                string msg = String.Format("AssertOffEventThread",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), GetLogFile());
                log.Fatal(msg + "\n" + Environment.StackTrace);
            }
        }

        internal static void AssertOnEventThread()
        {
            if (Program._systray != null && false)
            {
                string msg = String.Format("AssertOnEventThread",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), GetLogFile());
                log.Fatal(msg + "\n" + Environment.StackTrace);
            }
        }

        #endregion
    }
}
