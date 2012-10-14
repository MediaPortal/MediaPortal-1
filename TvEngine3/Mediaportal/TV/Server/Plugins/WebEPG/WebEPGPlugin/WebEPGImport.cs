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
using System.Threading;
using System.IO;
using Castle.Core;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.Plugins.WebEPGImport.Config;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using System.Runtime.CompilerServices;
using WebEPG.Utils;

namespace Mediaportal.TV.Server.Plugins.WebEPGImport
{
  [Interceptor("PluginExceptionInterceptor")]
  public class WebEPGImport : ITvServerPlugin, ITvServerPluginStartedAll, IWakeupHandler 
  {
    #region constants

    private const string _wakeupHandlerName = "WebEPGWakeupHandler";

    #endregion

    #region variables

    private bool _workerThreadRunning = false;
    private System.Timers.Timer _scheduleTimer;

    #endregion

    #region Constructor

    /// <summary>
    /// Create a new instance of a generic standby handler
    /// </summary>
    public WebEPGImport() {}

    #endregion

    #region properties

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get { return "WebEPG"; }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get { return "1.0.0.0"; }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get { return "Arion_p - James"; }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get { return true; }
    }

    #endregion

    #region public methods

    /// <summary>
    /// Starts the plugin
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      Log.WriteFile("plugin: webepg started");

