#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#region Usings

using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;


#endregion

namespace TvThumbnails
{
  /// <summary>
  /// Summary description for Thumbs.
  /// </summary>
  public class Thumbs
  {
    public enum ThumbQuality
    {
      fastest = 0,
      fast = 1,
      average = 2,
      higher = 3,
      highest = 4,
    }

    public enum LargeThumbSize
    {
      small = 400,
      average = 500,
      large = 600,
    }

    public enum ThumbSize
    {
      small = 100,
      average = 120,
      large = 140,
    }

    private static bool _enableThumbCreation;
    private static bool _leaveShareThumb;
    private static int _previewColumns = 1;
    private static int _previewRows = 1;

    private static string _recTvThumbsFolder = PathManager.GetDataPath + @"\thumbs";

    private static ThumbQuality _currentThumbQuality = ThumbQuality.highest;
    private static CompositingQuality _currentCompositingQuality = CompositingQuality.Default;
    private static InterpolationMode _currentInterpolationMode = InterpolationMode.Default;
    private static SmoothingMode _currentSmoothingMode = SmoothingMode.Default;

    private static LargeThumbSize _currentLargeThumbSize = LargeThumbSize.large;
    private static ThumbSize _currentThumbSize = ThumbSize.large;
    private static readonly ImageFormat _currentThumbFormat = ImageFormat.Jpeg;

    private static ImageCodecInfo _currentImageCodecInfo;
    private static EncoderParameters _currentEncoderParams;
    private static int _TimeOffset = 1;
    public static void LoadSettings()
    {
      Log.Debug("Thumbs.LoadSettings()");
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();

        _enableThumbCreation = (layer.GetSetting("TVThumbnailsEnabled", "yes").Value == "yes");
        Log.Debug("Thumbs.LoadSettings: Enable Thumbs: {0}", _enableThumbCreation);

        _leaveShareThumb = (layer.GetSetting("TVThumbnailsLeaveShareThumb", "no").Value == "yes");
        Log.Debug("Thumbs.LoadSettings: Share preview: {0}", _leaveShareThumb);

        _previewColumns = Convert.ToInt32(layer.GetSetting("TVThumbnailsColumns", "1").Value);

        _previewRows = Convert.ToInt32(layer.GetSetting("TVThumbnailsRows", "1").Value);

        _TimeOffset = Convert.ToInt32(layer.GetSetting("TVThumbnailsTimeOffset", "1").Value);

        int configQuality = Convert.ToInt32(layer.GetSetting("TVThumbnailsQuality", "4").Value);
        Log.Debug("Thumbs.LoadSettings: Thumbs quality: {0}", configQuality);
        switch (configQuality)
        {
          case 0:
            Quality = ThumbQuality.fastest;
            SetEncoderParams(25);
            Log.Info("Thumbs.LoadSettings: using the fastest thumbnail mode");
            break;
          case 1:
            Quality = ThumbQuality.fast;
            SetEncoderParams(33);
            Log.Info("Thumbs.LoadSettings: using fast thumbnails");
            break;
          case 2:
            Quality = ThumbQuality.average;
            SetEncoderParams(50);
            Log.Info("Thumbs.LoadSettings: using average thumbnails");
            break;
          case 3:
            Quality = ThumbQuality.higher;
            SetEncoderParams(77);
            Log.Info("Thumbs.LoadSettings: using high quality thumbnails");
            break;
          case 4:
            Quality = ThumbQuality.highest;
            SetEncoderParams(97);
            Log.Info("Thumbs.LoadSettings: using highest quality thumbnail mode");
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Thumbs.LoadSettings: Error loading settings - {0}", ex.Message);
      }
    }

    public static void CreateFolders()
    {
      try
      {
        Log.Debug("Thumbs.CreateFolders()");
        if (!Directory.Exists(_recTvThumbsFolder))
        {
          Directory.CreateDirectory(_recTvThumbsFolder);
          Log.Debug("Thumbs.CreateFolders: Folder {0} created", _recTvThumbsFolder);
        }

      }
      catch (Exception ex)
      {
        Log.Error("Thumbs.CreateFolders: Could not create folder {0} - {1}", _recTvThumbsFolder, ex.Message);
      }
    }

    #region Public properties

    public static bool Enabled
    {
      get { return _enableThumbCreation; }
    }

    public static bool LeaveShareThumb
    {
      get { return _leaveShareThumb; }
    }

    public static int PreviewColumns
    {
      get { return _previewColumns; }
    }

    public static int PreviewRows
    {
      get { return _previewRows; }
    }

    public static int TimeOffset
    {
      get { return _TimeOffset; }
    }

    public static string ThumbnailFolder
    {
      get { return _recTvThumbsFolder; }
    }

    public static bool SpeedThumbsSmall
    {
      get
      {
        switch (_currentThumbQuality)
        {
          case ThumbQuality.fastest:
            return true;
          case ThumbQuality.fast:
            return true;
          case ThumbQuality.average:
            return true;
          case ThumbQuality.higher:
            return true;
          case ThumbQuality.highest:
            return false;
          default:
            return true;
        }
      }
    }

