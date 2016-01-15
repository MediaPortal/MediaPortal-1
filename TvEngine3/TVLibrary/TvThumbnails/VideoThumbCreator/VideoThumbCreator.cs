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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using TvLibrary.Log;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace TvThumbnails.VideoThumbCreator
{
  public static class VideoThumbCreator
  {
    private static string _extractApp = "ffmpeg.exe";

    private static MediaInfoWrapper.MediaInfoWrapper MediaInfo = null;

    private static readonly string _extractorPath = ExtractorPath();

    public static string ExtractorPath()
    {
      string currentPath = Assembly.GetCallingAssembly().Location;

      FileInfo currentPathInfo = new FileInfo(currentPath);

      return currentPathInfo.DirectoryName + "\\" + _extractApp;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool CreateVideoThumb(string aVideoPath, string aThumbPath, bool aOmitCredits)
    {
      if (!File.Exists(aVideoPath))
      {
        Log.Info("TvThumbnails.VideoThumbCreator: File {0} not found!", aVideoPath);
        return false;
      }

      if (!File.Exists(_extractorPath))
      {
        Log.Info("TvThumbnails.VideoThumbCreator: No {0} found to generate thumbnails of your video!", _extractorPath);
        return false;
      }

      //TODO Blacklist stuff
      /*bool isCachedThumbBlacklisted = IsCachedThumbBlacklisted(aVideoPath);
      IVideoThumbBlacklist blacklist = GlobalServiceProvider.Get<IVideoThumbBlacklist>();
      if (blacklist != null && blacklist.Contains(aVideoPath))
        if (isCachedThumbBlacklisted)
        {
          Log.Info("Skipped creating thumbnail for {0}, it has been blacklisted because last attempt failed", aVideoPath);
          return false;
        }*/

      string ShareThumb = "";

      int TimeIntBwThumbs = 0;

      int TimeToSeek = 3;

      int preGapSec = Thumbs.TimeOffset * 60;

      Log.Debug("TvThumbnails.VideoThumbCreator: preGapSec: {0}", preGapSec);

      MediaInfo = new MediaInfoWrapper.MediaInfoWrapper(aVideoPath);

      int Duration = MediaInfo.VideoDuration / 1000;

      if (Duration == 0)
      {
        Log.Debug("TvThumbnails.VideoThumbCreator: the {0} is corrupt.", aVideoPath);
        return false;
      }

      if (preGapSec > Duration)
      {
        preGapSec = ( Duration / 100 ) * 20; // 20% of the duration
      }

      TimeIntBwThumbs = (Duration - preGapSec) / ((Thumbs.PreviewColumns * Thumbs.PreviewRows) + 1); ;

      Log.Debug("{0} duration is {1}, TimeIntBwThumbs is {2}", aVideoPath, Duration, TimeIntBwThumbs);

      bool Success = false;
      string strFilenamewithoutExtension = Path.ChangeExtension(aVideoPath, null);

      string ffmpegFallbackArgs =
        string.Format("yadif=0:-1:0,") +
        string.Format("scale={0}:{1},", 600, 337) +
        string.Format("setsar=1:1,") +
        string.Format("tile={0}x{1}", Thumbs.PreviewColumns, Thumbs.PreviewRows);

      string ExtractorFallbackArgs =
        string.Format("-loglevel quiet -ss 5 ") +
        string.Format("-i \"{0}\" ", aVideoPath) +
        string.Format("-y -vf {0} ", ffmpegFallbackArgs) +
        string.Format("-ss {0} ", TimeToSeek) +
        string.Format("-vframes 1 -vsync 0 ") +
        string.Format("-an \"{0}_s.jpg\"", strFilenamewithoutExtension);

      try
      {
        // Use this for the working dir to be on the safe side
        string TempPath = Path.GetTempPath();
        string OutputThumb = string.Format("{0}{1}", Path.ChangeExtension(aVideoPath, null), ".jpg");
        ShareThumb = OutputThumb.Replace(".jpg", ".jpg");
        // Use Temp folder 
        string strFilenamewithoutExtensionTemp = Path.GetTempPath() + Path.GetFileName(strFilenamewithoutExtension);
        string ShareThumbTemp = Path.GetTempPath() + Path.GetFileName(ShareThumb);

        if ((Thumbs.LeaveShareThumb && !File.Exists(ShareThumb)) // No thumb in share although it should be there
            || (!Thumbs.LeaveShareThumb && !File.Exists(aThumbPath))) // No thumb cached and no chance to find it in share
        {
          List<string> pictureList = new List<string>();
          string ffmpegArgs = null;
          string ExtractorArgs = null;
          int TimeOffset = 0;
          int i;

          for (i = 0; i < (Thumbs.PreviewColumns * Thumbs.PreviewRows); i++)
          {
            TimeOffset = preGapSec + i * TimeIntBwThumbs;

            ffmpegArgs = string.Format("yadif=0:-1:0,scale=600:337,setsar=1:1,tile={0}x{1}", 1, 1);
            ExtractorArgs = string.Format("-loglevel quiet -ss {0} -i \"{1}\" -y -ss {2} -vf {3} -vframes 1 -vsync 0 -an \"{4}_{5}.jpg\"", TimeOffset, aVideoPath, TimeToSeek, ffmpegArgs, strFilenamewithoutExtensionTemp, i);
            Success = StartProcess(_extractorPath, ExtractorArgs, TempPath, 120000, true, GetMtnConditions());
            Log.Debug("TvThumbnails.VideoThumbCreator: thumb creation {0}", ExtractorArgs);
            if (!Success)
            {
              Log.Debug("TvThumbnails.VideoThumbCreator: failed, try to fallback {0}", strFilenamewithoutExtensionTemp);
              break;
            }
            else
            {
              pictureList.Add(string.Format("{0}_{1}.jpg", strFilenamewithoutExtensionTemp, i));
            }
          }
          // generate thumb if all sub pictures was created
          if (i == Thumbs.PreviewColumns * Thumbs.PreviewRows)
          {
            if (CreateTileThumb(pictureList, string.Format("{0}.jpg", strFilenamewithoutExtensionTemp), Thumbs.PreviewColumns, Thumbs.PreviewRows))
            {
              Log.Debug("TvThumbnails.VideoThumbCreator: thumb creation success {0}", ShareThumbTemp);
              File.SetAttributes(ShareThumbTemp, File.GetAttributes(ShareThumbTemp) & ~FileAttributes.Hidden);
            }
            else
            {
              Log.Debug("TvThumbnails.VideoThumbCreator: failed, try to fallback {0}", strFilenamewithoutExtensionTemp);
            }
          }
          else
          {
            // Maybe the pre-gap was too large or not enough sharp & light scenes could be caught
            Log.Debug("TvThumbnails.VideoThumbCreator: 1st trying was not success, 2nd trying in progress with ExtractorFallbackArgs: {0}", ExtractorFallbackArgs);
            Thread.Sleep(500);
            Success = StartProcess(_extractorPath, ExtractorFallbackArgs, TempPath, 120000, true, GetMtnConditions());
            
           if (!Success)
              Log.Info("TvThumbnails.VideoThumbCreator: {0} has not been executed successfully with arguments: {1}", _extractApp,
                       ExtractorFallbackArgs);
          }
          // give the system a few IO cycles
          Thread.Sleep(500);
          // make sure there's no process hanging
          KillProcess(Path.ChangeExtension(_extractApp, null));

          try
          {
            // Move Final Thumbnail from Temp folder to Thumbs folder
            File.Move(ShareThumbTemp, aThumbPath);
            File.SetAttributes(aThumbPath, File.GetAttributes(aThumbPath) & ~FileAttributes.Hidden);
          }
          catch (FileNotFoundException)
          {
            Log.Info("TvThumbnails.VideoThumbCreator: {0} did not extract a thumbnail to: {1}", _extractApp, ShareThumbTemp);
          }
          catch (Exception)
          {
            try
            {
              // Clean up
              File.Delete(ShareThumb);
              Thread.Sleep(100);
            }
            catch (Exception)
            {
            }
          }

          if (Thumbs.LeaveShareThumb)
          {
            if (File.Exists(aThumbPath))
            {
              try
              {
                File.Copy(aThumbPath, ShareThumb);
                File.SetAttributes(ShareThumb, File.GetAttributes(ShareThumb) & ~FileAttributes.Hidden);
              }
              catch (Exception)
              {
                Log.Debug("TvThumbnails.VideoThumbCreator: Exception on File.Copy({0}, {1})", ShareThumbTemp, ShareThumb);
              }
            }
          }
        }
        else
        {
          // We have a thumbnail in share but the cache was wiped out - make sure it is recreated
          if (Thumbs.LeaveShareThumb && !File.Exists(aThumbPath)) // && !File.Exists(aThumbPath))
            Success = true;
        }

        Thread.Sleep(30);

        if (File.Exists(ShareThumbTemp))
        {
          if (Success)
          {
            int width = (int)Thumbs.ThumbLargeResolution;
            CreateThumbnail(ShareThumbTemp, aThumbPath, width, width, 0);
            //CreateThumbnail(ShareThumb, Utils.ConvertToLargeCoverArt(aThumbPath),
            //                        (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false);
          }

          if (Thumbs.LeaveShareThumb)
          {
            if (File.Exists(aThumbPath))
            {
              try
              {
                File.Copy(aThumbPath, ShareThumb);
                File.SetAttributes(ShareThumb, File.GetAttributes(ShareThumb) & ~FileAttributes.Hidden);
              }
              catch (Exception)
              {
                Log.Debug("TvThumbnails.VideoThumbCreator: Exception on File.Copy({0}, {1})", aThumbPath, ShareThumb);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvThumbnails.VideoThumbCreator: Thumbnail generation failed - {0}!", ex.ToString());
      }
      if (File.Exists(aThumbPath) || File.Exists(ShareThumb))
      {
        return true;
      }
      else
      {
        //TODO Blacklist stuff
        //if (blacklist != null)
        //{
        //  blacklist.Add(aVideoPath);
        //}
        //AddCachedThumbToBlacklist(aVideoPath);
        return false;
      }
    }
    

    /// <summary>
    /// Interface for video thumbnails blacklisting
    /// </summary>
    public interface IVideoThumbBlacklist
    {
      bool Add(string path);
      bool Remove(string path);
      bool Contains(string path);
      void Clear();
    }


    /// <summary>
    /// Creates a thumbnail of the specified image
    /// </summary>
    /// <param name="aDrawingImage">The source System.Drawing.Image</param>
    /// <param name="aThumbTargetPath">Filename of the thumbnail to create</param>
    /// <param name="aThumbWidth">Maximum width of the thumbnail</param>
    /// <param name="aThumbHeight">Maximum height of the thumbnail</param>
    /// <param name="aRotation">
    /// 0 = no rotate
    /// 1 = rotate 90 degrees
    /// 2 = rotate 180 degrees
    /// 3 = rotate 270 degrees
    /// </param>
    /// <param name="aFastMode">Use low quality resizing without interpolation suitable for small thumbnails</param>
    /// <returns>Whether the thumb has been successfully created</returns>
    private static bool CreateThumbnail(Image aDrawingImage, string aThumbTargetPath, int aThumbWidth, int aThumbHeight,
                                        int aRotation, bool aFastMode)
    {
      if (string.IsNullOrEmpty(aThumbTargetPath) || aThumbHeight <= 0 || aThumbHeight <= 0) return false;

      Bitmap myBitmap = null;
      Image myTargetThumb = null;

      try
      {
        switch (aRotation)
        {
          case 1:
            aDrawingImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            break;
          case 2:
            aDrawingImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            break;
          case 3:
            aDrawingImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            break;
          default:
            break;
        }

        int iWidth = aThumbWidth;
        int iHeight = aThumbHeight;
        float fAR = (aDrawingImage.Width) / ((float)aDrawingImage.Height);

        if (aDrawingImage.Width > aDrawingImage.Height)
          iHeight = (int)Math.Floor((((float)iWidth) / fAR));
        else
          iWidth = (int)Math.Floor((fAR * ((float)iHeight)));

        /*try
        {
          Utils.FileDelete(aThumbTargetPath);
        }
        catch (Exception ex)
        {
          Log.Error("Picture: Error deleting old thumbnail - {0}", ex.Message);
        }*/

        if (aFastMode)
        {
          Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
          myBitmap = new Bitmap(aDrawingImage, iWidth, iHeight);
          myTargetThumb = myBitmap.GetThumbnailImage(iWidth, iHeight, myCallback, IntPtr.Zero);
        }
        else
        {
          myBitmap = new Bitmap(iWidth, iHeight, aDrawingImage.PixelFormat);
          //myBitmap.SetResolution(aDrawingImage.HorizontalResolution, aDrawingImage.VerticalResolution);
          using (Graphics g = Graphics.FromImage(myBitmap))
          {
            g.CompositingQuality = Thumbs.Compositing;
            g.InterpolationMode = Thumbs.Interpolation;
            g.SmoothingMode = Thumbs.Smoothing;
            g.DrawImage(aDrawingImage, new Rectangle(0, 0, iWidth, iHeight));
            myTargetThumb = myBitmap;
          }
        }

        return SaveThumbnail(aThumbTargetPath, myTargetThumb);
      }
      catch (Exception)
      {
        return false;
      }
      finally
      {
        if (myTargetThumb != null)
          myTargetThumb.Dispose();
        if (myBitmap != null)
          myBitmap.Dispose();
      }
    }

    private static bool SaveThumbnail(string aThumbTargetPath, Image myImage)
    {
      try
      {
        myImage.Save(aThumbTargetPath);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("Picture: Error saving new thumbnail {0} - {1}", aThumbTargetPath, ex.Message);
        return false;
      }
    }

    private static void AddPicture(Graphics g, string strFileName, int x, int y, int w, int h)
    {
      Image img = null;
      try
      {
        try
        {
          using (FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
          {
            using (img = Image.FromStream(fs, true, false))
            {
              if (img != null)
                g.DrawImage(img, x, y, w, h);
            }
          }
        }
        catch (OutOfMemoryException)
        {
          Log.Info("Utils: Damaged picture file found: {0}. Try to repair or delete this file please!", strFileName);
        }
        catch (Exception ex)
        {
          Log.Info("Utils: An exception occured adding an image to the folder preview thumb: {0}", ex.Message);
        }
      }
      finally
      {
        if (img != null)
          img.Dispose();
      }
    }

    public static bool CreateTileThumb(List<string> aPictureList, string aThumbPath, int PreviewColumns, int PreviewRows)
    {
      bool result = false;

      if (aPictureList.Count > 0)
      {
        try
        {
          // Use first generated picture to take the ratio
          Image imgFolder = null;
          try
          {
            imgFolder = ImageFast.FromFile(aPictureList[0]);
          }
          catch (Exception)
          {
            Log.Debug("TvThumbnails: Fast loading failed {0} failed using safer fallback", aPictureList[0]);
            imgFolder = Image.FromFile(aPictureList[0]);
          }

          if (imgFolder != null)
          {
            using (imgFolder)
            {
              int width = imgFolder.Width;
              int height = imgFolder.Height;

              int thumbnailWidth = 256;
              int thumbnailHeight = 256;
              // draw a fullsize thumb if only 1 pic is available
              switch (PreviewColumns)
              {
                case 1:
                  thumbnailWidth = width;
                  break;
                case 2:
                  thumbnailWidth = width / 2;
                  break;
                case 3:
                  thumbnailWidth = width / 3;
                  break;
              }
              switch (PreviewRows)
              {
                case 1:
                  thumbnailHeight = height;
                  break;
                case 2:
                  thumbnailHeight = height / 2;
                  break;
                case 3:
                  thumbnailHeight = height / 3;
                  break;
              }

              using (Bitmap bmp = new Bitmap(width, height))
              {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                  g.CompositingQuality = Thumbs.Compositing;
                  g.InterpolationMode = Thumbs.Interpolation;
                  g.SmoothingMode = Thumbs.Smoothing;

                  g.DrawImage(imgFolder, 0, 0, width, height);
                  int w, h;
                  w = thumbnailWidth;
                  h = thumbnailHeight;

                  try
                  {
                    if (PreviewColumns == 1 && PreviewRows == 1)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                    }
                    if (PreviewColumns == 1 && PreviewRows == 2)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], 0, h, w, h);
                    }
                    if (PreviewColumns == 2 && PreviewRows == 1)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], w, 0, w, h);
                    }
                    if (PreviewColumns == 2 && PreviewRows == 2)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], w, 0, w, h);
                      AddPicture(g, (string)aPictureList[2], 0, h, w, h);
                      AddPicture(g, (string)aPictureList[3], w, h, w, h);
                    }
                    if (PreviewColumns == 1 && PreviewRows == 3)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], 0, h, w, h);
                      AddPicture(g, (string)aPictureList[2], 0, 2 * h, w, h);
                    }
                    if (PreviewColumns == 2 && PreviewRows == 3)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], w, 0, w, h);
                      AddPicture(g, (string)aPictureList[2], 0, h, w, h);
                      AddPicture(g, (string)aPictureList[3], w, h, w, h);
                      AddPicture(g, (string)aPictureList[4], 0, 2 * h, w, h);
                      AddPicture(g, (string)aPictureList[5], w, 2 * h, w, h);
                    }
                    if (PreviewColumns == 3 && PreviewRows == 3)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], w, 0, w, h);
                      AddPicture(g, (string)aPictureList[2], 2 * w, 0, w, h);
                      AddPicture(g, (string)aPictureList[3], 0, h, w, h);
                      AddPicture(g, (string)aPictureList[4], w, h, w, h);
                      AddPicture(g, (string)aPictureList[5], 2 * w, h, w, h);
                      AddPicture(g, (string)aPictureList[6], 0, 2 * h, w, h);
                      AddPicture(g, (string)aPictureList[7], w, 2 * h, w, h);
                      AddPicture(g, (string)aPictureList[8], 2 * w, 2 * h, w, h);
                    }
                    if (PreviewColumns == 3 && PreviewRows == 1)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], w, 0, w, h);
                      AddPicture(g, (string)aPictureList[2], 2 * w, 0, w, h);
                    }
                    if (PreviewColumns == 3 && PreviewRows == 2)
                    {
                      AddPicture(g, (string)aPictureList[0], 0, 0, w, h);
                      AddPicture(g, (string)aPictureList[1], w, 0, w, h);
                      AddPicture(g, (string)aPictureList[2], 2 * w, 0, w, h);
                      AddPicture(g, (string)aPictureList[3], 0, h, w, h);
                      AddPicture(g, (string)aPictureList[4], w, h, w, h);
                      AddPicture(g, (string)aPictureList[5], 2 * w, h, w, h);
                    }
                  }
                  catch (Exception ex)
                  {
                    Log.Error("Utils: An exception occured creating CreateTileThumb: {0}", ex.Message);
                  }
                }

                try
                {
                  string tmpFile = Path.GetTempFileName();
                  Log.Debug("CreateTileThumb: before Saving thumb! tmpFile: {0}", tmpFile);

                  bmp.Save(tmpFile, Thumbs.ThumbCodecInfo, Thumbs.ThumbEncoderParams);
                  Log.Debug("CreateTileThumb: Saving thumb!");
                  CreateThumbnail(tmpFile, aThumbPath, (int) Thumbs.ThumbLargeResolution,
                                  (int) Thumbs.ThumbLargeResolution, 0);
                  Log.Debug("CreateTileThumb: after Saving thumb!");

                  File.Delete(tmpFile);

                  if (imgFolder != null)
                  {
                    imgFolder.Dispose();
                  }

                  if (aPictureList.Count > 0)
                  {
                    string pictureListName = string.Empty;
                    try
                    {
                      for (int i = 0; i < (aPictureList.Count); i++)
                      {
                        pictureListName = aPictureList[i];
                        File.Delete(aPictureList[i]);
                      }
                    }
                    catch (FileNotFoundException)
                    {
                      Log.Debug("CreateTileThumb: {0} file not found.", pictureListName);
                    }
                  }

                  Thread.Sleep(100);

                  if (File.Exists(aThumbPath))
                    result = true;
                }
                catch (Exception ex2)
                {
                  Log.Error("CreateTileThumb: An exception occured saving CreateTileThumb: {0} - {1}", aThumbPath,
                            ex2.Message);
                }
              }
            }
          }
        }
        catch (FileNotFoundException)
        {
          Log.Error("CreateTileThumb: Your skin does not find first image to create CreateTileThumb!");
        }
        catch (Exception exm)
        {
          Log.Error("CreateTileThumb: An error occured creating folder CreateTileThumb: {0}", exm.Message);
        }
      }
      return result;
    }

    private static bool ThumbnailCallback()
    {
      return false;
    }

    /// <summary>
    /// Creates a thumbnail of the specified image filename
    /// </summary>
    /// <param name="aInputFilename">The source filename to load a System.Drawing.Image from</param>
    /// <param name="aThumbTargetPath">Filename of the thumbnail to create</param>
    /// <param name="aThumbWidth">Maximum width of the thumbnail</param>
    /// <param name="aThumbHeight">Maximum height of the thumbnail</param>
    /// <param name="aRotation">
    /// 0 = no rotate
    /// 1 = rotate 90 degrees
    /// 2 = rotate 180 degrees
    /// 3 = rotate 270 degrees
    /// </param>
    /// <param name="aFastMode">Use low quality resizing without interpolation suitable for small thumbnails</param>
    /// <returns>Whether the thumb has been successfully created</returns>
    private static bool CreateThumbnail(string aInputFilename, string aThumbTargetPath, 
                                        int iMaxWidth, int iMaxHeight, int iRotate)
    {
      if (string.IsNullOrEmpty(aInputFilename) || string.IsNullOrEmpty(aThumbTargetPath) || iMaxHeight <= 0 ||
          iMaxHeight <= 0) return false;

      if (!File.Exists(aInputFilename)) return false;

      Image myImage = null;

      try
      {
        myImage = ImageFast.FromFile(aInputFilename);
        return CreateThumbnail(myImage, aThumbTargetPath, iMaxWidth, iMaxHeight, iRotate, true);
      }
      catch (ArgumentException)
      {
        Log.Info("Picture: Fast loading of thumbnail {0} failed - trying safe fallback now", aInputFilename);

        try
        {
          myImage = Image.FromFile(aInputFilename, true);
          return CreateThumbnail(myImage, aThumbTargetPath, iMaxWidth, iMaxHeight, iRotate, true);
        }
        catch (OutOfMemoryException)
        {
          Log.Info("Picture: Creating thumbnail failed - image format is not supported of {0}", aInputFilename);
          return false;
        }
        catch (Exception ex)
        {
          Log.Error("Picture: CreateThumbnail exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          return false;
        }
      }
      finally
      {
        if (myImage != null)
          myImage.Dispose();
      }
    }

    private static void KillProcess(string aProcessName)
    {
      try
      {
        Process[] leftovers = System.Diagnostics.Process.GetProcessesByName(aProcessName);
        foreach (Process termProc in leftovers)
        {
          try
          {
            Log.Info("Util: Killing process: {0}", termProc.ProcessName);
            termProc.Kill();
          }
          catch (Exception exk)
          {
            Log.Error("Util: Error stopping processes - {0})", exk.ToString());
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Util: Error getting processes by name for {0} - {1})", aProcessName, ex.ToString());
      }
    }


    private static ProcessFailedConditions GetMtnConditions()
    {
      ProcessFailedConditions mtnStat = new ProcessFailedConditions();
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static bool StartProcess(string aAppName, string aArguments, string aWorkingDir, int aExpectedTimeoutMs,
                                     bool aLowerPriority, ProcessFailedConditions aFailConditions)
    {
      bool success = false;
      Process ExternalProc = new Process();
      ProcessStartInfo ProcOptions = new ProcessStartInfo(aAppName, aArguments);

      ProcOptions.UseShellExecute = false; // Important for WorkingDirectory behaviour
      ProcOptions.RedirectStandardError = true; // .NET bug? Some stdout reader abort to early without that!
      ProcOptions.RedirectStandardOutput = true; // The precious data we're after
      //ProcOptions.StandardOutputEncoding = Encoding.GetEncoding("ISO-8859-1"); // the output contains "Umlaute", etc.
      //ProcOptions.StandardErrorEncoding = Encoding.GetEncoding("ISO-8859-1");
      ProcOptions.WorkingDirectory = aWorkingDir; // set the dir because the binary might depend on cygwin.dll
      ProcOptions.CreateNoWindow = true; // Do not spawn a "Dos-Box"      
      ProcOptions.ErrorDialog = false; // Do not open an error box on failure        

      ExternalProc.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
      ExternalProc.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);
      ExternalProc.EnableRaisingEvents = true; // We want to know when and why the process died        
      ExternalProc.StartInfo = ProcOptions;
      if (File.Exists(ProcOptions.FileName))
      {
        try
        {
          ExternalProc.Start();
          if (aLowerPriority)
          {
            try
            {
              ExternalProc.PriorityClass = ProcessPriorityClass.BelowNormal;
              // Execute all processes in the background so movies, etc stay fluent
            }
            catch (Exception ex2)
            {
              Log.Error("Util: Error setting process priority for {0}: {1}", aAppName, ex2.Message);
            }
          }
          // Read in asynchronous  mode to avoid deadlocks (if error stream is full)
          // http://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.redirectstandarderror.aspx
          ExternalProc.BeginErrorReadLine();
          ExternalProc.BeginOutputReadLine();

          // wait this many seconds until the process has to be finished
          ExternalProc.WaitForExit(aExpectedTimeoutMs);

          success = (ExternalProc.HasExited && ExternalProc.ExitCode == aFailConditions.SuccessExitCode);

          ExternalProc.OutputDataReceived -= new DataReceivedEventHandler(OutputDataHandler);
          ExternalProc.ErrorDataReceived -= new DataReceivedEventHandler(ErrorDataHandler);
        }
        catch (Exception ex)
        {
          Log.Error("Util: Error executing {0}: {1}", aAppName, ex.Message);
        }
      }
      else
        Log.Info("Util: Could not start {0} because it doesn't exist!", ProcOptions.FileName);

      return success;
    }

    private static void OutputDataHandler(object sendingProcess,
                                          DataReceivedEventArgs outLine)
    {
      if (!String.IsNullOrEmpty(outLine.Data))
      {
        Log.Info("Util: StdOut - {0}", outLine.Data);
      }
    }

    private static void ErrorDataHandler(object sendingProcess,
                                         DataReceivedEventArgs errLine)
    {
      if (!String.IsNullOrEmpty(errLine.Data))
      {
        Log.Info("Util: StdErr - {0}", errLine.Data);
      }
    }
  }

  internal class ProcessFailedConditions
  {
    public List<string> CriticalOutputLines = new List<string>();
    public int SuccessExitCode = 0;

    internal void AddCriticalOutString(string aFailureString)
    {
      CriticalOutputLines.Add(aFailureString);
    }
  }
}