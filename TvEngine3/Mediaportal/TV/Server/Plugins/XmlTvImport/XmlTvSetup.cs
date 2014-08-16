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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.XmlTvImport.Service;
using Mediaportal.TV.Server.Plugins.XmlTvImport.Util;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  public partial class XmlTvSetup : SectionSettings
  {
    #region enums

    private enum MatchType
    {
      [Description("already mapped")]
      Mapped = 0,
      [Description("exact match")]
      Exact,
      [Description("partial match")]
      Partial,
      [Description("no match")]
      None
    }

    #endregion

    #region constants

    private const int ALL_TV_CHANNELS_GROUP_ID = -2;
    private const int ALL_RADIO_CHANNELS_GROUP_ID = -1;

    #endregion

    private class ComboBoxChannelGroup
    {
      public string Name;
      public int Id;

      public ComboBoxChannelGroup(string groupName, int idGroup)
      {
        Name = groupName;
        Id = idGroup;
      }

      public override string ToString()
      {
        return Name;
      }
    }

    private class ComboBoxGuideChannel
    {
      public string DisplayName { get; set; }
      public string Id { get; set; }

      public ComboBoxGuideChannel(string displayName, string id)
      {
        DisplayName = displayName;
        Id = id;
      }

      public ComboBoxGuideChannel ValueMember
      {
        get
        {
          return this;
        }
      }

      public override string ToString()
      {
        return DisplayName;
      }
    }

    #region variables

    private readonly ISettingService _settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;

    private IList<ChannelGroup> _channelGroups = null;
    private string _loadedGroupName = null;
    private int _loadedGroupId = 0;
    private Timer _statusUiUpdateTimer = null;

    private DateTime _importStatusDateTime = DateTime.MinValue;
    private string _importStatus = string.Empty;
    private string _importStatusChannelCounts = string.Empty;
    private string _importStatusProgramCounts = string.Empty;

    private DateTime _scheduledActionsStatusDateTime = DateTime.MinValue;
    private string _scheduledActionsStatus = string.Empty;

    #endregion

    public XmlTvSetup()
      : this("XmlTv")
    {
      ServiceAgents.Instance.AddGenericService<IXmlTvImportService>();
    }

    public XmlTvSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("XMLTV config: activating");
      textBoxFolder.Text = _settingServiceAgent.GetValue("xmlTvFolder", string.Empty);
      folderBrowserDialog.SelectedPath = textBoxFolder.Text;
      checkBoxDeleteBeforeImport.Checked = _settingServiceAgent.GetValue("xmlTvDeleteBeforeImport", true);

      checkBoxTimeCorrectionEnable.Checked = _settingServiceAgent.GetValue("xmlTvUseTimeCorrection", false);
      numericUpDownTimeCorrectionHours.Value = _settingServiceAgent.GetValue("xmlTvTimeCorrectionHours", 0);
      numericUpDownTimeCorrectionMinutes.Value = _settingServiceAgent.GetValue("xmlTvTimeCorrectionMinutes", 0);
      checkBoxTimeCorrectionEnable_CheckedChanged(null, null);

      checkBoxMappingsPartialMatch.Checked = _settingServiceAgent.GetValue("xmlTvUsePartialMatching", false);

      checkBoxScheduledActionsDownload.Checked = _settingServiceAgent.GetValue("xmlTvScheduledActionsDownload", false);
      textBoxScheduledActionsDownloadUrl.Text = _settingServiceAgent.GetValue("xmlTvScheduledActionsDownloadUrl", "http://www.mysite.com/tvguide.xml");
      checkBoxScheduledActionsDownload_CheckedChanged(null, null);

      checkBoxScheduledActionsProgram.Checked = _settingServiceAgent.GetValue("xmlTvScheduledActionsProgram", false);
      textBoxScheduledActionsProgramLocation.Text = _settingServiceAgent.GetValue("xmlTvScheduledActionsProgramLocation", @"c:\Program Files\My Program\MyProgram.exe");
      checkBoxScheduledActionsProgram_CheckedChanged(null, null);
      if (string.IsNullOrWhiteSpace(textBoxScheduledActionsProgramLocation.Text) || !File.Exists(textBoxScheduledActionsProgramLocation.Text))
      {
        selectScheduledActionsProgramDialog.InitialDirectory = folderBrowserDialog.SelectedPath;
      }
      else
      {
        selectScheduledActionsProgramDialog.InitialDirectory = Path.GetDirectoryName(textBoxScheduledActionsProgramLocation.Text);
        selectScheduledActionsProgramDialog.FileName = Path.GetFileName(textBoxScheduledActionsProgramLocation.Text);
      }

      numericUpDownScheduledActionsTimeFrequency.Value = _settingServiceAgent.GetValue("xmlTvScheduledActionsTimeFrequency", 24);
      dateTimePickerScheduledActionsTimeBetweenStart.Value = _settingServiceAgent.GetValue("xmlTvScheduledActionsTimeBetweenStart", DateTime.Now);
      dateTimePickerScheduledActionsTimeBetweenEnd.Value = _settingServiceAgent.GetValue("xmlTvScheduledActionsTimeBetweenEnd", DateTime.Now);
      radioScheduledActionsTimeStartup.Checked = _settingServiceAgent.GetValue("xmlTvScheduledActionsTimeOnStartup", false);
      radioScheduledActionsTimeBetween.Checked = !radioScheduledActionsTimeStartup.Checked;
      radioScheduledActionsTimeBetween_CheckedChanged(null, null);

      DebugSettings();

      try
      {
        comboBoxMappingsChannelGroup.Items.Clear();
        comboBoxMappingsChannelGroup.Items.Add(new ComboBoxChannelGroup("All TV Channels", ALL_TV_CHANNELS_GROUP_ID));
        comboBoxMappingsChannelGroup.Items.Add(new ComboBoxChannelGroup("All Radio Channels", ALL_RADIO_CHANNELS_GROUP_ID));
        comboBoxMappingsChannelGroup.Tag = "";

        _channelGroups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroups(ChannelGroupIncludeRelationEnum.None);
        foreach (ChannelGroup group in _channelGroups)
        {
          comboBoxMappingsChannelGroup.Items.Add(new ComboBoxChannelGroup(string.Format("{0} - {1}", ((MediaTypeEnum)group.MediaType).GetDescription(), group.GroupName), group.IdGroup));
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTV config: failed to load channel groups");
      }

      labelImportStatusDateTimeValue.Text = string.Empty;
      labelImportStatusValue.Text = string.Empty;
      labelImportStatusChannelCountsValue.Text = string.Empty;
      labelImportStatusProgramCountsValue.Text = string.Empty;
      labelScheduledActionsStatusDateTimeValue.Text = string.Empty;
      labelScheduledActionsStatusValue.Text = string.Empty;
      UpdateImportAndScheduleStatusUi();
      _statusUiUpdateTimer = new Timer();
      _statusUiUpdateTimer.Interval = 10000;
      _statusUiUpdateTimer.Tick += new EventHandler(OnStatusUiUpdateTimerTick);
      _statusUiUpdateTimer.Enabled = true;
      _statusUiUpdateTimer.Start();
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("XMLTV config: deactivating");
      SaveSettings();
      DebugSettings();
      _statusUiUpdateTimer.Enabled = false;
      _statusUiUpdateTimer.Stop();
      _statusUiUpdateTimer.Dispose();
      _statusUiUpdateTimer = null;
      base.OnSectionDeActivated();
    }

    public override void SaveSettings()
    {
      this.LogDebug("XMLTV config: saving settings");
      _settingServiceAgent.SaveValue("xmlTvFolder", textBoxFolder.Text);
      _settingServiceAgent.SaveValue("xmlTvDeleteBeforeImport", checkBoxDeleteBeforeImport.Checked);

      _settingServiceAgent.SaveValue("xmlTvUseTimeCorrection", checkBoxTimeCorrectionEnable.Checked);
      _settingServiceAgent.SaveValue("xmlTvTimeCorrectionHours", (int)numericUpDownTimeCorrectionHours.Value);
      _settingServiceAgent.SaveValue("xmlTvTimeCorrectionMinutes", (int)numericUpDownTimeCorrectionMinutes.Value);

      _settingServiceAgent.SaveValue("xmlTvUsePartialMatching", checkBoxMappingsPartialMatch.Checked);

      _settingServiceAgent.SaveValue("xmlTvScheduledActionsDownload", checkBoxScheduledActionsDownload.Checked);
      _settingServiceAgent.SaveValue("xmlTvScheduledActionsDownloadUrl", textBoxScheduledActionsDownloadUrl.Text);
      _settingServiceAgent.SaveValue("xmlTvScheduledActionsProgram", checkBoxScheduledActionsProgram.Checked);
      _settingServiceAgent.SaveValue("xmlTvScheduledActionsProgramLocation", textBoxScheduledActionsProgramLocation.Text);

      _settingServiceAgent.SaveValue("xmlTvScheduledActionsTimeFrequency", (int)numericUpDownScheduledActionsTimeFrequency.Value);
      _settingServiceAgent.SaveValue("xmlTvScheduledActionsTimeBetweenStart", dateTimePickerScheduledActionsTimeBetweenStart.Value);
      _settingServiceAgent.SaveValue("xmlTvScheduledActionsTimeBetweenEnd", dateTimePickerScheduledActionsTimeBetweenEnd.Value);
      _settingServiceAgent.SaveValue("xmlTvScheduledActionsTimeOnStartup", radioScheduledActionsTimeStartup.Checked);
      base.SaveSettings();
    }

    private void DebugSettings()
    {
      this.LogDebug("XMLTV config: settings");
      this.LogDebug("  folder                = {0}", textBoxFolder.Text);
      this.LogDebug("  delete before import? = {0}", checkBoxDeleteBeforeImport.Checked);
      this.LogDebug("  time correction...");
      this.LogDebug("    enabled             = {0}", checkBoxTimeCorrectionEnable.Checked);
      this.LogDebug("    hours               = {0}", numericUpDownTimeCorrectionHours.Value);
      this.LogDebug("    minutes             = {0}", numericUpDownTimeCorrectionMinutes.Value);
      this.LogDebug("  partial matching?     = {0}", checkBoxMappingsPartialMatch.Checked);
      this.LogDebug("  scheduled actions...");
      this.LogDebug("    download?           = {0}", checkBoxScheduledActionsDownload.Checked);
      this.LogDebug("    download URL        = {0}", textBoxScheduledActionsDownloadUrl.Text);
      this.LogDebug("    run program?        = {0}", checkBoxScheduledActionsProgram.Checked);
      this.LogDebug("    program location    = {0}", textBoxScheduledActionsProgramLocation.Text);
      this.LogDebug("    frequency           = {0} hour(s)", numericUpDownScheduledActionsTimeFrequency.Value);
      this.LogDebug("    on startup/resume?  = {0}", radioScheduledActionsTimeStartup.Checked);
      this.LogDebug("    between time start  = {0}", dateTimePickerScheduledActionsTimeBetweenStart.Value.TimeOfDay);
      this.LogDebug("    between time end    = {0}", dateTimePickerScheduledActionsTimeBetweenEnd.Value.TimeOfDay);
    }

    private void OnStatusUiUpdateTimerTick(object sender, EventArgs e)
    {
      // Update the status UI. Take care to do it on the UI thread.
      this.Invoke((MethodInvoker)delegate
      {
        UpdateImportAndScheduleStatusUi();
      });
    }

    private void UpdateImportAndScheduleStatusUi()
    {
      ServiceAgents.Instance.PluginService<IXmlTvImportService>().GetImportStatus(out _importStatusDateTime, out _importStatus, out _importStatusChannelCounts, out _importStatusProgramCounts);
      if (_importStatusDateTime > DateTime.MinValue && (
        !string.Equals(labelImportStatusDateTimeValue.Text, _importStatusDateTime.ToString()) ||
        !string.Equals(labelImportStatusValue.Text, _importStatus) ||
        !string.Equals(labelImportStatusChannelCountsValue.Text, _importStatusChannelCounts) ||
        !string.Equals(labelImportStatusProgramCountsValue.Text, _importStatusProgramCounts)
      ))
      {
        labelImportStatusDateTimeValue.Text = _importStatusDateTime.ToString();
        labelImportStatusValue.Text = _importStatus;
        labelImportStatusChannelCountsValue.Text = _importStatusChannelCounts;
        labelImportStatusProgramCountsValue.Text = _importStatusProgramCounts;
        this.LogDebug("XMLTV config: import status update...");
        this.LogDebug("  date/time = {0}", _importStatusDateTime);
        this.LogDebug("  status    = {0}", _importStatus);
        this.LogDebug("  channels  = {0}", _importStatusChannelCounts);
        this.LogDebug("  programs  = {0}", _importStatusProgramCounts);
      }

      ServiceAgents.Instance.PluginService<IXmlTvImportService>().GetScheduledActionsStatus(out _scheduledActionsStatusDateTime, out _scheduledActionsStatus);
      if (_scheduledActionsStatusDateTime > DateTime.MinValue && (
        !string.Equals(labelScheduledActionsStatusDateTimeValue.Text, _scheduledActionsStatusDateTime.ToString()) ||
        !string.Equals(labelScheduledActionsStatusValue.Text, _scheduledActionsStatus.ToString())
      ))
      {
        labelScheduledActionsStatusDateTimeValue.Text = _scheduledActionsStatusDateTime.ToString();
        labelScheduledActionsStatusValue.Text = _scheduledActionsStatus.ToString();
        this.LogDebug("XMLTV config: scheduled actions status update...");
        this.LogDebug("  date/time = {0}", _scheduledActionsStatusDateTime);
        this.LogDebug("  status    = {0}", _scheduledActionsStatus);
      }
    }

    private void buttonFolderBrowse_Click(object sender, EventArgs e)
    {
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
      {
        textBoxFolder.Text = folderBrowserDialog.SelectedPath;
      }
    }

    private void buttonImport_Click(object sender, EventArgs e)
    {
      this.LogDebug("XMLTV config: force-starting import");
      SaveSettings();
      ServiceAgents.Instance.PluginService<IXmlTvImportService>().ImportNow();
    }

    private void checkBoxTimeCorrectionEnable_CheckedChanged(object sender, EventArgs e)
    {
      numericUpDownTimeCorrectionHours.Enabled = checkBoxTimeCorrectionEnable.Checked;
      numericUpDownTimeCorrectionMinutes.Enabled = checkBoxTimeCorrectionEnable.Checked;
    }

    private void buttonMappingsLoad_Click(object sender, EventArgs e)
    {
      ComboBoxChannelGroup channelGroup = comboBoxMappingsChannelGroup.SelectedItem as ComboBoxChannelGroup;
      if (channelGroup == null)
      {
        return;
      }

      try
      {
        this.LogDebug("XMLTV config: loading mappings for channel group, ID = {0}, name = {1}, partial match = {2}", channelGroup.Id, channelGroup.Name, checkBoxMappingsPartialMatch.Checked);
        textBoxMappingsAction.Text = "loading";
        dataGridViewMappings.Rows.Clear();

        // Load the database channels.
        IList<Channel> databaseChannels;
        if (channelGroup.Id == ALL_TV_CHANNELS_GROUP_ID)
        {
          databaseChannels = ServiceAgents.Instance.ChannelServiceAgent.ListAllVisibleChannelsByMediaType(MediaTypeEnum.TV, ChannelIncludeRelationEnum.None);
        }
        else if (channelGroup.Id == ALL_RADIO_CHANNELS_GROUP_ID)
        {
          databaseChannels = ServiceAgents.Instance.ChannelServiceAgent.ListAllVisibleChannelsByMediaType(MediaTypeEnum.Radio, ChannelIncludeRelationEnum.None);
        }
        else
        {
          databaseChannels = ServiceAgents.Instance.ChannelServiceAgent.ListAllVisibleChannelsByGroupId(channelGroup.Id, ChannelIncludeRelationEnum.None);
        }
        if (databaseChannels.Count == 0)
        {
          MessageBox.Show("There are no channels available to map.");
          return;
        }

        // Load the guide channels from the server. The structure is:
        //    file name -> ID -> [display names]
        // Arrange the channels into 3 collections used for partial matching,
        // fast ID lookups, and display.
        IDictionary<string, IDictionary<string, IList<string>>> guideChannels = ServiceAgents.Instance.PluginService<IXmlTvImportService>().GetGuideChannelDetails();
        HashSet<string> guideChannelIds = new HashSet<string>();
        TstDictionary matchingDictionary = new TstDictionary();
        IList<ComboBoxGuideChannel> guideChannelsForComboBox = new List<ComboBoxGuideChannel>(200);
        IDictionary<string, ComboBoxGuideChannel> comboBoxValueLookup = new Dictionary<string, ComboBoxGuideChannel>(200);
        ComboBoxGuideChannel gc = new ComboBoxGuideChannel(string.Empty, string.Empty);
        guideChannelsForComboBox.Add(gc);
        comboBoxValueLookup.Add(gc.Id, gc);
        foreach (KeyValuePair<string, IDictionary<string, IList<string>>> fileChannels in guideChannels)
        {
          foreach (KeyValuePair<string, IList<string>> channel in fileChannels.Value)
          {
            string id = string.Format("xmltv|{0}|{1}", fileChannels.Key, channel.Key);
            this.LogDebug("XMLTV config: guide channel, ID = {0}, name(s) = [{1}]", id, string.Join(", ", channel.Value));
            if (!guideChannelIds.Add(id))
            {
              this.LogWarn("XMLTV config: found multiple channels with ID {0}, won't be able to distinguish which data to use", id);
            }
            foreach (string displayName in channel.Value)
            {
              string matchKey = displayName.Replace(" ", "").ToLowerInvariant();
              if (!matchingDictionary.ContainsKey(matchKey))
              {
                matchingDictionary.Add(matchKey, id);
              }
              else
              {
                this.LogWarn("XMLTV config: found multiple channels named {0}, might match to wrong channel", displayName);
              }

              string itemName = displayName;
              if (!string.Equals(displayName, channel.Key))
              {
                itemName = string.Format("{0} ({1})", displayName, channel.Key);
              }
              gc = new ComboBoxGuideChannel(itemName, id);
              guideChannelsForComboBox.Add(gc);
              comboBoxValueLookup.Add(gc.Id, gc);
            }
          }
        }

        // Populate the grid.
        progressBarMappingsProgress.Minimum = 0;
        progressBarMappingsProgress.Maximum = databaseChannels.Count;
        progressBarMappingsProgress.Value = 0;
        dataGridViewMappings.Rows.Add(databaseChannels.Count);
        int row = 0;
        foreach (Channel channel in databaseChannels)
        {
          DataGridViewRow gridRow = dataGridViewMappings.Rows[row++];

          gridRow.Cells["dataGridViewColumnId"].Value = channel.IdChannel.ToString();
          gridRow.Cells["dataGridViewColumnTuningChannel"].Value = channel.DisplayName;
          gridRow.Tag = channel;

          // Trust me - you don't want to mess with this! Data grid view combo
          // boxes are fragile.
          DataGridViewComboBoxCell guideChannelComboBox = (DataGridViewComboBoxCell)gridRow.Cells["dataGridViewColumnGuideChannel"];
          guideChannelComboBox.DataSource = guideChannelsForComboBox;
          guideChannelComboBox.ValueType = typeof(ComboBoxGuideChannel);
          guideChannelComboBox.DisplayMember = "DisplayName";
          guideChannelComboBox.ValueMember = "ValueMember";
          guideChannelComboBox.Tag = channel.ExternalId ?? string.Empty;

          // Find the best match guide channel for this channel.
          MatchType matchType = MatchType.None;
          string bestMatchId = string.Empty;
          if (channel.ExternalId != null && guideChannelIds.Contains(channel.ExternalId))
          {
            bestMatchId = channel.ExternalId;
            matchType = MatchType.Mapped;
          }
          else
          {
            string matchKey = channel.DisplayName.Replace(" ", "").ToLowerInvariant();
            if (matchingDictionary.ContainsKey(matchKey))
            {
              bestMatchId = (string)matchingDictionary[matchKey];
              matchType = MatchType.Exact;
            }
          }

          // Partial matching...
          if (matchType == MatchType.None && checkBoxMappingsPartialMatch.Checked)
          {
            // Try to match using all except the last word in the channel
            // name or the first three letters if there is only one word.
            string name = channel.DisplayName.Trim();
            int spaceIndex = name.LastIndexOf(" ");
            if (spaceIndex > 0)
            {
              name = name.Substring(0, spaceIndex).Trim();
            }
            else if (name.Length > 3)
            {
              name = name.Substring(0, 3);
            }

            try
            {
              // Note: the partial match code doesn't work as described by the
              // author so we'll use PrefixMatch (created by a codeproject
              // user) instead.
              ICollection partialMatches = matchingDictionary.PrefixMatch(name.Replace(" ", "").ToLowerInvariant());
              if (partialMatches != null && partialMatches.Count > 0)
              {
                IEnumerator pme = partialMatches.GetEnumerator();
                pme.MoveNext();
                bestMatchId = (string)matchingDictionary[(string)pme.Current];
                matchType = MatchType.Partial;
              }
            }
            catch (Exception ex)
            {
              this.LogError(ex, "XMLTV config: failed to find partial matches for channel \"{0}\"", channel.DisplayName);
            }
          }
          this.LogDebug("XMLTV config: DB channel, ID = {0}, name = {1}, match type = {2}, best match = {3}", channel.IdChannel, channel.DisplayName, matchType, bestMatchId);
          guideChannelComboBox.Value = comboBoxValueLookup[bestMatchId];

          // Note the mapping cell values are set so that the grid can be
          // sorted by mapping state without actually showing text in the
          // cells.
          DataGridViewCell cell = gridRow.Cells["dataGridViewColumnMatchType"];
          cell.ToolTipText = matchType.GetDescription();
          cell.Value = string.Empty.PadRight((int)matchType, ' ');
          if (matchType == MatchType.Mapped)
          {
            cell.Style.BackColor = Color.White;
          }
          else if (matchType == MatchType.Exact)
          {
            cell.Style.BackColor = Color.Green;
          }
          else if (matchType == MatchType.Partial)
          {
            cell.Style.BackColor = Color.Yellow;
          }
          else
          {
            cell.Style.BackColor = Color.Red;
          }

          progressBarMappingsProgress.Value++;
        }

        _loadedGroupId = channelGroup.Id;
        _loadedGroupName = channelGroup.Name;
        textBoxMappingsAction.Text = "loaded";
      }
      catch (Exception ex)
      {
        textBoxMappingsAction.Text = "load failed";
        this.LogError(ex, "XMLTV config: failed to load channel group mappings, ID = {0}, name = {1}", channelGroup.Id, channelGroup.Name);
      }
    }

    private void buttonMappingsSave_Click(object sender, EventArgs e)
    {
      if (_loadedGroupName == null)
      {
        return;
      }
      this.LogDebug("XMLTV config: saving mappings for channel group, ID = {0}, name = {1}", _loadedGroupId, _loadedGroupName);
      try
      {
        IList<Channel> channels = new List<Channel>(dataGridViewMappings.Rows.Count);

        progressBarMappingsProgress.Value = 0;
        progressBarMappingsProgress.Minimum = 0;
        progressBarMappingsProgress.Maximum = dataGridViewMappings.Rows.Count;
        textBoxMappingsAction.Text = "saving mappings";

        foreach (DataGridViewRow row in dataGridViewMappings.Rows)
        {
          DataGridViewComboBoxCell guideChannelCell = (DataGridViewComboBoxCell)row.Cells["dataGridViewColumnGuideChannel"];
          ComboBoxGuideChannel guideChannel = guideChannelCell.Value as ComboBoxGuideChannel;
          if (guideChannel == null)   // It seems the combobox value is null for the blank item.
          {
            guideChannel = new ComboBoxGuideChannel(string.Empty, string.Empty);
          }
          string previousExternalId = guideChannelCell.Tag as string;
          if (!string.Equals(guideChannel.Id, previousExternalId))
          {
            Channel channel = row.Tag as Channel;
            channel.ExternalId = guideChannel.Id;
            channels.Add(channel);
            this.LogDebug("XMLTV config: mapped channel change, ID = {0}, name = {1}, old external ID = {2}, new external ID = {3}, guide name = {4}", channel.IdChannel, channel.DisplayName, previousExternalId ?? "[null]", guideChannel.Id ?? "[null]", guideChannel.DisplayName);
          }
          progressBarMappingsProgress.Value++;
        }
        if (channels.Count > 0)
        {
          textBoxMappingsAction.Text = "mappings saved";
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
        }
        else
        {
          textBoxMappingsAction.Text = "no changes";
        }
      }
      catch (Exception ex)
      {
        textBoxMappingsAction.Text = "save failed";
        this.LogError(ex, "XMLTV config: failed to save channel group mappings, ID = {0}, name = {1}", _loadedGroupId, _loadedGroupName);
      }
    }

    private void checkBoxScheduledActionsDownload_CheckedChanged(object sender, EventArgs e)
    {
      textBoxScheduledActionsDownloadUrl.Enabled = checkBoxScheduledActionsDownload.Checked;
      UpdateScheduledActionsTimeFields();
    }

    private void checkBoxScheduledActionsProgram_CheckedChanged(object sender, EventArgs e)
    {
      textBoxScheduledActionsProgramLocation.Enabled = checkBoxScheduledActionsProgram.Checked;
      buttonScheduledActionsProgramBrowse.Enabled = checkBoxScheduledActionsProgram.Checked;
      UpdateScheduledActionsTimeFields();
    }

    private void buttonScheduledActionsProgramBrowse_Click(object sender, EventArgs e)
    {
      if (selectScheduledActionsProgramDialog.ShowDialog() == DialogResult.OK)
      {
        textBoxScheduledActionsProgramLocation.Text = selectScheduledActionsProgramDialog.FileName;
      }
    }

    private void UpdateScheduledActionsTimeFields()
    {
      bool enabled = checkBoxScheduledActionsDownload.Checked || checkBoxScheduledActionsProgram.Checked;
      radioScheduledActionsTimeBetween.Enabled = enabled;
      groupBoxScheduledActionsTime.Enabled = enabled;
    }

    private void radioScheduledActionsTimeBetween_CheckedChanged(object sender, EventArgs e)
    {
      dateTimePickerScheduledActionsTimeBetweenStart.Enabled = radioScheduledActionsTimeBetween.Checked;
      dateTimePickerScheduledActionsTimeBetweenEnd.Enabled = radioScheduledActionsTimeBetween.Checked;
    }

    private void dateTimePickerScheduledActionsTimeBetweenStart_ValueChanged(object sender, EventArgs e)
    {
      if (dateTimePickerScheduledActionsTimeBetweenStart.Value.TimeOfDay > dateTimePickerScheduledActionsTimeBetweenEnd.Value.TimeOfDay)
      {
        dateTimePickerScheduledActionsTimeBetweenStart.Value = dateTimePickerScheduledActionsTimeBetweenEnd.Value;
      }
    }

    private void dateTimePickerScheduledActionsTimeBetweenEnd_ValueChanged(object sender, EventArgs e)
    {
      if (dateTimePickerScheduledActionsTimeBetweenStart.Value.TimeOfDay > dateTimePickerScheduledActionsTimeBetweenEnd.Value.TimeOfDay)
      {
        dateTimePickerScheduledActionsTimeBetweenEnd.Value = dateTimePickerScheduledActionsTimeBetweenStart.Value;
      }
    }

    private void buttonScheduledActionsTimeNow_Click(object sender, EventArgs e)
    {
      this.LogDebug("XMLTV config: force-starting scheduled actions");
      SaveSettings();
      ServiceAgents.Instance.PluginService<IXmlTvImportService>().ExecuteScheduledActionsNow();
    }
  }
}