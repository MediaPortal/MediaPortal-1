#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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

using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;

namespace MediaPortal.Util
{
  public static class ExifExtenstions
  {
    #region mappings

    private static readonly Dictionary<int, string> _exif = new Dictionary<int, string>()
    {
      { ExifDirectoryBase.TagDateTime, "Date Picture Taken" },
      { ExifDirectoryBase.TagMake, "Equipment Make" },
      { ExifDirectoryBase.TagModel, "Camera Model" },
      { ExifDirectoryBase.TagArtist, "Author" },
      { ExifDirectoryBase.TagSoftware, "Application Name" },
      { ExifDirectoryBase.TagCopyright, "Copyright" },
      { ExifDirectoryBase.TagOrientation, "Orientation" },
      { ExifDirectoryBase.TagResolutionUnit, "Resolution Unit" },
      { ExifDirectoryBase.TagDateTimeOriginal, "Date/Time Original" },
      { ExifDirectoryBase.TagIsoEquivalent, "ISO" },
      { ExifDirectoryBase.TagMeteringMode, "Metering Mode" },
      { ExifDirectoryBase.TagFlash, "Flash" },
      { ExifDirectoryBase.TagExposureTime, "Exposure Time" },
      { ExifDirectoryBase.TagExposureProgram, "Exposure Program" },
      { ExifDirectoryBase.TagExposureMode, "Exposure Mode" },
      { ExifDirectoryBase.TagFNumber, "FStop" },
      { ExifDirectoryBase.TagShutterSpeed, "Shutter Speed" },
      { ExifDirectoryBase.TagSensingMethod, "Sensing Method" },
      { ExifDirectoryBase.TagSceneType, "Scene Type" },
      { ExifDirectoryBase.TagSceneCaptureType, "Scene Capture Type" },
      { ExifDirectoryBase.TagWhiteBalanceMode, "White Balance Mode" },
      { ExifDirectoryBase.TagLensMake, "Lens Make" },
      { ExifDirectoryBase.TagLensModel, "Lens Model" },
      { ExifDirectoryBase.TagFocalLength, "Focal Length" },
      { ExifDirectoryBase.Tag35MMFilmEquivFocalLength, "Focal Length (35mm film)" },
      { ExifDirectoryBase.TagUserComment, "Comment" },
      { IptcDirectory.TagCountryOrPrimaryLocationCode, "Country/Primary Location Code" },
      { IptcDirectory.TagCountryOrPrimaryLocationName, "Country/Primary Location Name" },
      { IptcDirectory.TagProvinceOrState, "Province/State" },
      { IptcDirectory.TagCity, "City" },
      { IptcDirectory.TagSubLocation, "Sublocation" },
      { IptcDirectory.TagKeywords, "Keywords" },
      { IptcDirectory.TagByLine, "By-line" },
      { IptcDirectory.TagCopyrightNotice, "Copyright Notice" },
      { IptcDirectory.TagCaption, "Caption/Abstract" },
      { GpsDirectory.TagLatitude, "GPS Latitude" },
      { GpsDirectory.TagLongitude, "GPS Longitude" },
      { GpsDirectory.TagAltitude, "GPS Altitude" },
    };

    #endregion

    public static string ToExifString(this int tag)
    {
      string result;
      return _exif.TryGetValue(tag, out result) ? result : string.Empty;
    }

    public static int ToRotation(this int orientation)
    {
      if (orientation == 6)
      {
        return 1; // 90 degree:  112/03/06 00
      }
      if (orientation == 3)
      {
        return 2; // 180 degree: 112/03/03 00
      }
      if (orientation == 8)
      {
        return 3; // 270 degree: 112/03/08 00
      }
      return 0; // not rotated
    }
  }
}
