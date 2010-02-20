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
using System.Windows.Forms;
using Gentle.Framework;
using TvDatabase;
using MediaPortal.UserInterface.Controls;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class ChannelsInRadioGroupControl : UserControl
  {
    private readonly MPListViewStringColumnSorter lvwColumnSorter;
    private static bool _userConfirmedAutoReorder = false;
    private SortOrder _lastSortOrder = SortOrder.None;

    private RadioChannelGroup _channelGroup;

    public ChannelsInRadioGroupControl()
    {
      InitializeComponent();

      lvwColumnSorter = new MPListViewStringColumnSorter();
      listView1.ListViewItemSorter = lvwColumnSorter;
      lvwColumnSorter.Order = SortOrder.None;

      listView1.IsChannelListView = true;
    }

    public RadioChannelGroup Group
    {
      get { return _channelGroup; }
      set { _channelGroup = value; }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e) {}

    private void ChannelsInGroupControl_Load(object sender, EventArgs e) {}

    public void OnActivated()
    {
      try
      {
        Application.DoEvents();

        Cursor.Current = Cursors.WaitCursor;

        UpdateMenuAndTabs();

        listView1.Items.Clear();

        if (Group != null)
        {
          SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (RadioGroupMap));

          sb.AddConstraint(Operator.Equals, "idGroup", Group.IdGroup);
          sb.AddOrderByField(true, "sortOrder");

          SqlStatement stmt = sb.GetStatement(true);

          IList<RadioGroupMap> maps = ObjectFactory.GetCollection<RadioGroupMap>(stmt.Execute());

          foreach (RadioGroupMap map in maps)
          {
            Channel channel = map.ReferencedChannel();
            if (channel.IsRadio == false)
            {
              continue;
            }

            int imageIndex = 1;

            if (channel.FreeToAir == false)
            {
              imageIndex = 2;
            }

            ListViewItem item = listView1.Items.Add(channel.DisplayName, imageIndex);

            item.Checked = channel.VisibleInGuide;
            item.Tag = map;

            IList<TuningDetail> details = channel.ReferringTuningDetail();
            if (details.Count > 0)
            {
              item.SubItems.Add(details[0].ChannelNumber.ToString());
            }
          }
        }
      }
      catch (Exception exp)
      {
        Log.Error("OnActivated error: {0}", exp.Message);
      }
      finally
      {
        Cursor.Current = Cursors.Default;
      }
    }

    private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      if (e.Item is ListViewItem)
      {
        ReOrder();
      }
      else if (e.Item is MPListView)
      {
        MPListView lv = e.Item as MPListView;
        if (lv != listView1)
        {
          AddSelectedItemsToGroup(e.Item as MPListView);
        }

        ReOrder();
      }
    }

    private void ReOrder()
    {
      for (int i = 0; i < listView1.Items.Count; ++i)
      {
        RadioGroupMap groupMap = (RadioGroupMap)listView1.Items[i].Tag;
        if (groupMap.SortOrder != i)
        {
          groupMap.SortOrder = i;
          groupMap.Persist();
        }
      }
    }

    private void AddSelectedItemsToGroup(MPListView sourceListView)
    {
      if (_channelGroup == null)
      {
        return;
      }

      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (ListViewItem sourceItem in sourceListView.SelectedItems)
      {
        Channel channel = null;
        if (sourceItem.Tag is Channel)
        {
          channel = (Channel)sourceItem.Tag;
        }
        else if (sourceItem.Tag is RadioGroupMap)
        {
          channel = layer.GetChannel(((RadioGroupMap)sourceItem.Tag).IdChannel);
        }
        else
        {
          continue;
        }

        RadioGroupMap groupMap = null;

        layer.AddChannelToRadioGroup(channel, _channelGroup);

        //get the new group map and set the listitem tag
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (RadioGroupMap));

        sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
        sb.AddConstraint(Operator.Equals, "idGroup", _channelGroup.IdGroup);

        SqlStatement stmt = sb.GetStatement(true);

        groupMap = ObjectFactory.GetInstance<RadioGroupMap>(stmt.Execute());

        foreach (ListViewItem item in listView1.Items)
        {
          if ((item.Tag as Channel) == channel)
          {
            item.Tag = groupMap;
            break;
          }
        }
      }
    }

    private void UpdateMenuAndTabs()
    {
      addToFavoritesToolStripMenuItem.DropDownItems.Clear();

      IList<RadioChannelGroup> groups = RadioChannelGroup.ListAll();

      foreach (RadioChannelGroup group in groups)
      {
        ToolStripMenuItem item = new ToolStripMenuItem(group.GroupName);

        item.Tag = group;
        item.Click += addToFavoritesToolStripMenuItem_Click;

        addToFavoritesToolStripMenuItem.DropDownItems.Add(item);
      }
    }

    private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      RadioChannelGroup group;
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

      if (menuItem.Tag == null)
      {
        GroupNameForm dlg = new GroupNameForm();
        dlg.IsRadio = true;

        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }

        group = new RadioChannelGroup(dlg.GroupName, 9999);
        group.Persist();

        UpdateMenuAndTabs();
      }
      else
      {
        group = (RadioChannelGroup)menuItem.Tag;
      }

      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;

      TvBusinessLayer layer = new TvBusinessLayer();

      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = listView1.Items[indexes[i]];
        RadioGroupMap map = (RadioGroupMap)item.Tag;
        Channel channel = map.ReferencedChannel();
        layer.AddChannelToRadioGroup(channel, group.GroupName);
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
      NotifyForm dlg = new NotifyForm("Deleting selected radio channels...",
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
          RadioGroupMap map = (RadioGroupMap)item.Tag;
          Channel channel = map.ReferencedChannel();
          channel.Delete();
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
          RadioGroupMap map = (RadioGroupMap)item.Tag;
          Channel channel = map.ReferencedChannel();
          FormEditChannel dlg = new FormEditChannel();
          dlg.Channel = channel;
          dlg.IsTv = false;
          dlg.ShowDialog();
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
        Channel channel = ((RadioGroupMap)listView1.Items[e.Item].Tag).ReferencedChannel();
        channel.Name = e.Label;
        channel.DisplayName = e.Label;
        channel.Persist();
      }
    }

    private void mpButtonPreview_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      RadioGroupMap map = (RadioGroupMap)listView1.Items[indexes[0]].Tag;
      FormPreview previewWindow = new FormPreview();
      previewWindow.Channel = map.ReferencedChannel();
      previewWindow.ShowDialog(this);
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      listView1.BeginUpdate();
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
      listView1.EndUpdate();
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      listView1.BeginUpdate();
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
      listView1.EndUpdate();
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
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
      NotifyForm dlg = new NotifyForm("Removing radio channels from group...",
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
          RadioGroupMap map = (RadioGroupMap)item.Tag;
          map.Remove();
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
            buttonSort.Image = global::SetupTv.Properties.Resources.icon_sort_asc;
            break;
          case SortOrder.Descending:
            buttonSort.Image = global::SetupTv.Properties.Resources.icon_sort_dsc;
            break;
          case SortOrder.None:
            buttonSort.Image = global::SetupTv.Properties.Resources.icon_sort_none;
            break;
        }
      }

      buttonOther.Image = global::SetupTv.Properties.Resources.icon_sort_none;

      //Reset the SortOrder again. Otherwise manual re-order won't be possible anymore
      lvwColumnSorter.Order = SortOrder.None;
    }

    private void mpButtonOrderByName_Click(object sender, EventArgs e)
    {
      this.listView1_ColumnClick(null, new ColumnClickEventArgs(0));
    }

    private void mpButtonOrderByNumber_Click(object sender, EventArgs e)
    {
      this.listView1_ColumnClick(null, new ColumnClickEventArgs(1));
    }
  }
}