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
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for ATSC terrestrial and SCTE
  /// (OpenCable, clear QAM) cable tuners with BDA drivers.
  /// </summary>
  internal class TunerBdaAtsc : TunerBdaBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaAtsc"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the tuner.</param>
    public TunerBdaAtsc(DsDevice device, BroadcastStandard supportedBroadcastStandards)
      : base(device, device.DevicePath + "ATSC", supportedBroadcastStandards)
    {
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
        hr |= tuningSpace.put_MinPhysicalChannel(1);    // 1 for terrestrial, 2 for cable
        hr |= tuningSpace.put_MaxPhysicalChannel(158);  // 69 for terrestrial, 158 for cable
        hr |= tuningSpace.put_MinChannel(-1);
        hr |= tuningSpace.put_MaxChannel(99);           // the number of scannable major channels
        hr |= tuningSpace.put_MinMinorChannel(-1);
        hr |= tuningSpace.put_MaxMinorChannel(999);     // the number of minor channels per major channel

        locator = (IATSCLocator)new ATSCLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_Modulation(ModulationType.Mod8Vsb); // 8 VSB or 16 VSB for terrestrial, 64 or 256 QAM for cable
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_PhysicalChannel(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_TSID(-1);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA ATSC: potential error creating tuning space, hr = 0x{0:x}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        TvExceptionDirectShowError.Throw(hr, "Failed to add new tuning space to tuning space container.");
        return tuningSpace;
      }
      catch
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
    protected override Guid NetworkProviderClsid
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

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading()
    {
      IList<ITunerExtension> extensions = base.PerformLoading();

      _subChannelManager = new SubChannelManagerMpeg2Ts(TsWriter as ITsWriter);
      _epgGrabber = new EpgGrabberAtsc(TsWriter as IGrabberEpgAtsc, TsWriter as IGrabberEpgScte);

      if (_channelScanner != null)
      {
        _channelScanner.Helper = new ChannelScannerHelperAtsc();
      }
      return extensions;
    }

    #endregion

    #region tuning

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The tuner's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected override ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel)
    {
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      ChannelScte scteChannel = channel as ChannelScte;
      if (atscChannel == null && scteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IATSCTuningSpace atscTuningSpace = tuningSpace as IATSCTuningSpace;
      if (atscTuningSpace == null)
      {
        throw new TvException("Failed to find ATSC tuning space interface on tuning space.");
      }

      int hr;
      if (atscChannel != null)
      {
        hr = atscTuningSpace.put_InputType(TunerInputType.Antenna);
      }
      else
      {
        hr = atscTuningSpace.put_InputType(TunerInputType.Cable);
      }

      ILocator locator;
      hr |= tuningSpace.get_DefaultLocator(out locator);
      TvExceptionDirectShowError.Throw(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IATSCLocator atscLocator = locator as IATSCLocator;
        if (atscLocator == null)
        {
          throw new TvException("Failed to find ATSC locator interface on locator.");
        }

        if (atscChannel != null)
        {
          hr = atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannelNumber);
          hr |= atscLocator.put_CarrierFrequency(-1);
          hr |= atscLocator.put_Modulation(GetBdaModulation(atscChannel.ModulationScheme));
          hr |= atscLocator.put_TSID(atscChannel.TransportStreamId);
        }
        else
        {
          hr = atscLocator.put_PhysicalChannel(scteChannel.PhysicalChannelNumber);
          // Convert from centre frequency to the analog video carrier
          // frequency. This is a BDA convention.
          hr |= atscLocator.put_CarrierFrequency(scteChannel.Frequency - 1750);
          hr |= atscLocator.put_Modulation(GetBdaModulation(scteChannel.ModulationScheme));
          hr |= atscLocator.put_TSID(scteChannel.TransportStreamId);
        }

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        TvExceptionDirectShowError.Throw(hr, "Failed to create tuning request from tuning space.");
        try
        {
          IATSCChannelTuneRequest atscTuneRequest = tuneRequest as IATSCChannelTuneRequest;
          if (atscTuneRequest == null)
          {
            throw new TvException("Failed to find ATSC tune request interface on tune request.");
          }

          // The MSDN remarks on IDigitalCableTuneRequest suggest there is more
          // than one way for the network provider to interpret the tune
          // request parameters.
          // http://msdn.microsoft.com/en-us/library/windows/desktop/dd693565%28v=vs.85%29.aspx
          //
          // Maybe we don't want to set these?
          if (atscChannel != null)
          {
            hr |= atscTuneRequest.put_Channel(atscChannel.MajorChannelNumber);
            hr |= atscTuneRequest.put_MinorChannel(atscChannel.MinorChannelNumber);
          }
          else
          {
            hr |= atscTuneRequest.put_Channel(scteChannel.MajorChannelNumber);
            hr |= atscTuneRequest.put_MinorChannel(scteChannel.MinorChannelNumber);
          }
          hr |= atscTuneRequest.put_Locator(locator);

          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("BDA ATSC: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return atscTuneRequest;
        }
        catch
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
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      ChannelScte scteChannel = channel as ChannelScte;
      if (atscChannel == null && scteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Bandwidth = uint.MaxValue,
        Polarity = DirectShowLib.BDA.Polarisation.NotSet,
        Range = uint.MaxValue
      };
      DigitalDemodulatorSettings demodulatorSettings = new DigitalDemodulatorSettings
      {
        InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
        InnerFECMethod = FECMethod.MethodNotSet,
        OuterFECMethod = FECMethod.MethodNotSet,
        OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
        SpectralInversion = SpectralInversion.NotSet,
        SymbolRate = uint.MaxValue
      };

      uint physicalChannelNumber;
      if (atscChannel != null)
      {
        frequencySettings.Frequency = uint.MaxValue;
        demodulatorSettings.Modulation = GetBdaModulation(atscChannel.ModulationScheme);
        physicalChannelNumber = (uint)atscChannel.PhysicalChannelNumber;
      }
      else
      {
        // Convert from centre frequency to the analog video carrier frequency.
        // This is a BDA convention.
        frequencySettings.Frequency = (uint)scteChannel.Frequency - 1750;
        demodulatorSettings.Modulation = GetBdaModulation(scteChannel.ModulationScheme);
        physicalChannelNumber = (uint)scteChannel.PhysicalChannelNumber;
      }
      return networkProvider.TuneATSC(physicalChannelNumber, frequencySettings, demodulatorSettings);
    }

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public override BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return BroadcastStandard.Atsc | BroadcastStandard.Scte;
      }
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
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
      if (scteChannel != null && scteChannel.Frequency <= 1750)  
      {
        return false;
      }
      return true;
    }

    #region tuning parameter translation

    private static ModulationType GetBdaModulation(ModulationSchemeVsb modulation)
    {
      switch (modulation)
      {
        case ModulationSchemeVsb.Vsb8:
          return ModulationType.Mod8Vsb;
        case ModulationSchemeVsb.Vsb16:
          return ModulationType.Mod16Vsb;
        case ModulationSchemeVsb.Automatic:
          Log.Warn("BDA ATSC: falling back to automatic modulation scheme");
          return ModulationType.ModNotSet;
        default:
          return (ModulationType)modulation;
      }
    }

    private static ModulationType GetBdaModulation(ModulationSchemeQam modulation)
    {
      switch (modulation)
      {
        case ModulationSchemeQam.Qam16:
          return ModulationType.Mod16Qam;
        case ModulationSchemeQam.Qam32:
          return ModulationType.Mod32Qam;
        case ModulationSchemeQam.Qam64:
          return ModulationType.Mod64Qam;
        case ModulationSchemeQam.Qam128:
          return ModulationType.Mod128Qam;
        case ModulationSchemeQam.Qam256:
          return ModulationType.Mod256Qam;
        case ModulationSchemeQam.Qam512:
          return ModulationType.Mod512Qam;
        case ModulationSchemeQam.Qam1024:
          return ModulationType.Mod1024Qam;
        case ModulationSchemeQam.Qam2048:
        case ModulationSchemeQam.Qam4096:
          Log.Warn("BDA ATSC: unsupported modulation scheme {0}, falling back to automatic", modulation);
          return ModulationType.ModNotSet;
        case ModulationSchemeQam.Automatic:
          Log.Warn("BDA ATSC: falling back to automatic modulation scheme");
          return ModulationType.ModNotSet;
        default:
          return (ModulationType)modulation;
      }
    }

    #endregion

    #endregion
  }
}