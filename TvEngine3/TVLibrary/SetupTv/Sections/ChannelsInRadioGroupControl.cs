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
  public partial class ChannelsInRadioGroupControl : UserControl
  {
    RadioChannelGroup _channelGroup;
    public ChannelsInRadioGroupControl()
    {
      InitializeComponent();
    }
    public RadioChannelGroup Group
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
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(RadioGroupMap));
        sb.AddConstraint(Operator.Equals, "idGroup", Group.IdGroup);
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        IList maps = ObjectFactory.GetCollection(typeof(RadioGroupMap), stmt.Execute());

        foreach (RadioGroupMap map in maps)
        {
          Channel channel = map.ReferencedChannel();
          if (channel.IsRadio == false) continue;
          int index = listView1.Items.Count + 1;
          int imageIndex = 3;
          if (channel.FreeToAir == false)
            imageIndex = 0;
          ListViewItem item = listView1.Items.Add(channel.DisplayName, imageIndex);
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
        //listView1.Items[i].Text = (i + 1).ToString();

        RadioGroupMap groupMap = (RadioGroupMap)listView1.Items[i].Tag;
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
      if (listView1.SelectedItems.Count > 0)
      {
        string holder = String.Format("Are you sure you want to delete these {0:d} channels?", listView1.SelectedItems.Count);

        if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          return;
        }
      }

      
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
      NotifyForm dlg = new NotifyForm("Removing radio channels from group...", "This can take some time\n\nPlease be patient...");
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

    private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonDel_Click(null, null);
    }

    private void deleteThisChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        string holder = String.Format("Are you sure you want to delete these {0:d} channels?", listView1.SelectedItems.Count);

        if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          return;
        }
      }

      
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
      NotifyForm dlg = new NotifyForm("Deleting selected radio channels...", "This can take some time\n\nPlease be patient...");
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

    private void removeEntireGroupToolStripMenuItem_Click(object sender, EventArgs e)
    {
      string holder = String.Format("Are you sure you want to delete this group?");

      if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
      {
        return;
      }
      NotifyForm dlg = new NotifyForm("Removing entire group...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      Group.Delete();
      Group = null;
      dlg.Close();
      OnActivated();

    }

    private void editChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
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

    private void mpButtonPreview_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
      if (indexes.Count == 0) return;
      RadioGroupMap map = (RadioGroupMap)listView1.Items[indexes[0]].Tag;
      FormPreview previewWindow = new FormPreview();
      previewWindow.Channel = map.ReferencedChannel();
      previewWindow.ShowDialog(this);
    }

  }
}
