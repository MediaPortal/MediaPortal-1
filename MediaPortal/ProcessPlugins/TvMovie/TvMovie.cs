#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using Timer=System.Threading.Timer;

namespace ProcessPlugins.TvMovie
{
  [PluginIcons("ProcessPlugins.TvMovie.tvmovie.gif", "ProcessPlugins.TvMovie.tvmovie_inactive.gif")]
  public class TvMovie : ISetupForm, IPlugin
  {
    private TvMovieDatabase _database;
    private TvMovieSchedules _schedules;
    private Timer _epgImportTimer;
    private bool _isImporting = false;

    private void ImportThread()
    {
      _isImporting = true;

      Log.Debug("TVMovie: Checking database");
      _database = new TvMovieDatabase();

      if (_database.WasUpdated)
      {
        TVDatabase.SupressEvents = true;

        //GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLEGUIDEREFRESH, 0, 0, 0, 0, 0, null);
        //GUIGraphicsContext.SendMessage(message);

        _database.Import();

        //message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLEGUIDEREFRESH, 0, 0, 0, 0, 0, null);
        //GUIGraphicsContext.SendMessage(message);

        TVDatabase.SupressEvents = false;
      }

      _isImporting = false;
    }

    private void StartImportThread(Object stateInfo)
    {
      if (!_isImporting)
      {
        Thread importThread = new Thread(new ThreadStart(ImportThread));
        importThread.Priority = ThreadPriority.Lowest;
        importThread.Name = "TVMovie Importer";
        importThread.Start();
      }
    }

    #region ISetupForm Members

    public string PluginName()
    {
      return "TV Movie Clickfinder";
    }

    public string Description()
    {
      return "Import TV Movie Clickfinder EPG data (disabled if no Clickfinder is installed)";
    }

    public string Author()
    {
      return "mPod";
    }

    public void ShowPlugin()
    {
      if (TvMovieDatabase.DatabasePath != string.Empty)
      {
        Form setup = new TvMovieSettings();
        setup.ShowDialog();
      }
      else
      {
        MessageBox.Show("Can't find TV Movie Clickfinder database.\nPlease install TV Movie Clickfinder first.",
                        "Error locating Clickfinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public bool CanEnable()
    {
      return (TvMovieDatabase.DatabasePath != string.Empty);
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    #endregion

    #region IPlugin Members

    public void Start()
    {
      TimerCallback timerCallBack = new TimerCallback(StartImportThread);
      _epgImportTimer = new Timer(timerCallBack, null, 10000, 1800000);

      _schedules = new TvMovieSchedules();
      _schedules.Start();
    }

    public void Stop()
    {
      if (_database != null)
      {
        _database.Canceled = true;
      }
      _epgImportTimer.Dispose();
      _schedules.Stop();
    }

    #endregion
  }
}