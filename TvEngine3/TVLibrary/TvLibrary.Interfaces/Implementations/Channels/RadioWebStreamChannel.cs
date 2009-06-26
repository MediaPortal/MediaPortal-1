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
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// class holding all tuning details for radio webstream channels
  /// </summary>
  [Serializable]
  public class RadioWebStreamChannel : IChannel
  {
    #region variables

    private string _channelName;
    private string _url;
    private Country _country;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioWebStreamChannel"/> class.
    /// </summary>
    public RadioWebStreamChannel()
    {
      CountryCollection collection = new CountryCollection();
      _country = collection.GetTunerCountryFromID(31);
      Name = String.Empty;
      Url = String.Empty;
    }

    #endregion

    #region properties

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
    /// gets/sets the url
    /// </summary>
    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    /// <summary>
    /// boolean indicating if this is a radio channel
    /// </summary>
    public bool IsRadio
    {
      get { return true; }
      set { }
    }

    /// <summary>
    /// boolean indicating if this is a tv channel
    /// </summary>
    public bool IsTv
    {
      get { return false; }
      set { }
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      string line = "radio:";
      line += String.Format("{0} Url:{1} Country:{2}", Name, Url, Country.Name);
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
      if ((obj as RadioWebStreamChannel) == null)
      {
        return false;
      }
      RadioWebStreamChannel ch = obj as RadioWebStreamChannel;
      if (ch.Country.Id != Country.Id)
      {
        return false;
      }
      if (ch.Name != Name)
      {
        return false;
      }
      if (ch.Url != Url)
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
      return base.GetHashCode() ^ _channelName.GetHashCode() ^ _url.GetHashCode() ^
             _country.GetHashCode();
    }

    /// <summary>
    /// Checks if the given channel and this instance are on the different transponder
    /// </summary>
    /// <param name="channel">Channel to check</param>
    /// <returns>true, if the channels are on the same transponder</returns>
    public bool IsDifferentTransponder(IChannel channel)
    {
      return true;
    }
  }
}