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
using System.Diagnostics;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog.QualityControl;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Components;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tuners
  /// and capture devices with WDM/DirectShow drivers.
  /// </summary>
  public class TunerAnalog : TunerDirectShowBase
  {
    #region constants

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    private class MpTsAnalyzer { }

    // The MediaPortal TS multiplexer delivers a DVB stream containing a single
    // service with a fixed service ID. The PMT PID starts at 0x20 and is
    // incremented with each channel change up to the fixed limit 0x90 after
    // which it is reset to 0x20. This is done in order to clearly signal
    // channel change transitions. TsWriter suppresses changes in PMT version
    // so we had to be a bit more radical.
    private const int SERVICE_ID = 1;
    private const int PMT_PID_FIRST = 0x20;
    private const int PMT_PID_MAX = 0x90;

    #endregion

    #region variables

    private Guid _mainDeviceCategory = Guid.Empty;
    private int _expectedPmtPid = PMT_PID_FIRST;

    private Tuner _tuner;
    private Crossbar _crossbar;
    private Capture _capture;
    private Encoder _encoder;

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
      : base(device)
    {
      _tunerType = CardType.Analog;
      _mainDeviceCategory = category;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is AnalogChannel;
    }

    #endregion

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();

      _externalTunerChannel = new AnalogChannel();
      _externalTunerChannel.TunerSource = (TunerInputType)SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerTunerSource", (int)TunerInputType.Cable);
      int countryId = SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerCountry", 1);
      CountryCollection countries = new CountryCollection();
      _externalTunerChannel.Country = countries.GetTunerCountryFromID(countryId);
      _externalTunerChannel.ChannelNumber = SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerSourceChannelNumber", 6);
      _externalTunerChannel.VideoSource = (CaptureSourceVideo)SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerSourceVideo", (int)CaptureSourceVideo.Composite1);
      _externalTunerChannel.AudioSource = (CaptureSourceAudio)SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerSourceAudio", (int)CaptureSourceAudio.Line1);
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

      _externalTunerCommand = SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerCommand", string.Empty);
      _externalTunerCommandArguments = SettingsManagement.GetValue("tuner" + _cardId + "ExternalTunerCommandArguments", string.Empty);
      this.LogDebug("WDM analog: external tuner command, {0} {1}", _externalTunerCommand, _externalTunerCommandArguments);

      if (_capture != null)
      {
        _capture.ReloadConfiguration(_cardId);
      }
    }

    #region properties

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (_tuner == null)
      {
        // Capture sources don't have a tuner.
        _isSignalPresent = true;
        _isSignalLocked = true;
        _signalLevel = 100;
        _signalQuality = 100;
        return;
      }
      _tuner.UpdateSignalStatus(out _isSignalLocked, out _signalLevel);
      _isSignalPresent = _isSignalLocked;
      _signalQuality = _signalLevel;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("WDM analog: perform loading");

      InitialiseGraph();

      string productInstanceIdentifier = _deviceMain.ProductInstanceIdentifier;
      if (_mainDeviceCategory == FilterCategory.AMKSCrossbar)
      {
        _crossbar = new Crossbar(_deviceMain);
        _crossbar.PerformLoading(_graph);
        if (_crossbar.PinIndexInputTunerVideo >= 0 || _crossbar.PinIndexInputTunerAudio >= 0)
        {
          _tuner = new Tuner();
          _tuner.PerformLoading(_graph, productInstanceIdentifier, _crossbar);
        }
        _capture = new Capture();
      }
      else
      {
        _capture = new Capture(_deviceMain);
      }
      _capture.PerformLoading(_graph, _captureGraphBuilder, productInstanceIdentifier, _crossbar);

      _encoder = new Encoder();
      _encoder.PerformLoading(_graph, productInstanceIdentifier, _capture);

      // Check for and load extensions, adding any additional filters to the graph.
      IBaseFilter lastFilter = _encoder.TsMultiplexerFilter;
      LoadPlugins(_filterMain, _graph, ref lastFilter);
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    protected override void PerformUnloading()
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

    #region tuning & scanning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      // TODO how am I going to do this???
      AnalogSubChannel analogSubChannel = subChannel as AnalogSubChannel;
      if (analogSubChannel != null)
      {
        _expectedPmtPid++;
        if (_expectedPmtPid == PMT_PID_MAX)
        {
          _expectedPmtPid = PMT_PID_FIRST;
        }
        analogSubChannel.SetServiceParameters(SERVICE_ID, _expectedPmtPid);
      }

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

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new ScannerAnalog(this, _filterTsWriter as ITsChannelScan);
      }
    }

    #endregion
  }
}