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
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using DbRecording = Mediaportal.TV.Server.TVDatabase.Entities.Recording;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Recording : SectionSettings
  {
    public Recording(ServerConfigurationChangedEventHandler handler)
      : base("Recording", handler)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("recording: activating");

      // storage tab
      textBoxRecordingFolder.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("recordingFolder", string.Empty);
      checkBoxDiskManagementEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("diskManagementEnable", false);
      numericUpDownDiskManagement.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("diskManagementReservedSpace", 1000);
      UpdateRecordingSpaceInfo();
      textBoxNamingTemplateSeries.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("recordingNamingTemplateSeries", @"%program_title%\%program_title% - %date%[ - S%series_number%E%episode_number%][ - %episode_name%]");
      textBoxNamingTemplateNonSeries.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("recordingNamingTemplateNonSeries", "%program_title% - %channel_name% - %date%");

      // general tab
      numericUpDownPreRecord.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("preRecordInterval", 7);
      numericUpDownPostRecord.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("postRecordInterval", 10);
      numericUpDownTunerLimit.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("recordTunerLimit", 100);
      checkBoxDuplicateDetectionSeriesEpisodeIdentifers.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("schedulerDuplicateDetectionSeriesEpisodeIdentifiers", true);
      checkBoxDuplicateDetectionSeasonEpisodeNumbers.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("schedulerDuplicateDetectionSeasonEpisodeNumbers", false);
      checkBoxDuplicateDetectionEpisodeNames.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("schedulerDuplicateDetectionEpisodeNames", false);
      comboBoxWeekEnd.SelectedIndex = ServiceAgents.Instance.SettingServiceAgent.GetValue("firstDayOfWeekEnd", (int)DayOfWeek.Saturday);

      // thumbnails
      checkBoxThumbnailerEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerEnabled", true);
      if (comboBoxThumbnailerQualitySpeed.Items.Count == 0)
      {
        comboBoxThumbnailerQualitySpeed.Items.AddRange(typeof(RecordingThumbnailQuality).GetDescriptions());
      }
      comboBoxThumbnailerQualitySpeed.SelectedItem = ((RecordingThumbnailQuality)ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerQuality", (int)RecordingThumbnailQuality.Highest)).GetDescription();
      numericUpDownThumbnailerTimeOffset.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerTimeOffset", 3);
      checkBoxThumbnailerCopyToRecordingFolder.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerCopyToRecordingFolder", false);
      numericUpDownThumbnailerColumnCount.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerColumnCount", 1);
      numericUpDownThumbnailerRowCount.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerRowCount", 1);

      DebugSettings();

      // database tab
      IList<DbRecording> allRecordings = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordings();
      try
      {
        listViewRecordings.BeginUpdate();
        listViewRecordings.Items.Clear();
        foreach (DbRecording recording in allRecordings)
        {
          ListViewItem item = new ListViewItem(recording.Title);
          item.Tag = recording;
          item.SubItems.Add(recording.Channel == null ? string.Empty : recording.Channel.Name);
          item.SubItems.Add(recording.StartTime.ToString());
          item.SubItems.Add(recording.SeasonNumber.ToString());
          item.SubItems.Add(recording.EpisodeNumber.ToString());
          item.SubItems.Add(recording.EpisodeName ?? string.Empty);
          item.SubItems.Add(recording.ProgramCategory == null ? string.Empty : recording.ProgramCategory.Category);
          RecordingKeepMethod keepMethod = (RecordingKeepMethod)recording.KeepMethod;
          if (keepMethod == RecordingKeepMethod.UntilDate)
          {
            item.SubItems.Add(recording.KeepUntilDate.ToString());
          }
          else
          {
            item.SubItems.Add(keepMethod.GetDescription().Replace("Until ", string.Empty));
          }
          item.SubItems.Add(recording.FileName);
          listViewRecordings.Items.Add(item);
        }
      }
      finally
      {
        listViewRecordings.EndUpdate();
      }
      listViewRecordings_SelectedIndexChanged(null, null);

      // debug
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          try
          {
            this.LogDebug("recordings: recordings...");
            foreach (DbRecording r in allRecordings)
            {
              this.LogDebug("  ID = {0, -4}, channel ID = {1, -4}, schedule ID = {1, -4}, type = {2}, start = {2}, end = {3}, title = {4, -30}, series/episode # = {5, -2}.{6, -2}.{7, -1}, episode name = {8, -30}, keep method = {9, -20}, keep until = {10}, series ID = {11, -20}, episode ID = {12, -20}, file name = {13}",
                r.IdRecording, r.IdChannel.GetValueOrDefault(-1), r.IdSchedule.GetValueOrDefault(-1), r.MediaType, r.StartTime, r.EndTime,
                r.Title, r.SeasonNumber, r.EpisodeNumber, r.EpisodePartNumber, r.EpisodeName, r.KeepMethod, r.KeepUntilDate.GetValueOrDefault(SqlDateTime.MinValue.Value),
                r.SeriesId ?? string.Empty, r.EpisodeId ?? string.Empty, r.FileName);
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
      this.LogDebug("recording: deactivating");

      // storage tab
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("recordingFolder", textBoxRecordingFolder.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diskManagementEnable", checkBoxDiskManagementEnable.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diskManagementReservedSpace", (int)numericUpDownDiskManagement.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("recordingNamingTemplateSeries", textBoxNamingTemplateSeries.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("recordingNamingTemplateNonSeries", textBoxNamingTemplateNonSeries.Text);

      // general tab
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("preRecordInterval", (int)numericUpDownPreRecord.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("postRecordInterval", (int)numericUpDownPostRecord.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("recordTunerLimit", (int)numericUpDownTunerLimit.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("schedulerDuplicateDetectionSeriesEpisodeIdentifiers", checkBoxDuplicateDetectionSeriesEpisodeIdentifers.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("schedulerDuplicateDetectionSeasonEpisodeNumbers", checkBoxDuplicateDetectionSeasonEpisodeNumbers.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("schedulerDuplicateDetectionEpisodeNames", checkBoxDuplicateDetectionEpisodeNames.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("firstDayOfWeekEnd", comboBoxWeekEnd.SelectedIndex);

      // thumbnails
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("thumbnailerEnabled", checkBoxThumbnailerEnable.Checked);
      int thumbnailerQuality = Convert.ToInt32(typeof(RecordingThumbnailQuality).GetEnumFromDescription((string)comboBoxThumbnailerQualitySpeed.SelectedItem));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("thumbnailerQuality", thumbnailerQuality);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("thumbnailerTimeOffset", (int)numericUpDownThumbnailerTimeOffset.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("thumbnailerCopyToRecordingFolder", checkBoxThumbnailerCopyToRecordingFolder.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("thumbnailerColumnCount", (int)numericUpDownThumbnailerColumnCount.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("thumbnailerRowCount", (int)numericUpDownThumbnailerRowCount.Value);

      DebugSettings();
      OnServerConfigurationChanged(this, true, null);

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  folder            = {0}", textBoxRecordingFolder.Text);
      this.LogDebug("  disk management...");
      this.LogDebug("    enabled?        = {0}", checkBoxDiskManagementEnable.Checked);
      this.LogDebug("    total           = {0}", labelDiskManagementSpaceTotalValue.Text);
      this.LogDebug("    free            = {0}", labelDiskManagementSpaceFreeValue.Text);
      this.LogDebug("    reserved        = {0} MB", numericUpDownDiskManagement.Value);
      this.LogDebug("  series naming     = {0}", textBoxNamingTemplateSeries.Text);
      this.LogDebug("  non-series naming = {0}", textBoxNamingTemplateNonSeries.Text);
      this.LogDebug("  pre-record        = {0} m", numericUpDownPreRecord.Value);
      this.LogDebug("  post-record       = {0} m", numericUpDownPostRecord.Value);
      this.LogDebug("  tuner limit       = {0}", numericUpDownTunerLimit.Value);
      this.LogDebug("  duplicate detection...");
      this.LogDebug("    series/ep. IDs? = {0}", checkBoxDuplicateDetectionSeriesEpisodeIdentifers.Checked);
      this.LogDebug("    season/ep. #s?  = {0}", checkBoxDuplicateDetectionSeasonEpisodeNumbers.Checked);
      this.LogDebug("    episode names?  = {0}", checkBoxDuplicateDetectionEpisodeNames.Checked);
      this.LogDebug("  week-end          = {0}", comboBoxWeekEnd.SelectedItem);
      this.LogDebug("  thumbnailer...");
      this.LogDebug("    enabled?        = {0}", checkBoxThumbnailerEnable.Checked);
      this.LogDebug("    quality/speed   = {0}", comboBoxThumbnailerQualitySpeed.SelectedItem);
      this.LogDebug("    time offset     = {0} m", numericUpDownThumbnailerTimeOffset.Value);
      this.LogDebug("    copy to folder? = {0}", checkBoxThumbnailerCopyToRecordingFolder.Checked);
      this.LogDebug("    column count    = {0}", numericUpDownThumbnailerColumnCount.Value);
      this.LogDebug("    row count       = {0}", numericUpDownThumbnailerRowCount.Value);
    }

    #region storage

    private void buttonRecordingFolderBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxRecordingFolder.Text;
      dlg.Description = "Select a folder for storing recordings.";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxRecordingFolder.Text = dlg.SelectedPath;
      }
    }

    private void textBoxNamingTemplate_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == (char)8 || e.KeyChar == '\\')  // allow back-space, and back-slash for specifying relative paths
      {
        return;
      }
      foreach (char c in Path.GetInvalidFileNameChars())
      {
        if (c == e.KeyChar)
        {
          e.Handled = true;
          return;
        }
      }
    }

    private void textBoxNamingTemplateSeries_Enter(object sender, EventArgs e)
    {
      DateTime start = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
      ProgramCategory category = new ProgramCategory();
      Program p = ProgramFactory.CreateProgram(0, start, start.AddMinutes(30), "The Big Bang Theory");
      category.Category = "Comedy";
      p.ProgramCategory = category;
      p.EpisodeName = "The Guitarist Amplification";
      p.SeasonNumber = 3;
      p.EpisodeNumber = 7;

      ToolTip tt = new ToolTip();
      tt.SetToolTip(textBoxNamingTemplateSeries, p.GetRecordingFileName(textBoxNamingTemplateSeries.Text, "CBS") + ".ts");
    }

    private void textBoxNamingTemplateNonSeries_Enter(object sender, EventArgs e)
    {
      DateTime start = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
      ProgramCategory category = new ProgramCategory();
      Program p = ProgramFactory.CreateProgram(0, start, start.AddHours(1).AddMinutes(30), "Philadelphia");
      category.Category = "Drama";
      p.ProgramCategory = category;

      ToolTip tt = new ToolTip();
      tt.SetToolTip(textBoxNamingTemplateNonSeries, p.GetRecordingFileName(textBoxNamingTemplateNonSeries.Text, "ProSieben") + ".ts");
    }

    #endregion

    #region disk management

    private void checkBoxDiskManagementEnable_CheckedChanged(object sender, EventArgs e)
    {
      numericUpDownDiskManagement.Enabled = checkBoxDiskManagementEnable.Checked;
    }

    private void UpdateRecordingSpaceInfo()
    {
      ulong bytesFree = 0;
      ulong bytesTotal = 0;
      if (!string.IsNullOrEmpty(textBoxRecordingFolder.Text))
      {
        bool result = ServiceAgents.Instance.ControllerServiceAgent.GetDriveSpaceInformation(
          Path.GetPathRoot(textBoxRecordingFolder.Text),
          out bytesTotal,
          out bytesFree);
        if (result)
        {
          labelDiskManagementSpaceTotalValue.Text = GetByteCountString(bytesTotal);
          labelDiskManagementSpaceFreeValue.Text = GetByteCountString(bytesFree);
          return;
        }
        this.LogWarn("recording: failed to get drive space information");
      }
      labelDiskManagementSpaceTotalValue.Text = "(information not available)";
      labelDiskManagementSpaceFreeValue.Text = "(information not available)";
    }

    private static string GetByteCountString(ulong byteCount)
    {
      if (byteCount < 1000)
      {
        return string.Format("{0} B", byteCount);
      }
      if (byteCount < 1000000)
      {
        return string.Format("{0} kB", Math.Round((decimal)byteCount / 1000, 2));
      }
      if (byteCount < 1000000000)
      {
        return string.Format("{0} MB", Math.Round((decimal)byteCount / 1000000, 2));
      }
      return string.Format("{0} GB", Math.Round((decimal)byteCount / 1000000000, 2));
    }

    #endregion

    #region thumbnails

    private void checkBoxThumbnailerEnable_CheckedChanged(object sender, EventArgs e)
    {
      labelThumbnailerQualitySpeed.Enabled = checkBoxThumbnailerEnable.Checked;
      comboBoxThumbnailerQualitySpeed.Enabled = checkBoxThumbnailerEnable.Checked;
      labelThumbnailerTimeOffset.Enabled = checkBoxThumbnailerEnable.Checked;
      numericUpDownThumbnailerTimeOffset.Enabled = checkBoxThumbnailerEnable.Checked;
      labelThumbnailerTimeOffsetUnit.Enabled = checkBoxThumbnailerEnable.Checked;
      checkBoxThumbnailerCopyToRecordingFolder.Enabled = checkBoxThumbnailerEnable.Checked;
      labelThumbnailerColumnCount.Enabled = checkBoxThumbnailerEnable.Checked;
      numericUpDownThumbnailerColumnCount.Enabled = checkBoxThumbnailerEnable.Checked;
      labelThumbnailerRowCount.Enabled = checkBoxThumbnailerEnable.Checked;
      numericUpDownThumbnailerRowCount.Enabled = checkBoxThumbnailerEnable.Checked;
    }

    private void buttonThumbnailerCreateMissing_Click(object sender, EventArgs e)
    {
      this.LogInfo("recordings: create missing thumbnails");
      ServiceAgents.Instance.ControllerServiceAgent.CreateMissingThumbnails();
    }

    private void buttonThumbnailerDeleteExisting_Click(object sender, EventArgs e)
    {
      this.LogInfo("recordings: delete existing thumbnails");
      ServiceAgents.Instance.ControllerServiceAgent.DeleteExistingThumbnails();
    }

    #endregion

    #region database

    private void listViewRecordings_SelectedIndexChanged(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewRecordings.SelectedItems;
      bool enableButtons = items != null && items.Count > 0;
      buttonRecordingChangeChannel.Enabled = enableButtons;
      buttonRecordingDelete.Enabled = enableButtons;
    }

    private void buttonRecordingChangeChannel_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewRecordings.SelectedItems;
      if (items == null || items.Count == 0)
      {
        return;
      }

      // Ensure the selected recordings all have the same media type.
      int mediaType = -1;
      List<DbRecording> recordings = new List<DbRecording>(items.Count);
      List<int> recordingIds = new List<int>(items.Count);
      foreach (ListViewItem item in items)
      {
        DbRecording recording = item.Tag as DbRecording;
        if (recording == null)
        {
          continue;
        }

        recordings.Add(recording);
        recordingIds.Add(recording.IdRecording);
        if (mediaType < 0)
        {
          mediaType = recording.MediaType;
          continue;
        }
        if (recording.MediaType != mediaType)
        {
          MessageBox.Show("Can't change the channel for TV and radio recordings at the same time.", MESSAGE_CAPTION, MessageBoxButtons.OK);
          return;
        }
      }
      if (recordings.Count == 0)
      {
        return;
      }

      // Select the new channel.
      Channel channel = null;
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllVisibleChannelsByMediaType((MediaType)mediaType, ChannelRelation.None);
      Channel[] channelsArray = new Channel[channels.Count];
      channels.CopyTo(channelsArray, 0);
      using (FormSelectItems dlgSelect = new FormSelectItems("Select Channel For Recording(s)", "Please select a channel:", channelsArray, "Name", false, null))
      {
        if (dlgSelect.ShowDialog() != DialogResult.OK || dlgSelect.Items == null || dlgSelect.Items.Count != 1)
        {
          return;
        }

        channel = dlgSelect.Items[0] as Channel;
        if (channel == null)
        {
          return;
        }
      }

      NotifyForm dlgNotify = null;
      try
      {
        if (items.Count > 10)
        {
          dlgNotify = new NotifyForm("Updating selected recordings...", "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
          dlgNotify.Show(this);
          dlgNotify.WaitForDisplay();
        }
        this.LogInfo("recording: change channel for recordings, channel ID = {0}, channel name = {1}, recordings = [{2}]", channel.IdChannel, channel.Name, string.Join(", ", recordingIds));

        // Update the channel reference for each recording.
        Dictionary<int, DbRecording> savedRecordings = new Dictionary<int, DbRecording>(recordings.Count);
        foreach (DbRecording recording in recordings)
        {
          recording.IdChannel = channel.IdChannel;
          ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(recording);
          savedRecordings.Add(recording.IdRecording, ServiceAgents.Instance.RecordingServiceAgent.GetRecording(recording.IdRecording));
        }

        // Update the recording list.
        listViewRecordings.BeginUpdate();
        try
        {
          foreach (ListViewItem item in items)
          {
            DbRecording recording = item.Tag as DbRecording;
            if (recording == null)
            {
              continue;
            }

            item.SubItems[1].Text = channel.Name;
            item.Tag = savedRecordings[recording.IdRecording];
          }
        }
        finally
        {
          listViewRecordings.EndUpdate();
        }
      }
      finally
      {
        if (dlgNotify != null)
        {
          dlgNotify.Close();
          dlgNotify.Dispose();
        }
      }
    }

    private void buttonRecordingDelete_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewRecordings.SelectedItems;
      if (items == null || items.Count == 0)
      {
        return;
      }

      if (MessageBox.Show(string.Format("Are you sure you want to delete the {0} selected recordings(s)?", items.Count), MESSAGE_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
      {
        return;
      }

      NotifyForm dlg = null;
      try
      {
        if (items.Count > 10)
        {
          dlg = new NotifyForm("Deleting selected recordings...", "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
          dlg.Show(this);
          dlg.WaitForDisplay();
        }

        List<ListViewItem> itemsToRemove = new List<ListViewItem>(items.Count);
        List<int> recordingIds = new List<int>(items.Count);
        foreach (ListViewItem item in items)
        {
          DbRecording recording = item.Tag as DbRecording;
          if (recording == null)
          {
            continue;
          }

          ServiceAgents.Instance.ControllerServiceAgent.DeleteRecording(recording.IdRecording);
          itemsToRemove.Add(item);
          recordingIds.Add(recording.IdRecording);
        }
        this.LogInfo("recording: delete, recordings = [{0}]", string.Join(", ", recordingIds));

        listViewRecordings.BeginUpdate();
        try
        {
          foreach (ListViewItem item in itemsToRemove)
          {
            listViewRecordings.Items.Remove(item);
          }
        }
        finally
        {
          listViewRecordings.EndUpdate();
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

    private void listViewRecordings_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        buttonRecordingDelete_Click(null, null);
        e.Handled = true;
      }
    }

    private void buttonImportBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxImportFolder.Text;
      dlg.Description = "Select a folder containing recordings you'd like to import.";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxImportFolder.Text = dlg.SelectedPath;
      }
    }

    private void buttonImport_Click(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(textBoxImportFolder.Text))
      {
        this.LogInfo("recording: import, folder = {0}", textBoxImportFolder.Text);
        ServiceAgents.Instance.ControllerServiceAgent.ImportRecordings(textBoxImportFolder.Text);
      }
    }

    #endregion
  }
}