#region Copyright (C) 2006-2009 Team MediaPortal

/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using TvControl;
using TvDatabase;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvEngine
{
  public class TvMovie : ITvServerPlugin, ITvServerPluginStartedAll
  {
    #region Members

    private TvMovieDatabase _database;
    private System.Timers.Timer _stateTimer;
    private bool _isImporting = false;
    private const long _timerIntervall = 1800000;

    #endregion

    #region Static properties

    /// <summary>
    /// Retrieves the location of TVDaten.mdb - prefers manual configured path, does fallback to registry.
    /// </summary>
    public static string TVMovieProgramPath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;
        try
        {
          using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
            if (rkey != null)
              path = string.Format("{0}", rkey.GetValue("ProgrammPath"));

          mpPath = TvMovieDatabase.TvBLayer.GetSetting("TvMovieInstallPath", path).Value;
        }
        catch (Exception ex)
        {
          Log.Info("TVMovie: Error getting TV Movie install dir (ProgrammPath) from registry {0}", ex.Message);
        }

        if (File.Exists(mpPath))
          return mpPath;

        return path;
      }
    }

    public static string DatabasePath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;

        try
        {
          mpPath = TvMovieDatabase.TvBLayer.GetSetting("TvMoviedatabasepath", path).Value;
          if (File.Exists(mpPath))
            return mpPath;
        }
        catch (Exception exdb)
        {
          Log.Info("TVMovie: Error getting TV Movie DB dir (DBDatei) from database {0}", exdb.Message);
        }
        try
        {
          using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
            if (rkey != null)
              path = string.Format("{0}", rkey.GetValue("DBDatei"));
        }
        catch (Exception ex)
        {
          Log.Info("TVMovie: Error getting TV Movie DB dir (DBDatei) from registry {0}", ex.Message);
        }
        return path;
      }
      set
      {
        string registryPath = string.Empty;
        string newParamPath = value;

        try
        {
          try
          {
            using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
              if (rkey != null)
                registryPath = string.Format("{0}", rkey.GetValue("DBDatei"));
          }
          catch (Exception exr)
          {
            Log.Info("TVMovie: A registry lookup failed when setting TV Movie DB dir (DBDatei) - : {0}", exr.Message);
          }

          // passed path is invalid try using the registry path
          if (!File.Exists(newParamPath))
            newParamPath = registryPath;

          Setting setting = TvMovieDatabase.TvBLayer.GetSetting("TvMoviedatabasepath", string.Empty);
          string mpDbPath = setting.Value; // TvBLayer.GetSetting("TvMoviedatabasepath", string.Empty).Value;          

          if (string.IsNullOrEmpty(newParamPath))
          {
            // use configured path
            if (!string.IsNullOrEmpty(mpDbPath)) // do not check File.Exists because it might temporarily unavailable
              return;
            else
              setting.Value = string.Empty;
          }
          else
            setting.Value = newParamPath;

          setting.Persist();
          Log.Info("TVMovie: Set TV Movie DB dir to {0}", setting.Value);
        }
        catch (Exception ex)
        {
          Log.Info("TVMovie: Error setting TV Movie DB dir (DBDatei) - {0}", ex.Message);
        }
      }
    }

    #endregion

    #region Powerscheduler handling

    private void SetStandbyAllowed(bool allowed)
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        GlobalServiceProvider.Instance.Get<IEpgHandler>().SetStandbyAllowed(this, allowed, 1800);
        if (!allowed)
          Log.Debug("TVMovie: Telling PowerScheduler standby is allowed: {0}, timeout is 30 minutes", allowed);
      }
    }

    private void RegisterForEPGSchedule()
    {
      // Register with the EPGScheduleDue event so we are informed when
      // the EPG wakeup schedule is due.
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          handler.EPGScheduleDue += new EPGScheduleHandler(EPGScheduleDue);
          Log.Debug("TVMovie: registered with PowerScheduler EPG handler");
          return;
        }
      }
      Log.Debug("TVMovie: NOT registered with PowerScheduler EPG handler");
    }

    private void EPGScheduleDue()
    {
      SpawnImportThread();
    }

    #endregion

    #region Import methods

    private void ImportThread()
    {      
      try
      {
        _isImporting = true;

        try
        {
          _database = new TvMovieDatabase();
          _database.Connect();
        }
        catch (Exception)
        {
          Log.Error("TVMovie: Import enabled but the ClickFinder database was not found.");
          return;
        }

        //Log.Debug("TVMovie: Checking database");
        try
        {
          if (_database.NeedsImport)
          {
            SetStandbyAllowed(false);

            long updateDuration = _database.LaunchTVMUpdater(true);
            if (updateDuration < 1200)
            {
              // Updating a least a few programs should take more than 20 seconds
              if (updateDuration > 20)
                _database.Import();
              else
                Log.Info("TVMovie: Import skipped because there was no new data.");
            }
            else
              Log.Info("TVMovie: Import skipped because the update process timed out / has been aborted.");
          }
        }
        catch (Exception ex)
        {
          Log.Info("TvMovie plugin error:");
          Log.Write(ex);
        }
      }
      finally
      {
        _isImporting = false;
        SetStandbyAllowed(true);
      }
    }

    private void StartImportThread(object source, ElapsedEventArgs e)
    {
      //TODO: check stateinfo
      SpawnImportThread();
    }

    private void SpawnImportThread()
    {
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        if (layer.GetSetting("TvMovieEnabled", "false").Value != "true")
          return;
      }
      catch (Exception ex1)
      {
        Log.Error("TVMovie: Error checking enabled status - {0},{1}", ex1.Message, ex1.StackTrace);
      }

      if (!_isImporting)
      {
        try
        {
          Thread importThread = new Thread(new ThreadStart(ImportThread));
          importThread.Name = "TV Movie importer";
          importThread.IsBackground = true;
          importThread.Priority = ThreadPriority.Lowest;
          importThread.Start();
        }
        catch (Exception ex2)
        {
          Log.Error("TVMovie: Error spawing import thread - {0},{1}", ex2.Message, ex2.StackTrace);
        }
      }
    }

    private void StartStopTimer(bool startNow)
    {
      if (startNow)
      {
        if (_stateTimer == null)
        {
          _stateTimer = new System.Timers.Timer();
          _stateTimer.Elapsed += new ElapsedEventHandler(StartImportThread);
          _stateTimer.Interval = _timerIntervall;
          _stateTimer.AutoReset = true;

          GC.KeepAlive(_stateTimer);
        }
        _stateTimer.Start();
        _stateTimer.Enabled = true;
      }
      else
      {
        _stateTimer.Enabled = false;
        _stateTimer.Stop();
        Log.Debug("TVMovie: background import timer stopped");
      }
    }

    #endregion

    #region ITvServerPlugin Members

    public string Name
    {
      get { return "TV Movie EPG import"; }
    }

    public string Version
    {
      get { return "1.0.3.0"; }
    }

    public string Author
    {
      get { return "rtv"; }
    }

    public bool MasterOnly
    {
      get { return false; }
    }

    public void Start(IController controller)
    {
      StartStopTimer(true);
    }

    public void StartedAll()
    {
      RegisterForEPGSchedule();
    }

    public void Stop()
    {
      if (_database != null)
        _database.Canceled = true;
      if (_stateTimer != null)
      {
        StartStopTimer(false);
        _stateTimer.Dispose();
      }
    }

    public SetupTv.SectionSettings Setup
    {
      get { return new SetupTv.Sections.TvMovieSetup(); }
    }

    #endregion
  }
}
