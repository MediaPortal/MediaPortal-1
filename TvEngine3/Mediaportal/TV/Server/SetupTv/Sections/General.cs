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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Ionic.Zip;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class General : SectionSettings
  {
    private enum ServicePriority
    {
      [Description("Real-time")]
      RealTime = (int)ProcessPriorityClass.RealTime,
      [Description("High")]
      High = (int)ProcessPriorityClass.High,
      [Description("Above Normal")]
      AboveNormal = (int)ProcessPriorityClass.AboveNormal,
      [Description("Normal")]
      Normal = (int)ProcessPriorityClass.Normal,
      [Description("Below Normal")]
      BelowNormal = (int)ProcessPriorityClass.BelowNormal,
      [Description("Idle")]
      Idle = (int)ProcessPriorityClass.Idle
    }

    private class FileDownloader : WebClient
    {
      public int TimeOut = 60000;

      public FileDownloader(int timeOutMilliseconds = 60000)
      {
        TimeOut = timeOutMilliseconds;
      }

      protected override WebRequest GetWebRequest(Uri address)
      {
        var result = base.GetWebRequest(address);
        if (result != null)
        {
          result.Timeout = TimeOut;
        }
        return result;
      }
    }

    private FileDownloader _downloader = null;
    private string _fileNameTuningDetails = null;

    public General()
      : this("General")
    {
    }

    public General(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("general: activating");

      // First activation.
      if (comboBoxServicePriority.Items.Count == 0)
      {
        comboBoxServicePriority.Items.AddRange(typeof(ServicePriority).GetDescriptions());
      }

      comboBoxServicePriority.SelectedItem = ((ServicePriority)ServiceAgents.Instance.SettingServiceAgent.GetValue("servicePriority", (int)ServicePriority.Normal)).GetDescription();
      numericUpDownTunerDetectionDelay.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerDetectionDelay", 0);

      numericUpDownTimeLimitSignal.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitSignalLock", 2500);
      numericUpDownTimeLimitScan.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitScan", 20000);

      checkBoxScanChannelMovementDetection.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("channelMovementDetectionEnabled", false);
      checkBoxScanAutomaticChannelGroupsChannelProviders.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateProviderChannelGroups", false);
      checkBoxScanAutomaticChannelGroupsBroadcastStandards.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateBroadcastStandardChannelGroups", false);
      checkBoxScanAutomaticChannelGroupsSatellites.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateSatelliteChannelGroups", false);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("general: deactivating");

      int servicePriority = Convert.ToInt32(typeof(ServicePriority).GetEnumFromDescription((string)comboBoxServicePriority.SelectedItem));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("servicePriority", servicePriority);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerDetectionDelay", (int)numericUpDownTunerDetectionDelay.Value);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitSignalLock", (int)numericUpDownTimeLimitSignal.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitScan", (int)numericUpDownTimeLimitScan.Value);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("channelMovementDetectionEnabled", checkBoxScanChannelMovementDetection.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAutoCreateProviderChannelGroups", checkBoxScanAutomaticChannelGroupsChannelProviders.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAutoCreateBroadcastStandardChannelGroups", checkBoxScanAutomaticChannelGroupsBroadcastStandards.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAutoCreateSatelliteChannelGroups", checkBoxScanAutomaticChannelGroupsSatellites.Checked);

      DebugSettings();

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("general: settings...");
      this.LogDebug("  service priority      = {0}", comboBoxServicePriority.SelectedItem);
      this.LogDebug("  tuner detection delay = {0} ms", numericUpDownTunerDetectionDelay.Value);
      this.LogDebug("  signal time limit     = {0} ms", numericUpDownTimeLimitSignal.Value);
      this.LogDebug("  channel movement?     = {0}", checkBoxScanChannelMovementDetection.Checked);
      this.LogDebug("  automatic channel groups...");
      this.LogDebug("    providers           = {0}", checkBoxScanAutomaticChannelGroupsChannelProviders.Checked);
      this.LogDebug("    broadcast standards = {0}", checkBoxScanAutomaticChannelGroupsBroadcastStandards.Checked);
      this.LogDebug("    satellites          = {0}", checkBoxScanAutomaticChannelGroupsSatellites.Checked);
      this.LogDebug("  scan time limit       = {0} ms", numericUpDownTimeLimitScan.Value);
    }

    private void buttonUpdateTuningDetails_Click(object sender, EventArgs e)
    {
      if (_downloader != null)
      {
        this.LogDebug("general: cancel tuning detail update");
        buttonUpdateTuningDetails.Text = "Cancelling...";
        _downloader.CancelAsync();
        return;
      }

      this.LogDebug("general: backup tuning details");
      string tuningDetailRootPath = TuningDetailFilter.GetDataPath();
      string fileNameBackupTemp = Path.Combine(tuningDetailRootPath, "backup_new.zip");
      try
      {
        ZipFile zipFile = new ZipFile(fileNameBackupTemp, System.Text.Encoding.UTF8);
        try
        {
          foreach (string directory in Directory.GetDirectories(tuningDetailRootPath))
          {
            zipFile.AddDirectory(directory, Path.GetFileName(directory));
          }
          zipFile.Save();
        }
        finally
        {
          zipFile.Dispose();
        }

        string fileNameBackupFinal = Path.Combine(tuningDetailRootPath, "backup.zip");
        if (File.Exists(fileNameBackupFinal))
        {
          File.Delete(fileNameBackupFinal);
        }
        File.Move(fileNameBackupTemp, fileNameBackupFinal);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "general: failed to backup tuning details before update");
        MessageBox.Show(string.Format("Backup failed. {0}", SENTENCE_CHECK_LOG_FILES), MESSAGE_CAPTION, MessageBoxButtons.OK);
        return;
      }

      this.LogDebug("general: tuning detail backup successful, starting download");
      buttonUpdateTuningDetails.Text = "Cancel...";
      _fileNameTuningDetails = Path.Combine(tuningDetailRootPath, "tuning_details.zip");
      using (_downloader = new FileDownloader(120000))
      {
        _downloader.Proxy.Credentials = CredentialCache.DefaultCredentials;
        _downloader.DownloadFileCompleted += OnTuningDetailDownloadCompleted;
        _downloader.DownloadFileAsync(new Uri("http://install.team-mediaportal.com/tvsetup/TVE_3.5/tuning_details.zip"), _fileNameTuningDetails);
      }
    }

    private void OnTuningDetailDownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Cancelled)
      {
        this.LogDebug("general: tuning detail download cancelled");
      }
      else if (e.Error != null)
      {
        this.LogError(e.Error, "general: failed to download tuning details");
        MessageBox.Show(string.Format("Download failed. {0}", SENTENCE_CHECK_LOG_FILES), MESSAGE_CAPTION, MessageBoxButtons.OK);
      }
      else
      {
        this.LogDebug("general: tuning detail download successful, extracting");
        try
        {
          foreach (string directory in Directory.GetDirectories(Path.GetDirectoryName(_fileNameTuningDetails)))
          {
            Directory.Delete(directory, true);
          }

          ZipFile zipFile = new ZipFile(_fileNameTuningDetails, System.Text.Encoding.UTF8);
          try
          {
            zipFile.ExtractAll(Path.GetDirectoryName(_fileNameTuningDetails), ExtractExistingFileAction.OverwriteSilently);
          }
          finally
          {
            zipFile.Dispose();
          }
          this.LogDebug("general: tuning details update successful");
        }
        catch (Exception ex)
        {
          this.LogError(ex, "general: failed to extract and update tuning details");
          MessageBox.Show(string.Format("Extract and update failed. {0}", SENTENCE_CHECK_LOG_FILES), MESSAGE_CAPTION, MessageBoxButtons.OK);
        }
      }

      _downloader = null;
      buttonUpdateTuningDetails.Text = "Update tuning details";
    }
  }
}