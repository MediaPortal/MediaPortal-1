#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

#region usings

using System;

#endregion

namespace MediaPortal.Music.Database
{
  public class ScrobblerRequestException : ApplicationException
  {
    public ScrobblerRequestException(string message) : base(message)
    {
    }
  }

  public class ScrobblerUtilsRequest
  {
    #region enums

    public enum RequestType
    {
      GetAudioScrobblerFeed,
      GetValidURLLastFMString,
      FilterForLocalSongs,
      GetTopAlbums,
      GetAlbumInfo,
      GetTagInfo,
      GetArtistInfo,
      GetTagsForArtist,
      GetTagsForTrack,
      GetSimilarToTag,
      GetSimilarArtists,
      GetNeighboursArtists,
      GetFriendsArtists,
      GetRandomTracks,
      GetUnhearedTracks,
      GetFavoriteTracks,
      GetRadioPlaylist,
      Unknown
    }

    #endregion

    private static uint _lastRequestID = 0;

    public readonly uint ID;
    public readonly RequestType Type;

    public ScrobblerUtilsRequest(RequestType type)
    {
      if (_lastRequestID == 4294967295)
      {
        _lastRequestID = 0;
      }
      ID = ++_lastRequestID;
      Type = type;
    }

    public virtual void PerformRequest()
    {
      throw new ScrobblerRequestException("not implemented");
    }

    public override bool Equals(object o)
    {
      return o is ScrobblerUtilsRequest && ((ScrobblerUtilsRequest) o).ID == ID;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}