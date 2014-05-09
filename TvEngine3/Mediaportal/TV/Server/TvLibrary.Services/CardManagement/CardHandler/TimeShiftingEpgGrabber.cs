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
using Mediaportal.TV.Server.TVLibrary.Services;

namespace Mediaportal.TV.Server.TVLibrary
{
  internal class TimeShiftingEpgGrabber : IEpgGrabberCallBack, IDisposable
  {
    #region Variables

    private readonly ITVCard _card;
    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();
    private DateTime _grabStartTime;
    private bool _updateThreadRunning;
    private readonly EpgDBUpdater _dbUpdater;
    private bool _disposed;

    #endregion

    public TimeShiftingEpgGrabber(ITVCard card)
    {
      _card = card;
      _dbUpdater = new EpgDBUpdater(ServiceManager.Instance.InternalControllerService.OnImportEpgPrograms, "TimeShiftingEpgGrabber", false);
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
      this.LogInfo("TimeshiftingEpgGrabber: timeout after {0} mins", ts.TotalMinutes);
      _epgTimer.Enabled = false;
      IEpgGrabber epgGrabber = _card.EpgGrabberInterface;
      if (epgGrabber != null)
      {
        epgGrabber.AbortGrabbing();
      }
    }

    #region IEpgCallBack implementation

    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public void OnEpgCancelled()
    {
      this.LogInfo("Timeshifting epg grabber stopped.");
      _epgTimer.Enabled = false;
    }

    /// <summary>
    /// Gets called when epg has been received
    /// Should be overriden by the class
    /// </summary>
    public void OnEpgReceived(IList<EpgChannel> epg)
    {
      if (epg == null || epg.Count == 0)
      {
        this.LogInfo("TimeshiftingEpgGrabber: No epg received.");
        return;
      }
      this.LogInfo("TimeshiftingEpgGrabber: OnEPGReceived got {0} channels", epg.Count);
      Thread workerThread = new Thread(new ParameterizedThreadStart(UpdateDatabaseThread));
      workerThread.IsBackground = true;
      workerThread.Name = "EPG Update thread";
      workerThread.Start(epg);
      _epgTimer.Enabled = false;
    }

    #endregion

    #region Database update routines

    private void UpdateDatabaseThread(object epgObj)
    {
      try
      {
        IList<EpgChannel> epg = epgObj as IList<EpgChannel>;
        if (epg == null)
        {
          return;
        }
        _updateThreadRunning = true;
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        _dbUpdater.ReloadConfig();
        foreach (EpgChannel epgChannel in epg)
        {
          _dbUpdater.UpdateEpgForChannel(epgChannel);
        }
        ProgramManagement.SynchProgramStatesForAllSchedules(ScheduleManagement.ListAllSchedules());
        this.LogInfo("TimeshiftingEpgGrabber: Finished updating the database.");
      }
      finally
      {
        _updateThreadRunning = false;         
      }            
    }

    #endregion

    #region IDisposable Members    

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> when Dispose() is called explicitly.</param>
    protected virtual void Dispose(bool isDisposing)
    {
      if (isDisposing)
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

    ~TimeShiftingEpgGrabber()
    {
      Dispose(false);
    }

    #endregion
  }
}