#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Timer = System.Timers.Timer;

namespace MediaPortal.GUI.Video
{
  public class VideosShareWatcherHelper
  {
    #region Variables

    private bool bMonitoring;
    private ArrayList m_Shares = new ArrayList();
    private ArrayList m_Watchers = new ArrayList();
    private ArrayList m_ScanShares = new ArrayList();
    
    // Lock order is _enterThread, _events.SyncRoot
    private object m_EnterThread = new object(); // Only one timer event is processed at any given moment
    private ArrayList m_Events = null;

    private Timer m_Timer = null;
    private int m_TimerInterval = 2000; // milliseconds

    #endregion

    #region Constructors/Destructors

    public VideosShareWatcherHelper()
    {
      LoadShares();
      Log.Info("VideosShareWatcher starting up!");
    }

    #endregion

    #region Main

    public void StartMonitor()
    {
      if (bMonitoring)
      {
        Log.Info("VideosShareWatcher: Starting up a worker thread...");
        Thread WorkerThread = new Thread(WatchShares);
        WorkerThread.IsBackground = true;
        WorkerThread.Name = "VideosShareWatcher";
        WorkerThread.Start();
      }
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
        foreach (DelayedVideosFileSystemWatcher watcher in m_Watchers)
        {
          watcher.EnableRaisingEvents = true;
        }
        m_Timer.Start();
        Log.Info("VideosShareWatcher: Monitoring of shares enabled");
      }
      else
      {
        bMonitoring = false;
        foreach (DelayedVideosFileSystemWatcher watcher in m_Watchers)
        {
          watcher.EnableRaisingEvents = false;
        }
        m_Timer.Stop();
        m_Events.Clear();
        Log.Info("VideosShareWatcher: Monitoring of shares disabled");
      }
    }

    public bool IsMonitoring()
    {
      return bMonitoring;
    }

    private void WatchShares()
    {
      Log.Info("VideosShareWatcher: Monitoring active for following shares:");
      Log.Info("VideosShareWatcher: ---------------------------------------");

      // Release existing FSW Objects first
      foreach (DelayedVideosFileSystemWatcher watcher in m_Watchers)
      {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
      }
      
      m_Watchers.Clear();
      
      foreach (String sharename in m_Shares)
      {
        try
        {
          m_Events = ArrayList.Synchronized(new ArrayList(64));
          // Create the watchers. 
          //I need 2 type of watchers. 1 for files and the other for directories
          // Reason is that i don't know if the event occured on a file or directory.
          // For a Create / Change / Rename i could figure that out using FileInfo or DirectoryInfo,
          // but when something gets deleted, i don't know if it is a File or directory
          DelayedVideosFileSystemWatcher watcherFile = new DelayedVideosFileSystemWatcher();
          DelayedVideosFileSystemWatcher watcherDirectory = new DelayedVideosFileSystemWatcher();
          watcherFile.Path = sharename;
          watcherDirectory.Path = sharename;
          /* Watch for changes in LastWrite times, and the renaming of files or directories. */
          watcherFile.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes;
          watcherDirectory.NotifyFilter = NotifyFilters.DirectoryName;
          // Monitor all Files and subdirectories.
          watcherFile.Filter = "*.*";
          watcherFile.IncludeSubdirectories = true;
          watcherDirectory.Filter = "*.*";
          watcherDirectory.IncludeSubdirectories = true;

          // Add event handlers.
          watcherFile.Changed += new FileSystemEventHandler(OnChanged);
          watcherFile.Created += new FileSystemEventHandler(OnFileCreated);
          watcherFile.Deleted += new FileSystemEventHandler(OnFileDeleted);
          watcherFile.Renamed += new RenamedEventHandler(OnFileRenamed);
          // For directories
          watcherDirectory.Deleted += new FileSystemEventHandler(OnDirectoryDeleted);
          watcherDirectory.Renamed += new RenamedEventHandler(OnDirectoryRenamed);
          watcherDirectory.Created += new FileSystemEventHandler(OnDirectoryCreated);
          // Begin watching.
          watcherFile.EnableRaisingEvents = true;
          watcherDirectory.EnableRaisingEvents = true;
          m_Watchers.Add(watcherFile);
          m_Watchers.Add(watcherDirectory);
          // Start Timer for processing events
          m_Timer = new Timer(m_TimerInterval);
          m_Timer.Elapsed += new ElapsedEventHandler(ProcessEvents);
          m_Timer.AutoReset = true;
          m_Timer.Enabled = watcherFile.EnableRaisingEvents;
          Log.Info("VideosShareWatcher: {0}", sharename);
        }
        catch (ArgumentException ex)
        {
          Log.Info("VideosShareWatcher: Unable to turn on monitoring for: {0} Exception: {1}", sharename,
                   ex.Message);
        }
      }
      Log.Info("VideosShareWatcher: ---------------------------------------");
      Log.Info("VideosShareWatcher: Note: Errors reported for CD/DVD drives can be ignored.");
    }

