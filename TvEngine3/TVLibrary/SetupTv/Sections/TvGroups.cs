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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using Gentle.Common;
using Gentle.Framework;
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
      IList groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
      {
        ListViewItem item = mpListViewGroups.Items.Add(group.GroupName);
        item.Tag = group;
      }
      mpListViewGroups.Sort();
    }

    private void mpButtonDeleteGroup_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListViewGroups.SelectedItems)
      {
        ChannelGroup group = (ChannelGroup)item.Tag;
        //group.Delete();
        mpListViewGroups.Items.Remove(item);
      }
    }

    private void mpButtonAddGroup_Click(object sender, EventArgs e)
    {
      GroupNameForm dlg = new GroupNameForm();
      dlg.ShowDialog(this);
      if (dlg.GroupName.Length == 0) return;
      ChannelGroup newGroup = new ChannelGroup(dlg.GroupName);
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
      IList groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
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
      if (mpComboBoxGroup.SelectedItem == null) return;
      ComboGroup g = (ComboGroup)mpComboBoxGroup.SelectedItem;
      //g.Group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);

      
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();

      mpListViewChannels.Items.Clear();
      mpListViewMapped.Items.Clear();

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      IList groupMaps = g.Group.ReferringGroupMap();
      foreach (Channel channel in channels)
      {
        if (channel.IsTv == false) continue;
        bool add = true;
        foreach (GroupMap map in groupMaps)
        {
          if (map.IdChannel == channel.IdChannel)
          {
            add = false;
            ListViewItem mappedItem = mpListViewMapped.Items.Add(channel.Name);
            mappedItem.Tag = map;
            break;
          }
        }
        if (!add) continue;
        ListViewItem newItem=mpListViewChannels.Items.Add(channel.Name);
        newItem.Tag = channel;
      }
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
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
        GroupMap newMap = new GroupMap(g.Group.IdGroup, channel.IdChannel);
        newMap.Persist();

        mpListViewChannels.Items.Remove(item);

        ListViewItem newItem = mpListViewMapped.Items.Add(channel.Name);
        newItem.Tag = newMap;
      }
//      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
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


        ListViewItem newItem = mpListViewChannels.Items.Add(map.ReferencedChannel().Name);
        newItem.Tag = map.ReferencedChannel();


        map.Remove();
      }
//      mpListViewMapped.Sort();
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpTabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {

        InitMapping();
    }

  }
}
