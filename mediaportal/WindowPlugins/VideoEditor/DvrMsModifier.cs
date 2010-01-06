#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Drawing;
using System.IO;
using System.Threading;
using System.Timers;
using DirectShowLib.SBE;
using DShowNET.Helper;
using MediaPortal.Core.Transcoding;
using MediaPortal.GUI.Library;
using Timer = System.Timers.Timer;

namespace WindowPlugins.VideoEditor
{
  internal class DvrMsModifier
  {
    private IStreamBufferRecComp recCompcut = null;
    private Timer cutProgresstime;
    private Timer joinProgresstime;
    private Timer transcodeProgresstime;
    private Timer convertProgresstime;

    public delegate void Finished();

    public event Finished OnFinished;

    public delegate void Progress(int percentage);

    public event Progress OnProgress;
    private int percent = 0;
    private int joinedFiles = 0;
    private double newDuration = 0;
    private FileInfo inFilename;
    private List<TimeDomain> cutPoints;
    private List<FileInfo> fileList;
    private Dvrms2Mpeg tompeg;
    private Dvrms2Divx toDivx;
    //string filetoConvert;
    //bool inDatabase;

    public DvrMsModifier()
    {
      //cutProgresstime = new System.Timers.Timer(1000);
      //cutProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(CutProgresstime_Elapsed);
    }

