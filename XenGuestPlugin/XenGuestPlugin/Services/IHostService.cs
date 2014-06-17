//
// Copyright (c) 2013 Citrix Systems, Inc.
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
using XenGuestPlugin.Types;

namespace XenGuestPlugin.Services
{
    public delegate void VMChangedEventHandler();
    public delegate void AlertEventHandler(string iface, string member, string[] args);

    public interface IHostService : IDisposable
    {
        string UUID { get; }
        IDictionary<int, XenVM> GetVMs();
        IDictionary<int, XenUSB> GetUSB();
        void Switch(XenVM vm);
        void AssignUSB(int id);
        void UnassignUSB(int id);
        bool SetAutorun(bool autorun);
        void SetAcceleration(int level);
        event VMChangedEventHandler VMsChanged;
        event AlertEventHandler NewAlert;
    }
}
