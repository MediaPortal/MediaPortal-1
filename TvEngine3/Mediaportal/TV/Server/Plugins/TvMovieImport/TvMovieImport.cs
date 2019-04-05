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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using Castle.Core;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.Plugins.TvMovieImport.Config;
using Mediaportal.TV.Server.Plugins.TvMovieImport.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TvMovieImport
{
  [Interceptor("PluginExceptionInterceptor")]
  [ComponentProxyBehavior(AdditionalInterfaces = new [] { typeof(ITvServerPluginStartedAll), typeof(ITvServerPluginCommunication) })]
  public class TvMovieImport : ITvServerPlugin, ITvServerPluginStartedAll, ITvServerPluginCommunication
  {
    #region constants

    private const string SETTING_NAME_IMPORT_PREVIOUS_FINISH = "tvMovieImportPreviousFinishDateTime";

    #endregion

    #region variables

    private static object _lockImport = new object();
    private static object _lockImportStatus = new object();

    private TvMovieImportService _service = null;
    private Importer _importer = new Importer();
    private bool _isRegisteredForPowerEvents = false;
    private bool _isRegisteredForPowerSchedulerTrigger = false;
    private System.Timers.Timer _timer = null;
    private bool _isImportRunning = false;
    private bool _isImportCancelled = false;

    #endregion

    public TvMovieImport()
    {
      _service = new TvMovieImportService();
      TvMovieImportService.Importer = this;
    }

    #region ITvServerPlugin members

    /// <summary>
    /// The name of this TV Server plugin.
    /// </summary>
    public string Name
    {
      get
      {
        return "TV Movie ClickFinder EPG Import";
      }
    }

    /// <summary>
    /// The version of this TV Server plugin.
    /// </summary>
    public string Version
    {
      get
      {
        return "1.1.0.0";
      }
    }

    /// <summary>
    /// The author of this TV Server plugin.
    /// </summary>
    public string Author
    {
      get
      {
        return "rtv, mm1352000";
      }
    }

    /// <summary>
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SectionSettings Setup
    {
      get
      {
        return new TvMovieImportConfig();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("TV Movie import: start");

      RegisterPowerEventHandler();
      ExecuteImportOnStartupOrResumeInThread();

      _timer = new System.Timers.Timer();
      _timer.Interval = 1800000;    // 30 minutes
      _timer.Enabled = true;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("TV Movie import: stop");

      CancelImport();
      lock (_lockImport)
      {
        _isImportCancelled = false;

        UnRegisterPowerEventHandler();
        UnRegisterPowerSchedulerEpgHandler();

        if (_timer != null)
        {
          _timer.Enabled = false;
          _timer.Stop();
          _timer.Dispose();
          _timer = null;
        }
      }
    }

    #endregion

    #region power event handling

    private void RegisterPowerEventHandler()
    {
      if (!_isRegisteredForPowerEvents)
      {
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
        {
          IPowerEventHandler handler = GlobalServiceProvider.Instance.Get<IPowerEventHandler>();
          if (handler != null)
          {
            handler.AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
            this.LogDebug("TV Movie import: registered for power events");
            _isRegisteredForPowerEvents = true;
          }
        }
        else if (_isRegisteredForPowerEvents)
        {
          this.LogWarn("TV Movie import: failed to register for power events");
        }
      }
    }

    private void UnRegisterPowerEventHandler()
    {
      if (!_isRegisteredForPowerEvents)
      {
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
        {
          IPowerEventHandler handler = GlobalServiceProvider.Instance.Get<IPowerEventHandler>();
          if (handler != null)
          {
            handler.RemovePowerEventHandler(new PowerEventHandler(OnPowerEvent));
            this.LogDebug("TV Movie import: unregistered for power events");
            _isRegisteredForPowerEvents = false;
          }
        }
        if (_isRegisteredForPowerEvents)
        {
          this.LogError("TV Movie import: failed to unregister for power events");
        }
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private bool OnPowerEvent(PowerEventType powerStatus)
    {
      switch (powerStatus)
      {
        case PowerEventType.ResumeAutomatic:
        case PowerEventType.ResumeCritical:
        case PowerEventType.ResumeSuspend:
          // ResumeSuspend may not be broadcast unless the user has triggered
          // resume, so we handle ResumeAutomatic and ResumeCritical as well.
          this.LogDebug("TV Movie import: resumed, status = {0}", powerStatus);
          ExecuteImportOnStartupOrResumeInThread();
          break;
      }
      return true;
    }

    #endregion

    #region PowerScheduler integration

    private void RegisterPowerSchedulerEpgHandler()
    {
      // PowerScheduler can be configured to wake the system and trigger EPG
      // grabbing. Register for that notification.
      if (!_isRegisteredForPowerSchedulerTrigger && GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          handler.EPGScheduleDue += new EPGScheduleHandler(PowerSchedulerEpgGrab);
          this.LogDebug("TV Movie import: registered as PowerScheduler EPG handler");
          _isRegisteredForPowerSchedulerTrigger = true;
        }
      }
    }

    private void UnRegisterPowerSchedulerEpgHandler()
    {
      if (_isRegisteredForPowerSchedulerTrigger && GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          handler.EPGScheduleDue -= new EPGScheduleHandler(PowerSchedulerEpgGrab);
          this.LogDebug("TV Movie import: unregistered as PowerScheduler EPG handler");
          _isRegisteredForPowerSchedulerTrigger = false;
        }
      }
    }

    private void SetStandbyAllowed(bool allowed)
    {
      if (_isRegisteredForPowerSchedulerTrigger && GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          this.LogDebug("TV Movie import: update standby permission, allowed = {0}, timeout = 1 hour", allowed);
          handler.SetStandbyAllowed(this, allowed, 3600);
        }
      }
    }

    #endregion

    #region action/import triggers

    private void PowerSchedulerEpgGrab()
    {
      this.LogInfo("TV Movie import: PowerScheduler triggered EPG grab/update");
      OnTimerElapsed(null, null);
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
      // Don't do anything if an import is currently running.
      if (!Monitor.TryEnter(_lockImport, 5000))
      {
        return;
      }

      try
      {
        ImportData(true, true);
      }
      catch (Exception ex)
      {
        // The function that executes the import should catch its own
        // exceptions. This is not expected.
        this.LogError(ex, "TV Movie import: failed to execute import");
      }
      finally
      {
        Monitor.Exit(_lockImport);
      }
    }

    #endregion

    #region channel mapping

    internal IList<string> ReadChannelsFromTvMovieDatabase()
    {
      this.LogDebug("TV Movie import: read channels from TV Movie database");
      IList<string> channels = Importer.GetTvMovieDatabaseChannelList();
      this.LogDebug("TV Movie import: channel count = {0}", channels.Count);
      return channels;
    }

    #endregion

    private void CancelImport()
    {
      if (_isImportRunning)
      {
        this.LogInfo("TV Movie import: cancelling import...");
      }
      _importer.CancelImport();
      _isImportCancelled = true;
    }

    #region import

    private void ExecuteImportOnStartupOrResumeInThread()
    {
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          this.LogDebug("TV Movie import: execute import on startup/resume");
          try
          {
            lock (_lockImport)
            {
              ImportData(true, true);
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TV Movie import: failed to execute import on startup/resume");
          }
        }
      );
    }

    internal void ImportData(bool checkTiming = true, bool checkForNewData = true)
    {
      // Don't do anything if an import is currently running. This is here to
      // handle forced imports only.
      if (!Monitor.TryEnter(_lockImport, 5000))
      {
        return;
      }

      _isImportRunning = true;
      try
      {
        int minutesBetweenUpdates = 60 * SettingsManagement.GetValue(TvMovieImportSetting.UpdateTimeFrequency, 24);
        TimeSpan timeSincePreviousImportFinish = DateTime.Now - SettingsManagement.GetValue(SETTING_NAME_IMPORT_PREVIOUS_FINISH, DateTime.MinValue);
        if (!checkTiming)
        {
          this.LogInfo("TV Movie import: starting forced import, frequency = {0} minutes, time since last import = {1}", minutesBetweenUpdates, (long)timeSincePreviousImportFinish.TotalMinutes);
        }
        else
        {
          if (timeSincePreviousImportFinish.TotalMinutes < minutesBetweenUpdates)
          {
            return;
          }
          if (SettingsManagement.GetValue(TvMovieImportSetting.UpdateTimeOnStartup, false))
          {
            // On startup/resume (or anytime while awake)...
            this.LogInfo("TV Movie import: time to execute import (startup/resume), frequency = {0} minutes, time since last import = {1}", minutesBetweenUpdates, (long)timeSincePreviousImportFinish.TotalMinutes);
          }
          else
          {
            // Only between certain times...
            TimeSpan scheduledTimeStart = SettingsManagement.GetValue(TvMovieImportSetting.UpdateTimeBetweenStart, DateTime.Now).TimeOfDay;
            TimeSpan scheduledTimeEnd = SettingsManagement.GetValue(TvMovieImportSetting.UpdateTimeBetweenEnd, DateTime.Now).TimeOfDay;
            if (DateTime.Now.TimeOfDay < scheduledTimeStart || DateTime.Now.TimeOfDay > scheduledTimeEnd)
            {
              return;
            }
            this.LogInfo("TV Movie import: time to execute import (between {0} and {1}), frequency = {2} minutes, time since last import = {3}", scheduledTimeStart, scheduledTimeEnd, minutesBetweenUpdates, (long)timeSincePreviousImportFinish.TotalMinutes);
          }
        }

        if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>() &&
            GlobalServiceProvider.Instance.Get<IPowerScheduler>().IsSuspendInProgress())
        {
          this.LogWarn("TV Movie import: import prevented by suspend in progress");
          return;
        }

        ImportStats stats = new ImportStats();
        UpdateImportStatus("starting import", stats);
        SetStandbyAllowed(false);

        try
        {
          bool success = true;
          try
          {
            success = _importer.Import(checkForNewData, UpdateImportStatus, ref stats);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TV Movie import: failed to import from TV Movie database");
          }

          if (_isImportCancelled)
          {
            UpdateImportStatus("import cancelled", stats);
            return;
          }

          UpdateImportStatus("waiting for database import to complete", stats);
          ProgramManagement.WaitForInsertProgramsToFinish();

          this.LogInfo("TV Movie import: import completed, result = {0}, [TVM DB/unmapped/TVE DB] channel count = {1}/{2}/{3}, program count = {2}/-/{3}",
                        success, stats.ChannelCountTvmDb, stats.ChannelCountTveDb,
                        stats.ProgramCountTvmDb, stats.ProgramCountTveDb);
          if (!success)
          {
            UpdateImportStatus("import completed, errors occurred", stats);
          }
          else if (stats.ProgramCountTveDb > 0)
          {
            UpdateImportStatus("import completed, success", stats);
          }
          else if (stats.ChannelCountTvmDb == 0)
          {
            UpdateImportStatus("import completed, no channels in TV Movie database", stats);
          }
          else if (stats.ChannelCountTvmDb == stats.ChannelCountTvmDbUnmapped)
          {
            UpdateImportStatus("import completed, channels not mapped", stats);
          }
          else
          {
            UpdateImportStatus("import completed, old data", stats);
          }

          SettingsManagement.SaveValue(SETTING_NAME_IMPORT_PREVIOUS_FINISH, DateTime.Now);
        }
        finally
        {
          SetStandbyAllowed(true);
        }
      }
      finally
      {
        _isImportRunning = false;
        Monitor.Exit(_lockImport);
      }
    }

    private void UpdateImportStatus(string status, ImportStats stats = null)
    {
      lock (_lockImportStatus)
      {
        SettingsManagement.SaveValue("tvMoveImportStatusDateTime", DateTime.Now);
        SettingsManagement.SaveValue("tvMoveImportStatus", status);
        if (stats != null)
        {
          SettingsManagement.SaveValue("tvMoveImportStatusChannelCounts", stats.GetTotalChannelDescription());
          SettingsManagement.SaveValue("tvMoveImportStatusProgramCounts", stats.GetTotalProgramDescription());
        }
      }
    }

    internal void ReadImportStatus(out DateTime dateTime, out string status, out string channelCounts, out string programCounts)
    {
      lock (_lockImportStatus)
      {
        dateTime = SettingsManagement.GetValue("tvMoveImportStatusDateTime", DateTime.Now);
        status = SettingsManagement.GetValue("tvMoveImportStatus", string.Empty);
        channelCounts = SettingsManagement.GetValue("tvMoveImportStatusChannelCounts", string.Empty);
        programCounts = SettingsManagement.GetValue("tvMoveImportStatusProgramCounts", string.Empty);
      }
    }

    #endregion

    #region ITvServerPluginStartedAll member

    public void StartedAll()
    {
      RegisterPowerSchedulerEpgHandler();
    }

    #endregion

    #region ITvServerPluginCommunication members

    public object GetServiceInstance
    {
      get
      {
        return _service;
      }
    }

    public Type GetServiceInterfaceForContractType
    {
      get
      {
        return typeof(ITvMovieImportService);
      }
    }

    #endregion
  }
}