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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Configuration;

namespace ProcessPlugins.DiskSpace
{
  /// <summary>
  /// Summary description for DiskManagement.
  /// </summary>
  public class DiskManagement : IPlugin, ISetupForm
  {
    System.Windows.Forms.Timer _timer;
    public DiskManagement()
    {
      _timer = new System.Windows.Forms.Timer();
      _timer.Interval = 15 * 60 * 1000;
      _timer.Enabled = false;
      _timer.Tick += new EventHandler(OnTimerElapsed);
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
      ulong minimiumFreeDiskSpace = 0;
      using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string quotaText = xmlReader.GetValueAsString("freediskspace", drive[0].ToString(), "51200");
        minimiumFreeDiskSpace = (ulong)Int32.Parse(quotaText);
        if (minimiumFreeDiskSpace <= 51200) // 50MB
        {
          minimiumFreeDiskSpace = 51200;
        }
        minimiumFreeDiskSpace *= 1024;
      }
      if (minimiumFreeDiskSpace <= 0) return false;
      ulong freeDiskSpace = Utils.GetFreeDiskSpace(drive);
      if (freeDiskSpace > minimiumFreeDiskSpace) return false;
      return true;
    }

    List<RecordingFileInfo> GetRecordingsOnDrive(string drive)
    {
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

      List<RecordingFileInfo> recordings = null;

      try
      {
        recordings = GetRecordingsOnDrive(drive);
      }
      catch (Exception ex)
      {
        Log.Error("DiskManagement: An error occured while out of diskspace getting info about recordings - {0}", ex.Message);
      }

      if (recordings.Count == 0) return;

      Log.Warn("DiskManagement: not enough free space on drive: {0}.", drive);
      Log.Warn("DiskManagement: found {0} recordings on drive: {1}", recordings.Count, drive);

      // Not enough free diskspace
      // start deleting recordings (oldest ones first)
      // until we have enough free disk space again
      recordings.Sort();
      while (OutOfDiskSpace(drive) && recordings.Count > 0)
      {
        try
        {
          RecordingFileInfo fi = (RecordingFileInfo)recordings[0];
          if (fi.record.KeepRecordingMethod == TVRecorded.KeepMethod.UntilSpaceNeeded)
          {
            Log.Info("Recorder: delete recording:{0} size:{1} date:{2} {3}",
                                                fi.filename,
                                                Utils.GetSize(fi.info.Length),
                                                fi.info.CreationTime.ToShortDateString(), fi.info.CreationTime.ToShortTimeString());
            Recorder.DeleteRecording(fi.record);
          }
        }
        catch (Exception ex)
        {
          Log.Error("DiskManagement: An error occured while out of diskspace deleting a record: {0}", ex.Message);
        }
        finally
        {
          recordings.RemoveAt(0);
        }
      }//while ( OutOfDiskSpace(drive) && recordings.Count > 0)
    }

    #region IPlugin Members

    public void Start()
    {
      _timer.Enabled = true;
    }

    public void Stop()
    {
      _timer.Enabled = false;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Deletes old TV recordings according to the defined quota when there is not enough free disk space";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      // TODO:  Add CallerIdPlugin.GetWindowId implementation
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add CallerIdPlugin.GetHome implementation
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "TV Diskspace";
    }

    public bool HasSetup()
    {
      // TODO:  Add CallerIdPlugin.HasSetup implementation
      return false;
    }

    public void ShowPlugin()
    {
      // TODO:  Add CallerIdPlugin.ShowPlugin implementation
    }

    #endregion
  }
}