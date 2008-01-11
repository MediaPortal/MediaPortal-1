#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using MediaPortal.Configuration;  // for config provider
using MediaPortal.GUI.Library; // for logging

namespace MediaPortal.Util
{
  /// <summary>
  /// Summary description for Thumbs.
  /// </summary>
  public class Thumbs
  {
    public enum ThumbQuality : int
    {
      fastest = 0,
      fast = 1,
      average = 2,
      higher = 3,
      highest = 4,
    }

    public enum LargeThumbSize : int
    {
      small = 400,
      average = 500,
      large = 600,
    }

    public enum ThumbSize : int
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

    public static ThumbQuality _currentThumbQuality = ThumbQuality.average;
    public static CompositingQuality _currentCompositingQuality = CompositingQuality.Default;
    public static InterpolationMode _currentInterpolationMode = InterpolationMode.Default;
    public static SmoothingMode _currentSmoothingMode = SmoothingMode.Default;

    public static LargeThumbSize _currentLargeThumbSize = LargeThumbSize.average;
    public static ThumbSize _currentThumbSize = ThumbSize.average;
    public static ImageFormat _currentThumbFormat = ImageFormat.Jpeg;

    static Thumbs()
    {
      LoadSettings();
    }

    private static void LoadSettings()
    {
      try
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          int configQuality = xmlreader.GetValueAsInt("thumbnails", "quality", 1);

          switch (configQuality)
          {
            case 0:
              Quality = ThumbQuality.fastest;
              Log.Warn("Thumbs: MediaPortal is using the fastest thumbnail mode");
              break;
            case 1:
              Quality = ThumbQuality.fast;
              Log.Info("Thumbs: MediaPortal is using fast thumbnails");
              break;
            case 2:
              Quality = ThumbQuality.average;
              Log.Info("Thumbs: MediaPortal is using average thumbnails");
              break;
            case 3:
              Quality = ThumbQuality.higher;
              Log.Info("Thumbs: MediaPortal is using high quality thumbnails");
              break;
            case 4:
              Quality = ThumbQuality.highest;
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
        System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Thumbs));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "Music"));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "Videos"));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "TV"));
        System.IO.Directory.CreateDirectory(MusicFolder);
        System.IO.Directory.CreateDirectory(MusicAlbum);
        System.IO.Directory.CreateDirectory(MusicArtists);
        System.IO.Directory.CreateDirectory(MusicGenre);
        System.IO.Directory.CreateDirectory(Pictures);
        System.IO.Directory.CreateDirectory(Radio);
        System.IO.Directory.CreateDirectory(TVChannel);
        System.IO.Directory.CreateDirectory(TVShows);
        System.IO.Directory.CreateDirectory(TVRecorded);
        System.IO.Directory.CreateDirectory(MovieGenre);
        System.IO.Directory.CreateDirectory(MovieTitle);
        System.IO.Directory.CreateDirectory(MovieActors);
        System.IO.Directory.CreateDirectory(Yac);
        System.IO.Directory.CreateDirectory(News);
        System.IO.Directory.CreateDirectory(Trailers);
      }
      catch (Exception) { }
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
            return false;
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
      get
      {
        return _currentThumbQuality;
      }
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
      get
      {
        return _currentCompositingQuality;
      }
    }

    public static InterpolationMode Interpolation
    {
      get
      {
        return _currentInterpolationMode;
      }
    }

    public static SmoothingMode Smoothing
    {
      get
      {
        return _currentSmoothingMode;
      }
    }

    public static ThumbSize ThumbResolution
    {
      get
      {
        return _currentThumbSize;
      }
    }

    public static LargeThumbSize ThumbLargeResolution
    {
      get
      {
        return _currentLargeThumbSize;        
      }
    }

    public static ImageFormat ThumbFormat
    {
      get
      {
        return _currentThumbFormat;
      }
    }
    #endregion

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