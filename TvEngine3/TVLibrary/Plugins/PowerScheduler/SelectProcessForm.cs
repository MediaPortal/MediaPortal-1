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
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TvEngine.PowerScheduler
{
  public partial class SelectProcessForm : Form
  {
    private string _selectedProcess = String.Empty;

    public SelectProcessForm()
    {
      InitializeComponent();
      LoadProcesses();
    }

    private void LoadProcesses()
    {
      comboBox1.Items.Clear();
      foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
      {
        comboBox1.Items.Add(p.ProcessName);
      }
    }

    public String SelectedProcess
    {
      get { return _selectedProcess; }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (comboBox1.SelectedIndex != -1)
        _selectedProcess = (string)comboBox1.Items[comboBox1.SelectedIndex];
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}