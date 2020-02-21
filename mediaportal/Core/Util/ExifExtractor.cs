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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using MediaPortal.GUI.Library;
using MediaPortal.Util;

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Xmp;

namespace MediaPortal.GUI.Pictures
{
  #region Exif Read Routines

  public class ExifMetadata : IDisposable
  {
    public ExifMetadata() { }

    public void Dispose() { }

    public struct MetadataItem
    {
      public string Tag;
      public string Value;
      public string DisplayValue;

      public bool IsEmpty()
      {
        return string.IsNullOrWhiteSpace(DisplayValue);
      }
    }

    public struct Metadata
    {
      public MetadataItem DatePictureTaken;
      public MetadataItem CameraModel;
      public MetadataItem EquipmentMake;
      public MetadataItem Lens;
      public Size ImageDimensions;
      public MetadataItem Fstop;
      public MetadataItem ShutterSpeed;
      public MetadataItem ExposureCompensation;
      public MetadataItem ExposureProgram;
      public MetadataItem ExposureMode;
      public MetadataItem ExposureTime;
      public MetadataItem FocalLength;
      public MetadataItem FocalLength35MM;
      public MetadataItem ISO;
      public MetadataItem Flash;
      public MetadataItem WhiteBalance;
      public MetadataItem MeteringMode;
      public MetadataItem SensingMethod;
      public MetadataItem SceneType;
      public MetadataItem SceneCaptureType;
      public MetadataItem Orientation;
      public MetadataItem CountryCode;
      public MetadataItem CountryName;
      public MetadataItem ProvinceOrState;
      public MetadataItem City;
      public MetadataItem SubLocation;
      public GeoLocation Location; // A location != null always represents a valid value
      public double Altitude;
      public MetadataItem Author;
      public MetadataItem Copyright;
      public MetadataItem CopyrightNotice;
      public MetadataItem Comment;
      public MetadataItem ViewerComments;
      public MetadataItem ByLine;
      public Size Resolution;
      public MetadataItem Keywords;
      public bool HDR;

      public bool IsEmpty()
      {
        Type type = typeof(Metadata);
        bool result = true;

        foreach (FieldInfo prop in type.GetFields())
        {
          if (prop.Name == nameof(DatePictureTaken) || prop.Name == nameof(Orientation) ||
              prop.Name == nameof(ImageDimensions) || prop.Name == nameof(Resolution) ||
              prop.Name == nameof(Altitude) || prop.Name == nameof(HDR))
          {
            continue;
          }

          if (prop.Name == nameof(Location))
          {
            result &= (prop.GetValue(this) == null);
            continue;
          }

          Type fieldtype = prop.FieldType;
          MethodInfo info = fieldtype.GetMethod("IsEmpty");
          result &= (bool)info.Invoke(prop.GetValue(this), null);
        }
        return result;
      }

      public string ImageDimensionsAsString()
      {
        return !ImageDimensions.IsEmpty ? ImageDimensions.Width.ToString() + "x" + ImageDimensions.Height.ToString() : string.Empty;
      }

      public string ResolutionAsString()
      {
        return !Resolution.IsEmpty ? Resolution.Width.ToString() + "x" + Resolution.Height.ToString() : string.Empty;
      }
    }

    public int Count()
    {
      Type type = typeof(Metadata);
      FieldInfo[] fields = type.GetFields();
      return fields.Length;
    }

