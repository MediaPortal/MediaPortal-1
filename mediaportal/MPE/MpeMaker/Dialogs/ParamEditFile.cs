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
using System.Drawing;
using System.Data;
using System.IO;
using MpeCore.Classes;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
  public partial class ParamEditFile : UserControl, IParamEdit
  {
    private SectionParam Param;

    public ParamEditFile()
    {
      InitializeComponent();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      txt_file.Text = "";
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        txt_file.Text = Path.GetFullPath(openFileDialog1.FileName);
      }
    }

    #region IParamEdit Members

    public void Set(SectionParam param)
    {
      Param = param;
      txt_file.Text = param.Value;
    }

    #endregion

    private void txt_file_TextChanged(object sender, EventArgs e)
    {
      Param.Value = txt_file.Text;
      if (File.Exists(Param.Value) &&
          (Path.GetExtension(Param.Value) == ".png" || Path.GetExtension(Param.Value) == ".jpg" ||
           Path.GetExtension(Param.Value) == ".bmp"))
      {
        pictureBox1.LoadAsync(Param.Value);
      }
      else
      {
        pictureBox1.ImageLocation = "";
      }
    }
  }
}