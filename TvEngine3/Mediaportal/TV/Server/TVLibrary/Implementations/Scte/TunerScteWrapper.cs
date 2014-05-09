
using DirectShowLib.BDA;
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
  /// 2. EyeTV Hybrid [North American model]
  /// EyeTV Hybrid ATSC Tuner
  /// @device:pnp:\\?\usb#vid_0fd9&pid_0024#081004017082#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{7c8095ab-c110-40e5-9f4d-310858bbbf64}
  /// 0dad2fdd-5fd7-11d3-8f50-00c04f7971e2
  /// 
  /// EyeTV Hybrid ClearQam Tuner
  /// @device:pnp:\\?\usb#vid_0fd9&pid_0024#081004017082#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{b50b8116-da24-4f97-80d1-00451702c5f7}
  /// 143827ab-f77b-498d-81ca-5a007aec28bf
  /// dc0c0fe7-0485-4266-b93f-68fbf80ed834
  /// </remarks>
  internal class TunerScteWrapper : TvCardBase
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
      : base(name, externalId)
    {
      DVBCChannel dvbChannel = new DVBCChannel();
      if (dvbcTuner == null || !dvbcTuner.CanTune(dvbChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _tunerType = CardType.Atsc;
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

      // TODO need a proper wrapper that does what ScannerDirectShowAtsc does
      _channelScanner = _dvbcTuner.ScanningInterface;
      IScannerInternal scanner = _channelScanner as IScannerInternal;
      if (scanner != null)
      {
        scanner.Tuner = this;
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
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    public override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      _dvbcTuner.PerformSignalStatusUpdate(onlyUpdateLock);
    }

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
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