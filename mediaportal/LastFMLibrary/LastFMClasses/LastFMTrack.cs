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

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MediaPortal.LastFM
{
  public class LastFMTrackBase
  {
    public string ArtistName { get; set; }
    public string TrackTitle { get; set; }
    public int Duration { get; set; }
    public string TrackURL { get; set; }
  }

  public class LastFMTrackInfo
  {
    public string TrackTitle { get; set; }
    public LastFMArtist Artist { get; set; }
    public string MusicBrainzID { get; set; }
    public List<LastFMTag> Tags { get; set; }
    public List<LastFMImage> Images { get; set; }
    public int Duration { get; set; }
    public int Identifier { get; set; }
    public int Playcount { get; set; }
    public int Listeners { get; set; }

    public LastFMTrackInfo()  { }

    public LastFMTrackInfo(XDocument xDoc)
    {
      
    }

  }

  public class LastFMSimilarTrack : LastFMTrackBase
  {
    public string MusicBrainzID { get; set; }
    public List<LastFMImage> Images { get; set; }
    public int Playcount { get; set; }
    public float Match { get; set; }
  }

  public class LastFMStreamingTrack : LastFMTrackBase
  {
    public int Identifier { get; set; }
    public string TrackStreamingURL { get; set; }
    public string ImageURL { get; set; }
  }

  public class LastFMScrobbleTrack : LastFMTrackBase
  {
    public DateTime DatePlayed { get; set; }
    public bool UserSelected { get; set; }
    public string AlbumName { get; set; }
  }

}
