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
using System.Collections;
using System.IO;
using System.Threading;
using System.Timers;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Util;
using Common.GUIPlugins;
using Timer = System.Timers.Timer;

namespace MediaPortal.GUI.Music
{
  public class MusicFolderWatcherHelper
  {
    #region Variables

    private bool bMonitoring;
    private ArrayList _Watchers = new ArrayList();
    private string _currentFolder = string.Empty;
    private object _EnterThread = new object(); // Only one timer event is processed at any given moment
    private ArrayList _Events = null;
    private Timer _Timer = null;
    private int _TimerInterval = 1000; // milliseconds
    private ArrayList _notReadyFiles = new ArrayList(); // locked (not available files will be placed here until unlock)

    #endregion

    #region Constructors/Destructors

    public MusicFolderWatcherHelper(string directory)
    {
      if (!Directory.Exists(directory))
        return;

      _currentFolder = directory;
    }

    #endregion

    #region Main

    public void StartMonitor()
    {
      if (bMonitoring)
      {
        Thread WorkerThread = new Thread(WatchFolders);
        WorkerThread.IsBackground = true;
        WorkerThread.Name = "MusicFolderWatcher";
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
        foreach (DelayedFileSystemWatcher watcher in _Watchers)
        {
          watcher.EnableRaisingEvents = true;
        }
        if (_Timer != null)
        {
          _Timer.Start();
        }
      }
      else
      {
        bMonitoring = false;
        foreach (DelayedFileSystemWatcher watcher in _Watchers)
        {
          watcher.EnableRaisingEvents = false;
        }
        if (_Timer != null)
        {
          _Timer.Stop();
        }
        _Events.Clear();
      }
    }

    private void WatchFolders()
    {
      // Release existing FSW Objects first
      foreach (DelayedFileSystemWatcher watcher in _Watchers)
      {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
      }
      _Watchers.Clear();

      try
      {
        _Events = ArrayList.Synchronized(new ArrayList(64));
        // Create the watchers. 
        //I need 2 type of watchers. 1 for files and the other for directories
        // Reason is that i don't know if the event occured on a file or directory.
        // For a Create / Change / Rename i could figure that out using FileInfo or DirectoryInfo,
        // but when something gets deleted, i don't know if it is a File or directory
        DelayedFileSystemWatcher watcherFile = new DelayedFileSystemWatcher();
        DelayedFileSystemWatcher watcherDirectory = new DelayedFileSystemWatcher();
        watcherFile.Path = _currentFolder;
        watcherDirectory.Path = _currentFolder;
        /* Watch for changes in LastWrite times, and the renaming of files or directories. */
        watcherFile.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes;
        watcherDirectory.NotifyFilter = NotifyFilters.DirectoryName;

        watcherFile.Filter = "*.*";
        watcherFile.IncludeSubdirectories = false;
        watcherDirectory.Filter = "*.*";
        watcherDirectory.IncludeSubdirectories = false;

        // Add event handlers.
        watcherFile.Changed += new FileSystemEventHandler(OnChanged);
        watcherFile.Created += new FileSystemEventHandler(OnCreated);
        watcherFile.Deleted += new FileSystemEventHandler(OnDeleted);
        watcherFile.Renamed += new RenamedEventHandler(OnRenamed);

        watcherDirectory.Deleted += new FileSystemEventHandler(OnDirectoryDeleted);
        watcherDirectory.Renamed += new RenamedEventHandler(OnDirectoryRenamed);
        watcherDirectory.Created += new FileSystemEventHandler(OnDirectoryCreated);

        // Begin watching.
        watcherFile.EnableRaisingEvents = true;
        watcherDirectory.EnableRaisingEvents = true;
        _Watchers.Add(watcherFile);
        _Watchers.Add(watcherDirectory);

        // Start Timer for processing events
        _Timer = new Timer(_TimerInterval);
        _Timer.Elapsed += new ElapsedEventHandler(ProcessEvents);
        _Timer.AutoReset = true;
        _Timer.Enabled = watcherFile.EnableRaisingEvents;
      }
      catch (ArgumentException ex)
      {
        Log.Error("MusicFolderWatcher Unable to turn on monitoring for: {0} Exception: {1}", _currentFolder,
                  ex.Message);
      }
    }

    #endregion Main

    #region EventHandlers

