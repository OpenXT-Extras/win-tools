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
using System.Windows.Forms;
using Microsoft.Win32;
using XenGuestPlugin.Services;
using XenGuestPlugin.Types;

namespace XenGuestPlugin
{
    public partial class TrayIcon
    {
        #region Constants
        private const int balloonTimeout = 30 * 1000; //30 seconds -> milliseconds
        private const string xcpKeyPath = "SOFTWARE\\Citrix\\XenGuestPlugin";
        #endregion

        #region Variables
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static IHostService hostService;
        private static List<Alert> alertList;
        private static VMSwitcher vmSwitcher;
        private static System.Windows.Forms.NotifyIcon notifyIcon1;
        private static System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private static System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private static System.Windows.Forms.ToolStripMenuItem enableRunOnStartupToolStripMenuItem;
        private static System.Windows.Forms.ToolStripMenuItem disableRunOnStartupToolStripMenuItem;
        private static System.Windows.Forms.ToolStripMenuItem enableToolbarToolStripMenuItem;
        private static System.Windows.Forms.ToolStripMenuItem disableToolbarToolStripMenuItem;
        private static System.ComponentModel.IContainer components = null;
        #endregion

        #region Constructor
        public TrayIcon()
        {
            InitializeComponent();
            hostService = Program.GetHostService();
            alertList = Program.GetAlertList();
            initRunOnStartup();

            //Event handlers
            notifyIcon1.BalloonTipClicked += new EventHandler(notifyIcon1_BalloonTipClicked);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrayIcon));
            notifyIcon1 = new System.Windows.Forms.NotifyIcon(components);
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            enableRunOnStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            disableRunOnStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            enableToolbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            disableToolbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            resources.ApplyResources(exitToolStripMenuItem, "exitToolStripMenuItem");
            exitToolStripMenuItem.Click += new System.EventHandler(exitToolStripMenuItem_Click);
            // 
            // enableRunOnStartupToolStripMenuItem
            // 
            enableRunOnStartupToolStripMenuItem.Name = "enableRunOnStartupToolStripMenuItem";
            enableRunOnStartupToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            resources.ApplyResources(enableRunOnStartupToolStripMenuItem, "enableRunOnStartupToolStripMenuItem");
            enableRunOnStartupToolStripMenuItem.Click += new System.EventHandler(enableRunOnStartupToolStripMenuItem_Click);
            // 
            // disableRunOnStartupToolStripMenuItem
            // 
            disableRunOnStartupToolStripMenuItem.Name = "disableRunOnStartupToolStripMenuItem";
            disableRunOnStartupToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            resources.ApplyResources(disableRunOnStartupToolStripMenuItem, "disableRunOnStartupToolStripMenuItem");
            disableRunOnStartupToolStripMenuItem.Click += new System.EventHandler(disableRunOnStartupToolStripMenuItem_Click);
            // 
            // enableToolbarToolStripMenuItem
            // 
            enableToolbarToolStripMenuItem.Name = "enableToolbarToolStripMenuItem";
            enableToolbarToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            resources.ApplyResources(enableToolbarToolStripMenuItem, "enableToolbarToolStripMenuItem");
            enableToolbarToolStripMenuItem.Click += new System.EventHandler(enableToolbarToolStripMenuItem_Click);
            // 
            // disableToolbarToolStripMenuItem
            // 
            disableToolbarToolStripMenuItem.Name = "disableToolbarToolStripMenuItem";
            disableToolbarToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            resources.ApplyResources(disableToolbarToolStripMenuItem, "disableToolbarToolStripMenuItem");
            disableToolbarToolStripMenuItem.Click += new System.EventHandler(disableToolbarToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            enableToolbarToolStripMenuItem,
            disableToolbarToolStripMenuItem,
            enableRunOnStartupToolStripMenuItem,
            disableRunOnStartupToolStripMenuItem,
            exitToolStripMenuItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(191, 136);
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = ((System.Drawing.Icon)(Properties.Resources._000_XenClient_Combo_Vista.Clone()));
            resources.ApplyResources(notifyIcon1, "notifyIcon1");
            notifyIcon1.Visible = true;
        }

        #endregion

        #region Destructor
        ~TrayIcon()
        {
            components.Dispose();
            vmSwitcher.Dispose();
        }
        #endregion

        #region Methods

        private bool ToggleToolbar(bool doToggle)
        {
            RegistryKey key = Registry.CurrentUser;
            object toolbarVal = 1;

            try
            {
                key = key.OpenSubKey(xcpKeyPath, true);
                if (key != null)
                {
                    toolbarVal = key.GetValue("Toolbar", 1);
                }
            }
            catch (Exception ex)
            {
                log.Debug("Exception reading Toolbar key", ex);
            }


            if (!doToggle)
            {
                if ((int)toolbarVal == 1)
                {
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if ((int)toolbarVal == 1)
                {
                    key.SetValue("Toolbar", 0, RegistryValueKind.DWord);
                    return false;
                }
                else
                {
                    key.SetValue("Toolbar", 1, RegistryValueKind.DWord);
                    return true;
                }
            }
        }

        private void initRunOnStartup()
        {
            if (hostService.SetAutorun(false))
            {
                enableRunOnStartupToolStripMenuItem.Visible = false;
                disableRunOnStartupToolStripMenuItem.Visible = true;
            }
            else
            {
                enableRunOnStartupToolStripMenuItem.Visible = true;
                disableRunOnStartupToolStripMenuItem.Visible = false;
            }

            if (ToggleToolbar(false))
            {
                enableToolbarToolStripMenuItem.Visible = false;
                disableToolbarToolStripMenuItem.Visible = true;
                vmSwitcher = new VMSwitcher();
                vmSwitcher.Show();
            }
            else
            {
                enableToolbarToolStripMenuItem.Visible = true;
                disableToolbarToolStripMenuItem.Visible = false;
            }
        }

        public void showAlert(Alert alert, string[] args)
        {
            switch (alert.AlertId)
            {
                case 6:
                    Reboot reboot = new Reboot(alert.TipTitle, alert.TipText);
                    reboot.Show();
                    break;
                default:
                    notifyIcon1.ShowBalloonTip(balloonTimeout, alert.TipTitle,
                        String.Format(alert.TipText, args),
                        alert.TipIcon);
                    break;
            }
        }
        #endregion

        #region Event Handlers
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            //Possibly switch to relevant VM on click
        }

        private void enableRunOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hostService.SetAutorun(true))
            {
                enableRunOnStartupToolStripMenuItem.Visible = false;
                disableRunOnStartupToolStripMenuItem.Visible = true;
            }
        }

        private void disableRunOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hostService.SetAutorun(true))
            {
                disableRunOnStartupToolStripMenuItem.Visible = false;
                enableRunOnStartupToolStripMenuItem.Visible = true;
            }
        }

        private void enableToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ToggleToolbar(true))
            {
                enableToolbarToolStripMenuItem.Visible = false;
                disableToolbarToolStripMenuItem.Visible = true;
                vmSwitcher = new VMSwitcher();
                vmSwitcher.Show();
            }
        }

        private void disableToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ToggleToolbar(true))
            {
                disableToolbarToolStripMenuItem.Visible = false;
                enableToolbarToolStripMenuItem.Visible = true;
                vmSwitcher.Close();
                vmSwitcher.Dispose();
            }
        }
        #endregion
    }
}
