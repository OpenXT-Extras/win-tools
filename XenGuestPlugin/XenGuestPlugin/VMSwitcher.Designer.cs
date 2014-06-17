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

using Microsoft.Win32;
namespace XenGuestPlugin
{
    partial class VMSwitcher
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.PictureBox pbPref;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VMSwitcher));
            this.pbUSB = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.lblUSB = new System.Windows.Forms.Label();
            this.workingAreaTimer = new System.Windows.Forms.Timer(this.components);
            this.USBMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            pbPref = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(pbPref)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbUSB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // pbPref
            // 
            pbPref.BackColor = System.Drawing.Color.Transparent;
            pbPref.Cursor = System.Windows.Forms.Cursors.Default;
            pbPref.Image = global::XenGuestPlugin.Properties.Resources.xenclient;
            resources.ApplyResources(pbPref, "pbPref");
            pbPref.Name = "pbPref";
            pbPref.TabStop = false;
            pbPref.Click += new System.EventHandler(this.configButton_Click);
            pbPref.MouseLeave += new System.EventHandler(this.pbPref_MouseLeave);
            pbPref.MouseHover += new System.EventHandler(this.pbPref_MouseHover);
            // 
            // pbUSB
            // 
            resources.ApplyResources(this.pbUSB, "pbUSB");
            this.pbUSB.BackColor = System.Drawing.Color.Transparent;
            this.pbUSB.Image = global::XenGuestPlugin.Properties.Resources._000_ToolBar_USB_Icon_up;
            this.pbUSB.Name = "pbUSB";
            this.pbUSB.TabStop = false;
            this.pbUSB.Click += new System.EventHandler(this.pbUSB_Click);
            this.pbUSB.MouseLeave += new System.EventHandler(this.pbUSB_MouseLeave);
            this.pbUSB.MouseHover += new System.EventHandler(this.pbUSB_MouseHover);
            // 
            // pictureBox3
            // 
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            this.pictureBox3.MouseEnter += new System.EventHandler(this.pictureBox3_MouseEnter);
            this.pictureBox3.MouseLeave += new System.EventHandler(this.pictureBox3_MouseLeave);
            // 
            // lblUSB
            // 
            resources.ApplyResources(this.lblUSB, "lblUSB");
            this.lblUSB.BackColor = System.Drawing.Color.Transparent;
            this.lblUSB.ForeColor = System.Drawing.Color.White;
            this.lblUSB.Name = "lblUSB";
            // 
            // workingAreaTimer
            // 
            this.workingAreaTimer.Interval = 1000;
            this.workingAreaTimer.Tick += new System.EventHandler(this.workingAreaTimer_Tick);
            // 
            // USBMenu
            // 
            this.USBMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            resources.ApplyResources(this.USBMenu, "USBMenu");
            this.USBMenu.Name = "USBMenu";
            this.USBMenu.ShowCheckMargin = true;
            this.USBMenu.ShowImageMargin = false;
            // 
            // VMSwitcher
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.lblUSB);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pbUSB);
            this.Controls.Add(pbPref);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "VMSwitcher";
            this.Opacity = 0.9D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.TopMost = true;
            this.MouseLeave += new System.EventHandler(this.VMSwitcher_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(pbPref)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbUSB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbUSB;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label lblUSB;
        private System.Windows.Forms.Timer workingAreaTimer;
        private System.Windows.Forms.ContextMenuStrip USBMenu;
    }
}
