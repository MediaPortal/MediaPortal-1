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
using MediaPortal.Configuration;
using MediaPortal.ServiceImplementations;
using System.Threading;

namespace MediaPortal.Util
{
  public class VideoThumbCreator
  {
    static string ExtractApp = "mtn.exe";
    static string ExtractorPath = Config.GetFile(Config.Dir.Base, "MovieThumbnailer", ExtractApp);

    #region Public methods

    public static bool CreateVideoThumb(string aVideoPath)
    {
      return CreateVideoThumb(aVideoPath, Path.ChangeExtension(aVideoPath, ".jpg"), false);
    }

    public static bool CreateVideoThumb(string aVideoPath, string aThumbPath, bool aCacheThumb)
    {
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
      string ExtractorArgs = string.Format(" -D 6 -B 420 -E 600 -c 2 -r 2 -s {0} -t -i -w {1} -n -P \"{2}\"", "300", /*(int)Thumbs.ThumbLargeResolution*/ "720", aVideoPath);
      // Honour we are using a unix app
      ExtractorArgs = ExtractorArgs.Replace('\\', '/');
      try
      {
        // Use this for the working dir to be on the safe side
        string TempPath = Path.GetTempPath();
        string OutputThumb = string.Format("{0}_s{1}", Path.ChangeExtension(aVideoPath, null), ".jpg");
        string ShareThumb = OutputThumb.Replace("_s.jpg", ".jpg");

        Utils.StartProcess(ExtractorPath, ExtractorArgs, TempPath, 60000, true);
        // give the system a few IO cycles
        Thread.Sleep(100);
        // make sure there's no process hanging
        Utils.KillProcess(Path.ChangeExtension(ExtractApp, null));
        Thread.Sleep(0);
        try
        {
          // remove the _s which mdn appends to its files            
          File.Move(OutputThumb, ShareThumb);
          // If no file was created the move will throw an exception and no cache attempt will happen
          if (aCacheThumb)
          {
            if (Picture.CreateThumbnail(ShareThumb, aThumbPath, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, false))
              Picture.CreateThumbnail(ShareThumb, Utils.ConvertToLargeCoverArt(aThumbPath), (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false);
          }
        }
        catch (Exception) { }
        try
        {
          // Clean up
          File.Delete(OutputThumb);
          Thread.Sleep(100);
        }
        catch (Exception) { }

      }
      catch (Exception ex)
      {
        Log.Error("VideoThumbCreator: Thumbnail generation failed - {0}!", ex.Message);
      }
      return File.Exists(aThumbPath);
    }

    #endregion
  }
}