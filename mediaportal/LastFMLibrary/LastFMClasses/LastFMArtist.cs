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

namespace MediaPortal.LastFM
{
  public class LastFMArtist
  {
    public string ArtistName { get; set; }
    public string ArtistURL { get; set; }
    public List<LastFMImage> Images { get; set; }
    public LastFMImage GetBestImage
    {
      get
      {
        //Mega
        foreach (var image in Images.Where(image => image.ImageSize == LastFMImage.LastFMImageSize.Mega))
        {
          return image;
        }
        //Extra-Large
        foreach (var image in Images.Where(image => image.ImageSize == LastFMImage.LastFMImageSize.ExtraLarge))
        {
          return image;
        }
        //Large
        foreach (var image in Images.Where(image => image.ImageSize == LastFMImage.LastFMImageSize.Large))
        {
          return image;
        }
        //Medium
        foreach (var image in Images.Where(image => image.ImageSize == LastFMImage.LastFMImageSize.Medium))
        {
          return image;
        }
        //Small
        return Images.FirstOrDefault(image => image.ImageSize == LastFMImage.LastFMImageSize.Small);
      }
    }


  }

  public class LastFMFullArtist : LastFMArtist
  {
    public string MusicBrainzID { get; set; }
    public List<string> BandMembers { get; set; }
    public int Listeners { get; set; }
    public int PlayCount { get; set; }
    public List<LastFMArtist> SimilarArtists { get; set; }
    public List<string> Tags { get; set; }
    public LastFMBio Bio { get; set; }

  }
}