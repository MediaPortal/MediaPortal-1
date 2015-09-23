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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardAnalog : SectionSettings
  {
    private const string SCAN_MODE_CAPTURE = "External Inputs";
    private const string SCAN_MODE_EXTERNAL_TUNER = "External Tuner";   // set top box (STB) connected to configured capture input
    private const string SCAN_MODE_FM_RADIO = "FM Radio";
    private const string SCAN_MODE_TV_CABLE = "Cable TV";
    private const string SCAN_MODE_TV_TERRESTRIAL = "Terrestrial TV";

    private readonly int _tunerId;
    private ChannelScanHelper _scanHelper = null;

    public CardAnalog(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
      base.Text = name;
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("analog: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxCountry.Items.Count == 0)
      {
        comboBoxCountry.Items.AddRange(CountryCollection.Instance.Countries);
      }

      Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerIncludeRelationEnum.None);

      BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
      comboBoxScanMode.BeginUpdate();
      try
      {
        comboBoxScanMode.Items.Clear();
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
        {
          comboBoxScanMode.Items.Add(SCAN_MODE_CAPTURE);
          comboBoxScanMode.Items.Add(SCAN_MODE_EXTERNAL_TUNER);
        }
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.FmRadio))
        {
          comboBoxScanMode.Items.Add(SCAN_MODE_FM_RADIO);
        }
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision))
        {
          if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
          {
            comboBoxScanMode.Items.Add(SCAN_MODE_EXTERNAL_TUNER);     // Support input via RF/coax.
          }
          comboBoxScanMode.Items.Add(SCAN_MODE_TV_CABLE);
          comboBoxScanMode.Items.Add(SCAN_MODE_TV_TERRESTRIAL);
        }
      }
      finally
      {
        comboBoxScanMode.EndUpdate();
      }

      comboBoxScanMode.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("analog" + _tunerId + "ScanMode", SCAN_MODE_CAPTURE);
      if (comboBoxScanMode.SelectedItem == null)
      {
        comboBoxScanMode.SelectedIndex = 0;
      }
      string countryName = ServiceAgents.Instance.SettingServiceAgent.GetValue("analog" + _tunerId + "Country", System.Globalization.RegionInfo.CurrentRegion.EnglishName);
      Country country = CountryCollection.Instance.GetCountryByName(countryName);
      if (country == null)
      {
        comboBoxCountry.SelectedIndex = 0;
      }
      else
      {
        comboBoxCountry.SelectedItem = country;
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("analog: deactivating, tuner ID = {0}", _tunerId);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("analog" + _tunerId + "ScanMode", (string)comboBoxScanMode.SelectedItem);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("analog" + _tunerId + "Country", ((Country)comboBoxCountry.SelectedItem).Name);
      base.OnSectionDeActivated();
    }

    private void buttonScan_Click(object sender, EventArgs e)
    {
      if (_scanHelper != null)
      {
        buttonScan.Text = "Cancelling...";
        _scanHelper.StopScan();
        return;
      }

      List<FileTuningDetail> tuningDetails = null;
      string scanMode = (string)comboBoxScanMode.SelectedItem;
      if (string.Equals(scanMode, SCAN_MODE_CAPTURE))
      {
        tuningDetails = new List<FileTuningDetail>
        {
          new FileTuningDetail
          {
            BroadcastStandard = BroadcastStandard.ExternalInput
          }
        };
      }
      else if (string.Equals(scanMode, SCAN_MODE_EXTERNAL_TUNER))
      {
        ImportExternalTunerChannelList();
        return;
      }
      else
      {
        TuningDetailFilter filter = new TuningDetailFilter("analog");
        if (string.Equals(scanMode, SCAN_MODE_FM_RADIO))
        {
          tuningDetails = filter.LoadList("FM Radio.xml", false);
        }
        else if (string.Equals(scanMode, SCAN_MODE_TV_CABLE) || string.Equals(scanMode, SCAN_MODE_TV_TERRESTRIAL))
        {
          AnalogTunerSource source = AnalogTunerSource.Cable;
          if (string.Equals(scanMode, SCAN_MODE_TV_TERRESTRIAL))
          {
            source = AnalogTunerSource.Antenna;
          }
          tuningDetails = filter.LoadList("TV.xml", false);
          foreach (FileTuningDetail td in tuningDetails)
          {
            td.Country = (Country)comboBoxCountry.SelectedItem;
            td.TunerSource = source;
          }
        }
      }
      if (tuningDetails == null)
      {
        return;
      }

      listViewProgress.Items.Clear();
      ListViewItem item = listViewProgress.Items.Add(string.Format("start scanning {0}...", scanMode));
      item.EnsureVisible();
      this.LogInfo("analog: start scanning {0}...", scanMode);

      _scanHelper = new ChannelScanHelper(_tunerId);
      if (_scanHelper.StartScan(tuningDetails, listViewProgress, progressBarProgress, OnGetDbExistingTuningDetailCandidates, null, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality))
      {
        comboBoxScanMode.Enabled = false;
        comboBoxCountry.Enabled = false;
        buttonScan.Text = "Cancel...";
      }
    }

    private IList<DbTuningDetail> OnGetDbExistingTuningDetailCandidates(FileTuningDetail tuningDetail, IChannel tuneChannel, IChannel foundChannel, bool useChannelMovementDetection)
    {
      ChannelCapture captureChannel = foundChannel as ChannelCapture;
      if (captureChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetCaptureTuningDetails(foundChannel.Name);
      }
      ChannelFmRadio fmRadioChannel = foundChannel as ChannelFmRadio;
      if (fmRadioChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetFmRadioTuningDetails(fmRadioChannel.Frequency);
      }
      ChannelAnalogTv analogTvChannel = foundChannel as ChannelAnalogTv;
      if (analogTvChannel != null)
      {
        return ServiceAgents.Instance.ChannelServiceAgent.GetAnalogTelevisionTuningDetails(analogTvChannel.PhysicalChannelNumber);
      }
      return null;
    }

    private void OnScanCompleted()
    {
      this.Invoke((MethodInvoker)delegate
      {
        buttonScan.Text = "Scan for channels";
        comboBoxScanMode.Enabled = true;
        comboBoxCountry.Enabled = true;
      });
      _scanHelper = null;
    }

    private void comboBoxScanMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool countryVisible = string.Equals((string)comboBoxScanMode.SelectedItem, SCAN_MODE_TV_CABLE) || string.Equals((string)comboBoxScanMode.SelectedItem, SCAN_MODE_TV_TERRESTRIAL);
      labelCountry.Visible = countryVisible;
      comboBoxCountry.Visible = countryVisible;
      if (string.Equals((string)comboBoxScanMode.SelectedItem, SCAN_MODE_EXTERNAL_TUNER))
      {
        buttonScan.Text = "Import channels";
      }
      else
      {
        buttonScan.Text = "Scan for channels";
      }
    }

    private void ImportExternalTunerChannelList()
    {
      if (openFileDialogExternalTunerChannelList.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      string fileName = openFileDialogExternalTunerChannelList.FileName;
      this.LogInfo("analog: import external tuner channel list, file name = {0}", fileName);
      try
      {
        List<IChannel> newChannels = new List<IChannel>();
        List<IChannel> updatedChannels = new List<IChannel>();

        string line;
        using (StreamReader file = new StreamReader(fileName))
        {
          while ((line = file.ReadLine()) != null)
          {
            Match m = Regex.Match(line, @"^\s*(\d+)\s*,\s*([^\s].*?)\s*$");
            if (!m.Success)
            {
              continue;
            }

            ChannelCapture channel = new ChannelCapture();
            channel.Name = m.Groups[2].Captures[0].Value;
            channel.MediaType = MediaType.Television;
            channel.LogicalChannelNumber = m.Groups[1].Captures[0].Value;
            channel.Provider = "External Tuner";
            channel.VideoSource = CaptureSourceVideo.TunerDefault;
            channel.AudioSource = CaptureSourceAudio.TunerDefault;
            channel.IsEncrypted = false;
            channel.IsHighDefinition = false;
            channel.IsThreeDimensional = false;
            channel.IsVcrSignal = false;

            IList<DbTuningDetail> possibleTuningDetails = OnGetDbExistingTuningDetailCandidates(null, null, channel, false);
            DbTuningDetail dbTuningDetail = null;
            if (possibleTuningDetails != null)
            {
              if (possibleTuningDetails.Count == 1)
              {
                dbTuningDetail = possibleTuningDetails[0];
              }
              else
              {
                foreach (TuningDetail td in possibleTuningDetails)
                {
                  if (string.Equals(td.LogicalChannelNumber, channel.LogicalChannelNumber))
                  {
                    dbTuningDetail = td;
                    break;
                  }
                }
              }
            }

            if (dbTuningDetail == null)
            {
              ChannelScanHelper.AddChannel(channel);
              newChannels.Add(channel);
            }
            else
            {
              ChannelScanHelper.UpdateChannel(channel, dbTuningDetail);
              updatedChannels.Add(channel);
            }
          }

          file.Close();
        }

        this.LogInfo("analog: import summary...");
        listViewProgress.BeginUpdate();
        try
        {
          listViewProgress.Items.Add("import summary...");
          if (newChannels.Count == 0 && updatedChannels.Count == 0)
          {
            listViewProgress.Items.Add("  no channels found");
            this.LogInfo("  no channels found");
          }
          else
          {
            line = string.Format("  updated, count = {0}", updatedChannels.Count);
            this.LogInfo(line);
            listViewProgress.Items.Add(line);
            foreach (IChannel c in updatedChannels)
            {
              this.LogDebug("    {0}", c);
              listViewProgress.Items.Add(new ListViewItem(string.Format("    {0}", c.Name)));
            }
            line = string.Format("  new, count = {0}", newChannels.Count);
            this.LogInfo(line);
            ListViewItem lastItem = listViewProgress.Items.Add(line);
            foreach (IChannel c in newChannels)
            {
              this.LogDebug("    {0}", c);
              lastItem = listViewProgress.Items.Add(new ListViewItem(string.Format("    {0}", c.Name)));
            }
            lastItem.EnsureVisible();
          }
        }
        finally
        {
          listViewProgress.EndUpdate();
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "analog: unexpected import exception");
        listViewProgress.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = listViewProgress.Items.Add(new ListViewItem("Unexpected error. Please create a report in our forum."));
          item.ForeColor = System.Drawing.Color.Red;
          item.EnsureVisible();
        });
        MessageBox.Show("Encountered unexpected error. Please create a report in our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}