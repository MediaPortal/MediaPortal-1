/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Drawing;
using System.Collections;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Core.Transcoding;
using MediaPortal.Util;
namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Summary description for Transcoder.
  /// </summary>
  public class Transcoder
  {
    #region vars
    static ArrayList queue = new ArrayList();
    static Thread WorkerThread = null;
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
      public int FPS;
      public int Type;
      public Quality quality;
      public bool deleteOriginal;
      public Size ScreenSize;
      public DateTime StartTime;
      public bool LowPriority = true;
      #endregion
      public TranscoderInfo(TVRecorded recording, int kbps, int fps, Size newSize, bool deleteWhenDone, int qualityIndex, DateTime dateTime, int outputType, bool priority)
      {
        recorded = recording;
        status = Status.Waiting;
        percentDone = 0;
        bitRate = kbps;
        FPS = fps;
        ScreenSize = newSize;
        deleteOriginal = deleteWhenDone;
        quality = (Quality)qualityIndex;
        StartTime = dateTime;
        Type = outputType;
        LowPriority = priority;
      }
      public void SetProperties()
      {
        if (status != Status.Busy)
        {
          GUIPropertyManager.SetProperty("#TV.Transcoding.Percentage", "0");
          GUIPropertyManager.SetProperty("#TV.Transcoding.File", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Title", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Genre", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Description", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Transcoding.Channel", String.Empty);
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
    static public void Transcode(TVRecorded rec, bool manual)
    {
      int bitRate, FPS, Priority, QualityIndex, ScreenSizeIndex, Type, AutoHours;
      bool deleteOriginal, AutoDeleteOriginal, AutoCompress;
      Size ScreenSize = new Size(0, 0);
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        bitRate = xmlreader.GetValueAsInt("compression", "bitrate", 4);
        FPS = xmlreader.GetValueAsInt("compression", "fps", 1);
        Priority = xmlreader.GetValueAsInt("compression", "priority", 0);
        QualityIndex = xmlreader.GetValueAsInt("compression", "quality", 3);
        ScreenSizeIndex = xmlreader.GetValueAsInt("compression", "screensize", 1);
        Type = xmlreader.GetValueAsInt("compression", "type", 0);
        deleteOriginal = xmlreader.GetValueAsBool("compression", "deleteoriginal", true);

        AutoHours = xmlreader.GetValueAsInt("autocompression", "hour", 4);
        AutoDeleteOriginal = xmlreader.GetValueAsBool("autocompression", "deleteoriginal", true);
        AutoCompress = xmlreader.GetValueAsBool("autocompression", "enabled", true);
      }
      switch (bitRate)
      {
        case 0:
          bitRate = 100;
          break;
        case 1:
          bitRate = 256;
          break;
        case 2:
          bitRate = 384;
          break;
        case 3:
          bitRate = 768;
          break;
      }
      switch (FPS)
      {
        case 0:
          FPS = 15;
          break;
        case 1:
          FPS = 25;
          break;
        case 2:
          FPS = 30;
          break;
      }
      switch (ScreenSizeIndex)
      {
        case 0:
          ScreenSize = new Size(1024, 768);
          break;
        case 1:
          ScreenSize = new Size(720, 576);
          break;
        case 2:
          ScreenSize = new Size(704, 480);
          break;
        case 3:
          ScreenSize = new Size(740, 288);
          break;
        case 4:
          ScreenSize = new Size(740, 240);
          break;
        case 5:
          ScreenSize = new Size(704, 576);
          break;
        case 6:
          ScreenSize = new Size(640, 480);
          break;
        case 7:
          ScreenSize = new Size(640, 288);
          break;
        case 8:
          ScreenSize = new Size(640, 240);
          break;
        case 9:
          ScreenSize = new Size(352, 288);
          break;
        case 10:
          ScreenSize = new Size(352, 240);
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
        TranscoderInfo info = new TranscoderInfo(rec, bitRate, FPS, ScreenSize, deleteWhenDone, QualityIndex, dtStart, Type, Priority == 0);
        queue.Add(info);
      }

      if (WorkerThread == null)
      {
        WorkerThread = new Thread(new ThreadStart(TranscodeWorkerThread));
        WorkerThread.SetApartmentState(ApartmentState.STA);
        WorkerThread.IsBackground = true;
        WorkerThread.Start();
      }
    }

    static public void Cancel(TranscoderInfo info)
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

    static public void ReQueue(TranscoderInfo info)
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

    static public void Clear()
    {
      lock (queue)
      {
        bool deleted = false;
        do
        {
          deleted = false;
          for (int i = 0; i < queue.Count; ++i)
          {
            TranscoderInfo info = (TranscoderInfo)queue[i];
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
    static public ArrayList Queue
    {
      get
      {
        return queue;
      }
    }
    static public bool IsTranscoding(TVRecorded rec)
    {
      lock (queue)
      {
        foreach (TranscoderInfo info in queue)
        {
          if (info.status == Status.Error || info.status == Status.Completed) continue;
          if (info.recorded.FileName == rec.FileName) return true;
        }
      }
      return false;
    }
    #endregion

    #region transcoding workerthread
    static void TranscodeWorkerThread()
    {

      while (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
      {
        if (queue.Count == 0)
        {
          System.Threading.Thread.Sleep(1000);
        }
        else
        {
          TranscoderInfo transcording = null;
          lock (queue)
          {
            for (int i = 0; i < queue.Count; ++i)
            {
              TranscoderInfo info = (TranscoderInfo)queue[i];
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
                transcording.status = Status.Error;
              Log.WriteFile(Log.LogType.Log, true, "Transcoder:Exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
            }
          }
          else System.Threading.Thread.Sleep(1000);
        }
      }
    }

    static void DoTranscode(TranscoderInfo tinfo)
    {

      if (tinfo.LowPriority)
        System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      else
        System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Normal;

      tinfo.status = Status.Busy;
      TranscodeInfo info = new TranscodeInfo();
      info.Author = "Mediaportal";
      info.Channel = tinfo.recorded.Channel;
      info.Description = tinfo.recorded.Description;
      info.Title = tinfo.recorded.Title;
      info.Start = tinfo.recorded.StartTime;
      info.End = tinfo.recorded.EndTime;
      TimeSpan ts = (tinfo.recorded.EndTime - tinfo.recorded.StartTime);
      info.Duration = (int)ts.TotalSeconds;
      info.file = tinfo.recorded.FileName;


      bool isMpeg = (tinfo.Type == 0);
      bool isWMV = (tinfo.Type == 1);
      bool isXVID = (tinfo.Type == 2);
      switch (tinfo.quality)
      {
        case Quality.High:
          tinfo.ScreenSize = new Size(0, 0);//keep video resolution
          tinfo.FPS = 0;//keep video FPS
          tinfo.bitRate = 768;
          break;

        case Quality.Medium:
          tinfo.ScreenSize = new Size(0, 0);//keep video resolution
          tinfo.FPS = 0;//keep video FPS
          tinfo.bitRate = 384;
          break;

        case Quality.Low:
          tinfo.ScreenSize = new Size(0, 0);//keep video resolution
          tinfo.FPS = 0;//keep video FPS
          tinfo.bitRate = 256;
          break;

        case Quality.Portable:
          tinfo.ScreenSize = new Size(352, 288);
          tinfo.FPS = 15;
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


      if (isXVID)
      {
        ConvertToXvid(info, tinfo);
        return;
      }
    }

    #endregion

    #region transcoding methods
    static void ConvertToWmv(TranscodeInfo info, TranscoderInfo tinfo)
    {
      TranscodeToWMV WMVConverter = new TranscodeToWMV();
      WMVConverter.CreateProfile(tinfo.ScreenSize, tinfo.bitRate, tinfo.FPS);
      if (!WMVConverter.Transcode(info, VideoFormat.Wmv, tinfo.quality))
      {
        tinfo.status = Status.Error;
        tinfo.SetProperties();
        return;
      }
      while (!WMVConverter.IsFinished())
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING) return;
        tinfo.percentDone = WMVConverter.Percentage();
        tinfo.SetProperties();
        System.Threading.Thread.Sleep(1000);
        if (tinfo.status == Status.Canceled)
        {
          WMVConverter.Stop();
          return;
        }
      }
      if (tinfo.deleteOriginal)
      {
        DiskManagement.DeleteRecording(info.file);
        tinfo.recorded.FileName = System.IO.Path.ChangeExtension(info.file, ".wmv");
        TVDatabase.SetRecordedFileName(tinfo.recorded);
      }
      tinfo.status = Status.Completed;
      tinfo.SetProperties();
    }

    static void ConvertToMpg(TranscodeInfo info, TranscoderInfo tinfo)
    {
      Dvrms2Mpeg mpgConverter = new Dvrms2Mpeg();
      if (!mpgConverter.Transcode(info, VideoFormat.Mpeg2, tinfo.quality))
      {
        tinfo.status = Status.Error;
        tinfo.SetProperties();
        return;
      }
      while (!mpgConverter.IsFinished())
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING) return;
        tinfo.percentDone = mpgConverter.Percentage();
        tinfo.SetProperties();
        System.Threading.Thread.Sleep(1000);
        if (tinfo.status == Status.Canceled)
        {
          mpgConverter.Stop();
          return;
        }
      }
      if (tinfo.deleteOriginal)
      {
        DiskManagement.DeleteRecording(info.file);
        tinfo.recorded.FileName = System.IO.Path.ChangeExtension(info.file, ".mpg");
        TVDatabase.SetRecordedFileName(tinfo.recorded);
      }
      tinfo.status = Status.Completed;
      tinfo.SetProperties();
    }

    static void ConvertToXvid(TranscodeInfo info, TranscoderInfo tinfo)
    {
      Dvrms2XVID xvidEncoder = new Dvrms2XVID();
      xvidEncoder.CreateProfile(tinfo.ScreenSize, tinfo.bitRate, tinfo.FPS);
      if (!xvidEncoder.Transcode(info, VideoFormat.Xvid, tinfo.quality))
      {
        tinfo.status = Status.Error;
        tinfo.SetProperties();
        return;
      }
      while (!xvidEncoder.IsFinished())
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING) return;
        tinfo.percentDone = xvidEncoder.Percentage();
        tinfo.SetProperties();
        System.Threading.Thread.Sleep(1000);
        if (tinfo.status == Status.Canceled)
        {
          xvidEncoder.Stop();
          return;
        }
      }
      if (tinfo.deleteOriginal)
      {
        DiskManagement.DeleteRecording(info.file);
        tinfo.recorded.FileName = System.IO.Path.ChangeExtension(info.file, ".avi");
        TVDatabase.SetRecordedFileName(tinfo.recorded);
      }
      tinfo.status = Status.Completed;
      tinfo.SetProperties();
      return;
    }
    #endregion
  }
}
