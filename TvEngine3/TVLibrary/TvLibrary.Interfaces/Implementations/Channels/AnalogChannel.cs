/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using DirectShowLib;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// class holding all tuning details for analog channels
  /// </summary>
  [Serializable]
  public class AnalogChannel : IChannel
  {
    #region enums

    /// <summary>
    /// Video input type
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
      YRYBYInput3
    }

    /// <summary>
    /// Audio input type
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

    private string _channelName;
    private long _channelFrequency;
    private int _channelNumber;
    private Country _country;
    private bool _isRadio;
    private TunerInputType _tunerSource;
    private VideoInputType _videoInputType;
    private AudioInputType _audioInputType;
    private bool _vcrSginal;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogChannel"/> class.
    /// </summary>
    public AnalogChannel()
    {
      CountryCollection collection = new CountryCollection();
      _country = collection.GetTunerCountryFromID(31);
      TunerSource = TunerInputType.Cable;
      _videoInputType = VideoInputType.Tuner;
      _audioInputType = AudioInputType.Automatic;
      _channelNumber = 4;
      _isRadio = false;
      _vcrSginal = false;
      Name = String.Empty;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets or sets the video source.
    /// </summary>
    /// <value>The video source.</value>
    public VideoInputType VideoSource
    {
      get { return _videoInputType; }
      set { _videoInputType = value; }
    }

    /// <summary>
    /// Gets or sets the audio source.
    /// </summary>
    /// <value>The audio source.</value>
    public AudioInputType AudioSource
    {
      get { return _audioInputType; }
      set { _audioInputType = value; }
    }

    /// <summary>
    /// Gets or sets the tuner source.
    /// </summary>
    /// <value>The tuner source.</value>
    public TunerInputType TunerSource
    {
      get { return _tunerSource; }
      set { _tunerSource = value; }
    }

    /// <summary>
    /// gets/sets the country
    /// </summary>
    public Country Country
    {
      get { return _country; }
      set { _country = value; }
    }

    /// <summary>
    /// gets/sets the channel name
    /// </summary>
    public string Name
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// gets/sets the frequency
    /// </summary>
    public long Frequency
    {
      get { return _channelFrequency; }
      set { _channelFrequency = value; }
    }

    /// <summary>
    /// gets/sets the channel number
    /// </summary>
    public int ChannelNumber
    {
      get { return _channelNumber; }
      set { _channelNumber = value; }
    }

    /// <summary>
    /// boolean indicating if this is a radio channel
    /// </summary>
    public bool IsRadio
    {
      get { return _isRadio; }
      set { _isRadio = value; }
    }

    /// <summary>
    /// boolean indicating if this is a tv channel
    /// </summary>
    public bool IsTv
    {
      get { return !_isRadio; }
      set { _isRadio = !value; }
    }

    /// <summary>
    /// boolean indicating if this is channel provides a vcr signal
    /// </summary>
    public bool IsVCRSignal
    {
      get { return _vcrSginal; }
      set { _vcrSginal = value; }
    }

    #endregion

    #region overrides
    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      string line = IsRadio ? "radio:" : "tv:";
      line += String.Format("{0} Freq:{1} Channel:{2} Country:{3} Tuner:{4} Video:{5} Audio:{6}",
                            Name, Frequency, ChannelNumber, Country.Name, TunerSource, VideoSource, AudioSource);
      return line;
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      if ((obj as AnalogChannel) == null)
      {
        return false;
      }
      AnalogChannel ch = obj as AnalogChannel;
      if (ch.VideoSource != VideoSource)
      {
        return false;
      }
      if (ch.AudioSource != AudioSource)
      {
        return false;
      }
      if (ch.TunerSource != TunerSource)
      {
        return false;
      }
      if (ch.Country.Id != Country.Id)
      {
        return false;
      }
      if (ch.Name != Name)
      {
        return false;
      }
      if (ch.Frequency != Frequency)
      {
        return false;
      }
      if (ch.ChannelNumber != ChannelNumber)
      {
        return false;
      }
      if (ch.IsRadio != IsRadio)
      {
        return false;
      }
      if (ch.IsTv != IsTv)
      {
        return false;
      }
      if (ch.IsVCRSignal != IsVCRSignal)
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
             _channelNumber.GetHashCode() ^ _country.GetHashCode() ^ _isRadio.GetHashCode() ^
             _tunerSource.GetHashCode() ^ _videoInputType.GetHashCode() ^ _audioInputType.GetHashCode() ^ _vcrSginal.GetHashCode();
    }
    #endregion
  }
}