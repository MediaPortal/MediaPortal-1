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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class ParamEditScript : UserControl, IParamEdit
  {
    private SectionParam Param;

    public ParamEditScript()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      EditScript dlg = new EditScript();
      dlg.Script = Param.Value;
      dlg.ShowDialog();
      Param.Value = dlg.Script;
    }

    public void Set(SectionParam param)
    {
      Param = param;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://wiki.team-mediaportal.com/MpeMakerScript");
    }
  }
}