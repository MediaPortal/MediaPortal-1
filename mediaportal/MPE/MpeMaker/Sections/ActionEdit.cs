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