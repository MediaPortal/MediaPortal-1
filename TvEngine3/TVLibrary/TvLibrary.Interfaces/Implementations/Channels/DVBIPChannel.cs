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
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  /// <summary>
  /// A class capable of holding the tuning parameter details required to tune a DVB-IP channel.
  /// </summary>
  [Serializable]
  public class DVBIPChannel : DVBBaseChannel
  {
    #region variables

    private string _url = String.Empty;

    #endregion

    #region constructors

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBIPChannel"/> class.
    /// </summary>
    public DVBIPChannel()
      : base()
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBIPChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The chan</param>
    public DVBIPChannel(DVBIPChannel channel)
      : base(channel)
    {
      _url = channel.Url;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel's URL.
    /// </summary>
    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    #endregion

    /// <summary>
    /// Get a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>
    /// </returns>
    public override string ToString()
    {
      return String.Format("DVBIP:{0} Url:{1}", base.ToString(), Url);
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
      DVBIPChannel ch = obj as DVBIPChannel;
      if (ch == null)
      {
        return false;
      }
      if (!base.Equals(obj))
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
      return base.GetHashCode() ^ _url.GetHashCode();
    }

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      DVBIPChannel dvbipChannel = channel as DVBIPChannel;
      if (dvbipChannel == null)
      {
        return true;
      }
      return dvbipChannel.Url != _url;
    }
  }
}