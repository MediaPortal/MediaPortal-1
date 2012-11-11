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
using System.Runtime.Serialization;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels
{
  /// <summary>
  /// class holding all tuning details for analog channels
  /// </summary>
  [DataContract]
  public class AnalogChannel : IChannel
  {
    #region enums

    /// <summary>
    /// Video input type.
    /// </summary>
    public enum VideoInputType
    {
      /// <summary>
      /// Tuner input
      /// </summary>
      Tuner,
      /// <summary>
      /// CVBS input #1
      /// </summary>
      VideoInput1,
      /// <summary>
      /// CVBS input #2
      /// </summary>
      VideoInput2,
      /// <summary>
      /// CVBS input #3
      /// </summary>
      VideoInput3,
      /// <summary>
      /// SVHS input #1
      /// </summary>
      SvhsInput1,
      /// <summary>
      /// SVHS input #2
      /// </summary>
      SvhsInput2,
      /// <summary>
      /// SVHS input #3
      /// </summary>
      SvhsInput3,
      /// <summary>
      /// RGB input #1
      /// </summary>
      RgbInput1,
      /// <summary>
      /// RGB input #2
      /// </summary>
      RgbInput2,
      /// <summary>
      /// RGB input #3
      /// </summary>
      RgbInput3,
      /// <summary>
      /// RGB input #1
      /// </summary>
      YRYBYInput1,
      /// <summary>
      /// RGB input #2
      /// </summary>
      YRYBYInput2,
      /// <summary>
      /// RGB input #3
      /// </summary>
      YRYBYInput3,
      /// <summary>
      /// HDMI input #1
      /// </summary>
      HdmiInput1,
      /// <summary>
      /// HDMI input #2
      /// </summary>
      HdmiInput2,
      /// <summary>
      /// HDMI input #3
      /// </summary>
      HdmiInput3
    }

    /// <summary>
    /// Audio input type.
    /// </summary>
    public enum AudioInputType
    {
      /// <summary>
      /// Automatic detection based on related pin index
      /// </summary>
      Automatic,
      /// <summary>
      /// Tuner input
      /// </summary>
      Tuner,
      /// <summary>
      /// AUX input #1
      /// </summary>
      AUXInput1,
      /// <summary>
      /// AUX input #2
      /// </summary>
      AUXInput2,
      /// <summary>
      /// AUX input #3
      /// </summary>
      AUXInput3,
      /// <summary>
      /// Line input #1
      /// </summary>
      LineInput1,
      /// <summary>
      /// Line input #2
      /// </summary>
      LineInput2,
      /// <summary>
      /// Line input #3
      /// </summary>
      LineInput3,
      /// <summary>
      /// SPDIF input #1
      /// </summary>
      SPDIFInput1,
      /// <summary>
      /// SPDIF input #2
      /// </summary>
      SPDIFInput2,
      /// <summary>
      /// SPDIF input #3
      /// </summary>
      SPDIFInput3
    }

    #endregion

    #region variables

    [DataMember]
    private string _channelName = String.Empty;

    [DataMember]
    private long _channelFrequency = -1;  // Used for FM radio; analog TV is usually tuned by channel number.

    [DataMember]
    private int _channelNumber = -1;

    [DataMember]
    private Country _country;

    [DataMember]
    private TunerInputType _tunerSource = TunerInputType.Cable;

    [DataMember]
    private VideoInputType _videoInputType = VideoInputType.Tuner;

    [DataMember]
    private AudioInputType _audioInputType = AudioInputType.Tuner;

    [DataMember]
    private bool _isVcrSignal;
    
    [DataMember]
    private bool _freeToAir = true;

    [DataMember]
    private MediaTypeEnum _mediaType;

    #endregion

    #region ctor

    /// <summary>
    /// Initialise a new instance of the <see cref="AnalogChannel"/> class.
    /// </summary>
    public AnalogChannel()
    {
      _channelName = String.Empty;
      _channelFrequency = -1;
      _channelNumber = 4;
      CountryCollection collection = new CountryCollection();
      _country = collection.GetTunerCountryFromID(31);  // The Netherlands.
      _tunerSource = TunerInputType.Cable;
      _videoInputType = VideoInputType.Tuner;
      _audioInputType = AudioInputType.Tuner;
      _isVcrSignal = false;
      _mediaType = MediaTypeEnum.TV;
      _freeToAir = true;
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="AnalogChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public AnalogChannel(AnalogChannel channel)
    {
      _channelName = channel.Name;
      _channelFrequency = channel.Frequency;
      _channelNumber = channel.ChannelNumber;
      _country = channel.Country;
      _tunerSource = channel.TunerSource;
      _videoInputType = channel.VideoSource;
      _audioInputType = channel.AudioSource;
      _isVcrSignal = channel.IsVcrSignal;
      _mediaType= channel.MediaType;      
      _freeToAir = channel.FreeToAir;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel's name.
    /// </summary>
    public string Name
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Get/set the carrier frequency for the channel. The frequency unit is kHz.
    /// </summary>
    public long Frequency
    {
      get { return _channelFrequency; }
      set { _channelFrequency = value; }
    }

    /// <summary>
    /// Get/set the channel number associated with the channel. This is converted to a frequency using per-country
    /// frequency-channel mappings.
    /// </summary>
    public int ChannelNumber
    {
      get { return _channelNumber; }
      set { _channelNumber = value; }
    }

    /// <summary>
    /// Get/set the country that the channel is broadcast in. This is used to convert the channel number to a
    /// frequency using per-country frequency-channel mappings.
    /// </summary>
    public Country Country
    {
      get { return _country; }
      set { _country = value; }
    }

    /// <summary>
    /// Get/set the tuner source type. This type is applied when the channel video source is set to
    /// <c>VideoSource.Tuner</c>.
    /// </summary>
    /// <value>The tuner source.</value>
    public TunerInputType TunerSource
    {
      get { return _tunerSource; }
      set { _tunerSource = value; }
    }

    /// <summary>
    /// Get/set the channel video input or source.
    /// </summary>
    public VideoInputType VideoSource
    {
      get { return _videoInputType; }
      set { _videoInputType = value; }
    }

    /// <summary>
    /// Get/set the channel audio input or source.
    /// </summary>
    public AudioInputType AudioSource
    {
      get { return _audioInputType; }
      set { _audioInputType = value; }
    }

    /// <summary>
    /// Get/set whether the channel is sourced from a VCR.
    /// </summary>
    public bool IsVcrSignal
    {
      get { return _isVcrSignal; }
      set { _isVcrSignal = value; }
    }

   

    /// <summary>
    /// Get/set whether the channel is a free-to-air or encrypted channel.
    /// </summary>
    public bool FreeToAir
    {
      get { return _freeToAir; }
      set { _freeToAir = value; }
    }

    public MediaTypeEnum MediaType
    {
      get { return _mediaType; }
      set { _mediaType = value; }
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Get a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>
    /// </returns>
    public override string ToString()
    {
      string line = MediaType.ToString();
      line += String.Format("{0} Freq:{1} Channel:{2} Country:{3} Tuner:{4} Video:{5} Audio:{6}",
                            Name, Frequency, ChannelNumber, Country.Name, TunerSource, VideoSource, AudioSource);
      return line;
    }

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>, otherwise <c>false</c>
    /// </returns>
    public override bool Equals(object obj)
    {
      AnalogChannel ch = obj as AnalogChannel;
      if (ch == null)
      {
        return false;
      }

      if (!ch.Name.Equals(_channelName))
      {
        return false;
      }
      if (ch.Frequency != _channelFrequency)
      {
        return false;
      }
      if (ch.ChannelNumber != _channelNumber)
      {
        return false;
      }
      if ((ch.Country == null && _country != null) ||
        (ch.Country != null && _country == null) ||
        (ch.Country != null && _country != null && ch.Country.Id != _country.Id))
      {
        return false;
      }
      if (ch.TunerSource != _tunerSource)
      {
        return false;
      }
      if (ch.VideoSource != _videoInputType)
      {
        return false;
      }
      if (ch.AudioSource != _audioInputType)
      {
        return false;
      }
      if (ch.IsVcrSignal != _isVcrSignal)
      {
        return false;
      }
      if (ch.MediaType != MediaType)
      {
        return false;
      }   
      if (ch.FreeToAir != _freeToAir)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _channelName.GetHashCode() ^ _channelFrequency.GetHashCode() ^
             _channelNumber.GetHashCode() ^ _country.GetHashCode() ^ _mediaType.GetHashCode() ^
             _tunerSource.GetHashCode() ^ _videoInputType.GetHashCode() ^ _audioInputType.GetHashCode() ^
             _isVcrSignal.GetHashCode();
    }

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the channel instance.
    /// </summary>
    /// <returns>a shallow clone of the channel instance</returns>
    public object Clone()
    {
      return MemberwiseClone();
    }

    #endregion

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public bool IsDifferentTransponder(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel == null)
      {
        return true;
      }
      return analogChannel.MediaType != MediaType ||
           analogChannel.Country.Id != Country.Id ||
           analogChannel.VideoSource != VideoSource ||
           analogChannel.TunerSource != TunerSource ||
           analogChannel.ChannelNumber != ChannelNumber ||
           analogChannel.Frequency != Frequency;
    }

    /// <summary>
    /// Get a channel instance with properties set to enable tuning of this channel.
    /// </summary>
    /// <returns>a channel instance with parameters adjusted as necessary</returns>
    public IChannel GetTuningChannel()
    {
      // No adjustments required.
      return (IChannel)Clone();
    }
  }
}