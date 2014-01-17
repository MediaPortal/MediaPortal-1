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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles ATSC
  /// terrestrial and SCTE cable tuners with BDA drivers.
  /// </summary>
  public class TunerBdaAtsc : TunerBdaBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaAtsc"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerBdaAtsc(DsDevice device)
      : base(device, device.DevicePath + "A")
    {
      _tunerType = CardType.Atsc;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("BDA ATSC: create tuning space");

      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      IATSCTuningSpace tuningSpace = null;
      IATSCLocator locator = null;
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to find tuning space container interface on system tuning spaces instance.");
        }

        tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(NetworkType.ATSC_TERRESTRIAL);
        hr |= tuningSpace.put_CountryCode(0);
        hr |= tuningSpace.put_InputType(TunerInputType.Antenna);
        hr |= tuningSpace.put_MaxMinorChannel(999);     // the number of minor channels per major channel
        hr |= tuningSpace.put_MaxPhysicalChannel(158);  // 69 for terrestrial, 158 for cable
        hr |= tuningSpace.put_MaxChannel(99);           // the number of scannable major channels
        hr |= tuningSpace.put_MinMinorChannel(0);
        hr |= tuningSpace.put_MinPhysicalChannel(1);    // 1 for terrestrial, 2 for cable
        hr |= tuningSpace.put_MinChannel(1);

        locator = (IATSCLocator)new ATSCLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_PhysicalChannel(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_Modulation(ModulationType.Mod8Vsb); // 8 VSB is terrestrial, 64 or 256 QAM is cable
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_TSID(-1);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("BDA ATSC: potential error creating tuning space, hr = 0x{0:x}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        HResult.ThrowException(hr, "Failed to add new tuning space to tuning space container.");
        return tuningSpace;
      }
      catch (Exception)
      {
        Release.ComObject("BDA ATSC tuner tuning space", ref tuningSpace);
        Release.ComObject("BDA ATSC tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("BDA ATSC tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// Get the class ID of the network provider for the tuner type.
    /// </summary>
    protected abstract Guid NetworkProviderClsid
    {
      get
      {
        return typeof(ATSCNetworkProvider).GUID;
      }
    }

    /// <summary>
    /// Get the registered name of the BDA tuning space for the tuner type.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal ATSC Tuning Space";
      }
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The tuner's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected override ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
      HResult.ThrowException(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IATSCLocator atscLocator = locator as IATSCLocator;
        if (atscLocator == null)
        {
          throw new TvException("Failed to find ATSC locator interface on locator.");
        }
        hr = atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
        hr |= atscLocator.put_CarrierFrequency((int)atscChannel.Frequency);
        hr |= atscLocator.put_Modulation(atscChannel.ModulationType);
        hr |= atscLocator.put_TSID(atscChannel.TransportId);

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        HResult.ThrowException(hr, "Failed to create tuning request from tuning space.");
        try
        {
          IATSCChannelTuneRequest atscTuneRequest = tuneRequest as IATSCChannelTuneRequest;
          if (atscTuneRequest == null)
          {
            throw new TvException("Failed to find ATSC tune request interface on tune request.");
          }
          hr |= atscTuneRequest.put_Channel(atscChannel.MajorChannel);
          hr |= atscTuneRequest.put_MinorChannel(atscChannel.MinorChannel);
          hr |= atscTuneRequest.put_Locator(locator);

          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("BDA ATSC: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return atscTuneRequest;
        }
        catch (Exception)
        {
          Release.ComObject("BDA ATSC tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("BDA ATSC tuner locator", ref locator);
      }
    }

    /// <summary>
    /// Tune using the MediaPortal network provider.
    /// </summary>
    /// <param name="networkProvider">The network provider interface to use for tuning.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>an HRESULT indicating whether the tuning parameters were applied successfully</returns>
    protected override int PerformMediaPortalNetworkProviderTuning(IDvbNetworkProvider networkProvider, IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Frequency = (uint)(atscChannel.Frequency),
        Bandwidth = uint.MaxValue,
        Polarity = Polarisation.NotSet,
        Range = uint.MaxValue
      };
      DigitalDemodulatorSettings demodulatorSettings = new DigitalDemodulatorSettings
      {
        InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
        InnerFECMethod = FECMethod.MethodNotSet,
        Modulation = atscChannel.ModulationType,
        OuterFECMethod = FECMethod.MethodNotSet,
        OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
        SpectralInversion = SpectralInversion.NotSet,
        SymbolRate = uint.MaxValue
      };
      return networkProvider.TuneATSC((uint)atscChannel.PhysicalChannel, frequencySettings, demodulatorSettings);
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new ScannerMpeg2TsAtsc(this, _filterTsWriter as ITsChannelScan);
      }
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is ATSCChannel;
    }

    #endregion

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new ATSCChannel();
    }
  }
}