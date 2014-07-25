
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

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
    /// <summary>
    /// Internal DVB-C tuner. This allows us to decouple this wrapper from the
    /// underlying implementation.
    /// </summary>
    private ITunerInternal _dvbcTuner = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerScteWrapper"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    /// <param name="dvbcTuner">The internal tuner implementation.</param>
    public TunerScteWrapper(string name, string externalId, ITunerInternal dvbcTuner)
      : base(name, externalId, CardType.Atsc)
    {
      DVBCChannel dvbChannel = new DVBCChannel();
      if (dvbcTuner == null || !dvbcTuner.CanTune(dvbChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _dvbcTuner = dvbcTuner;
    }

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();
      _dvbcTuner.ReloadConfiguration();
    }

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel != null && atscChannel.Frequency > 0 &&
        (atscChannel.ModulationType == ModulationType.Mod640Qam || atscChannel.ModulationType == ModulationType.Mod256Qam))
      {
        return true;
      }
      return false;
    }

    #region ITunerInternal members

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    public override void PerformLoading()
    {
      _dvbcTuner.PerformLoading();

      _channelScanner = _dvbcTuner.ChannelScanningInterface;
      IChannelScannerInternal scanner = _channelScanner as IChannelScannerInternal;
      if (scanner != null)
      {
        scanner.Tuner = this;
        scanner.Helper = new ChannelScannerHelperAtsc();
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public override void PerformUnloading()
    {
      _dvbcTuner.PerformUnloading();
    }

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
      _dvbcTuner.GetSignalStatus(onlyGetLock, out isLocked, out isPresent, out strength, out quality);
    }

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public override ITvSubChannel CreateNewSubChannel(int id)
    {
      return _dvbcTuner.CreateNewSubChannel(id);
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      DVBCChannel dvbChannel = new DVBCChannel();
      dvbChannel.Frequency = atscChannel.Frequency;
      dvbChannel.ModulationType = atscChannel.ModulationType;
      dvbChannel.SymbolRate = 6900;   // fake

      // The rest of these parameters are almost certainly irrelevant.
      dvbChannel.FreeToAir = atscChannel.FreeToAir;
      dvbChannel.LogicalChannelNumber = atscChannel.LogicalChannelNumber;
      dvbChannel.MediaType = atscChannel.MediaType;
      dvbChannel.Name = atscChannel.Name;
      dvbChannel.NetworkId = atscChannel.NetworkId;
      dvbChannel.PmtPid = atscChannel.PmtPid;
      dvbChannel.Provider = atscChannel.Provider;
      dvbChannel.ServiceId = atscChannel.ServiceId;
      dvbChannel.TransportId = atscChannel.TransportId;

      _dvbcTuner.PerformTuning(dvbChannel);
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public override void SetTunerState(TunerState state)
    {
      _dvbcTuner.SetTunerState(state);
      _state = state;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      if (_dvbcTuner != null)
      {
        _dvbcTuner.Dispose();
        _dvbcTuner = null;
      }
    }

    #endregion
  }
}