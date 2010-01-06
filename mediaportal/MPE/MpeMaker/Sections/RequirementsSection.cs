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
using MpeCore;
using MpeCore.Classes;
using MpeCore.Interfaces;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
  public partial class RequirementsSection : UserControl, ISectionControl
  {
    public PackageClass Package { get; set; }
    private DependencyItem SelectedItem { get; set; }

    public RequirementsSection()
    {
      InitializeComponent();
      SelectedItem = null;
      foreach (var versionProvider in MpeInstaller.VersionProviders)
      {
        ToolStripMenuItem testToolStripMenuItem = new ToolStripMenuItem();
        testToolStripMenuItem.Text = versionProvider.Value.DisplayName;
        testToolStripMenuItem.Tag = versionProvider.Value;
        testToolStripMenuItem.Click += testToolStripMenuItem_Click;
        mnu_add.DropDownItems.Add(testToolStripMenuItem);
        cmb_type.Items.Add(versionProvider.Value.DisplayName);
      }
    }

    private void testToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menu = sender as ToolStripMenuItem;
      IVersionProvider type = menu.Tag as IVersionProvider;
      if (type != null)
      {
        var item = new DependencyItem(type.DisplayName);
        Package.Dependencies.Add(item);
        list_versions.Items.Add(item);
      }
    }

    private void txt_id_TextChanged(object sender, EventArgs e)
    {
      if (SelectedItem == null)
        return;
      SelectedItem.Type = cmb_type.Text;
      SelectedItem.WarnOnly = chk_warn.Checked;
      SelectedItem.Id = txt_id.Text;
      SelectedItem.Message = txt_message.Text;
      SelectedItem.MinVersion.Major = txt_version1_min.Text;
      SelectedItem.MinVersion.Minor = txt_version2_min.Text;
      SelectedItem.MinVersion.Build = txt_version3_min.Text;
      SelectedItem.MinVersion.Revision = txt_version4_min.Text;
      SelectedItem.MaxVersion.Major = txt_version1_max.Text;
      SelectedItem.MaxVersion.Minor = txt_version2_max.Text;
      SelectedItem.MaxVersion.Build = txt_version3_max.Text;
      SelectedItem.MaxVersion.Revision = txt_version4_max.Text;
      list_versions.SelectedItem = SelectedItem;
    }

    public void Set(PackageClass pak)
    {
      Package = pak;
      list_versions.Items.Clear();
      foreach (DependencyItem item in Package.Dependencies.Items)
      {
        list_versions.Items.Add(item);
      }
      groupBox1.Enabled = false;
    }

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    private void list_versions_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (list_versions.SelectedItems.Count < 1)
      {
        groupBox1.Enabled = false;
        return;
      }
      groupBox1.Enabled = true;
      DependencyItem item = list_versions.SelectedItem as DependencyItem;
      SelectedItem = null;
      cmb_type.Text = item.Type;
      chk_warn.Checked = item.WarnOnly;
      txt_id.Text = item.Id;
      txt_message.Text = item.Message;
      txt_version1_min.Text = item.MinVersion.Major;
      txt_version2_min.Text = item.MinVersion.Minor;
      txt_version3_min.Text = item.MinVersion.Build;
      txt_version4_min.Text = item.MinVersion.Revision;
      txt_version1_max.Text = item.MaxVersion.Major;
      txt_version2_max.Text = item.MaxVersion.Minor;
      txt_version3_max.Text = item.MaxVersion.Build;
      txt_version4_max.Text = item.MaxVersion.Revision;
      SelectedItem = item;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      var dlg = new InstalledExtensionsSelector();
      dlg.ShowDialog();
      if (dlg.Result == null) return;
      txt_id.Text = dlg.Result.GeneralInfo.Id;
      txt_name.Text = dlg.Result.GeneralInfo.Name;
    }

    private void mnu_del_Click(object sender, EventArgs e)
    {
      if (list_versions.SelectedItems.Count < 1)
        return;
      if (SelectedItem == null)
        return;
      if (MessageBox.Show("Do you want to dependency " + SelectedItem.Name, "", MessageBoxButtons.YesNo) !=
          DialogResult.Yes)
        return;
      Package.Dependencies.Items.Remove(SelectedItem);
      list_versions.Items.Remove(list_versions.SelectedItem);
    }

    private void txt_version1_min_KeyDown(object sender, KeyEventArgs e)
    {
      bool result = true;

      bool numericKeys = (
                           ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                            (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
                           && e.Modifiers != Keys.Shift);

      bool ctrlA = e.KeyCode == Keys.A && e.Modifiers == Keys.Control;

      bool editKeys = (
                        (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control) ||
                        (e.KeyCode == Keys.X && e.Modifiers == Keys.Control) ||
                        (e.KeyCode == Keys.C && e.Modifiers == Keys.Control) ||
                        (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) ||
                        e.KeyCode == Keys.Delete ||
                        e.KeyCode == Keys.Back);

      bool navigationKeys = (
                              e.KeyCode == Keys.Up ||
                              e.KeyCode == Keys.Right ||
                              e.KeyCode == Keys.Down ||
                              e.KeyCode == Keys.Left ||
                              e.KeyCode == Keys.Home ||
                              e.KeyCode == Keys.End);

      if (!(numericKeys || editKeys || navigationKeys))
      {
        result = false;
      }
      if (!result) // If not valid key then suppress and handle.
      {
        e.SuppressKeyPress = true;
        e.Handled = true;
      }
      else
        base.OnKeyDown(e);
    }
  }
}