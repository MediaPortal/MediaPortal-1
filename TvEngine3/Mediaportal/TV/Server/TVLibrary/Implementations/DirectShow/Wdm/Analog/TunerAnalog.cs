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
using System.Diagnostics;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tuners
  /// and capture devices with WDM/DirectShow drivers.
  /// </summary>
  internal class TunerAnalog : TunerDirectShowBase
  {
    #region variables

    private Guid _mainDeviceCategory = Guid.Empty;

    private Tuner _tuner = null;
    private Crossbar _crossbar = null;
    private Capture _capture = null;
    private Encoder _encoder = null;

    private bool _hasTuner = false;
    private AMTunerModeType _tunerSupportedModes = AMTunerModeType.Default;

    private AnalogChannel _externalTunerChannel = null;
    private string _externalTunerCommand = string.Empty;
    private string _externalTunerCommandArguments = string.Empty;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerAnalog"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="category">The device/filter category of <paramref name="device"/>.</param>
    public TunerAnalog(DsDevice device, Guid category)
      : base(device, CardType.Analog)
    {
      _mainDeviceCategory = category;

      if (category == FilterCategory.AMKSCrossbar)
      {
        // As far as I know the tuner instance ID is only stored with the
        // tuner component moniker. So, we have to do some graph work.
        IFilterGraph2 graph = (IFilterGraph2)new FilterGraph();
        Crossbar crossbar = new Crossbar(device);
        try
        {
          crossbar.PerformLoading(graph);
          if (crossbar.PinIndexInputTunerVideo >= 0 || crossbar.PinIndexInputTunerAudio >= 0)
          {
            _hasTuner = true;
            Tuner tuner = new Tuner();
            try
            {
              tuner.PerformLoading(graph, ProductInstanceId, crossbar);
              _tunerSupportedModes = tuner.SupportedTuningModes;
              SetProductAndTunerInstanceIds(tuner.Device);
            }
            finally
            {
              tuner.PerformUnloading(graph);
            }
          }
          else
          {
            SetProductAndTunerInstanceIds(device);
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "WDM analog: failed to determine tuner instance ID for {0}", Name);
        }
        finally
        {
          crossbar.PerformUnloading(graph);
          Release.ComObject("analog tuner tuner instance ID graph", ref graph);
        }
      }
    }

    #endregion

    /// <summary>
    /// Get a list of channels representing the external non-tuner sources available from this tuner.
    /// </summary>
    /// <returns>a list of channels, one channel per source</returns>
    public IList<IChannel> GetSourceChannels()
    {
      if (_crossbar != null)
      {
        IList<AnalogChannel> sourceChannels = _crossbar.SourceChannels;
        IList<IChannel> channels = new List<IChannel>();
        foreach (AnalogChannel channel in sourceChannels)
        {
          if (channel.VideoSource != CaptureSourceVideo.None)
          {
            channel.Name = string.Format("Tuner {0} {1} Video Source", TunerId, channel.VideoSource.GetDescription());
          }
          else
          {
            channel.Name = string.Format("Tuner {0} {1} Audio Source", TunerId, channel.AudioSource.GetDescription());
          }
          channel.ChannelNumber = 10000;
          channels.Add(channel);
        }
        return channels;
      }
      else
      {
        AnalogChannel channel = new AnalogChannel();
        channel.AudioSource = CaptureSourceAudio.Automatic;
        if (_capture.VideoFilter != null)
        {
          channel.MediaType = MediaTypeEnum.TV;
          channel.Name = "Tuner " + TunerId + " Video Capture";
          channel.VideoSource = CaptureSourceVideo.Composite1;  // Anything other than "none" and "tuner" is okay.
        }
        else
        {
          channel.MediaType = MediaTypeEnum.Radio;
          channel.Name = "Tuner " + TunerId + " Audio Capture";
          channel.VideoSource = CaptureSourceVideo.None;
        }
        return new List<IChannel>() { channel };
      }
    }

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();

      this.LogDebug("WDM analog: reload configuration");
      _externalTunerChannel = new AnalogChannel();
      _externalTunerChannel.TunerSource = (TunerInputType)SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerTunerSource", (int)TunerInputType.Cable);
      int countryId = SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerCountry", 1);
      CountryCollection countries = new CountryCollection();
      _externalTunerChannel.Country = countries.GetTunerCountryFromID(countryId);
      _externalTunerChannel.ChannelNumber = SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerSourceChannelNumber", 6);
      _externalTunerChannel.VideoSource = (CaptureSourceVideo)SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerSourceVideo", (int)CaptureSourceVideo.Composite1);
      _externalTunerChannel.AudioSource = (CaptureSourceAudio)SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerSourceAudio", (int)CaptureSourceAudio.Line1);
      _externalTunerChannel.MediaType = MediaTypeEnum.TV;
      if (_externalTunerChannel.VideoSource == CaptureSourceVideo.None)
      {
        _externalTunerChannel.MediaType = MediaTypeEnum.Radio;
      }
      _externalTunerChannel.Name = "External Tuner Input";
      _externalTunerChannel.IsVcrSignal = true;
      _externalTunerChannel.Frequency = 0;
      _externalTunerChannel.FreeToAir = true;
      this.LogDebug("WDM analog: external tuner source, {0}", _externalTunerChannel.ToString());

      _externalTunerCommand = SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerCommand", string.Empty);
      _externalTunerCommandArguments = SettingsManagement.GetValue("tuner" + TunerId + "ExternalTunerCommandArguments", string.Empty);
      this.LogDebug("WDM analog: external tuner command, {0} {1}", _externalTunerCommand, _externalTunerCommandArguments);

      if (_capture != null)
      {
        _capture.ReloadConfiguration(TunerId);
      }
      if (_encoder != null)
      {
        _encoder.ReloadConfiguration(TunerId);
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ICustomDevice> PerformLoading()
    {
      this.LogDebug("WDM analog: perform loading");

      InitialiseGraph();

      if (_mainDeviceCategory == FilterCategory.AMKSCrossbar)
      {
        _crossbar = new Crossbar(_deviceMain);
        _crossbar.PerformLoading(_graph);
        if (_crossbar.PinIndexInputTunerVideo >= 0 || _crossbar.PinIndexInputTunerAudio >= 0)
        {
          _tuner = new Tuner();
          _tuner.PerformLoading(_graph, ProductInstanceId, _crossbar);
          _tunerSupportedModes = _tuner.SupportedTuningModes;
        }
        _capture = new Capture();
      }
      else
      {
        _capture = new Capture(_deviceMain);
      }
      _capture.PerformLoading(_graph, ProductInstanceId, _crossbar);
      _capture.ReloadConfiguration(TunerId);

      _encoder = new Encoder();
      _encoder.PerformLoading(_graph, ProductInstanceId, _capture);
      _encoder.ReloadConfiguration(TunerId);

      // Check for and load extensions, adding any additional filters to the graph.
      IList<ICustomDevice> extensions;
      IBaseFilter lastFilter = _encoder.TsMultiplexerFilter;
      if (_mainDeviceCategory == FilterCategory.AMKSCrossbar)
      {
        extensions = LoadExtensions(_crossbar.Filter, ref lastFilter);
      }
      else
      {
        extensions = LoadExtensions(_capture.VideoFilter ?? _capture.AudioFilter, ref lastFilter);
      }
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      // Teletext scraping and RDS grabbing currently not supported.
      _epgGrabber = null;

      _channelScanner = new ChannelScannerDirectShowAnalog(this, _filterTsWriter as ITsChannelScan);
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public override void PerformUnloading()
    {
      this.LogDebug("WDM analog: perform unloading");
      if (_tuner != null)
      {
        _tuner.PerformUnloading(_graph);
        _tuner = null;
      }
      if (_crossbar != null)
      {
        _crossbar.PerformUnloading(_graph);
        _crossbar = null;
      }
      if (_capture != null)
      {
        _capture.PerformUnloading(_graph);
        _capture = null;
      }
      if (_encoder != null)
      {
        _encoder.PerformUnloading(_graph);
        _encoder = null;
      }

      CleanUpGraph();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel == null ||
        (!_hasTuner && (analogChannel.VideoSource == CaptureSourceVideo.Tuner || analogChannel.AudioSource == CaptureSourceAudio.Tuner)) ||
        (_hasTuner && analogChannel.MediaType == MediaTypeEnum.TV && !_tunerSupportedModes.HasFlag(AMTunerModeType.TV)) ||
        (_hasTuner && analogChannel.MediaType == MediaTypeEnum.Radio && !_tunerSupportedModes.HasFlag(AMTunerModeType.FMRadio) && !_tunerSupportedModes.HasFlag(AMTunerModeType.AMRadio))
      )
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // Channel or external tuner settings?
      AnalogChannel tuneChannel = analogChannel;
      if (analogChannel.VideoSource == CaptureSourceVideo.None && analogChannel.AudioSource == CaptureSourceAudio.None)
      {
        this.LogDebug("WDM analog: using external tuner");
        tuneChannel = _externalTunerChannel;
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = true;
        startInfo.ErrorDialog = false;
        startInfo.LoadUserProfile = false;
        startInfo.UseShellExecute = true;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = _externalTunerCommand;
        startInfo.Arguments = _externalTunerCommandArguments.Replace("%channel number%", analogChannel.ChannelNumber.ToString());
        try
        {
          Process p = Process.Start(startInfo);
          if (p.WaitForExit(10000))
          {
            this.LogDebug("WDM analog: external tuner process exited with code {0}", p.ExitCode);
          }
          else
          {
            this.LogWarn("WDM analog: external tuner process failed to exit within 10 seconds");
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "WDM analog: external tuner process threw exception");
        }
      }

      if (tuneChannel.VideoSource == CaptureSourceVideo.Tuner || tuneChannel.AudioSource == CaptureSourceAudio.Tuner)
      {
        if (_tuner != null)
        {
          _tuner.PerformTuning(tuneChannel);
        }
        else
        {
          this.LogWarn("WDM analog: ignored request to tune with a capture device");
        }
      }
      if (_crossbar != null)
      {
        _crossbar.PerformTuning(tuneChannel);
      }
      _capture.PerformTuning(tuneChannel);
      _encoder.PerformTuning(tuneChannel);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public override void GetSignalStatus(bool onlyGetLock, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      if (_tuner == null)
      {
        // Capture sources don't have a tuner.
        isLocked = true;
        isPresent = true;
        strength = 100;
        quality = 100;
        return;
      }

      _tuner.GetSignalStatus(out isLocked, out strength);
      isPresent = isLocked;
      quality = strength;
    }

    #endregion

    #endregion
  }
}