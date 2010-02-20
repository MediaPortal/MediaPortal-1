#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
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

    public static readonly string TvNotifyIcon = "tvguide_notify_button.png";
    public static readonly string TvRecordingIcon = "tvguide_record_button.png";
    public static readonly string TvRecordingSeriesIcon = "tvguide_recordserie_button.png";
    public static readonly string TvConflictRecordingIcon = "tvguide_recordconflict_button.png";
    public static readonly string TvConflictRecordingSeriesIcon = "tvguide_recordserie_conflict_button.png";

    public static readonly string TvIsRecordingIcon = "tv_is_recording.png";
    public static readonly string TvIsTimeshiftingIcon = "tv_is_timeshifting.png";
    public static readonly string TvIsAvailableIcon = "tv_is_available.png";
    public static readonly string TvIsUnavailableIcon = "tv_is_unavailable.png";

    public static readonly string MusicFolder = Config.GetSubFolder(Config.Dir.Thumbs, @"Music\Folder");
    public static readonly string MusicAlbum = Config.GetSubFolder(Config.Dir.Thumbs, @"Music\Albums");
    public static readonly string MusicArtists = Config.GetSubFolder(Config.Dir.Thumbs, @"Music\Artists");
    public static readonly string MusicGenre = Config.GetSubFolder(Config.Dir.Thumbs, @"Music\Genre");

    public static readonly string MovieTitle = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos\Title");
    public static readonly string MovieActors = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos\Actors");
    public static readonly string MovieGenre = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos\Genre");

    public static readonly string TVChannel = Config.GetSubFolder(Config.Dir.Thumbs, @"TV\Logos");
    public static readonly string TVShows = Config.GetSubFolder(Config.Dir.Thumbs, @"TV\Shows");
    public static readonly string TVRecorded = Config.GetSubFolder(Config.Dir.Thumbs, @"TV\Recorded");

    public static readonly string Radio = Config.GetSubFolder(Config.Dir.Thumbs, @"Radio");
    public static readonly string Pictures = Config.GetSubFolder(Config.Dir.Thumbs, @"Pictures");
    public static readonly string Yac = Config.GetSubFolder(Config.Dir.Thumbs, @"yac");
    public static readonly string News = Config.GetSubFolder(Config.Dir.Thumbs, @"News");
    public static readonly string Trailers = Config.GetSubFolder(Config.Dir.Thumbs, @"Trailers");
    public static readonly string Videos = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos");

    private static ThumbQuality _currentThumbQuality = ThumbQuality.average;
    private static CompositingQuality _currentCompositingQuality = CompositingQuality.Default;
    private static InterpolationMode _currentInterpolationMode = InterpolationMode.Default;
    private static SmoothingMode _currentSmoothingMode = SmoothingMode.Default;

    private static LargeThumbSize _currentLargeThumbSize = LargeThumbSize.average;
    private static ThumbSize _currentThumbSize = ThumbSize.average;
    private static readonly ImageFormat _currentThumbFormat = ImageFormat.Jpeg;

    private static ImageCodecInfo _currentImageCodecInfo;
    private static EncoderParameters _currentEncoderParams;

    static Thumbs()
    {
      LoadSettings();
    }

    private static void LoadSettings()
    {
      try
      {
        using (Profile.Settings xmlreader = new Profile.MPSettings())
        {
          int configQuality = xmlreader.GetValueAsInt("thumbnails", "quality", 2);
          switch (configQuality)
          {
            case 0:
              Quality = ThumbQuality.fastest;
              SetEncoderParams(25);
              Log.Warn("Thumbs: MediaPortal is using the fastest thumbnail mode");
              break;
            case 1:
              Quality = ThumbQuality.fast;
              SetEncoderParams(33);
              Log.Info("Thumbs: MediaPortal is using fast thumbnails");
              break;
            case 2:
              Quality = ThumbQuality.average;
              SetEncoderParams(50);
              Log.Info("Thumbs: MediaPortal is using average thumbnails");
              break;
            case 3:
              Quality = ThumbQuality.higher;
              SetEncoderParams(77);
              Log.Info("Thumbs: MediaPortal is using high quality thumbnails");
              break;
            case 4:
              Quality = ThumbQuality.highest;
              SetEncoderParams(97);
              Log.Warn("Thumbs: MediaPortal is using highest quality thumbnail mode");
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Thumbs: Error loading thumbnail quality - {0}", ex.Message);
      }
    }

    public static void CreateFolders()
    {
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Thumbs));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "Music"));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "Videos"));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "TV"));
        Directory.CreateDirectory(MusicFolder);
        Directory.CreateDirectory(MusicAlbum);
        Directory.CreateDirectory(MusicArtists);
        Directory.CreateDirectory(MusicGenre);
        Directory.CreateDirectory(Pictures);
        Directory.CreateDirectory(Radio);
        Directory.CreateDirectory(TVChannel);
        Directory.CreateDirectory(TVShows);
        Directory.CreateDirectory(TVRecorded);
        Directory.CreateDirectory(MovieGenre);
        Directory.CreateDirectory(MovieTitle);
        Directory.CreateDirectory(MovieActors);
        Directory.CreateDirectory(Yac);
        Directory.CreateDirectory(News);
        Directory.CreateDirectory(Trailers);
      }
      catch (Exception) {}
    }

    #region Public getters and setters

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
        string[] possibleExtensions = ImgEncoders[i].FilenameExtension.Split(new[] {';'},
                                                                             StringSplitOptions.RemoveEmptyEntries);
        foreach (string ext in possibleExtensions)
        {
          // .jpg in *.JPG ?
          if (ext.ToUpper().Contains(Utils.GetThumbExtension().ToUpper()))
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