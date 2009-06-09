/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Globalization;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;
using Gentle.Framework;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvChannels : SectionSettings
  {
    public class CardInfo
    {
      protected Card _card;

      public Card Card
      {
        get
        {
          return _card;
        }
      }

      public CardInfo(Card card)
      {
        _card = card;
      }

      public override string ToString()
      {
        return _card.Name;
      }
    }
    private readonly MPListViewStringColumnSorter lvwColumnSorter;
    private readonly MPListViewStringColumnSorter lvwColumnSorter2;

    public TvChannels()
      : this("TV Channels")
    {
    }

    public TvChannels(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.Descending;
      lvwColumnSorter2.OrderType = MPListViewStringColumnSorter.OrderTypes.AsValue;
      mpListView1.ListViewItemSorter = lvwColumnSorter;
    }

    public override void OnSectionDeActivated()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        Channel ch = (Channel)mpListView1.Items[i].Tag;
        if (ch.SortOrder != i + 1)
        {
          ch.SortOrder = i + 1;
        }
        ch.VisibleInGuide = mpListView1.Items[i].Checked;
        ch.Persist();
      }

      //DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      base.OnSectionDeActivated();
    }

    private void UpdateMenu()
    {

      addToFavoritesToolStripMenuItem.DropDownItems.Clear();
      IList<ChannelGroup> groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
      {
        ToolStripMenuItem item = new ToolStripMenuItem(group.GroupName);
        item.Tag = group;
        item.Click += OnAddToFavoritesMenuItem_Click;
        addToFavoritesToolStripMenuItem.DropDownItems.Add(item);
      }
      ToolStripMenuItem itemNew = new ToolStripMenuItem("New...");
      itemNew.Click += OnAddToFavoritesMenuItem_Click;
      addToFavoritesToolStripMenuItem.DropDownItems.Add(itemNew);
    }

    public override void OnSectionActivated()
    {
      UpdateMenu();

      base.OnSectionActivated();

      RefreshAllChannels();
    }

    private void RefreshAllChannels()
    {
      IList<Card> dbsCards = Card.ListAll();
      Dictionary<int, CardType> cards = new Dictionary<int, CardType>();
      foreach (Card card in dbsCards)
      {
        cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
      }

      mpListView1.BeginUpdate();
      mpListView1.Items.Clear();
      Channel.ListAll();
      int channelCount = 0;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());
      ChannelMap.ListAll();
      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        bool dvbip = false;
        bool webstream = false;
        bool notmapped = true;
        if (ch.IsTv == false)
          continue;
        channelCount++;
        if (ch.IsWebstream())
        {
          webstream = true;
          notmapped = false;
        }
        if (notmapped)
        {
          IList<ChannelMap> maps = ch.ReferringChannelMap();
          foreach (ChannelMap map in maps)
          {
            if (cards.ContainsKey(map.IdCard))
            {
              CardType type = cards[map.IdCard];
              switch (type)
              {
                case CardType.Analog:
                  analog = true;
                  notmapped = false;
                  break;
                case CardType.DvbC:
                  dvbc = true;
                  notmapped = false;
                  break;
                case CardType.DvbT:
                  dvbt = true;
                  notmapped = false;
                  break;
                case CardType.DvbS:
                  dvbs = true;
                  notmapped = false;
                  break;
                case CardType.Atsc:
                  atsc = true;
                  notmapped = false;
                  break;
                case CardType.DvbIP: dvbip = true; notmapped = false; break;
              }
            }
          }
        }
        StringBuilder builder = new StringBuilder();

        if (analog)
        {
          builder.Append("Analog");
        }
        if (notmapped)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("Channel not mapped to a card");
        }
        if (dvbc)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("DVB-C");
        }
        if (dvbt)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("DVB-T");
        }
        if (dvbs)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("DVB-S");
        }
        if (atsc)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("ATSC");
        }
        if (dvbip)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-IP");
        }
        if (webstream)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("Webstream");
        }
        int imageIndex = 1;
        if (ch.FreeToAir == false)
          imageIndex = 2;

        ListViewItem item = new ListViewItem(ch.DisplayName, imageIndex);
        item.SubItems.Add("-");
        item.Checked = ch.VisibleInGuide;
        item.Tag = ch;
        item.SubItems.Add(builder.ToString());
        string provider = "";
        foreach (TuningDetail detail in ch.ReferringTuningDetail())
        {
          provider += String.Format("{0},", detail.Provider);
          float frequency;
          switch (detail.ChannelType)
          {
            case 0://Analog
              if (detail.VideoSource == (int)AnalogChannel.VideoInputType.Tuner)
              {
                frequency = detail.Frequency;
                frequency /= 1000000.0f;
                item.SubItems.Add(String.Format("#{0} {1} MHz", detail.ChannelNumber, frequency.ToString("f2")));
              }
              else
              {
                item.SubItems.Add(detail.VideoSource.ToString());
              }
              break;

            case 1://ATSC
              item.SubItems.Add(String.Format("{0} {1}:{2}", detail.ChannelNumber, detail.MajorChannel, detail.MinorChannel));
              break;

            case 2:// DVB-C
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz SR:{1}", frequency.ToString("f2"), detail.Symbolrate));
              break;

            case 3:// DVB-S
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz {1}", frequency.ToString("f2"), (((Polarisation)detail.Polarisation))));
              break;

            case 4:// DVB-T
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz BW:{1}", frequency.ToString("f2"), detail.Bandwidth));
              break;

            case 7:// DVB-IP
              item.SubItems.Add(detail.Url);
              break;

            case 5:// Webstream
              item.SubItems.Add(detail.Url);
              break;
          }
        }
        if (provider.Length > 1)
          provider = provider.Substring(0, provider.Length - 1);
        item.SubItems[1].Text = (provider);
        items.Add(item);
      }
      mpListView1.Items.AddRange(items.ToArray());
      mpListView1.EndUpdate();
      mpLabelChannelCount.Text = String.Format("Total channels: {0}", channelCount);
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void OnAddToFavoritesMenuItem_Click(object sender, EventArgs e)
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
        group = new ChannelGroup(dlg.GroupName, 9999);
        group.Persist();
        UpdateMenu();
      }
      else
      {
        group = (ChannelGroup)menuItem.Tag;
      }

      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      TvBusinessLayer layer = new TvBusinessLayer();
      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[indexes[i]];
        Channel channel = (Channel)item.Tag;
        layer.AddChannelToGroup(channel, group.GroupName);
      }
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      string holder = String.Format("Are you sure you want to clear all channels?");

      if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
      {
        return;
      }

      NotifyForm dlg = new NotifyForm("Clearing all tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      IList<Channel> channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        if (channel.IsTv)
        {
          Broker.Execute("delete from TvMovieMapping WHERE idChannel=" + channel.IdChannel);
          channel.Delete();
        }
      }
      dlg.Close();
      /*Gentle.Framework.Broker.Execute("delete from history");
      Gentle.Framework.Broker.Execute("delete from tuningdetail");
      Gentle.Framework.Broker.Execute("delete from GroupMap");
      Gentle.Framework.Broker.Execute("delete from Channelmap");
      Gentle.Framework.Broker.Execute("delete from Recording");
      Gentle.Framework.Broker.Execute("delete from CanceledSchedule");
      Gentle.Framework.Broker.Execute("delete from Schedule");
      Gentle.Framework.Broker.Execute("delete from Program");
      Gentle.Framework.Broker.Execute("delete from Channel");
      mpListView1.BeginUpdate();*/
      /*
      IList details = TuningDetail.ListAll();
      foreach (TuningDetail detail in details) detail.Remove();

      IList groupmaps = GroupMap.ListAll();
      foreach (GroupMap groupmap in groupmaps) groupmap.Remove();

      IList channelMaps = ChannelMap.ListAll();
      foreach (ChannelMap channelMap in channelMaps) channelMap.Remove();

      IList recordings = Recording.ListAll();
      foreach (Recording recording in recordings) recording.Remove();

      IList canceledSchedules = CanceledSchedule.ListAll();
      foreach (CanceledSchedule canceledSchedule in canceledSchedules) canceledSchedule.Remove();

      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules) schedule.Remove();

      IList programs = Program.ListAll();
      foreach (Program program in programs) program.Remove();

      IList channels = Channel.ListAll();
      foreach (Channel channel in channels) channel.Remove();
      */

      //mpListView1.EndUpdate();
      OnSectionActivated();
    }

    private void TvChannels_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      IList<Schedule> schedules = Schedule.ListAll();
      TvServer server = new TvServer();

      //Since it takes a very long time to add channels, make sure the user really wants to delete them
      if (mpListView1.SelectedItems.Count > 0)
      {
        string holder = String.Format("Are you sure you want to delete these {0:d} channels?", mpListView1.SelectedItems.Count);

        if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          mpListView1.EndUpdate();
          return;
        }
      }
      NotifyForm dlg = new NotifyForm("Deleting selected tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;

        //also delete any still active schedules
        if (schedules != null)
        {
          for (int i = schedules.Count - 1; i > -1; i--)
          {
            Schedule schedule = schedules[i];
            if (schedule.IdChannel == channel.IdChannel)
            {
              server.StopRecordingSchedule(schedule.IdSchedule);
              schedule.Delete();
              schedules.RemoveAt(i);
            }
          }
        }

        channel.Delete();
        mpListView1.Items.Remove(item);
      }

      dlg.Close();
      mpListView1.EndUpdate();
      ReOrder();
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void ReOrder()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        //mpListView1.Items[i].Text = (i + 1).ToString();

        Channel channel = (Channel)mpListView1.Items[i].Tag;
        if (channel.SortOrder != i)
        {
          channel.SortOrder = i;
          channel.Persist();
        }
      }
    }

    private void mpListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      int oldIndex = e.Item;
      ListViewItem item = mpListView1.Items[oldIndex];

      /* chemelli: 
       * works only for channels name equal to numbers.
       * Now fixed but commented out
       * (What is needed for ?)
       */
      int newIndex;
      if (Int32.TryParse(e.Label, out newIndex))
      {
        if ((newIndex - 1) == oldIndex)
          return;
        mpListView1.Items.RemoveAt(oldIndex);
        mpListView1.Items.Insert((newIndex - 1), item);
        ReOrder();
        e.CancelEdit = true;
      }
      /* chemelli: end of block
       */

      if (e.Label != null) // GEMX: ==null means, nothing has changed. This check is necessary, otherwise gentle gives an error about a null value
      {
        Channel channel = (Channel)mpListView1.Items[oldIndex].Tag;
        channel.Name = e.Label;
        channel.DisplayName = e.Label;
        channel.Persist();
      }
    }

    private void mpButtonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = channel;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        channel.Persist();
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          if (detail.Name != channel.Name)
          {
            detail.Name = channel.Name;
            detail.Persist();
          }
        }
        mpListView1.Items[indexes[0]].Text = channel.DisplayName;
      }
    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      mpListView1.Sort();
      ReOrder();
    }

    private void mpButtonPreview_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormPreview previewWindow = new FormPreview();
      previewWindow.Channel = channel;
      previewWindow.ShowDialog(this);
    }

    private void mpListView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      ReOrder();
    }

    private void mpListView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      mpButtonEdit_Click(null, null);
    }

    private void deleteThisChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonDel_Click(null, null);
    }

    private void editChannelToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonEdit_Click(null, null);
    }

    private void mpButtonAdd_Click(object sender, EventArgs e)
    {
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = null;
      if (dlg.ShowDialog(this) == DialogResult.OK)
        OnSectionActivated();
    }


    private void mpButtonUncheckEncrypted_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Unchecking all scrambled tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (ListViewItem item in mpListView1.Items)
      {
        Channel channel = (Channel)item.Tag;
        if (!channel.FreeToAir)
          item.Checked = false;
      }
      dlg.Close();
    }

    private void mpButtonDeleteEncrypted_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Clearing all scrambled tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      List<ListViewItem> itemsToRemove = new List<ListViewItem>();
      foreach (ListViewItem item in mpListView1.Items)
      {
        Channel channel = (Channel)item.Tag;
        if (channel.FreeToAir == false)
        {
          channel.Delete();
          itemsToRemove.Add(item);
        }
      }
      foreach (ListViewItem item in itemsToRemove)
        mpListView1.Items.Remove(item);
      dlg.Close();
      ReOrder();
      RemoteControl.Instance.OnNewSchedule();
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void renameMarkedChannelsBySIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Renaming selected tv channels by SID ...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList<TuningDetail> details = channel.ReferringTuningDetail();
        if (details.Count > 0)
        {
          channel.DisplayName = (details[0]).ServiceId.ToString();
          channel.Persist();
          item.Tag = channel;
        }
      }
      dlg.Close();
      OnSectionActivated();
    }

    private void addSIDInFrontOfNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Adding SID in front of name...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList<TuningDetail> details = channel.ReferringTuningDetail();
        if (details.Count > 0)
        {
          channel.DisplayName = (details[0]).ServiceId + " " + channel.DisplayName;
          channel.Persist();
          item.Tag = channel;
        }
      }
      dlg.Close();
      OnSectionActivated();
    }

    private void renumberChannelsBySIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Renumbering tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList<TuningDetail> details = channel.ReferringTuningDetail();
        foreach (TuningDetail detail in details)
        {
          detail.ChannelNumber = detail.ServiceId;
          detail.Persist();
        }
      }
      dlg.Close();
    }
  }
}
