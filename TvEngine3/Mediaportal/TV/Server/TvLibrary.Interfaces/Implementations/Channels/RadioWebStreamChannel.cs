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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels
{
  /// <summary>
  /// A class capable of holding the tuning parameter details required to tune a radio web stream.
  /// </summary>
  [Serializable]
  public class RadioWebStreamChannel : IChannel
  {
    #region variables

    private string _channelName;
    private string _url;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioWebStreamChannel"/> class.
    /// </summary>
    public RadioWebStreamChannel()
    {
      _channelName = String.Empty;
      _url = String.Empty;
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
    /// Get/set the channel's URL.
    /// </summary>
    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    /// <summary>
    /// Get/set whether the channel is a television channel.
    /// </summary>
    public bool IsTv
    {
      get { return false; }
      set { }
    }

    /// <summary>
    /// Get/set whether the channel is a radio channel.
    /// </summary>
    public bool IsRadio
    {
      get { return true; }
      set { }
    }

    /// <summary>
    /// Get/set whether the channel is a free-to-air or encrypted channel.
    /// </summary>
    public bool FreeToAir
    {
      get { return true; }
      set { }
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>
    /// </returns>
    public override string ToString()
    {
      string line = "radio:";
      line += String.Format("{0} Url:{1}", Name, Url);
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
      RadioWebStreamChannel ch = obj as RadioWebStreamChannel;
      if (ch == null)
      {
        return false;
      }

      if (!ch.Name.Equals(_channelName))
      {
        return false;
      }
      if (!ch.Url.Equals(_url))
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"></see></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _channelName.GetHashCode() ^ _url.GetHashCode();
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
      return true;
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

    public MediaTypeEnum MediaType
    {
      get { return MediaTypeEnum.Radio; }
      set { }
    }
  }
}