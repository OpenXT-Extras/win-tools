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

namespace XenGuestPlugin
{
    partial class Reboot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Reboot));
            this.panel1 = new System.Windows.Forms.Panel();
            this.rebootTitle = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.postponeButton = new System.Windows.Forms.Button();
            this.restartButton = new System.Windows.Forms.Button();
            this.rebootText = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.remindCombo = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gold;
            this.panel1.Controls.Add(this.rebootTitle);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // rebootTitle
            // 
            this.rebootTitle.BackColor = System.Drawing.Color.Gold;
            this.rebootTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.rebootTitle, "rebootTitle");
            this.rebootTitle.Name = "rebootTitle";
            this.rebootTitle.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel2.Controls.Add(this.postponeButton);
            this.panel2.Controls.Add(this.restartButton);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // postponeButton
            // 
            resources.ApplyResources(this.postponeButton, "postponeButton");
            this.postponeButton.Name = "postponeButton";
            this.postponeButton.UseVisualStyleBackColor = true;
            this.postponeButton.Click += new System.EventHandler(this.postponeButton_Click);
            // 
            // restartButton
            // 
            resources.ApplyResources(this.restartButton, "restartButton");
            this.restartButton.Name = "restartButton";
            this.restartButton.UseVisualStyleBackColor = true;
            this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // rebootText
            // 
            this.rebootText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.rebootText, "rebootText");
            this.rebootText.Name = "rebootText";
            this.rebootText.TabStop = false;
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textBox3, "textBox3");
            this.textBox3.Name = "textBox3";
            this.textBox3.TabStop = false;
            // 
            // remindCombo
            // 
            this.remindCombo.BackColor = System.Drawing.SystemColors.Window;
            this.remindCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.remindCombo.FormattingEnabled = true;
            resources.ApplyResources(this.remindCombo, "remindCombo");
            this.remindCombo.Name = "remindCombo";
            // 
            // Reboot
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.remindCombo);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.rebootText);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Reboot";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Reboot_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox rebootTitle;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox rebootText;
        private System.Windows.Forms.Button restartButton;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button postponeButton;
        private System.Windows.Forms.ComboBox remindCombo;
    }
}
