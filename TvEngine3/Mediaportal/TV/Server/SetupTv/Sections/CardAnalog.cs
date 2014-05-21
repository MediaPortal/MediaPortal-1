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
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardAnalog : SectionSettings
  {
    private readonly int _cardId;
    private bool _isScanning;
    private bool _stopScanning;
    private Dictionary<VideoProcAmpProperty, HScrollBar> _videoProcAmpControls = new Dictionary<VideoProcAmpProperty, HScrollBar>();
    private Dictionary<VideoProcAmpProperty, Label> _videoProcAmpLabels = new Dictionary<VideoProcAmpProperty, Label>();

    public CardAnalog()
      : this("Analog") {}

    public CardAnalog(string name)
      : base(name) {}

    public CardAnalog(string name, int cardNumber)
      : base(name)
    {
      _cardId = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }

    private void Init()
    {
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        mpComboBoxCountry.Items.Add(countries.Countries[i]);
      }
      mpComboBoxCountry.SelectedIndex = 0;
      mpComboBoxSource.Items.Add(TunerInputType.Antenna);
      mpComboBoxSource.Items.Add(TunerInputType.Cable);
      mpComboBoxSource.SelectedIndex = 0;

      _videoProcAmpControls.Add(VideoProcAmpProperty.Brightness, brightnessScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.Brightness, brightnessValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.Contrast, contrastScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.Contrast, contrastValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.Hue, hueScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.Hue, hueValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.Saturation, saturationScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.Saturation, saturationValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.Sharpness, sharpnessScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.Sharpness, sharpnessValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.Gamma, gammaScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.Gamma, gammaValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.ColorEnable, colorEnableScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.ColorEnable, colorEnableValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.WhiteBalance, whiteBalanceScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.WhiteBalance, whiteBalanceValue);
      _videoProcAmpControls.Add(VideoProcAmpProperty.BacklightCompensation, backlightCompensationScrollbar);
      _videoProcAmpLabels.Add(VideoProcAmpProperty.BacklightCompensation, backlightCompensationValue);
    }

    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardId));
      progressBarQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardId));
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpComboBoxSensitivity.SelectedIndex = 1;
      UpdateStatus();
      
      mpComboBoxCountry.SelectedIndex = ServiceAgents.Instance.SettingServiceAgent.GetValue("analog" + _cardId + "Country", 0);
      mpComboBoxSource.SelectedIndex = ServiceAgents.Instance.SettingServiceAgent.GetValue("analog" + _cardId + "Source", 0);
      checkBoxCreateSignalGroup.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("analog" + _cardId + "createsignalgroup", false);
      checkBoxCreateSignalGroup.Text = "Create \"" + TvConstants.TvGroupNames.Analog + "\" group";

      SetBitRateMode();
      SetBitRate();
      SetVideoProcAmp();
      SetVideoDecoder();
      SetStreamConfig();
    }

    private void SetStreamConfig()
    {
      double frameRate = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "FrameRate", (double)-1);
      if (frameRate < 0)
      {
        frameRateComboBox.Enabled = false;
      }
      else
      {
        frameRateComboBox.Enabled = true;
        string frameRateString = frameRate + " fps";
        foreach (string item in frameRateComboBox.Items)
        {
          if (item.StartsWith(frameRateString))
          {
            frameRateComboBox.SelectedItem = item;
            break;
          }
        }
      }

      int frameWidth = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "FrameWidth", -1);
      int frameHeight = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "FrameHeight", -1);
      if (frameWidth < 0)
      {
        resolutionComboBox.Enabled = false;
      }
      else
      {
        resolutionComboBox.Enabled = true;
        string resolution = frameWidth + "x" + frameHeight;
        foreach (string item in resolutionComboBox.Items)
        {
          if (item.StartsWith(resolution))
          {
            resolutionComboBox.SelectedItem = item;
            break;
          }
        }
      }
    }

    private void SetVideoDecoder()
    {
      videoStandardComboBox.Items.Clear();
      AnalogVideoStandard availableVideoStandard = (AnalogVideoStandard)ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "SupportedVideoStandards", (int)AnalogVideoStandard.None);
      if (availableVideoStandard != AnalogVideoStandard.None)
      {
        videoStandardComboBox.Enabled = true;
        foreach (AnalogVideoStandard standard in Enum.GetValues(typeof(AnalogVideoStandard)))
        {
          if (availableVideoStandard.HasFlag(standard))
          {
            videoStandardComboBox.Items.Add(standard);
          }
        }
        AnalogVideoStandard currentStandard = (AnalogVideoStandard)ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "VideoStandard", (int)AnalogVideoStandard.None);
        if (currentStandard != AnalogVideoStandard.None)
        {
          videoStandardComboBox.SelectedItem = currentStandard;
        }
      }
      else
      {
        videoStandardComboBox.Enabled = false;
      }
    }

    private void SetVideoProcAmp()
    {
      foreach (KeyValuePair<VideoProcAmpProperty, HScrollBar> property in _videoProcAmpControls)
      {
        int value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "VideoProcAmpProperty" + property.Key + "Value", -1);
        if (value != -1)
        {
          property.Value.Maximum = 100;
          property.Value.Minimum = 0;
          property.Value.SmallChange = 1;
          property.Value.LargeChange = 1;
          if (value > 100)
          {
            property.Value.Value = 100;
          }
          else if (value < 0)
          {
            property.Value.Value = 0;
          }
          else
          {
            property.Value.Value = value;
          }
          property.Value.Enabled = true;
          _videoProcAmpLabels[property.Key].Text = value.ToString();
          _videoProcAmpLabels[property.Key].Enabled = true;
        }
        else
        {
          property.Value.Enabled = false;
          _videoProcAmpLabels[property.Key].Text = string.Empty;
          _videoProcAmpLabels[property.Key].Enabled = false;
        }
      }
    }

    private void SetBitRateMode()
    {
      VIDEOENCODER_BITRATE_MODE bitRateMode = (VIDEOENCODER_BITRATE_MODE)ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "BitRateMode", (int)VIDEOENCODER_BITRATE_MODE.ConstantBitRate);
      switch (bitRateMode)
      {
        case VIDEOENCODER_BITRATE_MODE.ConstantBitRate:
          cbrPlayback.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage:
          vbrPlayback.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak:
          vbrPeakPlayback.Select();
          break;
      }
    }

    private void SetBitRate()
    {
      customValue.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "CustomBitRate", 50);
      customValuePeak.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "CustomPeakBitRate", 75);

      QualityType bitRateProfile = (QualityType)ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "BitRateProfile", (int)QualityType.Default);
      switch (bitRateProfile)
      {
        case QualityType.Default:
          defaultPlayback.Select();
          break;
        case QualityType.Custom:
          customPlayback.Select();
          break;
        case QualityType.Portable:
          portablePlayback.Select();
          break;
        case QualityType.Low:
          lowPlayback.Select();
          break;
        case QualityType.Medium:
          mediumPlayback.Select();
          break;
        case QualityType.High:
          highPlayback.Select();
          break;
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("analog" + _cardId + "Country", mpComboBoxCountry.SelectedIndex);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("analog" + _cardId + "Source", mpComboBoxSource.SelectedIndex);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("analog" + _cardId + "createsignalgroup", checkBoxCreateSignalGroup.Checked);
      
      SaveConfiguration();
      Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
      if (card.Enabled)
      {
        try
        {
          ServiceAgents.Instance.ControllerServiceAgent.ReloadCardConfiguration(_cardId);
        }
        catch
        {
          this.LogDebug("Could not reload card configuration");
        }
        return;
      }
    }

    private void SaveConfiguration()
    {
      if (customValue.Enabled)
      {
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "CustomBitRate", (int)customValue.Value);
      }
      if (customValuePeak.Enabled)
      {
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "CustomPeakBitRate", (int)customValuePeak.Value);
      }

      if (bitRateModeGroup.Enabled)
      {
        VIDEOENCODER_BITRATE_MODE bitRateMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
        if (cbrPlayback.Checked)
        {
          bitRateMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
        }
        else if (vbrPlayback.Checked)
        {
          bitRateMode = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
        }
        else if (vbrPeakPlayback.Checked)
        {
          bitRateMode = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
        }
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "BitRateMode", (int)bitRateMode);
      }

      if (bitRate.Enabled)
      {
        QualityType bitRateProfile = QualityType.Default;
        if (defaultPlayback.Checked)
        {
          bitRateProfile = QualityType.Default;
        }
        else if (customPlayback.Checked)
        {
          bitRateProfile = QualityType.Custom;
        }
        else if (portablePlayback.Checked)
        {
          bitRateProfile = QualityType.Portable;
        }
        else if (lowPlayback.Checked)
        {
          bitRateProfile = QualityType.Low;
        }
        else if (mediumPlayback.Checked)
        {
          bitRateProfile = QualityType.Medium;
        }
        else if (highPlayback.Checked)
        {
          bitRateProfile = QualityType.High;
        }
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "BitRateProfile", (int)bitRateProfile);
      }

      foreach (KeyValuePair<VideoProcAmpProperty, HScrollBar> property in _videoProcAmpControls)
      {
        if (property.Value.Enabled)
        {
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "VideoProcAmpProperty" + property.Key + "Value", property.Value.Value);
        }
      }

      if (videoStandardComboBox.Enabled)
      {
        if (videoStandardComboBox.SelectedIndex != -1)
        {
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "VideoStandard", (int)(AnalogVideoStandard)videoStandardComboBox.SelectedItem);
        }
      }
      if (frameRateComboBox.Enabled)
      {
        if (frameRateComboBox.SelectedIndex != -1)
        {
          string item = frameRateComboBox.SelectedItem.ToString();
          string frameRate = item.Substring(0, item.IndexOf(" fps"));
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "FrameRate", Double.Parse(frameRate, CultureInfo.GetCultureInfo("en-GB").NumberFormat));
        }
      }
      if (resolutionComboBox.Enabled)
      {
        if (resolutionComboBox.SelectedIndex != -1)
        {
          string item = resolutionComboBox.SelectedItem.ToString();
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "FrameWidth", Int32.Parse(item.Substring(0, 3)));
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("tuner" + _cardId + "FrameHeight", Int32.Parse(item.Substring(4, 3)));
        }
      }
    }

    private void mpButtonScan_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
          return;
        }
        if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
          return;
        }
        // Check if the card is locked for scanning.
        IUser user;
        if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardId, out user))
        {
          MessageBox.Show(this, "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid tuner?");
          return;
        }
        Thread scanThread = new Thread(DoTvScan);
        scanThread.Name = "Analog TV scan thread";
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }

    private void DoTvScan()
    {
      int channelsNew = 0;
      int channelsUpdated = 0;

      string buttonText = mpButtonScanTv.Text;
      checkButton.Enabled = false;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        checkBoxCreateSignalGroup.Enabled = false;
        checkBoxNoMerge.Enabled = false;
        mpButtonScanRadio.Enabled = false;
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();
        IUser user = new User();
        user.CardId = _cardId;
        AnalogChannel temp = new AnalogChannel();
        temp.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
        temp.VideoSource = CaptureSourceVideo.Tuner;
        temp.AudioSource = CaptureSourceAudio.Tuner;
        temp.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
        temp.ChannelNumber = 1;
        temp.MediaType = MediaTypeEnum.TV;
        TvResult tuneResult = ServiceAgents.Instance.ControllerServiceAgent.Scan(user.Name, user.CardId, out user, temp, -1);
        if (tuneResult == TvResult.SWEncoderMissing)
        {
          this.LogError("analog: failed to scan, missing software encoder");
          MessageBox.Show("Please install supported software encoders for your tuner.", "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if (tuneResult == TvResult.GraphBuildingFailed)
        {
          this.LogError("analog: failed to scan, tuner loading failed");
          MessageBox.Show("Failed to load the tuner. Your tuner is probably not supported. Please create a report in our forum.",
            "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        // Successful tuning means we're guaranteed to have found the tuner capabilities.
        // Now we can load the other settings.
        ReCheckSettings();

        // Add the external inputs.
        this.LogInfo("analog: adding external inputs");
        AnalogChannel channel = new AnalogChannel();
        channel.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
        channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
        channel.ChannelNumber = 0;
        channel.MediaType = MediaTypeEnum.TV;
        channel.VideoSource = CaptureSourceVideo.Composite1;
        channel.AudioSource = CaptureSourceAudio.Automatic;
        IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardId, channel);
        if (channels != null && channels.Length > 0)
        {
          this.LogInfo("analog: input count = {0}", channels.Length);
          foreach (IChannel ch in channels)
          {
            UpdateDatabase(card, ch);
          }
        }

        // TODO these are meant to be the channel range associated with each country.
        int minChannel = 1;
        int maxChannel = 150;
        if (maxChannel <= 0)
        {
          maxChannel = mpComboBoxSource.SelectedIndex == 0 ? 69 : 125;
        }
        if (minChannel < 0)
          minChannel = 1;
        this.LogInfo("Min channel = {0}. Max channel = {1}", minChannel, maxChannel);
        for (int channelNr = minChannel; channelNr <= maxChannel; channelNr++)
        {
          if (_stopScanning)
            return;
          float percent = ((float)((channelNr - minChannel)) / (maxChannel - minChannel));
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          if (percent < 0)
            percent = 0f;
          progressBar1.Value = (int)percent;

          channel = new AnalogChannel();
          channel.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.ChannelNumber = channelNr;
          channel.MediaType = MediaTypeEnum.TV;
          channel.VideoSource = CaptureSourceVideo.Tuner;
          channel.AudioSource = CaptureSourceAudio.Automatic;

          string line = String.Format("channel:{0} source:{1} ", channel.ChannelNumber, mpComboBoxSource.SelectedItem);
          ListViewItem item = mpListView1.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardId, channel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardId) == false)
            {
              line = String.Format("channel:{0} source:{1} : No Signal", channel.ChannelNumber,
                                   mpComboBoxSource.SelectedItem);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            line = String.Format("channel:{0} source:{1} : Nothing found", channel.ChannelNumber ,
                                 mpComboBoxSource.SelectedItem);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }

          foreach (IChannel c in channels)
          {
            if (UpdateDatabase(card, c))
            {
              line = String.Format("channel:{0} source:{1} : Channel update found - {2}", channel.ChannelNumber,
                                   mpComboBoxSource.SelectedItem, c.Name);
              channelsUpdated++;
            }
            else
            {
              line = String.Format("channel:{0} source:{1} : New channel found - {2}", channel.ChannelNumber,
                                   mpComboBoxSource.SelectedItem, c.Name);
              channelsNew++;
            }
            item.Text = line;
          }
        }
      }
      catch (TvExceptionSWEncoderMissing)
      {
        this.LogError("analog: failed to scan, missing software encoder");
        MessageBox.Show("Please install supported software encoders for your tuner.", "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      catch (TvExceptionTunerLoadFailed ex)
      {
        this.LogError(ex, "analog: failed to scan, tuner loading failed");
        MessageBox.Show("Failed to load the tuner. Your tuner is probably not supported. Please create a report in our forum.",
          "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "analog: failed to scan, generic error");
        MessageBox.Show("Failed to scan. Please create a report in our forum.", "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        IUser user = new User();
        user.CardId = _cardId;
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        mpButtonScanTv.Text = buttonText;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
        checkBoxCreateSignalGroup.Enabled = true;
        checkBoxNoMerge.Enabled = true;
        mpButtonScanTv.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        _isScanning = false;
        checkButton.Enabled = true;
      }
      ListViewItem lastItem =
        mpListView1.Items.Add(
          new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", channelsNew, channelsUpdated)));
      lastItem.EnsureVisible();
    }

    private bool UpdateDatabase(Card card, IChannel channel)
    {
      bool exists = false;
      Channel dbChannel = null;
      if (checkBoxNoMerge.Checked)
      {
        dbChannel = ChannelFactory.CreateChannel(channel.Name);
        AnalogChannel analogChannel = channel as AnalogChannel;
        if (analogChannel != null)
        {
          dbChannel.SortOrder = analogChannel.ChannelNumber;
          dbChannel.ChannelNumber = analogChannel.ChannelNumber;
        }
      }
      else
      {
        IList<TuningDetail> tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailsByName(channel.Name, 0);
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          dbChannel = tuningDetails[0].Channel;
        }

        if (dbChannel != null)
        {
          exists = true;
        }
        else
        {
          dbChannel = ChannelFactory.CreateChannel(channel.Name);
        }
      }
      dbChannel.MediaType = (int)channel.MediaType;
      dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
      dbChannel.AcceptChanges();
      ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel);
      MappingHelper.AddChannelToCard(dbChannel, card, false);

      if (dbChannel.MediaType == (decimal)MediaTypeEnum.TV)
      {
        ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
        MappingHelper.AddChannelToGroup(ref dbChannel, @group);
        if (checkBoxCreateSignalGroup.Checked)
        {
          group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.Analog, MediaTypeEnum.TV);
          MappingHelper.AddChannelToGroup(ref dbChannel, @group);
        }
      }
      else if (dbChannel.MediaType == (decimal)MediaTypeEnum.Radio)
      {
        ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);
        MappingHelper.AddChannelToGroup(ref dbChannel, @group);
        if (checkBoxCreateSignalGroup.Checked)
        {
          group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.Analog, MediaTypeEnum.Radio);
          MappingHelper.AddChannelToGroup(ref dbChannel, @group);
        }
      }
      return exists;
    }

    private void mpButtonScanRadio_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
          return;
        }
        if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
          return;
        }
        // Check if the card is locked for scanning.
        IUser user;
        if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardId, out user))
        {
          MessageBox.Show(this, "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid tuner?");
          return;
        }
        AnalogChannel radioChannel = new AnalogChannel();
        radioChannel.Frequency = 96000000;
        radioChannel.MediaType = MediaTypeEnum.Radio;
        radioChannel.VideoSource = CaptureSourceVideo.Tuner;
        radioChannel.AudioSource = CaptureSourceAudio.Automatic;
        // TODO this doesn't actually check FM radio support (not all analog tuners support both TV and FM radio)
        if (!ServiceAgents.Instance.ControllerServiceAgent.CanTune(_cardId, radioChannel))
        {
          MessageBox.Show(this, "This tuner does not support radio.");
          return;
        }

        Thread scanThread = new Thread(DoRadioScan);
        scanThread.Name = "Analog Radio scan thread";
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }

    private int SignalStrength(int sensitivity)
    {
      int i;
      for (i = 0; i < sensitivity * 2; i++)
      {
        if (!ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardId))
        {
          break;
        }
        Thread.Sleep(50);
      }
      return ((i * 50) / sensitivity);
    }

    private void DoRadioScan()
    {
      int channelsNew = 0;
      int channelsUpdated = 0;

      checkButton.Enabled = false;
      int sensitivity = 1;
      switch (mpComboBoxSensitivity.Text)
      {
        case "High":
          sensitivity = 10;
          break;
        case "Medium":
          sensitivity = 2;
          break;
        case "Low":
          sensitivity = 1;
          break;
      }
      string buttonText = mpButtonScanRadio.Text;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanRadio.Text = "Cancel...";
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        checkBoxCreateSignalGroup.Enabled = false;
        checkBoxNoMerge.Enabled = false;
        mpButtonScanTv.Enabled = false;
        UpdateStatus();
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();
        for (int freq = 87500000; freq < 108000000; freq += 100000)
        {
          if (_stopScanning)
            return;
          float percent = ((freq - 87500000)) / (108000000f - 87500000f);
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          progressBar1.Value = (int)percent;
          AnalogChannel channel = new AnalogChannel();
          channel.MediaType = MediaTypeEnum.Radio;
          channel.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
          channel.VideoSource = CaptureSourceVideo.Tuner;
          channel.AudioSource = CaptureSourceAudio.Automatic;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.Frequency = freq;
          
          channel.MediaType = MediaTypeEnum.Radio;
          float freqMHz = channel.Frequency;
          freqMHz /= 1000000f;
          string line = String.Format("frequence:{0} MHz ", freqMHz.ToString("f2"));
          ListViewItem item = mpListView1.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          IUser user = new User();
          user.CardId = _cardId;
          if (percent == 0)
          {
            TvResult tuneResult = ServiceAgents.Instance.ControllerServiceAgent.Scan(user.Name, user.CardId, out user, channel, -1);
            if (tuneResult == TvResult.SWEncoderMissing)
            {
              this.LogError("analog: failed to scan, missing software encoder");
              MessageBox.Show("Please install supported software encoders for your tuner.", "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
              break;
            }
            if (tuneResult == TvResult.GraphBuildingFailed)
            {
              this.LogError("analog: failed to scan, tuner loading failed");
              MessageBox.Show("Failed to load the tuner. Your tuner is probably not supported. Please create a report in our forum.",
                "Unable to scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
              break;
            }

            // Successful tuning means we're guaranteed to have found the tuner capabilities.
            // Now we can load the other settings.
            ReCheckSettings();
            UpdateStatus();
          }
          IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardId, channel);

          UpdateStatus();

          Thread.Sleep(2000);
          if (channels != null && channels.Length >= 0 && SignalStrength(sensitivity) >= 100)
          {
            foreach (IChannel c in channels)
            {
              if (UpdateDatabase(card, c))
              {
                line = String.Format("frequence:{0} MHz : Channel update found - {1}", freqMHz.ToString("f2"), c.Name);
                channelsUpdated++;
              }
              else
              {
                line = String.Format("frequence:{0} MHz : New channel found - {1}", freqMHz.ToString("f2"), c.Name);
                channelsNew++;
              }
              item.Text = line;
            }
          }
          else
          {
            line = String.Format("frequence:{0} MHz : No Signal", freqMHz.ToString("f2"));
            item.Text = line;
            item.ForeColor = Color.Red;
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        checkButton.Enabled = true;
        IUser user = new User();
        user.CardId = _cardId;
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        mpButtonScanRadio.Text = buttonText;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
        checkBoxCreateSignalGroup.Enabled = true;
        checkBoxNoMerge.Enabled = true;
        mpButtonScanTv.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        _isScanning = false;
      }
      ListViewItem lastItem =
        mpListView1.Items.Add(
          new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", channelsNew, channelsUpdated)));
      lastItem.EnsureVisible();
    }

    private void ReCheckSettings()
    {
      if (!videoStandardComboBox.Enabled)
      {
        SetVideoDecoder();
      }
      if (!frameRateComboBox.Enabled && !resolutionComboBox.Enabled)
      {
        SetStreamConfig();
      }
      if (!brightnessScrollbar.Enabled && !contrastScrollbar.Enabled && !hueScrollbar.Enabled &&
          !saturationScrollbar.Enabled && !sharpnessScrollbar.Enabled && !gammaScrollbar.Enabled &&
          !colorEnableScrollbar.Enabled && !whiteBalanceScrollbar.Enabled &&
          !backlightCompensationValue.Enabled)
      {
        SetVideoProcAmp();
      }
    }

    private void checkButton_Click(object sender, EventArgs e)
    {
      Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
      if (card.Enabled == false)
      {
        MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before checking for quality control support.");
        return;
      }
      if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
      {
        MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before checking for quality control support.");
        return;
      }

      IUser user;
      if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardId, out user))
      {
        user = new User();
        user.CardId = _cardId;
        AnalogChannel temp = new AnalogChannel();
        temp.VideoSource = CaptureSourceVideo.Tuner;
        temp.AudioSource = CaptureSourceAudio.Tuner;
        temp.MediaType = MediaTypeEnum.TV;

        TvResult tuneResult = ServiceAgents.Instance.ControllerServiceAgent.Tune(user.Name, user.CardId, out user, temp, -1);
        if (tuneResult == TvResult.SWEncoderMissing)
        {
          this.LogError("analog: failed to check quality control support, missing software encoder");
          MessageBox.Show("Please install supported software encoders for your tuner.", "Unable to check quality control support", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if (tuneResult == TvResult.GraphBuildingFailed)
        {
          this.LogError("analog: failed to check quality control support, tuner loading failed");
          MessageBox.Show("Failed to load the tuner. Your tuner is probably not supported. Please create a report in our forum.",
            "Unable to check quality control support", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
      }

      if (ServiceAgents.Instance.ControllerServiceAgent.SupportsQualityControl(_cardId))
      {
        bitRateModeGroup.Enabled = ServiceAgents.Instance.ControllerServiceAgent.SupportsBitRateModes(_cardId);
        vbrPeakPlayback.Enabled = ServiceAgents.Instance.ControllerServiceAgent.SupportsPeakBitRateMode(_cardId);

        if (ServiceAgents.Instance.ControllerServiceAgent.SupportsBitRate(_cardId))
        {
          bitRate.Enabled = true;
          customSettingsGroup.Enabled = true;
          customValue.Enabled = true;
          customValuePeak.Enabled = true;
        }
        else
        {
          bitRate.Enabled = false;
          customSettingsGroup.Enabled = false;
          customValue.Enabled = false;
          customValuePeak.Enabled = false;
        }

        SetBitRateMode();
        SetBitRate();
      }
      else
      {
        this.LogDebug("analog: quality control not supported");
        MessageBox.Show("The tuner's current encoder doesn't support quality control.",
                        "Quality control check result", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      ReCheckSettings();
    }

    private void defaultValuesButton_Click(object sender, EventArgs e)
    {
      foreach (VideoProcAmpProperty property in _videoProcAmpControls.Keys)
      {
        if (_videoProcAmpControls[property].Enabled)
        {
          int defaultValue = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _cardId + "VideoProcAmpProperty" + property + "DefaultValue", -1);
          if (defaultValue != -1)
          {
            _videoProcAmpControls[property].Value = defaultValue;
            _videoProcAmpLabels[property].Text = defaultValue.ToString();
          }
        }
      }
    }

    private void brightnessScrollbar_ValueChanged(object sender, EventArgs e)
    {
      brightnessValue.Text = brightnessScrollbar.Value.ToString();
    }

    private void contrastScrollbar_ValueChanged(object sender, EventArgs e)
    {
      contrastValue.Text = contrastScrollbar.Value.ToString();
    }

    private void hueScrollbar_ValueChanged(object sender, EventArgs e)
    {
      hueValue.Text = hueScrollbar.Value.ToString();
    }

    private void saturationScrollbar_ValueChanged(object sender, EventArgs e)
    {
      saturationValue.Text = saturationScrollbar.Value.ToString();
    }

    private void sharpnessScrollbar_ValueChanged(object sender, EventArgs e)
    {
      sharpnessValue.Text = sharpnessScrollbar.Value.ToString();
    }

    private void gammaScrollbar_ValueChanged(object sender, EventArgs e)
    {
      gammaValue.Text = gammaScrollbar.Value.ToString();
    }

    private void colorEnableScrollbar_ValueChanged(object sender, EventArgs e)
    {
      colorEnableValue.Text = colorEnableScrollbar.Value.ToString();
    }

    private void whiteBalanceScrollbar_ValueChanged(object sender, EventArgs e)
    {
      whiteBalanceValue.Text = whiteBalanceScrollbar.Value.ToString();
    }

    private void backlightCompensationScrollbar_ValueChanged(object sender, EventArgs e)
    {
      backlightCompensationValue.Text = backlightCompensationScrollbar.Value.ToString();
    }
  }
}