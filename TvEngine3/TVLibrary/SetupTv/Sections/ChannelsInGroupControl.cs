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
      if (Group != null)
      {
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
          int imageIndex = 1;
          if (channel.FreeToAir == false)
            imageIndex = 2;
          ListViewItem item = listView1.Items.Add(index.ToString(), imageIndex);
          item.SubItems.Add(channel.Name);
          item.Checked = channel.VisibleInGuide;
          item.Tag = map;
        }
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
      if (listView1.Items.Count < 2) return;
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
    }

    private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonDel_Click(null, null);
    }

    private void deleteThisChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
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
          Channel channel = map.ReferencedChannel();
          channel.Delete();
        }
      }
      ReOrder();
      OnActivated();
    }

    private void removeEntireGroupToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Group.Delete();
      Group = null;
      OnActivated();

    }

    private void editChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index > 0 && index + 1 < listView1.Items.Count)
        {
          ListViewItem item = listView1.Items[index];
          GroupMap map = (GroupMap)item.Tag;
          Channel channel = map.ReferencedChannel();
          FormEditChannel dlg = new FormEditChannel();
          dlg.Channel = channel;
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
      try
      {
        int oldIndex = e.Item;
        ListViewItem item = listView1.Items[oldIndex];
        int newIndex = (Int32.Parse(e.Label) - 1);
        if (newIndex == oldIndex) return;

        listView1.Items.RemoveAt(oldIndex);
        listView1.Items.Insert(newIndex, item);
        ReOrder();
        e.CancelEdit = true;
      }
      catch (Exception)
      {
      }
    }

  }
}
