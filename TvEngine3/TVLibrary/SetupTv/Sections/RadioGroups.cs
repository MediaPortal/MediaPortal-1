/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gentle.Framework;
using TvDatabase;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class RadioGroups : SectionSettings
  {
    struct ComboGroup
    {
      public RadioChannelGroup Group;
      public override string ToString()
      {
        return Group.GroupName;
      }

    }
    private readonly MPListViewStringColumnSorter lvwColumnSorter1;
    private readonly MPListViewStringColumnSorter lvwColumnSorter2;

    public RadioGroups()
      : this("Radio Groups")
    {
    }

    public RadioGroups(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter1 = new MPListViewStringColumnSorter();
      lvwColumnSorter1.Order = SortOrder.None;
      mpListViewChannels.ListViewItemSorter = lvwColumnSorter1;
      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.None;
      mpListViewMapped.ListViewItemSorter = lvwColumnSorter2;
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      Init();
      InitMapping();
    }

    void Init()
    {
      mpListViewGroups.Items.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(RadioChannelGroup));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList<RadioChannelGroup> groups = ObjectFactory.GetCollection<RadioChannelGroup>(stmt.Execute());
      foreach (RadioChannelGroup group in groups)
      {
        ListViewItem item = mpListViewGroups.Items.Add(group.GroupName);
        item.Tag = group;
      }
      mpListViewGroups.Sort();
    }

    private void mpButtonDeleteGroup_Click(object sender, EventArgs e)
    {
      string holder = String.Format("Are you sure you want to delete this group?");
      if (mpListViewGroups.SelectedItems.Count > 1)
      {
        holder = String.Format("Are you sure you want to delete these groups?");
      }
      if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
      {
        return;
      }
      foreach (ListViewItem item in mpListViewGroups.SelectedItems)
      {
        RadioChannelGroup group = (RadioChannelGroup)item.Tag;
        group.Delete();
        mpListViewGroups.Items.Remove(item);
      }
    }

    private void mpButtonAddGroup_Click(object sender, EventArgs e)
    {
      GroupNameForm dlg = new GroupNameForm();
      dlg.ShowDialog(this);
      if (dlg.GroupName.Length == 0)
        return;
      RadioChannelGroup newGroup = new RadioChannelGroup(dlg.GroupName, 9999);
      newGroup.Persist();
      Init();
    }
    private void mpTabControl1_TabIndexChanged(object sender, EventArgs e)
    {
      if (mpTabControl1.TabIndex == 1)
      {
        InitMapping();
      }
    }

    void InitMapping()
    {
      mpComboBoxGroup.Items.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(RadioChannelGroup));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList<RadioChannelGroup> groups = ObjectFactory.GetCollection<RadioChannelGroup>(stmt.Execute());
      foreach (RadioChannelGroup group in groups)
      {
        ComboGroup g = new ComboGroup();
        g.Group = group;
        mpComboBoxGroup.Items.Add(g);
      }

      if (mpComboBoxGroup.Items.Count > 0)
        mpComboBoxGroup.SelectedIndex = 0;
      //mpComboBoxGroup_SelectedIndexChanged(null,null);
    }

    private void mpComboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (mpComboBoxGroup.SelectedItem == null)
        return;
      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;
      //g.Group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);


      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();

      mpListViewChannels.Items.Clear();
      mpListViewMapped.Items.Clear();

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "isRadio", 1);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());


      Dictionary<int, bool> channelsMapped = new Dictionary<int, bool>();

      sb = new SqlBuilder(StatementType.Select, typeof(RadioGroupMap));
      sb.AddConstraint(Operator.Equals, "idGroup", g.Group.IdGroup);
      sb.AddOrderByField(true, "sortOrder");
      stmt = sb.GetStatement(true);
      IList<RadioGroupMap> maps = ObjectFactory.GetCollection<RadioGroupMap>(stmt.Execute());

      foreach (RadioGroupMap map in maps)
      {
        Channel channel = map.ReferencedChannel();
        if (channel.IsRadio == false)
          continue;
        int imageIndex = 0;
        if (channel.FreeToAir)
          imageIndex = 3;
        ListViewItem mappedItem = mpListViewMapped.Items.Add(channel.DisplayName, imageIndex);

        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          mappedItem.SubItems.Add(Convert.ToString(detail.ChannelNumber).PadLeft(5, '0'));
        }

        mappedItem.Tag = map;
        channelsMapped[channel.IdChannel] = true;
      }

      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Channel channel in channels)
      {
        if (channelsMapped.ContainsKey(channel.IdChannel))
          continue;
        int imageIndex = 0;
        if (channel.FreeToAir)
          imageIndex = 3;
        ListViewItem newItem = new ListViewItem(channel.DisplayName, imageIndex);
        newItem.Tag = channel;
        items.Add(newItem);
      }
      mpListViewChannels.Items.AddRange(items.ToArray());
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      mpListViewChannels.Sort();
      mpListViewMapped.Sort();
    }

    private void mpButtonMap_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Mapping selected channels to group...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
      foreach (ListViewItem item in selectedItems)
      {
        Channel channel = (Channel)item.Tag;
        RadioGroupMap newMap = new RadioGroupMap(g.Group.IdGroup, channel.IdChannel, channel.SortOrder);
        newMap.Persist();

        mpListViewChannels.Items.Remove(item);

        int imageIndex = 0;
        if (channel.FreeToAir)
          imageIndex = 3;
        ListViewItem newItem = mpListViewMapped.Items.Add(channel.DisplayName, imageIndex);
        newItem.Tag = newMap;
      }
      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      ReOrderMap();
      dlg.Close();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Unmapping selected channels to group...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

      foreach (ListViewItem item in selectedItems)
      {
        RadioGroupMap map = (RadioGroupMap)item.Tag;
        mpListViewMapped.Items.Remove(item);


        int imageIndex = 1;
        if (map.ReferencedChannel().FreeToAir == false)
          imageIndex = 2;
        ListViewItem newItem = mpListViewChannels.Items.Add(map.ReferencedChannel().DisplayName, imageIndex);
        newItem.Tag = map.ReferencedChannel();


        map.Remove();
      }
      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      dlg.Close();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpTabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {

      InitMapping();
    }

    private void mpListViewGroups_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Real sorting is now done via the up/down buttons
      /*if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter.Order == SortOrder.Ascending)
        {
          lvwColumnSorter.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListViewGroups.Sort();*/
    }

    private void mpListViewMapped_DragDrop(object sender, DragEventArgs e)
    {

    }

    private void mpListViewMapped_DragEnter(object sender, DragEventArgs e)
    {

    }

    private void mpListViewMapped_ItemDrag(object sender, ItemDragEventArgs e)
    {
      ReOrderMap();
    }
    void ReOrderMap()
    {
      for (int i = 0; i < mpListViewMapped.Items.Count; ++i)
      {
        RadioGroupMap map = (RadioGroupMap)mpListViewMapped.Items[i].Tag;
        map.SortOrder = i;
        map.Persist();
      }
    }
    void ReOrderGroups()
    {
      for (int i = 0; i < mpListViewGroups.Items.Count; ++i)
      {
        RadioChannelGroup group = (RadioChannelGroup)mpListViewGroups.Items[i].Tag;

        group.SortOrder = i;
        group.Persist();
      }
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      mpListViewGroups.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewGroups.SelectedIndices;
      if (indexes.Count == 0)
        return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListViewGroups.Items[index];
          mpListViewGroups.Items.RemoveAt(index);
          mpListViewGroups.Items.Insert(index - 1, item);
        }
      }
      ReOrderGroups();
      mpListViewGroups.EndUpdate();
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      mpListViewGroups.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewGroups.SelectedIndices;
      if (indexes.Count == 0)
        return;
      if (mpListViewGroups.Items.Count < 2)
        return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mpListViewGroups.Items[index];
        mpListViewGroups.Items.RemoveAt(index);
        if (index + 1 < mpListViewGroups.Items.Count)
          mpListViewGroups.Items.Insert(index + 1, item);
        else
          mpListViewGroups.Items.Add(item);
      }
      ReOrderGroups();
      mpListViewGroups.EndUpdate();
    }
    private void RenameGroup()
    {
      ListView.SelectedListViewItemCollection items = mpListViewGroups.SelectedItems;
      if (items.Count != 1)
        return;
      RadioChannelGroup group = (RadioChannelGroup)items[0].Tag;
      GroupNameForm dlg = new GroupNameForm(group.GroupName);
      dlg.ShowDialog(this);
      if (dlg.GroupName.Length == 0)
        return;
      group.GroupName = dlg.GroupName;
      group.Persist();
      Init();
    }

    private void mpButtonRenameGroup_Click(object sender, EventArgs e)
    {
      RenameGroup();
    }

    private void mpListViewGroups_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      RenameGroup();
    }

    private void mpListViewChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter1.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter1.Order = lvwColumnSorter1.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter1.SortColumn = e.Column;
        lvwColumnSorter1.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      mpListViewChannels.Sort();
    }

    private void mpListViewMapped_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Real sorting is now done via the up/down buttons
      /*
      mpListViewMapped.BeginUpdate();
      if (e.Column == lvwColumnSorter2.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter2.Order == SortOrder.Ascending)
        {
          lvwColumnSorter2.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter2.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter2.SortColumn = e.Column;
        lvwColumnSorter2.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListViewMapped.Sort();
      ReOrderMap();
      mpListViewMapped.EndUpdate();
       */
    }

    private void btnUp_Click(object sender, EventArgs e)
    {
      mpListViewMapped.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewMapped.SelectedIndices;
      if (indexes.Count == 0)
        return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListViewMapped.Items[index];
          mpListViewMapped.Items.RemoveAt(index);
          mpListViewMapped.Items.Insert(index - 1, item);
        }
      }
      ReOrderMap();
      mpListViewMapped.EndUpdate();
    }

    private void btnDown_Click(object sender, EventArgs e)
    {
      mpListViewMapped.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListViewMapped.SelectedIndices;
      if (indexes.Count == 0)
        return;
      if (mpListViewMapped.Items.Count < 2)
        return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mpListViewMapped.Items[index];
        mpListViewMapped.Items.RemoveAt(index);
        if (index + 1 < mpListViewMapped.Items.Count)
          mpListViewMapped.Items.Insert(index + 1, item);
        else
          mpListViewMapped.Items.Add(item);
      }
      ReOrderMap();
      mpListViewMapped.EndUpdate();
    }
  }
}
