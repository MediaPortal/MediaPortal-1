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
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.EPG;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary
{
  internal class TimeShiftingEPGGrabber : BaseEpgGrabber, IDisposable
  {

    #region Variables

    private readonly ITVCard _card;
    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();
    private DateTime _grabStartTime;
    private List<EpgChannel> _epg;
    private bool _updateThreadRunning;
    private readonly EpgDBUpdater _dbUpdater;
    private bool _disposed;

    #endregion

    public TimeShiftingEPGGrabber(IEpgEvents epgEvents, ITVCard card)
    {
      _card = card;
      _dbUpdater = new EpgDBUpdater(epgEvents, "TimeshiftingEpgGrabber", false);
      _updateThreadRunning = false;
      _epgTimer.Elapsed += _epgTimer_Elapsed;
    }

    private void LoadSettings()
    {

      double timeout = SettingsManagement.GetValue("timeshiftingEpgGrabberTimeout", 2.0);
      _epgTimer.Interval = timeout * 60000;
    }

    public bool StartGrab()
    {
      if (_updateThreadRunning)
      {
        this.LogInfo("Timeshifting epg grabber not started because the db update thread is still running.");
        return false;
      }
      LoadSettings();
      this.LogInfo("Timeshifting epg grabber started.");
      _grabStartTime = DateTime.Now;
      _epgTimer.Enabled = true;
      return true;
    }

    private void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      TimeSpan ts = DateTime.Now - _grabStartTime;
      this.LogInfo("TimeshiftingEpgGrabber: timeout after {1} mins", ts.TotalMinutes);
      _epgTimer.Enabled = false;
      _card.AbortGrabbing();
    }

    #region BaseEpgGrabber implementation

    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public new void OnEpgCancelled()
    {
      this.LogInfo("Timeshifting epg grabber stopped.");
      _card.IsEpgGrabbing = false;
      _epgTimer.Enabled = false;
    }

    /// <summary>
    /// Gets called when epg has been received
    /// Should be overriden by the class
    /// </summary>
    /// <returns></returns>
    public override int OnEpgReceived()
    {
      List<EpgChannel> grabbedEpg = null;
      try
      {
        grabbedEpg = _card.Epg;
      }
      catch (Exception ex)
      {
        this.LogInfo("TimeshiftingEpgGrabber: Error while retrieving the epg data: ", ex);
      }
      if (grabbedEpg == null)
      {
        this.LogInfo("TimeshiftingEpgGrabber: No epg received.");
        return 0;
      }
      _epg = new List<EpgChannel>(grabbedEpg);
      this.LogInfo("TimeshiftingEpgGrabber: OnEPGReceived got {0} channels", _epg.Count);
      if (_epg.Count == 0)
        this.LogInfo("TimeshiftingEpgGrabber: No epg received.");
      else
      {
        Thread workerThread = new Thread(UpdateDatabaseThread);
        workerThread.IsBackground = true;
        workerThread.Name = "EPG Update thread";
        workerThread.Start();
      }
      _epgTimer.Enabled = false;
      return 0;
    }

    #endregion

    #region Database update routines

    private void UpdateDatabaseThread()
    {
      try
      {
        if (_epg == null)
        {
          return;
        }
        _card.IsEpgGrabbing = true;
        _updateThreadRunning = true;
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        _dbUpdater.ReloadConfig();
        foreach (EpgChannel epgChannel in _epg)
        {
          _dbUpdater.UpdateEpgForChannel(epgChannel);
        }
        ProgramManagement.SynchProgramStatesForAllSchedules();
        this.LogInfo("TimeshiftingEpgGrabber: Finished updating the database.");
        _epg.Clear();
        _epg = null;
      }
      finally
      {
        _card.IsEpgGrabbing = false;
        _updateThreadRunning = false;         
      }            
    }

    #endregion

    #region IDisposable Members    

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // get rid of managed resources
        if (!_disposed)
        {
          _epgTimer.Dispose();
          _disposed = true;
        }
      }

      // get rid of unmanaged resources
    }


    /// <summary>
    /// Disposes the EPG card
    /// </summary>    
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~TimeShiftingEPGGrabber()
    {
      Dispose(false);
    }

    #endregion
  }
}