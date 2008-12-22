/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvDatabase;
using System.Collections.Specialized;

namespace TvLibrary
{
  class TimeShiftingEPGGrabber: BaseEpgGrabber
  {
    #region Variables
    private ITVCard _card;
    private System.Timers.Timer _epgTimer = new System.Timers.Timer();
    DateTime _grabStartTime;
    private List<EpgChannel> _epg;
    private bool _updateThreadRunning;
    private EpgDBUpdater _dbUpdater;
    #endregion

    public TimeShiftingEPGGrabber(ITVCard card)
    {
      _card = card;
      _dbUpdater = new EpgDBUpdater("TimeshiftingEpgGrabber", false);
      _updateThreadRunning = false;
      _epgTimer.Elapsed += new System.Timers.ElapsedEventHandler(_epgTimer_Elapsed);
    }
    private void LoadSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      double timeout;
      if (!double.TryParse(layer.GetSetting("timeshiftingEpgGrabberTimeout", "2").Value, out timeout))
        timeout = 2;
      _epgTimer.Interval = timeout * 60000;
    }
    public bool StartGrab()
    {
      if (_updateThreadRunning)
      {
        Log.Log.Info("Timeshifting epg grabber not started because the db update thread is still running.");
        return false;
      }
      else
      {
        LoadSettings();
        Log.Log.Info("Timeshifting epg grabber started.");
        _grabStartTime = DateTime.Now;
        _epgTimer.Enabled = true;
        return true;
      }
    }
    void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      TimeSpan ts = DateTime.Now - _grabStartTime;
      Log.Log.Epg("TimeshiftingEpgGrabber: timeout after {1} mins", ts.TotalMinutes);
      _epgTimer.Enabled = false;
      _card.AbortGrabbing();
    }

    #region BaseEpgGrabber implementation
    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public void OnEpgCancelled()
    {
      Log.Log.Info("Timeshifting epg grabber stopped.");
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
      catch (Exception)
      {
      }
      if (grabbedEpg == null)
      {
        Log.Log.Epg("TimeshiftingEpgGrabber: No epg received.");
        return 0;
      }
      _epg =new List<EpgChannel>(grabbedEpg);
      Log.Log.Epg("TimeshiftingEpgGrabber: OnEPGReceived got {0} channels",_epg.Count);
      if (_epg.Count == 0)
        Log.Log.Epg("TimeshiftingEpgGrabber: No epg received.");
      else
      {
        Thread workerThread = new Thread(new ThreadStart(UpdateDatabaseThread));
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
      if (_epg == null)
        return;

      _updateThreadRunning = true;
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      _dbUpdater.ReloadConfig();
      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (EpgChannel epgChannel in _epg)
        _dbUpdater.UpdateEpgForChannel(epgChannel);
      Log.Log.Epg("TimeshiftingEpgGrabber: Finished updating the database.");
      _epg.Clear();
      _epg = null;
      _card.IsEpgGrabbing = false;
      _updateThreadRunning = false;
    }
    #endregion
  }
}