    private void SetStuff(ref MetadataItem item, Directory directory, int tag)
    {
      try
      {
        item.Tag = tag.ToString("X");
        item.DisplayValue = directory.GetDescription(tag);
        item.Value = string.Empty;

        switch (tag)
        {
          case ExifDirectoryBase.TagOrientation:
            {
              Int32 intValue;
              if (directory.TryGetInt32(tag, out intValue))
              {
                item.Value = intValue.ToString();
              }
              else
              {
                item.Value = "0";
              }
              break;
            }
          case ExifDirectoryBase.TagDateTime:
          case ExifDirectoryBase.TagDateTimeOriginal:
            {
              DateTime dateTime;
              if (directory.TryGetDateTime(tag, out dateTime))
              {
                item.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                item.DisplayValue = dateTime.ToString(System.Threading.Thread.CurrentThread.CurrentCulture);
              }
              break;
            }
          case ExifDirectoryBase.TagLensModel:
            {
              string lensMake = directory.GetDescription(ExifDirectoryBase.TagLensMake);
              if (!string.IsNullOrEmpty(lensMake))
              {
                item.Value = lensMake;
              }
              break;
            }
          case ExifDirectoryBase.TagUserComment:
            { 
              if (!string.IsNullOrEmpty(item.DisplayValue) && item.DisplayValue.StartsWith("ALCSII"))
              {
                item.DisplayValue = string.Empty;
              }
              break;
            }
          case IptcDirectory.TagKeywords:
            {
              string keywords = string.Empty;
              var keywordsArray = directory.GetStringArray(tag);
              if (keywordsArray != null)
              {
                foreach (string keyword in keywordsArray)
                {
                  if (!string.IsNullOrWhiteSpace(keyword) && !Regex.IsMatch(keyword, @"geo(tagged|\:.+?=\d+?\.\d+)"))
                  {
                    keywords += keyword.Trim() + "; ";
                  }
                }
              }
              item.DisplayValue = keywords;
              break;
            }
        }
      }
      catch (Exception ex)
      {
        Log.Error("ExifExtractor: SetStuff " + ex.Message);
      }
    }

    private void SetGPSData(GpsDirectory gpsDirectory, ref Metadata myMetadata)
    {
      // GPS RAW Altitude: 96
      double altitude;
      if (gpsDirectory.TryGetDouble(GpsDirectory.TagAltitude, out altitude))
      {
        myMetadata.Altitude = altitude;

        uint index;
        if (gpsDirectory.TryGetUInt32(GpsDirectory.TagAltitudeRef, out index))
        {
          myMetadata.Altitude = myMetadata.Altitude * (index == 1 ? -1 : 1);
        }
      }

      // GPS Location: 50,5323033300363, 30,4931270299872
      myMetadata.Location = gpsDirectory.GetGeoLocation();
      if (myMetadata.Location != null && myMetadata.Location.IsZero)
      {
        myMetadata.Location = null;
      }
    }

    public void SetGPSDataFromGeotags(string[] keywords, ref Metadata MyMetadata)
    {
      if (keywords != null && keywords.Contains<string>("geotagged"))
      {
        string lats = keywords.Where(x => x.StartsWith("geo:lat=")).FirstOrDefault();
        string lons = keywords.Where(x => x.StartsWith("geo:lon=")).FirstOrDefault();
        if (!String.IsNullOrEmpty(lats) && !String.IsNullOrEmpty(lons))
        {
          double lat;
          double lon;
          if (double.TryParse(lats.Substring(8), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out lat) &&
              double.TryParse(lons.Substring(8), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out lon))
          {
            MyMetadata.Location = new GeoLocation(lat, lon);
          }
        }
      }
    }

