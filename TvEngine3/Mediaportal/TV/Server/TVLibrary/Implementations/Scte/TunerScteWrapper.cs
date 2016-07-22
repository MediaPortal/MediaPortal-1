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

using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Scte
{
  /// <summary>
  /// Some ATSC tuner drivers expose a DVB-C interface for clear QAM cable
  /// (SCTE ITU-T annex B) tuning support. This implementation wraps the native
  /// interface to expose a more desirable ATSC interface.
  /// </summary>
  /// <remarks>
  /// This class is currently not used, but might be used in future for:
  /// 
  /// 1. KWorld UB435-Q
  /// BDA 2875 TVTuner
  /// @device:pnp:\\?\usb#vid_1b80&pid_e346#0#{71985f48-1ca1-11d39cc8-00c04f7971e0}\{7c8095ab-c110-40e5-9f4d-310858bbbf64}
  /// 0dad2fdd-5fd7-11d3-8f50-00c04f7971e2
  /// 
  /// Tuner for Windows Media Center
  /// @device:pnp:\\?\usb#vid_1b80&pid_e346#0#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{b50b8116-da24-4f97-80d1-00451702c5f7}
  /// 143827ab-f77b-498d-81ca-5a007aec28bf
  /// dc0c0fe7-0485-4266-b93f-68fbf80ed834
  /// 
  /// 
  /// 2. EyeTV Hybrid [North American model]
  /// EyeTV Hybrid ATSC Tuner
  /// @device:pnp:\\?\usb#vid_0fd9&pid_0024#081004017082#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{7c8095ab-c110-40e5-9f4d-310858bbbf64}
  /// 0dad2fdd-5fd7-11d3-8f50-00c04f7971e2
  /// 
  /// EyeTV Hybrid ClearQam Tuner
  /// @device:pnp:\\?\usb#vid_0fd9&pid_0024#081004017082#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{b50b8116-da24-4f97-80d1-00451702c5f7}
  /// 143827ab-f77b-498d-81ca-5a007aec28bf
  /// dc0c0fe7-0485-4266-b93f-68fbf80ed834
  /// 
  /// 
  /// 3. ATI Theater 750 USB
  /// ATI AVStream Analog Tuner
  /// @device:pnp:\\?\usb#vid_0438&pid_ac14#1234-5678#{a799a800-a46d-11d0-a18c-00a02401dcd4}\{e6223d77-45f9-4025-a86f-27bddb4c8ca9}
  /// 
  /// ATI CQAM Digital Tuner
  /// @device:pnp:\\?\usb#vid_0438&pid_ac14#1234-5678#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{9c40b733-1a99-496e-bc6a-88a85232fad6}
  /// 143827ab-f77b-498d-81ca-5a007aec28bf
  /// dc0c0fe7-0485-4266-b93f-68fbf80ed834
  /// 
  /// ATI DVBT Digital Tuner
  /// @device:pnp:\\?\usb#vid_0438&pid_ac14#1234-5678#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{a9224736-86b6-4242-9cc4-4328f2cd2df4}
  /// 216c62df-6d7f-4e9a-8571-05f14edb766a
  /// 
  /// ATI BDA Digital Tuner
  /// @device:pnp:\\?\usb#vid_0438&pid_ac14#1234-5678#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{eec5a519-643f-4a74-bc7f-5ce7d46fefd5}
  /// 0dad2fdd-5fd7-11d3-8f50-00c04f7971e2
  /// </remarks>
  internal class TunerScteWrapper : TunerBase
  {
    #region variables

    /// <summary>
    /// Internal DVB-C tuner. This allows us to decouple this wrapper from the
    /// underlying implementation.
    /// </summary>
    private ITunerInternal _dvbcTuner = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    private IChannelScannerInternal _channelScanner = null;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerScteWrapper"/> class.
    /// </summary>
    /// <param name="dvbcTuner">The internal tuner implementation.</param>
    public TunerScteWrapper(ITuner dvbcTuner)
      : base(dvbcTuner.Name, dvbcTuner.ExternalId, dvbcTuner.TunerInstanceId, dvbcTuner.ProductInstanceId, BroadcastStandard.Scte)
    {
      ChannelDvbC dvbChannel = new ChannelDvbC();
      ITunerInternal internalTuner = dvbcTuner as ITunerInternal;
      if (internalTuner == null || !dvbcTuner.CanTune(dvbChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _dvbcTuner = new TunerInternalWrapper(internalTuner);
    }

    ~TunerScteWrapper()
    {
      Dispose(false);
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      _dvbcTuner.ReloadConfiguration(configuration);
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Scte;
      }
      IList<ITunerExtension> extensions = _dvbcTuner.PerformLoading(streamFormat);

      _channelScanner = _dvbcTuner.InternalChannelScanningInterface;
      if (_channelScanner != null)
      {
        _channelScanner.Tuner = this;
      }
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      if (!isFinalising)
      {
        _dvbcTuner.PerformSetTunerState(state);
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      if (!isFinalising)
      {
        _channelScanner = null;
        _dvbcTuner.PerformUnloading();
      }
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (!base.CanTune(channel))
      {
        return false;
      }

      // Channels delivered using switched digital video are not supported.
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel == null || scteChannel.Frequency <= 1750)
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
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ChannelDvbC dvbcChannel = new ChannelDvbC();
      dvbcChannel.Frequency = scteChannel.Frequency - 1750;   // BDA convention: use analog video carrier frequency
      dvbcChannel.ModulationScheme = scteChannel.ModulationScheme;
      dvbcChannel.SymbolRate = scteChannel.SymbolRate;

      // The rest of these parameters are almost certainly irrelevant.
      dvbcChannel.IsEncrypted = scteChannel.IsEncrypted;
      dvbcChannel.LogicalChannelNumber = scteChannel.LogicalChannelNumber;
      dvbcChannel.MediaType = scteChannel.MediaType;
      dvbcChannel.Name = scteChannel.Name;
      dvbcChannel.OriginalNetworkId = -1;  // fake
      dvbcChannel.PmtPid = scteChannel.PmtPid;
      dvbcChannel.Provider = scteChannel.Provider;
      dvbcChannel.ProgramNumber = scteChannel.ProgramNumber;
      dvbcChannel.TransportStreamId = scteChannel.TransportStreamId;

      _dvbcTuner.PerformTuning(dvbcChannel);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      _dvbcTuner.GetSignalStatus(out isLocked, out isPresent, out strength, out quality, onlyGetLock);
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's sub-channel manager.
    /// </summary>
    public override ISubChannelManager SubChannelManager
    {
      get
      {
        return _dvbcTuner.SubChannelManager;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public override IEpgGrabberInternal InternalEpgGrabberInterface
    {
      get
      {
        return _dvbcTuner.InternalEpgGrabberInterface;
      }
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the tuner is being disposed.</param>
    protected override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      if (isDisposing && _dvbcTuner != null)
      {
        _dvbcTuner.Dispose();
        _dvbcTuner = null;
      }
    }

    #endregion
  }
}