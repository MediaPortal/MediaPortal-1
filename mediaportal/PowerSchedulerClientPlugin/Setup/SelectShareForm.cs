#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Management;
using System.Text;
using System.Windows.Forms;
#if SERVER
using TvEngine.PowerScheduler;
#endif
#if CLIENT
using MediaPortal.Plugins.Process;
#endif

namespace PowerScheduler.Setup
{
  public partial class SelectShareForm : Form
  {
    private string _selectedShare = String.Empty;

    public SelectShareForm()
    {
      InitializeComponent();
      LoadShares();
    }

    private void LoadShares()
    {
      comboBox1.Items.Clear();
      foreach (ManagementObject obj in new ManagementObjectSearcher(
        "SELECT ComputerName, ShareName, UserName FROM Win32_ServerConnection WHERE NumberOfFiles > 0").Get())
      {
        comboBox1.Items.Add(obj["ShareName"].ToString() + "," + obj["ComputerName"].ToString() + "," + obj["UserName"].ToString());
      }
    }

    public String SelectedShare
    {
      get { return _selectedShare; }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (comboBox1.SelectedIndex != -1)
        _selectedShare = (string)comboBox1.Items[comboBox1.SelectedIndex];
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}