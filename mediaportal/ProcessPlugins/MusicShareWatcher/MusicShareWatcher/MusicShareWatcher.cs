#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Services;

namespace MediaPortal.MusicShareWatcher
{
  public partial class MusicShareWatcher : Form
  {
    private bool bMonitoring;
    private static MusicShareWatcherHelper watcher = null;

    public MusicShareWatcher()
    {
      InitializeComponent();
      Thread.CurrentThread.Name = "MusicShareWatcherApp";
      bMonitoring = true;
      // Setup the Watching
      watcher = new MusicShareWatcherHelper();
      watcher.SetMonitoring(true);
      watcher.StartMonitor();
    }

    #region CommonMethods

    // Hide the window on startup into the tray
    private void OnResize(object sender, EventArgs e)
    {
      if (FormWindowState.Minimized == WindowState)
      {
        Hide();
      }
    }

    // Close the Watcher Application
    private void closeMenuItem_Click(object sender, EventArgs e)
    {
      Log.Info(LogType.MusicShareWatcher, "MusicShareWatcher terminated.");
      Application.Exit();
    }

    // Enable / Disable the monitoring of shares
    private void monitoringEnabledMenuItem_Click(object sender, EventArgs e)
    {
      if (bMonitoring)
      {
        bMonitoring = false;
        this.monitoringEnabledMenuItem.Checked = false;
        watcher.ChangeMonitoring(false);
      }
      else
      {
        bMonitoring = true;
        this.monitoringEnabledMenuItem.Checked = true;
        watcher.ChangeMonitoring(true);
      }
    }

    // React on Windows System Shutdown
    private const int WM_QUERYENDSESSION = 0x11;

    protected override void WndProc(ref Message msg)
    {
      if (msg.Msg == WM_QUERYENDSESSION)
      {
        Log.Info(LogType.MusicShareWatcher, "MusicShareWatcher terminated because of System/Session shutdown.");
        // If system is shutting down, allow exit.
        Application.Exit();
      }
      base.WndProc(ref msg);
    }

    #endregion CommonMethods
  }
}