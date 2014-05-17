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
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A WDM analog DirectShow tuner and TV audio graph component.
  /// </summary>
  internal class Tuner : ComponentBase
  {
    #region constants

    private static readonly RegistryHive[] FREQUENCY_OVERRIDE_REGISTRY_HIVES = new RegistryHive[2] { RegistryHive.LocalMachine, RegistryHive.CurrentUser };
    private static List<RegistryView> FREQUENCY_OVERRIDE_REGISTRY_VIEWS = new List<RegistryView>();

    #endregion

    #region variables

    /// <summary>
    /// The tuner device.
    /// </summary>
    private DsDevice _deviceTuner = null;

    /// <summary>
    /// The TV audio device.
    /// </summary>
    private DsDevice _deviceTvAudio = null;

    /// <summary>
    /// The tuner filter.
    /// </summary>
    private IBaseFilter _filterTuner = null;

    /// <summary>
    /// The TV audio filter.
    /// </summary>
    private IBaseFilter _filterTvAudio = null;

    /// <summary>
    /// A set of flags indicating which tuning modes the tuner supports.
    /// </summary>
    private AMTunerModeType _supportedTuningModes = AMTunerModeType.Default;

    /// <summary>
    /// A set of flags indicating which audio modes the audio hardware supports.
    /// </summary>
    private TVAudioMode _supportedTvAudioModes = TVAudioMode.None;

    /// <summary>
    /// The current tuning parameters.
    /// </summary>
    private AnalogChannel _currentChannel = null;

    /// <summary>
    /// The downstream crossbar component tuner video input pin index.
    /// </summary>
    private int _crossbarInputPinIndexVideo = -1;

    /// <summary>
    /// The downstream crossbar component tuner audio input pin index.
    /// </summary>
    private int _crossbarInputPinIndexAudio = -1;

    /// <summary>
    /// An internal variable, used to control which implementation of
    /// ConnectFilters() is used during loading.
    /// </summary>
    private bool _useBaseConnectionMethod = false;

    #endregion

    #region properties

    /// <summary>
    /// Get the tuner device.
    /// </summary>
    public DsDevice Device
    {
      get
      {
        return _deviceTuner;
      }
    }

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="Tuner"/> class.
    /// </summary>
    public Tuner()
    {
      if (FREQUENCY_OVERRIDE_REGISTRY_VIEWS.Count == 0)
      {
        // When running under WOW64 we must explicitly set 64 bit registry
        // key values for drivers. Refer to JIRA 2959 (mantis 3956).
        FREQUENCY_OVERRIDE_REGISTRY_VIEWS.Add(RegistryView.Default);
        if (OSInfo.OSInfo.Is64BitOs() && IntPtr.Size != 8)
        {
          FREQUENCY_OVERRIDE_REGISTRY_VIEWS.Add(RegistryView.Registry64);
        }
      }
    }

    #endregion

    /// <summary>
    /// Load the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    /// <param name="productInstanceId">A common identifier shared by the tuner's components.</param>
    /// <param name="crossbar">The crossbar component.</param>
    public void PerformLoading(IFilterGraph2 graph, string productInstanceId, Crossbar crossbar)
    {
      this.LogDebug("WDM analog tuner: perform loading");
      _crossbarInputPinIndexVideo = crossbar.PinIndexInputTunerVideo;
      _crossbarInputPinIndexAudio = crossbar.PinIndexInputTunerAudio;
      _useBaseConnectionMethod = false;
      int expectedConnectionCount = 0;
      if (_crossbarInputPinIndexVideo >= 0)
      {
        expectedConnectionCount++;
      }
      if (_crossbarInputPinIndexAudio >= 0)
      {
        expectedConnectionCount++;
      }

      this.LogDebug("WDM analog tuner: add tuner filter");
      int connectionCount = AddAndConnectFilterFromCategory(graph, FilterCategory.AMKSTVTuner, crossbar.Filter, productInstanceId, out _filterTuner, out _deviceTuner);
      if (connectionCount != expectedConnectionCount)
      {
        if (connectionCount == 0 && _crossbarInputPinIndexVideo >= 0)
        {
          this.LogWarn("WDM analog tuner: failed to connect tuner filter to crossbar, allowing crossbar to operate without tuner and TV audio filters");
        }
        else
        {
          this.LogDebug("WDM analog tuner: add TV audio filter");
          if (_filterTuner != null)
          {
            // We prefer to find the TV audio filter by connection to the tuner
            // filter. This is because it seems some TV audio filters don't
            // connect to the crossbar input pins for some reason.
            _useBaseConnectionMethod = true;
            connectionCount = AddAndConnectFilterFromCategory(graph, FilterCategory.AMKSTVAudio, _filterTuner, productInstanceId, out _filterTvAudio, out _deviceTvAudio);
            if (connectionCount == 0)
            {
              this.LogWarn("WDM analog tuner: failed to connect TV audio filter to tuner, allowing crossbar to operate with tuner only");
            }
            else
            {
              this.LogDebug("WDM analog tuner: connect TV audio to crossbar");
              _useBaseConnectionMethod = false;
              connectionCount = ConnectFilters(graph, crossbar.Filter, _filterTvAudio);
              if (connectionCount == 0)
              {
                this.LogWarn("WDM analog tuner: failed to connect TV audio filter to crossbar, allowing crossbar to operate with unconnected TV audio pin");
              }
            }
          }
          else
          {
            connectionCount = AddAndConnectFilterFromCategory(graph, FilterCategory.AMKSTVAudio, crossbar.Filter, productInstanceId, out _filterTvAudio, out _deviceTvAudio);
            if (connectionCount == 0)
            {
              this.LogWarn("WDM analog tuner: failed to connect TV audio filter to crossbar, allowing crossbar to operate without tuner and TV audio filters");
            }
            else
            {
              this.LogDebug("WDM analog tuner: add audio tuner filter");
              connectionCount = AddAndConnectFilterFromCategory(graph, FilterCategory.AMKSTVTuner, crossbar.Filter, productInstanceId, out _filterTuner, out _deviceTuner);
              if (connectionCount == 0)
              {
                throw new TvException("Failed to connect audio tuner filter after connecting TV audio filter.");
              }
            }
          }
        }
      }

      CheckCapabilities();
    }

    /// <summary>
    /// Make as many direct connections as possible between two DirectShow filters.
    /// </summary>
    /// <param name="graph">The graph containing the two filters.</param>
    /// <param name="filterDownstream">The downstream filter.</param>
    /// <param name="filterUpstream">The upstream filter candidate.</param>
    /// <returns>the number of pin connections between the upstream and downstream filter</returns>
    protected override int ConnectFilters(IFilterGraph2 graph, IBaseFilter filterDownstream, IBaseFilter filterUpstream)
    {
      if (_useBaseConnectionMethod)
      {
        return base.ConnectFilters(graph, filterDownstream, filterUpstream);
      }

      this.LogDebug("WDM analog tuner: connect filters");
      int pinCountConnected = 0;
      int pinCountUnconnected = 0;

      IPin pinDownstreamVideo = null;
      IPin pinDownstreamAudio = null;
      try
      {
        if (_crossbarInputPinIndexVideo >= 0 && _filterTuner == null)
        {
          pinDownstreamVideo = DsFindPin.ByDirection(filterDownstream, PinDirection.Input, _crossbarInputPinIndexVideo);
          pinCountUnconnected++;
        }
        if (_crossbarInputPinIndexAudio >= 0)
        {
          pinDownstreamAudio = DsFindPin.ByDirection(filterDownstream, PinDirection.Input, _crossbarInputPinIndexAudio);
          pinCountUnconnected++;
        }

        IEnumPins pinEnumUpstream;
        int hr = filterUpstream.EnumPins(out pinEnumUpstream);
        HResult.ThrowException(hr, "Failed to obtain pin enumerator for upstream filter.");
        try
        {
          int pinIndex = 0;
          int pinCount = 0;
          IPin[] pinsUpstream = new IPin[2];
          while (pinEnumUpstream.Next(1, pinsUpstream, out pinCount) == (int)HResult.Severity.Success && pinCount == 1)
          {
            IPin pinUpstream = pinsUpstream[0];
            try
            {
              // We're not interested in input pins on the upstream filter.
              PinDirection direction;
              hr = pinUpstream.QueryDirection(out direction);
              HResult.ThrowException(hr, "Failed to query pin direction for upstream pin.");
              if (direction == PinDirection.Input)
              {
                this.LogDebug("WDM analog tuner: upstream pin {0} is an input pin", pinIndex++);
                continue;
              }

              // We can't use pins that are already connected.
              IPin tempPin = null;
              hr = pinUpstream.ConnectedTo(out tempPin);
              if (hr == (int)HResult.Severity.Success && tempPin != null)
              {
                this.LogDebug("WDM analog tuner: upstream output pin {0} already connected", pinIndex++);
                Release.ComObject("WDM analog tuner upstream filter connected pin", ref tempPin);
                continue;
              }

              // Try to connect the upstream output pin with a downstream input
              // pin. Either the video or audio pin.
              List<KeyValuePair<string, IPin>> pinsDownstream = new List<KeyValuePair<string, IPin>>();
              if (pinCountUnconnected == 1)
              {
                if (pinDownstreamVideo != null)
                {
                  pinsDownstream.Add(new KeyValuePair<string, IPin>("video", pinDownstreamVideo));
                }
                else
                {
                  pinsDownstream.Add(new KeyValuePair<string, IPin>("audio", pinDownstreamAudio));
                }
              }
              else
              {
                bool isVideo;
                if (IsVideoOrAudioPin(pinUpstream, out isVideo) && !isVideo)
                {
                  pinsDownstream.Add(new KeyValuePair<string, IPin>("audio", pinDownstreamAudio));
                  pinsDownstream.Add(new KeyValuePair<string, IPin>("video", pinDownstreamVideo));
                }
                else
                {
                  pinsDownstream.Add(new KeyValuePair<string, IPin>("video", pinDownstreamVideo));
                  pinsDownstream.Add(new KeyValuePair<string, IPin>("audio", pinDownstreamAudio));
                }
              }

              foreach (KeyValuePair<string, IPin> pair in pinsDownstream)
              {
                IPin pinDownstream = pair.Value;
                this.LogDebug("WDM analog tuner: try to connect upstream output pin {0} to downstream {1} pin...", pinIndex, pair.Key);
                try
                {
                  hr = graph.ConnectDirect(pinUpstream, pinDownstream, null);
                  HResult.ThrowException(hr, "Failed to connect pins.");
                  this.LogDebug("WDM analog tuner: connected!");
                  pinCountConnected++;
                  if (pinCountConnected == pinCountUnconnected)
                  {
                    return pinCountConnected;
                  }
                  break;
                }
                catch
                {
                  // Connection failed, maybe try the other downstream pin.
                }
              }
              pinIndex++;
            }
            finally
            {
              Release.ComObject("WDM analog tuner upstream filter output pin", ref pinUpstream);
            }
          }
        }
        finally
        {
          Release.ComObject("WDM analog tuner upstream filter pin enumerator", ref pinEnumUpstream);
        }
      }
      finally
      {
        Release.ComObject("WDM analog tuner downstream filter video pin", ref pinDownstreamVideo);
        Release.ComObject("WDM analog tuner downstream filter audio pin", ref pinDownstreamAudio);
      }
      return pinCountConnected;
    }

    /// <summary>
    /// Check the capabilites of the component.
    /// </summary>
    private void CheckCapabilities()
    {
      IAMTVTuner tuner = _filterTuner as IAMTVTuner;
      if (tuner != null)
      {
        int hr = tuner.GetAvailableModes(out _supportedTuningModes);
        HResult.ThrowException(hr, "Failed to read supported tuning modes.");
        this.LogDebug("WDM analog tuner: supported tuning modes = {0}", _supportedTuningModes);
      }

      IAMTVAudio tvAudio = _filterTvAudio as IAMTVAudio;
      if (tvAudio == null)
      {
        tvAudio = _filterTuner as IAMTVAudio;
      }
      if (tvAudio != null)
      {
        int hr = tvAudio.GetHardwareSupportedTVAudioModes(out _supportedTvAudioModes);
        HResult.ThrowException(hr, "Failed to read supported TV audio modes.");
        this.LogDebug("WDM analog tuner: supported TV audio modes = {0}", _supportedTvAudioModes);
      }
      else
      {
        this.LogWarn("WDM analog tuner: failed to find TV audio interface for tuner");
      }
    }

    /// <summary>
    /// Update tuner signal status measurements.
    /// </summary>
    /// <param name="isSignalLocked">A flag indicating whether the tuner is locked on signal.</param>
    /// <param name="signalLevel">The tuner input signal level.</param>
    public void UpdateSignalStatus(out bool isSignalLocked, out int signalLevel)
    {
      isSignalLocked = false;
      signalLevel = 0;
      IAMTVTuner tuner = _filterTuner as IAMTVTuner;
      if (tuner == null)
      {
        return;
      }

      // Some tuners (in particular, cards based on the Philips/NXP SAA713x and
      // SAA716x PCI-e bridge chipsets such as the Hauppauge HVR2200) report
      // values outside the documented range when they are locked. It is best
      // to assume that the tuner is locked unless "no signal" is reported. See
      // Mantis #0002445.
      AMTunerSignalStrength signalStrength;
      int hr = tuner.SignalPresent(out signalStrength);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogWarn("WDM analog tuner: potential error updating signal status, hr = 0x{0:x}", hr);
      }
      else
      {
        isSignalLocked = (signalStrength != AMTunerSignalStrength.NoSignal);
        signalLevel = (int)signalStrength;
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(AnalogChannel channel)
    {
      this.LogDebug("WDM analog tuner: perform tuning");
      IAMTVTuner tuner = _filterTuner as IAMTVTuner;
      if (tuner == null)
      {
        throw new TvException("Failed to find tuner interface on filter.");
      }
      if (channel.MediaType == MediaTypeEnum.TV)
      {
        UpdateFrequencyOverride(channel);
      }

      // Set tuning parameters.
      int hr = (int)HResult.Severity.Success;
      if (_currentChannel == null ||
        _currentChannel.MediaType != channel.MediaType ||
        _currentChannel.Country.Id != channel.Country.Id ||
        _currentChannel.TunerSource != channel.TunerSource ||
        (channel.MediaType == MediaTypeEnum.TV &&
          (
            _currentChannel.ChannelNumber != channel.ChannelNumber ||
            (channel.Frequency > 0 && _currentChannel.Frequency != channel.Frequency)
          )
        ) ||
        (channel.MediaType == MediaTypeEnum.Radio && _currentChannel.Frequency != channel.Frequency))
      {
        this.LogDebug("WDM analog tuner: setting tuning parameters");
        if (channel.MediaType == MediaTypeEnum.Radio)
        {
          if (channel.Frequency < 30000000)
          {
            if (!_supportedTuningModes.HasFlag(AMTunerModeType.AMRadio))
            {
              this.LogWarn("WDM analog tuner: requested tuning mode AM radio is not supported");
            }
            else
            {
              hr |= tuner.put_Mode(AMTunerModeType.AMRadio);
            }
          }
          else
          {
            if (!_supportedTuningModes.HasFlag(AMTunerModeType.FMRadio))
            {
              this.LogWarn("WDM analog tuner: requested tuning mode FM radio is not supported");
            }
            else
            {
              hr |= tuner.put_Mode(AMTunerModeType.FMRadio);
            }
          }
        }
        else if (channel.MediaType == MediaTypeEnum.TV)
        {
          if (!_supportedTuningModes.HasFlag(AMTunerModeType.TV))
          {
            this.LogWarn("WDM analog tuner: requested tuning mode analog TV is not supported");
          }
          else
          {
            hr |= tuner.put_Mode(AMTunerModeType.TV);
          }
        }
        hr |= tuner.put_TuningSpace(channel.Country.Id);
        hr |= tuner.put_CountryCode(channel.Country.Id);
        hr |= tuner.put_InputType(0, channel.TunerSource);
        if (channel.MediaType == MediaTypeEnum.Radio)
        {
          hr |= tuner.put_Channel((int)channel.Frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
        }
        else
        {
          hr |= tuner.put_Channel(channel.ChannelNumber, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
        }

        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("WDM analog tuner: potential error setting tuning parameters, hr = 0x{0:x}", hr);
        }
      }

      int frequencyVideo = 0;
      int frequencyAudio = 0;
      if (_currentChannel.MediaType == MediaTypeEnum.TV)
      {
        hr = tuner.get_VideoFrequency(out frequencyVideo);
        HResult.ThrowException(hr, "Failed to read current video frequency.");
      }
      hr = tuner.get_AudioFrequency(out frequencyAudio);
      HResult.ThrowException(hr, "Failed to read current audio frequency.");

      this.LogDebug("WDM analog tuner: tuned to frequency, video = {0} Hz, audio = {1} Hz", frequencyVideo, frequencyAudio);

      // Set TV audio mode.
      if (_supportedTvAudioModes != TVAudioMode.None)
      {
        IAMTVAudio tvAudio = _filterTvAudio as IAMTVAudio;
        if (tvAudio == null)
        {
          tvAudio = _filterTuner as IAMTVAudio;
        }
        if (tvAudio != null)
        {
          TVAudioMode availableTvAudioModes;
          hr = tvAudio.GetAvailableTVAudioModes(out availableTvAudioModes);
          if (hr == (int)HResult.Severity.Success)
          {
            // TODO: add TV audio mode to analog tuning details
            this.LogDebug("WDM analog tuner: available TV audio modes = {0}", availableTvAudioModes);
            if (!availableTvAudioModes.HasFlag(TVAudioMode.Stereo) || !_supportedTvAudioModes.HasFlag(TVAudioMode.Stereo))
            {
              this.LogWarn("WDM analog tuner: requested TV audio mode is not supported or available");
            }
            else
            {
              hr = tvAudio.put_TVAudioMode(TVAudioMode.Stereo);
              if (hr != (int)HResult.Severity.Success)
              {
                this.LogWarn("WDM analog tuner: potential error setting TV audio mode, hr = 0x{0:x}", hr);
              }
            }
          }
          else
          {
            this.LogWarn("WDM analog tuner: failed to get available TV audio modes, hr = 0x{0:x}", hr);
          }
        }
      }

      _currentChannel = channel;
    }

    /// <summary>
    /// Set or unset an override for the DirectShow channel to frequency mapping used for analog
    /// tuning.
    /// </summary>
    /// <remarks>
    /// Refer to MSDN Frequency Overrides:
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd375809%28v=vs.85%29.aspx
    /// </remarks>
    /// <param name="channel">A channel instance populated with the required override parameters.</param>
    private static void UpdateFrequencyOverride(AnalogChannel channel)
    {
      int countryCode = channel.Country.Id;
      int broadcastOrCable = 0;
      if (channel.TunerSource == TunerInputType.Cable)
      {
        broadcastOrCable = 1;
      }
      string[] registryLocations = new string[]
      {
        string.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-{1}", broadcastOrCable),
        string.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-{1}", countryCode, broadcastOrCable)
      };

      foreach (string location in registryLocations)
      {
        foreach (RegistryHive hive in FREQUENCY_OVERRIDE_REGISTRY_HIVES)
        {
          foreach (RegistryView view in FREQUENCY_OVERRIDE_REGISTRY_VIEWS)
          {
            RegistryKey key = RegistryKey.OpenBaseKey(hive, view).CreateSubKey(location);
            if (key != null)
            {
              if (channel.Frequency <= 0)
              {
                key.DeleteValue(channel.ChannelNumber.ToString(), false);
              }
              else
              {
                key.SetValue(channel.ChannelNumber.ToString(), (int)channel.Frequency, RegistryValueKind.DWord);
              }
              key.Close();
            }
          }
        }
      }
    }

    /// <summary>
    /// Unload the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformUnloading(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog tuner: perform unloading");

      _supportedTuningModes = AMTunerModeType.Default;
      _supportedTvAudioModes = TVAudioMode.None;
      _currentChannel = null;

      if (_filterTuner != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterTuner);
        }
        Release.ComObject("tuner filter", ref _filterTuner);

        DevicesInUse.Instance.Remove(_deviceTuner);
        _deviceTuner.Dispose();
        _deviceTuner = null;
      }
      if (_filterTvAudio != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filterTvAudio);
        }
        Release.ComObject("tuner TV audio filter", ref _filterTvAudio);

        DevicesInUse.Instance.Remove(_deviceTvAudio);
        _deviceTvAudio.Dispose();
        _deviceTvAudio = null;
      }
    }
  }
}