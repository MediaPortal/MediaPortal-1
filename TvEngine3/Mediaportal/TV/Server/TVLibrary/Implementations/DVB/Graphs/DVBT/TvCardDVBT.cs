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

using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.DVBT
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-T tuners with BDA drivers.
  /// </summary>
  public class TvCardDVBT : TvCardDvbBase
  {
    #region variables

    /// <summary>
    /// A pre-configured tuning space, used to speed up the tuning process. 
    /// </summary>
    private IDVBTuningSpace _tuningSpace = null;

    /// <summary>
    /// A tune request template, used to speed up the tuning process.
    /// </summary>
    private IDVBTuneRequest _tuneRequest = null;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDVBT"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDVBT(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _tunerType = CardType.DvbT;
    }

    #endregion

    #region graphbuilding

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected override void CreateTuningSpace()
    {
      Log.Debug("TvCardDvbT: create tuning space");

      // Check if the system already has an appropriate tuning space.
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      if (container == null)
      {
        Log.Error("TvCardDvbT: failed to get the tuning space container");
        return;
      }

      ITuner tuner = (ITuner)_filterNetworkProvider;
      ITuneRequest request;

      IEnumTuningSpaces enumTuning;
      container.get_EnumTuningSpaces(out enumTuning);
      try
      {
        ITuningSpace[] spaces = new ITuningSpace[2];
        while (true)
        {
          int fetched;
          enumTuning.Next(1, spaces, out fetched);
          if (fetched != 1)
          {
            break;
          }
          string name;
          spaces[0].get_UniqueName(out name);
          if (name.Equals("MediaPortal DVBT TuningSpace"))
          {
            Log.Debug("TvCardDvbT: found correct tuningspace");
            _tuningSpace = (IDVBTuningSpace)spaces[0];
            tuner.put_TuningSpace(_tuningSpace);
            _tuningSpace.CreateTuneRequest(out request);
            _tuneRequest = (IDVBTuneRequest)request;
            Release.ComObject("TuningSpaceContainer", container);
            return;
          }
          Release.ComObject("ITuningSpace", spaces[0]);
        }
      }
      finally
      {
        Release.ComObject("IEnumTuningSpaces", enumTuning);
      }

      // We didn't find our tuning space registered in the system, so create a new one.
      Log.Debug("TvCardDvbT: create new tuningspace");
      _tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
      _tuningSpace.put_UniqueName("MediaPortal DVBT TuningSpace");
      _tuningSpace.put_FriendlyName("MediaPortal DVBT TuningSpace");
      _tuningSpace.put__NetworkType(typeof(DVBTNetworkProvider).GUID);
      _tuningSpace.put_SystemType(DVBSystemType.Terrestrial);

      IDVBTLocator locator = (IDVBTLocator)new DVBTLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_SymbolRate(-1);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);

      _tuningSpace.put_DefaultLocator(locator);

      object newIndex;
      container.Add(_tuningSpace, out newIndex);
      Release.ComObject("TuningSpaceContainer", container);

      tuner.put_TuningSpace(_tuningSpace);
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    /// <returns>the assembled tune request</returns>
    protected override ITuneRequest AssembleTuneRequest(IChannel channel)
    {
      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel == null)
      {
        Log.Debug("TvCardDvbT: channel is not a DVB-T channel!!! {0}", channel.GetType().ToString());
        return null;
      }

      ILocator locator;
      _tuningSpace.get_DefaultLocator(out locator);
      IDVBTLocator dvbtLocator = (IDVBTLocator)locator;
      dvbtLocator.put_CarrierFrequency((int)dvbtChannel.Frequency);
      dvbtLocator.put_Bandwidth(dvbtChannel.BandWidth);

      _tuneRequest.put_ONID(dvbtChannel.NetworkId);
      _tuneRequest.put_TSID(dvbtChannel.TransportId);
      _tuneRequest.put_SID(dvbtChannel.ServiceId);
      _tuneRequest.put_Locator(locator);

      return _tuneRequest;
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (channel is DVBTChannel)
      {
        return true;
      }
      return false;
    }

    #endregion
  }
}