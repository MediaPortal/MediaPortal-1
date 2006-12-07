/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using MediaPortal.Utils;       // for config provider
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
      small = 300,
      average = 512,
      large = 768,
    }

    public enum ThumbSize : int
    {
      small = 96,
      average = 128,
      large = 192,
    }

    static public readonly string TvNotifyIcon = "tvguide_notify_button.png";
    static public readonly string TvRecordingIcon = "tvguide_record_button.png";
    static public readonly string TvRecordingSeriesIcon = "tvguide_recordserie_button.png";
    static public readonly string TvConflictRecordingIcon = "tvguide_recordconflict_button.png";

    static public readonly string MusicFolder = Config.GetSubFolder(Config.Dir.Thumbs, @"music\folder");
    static public readonly string MusicAlbum = Config.GetSubFolder(Config.Dir.Thumbs, @"music\albums");
    static public readonly string MusicArtists = Config.GetSubFolder(Config.Dir.Thumbs, @"music\artists");
    static public readonly string MusicGenre = Config.GetSubFolder(Config.Dir.Thumbs, @"music\genre");

    static public readonly string MovieTitle = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos\Title");
    static public readonly string MovieActors = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos\Actors");
    static public readonly string MovieGenre = Config.GetSubFolder(Config.Dir.Thumbs, @"Videos\genre");

    static public readonly string TVChannel = Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos");
    static public readonly string TVShows = Config.GetSubFolder(Config.Dir.Thumbs, @"tv\shows");

    static public readonly string Radio = Config.GetSubFolder(Config.Dir.Thumbs, @"Radio");
    static public readonly string Pictures = Config.GetSubFolder(Config.Dir.Thumbs, @"Pictures");
    static public readonly string Yac = Config.GetSubFolder(Config.Dir.Thumbs, @"yac");

    static public ThumbQuality _currentThumbQuality = ThumbQuality.average;
    static public CompositingQuality _currentCompositingQuality = CompositingQuality.Default;
    static public InterpolationMode _currentInterpolationMode = InterpolationMode.Default;
    static public SmoothingMode _currentSmoothingMode = SmoothingMode.Default;

    static public LargeThumbSize _currentLargeThumbSize = LargeThumbSize.average;
    static public ThumbSize _currentThumbSize = ThumbSize.average;
    static public ImageFormat _currentThumbFormat = ImageFormat.Jpeg;

    static Thumbs()
    {
      LoadSettings();
    }

    static private void LoadSettings()
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

    static public void CreateFolders()
    {
      try
      {
        System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Thumbs));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "music"));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "videos"));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Thumbs, "tv"));
        System.IO.Directory.CreateDirectory(MusicFolder);
        System.IO.Directory.CreateDirectory(MusicAlbum);
        System.IO.Directory.CreateDirectory(MusicArtists);
        System.IO.Directory.CreateDirectory(MusicGenre);
        System.IO.Directory.CreateDirectory(Pictures);
        System.IO.Directory.CreateDirectory(Radio);
        System.IO.Directory.CreateDirectory(TVChannel);
        System.IO.Directory.CreateDirectory(TVShows);
        System.IO.Directory.CreateDirectory(MovieGenre);
        System.IO.Directory.CreateDirectory(MovieTitle);
        System.IO.Directory.CreateDirectory(MovieActors);
        System.IO.Directory.CreateDirectory(Yac);
      }
      catch (Exception) { }
    }

    #region Public getters and setters
    /// <summary>
    /// Change thumbnail quality
    /// </summary>
    static public ThumbQuality Quality
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

    static public CompositingQuality Compositing
    {
      get
      {
        return _currentCompositingQuality;
      }
    }

    static public InterpolationMode Interpolation
    {
      get
      {
        return _currentInterpolationMode;
      }
    }

    static public SmoothingMode Smoothing
    {
      get
      {
        return _currentSmoothingMode;
      }
    }

    static public ThumbSize ThumbResolution
    {
      get
      {
        return _currentThumbSize;
      }
    }

    static public LargeThumbSize ThumbLargeResolution
    {
      get
      {
        return _currentLargeThumbSize;        
      }
    }

    static public ImageFormat ThumbFormat
    {
      get
      {
        return _currentThumbFormat;
      }
    }
    #endregion

    static private void SetQualityParams(ThumbQuality quality_)
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