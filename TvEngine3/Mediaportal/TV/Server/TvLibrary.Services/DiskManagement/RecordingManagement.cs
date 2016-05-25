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
using System.Timers;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  public class RecordingManagement
  {
    private System.Timers.Timer _timer;

    public RecordingManagement()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 60 * 60 * 1000;
      _timer.Elapsed += OnTimerElapsed;
      _timer.Enabled = true;
    }

    ~RecordingManagement()
    {
      if (_timer != null)
      {
        _timer.Stop();
        _timer.Enabled = false;
        _timer.Dispose();
        _timer = null;
      }
    }

    /// <summary>
    /// Delete a recording completely.
    /// </summary>
    /// <remarks>
    /// Includes the actual recording, thumbnail and the corresponding database record.
    /// </remarks>
    /// <param name="recording">The recording's database record.</param>
    /// <returns><c>true</c> if the recording is deleted successfully, otherwise <c>false</c></returns>
    public static bool DeleteRecording(Recording recording)
    {
      string fileName = recording.FileName;
      Log.Info("recording management: delete recording, ID = {0}, file name = {1}", recording.IdRecording, fileName);
      TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(recording.IdRecording);
      if (DeleteRecordingOnDisk(fileName))
      {
        return true;
      }
      if (!TVDatabase.TVBusinessLayer.RecordingManagement.HasRecordingPendingDeletion(recording.FileName))
      {
        PendingDeletion pd = TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecordingPendingDeletion(new PendingDeletion { FileName = fileName });
        Log.Debug("recording management: add pending deletion, ID = {0}, file name = {1}", pd.IdPendingDeletion, fileName);
      }
      return false;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
      // Re-attempt deletion for files that we have not been able to delete previously.
      IList<PendingDeletion> pendingDeletions = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllPendingRecordingDeletions();
      foreach (var pendingDeletion in pendingDeletions)
      {
        this.LogInfo("recording management: execute pending deletion, ID = {0}, file name = {1}", pendingDeletion.IdPendingDeletion, pendingDeletion.FileName);
        if (DeleteRecordingOnDisk(pendingDeletion.FileName))
        {
          TVDatabase.TVBusinessLayer.RecordingManagement.DeletePendingRecordingDeletion(pendingDeletion.IdPendingDeletion);
        }
      }

      // Remove invalid recordings from the database and completely delete
      // recordings that don't need to be kept any longer.
      IList<Recording> recordings = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordings();
      IList<Recording> remainingRecordings = new List<Recording>(recordings.Count);
      foreach (Recording recording in recordings)
      {
        if (Directory.Exists(Path.GetPathRoot(recording.FileName)) && !File.Exists(recording.FileName))
        {
          this.LogInfo("recording management: delete invalid recording record, ID = {0}, file name = {1}", recording.IdRecording, recording.FileName);
          TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(recording.IdRecording);
          continue;
        }

        DateTime keepUntilDate = recording.KeepUntilDate.GetValueOrDefault(DateTime.MinValue);
        if (recording.KeepMethod != (int)RecordingKeepMethod.UntilDate || keepUntilDate > DateTime.Now)
        {
          remainingRecordings.Add(recording);
          continue;
        }

        this.LogInfo("recording management: delete expired recording, keep until = {0}", keepUntilDate);
        DeleteRecording(recording);
      }

      // If configured to do so, ensure that we maintain a configurable amount
      // of free space on each recording disk.
      if (SettingsManagement.GetValue("diskManagementEnable", false))
      {
        List<string> checkedDisks = new List<string>();
        foreach (string folder in GetRecordingFolders())
        {
          string disk = Path.GetPathRoot(folder);
          if (!checkedDisks.Contains(disk))
          {
            EnsureAdequateFreeDiskSpace(disk, remainingRecordings);
            checkedDisks.Add(disk);
          }
        }
      }
    }

    private void EnsureAdequateFreeDiskSpace(string disk, IList<Recording> allRecordings)
    {
      ulong spaceReservedMegaBytes = (ulong)SettingsManagement.GetValue("diskManagementReservedSpace", 1000);
      ulong spaceFreeBytes;
      ulong spaceTotalBytes;
      if (!Utils.GetFreeDiskSpace(disk, out spaceTotalBytes, out spaceFreeBytes) || spaceFreeBytes / 1000000 > spaceReservedMegaBytes)
      {
        return;
      }

      this.LogWarn("recording management: inadequate free space, disk = {0}, reserved = {1} MB, free = {2} MB", disk, spaceReservedMegaBytes, Math.Round((decimal)spaceFreeBytes / 1000000, 2));
      List<Recording> recordings = new List<Recording>(allRecordings.Count);
      foreach (Recording recording in allRecordings)
      {
        if (recording.KeepMethod == (int)RecordingKeepMethod.UntilSpaceNeeded && string.Equals(Path.GetPathRoot(recording.FileName), disk))
        {
          recordings.Add(recording);
        }
      }
      if (recordings.Count == 0)
      {
        this.LogError("DiskManagement: disk space is inadequate but no recordings can be deleted, disk = {0}", disk);
        return;
      }

      // Delete the oldest recordings until we have enough free disk space again.
      recordings.Sort(
        delegate(Recording r1, Recording r2)
        {
          return r1.StartTime.CompareTo(r2.StartTime);
        }
      );
      while (recordings.Count > 0 && Utils.GetFreeDiskSpace(disk, out spaceTotalBytes, out spaceFreeBytes) && spaceFreeBytes / 1000000 <= spaceReservedMegaBytes)
      {
        Recording recording = recordings[0];
        if (recording.KeepMethod == (int)RecordingKeepMethod.UntilSpaceNeeded)
        {
          if (!DeleteRecording(recording))
          {
            // We could end up deleting all existing recordings if we're not
            // careful. Assume that is even more undesirable than not being
            // able to complete new recordings.
            break;
          }
        }
        recordings.RemoveAt(0);
      }
    }

    private static bool DeleteRecordingOnDisk(string recordingFileName)
    {
      if (!Directory.Exists(Path.GetPathRoot(recordingFileName)))
      {
        // Share not available.
        return false;
      }

      try
      {
        string thumbnailFileName = Thumbnailer.Thumbnailer.GetThumbnailFileName(recordingFileName);
        Log.Info("  file, {0}", thumbnailFileName);
        File.Delete(thumbnailFileName);

        // Find and delete all files with same name in the recording folder.
        string directoryName = Path.GetDirectoryName(recordingFileName);
        if (Directory.Exists(directoryName))
        {
          string[] relatedFiles = Directory.GetFiles(directoryName, string.Format("{0}.*", Path.GetFileNameWithoutExtension(recordingFileName)));
          foreach (string fileName in relatedFiles)
          {
            Log.Info("  file, {0}", fileName);
            File.Delete(fileName);
          }
        }

        // Delete the containing folder and any parent folders if they're empty now.
        string recordingFolder = null;
        List<string> recordingFolders = GetRecordingFolders();
        foreach (string folder in recordingFolders)
        {
          if (directoryName.StartsWith(folder) && (recordingFolder == null || folder.Length > recordingFolder.Length))
          {
            recordingFolder = folder;
          }
        }

        // Do not attempt to delete the base recording folder (or its parents).
        while (!string.Equals(directoryName, recordingFolder) && !string.Equals(Path.GetPathRoot(directoryName), directoryName))
        {
          if (Directory.Exists(directoryName))  // If this is a pending deletion some of the path may have already been deleted.
          {
            if (Directory.GetDirectories(directoryName).Length != 0)
            {
              break;
            }

            string[] fileNames = Directory.GetFiles(directoryName);
            if (fileNames.Length > 1 || !string.Equals(fileNames[0].ToLowerInvariant(), "thumbs.db"))
            {
              break;
            }

            foreach (string fileName in fileNames)
            {
              Log.Info("  file, {0}", fileName);
              File.Delete(fileName);
            }

            Log.Info("  directory, {0}", directoryName);
            Directory.Delete(directoryName);
          }

          DirectoryInfo di = Directory.GetParent(directoryName);
          if (di == null)
          {
            break;
          }
          directoryName = di.Name;
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "recording management: failed to delete recording");
        return false;
      }
    }

    private static List<string> GetRecordingFolders()
    {
      var recordingFolders = new List<string>();
      string folder = SettingsManagement.GetValue("recordingFolder", string.Empty);
      if (!string.IsNullOrEmpty(folder))
      {
        recordingFolders.Add(folder);
      }
      return recordingFolders;
    }
  }
}