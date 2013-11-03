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
using System.Globalization;
using System.Linq;
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

  public class LastFMTrackInfo : LastFMTrackBase
  {
    public string MusicBrainzId { get; set; }
    public List<LastFMTag> TopTags { get; set; }
    public List<LastFMImage> Images { get; set; }
    public int Identifier { get; set; }
    public int Playcount { get; set; }
    public int Listeners { get; set; }

    public LastFMTrackInfo()  { }

    public LastFMTrackInfo(XDocument xDoc)
    {
      if (xDoc.Root == null) return;
      var track = xDoc.Root.Element("track");
      if (track == null) return;

      Identifier = (int) track.Element("id");
      TrackTitle = (string) track.Element("name");
      MusicBrainzId = (string) track.Element("mbid");
      TrackURL = (string) track.Element("url");
      Duration = (int) track.Element("duration");
      Listeners = (int) track.Element("listeners");
      Playcount = (int) track.Element("playcount");

      var artistElement = track.Element("artist");
      if (artistElement != null) ArtistName = (string) artistElement.Element("name");

      TopTags = (from tagElement in track.Descendants("tag")
                 let tagName = (string) tagElement.Element("name")
                 let tagURL = (string) tagElement.Element("url")
                 select new LastFMTag(tagName, tagURL)
                ).ToList();

      var albumElement = track.Element("album");
      if (albumElement != null)
      {
        Images = (from i in albumElement.Elements("image")
                  select new LastFMImage(
                    LastFMImage.GetImageSizeEnum((string) i.Attribute("size")),
                    (string) i
                    )
                 ).ToList();
      }
    }

  }

  public class LastFMSimilarTrack : LastFMTrackInfo
  {
    public float Match { get; set; }

    public static List<LastFMSimilarTrack> GetSimilarTracks(XDocument xDoc)
    {
      var ci = new CultureInfo("en-GB");
      if (xDoc != null)
      {
        var tracks = (from t in xDoc.Descendants("track")
                      let trackName = (string) t.Element("name")
                      let playcount = (string) t.Element("playcount")
                      let mbid = (string) t.Element("mbid")
                      let duration = (string) t.Element("duration")
                      let match = (string) t.Element("match")
                      let trackURL = (string) t.Element("url")
                      let artistElement = t.Element("artist")
                      where artistElement != null
                      let artistName = (string) artistElement.Element("name")
                      let images = (
                                     from i in t.Elements("image")
                                     select new LastFMImage(
                                       LastFMImage.GetImageSizeEnum((string) i.Attribute("size")),
                                       (string) i
                                       )
                                   ).ToList()
                      select new LastFMSimilarTrack
                               {
                                 TrackTitle = trackName,
                                 Playcount = string.IsNullOrEmpty(playcount) ? 0 : int.Parse(playcount, ci),
                                 MusicBrainzId = mbid,
                                 Duration = string.IsNullOrEmpty(duration) ? 0 : int.Parse(duration, ci),
                                 Match = string.IsNullOrEmpty(match) ? 0 : float.Parse(match, ci),
                                 TrackURL = trackURL,
                                 ArtistName = artistName,
                                 Images = images
                               }
                     ).ToList();
        return tracks;
      }
      return null;
    }

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
