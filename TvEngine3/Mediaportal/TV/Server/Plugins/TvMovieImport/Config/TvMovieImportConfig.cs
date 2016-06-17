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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.TvMovieImport.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Simmetrics;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.Plugins.TvMovieImport.Config
{
  public partial class TvMovieImportConfig : SectionSettings
  {
    #region enums

    private enum MatchType
    {
      [Description("already mapped")]
      Mapped = 0,
      [Description("new mapping, exact match")]
      Exact,
      [Description("new mapping, partial match")]
      Partial,
      [Description("new mapping, no match")]
      None,
      [Description("broken mapping, exact match")]
      BrokenExact,
      [Description("broken mapping, partial match")]
      BrokenPartial,
      [Description("broken mapping, no match")]
      Broken,
      [Description("non-TV Movie")]
      External
    }

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

    private IList<ChannelGroup> _channelGroups = null;
    private string _loadedGroupName = null;
    private int _loadedGroupId = 0;
    private Timer _statusUiUpdateTimer = null;

    private DateTime _importStatusDateTime = DateTime.MinValue;
    private string _importStatus = string.Empty;
    private string _importStatusChannelCounts = string.Empty;
    private string _importStatusProgramCounts = string.Empty;

    #endregion

    public TvMovieImportConfig()
      : base("TV Movie ClickFinder EPG Import")
    {
      ServiceAgents.Instance.AddGenericService<ITvMovieImportService>();
      InitializeComponent();
    }

    private void linkLabelInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://www.ewe-software.de/download.html#tvmovieclickfinder");
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("TV Movie import config: activating");
      ISettingService settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;
      textBoxDatabaseFile.Text = settingServiceAgent.GetValue(TvMovieImportSetting.DatabaseFile, string.Empty);
      if (string.IsNullOrEmpty(textBoxDatabaseFile.Text))
      {
        textBoxDatabaseFile.Text = ServiceAgents.Instance.PluginService<ITvMovieImportService>().GetDatabaseFilePath();
        settingServiceAgent.SaveValue(TvMovieImportSetting.DatabaseFile, textBoxDatabaseFile.Text);
      }
      openFileDialog.FileName = textBoxDatabaseFile.Text;

      numericUpDownUpdateTimeFrequency.Value = settingServiceAgent.GetValue(TvMovieImportSetting.UpdateTimeFrequency, 24);
      dateTimePickerUpdateTimeBetweenStart.Value = settingServiceAgent.GetValue(TvMovieImportSetting.UpdateTimeBetweenStart, DateTime.Now);
      dateTimePickerUpdateTimeBetweenEnd.Value = settingServiceAgent.GetValue(TvMovieImportSetting.UpdateTimeBetweenEnd, DateTime.Now);
      radioButtonUpdateTimeStartup.Checked = settingServiceAgent.GetValue(TvMovieImportSetting.UpdateTimeOnStartup, false);
      radioButtonUpdateTimeBetween.Checked = !radioButtonUpdateTimeStartup.Checked;
      radioUpdateTimeBetween_CheckedChanged(null, null);

      checkBoxMappingsPartialMatch.Checked = settingServiceAgent.GetValue(TvMovieImportSetting.UsePartialMatching, false);

      DebugSettings();

      try
      {
        comboBoxMappingsChannelGroup.Items.Clear();
        comboBoxMappingsChannelGroup.Tag = "";

        _channelGroups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroups(ChannelGroupRelation.None);
        foreach (ChannelGroup group in _channelGroups)
        {
          comboBoxMappingsChannelGroup.Items.Add(new ComboBoxChannelGroup(string.Format("{0} - {1}", ((MediaType)group.MediaType).GetDescription(), group.GroupName), group.IdGroup));
        }
        comboBoxMappingsChannelGroup.SelectedIndex = 0;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TV Movie import config: failed to load channel groups");
      }

      labelImportStatusDateTimeValue.Text = string.Empty;
      labelImportStatusValue.Text = string.Empty;
      labelImportStatusChannelCountsValue.Text = string.Empty;
      labelImportStatusProgramCountsValue.Text = string.Empty;
      UpdateImportStatusUi();
      _statusUiUpdateTimer = new Timer();
      _statusUiUpdateTimer.Interval = 10000;
      _statusUiUpdateTimer.Tick += new EventHandler(OnStatusUiUpdateTimerTick);
      _statusUiUpdateTimer.Enabled = true;
      _statusUiUpdateTimer.Start();
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("TV Movie import config: deactivating");
      SaveSettings();
      DebugSettings();
      _statusUiUpdateTimer.Enabled = false;
      _statusUiUpdateTimer.Stop();
      _statusUiUpdateTimer.Dispose();
      _statusUiUpdateTimer = null;
      base.OnSectionDeActivated();
    }

    private new void SaveSettings()
    {
      this.LogDebug("TV Movie import config: saving settings");
      ISettingService settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;
      settingServiceAgent.SaveValue(TvMovieImportSetting.DatabaseFile, textBoxDatabaseFile.Text);

      settingServiceAgent.SaveValue(TvMovieImportSetting.UpdateTimeFrequency, (int)numericUpDownUpdateTimeFrequency.Value);
      settingServiceAgent.SaveValue(TvMovieImportSetting.UpdateTimeBetweenStart, dateTimePickerUpdateTimeBetweenStart.Value);
      settingServiceAgent.SaveValue(TvMovieImportSetting.UpdateTimeBetweenEnd, dateTimePickerUpdateTimeBetweenEnd.Value);
      settingServiceAgent.SaveValue(TvMovieImportSetting.UpdateTimeOnStartup, radioButtonUpdateTimeStartup.Checked);

      settingServiceAgent.SaveValue(TvMovieImportSetting.UsePartialMatching, checkBoxMappingsPartialMatch.Checked);
    }

    private void DebugSettings()
    {
      this.LogDebug("TV Movie import config: settings");
      this.LogDebug("  database file        = {0}", textBoxDatabaseFile.Text);
      this.LogDebug("  update time...");
      this.LogDebug("    frequency          = {0} hour(s)", numericUpDownUpdateTimeFrequency.Value);
      this.LogDebug("    on startup/resume? = {0}", radioButtonUpdateTimeStartup.Checked);
      this.LogDebug("    between time start = {0}", dateTimePickerUpdateTimeBetweenStart.Value.TimeOfDay);
      this.LogDebug("    between time end   = {0}", dateTimePickerUpdateTimeBetweenEnd.Value.TimeOfDay);
      this.LogDebug("  partial matching?    = {0}", checkBoxMappingsPartialMatch.Checked);
    }

    private void OnStatusUiUpdateTimerTick(object sender, EventArgs e)
    {
      // Update the status UI. Take care to do it on the UI thread.
      if (!this.InvokeRequired)
      {
        UpdateImportStatusUi();
        return;
      }
      this.Invoke((MethodInvoker)delegate
      {
        UpdateImportStatusUi();
      });
    }

    private void UpdateImportStatusUi()
    {
      ServiceAgents.Instance.PluginService<ITvMovieImportService>().GetImportStatus(out _importStatusDateTime, out _importStatus, out _importStatusChannelCounts, out _importStatusProgramCounts);
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
        this.LogDebug("TV Movie import config: import status update...");
        this.LogDebug("  date/time = {0}", _importStatusDateTime);
        this.LogDebug("  status    = {0}", _importStatus);
        this.LogDebug("  channels  = {0}", _importStatusChannelCounts);
        this.LogDebug("  programs  = {0}", _importStatusProgramCounts);
      }
    }

    private static void FindBestMatchGuideChannel(
      Channel channel, bool enablePartialMatching,
      HashSet<string> guideChannelIds, IDictionary<string, string> matchingDictionary,
      out MatchType matchType, out string bestMatchId
    )
    {
      matchType = MatchType.None;
      bestMatchId = string.Empty;

      if (!string.IsNullOrEmpty(channel.ExternalId))
      {
        if (!TvMovieImportId.HasTvMovieMapping(channel.ExternalId))
        {
          matchType = MatchType.External;
          return;
        }

        if (guideChannelIds.Contains(channel.ExternalId))
        {
          matchType = MatchType.Mapped;
          bestMatchId = channel.ExternalId;
          return;
        }

        matchType = MatchType.Broken;
      }

      // Exact matching...
      if (matchingDictionary.TryGetValue(channel.Name, out bestMatchId))
      {
        if (matchType == MatchType.Broken)
        {
          matchType = MatchType.BrokenExact;
          return;
        }
        matchType = MatchType.Exact;
        return;
      }

      // Partial matching...
      if (enablePartialMatching)
      {
        // Find the best match...
        float bestSimilarity = 0.5f;
        foreach (KeyValuePair<string, string> match in matchingDictionary)
        {
          float similarity = Levenshtein.GetSimilarity(match.Key, channel.Name);
          if (similarity > bestSimilarity)
          {
            bestMatchId = match.Value;
            bestSimilarity = similarity;
            if (similarity == 1f)
            {
              break;
            }
          }
        }

        if (!string.IsNullOrEmpty(bestMatchId))
        {
          if (matchType == MatchType.Broken)
          {
            matchType = MatchType.BrokenPartial;
            return;
          }

          matchType = MatchType.Partial;
        }
      }
    }

    #region general tab

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        textBoxDatabaseFile.Text = openFileDialog.FileName;
      }
    }

    private void radioUpdateTimeBetween_CheckedChanged(object sender, EventArgs e)
    {
      dateTimePickerUpdateTimeBetweenStart.Enabled = radioButtonUpdateTimeBetween.Checked;
      dateTimePickerUpdateTimeBetweenEnd.Enabled = radioButtonUpdateTimeBetween.Checked;
    }

    private void dateTimePickerUpdateTimeBetweenStart_ValueChanged(object sender, EventArgs e)
    {
      if (dateTimePickerUpdateTimeBetweenStart.Value.TimeOfDay > dateTimePickerUpdateTimeBetweenEnd.Value.TimeOfDay)
      {
        dateTimePickerUpdateTimeBetweenStart.Value = dateTimePickerUpdateTimeBetweenEnd.Value;
      }
    }

    private void dateTimePickerUpdateTimeBetweenEnd_ValueChanged(object sender, EventArgs e)
    {
      if (dateTimePickerUpdateTimeBetweenStart.Value.TimeOfDay > dateTimePickerUpdateTimeBetweenEnd.Value.TimeOfDay)
      {
        dateTimePickerUpdateTimeBetweenEnd.Value = dateTimePickerUpdateTimeBetweenStart.Value;
      }
    }

    private void buttonImport_Click(object sender, EventArgs e)
    {
      this.LogDebug("TV Movie import config: force-starting import");
      SaveSettings();
      DebugSettings();
      ServiceAgents.Instance.PluginService<ITvMovieImportService>().ImportNow();
    }

    #endregion

    #region mappings tab

    private void buttonMappingsLoad_Click(object sender, EventArgs e)
    {
      ComboBoxChannelGroup channelGroup = comboBoxMappingsChannelGroup.SelectedItem as ComboBoxChannelGroup;
      if (channelGroup == null)
      {
        return;
      }

      try
      {
        this.LogDebug("TV Movie import config: loading mappings for channel group, ID = {0}, name = {1}, partial match = {2}", channelGroup.Id, channelGroup.Name, checkBoxMappingsPartialMatch.Checked);
        textBoxMappingsAction.Text = "loading";
        dataGridViewMappings.Rows.Clear();

        // Load the database channels.
        IList<Channel> databaseChannels = ServiceAgents.Instance.ChannelServiceAgent.ListAllVisibleChannelsByGroupId(channelGroup.Id, ChannelRelation.None);
        if (databaseChannels.Count == 0)
        {
          MessageBox.Show("There are no channels available to map.", MESSAGE_CAPTION);
          return;
        }

        // Load the guide channels from the server. The structure is:
        //    file name -> ID -> [display names]
        // Arrange the channels into 3 collections used for partial matching,
        // fast ID lookups, and display.
        IList<string> guideChannels = ServiceAgents.Instance.PluginService<ITvMovieImportService>().GetGuideChannelNames();
        HashSet<string> guideChannelIds = new HashSet<string>();
        IDictionary<string, string> matchingDictionary = new Dictionary<string, string>(200);
        IList<ComboBoxGuideChannel> guideChannelsForComboBox = new List<ComboBoxGuideChannel>(200);
        IDictionary<string, ComboBoxGuideChannel> comboBoxValueLookup = new Dictionary<string, ComboBoxGuideChannel>(200);
        ComboBoxGuideChannel gc = new ComboBoxGuideChannel(string.Empty, string.Empty);
        guideChannelsForComboBox.Add(gc);
        comboBoxValueLookup.Add(gc.Id, gc);
        foreach (string channelName in guideChannels)
        {
          string id = TvMovieImportId.GetQualifiedIdForChannel(channelName);
          this.LogDebug("TV Movie import config: guide channel, name = {0}", channelName);
          if (!guideChannelIds.Add(id))
          {
            this.LogWarn("TV Movie import config: found multiple channels with ID {0}, won't be able to distinguish which data to use", id);
          }

          if (!matchingDictionary.ContainsKey(channelName))
          {
            matchingDictionary.Add(channelName, id);
          }
          else
          {
            this.LogWarn("TV Movie import config: found multiple channels named {0}, might match to wrong channel", channelName);
          }

          gc = new ComboBoxGuideChannel(channelName, id);
          guideChannelsForComboBox.Add(gc);
          comboBoxValueLookup.Add(gc.Id, gc);
        }

        // Trust me - you don't want to mess with this! Data grid view combo
        // boxes are fragile.
        DataGridViewComboBoxColumn guideChannelColumn = (DataGridViewComboBoxColumn)dataGridViewMappings.Columns["dataGridViewColumnGuideChannel"];
        guideChannelColumn.DataSource = guideChannelsForComboBox;
        guideChannelColumn.ValueType = typeof(ComboBoxGuideChannel);
        guideChannelColumn.DisplayMember = "DisplayName";
        guideChannelColumn.ValueMember = "ValueMember";

        // Populate the grid.
        progressBarMappingsProgress.Minimum = 0;
        progressBarMappingsProgress.Maximum = databaseChannels.Count;
        progressBarMappingsProgress.Value = 0;
        dataGridViewColumnId.ValueType = typeof(int);
        dataGridViewMappings.Rows.Add(databaseChannels.Count);
        int rowIndex = 0;
        foreach (Channel channel in databaseChannels)
        {
          DataGridViewRow row = dataGridViewMappings.Rows[rowIndex++];
          row.Tag = channel;

          row.Cells["dataGridViewColumnId"].Value = channel.IdChannel;
          row.Cells["dataGridViewColumnTuningChannel"].Value = channel.Name;

          MatchType matchType = MatchType.None;
          string bestMatchId = string.Empty;
          FindBestMatchGuideChannel(channel, checkBoxMappingsPartialMatch.Checked, guideChannelIds, matchingDictionary, out matchType, out bestMatchId);
          this.LogDebug("TV Movie import config: DB channel, ID = {0}, name = {1}, external ID = {2}, match type = {3}, best match = {4}", channel.IdChannel, channel.Name, channel.ExternalId ?? string.Empty, matchType, bestMatchId);
          DataGridViewCell cell = row.Cells["dataGridViewColumnGuideChannel"];
          if (!string.IsNullOrEmpty(bestMatchId))
          {
            cell.Value = comboBoxValueLookup[bestMatchId];
          }
          cell.Tag = channel.ExternalId ?? string.Empty;

          // Note the mapping cell values are set so that the grid can be
          // sorted by mapping state without actually showing text in the
          // cells.
          cell = row.Cells["dataGridViewColumnMatchType"];
          cell.ToolTipText = matchType.GetDescription();
          cell.Value = string.Empty.PadRight((int)matchType, ' ');
          if (matchType == MatchType.Mapped)
          {
            cell.Style.BackColor = Color.White;
          }
          else if (matchType == MatchType.Exact)
          {
            cell.Style.BackColor = Color.MediumSeaGreen;
          }
          else if (matchType == MatchType.Partial)
          {
            cell.Style.BackColor = Color.Orange;
          }
          else if (matchType == MatchType.None)
          {
            cell.Style.BackColor = Color.Red;
          }
          else if (matchType == MatchType.BrokenExact)
          {
            cell.Style.BackColor = Color.LightGreen;
          }
          else if (matchType == MatchType.BrokenPartial)
          {
            cell.Style.BackColor = Color.NavajoWhite;
          }
          else if (matchType == MatchType.Broken)
          {
            cell.Style.BackColor = Color.LightPink;
          }
          else if (matchType == MatchType.External)
          {
            cell.Style.BackColor = Color.LightGray;
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
        this.LogError(ex, "TV Movie import config: failed to load channel group mappings, ID = {0}, name = {1}", channelGroup.Id, channelGroup.Name);
      }
    }

    private void buttonMappingsSave_Click(object sender, EventArgs e)
    {
      if (_loadedGroupName == null)
      {
        return;
      }
      this.LogDebug("TV Movie import config: saving mappings for channel group, ID = {0}, name = {1}", _loadedGroupId, _loadedGroupName);
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
          if (
            !string.Equals(guideChannel.Id, previousExternalId) &&
            // Don't touch non-TV-movie mappings unless the user has selected a guide channel.
            (
              string.IsNullOrEmpty(previousExternalId) ||
              TvMovieImportId.HasTvMovieMapping(previousExternalId) ||
              !string.IsNullOrEmpty(guideChannel.Id)
            )
          )
          {
            Channel channel = row.Tag as Channel;
            channel.ExternalId = guideChannel.Id;
            channels.Add(channel);
            this.LogDebug("TV Movie import config: mapped channel change, ID = {0}, name = {1}, old external ID = {2}, new external ID = {3}, guide name = {4}", channel.IdChannel, channel.Name, previousExternalId ?? "[null]", guideChannel.Id ?? "[null]", guideChannel.DisplayName);
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
        this.LogError(ex, "TV Movie import config: failed to save channel group mappings, ID = {0}, name = {1}", _loadedGroupId, _loadedGroupName);
      }
    }

    #endregion
  }
}