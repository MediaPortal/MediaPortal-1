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
        return "0.0.0.1";
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
    /// timer event wich starts UpdateConflicts() thread
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void _cmWorkerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      UpdateConflicts();
    }
    /// <summary>
    /// Parses the sheduled recordings
    /// and updates the conflicting recordings table
    /// </summary>
    protected void UpdateConflicts()
    {
      if (!_cmWorkerThreadRunning)
      {
        _cmWorkerThreadRunning = true;
        Thread workerThread = new Thread(new ThreadStart(cmWorkThread));
        workerThread.Priority = ThreadPriority.Lowest;
        workerThread.Start();
      }
    }
    /// <summary>
    /// Conflicts Manager Worker thread
    /// </summary>
    void cmWorkThread()
    {
      Log.WriteFile("ConflictManager: Main worker thread");
      TvBusinessLayer cmLayer = new TvBusinessLayer();
      IList _cards = cmLayer.Cards;
      List<Schedule> _shedules;
      try
      {
      }
      catch (Exception)
      {
      }
      _cmWorkerThreadRunning = false;
    }

    private void Init()
    {

    }
    #endregion
  }
}
