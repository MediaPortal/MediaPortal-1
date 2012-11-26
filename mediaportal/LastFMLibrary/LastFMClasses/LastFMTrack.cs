#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

namespace MediaPortal.LastFM
{
  public class LastFMTrack
  {
    public string ArtistName { get; set; }
    public string TrackTitle { get; set; }

    public LastFMTrack (string strArtist, string strTrack)
    {
      ArtistName = strArtist;
      TrackTitle = strTrack;
    }

    public LastFMTrack() { }

  }

  public class LastFMStreamingTrack : LastFMTrack
  {
    public string TrackURL { get; set; }
    public int Duration { get; set; }

    public LastFMStreamingTrack(string strArtist, string strTrack, string strURL, int iDuration)
    {
      ArtistName = strArtist;
      TrackTitle = strTrack;
      TrackURL = strURL;
      Duration = iDuration;
    }

    public LastFMStreamingTrack()   { }

  }

}
