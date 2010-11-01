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
using MpeCore.Classes;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
  public partial class ParamEditTemplate : UserControl, IParamEdit
  {
    private SectionParam Param = new SectionParam();

    public ParamEditTemplate()
    {
      InitializeComponent();
    }

    #region IParamEdit Members

    public void Set(SectionParam param)
    {
      Param = param;
      textBox1.Text = param.Value;
    }

    #endregion

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      Param.Value = textBox1.Text;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      PathTemplateSelector dlg = new PathTemplateSelector();
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBox1.Text = dlg.Result;
      }
    }
  }
}