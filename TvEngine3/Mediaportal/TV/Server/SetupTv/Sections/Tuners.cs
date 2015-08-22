#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Tuners : SectionSettings
  {
    private Dictionary<int, bool> _tunerStates = new Dictionary<int, bool>(20);   // ID => present?
    private HashSet<int> _changedTuners = new HashSet<int>();
    private int _originalStreamTunerCount = -1;

    public Tuners(ServerConfigurationChangedEventHandler handler)
      : base("Tuners", handler)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("tuners: activating");

      _changedTuners.Clear();
      UpdateTunerList();
      UpdateTunerGroups();

      _originalStreamTunerCount = ServiceAgents.Instance.SettingServiceAgent.GetValue("streamTunerCount", 0);
      numericUpDownStreamTunerCount.Value = _originalStreamTunerCount;
      this.LogDebug("tuners: stream tuner count = {0}", _originalStreamTunerCount);

      listViewTuners_SelectedIndexChanged(null, null);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("tuners: deactivating");

      _changedTuners.UnionWith(SaveTunerSettings());

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("streamTunerCount", (int)numericUpDownStreamTunerCount.Value);
      if (numericUpDownStreamTunerCount.Value != _originalStreamTunerCount)
      {
        this.LogInfo("tuners: stream tuner count changed from {0} to {1}", _originalStreamTunerCount, numericUpDownStreamTunerCount.Value);
        if (_changedTuners.Count == 0)
        {
          // Force the tuner detector to redetect tuners now.
          _changedTuners.Add(-1);
        }
      }

      if (_changedTuners.Count > 0)
      {
        OnServerConfigurationChanged(this, false, false, _changedTuners);
      }

      base.OnSectionDeActivated();
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl.SelectedIndex == 1)
      {
        treeViewTunerGroups.Focus();
      }
    }

    #region tuners

    private void UpdateTunerList()
    {
      try
      {
        listViewTuners.BeginUpdate();
        listViewTuners.Items.Clear();

        foreach (Tuner tuner in ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None))
        {
          DebugTunerSettings(tuner);
          bool isPresent = ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(tuner.IdTuner);
          _tunerStates[tuner.IdTuner] = isPresent;

          listViewTuners.Items.Add(CreateItemForTuner(tuner, isPresent));
        }
        listViewTuners.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      catch
      {
        MessageBox.Show("Failed to get the tuner information from TV Server. Is the TV service running?", MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        listViewTuners.EndUpdate();
      }
    }

    private ListViewItem CreateItemForTuner(Tuner tuner, bool isPresent)
    {
      ListViewItem item = new ListViewItem();
      item.Tag = tuner;
      item.UseItemStyleForSubItems = true;
      if (!isPresent)
      {
        item.BackColor = Color.Red;
      }
      item.Checked = tuner.IsEnabled;
      if (tuner.IsEnabled)
      {
        item.Font = new Font(item.Font, FontStyle.Regular);
      }
      else
      {
        item.Font = new Font(item.Font, FontStyle.Strikeout);
      }

      item.SubItems.Add(tuner.IdTuner.ToString());
      item.SubItems.Add(string.Join(", ", typeof(BroadcastStandard).GetDescriptions((int)tuner.SupportedBroadcastStandards, false)));
      item.SubItems.Add(tuner.Name);

      if (tuner.UseConditionalAccess)
      {
        item.SubItems.Add(string.Format("Yes, {0}", tuner.DecryptLimit));
      }
      else
      {
        item.SubItems.Add("No");
      }

      if (tuner.UseForEpgGrabbing)
      {
        item.SubItems.Add("Yes");
      }
      else
      {
        item.SubItems.Add("No");
      }

      item.SubItems.Add(tuner.ExternalId);
      return item;
    }

    private HashSet<int> SaveTunerSettings()
    {
      HashSet<int> changedTuners = new HashSet<int>();
      int priority = 1;
      foreach (ListViewItem item in listViewTuners.Items)
      {
        Tuner tuner = item.Tag as Tuner;
        bool save = false;
        if (tuner.IsEnabled != item.Checked)
        {
          this.LogInfo("tuners: tuner {0} enabled changed from {1} to {2}", tuner.IdTuner, tuner.IsEnabled, item.Checked);
          tuner.IsEnabled = item.Checked;
          save = true;
        }
        if (tuner.Priority != priority)
        {
          this.LogInfo("tuners: tuner {0} priority changed from {1} to {2}", tuner.IdTuner, tuner.Priority, priority);
          tuner.Priority = priority;
          save = true;
        }
        if (save)
        {
          ServiceAgents.Instance.TunerServiceAgent.SaveTuner(tuner);
          changedTuners.Add(tuner.IdTuner);
        }

        priority++;
      }
      return changedTuners;
    }

    private void DebugTunerSettings(Tuner tuner)
    {
      this.LogDebug("tuners: tuner...");
      this.LogDebug("  ID                   = {0}", tuner.IdTuner);
      this.LogDebug("  name                 = {0}", tuner.Name);
      this.LogDebug("  external ID          = {0}", tuner.ExternalId);
      this.LogDebug("  standards            = {0}", (BroadcastStandard)tuner.SupportedBroadcastStandards);
      this.LogDebug("  tuner group ID       = {0}", tuner.IdTunerGroup == null ? "[null]" : tuner.IdTunerGroup.ToString());
      this.LogDebug("  enabled?             = {0}", tuner.IsEnabled);
      this.LogDebug("  priority             = {0}", tuner.Priority);
      this.LogDebug("  EPG grabbing?        = {0}", tuner.UseForEpgGrabbing);
      this.LogDebug("  preload?             = {0}", tuner.Preload);
      this.LogDebug("  always send DiSEqC?  = {0}", tuner.AlwaysSendDiseqcCommands);
      this.LogDebug("  conditional access?  = {0}", tuner.UseConditionalAccess);
      this.LogDebug("    providers          = {0}", tuner.ConditionalAccessProviders);
      this.LogDebug("    CAM type           = {0}", (CamType)tuner.CamType);
      this.LogDebug("    decrypt limit      = {0}", tuner.DecryptLimit);
      this.LogDebug("    MCD mode           = {0}", (MultiChannelDecryptMode)tuner.MultiChannelDecryptMode);
      this.LogDebug("  idle mode            = {0}", (TunerIdleMode)tuner.IdleMode);
      this.LogDebug("  BDA network provider = {0}", (BdaNetworkProvider)tuner.BdaNetworkProvider);
      this.LogDebug("  PID filter mode      = {0}", (PidFilterMode)tuner.PidFilterMode);
      this.LogDebug("  custom tuning?       = {0}", tuner.UseCustomTuning);
      this.LogDebug("  TsWriter dump mask   = 0x{0:x}", tuner.TsWriterInputDumpMask);
      this.LogDebug("  TsWriter CRC check?  = {0}", !tuner.DisableTsWriterCrcChecking);
      this.LogDebug("  TsMuxer dump mask    = 0x{0:x}", tuner.TsMuxerInputDumpMask);
    }

    private void listViewTuners_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      FontStyle style = FontStyle.Regular;
      if (!e.Item.Checked)
      {
        style = FontStyle.Strikeout;
      }
      e.Item.Font = new Font(e.Item.Font, style);
    }

    private void listViewTuners_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enablePriorityUp = false;
      bool enablePriorityDown = false;
      bool enableEdit = false;
      bool enableRemove = false;
      try
      {
        ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
        if (items == null || items.Count == 0)
        {
          return;
        }

        if (items.Count == 1)
        {
          int index = items[0].Index;
          if (index != 0)
          {
            enablePriorityUp = true;
          }
          if (index != listViewTuners.Items.Count - 1)
          {
            enablePriorityDown = true;
          }
        }
        else if (items.Count > 1)
        {
          enablePriorityUp = true;
          enablePriorityDown = true;
        }

        foreach (ListViewItem item in items)
        {
          Tuner tuner = item.Tag as Tuner;
          if (tuner == null)
          {
            return;
          }

          if (_tunerStates[tuner.IdTuner])
          {
            enableEdit = true;
          }
          else
          {
            enableRemove = true;
          }
        }
      }
      finally
      {
        buttonTunerPriorityUp.Enabled = enablePriorityUp;
        buttonTunerPriorityDown.Enabled = enablePriorityDown;
        buttonTunerEdit.Enabled = enableEdit;
        buttonTunerDelete.Enabled = enableRemove;
      }
    }

    private void listViewTuners_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      buttonTunerEdit_Click(sender, e);
    }

    private void listViewTuners_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        buttonTunerDelete_Click(null, null);
        e.Handled = true;
      }
    }

    private void buttonTunerPriorityUp_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
      if (items == null || listViewTuners.Items.Count < 2)
      {
        return;
      }

      listViewTuners.BeginUpdate();
      try
      {
        foreach (ListViewItem item in items)
        {
          int index = item.Index;
          if (index > 0)
          {
            listViewTuners.Items.RemoveAt(index);
            listViewTuners.Items.Insert(index - 1, item);
          }
        }
      }
      finally
      {
        listViewTuners.EndUpdate();
      }
    }

    private void buttonTunerPriorityDown_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
      if (items == null || listViewTuners.Items.Count < 2)
      {
        return;
      }

      listViewTuners.BeginUpdate();
      try
      {
        for (int i = items.Count - 1; i >= 0; i--)
        {
          ListViewItem item = items[i];
          int index = item.Index;
          if (index + 1 < listViewTuners.Items.Count)
          {
            listViewTuners.Items.RemoveAt(index);
            listViewTuners.Items.Insert(index + 1, item);
          }
        }
      }
      finally
      {
        listViewTuners.EndUpdate();
      }
    }

    private void buttonTunerEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
      if (items == null)
      {
        return;
      }

      bool shownExplanation = false;
      IList<ListViewItem> itemsToReselect = new List<ListViewItem>(items.Count);
      foreach (ListViewItem item in items)
      {
        Tuner tuner = item.Tag as Tuner;
        if (tuner == null)
        {
          continue;
        }

        if (_tunerStates[tuner.IdTuner])
        {
          using (FormEditTuner dlg = new FormEditTuner(tuner.IdTuner))
          {
            if (dlg.ShowDialog() != DialogResult.OK)
            {
              continue;
            }
          }

          tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(tuner.IdTuner, TunerIncludeRelationEnum.None);
          DebugTunerSettings(tuner);
          _changedTuners.Add(tuner.IdTuner);
          int index = item.Index;
          listViewTuners.Items.RemoveAt(index);
          itemsToReselect.Add(listViewTuners.Items.Insert(index, CreateItemForTuner(tuner, true)));

          // Update the name in the tuner group tree view.
          if (tuner.IdTunerGroup.HasValue)
          {
            bool found = false;
            foreach (TreeNode n in treeViewTunerGroups.Nodes)
            {
              foreach (TreeNode sn in n.Nodes)
              {
                Tuner t = sn.Tag as Tuner;
                if (t != null && t.IdTuner == tuner.IdTuner)
                {
                  sn.Tag = tuner;
                  sn.Text = tuner.Name;
                  found = true;
                  break;
                }
              }
              if (found)
              {
                break;
              }
            }
          }
        }
        else if (!shownExplanation)
        {
          shownExplanation = true;
          MessageBox.Show("It is not possible to edit settings for tuners that are not connected/detected.", MESSAGE_CAPTION);
        }
      }

      foreach (ListViewItem item in itemsToReselect)
      {
        item.Selected = true;
      }
      listViewTuners.Focus();
    }

    private void buttonTunerDelete_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
      if (items == null || items.Count == 0)
      {
        return;
      }
      DialogResult result = MessageBox.Show("Are you sure you want to delete the selected tuner(s)?", MESSAGE_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes)
      {
        return;
      }

      bool shownExplanation = false;
      foreach (ListViewItem item in items)
      {
        Tuner tuner = item.Tag as Tuner;
        if (tuner == null)
        {
          continue;
        }

        if (!_tunerStates[tuner.IdTuner])
        {
          int id = tuner.IdTuner;
          this.LogInfo("tuners: tuner {0} deleted", id);
          ServiceAgents.Instance.ControllerServiceAgent.CardRemove(id);
          item.Remove();

          // Remove the tuner from the tuner group tree view, and delete the
          // tuner group too if this leaves the tuner group empty.
          for (int n = 0; n < treeViewTunerGroups.Nodes.Count; n++)
          {
            bool removedNode = false;
            TreeNode groupNode = treeViewTunerGroups.Nodes[n];
            for (int sn = 0; sn < groupNode.Nodes.Count; sn++)
            {
              TreeNode tunerNode = groupNode.Nodes[sn];
              Tuner t = tunerNode.Tag as Tuner;
              if (t != null && t.IdTuner == id)
              {
                groupNode.Nodes.RemoveAt(sn);
                removedNode = true;
                break;
              }
            }

            if (removedNode)
            {
              if (groupNode.Nodes.Count == 0)
              {
                TunerGroup g = groupNode.Tag as TunerGroup;
                if (g != null)
                {
                  this.LogInfo("tuners: empty tuner group {0} deleted", g.IdTunerGroup);
                  ServiceAgents.Instance.TunerServiceAgent.DeleteTunerGroup(g.IdTunerGroup);
                  treeViewTunerGroups.Nodes.RemoveAt(n);
                }
              }
              break;
            }
          }
        }
        else if (!shownExplanation)
        {
          shownExplanation = true;
          MessageBox.Show("It is not possible to remove tuners that are still connected. They would simply be redetected again. Consider disabling the tuner(s) instead.", MESSAGE_CAPTION);
        }
      }
    }

    #endregion

    #region tuner groups

    private void UpdateTunerGroups()
    {
      try
      {
        treeViewTunerGroups.BeginUpdate();
        treeViewTunerGroups.Nodes.Clear();
        foreach (TunerGroup group in ServiceAgents.Instance.TunerServiceAgent.ListAllTunerGroups())
        {
          DebugTunerGroupSettings(group);
          TreeNode node = treeViewTunerGroups.Nodes.Add(group.Name);
          node.Tag = group;
          foreach (Tuner tuner in group.Tuners)
          {
            TreeNode tunerNode = node.Nodes.Add(tuner.Name);
            tunerNode.Tag = tuner;
          }
        }
      }
      finally
      {
        treeViewTunerGroups.EndUpdate();
      }

      if (treeViewTunerGroups.Nodes.Count > 0)
      {
        treeViewTunerGroups.SelectedNode = treeViewTunerGroups.Nodes[0];
      }
      UpdateGroupButtonStates();
    }

    private void DebugTunerGroupSettings(TunerGroup group)
    {
      HashSet<int> temp = new HashSet<int>();
      foreach (Tuner tuner in group.Tuners)
      {
        temp.Add(tuner.IdTuner);
      }
      this.LogDebug("tuners: tuner group...");
      this.LogDebug("  ID     = {0}", group.IdTunerGroup);
      this.LogDebug("  name   = {0}", group.Name);
      this.LogDebug("  tuners = [{0}]", string.Join(", ", temp));
    }

    private void UpdateGroupButtonStates()
    {
      buttonGroupRename.Enabled = treeViewTunerGroups.Nodes.Count > 0;
      buttonGroupDelete.Enabled = buttonGroupRename.Enabled;
      buttonTunerInGroupAdd.Enabled = buttonGroupRename.Enabled;
      treeViewTunerGroups_AfterSelect(null, null);
    }

    private void treeViewTunerGroups_AfterSelect(object sender, TreeViewEventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      buttonTunerInGroupRemove.Enabled = node.Tag is Tuner;
    }

    private void treeViewTunerGroups_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Delete && e.KeyCode != Keys.F2)
      {
        return;
      }

      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      TunerGroup group = node.Tag as TunerGroup;
      if (group != null)
      {
        if (e.KeyCode == Keys.Delete)
        {
          buttonGroupDelete_Click(null, null);
        }
        else if (e.KeyCode == Keys.F2)
        {
          buttonGroupRename_Click(null, null);
        }
        e.Handled = true;
        return;
      }

      Tuner tuner = node.Tag as Tuner;
      if (tuner != null && e.KeyCode == Keys.Delete)
      {
        buttonTunerInGroupRemove_Click(null, null);
        e.Handled = true;
      }
    }

    private string GetGroupName(string currentName = null)
    {
      FormEnterText dlg;
      if (currentName == null)
      {
        dlg = new FormEnterText("New Tuner Group", "Please enter a name for the new tuner group:", "tuner group");
      }
      else
      {
        dlg = new FormEnterText("Rename Tuner Group", "Please enter a new name for the tuner group:", currentName);
      }
      while (true)
      {
        if (dlg.ShowDialog() != DialogResult.OK)
        {
          dlg.Dispose();
          return null;
        }

        bool found = false;
        foreach (TreeNode node in treeViewTunerGroups.Nodes)
        {
          TunerGroup group = node.Tag as TunerGroup;
          if (group != null && string.Equals(group.Name, dlg.TextValue))
          {
            if (currentName != null)
            {
              return null;
            }
            found = true;
            MessageBox.Show(string.Format("There is already a group named {0}. Please choose a different name.", group.Name), MESSAGE_CAPTION);
            break;
          }
        }
        if (!found)
        {
          string name = dlg.TextValue;
          dlg.Dispose();
          return name;
        }
      }
    }

    private void buttonGroupAdd_Click(object sender, EventArgs e)
    {
      string name = GetGroupName();
      if (name == null)
      {
        return;
      }

      TunerGroup group = new TunerGroup { Name = name };
      group = ServiceAgents.Instance.TunerServiceAgent.SaveTunerGroup(group);
      this.LogInfo("tuners: tuner group {0} added, name = {1}", group.IdTunerGroup, name);
      TreeNode node = treeViewTunerGroups.Nodes.Add(name);
      node.Tag = group;

      node.EnsureVisible();
      treeViewTunerGroups.SelectedNode = node;
      treeViewTunerGroups.Focus();
      UpdateGroupButtonStates();
    }

    private void buttonGroupRename_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      TunerGroup group = node.Tag as TunerGroup;
      if (group == null)
      {
        node = node.Parent;
        group = node.Tag as TunerGroup;
      }
      string name = GetGroupName(group.Name);
      if (name == null)
      {
        return;
      }
      this.LogInfo("tuners: tuner group {0} renamed, old name = {1}, new name = {2}", group.IdTunerGroup, group.Name, name);
      group.Name = name;
      group = ServiceAgents.Instance.TunerServiceAgent.SaveTunerGroup(group);

      // We need to re-query in order to get the tuner relations which are
      // removed during the save process.
      node.Tag = ServiceAgents.Instance.TunerServiceAgent.GetTunerGroup(group.IdTunerGroup);
      node.Text = name;

      node.EnsureVisible();
      treeViewTunerGroups.SelectedNode = node;
      treeViewTunerGroups.Focus();
    }

    private void buttonGroupDelete_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      TunerGroup group = node.Tag as TunerGroup;
      if (group == null)
      {
        node = node.Parent;
        group = node.Tag as TunerGroup;
      }

      HashSet<int> temp = new HashSet<int>();
      foreach (Tuner tuner in group.Tuners)
      {
        _changedTuners.Add(tuner.IdTuner);
        temp.Add(tuner.IdTuner);
      }
      foreach (ListViewItem item in listViewTuners.Items)
      {
        Tuner tuner = item.Tag as Tuner;
        if (tuner != null && tuner.IdTunerGroup == group.IdTunerGroup)
        {
          tuner.IdTunerGroup = null;
          tuner = ServiceAgents.Instance.TunerServiceAgent.SaveTuner(tuner);
          item.Tag = tuner;
        }
      }

      this.LogInfo("tuners: tuner group {0} deleted, tuners = [{1}]", group.IdTunerGroup, string.Join(", ", temp));
      ServiceAgents.Instance.TunerServiceAgent.DeleteTunerGroup(group.IdTunerGroup);
      int index = node.Index - 1;
      node.Remove();

      if (treeViewTunerGroups.Nodes.Count > 0)
      {
        if (index < 0)
        {
          index = 0;
        }
        node = treeViewTunerGroups.Nodes[index];
        node.EnsureVisible();
        treeViewTunerGroups.SelectedNode = node;
        treeViewTunerGroups.Focus();
      }
      UpdateGroupButtonStates();
    }

    private void buttonTunerInGroupAdd_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }

      // Tuners can only be in one group.
      List<Tuner> groupableTuners = new List<Tuner>(listViewTuners.Items.Count);
      foreach (ListViewItem item in listViewTuners.Items)
      {
        Tuner tuner = item.Tag as Tuner;
        if (!tuner.IdTunerGroup.HasValue)
        {
          groupableTuners.Add(tuner);
        }
      }
      if (groupableTuners.Count == 0)
      {
        MessageBox.Show("Each tuner can only be in one group, and all tuners are already in groups.", MESSAGE_CAPTION);
        return;
      }

      TunerGroup group = null;
      TreeNode selectedNode = null;
      using (FormSelectItems dlg = new FormSelectItems("Add Tuner(s) To Group", "Please select one or more tuners to add:", groupableTuners.ToArray(), "Name", true))
      {
        if (dlg.ShowDialog() != DialogResult.OK || dlg.Items == null || dlg.Items.Count == 0)
        {
          return;
        }

        group = node.Tag as TunerGroup;
        if (group == null)
        {
          node = node.Parent;
          group = node.Tag as TunerGroup;
        }

        foreach (object i in dlg.Items)
        {
          Tuner tunerToAdd = i as Tuner;
          if (tunerToAdd == null)
          {
            continue;
          }

          this.LogInfo("tuners: tuner {0} added to group {1}", tunerToAdd.IdTuner, group.IdTunerGroup);
          tunerToAdd.IdTunerGroup = group.IdTunerGroup;
          tunerToAdd = ServiceAgents.Instance.TunerServiceAgent.SaveTuner(tunerToAdd);

          foreach (ListViewItem item in listViewTuners.Items)
          {
            Tuner tuner = item.Tag as Tuner;
            if (tuner != null && tuner.IdTuner == tunerToAdd.IdTuner)
            {
              item.Tag = tunerToAdd;
              break;
            }
          }

          group.Tuners.Add(tunerToAdd);

          selectedNode = node.Nodes.Add(tunerToAdd.Name);
          selectedNode.Tag = tunerToAdd;
        }
      }
      group.AcceptChanges();
      node.Tag = group;

      foreach (Tuner t in group.Tuners)
      {
        _changedTuners.Add(t.IdTuner);
      }

      if (selectedNode != null)
      {
        selectedNode.EnsureVisible();
        treeViewTunerGroups.SelectedNode = selectedNode;
        treeViewTunerGroups.Focus();
      }
      UpdateGroupButtonStates();
    }

    private void buttonTunerInGroupRemove_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      Tuner tunerToRemove = node.Tag as Tuner;
      if (tunerToRemove == null)
      {
        return;
      }
      TunerGroup group = node.Parent.Tag as TunerGroup;
      if (group == null)
      {
        return;
      }

      this.LogInfo("tuners: tuner {0} removed from group {1}", tunerToRemove.IdTuner, group.IdTunerGroup);
      tunerToRemove.IdTunerGroup = null;
      tunerToRemove = ServiceAgents.Instance.TunerServiceAgent.SaveTuner(tunerToRemove);

      foreach (ListViewItem item in listViewTuners.Items)
      {
        Tuner tuner = item.Tag as Tuner;
        if (tuner != null && tuner.IdTuner == tunerToRemove.IdTuner)
        {
          item.Tag = tunerToRemove;
          break;
        }
      }

      foreach (Tuner tuner in group.Tuners)
      {
        _changedTuners.Add(tunerToRemove.IdTuner);
      }
      group.Tuners.Remove(tunerToRemove);
      group.AcceptChanges();
      TreeNode groupNode = node.Parent;
      groupNode.Tag = group;
      int index = node.Index - 1;
      node.Remove();

      if (groupNode.Nodes.Count == 0)
      {
        groupNode.EnsureVisible();
        treeViewTunerGroups.SelectedNode = groupNode;
      }
      else
      {
        if (index < 0)
        {
          index = 0;
        }
        node = groupNode.Nodes[index];
        node.EnsureVisible();
        treeViewTunerGroups.SelectedNode = node;
      }
      treeViewTunerGroups.Focus();
      UpdateGroupButtonStates();
    }

    #endregion
  }
}