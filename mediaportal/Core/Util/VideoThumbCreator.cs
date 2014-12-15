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

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MediaPortal.Configuration;
using MediaPortal.ServiceImplementations;
using MediaPortal.Profile;
using MediaPortal.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MediaPortal.Util
{
  public class VideoThumbCreator
  {
    private static string ExtractApp = "ffmpeg.exe";
    private static string ExtractorPath = Config.GetFile(Config.Dir.Base, "MovieThumbnailer", ExtractApp);
    private static int PreviewColumns = 2;
    private static int PreviewRows = 2;
    private static int preRecordInterval = 1;
    private static bool LeaveShareThumb = false;
    private static MediaPortal.Player.MediaInfoWrapper MediaInfo = null;
    private static int TimeBetweenThumbs = 60;

    #region Serialisation

    private static void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        PreviewColumns = xmlreader.GetValueAsInt("thumbnails", "videothumbcols", 1);
        PreviewRows = xmlreader.GetValueAsInt("thumbnails", "videothumbrows", 1);
        LeaveShareThumb = xmlreader.GetValueAsBool("thumbnails", "videosharepreview", false);
        preRecordInterval = xmlreader.GetValueAsInt("thumbnails", "preRecordInterval", 1);
        Log.Debug("VideoThumbCreator: Settings loaded - using {0} columns and {1} rows. Share thumb = {2}, preRecordInterval = {3}.",
                  PreviewColumns, PreviewRows, LeaveShareThumb, preRecordInterval);
      }
    }

    #endregion

    #region Public methods

    //[MethodImpl(MethodImplOptions.Synchronized)]
    //public static bool CreateVideoThumb(string aVideoPath, bool aOmitCredits)
    //{
    //  string sharethumb = Path.ChangeExtension(aVideoPath, ".jpg");
    //  if (Util.Utils.FileExistsInCache(sharethumb))
    //    return true;
    //  else
    //    return CreateVideoThumb(aVideoPath, sharethumb, false, aOmitCredits);
    //}

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool CreateVideoThumb(string aVideoPath, string aThumbPath, bool aCacheThumb, bool aOmitCredits)
    {
      //Log.Debug("VideoThumbCreator: args {0}, {1}!", aVideoPath, aThumbPath);

        LoadSettings();

      if (String.IsNullOrEmpty(aVideoPath) || String.IsNullOrEmpty(aThumbPath))
      {
        Log.Warn("VideoThumbCreator: Invalid arguments to generate thumbnails of your video!");
        return false;
      }
      if (!Util.Utils.FileExistsInCache(aVideoPath))
      {
        Log.Warn("VideoThumbCreator: File {0} not found!", aVideoPath);
        return false;
      }
      if (!Util.Utils.FileExistsInCache(ExtractorPath))
      {
        Log.Warn("VideoThumbCreator: No {0} found to generate thumbnails of your video!", ExtractApp);
        return false;
      }
      if (!LeaveShareThumb && !aCacheThumb)
      {
        Log.Warn(
          "VideoThumbCreator: No share thumbs wanted by config option AND no caching wanted - where should the thumb go then? Aborting..");
        return false;
      }

      IVideoThumbBlacklist blacklist = GlobalServiceProvider.Get<IVideoThumbBlacklist>();
      if (blacklist != null && blacklist.Contains(aVideoPath))
      {
        Log.Debug("Skipped creating thumbnail for {0}, it has been blacklisted because last attempt failed", aVideoPath);
        return false;
      }

      // Params for ffmpeg
      // string ExtractorArgs = string.Format(" -i \"{0}\" -vframes 1 -ss {1} -s {2}x{3} \"{4}\"", aVideoPath, @"00:08:21", (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, aThumbPath);

      // Params for mplayer (outputs 00000001.jpg in video resolution into working dir) -vf scale=600:-3
      //string ExtractorArgs = string.Format(" -noconsolecontrols -nosound -vo jpeg:quality=90 -vf scale -frames 1 -ss {0} \"{1}\"", "501", aVideoPath);

      // Params for mtm (http://moviethumbnail.sourceforge.net/usage.en.html)
      //   -D 8         : edge detection; 0:off >0:on; higher detects more; try -D4 -D6 or -D8
      //   -B 420/E 600 : omit this seconds from the beginning / ending TODO: use pre- / postrecording values
      //   -c 2 / r 2   : # of column / # of rows
      //   -b 0.60      : skip if % blank is higher; 0:skip all 1:skip really blank >1:off
      //   -h 100       : minimum height of each shot; will reduce # of column to fit
      //   -t           : time stamp off
      //   -i           : info text off
      //   -w 0         : width of output image; 0:column * movie width
      //   -n           : run at normal priority
      //   -W           : dont overwrite existing files, i.e. update mode
      //   -P           : dont pause before exiting; override -p

      const double flblank = 0.6;
      string blank = flblank.ToString("F", CultureInfo.CurrentCulture);

      int preGapSec = preRecordInterval*60;

      int TimeToSeek = 3;

      int Duration = 0;

      // intRnd is used if user refresh the thumbnail from context menu
      int intRnd = 0;
      if (aOmitCredits)
      {
        Random rnd = new Random();
        intRnd = rnd.Next(10, 300);
      }

      preGapSec = preGapSec+intRnd;
      Log.Debug("VideoThumbCreator: random value: {0}", intRnd);
      bool Success = false;

      MediaInfo = new MediaPortal.Player.MediaInfoWrapper(aVideoPath);

      if (MediaInfo != null)
      {
        Duration = MediaInfo.VideoDuration/1000;
      }

      if (Duration == 0)
      {
        Log.Debug("VideoThumbCreator: the {0} is corrupt.", aVideoPath);
        return false;
      }

      if (preGapSec > Duration)
      {
        preGapSec = ( Duration / 100 ) * 20; // 20% of the duration
      }

      TimeBetweenThumbs = (Duration - preGapSec) / ((PreviewColumns * PreviewRows) + 1);

      Log.Debug("{0} duration is {1}.", aVideoPath, Duration);
      
      // Honour we are using a unix app
      //ExtractorArgs = ExtractorArgs.Replace('\\', '/');
      try
      {
        // Set String
        string strFilenamewithoutExtension = Path.ChangeExtension(aVideoPath, null);
        // Use this for the working dir to be on the safe side
        string TempPath = Path.GetTempPath();
        string OutputThumb = string.Format("{0}{1}", Path.ChangeExtension(aVideoPath, null), ".jpg");
        string ShareThumb = OutputThumb.Replace(".jpg", ".jpg");
        // Use Temp folder 
        string strFilenamewithoutExtensionTemp = Path.GetTempPath() + Path.GetFileName(strFilenamewithoutExtension);
        string ShareThumbTemp = Path.GetTempPath() + Path.GetFileName(ShareThumb);
        // ffmpeg
        string ffmpegFallbackArgs = string.Format("yadif=0:-1:0,scale=600:337,setsar=1:1,tile={0}x{1}", PreviewColumns, PreviewRows);
        string ExtractorFallbackArgs = string.Format("-loglevel quiet -ss {0} -i \"{1}\" -y -ss {2} -vf {3} -vframes 1 -vsync 0 -an \"{4}.jpg\"", 5, aVideoPath, TimeToSeek, ffmpegFallbackArgs, strFilenamewithoutExtensionTemp);
        
        if ((LeaveShareThumb && !Util.Utils.FileExistsInCache(ShareThumb))
            // No thumb in share although it should be there 
            || (aOmitCredits)
            // or a refress needs by user (from context menu)
            || (!LeaveShareThumb && !Util.Utils.FileExistsInCache(aThumbPath)))
          // No thumb cached and no chance to find it in share
        {
          Log.Debug("VideoThumbCreator: No thumb in share {0} - trying to create.", aVideoPath);
          if (aOmitCredits)
          {
            File.Delete(ShareThumb);
          }

          List<string> pictureList = new List<string>();
          string ffmpegArgs = null;
          string ExtractorArgs = null;
          int TimeOffset = 0;
          int i;

          for (i = 0; i < (PreviewColumns * PreviewRows); i++)
          {
            TimeOffset = preGapSec + i * TimeBetweenThumbs;

            ffmpegArgs = string.Format("yadif=0:-1:0,scale=600:337,setsar=1:1,tile=1x1");
            ExtractorArgs = string.Format("-loglevel quiet -ss {0} -i \"{1}\" -y -ss {2} -vf {3} -vframes 1 -vsync 0 -an \"{4}_{5}.jpg\"", TimeOffset, aVideoPath, TimeToSeek, ffmpegArgs, strFilenamewithoutExtensionTemp, i);
            Success = Utils.StartProcess(ExtractorPath, ExtractorArgs, TempPath, 120000, true, GetMtnConditions());
            Log.Debug("VideoThumbCreator: thumb creation {0}", ExtractorArgs);
            if (!Success)
            {
              Log.Debug("VideoThumbCreator: failed, try to fallback {0}", strFilenamewithoutExtensionTemp);
              break;
            }
            else
            {
              pictureList.Add(string.Format("{0}_{1}.jpg", strFilenamewithoutExtensionTemp, i));
            }
          }
          // generate thumb if all sub pictures was created
          if (i == PreviewColumns * PreviewRows)
          {
            if (Util.Utils.CreateTileThumb(pictureList, string.Format("{0}.jpg", strFilenamewithoutExtensionTemp), PreviewColumns, PreviewRows))
            {
              Log.Debug("VideoThumbCreator: thumb creation success {0}", ShareThumbTemp);
              File.SetAttributes(ShareThumbTemp, File.GetAttributes(ShareThumbTemp) & ~FileAttributes.Hidden);
            }
            else
            {
              Log.Debug("VideoThumbCreator: failed, try to fallback {0}", strFilenamewithoutExtensionTemp);
            }

          }
          else
          {
            // Maybe the pre-gap was too large or not enough sharp & light scenes could be caught
            Thread.Sleep(100);
            Success = Utils.StartProcess(ExtractorPath, ExtractorFallbackArgs, TempPath, 120000, true, GetMtnConditions());
            if (!Success)
            {
              Log.Info("VideoThumbCreator: {0} has not been executed successfully with arguments: {1}", ExtractApp, ExtractorFallbackArgs);
                /*Utils.KillProcess(Path.ChangeExtension(ExtractApp, null));
                return false;*/
            }
          }
          // give the system a few IO cycles
          Thread.Sleep(100);
          // make sure there's no process hanging
          Utils.KillProcess(Path.ChangeExtension(ExtractApp, null));
        }
        else
        {
          // We have a thumbnail in share but the cache was wiped out - make sure it is recreated
          if (LeaveShareThumb && Util.Utils.FileExistsInCache(ShareThumb) && !Util.Utils.FileExistsInCache(aThumbPath))
            Success = true;
        }

        Thread.Sleep(30);

        if (aCacheThumb && Success)
        {
          if (Picture.CreateThumbnailVideo(ShareThumbTemp, aThumbPath, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution,
                                      0, false))
          {
            Picture.CreateThumbnailVideo(ShareThumbTemp, Utils.ConvertToLargeCoverArt(aThumbPath),
                                    (int) Thumbs.ThumbLargeResolution, (int) Thumbs.ThumbLargeResolution, 0, false);
          }
        }

        if (LeaveShareThumb)
        {
          if (Utils.FileExistsInCache(aThumbPath))
          {
            try
            {
              string aThumbPathLarge = Utils.ConvertToLargeCoverArt(aThumbPath);
              if (Utils.FileExistsInCache(aThumbPathLarge))
              {
                aThumbPath = aThumbPathLarge;
              }
              File.Copy(aThumbPath, ShareThumb);
              File.SetAttributes(ShareThumb, File.GetAttributes(ShareThumb) & ~FileAttributes.Hidden);
            }
            catch (Exception)
            {
              Log.Debug("TvThumbnails.VideoThumbCreator: Exception on File.Copy({0}, {1})", ShareThumbTemp, ShareThumb);
            }
          }
        }

        // delete final thumb from temp folder
        try
        {
          if (Utils.FileExistsInCache(ShareThumbTemp))
          {
            File.Delete(ShareThumbTemp);
          }
        }
        catch (FileNotFoundException)
        {
          // No need to log
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideoThumbCreator: Thumbnail generation failed - {0}!", ex.ToString());
      }
      if (Util.Utils.FileExistsInCache(aThumbPath))
      {
        return true;
      }
      else
      {
        if (blacklist != null)
        {
          blacklist.Add(aVideoPath);
        }
        return false;
      }
    }

    public static string GetThumbExtractorVersion()
    {
      try
      {
        //System.Diagnostics.FileVersionInfo newVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(ExtractorPath);
        //return newVersion.FileVersion;
        // mtn.exe has no version info, so let's use "time modified" instead
        FileInfo fi = new FileInfo(ExtractorPath);
        return fi.LastWriteTimeUtc.ToString("s"); // use culture invariant format
      }
      catch (Exception ex)
      {
        Log.Error("GetThumbExtractorVersion failed:");
        Log.Error(ex);
        return "";
      }
    }

    #endregion

    #region Private methods

    private static Utils.ProcessFailedConditions GetMtnConditions()
    {
      Utils.ProcessFailedConditions mtnStat = new Utils.ProcessFailedConditions();
      // The input file is shorter than pre- and post-recording time
      mtnStat.AddCriticalOutString("net duration after -B & -E is negative");
      mtnStat.AddCriticalOutString("all rows're skipped?");
      mtnStat.AddCriticalOutString("step is zero; movie is too short?");
      mtnStat.AddCriticalOutString("failed: -");
      // unsupported video format by mtn.exe - maybe there's an update?
      mtnStat.AddCriticalOutString("couldn't find a decoder for codec_id");

      mtnStat.SuccessExitCode = 0;

      return mtnStat;
    }

    #endregion
  }
}