    public Metadata GetExifMetadata(string photoName)
    {
      // Create an instance of Metadata 
      Metadata MyMetadata = new Metadata();

      try
      {
        // Read EXIF Data
        IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(photoName);
        var exifDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

        if (exifDirectory != null)
        {
          // [Exif IFD0] Date/Time: 2017:08:19 16:22:38
          SetStuff(ref MyMetadata.DatePictureTaken, exifDirectory, ExifDirectoryBase.TagDateTime);

          // [Exif IFD0] Make: Equipment Make: NIKON CORPORATION
          SetStuff(ref MyMetadata.EquipmentMake, exifDirectory, ExifDirectoryBase.TagMake);

          // [Exif IFD0] Model: Camera Model: NIKON D90
          SetStuff(ref MyMetadata.CameraModel, exifDirectory, ExifDirectoryBase.TagModel);

          // [Exif IFD0] Artist: Author: Andrew J.Swan 
          SetStuff(ref MyMetadata.Author, exifDirectory, ExifDirectoryBase.TagArtist);

          // [Exif IFD0] Software: ApplicationName: Adobe Photoshop CC (Windows)
          SetStuff(ref MyMetadata.ViewerComments, exifDirectory, ExifDirectoryBase.TagSoftware);

          // [Exif IFD0] Copyright: Copyright: Copyright (c) 2014 by Andrew J.Swan
          SetStuff(ref MyMetadata.Copyright, exifDirectory, ExifDirectoryBase.TagCopyright);

          // [Exif IFD0] Orientation: Orientation: 1 - Normal
          SetStuff(ref MyMetadata.Orientation, exifDirectory, ExifDirectoryBase.TagOrientation);

          // [Exif IFD0] Resolution Unit: Resolution Unit: 2 - Inch
          // = exifDirectory.GetDescription(ExifDirectoryBase.TagResolutionUnit);

          int value;
          // [Exif IFD0] X Resolution = 300 dots per inch
          if (exifDirectory.TryGetInt32(ExifDirectoryBase.TagXResolution, out value))
          {
            MyMetadata.Resolution.Width = value;
          }
          // [Exif IFD0] Y Resolution = 300 dots per inch
          if (exifDirectory.TryGetInt32(ExifDirectoryBase.TagYResolution, out value))
          {
            MyMetadata.Resolution.Height = value;
          }
          if (MyMetadata.Resolution.Width <= 1 || MyMetadata.Resolution.Height <= 1)
          {
            MyMetadata.Resolution = Size.Empty;
          }
        }

        DateTime dateTime;
        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().Where((x) => x.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTime)).FirstOrDefault();
        if (subIfdDirectory != null)
        {
          // [Exif SubIFD] Date/Time Original: Date Picture Taken: 23/11/2014 17:09:41
          SetStuff(ref MyMetadata.DatePictureTaken, subIfdDirectory, ExifDirectoryBase.TagDateTimeOriginal);

          // [Exif SubIFD] ISO Speed Ratings: ISO: 200
          SetStuff(ref MyMetadata.ISO, subIfdDirectory, ExifDirectoryBase.TagIsoEquivalent);

          // [Exif SubIFD] Metering Mode: Metering Mode: 5 - Multi-segment
          SetStuff(ref MyMetadata.MeteringMode, subIfdDirectory, ExifDirectoryBase.TagMeteringMode);

          // [Exif SubIFD] Flash: Flash: 15 - Flash fired, compulsory flash mode, return light detected
          SetStuff(ref MyMetadata.Flash, subIfdDirectory, ExifDirectoryBase.TagFlash);

          // [Exif SubIFD] Exposure Time: Exposure Time: 1/60(s)
          SetStuff(ref MyMetadata.ExposureTime, subIfdDirectory, ExifDirectoryBase.TagExposureTime);

          // [Exif SubIFD] Exposure Program: Exposure Program: 2 - Normal program
          SetStuff(ref MyMetadata.ExposureProgram, subIfdDirectory, ExifDirectoryBase.TagExposureProgram);

          // [Exif SubIFD] Exposure Mode: Auto exposure
          SetStuff(ref MyMetadata.ExposureMode, subIfdDirectory, ExifDirectoryBase.TagExposureMode);

          // [Exif SubIFD] Exposure Bias Value: Exposure Compensation: 0/6
          SetStuff(ref MyMetadata.ExposureCompensation, subIfdDirectory, ExifDirectoryBase.TagExposureBias);

          // [Exif SubIFD] F-Number: FStop: F4
          SetStuff(ref MyMetadata.Fstop, subIfdDirectory, ExifDirectoryBase.TagFNumber);

          // [Exif SubIFD] Shutter Speed Value: Shutter Speed: 5,906891 (5906891/1000000)
          SetStuff(ref MyMetadata.ShutterSpeed, subIfdDirectory, ExifDirectoryBase.TagShutterSpeed);

          // [Exif SubIFD] Sensing Method: One-chip color area sensor
          SetStuff(ref MyMetadata.SensingMethod, subIfdDirectory, ExifDirectoryBase.TagSensingMethod);

          // [Exif SubIFD] Scene Type: Directly photographed image
          SetStuff(ref MyMetadata.SceneType, subIfdDirectory, ExifDirectoryBase.TagSceneType);

          // [Exif SubIFD] Scene Capture Type: Standard
          SetStuff(ref MyMetadata.SceneCaptureType, subIfdDirectory, ExifDirectoryBase.TagSceneCaptureType);

          // [Exif SubIFD] White Balance Mode:Auto white balance
          SetStuff(ref MyMetadata.WhiteBalance, subIfdDirectory, ExifDirectoryBase.TagWhiteBalanceMode);

          // [Exif SubIFD] Lens Make: NIKON
          // [Exif SubIFD] Lens Model: Lens Model: 35.0 mm f/1.8
          SetStuff(ref MyMetadata.Lens, subIfdDirectory, ExifDirectoryBase.TagLensModel);

          // [Exif SubIFD] Focal Length: Focal Length: 35 (350/10)
          SetStuff(ref MyMetadata.FocalLength, subIfdDirectory, ExifDirectoryBase.TagFocalLength);

          // [Exif SubIFD] Focal Length 35: Focal Length (35mm film): 52
          SetStuff(ref MyMetadata.FocalLength35MM, subIfdDirectory, ExifDirectoryBase.Tag35MMFilmEquivFocalLength);

          // [Exif SubIFD] User Comment: Comment: Copyright (C) by Andrew J.Swan
          SetStuff(ref MyMetadata.Comment, subIfdDirectory, ExifDirectoryBase.TagUserComment);
        }

        GetMakerNoteData(ref MyMetadata, directories);

        var iptcDirectory = directories.OfType<IptcDirectory>().FirstOrDefault();
        if (iptcDirectory != null)
        {
          // [IPTC] Country/Primary Location Code: UKR
          SetStuff(ref MyMetadata.CountryCode, iptcDirectory, IptcDirectory.TagCountryOrPrimaryLocationCode);

          // [IPTC] Country/Primary Location Name: Country: Ukraine
          SetStuff(ref MyMetadata.CountryName, iptcDirectory, IptcDirectory.TagCountryOrPrimaryLocationName);

          // [IPTC] Province/State: State: Kyiv
          SetStuff(ref MyMetadata.ProvinceOrState, iptcDirectory, IptcDirectory.TagProvinceOrState);

          // [IPTC] City: City: Kyiv
          SetStuff(ref MyMetadata.City, iptcDirectory, IptcDirectory.TagCity);

          // [IPTC] Sub-location: Sublocation: Mamaeva sloboda
          SetStuff(ref MyMetadata.SubLocation, iptcDirectory, IptcDirectory.TagSubLocation);

          // [IPTC] Keywords: Keywords: 2014, UKR, Ukraine, Kuiv, Mamaeva sloboda
          SetStuff(ref MyMetadata.Keywords, iptcDirectory, IptcDirectory.TagKeywords);

          // [IPTC] By-line: Jason P. Odell
          SetStuff(ref MyMetadata.ByLine, iptcDirectory, IptcDirectory.TagByLine);

          // [IPTC] Copyright Notice: © Jason P. Odell
          SetStuff(ref MyMetadata.CopyrightNotice, iptcDirectory, IptcDirectory.TagCopyrightNotice);

          // [IPTC] Caption/Abstract: For Educational Use Only
          // string captionAbstract = iptcDirectory.GetDescription(IptcDirectory.TagCaption);

          // Flickr GEO Tag: geotagged; geo:lat=57.64911; geo:lon=10.40744
          SetGPSDataFromGeotags(iptcDirectory.GetStringArray(IptcDirectory.TagKeywords), ref MyMetadata);
        }

        if (MyMetadata.Location == null)
        {
          var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
          if (gpsDirectory != null)
          {
            SetGPSData(gpsDirectory, ref MyMetadata);
          }
        }

        foreach (var directory in directories)
          if (!directory.Name.Contains("Thumbnail"))
          {
            if (MyMetadata.ImageDimensions.IsEmpty)
            {
              var wTag = directory.Tags.Where((x) => x.Name == "Image Width" || x.Name == "Exif Image Width").FirstOrDefault();
              var hTag = directory.Tags.Where((x) => x.Name == "Image Height" || x.Name == "Exif Image Height").FirstOrDefault();
              if (wTag != null && hTag != null)
              {
                MyMetadata.ImageDimensions.Width = directory.GetInt32(wTag.Type);
                MyMetadata.ImageDimensions.Height = directory.GetInt32(hTag.Type);
              }
            }

            if (MyMetadata.Resolution.IsEmpty)
            {
              var wTag = directory.Tags.Where((x) => x.Name == "X Resolution").FirstOrDefault();
              var hTag = directory.Tags.Where((x) => x.Name == "Y Resolution").FirstOrDefault();
              if (wTag != null && hTag != null)
              {
                MyMetadata.Resolution.Width = directory.GetInt32(wTag.Type);
                MyMetadata.Resolution.Height = directory.GetInt32(hTag.Type);
                if (MyMetadata.Resolution.Width <= 1 || MyMetadata.Resolution.Height <= 1)
                {
                  MyMetadata.Resolution = Size.Empty;
                }
              }
            }
          }

        if (MyMetadata.Resolution.IsEmpty || MyMetadata.ImageDimensions.IsEmpty)
        {
          try
          {
            // Only fetch them if they were not present in Exif data
            Picture.GetImageSizes(photoName, out MyMetadata.Resolution, out MyMetadata.ImageDimensions);
          }
          catch
          {
            Log.Debug("ExifExtractor: File does not have a valid image format. {0}", photoName);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("ExifExtractor: GetExifMetadata for {0}: {1}", photoName, ex.Message);
      }
      return MyMetadata;
    }

    private void GetMakerNoteData(ref Metadata item, IEnumerable<Directory> directory)
    {
      string lensModel = string.Empty;

      var nikonDirectory = directory.OfType<NikonType2MakernoteDirectory>().FirstOrDefault();
      if (nikonDirectory != null)
      {
        if (!item.Lens.IsEmpty())
        {
          return;
        }

        // [Nikon Makernote] Lens Type: AF, D
        string lensType = nikonDirectory.GetDescription(NikonType2MakernoteDirectory.TagLensType);
        if (string.IsNullOrWhiteSpace(lensModel))
        {
          lensType = string.Empty;
        }
        else
        {
          lensType = " " + lensType.Trim();
        }
        // [Nikon Makernote] Lens: 18.0-105.0 mm f/3.5-5.6
        lensModel = nikonDirectory.GetDescription(NikonType2MakernoteDirectory.TagLens);
        if (!string.IsNullOrEmpty(lensModel))
        {
          item.Lens.DisplayValue = lensModel + lensType;
          return;
        }
      }

      var canonDirectory = directory.OfType<CanonMakernoteDirectory>().FirstOrDefault();
      if (canonDirectory != null)
      {
        if (!item.Lens.IsEmpty())
        {
          return;
        }

        // [Canon Makernote] Lens Type: Canon EF-S 18-135mm f/3.5-5.6 IS
        lensModel = canonDirectory.GetDescription(CanonMakernoteDirectory.CameraSettings.TagLensType);
        if (!string.IsNullOrEmpty(lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
        // [Canon Makernote] Lens Model: EF-S18-135mm f/3.5-5.6 IS
        lensModel = canonDirectory.GetDescription(CanonMakernoteDirectory.TagLensModel);
        if (!string.IsNullOrEmpty(lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
      }

      var panasonicDirectory = directory.OfType<PanasonicMakernoteDirectory>().FirstOrDefault();
      if (panasonicDirectory != null)
      {
        // [Panasonic Makernote - 0x009e] HDR = On
        ushort value;
        if (panasonicDirectory.TryGetUInt16(PanasonicMakernoteDirectory.TagHDR, out value))
        {
          item.HDR = value != 0;
        }

        if (!item.Lens.IsEmpty())
        {
          return;
        }

        // [Panasonic Makernote] Lens Type: 14-150mm F/3.5-5.8 DiIII C001
        lensModel = panasonicDirectory.GetDescription(PanasonicMakernoteDirectory.TagLensType);
        if (!string.IsNullOrEmpty(lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
      }

      var olympusSettingsDirectory = directory.OfType<OlympusCameraSettingsMakernoteDirectory>().FirstOrDefault();
      if (olympusSettingsDirectory != null)
      {
        // [Olympus Camera Settings - 0x0509] Scene Mode = HDR
        string sceneMode = olympusSettingsDirectory.GetDescription(OlympusCameraSettingsMakernoteDirectory.TagSceneMode);
        if (!string.IsNullOrEmpty(sceneMode))
        {
          item.HDR = sceneMode == "HDR";
        }
      }

      var olympusDirectory = directory.OfType<OlympusEquipmentMakernoteDirectory>().FirstOrDefault();
      if (olympusDirectory != null)
      {
        if (!item.Lens.IsEmpty())
        {
          return;
        }

        // [Olympus Equipment] Lens Type: Olympus M.Zuiko Digital ED 12-50mm F3.5-6.3 EZ
        lensModel = olympusDirectory.GetDescription(OlympusEquipmentMakernoteDirectory.TagLensType);
        if (!string.IsNullOrEmpty(lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
        // [Olympus Equipment] Lens Model: OLYMPUS M.75mm F1.8
        lensModel = olympusDirectory.GetDescription(OlympusEquipmentMakernoteDirectory.TagLensModel);
        if (!string.IsNullOrEmpty(lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
      }

      var sonyDirectory = directory.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
      if (sonyDirectory != null)
      {
        // [Sony Makernote - 0x200a] HDR = 0
        ushort value;
        if (sonyDirectory.TryGetUInt16(SonyType1MakernoteDirectory.TagHdr, out value))
        {
          item.HDR = value != 0;
        }
      }

      var appleDirectory = directory.OfType<AppleMakernoteDirectory>().FirstOrDefault();
      if (appleDirectory != null)
      {
        // [Apple Makernote - 0x000a] HDR Image Type = HDR Image
        string imageType = appleDirectory.GetDescription(AppleMakernoteDirectory.TagHdrImageType);
        if (!string.IsNullOrEmpty(imageType))
        {
          item.HDR = imageType == "HDR Image";
        }
      }

      if (!item.Lens.IsEmpty())
      {
        return;
      }

      var xmpDirectory = directory.OfType<XmpDirectory>().FirstOrDefault();
      if (xmpDirectory != null)
      {
        var xmpDictionary = xmpDirectory.GetXmpProperties();
        // [XMPMeta] Lens: 18.0-105.0 mm f/3.5-5.6
        if (xmpDictionary.TryGetValue("aux:Lens", out lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
        // [XMPMeta] LensModel: iPhone 6 Plus back camera 4.15mm f/2.2
        if (xmpDictionary.TryGetValue("exifEX:LensModel", out lensModel))
        {
          item.Lens.DisplayValue = lensModel;
          return;
        }
      }
    }
  }

  #endregion
}