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
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class ParamEditBool : UserControl, IParamEdit
  {
    private SectionParam Param;

    public ParamEditBool()
    {
      InitializeComponent();
    }

    #region IParamEdit Members

    public void Set(SectionParam param)
    {
      Param = param;
      if (Param.GetValueAsBool())
        radio_Yes.Checked = true;
      else
        radio_No.Checked = true;
    }

    #endregion

    private void radio_No_CheckedChanged(object sender, EventArgs e)
    {
      if (radio_Yes.Checked)
        Param.Value = "YES";
      else
        Param.Value = "NO";
    }
  }
}