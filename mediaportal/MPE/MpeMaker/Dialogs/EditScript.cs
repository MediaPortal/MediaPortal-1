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
using System.Text;
using System.Windows.Forms;
using CSScriptLibrary;


namespace MpeMaker.Dialogs
{
  public partial class EditScript : Form
  {
    public string Script
    {
      get { return textBox_code.Text; }
      set { textBox_code.Text = value; }
    }

    public EditScript()
    {
      InitializeComponent();
    }

    private void validateToolStripMenuItem_Click(object sender, EventArgs e)
    {
      textBox_error.Text = "";
      try
      {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        CSScript.AssemblyResolvingEnabled = true;
        AsmHelper script =
          new AsmHelper(CSScriptLibrary.CSScript.LoadCode(textBox_code.Text, Path.GetTempFileName(), true));
        MessageBox.Show("No error");
      }
      catch (Exception ex)
      {
        textBox_error.Text = ex.Message;
      }
    }
  }
}