/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvDatabase;
using System.Threading;
using TvLibrary.Log;

namespace TvEngine
{
  public class ConflictsManager : ITvServerPlugin
  {
    #region variables
    bool _cmWorkerThreadRunning = false;
    System.Timers.Timer _cmWorkerTimer;
    TvBusinessLayer cmLayer = new TvBusinessLayer();
    IList _schedules = Schedule.ListAll();

    #endregion

    #region properties
    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get
      {
        return "ConflictsManager";
      }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.0";
      }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get
      {
        return "Broceliande";
      }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get
      {
        return true;
      }
    }
    #endregion

    #region public members

    /// <summary>
    /// Starts the plugin
    /// </summary>
    public void Start(IController controller)
    {
      Log.WriteFile("plugin: ConflictsManager started");
      _cmWorkerTimer = new System.Timers.Timer();
      _cmWorkerTimer.Interval = 60000;
      _cmWorkerTimer.Enabled = true;
      _cmWorkerTimer.Elapsed += new System.Timers.ElapsedEventHandler(_cmWorkerTimer_Elapsed);
    }

    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      Log.WriteFile("plugin: ConflictsManager stopped");
      if (_cmWorkerTimer != null)
      {
        _cmWorkerTimer.Enabled = false;
        _cmWorkerTimer.Dispose();
        _cmWorkerTimer = null;
      }
    }

    /// <summary>
    /// Plugin setup form
    /// </summary>
    public SetupTv.SectionSettings Setup
    {
      get { return null; }
    }

    #endregion

    #region private members

    /// <summary>
    /// timer event callback
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void _cmWorkerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      if (!_cmWorkerThreadRunning) // avoids reentrancy
      {
        _cmWorkerThreadRunning = true;
        UpdateConflicts();
        _cmWorkerThreadRunning = false;
      }
    }

    /// <summary>
    /// Parses the sheduled recordings
    /// and updates the conflicting recordings table
    /// </summary>
    protected void UpdateConflicts()
    {
      Log.WriteFile("ConflictManager: Updating conflicts list");
      _schedules = Schedule.ListAll();

      foreach (Schedule _schedule in _schedules)
      {
        foreach (Schedule _otherschedule in _schedules)
        {
          if (IsOverlap(_schedule, _otherschedule))
          {
            //todo
          }
        }
      }
    }

    private void Init()
    {
    }

    /// <summary>
    /// Checks if 2 scheduled recordings are overlapping
    /// </summary>
    /// <param name="Schedule 1"></param>
    /// <param name="Schedule 2"></param>
    /// <returns>true if sheduled recordings are overlapping</returns>
    static private bool IsOverlap(Schedule sched_1, Schedule sched_2)
    {
      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2  ------------------e
      if ((sched_2.StartTime >= sched_1.StartTime && sched_2.StartTime < sched_1.EndTime) ||
          (sched_2.StartTime <= sched_1.StartTime && sched_2.EndTime >= sched_1.EndTime) ||
          (sched_2.EndTime > sched_1.StartTime && sched_2.StartTime <= sched_1.EndTime)) return true;
      return false;
    }

    /// <summary>Tries to assign a recording to a card</summary>
    /// <param name="arec">The recording you wan't to try to assign</param>
    /// <param name="cardrec">An array of Recordings lists (one list for each card)</param>
    /// <returns>True if succeed, False either</returns>
     private bool AssignScheduleToCard(Schedule _Sched)
    {
      IList _cards = cmLayer.Cards;
      foreach (Card _card in _cards)
      {
        if (_card.canViewTvChannel(_Sched.IdChannel)) _Sched.RecommendedCard = _card.IdCard;
      }
      return false;
    }

    #endregion
  }
}
