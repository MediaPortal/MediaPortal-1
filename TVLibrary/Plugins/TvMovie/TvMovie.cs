#region Copyright (C) 2006-2008 Team MediaPortal

/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
using System.Threading;
using System.Timers;
using System.IO;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;

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
            if (updateDuration < 600)
            {
              // Updating a least a few programs would take more than 15 seconds
              if (updateDuration > 15)
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

    private void SetStandbyAllowed(bool allowed)
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        GlobalServiceProvider.Instance.Get<IEpgHandler>().SetStandbyAllowed(this, allowed, 1800);
        if (!allowed)
          Log.Debug("TVMovie: Telling PowerScheduler standby is allowed: {0}, timeout is 30 minutes", allowed);
      }
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

    #region ITvServerPlugin Members

    public string Name
    {
      get { return "TV Movie EPG import"; }
    }

    public string Version
    {
      get { return "1.0.0.0"; }
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
