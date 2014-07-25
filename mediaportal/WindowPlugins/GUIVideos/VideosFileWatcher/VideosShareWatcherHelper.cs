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
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
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

    private ArrayList _notReadyFiles = new ArrayList(); // locked (not available files will be placed here until unlock)
    private Timer _scanMoviesTimer= null;
    private int _scanMoviesTimerInterval = 3 * (60 * 1000); // 5 min
    private bool _useOnlyNfoScraper;

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

        _scanMoviesTimer = new Timer();
        _scanMoviesTimer.Interval = _scanMoviesTimerInterval;
        _scanMoviesTimer.Elapsed -= new ElapsedEventHandler(OnScanMovieTimerTickEvent);
        _scanMoviesTimer.Elapsed += new ElapsedEventHandler(OnScanMovieTimerTickEvent);
        _scanMoviesTimer.Start();
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

        _scanMoviesTimer.Interval = _scanMoviesTimerInterval;
        _scanMoviesTimer.Elapsed += new ElapsedEventHandler(OnScanMovieTimerTickEvent);
        _scanMoviesTimer.Start();
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

        _scanMoviesTimer.Elapsed -= new ElapsedEventHandler(OnScanMovieTimerTickEvent);
        _scanMoviesTimer.Stop();
        Log.Info("VideosShareWatcher: Monitoring of shares disabled");
      }
    }

    public bool IsMonitoring()
    {
      return bMonitoring;
    }

    public ArrayList WatchedShares
    {
      get { return m_Shares; }
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

    protected delegate void TimerElapsedDelegate();

    private void OnScanMovieTimerTickEvent(object sender, EventArgs e)
    {
      TimerElapsedDelegate scanTimer = new TimerElapsedDelegate(ScanNewMovies);
      scanTimer.Invoke();
    }

    private void ScanNewMovies()
    {
      _scanMoviesTimer.Stop();
      // If playing video, do not do anything
      if (g_Player.Playing && g_Player.IsVideo || GUIVideoFiles.ScrapperRunning)
      {
        _scanMoviesTimer.Start();
        return;
      }

      GUIVideoFiles.ScrapperRunning = true;
      ArrayList files = new ArrayList();

      try
      {
        VideoDatabase.GetMovieQueueFiles(ref files);

        foreach (string file in files)
        {
          IMDBSilentFetcher fetcher = new IMDBSilentFetcher();

          if (fetcher.FindMovie(file))
          {
            // Send database view change
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEODATABASE_REFRESH, 0, 0, 0, 0, 0,
              null);
            GUIWindowManager.SendMessage(msg);

            // Send share view change (update item with movie data and thumb, current visible is empty)
            string strPath = file;

            if (strPath.ToUpperInvariant().Contains(@"\VIDEO_TS"))
            {
              strPath = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\VIDEO_TS"));
            }
            else if (strPath.ToUpperInvariant().Contains(@"\BDMV"))
            {
              strPath = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\BDMV"));
            }

            strPath = Path.GetDirectoryName(strPath);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEODIRECTORY_REFRESH, 0, 0, 0, 0, 0,
              strPath);
            GUIWindowManager.SendMessage(msg);
          }

          VideoDatabase.DeleteMovieQueueFile(file);
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideoShareWatcherHelper ScanNewMovies error: {0}", ex.Message);
      }

      GUIVideoFiles.ScrapperRunning = false;
      _scanMoviesTimer.Start();
    }

    // Event handler for Create of a file
    private void OnFileCreated(object source, FileSystemEventArgs e)
    {
      if (GUIVideoFiles.CheckVideoExtension(e.FullPath))
      {
        // Is file been locked before?
        if (_notReadyFiles.Contains(e.FullPath))
        {
          // Exit beacuse it will be processed by changed event
          return;
        }

        FileInfo fi = new FileInfo(e.FullPath);
        if (fi.Exists)
        {
          try
          {
            Stream s = null;
            s = fi.OpenRead();
            s.Close();
          }
          catch (IOException)
          {
            // File is locked (not copied yet), add it to blacklisted array
            _notReadyFiles.Add(e.FullPath);
            // The file is not closed yet. Ignore the event, it will be processed by the Change event
            Log.Info("VideosShareWatcher: VideoFile not ready yet: {0}", e.FullPath);
            return;
          }

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
        // Check if file is available
        FileInfo fi = new FileInfo(e.FullPath);
        if (fi.Exists)
        {
          try
          {
            Stream s = null;
            s = fi.OpenRead();
            s.Close();
          }
          catch (IOException)
          {
            return;
          }
          // Check if file was blacklisted and remove it from that list
          if (_notReadyFiles.Contains(e.FullPath))
          {
            _notReadyFiles.Remove(e.FullPath);
          }

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
    private void OnDirectoryRenamed(object source, RenamedEventArgs e)
    {
      Log.Debug("VideosShareWatcher: Rename VideoDirectory Fired: {0}", e.FullPath);
      m_Events.Add(new VideosShareWatcherEvent(VideosShareWatcherEvent.EventType.RenameDirectory, e.FullPath, e.OldFullPath));
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
            bool movieEventFired = false;

            #region Affected directories

            // Get parent directories where events occured so we can avoid multiple refreshes on
            // the same directory
            VideosShareWatcherEvent currentEvent = null;
            ArrayList refreshDir = new ArrayList();
            
            for (int i = 0; i < m_Events.Count; i++)
            {
              currentEvent = m_Events[i] as VideosShareWatcherEvent;
              
              if (currentEvent != null)
              {
                string strPath = currentEvent.FileName;

                if (!string.IsNullOrEmpty(strPath))
                {
                  // Case if change occured inside of DVD/BD folder
                  if (strPath.ToUpperInvariant().Contains(@"\VIDEO_TS"))
                  {
                    strPath = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\VIDEO_TS"));
                  }
                  else if (strPath.ToUpperInvariant().Contains(@"\BDMV"))
                  {
                    strPath = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\BDMV"));
                  }

                  strPath = Path.GetDirectoryName(strPath);
                  // Add only one copy of changed directory
                  if (strPath != null && !refreshDir.Contains(strPath))
                  {
                    refreshDir.Add(strPath);
                  }
                }
              }
            }

            #endregion

            #region Process all events

            // Process all events for videodatabase purpose (delete event only)
            // Does not fire any GUIWindowsMessage
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

                    if (!_useOnlyNfoScraper)
                    {
                      foreach (string mScanShare in m_ScanShares)
                      {
                        if (currentEvent.FileName.Contains(mScanShare))
                        {
                          VideoDatabase.AddMovieQueueFile(currentEvent.FileName);
                        }
                      }
                    }
                    break;
                  }

                  // Delete video
                  case VideosShareWatcherEvent.EventType.Delete:
                    {
                      if (!movieEventFired)
                      {
                        foreach (string mScanShare in m_ScanShares)
                        {
                          if (currentEvent.FileName.Contains(mScanShare))
                          {
                            movieEventFired = true;
                          }
                        }
                      }
                      DeleteVideo(currentEvent.FileName);
                      break;
                    }

                  // Rename video
                  case VideosShareWatcherEvent.EventType.Rename:
                    {
                      RenameVideo(currentEvent.OldFileName, currentEvent.FileName);
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
                      foreach (string mScanShare in m_ScanShares)
                      {
                        if (!movieEventFired && currentEvent.FileName.Contains(mScanShare))
                        {
                          movieEventFired = true;
                        }
                      }

                      DeleteVideoDirectory(currentEvent.FileName);
                      break;
                    }

                  // Rename directory
                  case VideosShareWatcherEvent.EventType.RenameDirectory:
                    {
                      foreach (string mScanShare in m_ScanShares)
                      {
                        if (!movieEventFired && currentEvent.FileName.Contains(mScanShare))
                        {
                          movieEventFired = true;
                        }
                      }

                      RenameVideoDirectory(currentEvent.OldFileName, currentEvent.FileName);
                      break;
                    }

                    #endregion

                }

                m_Events.RemoveAt(i);
                i--; // Don't skip next event
              }
            }

            #endregion

            #region Update MyVideos cache and send refresh message

            // Force MyVideos cache and db refresh for affected directories
            
            foreach (string directory in refreshDir)
            {
              // Update MyVideos shares cache
              if (GUIVideoFiles.CachedItems != null && GUIVideoFiles.CachedItems.ContainsKey(directory) ||
                  GUIVideoFiles.CachedItems != null && GUIVideoFiles.CachedItems.Count == 0 && GUIVideoFiles.GetCurrentFolder == directory)
              {
                GUIVideoFiles.CachedItems.Remove(directory);
                Log.Debug("VideosShareWatcher: {0} removed from video cache", directory);
                // Send message for auto refresh if user is currently in affected dir
                // For other affected directories, they will be refreshed on entry beacuse
                // cache record is removed
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEODIRECTORY_REFRESH, 0, 0, 0, 0, 0,
                      directory);
                GUIWindowManager.SendMessage(msg);
              }
            }

            #endregion

            // Send only one message for database views refresh (delete)
            if (movieEventFired)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEODATABASE_REFRESH, 0, 0, 0, 0, 0,
                           null);
              GUIWindowManager.SendMessage(msg);
            }
          }
        }
        finally
        {
          Monitor.Exit(m_EnterThread);
        }
      }
    }

    private void AddVideo(string strFilename)
    {
      Log.Info("VideosShareWatcher: Created VideoFile: {0}", strFilename);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOFILE_CREATED, 0, 0, 0, 0, 0,
                           strFilename);
      GUIWindowManager.SendMessage(msg);
    }

    private void DeleteVideo(string strFilename)
    {
      VideoDatabase.DeleteMovie(strFilename);
      Log.Info("VideosShareWatcher: Deleted VideoFile: {0}", strFilename);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOFILE_DELETED, 0, 0, 0, 0, 0,
                           strFilename);
      GUIWindowManager.SendMessage(msg);
    }

    private void RenameVideo(string oldFilename, string newFilename)
    {
      Log.Info("VideosShareWatcher: VideoFile {0} renamed to {1}", oldFilename,
                               newFilename);
      VideoDatabase.RenameFile(oldFilename, newFilename);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOFILE_RENAMED, 0, 0, 0, 0, 0,
                           (string.Format("{0}|{1}", oldFilename, newFilename)));
      GUIWindowManager.SendMessage(msg);
    }

    private void AddVideoDirectory(string strPath)
    {
      Log.Info("VideosShareWatcher: Created VideoDirectory: {0}", strPath);
    }

    private void DeleteVideoDirectory (string strPath)
    {
      string dvdFolder = string.Empty;
      string bdFolder = string.Empty;

      try
      {
        // Check if DVD/BD main folder is removed (strange case but can happen) and parent folder
        // still exist
        if (strPath.ToUpperInvariant().Contains(@"\VIDEO_TS"))
        {
          dvdFolder = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\VIDEO_TS"));
          VideoDatabase.DeleteMoviesInFolder(dvdFolder);
        }
        else if (strPath.ToUpperInvariant().Contains(@"\BDMV"))
        {
          bdFolder = strPath.Substring(0, strPath.ToUpperInvariant().IndexOf(@"\BDMV"));
          VideoDatabase.DeleteMoviesInFolder(bdFolder);
        }
        else
        {
          // Update video database
          VideoDatabase.DeleteMoviesInFolder(strPath);
        }
      }
      catch (Exception ex)
      {
        Log.Error("VideosShareWatcher: VideoDatabase update for directory {0}, error: {1}", strPath, ex.Message);
        return;
      }

      Log.Info("VideosShareWatcher: Deleted VideoDirectory: {0}", strPath);
    }

    private void RenameVideoDirectory(string oldDirectory, string newDirectory)
    {
      VideoDatabase.RenamePath(oldDirectory, newDirectory);
      Log.Info("VideosShareWatcher: VideoDirectory {0} renamed to {1}", oldDirectory,
                               newDirectory);
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

      _useOnlyNfoScraper = xmlreader.GetValueAsBool("moviedatabase", "useonlynfoscraper", false);
      //_doNotUseDatabase = xmlreader.GetValueAsBool("moviedatabase", "donotusedatabase", false);

      xmlreader = null;
      return 0;
    }

    #endregion
    
  }
}