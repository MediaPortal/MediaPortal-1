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
using MpeCore;
using MpeCore.Classes;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
  public partial class ActionEdit : Form
  {
    private PackageClass _packageClass;
    private ActionItem _actionItem;
    private bool _loading = false;

    public ActionEdit(PackageClass packageClass, ActionItem item)
    {
      _loading = true;
      _packageClass = packageClass;
      _actionItem = item;
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
      cmb_type.Text = item.ActionType;
      cmb_group.Items.Add("");
      foreach (var group in packageClass.Groups.Items)
      {
        cmb_group.Items.Add(group.Name);
      }
      cmb_group.SelectedItem = item.ConditionGroup;
      if (_actionItem.Params.Items.Count < 1)
        btn_params.Enabled = false;
      lbl_description.Text = MpeInstaller.ActionProviders[item.ActionType].Description;
      cmb_execute.SelectedIndex = (int)item.ExecuteLocation;
      _loading = false;
    }

    private void btn_ok_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      this.Close();
    }

    private void btn_cancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_loading)
        return;
      _actionItem.ConditionGroup = cmb_group.Text;
      _actionItem.ExecuteLocation = (ActionExecuteLocationEnum)cmb_execute.SelectedIndex;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      ParamEdit dlg = new ParamEdit();
      dlg.Set(_actionItem.Params);
      dlg.ShowDialog();
    }
  }
}