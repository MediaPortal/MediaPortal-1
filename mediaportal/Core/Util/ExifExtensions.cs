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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;

using MetadataExtractor;

namespace MediaPortal.Util
{
  public static class ExifExtensions
  {
    #region Exif mappings

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
      { "Location", GUILocalizeStrings.Get(9038) },
    };

    private static Dictionary<string, string> _translated = new Dictionary<string, string>();

    #endregion

    static ExifExtensions()
    {
      LoadTranslations();
    }

    public static string ToCaption(this string tag)
    {
      string result;
      return _fieldname.TryGetValue(tag, out result) ? result : tag;
    }

    public static string ToValue(this string tag)
    {
      string result;
      return _translated.TryGetValue(tag, out result) ? result : tag;
    }

    public static string ToLatitudeString(this double tag)
    {
      if (tag == 0)
      {
        return GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
      }
      return (tag > 0 ? GUILocalizeStrings.Get(9093) : GUILocalizeStrings.Get(9094)) + GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
    }

    public static string ToLongitudeString(this double tag)
    {
      if (tag == 0)
      {
        return GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
      }
      return (tag > 0 ? GUILocalizeStrings.Get(9095) : GUILocalizeStrings.Get(9096)) + GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
    }

    public static string ToAltitudeString(this double tag)
    {
      return tag == 0 ? GUILocalizeStrings.Get(9097) : String.Format(tag > 0 ? GUILocalizeStrings.Get(9098) : GUILocalizeStrings.Get(9099), Math.Round(tag, 2));
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

    #region Exif Properties

    public static void SetExifProperties(this ExifMetadata.Metadata metadata)
    {
      string full = string.Empty;
      Type type = typeof(ExifMetadata.Metadata);
      foreach (FieldInfo prop in type.GetFields())
      {
        string value = string.Empty;
        string caption = prop.Name.ToCaption() ?? prop.Name;
        switch (prop.Name)
        {
          case nameof(ExifMetadata.Metadata.ImageDimensions): 
            value = metadata.ImageDimensionsAsString(); 
            break;
          case nameof(ExifMetadata.Metadata.Resolution):
            value = metadata.ResolutionAsString(); 
            break;
          case nameof(ExifMetadata.Metadata.Location):
            if (metadata.Location != null)
            {
              string latitude = metadata.Location.Latitude.ToLatitudeString() ?? string.Empty;
              string longitude = metadata.Location.Longitude.ToLongitudeString() ?? string.Empty;
              if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
              {
                value = latitude + " / " + longitude;
              }
            }
            break;
          case nameof(ExifMetadata.Metadata.Altitude):
            if (metadata.Location != null)
            {
              value = metadata.Altitude.ToAltitudeString();
            }
            break;
          case nameof(ExifMetadata.Metadata.HDR):
            GUIPropertyManager.SetProperty("#pictures.exif.is" + prop.Name.ToLower(), metadata.HDR ? "true" : "false");
            continue;
          default:
            value = ((ExifMetadata.MetadataItem)prop.GetValue(metadata)).DisplayValue; 
            break;
        }
        if (!string.IsNullOrEmpty(value))
        {
          value = value.ToValue() ?? value;
          full = full + caption + ": " + value + "\n";
        }
        GUIPropertyManager.SetProperty("#pictures.exif." + prop.Name.ToLower(), value);
      }
      GUIPropertyManager.SetProperty("#pictures.exif.full", full);
      GUIPropertyManager.SetProperty("#pictures.haveexif", metadata.IsEmpty() ? "false" : "true");
    }

    public static List<string> GetExifInfoList(this ExifMetadata.Metadata metadata)
    {
      List<string> infoList = new List<string>();

      if (!metadata.EquipmentMake.IsEmpty())
      {
        infoList.Add(@"maker\" + metadata.EquipmentMake.DisplayValue + ".png");
      }

      if (!metadata.CameraModel.IsEmpty())
      {
        infoList.Add(@"camera\" + metadata.CameraModel.DisplayValue + ".png");
      }

      if (!metadata.ISO.IsEmpty())
      {
        infoList.Add(@"iso\" + metadata.ISO.DisplayValue + ".png");
      }

      if (!metadata.Fstop.IsEmpty())
      {
        string fstop = Regex.Replace(metadata.Fstop.DisplayValue, @"f\/(\d+?)[\.,](\d+?)", "$1.$2");
        if (!string.IsNullOrEmpty(fstop))
        {
          infoList.Add(@"fstop\" + fstop + ".png");
        }
      }

      string focal = (!metadata.FocalLength35MM.IsEmpty() ? metadata.FocalLength35MM.DisplayValue : metadata.FocalLength.DisplayValue);
      if (!string.IsNullOrEmpty(focal))
      {
        int intValue;
        if (int.TryParse(Regex.Replace(focal, @"(\d+?)(\s.*)?", "$1"), out intValue))
        {
          string lensType = string.Empty;
          if (intValue > 0 && intValue < 14)
          {
            lensType = "fisheye";
          }
          else if (intValue >= 14 && intValue < 40)
          {
            lensType = "wide";
          }
          else if (intValue >= 40 && intValue < 85)
          {
            lensType = "normal";
          }
          else if (intValue >= 86 && intValue < 400)
          {
            lensType = "telephoto";
          }
          else if (intValue >= 400)
          {
            lensType = "supertelephoto";
          }
          if (!string.IsNullOrEmpty(lensType))
          {
            infoList.Add(@"lens\" + lensType + ".png");
          }
        }
      }

      if (!metadata.ImageDimensions.IsEmpty)
      {
        if (metadata.ImageDimensions.Width > 0 && metadata.ImageDimensions.Height > 0)
        {
          double dRatio = (double)Math.Max(metadata.ImageDimensions.Width, metadata.ImageDimensions.Height) / (double)Math.Min(metadata.ImageDimensions.Width, metadata.ImageDimensions.Height);
          string aspect = string.Empty;
          if (dRatio >= 1.00 && dRatio < 3.00) // 1:1
          {
            // 1:1 - 1, 5:4 - 1.25, 4:3 - 1.33, 3:2 - 1.5, 5:3 - 1.67, 16:9 - 1.78
            aspect = Math.Round(dRatio, 2).ToString().Replace(",","."); 
          }
          else if (dRatio >= 3) // 3:1 ... Panorama
          {
            aspect = @"panorama"; 
          }

          if (!string.IsNullOrEmpty(aspect))
          {
            infoList.Add(@"aspect\" + aspect + ".png");
          }
        }
      }

      if (!metadata.Flash.IsEmpty())
      {
        string flash = string.Empty;
        if (metadata.Flash.DisplayValue.Contains("red-eye") && metadata.Flash.DisplayValue.Contains("auto"))
        {
          flash = "autoredeye";
        }
        else if (metadata.Flash.DisplayValue.Contains("red-eye"))
        {
          flash = "redeye";
        }
        else if (metadata.Flash.DisplayValue.Contains("auto"))
        {
          flash = "auto";
        }
        else if (metadata.Flash.DisplayValue.Contains("fired"))
        {
          flash = "flash";
        }
        else if (metadata.Flash.DisplayValue.ToLowerInvariant().Contains("strobe"))
        {
          flash = "strobe";
        }
        else if (metadata.Flash.DisplayValue.Contains("not fire"))
        {
          flash = "noflash";
        }
        if (!string.IsNullOrEmpty(flash))
        {
          infoList.Add(@"flash\" + flash + ".png");
        }
      }

      if (metadata.Location != null)
      {
        string latitude = string.Empty;
        string longitude = string.Empty;

        if (metadata.Location.Latitude > 0)
        {
          latitude = "N";
        }
        else if (metadata.Location.Latitude < 0)
        {
          latitude = "S";
        }
        if (metadata.Location.Longitude > 0)
        {
          longitude = "E";
        }
        else if (metadata.Location.Longitude < 0)
        {
          longitude = "W";
        }

        if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
        {
          infoList.Add(@"geo\" + latitude[0] + longitude[0] + ".png");
        }
        else
        {
          infoList.Add(@"geo\tagged.png");
        }
      }

      if (!metadata.CountryCode.IsEmpty())
      {
        infoList.Add(@"country\" + metadata.CountryCode.DisplayValue + ".png");
      }
      return infoList;
    }

    public static List<GUIOverlayImage> GetExifInfoOverlayImage(this ExifMetadata.Metadata metadata, ref int width, ref int height)
    {
      List<GUIOverlayImage> iconList = new List<GUIOverlayImage>();
      List<string> infoList = metadata.GetExifInfoList();

      bool vertical = height == 0;

      int i = 0;
      int step = 50;
      foreach (string info in infoList)
      {
        string image = GUIGraphicsContext.GetThemedSkinFile(@"\media\logos\exif\" + info);
        if (!File.Exists(image))
        {
          image = Thumbs.Pictures + @"\exif\" + info;
          if (!File.Exists(image))
          {
            continue;
          }
        }

        if (vertical)
        {
          if (height > 0)
          {
            height += step;
          }
          iconList.Add (new GUIOverlayImage(0, (width + step) * i, width, width, image));
          height += width;
        }
        else
        {
          if (width > 0)
          {
            width += step;
          }
          iconList.Add (new GUIOverlayImage((height + step) * i, 0, height, height, image));
          width += height;
        }
        i++;
      }
      return iconList;
    }

    #endregion

    #region Translation

    public static void LoadTranslations()
    {
      string _path = Config.GetSubFolder(Config.Dir.Language, "Exif");
      if (!System.IO.Directory.Exists(_path))
      {
        return;
      }

      string lang = string.Empty;
      try
      {
        lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
      }
      catch (Exception ex)
      {
        Log.Debug("Loadtranslations: {0}", ex.Message);
        lang = CultureInfo.CurrentUICulture.Name;
      }
      if (lang == "en")
      {
        return;
      }

      string langPath = Path.Combine(_path, lang + ".xml");
      if (!File.Exists(langPath))
      {
        return;
      }

      XmlDocument doc = new XmlDocument();
      try
      {
        Log.Debug(string.Format("EXIF Translation: Try load Translation file {0}.", langPath));
        doc.Load(langPath);
        Log.Info( string.Format("EXIF Translation: Translation file loaded {0}.", langPath));
      }
      catch (Exception e)
      {
        Log.Info(string.Format("EXIF Translation: Error in translation xml file: {0}. Failing back to English", lang));
        Log.Debug("EXIF Translation:" + e.ToString());
        return;
      }

      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        if (stringEntry.NodeType == XmlNodeType.Element)
        {
          try
          {
            _translated.Add(stringEntry.Attributes.GetNamedItem("name").Value, stringEntry.InnerText);
          }
          catch (Exception ex)
          {
            Log.Error("EXIF Translation:" + ex.ToString());
          }
        }
      }
    }

    #endregion
  }
}
