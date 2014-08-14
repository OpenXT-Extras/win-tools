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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

using XenClientGuestService.wcf.Contracts.Clients;
using Udbus.Types;
using Udbus.WCF.Client;
using Udbus.WCF.Client.Extensions;
using XenClientDisplayResolutionAgent.Properties;
using XenClientDisplayResolutionAgent.Services;
using XenClientDisplayResolutionAgent.Services.XenClientGuest;
using System.Runtime.InteropServices;

namespace XenClientDisplayResolutionAgent
{
    public partial class ResolutionAgent : Form
    {
        private static IHostService hostService = null;
        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        [DllImport("user32.dll")]
        private static extern void LockWorkStation();

        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        public ResolutionAgent()
        {
            hostService = new XenClientGuestHostService();

            SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(SystemEvents_UserPreferenceChanging);
            SystemEvents.PaletteChanged += new EventHandler(SystemEvents_PaletteChanged);
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            
            updateScreen();

            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        static void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
        }

        static void SystemEvents_PaletteChanged(object sender, EventArgs e)
        {
        }

        static void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            updateScreen();
        }

        static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                hostService.XenStoreWrite("display/activeAdapter", "0");

                // For some reason XP fails to lock when going to sleep, so force it.
                if (IsWinXP())
                {
                    LockWorkStation();
                }
            }
            else if (e.Mode == PowerModes.Resume)
            {
                updateScreen();
            }
        }

        static void updateScreen()
        {
            int display_num = 0;
            string data, path;
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);

            // loop for all available displays
            foreach (var screen in Screen.AllScreens)
            {

                EnumDisplayDevices(null, (uint)display_num, ref d, 0);

                // write display name
                data = d.DeviceString;
                //data = screen.DeviceName;
                path = "display/activeAdapter/" + display_num;
                hostService.XenStoreWrite(path, data);

                // write resolution
                data = screen.Bounds.Width + " " + screen.Bounds.Height;
                path = "display/activeAdapter/" + display_num + "/resolution";
                hostService.XenStoreWrite(path, data);

                // write position
                data = screen.Bounds.X + " " + screen.Bounds.Y;
                path = "display/activeAdapter/" + display_num + "/position";
                hostService.XenStoreWrite(path, data);

                display_num++;
            }

            // write number of total active adapters at the root node
            data = display_num.ToString();
            path = "display/activeAdapter";
            hostService.XenStoreWrite(path, data);

            // write virtual screen size
            data = SystemInformation.VirtualScreen.Width + " " + SystemInformation.VirtualScreen.Height;
            path = "attr/desktopDimension";
            hostService.XenStoreWrite(path, data);
        }

        static bool IsWinXP()
        {
            Version WINDOWS_XP = new Version(5, 1);
            Version currentVersion = Environment.OSVersion.Version;

            if (currentVersion.Major == WINDOWS_XP.Major && currentVersion.Minor == WINDOWS_XP.Minor)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
