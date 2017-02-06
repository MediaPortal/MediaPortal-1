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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuner : Form, ICiMenuEventCallback
  {
    private class TunerPropertyInfo
    {
      public int Value;
      public VideoOrCameraPropertyFlag ValueFlag;
      public TunerProperty DbProperty;

      public override string ToString()
      {
        return ((VideoOrCameraProperty)DbProperty.PropertyId).GetDescription();
      }
    }

    private const string ENCODER_NAME_AUTOMATIC = "Automatic";

    private Tuner _tuner;
    private IList<MPCheckBox> _supportedBroadcastStandardCheckBoxes = null;
    private AnalogTunerSettings _analogSettings = null;
    private StreamTunerSettings _streamSettings = null;
    private CiMenuState _caMenuState = CiMenuState.Closed;
    private int _caMenuChoiceCount = 0;
    private bool _enableVideoOrCameraPropertyValueUpdate = true;

    public FormEditTuner(int idTuner)
    {
      _tuner = ServiceAgents.Instance.TunerServiceAgent.GetTuner(idTuner, TunerRelation.None);
      InitializeComponent();
    }

    private void FormEditCard_Load(object sender, EventArgs e)
    {
      this.LogInfo("tuner: start edit, ID = {0}", _tuner.IdTuner);
      bool isAnalogOrCaptureTuner = (_tuner.SupportedBroadcastStandards & (int)(BroadcastStandard.MaskAnalog | BroadcastStandard.ExternalInput)) != 0;

      // general tab
      textBoxTunerName.Text = _tuner.Name;
      checkBoxUseForEpgGrabbing.Checked = _tuner.UseForEpgGrabbing;

      // Tuners can't be preloaded if they're part of a tuner group.
      if (_tuner.IdTunerGroup != null)
      {
        checkBoxPreload.Enabled = false;
        _tuner.Preload = false;
      }
      checkBoxPreload.Checked = _tuner.Preload;

      checkBoxAlwaysSendDiseqcCommands.Checked = _tuner.AlwaysSendDiseqcCommands;
      checkBoxAlwaysSendDiseqcCommands.Enabled = (_tuner.SupportedBroadcastStandards & (int)BroadcastStandard.MaskSatellite) != 0;

      // supported broadcast standards
      BroadcastStandard possibleBroadcastStandards = ServiceAgents.Instance.ControllerServiceAgent.PossibleBroadcastStandards(_tuner.IdTuner);
      int tabIndex = groupBoxGeneral.Controls.Count;
      int originalGroupBoxHeight = groupBoxGeneral.Height;
      SuspendLayout();
      groupBoxGeneral.SuspendLayout();
      MPLabel labelSupportedBroadcastStandards = new MPLabel();
      labelSupportedBroadcastStandards.AutoSize = true;
      labelSupportedBroadcastStandards.Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      labelSupportedBroadcastStandards.Location = new Point(6, 115);
      labelSupportedBroadcastStandards.Name = "labelSupportedBroadcastStandards";
      labelSupportedBroadcastStandards.TabIndex = tabIndex++;
      labelSupportedBroadcastStandards.Text = "Supported broadcast standards:";
      groupBoxGeneral.Controls.Add(labelSupportedBroadcastStandards);
      groupBoxGeneral.Height += 16;

      _supportedBroadcastStandardCheckBoxes = new List<MPCheckBox>();
      int verticalPosition = 132;
      const int verticalIncrement = 23;
      const int horizontalBase = 9;
      const int horizontalIncrement = 100;
      int horizontalPosition = horizontalBase;
      foreach (BroadcastStandard broadcastStandard in Enum.GetValues(typeof(BroadcastStandard)))
      {
        if (broadcastStandard != BroadcastStandard.Unknown && !broadcastStandard.ToString().StartsWith("Mask") && possibleBroadcastStandards.HasFlag(broadcastStandard))
        {
          MPCheckBox checkBox = new MPCheckBox();
          checkBox.Checked = (_tuner.SupportedBroadcastStandards & (int)broadcastStandard) != 0;
          checkBox.Location = new Point(horizontalPosition, verticalPosition);
          checkBox.Name = "checkBoxBroadcastStandard" + broadcastStandard.ToString();
          checkBox.Width = horizontalIncrement - 8;
          checkBox.TabIndex = tabIndex++;
          checkBox.Tag = broadcastStandard;
          checkBox.Text = broadcastStandard.GetDescription();
          groupBoxGeneral.Controls.Add(checkBox);
          _supportedBroadcastStandardCheckBoxes.Add(checkBox);

          if (horizontalPosition == horizontalBase + (2 * horizontalIncrement))
          {
            // New row.
            verticalPosition += verticalIncrement;
            groupBoxGeneral.Height += verticalIncrement;
            horizontalPosition = horizontalBase;
          }
          else
          {
            // Next space.
            horizontalPosition += horizontalIncrement;
          }
        }
      }
      if (horizontalPosition != horizontalBase)
      {
        groupBoxGeneral.Height += verticalIncrement;
      }
      groupBoxGeneral.PerformLayout();
      groupBoxGeneral.ResumeLayout(false);

      int heightChange = groupBoxGeneral.Height - originalGroupBoxHeight;
      groupBoxAdvanced.Top += heightChange;
      groupBoxDebug.Top += heightChange;
      PerformLayout();
      ResumeLayout(false);

      comboBoxIdleMode.Items.AddRange(typeof(TunerIdleMode).GetDescriptions());
      comboBoxIdleMode.SelectedItem = ((TunerIdleMode)_tuner.IdleMode).GetDescription();

      if (isAnalogOrCaptureTuner || !_tuner.ExternalId.StartsWith("@device"))
      {
        comboBoxBdaNetworkProvider.Enabled = false;
      }
      else
      {
        comboBoxBdaNetworkProvider.BeginUpdate();
        try
        {
          comboBoxBdaNetworkProvider.Enabled = true;
          foreach (BdaNetworkProvider availableNetworkProvider in ServiceAgents.Instance.TunerServiceAgent.ListAvailableBdaNetworkProviders())
          {
            comboBoxBdaNetworkProvider.Items.Add(availableNetworkProvider.GetDescription());
          }
          comboBoxBdaNetworkProvider.SelectedItem = ((BdaNetworkProvider)_tuner.BdaNetworkProvider).GetDescription();
        }
        finally
        {
          comboBoxBdaNetworkProvider.EndUpdate();
        }
      }

      if (isAnalogOrCaptureTuner)
      {
        comboBoxPidFilterMode.Enabled = false;
      }
      else
      {
        comboBoxPidFilterMode.Items.AddRange(typeof(PidFilterMode).GetDescriptions());
        comboBoxPidFilterMode.SelectedItem = ((PidFilterMode)_tuner.PidFilterMode).GetDescription();

        checkBoxTsMuxerDumpInputs.Enabled = false;
      }

      checkBoxUseCustomTuning.Checked = _tuner.UseCustomTuning;

      checkBoxTsWriterDumpInputs.Checked = _tuner.TsWriterInputDumpMask != 0;
      checkBoxTsWriterDisableCrcCheck.Checked = _tuner.DisableTsWriterCrcChecking;
      checkBoxTsMuxerDumpInputs.Checked = _tuner.TsMuxerInputDumpMask != 0;

      // conditional access tab
      checkBoxUseConditionalAccess.Checked = _tuner.UseConditionalAccess;
      textBoxConditionalAccessProviders.Text = _tuner.ConditionalAccessProviders;
      comboBoxCamType.Items.AddRange(typeof(CamType).GetDescriptions());
      comboBoxCamType.SelectedItem = ((CamType)_tuner.CamType).GetDescription();
      numericUpDownDecryptLimit.Value = _tuner.DecryptLimit;
      comboBoxMultiChannelDecryptMode.Items.AddRange(typeof(MultiChannelDecryptMode).GetDescriptions());
      comboBoxMultiChannelDecryptMode.SelectedItem = ((MultiChannelDecryptMode)_tuner.MultiChannelDecryptMode).GetDescription();
      SetConditionalAccessFieldAvailability();

      UpdateCaMenuFieldStates();
      ServiceAgents.Instance.EventServiceAgent.RegisterCiMenuCallbacks(this);

      // stream tab
      if (!((BroadcastStandard)_tuner.SupportedBroadcastStandards).HasFlag(BroadcastStandard.DvbIp))
      {
        tabPageStream.Dispose();
      }
      else
      {
        _streamSettings = ServiceAgents.Instance.TunerServiceAgent.GetStreamTunerSettings(_tuner.IdTuner);
        if (_streamSettings == null)
        {
          // This should not happen!
          this.LogWarn("tuner: failed to load stream settings");
          tabPageStream.Dispose();
        }
        else
        {
          DebugStreamTunerSettings(_streamSettings);

          numericUpDownStreamReceiveDataTimeLimit.Value = _streamSettings.ReceiveDataTimeLimit;
          numericUpDownStreamBufferSize.Value = _streamSettings.BufferSize;
          numericUpDownStreamBufferSizeMaximum.Value = _streamSettings.BufferSizeMaximum;
          numericUpDownStreamOpenConnectionAttemptLimit.Value = _streamSettings.OpenConnectionAttemptLimit;
          checkBoxStreamDumpInput.Checked = _streamSettings.DumpInput;
          numericUpDownStreamRtspCommandResponseTimeLimit.Value = _streamSettings.RtspCommandResponseTimeLimit;
          checkBoxStreamRtspSendCommandOptions.Checked = _streamSettings.RtspSendCommandOptions;
          checkBoxStreamRtspSendCommandDescribe.Checked = _streamSettings.RtspSendCommandDescribe;
          numericUpDownStreamFileRepeatCount.Value = _streamSettings.FileRepeatCount;
          numericUpDownStreamRtpSwitchToUdpPacketCount.Value = _streamSettings.RtpSwitchToUdpPacketCount;

          comboBoxStreamHttpRtpUdpInterface.BeginUpdate();
          try
          {
            comboBoxStreamHttpRtpUdpInterface.Items.Add("Default");
            comboBoxStreamHttpRtpUdpInterface.SelectedIndex = 0;
            foreach (string interfaceName in ServiceAgents.Instance.TunerServiceAgent.ListAvailableNetworkInterfaceNames())
            {
              comboBoxStreamHttpRtpUdpInterface.Items.Add(interfaceName);
              if (string.Equals(interfaceName, _streamSettings.NetworkInterface))
              {
                comboBoxStreamHttpRtpUdpInterface.SelectedIndex = comboBoxStreamHttpRtpUdpInterface.Items.Count - 1;
              }
            }
          }
          finally
          {
            comboBoxStreamHttpRtpUdpInterface.EndUpdate();
          }
        }
      }

      // analog and external input tabs
      if (!isAnalogOrCaptureTuner)
      {
        tabPageAnalog.Dispose();
        tabPageExternalInput.Dispose();
        return;
      }

      _analogSettings = ServiceAgents.Instance.TunerServiceAgent.GetAnalogTunerSettings(_tuner.IdTuner);
      if (_analogSettings == null)
      {
        // This should not happen!
        this.LogWarn("tuner: failed to load analog settings");
        tabPageAnalog.Dispose();
        tabPageExternalInput.Dispose();
        return;
      }
      DebugAnalogTunerSettings(_analogSettings);

      comboBoxSoftwareEncoderAudio.BeginUpdate();
      try
      {
        comboBoxSoftwareEncoderAudio.Items.Add(new AudioEncoder { Name = ENCODER_NAME_AUTOMATIC });
        comboBoxSoftwareEncoderAudio.SelectedIndex = 0;
        foreach (AudioEncoder encoder in ServiceAgents.Instance.TunerServiceAgent.ListAvailableSoftwareEncodersAudio())
        {
          comboBoxSoftwareEncoderAudio.Items.Add(encoder);
          if (encoder.IdAudioEncoder == _analogSettings.IdAudioEncoder)
          {
            comboBoxSoftwareEncoderAudio.SelectedIndex = comboBoxSoftwareEncoderAudio.Items.Count - 1;
          }
        }
      }
      finally
      {
        comboBoxSoftwareEncoderAudio.EndUpdate();
      }

      if ((_tuner.SupportedBroadcastStandards & (int)(BroadcastStandard.AnalogTelevision | BroadcastStandard.ExternalInput)) == 0)
      {
        // No video support. For example, an FM radio tuner.
        groupBoxVideo.Enabled = false;
        groupBoxVideoAndCameraProperties.Enabled = false;
        comboBoxSoftwareEncoderVideo.Enabled = false;
        tabPageExternalInput.Dispose();
        return;
      }

      comboBoxAnalogVideoStandard.Items.AddRange(typeof(AnalogVideoStandard).GetDescriptions(_analogSettings.SupportedVideoStandards, _analogSettings.SupportedVideoStandards == 0));
      comboBoxAnalogVideoStandard.SelectedItem = ((AnalogVideoStandard)_analogSettings.VideoStandard).GetDescription();
      comboBoxFrameSize.Items.AddRange(typeof(FrameSize).GetDescriptions(_analogSettings.SupportedFrameSizes, true));
      comboBoxFrameSize.SelectedItem = ((FrameSize)_analogSettings.FrameSize).GetDescription();
      comboBoxFrameRate.Items.AddRange(typeof(FrameRate).GetDescriptions(_analogSettings.SupportedFrameRates, true));
      comboBoxFrameRate.SelectedItem = ((FrameRate)_analogSettings.FrameRate).GetDescription();

      IList<TunerProperty> tunerProperties = ServiceAgents.Instance.TunerServiceAgent.ListAllTunerPropertiesByTuner(_tuner.IdTuner);
      if (tunerProperties == null || tunerProperties.Count == 0)
      {
        comboBoxVideoOrCameraProperty.Enabled = false;
      }
      else
      {
        this.LogDebug("tuner: properties...");
        comboBoxVideoOrCameraProperty.BeginUpdate();
        try
        {
          comboBoxVideoOrCameraProperty.Items.Clear();
          foreach (TunerProperty property in tunerProperties)
          {
            this.LogDebug("  ID = {0}, value = {1}, flag = {2}", (VideoOrCameraProperty)property.PropertyId, property.Value, (VideoOrCameraPropertyFlag)property.ValueFlags);
            TunerPropertyInfo propertyInfo = new TunerPropertyInfo
            {
              Value = property.Value,
              ValueFlag = (VideoOrCameraPropertyFlag)property.ValueFlags,
              DbProperty = property
            };
            comboBoxVideoOrCameraProperty.Items.Add(propertyInfo);
          }
          comboBoxVideoOrCameraProperty.SelectedIndex = 0;
        }
        finally
        {
          comboBoxVideoOrCameraProperty.EndUpdate();
        }
      }

      comboBoxSoftwareEncoderVideo.BeginUpdate();
      try
      {
        comboBoxSoftwareEncoderVideo.Items.Add(new VideoEncoder { Name = ENCODER_NAME_AUTOMATIC });
        comboBoxSoftwareEncoderVideo.SelectedIndex = 0;
        foreach (VideoEncoder encoder in ServiceAgents.Instance.TunerServiceAgent.ListAvailableSoftwareEncodersVideo())
        {
          comboBoxSoftwareEncoderVideo.Items.Add(encoder);
          if (encoder.IdVideoEncoder == _analogSettings.IdVideoEncoder)
          {
            comboBoxSoftwareEncoderVideo.SelectedIndex = comboBoxSoftwareEncoderVideo.Items.Count - 1;
          }
        }
      }
      finally
      {
        comboBoxSoftwareEncoderVideo.EndUpdate();
      }

      groupBoxEncoderSettings.Height = 48;

      comboBoxExternalInputSourceVideo.Items.AddRange(typeof(CaptureSourceVideo).GetDescriptions(_analogSettings.SupportedVideoSources, true));
      comboBoxExternalInputSourceVideo.SelectedItem = ((CaptureSourceVideo)_analogSettings.ExternalInputSourceVideo).GetDescription();
      comboBoxExternalInputSourceAudio.Items.AddRange(typeof(CaptureSourceAudio).GetDescriptions(_analogSettings.SupportedAudioSources, true));
      comboBoxExternalInputSourceAudio.SelectedItem = ((CaptureSourceAudio)_analogSettings.ExternalInputSourceAudio).GetDescription();
      comboBoxExternalInputCountry.Items.AddRange(CountryCollection.Instance.Countries);
      foreach (Country country in CountryCollection.Instance.Countries)
      {
        if (country.Id == _analogSettings.ExternalInputCountryId)
        {
          comboBoxExternalInputCountry.SelectedItem = country;
          break;
        }
      }
      numericUpDownExternalInputPhysicalChannelNumber.Value = _analogSettings.ExternalInputPhysicalChannelNumber;
      textBoxExternalTunerProgram.Text = _analogSettings.ExternalTunerProgram;
      textBoxExternalTunerProgramArguments.Text = _analogSettings.ExternalTunerProgramArguments;
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      // general tab
      textBoxTunerName.Text = textBoxTunerName.Text.Trim();
      if (string.IsNullOrWhiteSpace(textBoxTunerName.Text))
      {
        MessageBox.Show("Please enter a name for the tuner.", SetupControls.SectionSettings.MESSAGE_CAPTION);
        return;
      }

      this.LogInfo("tuner: save changes, ID = {0}", _tuner.IdTuner);
      _tuner.Name = textBoxTunerName.Text;
      _tuner.UseForEpgGrabbing = checkBoxUseForEpgGrabbing.Checked;
      _tuner.Preload = checkBoxPreload.Checked;
      _tuner.AlwaysSendDiseqcCommands = checkBoxAlwaysSendDiseqcCommands.Checked;

      foreach (MPCheckBox checkBox in _supportedBroadcastStandardCheckBoxes)
      {
        if (checkBox.Checked)
        {
          _tuner.SupportedBroadcastStandards |= (int)checkBox.Tag;
        }
        else
        {
          _tuner.SupportedBroadcastStandards &= ~(int)checkBox.Tag;
        }
      }

      _tuner.IdleMode = Convert.ToInt32(typeof(TunerIdleMode).GetEnumFromDescription((string)comboBoxIdleMode.SelectedItem));
      if (comboBoxBdaNetworkProvider.Enabled)
      {
        _tuner.BdaNetworkProvider = Convert.ToInt32(typeof(BdaNetworkProvider).GetEnumFromDescription((string)comboBoxBdaNetworkProvider.SelectedItem));
      }
      if (comboBoxPidFilterMode.Enabled)
      {
        _tuner.PidFilterMode = Convert.ToInt32(typeof(PidFilterMode).GetEnumFromDescription((string)comboBoxPidFilterMode.SelectedItem));
      }
      _tuner.UseCustomTuning = checkBoxUseCustomTuning.Checked;

      _tuner.TsWriterInputDumpMask = checkBoxTsWriterDumpInputs.Checked ? 3 : 0;    // bit 0 = TS input, bit 1 = OOB SI
      _tuner.DisableTsWriterCrcChecking = checkBoxTsWriterDisableCrcCheck.Checked;
      if (checkBoxTsMuxerDumpInputs.Enabled)
      {
        _tuner.TsMuxerInputDumpMask = checkBoxTsMuxerDumpInputs.Checked ? -1 : 0;   // -1 = 0xffffffff = all inputs
      }

      // conditional access tab
      _tuner.UseConditionalAccess = checkBoxUseConditionalAccess.Checked;
      _tuner.ConditionalAccessProviders = textBoxConditionalAccessProviders.Text;
      _tuner.CamType = Convert.ToInt32(typeof(CamType).GetEnumFromDescription((string)comboBoxCamType.SelectedItem));
      _tuner.DecryptLimit = (int)numericUpDownDecryptLimit.Value;
      _tuner.MultiChannelDecryptMode = Convert.ToInt32(typeof(MultiChannelDecryptMode).GetEnumFromDescription((string)comboBoxMultiChannelDecryptMode.SelectedItem));

      ServiceAgents.Instance.EventServiceAgent.UnRegisterCiMenuCallbacks(this, false);

      ServiceAgents.Instance.TunerServiceAgent.SaveTuner(_tuner);

      if (_streamSettings != null)
      {
        _streamSettings.ReceiveDataTimeLimit = (int)numericUpDownStreamReceiveDataTimeLimit.Value;
        _streamSettings.BufferSize = (int)numericUpDownStreamBufferSize.Value;
        _streamSettings.BufferSizeMaximum = (int)numericUpDownStreamBufferSizeMaximum.Value;
        _streamSettings.OpenConnectionAttemptLimit = (int)numericUpDownStreamOpenConnectionAttemptLimit.Value;
        _streamSettings.DumpInput = checkBoxStreamDumpInput.Checked;
        _streamSettings.RtspCommandResponseTimeLimit = (int)numericUpDownStreamRtspCommandResponseTimeLimit.Value;
        _streamSettings.RtspSendCommandOptions = checkBoxStreamRtspSendCommandOptions.Checked;
        _streamSettings.RtspSendCommandDescribe = checkBoxStreamRtspSendCommandDescribe.Checked;
        if (comboBoxStreamHttpRtpUdpInterface.SelectedIndex == 0)
        {
          _streamSettings.NetworkInterface = string.Empty;
        }
        else
        {
          _streamSettings.NetworkInterface = (string)comboBoxStreamHttpRtpUdpInterface.SelectedItem;
        }
        _streamSettings.FileRepeatCount = (int)numericUpDownStreamFileRepeatCount.Value;
        _streamSettings.RtpSwitchToUdpPacketCount = (int)numericUpDownStreamRtpSwitchToUdpPacketCount.Value;

        DebugStreamTunerSettings(_streamSettings);
        ServiceAgents.Instance.TunerServiceAgent.SaveStreamTunerSettings(_streamSettings);
      }

      if (_analogSettings == null)
      {
        return;
      }

      // analog tab
      if (groupBoxVideo.Enabled)
      {
        _analogSettings.VideoStandard = Convert.ToInt32(typeof(AnalogVideoStandard).GetEnumFromDescription((string)comboBoxAnalogVideoStandard.SelectedItem));
        _analogSettings.FrameSize = Convert.ToInt32(typeof(FrameSize).GetEnumFromDescription((string)comboBoxFrameSize.SelectedItem));
        _analogSettings.FrameRate = Convert.ToInt32(typeof(FrameRate).GetEnumFromDescription((string)comboBoxFrameRate.SelectedItem));
      }

      if (groupBoxVideoAndCameraProperties.Enabled && comboBoxVideoOrCameraProperty.Enabled)
      {
        IList<TunerProperty> propertiesToSave = new List<TunerProperty>(comboBoxVideoOrCameraProperty.Items.Count);
        foreach (TunerPropertyInfo property in comboBoxVideoOrCameraProperty.Items)
        {
          bool save = false;
          if (property.Value != property.DbProperty.Value)
          {
            this.LogInfo("tuner: property {0} value changed from {1} to {2}", (VideoOrCameraProperty)property.DbProperty.PropertyId, property.DbProperty.Value, property.Value);
            property.DbProperty.Value = property.Value;
            save = true;
          }
          if ((int)property.ValueFlag != property.DbProperty.ValueFlags)
          {
            this.LogInfo("tuner: property {0} value flag changed from {1} to {2}", (VideoOrCameraProperty)property.DbProperty.PropertyId, (VideoOrCameraPropertyFlag)property.DbProperty.ValueFlags, property.ValueFlag);
            property.DbProperty.ValueFlags = (int)property.ValueFlag;
            save = true;
          }
          if (save)
          {
            propertiesToSave.Add(property.DbProperty);
          }
        }
        if (propertiesToSave.Count > 0)
        {
          ServiceAgents.Instance.TunerServiceAgent.SaveTunerProperties(propertiesToSave);
        }
      }

      if (comboBoxSoftwareEncoderVideo.Enabled)
      {
        VideoEncoder videoEncoder = (VideoEncoder)comboBoxSoftwareEncoderVideo.SelectedItem;
        if (string.Equals(videoEncoder.Name, ENCODER_NAME_AUTOMATIC))
        {
          _analogSettings.IdVideoEncoder = null;
        }
        else
        {
          _analogSettings.IdVideoEncoder = videoEncoder.IdVideoEncoder;
        }
      }
      AudioEncoder audioEncoder = (AudioEncoder)comboBoxSoftwareEncoderAudio.SelectedItem;
      if (string.Equals(audioEncoder.Name, ENCODER_NAME_AUTOMATIC))
      {
        _analogSettings.IdAudioEncoder = null;
      }
      else
      {
        _analogSettings.IdAudioEncoder = audioEncoder.IdAudioEncoder;
      }

      if (!buttonEncoderSettingsCheckSupport.Visible)
      {
        if (comboBoxEncoderBitRateModeTimeShifting.Enabled)
        {
          _analogSettings.EncoderBitRateModeTimeShifting = Convert.ToInt32(typeof(EncodeMode).GetEnumFromDescription((string)comboBoxEncoderBitRateModeTimeShifting.SelectedItem));
          _analogSettings.EncoderBitRateModeRecording = Convert.ToInt32(typeof(EncodeMode).GetEnumFromDescription((string)comboBoxEncoderBitRateModeRecording.SelectedItem));
        }
        _analogSettings.EncoderBitRateTimeShifting = (int)numericUpDownEncoderBitRateValueTimeShifting.Value;
        _analogSettings.EncoderBitRateRecording = (int)numericUpDownEncoderBitRateValueRecording.Value;
        _analogSettings.EncoderBitRatePeakTimeShifting = (int)numericUpDownEncoderBitRateValuePeakTimeShifting.Value;
        _analogSettings.EncoderBitRatePeakRecording = (int)numericUpDownEncoderBitRateValuePeakRecording.Value;
      }

      // external input tab
      if ((_tuner.SupportedBroadcastStandards & (int)(BroadcastStandard.AnalogTelevision | BroadcastStandard.ExternalInput)) != 0)
      {
        _analogSettings.ExternalInputSourceVideo = Convert.ToInt32(typeof(CaptureSourceVideo).GetEnumFromDescription((string)comboBoxExternalInputSourceVideo.SelectedItem));
        _analogSettings.ExternalInputSourceAudio = Convert.ToInt32(typeof(CaptureSourceAudio).GetEnumFromDescription((string)comboBoxExternalInputSourceAudio.SelectedItem));
        _analogSettings.ExternalInputCountryId = ((Country)comboBoxExternalInputCountry.SelectedItem).Id;
        _analogSettings.ExternalInputPhysicalChannelNumber = (int)numericUpDownExternalInputPhysicalChannelNumber.Value;
        _analogSettings.ExternalTunerProgram = textBoxExternalTunerProgram.Text;
        _analogSettings.ExternalTunerProgramArguments = textBoxExternalTunerProgramArguments.Text;
      }

      DebugAnalogTunerSettings(_analogSettings);
      ServiceAgents.Instance.TunerServiceAgent.SaveAnalogTunerSettings(_analogSettings);

      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.LogInfo("tuner: cancel changes, ID = {0}", _tuner.IdTuner);
      ServiceAgents.Instance.EventServiceAgent.UnRegisterCiMenuCallbacks(this, false);
      Close();
    }

    private void DebugStreamTunerSettings(StreamTunerSettings settings)
    {
      this.LogDebug("tuner: stream settings...");
      this.LogDebug("  ID                      = {0}", settings.IdStreamTunerSettings);
      this.LogDebug("  receive data time limit = {0} ms", settings.ReceiveDataTimeLimit);
      this.LogDebug("  buffer size             = {0} kB", settings.BufferSize);
      this.LogDebug("  buffer size maximum     = {0} kB", settings.BufferSizeMaximum);
      this.LogDebug("  connect attempt limit   = {0}", settings.OpenConnectionAttemptLimit);
      this.LogDebug("  dump input              = {0}", settings.DumpInput);
      this.LogDebug("  RTSP...");
      this.LogDebug("    response time limit   = {0} ms", settings.RtspCommandResponseTimeLimit);
      this.LogDebug("    send OPTIONS command  = {0}", settings.RtspSendCommandOptions);
      this.LogDebug("    send DESCRIBE command = {0}", settings.RtspSendCommandDescribe);
      this.LogDebug("  network interface       = {0}", settings.NetworkInterface);
      this.LogDebug("  file repeat count       = {0}", settings.FileRepeatCount);
      this.LogDebug("  RTP to UDP packet count = {0}", settings.RtpSwitchToUdpPacketCount);
    }

    private void DebugAnalogTunerSettings(AnalogTunerSettings settings)
    {
      this.LogDebug("tuner: analog settings...");
      this.LogDebug("  ID                 = {0}", settings.IdAnalogTunerSettings);
      this.LogDebug("  video standard     = {0} ({1})", (AnalogVideoStandard)settings.VideoStandard, (AnalogVideoStandard)settings.SupportedVideoStandards);
      this.LogDebug("  frame size         = {0} ({1})", (FrameSize)settings.FrameSize, (FrameSize)settings.SupportedFrameSizes);
      this.LogDebug("  frame rate         = {0} ({1})", (FrameRate)settings.FrameRate, (FrameRate)settings.SupportedFrameRates);
      this.LogDebug("  encoders...");
      this.LogDebug("    video ID         = {0}", settings.IdVideoEncoder ?? 0);
      this.LogDebug("    audio ID         = {0}", settings.IdAudioEncoder ?? 0);
      this.LogDebug("  encoder timeshifting...");
      this.LogDebug("    mode             = {0}", (EncodeMode)settings.EncoderBitRateModeTimeShifting);
      this.LogDebug("    bit-rate         = {0} %", settings.EncoderBitRateTimeShifting);
      this.LogDebug("    peak bit-rate    = {0} %", settings.EncoderBitRatePeakTimeShifting);
      this.LogDebug("  encoder recording...");
      this.LogDebug("    mode             = {0}", (EncodeMode)settings.EncoderBitRateModeRecording);
      this.LogDebug("    bit-rate         = {0} %", settings.EncoderBitRateRecording);
      this.LogDebug("    peak bit-rate    = {0} %", settings.EncoderBitRatePeakRecording);
      this.LogDebug("  external input...");
      this.LogDebug("    video source     = {0} ({1})", (CaptureSourceVideo)settings.ExternalInputSourceVideo, (CaptureSourceVideo)settings.SupportedVideoSources);
      this.LogDebug("    audio source     = {0} ({1})", (CaptureSourceAudio)settings.ExternalInputSourceAudio, (CaptureSourceAudio)settings.SupportedAudioSources);
      this.LogDebug("    country          = {0}", settings.ExternalInputCountryId);
      this.LogDebug("    physical channel = {0}", settings.ExternalInputPhysicalChannelNumber);
      this.LogDebug("  external tuner...");
      this.LogDebug("    program          = {0}", settings.ExternalTunerProgram ?? string.Empty);
      this.LogDebug("    program args     = {0}", settings.ExternalTunerProgramArguments ?? string.Empty);
    }

    private void SetConditionalAccessFieldAvailability()
    {
      labelConditionalAccessProviders.Enabled = checkBoxUseConditionalAccess.Checked;
      textBoxConditionalAccessProviders.Enabled = checkBoxUseConditionalAccess.Checked;
      labelCamType.Enabled = checkBoxUseConditionalAccess.Checked;
      comboBoxCamType.Enabled = checkBoxUseConditionalAccess.Checked;
      labelDecryptLimit1.Enabled = checkBoxUseConditionalAccess.Checked;
      numericUpDownDecryptLimit.Enabled = checkBoxUseConditionalAccess.Checked;
      labelDecryptLimit2.Enabled = checkBoxUseConditionalAccess.Checked;
      labelMultiChannelDecryptMode.Enabled = checkBoxUseConditionalAccess.Checked;
      comboBoxMultiChannelDecryptMode.Enabled = checkBoxUseConditionalAccess.Checked;
      groupBoxCaMenu.Enabled = checkBoxUseConditionalAccess.Checked;
    }

    private void checkBoxConditionalAccessEnabled_CheckedChanged(object sender, EventArgs e)
    {
      SetConditionalAccessFieldAvailability();
    }

    #region CA menu

    private void buttonCaMenuOpen_Click(object sender, EventArgs e)
    {
      this.LogInfo("tuner: CA menu open, tuner ID = {0}", _tuner.IdTuner);
      try
      {
        if (!ServiceAgents.Instance.ControllerServiceAgent.CiMenuSupported(_tuner.IdTuner))
        {
          this.LogInfo("tuner: CA menu access is currently not possible");
          MessageBox.Show("This tuner doesn't support CA menu access, or the CAM is not present, compatible or ready yet." + Environment.NewLine + "(CAM inititialisation may require up to 30 seconds.)", SectionSettings.MESSAGE_CAPTION);
          return;
        }

        ServiceAgents.Instance.ControllerServiceAgent.SetCiMenuHandler(_tuner.IdTuner);
        if (!ServiceAgents.Instance.ControllerServiceAgent.EnterCiMenu(_tuner.IdTuner))
        {
          throw new Exception("Server returned error result.");
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner: failed to open CA menu, tuner ID = {0}", _tuner.IdTuner);
        MessageBox.Show("Failed to open the menu. " + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void buttonCaMenuBackClose_Click(object sender, EventArgs e)
    {
      this.LogInfo("tuner: CA menu back/close, tuner ID = {0}, menu state = {1}", _tuner.IdTuner, _caMenuState);
      try
      {
        bool success = true;
        if (_caMenuState == CiMenuState.NoChoices || _caMenuState == CiMenuState.Ready)
        {
          success = ServiceAgents.Instance.ControllerServiceAgent.SelectMenu(_tuner.IdTuner, 0); // back
          _caMenuState = CiMenuState.Closed;
        }
        else if (_caMenuState == CiMenuState.Request)
        {
          success = ServiceAgents.Instance.ControllerServiceAgent.SendMenuAnswer(_tuner.IdTuner, true, null);
          _caMenuState = CiMenuState.Ready;
        }

        if (!success)
        {
          throw new Exception("Server returned error result.");
        }

        UpdateCaMenuFieldStates();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner: failed to back/close CA menu, tuner ID = {0}, menu state = {1}", _tuner.IdTuner, _caMenuState);
        MessageBox.Show("Failed to close the menu. " + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void buttonCaMenuOkaySelect_Click(object sender, EventArgs e)
    {
      this.LogInfo("tuner: CA menu okay/select, tuner ID = {0}, menu state = {1}, selected item = {2}, answer = {3}", _tuner.IdTuner, _caMenuState, listBoxCaMenuChoices.SelectedIndex, textBoxCaMenuAnswer.Text);
      try
      {
        bool success = true;
        if (_caMenuState == CiMenuState.Ready && listBoxCaMenuChoices.SelectedIndex != -1)
        {
          success = ServiceAgents.Instance.ControllerServiceAgent.SelectMenu(_tuner.IdTuner, Convert.ToByte(listBoxCaMenuChoices.SelectedIndex + 1));
        }
        else if (_caMenuState == CiMenuState.Request)
        {
          success = ServiceAgents.Instance.ControllerServiceAgent.SendMenuAnswer(_tuner.IdTuner, false, textBoxCaMenuAnswer.Text);
          _caMenuState = CiMenuState.Ready;
        }

        if (!success)
        {
          throw new Exception("Server returned error result.");
        }

        UpdateCaMenuFieldStates();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner: failed to okay/select CA menu, tuner ID = {0}, menu state = {1}, selected item = {2}, answer = {3}", _tuner.IdTuner, _caMenuState, listBoxCaMenuChoices.SelectedIndex, textBoxCaMenuAnswer.Text);
        MessageBox.Show("Failed to select or send response. " + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// Handles all CiMenu actions from callback
    /// </summary>
    /// <param name="menu">complete CI menu object</param>
    public void CiMenuCallback(CiMenu menu)
    {
      try
      {
        this.LogInfo("tuner: CA menu response, tuner ID = {0}", _tuner.IdTuner);
        HandleCaMenuCallBack(menu);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner: exception in CA menu call back, tuner ID = {0}, menu state = {1}", _tuner.IdTuner, _caMenuState);
        menu = new CiMenu("Unexpected Error", "An unexpected error occurred.", string.Empty, CiMenuState.Error);
        HandleCaMenuCallBack(menu);
      }
    }

    private void HandleCaMenuCallBack(CiMenu menu)
    {
      _caMenuState = menu.State;

      switch (_caMenuState)
      {
        // choices available, so show them
        case CiMenuState.Ready:
          labelCaMenuTitle.Text = menu.Title ?? string.Empty;
          labelCaMenuSubTitle.Text = menu.Subtitle ?? string.Empty;
          labelCaMenuFooter.Text = menu.BottomText ?? string.Empty;
          _caMenuChoiceCount = menu.NumChoices;

          listBoxCaMenuChoices.Items.Clear();

          // no choices then we are ready yet
          if (_caMenuChoiceCount == 0)
          {
            _caMenuState = CiMenuState.NoChoices;
          }
          else
          {
            foreach (CiMenuEntry entry in menu.MenuEntries)
            {
              listBoxCaMenuChoices.Items.Add(entry);
            }
          }
          break;

        // errors and menu options with no choices
        case CiMenuState.Error:
        case CiMenuState.NoChoices:
          labelCaMenuTitle.Text = menu.Title ?? string.Empty;
          labelCaMenuSubTitle.Text = menu.Subtitle ?? string.Empty;
          labelCaMenuFooter.Text = menu.BottomText ?? string.Empty;
          _caMenuChoiceCount = menu.NumChoices;
          break;

        // requests require users input so open keyboard
        case CiMenuState.Request:
          UpdateCaMenuFieldStates();
          labelCaMenuEnquiry.Text = string.Format("{0} ({1} characters)", menu.RequestText ?? string.Empty, menu.AnswerLength);
          textBoxCaMenuAnswer.MaxLength = (int)menu.AnswerLength;
          textBoxCaMenuAnswer.Text = string.Empty;
          textBoxCaMenuAnswer.Focus();
          break;

        case CiMenuState.Close:
          _caMenuState = CiMenuState.Closed;
          break;
      }

      this.Invoke(new MethodInvoker(delegate()
      {
        UpdateCaMenuFieldStates();
      }));
    }

    private void UpdateCaMenuFieldStates()
    {
      if (_caMenuState == CiMenuState.Closed)
      {
        this.LogDebug("tuner: CA menu closed");
        labelCaMenuTitle.Text = string.Empty;
        labelCaMenuSubTitle.Text = string.Empty;
        labelCaMenuFooter.Text = string.Empty;
        labelCaMenuEnquiry.Text = string.Empty;
        textBoxCaMenuAnswer.Text = string.Empty;
        listBoxCaMenuChoices.Items.Clear();
        buttonCaMenuOpen.Enabled = true;
        buttonCaMenuBackClose.Enabled = false;
        buttonCaMenuOkaySelect.Enabled = false;
        labelCaMenuEnquiry.Visible = false;
        textBoxCaMenuAnswer.Visible = false;
        return;
      }

      buttonCaMenuOpen.Enabled = false;
      buttonCaMenuBackClose.Enabled = _caMenuState == CiMenuState.Ready || _caMenuState == CiMenuState.Request || _caMenuState == CiMenuState.NoChoices;
      buttonCaMenuOkaySelect.Enabled = _caMenuState == CiMenuState.Ready || _caMenuState == CiMenuState.Request;
      labelCaMenuEnquiry.Visible = _caMenuState == CiMenuState.Request;
      textBoxCaMenuAnswer.Visible = _caMenuState == CiMenuState.Request;

      this.LogDebug("tuner: CA menu...");
      this.LogDebug("  state       = {0}", _caMenuState);
      this.LogDebug("  title       = {0}", labelCaMenuTitle.Text);
      this.LogDebug("  sub-title   = {0}", labelCaMenuSubTitle.Text);
      this.LogDebug("  footer      = {0}", labelCaMenuFooter.Text);
      this.LogDebug("  enquiry     = {0}", labelCaMenuEnquiry.Text);
      this.LogDebug("  ans. length = {0}", textBoxCaMenuAnswer.MaxLength);
      this.LogDebug("  entries...");
      foreach (string entry in listBoxCaMenuChoices.Items)
      {
        this.LogDebug("    {0}", entry);
      }
    }

    #endregion

    #region stream

    private void numericUpDownStreamBufferSize_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownStreamBufferSize.Value > numericUpDownStreamBufferSizeMaximum.Value)
      {
        numericUpDownStreamBufferSizeMaximum.Value = numericUpDownStreamBufferSize.Value;
      }
    }

    private void numericUpDownStreamBufferSizeMaximum_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownStreamBufferSizeMaximum.Value < numericUpDownStreamBufferSize.Value)
      {
        numericUpDownStreamBufferSize.Value = numericUpDownStreamBufferSizeMaximum.Value;
      }
    }

    #endregion

    #region video or camera property

    private void comboBoxVideoOrCameraProperty_SelectedIndexChanged(object sender, EventArgs e)
    {
      _enableVideoOrCameraPropertyValueUpdate = false;

      // Adjust scroll bar limits first, otherwise the new property value might
      // be out of bounds for the scroll bar.
      TunerPropertyInfo info = (TunerPropertyInfo)comboBoxVideoOrCameraProperty.SelectedItem;
      checkBoxVideoOrCameraPropertyValue.Enabled = info.DbProperty.PossibleValueFlags == (int)(VideoOrCameraPropertyFlag.Auto | VideoOrCameraPropertyFlag.Manual);
      scrollBarVideoOrCameraPropertyValue.Minimum = info.DbProperty.Minimum;
      scrollBarVideoOrCameraPropertyValue.Maximum = info.DbProperty.Maximum;
      scrollBarVideoOrCameraPropertyValue.SmallChange = info.DbProperty.Step;
      scrollBarVideoOrCameraPropertyValue.LargeChange = info.DbProperty.Step;

      checkBoxVideoOrCameraPropertyValue.Checked = info.ValueFlag == VideoOrCameraPropertyFlag.Auto;
      scrollBarVideoOrCameraPropertyValue.Enabled = !checkBoxVideoOrCameraPropertyValue.Checked;
      scrollBarVideoOrCameraPropertyValue.Value = info.Value;
      labelVideoOrCameraPropertyValueDisplay.Text = info.Value.ToString();

      _enableVideoOrCameraPropertyValueUpdate = true;
    }

    private void checkBoxVideoOrCameraPropertyValue_CheckedChanged(object sender, EventArgs e)
    {
      if (_enableVideoOrCameraPropertyValueUpdate)
      {
        TunerPropertyInfo info = (TunerPropertyInfo)comboBoxVideoOrCameraProperty.SelectedItem;
        if (checkBoxVideoOrCameraPropertyValue.Checked)
        {
          info.ValueFlag = VideoOrCameraPropertyFlag.Auto;
        }
        else
        {
          info.ValueFlag = VideoOrCameraPropertyFlag.Manual;
        }
        scrollBarVideoOrCameraPropertyValue.Enabled = checkBoxVideoOrCameraPropertyValue.Checked;
      }
    }

    private void scrollBarVideoOrCameraPropertyValue_ValueChanged(object sender, EventArgs e)
    {
      if (_enableVideoOrCameraPropertyValueUpdate)
      {
        TunerPropertyInfo info = (TunerPropertyInfo)comboBoxVideoOrCameraProperty.SelectedItem;
        info.Value = scrollBarVideoOrCameraPropertyValue.Value;
        labelVideoOrCameraPropertyValueDisplay.Text = info.Value.ToString();
      }
    }

    private void buttonRestoreDefault_Click(object sender, EventArgs e)
    {
      TunerPropertyInfo info = (TunerPropertyInfo)comboBoxVideoOrCameraProperty.SelectedItem;
      checkBoxVideoOrCameraPropertyValue.Checked = ((VideoOrCameraPropertyFlag)info.DbProperty.PossibleValueFlags).HasFlag(VideoOrCameraPropertyFlag.Auto);
      scrollBarVideoOrCameraPropertyValue.Value = info.DbProperty.Default;
    }

    private void buttonRestoreAllDefaults_Click(object sender, EventArgs e)
    {
      foreach (TunerPropertyInfo info in comboBoxVideoOrCameraProperty.Items)
      {
        if (((VideoOrCameraPropertyFlag)info.DbProperty.PossibleValueFlags).HasFlag(VideoOrCameraPropertyFlag.Auto))
        {
          info.ValueFlag = VideoOrCameraPropertyFlag.Auto;
        }
        else
        {
          info.ValueFlag = VideoOrCameraPropertyFlag.Manual;
        }
        info.Value = info.DbProperty.Default;
      }
      buttonRestoreDefault_Click(null, null);
    }

    #endregion

    #region encoder settings

    private void buttonEncoderSettingsCheckSupport_Click(object sender, EventArgs e)
    {
      this.LogInfo("tuner: check encoder setting support, tuner ID = {0}", _tuner.IdTuner);
      EncodeMode supportedEncodeModes;
      bool canSetBitRate;
      if (!ServiceAgents.Instance.ControllerServiceAgent.GetSupportedQualityControlFeatures(_tuner.IdTuner, out supportedEncodeModes, out canSetBitRate))
      {
        this.LogError("tuner: failed to get supported features, tuner ID = {0}", _tuner.IdTuner);
        MessageBox.Show("Support check failed. " + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      this.LogInfo("tuner: encoder settings support result...");
      this.LogInfo("  supported encode modes = [{0}]", supportedEncodeModes);
      this.LogInfo("  can set bit-rate?      = {0}", canSetBitRate);
      bool onlySupportsOneEncodeMode =
      (
        supportedEncodeModes == EncodeMode.Default ||
        (supportedEncodeModes & (supportedEncodeModes - 1)) == 0
      );
      if (onlySupportsOneEncodeMode && !canSetBitRate)
      {
        MessageBox.Show("This tuner does not support encoder settings.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      groupBoxEncoderSettings.Height = 120;
      buttonEncoderSettingsCheckSupport.Visible = false;

      labelEncoderSettingsTimeShifting.Visible = true;
      labelEncoderBitRateModeTimeShifting.Visible = true;
      comboBoxEncoderBitRateModeTimeShifting.Visible = true;
      labelEncoderBitRateValueTimeShifting.Visible = true;
      numericUpDownEncoderBitRateValueTimeShifting.Visible = true;
      labelEncoderBitRateValueUnitTimeShifting.Visible = true;
      labelEncoderBitRateValuePeakTimeShifting.Visible = true;
      numericUpDownEncoderBitRateValuePeakTimeShifting.Visible = true;
      labelEncoderBitRateValuePeakUnitTimeShifting.Visible = true;

      labelEncoderSettingsRecording.Visible = true;
      labelEncoderBitRateModeRecording.Visible = true;
      comboBoxEncoderBitRateModeRecording.Visible = true;
      labelEncoderBitRateValueRecording.Visible = true;
      numericUpDownEncoderBitRateValueRecording.Visible = true;
      labelEncoderBitRateValueUnitRecording.Visible = true;
      labelEncoderBitRateValuePeakRecording.Visible = true;
      numericUpDownEncoderBitRateValuePeakRecording.Visible = true;
      labelEncoderBitRateValuePeakUnitRecording.Visible = true;

      if (onlySupportsOneEncodeMode)
      {
        comboBoxEncoderBitRateModeTimeShifting.Enabled = false;
        comboBoxEncoderBitRateModeRecording.Enabled = false;
      }
      else
      {
        comboBoxEncoderBitRateModeTimeShifting.BeginUpdate();
        comboBoxEncoderBitRateModeRecording.BeginUpdate();
        try
        {
          comboBoxEncoderBitRateModeTimeShifting.Items.Add(EncodeMode.Default.GetDescription());
          comboBoxEncoderBitRateModeRecording.Items.Add(EncodeMode.Default.GetDescription());
          if (supportedEncodeModes.HasFlag(EncodeMode.ConstantBitRate))
          {
            comboBoxEncoderBitRateModeTimeShifting.Items.Add(EncodeMode.ConstantBitRate.GetDescription());
            comboBoxEncoderBitRateModeRecording.Items.Add(EncodeMode.ConstantBitRate.GetDescription());
          }
          if (supportedEncodeModes.HasFlag(EncodeMode.VariableBitRate))
          {
            comboBoxEncoderBitRateModeTimeShifting.Items.Add(EncodeMode.VariableBitRate.GetDescription());
            comboBoxEncoderBitRateModeRecording.Items.Add(EncodeMode.VariableBitRate.GetDescription());
          }
          if (supportedEncodeModes.HasFlag(EncodeMode.VariablePeakBitRate))
          {
            comboBoxEncoderBitRateModeTimeShifting.Items.Add(EncodeMode.VariablePeakBitRate.GetDescription());
            comboBoxEncoderBitRateModeRecording.Items.Add(EncodeMode.VariablePeakBitRate.GetDescription());
          }
          comboBoxEncoderBitRateModeTimeShifting.SelectedItem = ((EncodeMode)_analogSettings.EncoderBitRateModeTimeShifting).GetDescription();
          comboBoxEncoderBitRateModeRecording.SelectedItem = ((EncodeMode)_analogSettings.EncoderBitRateModeRecording).GetDescription();
          if (comboBoxEncoderBitRateModeTimeShifting.SelectedItem == null)
          {
            comboBoxEncoderBitRateModeTimeShifting.SelectedIndex = 0;
          }
          if (comboBoxEncoderBitRateModeRecording.SelectedItem == null)
          {
            comboBoxEncoderBitRateModeRecording.SelectedIndex = 0;
          }
        }
        finally
        {
          comboBoxEncoderBitRateModeTimeShifting.EndUpdate();
          comboBoxEncoderBitRateModeRecording.EndUpdate();
        }
      }

      numericUpDownEncoderBitRateValueTimeShifting.Value = _analogSettings.EncoderBitRateTimeShifting;
      numericUpDownEncoderBitRateValueRecording.Value = _analogSettings.EncoderBitRateRecording;
      if (!canSetBitRate)
      {
        numericUpDownEncoderBitRateValueTimeShifting.Enabled = false;
        numericUpDownEncoderBitRateValueRecording.Enabled = false;
      }

      numericUpDownEncoderBitRateValuePeakTimeShifting.Value = _analogSettings.EncoderBitRatePeakTimeShifting;
      numericUpDownEncoderBitRateValuePeakRecording.Value = _analogSettings.EncoderBitRatePeakRecording;
      if (!supportedEncodeModes.HasFlag(EncodeMode.VariablePeakBitRate))
      {
        numericUpDownEncoderBitRateValuePeakTimeShifting.Enabled = false;
        numericUpDownEncoderBitRateValuePeakRecording.Enabled = false;
      }
    }

    private void comboBoxEncoderBitRateModeTimeShifting_SelectedIndexChanged(object sender, EventArgs e)
    {
      numericUpDownEncoderBitRateValuePeakTimeShifting.Enabled = string.Equals(comboBoxEncoderBitRateModeTimeShifting.SelectedItem, EncodeMode.VariablePeakBitRate.GetDescription());
    }

    private void comboBoxEncoderBitRateModeRecording_SelectedIndexChanged(object sender, EventArgs e)
    {
      numericUpDownEncoderBitRateValuePeakRecording.Enabled = string.Equals(comboBoxEncoderBitRateModeRecording.SelectedItem, EncodeMode.VariablePeakBitRate.GetDescription());
    }

    private void numericUpDownEncoderBitRateValueTimeShifting_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownEncoderBitRateValuePeakTimeShifting.Value < numericUpDownEncoderBitRateValueTimeShifting.Value)
      {
        numericUpDownEncoderBitRateValuePeakTimeShifting.Value = numericUpDownEncoderBitRateValueTimeShifting.Value;
      }
    }

    private void numericUpDownEncoderBitRateValuePeakTimeShifting_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownEncoderBitRateValuePeakTimeShifting.Value < numericUpDownEncoderBitRateValueTimeShifting.Value)
      {
        numericUpDownEncoderBitRateValueTimeShifting.Value = numericUpDownEncoderBitRateValuePeakTimeShifting.Value;
      }
    }

    private void numericUpDownEncoderBitRateValueRecording_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownEncoderBitRateValuePeakRecording.Value < numericUpDownEncoderBitRateValueRecording.Value)
      {
        numericUpDownEncoderBitRateValuePeakRecording.Value = numericUpDownEncoderBitRateValueRecording.Value;
      }
    }

    private void numericUpDownEncoderBitRateValuePeakRecording_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownEncoderBitRateValuePeakRecording.Value < numericUpDownEncoderBitRateValueRecording.Value)
      {
        numericUpDownEncoderBitRateValueRecording.Value = numericUpDownEncoderBitRateValuePeakRecording.Value;
      }
    }

    #endregion

    #region external tuner

    private void comboBoxExternalInputSourceVideo_SelectedIndexChanged(object sender, EventArgs e)
    {
      CaptureSourceVideo videoSource = (CaptureSourceVideo)typeof(CaptureSourceVideo).GetEnumFromDescription((string)comboBoxExternalInputSourceVideo.SelectedItem);
      comboBoxExternalInputCountry.Enabled = videoSource == CaptureSourceVideo.Tuner;
      numericUpDownExternalInputPhysicalChannelNumber.Enabled = comboBoxExternalInputCountry.Enabled;
    }

    private void buttonExternalTunerProgramBrowse_Click(object sender, EventArgs e)
    {
      if (openFileDialogExternalTunerProgram.ShowDialog() == DialogResult.OK)
      {
        textBoxExternalTunerProgram.Text = openFileDialogExternalTunerProgram.FileName;
      }
    }

    #endregion
  }
}