    public void CutDvr(FileInfo inFilename, List<TimeDomain> cutPoints)
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
        cutProgresstime = new Timer(1000);
        cutProgresstime.Elapsed += new ElapsedEventHandler(CutProgresstime_Elapsed);
        recCompcut = (IStreamBufferRecComp)ClassId.CoCreateInstance(ClassId.RecComp);
        if (recCompcut != null)
        {
          FileInfo outFilename;
          percent = 0;
          //CutProgressTime();
          cutProgresstime.Start();
          string outPath = inFilename.FullName;
          //rename the source file ------------later this could be configurable to delete it
          //TODO behavior if the renamed sourcefile (_original) exists
          inFilename.MoveTo(inFilename.FullName.Replace(inFilename.Extension, "_original" + inFilename.Extension));
          //to not to change the database the outputfile has the same name 
          outFilename = new FileInfo(outPath);
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

            recCompcut.AppendEx(inFilename.FullName, (long)(cutPoints[i].StartTime * 10000000),
                                (long)(cutPoints[i].EndTime * 10000000));
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
          {
            OnFinished();
          }
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

    public void JoinDvr(FileInfo firstFile, FileInfo secondFile)
    {
      fileList = new List<FileInfo>(2);
      fileList.Add(firstFile);
      fileList.Add(secondFile);
      Thread joinThread = new Thread(new ThreadStart(JoinDvr));
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

    public void JoinDvr(List<FileInfo> fileList)
    {
      this.fileList = fileList;
      Thread joinThread = new Thread(new ThreadStart(JoinDvr));
      joinThread.Start();
      //JoinDvr();
    }

    public void JoinDvr()
    {
      try
      {
        joinProgresstime = new Timer(1000);
        joinProgresstime.Elapsed += new ElapsedEventHandler(JoinProgresstime_Elapsed);
        recCompcut = (IStreamBufferRecComp)ClassId.CoCreateInstance(ClassId.RecComp);
        if (recCompcut != null)
        {
          FileInfo outFilename;
          percent = 0;
          joinedFiles = 0;
          joinProgresstime.Start();
          //cutProgresstime.Start();
          string outPath = fileList[0].FullName.Replace(".dvr-ms", "_joined.dvr-ms");
          //rename the source file ------------later this could be configurable to delete it
          //TODO behavior if the renamed sourcefile (_original) exists
          //inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
          //to not to change the database the outputfile has the same name 
          outFilename = new FileInfo(outPath);


          if (outFilename.Exists)
          {
            outFilename.Delete();
          }
          recCompcut.Initialize(outFilename.FullName, fileList[0].FullName);
          foreach (FileInfo file in fileList)
          {
            recCompcut.Append(file.FullName);
            joinedFiles++;
          }
          recCompcut.Close();
          percent = 100;
          if (OnFinished != null)
          {
            OnFinished();
          }
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
    private void JoinProgresstime_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (fileList.Count > 0)
      {
        percent = (100 / fileList.Count) * joinedFiles;
      }
      if (OnProgress != null)
      {
        OnProgress(percent);
      }
    }

    /// <summary>
    /// Calculates the percentage of the cut progress and fires the OnProgress-Event with that 
    /// information
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CutProgresstime_Elapsed(object sender, ElapsedEventArgs e)
    {
      int progress = 0;
      if (recCompcut != null)
      {
        recCompcut.GetCurrentLength(out progress);
      }
      percent = Convert.ToInt32((progress * 100) / newDuration);
      if (OnProgress != null)
      {
        OnProgress(percent);
      }
    }

    private void TranscodeProgresstime_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (tompeg.IsTranscoding())
      {
        if (OnProgress != null)
        {
          OnProgress(tompeg.Percentage());
        }
      }
      else if (tompeg.IsFinished())
      {
        transcodeProgresstime.Stop();
        tompeg.Stop();
        if (OnFinished != null)
        {
          OnFinished();
        }
      }
    }

    private void convertProgresstime_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (toDivx.IsTranscoding())
      {
        if (OnProgress != null)
        {
          OnProgress(toDivx.Percentage());
        }
      }
      else if (toDivx.IsFinished())
      {
        convertProgresstime.Stop();
        toDivx.Stop();
        if (OnProgress != null)
        {
          OnProgress(100);
        }
        if (OnFinished != null)
        {
          OnFinished();
        }
      }
    }

    #endregion

    public void TranscodeToMpeg(FileInfo inFilename, int duration)
    {
      this.inFilename = inFilename;
      newDuration = duration;
      Thread transcodeThread = new Thread(new ThreadStart(TranscodeToMpeg));
      transcodeThread.Start();
      //TranscodeToMpeg();
    }

    public void TranscodeToMpeg()
    {
      try
      {
        tompeg = new Dvrms2Mpeg();
        transcodeProgresstime = new Timer(1000);
        transcodeProgresstime.Elapsed += new ElapsedEventHandler(TranscodeProgresstime_Elapsed);

        TranscodeInfo mpegInfo = new TranscodeInfo();
        mpegInfo.Author = "MediaPortal";

        mpegInfo.Channel = "none";
        mpegInfo.Description = "none";
        //MediaPortal.Player.g_Player.Play(inFilename.FullName);
        mpegInfo.Duration = (int)newDuration; //(int)MediaPortal.Player.g_Player.Duration;
        //MediaPortal.Player.g_Player.Stop();
        mpegInfo.file = inFilename.FullName;
        mpegInfo.Start = DateTime.Now;
        mpegInfo.End = mpegInfo.Start.AddSeconds(mpegInfo.Duration);
        mpegInfo.Title = inFilename.Name;
        transcodeProgresstime.Start();
        //filetoConvert = inFilename.FullName;

        if (!tompeg.Transcode(mpegInfo, VideoFormat.Mpeg2, Quality.High, Standard.PAL))
        {
          //	titelLbl.Label = "finished";
        }
        while (!tompeg.IsFinished())
        {
          Thread.Sleep(500);
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

    public void ConvertToDivx(FileInfo inFilename, int duration)
    {
      this.inFilename = inFilename;
      newDuration = duration;
      Thread convertThread = new Thread(new ThreadStart(ConvertToDivx));
      convertThread.IsBackground = true;
      convertThread.Priority = ThreadPriority.BelowNormal;
      convertThread.Start();
      //TranscodeToMpeg();
    }

    public void ConvertToDivx()
    {
      try
      {
        toDivx = new Dvrms2Divx();
        convertProgresstime = new Timer(1000);
        convertProgresstime.Elapsed += new ElapsedEventHandler(convertProgresstime_Elapsed);
        TranscodeInfo divxInfo = new TranscodeInfo();
        divxInfo.Author = "MediaPortal";

        divxInfo.Channel = "none";
        divxInfo.Description = "none";
        //MediaPortal.Player.g_Player.Play(inFilename.FullName);
        divxInfo.Duration = (int)newDuration; //(int)MediaPortal.Player.g_Player.Duration;
        //MediaPortal.Player.g_Player.Stop();
        divxInfo.file = inFilename.FullName;
        divxInfo.Start = DateTime.Now;
        divxInfo.End = divxInfo.Start.AddSeconds(divxInfo.Duration);
        divxInfo.Title = inFilename.Name;

        //filetoConvert = inFilename.FullName;
        toDivx.CreateProfile(new Size(360, 288), 2000, 25);
        toDivx.Transcode(divxInfo, VideoFormat.Divx, Quality.Custom, Standard.PAL);
        convertProgresstime.Start();
        while (!toDivx.IsFinished())
        {
          Thread.Sleep(500);
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


    public FileInfo InFilename
    {
      get { return inFilename; }
      set { inFilename = value; }
    }

    public List<TimeDomain> CutPoints
    {
      get { return cutPoints; }
      set { cutPoints = value; }
    }
  }
}