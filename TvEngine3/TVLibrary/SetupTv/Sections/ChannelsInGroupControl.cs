using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
namespace SetupTv.Sections
{
  public partial class ChannelsInGroupControl : UserControl
  {
    ChannelGroup _channelGroup;
    public ChannelsInGroupControl()
    {
      InitializeComponent();
    }
    public ChannelGroup Group
    {
      get
      {
        return _channelGroup;
      }
      set
      {
        _channelGroup = value;
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void ChannelsInGroupControl_Load(object sender, EventArgs e)
    {

    }

    public void OnActivated()
    {
      listView1.Items.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(GroupMap));
      sb.AddConstraint(Operator.Equals, "idGroup", Group.IdGroup);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList maps = ObjectFactory.GetCollection(typeof(GroupMap), stmt.Execute());
      
      foreach (GroupMap map in maps)
      {
        Channel channel = map.ReferencedChannel();
        if (channel.IsTv == false) continue;
        int index = listView1.Items.Count + 1;
        ListViewItem item = listView1.Items.Add(index.ToString());
        item.SubItems.Add(channel.Name);
        item.Checked = channel.VisibleInGuide;
        item.Tag = map;
      }
    }

    private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      ReOrder();
    }

    void ReOrder()
    {
      for (int i = 0; i < listView1.Items.Count; ++i)
      {
        listView1.Items[i].Text = (i + 1).ToString();

        GroupMap groupMap = (GroupMap)listView1.Items[i].Tag;
        if (groupMap.SortOrder != i)
        {
          groupMap.SortOrder = i;
          groupMap.Persist();
        }
      }
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      listView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
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
      if (indexes.Count == 0) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index > 0 && index + 1 < listView1.Items.Count)
        {
          ListViewItem item = listView1.Items[index];
          listView1.Items.RemoveAt(index);
          listView1.Items.Insert(index + 1, item);
        }
      }
      ReOrder();
      listView1.EndUpdate();
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      listView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index > 0 && index + 1 < listView1.Items.Count)
        {
          ListViewItem item = listView1.Items[index];
          listView1.Items.RemoveAt(index);
          GroupMap map = (GroupMap)item.Tag;
          map.Remove();
        }
      }
      ReOrder();
      listView1.EndUpdate();
    }

  }
}
