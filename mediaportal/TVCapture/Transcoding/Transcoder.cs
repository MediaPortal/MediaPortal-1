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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Core.Transcoding;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Summary description for Transcoder.
  /// </summary>
  public class Transcoder
  {
    #region vars

    private static ArrayList queue = new ArrayList();
    private static Thread WorkerThread = null;

    #endregion

    #region enums

    public enum Status
    {
      Waiting,
      Busy,
      Completed,
      Error,
      Canceled
    }

    #endregion

    #region TranscoderInfo class

    public class TranscoderInfo
    {
      #region vars

      public TVRecorded recorded;
      public Status status;
      public int percentDone;
      public int bitRate;
      public double FPS;
      public int Type;
      public Quality quality;
      public Standard standard;
      public bool deleteOriginal;
      public Size ScreenSize;
      public DateTime StartTime;
      public bool LowPriority = true;

      #endregion

      public TranscoderInfo(TVRecorded recording, int kbps, double fps, Size newSize, bool deleteWhenDone,
                            int qualityIndex, int standardIndex, DateTime dateTime, int outputType, bool priority)
      {
        recorded = recording;
        status = Status.Waiting;
        percentDone = 0;
        bitRate = kbps;
        FPS = fps;
        ScreenSize = newSize;
        deleteOriginal = deleteWhenDone;
        quality = (Quality) qualityIndex;
        standard = (Standard) standardIndex;
        StartTime = dateTime;
        Type = outputType;
        LowPriority = priority;
      }

      public void SetProperties()
      {
        if (status != Status.Busy)
        {
          GUIPropertyManager.SetProperty("#TV.Transcoding.Percentage", "0");
          GUIPropertyManager.SetProperty("#TV.Transcoding.File", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Title", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Genre", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Description", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Channel", string.Empty);
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.Transcoding.Percentage", percentDone.ToString());
          GUIPropertyManager.SetProperty("#TV.Transcoding.File", recorded.FileName);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Title", recorded.Title);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Genre", recorded.Genre);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Description", recorded.Description);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Channel", recorded.Channel);
        }
      }
    }

    #endregion

    #region ctor

    static Transcoder()
    {
    }

    #endregion

    #region public methods

    public static void Transcode(TVRecorded rec, bool manual)
    {
      int bitRate, Priority, QualityIndex, StandardIndex, ScreenSizeIndex, Type, AutoHours;
      double FPS;
      bool deleteOriginal, AutoDeleteOriginal, AutoCompress;
      Size ScreenSize = new Size(0, 0);
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bitRate = xmlreader.GetValueAsInt("compression", "bitrate", 3);
        FPS = xmlreader.GetValueAsInt("compression", "fps", 2);
        Priority = xmlreader.GetValueAsInt("compression", "priority", 0);
        QualityIndex = xmlreader.GetValueAsInt("compression", "quality", 3);
        ScreenSizeIndex = xmlreader.GetValueAsInt("compression", "screensize", 2);
        Type = xmlreader.GetValueAsInt("compression", "type", 2);
        deleteOriginal = xmlreader.GetValueAsBool("compression", "deleteoriginal", false);
        AutoHours = xmlreader.GetValueAsInt("autocompression", "hour", 4);
        AutoDeleteOriginal = xmlreader.GetValueAsBool("autocompression", "deleteoriginal", false);
        AutoCompress = xmlreader.GetValueAsBool("autocompression", "enabled", false);
        StandardIndex = xmlreader.GetValueAsInt("compression", "standard", 2);
      }
      switch (bitRate)
      {
        case 0:
          bitRate = 100; //Portable
          break;
        case 1:
          bitRate = 256; //Low
          break;
        case 2:
          bitRate = 384; //Medium
          break;
        case 3:
          bitRate = 768; //High
          break;
        case 4:
          bitRate = 1536; //Very High
          break;
        case 5:
          bitRate = 3072; //HiDef
          break;
        case 6:
          bitRate = 5376; //Custom
          break;
      }
      switch ((int) FPS)
      {
        case 0:
          FPS = 12.5; //Portable (PAL)
          break;
        case 1:
          FPS = 15; //Portable (NTSC)
          break;
        case 2:
          FPS = 23.97; //Film
          break;
        case 3:
          FPS = 25; //PAL
          break;
        case 4:
          FPS = 29.97; //NTSC
          break;
      }
      switch (ScreenSizeIndex)
      {
        case 0:
          ScreenSize = new Size(240, 180); //Portable (NTSC)
          break;
        case 1:
          ScreenSize = new Size(288, 216); //Portable (PAL)
          break;
        case 2:
          ScreenSize = new Size(352, 240); //Low (NTSC)
          break;
        case 3:
          ScreenSize = new Size(352, 288); //Low (PAL)
          break;
        case 4:
          ScreenSize = new Size(640, 480); //Medium
          break;
        case 5:
          ScreenSize = new Size(704, 480); //NTSC
          break;
        case 6:
          ScreenSize = new Size(720, 576); //PAL
          break;
        case 7:
          ScreenSize = new Size(1280, 720); //HiDef
          break;
        case 8:
          ScreenSize = new Size(1920, 1080); //Custom only
          break;
      }
      lock (queue)
      {
        DateTime dtStart = DateTime.Now;
        bool deleteWhenDone = deleteOriginal;
        if (AutoCompress && !manual)
        {
          deleteWhenDone = AutoDeleteOriginal;
          dtStart = dtStart.AddHours(AutoHours);
        }
        TranscoderInfo info = new TranscoderInfo(rec, bitRate, FPS, ScreenSize, deleteWhenDone, QualityIndex,
                                                 StandardIndex, dtStart, Type, Priority == 0);
        queue.Add(info);
      }
      if (WorkerThread == null)
      {
        WorkerThread = new Thread(new ThreadStart(TranscodeWorkerThread));
        WorkerThread.Name = "Transcoder";
        WorkerThread.SetApartmentState(ApartmentState.STA);
        WorkerThread.IsBackground = true;
        WorkerThread.Start();
      }
    }

    public static void Cancel(TranscoderInfo info)
    {
      lock (queue)
      {
        foreach (TranscoderInfo tinfo in queue)
        {
          if (tinfo.recorded.FileName == info.recorded.FileName)
          {
            tinfo.status = Status.Canceled;
            return;
          }
        }
      }
    }

    public static void ReQueue(TranscoderInfo info)
    {
      lock (queue)
      {
        foreach (TranscoderInfo tinfo in queue)
        {
          if (tinfo.recorded.FileName == info.recorded.FileName)
          {
            tinfo.percentDone = 0;
            tinfo.status = Status.Waiting;
            return;
          }
        }
      }
    }

    public static void Clear()
    {
      lock (queue)
      {
        bool deleted = false;
        do
        {
          deleted = false;
          for (int i = 0; i < queue.Count; ++i)
          {
            TranscoderInfo info = (TranscoderInfo) queue[i];
            if (info.status == Status.Error || info.status == Status.Completed || info.status == Status.Canceled)
            {
              deleted = true;
              queue.RemoveAt(i);
              break;
            }
          }
        } while (deleted);
      }
    }

    public static ArrayList Queue
    {
      get { return queue; }
    }

    public static bool IsTranscoding(TVRecorded rec)
    {
      lock (queue)
      {
        foreach (TranscoderInfo info in queue)
        {
          if (info.status == Status.Error || info.status == Status.Completed)
          {
            continue;
          }
          if (info.recorded.FileName == rec.FileName)
          {
            return true;
          }
        }
      }
      return false;
    }

    #endregion

    #region transcoding workerthread

    private static void TranscodeWorkerThread()
    {
      while (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
      {
        if (queue.Count == 0)
        {
          Thread.Sleep(1000);
        }
        else
        {
          TranscoderInfo transcording = null;
          lock (queue)
          {
            for (int i = 0; i < queue.Count; ++i)
            {
              TranscoderInfo info = (TranscoderInfo) queue[i];
              if (DateTime.Now >= info.StartTime)
              {
                if (info.status == Status.Waiting)
                {
                  transcording = info;
                  break;
                }
              }
            }
          }
          if (transcording != null && transcording.status == Status.Waiting)
          {
            try
            {
              DoTranscode(transcording);
            }
            catch (Exception ex)
            {
              if (transcording.status == Status.Busy)
              {
                transcording.status = Status.Error;
              }
              Log.Error(ex);
            }
          }
          else
          {
            Thread.Sleep(1000);
          }
        }
      }
    }

    private static void DoTranscode(TranscoderInfo tinfo)
    {
      if (tinfo.LowPriority)
      {
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      }
      else
      {
        Thread.CurrentThread.Priority = ThreadPriority.Normal;
      }
      tinfo.status = Status.Busy;
      TranscodeInfo info = new TranscodeInfo();
      info.Author = "MediaPortal";
      info.Channel = tinfo.recorded.Channel;
      info.Description = tinfo.recorded.Description;
      info.Title = tinfo.recorded.Title;
      info.Start = tinfo.recorded.StartTime;
      info.End = tinfo.recorded.EndTime;
      TimeSpan ts = (tinfo.recorded.EndTime - tinfo.recorded.StartTime);
      info.Duration = (int) ts.TotalSeconds;
      info.file = tinfo.recorded.FileName;
      bool isMpeg = (tinfo.Type == 0);
      bool isWMV = (tinfo.Type == 1);
      bool isMP4 = (tinfo.Type == 2);
      switch (tinfo.quality)
      {
        case Quality.HiDef:
          tinfo.ScreenSize = new Size(1280, 720); //HiDef
          tinfo.FPS = 0; //keep video FPS
          tinfo.bitRate = 3072;
          break;
        case Quality.VeryHigh:
          tinfo.ScreenSize = new Size(0, 0); //keep video resolution
          tinfo.FPS = 0; //keep video FPS
          tinfo.bitRate = 1536;
          break;
        case Quality.High:
          tinfo.ScreenSize = new Size(0, 0); //keep video resolution
          tinfo.FPS = 0; //keep video FPS
          tinfo.bitRate = 768;
          break;
        case Quality.Medium:
          tinfo.ScreenSize = new Size(640, 480); //Medium
          tinfo.FPS = 0; //keep video FPS
          tinfo.bitRate = 384;
          break;
        case Quality.Low:
          tinfo.ScreenSize = new Size(352, 288); //Low (PAL)
          tinfo.FPS = 0; //keep video FPS
          tinfo.bitRate = 256;
          break;
        case Quality.Portable:
          tinfo.ScreenSize = new Size(288, 216); //Low (PAL)
          tinfo.FPS = 12.5; //set fps to 15fps
          tinfo.bitRate = 100;
          break;
      }
      tinfo.SetProperties();
      if (isWMV)
      {
        ConvertToWmv(info, tinfo);
        return;
      }
      if (isMpeg)
      {
        ConvertToMpg(info, tinfo);
        return;
      }
      if (isMP4)
      {
        ConvertToMP4(info, tinfo);
        return;
      }
    }

    #endregion

    #region transcoding methods

    private static void ConvertToWmv(TranscodeInfo info, TranscoderInfo tinfo)
    {
      DVRMS2WMV WMVConverter = new DVRMS2WMV();
      WMVConverter.CreateProfile(tinfo.ScreenSize, tinfo.bitRate, tinfo.FPS);
      if (!WMVConverter.Transcode(info, VideoFormat.Wmv, tinfo.quality, tinfo.standard))
      {
        tinfo.status = Status.Error;
        tinfo.SetProperties();
        return;
      }
      while (!WMVConverter.IsFinished())
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }
        tinfo.percentDone = WMVConverter.Percentage();
        tinfo.SetProperties();
        Thread.Sleep(1000);
        if (tinfo.status == Status.Canceled)
        {
          WMVConverter.Stop();
          return;
        }
      }
      if (tinfo.deleteOriginal)
      {
        Util.Utils.DeleteRecording(info.file);
        tinfo.recorded.FileName = Path.ChangeExtension(info.file, ".wmv");
        TVDatabase.SetRecordedFileName(tinfo.recorded);
      }
      tinfo.status = Status.Completed;
      tinfo.SetProperties();
    }

    private static void ConvertToMpg(TranscodeInfo info, TranscoderInfo tinfo)
    {
      Dvrms2Mpeg mpgConverter = new Dvrms2Mpeg();
      if (!mpgConverter.Transcode(info, VideoFormat.Mpeg2, tinfo.quality, tinfo.standard))
      {
        tinfo.status = Status.Error;
        tinfo.SetProperties();
        return;
      }
      while (!mpgConverter.IsFinished())
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }
        tinfo.percentDone = mpgConverter.Percentage();
        tinfo.SetProperties();
        Thread.Sleep(1000);
        if (tinfo.status == Status.Canceled)
        {
          mpgConverter.Stop();
          return;
        }
      }
      if (tinfo.deleteOriginal)
      {
        Util.Utils.DeleteRecording(info.file);
        tinfo.recorded.FileName = Path.ChangeExtension(info.file, ".mpg");
        TVDatabase.SetRecordedFileName(tinfo.recorded);
      }
      tinfo.status = Status.Completed;
      tinfo.SetProperties();
    }

    private static void ConvertToMP4(TranscodeInfo info, TranscoderInfo tinfo)
    {
      Transcode2MP4 mp4Encoder = new Transcode2MP4();
      mp4Encoder.CreateProfile(tinfo.ScreenSize, tinfo.bitRate, tinfo.FPS);
      if (!mp4Encoder.Transcode(info, VideoFormat.MP4, tinfo.quality, tinfo.standard))
      {
        tinfo.status = Status.Error;
        tinfo.SetProperties();
        return;
      }
      while (!mp4Encoder.IsFinished())
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }
        tinfo.percentDone = mp4Encoder.Percentage();
        tinfo.SetProperties();
        Thread.Sleep(1000);
        if (tinfo.status == Status.Canceled)
        {
          mp4Encoder.Stop();
          return;
        }
      }
      if (tinfo.deleteOriginal)
      {
        Util.Utils.DeleteRecording(info.file);
        tinfo.recorded.FileName = Path.ChangeExtension(info.file, ".mp4");
        TVDatabase.SetRecordedFileName(tinfo.recorded);
      }
      tinfo.status = Status.Completed;
      tinfo.SetProperties();
      return;
    }

    #endregion
  }
}