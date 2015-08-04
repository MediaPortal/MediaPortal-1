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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Services;

namespace Mediaportal.TV.Server.TVLibrary
{
  internal class TimeShiftingEpgGrabber : IEpgGrabberCallBack, IDisposable
  {
    #region Variables

    private readonly ITuner _tuner = null;
    private IChannel _tuningDetail = null;
    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();

    private bool _updateThreadRunning = false;
    private readonly EpgDBUpdater _dbUpdater;
    private bool _disposed;

    private bool _isGrabbing = false;
    private bool _isNewTransmitter = false;
    private DateTime _grabStartTime = DateTime.MinValue;
    private DateTime _grabFinishTime = DateTime.MinValue;

    #endregion

    public TimeShiftingEpgGrabber(ITuner tuner)
    {
      _tuner = tuner;
      _dbUpdater = new EpgDBUpdater(ServiceManager.Instance.InternalControllerService.OnImportEpgPrograms, "TS EPG grabber");
      _epgTimer.Elapsed += EpgTimerElapsed;
      _epgTimer.Interval = 60000;
      _epgTimer.Enabled = true;
    }

    public void StartGrab(IChannel tuningDetail)
    {
      _isNewTransmitter = true;
      _tuningDetail = tuningDetail;
    }

    public void StopGrab()
    {
      this.LogInfo("TS EPG grabber: stopped");
      _tuningDetail = null;
      if (_isGrabbing)
      {
        IEpgGrabber epgGrabber = _tuner.EpgGrabberInterface;
        if (epgGrabber != null)
        {
          epgGrabber.AbortGrabbing();
        }
      }
    }

    private void EpgTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (_tuningDetail == null || _tuner.EpgGrabberInterface == null)
      {
        return;
      }

      IEpgGrabber epgGrabber = _tuner.EpgGrabberInterface;
      if (!SettingsManagement.GetValue("tunerEpgGrabberTimeShiftingRecordingEnabled", false) || !_tuner.IsEpgGrabbingAllowed)
      {
        if (_isGrabbing)
        {
          this.LogInfo("TS EPG grabber: disabled");
          epgGrabber.AbortGrabbing();
        }
        return;
      }

      if (_isNewTransmitter)
      {
        if (_updateThreadRunning)
        {
          return;
        }
        if (_isGrabbing)
        {
          epgGrabber.AbortGrabbing();
        }
        _isNewTransmitter = false;
        epgGrabber.GrabEpg(_tuningDetail, this);
        _isGrabbing = true;
        _grabStartTime = DateTime.Now;
        this.LogInfo("TS EPG grabber: started");
        return;
      }

      if (_isGrabbing)
      {
        if (!_updateThreadRunning && (DateTime.Now - _grabStartTime).TotalMinutes >= SettingsManagement.GetValue("tunerEpgGrabberIdleTimeOut", 10))
        {
          this.LogInfo("TS EPG grabber: timed out");
          epgGrabber.AbortGrabbing();
          _grabFinishTime = DateTime.Now;
        }
        return;
      }

      if ((DateTime.Now - _grabFinishTime).TotalMinutes >= SettingsManagement.GetValue("dvbEpgGrabberTimeShiftingRecordingRefresh", 15))
      {
        this.LogInfo("TS EPG grabber: refreshing");
        epgGrabber.GrabEpg(_tuningDetail, this);
        _isGrabbing = true;
        _grabStartTime = DateTime.Now;
      }
    }

    #region IEpgCallBack implementation

    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public void OnEpgCancelled()
    {
      this.LogInfo("TS EPG grabber: stopped");
      _isGrabbing = false;
    }

    /// <summary>
    /// Called when electronic programme guide data grabbing is complete.
    /// </summary>
    /// <param name="tuningDetail">The tuning details of the transmitter from which the EPG was grabbed.</param>
    /// <param name="epg">The grabbed data.</param>
    public void OnEpgReceived(IChannel tuningDetail, IDictionary<IChannel, IList<EpgProgram>> epg)
    {
      if (epg == null || epg.Count == 0)
      {
        this.LogInfo("TS EPG grabber: finished, no EPG found");
        _isGrabbing = false;
        _grabFinishTime = DateTime.Now;
        return;
      }

      this.LogInfo("TS EPG grabber: collected, {0} channel(s)", epg.Count);
      ThreadStart starter = delegate { UpdateDatabaseThread(tuningDetail, epg); };
      Thread workerThread = new Thread(starter);
      workerThread.IsBackground = true;
      workerThread.Priority = ThreadPriority.Lowest;
      workerThread.Name = "EPG database updater";
      workerThread.Start();
    }

    #endregion

    #region Database update routines

    private void UpdateDatabaseThread(IChannel tuningDetail, IDictionary<IChannel, IList<EpgProgram>> epg)
    {
      try
      {
        _updateThreadRunning = true;
        _dbUpdater.ReloadConfig();
        foreach (var epgChannel in epg)
        {
          _dbUpdater.UpdateEpgForChannel(tuningDetail, epgChannel);
        }
        ProgramManagement.SynchProgramStatesForAllSchedules();
        this.LogInfo("TS EPG grabber: finished");
      }
      finally
      {
        _updateThreadRunning = false;
        _isGrabbing = false;
        _grabFinishTime = DateTime.Now;
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