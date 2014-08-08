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
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TvCards : SectionSettings
  {
    private IList<Card> _tuners = new List<Card>(20);
    private IList<CardGroup> _tunerGroups = new List<CardGroup>(5);
    private Dictionary<int, CardType> _tunerTypes = new Dictionary<int, CardType>(20);
    private HashSet<int> _changedTuners = new HashSet<int>();
    private int _originalStreamTunerCount = -1;

    public TvCards()
      : this("Tuners", null)
    {
    }

    public TvCards(string name, ServerConfigurationChangedEventHandler handler)
      : base(name, handler)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("tuners: activating");
      base.OnSectionActivated();
      _changedTuners.Clear();
      UpdateTunerList();
      UpdateTunerGroups();

      _originalStreamTunerCount = ServiceAgents.Instance.SettingServiceAgent.GetValue("iptvCardCount", 1);
      numericUpDownStreamTunerCount.Value = _originalStreamTunerCount;
      this.LogDebug("tuners: stream tuner count = {0}", _originalStreamTunerCount);
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("tuners: deactivating");
      _changedTuners.UnionWith(SaveTunerSettings());

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("iptvCardCount", (int)numericUpDownStreamTunerCount.Value);
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
        OnServerConfigurationChanged(this, false, _changedTuners);
      }
      base.OnSectionDeActivated();
    }

    #region tuners

    private void UpdateTunerList()
    {
      try
      {
        listViewTuners.BeginUpdate();
        listViewTuners.Items.Clear();

        _tuners = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.CardGroupMaps).OrderByDescending(c => c.Priority).ToList();
        foreach (Card tuner in _tuners)
        {
          DebugTunerSettings(tuner);
          CardType tunerType = ServiceAgents.Instance.ControllerServiceAgent.Type(tuner.IdCard);
          _tunerTypes[tuner.IdCard] = tunerType;

          listViewTuners.Items.Add(CreateItemForTuner(tuner, tunerType));
        }
        listViewTuners.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      catch (Exception)
      {
        MessageBox.Show(this, "Failed to get the tuner information from TV Server. Is the TV service running?");
      }
      finally
      {
        listViewTuners.EndUpdate();
      }
    }

    private ListViewItem CreateItemForTuner(Card tuner, CardType tunerType)
    {
      ListViewItem item = new ListViewItem();
      item.Tag = tuner;
      item.Checked = tuner.Enabled;
      if (tuner.Enabled)
      {
        item.Font = new Font(item.Font, FontStyle.Regular);
      }
      else
      {
        item.Font = new Font(item.Font, FontStyle.Strikeout);
      }

      item.SubItems.Add(tuner.IdCard.ToString());
      item.SubItems.Add(tunerType.GetDescription());
      item.SubItems.Add(tuner.Name);

      if (tuner.UseConditionalAccess)
      {
        item.SubItems.Add("Yes");
      }
      else
      {
        item.SubItems.Add("No");
      }
      item.SubItems.Add(tuner.DecryptLimit.ToString());

      if (tuner.GrabEPG)
      {
        item.SubItems.Add("Yes");
      }
      else
      {
        item.SubItems.Add("No");
      }

      item.SubItems.Add(tuner.DevicePath);
      return item;
    }

    private HashSet<int> SaveTunerSettings()
    {
      HashSet<int> changedTuners = new HashSet<int>();
      int priority = listViewTuners.Items.Count;
      foreach (ListViewItem item in listViewTuners.Items)
      {
        Card tuner = item.Tag as Card;
        if (tuner.Enabled != item.Checked || tuner.Priority != priority)
        {
          this.LogInfo("tuners: tuner {0} enabled changed from {1} to {2}", tuner.IdCard, tuner.Enabled, item.Checked);
          tuner.Enabled = item.Checked;

          this.LogInfo("tuners: tuner {0} priority changed from {1} to {2}", tuner.IdCard, tuner.Priority, priority);
          tuner.Priority = priority;

          ServiceAgents.Instance.CardServiceAgent.SaveCard(tuner);
          changedTuners.Add(tuner.IdCard);
        }

        DebugTunerSettings(tuner);
        priority--;
      }
      return changedTuners;
    }

    private void DebugTunerSettings(Card tuner)
    {
      this.LogDebug("tuners: tuner...");
      this.LogDebug("  ID               = {0}", tuner.IdCard);
      this.LogDebug("  name             = {0}", tuner.Name);
      this.LogDebug("  external ID      = {0}", tuner.DevicePath);
      this.LogDebug("  enabled?         = {0}", tuner.Enabled);
      this.LogDebug("  priority         = {0}", tuner.Priority);
      this.LogDebug("  preload?         = {0}", tuner.PreloadCard);
      this.LogDebug("  idle mode        = {0}", tuner.IdleMode);
      this.LogDebug("  cond. access?    = {0}", tuner.UseConditionalAccess);
      this.LogDebug("  decrypt limit    = {0}", tuner.DecryptLimit);
      this.LogDebug("  MCD mode         = {0}", tuner.MultiChannelDecryptMode);
      this.LogDebug("  CAM type         = {0}", tuner.CamType);
      this.LogDebug("  grab EPG?        = {0}", tuner.GrabEPG);
      this.LogDebug("  PID filter mode  = {0}", tuner.PidFilterMode);
      this.LogDebug("  custom tuning?   = {0}", tuner.UseCustomTuning);
      this.LogDebug("  always DiSEqC?   = {0}", tuner.AlwaysSendDiseqcCommands);
      this.LogDebug("  DiSEqC repeat    = {0}", tuner.DiseqcCommandRepeatCount);
      this.LogDebug("  network provider = {0}", tuner.NetProvider);
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
      bool enableEdit = false;
      bool enableRemove = false;
      try
      {
        ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
        if (items == null)
        {
          return;
        }
        foreach (ListViewItem item in items)
        {
          Card tuner = item.Tag as Card;
          if (tuner == null)
          {
            return;
          }

          CardType tunerType = _tunerTypes[tuner.IdCard];
          if (tunerType == CardType.Unknown)
          {
            enableRemove = true;
          }
          else
          {
            enableEdit = true;
          }
        }
      }
      finally
      {
        buttonTunerEdit.Enabled = enableEdit;
        buttonTunerDelete.Enabled = enableRemove;
      }
    }

    private void listViewTuners_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      buttonTunerEdit_Click(sender, e);
    }

    private void buttonTunerPriorityUp_Click(object sender, EventArgs e)
    {
      listViewTuners.BeginUpdate();
      try
      {
        ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
        if (items == null || listViewTuners.Items.Count < 2)
        {
          return;
        }
        for (int i = 0; i < items.Count; ++i)
        {
          ListViewItem item = items[i];
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
      listViewTuners.BeginUpdate();
      try
      {
        ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
        if (items == null || listViewTuners.Items.Count < 2)
        {
          return;
        }
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
      foreach (ListViewItem item in items)
      {
        Card tuner = item.Tag as Card;
        if (tuner == null)
        {
          return;
        }

        CardType tunerType = _tunerTypes[tuner.IdCard];
        if (tunerType != CardType.Unknown)
        {
          FormEditCard dlg = new FormEditCard(tuner, tunerType);
          if (dlg.ShowDialog() == DialogResult.OK)
          {
            this.LogInfo("tuners: tuner {0} settings changed", tuner.IdCard);
            _changedTuners.Add(tuner.IdCard);
            listViewTuners.Items.RemoveAt(item.Index);
            listViewTuners.Items.Insert(item.Index, CreateItemForTuner(tuner, tunerType));
          }
        }
        else if (!shownExplanation)
        {
          shownExplanation = true;
          MessageBox.Show(this, "It is not possible to edit settings for tuners with unknown type.");
        }
      }
    }

    private void buttonTunerDelete_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuners.SelectedItems;
      if (items == null)
      {
        return;
      }
      DialogResult result = MessageBox.Show(this, "Are you sure you want to delete the selected tuner(s)?", "Delete Tuner(s)?", MessageBoxButtons.YesNo);
      if (result == DialogResult.Yes)
      {
        bool shownExplanation = false;
        foreach (ListViewItem item in items)
        {
          Card tuner = item.Tag as Card;
          if (tuner == null)
          {
            return;
          }

          CardType tunerType = _tunerTypes[tuner.IdCard];
          if (tunerType == CardType.Unknown)
          {
            this.LogInfo("tuners: tuner {0} deleted", tuner.IdCard);
            ServiceAgents.Instance.ControllerServiceAgent.CardRemove(tuner.IdCard);
            listViewTuners.Items.Remove(item);
          }
          else if (!shownExplanation)
          {
            shownExplanation = true;
            MessageBox.Show(this, "It is not possible to remove tuners that are still connected. They would simply be redetected again.");
          }
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
        _tunerGroups = ServiceAgents.Instance.CardServiceAgent.ListAllCardGroups();
        foreach (CardGroup group in _tunerGroups)
        {
          DebugTunerGroupSettings(group);
          TreeNode node = treeViewTunerGroups.Nodes.Add(group.Name);
          node.Tag = group;
          IList<CardGroupMap> mappings = group.CardGroupMaps;
          foreach (CardGroupMap mapping in mappings)
          {
            Card tuner = mapping.Card;
            TreeNode tunerNode = node.Nodes.Add(tuner.Name);
            tunerNode.Tag = tuner;
          }
        }
      }
      finally
      {
        treeViewTunerGroups.EndUpdate();
      }
    }

    private void DebugTunerGroupSettings(CardGroup group)
    {
      HashSet<int> temp = new HashSet<int>();
      foreach (CardGroupMap mapping in group.CardGroupMaps)
      {
        temp.Add(mapping.IdCard);
      }
      this.LogDebug("tuners: tuner group...");
      this.LogDebug("  ID     = {0}", group.IdCardGroup);
      this.LogDebug("  name   = {0}", group.Name);
      this.LogDebug("  tuners = [{0}]", string.Join(", ", temp));
    }

    private void treeViewTunerGroups_AfterSelect(object sender, TreeViewEventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      Card tuner = node.Tag as Card;
      buttonTunerInGroupRemove.Enabled = tuner != null;
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
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return null;
        }

        bool found = false;
        foreach (CardGroup group in _tunerGroups)
        {
          if (string.Equals(group.Name, dlg.TextValue))
          {
            found = true;
            MessageBox.Show(string.Format("There is already a group named {0}.", group.Name));
            break;
          }
        }
        if (!found)
        {
          return dlg.TextValue;
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

      CardGroup group = new CardGroup { Name = name };
      group = ServiceAgents.Instance.CardServiceAgent.SaveCardGroup(group);
      this.LogInfo("tuners: tuner group {0} added, name = {1}", group.IdCardGroup, name);
      TreeNode node = treeViewTunerGroups.Nodes.Add(name);
      node.Tag = group;
    }

    private void buttonGroupRename_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      CardGroup group = node.Tag as CardGroup;
      if (group == null)
      {
        node = node.Parent;
        group = node.Tag as CardGroup;
      }
      string name = GetGroupName(group.Name);
      if (name == null)
      {
        return;
      }
      this.LogInfo("tuners: tuner group {0} renamed, old name = {1}, new name = {2}", group.IdCardGroup, group.Name, name);
      group.Name = name;
      group = ServiceAgents.Instance.CardServiceAgent.SaveCardGroup(group);
      node.Tag = group;
      node.Text = name;
    }

    private void buttonGroupDelete_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      CardGroup group = node.Tag as CardGroup;
      if (group == null)
      {
        node = node.Parent;
        group = node.Tag as CardGroup;
      }
      HashSet<int> temp = new HashSet<int>();
      foreach (CardGroupMap mapping in group.CardGroupMaps)
      {
        _changedTuners.Add(mapping.IdCard);
        temp.Add(mapping.IdCard);
      }
      this.LogInfo("tuners: tuner group {0} deleted, tuners = [{1}]", group.IdCardGroup, string.Join(", ", temp));
      ServiceAgents.Instance.CardServiceAgent.DeleteCardGroup(group.IdCardGroup);
      treeViewTunerGroups.Nodes.Remove(node);
    }

    private void buttonTunerInGroupAdd_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }

      // Tuners can only be in one group.
      HashSet<int> groupedTuners = new HashSet<int>();
      foreach (CardGroup g in _tunerGroups)
      {
        foreach (CardGroupMap m in g.CardGroupMaps)
        {
          groupedTuners.Add(m.IdCard);
        }
      }
      List<Card> groupableTuners = new List<Card>(_tuners.Count);
      foreach (Card t in _tuners)
      {
        if (!groupedTuners.Contains(t.IdCard))
        {
          groupableTuners.Add(t);
        }
      }
      if (groupableTuners.Count == 0)
      {
        MessageBox.Show(this, "Each tuner can only be in one group, and all tuners are already in groups.");
        return;
      }

      FormSelectItem dlg = new FormSelectItem("Add Tuner To Group", "Please select a tuner to add:", groupableTuners.ToArray(), "Name");
      if (dlg.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      Card tuner = dlg.Item as Card;
      if (tuner == null)
      {
        return;
      }

      CardGroup group = node.Tag as CardGroup;
      if (group == null)
      {
        node = node.Parent;
        group = node.Tag as CardGroup;
      }
      this.LogInfo("tuners: tuner {0} added to group {1}", tuner.IdCard, group.IdCardGroup);
      CardGroupMap mapping = new CardGroupMap();
      mapping.IdCard = tuner.IdCard;
      mapping.IdCardGroup = group.IdCardGroup;
      mapping = ServiceAgents.Instance.CardServiceAgent.SaveCardGroupMap(mapping);
      group.CardGroupMaps.Add(mapping);
      group.AcceptChanges();

      foreach (CardGroupMap m in group.CardGroupMaps)
      {
        _changedTuners.Add(m.IdCard);
      }

      node = node.Nodes.Add(tuner.Name);
      node.Tag = tuner;
    }

    private void buttonTunerInGroupRemove_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewTunerGroups.SelectedNode;
      if (node == null)
      {
        return;
      }
      Card tuner = node.Tag as Card;
      if (tuner == null)
      {
        return;
      }
      CardGroup group = node.Parent.Tag as CardGroup;
      if (group != null)
      {
        foreach (CardGroupMap mapping in group.CardGroupMaps)
        {
          if (mapping.IdCard == tuner.IdCard)
          {
            this.LogInfo("tuners: tuner {0} removed from group {1}", tuner.IdCard, group.IdCardGroup);
            ServiceAgents.Instance.CardServiceAgent.DeleteGroupMap(mapping.IdMapping);
          }
          _changedTuners.Add(tuner.IdCard);
        }
        treeViewTunerGroups.Nodes.Remove(node);
      }
    }

    #endregion
  }
}