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

namespace MediaPortal.GUI.Pictures
{
  public class PicturesFolderWatcherHelper
  {
    #region Variables

    private bool bMonitoring;
    private ArrayList _Watchers = new ArrayList();
    private string _currentFolder = string.Empty;
    private object _EnterThread = new object(); // Only one timer event is processed at any given moment
    private ArrayList _Events = null;
    private Timer _Timer = null;
    private int _TimerInterval = 3000; // milliseconds

    #endregion

    #region Constructors/Destructors

    public PicturesFolderWatcherHelper(string directory)
    {
      if (!Directory.Exists(directory))
        return;

      _currentFolder = directory;
      Log.Debug("PicturesFolderWatcher Monitoring of enabled for {0}", _currentFolder);
    }

    #endregion

    #region Main

    public void StartMonitor()
    {
      if (bMonitoring)
      {
        Thread WorkerThread = new Thread(WatchShares);
        WorkerThread.IsBackground = true;
        WorkerThread.Name = "PicturesFolderWatcher";
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
        Log.Debug("PicturesFolderWatcher Monitoring of disabled for {0}", _currentFolder);
      }
    }

    private void WatchShares()
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
        Log.Error("PicturesFolderWatcher Unable to turn on monitoring for: {0} Exception: {1}", _currentFolder,
                  ex.Message);
      }
    }

    #endregion Main

    #region EventHandlers

    // Event handler for Create of a file
    private void OnCreated(object source, FileSystemEventArgs e)
    {
      if (Util.Utils.IsPicture(e.FullPath))
      {
        Log.Debug("PicturesFolderWatcher Add File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Create, e.FullPath));
      }
    }

    // Event handler for Change of a file
    private void OnChanged(object source, FileSystemEventArgs e)
    {
      FileInfo fi = new FileInfo(e.FullPath);
      // A Change event occured.
      // Was it on a file? Ignore change events on directories
      if (fi.Exists && Util.Utils.IsPicture(e.FullPath))
      {
        Log.Debug("PicturesFolderWatcher Change File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Change, e.FullPath));
      }
    }

    // Event handler handling the Delete of a file
    private void OnDeleted(object source, FileSystemEventArgs e)
    {
      if (Util.Utils.IsPicture(e.FullPath))
      {
        Log.Debug("PicturesFolderWatcher Delete File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Delete, e.FullPath));
      }
    }

    // Event handler for Create of a directory
    private void OnDirectoryCreated(object source, FileSystemEventArgs e)
    {
      Log.Debug("PicturesFolderWatcher Add Directory Fired: {0}", e.FullPath);
      _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.CreateDirectory, e.FullPath));
    }    

    // Event handler handling the Delete of a directory
    private void OnDirectoryDeleted(object source, FileSystemEventArgs e)
    {
      Log.Debug("PicturesFolderWatcher Delete Directory Fired: {0}", e.FullPath);
      _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.DeleteDirectory, e.FullPath));
    }

    // Event handler handling the Rename of a file
    private void OnRenamed(object source, RenamedEventArgs e)
    {
      if (Util.Utils.IsPicture(e.FullPath))
      {
        Log.Debug("PicturesFolderWatcher Rename File Fired: {0}", e.FullPath);
        _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.Rename, e.FullPath, e.OldFullPath));
      }
    }

    // Event handler handling the Rename of a directory
    private void OnDirectoryRenamed(object source, FileSystemEventArgs e)
    {
      Log.Debug("PicturesFolderWatcher Rename Directory Fired: {0}", e.FullPath);
      _Events.Add(new FolderWatcherEvent(FolderWatcherEvent.EventType.RenameDirectory, e.FullPath));
    }

    #endregion EventHandlers

    #region Private Methods

    private void ProcessEvents(object sender, ElapsedEventArgs e)
    {
      if (_Events.Count > 0)
      {
        Log.Debug("PicturesFolderWatcher event count {0}", _Events.Count);
        GUIPropertyManager.SetProperty("#PicturesFolderChanged", "true");
        _Events.Clear();
      }
    }

    #endregion
  }
}