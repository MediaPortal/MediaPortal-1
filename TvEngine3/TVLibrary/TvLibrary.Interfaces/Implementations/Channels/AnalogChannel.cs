using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using DirectShowLib;

namespace TvLibrary.Implementations
{

  /// <summary>
  /// class holding all tuning details for analog channels
  /// </summary>
  [Serializable]
  public class AnalogChannel : IChannel
  {
    #region enums
    public enum VideoInputType
    {
      Tuner,
      VideoInput1,
      VideoInput2,
      VideoInput3,
      SvhsInput1,
      SvhsInput2,
      SvhsInput3,
      RgbInput1,
      RgbInput2,
      RgbInput3
    }
    #endregion

    #region variables
    string _channelName;
    long _channelFrequency;
    int _channelNumber;
    Country _country;
    bool _isRadio;
    TunerInputType _tunerSource;
    VideoInputType _videoInputType;

    #endregion

    #region ctor

    public AnalogChannel()
    {
      CountryCollection collection = new CountryCollection();
      _country = collection.GetTunerCountryFromID(31);
      TunerSource = TunerInputType.Cable;
      _videoInputType = VideoInputType.Tuner;
      _channelNumber = 4;
      _isRadio = false;
      Name = String.Empty;
    }

    #endregion

    #region properties
    public VideoInputType VideoSource
    {
      get
      {
        return _videoInputType;
      }
      set
      {
        _videoInputType = value;
      }
    }
    /// <summary>
    /// gets/sets the country
    /// </summary>
    public TunerInputType TunerSource
    {
      get
      {
        return _tunerSource;
      }
      set
      {
        _tunerSource = value;
      }
    }
    /// <summary>
    /// gets/sets the country
    /// </summary>
    public Country Country
    {
      get
      {
        return _country;
      }
      set
      {
        _country = value;
      }
    }

    /// <summary>
    /// gets/sets the channel name
    /// </summary>
    public string Name
    {
      get
      {
        return _channelName;
      }
      set
      {
        _channelName = value;
      }
    }

    /// <summary>
    /// gets/sets the frequency
    /// </summary>
    public long Frequency
    {
      get
      {
        return _channelFrequency;
      }
      set
      {
        _channelFrequency = value;
      }
    }

    /// <summary>
    /// gets/sets the channel number
    /// </summary>
    public int ChannelNumber
    {
      get
      {
        return _channelNumber;
      }
      set
      {
        _channelNumber = value;
      }
    }
    /// <summary>
    /// boolean indicating if this is a radio channel
    /// </summary>
    public bool IsRadio
    {
      get
      {
        return _isRadio;
      }
      set
      {
        _isRadio = value;
      }
    }

    /// <summary>
    /// boolean indicating if this is a tv channel
    /// </summary>
    public bool IsTv
    {
      get
      {
        return !_isRadio;
      }
      set
      {
        _isRadio = !value;
      }
    }

    #endregion

    public override string ToString()
    {
      string line = "";
      if (IsRadio)
      {
        line = "radio:";
      }
      else
      {
        line = "tv:";
      }
      line += String.Format("{0} Freq:{1} Channel:{2} Country:{3} Tuner:{4} Video:{5}",
        Name, Frequency, ChannelNumber, Country.Name, TunerSource,VideoSource);
      return line;
    }


    public override bool Equals(object obj)
    {
      if ((obj as AnalogChannel) == null) return false;
      AnalogChannel ch = obj as AnalogChannel;
      if (ch.VideoSource != VideoSource) return false;
      if (ch.TunerSource != TunerSource) return false;
      if (ch.Country.Id != Country.Id) return false;
      if (ch.Name != Name) return false;
      if (ch.Frequency != Frequency) return false;
      if (ch.ChannelNumber != ChannelNumber) return false;
      if (ch.IsRadio != IsRadio) return false;
      if (ch.IsTv != IsTv) return false;
      return true;
    }
  }
}
