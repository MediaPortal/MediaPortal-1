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
using System.Text;
using System.Windows.Forms;
using TvControl;
using MediaPortal.Playlists;
using Gentle.Framework;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class RadioChannels : SectionSettings
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

    private bool suppressRefresh = false;

    public RadioChannels()
      : this("Radio Channels")
    {
      mpListView1.IsChannelListView = true;
      tabControl1.AllowReorderTabs = true;
    }

    public RadioChannels(string name)
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
    

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();

      this.RefreshAll();
    }

    private void RefreshAll()
    {
      this.RefreshTabs();
      this.RefreshContextMenu();

      Application.DoEvents();

      this.RefreshAllChannels();
    }

    private void RefreshTabs()
    {
      while (tabControl1.TabPages.Count > 1)
      {
        tabControl1.TabPages.RemoveAt(1);
      }

      IList<RadioChannelGroup> groups = RadioChannelGroup.ListAll();

      foreach (RadioChannelGroup group in groups)
      {
        TabPage page = new TabPage(group.GroupName);
        page.SuspendLayout();

        ChannelsInRadioGroupControl channelsInRadioGroupControl = new ChannelsInRadioGroupControl();
        channelsInRadioGroupControl.Location = new System.Drawing.Point(9, 9);
        channelsInRadioGroupControl.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom)
                                              | AnchorStyles.Left)
                                             | AnchorStyles.Right;

        page.Controls.Add(channelsInRadioGroupControl);

        page.Tag = group;
        page.Location = new System.Drawing.Point(4, 22);
        page.Padding = new Padding(3);
        page.Size = new System.Drawing.Size(457, 374);
        page.UseVisualStyleBackColor = true;
        page.PerformLayout();
        page.ResumeLayout(false);

        tabControl1.TabPages.Add(page);
      }
    }

    private void RefreshContextMenu()
    {
      addToFavoritesToolStripMenuItem.DropDownItems.Clear();

      IList<RadioChannelGroup> groups = RadioChannelGroup.ListAll();

      foreach (RadioChannelGroup group in groups)
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

    private void RefreshAllChannels()
    {
      try
      {
        Cursor.Current = Cursors.WaitCursor;

        IList<Card> dbsCards = Card.ListAll();

        Dictionary<int, CardType> cards = new Dictionary<int, CardType>();

        foreach (Card card in dbsCards)
        {
          cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
        }
        base.OnSectionActivated();

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
          bool fmRadio = false;
          bool notmapped = true;
          if (ch.IsRadio == false)
            continue;
          channelCount++;
          if (ch.IsWebstream())
          {
            webstream = true;
            notmapped = false;
          }
          if (ch.IsFMRadio())
          {
            fmRadio = true;
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
          if (fmRadio)
          {
            if (builder.Length > 0)
              builder.Append(",");
            builder.Append("FM Radio");
          }
          int imageIndex = 3;
          if (ch.FreeToAir == false)
            imageIndex = 0;

          ListViewItem item = new ListViewItem(ch.DisplayName, imageIndex);

          IList<string> groupNames = ch.GroupNames;
          if (groupNames.Count > 0)
          {
            StringBuilder sbGroupNames = new StringBuilder();

            foreach (string name in groupNames)
            {
              if (name == TvConstants.RadioGroupNames.AllChannels)
                continue;

              if (sbGroupNames.Length > 0)
                sbGroupNames.Append(", ");

              sbGroupNames.Append(name);
            }

            item.SubItems.Add(sbGroupNames.ToString());
          }
          else
          {
            item.SubItems.Add(string.Empty);
          }

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
              case 0://analog
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

              case 2:// DVBC
                frequency = detail.Frequency;
                frequency /= 1000.0f;
                item.SubItems.Add(String.Format("{0} MHz SR:{1}", frequency.ToString("f2"), detail.Symbolrate));
                break;

              case 3:// DVBS
                frequency = detail.Frequency;
                frequency /= 1000.0f;
                item.SubItems.Add(String.Format("{0} MHz {1}", frequency.ToString("f2"), (((Polarisation)detail.Polarisation))));
                break;

              case 4:// DVBT
                frequency = detail.Frequency;
                frequency /= 1000.0f;
                item.SubItems.Add(String.Format("{0} MHz BW:{1}", frequency.ToString("f2"), detail.Bandwidth));
                break;
              case 5:// Webstream
                item.SubItems.Add(detail.Url);
                break;
              case 6:// FM Radio
                frequency = detail.Frequency;
                frequency /= 1000000.0f;
                item.SubItems.Add(String.Format("{0} MHz", frequency.ToString("f3")));
                break;
              case 7:// DVB-IP
                item.SubItems.Add(detail.Url);
                break;
            }
          }
          if (provider.Length > 1)
            provider = provider.Substring(0, provider.Length - 1);

          item.SubItems[2].Text = (provider);

          items.Add(item);
        }
        mpListView1.Items.AddRange(items.ToArray());
        mpListView1.EndUpdate();
        tabControl1.TabPages[0].Text = string.Format("Channels ({0})", channelCount);
        mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      catch (Exception exp)
      {
        Log.Error("OnSectionActivated error: {0}", exp.Message);
      }
      finally
      {
        Cursor.Current = Cursors.Default;
      }
    }

    void OnAddToFavoritesMenuItem_Click(object sender, EventArgs e)
    {
      RadioChannelGroup group;
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
      if (menuItem.Tag == null)
      {
        GroupNameForm dlg = new GroupNameForm();
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }
        group = new RadioChannelGroup(dlg.GroupName, 9999);
        group.Persist();

        this.RefreshContextMenu();
        this.RefreshTabs();
      }
      else
      {
        group = (RadioChannelGroup)menuItem.Tag;
      }

      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      TvBusinessLayer layer = new TvBusinessLayer();
      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[indexes[i]];

        Channel channel = (Channel)item.Tag;
        layer.AddChannelToRadioGroup(channel, group);

        string groupString = item.SubItems[1].Text;
        if (groupString == string.Empty)
        {
          groupString = group.GroupName;
        }
        else
        {
          groupString += ", " + group.GroupName;
        }

        item.SubItems[1].Text = groupString;
      }

      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      string holder = String.Format("Are you sure you want to clear all radio channels?");

      if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
      {
        return;
      }

      NotifyForm dlg = new NotifyForm("Clearing all radio channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      IList<Channel> channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        if (channel.IsRadio)
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

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();

      if (mpListView1.SelectedItems.Count > 0)
      {
        string holder = String.Format("Are you sure you want to delete these {0:d} radio channels?", mpListView1.SelectedItems.Count);

        if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          mpListView1.EndUpdate();
          return;
        }
      }
      NotifyForm dlg = new NotifyForm("Deleting selected radio channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList<RadioGroupMap> mapsRadio = channel.ReferringRadioGroupMap();
        // Bav: fixing Mantis bug 1178: Can't delete Radio channels in SetupTV
        foreach (RadioGroupMap map in mapsRadio)
        {
          map.Remove();
        }
        IList<GroupMap> maps = channel.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          map.Remove();
        }
        // Bav - End of fix
        channel.Delete();
        mpListView1.Items.Remove(item);
      }
      dlg.Close();
      mpListView1.EndUpdate();
      ReOrder();
    }

    private void mpButtonUp_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index - 1, item);
        }
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    private void mpButtonDown_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      if (mpListView1.Items.Count < 2)
        return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mpListView1.Items[index];
        mpListView1.Items.RemoveAt(index);
        if (index + 1 < mpListView1.Items.Count)
          mpListView1.Items.Insert(index + 1, item);
        else
          mpListView1.Items.Add(item);
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    void ReOrder()
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

    private void ReOrderGroups()
    {
      for (int i = 1; i < tabControl1.TabPages.Count; i++)
      {
        RadioChannelGroup group = (RadioChannelGroup)tabControl1.TabPages[i].Tag;

        group.SortOrder = i - 1;
        group.Persist();
      }
    }

    private void mpListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      if (e.Label != null)
      {
        Channel channel = (Channel)mpListView1.Items[e.Item].Tag;
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
      dlg.IsTv = false;
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
        OnSectionActivated();
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

    private void mpListView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      if (e.Item is ListViewItem)
      {
        ReOrder();
      }
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
      dlg.IsTv = false;
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

    private void btnPlaylist_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.AddExtension = false;
      dlg.CheckFileExists = true;
      dlg.CheckPathExists = true;
      dlg.Filter = "playlists (*.m3u;*.pls;*.b4s;*.wpl)|*.m3u;*.pls;*.b4s;*.wpl";
      dlg.Multiselect = false;
      dlg.Title = "Select the playlist file to import";
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      IPlayListIO listIO = PlayListFactory.CreateIO(dlg.FileName);
      PlayList playlist = new PlayList();
      if (!listIO.Load(playlist, dlg.FileName))
      {
        MessageBox.Show("There was an error parsing the playlist file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      TvBusinessLayer layer = new TvBusinessLayer();
      int iInserted = 0;
      foreach (PlayListItem item in playlist)
      {
        if (string.IsNullOrEmpty(item.FileName))
          continue;
        if (string.IsNullOrEmpty(item.Description))
          item.Description = item.FileName;
        Channel channel = new Channel(item.Description, true, false, 0, Schedule.MinSchedule, false, Schedule.MinSchedule, 10000, true, "", true, item.Description);
        channel.Persist();
        layer.AddWebStreamTuningDetails(channel, item.FileName, 0);
        iInserted++;
      }
      MessageBox.Show("Imported " + iInserted + " new channels from playlist");
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

    private void renameSelectedChannelsBySIDToolStripMenuItem_Click(object sender, EventArgs e)
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
      NotifyForm dlg = new NotifyForm("Renumbering radio channels...", "This can take some time\n\nPlease be patient...");
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

    private void mpButtonTestScrambled_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Testing all radio channels...", "Please be patient...");
      dlg.Show();
      dlg.WaitForDisplay();

      // Create tunning objects Server, User and Card
      TvServer _server = new TvServer();
      User _user = new User();
      VirtualCard _card;

      foreach (ListViewItem item in mpListView1.Items)
      {
        if (item.Checked == false) continue;  // do not test "un-checked" channels
        Channel _channel = (Channel)item.Tag; // get channel
        dlg.SetMessage(
          string.Format("Please be patient...\n\nTesting channel {0} ( {1} of {2} )",
                        _channel.DisplayName, item.Index + 1, mpListView1.Items.Count));
        TvResult result = _server.StartTimeShifting(ref _user, _channel.IdChannel, out _card);
        // test the channel for scrambled or unable to start graph
        if ((result == TvResult.ChannelIsScrambled))
        {
          item.Checked = false;
        } else if (result == TvResult.Succeeded)
        {
          _card.StopTimeShifting();
        }

      }
      dlg.Close();
    }

	  private void mpButtonAddGroup_Click(object sender, EventArgs e)
	  {
      GroupNameForm dlg = new GroupNameForm();
      if (dlg.ShowDialog(this) != DialogResult.OK)
      {
        return;
      }

      RadioChannelGroup group = new RadioChannelGroup(dlg.GroupName, 9999);
      group.Persist();

      this.RefreshContextMenu();
      this.RefreshTabs();
	  }

    private void mpButtonRenameGroup_Click(object sender, EventArgs e)
    {
      GroupSelectionForm dlgGrpSel = new GroupSelectionForm();

      if (dlgGrpSel.ShowDialog(typeof(RadioChannelGroup), this) != DialogResult.OK)
      {
        return;
      }

      RadioChannelGroup group = dlgGrpSel.Group as RadioChannelGroup;
      if (group == null)
      {
        return;
      }

      GroupNameForm dlgGrpName = new GroupNameForm(group.GroupName);
      if (dlgGrpName.ShowDialog(this) != DialogResult.OK)
      {
        return;
      }

      group.GroupName = dlgGrpName.GroupName;
      group.Persist();

      if (group.ReferringRadioGroupMap().Count > 0)
      {
        this.RefreshAll();
      }
      else
      {
        this.RefreshContextMenu();
        this.RefreshTabs();
      }
    }

    private void mpButtonDelGroup_Click(object sender, EventArgs e)
    {
      GroupSelectionForm dlgGrpSel = new GroupSelectionForm();

      if (dlgGrpSel.ShowDialog(typeof(RadioChannelGroup), this) != DialogResult.OK)
      {
        return;
      }

      RadioChannelGroup group = dlgGrpSel.Group as RadioChannelGroup;
      if (group == null)
      {
        return;
      }

      DialogResult result = MessageBox.Show(string.Format("Are you sure you want to delete the group '{0}'?",
        group.GroupName), "", MessageBoxButtons.YesNo);

      if (result == DialogResult.No)
      {
        return;
      }

      bool isGroupEmpty = (group.ReferringRadioGroupMap().Count <= 0);

      group.Delete();

      if (!isGroupEmpty)
      {
        this.RefreshAll();
      }
      else
      {
        this.RefreshContextMenu();
        this.RefreshTabs();
      }
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (suppressRefresh)
      {
        return;
      }

      if (tabControl1.SelectedIndex == 0)
      {
        OnSectionActivated();
      }
      else
      {
        if (tabControl1.TabCount > 0)
        {
          TabPage page = tabControl1.TabPages[tabControl1.SelectedIndex];
          foreach (Control control in page.Controls)
          {
            ChannelsInRadioGroupControl groupCnt = control as ChannelsInRadioGroupControl;
            if (groupCnt != null)
            {
              groupCnt.Group = (RadioChannelGroup)page.Tag;
              groupCnt.OnActivated();
            }
          }
        }
      }
    }

    private void tabControl1_DragOver(object sender, DragEventArgs e)
    {
      //means a channel group assignment is going to be performed
      if (e.Data.GetData(typeof(MPListView)) != null)
      {
        for (int i = 0; i < tabControl1.TabPages.Count; i++)
        {
          if (i == tabControl1.SelectedIndex)
          {
            continue;
          }

          if (tabControl1.GetTabRect(i).Contains(this.PointToClient(new System.Drawing.Point(e.X, e.Y))))
          {
            tabControl1.SelectedIndex = i;
            break;
          }
        }
      }
    }

    private void tabControl1_DragDrop(object sender, DragEventArgs e)
    {
      TabPage droppedTabPage = e.Data.GetData(typeof(TabPage)) as TabPage;
      if (droppedTabPage == null)
      {
        return;
      }

      int targetIndex = -1;
      

      System.Drawing.Point pt = new System.Drawing.Point(e.X, e.Y);

      pt = PointToClient(pt);

      for (int i = 0; i < tabControl1.TabPages.Count; i++)
      {
        if (tabControl1.GetTabRect(i).Contains(pt))
        {
          targetIndex = i;
          break;
        }
      }

      if (targetIndex < 0)
      {
        return;
      }

      suppressRefresh = true;
      
      int sourceIndex = tabControl1.TabPages.IndexOf(droppedTabPage);

      //it looks a bit ugly when the first tab gets the focus, due to the other design
      if (sourceIndex == tabControl1.TabPages.Count - 1)
      {
        tabControl1.SelectedIndex = sourceIndex - 1;
      }
      else
      {
        tabControl1.DeselectTab(sourceIndex);
      }
 
      tabControl1.TabPages.RemoveAt(sourceIndex);

      tabControl1.TabPages.Insert(targetIndex, droppedTabPage);
      tabControl1.SelectedIndex = targetIndex;

      suppressRefresh = false;

      this.ReOrderGroups();
    }

    private void tabControl1_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
      {
        return;
      }

      int targetIndex = -1;
      System.Drawing.Point pt = new System.Drawing.Point(e.X, e.Y);

      for (int i = 0; i < tabControl1.TabPages.Count; i++)
      {
        if (tabControl1.GetTabRect(i).Contains(pt))
        {
          targetIndex = i;
          break;
        }
      }

      //first tab isn't a group tab
      if (targetIndex < 1)
      {
        return;
      }

      RadioChannelGroup group = tabControl1.TabPages[targetIndex].Tag as RadioChannelGroup;
      if (group == null)
      {
        return;
      }

      bool isAllChannelsGroup = (group.GroupName == TvConstants.TvGroupNames.AllChannels);

      renameGroupToolStripMenuItem.Tag = tabControl1.TabPages[targetIndex];
      deleteGroupToolStripMenuItem.Tag = renameGroupToolStripMenuItem.Tag;

      renameGroupToolStripMenuItem.Enabled = !isAllChannelsGroup;
      deleteGroupToolStripMenuItem.Enabled = !isAllChannelsGroup;

      pt = tabControl1.PointToScreen(pt);

      groupTabContextMenuStrip.Show(pt);
    }

    private void renameGroupToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripDropDownItem menuItem = sender as ToolStripDropDownItem;
      if (menuItem == null)
      {
        return;
      }

      TabPage tab = menuItem.Tag as TabPage;
      if (tab == null)
      {
        return;
      }

      RadioChannelGroup group = tab.Tag as RadioChannelGroup;
      if (group == null)
      {
        return;
      }

      GroupNameForm dlg = new GroupNameForm(group.GroupName);

      dlg.ShowDialog(this);

      if (dlg.GroupName.Length == 0)
      {
        return;
      }

      group.GroupName = dlg.GroupName;
      group.Persist();

      tab.Text = dlg.GroupName;

      if (group.ReferringRadioGroupMap().Count > 0 && tabControl1.SelectedIndex == 0)
      {
        this.RefreshContextMenu();
        this.RefreshAllChannels();
      }
      else
      {
        this.RefreshContextMenu();
      }
    }

    private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripDropDownItem menuItem = sender as ToolStripDropDownItem;
      if (menuItem == null)
      {
        return;
      }

      TabPage tab = menuItem.Tag as TabPage;
      if (tab == null)
      {
        return;
      }

      RadioChannelGroup group = tab.Tag as RadioChannelGroup;
      if (group == null)
      {
        return;
      }

      DialogResult result = MessageBox.Show(string.Format("Are you sure you want to delete the group '{0}'?",
        group.GroupName), "", MessageBoxButtons.YesNo);

      if (result == DialogResult.No)
      {
        return;
      }

      bool groupIsEmpty = (group.ReferringRadioGroupMap().Count <= 0);

      group.Delete();
      tabControl1.TabPages.Remove(tab);

      if (!groupIsEmpty && tabControl1.SelectedIndex == 0)
      {
        this.RefreshContextMenu();
        this.RefreshAllChannels();
      }
      else
      {
        this.RefreshContextMenu();
      }
    }
  }
}
