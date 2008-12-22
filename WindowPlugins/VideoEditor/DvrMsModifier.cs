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
using System.Collections.Generic;
using System.Text;
using DirectShowLib.SBE;
using DirectShowLib;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using DShowNET.Helper;
using System.Threading;

namespace WindowPlugins.VideoEditor
{
  class DvrMsModifier
  {
    IStreamBufferRecComp recCompcut = null;
    System.Timers.Timer cutProgresstime;
    System.Timers.Timer joinProgresstime;
    System.Timers.Timer transcodeProgresstime;
    System.Timers.Timer convertProgresstime;
    public delegate void Finished();
    public event Finished OnFinished;
    public delegate void Progress(int percentage);
    public event Progress OnProgress;
    int percent = 0;
    int joinedFiles = 0;
    double newDuration = 0;
    System.IO.FileInfo inFilename;
    List<TimeDomain> cutPoints;
    List<System.IO.FileInfo> fileList;
    MediaPortal.Core.Transcoding.Dvrms2Mpeg tompeg;
    MediaPortal.Core.Transcoding.Dvrms2Divx toDivx;
    //string filetoConvert;
    //bool inDatabase;
    MediaPortal.TV.Database.TVRecorded recInfo;

    public DvrMsModifier()
    {
      //cutProgresstime = new System.Timers.Timer(1000);
      //cutProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(CutProgresstime_Elapsed);
    }

    public void CutDvr(System.IO.FileInfo inFilename, List<TimeDomain> cutPoints)
    {
      this.inFilename = inFilename;
      this.cutPoints = cutPoints;
      Thread cutThread = new Thread(new ThreadStart(CutDvr));
      cutThread.Name = "CutDvr";
      cutThread.Start();
    }

