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
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Channel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using TuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Channels : SectionSettings
  {
    private const ChannelRelation REQUIRED_CHANNEL_RELATIONS = ChannelRelation.TuningDetails | ChannelRelation.ChannelGroupMappings;

    #region variables

    private MediaType _mediaType = MediaType.Television;

    #region channels

    // Channel name and number inline editing.
    private int _channelInlineEditSubItemIndex = -1;
    private ListViewItem.ListViewSubItem _channelInlineEditSubItem = null;
    private Point _channelInlineEditTriggerMouseLocation = Point.Empty;

    private IDictionary<int, FormEditChannel> _channelEditDialogs = new Dictionary<int, FormEditChannel>(5);
    private int _newChannelFakeId = -1;

    private IList<Channel> _channelVisibleInGuideChanges = new List<Channel>();
    private NotifyForm _channelVisibleInGuideNotifyForm = null;

    private bool _channelTestRunning = false;
    private bool _channelTestAbort = false;
    private Thread _channelTestThread;

    private readonly MPListViewStringColumnSorter _listViewChannelsColumnSorter = null;
    private ChannelListViewHandler _listViewChannelsHandler = null;

    #endregion

    #region channel groups

    private ChannelGroup _currentChannelGroup = null;
    private readonly MPListViewStringColumnSorter _listViewChannelsInGroupColumnSorter = null;
    private SortOrder _channelsInGroupLastSortOrder = SortOrder.None;

    // channel ID => item
    private IDictionary<int, ListViewItem> _listViewChannelsInGroupItemCache = null;
    // group ID => [ordered mappings]
    private IDictionary<int, List<ChannelGroupChannelMapping>> _channelsInGroupMappingCache = null;

    #endregion

    #endregion

    public Channels(string name, MediaType mediaType)
      : base(name)
    {
      _mediaType = mediaType;

      InitializeComponent();

      _listViewChannelsColumnSorter = new MPListViewStringColumnSorter();
      _listViewChannelsColumnSorter.Order = SortOrder.Ascending;
      listViewChannels.ListViewItemSorter = _listViewChannelsColumnSorter;

      _listViewChannelsInGroupColumnSorter = new MPListViewStringColumnSorter();
      _listViewChannelsInGroupColumnSorter.Order = SortOrder.None;
      listViewChannelsInGroup.ListViewItemSorter = _listViewChannelsInGroupColumnSorter;
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("channels: activating, type = {0}", _mediaType);

      IList<Channel> allChannels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByMediaType(_mediaType, REQUIRED_CHANNEL_RELATIONS);
      IList<ChannelGroup> allGroups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(_mediaType, ChannelGroupRelation.None);

      this.LogDebug("channels: channel count = {0}, group count = {1}", allChannels.Count, allGroups.Count);

      // channel group channel items and mappings
      _listViewChannelsInGroupItemCache = new Dictionary<int, ListViewItem>(allChannels.Count);
      _channelsInGroupMappingCache = new Dictionary<int, List<ChannelGroupChannelMapping>>(allGroups.Count);
      foreach (Channel channel in allChannels)
      {
        _listViewChannelsInGroupItemCache.Add(channel.IdChannel, CreateChannelsInGroupItemForChannel(channel));

        foreach (ChannelGroupChannelMapping mapping in channel.ChannelGroupMappings)
        {
          List<ChannelGroupChannelMapping> mappings;
          if (!_channelsInGroupMappingCache.TryGetValue(mapping.IdChannelGroup, out mappings))
          {
            mappings = new List<ChannelGroupChannelMapping>(allChannels.Count);
            _channelsInGroupMappingCache[mapping.IdChannelGroup] = mappings;
          }
          mappings.Add(mapping);
        }
      }

      foreach (List<ChannelGroupChannelMapping> mappings in _channelsInGroupMappingCache.Values)
      {
        mappings.Sort(delegate(ChannelGroupChannelMapping m1, ChannelGroupChannelMapping m2)
        {
          return m1.SortOrder.CompareTo(m2.SortOrder);
        });
      }

      // channel group combo box
      _currentChannelGroup = null;
      ChannelGroup[] groups = new ChannelGroup[allGroups.Count];
      allGroups.CopyTo(groups, 0);
      comboBoxChannelGroup.BeginUpdate();
      try
      {
        comboBoxChannelGroup.Items.Clear();
        comboBoxChannelGroup.Items.AddRange(groups);
        comboBoxChannelGroup.DisplayMember = "GroupName";
        comboBoxChannelGroup.SelectedIndex = 0;
      }
      finally
      {
        comboBoxChannelGroup.EndUpdate();
      }
      buttonGroupDelete.Enabled = allGroups.Count > 1;
      buttonGroupOrder.Enabled = buttonGroupDelete.Enabled;

      // channels tab
      _listViewChannelsHandler = new ChannelListViewHandler(listViewChannels, OnFilteringCompleted);
      foreach (ChannelGroup group in allGroups)
      {
        _listViewChannelsHandler.AddGroup(group);
      }
      _listViewChannelsHandler.FilterListView(textBoxFilter.Text);
      _listViewChannelsHandler.AddOrUpdateChannels(allChannels);
      listViewChannels_SelectedIndexChanged(null, null);
      toolTip.SetToolTip(textBoxFilter,
        "Properties: visible, name, number, group, provider, type, encryption" + Environment.NewLine +
        "Operators: ! (conditional NOT), & (conditional AND), | (conditional OR), : (contains), < (less than), <= (less than or equal), == (equal), != (not equal), >= (greater than or equal), > (greater than)" + Environment.NewLine +
        "Example: provider == \"Freesat\" & !name : \"BBC\" (find all Freesat channels that don't have \"BBC\" in their name)"
      );

      // debug
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          try
          {
            this.LogDebug("channels: channels...");
            foreach (Channel channel in allChannels)
            {
              this.LogDebug("  ID = {0, -4}, name = {1, -30}, number = {2, -5}, tuning detail count = {3}, group count = {4}", channel.IdChannel, channel.Name, channel.ChannelNumber, channel.TuningDetails.Count, channel.ChannelGroupMappings.Count);
            }
            this.LogDebug("channels: groups...");
            foreach (ChannelGroup group in allGroups)
            {
              List<ChannelGroupChannelMapping> mappings = _channelsInGroupMappingCache[group.IdChannelGroup];
              this.LogDebug("  ID = {0, -3}, name = {1, -30}, channel count = {2, -4}, channels = [{3}]", group.IdChannelGroup, group.Name, mappings.Count, string.Join(", ", from mapping in mappings select mapping.IdChannel));
            }
          }
          catch
          {
          }
        }
      );

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("channels: deactivating, type = {0}", _mediaType);

      CancelChannelEditing();
      SaveChannelsInGroupOrder();

      base.OnSectionDeActivated();
    }

    private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
      CancelChannelEditing();
    }

    private void CancelChannelEditing()
    {
      // Careful! Looping over _channelEditDialogs would result in an invalid
      // operation exception (collection modified) in
      // OnAddOrEditChannelFormClosed().
      List<FormEditChannel> openDialogs = new List<FormEditChannel>(_channelEditDialogs.Values);
      foreach (FormEditChannel dlg in openDialogs)
      {
        dlg.DialogResult = DialogResult.Cancel;
        dlg.Close();
        dlg.Dispose();
      }
    }

    private Channel SaveChannel(Channel channel)
    {
      channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel);
      // We need to re-query in order to get the channel relations which are
      // removed during the save process.
      return ServiceAgents.Instance.ChannelServiceAgent.GetChannel(channel.IdChannel, REQUIRED_CHANNEL_RELATIONS);
    }

    #region channels

    #region list view

    private void listViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewChannels.SelectedItems;
      buttonChannelEdit.Enabled = items.Count > 0 && !_channelTestRunning;
      buttonChannelDelete.Enabled = buttonChannelEdit.Enabled && _channelEditDialogs.Count == 0;

      buttonChannelAddToGroup.Enabled = buttonChannelDelete.Enabled;

      buttonChannelMerge.Enabled = items.Count > 1 && !_channelTestRunning && _channelEditDialogs.Count == 0;
      bool isSplitEnabled = buttonChannelDelete.Enabled;
      if (isSplitEnabled)
      {
        isSplitEnabled = false;
        foreach (ListViewItem item in listViewChannels.SelectedItems)
        {
          Channel channel = _listViewChannelsHandler.GetChannelForItem(item);
          if (channel.TuningDetails != null && channel.TuningDetails.Count > 1)
          {
            isSplitEnabled = true;
            break;
          }
        }
      }
      buttonChannelSplit.Enabled = isSplitEnabled;

      buttonChannelPreview.Enabled = items.Count == 1;
      buttonChannelTest.Enabled = _channelEditDialogs.Count == 0;
    }

    private void listViewChannels_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      buttonChannelEdit_Click(null, null);
    }

    private void listViewChannels_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        buttonChannelDelete_Click(null, null);
        e.Handled = true;
      }
    }

    private void listViewChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == _listViewChannelsColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        _listViewChannelsColumnSorter.Order = _listViewChannelsColumnSorter.Order == SortOrder.Ascending
                                              ? SortOrder.Descending
                                              : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        _listViewChannelsColumnSorter.SortColumn = e.Column;
        _listViewChannelsColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      listViewChannels.Sort();
    }

    private void listViewChannels_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      // Prevent channel editing via checkbox at the same time
      if (_channelEditDialogs.Count > 0)
      {
        e.NewValue = e.CurrentValue;
      }
    }

    private void listViewChannels_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (_listViewChannelsHandler.IsFilling)
      {
        return;
      }

      if (_channelVisibleInGuideNotifyForm == null && listViewChannels.SelectedItems.Count > 10)
      {
        _channelVisibleInGuideNotifyForm = new NotifyForm("Toggling visible-in-guide state...",
                                        "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
        _channelVisibleInGuideNotifyForm.Show(this);
        _channelVisibleInGuideNotifyForm.WaitForDisplay();
      }

      Channel channel = _listViewChannelsHandler.GetChannelForItem(e.Item);
      if (channel != null)
      {
        this.LogInfo("channels: channel {0} visible in guide changed from {1} to {2}", channel.IdChannel, channel.VisibleInGuide, e.Item.Checked);
        channel.VisibleInGuide = e.Item.Checked;
        channel = SaveChannel(channel);
        bool selected = e.Item.Selected;
        if (!selected)
        {
          // User only un/checked one channel.
          UpdateAllViewsOnAddOrEditChannels(new List<Channel> { channel });

          // Undo the selection caused by UpdateAllViewsForChannels().
          ListViewItem item = _listViewChannelsHandler.GetItemForChannel(channel);
          if (item != null)
          {
            item.Selected = false;
          }
        }
        else
        {
          // User un/checked multiple channels.
          _channelVisibleInGuideChanges.Add(channel);

          // Check if this is the last ItemChecked event that we're expecting.
          // If it is, update the list views. This saves refiltering for each
          // item that is un/checked, which is slow.
          bool updateListViews = true;
          foreach (ListViewItem item in listViewChannels.SelectedItems)
          {
            if (item.Checked != e.Item.Checked && item != e.Item)
            {
              updateListViews = false;
              break;
            }
          }
          if (updateListViews)
          {
            UpdateAllViewsOnAddOrEditChannels(_channelVisibleInGuideChanges);
            _channelVisibleInGuideChanges.Clear();
            if (_channelVisibleInGuideNotifyForm != null)
            {
              _channelVisibleInGuideNotifyForm.Close();
              _channelVisibleInGuideNotifyForm.Dispose();
              _channelVisibleInGuideNotifyForm = null;
            }
          }
        }
      }
    }

    #endregion

    #region filtering

    private void textBoxFilter_TextChanged(object sender, EventArgs e)
    {
      _listViewChannelsHandler.FilterListView(textBoxFilter.Text);
    }

    private void OnFilteringCompleted()
    {
      tabControl.Invoke(new MethodInvoker(delegate()
      {
        tabControl.TabPages[0].Text = string.Format("Channels ({0})", listViewChannels.Items.Count);
      }));
    }

    #endregion

    #region button actions

    private void buttonChannelAdd_Click(object sender, EventArgs e)
    {
      FormEditChannel dlg = new FormEditChannel(_newChannelFakeId, _mediaType);
      dlg.Tag = _newChannelFakeId;
      dlg.Text = "Add Channel";
      dlg.FormClosed += new FormClosedEventHandler(OnAddOrEditChannelFormClosed);
      _channelEditDialogs.Add(_newChannelFakeId--, dlg);
      dlg.Show();
      listViewChannels_SelectedIndexChanged(null, null);
    }

    private void buttonChannelEdit_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listViewChannels.SelectedItems)
      {
        Channel channel = _listViewChannelsHandler.GetChannelForItem(item);
        FormEditChannel dlg = null;
        if (channel == null || _channelEditDialogs.TryGetValue(channel.IdChannel, out dlg))
        {
          if (dlg != null)
          {
            dlg.BringToFront();
            dlg.Focus();
          }
          continue;
        }

        dlg = new FormEditChannel(channel.IdChannel, _mediaType);
        dlg.Tag = channel.IdChannel;
        dlg.Text = "Edit Channel";
        dlg.FormClosed += new FormClosedEventHandler(OnAddOrEditChannelFormClosed);
        _channelEditDialogs.Add(channel.IdChannel, dlg);
        dlg.Show();
        listViewChannels_SelectedIndexChanged(null, null);
      }
    }

    private void OnAddOrEditChannelFormClosed(object sender, FormClosedEventArgs e)
    {
      FormEditChannel dlg = sender as FormEditChannel;
      if (dlg == null)
      {
        return;
      }

      int channelId = (int)dlg.Tag;
      _channelEditDialogs.Remove(channelId);
      try
      {
        if (dlg.DialogResult != DialogResult.OK)
        {
          return;
        }

        Channel channel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(dlg.IdChannel, REQUIRED_CHANNEL_RELATIONS);
        UpdateAllViewsOnAddOrEditChannels(new List<Channel> { channel });
      }
      finally
      {
        dlg.Dispose();
        listViewChannels_SelectedIndexChanged(null, null);
      }
    }

    private void UpdateAllViewsOnAddOrEditChannels(IEnumerable<Channel> channels)
    {
      _listViewChannelsHandler.AddOrUpdateChannels(channels);
      tabControl.TabPages[0].Text = string.Format("Channels ({0})", listViewChannels.Items.Count);

      // Select the new/modified items.
      foreach (Channel channel in channels)
      {
        ListViewItem item = _listViewChannelsHandler.GetItemForChannel(channel);
        if (item != null && item.ListView != null)
        {
          item.EnsureVisible();
          item.Selected = true;
        }
      }
      listViewChannels.Focus();

      UpdateChannelsInGroupViewOnAddOrEditChannels(channels);
    }

    private void UpdateAllViewsOnDeleteChannels(IEnumerable<ListViewItem> items, out ICollection<Channel> channels)
    {
      _listViewChannelsHandler.DeleteChannels(items, out channels);
      tabControl.TabPages[0].Text = string.Format("Channels ({0})", listViewChannels.Items.Count);
      listViewChannels.Focus();
      UpdateChannelsInGroupViewOnDeleteChannels(channels);
    }

    private void buttonChannelDelete_Click(object sender, EventArgs e)
    {
      if (listViewChannels.SelectedItems.Count == 0 || _channelEditDialogs.Count > 0)
      {
        return;
      }

      // It is not easy to undo deleting channels so always confirm.
      if (MessageBox.Show(string.Format("Are you sure you want to delete the {0} selected channel(s)?", listViewChannels.SelectedItems.Count), MESSAGE_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
      {
        return;
      }
      NotifyForm dlg = null;
      try
      {
        if (listViewChannels.SelectedItems.Count > 10)
        {
          dlg = new NotifyForm("Deleting selected channels...", "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
          dlg.Show(this);
          dlg.WaitForDisplay();
        }

        ICollection<Channel> channels;
        UpdateAllViewsOnDeleteChannels(listViewChannels.SelectedItems.Cast<ListViewItem>(), out channels);

        // TODO IMO schedule deletion should go to the server side; better to have central logic
        IList<Schedule> schedules = ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules();
        foreach (Channel channel in channels)
        {
          if (schedules != null)
          {
            for (int i = schedules.Count - 1; i > -1; i--)
            {
              Schedule schedule = schedules[i];
              if (schedule.IdChannel == channel.IdChannel)
              {
                ServiceAgents.Instance.ControllerServiceAgent.StopRecordingSchedule(schedule.IdSchedule);
                ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(schedule.IdSchedule);
                schedules.RemoveAt(i);
              }
            }
          }

          this.LogInfo("channels: channel {0} deleted", channel.IdChannel);
          ServiceAgents.Instance.ChannelServiceAgent.DeleteChannel(channel.IdChannel);
        }
      }
      finally
      {
        if (dlg != null)
        {
          dlg.Close();
          dlg.Dispose();
        }
      }
    }

    private void buttonChannelAddToGroup_Click(object sender, EventArgs e)
    {
      int groupCount = comboBoxChannelGroup.Items.Count;
      ChannelGroup[] groups = new ChannelGroup[groupCount + 1];
      groups[0] = new ChannelGroup
      {
        IdChannelGroup = -1,
        Name = "[New]"
      };
      int i = 1;
      foreach (ChannelGroup g in comboBoxChannelGroup.Items)
      {
        groups[i++] = g;
      }

      ChannelGroup group = null;
      using (FormSelectItems dlg = new FormSelectItems("Add Channel(s) To Group", "Please select the group to add to:", groups, "GroupName", false))
      {
        if (dlg.ShowDialog() != DialogResult.OK || dlg.Items == null || dlg.Items.Count != 1)
        {
          return;
        }
        group = dlg.Items[0] as ChannelGroup;
      }
      if (group == null)
      {
        return;
      }

      IList<Channel> channels;
      if (group.IdChannelGroup == -1)
      {
        // new group
        group = CreateNewChannelGroup();
        if (group == null)
        {
          return;
        }
        channels = new List<Channel>(listViewChannels.SelectedItems.Count);
        foreach (ListViewItem item in listViewChannels.SelectedItems)
        {
          channels.Add(_listViewChannelsHandler.GetChannelForItem(item));
        }
      }
      else
      {
        channels = new List<Channel>(listViewChannels.SelectedItems.Count);
        foreach (ListViewItem item in listViewChannels.SelectedItems)
        {
          if (!_listViewChannelsHandler.IsChannelInGroup(item, group))
          {
            channels.Add(_listViewChannelsHandler.GetChannelForItem(item));
          }
        }
        if (channels.Count == 0)
        {
          MessageBox.Show(string.Format("The selected channel(s) are already in group {0}.", group.Name), MESSAGE_CAPTION);
          return;
        }
      }

      AddChannelsToGroup(group, channels);
    }

    private void buttonChannelMerge_Click(object sender, EventArgs e)
    {
      if (listViewChannels.SelectedItems == null || listViewChannels.SelectedItems.Count < 2)
      {
        return;
      }

      ICollection<Channel> channels;
      UpdateAllViewsOnDeleteChannels(listViewChannels.SelectedItems.Cast<ListViewItem>(), out channels);

      Channel mergedChannel = ServiceAgents.Instance.ChannelServiceAgent.MergeChannels(channels, REQUIRED_CHANNEL_RELATIONS);
      this.LogInfo("channels: channels {0} merged to channel {1}", string.Join(", ", channels.Select(c => c.IdChannel)), mergedChannel.IdChannel);

      UpdateAllViewsOnAddOrEditChannels(new List<Channel> { mergedChannel });
    }

    private void buttonChannelSplit_Click(object sender, EventArgs e)
    {
      if (listViewChannels.SelectedItems == null)
      {
        return;
      }

      IList<Channel> modifiedChannels = new List<Channel>();
      foreach (ListViewItem item in listViewChannels.SelectedItems)
      {
        Channel channel = _listViewChannelsHandler.GetChannelForItem(item);
        if (channel != null && channel.TuningDetails.Count > 1)
        {
          this.LogInfo("channels: splitting channel {0}...", channel.IdChannel);
          for (int i = channel.TuningDetails.Count - 1; i > 0; i--)
          {
            TuningDetail tuningDetail = channel.TuningDetails[i];
            channel.TuningDetails.RemoveAt(i);

            Channel newChannel = new Channel();
            newChannel.Name = tuningDetail.Name;
            newChannel.ChannelNumber = tuningDetail.LogicalChannelNumber;
            newChannel.MediaType = (int)_mediaType;
            newChannel.VisibleInGuide = true;
            newChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(newChannel);

            tuningDetail.IdChannel = newChannel.IdChannel;
            tuningDetail.Priority = 1;
            tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);

            newChannel.TuningDetails.Add(tuningDetail);
            newChannel.AcceptChanges();
            this.LogInfo("channels: channel {0} created from tuning detail {1}", newChannel.IdChannel, tuningDetail.IdTuningDetail);
            modifiedChannels.Add(newChannel);
          }
          channel.AcceptChanges();
          modifiedChannels.Add(channel);
        }
      }
      if (modifiedChannels.Count > 0)
      {
        UpdateAllViewsOnAddOrEditChannels(modifiedChannels);
      }
    }

    private void buttonChannelPreview_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewChannels.SelectedItems;
      if (items == null || items.Count < 1)
      {
        return;
      }
      Channel channel = _listViewChannelsHandler.GetChannelForItem(items[0]);
      if (channel == null)
      {
        return;
      }
      FormPreview previewWindow = new FormPreview();
      if (!previewWindow.SetChannel(channel))
      {
        return;
      }
      previewWindow.ShowDialog(this);
    }

    private void buttonChannelTest_Click(object sender, EventArgs e)
    {
      if (_channelTestRunning)
      {
        StopChannelTestThread();
      }
      else if (!_channelTestAbort)
      {
        StartChannelTestThread();
      }
    }

    private void StartChannelTestThread()
    {
      _channelTestThread = new Thread(TestChannels);
      _channelTestThread.Name = "channel testing";
      _channelTestThread.Start();
      buttonChannelTest.Text = "S&top";
    }

    private void StopChannelTestThread()
    {
      _channelTestAbort = true;
    }

    private void TestChannels()
    {
      _channelTestAbort = false;
      _channelTestRunning = true;
      NotifyForm dlg = null;

      try
      {
        dlg = new NotifyForm("Testing all checked channels...", "Please be patient...");
        dlg.Show();
        dlg.WaitForDisplay();
        Thread.Sleep(10000);

        // TODO
        /*IVirtualCard tuner;
        IUser user = new User();
        foreach (ListViewItem item in listViewChannels.Items)
        {
          if (item.Checked == false)
          {
            continue; // do not test "un-checked" channels
          }
          Channel channel = (Channel)item.Tag; // get channel
          dlg.SetMessage(
            string.Format("Please be patient...{0}{0}Testing channel {1} ({2} of {3})",
                          Environment.NewLine, channel.DisplayName, item.Index + 1, listViewChannels.Items.Count));
          Application.DoEvents();

          TvResult result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(user.Name, channel.IdChannel, out tuner, out user);
          if (result == TvResult.Succeeded)
          {
            tuner.StopTimeShifting();
          }
          else
          {
            item.Checked = false;
            channel.VisibleInGuide = false;
            channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel);
            channel.AcceptChanges();
          }
          if (_channelTestAbort)
          {
            break;
          }
        }*/
      }
      finally
      {
        buttonChannelTest.Invoke(new MethodInvoker(delegate()
        {
          buttonChannelTest.Text = "&Test";
        }));
        dlg.Close();
        dlg.Dispose();
        _channelTestRunning = false;
        _channelTestAbort = false;
      }
    }

    #endregion

    #region channel name and number inline editing and tab behaviour

    private void listViewChannels_MouseClick(object sender, MouseEventArgs e)
    {
      // Record the location of clicks for triggering editing of the correct sub item.
      if (e.Clicks == 1 && e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.None)
      {
        _channelInlineEditTriggerMouseLocation = e.Location;
      }
    }

    private void listViewChannels_BeforeLabelEdit(object sender, LabelEditEventArgs e)
    {
      if (_channelEditDialogs.Count > 0)
      {
        // Prevent inline editing when any add/edit channel dialog is open.
        e.CancelEdit = true;
        return;
      }

      ListViewItem item = listViewChannels.Items[e.Item];
      _channelInlineEditSubItem = item.GetSubItemAt(_channelInlineEditTriggerMouseLocation.X, _channelInlineEditTriggerMouseLocation.Y);
      _channelInlineEditSubItemIndex = item.SubItems.IndexOf(_channelInlineEditSubItem);
      if (_channelInlineEditSubItemIndex == ChannelListViewHandler.SUBITEM_INDEX_NAME)
      {
        // Clicking on the channel name (which is the item label) triggers
        // normal label editing.
        return;
      }

      // Prevent normal edit behaviour when clicking on any other sub item.
      e.CancelEdit = true;

      // Custom editing for the channel number...
      if (_channelInlineEditSubItemIndex != ChannelListViewHandler.SUBITEM_INDEX_NUMBER)
      {
        _channelInlineEditSubItemIndex = -1;
        return;
      }

      Rectangle subItemBounds = _channelInlineEditSubItem.Bounds;
      textBoxChannelNumber.SetBounds(
        subItemBounds.Left + listViewChannels.Left + 5,
        subItemBounds.Top + listViewChannels.Top + 1,
        subItemBounds.Width - 6,
        subItemBounds.Height
      );
      textBoxChannelNumber.Text = _channelInlineEditSubItem.Text;
      textBoxChannelNumber.Show();
      textBoxChannelNumber.Focus();

      // Force redrawing the item to prevent a bug: the text in the first
      // sub item is blanked after tabbing from one channel number to the next.
      item.BackColor = Color.Black;
      item.BackColor = Color.White;
    }

    private void listViewChannels_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      try
      {
        if (e.Label == null)
        {
          // No change.
          return;
        }
        ListViewItem item = listViewChannels.SelectedItems[0];
        if (item == null)
        {
          return;
        }
        Channel channel = _listViewChannelsHandler.GetChannelForItem(item);
        if (channel == null)
        {
          return;
        }

        // Always cancel the edit. We handle the label update as part of the
        // save process. If we allow the edit to be commited as normal we get
        // multiple items with the same label if the label change causes an
        // order position change.
        e.CancelEdit = true;

        // Save the change.
        if (_channelInlineEditSubItemIndex == ChannelListViewHandler.SUBITEM_INDEX_NAME)
        {
          if (string.IsNullOrWhiteSpace(e.Label))
          {
            return;
          }
          this.LogInfo("channels: channel {0} renamed, old name = {1}, new name = {2}", channel.IdChannel, channel.Name, e.Label);
          channel.Name = e.Label;
        }
        else if (_channelInlineEditSubItemIndex == ChannelListViewHandler.SUBITEM_INDEX_NUMBER)
        {
          string lcn;
          if (!LogicalChannelNumber.Create(e.Label, out lcn))
          {
            return;
          }
          this.LogInfo("channels: channel {0} renumbered, old number = {1}, new number = {2} ({3})", channel.IdChannel, channel.ChannelNumber, lcn, e.Label);
          channel.ChannelNumber = lcn;
        }

        channel = SaveChannel(channel);
        UpdateAllViewsOnAddOrEditChannels(new List<Channel> { channel });
      }
      finally
      {
        // Revert to normal tab behaviour (select next control instead of next item).
        _channelInlineEditSubItemIndex = -1;
      }
    }

    private void listViewChannels_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      // Tab to edit next item.
      if (e.KeyCode == Keys.Tab && _channelInlineEditSubItemIndex >= 0 && listViewChannels.SelectedItems.Count > 0)
      {
        // Prevent normal tab behaviour (select next item instead of next control).
        e.IsInputKey = true;

        // Begin editing the next item. If editing a name then start editing
        // the next item's name; if editing a number then start editing the
        // next item's number.
        ListViewItem item = listViewChannels.SelectedItems[0];
        if (e.Shift)
        {
          item = listViewChannels.Items[(listViewChannels.SelectedItems[0].Index + listViewChannels.Items.Count - 1) % listViewChannels.Items.Count];
        }
        else
        {
          item = listViewChannels.Items[(listViewChannels.SelectedItems[0].Index + 1) % listViewChannels.Items.Count];
        }

        // The first BeginEdit() completes editing on the previous item; the
        // second is needed to actually start editing the new item, because
        // completing editing on the first item filters the the list.
        int subItemIndex = _channelInlineEditSubItemIndex;
        item.BeginEdit();
        item.EnsureVisible();
        _channelInlineEditTriggerMouseLocation = item.SubItems[subItemIndex].Bounds.Location;
        item.BeginEdit();
        item.Selected = true;
      }
    }

    private void textBoxChannelNumber_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      // Catch tab to edit the next channel number.
      listViewChannels_PreviewKeyDown(sender, e);
    }

    private void textBoxChannelNumber_KeyDown(object sender, KeyEventArgs e)
    {
      // Catch key presses that complete/finish channel number editing.
      if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
      {
        if (e.KeyCode == Keys.Escape)
        {
          _channelInlineEditSubItem = null;        // Avoid saving the change.
        }
        e.Handled = true;
        textBoxChannelNumber_Leave(textBoxChannelNumber, null);
      }
    }

    private void textBoxChannelNumber_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == '.')
      {
        e.Handled = (sender as TextBox).Text.IndexOf('.') > -1;
      }
      else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
      {
        e.Handled = true;
      }
    }

    private void textBoxChannelNumber_Leave(object sender, EventArgs e)
    {
      // Trigger saving, mimicing normal list view label after edit behaviour.
      string newNumberAsString = textBoxChannelNumber.Text;
      if (_channelInlineEditSubItem != null && !string.Equals(newNumberAsString, _channelInlineEditSubItem.Text))
      {
        LabelEditEventArgs lee = new LabelEditEventArgs(-1, newNumberAsString);
        listViewChannels_AfterLabelEdit(sender, lee);
        if (!lee.CancelEdit)
        {
          _channelInlineEditSubItem.Text = lee.Label;
        }
      }
      else
      {
        listViewChannels_AfterLabelEdit(sender, new LabelEditEventArgs(-1, null));
      }
      textBoxChannelNumber.Hide();
    }

    #endregion

    #endregion

    #region channel groups

    private void comboBoxChannelGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
      ChannelGroup group = comboBoxChannelGroup.SelectedItem as ChannelGroup;
      if (group == null)
      {
        return;
      }

      this.LogDebug("channels: selecting channel group, ID = {0}, name = {1}", group.IdChannelGroup, group.Name);
      if (_currentChannelGroup != null)
      {
        SaveChannelsInGroupOrder();
      }
      _currentChannelGroup = group;

      listViewChannelsInGroup.BeginUpdate();
      try
      {
        listViewChannelsInGroup.Items.Clear();
        List<ChannelGroupChannelMapping> mappings;
        if (_channelsInGroupMappingCache.TryGetValue(group.IdChannelGroup, out mappings))
        {
          ListViewItem[] items = new ListViewItem[mappings.Count];
          int i = 0;
          foreach (ChannelGroupChannelMapping mapping in mappings)
          {
            ListViewItem item;
            if (_listViewChannelsInGroupItemCache.TryGetValue(mapping.IdChannel, out item))
            {
              item.Tag = mapping;
              items[i++] = item;
            }
          }
          listViewChannelsInGroup.Items.AddRange(items);
        }
      }
      finally
      {
        listViewChannelsInGroup.EndUpdate();
      }

      listViewChannelsInGroup_SelectedIndexChanged(null, null);
    }

    private void listViewChannelsInGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewChannelsInGroup.SelectedItems;
      buttonGroupChannelsRemove.Enabled = items.Count > 0;
      buttonGroupOrderByNumber.Enabled = listViewChannelsInGroup.Items.Count > 1;
      buttonGroupOrderByName.Enabled = buttonGroupOrderByNumber.Enabled;
      buttonGroupOrderUp.Enabled = items.Count > 1 || (items.Count == 1 && items[0].Index != 0);
      buttonGroupOrderDown.Enabled = items.Count > 1 || (items.Count == 1 && items[0].Index != listViewChannelsInGroup.Items.Count - 1);
    }

    private void listViewChannelsInGroup_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        buttonGroupChannelsRemove_Click(null, null);
        e.Handled = true;
      }
    }

    private ListViewItem CreateChannelsInGroupItemForChannel(Channel channel)
    {
      ListViewItem item = new ListViewItem(channel.Name);
      ListViewItem.ListViewSubItem subItem = item.SubItems.Add(channel.ChannelNumber.ToString());
      subItem.Tag = channel;
      return item;
    }

    private void UpdateChannelsInGroupViewOnAddOrEditChannels(IEnumerable<Channel> channels)
    {
      listViewChannelsInGroup.BeginUpdate();
      try
      {
        foreach (Channel channel in channels)
        {
          ListViewItem newItem = CreateChannelsInGroupItemForChannel(channel);
          ListViewItem oldItem;
          if (_listViewChannelsInGroupItemCache.TryGetValue(channel.IdChannel, out oldItem) && oldItem.Index >= 0)
          {
            newItem.Tag = oldItem.Tag;
            int index = oldItem.Index;
            oldItem.Remove();
            listViewChannelsInGroup.Items.Insert(index, newItem);
          }
          _listViewChannelsInGroupItemCache[channel.IdChannel] = newItem;
        }
      }
      finally
      {
        listViewChannelsInGroup.EndUpdate();
      }
    }

    private void UpdateChannelsInGroupViewOnDeleteChannels(IEnumerable<Channel> channels)
    {
      HashSet<int> channelIds = new HashSet<int>();
      listViewChannelsInGroup.BeginUpdate();
      try
      {
        foreach (Channel channel in channels)
        {
          channelIds.Add(channel.IdChannel);

          ListViewItem item;
          if (_listViewChannelsInGroupItemCache.TryGetValue(channel.IdChannel, out item))
          {
            item.Remove();
            _listViewChannelsInGroupItemCache.Remove(channel.IdChannel);
          }
        }
      }
      finally
      {
        listViewChannelsInGroup.EndUpdate();
      }

      foreach (KeyValuePair<int, List<ChannelGroupChannelMapping>> groupMappings in _channelsInGroupMappingCache)
      {
        groupMappings.Value.RemoveAll(mapping => channelIds.Contains(mapping.IdChannel));
      }
    }

    #region group buttons

    private void buttonGroupAdd_Click(object sender, EventArgs e)
    {
      CreateNewChannelGroup();
    }

    private ChannelGroup CreateNewChannelGroup()
    {
      FormEnterText dlg = new FormEnterText("New Channel Group", "Please enter a name for the new channel group:", "channel group");
      while (true)
      {
        if (dlg.ShowDialog() != DialogResult.OK)
        {
          dlg.Dispose();
          return null;
        }

        bool found = false;
        int lastGroupSortOrder = 0;
        foreach (ChannelGroup g in comboBoxChannelGroup.Items)
        {
          if (string.Equals(g.Name, dlg.TextValue))
          {
            MessageBox.Show(string.Format("There is already a group named {0}. Please choose a different name.", g.Name), MESSAGE_CAPTION);
            found = true;
            break;
          }
          lastGroupSortOrder = g.SortOrder;
        }

        if (!found)
        {
          ChannelGroup group = new ChannelGroup();
          group.Name = dlg.TextValue;
          group.MediaType = (int)_mediaType;
          group.SortOrder = lastGroupSortOrder + 1;
          group = ServiceAgents.Instance.ChannelGroupServiceAgent.SaveChannelGroup(group);
          this.LogInfo("channels: channel group {0} added, name = {1}", group.IdChannelGroup, group.Name);

          _listViewChannelsHandler.AddGroup(group);
          _channelsInGroupMappingCache[group.IdChannelGroup] = new List<ChannelGroupChannelMapping>(_listViewChannelsHandler.AllItems.Count);
          comboBoxChannelGroup.BeginUpdate();
          try
          {
            comboBoxChannelGroup.Items.Add(group);
            comboBoxChannelGroup.SelectedIndex = comboBoxChannelGroup.Items.Count - 1;
          }
          finally
          {
            comboBoxChannelGroup.EndUpdate();
          }

          buttonGroupDelete.Enabled = comboBoxChannelGroup.Items.Count > 1;
          buttonGroupOrder.Enabled = buttonGroupDelete.Enabled;

          dlg.Dispose();
          return group;
        }
      }
    }

    private void buttonGroupRename_Click(object sender, EventArgs e)
    {
      FormEnterText dlg = new FormEnterText("Rename Channel Group", "Please enter a new name for the channel group:", _currentChannelGroup.Name);
      while (true)
      {
        if (dlg.ShowDialog() != DialogResult.OK)
        {
          dlg.Dispose();
          return;
        }

        bool found = false;
        foreach (ChannelGroup g in comboBoxChannelGroup.Items)
        {
          if (string.Equals(g.Name, dlg.TextValue) && g.IdChannelGroup != _currentChannelGroup.IdChannelGroup)
          {
            found = true;
            MessageBox.Show(string.Format("There is already a group named {0}. Please choose a different name.", g.Name), MESSAGE_CAPTION);
            break;
          }
        }

        if (!found)
        {
          this.LogInfo("channels: channel group {0} renamed, old name = {1}, new name = {2}", _currentChannelGroup.IdChannelGroup, _currentChannelGroup.Name, dlg.TextValue);
          _currentChannelGroup.Name = dlg.TextValue;
          _currentChannelGroup = ServiceAgents.Instance.ChannelGroupServiceAgent.SaveChannelGroup(_currentChannelGroup);

          int index = comboBoxChannelGroup.SelectedIndex;
          comboBoxChannelGroup.BeginUpdate();
          comboBoxChannelGroup.SelectedIndexChanged -= comboBoxChannelGroup_SelectedIndexChanged;
          try
          {
            comboBoxChannelGroup.Items.RemoveAt(index);
            comboBoxChannelGroup.Items.Insert(index, _currentChannelGroup);
            comboBoxChannelGroup.SelectedIndex = index;
          }
          finally
          {
            comboBoxChannelGroup.SelectedIndexChanged += comboBoxChannelGroup_SelectedIndexChanged;
            comboBoxChannelGroup.EndUpdate();
          }

          _listViewChannelsHandler.AddOrUpdateGroup(_currentChannelGroup);

          dlg.Dispose();
          return;
        }
      }
    }

    private void buttonGroupDelete_Click(object sender, EventArgs e)
    {
      // We must ensure that at least one group always exists so that plugins etc. function correctly.
      if (comboBoxChannelGroup.Items.Count < 2)
      {
        return;
      }

      // Prompt if one or more channels are in the group.
      List<ChannelGroupChannelMapping> mappings;
      if (!_channelsInGroupMappingCache.TryGetValue(_currentChannelGroup.IdChannelGroup, out mappings))
      {
        mappings = new List<ChannelGroupChannelMapping>(0);
      }
      if (mappings.Count > 0)
      {
        DialogResult result = MessageBox.Show("Are you sure you want to delete this group?", MESSAGE_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
        {
          return;
        }
      }

      this.LogInfo("channels: channel group {0} deleted, channels = [{1}]", _currentChannelGroup.IdChannelGroup, string.Join(", ", from mapping in mappings select mapping.IdChannel));
      _listViewChannelsHandler.DeleteGroup(_currentChannelGroup);
      ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroup(_currentChannelGroup.IdChannelGroup);
      _channelsInGroupMappingCache.Remove(_currentChannelGroup.IdChannelGroup);
      comboBoxChannelGroup.Items.Remove(_currentChannelGroup);
      comboBoxChannelGroup.SelectedIndex = 0;

      buttonGroupDelete.Enabled = comboBoxChannelGroup.Items.Count > 1;
      buttonGroupOrder.Enabled = buttonGroupDelete.Enabled;
    }

    private void buttonGroupOrder_Click(object sender, EventArgs e)
    {
      ListViewItem[] items = new ListViewItem[comboBoxChannelGroup.Items.Count];
      int i = 0;
      foreach (ChannelGroup group in comboBoxChannelGroup.Items)
      {
        ListViewItem item = new ListViewItem(group.Name);
        item.Tag = group;
        items[i++] = item;
      }

      using (FormOrderItems dlg = new FormOrderItems("Order Channel Groups", "Please set the order of the channel groups:", items))
      {
        i = 0;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          bool isOrderChanged = false;
          int selectedIndex = 0;
          ChannelGroup[] groups = new ChannelGroup[comboBoxChannelGroup.Items.Count];
          foreach (ListViewItem item in dlg.Items)
          {
            ChannelGroup group = item.Tag as ChannelGroup;
            if (group == null)
            {
              continue;
            }

            if (group.IdChannelGroup == _currentChannelGroup.IdChannelGroup)
            {
              selectedIndex = i;
            }
            if (group.SortOrder != ++i)
            {
              isOrderChanged = true;
              this.LogInfo("channels: channel group {0} sort order changed from {1} to {2}", group.IdChannelGroup, group.SortOrder, i);
              group.SortOrder = i;
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.SaveChannelGroup(group);
            }
            groups[i - 1] = group;
          }

          if (isOrderChanged)
          {
            comboBoxChannelGroup.BeginUpdate();
            comboBoxChannelGroup.SelectedIndexChanged -= comboBoxChannelGroup_SelectedIndexChanged;
            try
            {
              comboBoxChannelGroup.Items.Clear();
              comboBoxChannelGroup.Items.AddRange(groups);
              comboBoxChannelGroup.SelectedIndex = selectedIndex;
            }
            finally
            {
              comboBoxChannelGroup.SelectedIndexChanged += comboBoxChannelGroup_SelectedIndexChanged;
              comboBoxChannelGroup.EndUpdate();
            }
          }
        }
      }
    }

    #endregion

    #region channel buttons

    private void buttonGroupChannelsAdd_Click(object sender, EventArgs e)
    {
      List<ChannelGroupChannelMapping> channelsInGroup;
      if (!_channelsInGroupMappingCache.TryGetValue(_currentChannelGroup.IdChannelGroup, out channelsInGroup))
      {
        channelsInGroup = new List<ChannelGroupChannelMapping>(0);
      }
      List<Channel> availableChannels = new List<Channel>(_listViewChannelsHandler.AllItems.Count);
      foreach (ListViewItem item in _listViewChannelsHandler.AllItems)
      {
        Channel channel = _listViewChannelsHandler.GetChannelForItem(item);
        if (channel != null && !channelsInGroup.Exists(mapping => mapping.IdChannel == channel.IdChannel))
        {
          availableChannels.Add(channel);
        }
      }
      if (availableChannels.Count == 0)
      {
        MessageBox.Show("All channels are already in this group.", MESSAGE_CAPTION);
        return;
      }

      using (FormSelectItems dlg = new FormSelectItems("Add Channel(s) To Group", "Please select one or more channels to add:", availableChannels.ToArray(), "DisplayName", true))
      {
        if (dlg.ShowDialog() != DialogResult.OK || dlg.Items == null || dlg.Items.Count == 0)
        {
          return;
        }

        IList<Channel> channelsToAdd = new List<Channel>(dlg.Items.Count);
        foreach (Channel channel in dlg.Items)
        {
          channelsToAdd.Add(channel);
        }
        AddChannelsToGroup(_currentChannelGroup, channelsToAdd);
      }
    }

    private void AddChannelsToGroup(ChannelGroup group, ICollection<Channel> channels)
    {
      if (group == null || channels == null || channels.Count == 0)
      {
        return;
      }

      NotifyForm dlg = null;
      if (channels.Count > 10)
      {
        dlg = new NotifyForm("Adding selected channels to group...",
                                        "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
      }

      try
      {
        List<ChannelGroupChannelMapping> existingMappings;
        if (!_channelsInGroupMappingCache.TryGetValue(group.IdChannelGroup, out existingMappings))
        {
          existingMappings = new List<ChannelGroupChannelMapping>(channels.Count);
          _channelsInGroupMappingCache[group.IdChannelGroup] = existingMappings;
        }

        int lastMappingSortOrder = existingMappings.Count + 1;
        IList<ChannelGroupChannelMapping> mappings = new List<ChannelGroupChannelMapping>(channels.Count);
        IDictionary<int, Channel> channelDictionary = new Dictionary<int, Channel>(channels.Count);
        foreach (Channel channel in channels)
        {
          channelDictionary.Add(channel.IdChannel, channel);
          ChannelGroupChannelMapping mapping = new ChannelGroupChannelMapping();
          mapping.IdChannel = channel.IdChannel;
          mapping.IdChannelGroup = group.IdChannelGroup;
          mapping.SortOrder = lastMappingSortOrder++;
          mappings.Add(mapping);
        }
        this.LogInfo("channels: {0} channel(s) added to group {1}, channels = [{2}]", channelDictionary.Count, group.IdChannelGroup, string.Join(", ", channelDictionary.Keys));
        mappings = ServiceAgents.Instance.ChannelServiceAgent.SaveChannelGroupMappings(mappings);

        existingMappings.AddRange(mappings);
        _listViewChannelsHandler.AddChannelsToGroup(mappings);

        // If the group that we're adding to is currently selected on the
        // channel groups tab then add the items.
        if (comboBoxChannelGroup.SelectedItem == group)
        {
          ListViewItem[] items = new ListViewItem[mappings.Count];
          int i = 0;
          foreach (ChannelGroupChannelMapping mapping in mappings)
          {
            ListViewItem item;
            if (_listViewChannelsInGroupItemCache.TryGetValue(mapping.IdChannel, out item))
            {
              item.Tag = mapping;
              items[i++] = item;
            }
          }

          listViewChannelsInGroup.BeginUpdate();
          listViewChannelsInGroup.Items.AddRange(items);
          listViewChannelsInGroup.EndUpdate();

          listViewChannelsInGroup_SelectedIndexChanged(null, null);
        }
      }
      finally
      {
        if (dlg != null)
        {
          dlg.Close();
          dlg.Dispose();
        }
      }
    }

    private void buttonGroupChannelsRemove_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewChannelsInGroup.SelectedItems;
      if (items == null || items.Count == 0)
      {
        return;
      }

      NotifyForm dlg = null;
      if (items.Count > 10)
      {
        dlg = new NotifyForm("Removing selected channels from group...",
                                        "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
      }

      try
      {
        List<ChannelGroupChannelMapping> mappings = new List<ChannelGroupChannelMapping>(items.Count);
        HashSet<int> mappingIds = new HashSet<int>();
        IList<int> channelIds = new List<int>(items.Count);
        listViewChannelsInGroup.BeginUpdate();
        try
        {
          foreach (ListViewItem item in items)
          {
            ChannelGroupChannelMapping mapping = item.Tag as ChannelGroupChannelMapping;
            if (mapping != null)
            {
              mappings.Add(mapping);
              mappingIds.Add(mapping.IdChannelGroupChannelMapping);
              channelIds.Add(mapping.IdChannel);
            }
            listViewChannelsInGroup.Items.Remove(item);
          }
        }
        finally
        {
          listViewChannelsInGroup.EndUpdate();
        }

        this.LogInfo("channels: {0} channel(s) removed from group {1}, channels = [{2}]", channelIds.Count, _currentChannelGroup.IdChannelGroup, string.Join(", ", channelIds));
        ServiceAgents.Instance.ChannelServiceAgent.DeleteChannelGroupMappings(mappingIds);

        _listViewChannelsHandler.RemoveChannelsFromGroup(mappings);
        if (_channelsInGroupMappingCache.TryGetValue(_currentChannelGroup.IdChannelGroup, out mappings))
        {
          mappings.RemoveAll(mapping => mappingIds.Contains(mapping.IdChannelGroupChannelMapping));
        }
      }
      finally
      {
        listViewChannelsInGroup_SelectedIndexChanged(null, null);
        if (dlg != null)
        {
          dlg.Close();
          dlg.Dispose();
        }
      }
    }

    #endregion

    #region order

    private void listViewChannelsInGroup_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      MPButton buttonSort = null;
      MPButton buttonOther = null;
      switch (e.Column)
      {
        case 0:
          buttonSort = buttonGroupOrderByName;
          buttonOther = buttonGroupOrderByNumber;
          break;
        case 1:
          buttonSort = buttonGroupOrderByNumber;
          buttonOther = buttonGroupOrderByName;
          break;
      }

      if (e.Column == _listViewChannelsInGroupColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        _listViewChannelsInGroupColumnSorter.Order = _channelsInGroupLastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        _listViewChannelsInGroupColumnSorter.SortColumn = e.Column;
        _listViewChannelsInGroupColumnSorter.Order = SortOrder.Ascending;
      }

      _channelsInGroupLastSortOrder = _listViewChannelsInGroupColumnSorter.Order;

      // Perform the sort with these new sort options.
      listViewChannelsInGroup.Sort();

      if (buttonSort != null)
      {
        switch (_listViewChannelsInGroupColumnSorter.Order)
        {
          // Ascending and descending icons reversed intentionally. Having the
          // arrows point the opposite way to the sort sequence was confusing.
          case SortOrder.Ascending:
            buttonSort.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_dsc;
            break;
          case SortOrder.Descending:
            buttonSort.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_asc;
            break;
          case SortOrder.None:
            buttonSort.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;
            break;
        }
      }

      buttonOther.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;

      // Reset sort order to enable manual re-ordering.
      _listViewChannelsInGroupColumnSorter.Order = SortOrder.None;
    }

    private void buttonGroupOrderByName_Click(object sender, EventArgs e)
    {
      listViewChannelsInGroup_ColumnClick(null, new ColumnClickEventArgs(0));
    }

    private void buttonGroupOrderByNumber_Click(object sender, EventArgs e)
    {
      listViewChannelsInGroup_ColumnClick(null, new ColumnClickEventArgs(1));
    }

    private void buttonGroupOrderUp_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewChannelsInGroup.SelectedItems;
      if (items == null || listViewChannelsInGroup.Items.Count < 2)
      {
        return;
      }
      listViewChannelsInGroup.BeginUpdate();
      try
      {
        foreach (ListViewItem item in items)
        {
          int index = item.Index;
          if (index > 0)
          {
            listViewChannelsInGroup.Items.RemoveAt(index);
            listViewChannelsInGroup.Items.Insert(index - 1, item);
          }
        }
      }
      finally
      {
        listViewChannelsInGroup.EndUpdate();
      }

      listViewChannelsInGroup.Focus();
    }

    private void buttonGroupOrderDown_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewChannelsInGroup.SelectedItems;
      if (items == null || listViewChannelsInGroup.Items.Count < 2)
      {
        return;
      }
      listViewChannelsInGroup.BeginUpdate();
      try
      {
        for (int i = items.Count - 1; i >= 0; i--)
        {
          ListViewItem item = items[i];
          int index = item.Index;
          if (index + 1 < listViewChannelsInGroup.Items.Count)
          {
            listViewChannelsInGroup.Items.RemoveAt(index);
            listViewChannelsInGroup.Items.Insert(index + 1, item);
          }
        }
      }
      finally
      {
        listViewChannelsInGroup.EndUpdate();
      }

      listViewChannelsInGroup.Focus();
    }

    private void SaveChannelsInGroupOrder()
    {
      List<ChannelGroupChannelMapping> existingMappings;
      if (!_channelsInGroupMappingCache.TryGetValue(_currentChannelGroup.IdChannelGroup, out existingMappings))
      {
        // This can happen when we delete the channel group.
        return;
      }

      existingMappings.Clear();
      IList<ChannelGroupChannelMapping> mappings = new List<ChannelGroupChannelMapping>(listViewChannelsInGroup.Items.Count);
      IList<int> channelIds = new List<int>(listViewChannelsInGroup.Items.Count);
      int i = 1;
      foreach (ListViewItem item in listViewChannelsInGroup.Items)
      {
        ChannelGroupChannelMapping mapping = item.Tag as ChannelGroupChannelMapping;
        if (mapping == null)
        {
          continue;
        }
        if (mapping.SortOrder != i)
        {
          mapping.SortOrder = i;
          mappings.Add(mapping);
          channelIds.Add(mapping.IdChannel);
        }
        existingMappings.Add(mapping);
        i++;
      }
      if (mappings.Count > 0)
      {
        this.LogInfo("channels: {0} channel(s) reordered in group {1}, channels = [{2}]", mappings.Count, _currentChannelGroup.IdChannelGroup, string.Join(", ", channelIds));
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannelGroupMappings(mappings);

        // We're not creating new mappings here, so no need to bother
        // integrating the mappings returned by Save(). Also, since this
        // function is called on group or section change there is no need to
        // touch the items in the list view. This means we can get away with
        // simply reseting the change trackers, which is easier than searching
        // and replacing each saved mapping.
        foreach (ChannelGroupChannelMapping mapping in existingMappings)
        {
          mapping.AcceptChanges();
        }
        this.LogDebug("channels: new order for group {0}, channels = [{1}]", _currentChannelGroup.IdChannelGroup, string.Join(", ", from mapping in existingMappings select mapping.IdChannel));
      }
    }

    #endregion

    #endregion
  }
}