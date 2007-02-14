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

#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvEngine;
#endregion

namespace TvEngine.PowerScheduler
{
  public class PowerSchedulerFactory
  {
    #region Variables
    List<IStandbyHandler> _standbyHandlers;
    List<IWakeupHandler> _wakeupHandlers;
    #endregion

    #region Constructor
    public PowerSchedulerFactory(IController controller)
    {
      IStandbyHandler standbyHandler;
      IWakeupHandler wakeupHandler;

      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();

      // Add handlers for preventing the system from entering standby
      standbyHandler = new ActiveStreamsHandler(controller);
      _standbyHandlers.Add(standbyHandler);
      standbyHandler = new EpgGrabbingHandler(controller);
      _standbyHandlers.Add(standbyHandler);
      standbyHandler = new SetupActiveHandler();
      _standbyHandlers.Add(standbyHandler);

      // Add handlers for resuming from standby
      wakeupHandler = new ScheduledRecordingsHandler();
      _wakeupHandlers.Add(wakeupHandler);
      //wakeupHandler = new TestWakeupHandler();
      //_wakeupHandlers.Add(wakeupHandler);
    }
    #endregion

    #region Public methods
    public void CreateDefaultSet()
    {
      IPowerScheduler powerScheduler = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      foreach (IStandbyHandler handler in _standbyHandlers)
        powerScheduler.Register(handler);
      foreach (IWakeupHandler handler in _wakeupHandlers)
        powerScheduler.Register(handler);
    }

    public void RemoveDefaultSet()
    {
      IPowerScheduler powerScheduler = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      foreach (IStandbyHandler handler in _standbyHandlers)
        powerScheduler.Unregister(handler);
      foreach (IWakeupHandler handler in _wakeupHandlers)
        powerScheduler.Unregister(handler);
    }
    #endregion
  }

  #region IStandbyHandler implementations

  public class ActiveStreamsHandler : IStandbyHandler
  {
    IController _controller;
    public ActiveStreamsHandler(IController controller)
    {
      _controller = controller;
    }
    public bool DisAllowShutdown
    {
      get { return (_controller.ActiveStreams > 0); }
    }
    public string HandlerName
    {
      get { return "ActiveStreamsHandler"; }
    }
  }

  public class EpgGrabbingHandler : IStandbyHandler
  {
    IController _controller;
    public EpgGrabbingHandler(IController controller)
    {
      _controller = controller;
    }
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
    public string HandlerName
    {
      get { return "EpgGrabbingHandler"; }
    }
  }

  public class SetupActiveHandler : IStandbyHandler
  {
    public bool DisAllowShutdown
    {
      get
      {
        System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("SetupTv");
        if (processes.Length > 0)
          return true;
        return false;
      }
    }
    public string HandlerName
    {
      get { return "SetupActiveHandler"; }
    }
  }

  #endregion

  #region IWakeupHandler implementations

  public class ScheduledRecordingsHandler : IWakeupHandler
  {
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      DateTime nextWakeuptime = DateTime.MaxValue;
      foreach (Schedule schedule in Schedule.ListAll())
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.StartTime < nextWakeuptime && schedule.StartTime >= earliestWakeupTime)
          nextWakeuptime = schedule.StartTime;
      }
      return nextWakeuptime;
    }
    public string HandlerName
    {
      get { return "ScheduledRecordingsHandler"; }
    }
  }

  public class TestWakeupHandler : IWakeupHandler
  {
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      return earliestWakeupTime.AddMinutes(5);
    }
    public string HandlerName
    {
      get { return "TestWakeupHandler"; }
    }
  }

  #endregion
}