    public void CutDvr()
    {
      try
      {
        cutProgresstime = new System.Timers.Timer(1000);
        cutProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(CutProgresstime_Elapsed);
        recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
        if (recCompcut != null)
        {
          System.IO.FileInfo outFilename;
          percent = 0;
          //CutProgressTime();
          cutProgresstime.Start();
          string outPath = inFilename.FullName;
          //rename the source file ------------later this could be configurable to delete it
          //TODO behavior if the renamed sourcefile (_original) exists
          inFilename.MoveTo(inFilename.FullName.Replace(inFilename.Extension, "_original" + inFilename.Extension));
          //to not to change the database the outputfile has the same name 
          outFilename = new System.IO.FileInfo(outPath);
          if (outFilename.Exists)
          {
            outFilename.Delete();
          }
          for (int i = 0; i < cutPoints.Count; i++)
          {
            newDuration += cutPoints[i].Duration;
          }
          recCompcut.Initialize(outFilename.FullName, inFilename.FullName);
          for (int i = 0; i < cutPoints.Count; i++)
          {
            //string[] split = cutList[i].ToString().Split(new char[] { ':' });
            //startCut = cutPoints[i].StartTime;
            //endCut = cutPoints[i].EndTime;

            recCompcut.AppendEx(inFilename.FullName, (long)(cutPoints[i].StartTime * 10000000), (long)(cutPoints[i].EndTime * 10000000));
          }
          cutProgresstime.Stop();
          recCompcut.Close();
          percent = 100;
          /*recInfo = new MediaPortal.TV.Database.TVRecorded();
          if (TVDatabase.GetRecordedTVByFilename(outFilename.FullName, ref rec))
          {
              TVDatabase.RemoveRecordedTV(recInfo);
              recInfo.End = MediaPortal.Util.Utils.datetolong(recInfo.StartTime.AddSeconds(durationNew));
              TVDatabase.AddRecordedTV(recInfo);
          }*/

          if (OnFinished != null)
            OnFinished();
          //cutFinished = true;
          //progressLbl.Label = "100";
          //progressBar.Percentage = 100;
          //MessageBox(GUILocalizeStrings.Get(2083), GUILocalizeStrings.Get(2111)); //Dvrms:Finished to cut the video file , Finished !
          //progressBar.IsVisible = false;
          //progressLbl.IsVisible = false;

        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        DirectShowUtil.ReleaseComObject((object)recCompcut);
        recCompcut = null;
        cutPoints = null;
        percent = 0;
        newDuration = 0;
        //cutProgresstime.Stop();
      }
    }

    public void JoinDvr(System.IO.FileInfo firstFile, System.IO.FileInfo secondFile)
    {
      fileList = new List<System.IO.FileInfo>(2);
      fileList.Add(firstFile);
      fileList.Add(secondFile);
      System.Threading.Thread joinThread = new System.Threading.Thread(new System.Threading.ThreadStart(JoinDvr));
      joinThread.Start();
      /*
      try
      {
          cutProgresstime = new System.Timers.Timer(1000);
          cutProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(progressTime_Elapsed);
          recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
          if (recCompcut != null)
          {
              System.IO.FileInfo outFilename;
              percent = 0;
              //cutProgresstime.Start();
              string outPath = firstFile.FullName.Replace(firstFile.Extension, "_joined" + firstFile.Extension);
              //rename the source file ------------later this could be configurable to delete it
              //TODO behavior if the renamed sourcefile (_original) exists
              //inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
              //to not to change the database the outputfile has the same name 
              outFilename = new System.IO.FileInfo(outPath);


              if (outFilename.Exists)
              {
                  outFilename.Delete();
              }
              recCompcut.Initialize(outFilename.FullName, firstFile.FullName);
              recCompcut.Append(firstFile.FullName);
              recCompcut.Append(secondFile.FullName);
              recCompcut.Close();
              percent = 100;
              if (OnFinished != null)
                  OnFinished();
          }
      }
      catch (Exception ex)
      {
          Log.Error(ex);
      }
      finally
      {
          DirectShowUtil.ReleaseComObject((object)recCompcut);
          recCompcut = null;
          cutPoints = null;
          percent = 0;
          newDuration = 0;
          //cutProgresstime.Stop();
      }*/
    }

    public void JoinDvr(List<System.IO.FileInfo> fileList)
    {
      this.fileList = fileList;
      System.Threading.Thread joinThread = new System.Threading.Thread(new System.Threading.ThreadStart(JoinDvr));
      joinThread.Start();
      //JoinDvr();
    }

    public void JoinDvr()
    {
      try
      {
        joinProgresstime = new System.Timers.Timer(1000);
        joinProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(JoinProgresstime_Elapsed);
        recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
        if (recCompcut != null)
        {
          System.IO.FileInfo outFilename;
          percent = 0;
          joinedFiles = 0;
          joinProgresstime.Start();
          //cutProgresstime.Start();
          string outPath = fileList[0].FullName.Replace(".dvr-ms", "_joined.dvr-ms");
          //rename the source file ------------later this could be configurable to delete it
          //TODO behavior if the renamed sourcefile (_original) exists
          //inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
          //to not to change the database the outputfile has the same name 
          outFilename = new System.IO.FileInfo(outPath);


          if (outFilename.Exists)
          {
            outFilename.Delete();
          }
          recCompcut.Initialize(outFilename.FullName, fileList[0].FullName);
          foreach (System.IO.FileInfo file in fileList)
          {
            recCompcut.Append(file.FullName);
            joinedFiles++;
          }
          recCompcut.Close();
          percent = 100;
          if (OnFinished != null)
            OnFinished();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        DirectShowUtil.ReleaseComObject((object)recCompcut);
        recCompcut = null;
        cutPoints = null;
        percent = 0;
        newDuration = 0;
        joinProgresstime.Stop();
      }
    }

    #region Eventhandler

    /// <summary>
    /// Calculates the percentage of the join progress and fires the OnProgress-Event with that 
    /// information
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void JoinProgresstime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (fileList.Count > 0)
        percent = (100 / fileList.Count) * joinedFiles;
      if (OnProgress != null)
        OnProgress(percent);
    }

    /// <summary>
    /// Calculates the percentage of the cut progress and fires the OnProgress-Event with that 
    /// information
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CutProgresstime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      int progress = 0;
      if (recCompcut != null)
        recCompcut.GetCurrentLength(out progress);
      percent = System.Convert.ToInt32((progress * 100) / newDuration);
      if (OnProgress != null)
        OnProgress(percent);
    }

    void TranscodeProgresstime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (tompeg.IsTranscoding())
      {
        if (OnProgress != null)
          OnProgress(tompeg.Percentage());
      }
      else if (tompeg.IsFinished())
      {
        transcodeProgresstime.Stop();
        tompeg.Stop();
        if (OnFinished != null)
          OnFinished();

      }
    }

