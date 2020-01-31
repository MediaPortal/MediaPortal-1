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
using MediaPortal.GUI.Library;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;

namespace MediaPortal.Util
{
  public static class ExifExtensions
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
      { ExifDirectoryBase.TagExposureBias, "Exposure Compensation" },
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

    private static readonly Dictionary<string, string> _fieldname = new Dictionary<string, string>()
    {
      { "DatePictureTaken", GUILocalizeStrings.Get(9006) },
      { "EquipmentMake", GUILocalizeStrings.Get(9010) },
      { "CameraModel", GUILocalizeStrings.Get(9009) },
      { "Author", GUILocalizeStrings.Get(9019) },
      { "ViewerComments", GUILocalizeStrings.Get(9011) },
      { "Copyright", GUILocalizeStrings.Get(9020) },
      { "Orientation", GUILocalizeStrings.Get(9021) },
      { "ISO", GUILocalizeStrings.Get(9022) },
      { "MeteringMode", GUILocalizeStrings.Get(9003) },
      { "Flash", GUILocalizeStrings.Get(9002) },
      { "ExposureTime", GUILocalizeStrings.Get(9008) },
      { "ExposureProgram", GUILocalizeStrings.Get(9023) },
      { "ExposureMode", GUILocalizeStrings.Get(9024) },
      { "ExposureCompensation", GUILocalizeStrings.Get(9025) },
      { "Fstop", GUILocalizeStrings.Get(9007) },
      { "ShutterSpeed", GUILocalizeStrings.Get(9005) },
      { "SensingMethod", GUILocalizeStrings.Get(9026) },
      { "SceneType", GUILocalizeStrings.Get(9027) },
      { "SceneCaptureType", GUILocalizeStrings.Get(9028) },
      { "WhiteBalance", GUILocalizeStrings.Get(9029) },
      { "Lens", GUILocalizeStrings.Get(9030) },
      { "FocalLength", GUILocalizeStrings.Get(9031) },
      { "FocalLength35MM", GUILocalizeStrings.Get(9032) },
      { "Comment", GUILocalizeStrings.Get(9018) },
      { "CountryCode", GUILocalizeStrings.Get(9013) },
      { "CountryName", GUILocalizeStrings.Get(9014) },
      { "ProvinceOrState", GUILocalizeStrings.Get(9015) },
      { "City", GUILocalizeStrings.Get(9016) },
      { "SubLocation", GUILocalizeStrings.Get(9017) },
      { "Keywords", GUILocalizeStrings.Get(9012) },
      { "ByLine", GUILocalizeStrings.Get(9033) },
      { "CopyrightNotice", GUILocalizeStrings.Get(9034) },
      { "Latitude", GUILocalizeStrings.Get(9035) },
      { "Longitude", GUILocalizeStrings.Get(9036) },
      { "Altitude", GUILocalizeStrings.Get(9037) },
      { "Resolution", GUILocalizeStrings.Get(9001) },
      { "ImageDimensions", GUILocalizeStrings.Get(9000) },
    };

    #endregion

    public static string ToExifString(this int tag)
    {
      string result;
      return _exif.TryGetValue(tag, out result) ? result : string.Empty;
    }

    public static string ToCaption(this string tag)
    {
      string result;
      return _fieldname.TryGetValue(tag, out result) ? result : string.Empty;
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
