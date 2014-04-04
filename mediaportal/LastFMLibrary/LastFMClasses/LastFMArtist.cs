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
  public class LastFMArtist
  {
    public string ArtistName { get; set; }
    public string ArtistURL { get; set; }
    public List<LastFMImage> Images { get; set; }
    public LastFMImage BestImage
    {
      get { return LastFMImage.GetBestImage(Images); }
    }

  }

  public class LastFMFullArtist : LastFMArtist
  {
    public string MusicBrainzId { get; set; }
    public List<LastFMBandMember> BandMembers { get; set; }
    public int Listeners { get; set; }
    public int Playcount { get; set; }
    public List<LastFMArtist> SimilarArtists { get; set; }
    public List<LastFMTag> Tags { get; set; }
    public LastFMBio Bio { get; set; } //TODO: last.fm have truncated this to 300 chars due to miuse.   Not sure this is worth it now

    /// <summary>
    /// CTOR based on xml returned by last.fm
    /// </summary>
    /// <param name="xDoc">xml returned by last.fm</param>
    public LastFMFullArtist(XDocument xDoc)
    {
      if (xDoc.Root == null) return;
      var artistElement = xDoc.Root.Element("artist");
      if (artistElement == null) return;

      MusicBrainzId = (string) artistElement.Element("mbid");
      ArtistName = (string) artistElement.Element("name");
      ArtistURL = (string) artistElement.Element("url");


      Images = (from i in artistElement.Elements("image")
                select new LastFMImage(
                  LastFMImage.GetImageSizeEnum((string) i.Attribute("size")),
                  (string) i
                  )).ToList();


      var stats = artistElement.Element("stats");
      if (stats != null)
      {
        Listeners = (int) stats.Element("listeners");
        Playcount = (int) stats.Element("playcount");
      }

      SimilarArtists = (from similarArtistElement in xDoc.Descendants("similar").Elements("artist")
                        let artistName = (string) similarArtistElement.Element("name")
                        let artistURL = (string) similarArtistElement.Element("url")
                        let artistImages = (
                                             from i in similarArtistElement.Elements("image")
                                             select new LastFMImage(
                                               LastFMImage.GetImageSizeEnum((string) i.Attribute("size")),
                                               (string) i
                                               )
                                           ).ToList()
                        select new LastFMArtist {ArtistName = artistName, ArtistURL = artistURL, Images = artistImages})
        .ToList();

      Tags = (from tagElement in xDoc.Descendants("tags").Elements("tag")
              let tagName = (string) tagElement.Element("name")
              let tagURL = (string) tagElement.Element("url")
              select new LastFMTag(tagName, tagURL)
             ).ToList();

      BandMembers = (from memberElement in xDoc.Descendants("bandmembers").Elements("member")
                     let memberName = (string) memberElement.Element("name")
                     let yearFrom = (string) memberElement.Element("yearfrom")
                     let yearTo = (string) memberElement.Element("yearto")
                     select new LastFMBandMember(memberName, yearFrom, yearTo)
                    ).ToList();

    }

  }

  public class LastFMBandMember
  {
    public string BandMember { get; set; }
    public string YearFrom { get; set; }
    public string YearTo { get; set; }

    public LastFMBandMember(string bandMember, string yearFrom, string yearTo)
    {
      BandMember = bandMember;
      YearFrom = yearFrom;
      YearTo = yearTo;
    }
  }
}