    void convertProgresstime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (toDivx.IsTranscoding())
      {
        if (OnProgress != null)
          OnProgress(toDivx.Percentage());
      }
      else if (toDivx.IsFinished())
      {
        convertProgresstime.Stop();
        toDivx.Stop();
        if (OnProgress != null)
          OnProgress(100);
        if (OnFinished != null)
          OnFinished();

      }
    }

    #endregion

    public void TranscodeToMpeg(System.IO.FileInfo inFilename, int duration)
    {
      this.inFilename = inFilename;
      newDuration = duration;
      System.Threading.Thread transcodeThread = new System.Threading.Thread(new System.Threading.ThreadStart(TranscodeToMpeg));
      transcodeThread.Start();
      //TranscodeToMpeg();
    }

    public void TranscodeToMpeg()
    {
      try
      {
        tompeg = new MediaPortal.Core.Transcoding.Dvrms2Mpeg();
        recInfo = new MediaPortal.TV.Database.TVRecorded();
        transcodeProgresstime = new System.Timers.Timer(1000);
        transcodeProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(TranscodeProgresstime_Elapsed);

        MediaPortal.Core.Transcoding.TranscodeInfo mpegInfo = new MediaPortal.Core.Transcoding.TranscodeInfo();
        mpegInfo.Author = "MediaPortal";

        if (MediaPortal.TV.Database.TVDatabase.GetRecordedTVByFilename(inFilename.FullName, ref recInfo))
        {
          mpegInfo.Channel = recInfo.Channel;
          mpegInfo.Description = recInfo.Description;
          mpegInfo.Duration = (int)(recInfo.EndTime.Subtract(recInfo.StartTime)).Seconds;
          mpegInfo.End = recInfo.EndTime;
          mpegInfo.file = recInfo.FileName;
          mpegInfo.Start = recInfo.StartTime;
          mpegInfo.Title = recInfo.Title;
        }
        else
        {
          mpegInfo.Channel = "none";
          mpegInfo.Description = "none";
          //MediaPortal.Player.g_Player.Play(inFilename.FullName);
          mpegInfo.Duration = (int)newDuration;//(int)MediaPortal.Player.g_Player.Duration;
          //MediaPortal.Player.g_Player.Stop();
          mpegInfo.file = inFilename.FullName;
          mpegInfo.Start = DateTime.Now;
          mpegInfo.End = mpegInfo.Start.AddSeconds(mpegInfo.Duration);
          mpegInfo.Title = inFilename.Name;
        }
        transcodeProgresstime.Start();
        //filetoConvert = inFilename.FullName;

        if (!tompeg.Transcode(mpegInfo, MediaPortal.Core.Transcoding.VideoFormat.Mpeg2, MediaPortal.Core.Transcoding.Quality.High, MediaPortal.Core.Transcoding.Standard.PAL))
        {
          //	titelLbl.Label = "finished";

        }
        while (!tompeg.IsFinished())
        {
          System.Threading.Thread.Sleep(500);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        //tompeg.Stop();
      }
    }

    public void ConvertToDivx(System.IO.FileInfo inFilename, int duration)
    {
      this.inFilename = inFilename;
      newDuration = duration;
      System.Threading.Thread convertThread = new System.Threading.Thread(new System.Threading.ThreadStart(ConvertToDivx));
      convertThread.IsBackground = true;
      convertThread.Priority = System.Threading.ThreadPriority.BelowNormal;
      convertThread.Start();
      //TranscodeToMpeg();
    }

    public void ConvertToDivx()
    {
      try
      {
        toDivx = new MediaPortal.Core.Transcoding.Dvrms2Divx();
        recInfo = new MediaPortal.TV.Database.TVRecorded();
        convertProgresstime = new System.Timers.Timer(1000);
        convertProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(convertProgresstime_Elapsed);
        MediaPortal.Core.Transcoding.TranscodeInfo divxInfo = new MediaPortal.Core.Transcoding.TranscodeInfo();
        divxInfo.Author = "MediaPortal";

        if (MediaPortal.TV.Database.TVDatabase.GetRecordedTVByFilename(inFilename.FullName, ref recInfo))
        {
          divxInfo.Channel = recInfo.Channel;
          divxInfo.Description = recInfo.Description;
          divxInfo.Duration = (int)(recInfo.EndTime.Subtract(recInfo.StartTime)).Seconds;
          divxInfo.End = recInfo.EndTime;
          divxInfo.file = recInfo.FileName;
          divxInfo.Start = recInfo.StartTime;
          divxInfo.Title = recInfo.Title;
        }
        else
        {
          divxInfo.Channel = "none";
          divxInfo.Description = "none";
          //MediaPortal.Player.g_Player.Play(inFilename.FullName);
          divxInfo.Duration = (int)newDuration;//(int)MediaPortal.Player.g_Player.Duration;
          //MediaPortal.Player.g_Player.Stop();
          divxInfo.file = inFilename.FullName;
          divxInfo.Start = DateTime.Now;
          divxInfo.End = divxInfo.Start.AddSeconds(divxInfo.Duration);
          divxInfo.Title = inFilename.Name;
        }

        //filetoConvert = inFilename.FullName;
        toDivx.CreateProfile(new System.Drawing.Size(360, 288), 2000, 25);
        toDivx.Transcode(divxInfo, MediaPortal.Core.Transcoding.VideoFormat.Divx, MediaPortal.Core.Transcoding.Quality.Custom, MediaPortal.Core.Transcoding.Standard.PAL);
        convertProgresstime.Start();
        while (!toDivx.IsFinished())
        {
          System.Threading.Thread.Sleep(500);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        //tompeg.Stop();
      }
    }



    public System.IO.FileInfo InFilename
    {
      get
      {
        return inFilename;
      }
      set
      {
        inFilename = value;
      }
    }

    public List<TimeDomain> CutPoints
    {
      get
      {
        return cutPoints;
      }
      set
      {
        cutPoints = value;
      }
    }
  }
}
