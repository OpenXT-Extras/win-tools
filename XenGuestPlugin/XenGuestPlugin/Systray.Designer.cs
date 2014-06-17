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

namespace XenGuestPlugin
{
    partial class Systray
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Systray));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableRunOnStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableRunOnStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableToolbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableToolbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "XenClient Alerts Plug-in";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableToolbarToolStripMenuItem,
            this.disableToolbarToolStripMenuItem,
            this.enableRunOnStartupToolStripMenuItem,
            this.disableRunOnStartupToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(191, 136);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // enableRunOnStartupToolStripMenuItem
            // 
            this.enableRunOnStartupToolStripMenuItem.Name = "enableRunOnStartupToolStripMenuItem";
            this.enableRunOnStartupToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.enableRunOnStartupToolStripMenuItem.Text = "Enable run on startup";
            this.enableRunOnStartupToolStripMenuItem.Click += new System.EventHandler(this.enableRunOnStartupToolStripMenuItem_Click);
            // 
            // disableRunOnStartupToolStripMenuItem
            // 
            this.disableRunOnStartupToolStripMenuItem.Name = "disableRunOnStartupToolStripMenuItem";
            this.disableRunOnStartupToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.disableRunOnStartupToolStripMenuItem.Text = "Disable run on startup";
            this.disableRunOnStartupToolStripMenuItem.Click += new System.EventHandler(this.disableRunOnStartupToolStripMenuItem_Click);
            // 
            // enableToolbarToolStripMenuItem
            // 
            this.enableToolbarToolStripMenuItem.Name = "enableToolbarToolStripMenuItem";
            this.enableToolbarToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.enableToolbarToolStripMenuItem.Text = "Enable toolbar";
            this.enableToolbarToolStripMenuItem.Click += new System.EventHandler(this.enableToolbarToolStripMenuItem_Click);
            // 
            // disableToolbarToolStripMenuItem
            // 
            this.disableToolbarToolStripMenuItem.Name = "disableToolbarToolStripMenuItem";
            this.disableToolbarToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.disableToolbarToolStripMenuItem.Text = "Disable toolbar";
            this.disableToolbarToolStripMenuItem.Click += new System.EventHandler(this.disableToolbarToolStripMenuItem_Click);
            // 
            // Systray
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Systray";
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableRunOnStartupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableRunOnStartupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableToolbarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableToolbarToolStripMenuItem;
    }
}
