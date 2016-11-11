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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using LcnSyntax = Mediaportal.TV.Server.Common.Types.Channel.LogicalChannelNumber;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for SCTE (OpenCable, clear
  /// QAM) channels.
  /// </summary>
  [DataContract]
  public class ChannelScte : ChannelMpeg2TsBase, IChannelQam
  {
    #region constants

    public const int FREQUENCY_OUT_OF_BAND_CHANNEL_SCAN = -1;
    public const int FREQUENCY_SWITCHED_DIGITAL_VIDEO = 0;

    #endregion

    #region variables

    [DataMember]
    private int _frequency = -1;

    [DataMember]
    private ModulationSchemeQam _modulationScheme = ModulationSchemeQam.Automatic;

    [DataMember]
    private int _sourceId = -1;

    #endregion

    #region properties

    /// <summary>
    /// Get the number of the physical (broadcast) channel that the channel is transmitted in.
    /// </summary>
    public short PhysicalChannelNumber
    {
      get
      {
        return GetPhysicalChannelNumberForFrequency(Frequency);
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's carrier frequency. The frequency unit is kilo-Hertz (kHz).
    /// </summary>
    /// <remarks>
    /// This is the center frequency. BDA needs the analog video carrier
    /// frequency, which can be obtained simply by subtracting 1750 kHz.
    /// </remarks>
    public int Frequency
    {
      get
      {
        return _frequency;
      }
      set
      {
        _frequency = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's modulation scheme.
    /// </summary>
    public ModulationSchemeQam ModulationScheme
    {
      get
      {
        return _modulationScheme;
      }
      set
      {
        _modulationScheme = value;
      }
    }

    /// <summary>
    /// Get the channel transmitter's symbol rate. The symbol rate unit is kilo-symbols per second (ks/s).
    /// </summary>
    /// <remarks>
    /// This parameter is typically not needed for tuning.
    /// </remarks>
    public int SymbolRate
    {
      get
      {
        // Refer to SCTE 07 table 3 "cable transmission format".
        switch (ModulationScheme)
        {
          case ModulationSchemeQam.Qam64:
            return 5057;
          case ModulationSchemeQam.Qam256:
            return 5361;
          default:
            return 6900;  // fake, probably doesn't matter anyway
        }
      }
      set
      {
      }
    }

    /// <summary>
    /// Get/set the channel's source identifier.
    /// </summary>
    public int SourceId
    {
      get
      {
        return _sourceId;
      }
      set
      {
        _sourceId = value;
      }
    }

    /// <summary>
    /// Get the channel's major channel number.
    /// </summary>
    public int MajorChannelNumber
    {
      get
      {
        ushort majorChannelNumber;
        ushort? minorChannelNumber;
        if (LcnSyntax.Parse(LogicalChannelNumber, out majorChannelNumber, out minorChannelNumber))
        {
          return (int)majorChannelNumber;
        }
        return -1;
      }
    }

    /// <summary>
    /// Get the channel's minor channel number.
    /// </summary>
    public int MinorChannelNumber
    {
      get
      {
        ushort majorChannelNumber;
        ushort? minorChannelNumber;
        if (LcnSyntax.Parse(LogicalChannelNumber, out majorChannelNumber, out minorChannelNumber) && minorChannelNumber.HasValue)
        {
          return (int)minorChannelNumber.Value;
        }
        return -1;
      }
    }

    #endregion

    #region IChannel members

    /// <summary>
    /// Get the default logical number associated with the channel.
    /// </summary>
    public override string DefaultLogicalChannelNumber
    {
      get
      {
        // This format is sometimes known as a QAM channel number.
        return string.Format("{0}.{1}", PhysicalChannelNumber, ProgramNumber);
      }
    }

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public override bool IsDifferentTransmitter(IChannel channel)
    {
      ChannelScte scteChannel = channel as ChannelScte;
      if (
        scteChannel == null ||
        Frequency != scteChannel.Frequency ||
        Frequency == FREQUENCY_OUT_OF_BAND_CHANNEL_SCAN ||
        Frequency == FREQUENCY_SWITCHED_DIGITAL_VIDEO ||
        ModulationScheme != scteChannel.ModulationScheme
      )
      {
        return true;
      }

      // CableCARD tuners do not provide an independent decrypt mechanism.
      // Applying a decrypt request also implies tuning and PID filtering. In
      // particular, it is possible to tune and decrypt an encrypted program,
      // and then also receive an additional non-encrypted program from the
      // same transmitter and transport stream by manipulation of the PID
      // filter. However it is not possible to reverse the order of operations
      // (tune non-encrypted program followed by encrypted program) because the
      // decrypt request would cause the PID filter to block the PIDs
      // associated with the non-encrypted program.
      // The condition below exists to force TVE to respect the CableCARD tuner
      // limitation described above. In reality encryption, channel numbers and
      // source IDs have nothing to do with whether channels/programs are
      // broadcast from different transmitters.
      if (
        IsEncrypted != scteChannel.IsEncrypted ||
        (
          IsEncrypted &&
          (
            LogicalChannelNumber != scteChannel.LogicalChannelNumber ||
            SourceId != scteChannel.SourceId
          )
        )
      )
      {
        return true;
      }
      return false;
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>, otherwise <c>false</c></returns>
    public override bool Equals(object obj)
    {
      ChannelScte channel = obj as ChannelScte;
      if (
        channel == null ||
        !base.Equals(obj) ||
        Frequency != channel.Frequency ||
        ModulationScheme != channel.ModulationScheme ||
        SourceId != channel.SourceId
      )
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// A hash function for this type.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"/></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ Frequency.GetHashCode() ^
              ModulationScheme.GetHashCode() ^ SourceId.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("SCTE, {0}, source ID = {1}, frequency = {2} kHz (#{3}), modulation scheme = {4}",
                            base.ToString(), SourceId, Frequency,
                            PhysicalChannelNumber, ModulationScheme);
    }

    #endregion

    /// <summary>
    /// Calculate the physical channel number corresponding with a centre
    /// frequency in any one of the US FCC cable frequency plans.
    /// </summary>
    /// <param name="carrierFrequency">The centre frequency. The unit is kilo-Hertz (kHz).</param>
    /// <returns>the physical channel number corresponding with <paramref name="carrierFrequency">the carrier frequency</paramref>></returns>
    public static short GetPhysicalChannelNumberForFrequency(int carrierFrequency)
    {
      if (carrierFrequency >= 648000)
      {
        short channelNumber = (short)(100 + ((carrierFrequency - 648000) / 6000));
        if (channelNumber > 158)
        {
          channelNumber = 0;
        }
        return channelNumber;
      }
      if (carrierFrequency >= 216000)
      {
        return (short)(23 + ((carrierFrequency - 216000) / 6000));
      }
      if (carrierFrequency >= 174000)
      {
        return (short)(7 + ((carrierFrequency - 174000) / 6000));
      }
      if (carrierFrequency >= 120000)
      {
        return (short)(14 + ((carrierFrequency - 120000) / 6000));
      }
      if (carrierFrequency >= 90000)
      {
        return (short)(95 + ((carrierFrequency - 90000) / 6000));
      }

      // Be careful with channels 1, 5 and 6. They're less standard.
      if (carrierFrequency >= 76500)
      {
        return (short)(5 + ((carrierFrequency - 76500) / 6000));
      }
      if (carrierFrequency >= 72000)
      {
        return 1;
      }

      if (carrierFrequency >= 54000)
      {
        return (short)(2 + ((carrierFrequency - 54000) / 6000));
      }
      return 0;
    }

    /// <summary>
    /// Determine if a CableCARD is needed to tune the channel.
    /// </summary>
    /// <returns><c>true</c> if a CableCARD is needed to tune the channel, otherwise <c>false</c></returns>
    public bool IsCableCardNeededToTune()
    {
      if (
        IsEncrypted ||
        IsOutOfBandScanChannel() ||
        Frequency == FREQUENCY_SWITCHED_DIGITAL_VIDEO
      )
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determine if the channel is the out-of-band scan channel.
    /// </summary>
    /// <returns><c>true</c> if the channel is the out-of-band scan channel, otherwise <c>false</c></returns>
    public bool IsOutOfBandScanChannel()
    {
      return Frequency == FREQUENCY_OUT_OF_BAND_CHANNEL_SCAN;
    }
  }
}