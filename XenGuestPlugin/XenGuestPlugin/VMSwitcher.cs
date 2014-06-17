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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using XenGuestPlugin.Properties;
using XenGuestPlugin.Services;
using XenGuestPlugin.Types;

namespace XenGuestPlugin
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class VMSwitcher : Form
    {
        #region Constants

        // Height of switcherBar not including handle
        private int switcherHeight;

        /// <summary>
        /// Total time of the show/hide animations in milliseconds
        /// </summary>
        private const int ANIMDURATION = 200;
        private const int BUTTON_PADDING = 20;
        private const int BUTTON_TOP_OFFSET = 3;

        #endregion

        #region Variables

        private IHostService hostService;
        private log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<VmButton> buttons;
        private bool hiding = false;
        private long? startAnimTicks = null;
        private bool hidden = false;
        private Rectangle _workingArea = SystemInformation.WorkingArea;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private XenVM uivm = null;

        #endregion

        #region Properties

        /// <summary>
        /// Wraps Location.Y and handles setting its value
        /// </summary>
        private int Y
        {
            get { return Location.Y; }
            set { Location = new Point(Location.X, value); }
        }

        #endregion

        #region Constructor

        public VMSwitcher()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            this.SetSize();

            hostService = Program.GetHostService();

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            timer.Tick += new EventHandler(timer_Tick);

            this.buttons = new List<VmButton>();

            workingAreaTimer.Start();

            Setup();            
        }
        
        #endregion

        #region Methods

        private void SetSize()
        {
            // XC-7132, Korean OS does not honour designer/resource sizes
            this.Size = new Size(_workingArea.Width, 86);
        }

        /// <summary>
        /// Sets up the windows dimensions and position.
        /// </summary>
        private void Setup()
        {
            // Handle pos
            pictureBox3.Top = this.Height - this.pictureBox3.Height;
            pictureBox3.Left = (this.Width - this.pictureBox3.Width) / 2;
            this.switcherHeight = pictureBox3.Top;

            if (hidden)
            {
                this.Location = new Point(_workingArea.Left, _workingArea.Top - this.switcherHeight);
                this.Region = GetRegion();
            }
            else
            {
                this.Location = new Point(_workingArea.Left, _workingArea.Top);
                this.Region = GetRegion();
                HideAnimated(true);
            }

            pictureBox3.Image = (hidden) ? Resources._075_ToolbarTabClosed_h32bit_118x11 : Resources._075_ToolbarTab_h32bit_118x11;

        }

        /// <summary>
        /// Updates the VM Images and buttons
        /// </summary>
        /// <param name="waitCallback">WaitCallback if one is passed through</param>
        private void UpdateVMs(Object waitCallback)
        {
            Program.AssertOffEventThread();
            Invoke((MethodInvoker)RefreshButtons);
        }

        private void RefreshButtons()
        {
            Program.AssertOnEventThread();
            var VMs = hostService.GetVMs();

            while (buttons.Count > 0)
            {
                VmButton button = buttons[0];
                buttons.RemoveAt(0);
                button.Dispose();
            }

            int totalWidth = 0;

            foreach (XenVM vm in VMs.Values)
            {
                if (vm.Slot == 0) // If uivm
                {
                    // Stash it, and skip.
                    this.uivm = vm;
                    continue;

                } // Ends if uivm

                if (vm.Hidden)
                {
                    continue;
                }
                VmButton button = new VmButton(vm, vm.Image, vm.UUID == hostService.UUID);
                //button.Cursor = Cursors.Hand;

                Controls.Add(button);
                buttons.Add(button);

                totalWidth += button.Width + BUTTON_PADDING;
            }

            totalWidth -= BUTTON_PADDING;

            int x = (Width - totalWidth) / 2;

            foreach (VmButton button in buttons)
            {
                button.Location = new Point(x, BUTTON_TOP_OFFSET);
                x += button.Width + BUTTON_PADDING;
            }
        }

        /// <summary>
        /// Defines the shape of the form
        /// </summary>
        /// <returns>The region of the form</returns>
        private Region GetRegion()
        {
            int cornerChop = 4;

            using (GraphicsPath path = new GraphicsPath())
            {
                //           Layout 'Jockstrap'
                //  1*------------------------------*2
                //   |                              |
                // 10*--------*9          4*--------*3
                //            |            |        
                //            *8          5*
                //             \          /
                //             7*--------*6

                path.AddLines(new [] {
                    // 1-2-3
                    new Point(0, _workingArea.Top - this.Y),
                    new Point(this.Width, _workingArea.Top - this.Y),
                    new Point(this.Width, this.switcherHeight),

                    // 4-5-6-7-8-9 Intricate bits round the handle
                    new Point(this.pictureBox3.Right, this.switcherHeight),
                    new Point(this.pictureBox3.Right, this.Height - cornerChop),
                    new Point(this.pictureBox3.Right - cornerChop, Height),
                    new Point(this.pictureBox3.Left + cornerChop, Height),
                    new Point(this.pictureBox3.Left, this.Height - cornerChop),
                    new Point(this.pictureBox3.Left, this.switcherHeight),

                    // 10
                    new Point(0, this.switcherHeight),
                });

                path.CloseFigure();
                return new Region(path);
            }
        }

        /// <summary>
        /// Start the Show animation
        /// </summary>
        private void ShowAnimated()
        {
            if (!hiding)
                return;

            timer.Interval = 1;
            hiding = false;
            timer.Start();
        }

        /// <summary>
        /// Start the Hide animation
        /// </summary>
        /// <param name="onload">If true, the time should have a larger interval</param>
        private void HideAnimated(bool onload)
        {
            if (hiding)
                return;

            if (onload)
                timer.Interval = 1000;
            else
                timer.Interval = 1;

            hiding = true;
            timer.Start();
        }

        /// <summary>
        /// Method available internally to switch to a VM and then hide the form
        /// </summary>
        /// <param name="vm"></param>
        internal void SwitchVM(XenVM vm)
        {
            hostService.Switch(vm);
            HideAnimated(false);
        }

        /// <summary>
        /// Performs a step in the animation sequence
        /// </summary>
        private void AnimationStep()
        {
            if (!startAnimTicks.HasValue)
            {
                startAnimTicks = DateTime.Now.Ticks;
            }
            int end = hiding ? _workingArea.Top - this.switcherHeight : _workingArea.Top;
            int start = hiding ? _workingArea.Top : _workingArea.Top - this.switcherHeight;
            bool again = true;

            long n = DateTime.Now.Ticks - startAnimTicks.Value;
            double state = (double)n / (double)(ANIMDURATION * TimeSpan.TicksPerMillisecond);
            double pos = ((-Math.Cos(state * Math.PI) / 2) + 0.5);
            Y = (int)(start + ((end - start) * pos));

            if ((hiding && Y <= end) || (!hiding && Y >= end) || n > ANIMDURATION * TimeSpan.TicksPerMillisecond) // end condition
            {
                Y = end;
                again = false;
            }

            this.Region = GetRegion();

            if (!again)
            {
                hidden = hiding;
                pictureBox3.Image = (hidden) ? Resources._075_ToolbarTabClosed_h32bit_118x11 : Resources._075_ToolbarTab_h32bit_118x11;
                startAnimTicks = null;
                timer.Stop();
                this.Region = GetRegion();
                CheckMouseLeave();
            }
        }

        /// <summary>
        /// Function that checks if the mouse is still in the forms bounds or not
        /// </summary>
        internal void CheckMouseLeave()
        {
            Point clientMouse = PointToClient(Control.MousePosition);
            Rectangle r = new Rectangle(0, 0, Width, this.switcherHeight);

            if (!r.Contains(clientMouse) && !hidden)
                HideAnimated(false);
        }
        #endregion

        #region Event Handlers

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            this.SetSize();
            Setup();
            UpdateVMs(null);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            AnimationStep();
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            if (this.uivm != null)
            {
                hostService.Switch(this.uivm);
                HideAnimated(false);
            }
        }

        private void pbPref_MouseHover(object sender, EventArgs e)
        {
            //pbPref.Image = XenClientPlugin.Properties.Resources._000_ToolBar_Pref_Icon_ovr;
        }

        private void pbUSB_MouseHover(object sender, EventArgs e)
        {
            //pbUSB.Image = XenClientPlugin.Properties.Resources._000_ToolBar_USB_Icon_ovr;
        }

        private void pbUSB_MouseLeave(object sender, EventArgs e)
        {
            //pbUSB.Image = XenClientPlugin.Properties.Resources._000_ToolBar_USB_Icon_up;
        }

        private void pbPref_MouseLeave(object sender, EventArgs e)
        {
            //pbPref.Image = XenClientPlugin.Properties.Resources._000_ToolBar_Pref_Icon_up;
        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            if (hidden)
            {
                ShowAnimated();
            }
        }

        private void VMSwitcher_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }

        private void workingAreaTimer_Tick(object sender, EventArgs e)
        {
            if (!SystemInformation.WorkingArea.Equals(_workingArea))
            {
                _workingArea = SystemInformation.WorkingArea;
                SystemEvents_DisplaySettingsChanged(null, new EventArgs());
            }
        }

        #region Overridden

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            hostService.VMsChanged += VMsChanged;
            ThreadPool.QueueUserWorkItem(UpdateVMs);
        }

        void VMsChanged()
        {
            Program.AssertOffEventThread();
            UpdateVMs(null);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        #endregion

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //if (hidden)
            //{
            //    ShowAnimated();
            //}
        }

        #endregion

        private void pbUSB_Click(object sender, EventArgs e)
        {
            var devices = this.hostService.GetUSB();

            this.USBMenu.Items.Clear();
            ToolStripMenuItem item;
            XenUSB device;
            foreach (var usbDevice in devices)
            {
                device = usbDevice.Value;
                item = new ToolStripMenuItem(device.Name);
                item.Checked = (device.State == USBState.Assigned);
                item.Enabled = (device.State != USBState.Unavailable);
                item.ToolTipText = device.Description;
                item.ForeColor = Color.White;
                item.Tag = usbDevice.Key;
                item.Click += item_Click;
                this.USBMenu.Items.Add(item);
            }
            this.USBMenu.Show(MousePosition.X, 75);
        }

        void item_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                var item = sender as ToolStripMenuItem;
                if (item.Checked)
                {
                    this.hostService.UnassignUSB((int)item.Tag);
                }
                else
                {
                    this.hostService.AssignUSB((int)item.Tag);
                }
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Ignore all process key input such as close (ALT+F4)
            return true;
        }
    }
}
