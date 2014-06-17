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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using XenGuestPlugin.Properties;
using XenGuestPlugin.Types;

namespace XenGuestPlugin
{
    public partial class VmButton : UserControl
    {
        #region Variables

        private XenVM _VM;
        private Image _Image;
        private bool _Current;

        #endregion

        #region Constructors

        public VmButton(XenVM vm, Image img, bool current)
        {
            InitializeComponent();

            // XC-7132, Korean OS does not honour designer/resource sizes
            this.Size = new Size(105, 64);

            this._VM = vm;
            this._Image = img;
            this.lblName.Text = vm.Name;
            this._Current = current;

            if (current)
            {
                this.lblName.Font = new Font(lblName.Font, FontStyle.Bold);
            }
            else
            {
                this.lblName.Font = new Font(lblName.Font, FontStyle.Regular);
            }

            if (this.lblName.PreferredWidth > this.lblName.Width)
            {
                this.toolTip.SetToolTip(this.lblName, vm.Name);
            }
            else
            {
                this.toolTip.SetToolTip(this.lblName, null);
            }
        }

        #endregion

        #region Event Handlers

        private void VmButton_Click(object sender, EventArgs e)
        {
            if (!_Current)
            {
                VMSwitcher parent = this.Parent as VMSwitcher;
                if (parent != null)
                {
                    parent.SwitchVM(_VM);
                }
            }
        }

        private void lblName_Click(object sender, EventArgs e)
        {
            VmButton_Click(sender, e);
        }

        void lblName_MouseLeave(object sender, System.EventArgs e)
        {
            if (!_Current)
            {
                VMSwitcher parent = this.Parent as VMSwitcher;
                if (parent != null)
                {
                    parent.CheckMouseLeave();
                }
            }
        }

        private void VmButton_Paint(object sender, PaintEventArgs e)
        {
            if (this._Current)
            {
                e.Graphics.DrawImage(Resources.bevel_selector, 0, 0, 105, 64);
            }

            e.Graphics.DrawImage(this._Image, 30, 3, 45, 45);
        }

        #endregion
    }
}
