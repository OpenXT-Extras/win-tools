//
// Copyright (c) 2011 Citrix Systems, Inc.
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
using System.Diagnostics;

namespace XenGuestPlugin
{
    public partial class Reboot : Form
    {
        private static Dictionary<string, int> remindTimes;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Int32 remindInterval;
        private static Timer remindTimer;

        public Reboot(string title, string text)
        {
            InitializeComponent();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Reboot));
            Rectangle r = Screen.PrimaryScreen.WorkingArea;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width, Screen.PrimaryScreen.WorkingArea.Height - this.Height);

            //Dictionary contains reminder durations
            remindTimes = new Dictionary<string, int>();
            remindTimes.Add(resources.GetString("5 Minutes"), 5);
            remindTimes.Add(resources.GetString("15 Minutes"), 15);
            remindTimes.Add(resources.GetString("1 Hour"), 60);
            remindTimes.Add(resources.GetString("4 Hours"), 240);

            //Assigns dictionary as data source for combobox
            remindCombo.DataSource = new BindingSource(remindTimes, null);
            remindCombo.DisplayMember = "Key";
            remindCombo.ValueMember = "Value";
            
            //Sets default dropbox value
            remindCombo.SelectedIndex = 0;

            //Load title and text
            this.rebootTitle.Text = title;
            this.rebootText.Text = text;           
        }

        private void Reboot_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;            
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo rebootProcess = new ProcessStartInfo();
            rebootProcess.CreateNoWindow = true;
            rebootProcess.Arguments = "/r /t 0";
            rebootProcess.WindowStyle = ProcessWindowStyle.Hidden;
            rebootProcess.FileName = "shutdown";
            rebootProcess.UseShellExecute = true;
            Process.Start(rebootProcess);
        }

        private void postponeButton_Click(object sender, EventArgs e)
        {
            remindInterval = Convert.ToInt32(remindCombo.SelectedValue);
            this.Hide();
            setupTimer();
        }

        private void setupTimer()
        {
            remindTimer = new Timer();
            remindTimer.Tick += new EventHandler(remindTimer_Tick);
            remindTimer.Interval = remindInterval * 1000 * 60; //Convert minutes to milliseconds
            remindTimer.Start();
        }

        void remindTimer_Tick(object sender, EventArgs e)
        {
            //Resets default dropbox value
            remindCombo.SelectedIndex = 0;

            this.Show();
            remindTimer.Dispose();
        }
    }
}