    // Event handler for Create of a file
    private void OnCreated(object source, FileSystemEventArgs e)
    {
      if (Util.Utils.IsAudio(e.FullPath))
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
            Log.Info("MusicFolderWatcher: File not ready yet: {0}", e.FullPath);
            return;
          }
          _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Create, e.FullPath));
          Log.Debug("MusicFolderWatcher Add File Fired: {0}", e.FullPath);
        }
      }
    }

    // Event handler for Change of a file
    private void OnChanged(object source, FileSystemEventArgs e)
    {
      FileInfo fi = new FileInfo(e.FullPath);
      // A Change event occured.
      // Was it on a file? Ignore change events on directories
      if (fi.Exists && Util.Utils.IsAudio(e.FullPath))
      {
        // Check if file is available
        try
        {
          Stream s = null;
          s = fi.OpenRead();
          s.Close();
        }
        catch (IOException)
        {
          return; // file is not ready yet
        }

        // Check if file was blacklisted and remove it from that list
        if (_notReadyFiles.Contains(e.FullPath))
        {
          _notReadyFiles.Remove(e.FullPath);
        }

        Log.Debug("MusicFolderWatcher Change File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Change, e.FullPath));
      }
    }

    // Event handler handling the Delete of a file
    private void OnDeleted(object source, FileSystemEventArgs e)
    {
      if (Util.Utils.IsAudio(e.FullPath))
      {
        Log.Debug("MusicFolderWatcher Delete File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Delete, e.FullPath));
      }
    }

    // Event handler for Create of a directory
    private void OnDirectoryCreated(object source, FileSystemEventArgs e)
    {
      Log.Debug("MusicFolderWatcher Add Directory Fired: {0}", e.FullPath);
      _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.CreateDirectory, e.FullPath));
    }

    // Event handler handling the Delete of a directory
    private void OnDirectoryDeleted(object source, FileSystemEventArgs e)
    {
      Log.Debug("MusicFolderWatcher Delete Directory Fired: {0}", e.FullPath);
      _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.DeleteDirectory, e.FullPath));
    }

    // Event handler handling the Rename of a file
    private void OnRenamed(object source, RenamedEventArgs e)
    {
      if (Util.Utils.IsAudio(e.FullPath) || Util.Utils.IsAudio(e.OldFullPath))
      {
        Log.Debug("MusicFolderWatcher Rename File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Rename, e.FullPath, e.OldFullPath));
      }
    }

    // Event handler handling the Rename of a directory
    private void OnDirectoryRenamed(object source, RenamedEventArgs e)
    {
      Log.Debug("MusicFolderWatcher Rename Directory Fired: {0}", e.FullPath);
      _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.RenameDirectory, e.FullPath, e.OldFullPath));
    }

    #endregion EventHandlers

    #region Private Methods

    private void AddMusic(string strFilename)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MUSICFILE_CREATED, 0, 0, 0, 0, 0,
                           null);
      msg.Label = strFilename;
      GUIWindowManager.SendMessage(msg);
    }

    private void DeleteMusic(string strFilename)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MUSICFILE_DELETED, 0, 0, 0, 0, 0,
                           null);
      msg.Label = strFilename;
      GUIWindowManager.SendMessage(msg);
    }

    private void RenameMusic(string oldFilename, string newFilename)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MUSICFILE_RENAMED, 0, 0, 0, 0, 0,
                           null);
      msg.Label = newFilename;
      msg.Label2 = oldFilename;
      GUIWindowManager.SendMessage(msg);
    }

    private void AddMusicDirectory(string strPath)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MUSICDIRECTORY_CREATED, 0, 0, 0, 0, 0,
                           null);
      msg.Label = strPath;
      GUIWindowManager.SendMessage(msg);
    }

    private void DeleteMusicDirectory(string strPath)
    {
      string dvdFolder = string.Empty;
      string bdFolder = string.Empty;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MUSICDIRECTORY_DELETED, 0, 0, 0, 0, 0,
                           null);
      msg.Label = strPath;
      GUIWindowManager.SendMessage(msg);
    }

    private void RenameMusicDirectory(string oldDirectory, string newDirectory)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MUSICDIRECTORY_RENAMED, 0, 0, 0, 0, 0,
                           null);
      msg.Label = newDirectory;
      msg.Label2 = oldDirectory;
      GUIWindowManager.SendMessage(msg);
    }

    private void ProcessEvents(object sender, ElapsedEventArgs e)
    {
      // Allow only one Timer event to be executed.
      if (Monitor.TryEnter(_EnterThread))
      {
        // Only one thread at a time is processing the events                
        try
        {
          // Lock the Collection, while processing the Events
          lock (_Events.SyncRoot)
          {
            #region Affected directories

            // Get parent directories where events occured so we can avoid multiple refreshes on
            // the same directory
            FolderWatcherEvent currentEvent = null;
            ArrayList refreshDir = new ArrayList();

            for (int i = 0; i < _Events.Count; i++)
            {
              currentEvent = _Events[i] as FolderWatcherEvent;

              if (currentEvent != null)
              {
                string strPath = currentEvent.FileName;

                if (!string.IsNullOrEmpty(strPath))
                {
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

            // Process all events for Musicdatabase purpose (delete event only)
            // Does not fire any GUIWindowsMessage
            for (int i = 0; i < _Events.Count; i++)
            {
              currentEvent = _Events[i] as FolderWatcherEvent;
              
              if (currentEvent != null)
              {
                switch (currentEvent.Type)
                {

                  #region file events handlers

                  // Create Music
                  case FolderWatcherEvent.EventType.Create:
                  case FolderWatcherEvent.EventType.Change:
                  {
                    AddMusic(currentEvent.FileName);
                    break;
                  }

                  // Delete Music
                  case FolderWatcherEvent.EventType.Delete:
                    {
                      DeleteMusic(currentEvent.FileName);
                      break;
                    }

                  // Rename Music
                  case FolderWatcherEvent.EventType.Rename:
                    {
                      RenameMusic(currentEvent.OldFileName, currentEvent.FileName);
                      break;
                    }

                    #endregion

                  #region directory events handlers

                  // Create directory
                  case FolderWatcherEvent.EventType.CreateDirectory:
                    {
                      AddMusicDirectory(currentEvent.FileName);
                      break;
                    }

                  // Delete directory
                  case FolderWatcherEvent.EventType.DeleteDirectory:
                    {
                      DeleteMusicDirectory(currentEvent.FileName);
                      break;
                    }

                  // Rename directory
                  case FolderWatcherEvent.EventType.RenameDirectory:
                    {
                      RenameMusicDirectory(currentEvent.OldFileName, currentEvent.FileName);
                      break;
                    }

                    #endregion

                }

                _Events.RemoveAt(i);
                i--; // Don't skip next event
              }
            }

            #endregion
          }
        }
        finally
        {
          Monitor.Exit(_EnterThread);
        }
      }
    }

    #endregion Private Methods

  }
}