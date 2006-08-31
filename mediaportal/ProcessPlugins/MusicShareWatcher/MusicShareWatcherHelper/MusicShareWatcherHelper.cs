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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using SQLite.NET;
using MediaPortal.GUI.Library;
using MediaPortal.TagReader;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.Util;


namespace MediaPortal.MusicShareWatcher
{
  public class MusicShareWatcherHelper
  {
    #region Variables
    private bool bMonitoring;
    static public MusicDatabase musicDB = null;
    ArrayList m_shares = new ArrayList();
    ArrayList m_watchers = new ArrayList();
    static Thread m_CheckForVariousArtistsThread;
    #endregion


    public MusicShareWatcherHelper()
    {
      Config.LoadDirs(System.IO.Directory.GetCurrentDirectory());
      // Create Log File
      Log.BackupLogFile(Log.LogType.MusicShareWatcher);

      musicDB = new MusicDatabase();
      m_CheckForVariousArtistsThread = new Thread(new ThreadStart(CheckForVariousArtists));
      m_CheckForVariousArtistsThread.Name = "Check for Various Artists";
    }

    #region Main
    public void StartMonitor()
    {
      if (bMonitoring)
        WatchShares();
    }

    public void SetMonitoring(bool status)
    {
      if (status)
      {
        bMonitoring = true;
      }
      else
      {
        bMonitoring = false;
      }
    }

    public void ChangeMonitoring(bool status)
    {
      if (status)
      {
        bMonitoring = true;
        foreach (DelayedFileSystemWatcher watcher in m_watchers)
        {
          watcher.EnableRaisingEvents = true;
        }
        Log.Info(Log.LogType.MusicShareWatcher, "Monitoring of shares enabled");
      }
      else
      {
        bMonitoring = false;
        foreach (DelayedFileSystemWatcher watcher in m_watchers)
        {
          watcher.EnableRaisingEvents = false;
        }
        Log.Info(Log.LogType.MusicShareWatcher, "Monitoring of shares disabled");
      }
    }

    private void WatchShares()
    {
      Log.Info(Log.LogType.MusicShareWatcher, "MusicShareWatcher starting up!");
      LoadShares();
      Log.Info(Log.LogType.MusicShareWatcher, "Monitoring active for following shares:");
      Log.Info(Log.LogType.MusicShareWatcher, "---------------------------------------");
      foreach (String sharename in m_shares)
      {
        try
        {
          // Create the watchers. 
          //I need 2 type of watchers. 1 for files and the other for directories
          // Reason is that i don't know if the event occured on a file or directory.
          // For a Create / Change / Rename i could figure that out using FileInfo or DirectoryInfo,
          // but when something gets deleted, i don't know if it is a File or directory
          DelayedFileSystemWatcher watcherFile = new DelayedFileSystemWatcher();
          DelayedFileSystemWatcher watcherDirectory = new DelayedFileSystemWatcher();
          watcherFile.Path = sharename;
          watcherDirectory.Path = sharename;
          /* Watch for changes in LastWrite times, and the renaming of files or directories. */
          watcherFile.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
          watcherDirectory.NotifyFilter = NotifyFilters.DirectoryName;
          // Monitor all Files and subdirectories.
          watcherFile.Filter = "*.*";
          watcherFile.IncludeSubdirectories = true;
          watcherDirectory.Filter = "*.*";
          watcherDirectory.IncludeSubdirectories = true;

          // Add event handlers.
          watcherFile.Changed += new FileSystemEventHandler(OnChanged);
          watcherFile.Created += new FileSystemEventHandler(OnCreated);
          watcherFile.Deleted += new FileSystemEventHandler(OnDeleted);
          watcherFile.Renamed += new RenamedEventHandler(OnRenamed);
          // For directories, i'm only interested in Create, Delete and Rename events
          watcherDirectory.Created += new FileSystemEventHandler(OnDirectoryCreated);
          watcherDirectory.Deleted += new FileSystemEventHandler(OnDirectoryDeleted);
          watcherDirectory.Renamed += new RenamedEventHandler(OnRenamed);

          // Begin watching.
          watcherFile.EnableRaisingEvents = true;
          watcherDirectory.EnableRaisingEvents = true;
          m_watchers.Add(watcherFile);
          m_watchers.Add(watcherDirectory);
          Log.Info(Log.LogType.MusicShareWatcher, sharename);
        }
        catch (System.ArgumentException ex)
        {
          Log.Info(Log.LogType.MusicShareWatcher, "Unable to turn on monitoring for: " + sharename + " ( " + ex.Message + " )");
        }
      }
      Log.Info(Log.LogType.MusicShareWatcher, "---------------------------------------");
      Log.Info(Log.LogType.MusicShareWatcher, "Note: Errors reported for CD/DVD drives can be ignored.");
    }
    #endregion Main

