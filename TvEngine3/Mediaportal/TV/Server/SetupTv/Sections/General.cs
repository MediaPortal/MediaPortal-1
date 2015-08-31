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
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib;
using Ionic.Zip;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using MediaPortal.Common.Utils.ExtensionMethods;

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

    #region constants

    private static readonly Guid MEDIA_SUB_TYPE_AVC = new Guid(0x31435641, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    private static readonly Guid MEDIA_SUB_TYPE_LATM_AAC = new Guid(0x00001ff, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    private static readonly Guid MEDIA_SUB_TYPE_DOLBY_DIGITAL = new Guid(0xe06d802c, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
    private static readonly Guid MEDIA_SUB_TYPE_DOLBY_DIGITAL_PLUS = new Guid(0xa7fb87af, 0x2d02, 0x42fb, 0xa4, 0xd4, 0x05, 0xcd, 0x93, 0x84, 0x3b, 0xdd);

    #endregion

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

        comboBoxPreviewCodecVideo.Items.Add(new Codec("Automatic", Guid.Empty));
        comboBoxPreviewCodecVideo.Items.AddRange(GetCodecs(MediaType.Video, new Guid[3] { MediaSubType.Mpeg2Video, MEDIA_SUB_TYPE_AVC, MediaSubType.H264 }));

        comboBoxPreviewCodecAudio.Items.Add(new Codec("Automatic", Guid.Empty));
        comboBoxPreviewCodecAudio.Items.AddRange(GetCodecs(MediaType.Audio, new Guid[4] { MediaSubType.Mpeg2Audio, MEDIA_SUB_TYPE_LATM_AAC, MEDIA_SUB_TYPE_DOLBY_DIGITAL, MEDIA_SUB_TYPE_DOLBY_DIGITAL_PLUS }));
      }

      comboBoxServicePriority.SelectedItem = ((ServicePriority)ServiceAgents.Instance.SettingServiceAgent.GetValue("servicePriority", (int)ServicePriority.Normal)).GetDescription();
      numericUpDownTunerDetectionDelay.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerDetectionDelay", 0);

      Codec codecVideo = Codec.Deserialise(ServiceAgents.Instance.SettingServiceAgent.GetValue("previewCodecVideo", Codec.DEFAULT_VIDEO.Serialise()));
      if (codecVideo == null)
      {
        comboBoxPreviewCodecVideo.SelectedIndex = 0;
      }
      else
      {
        comboBoxPreviewCodecVideo.SelectedItem = codecVideo;
      }

      Codec codecAudio = Codec.Deserialise(ServiceAgents.Instance.SettingServiceAgent.GetValue("previewCodecAudio", Codec.DEFAULT_AUDIO.Serialise()));
      if (codecAudio == null)
      {
        comboBoxPreviewCodecAudio.SelectedIndex = 0;
      }
      else
      {
        comboBoxPreviewCodecAudio.SelectedItem = codecAudio;
      }

      checkBoxScanChannelMovementDetection.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("channelMovementDetectionEnabled", false);
      checkBoxScanAutomaticChannelGroupsChannelProviders.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateProviderChannelGroups", false);
      checkBoxScanAutomaticChannelGroupsBroadcastStandards.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateBroadcastStandardChannelGroups", false);
      checkBoxScanAutomaticChannelGroupsSatellites.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAutoCreateSatelliteChannelGroups", false);
      numericUpDownTimeLimitScan.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitScan", 20000);

      numericUpDownTimeLimitSignalLock.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitSignalLock", 2500);
      numericUpDownTimeLimitReceiveStream.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitReceiveStream", 7500);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("general: deactivating");

      int servicePriority = Convert.ToInt32(typeof(ServicePriority).GetEnumFromDescription((string)comboBoxServicePriority.SelectedItem));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("servicePriority", servicePriority);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerDetectionDelay", (int)numericUpDownTunerDetectionDelay.Value);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("previewCodecVideo", ((Codec)comboBoxPreviewCodecVideo.SelectedItem).Serialise());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("previewCodecAudio", ((Codec)comboBoxPreviewCodecAudio.SelectedItem).Serialise());

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("channelMovementDetectionEnabled", checkBoxScanChannelMovementDetection.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAutoCreateProviderChannelGroups", checkBoxScanAutomaticChannelGroupsChannelProviders.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAutoCreateBroadcastStandardChannelGroups", checkBoxScanAutomaticChannelGroupsBroadcastStandards.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAutoCreateSatelliteChannelGroups", checkBoxScanAutomaticChannelGroupsSatellites.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitScan", (int)numericUpDownTimeLimitScan.Value);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitSignalLock", (int)numericUpDownTimeLimitSignalLock.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitReceiveStream", (int)numericUpDownTimeLimitReceiveStream.Value);

      DebugSettings();

      // TODO trigger server-side config reloading for service priority and time limits

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("general: settings...");
      this.LogDebug("  service priority      = {0}", comboBoxServicePriority.SelectedItem);
      this.LogDebug("  tuner detection delay = {0} ms", numericUpDownTunerDetectionDelay.Value);
      this.LogDebug("  preview codecs...");
      Codec c = (Codec)comboBoxPreviewCodecVideo.SelectedItem;
      this.LogDebug("    video               = {0} ({1})", c.Name, c.ClassId);
      c = (Codec)comboBoxPreviewCodecAudio.SelectedItem;
      this.LogDebug("    audio               = {0} ({1})", c.Name, c.ClassId);
      this.LogDebug("  channel movement?     = {0}", checkBoxScanChannelMovementDetection.Checked);
      this.LogDebug("  automatic channel groups...");
      this.LogDebug("    providers           = {0}", checkBoxScanAutomaticChannelGroupsChannelProviders.Checked);
      this.LogDebug("    broadcast standards = {0}", checkBoxScanAutomaticChannelGroupsBroadcastStandards.Checked);
      this.LogDebug("    satellites          = {0}", checkBoxScanAutomaticChannelGroupsSatellites.Checked);
      this.LogDebug("  time limits...");
      this.LogDebug("    scan                = {0} ms", numericUpDownTimeLimitScan.Value);
      this.LogDebug("    signal lock         = {0} ms", numericUpDownTimeLimitSignalLock.Value);
      this.LogDebug("    receive stream      = {0} ms", numericUpDownTimeLimitReceiveStream.Value);
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

    private static Codec[] GetCodecs(Guid mediaType, Guid[] mediaSubTypes)
    {
      List<Codec> codecs = new List<Codec>();
      Guid[] types = new Guid[mediaSubTypes.Length * 2];
      for (int i = 0; i < mediaSubTypes.Length; i++)
      {
        types[i * 2] = mediaType;
        types[(i * 2) + 1] = mediaSubTypes[i];
      }

      IFilterMapper2 mapper = null;
      try
      {
        mapper = (IFilterMapper2)new FilterMapper2();
        if (mapper == null)
        {
          return codecs.ToArray();
        }

        IEnumMoniker enumMoniker = null;
        int hr = mapper.EnumMatchingFilters(
          out enumMoniker,
          0,                // flags - must be 0
          true,             // exact match
          (Merit)0x080001,  // merit - match MediaPortal
          true,             // input needed
          mediaSubTypes.Length,
          types,
          null,             // input pin medium
          null,             // input pin category
          false,            // render
          true,             // output needed
          0,                // output pin media type count
          new Guid[0],      // output pin media types
          null,             // output pin medium
          null);            // output pin category

        TvExceptionDirectShowError.Throw(hr, "Failed to enumerate filters.");
        try
        {
          while (true)
          {
            IMoniker[] monikers = new IMoniker[1];
            hr = enumMoniker.Next(1, monikers, IntPtr.Zero);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              break;
            }

            if (monikers[0] == null)
            {
              continue;
            }
            using (DsDevice d = new DsDevice(monikers[0]))
            {
              string name = d.Name;
              if (!string.IsNullOrEmpty(name))
              {
                string lowerName = name.ToLowerInvariant();
                if (!lowerName.Contains("encoder") && !lowerName.Contains("muxer"))
                {
                  codecs.Add(new Codec(name, d.ClassID));
                }
              }
              d.Dispose();
            }
          }
        }
        finally
        {
          Release.ComObject("moniker enumerator", ref enumMoniker);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "general: failed to get codec list, media type = {0}", mediaType);
      }
      finally
      {
        Release.ComObject("filter mapper", ref mapper);
      }

      codecs.Sort();
      return codecs.ToArray();
    }
  }
}