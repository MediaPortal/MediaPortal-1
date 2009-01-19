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
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MediaPortal.Configuration;
using MediaPortal.ServiceImplementations;
using MediaPortal.Profile;


namespace MediaPortal.Util
{
  public class VideoThumbCreator
  {
    static string ExtractApp = "mtn.exe";
    static string ExtractorPath = Config.GetFile(Config.Dir.Base, "MovieThumbnailer", ExtractApp);
    static int PreviewColumns = 2;
    static int PreviewRows = 2;
    static bool LeaveShareThumb = false;
    static bool NeedsConfigRefresh = true;

    #region Serialisation
    
    private static void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        PreviewColumns = xmlreader.GetValueAsInt("thumbnails", "tvthumbcols", 2);
        PreviewRows = xmlreader.GetValueAsInt("thumbnails", "tvthumbrows", 2);
        LeaveShareThumb = xmlreader.GetValueAsBool("thumbnails", "tvrecordedsharepreview", false);
        Log.Debug("VideoThumbCreator: Settings loaded - using {0} columns and {1} rows. Share thumb = {2}", PreviewColumns, PreviewRows, LeaveShareThumb);
        NeedsConfigRefresh = false;
      }
    }

    #endregion

    #region Public methods

    //[MethodImpl(MethodImplOptions.Synchronized)]
    //public static bool CreateVideoThumb(string aVideoPath, bool aOmitCredits)
    //{
    //  string sharethumb = Path.ChangeExtension(aVideoPath, ".jpg");
    //  if (File.Exists(sharethumb))
    //    return true;
    //  else
    //    return CreateVideoThumb(aVideoPath, sharethumb, false, aOmitCredits);
    //}

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool CreateVideoThumb(string aVideoPath, string aThumbPath, bool aCacheThumb, bool aOmitCredits)
    {
      if (NeedsConfigRefresh)
        LoadSettings();
      if (String.IsNullOrEmpty(aVideoPath) || String.IsNullOrEmpty(aThumbPath))
      {
        Log.Warn("VideoThumbCreator: Invalid arguments to generate thumbnails of your video!");
        return false;
      }
      if (!File.Exists(ExtractorPath))
      {
        Log.Warn("VideoThumbCreator: No {0} found to generate thumbnails of your video!", ExtractApp);
        return false;
      }
      if (!LeaveShareThumb && !aCacheThumb)
      {
        Log.Warn("VideoThumbCreator: No share thumbs wanted by config option AND no caching wanted - where should the thumb go then? Aborting..");
        return false;
      }

      // Params for ffmpeg
      // string ExtractorArgs = string.Format(" -i \"{0}\" -vframes 1 -ss {1} -s {2}x{3} \"{4}\"", aVideoPath, @"00:08:21", (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, aThumbPath);

      // Params for mplayer (outputs 00000001.jpg in video resolution into working dir) -vf scale=600:-3
      //string ExtractorArgs = string.Format(" -noconsolecontrols -nosound -vo jpeg:quality=90 -vf scale -frames 1 -ss {0} \"{1}\"", "501", aVideoPath);

      // Params for mtm (http://moviethumbnail.sourceforge.net/usage.en.html)
      //   -D 8         : edge detection; 0:off >0:on; higher detects more; try -D4 -D6 or -D8
      //   -B 420/E 600 : omit this seconds from the beginning / ending TODO: use pre- / postrecording values
      //   -c 2  /r 2   : # of column / # of rows
      //   -s 300       : time step between each shot
      //   -t           : time stamp off
      //   -i           : info text off
      //   -w 0         : width of output image; 0:column * movie width
      //   -n           : run at normal priority
      //   -W           : dont overwrite existing files, i.e. update mode
      //   -P           : dont pause before exiting; override -p

      int preGapSec = 0;
      int postGapSec = 0;
      if (aOmitCredits)
      {
        preGapSec = 420;
        postGapSec = 600;
      }
      bool Success = false;
      string ExtractorArgs = string.Format(" -D 6 -B {0} -E {1} -c {2} -r {3} -s {4} -t -i -w {5} -n -P \"{6}\"", preGapSec, postGapSec, PreviewColumns, PreviewRows, "300", /*(int)Thumbs.ThumbLargeResolution*/ "0", aVideoPath);
      string ExtractorFallbackArgs = string.Format(" -D 8 -B {0} -E {1} -c {2} -r {3} -s {4} -t -i -w {5} -n -P \"{6}\"", 0, 0, PreviewColumns, PreviewRows, "60", /*(int)Thumbs.ThumbLargeResolution*/ "0", aVideoPath);
      // Honour we are using a unix app
      ExtractorArgs = ExtractorArgs.Replace('\\', '/');
      try
      {
        // Use this for the working dir to be on the safe side
        string TempPath = Path.GetTempPath();
        string OutputThumb = string.Format("{0}_s{1}", Path.ChangeExtension(aVideoPath, null), ".jpg");
        string ShareThumb = OutputThumb.Replace("_s.jpg", ".jpg");

        if ((LeaveShareThumb && !File.Exists(ShareThumb))    // No thumb in share although it should be there
        || (!LeaveShareThumb && !File.Exists(aThumbPath))) // No thumb cached and no chance to find it in share
        {
          //Log.Debug("VideoThumbCreator: No thumb in share {0} - trying to create one with arguments: {1}", ShareThumb, ExtractorArgs);
          Success = Utils.StartProcess(ExtractorPath, ExtractorArgs, TempPath, 15000, true, GetMtnConditions());
          if (!Success)
          {
            // Maybe the pre-gap was too large or not enough sharp & light scenes could be caught
            Thread.Sleep(100);
            Success = Utils.StartProcess(ExtractorPath, ExtractorFallbackArgs, TempPath, 30000, true, GetMtnConditions());
            if (!Success)
              Log.Info("VideoThumbCreator: {0} has not been executed successfully with arguments: {1}", ExtractApp, ExtractorFallbackArgs);
          }
          // give the system a few IO cycles
          Thread.Sleep(100);
          // make sure there's no process hanging
          Utils.KillProcess(Path.ChangeExtension(ExtractApp, null));
          try
          {
            // remove the _s which mdn appends to its files
            File.Move(OutputThumb, ShareThumb);
          }
          catch (FileNotFoundException)
          {
            Log.Debug("VideoThumbCreator: {0} did not extract a thumbnail to: {1}", ExtractApp, OutputThumb);
          }
          catch (Exception)
          {
            try
            {
              // Clean up
              File.Delete(OutputThumb);
              Thread.Sleep(50);
            }
            catch (Exception) { }
          }
        }
        Thread.Sleep(30);

        if (aCacheThumb && Success)
        {
          if (Picture.CreateThumbnail(ShareThumb, aThumbPath, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, false))
            Picture.CreateThumbnail(ShareThumb, Utils.ConvertToLargeCoverArt(aThumbPath), (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false);
        }

        if (!LeaveShareThumb)
        {
          try
          {
            File.Delete(ShareThumb);
            Thread.Sleep(30);
          }
          catch (Exception) { }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideoThumbCreator: Thumbnail generation failed - {0}!", ex.ToString());
      }
      return File.Exists(aThumbPath);
    }

    #endregion

    #region Private methods

    private static Utils.ProcessFailedConditions GetMtnConditions()
    {
      Utils.ProcessFailedConditions mtnStat = new Utils.ProcessFailedConditions();
      mtnStat.AddCriticalOutString("net duration after -B & -E is negative");
      mtnStat.AddCriticalOutString("all rows're skipped?");
      mtnStat.AddCriticalOutString("step is zero; movie is too short?");
      mtnStat.AddCriticalOutString("failed: -");
      mtnStat.AddCriticalOutString("couldn't find a decoder for codec_id");

      mtnStat.SuccessExitCode = 0;

      return mtnStat;
    }

    #endregion
  }
}