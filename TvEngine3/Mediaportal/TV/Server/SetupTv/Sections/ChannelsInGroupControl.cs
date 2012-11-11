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
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{  
  public partial class ChannelsInGroupControl : UserControl
  {


    private string _allChannelsGroupName = TvConstants.TvGroupNames.AllChannels;
    private MediaTypeEnum _mediaTypeEnum = MediaTypeEnum.TV;
    private readonly MPListViewStringColumnSorter lvwColumnSorter;

    private static bool _userConfirmedAutoReorder = false;
    private SortOrder _lastSortOrder = SortOrder.None;

    private ChannelGroup _channelGroup;

    public ChannelsInGroupControl(MediaTypeEnum mediaType)
    {
      _mediaTypeEnum = mediaType;
      InitializeComponent();

      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      listView1.ListViewItemSorter = lvwColumnSorter;
      listView1.IsChannelListView = true;
    }

    public ChannelGroup Group
    {
      get { return _channelGroup; }
      set { _channelGroup = value; }
    }

    public MediaTypeEnum MediaTypeEnum
    {
      get { return _mediaTypeEnum; }
      set
      {
        _mediaTypeEnum = value;
        if (value == MediaTypeEnum.TV)
        {
          _allChannelsGroupName = TvConstants.TvGroupNames.AllChannels;
        }
        else if (value == MediaTypeEnum.Radio)
        {
          _allChannelsGroupName = TvConstants.RadioGroupNames.AllChannels;
        }
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e) {}

    private void ChannelsInGroupControl_Load(object sender, EventArgs e) {}

    private bool _ignoreItemCheckedEvent = false;    

    public void OnActivated()
    {
      try
      {
        _ignoreItemCheckedEvent = true;
        Application.DoEvents();

        Cursor.Current = Cursors.WaitCursor;

        UpdateMenuAndTabs();

        listView1.Items.Clear();
       

        ChannelIncludeRelationEnum include = ChannelIncludeRelationEnum.ChannelMaps;
        include |= ChannelIncludeRelationEnum.TuningDetails;
        include |= ChannelIncludeRelationEnum.GroupMaps;
        include |= ChannelIncludeRelationEnum.GroupMapsChannelGroup;
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.GetAllChannelsByGroupIdAndMediaType(_channelGroup.IdGroup, _mediaTypeEnum, include);

        foreach (var channel in channels)
        {
          foreach (var groupMap in channel.GroupMaps)
          {
            if (groupMap.IdGroup == _channelGroup.IdGroup)
            {
              listView1.Items.Add(CreateItemForChannel(channel, groupMap));
              break;
            }
          }          
        }

        bool isAllChannelsGroup = (_channelGroup.GroupName == _allChannelsGroupName);
        removeChannelFromGroup.Enabled = !isAllChannelsGroup;
        mpButtonDel.Enabled = !isAllChannelsGroup;
      }
      catch (Exception exp)
      {
        this.LogError("OnActivated error: {0}", exp.Message);
      }
      finally
      {
        Cursor.Current = Cursors.Default;
        _ignoreItemCheckedEvent = false;
      }
    }

    private ListViewItem CreateItemForChannel(Channel channel, object map)
    {
      bool hasFta = false;
      bool hasScrambled = false;
      IList<TuningDetail> tuningDetails = channel.TuningDetails;
      foreach (TuningDetail detail in tuningDetails)
      {
        if (detail.FreeToAir)
        {
          hasFta = true;
        }
        if (!detail.FreeToAir)
        {
          hasScrambled = true;
        }
      }

      int imageIndex;
      if (hasFta && hasScrambled)
      {
        imageIndex = 5;
      }
      else if (hasScrambled)
      {
        imageIndex = 4;
      }
      else
      {
        imageIndex = 3;
      }

      ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);

      item.Checked = channel.VisibleInGuide;
      item.Tag = map;

      IList<TuningDetail> details = channel.TuningDetails;
      if (details.Count > 0)
      {
        item.SubItems.Add(details[0].ChannelNumber.ToString());
      }
      return item;
    }

    private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      if (e.Item is ListViewItem)
      {
        ReOrder();
      }
    }

    private void ReOrder()
    {
      IList<GroupMap> groupMaps = new List<GroupMap>();
      for (int i = 0; i < listView1.Items.Count; ++i)
      {
        GroupMap groupMap = (GroupMap)listView1.Items[i].Tag;
        if (groupMap.SortOrder != i)
        {
          groupMap.SortOrder = i;
          groupMap.UnloadAllUnchangedRelationsForEntity();
          groupMap.ChangeTracker.State = ObjectState.Modified;
          groupMaps.Add(groupMap);
        }
      }
      ServiceAgents.Instance.ChannelServiceAgent.SaveChannelGroupMaps(groupMaps);
    }

    private void UpdateMenuAndTabs()
    {
      addToFavoritesToolStripMenuItem.DropDownItems.Clear();

      IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(_mediaTypeEnum, ChannelGroupIncludeRelationEnum.None);

      foreach (ChannelGroup group in groups)
      {
        ToolStripMenuItem item = new ToolStripMenuItem(group.GroupName);

        item.Tag = group;
        item.Click += addToFavoritesToolStripMenuItem_Click;

        addToFavoritesToolStripMenuItem.DropDownItems.Add(item);
      }
    }

    private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ChannelGroup group;
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

      if (menuItem.Tag == null)
      {
        GroupNameForm dlg = new GroupNameForm();

        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }

        group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(dlg.GroupName, _mediaTypeEnum);        
        UpdateMenuAndTabs();
      }
      else
      {
        group = (ChannelGroup)menuItem.Tag;
      }

      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = listView1.Items[indexes[i]];
        GroupMap map = (GroupMap)item.Tag;
        Channel channel = map.Channel;
        MappingHelper.AddChannelToGroup(ref channel, @group);
      }
    }

    private void removeChannelFromGroup_Click(object sender, EventArgs e)
    {
      mpButtonDel_Click(null, null);
    }

    private void deleteThisChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        string holder = String.Format("Are you sure you want to delete these {0:d} channels?",
                                      listView1.SelectedItems.Count);

        if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          return;
        }
      }

      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;

      NotifyForm dlg = new NotifyForm("Deleting selected tv channels...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index >= 0 && index < listView1.Items.Count)
        {
          ListViewItem item = listView1.Items[index];
          listView1.Items.RemoveAt(index);
          GroupMap map = (GroupMap)item.Tag;
          Channel channel = map.Channel;
          ServiceAgents.Instance.ChannelServiceAgent.DeleteChannel(channel.IdChannel);
        }
      }
      dlg.Close();
      ReOrder();
      OnActivated();
    }

    private void editChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index >= 0 && index < listView1.Items.Count)
        {
          ListViewItem item = listView1.Items[index];
          GroupMap map = (GroupMap)item.Tag;
          Channel channel = map.Channel;
          FormEditChannel dlg = new FormEditChannel();
          dlg.Channel = channel;
          dlg.ShowDialog();
          map.Channel = dlg.Channel;
          return;
        }
      }
      ReOrder();
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      editChannelToolStripMenuItem_Click(null, null);
    }

    private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      if (e.Label != null)
      {
        Channel channel = ((GroupMap)listView1.Items[e.Item].Tag).Channel;
        channel.DisplayName = e.Label;
        channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel);
        channel.AcceptChanges();
      }
    }

    private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (!_ignoreItemCheckedEvent)
      {
        Channel ch = ((GroupMap) e.Item.Tag).Channel;
        if (ch.VisibleInGuide != e.Item.Checked)
        {
          ch.VisibleInGuide = e.Item.Checked;          
          ch = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(ch);
          ch.AcceptChanges();
          ((GroupMap) e.Item.Tag).Channel = ch;
        }
      }
    }

   

    private void mpButtonPreview_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      FormPreview previewWindow = new FormPreview();

      GroupMap map = (GroupMap)listView1.Items[indexes[0]].Tag;
      previewWindow.Channel = map.Channel;
      previewWindow.ShowDialog(this);
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      listView1.BeginUpdate();
      try
      {
        ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
        if (indexes.Count == 0)
          return;
        for (int i = 0; i < indexes.Count; ++i)
        {
          int index = indexes[i];
          if (index > 0)
          {
            ListViewItem item = listView1.Items[index];
            listView1.Items.RemoveAt(index);
            listView1.Items.Insert(index - 1, item);
          }
        }
        ReOrder();
      }
      finally
      {
        listView1.EndUpdate();
      }
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      listView1.BeginUpdate();
      try
      {
        ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
        if (indexes.Count == 0)
          return;
        if (listView1.Items.Count < 2)
          return;
        for (int i = indexes.Count - 1; i >= 0; i--)
        {
          int index = indexes[i];
          ListViewItem item = listView1.Items[index];
          listView1.Items.RemoveAt(index);
          if (index + 1 < listView1.Items.Count)
            listView1.Items.Insert(index + 1, item);
          else
            listView1.Items.Add(item);
        }
        ReOrder();
      }
      finally
      {
        listView1.EndUpdate();
      }
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        string holder = String.Format("Are you sure you want to remove these {0:d} channels?",
                                      listView1.SelectedItems.Count);

        if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          return;
        }
      }

      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      NotifyForm dlg = new NotifyForm("Removing tv channels from group...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

            
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index >= 0 && index < listView1.Items.Count)
        {
          ListViewItem item = listView1.Items[index];
          listView1.Items.RemoveAt(index);
          GroupMap map = (GroupMap)item.Tag;
          
          ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroupMap(map.IdMap);
        }
      }
      dlg.Close();
      ReOrder();
    }

    private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (sender != null)
      {
        if (!_userConfirmedAutoReorder)
        {
          if (MessageBox.Show("The current channel order will be overwritten after the sorting operation. Continue?",
                              "Re-order channels?", MessageBoxButtons.YesNo)
              == DialogResult.No)
          {
            return;
          }
          else
          {
            _userConfirmedAutoReorder = true;
          }
        }
      }

      MPButton buttonSort = null;
      MPButton buttonOther = null;
      switch (e.Column)
      {
        case 0:
          lvwColumnSorter.OrderType = MPListViewStringColumnSorter.OrderTypes.AsString;
          buttonSort = mpButtonOrderByName;
          buttonOther = mpButtonOrderByNumber;
          break;
        case 1:
          lvwColumnSorter.OrderType = MPListViewStringColumnSorter.OrderTypes.AsValue;
          buttonSort = mpButtonOrderByNumber;
          buttonOther = mpButtonOrderByName;
          break;
      }

      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter.Order = _lastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      _lastSortOrder = lvwColumnSorter.Order;

      // Perform the sort with these new sort options.
      listView1.Sort();
      ReOrder();

      if (buttonSort != null)
      {
        switch (lvwColumnSorter.Order)
        {
          case SortOrder.Ascending:
            buttonSort.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_asc;
            break;
          case SortOrder.Descending:
            buttonSort.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_dsc;
            break;
          case SortOrder.None:
            buttonSort.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;
            break;
        }
      }

      buttonOther.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;

      //Reset the SortOrder again. Otherwise manual re-order won't be possible anymore
      lvwColumnSorter.Order = SortOrder.None;
    }

    private void mpButtonOrderByName_Click(object sender, EventArgs e)
    {
      listView1_ColumnClick(null, new ColumnClickEventArgs(0));
    }

    private void mpButtonOrderByNumber_Click(object sender, EventArgs e)
    {
      listView1_ColumnClick(null, new ColumnClickEventArgs(1));
    }
  }
}