    #region EventHandlers
    // Event handler handling the Create of a file
    private static void OnCreated(object source, FileSystemEventArgs e)
    {
      AddNewSong(e.FullPath);
    }

    // Method used by OnCreated and ScanDir to fill the song structure
    private static bool AddNewSong(string strFileName)
    {
      // Has the song already be added? 
      // This happens when a full directory is copied into the share.
      // Because the create is called twice from OnCreated and On DirectoryCreated
      if (musicDB.SongExists(strFileName))
        return false;
      // For some reason the Create is fired already by windows while the file is still copied.
      // This happens especially on large songs copied via WLAN.
      // The result is that MP Readtag is throwing an IO Exception.
      // I'm trying to open the file here and in case of an exception i'll put it on a thread to be
      // processed 5 seconds later again.
      try
      {
        FileInfo file = new FileInfo(strFileName);
        Stream s = null;
        s = file.OpenRead();
        s.Close();
      }
      catch (System.IO.IOException)
      {
        // We got an I/O exception, because the file is still copied.
        // Start the thread. I use pooling to pass the filename as a parm.
        ThreadPool.QueueUserWorkItem(new WaitCallback(DelayAddSong), strFileName);
        return false;
      }
      MusicTag tag = new MusicTag();
      tag = TagReader.TagReader.ReadTag(strFileName);
      if (tag != null)
      {
        // We got a valid file, so let's add it
        Song song = new Song();
        song.Title = tag.Title;
        song.Genre = tag.Genre;
        song.FileName = strFileName;
        song.Artist = tag.Artist;
        song.Album = tag.Album;
        song.Year = tag.Year;
        song.Track = tag.Track;
        song.Duration = tag.Duration;
        musicDB.AddSong(song, true);
        Log.Info(Log.LogType.MusicShareWatcher, "Added Song: " + strFileName);
        /// Whenever a new song is added it might happen that multiple artists are there for the same album. 
        /// Scan all albums in cache
        if (m_CheckForVariousArtistsThread.ThreadState == ThreadState.Stopped || m_CheckForVariousArtistsThread.ThreadState == ThreadState.Unstarted)
        {
          try
          {
            m_CheckForVariousArtistsThread = new Thread(new ThreadStart(CheckForVariousArtists));
            m_CheckForVariousArtistsThread.Name = "Check for Various Artists";
            m_CheckForVariousArtistsThread.Start();
          }
          catch (ThreadStateException)
          { }
        }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Delays Adding of a song, because we got an I/O Problem in the first case
    /// </summary>
    private static void DelayAddSong(object filename)
    {
      Thread.Sleep(5000);
      AddNewSong((string)filename);
    }

    /// <summary>
    /// Scan the Album cache for multiple artists
    /// Scanning is however delayed, in order to make sure that complete albums may get copied and 
    /// not checked several times.
    /// </summary>
    private static void CheckForVariousArtists()
    {
      Thread.Sleep(20000);
      musicDB.UpdateAlbumArtistsCounts(0, 0);
    }


    // Event handler handling the Create of a Directory
    private static void OnDirectoryCreated(object source, FileSystemEventArgs e)
    {
      // We need to loop now through all sub directories and issue a AddSong
      DirectoryInfo di = new DirectoryInfo(e.FullPath);
      // Delay the start of the directoryscan
      ThreadPool.QueueUserWorkItem(new WaitCallback(DelayDirScan), di);
    }

    // Delay execution of subdirectory scan, as this may cause double calls for add
    // Also not all files are recognized, when moving large directories
    private static void DelayDirScan(object di)
    {
      Thread.Sleep(5000);
      ScanDir((DirectoryInfo)di);
    }

    // Method to scan sub directories for files
    private static void ScanDir(DirectoryInfo di)
    {
      // First add all files found in this directory
      FileSystemInfo[] files = di.GetFiles();
      foreach (FileInfo newfile in files)
      {
        string strFileName = di.FullName + "\\" + newfile;
        AddNewSong(strFileName);
      }
      // Now we get all Sub directories and scan trough them as well
      FileSystemInfo[] dirs = di.GetDirectories();
      foreach (DirectoryInfo subDir in dirs)
      {
        ScanDir(subDir);
      }
    }

    // Event handler handling the Change and Delete of a file
    private static void OnChanged(object source, FileSystemEventArgs e)
    {
      FileInfo fi = new FileInfo(e.FullPath);
      // A Change event occured.
      // Was it on a file? Ignore change events on directories
      if (fi.Exists)
      {
        Song song = new Song();
        if (musicDB.GetSongByFileName(e.FullPath, ref song))
        {
          if (musicDB.UpdateSong(e.FullPath, song.songId))
            Log.Info(Log.LogType.MusicShareWatcher, "Updated Song: " + e.FullPath);
        }
        else
        {
          // The song was not found in the database, add it
          Log.Warn(Log.LogType.MusicShareWatcher, "Song was not found in database. Added it." + e.FullPath);
          AddNewSong(e.FullPath);
        }
      }
    }

    // Event handler handling the Delete of a file
    private static void OnDeleted(object source, FileSystemEventArgs e)
    {
      Log.Debug(Log.LogType.MusicShareWatcher, "Delete Song Fired: " + e.FullPath);
      musicDB.DeleteSong(e.FullPath, true);
      Log.Info(Log.LogType.MusicShareWatcher, "Deleted Song: " + e.FullPath);
    }

    // Event handler handling the Delete of a directory
    private static void OnDirectoryDeleted(object source, FileSystemEventArgs e)
    {
      Log.Debug(Log.LogType.MusicShareWatcher, "Delete Directory Fired: " + e.FullPath);
      musicDB.DeleteSongDirectory(e.FullPath);
      Log.Info(Log.LogType.MusicShareWatcher, "Deleted Directory: " + e.FullPath);
    }

    // Event handler handling the Rename of a file/directory
    private static void OnRenamed(object source, RenamedEventArgs e)
    {
      // A File has been renamed.
      if (musicDB.RenameSong(e.OldFullPath, e.FullPath))
        Log.Info(Log.LogType.MusicShareWatcher, "Song / Directory: " + e.OldFullPath + " renamed to " + e.FullPath);
    }
    #endregion EventHandlers

    #region Common Methods

    // Retrieve the Music Shares that should be monitored
    int LoadShares()
    {
      MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml");
      string strDefault = xmlreader.GetValueAsString("music", "default", String.Empty);
      for (int i = 0; i < 20; i++)
      {
        string strShareName = String.Format("sharename{0}", i);
        string strSharePath = String.Format("sharepath{0}", i);
        string shareType = String.Format("sharetype{0}", i);

        string ShareType = xmlreader.GetValueAsString("music", shareType, String.Empty);
        if (ShareType == "yes") continue;
        string SharePath = xmlreader.GetValueAsString("music", strSharePath, String.Empty);

        if (SharePath.Length > 0)
          m_shares.Add(SharePath);
      }

      xmlreader = null;
      return 0;
    }
    #endregion
  }
}