    #endregion Main

    #region EventHandlers

    // Event handler for Create of a file
    private void OnFileCreated(object source, FileSystemEventArgs e)
    {
      if (GUIVideoFiles.CheckVideoExtension(e.FullPath))
      {
        FileInfo fi = new FileInfo(e.FullPath);
        if (fi.Exists)
        {
          Log.Debug("VideosShareWatcher: Add Video Fired: {0}", e.FullPath);
          m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.Create, e.FullPath));
        }
      }
    }

    // Event handler for Change of a file
    private void OnChanged(object source, FileSystemEventArgs e)
    {
      if (GUIVideoFiles.CheckVideoExtension(e.FullPath))
      {
        FileInfo fi = new FileInfo(e.FullPath);
        if (fi.Exists)
        {
          Log.Debug("VideosShareWatcher: Change VideoFile Fired: {0}", e.FullPath);
          m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.Change, e.FullPath));
        }
      }
    }
    
    // Event handler handling the Delete of a file
    private void OnFileDeleted(object source, FileSystemEventArgs e)
    {
      if (GUIVideoFiles.CheckVideoExtension(e.FullPath))
      {
        Log.Debug("VideosShareWatcher: Delete VideoFile Fired: {0}", e.FullPath);
        m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.Delete, e.FullPath));
      }
    }

    // Event handler handling the Rename of a file
    private void OnFileRenamed(object source, RenamedEventArgs e)
    {
      if (GUIVideoFiles.CheckVideoExtension(e.FullPath))
      {
        FileInfo fi = new FileInfo(e.FullPath);
        if (fi.Exists)
        {
          Log.Debug("VideosShareWatcher: Rename VideoFile Fired: {0}", e.FullPath);
          m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.Rename, e.FullPath, e.OldFullPath));
        }
      }
    }

    // Event handler handling the Delete of a directory
    private void OnDirectoryDeleted(object source, FileSystemEventArgs e)
    {
      Log.Debug("VideosShareWatcher: Delete VideoDirectory Fired: {0}", e.FullPath);
      m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.DeleteDirectory, e.FullPath));
    }

    // Event handler for Create of a directory
    private void OnDirectoryCreated(object source, FileSystemEventArgs e)
    {
      Log.Debug("VideosShareWatcher: Create VideoDirectory Fired: {0}", e.FullPath);
      m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.CreateDirectory, e.FullPath));
    }

    // Event handler for Rename of a directory
    private void OnDirectoryRenamed(object source, FileSystemEventArgs e)
    {
      Log.Debug("VideosShareWatcher: Rename VideoDirectory Fired: {0}", e.FullPath);
      m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.RenameDirectory, e.FullPath));
    }

    #endregion EventHandlers

    #region Private Methods

    private void ProcessEvents(object sender, ElapsedEventArgs e)
    {
      // Allow only one Timer event to be executed.
      if (Monitor.TryEnter(m_EnterThread))
      {
        // Only one thread at a time is processing the events                
        try
        {
          // Lock the Collection, while processing the Events
          lock (m_Events.SyncRoot)
          {
            VideosShareWatcherEvent currentEvent = null;
            
            for (int i = 0; i < m_Events.Count; i++)
            {
              currentEvent = m_Events[i] as VideosShareWatcherEvent;
              
              if (currentEvent != null)
              {
                switch (currentEvent.Type)
                {
                  #region file events handlers

                  // Create video
                  case VideosShareWatcherEvent.EventType.Create:
                  case VideosShareWatcherEvent.EventType.Change:
                    {
                      AddVideo(currentEvent.FileName);
                      break;
                    }

                  // Delete video
                  case VideosShareWatcherEvent.EventType.Delete:
                    {
                      DeleteVideo(currentEvent.FileName);
                      break;
                    }

                  // Rename video
                  case VideosShareWatcherEvent.EventType.Rename:
                    {
                      Log.Info("VideosShareWatcher: VideoFile {0} renamed to {1]", currentEvent.OldFileName,
                               currentEvent.FileName);
                      break;
                    }

                    #endregion

                  #region directory events handlers

                  // Create directory
                  case VideosShareWatcherEvent.EventType.CreateDirectory:
                    {
                      AddVideoDirectory(currentEvent.FileName);
                      break;
                    }

                  // Delete directory
                  case VideosShareWatcherEvent.EventType.DeleteDirectory:
                    {
                      DeleteVideoDirectory(currentEvent.FileName);
                      break;
                    }

                  // Rename directory
                  case VideosShareWatcherEvent.EventType.RenameDirectory:
                    {
                      Log.Info("VideosShareWatcher: VideoDirectory {0} renamed to {1]", currentEvent.OldFileName,
                               currentEvent.FileName);
                      break;
                    }

                    #endregion

                }

                m_Events.RemoveAt(i);
                i--; // Don't skip next event
              }
            }
          }
        }
        finally
        {
          Monitor.Exit(m_EnterThread);
        }
      }
    }

    private void AddVideo(string strFileName)
    {
      try
      {
        FileInfo file = new FileInfo(strFileName);
        Stream s = null;
        s = file.OpenRead();
        s.Close();
      }
      catch (IOException)
      {
        // The file is not closed yet. Ignore the event, it will be processed by the Change event
        Log.Info("VideosShareWatcher: VideoFile not ready yet: {0}", strFileName);
        return;
      }

      try
      {
        // Force refresh for parent directory of deleted file
        string deletedFileDirectory = Path.GetDirectoryName(strFileName);

        if (GUIVideoFiles.CachedItems != null)
        {
          if (deletedFileDirectory != null)
          {
            GUIVideoFiles.CachedItems.Remove(deletedFileDirectory);
            Log.Debug("VideosShareWatcher: {0} removed from video cache", deletedFileDirectory);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideosShareWatcher cache update for file {0}, error {1}", strFileName, ex.Message);
        return;
      }

      int isScanShare = 0;
      foreach (string share in m_ScanShares)
      {
        if (Util.Utils.AreEqual(share, strFileName))
        {
          isScanShare = 1;
          break;
        }
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOFILE_CREATED, 0, 0, 0, 0, isScanShare,
                           strFileName);
      GUIWindowManager.SendMessage(msg);
      Log.Info("VideosShareWatcher: Created VideoFile: {0}", strFileName);
    }

    private void DeleteVideo(string strFilename)
    {
      try
      {
        // Update video database
        VideoDatabase.DeleteMovie(strFilename);
        // Force refresh for parent directory of deleted file
        string deletedFileDirectory = Path.GetDirectoryName(strFilename);

        if (GUIVideoFiles.CachedItems != null)
        {
          if (deletedFileDirectory != null)
          {
            GUIVideoFiles.CachedItems.Remove(deletedFileDirectory);
            Log.Debug("VideosShareWatcher: {0} removed from video cache", deletedFileDirectory);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideosShareWatcher cache update for file {0}, error {1}", strFilename, ex.Message);
        return;
      }
      
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOFILE_DELETED, 0, 0, 0, 0, 0,
                            strFilename);
      GUIWindowManager.SendMessage(msg);
      Log.Info("VideosShareWatcher: Deleted VideoFile: {0}", strFilename);
    }

    private void AddVideoDirectory(string strPath)
    {
      bool isDvd = false;
      bool isBD = false;
      string dvdFolder = string.Empty;
      string bdFolder = string.Empty;

      try
      {
        if (strPath.ToUpperInvariant().Contains(@"\VIDEO_TS"))
        {
          dvdFolder = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\VIDEO_TS"));
          dvdFolder = Path.GetDirectoryName(dvdFolder);
          isDvd = true;
        }
        else if (strPath.ToUpperInvariant().Contains(@"\BDMV"))
        {
          bdFolder = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\BDMV"));
          bdFolder = Path.GetDirectoryName(bdFolder);
          isBD = true;
        }

        // Update MyVideos shares cache
        if (GUIVideoFiles.CachedItems != null)
        {
          // Get all cached directories
          ArrayList keys = new ArrayList(GUIVideoFiles.CachedItems.Keys);
          string parentDir = Path.GetDirectoryName(strPath);

          foreach (string key in keys)
          {
            // Find if created directory parent path exist in cache
            if (key == parentDir || isBD && key == bdFolder || isDvd && key == dvdFolder)
            {
              // And force refresh by deleting cached parent directory
              if (key != null)
              {
                GUIVideoFiles.CachedItems.Remove(key);
                Log.Debug("VideosShareWatcher: {0} removed from video cache", key);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideosShareWatcher cache update for directory {0}, error {1}", strPath, ex.Message);
        return;
      }

      if (isBD)
      {
        strPath = bdFolder;
      }
      else if (isDvd)
      {
        strPath = dvdFolder;
      }

      int isScanShare = 0;
      foreach (string share in m_ScanShares)
      {
        if (Util.Utils.AreEqual(share, strPath))
        {
          isScanShare = 1;
          break;
        }
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEODIRECTORY_CREATED, 0, 0, 0, 0, isScanShare,
                           strPath);
      GUIWindowManager.SendMessage(msg);
      Log.Info("VideosShareWatcher: Created VideoDirectory: {0}", strPath);
    }

    private void DeleteVideoDirectory (string strPath)
    {
      bool isDvd = false;
      bool isBD = false;
      string dvdFolder = string.Empty;
      string bdFolder = string.Empty;

      try
      {
        if (strPath.ToUpperInvariant().Contains(@"\VIDEO_TS"))
        {
          dvdFolder = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\VIDEO_TS"));
          VideoDatabase.DeleteMoviesInFolder(dvdFolder);
          dvdFolder = Path.GetDirectoryName(dvdFolder);
          isDvd = true;
        }
        else if (strPath.ToUpperInvariant().Contains(@"\BDMV"))
        {
          bdFolder = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\BDMV"));
          VideoDatabase.DeleteMoviesInFolder(bdFolder);
          bdFolder = Path.GetDirectoryName(bdFolder);
          isBD = true;
        }
        else
        {
          // Update video database
          VideoDatabase.DeleteMoviesInFolder(strPath);
        }

        // Update MyVideos shares cache
        if (GUIVideoFiles.CachedItems != null)
        {
          // Get all cached directories
          ArrayList keys = new ArrayList(GUIVideoFiles.CachedItems.Keys);
          string parentDir = Path.GetDirectoryName(strPath);

          foreach (string key in keys)
          {
            // Get all cached directories which contains deleted directory (not parent)
            if (key.Contains(strPath) || key == parentDir || isBD && key == bdFolder || isDvd && key == dvdFolder)
            {
              Log.Debug("VideosShareWatcher: {0} removed from video cache", key);
              GUIVideoFiles.CachedItems.Remove(key);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideosShareWatcher cache update for directory {0}, error {1}", strPath, ex.Message);
        return;
      }

      if (isBD)
      {
        strPath = bdFolder;
      }
      else if (isDvd)
      {
        strPath = dvdFolder;
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEODIRECTORY_DELETED, 0, 0, 0, 0, 0,
                           strPath);
      GUIWindowManager.SendMessage(msg);
      Log.Info("VideosShareWatcher: Deleted VideoDirectory: {0}", strPath);
    }
    
    #endregion

    #region Common Methods

    // Retrieve the Videos Shares that should be monitored
    private int LoadShares()
    {
      MPSettings xmlreader = new MPSettings();

      for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
      {
        bool isScanShare = false;
        string strSharePath = String.Format("sharepath{0}", i);
        string shareType = String.Format("sharetype{0}", i);
        string shareScan = String.Format("sharescan{0}", i);

        string ShareType = xmlreader.GetValueAsString("movies", shareType, string.Empty);
        if (ShareType == "yes")
        {
          continue; // We can't monitor ftp shares
        }

        bool shareScanData = xmlreader.GetValueAsBool("movies", shareScan, true);
        if (shareScanData)
        {
          isScanShare = true;
        }

        string sharePath = xmlreader.GetValueAsString("movies", strSharePath, string.Empty);

        if (sharePath.Length > 0 && !Util.Utils.IsDVD(sharePath))
        {
          m_Shares.Add(sharePath);

          if (isScanShare)
          {
            m_ScanShares.Add(sharePath);
          }
        }
      }

      xmlreader = null;
      return 0;
    }

    #endregion
    
  }
}