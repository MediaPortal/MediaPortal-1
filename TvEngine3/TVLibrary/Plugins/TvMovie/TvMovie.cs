#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using SetupTv;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using TvEngine;


namespace TvEngine
{
  public class TvMovie : ITvServerPlugin
  {
    private TvMovieDatabase _database;
    private System.Threading.Timer _stateTimer;
    private bool _isImporting = false;

    private void ImportThread()
    {
      _isImporting = true;

      Log.Debug("TVMovie: Checking database");
      _database = new TvMovieDatabase();
      _database.Connect();

      if (_database.WasUpdated)
      {
        //TVDatabase.SupressEvents = true;

        _database.Import();

        //TVDatabase.SupressEvents = false;
      }

      _isImporting = false;
    }

    private void StartImportThread(Object stateInfo)
    {
      if (!_isImporting)
      {
        Thread importThread = new Thread(new ThreadStart(ImportThread));
        importThread.Priority = ThreadPriority.Lowest;
        importThread.Start();
      }
    }

    #region ITvServerPlugin Members

    public string Name
    {
      get { return "TV Movie Clickfinder Import"; }
    }

    public string Version
    {
      get { return "0.1"; }
    }

    public string Author
    {
      get { return "mPod"; }
    }

    public bool MasterOnly
    {
      get { return false; }
    }

    public void Start(IController controller)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      if (layer.GetSetting("TvMovieEnabled", "false").Value != "true")
        return;

      TimerCallback timerCallBack = new TimerCallback(StartImportThread);
      _stateTimer = new System.Threading.Timer(timerCallBack, null, 10000, 1800000);
    }

    public void Stop()
    {
      if (_database != null)
        _database.Canceled = true;
      _stateTimer.Dispose();
    }

    public SetupTv.SectionSettings Setup
    {
      get { return new SetupTv.Sections.TvMovieSetup(); }
    }

    #endregion
  }
}
