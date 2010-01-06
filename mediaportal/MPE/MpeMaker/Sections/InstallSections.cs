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
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Interfaces;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
  public partial class InstallSections : UserControl, ISectionControl
  {
    public PackageClass Package { get; set; }
    private SectionItem SelectedSection;

    public InstallSections()
    {
      InitializeComponent();
      foreach (var panels in MpeInstaller.SectionPanels.Items)
      {
        ToolStripMenuItem testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        testToolStripMenuItem.Text = panels.Key;
        testToolStripMenuItem.Tag = Activator.CreateInstance(panels.Value);
        testToolStripMenuItem.Click += TestToolStripMenuItemClick;
        mnu_add.DropDownItems.Add(testToolStripMenuItem);
        cmb_sectiontype.Items.Add(panels.Key);
      }

      foreach (var actionProvider in MpeInstaller.ActionProviders)
      {
        ToolStripMenuItem testToolStripMenuItem = new ToolStripMenuItem
                                                    {
                                                      Text = actionProvider.Value.DisplayName,
                                                      Tag = actionProvider.Value
                                                    };
        testToolStripMenuItem.Click += testToolStripMenuItem_Click;
        mnu_action_add.DropDownItems.Add(testToolStripMenuItem);
        cmb_sectiontype.Items.Add(actionProvider.Value.DisplayName);
      }
    }

    private void testToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (SelectedSection == null)
        return;
      ToolStripMenuItem menu = sender as ToolStripMenuItem;
      IActionType type = menu.Tag as IActionType;
      if (type != null)
      {
        ActionItem item = new ActionItem(type.DisplayName);
        item.Params =
          new SectionParamCollection(MpeInstaller.ActionProviders[type.DisplayName].GetDefaultParams());
        ActionEdit dlg = new ActionEdit(Package, item);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          if (SelectedSection != null)
          {
            SelectedSection.Actions.Items.Add(item);
            list_actions.Items.Add(item);
          }
        }
      }
    }

    private void InstallSections_Load(object sender, EventArgs e) {}

    #region ISectionControl Members

    public void Set(PackageClass pak)
    {
      listBox_sections.Items.Clear();
      Package = pak;
      PopulateList();
      if (listBox_sections.Items.Count > 0)
      {
        listBox_sections.SelectedItem = listBox_sections.Items[0];
      }
      cmb_grupvisibility.Items.Clear();
      cmb_grupvisibility.Items.Add(string.Empty);
      mnu_groulist.Items.Clear();
      foreach (GroupItem groupItem in Package.Groups.Items)
      {
        cmb_grupvisibility.Items.Add(groupItem.Name);
        mnu_groulist.Items.Add(groupItem.Name);
      }
    }

    private void PopulateList()
    {
      foreach (SectionItem item in Package.Sections.Items)
      {
        AddSection(item);
      }
      if (SelectedSection != null)
      {
        listBox_sections.SelectedItem = SelectedSection;
      }
    }

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    #endregion

    private void AddSection(SectionItem item)
    {
      listBox_sections.Items.Add(item);
    }

    private void TestToolStripMenuItemClick(object sender, EventArgs e)
    {
      ToolStripMenuItem menu = sender as ToolStripMenuItem;
      SectionItem item = new SectionItem();
      ISectionPanel panel = menu.Tag as ISectionPanel;
      if (panel == null)
        return;
      SelectedSection = null;
      item.Name = panel.DisplayName;
      item.PanelName = panel.DisplayName;
      item.Params = panel.GetDefaultParams();
      Package.Sections.Add(item);
      AddSection(item);
      cmb_sectiontype.SelectedItem = item;
    }

    private void listBox_sections_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listBox_sections.SelectedItems.Count > 0)
      {
        SelectedSection = null;
        var param = listBox_sections.SelectedItem as SectionItem;
        txt_guid.Text = param.Guid;
        txt_name.Text = param.Name;
        cmb_sectiontype.Text = param.PanelName;
        cmb_grupvisibility.Text = param.ConditionGroup;
        list_groups.Items.Clear();
        list_actions.Items.Clear();
        cmb_buttons.SelectedIndex = (int)param.WizardButtonsEnum;
        foreach (var s in Package.Groups.Items)
        {
          if (param.IncludedGroups.Contains(s.Name))
            list_groups.Items.Add(s.Name);
        }

        foreach (var acton in param.Actions.Items)
        {
          list_actions.Items.Add(acton);
        }

        SelectedSection = param;
      }
    }

    private void txt_name_TextChanged(object sender, EventArgs e)
    {
      if (SelectedSection == null)
        return;
      SelectedSection.Name = txt_name.Text;
      SelectedSection.PanelName = cmb_sectiontype.Text;
      SelectedSection.ConditionGroup = cmb_grupvisibility.Text;
      SelectedSection.WizardButtonsEnum = (WizardButtonsEnum)cmb_buttons.SelectedIndex;
    }

    private void list_groups_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (SelectedSection == null)
        return;
      if (e.NewValue == CheckState.Checked)
      {
        SelectedSection.IncludedGroups.Add((string)list_groups.Items[e.Index]);
      }
      else
      {
        SelectedSection.IncludedGroups.Remove((string)list_groups.Items[e.Index]);
      }
    }

    private void btn_params_Click(object sender, EventArgs e)
    {
      if (SelectedSection == null)
        return;
      ParamEdit dlg = new ParamEdit();
      dlg.Set(SelectedSection.Params);
      dlg.ShowDialog();
    }

    private void btn_preview_Click(object sender, EventArgs e)
    {
      if (MpeInstaller.SectionPanels.Items.ContainsKey(cmb_sectiontype.Text))
        MpeInstaller.SectionPanels[cmb_sectiontype.Text].Preview(Package, SelectedSection);
    }

    private void btn_up_Click(object sender, EventArgs e)
    {
      if (SelectedSection == null)
        return;
      int idx = Package.Sections.Items.IndexOf(SelectedSection);
      if (idx < 1)
        return;
      Package.Sections.Items.Remove(SelectedSection);
      Package.Sections.Items.Insert(idx - 1, SelectedSection);
      listBox_sections.Items.Clear();
      PopulateList();
    }

    private void btn_down_Click(object sender, EventArgs e)
    {
      if (SelectedSection == null)
        return;
      int idx = Package.Sections.Items.IndexOf(SelectedSection);
      if (idx > Package.Sections.Items.Count - 2)
        return;
      Package.Sections.Items.Remove(SelectedSection);
      Package.Sections.Items.Insert(idx + 1, SelectedSection);
      listBox_sections.Items.Clear();
      PopulateList();
    }

    private void mnu_remove_Click(object sender, EventArgs e)
    {
      if (SelectedSection == null)
        return;
      if (MessageBox.Show("Do you want to Delete section " + SelectedSection.Name, "", MessageBoxButtons.YesNo) !=
          DialogResult.Yes)
        return;
      Package.Sections.Items.Remove(SelectedSection);
      if (Package.Sections.Items.Count > 0)
        SelectedSection = Package.Sections.Items[0];
      else
        SelectedSection = null;
      listBox_sections.Items.Clear();
      PopulateList();
    }

    private void mnu_action_edit_Click(object sender, EventArgs e)
    {
      if (list_actions.SelectedItems.Count < 1)
        return;
      ActionEdit dlg = new ActionEdit(Package, (ActionItem)list_actions.SelectedItem);
      dlg.ShowDialog();
    }

    private void mnu_action_del_Click(object sender, EventArgs e)
    {
      if (list_actions.SelectedItems.Count < 1)
        return;
      var item = (ActionItem)list_actions.SelectedItem;
      if (MessageBox.Show("Do you want to Delete action " + item.Name, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
        return;
      SelectedSection.Actions.Items.Remove(item);
      list_actions.Items.Remove(item);
    }

    private void mnu_group_add_Click(object sender, EventArgs e)
    {
      if (mnu_groulist.SelectedItem != null &&
          !SelectedSection.IncludedGroups.Contains(mnu_groulist.SelectedItem.ToString()))
      {
        list_groups.Items.Add(mnu_groulist.SelectedItem);
        SelectedSection.IncludedGroups.Add(mnu_groulist.SelectedItem.ToString());
      }
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
      if (list_groups.SelectedItems.Count > 0)
      {
        SelectedSection.IncludedGroups.Remove((string)list_groups.SelectedItem);
        list_groups.Items.Remove(list_groups.SelectedItem);
      }
    }

    private void btn_group_up_Click(object sender, EventArgs e)
    {
      if (list_groups.SelectedItem == null)
        return;
      int idx = SelectedSection.IncludedGroups.IndexOf(list_groups.SelectedItem.ToString());
      if (idx < 1)
        return;
      SelectedSection.IncludedGroups.Remove(list_groups.SelectedItem.ToString());
      SelectedSection.IncludedGroups.Insert(idx - 1, list_groups.SelectedItem.ToString());
      list_groups.Items.Clear();
      foreach (string s in SelectedSection.IncludedGroups)
      {
        list_groups.Items.Add(s);
      }
      list_groups.SelectedIndex = idx - 1;
    }

    private void btn_group_down_Click(object sender, EventArgs e)
    {
      if (list_groups.SelectedItem == null)
        return;
      int idx = SelectedSection.IncludedGroups.IndexOf(list_groups.SelectedItem.ToString());
      if (idx > SelectedSection.IncludedGroups.Count - 2)
        return;
      SelectedSection.IncludedGroups.Remove(list_groups.SelectedItem.ToString());
      SelectedSection.IncludedGroups.Insert(idx + 1, list_groups.SelectedItem.ToString());
      list_groups.Items.Clear();
      foreach (string s in SelectedSection.IncludedGroups)
      {
        list_groups.Items.Add(s);
      }
      list_groups.SelectedIndex = idx + 1;
    }
  }
}