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
  public class LastFMImage
  {
    public enum LastFMImageSize
    {
      Unknown,
      Small,
      Medium,
      Large,
      ExtraLarge,
      Mega
    }

    public LastFMImageSize ImageSize { get; set; }
    public string ImageURL { get; set; }

    public LastFMImage(LastFMImageSize imageSize, string strUrl)
    {
      ImageSize = imageSize;
      ImageURL = strUrl;
    }

    public static LastFMImageSize GetImageSizeEnum(string strSize)
    {
      if (strSize == "small")
      {
        return LastFMImageSize.Small;
      }
      if (strSize == "medium")
      {
        return LastFMImageSize.Medium;
      }
      if (strSize == "large")
      {
        return LastFMImageSize.Large;
      }
      if (strSize == "extralarge")
      {
        return LastFMImageSize.ExtraLarge;
      }
      if (strSize == "mega")
      {
        return LastFMImageSize.Mega;
      }
      return LastFMImageSize.Unknown;
    }

    /// <summary>
    /// Given a list of images choose the best quality one
    /// </summary>
    /// <param name="images">List of LastFMImage instances to check</param>
    /// <returns>Best quality LastFMImage in list</returns>
    public static LastFMImage GetBestImage(List<LastFMImage> images)
    {
      //Mega
      foreach (var image in images.Where(image => image.ImageSize == LastFMImageSize.Mega))
      {
        return image;
      }
      //Extra-Large
      foreach (var image in images.Where(image => image.ImageSize == LastFMImageSize.ExtraLarge))
      {
        return image;
      }
      //Large
      foreach (var image in images.Where(image => image.ImageSize == LastFMImageSize.Large))
      {
        return image;
      }
      //Medium
      foreach (var image in images.Where(image => image.ImageSize == LastFMImageSize.Medium))
      {
        return image;
      }
      //Small
      return images.FirstOrDefault(image => image.ImageSize == LastFMImageSize.Small);      
    }



  }
}