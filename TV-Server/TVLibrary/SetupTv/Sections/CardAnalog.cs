/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using DirectShowLib;
using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Analog;

namespace SetupTv.Sections
{
  public partial class CardAnalog : SectionSettings
  {
    readonly int _cardNumber;
    bool _isScanning;
    bool _stopScanning;
    string _cardName;
    string _devicePath;
    Configuration _configuration;

    public CardAnalog()
      : this("Analog")
    {
    }

    public CardAnalog(string name)
      : base(name)
    {
    }

    public CardAnalog(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();

    }

    void Init()
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
    }

    void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpComboBoxSensitivity.SelectedIndex = 1;
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("analog" + _cardNumber + "Country", "0").Value);
      mpComboBoxSource.SelectedIndex = Int32.Parse(layer.GetSetting("analog" + _cardNumber + "Source", "0").Value);
      _cardName = RemoteControl.Instance.CardName(_cardNumber);
      _devicePath = RemoteControl.Instance.CardDevice(_cardNumber);
      if (!String.IsNullOrEmpty(_cardName) && !String.IsNullOrEmpty(_devicePath))
      {
        _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
        customValue.Value = _configuration.CustomQualityValue;
        customValuePeak.Value = _configuration.CustomPeakQualityValue;
        SetBitRateModes();
        SetBitRate();
        SetVideoProcAmp(_configuration.Graph.Capture.VideoProcAmpValues);
        SetVideoDecoder();
        SetStreamConfig();
      } else
      {
        SetVideoProcAmp(new Dictionary<VideoProcAmpProperty, VideoQuality>());
        videoStandardComboBox.Enabled = false;
        resolutionComboBox.Enabled = false;
        frameRateComboBox.Enabled = false;
      }
    }

    private void SetStreamConfig()
    {
      double frameRate = _configuration.Graph.Capture.FrameRate;
      if (frameRate < 0)
      {
        frameRateComboBox.Enabled = false;
      } else
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
      int imageWidth = _configuration.Graph.Capture.ImageWidth;
      int imageHeight = _configuration.Graph.Capture.ImageHeight;
      if (imageWidth < 0)
      {
        resolutionComboBox.Enabled = false;
      } else
      {
        resolutionComboBox.Enabled = true;
        string resolution = imageWidth + "x" + imageHeight;
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
      AnalogVideoStandard availableVideoStandard = _configuration.Graph.Capture.AvailableVideoStandard;
      if (availableVideoStandard != AnalogVideoStandard.None)
      {
        videoStandardComboBox.Enabled = true;
        if ((availableVideoStandard & AnalogVideoStandard.NTSC_433) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.NTSC_433);
        }
        if ((availableVideoStandard & AnalogVideoStandard.NTSC_M) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.NTSC_M);
        }
        if ((availableVideoStandard & AnalogVideoStandard.NTSC_M_J) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.NTSC_M_J);
        }
        if ((availableVideoStandard & AnalogVideoStandard.NTSCMask) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.NTSCMask);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_60) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_60);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_B) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_B);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_D) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_D);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_G) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_G);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_H) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_H);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_I) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_I);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_M) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_M);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_N) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_N);
        }
        if ((availableVideoStandard & AnalogVideoStandard.PAL_N_COMBO) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.PAL_N_COMBO);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_B) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_B);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_D) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_D);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_G) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_G);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_H) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_H);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_K) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_K);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_K1) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_K1);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_L) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_L);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAM_L1) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAM_L1);
        }
        if ((availableVideoStandard & AnalogVideoStandard.SECAMMask) != 0)
        {
          videoStandardComboBox.Items.Add(AnalogVideoStandard.SECAMMask);
        }
        AnalogVideoStandard currentStandard = _configuration.Graph.Capture.CurrentVideoStandard;
        if (currentStandard != AnalogVideoStandard.None)
        {
          videoStandardComboBox.SelectedItem = currentStandard;
        }
      } else
      {
        videoStandardComboBox.Enabled = false;
      }
    }

    private void SetVideoProcAmp(IDictionary<VideoProcAmpProperty, VideoQuality> map)
    {
      VideoQuality quality;
      if (map.ContainsKey(VideoProcAmpProperty.Brightness))
      {
        quality = map[VideoProcAmpProperty.Brightness];
        brightnessScrollbar.Maximum = quality.MaxValue;
        brightnessScrollbar.Minimum = quality.MinValue;
        brightnessScrollbar.SmallChange = quality.SteppingDelta;
        brightnessScrollbar.LargeChange = quality.SteppingDelta;
        brightnessScrollbar.Value = quality.Value;
        brightnessValue.Text = quality.Value.ToString();
        brightnessScrollbar.Enabled = true;
        brightnessValue.Enabled = true;
        label5.Enabled = true;
      } else
      {
        brightnessScrollbar.Enabled = false;
        brightnessValue.Text = string.Empty;
        brightnessValue.Enabled = false;
        label5.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.Contrast))
      {
        quality = map[VideoProcAmpProperty.Contrast];
        contrastScrollbar.Maximum = quality.MaxValue;
        contrastScrollbar.Minimum = quality.MinValue;
        contrastScrollbar.SmallChange = quality.SteppingDelta;
        contrastScrollbar.LargeChange = quality.SteppingDelta;
        contrastScrollbar.Value = quality.Value;
        contrastValue.Text = quality.Value.ToString();
        contrastScrollbar.Enabled = true;
        contrastValue.Enabled = true;
        label6.Enabled = true;
      } else
      {
        contrastScrollbar.Enabled = false;
        contrastValue.Text = string.Empty;
        contrastValue.Enabled = false;
        label6.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.Hue))
      {
        quality = map[VideoProcAmpProperty.Hue];
        hueScrollbar.Maximum = quality.MaxValue;
        hueScrollbar.Minimum = quality.MinValue;
        hueScrollbar.SmallChange = quality.SteppingDelta;
        hueScrollbar.LargeChange = quality.SteppingDelta;
        hueScrollbar.Value = quality.Value;
        hueValue.Text = quality.Value.ToString();
        hueScrollbar.Enabled = true;
        hueValue.Enabled = true;
        label7.Enabled = true;
      } else
      {
        hueScrollbar.Enabled = false;
        hueValue.Text = string.Empty;
        hueValue.Enabled = false;
        label7.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.Saturation))
      {
        quality = map[VideoProcAmpProperty.Saturation];
        saturationScrollbar.Maximum = quality.MaxValue;
        saturationScrollbar.Minimum = quality.MinValue;
        saturationScrollbar.SmallChange = quality.SteppingDelta;
        saturationScrollbar.LargeChange = quality.SteppingDelta;
        saturationScrollbar.Value = quality.Value;
        saturationValue.Text = quality.Value.ToString();
        saturationScrollbar.Enabled = true;
        saturationValue.Enabled = true;
        label8.Enabled = true;
      } else
      {
        saturationScrollbar.Enabled = false;
        saturationValue.Text = string.Empty;
        saturationValue.Enabled = false;
        label8.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.Sharpness))
      {
        quality = map[VideoProcAmpProperty.Sharpness];
        sharpnessScrollbar.Maximum = quality.MaxValue;
        sharpnessScrollbar.Minimum = quality.MinValue;
        sharpnessScrollbar.SmallChange = quality.SteppingDelta;
        sharpnessScrollbar.LargeChange = quality.SteppingDelta;
        sharpnessScrollbar.Value = quality.Value;
        sharpnessValue.Text = quality.Value.ToString();
        sharpnessScrollbar.Enabled = true;
        sharpnessValue.Enabled = true;
        label9.Enabled = true;
      } else
      {
        sharpnessScrollbar.Enabled = false;
        sharpnessValue.Text = string.Empty;
        sharpnessValue.Enabled = false;
        label9.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.Gamma))
      {
        quality = map[VideoProcAmpProperty.Gamma];
        gammaScrollbar.Maximum = quality.MaxValue;
        gammaScrollbar.Minimum = quality.MinValue;
        gammaScrollbar.SmallChange = quality.SteppingDelta;
        gammaScrollbar.LargeChange = quality.SteppingDelta;
        gammaScrollbar.Value = quality.Value;
        gammaValue.Text = quality.Value.ToString();
        gammaScrollbar.Enabled = true;
        gammaValue.Enabled = true;
        label10.Enabled = true;
      } else
      {
        gammaScrollbar.Enabled = false;
        gammaValue.Text = string.Empty;
        gammaValue.Enabled = false;
        label10.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.ColorEnable))
      {
        quality = map[VideoProcAmpProperty.ColorEnable];
        colorEnableScrollbar.Maximum = quality.MaxValue;
        colorEnableScrollbar.Minimum = quality.MinValue;
        colorEnableScrollbar.SmallChange = quality.SteppingDelta;
        colorEnableScrollbar.LargeChange = quality.SteppingDelta;
        colorEnableScrollbar.Value = quality.Value;
        colorEnableValue.Text = quality.Value.ToString();
        colorEnableScrollbar.Enabled = true;
        colorEnableValue.Enabled = true;
        label11.Enabled = true;
      } else
      {
        colorEnableScrollbar.Enabled = false;
        colorEnableValue.Text = string.Empty;
        colorEnableValue.Enabled = false;
        label11.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.WhiteBalance))
      {
        quality = map[VideoProcAmpProperty.WhiteBalance];
        whiteBalanceScrollbar.Maximum = quality.MaxValue;
        whiteBalanceScrollbar.Minimum = quality.MinValue;
        whiteBalanceScrollbar.SmallChange = quality.SteppingDelta;
        whiteBalanceScrollbar.LargeChange = quality.SteppingDelta;
        whiteBalanceScrollbar.Value = quality.Value;
        whiteBalanceValue.Text = quality.Value.ToString();
        whiteBalanceScrollbar.Enabled = true;
        whiteBalanceValue.Enabled = true;
        label12.Enabled = true;
      } else
      {
        whiteBalanceScrollbar.Enabled = false;
        whiteBalanceValue.Text = string.Empty;
        whiteBalanceValue.Enabled = false;
        label12.Enabled = false;
      }
      if (map.ContainsKey(VideoProcAmpProperty.BacklightCompensation))
      {
        quality = map[VideoProcAmpProperty.BacklightCompensation];
        backlightCompensationScrollbar.Maximum = quality.MaxValue;
        backlightCompensationScrollbar.Minimum = quality.MinValue;
        backlightCompensationScrollbar.SmallChange = quality.SteppingDelta;
        backlightCompensationScrollbar.LargeChange = quality.SteppingDelta;
        backlightCompensationScrollbar.Value = quality.Value;
        backlightCompensationValue.Text = quality.Value.ToString();
        backlightCompensationScrollbar.Enabled = true;
        backlightCompensationValue.Enabled = true;
        label13.Enabled = true;
      } else
      {
        backlightCompensationScrollbar.Enabled = false;
        backlightCompensationValue.Text = string.Empty;
        backlightCompensationValue.Enabled = false;
        label13.Enabled = false;
      }
    }

    private void SetBitRateModes()
    {
      switch (_configuration.PlaybackQualityMode)
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
      switch (_configuration.RecordQualityMode)
      {
        case VIDEOENCODER_BITRATE_MODE.ConstantBitRate:
          cbrRecord.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage:
          vbrRecord.Select();
          break;
        case VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak:
          vbrPeakRecord.Select();
          break;
      }
    }

    private void SetBitRate()
    {
      switch (_configuration.PlaybackQualityType)
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
      switch (_configuration.RecordQualityType)
      {
        case QualityType.Default:
          defaultRecord.Select();
          break;
        case QualityType.Custom:
          customRecord.Select();
          break;
        case QualityType.Portable:
          portableRecord.Select();
          break;
        case QualityType.Low:
          lowRecord.Select();
          break;
        case QualityType.Medium:
          mediumRecord.Select();
          break;
        case QualityType.High:
          highRecord.Select();
          break;
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("analog" + _cardNumber + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("analog" + _cardNumber + "Source", "0");
      setting.Value = mpComboBoxSource.SelectedIndex.ToString();
      setting.Persist();
      UpdateConfiguration();
      Configuration.writeConfiguration(_configuration);
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      if (card.Enabled)
      {
        try
        {
          RemoteControl.Instance.ReloadCardConfiguration(_cardNumber);
        } catch
        {
          Log.WriteFile("Could not reload card configuration");
        }
        return;
      }
    }

    private void UpdateConfiguration()
    {
      _configuration.CustomQualityValue = (int)customValue.Value;
      _configuration.CustomPeakQualityValue = (int)customValuePeak.Value;
      if (cbrPlayback.Checked)
      {
        _configuration.PlaybackQualityMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
      } else if (vbrPlayback.Checked)
      {
        _configuration.PlaybackQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
      } else if (vbrPeakPlayback.Checked)
      {
        _configuration.PlaybackQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
      }
      if (cbrRecord.Checked)
      {
        _configuration.RecordQualityMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
      } else if (vbrRecord.Checked)
      {
        _configuration.RecordQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
      } else if (vbrPeakRecord.Checked)
      {
        _configuration.RecordQualityMode = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
      }
      if (defaultPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Default;
      } else if (customPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Custom;
      } else if (portablePlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Portable;
      } else if (lowPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Low;
      } else if (mediumPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.Medium;
      } else if (highPlayback.Checked)
      {
        _configuration.PlaybackQualityType = QualityType.High;
      }
      if (defaultRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Default;
      } else if (customRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Custom;
      } else if (portableRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Portable;
      } else if (lowRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Low;
      } else if (mediumRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.Medium;
      } else if (highRecord.Checked)
      {
        _configuration.RecordQualityType = QualityType.High;
      }
      if (_configuration.Graph != null && _configuration.Graph.Capture != null)
      {
        UpdateVideoProcAmp(_configuration.Graph.Capture.VideoProcAmpValues);
        if (videoStandardComboBox.Enabled)
        {
          if (videoStandardComboBox.SelectedIndex != -1 && !videoStandardComboBox.SelectedItem.Equals(AnalogVideoStandard.None))
          {
            _configuration.Graph.Capture.CurrentVideoStandard = (AnalogVideoStandard)videoStandardComboBox.SelectedItem;
          } else
          {
            _configuration.Graph.Capture.CurrentVideoStandard = AnalogVideoStandard.None;
          }
        }
        if (frameRateComboBox.Enabled)
        {
          string item = frameRateComboBox.SelectedItem.ToString();
          string frameRate = item.Substring(0, item.IndexOf(" fps"));
          _configuration.Graph.Capture.FrameRate = Double.Parse(frameRate, CultureInfo.GetCultureInfo("en-GB").NumberFormat);
        }
        if (resolutionComboBox.Enabled)
        {
          string item = resolutionComboBox.SelectedItem.ToString();
          _configuration.Graph.Capture.ImageWidth = Int32.Parse(item.Substring(0, 3));
          _configuration.Graph.Capture.ImageHeight = Int32.Parse(item.Substring(4, 3));
        }
      }
    }

    private void UpdateVideoProcAmp(IDictionary<VideoProcAmpProperty, VideoQuality> map)
    {
      if (brightnessScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.Brightness].Value = brightnessScrollbar.Value;
      }
      if (contrastScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.Contrast].Value = contrastScrollbar.Value;
      }
      if (hueScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.Hue].Value = hueScrollbar.Value;
      }
      if (saturationScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.Saturation].Value = saturationScrollbar.Value;
      }
      if (sharpnessScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.Sharpness].Value = sharpnessScrollbar.Value;
      }
      if (gammaScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.Gamma].Value = gammaScrollbar.Value;
      }
      if (colorEnableScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.ColorEnable].Value = colorEnableScrollbar.Value;
      }
      if (whiteBalanceScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.WhiteBalance].Value = whiteBalanceScrollbar.Value;
      }
      if (backlightCompensationScrollbar.Enabled)
      {
        map[VideoProcAmpProperty.BacklightCompensation].Value = backlightCompensationScrollbar.Value;
      }
    }

    private void mpButtonScan_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        Thread scanThread = new Thread(DoTvScan);
        scanThread.Name = "Analog TV scan thread";
        scanThread.Start();
      } else
      {
        _stopScanning = true;
      }
    }

    void DoTvScan()
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
        RemoteControl.Instance.EpgGrabberEnabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpButtonScanRadio.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        mpButton1.Enabled = false;
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();
        User user = new User();
        user.CardId = _cardNumber;
        AnalogChannel temp = new AnalogChannel();
        temp.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
        temp.VideoSource = AnalogChannel.VideoInputType.Tuner;
        temp.AudioSource = AnalogChannel.AudioInputType.Tuner;
        temp.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
        temp.IsRadio = false;
        temp.IsTv = true;
        RemoteControl.Instance.Tune(ref user, temp, -1);
        if (string.IsNullOrEmpty(_configuration.Graph.Capture.Name))
        {
          _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
          ReCheckSettings();
        }
        int minChannel = RemoteControl.Instance.MinChannel(_cardNumber);
        int maxChannel = RemoteControl.Instance.MaxChannel(_cardNumber);
        if (maxChannel < 0)
        {
          maxChannel = mpComboBoxSource.SelectedIndex == 0 ? 69 : 125;
        }
        if (minChannel < 0)
          minChannel = 1;
        Log.Info("Min channel = {0}. Max channel = {1}", minChannel, maxChannel);
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
          AnalogChannel channel = new AnalogChannel();
          channel.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.ChannelNumber = channelNr;
          channel.IsTv = true;
          channel.IsRadio = false;
          channel.VideoSource = AnalogChannel.VideoInputType.Tuner;
          channel.AudioSource = AnalogChannel.AudioInputType.Automatic;
          string line = String.Format("channel:{0} source:{1} ", channel.ChannelNumber, mpComboBoxSource.SelectedItem);
          ListViewItem item = mpListView1.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, channel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("channel:{0} source:{1} : No Signal", channel.ChannelNumber, mpComboBoxSource.SelectedItem);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            line = String.Format("channel:{0} source:{1} : Nothing found", channel.ChannelNumber, mpComboBoxSource.SelectedItem);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          bool exists = false;
          channel = (AnalogChannel)channels[0];
          if (channel.Name == "")
            channel.Name = String.Format(channel.ChannelNumber.ToString());
          Channel dbChannel;
          if (checkBoxNoMerge.Checked)
          {
            dbChannel = new Channel(channel.Name, false, false, 0, new DateTime(2000, 1, 1), false, new DateTime(2000, 1, 1), -1, true, "", true, channel.Name);
          } else
          {
            dbChannel = layer.GetChannelByName("", channel.Name);
            if (dbChannel != null)
            {
              dbChannel.Name = channel.Name;
              exists = true;
            } else
            {
              dbChannel = layer.AddNewChannel(channel.Name);
            }
          }
          dbChannel.IsTv = channel.IsTv;
          dbChannel.IsRadio = channel.IsRadio;
          dbChannel.FreeToAir = true;
          dbChannel.Persist();
          layer.AddTuningDetails(dbChannel, channel);
          layer.MapChannelToCard(card, dbChannel, false);
          layer.AddChannelToGroup(dbChannel, "Analog");
          if (exists)
          {
            line = String.Format("channel:{0} source:{1} : Channel update found - {2}", channel.ChannelNumber, mpComboBoxSource.SelectedItem, channel.Name);
            channelsUpdated++;
          } else
          {
            line = String.Format("channel:{0} source:{1} : New channel found - {2}", channel.ChannelNumber, mpComboBoxSource.SelectedItem, channel.Name);
            channelsNew++;
          }
          item.Text = line;
        }
      } finally
      {
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        mpButtonScanTv.Text = buttonText;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
        mpButton1.Enabled = true;
        _isScanning = false;
        checkButton.Enabled = true;
      }
      ListViewItem lastItem = mpListView1.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", channelsNew, channelsUpdated)));
      lastItem.EnsureVisible();

    }

    private void mpButtonScanRadio_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        AnalogChannel radioChannel = new AnalogChannel();
        radioChannel.Frequency = 96000000;
        radioChannel.IsRadio = true;
        radioChannel.VideoSource = AnalogChannel.VideoInputType.Tuner;
        radioChannel.AudioSource = AnalogChannel.AudioInputType.Automatic;
        if (!RemoteControl.Instance.CanTune(_cardNumber, radioChannel))
        {
          MessageBox.Show(this, "The Tv Card does not support radio");
          return;
        }
        if(string.IsNullOrEmpty(_configuration.Graph.Capture.Name))
        {
          _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
          ReCheckSettings();
        }
        Thread scanThread = new Thread(DoRadioScan);
        scanThread.Name = "Analog Radio scan thread";
        scanThread.Start();
      } else
      {
        _stopScanning = true;
      }
    }

    int SignalStrength(int sensitivity)
    {
      int i;
      for (i = 0; i < sensitivity * 2; i++)
      {
        if (!RemoteControl.Instance.TunerLocked(_cardNumber))
        {
          break;
        }
        Thread.Sleep(50);
      }
      return ((i * 50) / sensitivity);
    }

    void DoRadioScan()
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
        RemoteControl.Instance.EpgGrabberEnabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        mpButtonScanTv.Enabled = false;
        mpButton1.Enabled = false;
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
          channel.IsRadio = true;
          channel.TunerSource = mpComboBoxSource.SelectedIndex == 0 ? TunerInputType.Antenna : TunerInputType.Cable;
          channel.VideoSource = AnalogChannel.VideoInputType.Tuner;
          channel.AudioSource = AnalogChannel.AudioInputType.Automatic;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.Frequency = freq;
          channel.IsTv = false;
          channel.IsRadio = true;
          float freqMHz = channel.Frequency;
          freqMHz /= 1000000f;
          string line = String.Format("frequence:{0} MHz ", freqMHz.ToString("f2"));
          ListViewItem item = mpListView1.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          User user = new User();
          user.CardId = _cardNumber;
          RemoteControl.Instance.Tune(ref user, channel, -1);
          UpdateStatus();
          Thread.Sleep(2000);
          if (SignalStrength(sensitivity) == 100)
          {
            channel.Name = String.Format("{0}", freq);
            Channel dbChannel = layer.GetChannelByName("", channel.Name);
            if (dbChannel != null)
            {
              dbChannel.Name = channel.Name;
              line = String.Format("frequence:{0} MHz : Channel update found - {1}", freqMHz.ToString("f2"), channel.Name);
              channelsUpdated++;
            } else
            {
              dbChannel = layer.AddNewChannel(channel.Name);
              line = String.Format("frequence:{0} MHz : New channel found - {1}", freqMHz.ToString("f2"), channel.Name);
              channelsNew++;
            }
            item.Text = line;
            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            dbChannel.FreeToAir = true;
            dbChannel.Persist();
            layer.AddChannelToGroup(dbChannel, "Analog channels");
            layer.AddTuningDetails(dbChannel, channel);
            layer.MapChannelToCard(card, dbChannel, false);
            freq += 300000;
          } else
          {
            line = String.Format("frequence:{0} MHz : No Signal", freqMHz.ToString("f2"));
            item.Text = line;
            item.ForeColor = Color.Red;
          }
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      } finally
      {
        checkButton.Enabled = true;
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        mpButtonScanRadio.Text = buttonText;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        mpButtonScanTv.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
        mpButton1.Enabled = true;
        _isScanning = false;
      }
      ListViewItem lastItem = mpListView1.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", channelsNew, channelsUpdated)));
      lastItem.EnsureVisible();

    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(_configuration.Graph.Crossbar.Name))
      {
        User user = new User();
        user.CardId = _cardNumber;
        AnalogChannel temp = new AnalogChannel();
        temp.TunerSource = TunerInputType.Antenna;
        temp.VideoSource = AnalogChannel.VideoInputType.Tuner;
        temp.AudioSource = AnalogChannel.AudioInputType.Tuner;
        temp.IsRadio = false;
        temp.IsTv = true;
        RemoteControl.Instance.Tune(ref user, temp, -1);
        _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
        if (string.IsNullOrEmpty(_configuration.Graph.Crossbar.Name))
        {
          MessageBox.Show(this, "The S-Video channels could not be detected.");
          return;
        }
        ReCheckSettings();
      }

      TvBusinessLayer layer = new TvBusinessLayer();
      Dictionary<AnalogChannel.VideoInputType, int> videoPinMap = _configuration.Graph.Crossbar.VideoPinMap;
      AnalogChannel tuningDetail;
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      Channel dbChannel;
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.VideoInput1))
      {
        dbChannel = layer.AddChannel("", "CVBS#1 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.VideoInput1;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.VideoInput2))
      {
        dbChannel = layer.AddChannel("", "CVBS#2 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.VideoInput2;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.VideoInput3))
      {
        dbChannel = layer.AddChannel("", "CVBS#3 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.VideoInput3;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.SvhsInput1))
      {
        dbChannel = layer.AddChannel("", "S-Video#1 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.SvhsInput1;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.SvhsInput2))
      {
        dbChannel = layer.AddChannel("", "S-Video#2 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.SvhsInput2;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.SvhsInput3))
      {
        dbChannel = layer.AddChannel("", "S-Video#3 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.SvhsInput3;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.RgbInput1))
      {
        dbChannel = layer.AddChannel("", "RGB#1 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.RgbInput1;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.RgbInput2))
      {
        dbChannel = layer.AddChannel("", "RGB#2 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.RgbInput2;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.RgbInput3))
      {
        dbChannel = layer.AddChannel("", "RGB#3 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.RgbInput3;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.YRYBYInput1))
      {
        dbChannel = layer.AddChannel("", "YRYBY#1 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.YRYBYInput1;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.YRYBYInput2))
      {
        dbChannel = layer.AddChannel("", "YRYBY#2 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.YRYBYInput2;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      if (videoPinMap.ContainsKey(AnalogChannel.VideoInputType.YRYBYInput3))
      {
        dbChannel = layer.AddChannel("", "YRYBY#3 on " + card.IdCard);
        dbChannel.IsTv = true;
        dbChannel.Persist();
        tuningDetail = new AnalogChannel();
        tuningDetail.IsTv = true;
        tuningDetail.Name = dbChannel.Name;
        tuningDetail.VideoSource = AnalogChannel.VideoInputType.YRYBYInput3;
        layer.AddTuningDetails(dbChannel, tuningDetail);
        layer.MapChannelToCard(card, dbChannel, false);
        layer.AddChannelToGroup(dbChannel, "All Channels");
      }
      MessageBox.Show(this, "Channels added.");
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
        SetVideoProcAmp(_configuration.Graph.Capture.VideoProcAmpValues);
      }
    }

    private void checkButton_Click(object sender, EventArgs e)
    {
      User user;
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before checking quality control");
          return;
        } else if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before checking quality control");
          return;
        }
        // Check if the card is locked for scanning.
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Checking quality control not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        user = new User();
        user.CardId = _cardNumber;
        AnalogChannel temp = new AnalogChannel();
        temp.VideoSource = AnalogChannel.VideoInputType.Tuner;
        temp.AudioSource = AnalogChannel.AudioInputType.Tuner;
        temp.IsRadio = false;
        temp.IsTv = true;
        RemoteControl.Instance.Tune(ref user, temp, -1);
        if (RemoteControl.Instance.SupportsQualityControl(_cardNumber))
        {
          _cardName = RemoteControl.Instance.CardName(_cardNumber);
          _devicePath = RemoteControl.Instance.CardDevice(_cardNumber);
          bitRateModeGroup.Enabled = RemoteControl.Instance.SupportsBitRateModes(_cardNumber);
          if (RemoteControl.Instance.SupportsPeakBitRateMode(_cardNumber))
          {
            vbrPeakPlayback.Enabled = true;
            vbrPeakRecord.Enabled = true;
          } else
          {
            vbrPeakPlayback.Enabled = false;
            vbrPeakRecord.Enabled = false;
          }
          if (RemoteControl.Instance.SupportsBitRate(_cardNumber))
          {
            bitRate.Enabled = true;
            customSettingsGroup.Enabled = true;
            customValue.Enabled = true;
            customValuePeak.Enabled = true;
          } else
          {
            bitRate.Enabled = false;
            customSettingsGroup.Enabled = false;
            customValue.Enabled = false;
            customValuePeak.Enabled = false;
          }
          _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
          customValue.Value = _configuration.CustomQualityValue;
          customValuePeak.Value = _configuration.CustomPeakQualityValue;
          SetBitRateModes();
          SetBitRate();
          ReCheckSettings();
        } else
        {
          Log.WriteFile("Card doesn't support quality control");
          MessageBox.Show("The used encoder doesn't support quality control.", "MediaPortal - TV Server management console", MessageBoxButtons.OK, MessageBoxIcon.Information);
          if(string.IsNullOrEmpty(_configuration.Graph.Capture.Name))
          {
            _configuration = Configuration.readConfiguration(_cardNumber, _cardName, _devicePath);
            ReCheckSettings();
          }
        }
      } finally
      {
        user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
      }
    }

    private void defaultValuesButton_Click(object sender, EventArgs e)
    {
      Dictionary<VideoProcAmpProperty, VideoQuality> map = _configuration.Graph.Capture.VideoProcAmpValues;
      VideoQuality quality;
      if (map.ContainsKey(VideoProcAmpProperty.Brightness))
      {
        quality = map[VideoProcAmpProperty.Brightness];
        brightnessScrollbar.Value = quality.DefaultValue;
        brightnessValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.Contrast))
      {
        quality = map[VideoProcAmpProperty.Contrast];
        contrastScrollbar.Value = quality.DefaultValue;
        contrastValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.Hue))
      {
        quality = map[VideoProcAmpProperty.Hue];
        hueScrollbar.Value = quality.DefaultValue;
        hueValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.Saturation))
      {
        quality = map[VideoProcAmpProperty.Saturation];
        saturationScrollbar.Value = quality.DefaultValue;
        saturationValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.Sharpness))
      {
        quality = map[VideoProcAmpProperty.Sharpness];
        sharpnessScrollbar.Value = quality.DefaultValue;
        sharpnessValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.Gamma))
      {
        quality = map[VideoProcAmpProperty.Gamma];
        gammaScrollbar.Value = quality.DefaultValue;
        gammaValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.ColorEnable))
      {
        quality = map[VideoProcAmpProperty.ColorEnable];
        colorEnableScrollbar.Value = quality.DefaultValue;
        colorEnableValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.WhiteBalance))
      {
        quality = map[VideoProcAmpProperty.WhiteBalance];
        whiteBalanceScrollbar.Value = quality.DefaultValue;
        whiteBalanceValue.Text = quality.DefaultValue.ToString();
      }
      if (map.ContainsKey(VideoProcAmpProperty.BacklightCompensation))
      {
        quality = map[VideoProcAmpProperty.BacklightCompensation];
        backlightCompensationScrollbar.Value = quality.DefaultValue;
        backlightCompensationValue.Text = quality.DefaultValue.ToString();
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
