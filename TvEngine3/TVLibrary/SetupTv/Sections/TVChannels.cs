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
using System.Text;
using System.Windows.Forms;
using SetupTv.Dialogs;
using TvControl;
using Gentle.Framework;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using MediaPortal.UserInterface.Controls;
using System.Threading;
using SetupTv.Sections.Helpers;

namespace SetupTv.Sections
{
  public partial class TvChannels : SectionSettings
  {
    public class CardInfo
    {
      protected Card _card;

      public Card Card
      {
        get { return _card; }
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
    private ChannelListViewHandler _lvChannelHandler;

    private bool _suppressRefresh = false;
    private bool _isScanning = false;
    private bool _abortScanning = false;
    private Thread _scanThread;

    private Dictionary<int, CardType> _cards = null;
    private IList<Channel> _allChannels = null;

    public TvChannels()
      : this("TV Channels")
    {
      mpListView1.IsChannelListView = true;
      tabControl1.AllowReorderTabs = true;
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
      // bugfix for tab removal, RemoveAt fails sometimes
      tabControl1.TabPages.Clear();
      tabControl1.TabPages.Add(tabPage1);

      IList<ChannelGroup> groups = ChannelGroup.ListAll();

      foreach (ChannelGroup group in groups)
      {
        TabPage page = new TabPage(group.GroupName);
        page.SuspendLayout();

        ChannelsInGroupControl channelsInRadioGroupControl = new ChannelsInGroupControl();
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

    /// <summary>
    /// Get all channels from the database
    /// </summary>
    private void RefreshAllChannels()
    {
      Cursor.Current = Cursors.WaitCursor;
      IList<Card> dbsCards = Card.ListAll();
      _cards = new Dictionary<int, CardType>();
      foreach (Card card in dbsCards)
      {
        _cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
      }

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      sb.AddConstraint(Operator.Equals, "isTv", true);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      _allChannels = ObjectFactory.GetCollection<Channel>(stmt.Execute());
      tabControl1.TabPages[0].Text = string.Format("Channels ({0})", _allChannels.Count);

      _lvChannelHandler = new ChannelListViewHandler(mpListView1, _allChannels, _cards, txtFilterString, ChannelType.Tv);
      _lvChannelHandler.FilterListView("");
    }

    /// <summary>
    /// Text of the filter has changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void txtFilterString_TextChanged(object sender, EventArgs e)
    {
      //Filter the listview so only items that contain the text of txtFilterString are shown
      _lvChannelHandler.FilterListView(txtFilterString.Text);
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

        this.RefreshContextMenu();
        this.RefreshTabs();
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
      string holder = String.Format("Are you sure you want to clear all channels?");

      if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
      {
        return;
      }

      NotifyForm dlg = new NotifyForm("Clearing all tv channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
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

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        IList<Schedule> schedules = Schedule.ListAll();
        TvServer server = new TvServer();

        //Since it takes a very long time to add channels, make sure the user really wants to delete them
        if (mpListView1.SelectedItems.Count > 0)
        {
          string holder = String.Format("Are you sure you want to delete these {0:d} channels?",
                                        mpListView1.SelectedItems.Count);

          if (MessageBox.Show(holder, "", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            //mpListView1.EndUpdate();
            return;
          }
        }
        NotifyForm dlg = new NotifyForm("Deleting selected tv channels...",
                                        "This can take some time\n\nPlease be patient...");
        dlg.Show(this);
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
        ReOrder();
        mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      finally
      {
        mpListView1.EndUpdate();
      }
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

    private void ReOrderGroups()
    {
      for (int i = 1; i < tabControl1.TabPages.Count; i++)
      {
        ChannelGroup group = (ChannelGroup)tabControl1.TabPages[i].Tag;

        group.SortOrder = i - 1;
        group.Persist();
      }
    }

    private void mpListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      if (e.Label != null)
      {
        Channel channel = (Channel)mpListView1.Items[e.Item].Tag;
        channel.DisplayName = e.Label;
        channel.Persist();
      }
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      Channel ch = (Channel)e.Item.Tag;
      if (ch.VisibleInGuide != e.Item.Checked && !_lvChannelHandler.PopulateRunning)
      {
        ch.VisibleInGuide = e.Item.Checked;
        ch.Persist();
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
        IList<Card> dbsCards = Card.ListAll();
        Dictionary<int, CardType> cards = new Dictionary<int, CardType>();
        foreach (Card card in dbsCards)
        {
          cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
        }
        mpListView1.BeginUpdate();
        try
        {
          mpListView1.Items[indexes[0]] = _lvChannelHandler.CreateListViewItemForChannel(channel, cards);
          mpListView1.Sort();
          ReOrder();
        }
        finally
        {
          mpListView1.EndUpdate();
        }
      }
    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending
                                  ? SortOrder.Descending
                                  : SortOrder.Ascending;
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
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        IList<Card> dbsCards = Card.ListAll();
        Dictionary<int, CardType> cards = new Dictionary<int, CardType>();
        foreach (Card card in dbsCards)
        {
          cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
        }
        mpListView1.BeginUpdate();
        try
        {
          mpListView1.Items.Add(_lvChannelHandler.CreateListViewItemForChannel(dlg.Channel, cards));
          mpListView1.Sort();
          ReOrder();
        }
        finally
        {
          mpListView1.EndUpdate();
        }
      }
    }

    private void mpButtonUncheckEncrypted_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Unchecking all scrambled tv channels...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
      dlg.WaitForDisplay();
      foreach (ListViewItem item in mpListView1.Items)
      {
        Channel channel = (Channel)item.Tag;
        bool hasFTA = false;
        foreach (TuningDetail tuningDetail in channel.ReferringTuningDetail())
        {
          if (tuningDetail.FreeToAir)
          {
            hasFTA = true;
            break;
          }
        }
        if (!hasFTA)
        {
          item.Checked = false;
        }
      }
      dlg.Close();
    }

    private void mpButtonDeleteEncrypted_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Deleting all scrambled tv channels...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
      dlg.WaitForDisplay();
      List<ListViewItem> itemsToRemove = new List<ListViewItem>();
      foreach (ListViewItem item in mpListView1.Items)
      {
        Channel channel = (Channel)item.Tag;
        bool hasFTA = false;
        foreach (TuningDetail tuningDetail in channel.ReferringTuningDetail())
        {
          if (tuningDetail.FreeToAir)
          {
            hasFTA = true;
            break;
          }
        }
        if (!hasFTA)
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
      NotifyForm dlg = new NotifyForm("Renaming selected tv channels by SID ...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
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
      NotifyForm dlg = new NotifyForm("Adding SID in front of name...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
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
      dlg.Show(this);
      dlg.WaitForDisplay();

      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList<TuningDetail> details = channel.ReferringTuningDetail();
        foreach (TuningDetail detail in details)
        {
          if (detail.ChannelType != 0)  // SID is not relevant for analog channels
          {
            channel.ChannelNumber = detail.ServiceId;
            channel.Persist();
            item.Tag = channel;
            break;
          }
        }
      }
      dlg.Close();
    }

    private void StartScanThread()
    {
      _scanThread = new Thread(ScanForUsableChannels);
      _scanThread.Name = "Channels test thread";
      _scanThread.Start();
      mpButtonTestAvailable.Text = "Stop";
    }

    private void StopScanThread()
    {
      _abortScanning = true;
    }

    private void mpButtonTestAvailable_Click(object sender, EventArgs e)
    {
      if (_isScanning)
      {
        StopScanThread();
      }
      else if (!_abortScanning) // cancel in progress
      {
        StartScanThread();
      }
    }

    private void ScanForUsableChannels()
    {
      _abortScanning = false;
      _isScanning = true;
      NotifyForm dlg = new NotifyForm("Testing all checked tv channels...", "Please be patient...");
      dlg.Show(this);
      dlg.WaitForDisplay();

      // Create tunning objects Server, User and Card
      TvServer _server = new TvServer();
      IUser _user = new User();
      VirtualCard _card;

      foreach (ListViewItem item in mpListView1.Items)
      {
        if (item.Checked == false)
        {
          continue; // do not test "un-checked" channels
        }
        Channel _channel = (Channel)item.Tag; // get channel
        dlg.SetMessage(
          string.Format("Please be patient...\n\nTesting channel {0} ( {1} of {2} )",
                        _channel.DisplayName, item.Index + 1, mpListView1.Items.Count));
        Application.DoEvents();
        TvResult result = _server.StartTimeShifting(ref _user, _channel.IdChannel, out _card);
        if (result == TvResult.Succeeded)
        {
          _card.StopTimeShifting();
        }
        else
        {
          item.Checked = false;
          _channel.VisibleInGuide = false;
          _channel.Persist();
        }
        if (_abortScanning)
        {
          break;
        }
      }
      mpButtonTestAvailable.Text = "Test";
      dlg.Close();
      _isScanning = false;
      _abortScanning = false;
    }

    private void mpButtonUp_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
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
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void mpButtonDown_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
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
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void mpButtonAddGroup_Click(object sender, EventArgs e)
    {
      GroupNameForm dlg = new GroupNameForm();
      if (dlg.ShowDialog(this) != DialogResult.OK)
      {
        return;
      }

      ChannelGroup group = new ChannelGroup(dlg.GroupName, 9999);
      group.Persist();

      this.RefreshContextMenu();
      this.RefreshTabs();
    }

    private void mpButtonRenameGroup_Click(object sender, EventArgs e)
    {
      GroupSelectionForm dlgGrpSel = new GroupSelectionForm();
      dlgGrpSel.Selection = GroupSelectionForm.SelectionType.ForRenaming;

      if (dlgGrpSel.ShowDialog(typeof (ChannelGroup), this) != DialogResult.OK)
      {
        return;
      }

      ChannelGroup group = dlgGrpSel.Group as ChannelGroup;
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

      if (group.ReferringGroupMap().Count > 0)
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

      if (dlgGrpSel.ShowDialog(typeof (ChannelGroup), this) != DialogResult.OK)
      {
        return;
      }

      ChannelGroup group = dlgGrpSel.Group as ChannelGroup;
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

      bool isGroupEmpty = (group.ReferringGroupMap().Count <= 0);

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
      if (_suppressRefresh)
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
            ChannelsInGroupControl groupCnt = control as ChannelsInGroupControl;
            if (groupCnt != null)
            {
              groupCnt.Group = (ChannelGroup)page.Tag;
              groupCnt.OnActivated();
            }
          }
        }
      }
    }

    private void tabControl1_DragOver(object sender, DragEventArgs e)
    {
      //means a channel group assignment is going to be performed
      if (e.Data.GetData(typeof (MPListView)) != null)
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
      TabPage droppedTabPage = e.Data.GetData(typeof (TabPage)) as TabPage;
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

      _suppressRefresh = true;

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

      _suppressRefresh = false;

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

      ChannelGroup group = tabControl1.TabPages[targetIndex].Tag as ChannelGroup;
      if (group == null)
      {
        return;
      }

      bool isFixedGroupName = (
                                group.GroupName == TvConstants.TvGroupNames.AllChannels ||
                                group.GroupName == TvConstants.TvGroupNames.Analog ||
                                group.GroupName == TvConstants.TvGroupNames.DVBC ||
                                group.GroupName == TvConstants.TvGroupNames.DVBS ||
                                group.GroupName == TvConstants.TvGroupNames.DVBT
                              );

      bool isGlobalChannelsGroup = (
                                     group.GroupName == TvConstants.TvGroupNames.AllChannels
                                   );

      renameGroupToolStripMenuItem.Tag = tabControl1.TabPages[targetIndex];
      deleteGroupToolStripMenuItem.Tag = renameGroupToolStripMenuItem.Tag;

      renameGroupToolStripMenuItem.Enabled = !isFixedGroupName;
      deleteGroupToolStripMenuItem.Enabled = !isGlobalChannelsGroup;

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

      ChannelGroup group = tab.Tag as ChannelGroup;
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

      if (group.ReferringGroupMap().Count > 0 && tabControl1.SelectedIndex == 0)
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

      ChannelGroup group = tab.Tag as ChannelGroup;
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

      bool groupIsEmpty = (group.ReferringGroupMap().Count <= 0);

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