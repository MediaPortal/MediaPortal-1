#region Copyright (C) 2007 Team MediaPortal
/* 
 *	Copyright (C) 2007 Team MediaPortal
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

#region Usings
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TvControl;
using TvService;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  #region Enums
  public enum EPGGrabDays
  {
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
  }
  #endregion

  #region EPG Wakeup config class
  [Serializable]
  public class EPGWakeupConfig
  {
    public DateTime LastRun = DateTime.MinValue;
    public ArrayList Days = new ArrayList();
    public int Hour;
    public int Minutes;
    public EPGWakeupConfig() { }
    public EPGWakeupConfig(string serializedConfig)
    {
      EPGWakeupConfig cfg = new EPGWakeupConfig();
      try
      {
        BinaryFormatter formatter = new BinaryFormatter();
        byte[] buffer = Convert.FromBase64String(serializedConfig);
        using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length))
        {
          cfg = (EPGWakeupConfig)formatter.Deserialize(stream);
        }
      }
      catch (SerializationException) { }
      Hour = cfg.Hour;
      Minutes = cfg.Minutes;
      Days = cfg.Days;
      LastRun = cfg.LastRun;
    }
    public string SerializeAsString()
    {
      BinaryFormatter formatter = new BinaryFormatter();
      string result;
      using (MemoryStream stream = new MemoryStream())
      {
        formatter.Serialize(stream, this);
        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[stream.Length];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        result = Convert.ToBase64String(buffer, 0, bytesRead);
      }
      return result;
    }
    public override bool Equals(object obj)
    {
      if (obj is EPGWakeupConfig)
      {
        EPGWakeupConfig cfg = (EPGWakeupConfig)obj;
        if (cfg.Hour == Hour && cfg.Minutes == Minutes && cfg.Days.Equals(Days))
          return true;
      }
      return false;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
  #endregion

  /// <summary>
  /// Handles standby/wakeup for EPG grabbing
  /// </summary>
  public class EpgGrabbingHandler : IStandbyHandler, IWakeupHandler
  {
    #region Variables
    IController _controller;
    #endregion

    #region Constructor
    public EpgGrabbingHandler(IController controller)
    {
      _controller = controller;
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(EpgGrabbingHandler_OnPowerSchedulerEvent);
    }
    #endregion

    #region Private methods
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void EpgGrabbingHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:

          IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
          TvBusinessLayer layer = new TvBusinessLayer();
          PowerSetting setting;
          bool enabled;

          // Check if standby should be prevented when grabbing EPG
          setting = ps.Settings.GetSetting("PreventStandbyWhenGrabbingEPG");
          enabled = Convert.ToBoolean(layer.GetSetting("PreventStandbyWhenGrabbingEPG", "false").Value);
          if (setting.Get<bool>() != enabled)
          {
            setting.Set<bool>(enabled);
            if (enabled)
            {
              if (ps.IsRegistered(this as IStandbyHandler))
                ps.Unregister(this as IStandbyHandler);
              ps.Register(this as IStandbyHandler);
            }
            else
            {
              ps.Unregister(this as IStandbyHandler);
            }
            Log.Debug("PowerScheduler: preventing standby when grabbing EPG: {0}", enabled);
          }

          // Check if system should wakeup for EPG grabs
          setting = ps.Settings.GetSetting("WakeupSystemForEPGGrabbing");
          enabled = Convert.ToBoolean(layer.GetSetting("WakeupSystemForEPGGrabbing", "false").Value);
          if (setting.Get<bool>() != enabled)
          {
            setting.Set<bool>(enabled);
            if (enabled)
            {
              if (ps.IsRegistered(this as IWakeupHandler))
                ps.Unregister(this as IWakeupHandler);
              ps.Register(this as IWakeupHandler);
            }
            else
            {
              ps.Unregister(this as IWakeupHandler);
            }
            Log.Debug("PowerScheduler: wakeup system for EPG grabbing: {0}", enabled);
          }

          // Check if a wakeup time is set
          setting = ps.Settings.GetSetting("EPGWakeupConfig");
          EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("EPGWakeupConfig", String.Empty).Value));
          if (setting.Get<EPGWakeupConfig>() != config)
          {
            setting.Set<EPGWakeupConfig>(config);
            Log.Debug("PowerScheduler: wakeup system for EPG at time: {0}:{1}", config.Hour, config.Minutes);
            if (config.Days != null)
            {
              foreach (EPGGrabDays day in config.Days)
                Log.Debug("PowerScheduler: wakeup on day {0}", day);
            }
            Log.Debug("PowerScheduler: last run: {0}", config.LastRun);
          }

          // check if schedule is due
          // check if we've already run today
          if (config.LastRun.Day != DateTime.Now.Day)
          {
            // check if we should run today
            if (ShouldRun(config, DateTime.Now.DayOfWeek))
            {
              // check if schedule is due
              if (DateTime.Now.Hour >= config.Hour)
              {
                if (DateTime.Now.Minute >= config.Minutes)
                {
                  Log.Info("PowerScheduler: EPG schedule {0}:{1} is due: {2}:{3}",
                    config.Hour, config.Minutes, DateTime.Now.Hour, DateTime.Now.Minute);
                  // try a forced start of EPG grabber if not already started 
                  if (!_controller.EpgGrabberEnabled)
                    _controller.EpgGrabberEnabled = true;
                  config.LastRun = DateTime.Now;
                  Setting s = layer.GetSetting("EPGWakeupConfig", String.Empty);
                  s.Value = config.SerializeAsString();
                  s.Persist();
                }
              }
            }
          }

          break;
      }
    }

    private bool ShouldRun(EPGWakeupConfig config, DayOfWeek dow)
    {
      switch (dow)
      {
        case DayOfWeek.Monday:
          if (config.Days.Contains(EPGGrabDays.Monday))
            return true;
          return false;
        case DayOfWeek.Tuesday:
          if (config.Days.Contains(EPGGrabDays.Tuesday))
            return true;
          return false;
        case DayOfWeek.Wednesday:
          if (config.Days.Contains(EPGGrabDays.Wednesday))
            return true;
          return false;
        case DayOfWeek.Thursday:
          if (config.Days.Contains(EPGGrabDays.Thursday))
            return true;
          return false;
        case DayOfWeek.Friday:
          if (config.Days.Contains(EPGGrabDays.Friday))
            return true;
          return false;
        case DayOfWeek.Saturday:
          if (config.Days.Contains(EPGGrabDays.Saturday))
            return true;
          return false;
        case DayOfWeek.Sunday:
          if (config.Days.Contains(EPGGrabDays.Sunday))
            return true;
          return false;
        default:
          return false;
      }
    }
    #endregion

    #region IStandbyHandler/IWakeupHandler implementation
    public bool DisAllowShutdown
    {
      get
      {
        for (int i = 0; i < _controller.Cards; i++)
        {
          int cardId = _controller.CardId(i);
          if (_controller.IsGrabbingEpg(cardId))
            return true;
        }
        return false;
      }
    }
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      EPGWakeupConfig cfg = ps.Settings.GetSetting("EPGWakeupConfig").Get<EPGWakeupConfig>();
      // Start by thinking we should run today
      DateTime nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, cfg.Hour, cfg.Minutes, 0);
      // check if we've already run today
      if (cfg.LastRun.Day == DateTime.Now.Day)
      {
        // determine first next day to run EPG grabber
        for (int i = 0; i < 8; i++)
        {
          if (ShouldRun(cfg, nextRun.AddDays(i).DayOfWeek))
          {
            nextRun = nextRun.AddDays(i);
            break;
          }
        }
        if (DateTime.Now.Day == nextRun.Day)
        {
          Log.Error("PowerScheduler: no valid next wakeup date for EPG grabbing found!");
          nextRun = DateTime.MaxValue;
        }
      }
      return nextRun;
    }
    public string HandlerName
    {
      get { return "EpgGrabbingHandler"; }
    }
    #endregion
  }
}
