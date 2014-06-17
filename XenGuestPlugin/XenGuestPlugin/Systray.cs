//
// Copyright (c) 2010 Citrix Systems, Inc.
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace XenGuestPlugin
{
    public partial class Systray : Form
    {
        #region Constants
        private const int balloonTimeout = 30 * 1000; //30 seconds -> milliseconds
        #endregion

        #region Variables
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static XenGuestServices xgs;
        private static List<Alert> alertList;
        private static VMSwitcher vmSwitcher;
        #endregion

        #region Constructor
        public Systray()
        {
            InitializeComponent();
            xgs = Program.GetXGS();
            alertList = Program.GetAlertList();
            initRunOnStartup();

            //Event handlers
            this.notifyIcon1.BalloonTipClicked += new EventHandler(notifyIcon1_BalloonTipClicked);
        }
        #endregion

        #region Methods

        private void initRunOnStartup()
        {
            enableToolbarToolStripMenuItem.Visible = false;
            disableToolbarToolStripMenuItem.Visible = true;
            vmSwitcher = new VMSwitcher();
            vmSwitcher.Show();

            if (xgs.ToggleAutorun(false))
            {
                enableRunOnStartupToolStripMenuItem.Visible = false;
                disableRunOnStartupToolStripMenuItem.Visible = true;
            }
            else
            {
                enableRunOnStartupToolStripMenuItem.Visible = true;
                disableRunOnStartupToolStripMenuItem.Visible = false;
            }
        }

        //public void fireAlertMethod(Alert alert)
        //{
        //    if (alert != null)
        //    {
        //        if (alert.Reboot == false)
        //        {
        //            try
        //            {
        //                notifyIcon1.ShowBalloonTip(balloonTimeout, alert.TipTitle, alert.TipText, alert.TipIcon);
        //            }
        //            catch (Exception ex)
        //            {
        //                log.Error(ex);
        //                log.Error("Malformed alert data for alert ID " + alert.AlertId);
        //                MessageBox.Show("Malformed alert data for alert ID " + alert.AlertId);
        //            }
        //        }
        //        else
        //        {
        //            Reboot reboot = new Reboot(alert.TipTitle, alert.TipText);
        //            reboot.Show();
        //        }
        //    }
        //    else
        //    {
        //        log.Error("Alert ID " + alert.AlertId + " not found.");
        //    }
        //}

        public void showAlert(Alert alert, Array alertParams)
        {
            switch (alert.AlertId)
            {
                //NOTE: You can stack cases, e.g.
                //case 0:
                case 1:
                    notifyIcon1.ShowBalloonTip(balloonTimeout, alert.TipTitle,
                        String.Format(alert.TipText, (string)alertParams.GetValue(2)), //Display disk % free
                        alert.TipIcon);
                    break;
                case 6:
                    Reboot reboot = new Reboot(alert.TipTitle, alert.TipText);
                    reboot.Show();
                    break;
                default:
                    notifyIcon1.ShowBalloonTip(balloonTimeout, alert.TipTitle,
                        alert.TipText, alert.TipIcon);
                    break;
            }
        }
        #endregion

        #region Event Handlers
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            //Possibly switch to relevant VM on click
        }

        private void enableRunOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (xgs.ToggleAutorun(true))
            {
                enableRunOnStartupToolStripMenuItem.Visible = false;
                disableRunOnStartupToolStripMenuItem.Visible = true;
            }
        }

        private void disableRunOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!xgs.ToggleAutorun(true))
            {
                disableRunOnStartupToolStripMenuItem.Visible = false;
                enableRunOnStartupToolStripMenuItem.Visible = true;
            }
        }

        private void enableToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableToolbarToolStripMenuItem.Visible = false;
            disableToolbarToolStripMenuItem.Visible = true;
            vmSwitcher = new VMSwitcher();
            vmSwitcher.Show();
        }

        private void disableToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disableToolbarToolStripMenuItem.Visible = false;
            enableToolbarToolStripMenuItem.Visible = true;
            vmSwitcher.Close();
            vmSwitcher.Dispose();
        }
        #endregion
    }
}
