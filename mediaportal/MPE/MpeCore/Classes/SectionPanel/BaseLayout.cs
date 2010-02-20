#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MpeCore.Classes.SectionPanel
{
  public partial class BaseLayout : Form
  {
    [DllImport("user32")]
    private static extern int GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32")]
    private static extern bool DeleteMenu(int hMenu, int uPosition, int uFlags);

    private int s_SystemMenuHandle = 0;

    /// <summary>
    /// disables X-button and remove CloseMenuItem from Form's Icon's menu
    /// </summary>
    protected void DisableX_Click()
    {
      this.s_SystemMenuHandle = GetSystemMenu(this.Handle, false);
      DeleteMenu(this.s_SystemMenuHandle, 6, 1024);
    }

    /// <summary>
    /// enables X-button and readds CloseMenuItem to Form's Icon's menu
    /// </summary>
    protected void EnableX_Click()
    {
      this.s_SystemMenuHandle = GetSystemMenu(this.Handle, true);
      DeleteMenu(this.s_SystemMenuHandle, 6, 1024);
    }

    public BaseLayout()
    {
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
    }
  }
}