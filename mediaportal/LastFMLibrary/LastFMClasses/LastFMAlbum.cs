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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MediaPortal.LastFM

{
  public class LastFMAlbum
  {
    public string AlbumName { get; set; }
    public string ArtistName { get; set; }
    public string AlbumURL { get; set; }
    public string MusicBrainzId { get; set; }
    public int Playcount { get; set; }
    public int Listeners { get; set; }
    public List<LastFMImage> Images { get; set; }
    public List<LastFMTag> Tags { get; set; }
    public List<LastFMTrackBase> Tracks { get; set; }

    public LastFMAlbum(XDocument xDoc)
    {
      if (xDoc.Root == null) return;
      var albumElement = xDoc.Root.Element("album");
      if (albumElement == null) return;

      MusicBrainzId = (string) albumElement.Element("mbid");
      AlbumName = (string) albumElement.Element("name");
      ArtistName = (string) albumElement.Element("artist");
      AlbumURL = (string) albumElement.Element("url");
      Listeners = (int) albumElement.Element("listeners");
      Playcount = (int) albumElement.Element("playcount");

      Images = (from i in albumElement.Elements("image")
                select new LastFMImage(
                  LastFMImage.GetImageSizeEnum((string)i.Attribute("size")),
                  (string)i
                  )).ToList();

      Tags = (from tagElement in xDoc.Descendants("toptags").Elements("tag")
              let tagName = (string)tagElement.Element("name")
              let tagURL = (string)tagElement.Element("url")
              select new LastFMTag(tagName, tagURL)
             ).ToList();

      Tracks = (from trackElement in xDoc.Descendants("tracks").Elements("track")
                let trackName = (string) trackElement.Element("name")
                let duration = (int) trackElement.Element("duration")
                let trackURL = (string) trackElement.Element("url")
                select
                  new LastFMTrackBase
                    {
                      ArtistName = ArtistName,  // response does not include artists per track so pull from album
                      Duration = duration,
                      TrackTitle = trackName,
                      TrackURL = trackURL
                    }
               ).ToList();


    }

  }
}