    public static bool SpeedThumbsLarge
    {
      get
      {
        switch (_currentThumbQuality)
        {
          case ThumbQuality.fastest:
            return true;
          case ThumbQuality.fast:
            return true;
          case ThumbQuality.average:
            return true;
          case ThumbQuality.higher:
            return false;
          case ThumbQuality.highest:
            return false;
          default:
            return true;
        }
      }
    }

    public static string GetThumbExtension()
    {
      if (Thumbs.ThumbFormat == ImageFormat.Jpeg)
        return ".jpg";
      else if (Thumbs.ThumbFormat == ImageFormat.Png)
        return ".png";
      else if (Thumbs.ThumbFormat == ImageFormat.Gif)
        return ".gif";
      else if (Thumbs.ThumbFormat == ImageFormat.Icon)
        return ".ico";
      else if (Thumbs.ThumbFormat == ImageFormat.Bmp)
        return ".bmp";

      return ".jpg";
    }

    /// <summary>
    /// Change thumbnail quality
    /// </summary>
    public static ThumbQuality Quality
    {
      get { return _currentThumbQuality; }
      set
      {
        if (value != _currentThumbQuality)
        {
          _currentThumbQuality = value;
          SetQualityParams(_currentThumbQuality);
        }
      }
    }

    public static CompositingQuality Compositing
    {
      get { return _currentCompositingQuality; }
    }

    public static InterpolationMode Interpolation
    {
      get { return _currentInterpolationMode; }
    }

    public static SmoothingMode Smoothing
    {
      get { return _currentSmoothingMode; }
    }

    public static ThumbSize ThumbResolution
    {
      get { return _currentThumbSize; }
    }

    public static LargeThumbSize ThumbLargeResolution
    {
      get { return _currentLargeThumbSize; }
    }

    public static ImageFormat ThumbFormat
    {
      get { return _currentThumbFormat; }
    }

    public static ImageCodecInfo ThumbCodecInfo
    {
      get { return _currentImageCodecInfo; }
    }

    public static EncoderParameters ThumbEncoderParams
    {
      get { return _currentEncoderParams; }
    }

    #endregion

    private static void SetEncoderParams(int aQuality)
    {
      if (aQuality < 1 || aQuality > 100)
        return;

      // This will specify the image quality to the encoder
      EncoderParameter epQuality = new EncoderParameter(Encoder.Quality, aQuality);
      // Get all image codecs that are available
      ImageCodecInfo[] ImgEncoders = ImageCodecInfo.GetImageEncoders();
      // Store the quality parameter in the list of encoder parameters
      _currentEncoderParams = new EncoderParameters(1);
      _currentEncoderParams.Param[0] = epQuality;

      // Loop through all the image codecs
      for (int i = 0; i < ImgEncoders.Length; i++)
      {
        // Until the one that we are interested in is found, which might be "*.JPG;*.JPEG;*.JPE;*.JFIF"
        string[] possibleExtensions = ImgEncoders[i].FilenameExtension.Split(new[] { ';' },
                                                                             StringSplitOptions.RemoveEmptyEntries);
        foreach (string ext in possibleExtensions)
        {
          // .jpg in *.JPG ?
          if (ext.ToUpperInvariant().Contains(GetThumbExtension().ToUpperInvariant()))
          {
            _currentImageCodecInfo = ImgEncoders[i];
            break;
          }
        }
      }
    }

    private static void SetQualityParams(ThumbQuality quality_)
    {
      switch (quality_)
      {
        case ThumbQuality.fastest:
          _currentCompositingQuality = CompositingQuality.HighSpeed;
          _currentInterpolationMode = InterpolationMode.NearestNeighbor;
          _currentSmoothingMode = SmoothingMode.None;
          _currentThumbSize = ThumbSize.small;
          _currentLargeThumbSize = LargeThumbSize.small;
          break;

        case ThumbQuality.fast:
          _currentCompositingQuality = CompositingQuality.HighSpeed;
          _currentInterpolationMode = InterpolationMode.Low;
          _currentSmoothingMode = SmoothingMode.HighSpeed;
          _currentThumbSize = ThumbSize.small;
          _currentLargeThumbSize = LargeThumbSize.small;
          break;

        case ThumbQuality.higher:
          _currentCompositingQuality = CompositingQuality.AssumeLinear;
          _currentInterpolationMode = InterpolationMode.High;
          _currentSmoothingMode = SmoothingMode.HighQuality;
          _currentThumbSize = ThumbSize.average;
          _currentLargeThumbSize = LargeThumbSize.average;
          break;

        case ThumbQuality.highest:
          _currentCompositingQuality = CompositingQuality.HighQuality;
          _currentInterpolationMode = InterpolationMode.HighQualityBicubic;
          _currentSmoothingMode = SmoothingMode.HighQuality;
          _currentThumbSize = ThumbSize.large;
          _currentLargeThumbSize = LargeThumbSize.large;
          break;

        default:
          _currentCompositingQuality = CompositingQuality.Default;
          _currentInterpolationMode = InterpolationMode.Default;
          _currentSmoothingMode = SmoothingMode.Default;
          _currentThumbSize = ThumbSize.average;
          _currentLargeThumbSize = LargeThumbSize.average;
          break;
      }
    }
  }
}