      //CheckNewTVGuide();
      _scheduleTimer = new System.Timers.Timer {Interval = 60000, Enabled = true};
      _scheduleTimer.Elapsed += new System.Timers.ElapsedEventHandler(_scheduleTimer_Elapsed);
    }

    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      Log.WriteFile("plugin: webepg stopped");
      UnregisterWakeupHandler();
      if (_scheduleTimer != null)
      {
        _scheduleTimer.Enabled = false;
        _scheduleTimer.Dispose();
        _scheduleTimer = null;
      }
    }

    /// <summary>
    /// returns the setup sections for display in SetupTv
    /// </summary>
    public SectionSettings Setup
    {
      get { return new WebEPGSetup(); }
    }

    /// <summary>
    /// Forces the import of the tvguide. Usable when testing the grabber
    /// </summary>
    public void ForceImport()
    {
      ForceImport(null);
    }

    /// <summary>
    /// Forces the import of the tvguide. Usable when testing the grabber
    /// </summary>
    public void ForceImport(WebEPG.WebEPG.ShowProgressHandler showProgress)
    {
      StartImport(showProgress);
    }

    #endregion

    #region private members

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void StartImport(WebEPG.WebEPG.ShowProgressHandler showProgress)
    {
      if (_workerThreadRunning)
        return;


      _workerThreadRunning = true;
      ThreadParams param = new ThreadParams();
      param.showProgress = showProgress;
      Thread workerThread = new Thread(new ParameterizedThreadStart(ThreadFunctionImportTVGuide));
      workerThread.Name = "WebEPGImporter";
      workerThread.IsBackground = true;
      workerThread.Priority = ThreadPriority.Lowest;
      workerThread.Start(param);
    }

    private class ThreadParams
    {
      public WebEPG.WebEPG.ShowProgressHandler showProgress;
    } ;

    private void ThreadFunctionImportTVGuide(object aparam)
    {
      SetStandbyAllowed(false);

      try
      {
        ThreadParams param = (ThreadParams)aparam;

        Setting setting;

        
        string destination = SettingsManagement.GetSetting("webepgDestination", "db").Value;
        string webepgDirectory = PathManager.GetDataPath;
        string configFile = webepgDirectory + @"\WebEPG\WebEPG.xml";

        //int numChannels = 0, numPrograms = 0;
        //string errors = "";

        try
        {
          Log.Write("plugin:webepg importing");
          Log.Info("WebEPG: Using directory {0}", webepgDirectory);


          IEpgDataSink epgSink;

          if (destination == "db")
          {
            bool deleteBeforeImport = Convert.ToBoolean(SettingsManagement.GetSetting("webepgDeleteBeforeImport", "true").Value);
            //// Allow for deleting of all existing programs before adding the new ones. 
            //// Already imported programs might have incorrect data depending on the grabber & setup
            //// f.e when grabbing programs many days ahead
            //if (deleteBeforeImport && ! deleteOnlyOverlapping)
            //{
            //  SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
            //  SqlStatement stmt = sb.GetStatement();
            //  stmt.Execute();
            //}
            epgSink = new DatabaseEPGDataSink(deleteBeforeImport);
            Log.Info("Writing to TVServer database");
          }
          else
          {
            string xmltvDirectory = string.Empty;
            if (destination == "xmltv")
            {
              xmltvDirectory = SettingsManagement.GetSetting("webepgDestinationFolder", string.Empty).Value;
            }
            if (xmltvDirectory == string.Empty)
            {
              // Do not use XmlTvImporter.DefaultOutputFolder to avoid reference to XmlTvImport
              xmltvDirectory = SettingsManagement.GetSetting("xmlTv", PathManager.GetDataPath + @"\xmltv").Value;
            }
            Log.Info("Writing to tvguide.xml in {0}", xmltvDirectory);
            // Open XMLTV output file
            if (!Directory.Exists(xmltvDirectory))
            {
              Directory.CreateDirectory(xmltvDirectory);
            }
            epgSink = new XMLTVExport(xmltvDirectory);
          }

          WebEPG.WebEPG epg = new WebEPG.WebEPG(configFile, epgSink, webepgDirectory);
          if (param.showProgress != null)
          {
            epg.ShowProgress += param.showProgress;
          }
          epg.Import();
          if (param.showProgress != null)
          {
            epg.ShowProgress -= param.showProgress;
          }

          SettingsManagement.SaveSetting("webepgResultLastImport", DateTime.Now.ToString());
          SettingsManagement.SaveSetting("webepgResultChannels", epg.ImportStats.Channels.ToString());
          SettingsManagement.SaveSetting("webepgResultPrograms", epg.ImportStats.Programs.ToString());
          SettingsManagement.SaveSetting("webepgResultStatus", epg.ImportStats.Status);
          
          //Log.Write("Xmltv: imported {0} channels, {1} programs status:{2}", numChannels, numPrograms, errors);
        }
        catch (Exception ex)
        {
          Log.Error(@"plugin:webepg import failed");
          Log.Write(ex);
        }

        SettingsManagement.SaveSetting("webepgResultLastImport", DateTime.Now.ToString());
      }
      finally
      {
        Log.WriteFile(@"plugin:webepg import done");
        _workerThreadRunning = false;
        SetStandbyAllowed(true);
      }
    }

    private void _scheduleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      
      bool scheduleEnabled = Convert.ToBoolean(SettingsManagement.GetSetting("webepgScheduleEnabled", "true").Value);
      if (scheduleEnabled)
      {
        Setting configSetting = SettingsManagement.GetSetting("webepgSchedule", String.Empty);
        EPGWakeupConfig config = new EPGWakeupConfig(configSetting.Value);
        if (ShouldRunNow())
        {
          Log.Info("WebEPGImporter: WebEPG schedule {0}:{1} is due: {2}:{3}",
                   config.Hour, config.Minutes, DateTime.Now.Hour, DateTime.Now.Minute);
          StartImport(null);
          config.LastRun = DateTime.Now;
          SettingsManagement.SaveSetting("webepgSchedule", config.SerializeAsString());
        }
      }
    }

    private void EPGScheduleDue()
    {
      StartImport(null);
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
          Log.Debug("WebEPGImporter: registered with PowerScheduler EPG handler");
          return;
        }
      }
      Log.Debug("WebEPGImporter: NOT registered with PowerScheduler EPG handler");
    }

    private void RegisterWakeupHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().Register(this as IWakeupHandler);
        Log.Debug("WebEPGImporter: registered WakeupHandler with PowerScheduler ");
        return;
      }
      Log.Debug("WebEPGImporter: NOT registered WakeupHandler with PowerScheduler ");
    }

    private void UnregisterWakeupHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().Unregister(this as IWakeupHandler);
        Log.Debug("WebEPGImporter: unregistered WakeupHandler with PowerScheduler ");
      }
    }

    private void SetStandbyAllowed(bool allowed)
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        Log.Debug("plugin:webepg: Telling PowerScheduler standby is allowed: {0}, timeout is one hour", allowed);
        GlobalServiceProvider.Instance.Get<IEpgHandler>().SetStandbyAllowed(this, allowed, 3600);
      }
    }

    /// <summary>
    /// Returns whether a schedule is due, and the EPG should run now.
    /// </summary>
    /// <returns></returns>
    private bool ShouldRunNow()
    {
      
      EPGWakeupConfig config = new EPGWakeupConfig(SettingsManagement.GetSetting("webepgSchedule", String.Empty).Value);

      // check if schedule is due
      // check if we've already run today
      if (config.LastRun.Day != DateTime.Now.Day)
      {
        // check if we should run today
        if (ShouldRun(config.Days, DateTime.Now.DayOfWeek))
        {
          // check if schedule is due
          DateTime now = DateTime.Now;
          if (now.Hour > config.Hour || (now.Hour == config.Hour && now.Minute >= config.Minutes))
          {
            return true;
          }
        }
      }
      return false;
    }

    private bool ShouldRun(List<EPGGrabDays> days, DayOfWeek dow)
    {
      switch (dow)
      {
        case DayOfWeek.Monday:
          return (days.Contains(EPGGrabDays.Monday));
        case DayOfWeek.Tuesday:
          return (days.Contains(EPGGrabDays.Tuesday));
        case DayOfWeek.Wednesday:
          return (days.Contains(EPGGrabDays.Wednesday));
        case DayOfWeek.Thursday:
          return (days.Contains(EPGGrabDays.Thursday));
        case DayOfWeek.Friday:
          return (days.Contains(EPGGrabDays.Friday));
        case DayOfWeek.Saturday:
          return (days.Contains(EPGGrabDays.Saturday));
        case DayOfWeek.Sunday:
          return (days.Contains(EPGGrabDays.Sunday));
        default:
          return false;
      }
    }

    private DateTime GetNextWakeupSchedule(DateTime earliestWakeupTime)
    {


      EPGWakeupConfig cfg = new EPGWakeupConfig(SettingsManagement.GetSetting("webepgSchedule", String.Empty).Value);

      // Start by thinking we should run today
      DateTime nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, cfg.Hour, cfg.Minutes, 0);
      // check if we should run today or some other day in the future
      if (cfg.LastRun.Day == DateTime.Now.Day || nextRun < earliestWakeupTime)
      {
        // determine first next day to run EPG grabber
        for (int i = 1; i < 8; i++)
        {
          if (ShouldRun(cfg.Days, nextRun.AddDays(i).DayOfWeek))
          {
            nextRun = nextRun.AddDays(i);
            break;
          }
        }
        if (DateTime.Now.Day == nextRun.Day)
        {
          Log.Error("WebEPG: no valid next wakeup date for EPG grabbing found!");
          nextRun = DateTime.MaxValue;
        }
      }
      return nextRun;
    }

    #endregion

    #region ITvServerPluginStartedAll Members

    public void StartedAll()
    {
      RegisterForEPGSchedule();
      RegisterWakeupHandler();
    }

    #endregion

    #region IWakeupHandler implementation

    [MethodImpl(MethodImplOptions.Synchronized)]
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {

      bool scheduleEnabled = Convert.ToBoolean(SettingsManagement.GetSetting("webepgScheduleEnabled", "true").Value);
      if (!scheduleEnabled)
      {
        return DateTime.MaxValue;
      }

      return GetNextWakeupSchedule(earliestWakeupTime);
    }

    public string HandlerName
    {
      get { return _wakeupHandlerName; }
    }

    #endregion
  }
}