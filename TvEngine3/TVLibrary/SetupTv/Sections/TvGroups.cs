/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvGroups : SectionSettings
  {
    struct ComboGroup
    {
      public ChannelGroup Group;
      public override string ToString()
      {
        return Group.GroupName;
      }

    }
    public TvGroups()
      : this("TV Groups")
    {
    }

    public TvGroups(string name)
      : base(name)
    {
      InitializeComponent();
      mpListViewChannels.ListViewItemSorter = new MPListViewSortOnColumn(0);
//      mpListViewMapped.ListViewItemSorter = new MPListViewSortOnColumn(0);
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
      EntityList<ChannelGroup> groups = DatabaseManager.Instance.GetEntities<ChannelGroup>();
      foreach (ChannelGroup group in groups)
      {
        ListViewItem item = mpListViewGroups.Items.Add(group.GroupName);
        item.Tag = group;
      }
    }

    private void mpButtonDeleteGroup_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListViewGroups.SelectedItems)
      {
        ChannelGroup group = (ChannelGroup)item.Tag;
        group.DeleteAll();
        mpListViewGroups.Items.Remove(item);
      }
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpButtonAddGroup_Click(object sender, EventArgs e)
    {
      GroupNameForm dlg = new GroupNameForm();
      dlg.ShowDialog(this);
      if (dlg.GroupName.Length == 0) return;
      ChannelGroup newGroup = ChannelGroup.Create();
      newGroup.GroupName = dlg.GroupName;
      DatabaseManager.Instance.SaveChanges();
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
      EntityList<ChannelGroup> groups = DatabaseManager.Instance.GetEntities<ChannelGroup>();
      foreach (ChannelGroup group in groups)
      {
        ComboGroup g = new ComboGroup();
        g.Group = group;
        mpComboBoxGroup.Items.Add(g);
      }
      if (mpComboBoxGroup.Items.Count > 0)
        mpComboBoxGroup.SelectedIndex = 0;
      mpComboBoxGroup_SelectedIndexChanged(null,null);
    }

    private void mpComboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (mpComboBoxGroup.SelectedItem == null) return;
      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;
      mpListViewMapped.Items.Clear();
      g.Group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);

      mpListViewMapped.BeginUpdate();
      foreach (GroupMap map in g.Group.GroupMaps)
      {
        ListViewItem item=mpListViewMapped.Items.Add(map.Channel.Name);
        item.Tag = map;
      }
      mpListViewMapped.EndUpdate();

      mpListViewChannels.BeginUpdate();
      mpListViewChannels.Items.Clear();
      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddOrderBy(Channel.SortOrderEntityColumn);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);
      foreach (Channel channel in channels)
      {
        if (channel.IsTv == false) continue;
        bool add = true;
        foreach (GroupMap map in g.Group.GroupMaps)
        {
          if (map.Channel.IdChannel == channel.IdChannel)
          {
            add = false;
            break;
          }
        }
        if (!add) continue;
        ListViewItem newItem=mpListViewChannels.Items.Add(channel.Name);
        newItem.Tag = channel;
      }
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
    }

    private void mpButtonMap_Click(object sender, EventArgs e)
    {
      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
      foreach (ListViewItem item in selectedItems)
      {
        Channel channel = (Channel)item.Tag;
        GroupMap newMap = GroupMap.Create();
        newMap.Channel = channel;
        newMap.ChannelGroup = g.Group;

        mpListViewChannels.Items.Remove(item);

        ListViewItem newItem = mpListViewMapped.Items.Add(channel.Name);
        newItem.Tag = newMap;
      }
//      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

      foreach (ListViewItem item in selectedItems)
      {
        GroupMap map = (GroupMap)item.Tag;
        mpListViewMapped.Items.Remove(item);


        ListViewItem newItem = mpListViewChannels.Items.Add(map.Channel.Name);
        newItem.Tag = map.Channel;


        map.Delete();
      }
//      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpTabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {

        InitMapping();
    }

  }
}
