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
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Channel;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using DbTuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;
using FileTuningDetail = Mediaportal.TV.Server.SetupTV.Sections.Helpers.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class ScanAnalog : SectionSettings
  {
    #region constants

    public const BroadcastStandard SUPPORTED_BROADCAST_STANDARDS = BroadcastStandard.AnalogTelevision | BroadcastStandard.AmRadio | BroadcastStandard.FmRadio | BroadcastStandard.ExternalInput;

    private const string SCAN_MODE_AM_RADIO = "AM Radio";
    private const string SCAN_MODE_CAPTURE = "External Inputs";
    private const string SCAN_MODE_EXTERNAL_TUNER = "External Tuner";   // set top box (STB) connected to configured capture input
    private const string SCAN_MODE_FM_RADIO = "FM Radio";
    private const string SCAN_MODE_TV_CABLE = "Cable TV";
    private const string SCAN_MODE_TV_TERRESTRIAL = "Terrestrial TV";

    // Countries in ITU region 2 use a different frequency range for AM radio.
    private static readonly List<string> AM_RADIO_AMERICAN_COUNTRIES = new List<string>
    {
      // .NET names for countries in this list:
      // http://life.itu.int/radioclub/rr/itureg.htm
      "Argentina",
      "Belize",
      "Bolivia",
      "Brazil",
      "Canada",
      "Chile",
      "Colombia",
      "Costa Rica",
      "Dominican Republic",
      "Ecuador",
      "El Salvador",
      "Guatemala",
      "Honduras",
      "Jamaica",
      "Mexico",
      "Nicaragua",
      "Panama",
      "Paraguay",
      "Peru",
      "Trinidad and Tobago",
      "United States",
      "Uruguay",
      "Bolivarian Republic of Venezuela",

      // Countries from the ITU list that aren't supported by .NET.
      "Antigua and Barbuda",
      "Bahamas",
      "Barbados",
      "Cuba",
      "Dominica",   // Commonwealth of Dominica
      "Grenada",
      "Guyana",
      "Haiti",
      "St Vincent and the Grenadines",
      "Suriname",

      // Countries in North and South America that aren't ITU members, but that
      // we assume would use the region 2 AM radio frequency range.
      // https://www.countries-ofthe-world.com/countries-of-north-america.html
      // https://www.countries-ofthe-world.com/countries-of-south-america.html
      "Saint Kitts and Nevis",
      "Saint Lucia"
    };

    #endregion

    private readonly int _tunerId;
    private ChannelScanHelper _scanHelper = null;

    public ScanAnalog(string name, int tunerId)
      : base(name)
    {
      _tunerId = tunerId;
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("scan analog: activating, tuner ID = {0}", _tunerId);

      // First activation.
      if (comboBoxCountry.Items.Count == 0)
      {
        comboBoxCountry.Items.AddRange(CountryCollection.Instance.Countries);
      }

      Tuner tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(_tunerId, TunerRelation.AnalogTunerSettings);
      BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
      this.LogDebug("  standard(s) = [{0}]", tunerSupportedBroadcastStandards);

      if (_scanHelper != null)
      {
        DebugSettings();
        base.OnSectionActivated();
        return;
      }

      comboBoxScanMode.BeginUpdate();
      try
      {
        comboBoxScanMode.Items.Clear();
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
        {
          if (
            (
              tuner.AnalogTunerSettings.SupportedVideoSources != (int)CaptureSourceVideo.None &&
              tuner.AnalogTunerSettings.SupportedVideoSources != (int)CaptureSourceVideo.Tuner
            ) ||
            (
              tuner.AnalogTunerSettings.SupportedAudioSources != (int)CaptureSourceAudio.None &&
              tuner.AnalogTunerSettings.SupportedAudioSources != (int)CaptureSourceAudio.Tuner
            )
          )
          {
            comboBoxScanMode.Items.Add(SCAN_MODE_CAPTURE);
          }
          comboBoxScanMode.Items.Add(SCAN_MODE_EXTERNAL_TUNER);
        }
        if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AmRadio))
        {
          comboBoxScanMode.Items.Add(SCAN_MODE_AM_RADIO);
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

      comboBoxScanMode.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAnalog" + _tunerId + "ScanMode", SCAN_MODE_CAPTURE);
      if (comboBoxScanMode.SelectedItem == null)
      {
        comboBoxScanMode.SelectedIndex = 0;
      }

      string countryName = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAnalog" + _tunerId + "Country", RegionInfo.CurrentRegion.EnglishName);
      Country country = CountryCollection.Instance.GetCountryByName(countryName);
      if (country == null)
      {
        comboBoxCountry.SelectedIndex = 0;
      }
      else
      {
        comboBoxCountry.SelectedItem = country;
      }

      string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("scanAnalog" + _tunerId + "Transmitter", TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
      foreach (object item in comboBoxTransmitter.Items)
      {
        if (string.Equals(item.ToString(), transmitter))
        {
          comboBoxTransmitter.SelectedItem = item;
          break;
        }
      }
      if (comboBoxTransmitter.SelectedItem == null)
      {
        comboBoxTransmitter.SelectedIndex = 0;
      }

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("scan analog: deactivating, tuner ID = {0}", _tunerId);

      DebugSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAnalog" + _tunerId + "ScanMode", (string)comboBoxScanMode.SelectedItem);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAnalog" + _tunerId + "Country", ((Country)comboBoxCountry.SelectedItem).Name);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("scanAnalog" + _tunerId + "Transmitter", comboBoxTransmitter.SelectedItem.ToString());

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  scan mode   = {0}", comboBoxScanMode.SelectedItem);
      this.LogDebug("  country     = {0}", comboBoxCountry.SelectedItem);
      this.LogDebug("  transmitter = {0}", comboBoxTransmitter.SelectedItem);
    }

    private void buttonScan_Click(object sender, EventArgs e)
    {
      if (_scanHelper != null)
      {
        buttonScan.Enabled = false;
        buttonScan.Text = "Stopping...";
        _scanHelper.StopScan();
        return;
      }

      IList<FileTuningDetail> tuningDetails = null;
      string scanMode = (string)comboBoxScanMode.SelectedItem;
      Country country = (Country)comboBoxCountry.SelectedItem;
      if (string.Equals(scanMode, SCAN_MODE_CAPTURE))
      {
        tuningDetails = new List<FileTuningDetail>(1)
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
        AnalogTunerSource source = AnalogTunerSource.Antenna;
        if (string.Equals(scanMode, SCAN_MODE_TV_CABLE))
        {
          source = AnalogTunerSource.Cable;
        }
        tuningDetails = TuningDetailFilter.GetTuningDetails(comboBoxTransmitter);
        if (tuningDetails == null || tuningDetails.Count == 0)
        {
          return;
        }
        foreach (FileTuningDetail tuningDetail in tuningDetails)
        {
          tuningDetail.Country = country;
          tuningDetail.TunerSource = source;
        }
      }

      listViewProgress.Items.Clear();
      ListViewItem item = listViewProgress.Items.Add(string.Format("start scanning {0}...", scanMode));
      item.EnsureVisible();
      this.LogInfo("scan analog: start scanning, mode = {0}, country = {1}", scanMode, country);

      _scanHelper = new ChannelScanHelper(_tunerId, listViewProgress, progressBarProgress, null, OnScanCompleted, progressBarSignalStrength, progressBarSignalQuality);
      if (_scanHelper.StartScan(tuningDetails))
      {
        comboBoxScanMode.Enabled = false;
        comboBoxCountry.Enabled = false;
        comboBoxTransmitter.Enabled = false;
        buttonScan.Text = "&Stop";
      }
    }

    private void OnScanCompleted()
    {
      this.Invoke((MethodInvoker)delegate
      {
        comboBoxScanMode.Enabled = true;
        string scanMode = (string)comboBoxScanMode.SelectedItem;
        if (string.Equals(scanMode, SCAN_MODE_TV_CABLE) || string.Equals(scanMode, SCAN_MODE_TV_TERRESTRIAL))
        {
          comboBoxCountry.Enabled = true;
          comboBoxTransmitter.Enabled = true;
        }
        else
        {
          comboBoxCountry.Enabled = false;
          if (string.Equals(scanMode, SCAN_MODE_AM_RADIO) || string.Equals(scanMode, SCAN_MODE_FM_RADIO))
          {
            comboBoxTransmitter.Enabled = true;
          }
          else
          {
            comboBoxTransmitter.Enabled = false;
          }
        }
        buttonScan.Enabled = true;
      });
      _scanHelper = null;
    }

    private void comboBoxScanMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      string scanMode = (string)comboBoxScanMode.SelectedItem;
      string fileName = null;

      if (string.Equals(scanMode, SCAN_MODE_TV_CABLE) || string.Equals(scanMode, SCAN_MODE_TV_TERRESTRIAL))
      {
        comboBoxCountry.Enabled = true;
        fileName = "TV.xml";
      }
      else
      {
        comboBoxCountry.Enabled = false;
        string countryName = RegionInfo.CurrentRegion.EnglishName;
        if (string.Equals(scanMode, SCAN_MODE_AM_RADIO))
        {
          if (countryName != null && AM_RADIO_AMERICAN_COUNTRIES.Contains(countryName))
          {
            fileName = "AM Radio America.xml";
          }
          else
          {
            fileName = "AM Radio.xml";
          }
        }
        else if (string.Equals(scanMode, SCAN_MODE_FM_RADIO))
        {
          if (string.Equals(countryName, "Japan"))
          {
            fileName = "FM Radio Japan.xml";
          }
          else
          {
            fileName = "FM Radio.xml";
          }
        }
      }

      TuningDetailFilter.Load(_tunerId, TuningDetailGroup.Analog, fileName, comboBoxTransmitter);
      SetScanButtonText(scanMode);
    }

    private void SetScanButtonText(string scanMode)
    {
      if (string.Equals(scanMode, SCAN_MODE_EXTERNAL_TUNER))
      {
        buttonScan.Text = "&Import channels";
      }
      else
      {
        buttonScan.Text = "&Scan for channels";
      }
    }

    private void ImportExternalTunerChannelList()
    {
      if (openFileDialogExternalTunerChannelList.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      string fileName = openFileDialogExternalTunerChannelList.FileName;
      this.LogInfo("scan analog: import external tuner channel list, file name = {0}", fileName);
      try
      {
        List<DbTuningDetail> newTuningDetails = new List<DbTuningDetail>();
        List<DbTuningDetail> updatedTuningDetails = new List<DbTuningDetail>();
        HashSet<int> touchedTuningDetailIds = new HashSet<int>();

        string line;
        using (StreamReader file = new StreamReader(fileName))
        {
          while ((line = file.ReadLine()) != null)
          {
            Match m = Regex.Match(line, @"^\s*([^\,]),\s*([^\s].*?)\s*$");
            if (!m.Success)
            {
              if (!string.IsNullOrWhiteSpace(line))
              {
                this.LogWarn("scan analog: skipped non-matching, non-blank line \"{0}\"", line);
              }
              continue;
            }
            string lcnTemp = m.Groups[1].Captures[0].Value.Trim();
            string name = m.Groups[2].Captures[0].Value;
            string lcn;
            if (!LogicalChannelNumber.Create(lcnTemp, out lcn))
            {
              if (!LogicalChannelNumber.Create(name, out lcn))
              {
                this.LogWarn("scan analog: skipped line with invalid channel number \"{0}\"", line);
                continue;
              }
              name = lcnTemp;
            }

            ChannelCapture channel = new ChannelCapture();
            channel.Name = name;
            channel.MediaType = MediaType.Television;
            channel.LogicalChannelNumber = lcn;
            channel.Provider = "External Tuner";
            channel.VideoSource = CaptureSourceVideo.TunerDefault;
            channel.AudioSource = CaptureSourceAudio.TunerDefault;
            channel.IsEncrypted = false;
            channel.IsHighDefinition = false;
            channel.IsThreeDimensional = false;
            channel.IsVcrSignal = false;
            ScannedChannel scannedChannel = new ScannedChannel(channel);
            scannedChannel.IsVisibleInGuide = true;

            IList<DbTuningDetail> possibleTuningDetails = ChannelScanHelper.GetDbExistingTuningDetailCandidates(scannedChannel, false, TuningDetailRelation.Channel);
            DbTuningDetail dbTuningDetail = null;
            if (possibleTuningDetails != null)
            {
              if (possibleTuningDetails.Count == 1)
              {
                dbTuningDetail = possibleTuningDetails[0];
              }
              else
              {
                foreach (DbTuningDetail td in possibleTuningDetails)
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
              dbTuningDetail = ChannelScanHelper.AddChannel(channel, true);
              newTuningDetails.Add(dbTuningDetail);
            }
            else
            {
              ChannelScanHelper.UpdateChannel(channel, true, ref dbTuningDetail);
              updatedTuningDetails.Add(dbTuningDetail);
            }
            touchedTuningDetailIds.Add(dbTuningDetail.IdTuningDetail);
          }

          file.Close();
        }

        this.LogInfo("scan analog: import summary...");
        listViewProgress.BeginUpdate();
        try
        {
          listViewProgress.Items.Add("import summary...");
          if (newTuningDetails.Count == 0 && updatedTuningDetails.Count == 0)
          {
            listViewProgress.Items.Add("  no channels found");
            this.LogInfo("  no channels found");
          }
          else
          {
            line = string.Format("  updated, count = {0}", updatedTuningDetails.Count);
            this.LogInfo(line);
            listViewProgress.Items.Add(line);
            foreach (DbTuningDetail td in updatedTuningDetails)
            {
              this.LogDebug("    {0}", td.GetDescription());
              listViewProgress.Items.Add(new ListViewItem(string.Format("    {0}", td.Name)));
            }
            line = string.Format("  new, count = {0}", newTuningDetails.Count);
            this.LogInfo(line);
            ListViewItem lastItem = listViewProgress.Items.Add(line);
            foreach (DbTuningDetail td in newTuningDetails)
            {
              this.LogDebug("    {0}", td.GetDescription());
              lastItem = listViewProgress.Items.Add(new ListViewItem(string.Format("    {0}", td.Name)));
            }
            lastItem.EnsureVisible();
          }
        }
        finally
        {
          listViewProgress.EndUpdate();
        }

        ChannelScanHelper.DeleteDiscontinuedTuningDetails(_tunerId, null, ScanType.Standard, touchedTuningDetailIds, null);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "scan analog: unexpected import exception");
        listViewProgress.Invoke((MethodInvoker)delegate
        {
          ListViewItem item = listViewProgress.Items.Add(new ListViewItem("Unexpected error. Please report this error on our forum."));
          item.ForeColor = System.Drawing.Color.Red;
          item.EnsureVisible();
        });
        MessageBox.Show("Encountered unexpected error. Please report this error on our forum.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}