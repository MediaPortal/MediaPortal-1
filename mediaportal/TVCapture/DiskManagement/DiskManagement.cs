/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.DiskSpace
{
  /// <summary>
  /// Summary description for DiskManagement.
  /// </summary>
  public class DiskManagement
  {
    static DateTime _diskSpaceCheckTimer = DateTime.MinValue;
    static DateTime _deleteOldRecordingTimer = DateTime.MinValue;
    static  DiskManagement()
    {
      Recorder.OnTvRecordingEnded += new MediaPortal.TV.Recording.Recorder.OnTvRecordingHandler(DiskManagement.Recorder_OnTvRecordingEnded);
    }

    #region recording/timeshift file delete methods
    static public void DeleteRecording(string recordingFilename)
    {
      Utils.FileDelete(recordingFilename);

      int pos = recordingFilename.LastIndexOf(@"\");
      if (pos < 0) return;
      string path = recordingFilename.Substring(0, pos);
      string filename = recordingFilename.Substring(pos + 1);
      pos = filename.LastIndexOf(".");
      if (pos >= 0)
        filename = filename.Substring(0, pos);
      filename = filename.ToLower();
      string[] files;
      try
      {
        files = System.IO.Directory.GetFiles(path);
        foreach (string fileName in files)
        {
          try
          {
            if (fileName.ToLower().IndexOf(filename) >= 0)
            {
              if (fileName.ToLower().IndexOf(".sbe") >= 0)
              {
                System.IO.File.Delete(fileName);
              }
            }
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }
    static public void DeleteOldTimeShiftFiles(string path)
    {
      if (path == null) return;
      if (path == String.Empty) return;
      // Remove any trailing slashes
      path = Utils.RemoveTrailingSlash(path);


      // clean the TempDVR\ folder
      string directory = String.Empty;
      string[] files;
      try
      {
        directory = String.Format(@"{0}\TempDVR", path);
        files = System.IO.Directory.GetFiles(directory, "*.tmp");
        foreach (string fileName in files)
        {
          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }

      // clean the TempSBE\ folder
      try
      {
        directory = String.Format(@"{0}\TempSBE", path);
        files = System.IO.Directory.GetFiles(directory, "*.tmp");
        foreach (string fileName in files)
        {
          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }

      // delete *.tv
      try
      {
        directory = String.Format(@"{0}", path);
        files = System.IO.Directory.GetFiles(directory, "*.tv");
        foreach (string fileName in files)
        {
          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }//static void DeleteOldTimeShiftFiles(string path)
    #endregion    

    #region diskmanagement
    /// <summary>
    /// This method will get all the tv-recordings present in the tv database
    /// For each recording it looks at the Keep until settings. If the recording should be
    /// deleted by date, then it will delete the recording from the database, and harddisk
    /// if the the current date > keep until date
    /// </summary>
    /// <remarks>Note, this method will only work after a day-change has occured(and at startup)
    /// </remarks>
    static public void DeleteOldRecordings()
    {
      if (DateTime.Now.Date == _deleteOldRecordingTimer.Date) return;
      _deleteOldRecordingTimer = DateTime.Now;

      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        if (rec.KeepRecordingMethod != TVRecorded.KeepMethod.TillDate) continue;
        if (rec.KeepRecordingTill.Date > DateTime.Now.Date) continue;

        Log.WriteFile(Log.LogType.Recorder, "Recorder: delete old recording:{0} date:{1}",
                          rec.FileName,
                          rec.StartTime.ToShortDateString());

        DeleteRecording(rec.FileName);
        TVDatabase.RemoveRecordedTV(rec);
        VideoDatabase.DeleteMovie(rec.FileName);
        VideoDatabase.DeleteMovieInfo(rec.FileName);
      }
    }

    static List<string> GetRecordingDisks()
    {
      List<string> drives = new List<string>();
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (dev.RecordingPath == null) continue;
        if (dev.RecordingPath.Length < 2) continue;
        string drive = dev.RecordingPath.Substring(0, 2);
        bool newDrive = true;
        foreach (string tmpDrive in drives)
        {
          if (String.Compare(drive, tmpDrive, true) == 0)
          {
            newDrive = false;
          }
        }
        if (newDrive) 
          drives.Add(drive);
      }
      return drives;
    }

    /// <summary>
    /// This method checks the diskspace on each harddisk
    /// if the diskspace used by recordings exceeds the disk quota set on the drive
    /// then this method will delete recordings until the disk quota is not exceeded anymore
    /// </summary>
    /// <remarks>Note, this method will run once every 15 minutes
    /// </remarks>
    static public void CheckFreeDiskSpace()
    {
      //check diskspace every 15 minutes...
      TimeSpan ts = DateTime.Now - _diskSpaceCheckTimer;
      if (ts.TotalMinutes < 15) return; 
      _diskSpaceCheckTimer = DateTime.Now;

      //first get all drives..
      List<string> drives = GetRecordingDisks();

      // next check diskspace on each drive.
      foreach (string drive in drives)
      {
        CheckDriveSpace(drive);      
      }
    }
    static long GetDiskSize(string drive)
    {
      long diskSize = 0;
      try
      {
        string cmd = String.Format("win32_logicaldisk.deviceid=\"{0}:\"", drive[0]);
        using (ManagementObject disk = new ManagementObject(cmd))
        {
          disk.Get();
          diskSize = Int64.Parse(disk["Size"].ToString());
        }
      }
      catch (Exception)
      {
        return -1;
      }
      return diskSize;
    }

    static void CheckDriveSpace(string drive)
    {
      float diskSize = (float)GetDiskSize(drive);
      if (diskSize < 0) return;

      //get disk quota to use
      long recordingDiskQuota = 0;
      List<RecordingFileInfo> recordings = new List<RecordingFileInfo>();
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (!dev.RecordingPath.ToLower().StartsWith(drive.ToLower())) continue;
        dev.GetRecordings(drive, ref recordings);

        float percentage = (float)dev.MaxSizeLimit;
        percentage /= 100.0f;
        float cardLimitQuota = diskSize * percentage;
        if (cardLimitQuota > recordingDiskQuota)
          recordingDiskQuota = (long)cardLimitQuota;
      }//foreach (TVCaptureDevice dev in m_tvcards)

      if (recordingDiskQuota <= 0) return;

      //calculate disk space currently used by recordings.
      long diskSpaceUsedByRecordings = 0;
      foreach (RecordingFileInfo info in recordings)
      {
        diskSpaceUsedByRecordings += info.info.Length;
      }
      if (diskSpaceUsedByRecordings < recordingDiskQuota) return;

      Log.WriteFile(Log.LogType.Recorder, "Recorder: exceeded diskspace quota for recordings on drive:{0}", drive);
      Log.WriteFile(Log.LogType.Recorder, "Recorder:   {0} recordings contain {1} while limit is {2}",
                                            recordings.Count, Utils.GetSize(diskSpaceUsedByRecordings), Utils.GetSize((long)recordingDiskQuota));

      // we exceeded the disk spacee quota
      // start deleting recordings (oldest ones first)
      // until we have enough free disk space again
      recordings.Sort();
      while (diskSpaceUsedByRecordings > recordingDiskQuota && recordings.Count > 0)
      {
        RecordingFileInfo fi = (RecordingFileInfo)recordings[0];
        List<TVRecorded> tvrecs = new List<TVRecorded>();
        TVDatabase.GetRecordedTV(ref tvrecs);
        foreach (TVRecorded tvrec in tvrecs)
        {
          if (String.Compare(tvrec.FileName, fi.filename, true) != 0) continue;
          if (tvrec.KeepRecordingMethod != TVRecorded.KeepMethod.UntilSpaceNeeded) continue;
          Log.WriteFile(Log.LogType.Recorder, "Recorder: delete old recording:{0} size:{1} date:{2} {3}",
                                              fi.filename,
                                              Utils.GetSize(fi.info.Length),
                                              fi.info.CreationTime.ToShortDateString(), fi.info.CreationTime.ToShortTimeString());
          if (Utils.FileDelete(fi.filename))
          {
            diskSpaceUsedByRecordings -= fi.info.Length;
            TVDatabase.RemoveRecordedTV(tvrec);
            DeleteRecording(fi.filename);
            VideoDatabase.DeleteMovie(fi.filename);
            VideoDatabase.DeleteMovieInfo(fi.filename);
          }
          break;
        }//foreach (TVRecorded tvrec in tvrecs)
        recordings.RemoveAt(0);
      }//while (diskSpaceUsedByRecordings > m_recordingDiskQuota && files.Count>0)
    }

    #endregion

    #region episode disk management
    static private void Recorder_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(Log.LogType.Recorder, "diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
          recording.Title,recording.RecType.ToString(), recording.EpisodesToKeep);

      if (recording.EpisodesToKeep == Int32.MaxValue) return;
      if (recording.RecType == TVRecording.RecordingType.Once) return;

      //check how many episodes we got
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      while (true)
      {
        Log.WriteFile(Log.LogType.Recorder, "got:{0} recordings", recordings.Count);
        int recordingsFound = 0;
        DateTime oldestRecording = DateTime.MaxValue;
        string oldestFileName = String.Empty;
        TVRecorded oldestRec = null;
        foreach (TVRecorded rec in recordings)
        {
          Log.WriteFile(Log.LogType.Recorder, "check:{0}", rec.Title);
          if (String.Compare(rec.Title,recording.Title,true)==0)
          {
            recordingsFound++;
            if (rec.StartTime < oldestRecording)
            {
              oldestRecording = rec.StartTime;
              oldestFileName = rec.FileName;
              oldestRec = rec;
            }
          }
        }
        Log.WriteFile(Log.LogType.Recorder, "diskmanagement:   total episodes now:{0}", recordingsFound);
        if (oldestRec!=null)
        {
          Log.WriteFile(Log.LogType.Recorder, "diskmanagement:   oldest episode:{0} {1}", oldestRec.StartTime.ToShortDateString(), oldestRec.StartTime.ToLongTimeString() );
        }

        if (oldestRec == null) return;
        if (recordingsFound == 0) return;
        if (recordingsFound <= recording.EpisodesToKeep) return;
        Log.WriteFile(Log.LogType.Recorder, false, "diskmanagement:   Delete episode {0} {1} {2} {3}",
                             oldestRec.Channel,
                             oldestRec.Title,
                             oldestRec.StartTime.ToLongDateString(),
                             oldestRec.StartTime.ToLongTimeString());

        if (Utils.FileDelete(oldestFileName))
        {
          DeleteRecording(oldestFileName);

          VideoDatabase.DeleteMovie(oldestFileName);
          VideoDatabase.DeleteMovieInfo(oldestFileName);
          recordings.Remove(oldestRec);
          TVDatabase.RemoveRecordedTV(oldestRec);
        }
      }
    }
    #endregion
  }
}
