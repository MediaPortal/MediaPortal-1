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
    static DiskManagement()
    {
    }

    #region recording/timeshift file delete methods

    static public void DeleteRecording(TVRecorded rec)
    {
      DiskManagement.DeleteRecording(rec.FileName);
      TVDatabase.RemoveRecordedTV(rec);
      VideoDatabase.DeleteMovie(rec.FileName);
      VideoDatabase.DeleteMovieInfo(rec.FileName);
    }

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


    static List<string> GetDisks()
    {
      List<string> drives = new List<string>();
      for (char drive = 'a'; drive <= 'z'; drive++)
      {
        string driveLetter = String.Format("{0}:", drive);
        if (Utils.getDriveType(driveLetter) == 3)
        {
          bool newDrive = true;
          foreach (string tmpDrive in drives)
          {
            if (String.Compare(drive.ToString(), tmpDrive, true) == 0)
            {
              newDrive = false;
            }
          }
          if (newDrive)
            drives.Add(drive.ToString());
        }
      }
      return drives;
    }

    static public void ResetTimer()
    {
      _diskSpaceCheckTimer = DateTime.MinValue;
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
      List<string> drives = GetDisks();

      // next check diskspace on each drive.
      foreach (string drive in drives)
      {
        CheckDriveFreeDiskSpace(drive);
      }
    }

    static void CheckDriveFreeDiskSpace(string drive)
    {
      //get disk quota to use
      ulong minimiumFreeDiskSpace = 0;
      using (MediaPortal.Profile.Xml xmlReader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        string quotaText = xmlReader.GetValueAsString("freediskspace", drive[0].ToString(), "0");
        minimiumFreeDiskSpace = (ulong)Int32.Parse(quotaText);
        if (minimiumFreeDiskSpace <= 0) return;
        minimiumFreeDiskSpace *= 1024;
      }
      if (minimiumFreeDiskSpace <= 0) return;
      ulong freeDiskSpace = Utils.GetFreeDiskSpace(drive);
      if (freeDiskSpace > minimiumFreeDiskSpace) return;

      List<RecordingFileInfo> recordings = new List<RecordingFileInfo>();
      List<TVRecorded> recordedTvShows = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordedTvShows);
      foreach (TVRecorded recorded in recordedTvShows)
      {
        if (recorded.FileName.ToLower()[0] != drive.ToLower()[0]) continue;

        bool add = true;
        foreach (RecordingFileInfo fi in recordings)
        {
          if (String.Compare(fi.filename, recorded.FileName, true) == 0)
          {
            add = false;
          }
        }
        if (add)
        {
          FileInfo info = new FileInfo(recorded.FileName);
          RecordingFileInfo fi = new RecordingFileInfo();
          fi.info = info;
          fi.filename = recorded.FileName;
          recordings.Add(fi);
        }
      }
      

      //calculate disk space currently used by recordings.
      long diskSpaceUsedByRecordings = 0;
      foreach (RecordingFileInfo info in recordings)
      {
        diskSpaceUsedByRecordings += info.info.Length;
      }

      Log.WriteFile(Log.LogType.Recorder, "Recorder: not enough free space on drive:{0}. Currently free:{1}", drive, Utils.GetSize((long)freeDiskSpace));
      Log.WriteFile(Log.LogType.Recorder, "Recorder:   {0} recordings use {1} while minimum free space is {2}",
                                            recordings.Count, 
                                            Utils.GetSize(diskSpaceUsedByRecordings), 
                                            Utils.GetSize((long)minimiumFreeDiskSpace));

      // Not enough free diskspace
      // start deleting recordings (oldest ones first)
      // until we have enough free disk space again
      recordings.Sort();
      while (freeDiskSpace < minimiumFreeDiskSpace && recordings.Count > 0)
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
            TVDatabase.RemoveRecordedTV(tvrec);
            DeleteRecording(fi.filename);
            VideoDatabase.DeleteMovie(fi.filename);
            VideoDatabase.DeleteMovieInfo(fi.filename);
          }
          freeDiskSpace = Utils.GetFreeDiskSpace(drive);
          break;
        }//foreach (TVRecorded tvrec in tvrecs)
        recordings.RemoveAt(0);
      }//while (diskSpaceUsedByRecordings > m_minimiumFreeDiskSpace && files.Count>0)
    }

  }
}
