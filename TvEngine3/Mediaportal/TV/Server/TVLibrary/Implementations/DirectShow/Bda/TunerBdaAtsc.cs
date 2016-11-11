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
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
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
    #region constants

    private const string TUNING_SPACE_NAME_ATSC = "MediaPortal ATSC Tuning Space";
    private const string TUNING_SPACE_NAME_SCTE = "MediaPortal SCTE Tuning Space";

    #endregion

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
    /// Create a BDA tuning space for tuning a given channel type.
    /// </summary>
    /// <param name="channelType">The channel type.</param>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace(Type channelType)
    {
      this.LogDebug("BDA ATSC: create tuning space, type = {0}", channelType.Name);

      TunerInputType inputType = TunerInputType.Antenna;
      int majorChannelMaximum = 99;
      int minorChannelMaximum = 99;
      string name = TUNING_SPACE_NAME_ATSC;
      int physicalChannelMaximum = 69;
      int physicalChannelMinimum = 2;
      if (channelType == typeof(ChannelScte))
      {
        inputType = TunerInputType.Cable;
        majorChannelMaximum = 999;
        minorChannelMaximum = 999;
        name = TUNING_SPACE_NAME_SCTE;
        physicalChannelMaximum = 158;
        physicalChannelMinimum = 1;
      }

      IATSCTuningSpace tuningSpace = null;
      IATSCLocator locator = null;
      try
      {
        tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();
        int hr = tuningSpace.put_CountryCode(0);
        hr |= tuningSpace.put_FriendlyName(name);
        hr |= tuningSpace.put_InputType(inputType);
        hr |= tuningSpace.put_MaxChannel(majorChannelMaximum);
        hr |= tuningSpace.put_MaxMinorChannel(minorChannelMaximum);
        hr |= tuningSpace.put_MaxPhysicalChannel(physicalChannelMaximum);
        hr |= tuningSpace.put_MinChannel(1);
        hr |= tuningSpace.put_MinMinorChannel(1);
        hr |= tuningSpace.put_MinPhysicalChannel(physicalChannelMinimum);
        hr |= tuningSpace.put__NetworkType(NetworkType.ATSC_TERRESTRIAL);
        hr |= tuningSpace.put_UniqueName(name);

        locator = (IATSCLocator)new ATSCLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_PhysicalChannel(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_TSID(-1);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA ATSC: potential error creating tuning space, hr = 0x{0:x}, type = {1}", hr, channelType.Name);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject(string.Format("BDA ATSC tuner {0} tuning space", channelType.Name), ref tuningSpace);
        Release.ComObject(string.Format("BDA ATSC tuner {0} locator", channelType.Name), ref locator);
        throw;
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
    /// Get the name(s) of the registered BDA tuning space(s) for the tuner type.
    /// </summary>
    protected override IDictionary<string, Type> TuningSpaceNames
    {
      get
      {
        return new Dictionary<string, Type>
        {
          { TUNING_SPACE_NAME_ATSC, null },
          { TUNING_SPACE_NAME_SCTE, typeof(ChannelScte) }
        };
      }
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Atsc | StreamFormat.Scte;
      }
      return base.PerformLoading(streamFormat);
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

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
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

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA ATSC: potential error configuring locator, hr = 0x{0:x}", hr);
        }

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        TvExceptionDirectShowError.Throw(hr, "Failed to create tune request from tuning space.");

        IATSCChannelTuneRequest atscTuneRequest = tuneRequest as IATSCChannelTuneRequest;
        if (atscTuneRequest == null)
        {
          this.LogWarn("BDA ATSC: ATSC tune request interface is not available on tune request.");
        }

        // The MSDN remarks on IDigitalCableTuneRequest suggest there is more
        // than one way for the network provider to interpret the tune
        // request parameters.
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd693565%28v=vs.85%29.aspx
        //
        // Maybe we don't want to set these?
        else if (atscChannel != null)
        {
          hr |= atscTuneRequest.put_Channel(atscChannel.MajorChannelNumber);
          hr |= atscTuneRequest.put_MinorChannel(atscChannel.MinorChannelNumber);
        }
        else
        {
          hr |= atscTuneRequest.put_Channel(scteChannel.MajorChannelNumber);
          hr |= atscTuneRequest.put_MinorChannel(scteChannel.MinorChannelNumber);
        }
        hr |= tuneRequest.put_Locator(locator);

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA ATSC: potential error assembling tune request, hr = 0x{0:x}", hr);
        }

        return atscTuneRequest;
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

      ChannelScte scteChannel = channel as ChannelScte;
      return scteChannel == null || !scteChannel.IsCableCardNeededToTune();
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