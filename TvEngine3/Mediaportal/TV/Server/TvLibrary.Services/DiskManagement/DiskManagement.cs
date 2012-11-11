#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.IO;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  /// <summary>
  /// Summary description for DiskManagement.
  /// </summary>
  public class DiskManagement
  {
 
    
    private readonly System.Timers.Timer _timer;
    

    public DiskManagement()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 15 * 60 * 1000;
      _timer.Enabled = true;
      _timer.Elapsed += _timer_Elapsed;
      this.LogDebug("DiskManagement: started");
    }


    private static IEnumerable<string> GetDisks()
    {
      var drives = new List<string>();
      IList<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      foreach (Card card in cards)
      {
        if (card.RecordingFolder.Length > 0)
        {
          string driveLetter = String.Format("{0}:", card.RecordingFolder[0]);
          if (Utils.getDriveType(driveLetter) == 3)
          {
            if (!drives.Contains(driveLetter))
            {
              drives.Add(driveLetter);
            }
          }
        }
      }
      return drives;
    }

    private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
    private static void CheckFreeDiskSpace()
    {
      //check diskspace every 15 minutes...
      
      if (SettingsManagement.GetSetting("diskQuotaEnabled", "False").Value != "True")
      {
        //Disk Quota Management disabled: quitting
        return;
      }

      Log.Debug("DiskManagement: checking free disk space");

      //first get all drives..
      IEnumerable<string> drives = GetDisks();

      // next check diskspace on each drive.
      foreach (string drive in drives)
      {
        CheckDriveFreeDiskSpace(drive);
      }
    }

    private static bool OutOfDiskSpace(string drive)
    {
      ulong minimiumFreeDiskSpace;
      string quotaText = SettingsManagement.GetSetting("freediskspace" + drive[0], "51200").Value;
      try
      {
        minimiumFreeDiskSpace = (ulong)Int32.Parse(quotaText);
      }
      catch (Exception e)
      {
        Log.Error("DiskManagement: Exception at parsing freediskspace ({0}) to drive {1}", quotaText, drive);
        Log.Error(e.ToString());
        //no setting for this drive: quitting
        return false;
      }

      if (minimiumFreeDiskSpace <= 51200) // 50MB
      {
        minimiumFreeDiskSpace = 51200;
      }

      // Kilobytes to Bytes
      minimiumFreeDiskSpace *= 1024;

      if (minimiumFreeDiskSpace <= 0)
        return false;

      ulong freeDiskSpace = Utils.GetFreeDiskSpace(drive);
      if (freeDiskSpace > minimiumFreeDiskSpace)
        return false;

      Log.Info("DiskManagement: Drive {0} is out of free space!", drive);
      Log.Info("DiskManagement: Has: {0} Minimum Set: {1}", freeDiskSpace.ToString(), minimiumFreeDiskSpace.ToString());
      return true;
    }

    private static List<RecordingFileInfo> GetRecordingsOnDrive(string drive)
    {
      var recordings = new List<RecordingFileInfo>();
      var recordedTvShows = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);

      foreach (Recording recorded in recordedTvShows)
      {
        if (recorded.FileName.ToUpperInvariant()[0] != drive.ToUpperInvariant()[0])
        {
          continue;
        }

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
          try
          {
            FileInfo info = new FileInfo(recorded.FileName);
            RecordingFileInfo fi = new RecordingFileInfo();
            fi.info = info;
            fi.filename = recorded.FileName;
            fi.record = recorded;
            recordings.Add(fi);
          }
          catch (Exception e)
          {
            Log.Error(e, "DiskManagement: Exception at building FileInfo ({0})", recorded.FileName);
          }
        }
      }
      return recordings;
    }

    private static void CheckDriveFreeDiskSpace(string drive)
    {
      //get disk quota to use
      if (!OutOfDiskSpace(drive))
        return;

      Log.Debug("DiskManagement: not enough free space on drive: {0}", drive);

      List<RecordingFileInfo> recordings = GetRecordingsOnDrive(drive);
      if (recordings.Count == 0)
      {
        Log.Debug("DiskManagement: no recordings to delete");
        return;
      }

      Log.Debug("DiskManagement: found {0} recordings", recordings.Count);

      // Not enough free diskspace
      // start deleting recordings (oldest ones first)
      // until we have enough free disk space again
      recordings.Sort();
      while (OutOfDiskSpace(drive) && recordings.Count > 0)
      {
        RecordingFileInfo fi = recordings[0];
        if (fi.record.KeepUntil == (int)KeepMethodType.UntilSpaceNeeded)
        {
          // Delete the file from disk and the recording entry from the database.          
          bool result = RecordingFileHandler.DeleteRecordingOnDisk(fi.record.FileName);
          if (result)
          {
            TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(fi.record.IdRecording);            
          }
        }
        recordings.RemoveAt(0);
      }
    }
  }
}