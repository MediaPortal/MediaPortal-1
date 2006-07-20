#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
using TvDatabase;
using TvLibrary.Log;

namespace TvService
{
  /// <summary>
  /// Summary description for DiskManagement.
  /// </summary>
  public class DiskManagement
  {
    System.Timers.Timer _timer;
    public DiskManagement()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 15 * 60 * 1000;
      _timer.Enabled = false;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
    }


    List<string> GetDisks()
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

    void OnTimerElapsed(object sender, EventArgs e)
    {
      CheckFreeDiskSpace();
    }
    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      CheckFreeDiskSpace();
    }

    /// <summary>
    /// This method checks the diskspace on each harddisk
    /// if the diskspace used by recordings exceeds the disk quota set on the drive
    /// then this method will delete recordings until the disk quota is not exceeded anymore
    /// </summary>
    /// <remarks>Note, this method will run once every 15 minutes
    /// </remarks>
    void CheckFreeDiskSpace()
    {
      //check diskspace every 15 minutes...

      //first get all drives..
      List<string> drives = GetDisks();

      // next check diskspace on each drive.
      foreach (string drive in drives)
      {
        CheckDriveFreeDiskSpace(drive);
      }
    }

    bool OutOfDiskSpace(string drive)
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      ulong minimiumFreeDiskSpace = 0;

      string quotaText = layer.GetSetting("freediskspace"+ drive[0].ToString(), "51200").Value;
      minimiumFreeDiskSpace = (ulong)Int32.Parse(quotaText);
      if (minimiumFreeDiskSpace <= 51200) // 50MB
      {
        minimiumFreeDiskSpace = 51200;
      }
      minimiumFreeDiskSpace *= 1024;

      if (minimiumFreeDiskSpace <= 0) return false;
      ulong freeDiskSpace = Utils.GetFreeDiskSpace(drive);
      if (freeDiskSpace > minimiumFreeDiskSpace) return false;
      return true;
    }

    List<RecordingFileInfo> GetRecordingsOnDrive(string drive)
    {
      List<RecordingFileInfo> recordings = new List<RecordingFileInfo>();
      EntityList<Recording> recordedTvShows = DatabaseManager.Instance.GetEntities<Recording>();
      
      foreach (Recording recorded in recordedTvShows)
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
          fi.record = recorded;
          recordings.Add(fi);
        }
      }
      return recordings;
    }


    void CheckDriveFreeDiskSpace(string drive)
    {
      //get disk quota to use
      if (!OutOfDiskSpace(drive)) return;

      List<RecordingFileInfo> recordings = GetRecordingsOnDrive(drive);
      if (recordings.Count == 0) return;

      Log.Write("Recorder: not enough free space on drive:{0}.", drive);
      Log.Write("Recorder: found {0} recordings on drive:{0}", recordings.Count, drive);

      // Not enough free diskspace
      // start deleting recordings (oldest ones first)
      // until we have enough free disk space again
      recordings.Sort();
      while (OutOfDiskSpace(drive) && recordings.Count > 0)
      {
        RecordingFileInfo fi = (RecordingFileInfo)recordings[0];
        if (fi.record.KeepUntil == (int)KeepMethodType.UntilSpaceNeeded)
        {
          Log.Write( "Recorder: delete recording:{0} size:{1} date:{2} {3}",
                                              fi.filename,
                                              Utils.GetSize(fi.info.Length),
                                              fi.info.CreationTime.ToShortDateString(), fi.info.CreationTime.ToShortTimeString());
          fi.record.Delete();
        }
        recordings.RemoveAt(0);
      }//while ( OutOfDiskSpace(drive) && recordings.Count > 0)
    }

  }
}
