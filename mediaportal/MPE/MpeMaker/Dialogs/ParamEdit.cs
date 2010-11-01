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
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class ParamEdit : Form
  {
    private SectionParam SelectedItem = null;
    private SectionParamCollection Params;
    private Dictionary<ValueTypeEnum, IParamEdit> Panels = new Dictionary<ValueTypeEnum, IParamEdit>();

    public ParamEdit()
    {
      InitializeComponent();
      Panels.Add(ValueTypeEnum.File, new ParamEditFile());
      Panels.Add(ValueTypeEnum.String, new ParamEditString());
      Panels.Add(ValueTypeEnum.Template, new ParamEditTemplate());
      Panels.Add(ValueTypeEnum.Bool, new ParamEditBool());
      Panels.Add(ValueTypeEnum.Script, new ParamEditScript());
    }


    public void Set(SectionParamCollection paramCollection)
    {
      Params = paramCollection;
      foreach (SectionParam param in Params.Items)
      {
        cmb_params.Items.Add(param);
      }
      cmb_params.SelectedIndex = 0;
    }

    private void cmb_params_SelectedIndexChanged(object sender, EventArgs e)
    {
      SelectedItem = cmb_params.SelectedItem as SectionParam;
      panel1.Controls.Clear();
      Panels[SelectedItem.ValueType].Set(SelectedItem);
      panel1.Controls.Add((Control)Panels[SelectedItem.ValueType]);
      label_desc.Text = SelectedItem.Description;
    }
  }
}