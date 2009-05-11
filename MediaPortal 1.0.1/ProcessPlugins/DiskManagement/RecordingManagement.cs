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
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace ProcessPlugins.DiskSpace
{
  public class RecordingManagement : IPlugin, ISetupForm
  {
    private Timer _timer;

    public RecordingManagement()
    {
      _timer = new Timer();
      _timer.Interval = 4*60*60*1000;
      _timer.Enabled = false;
      _timer.Tick += new EventHandler(OnTimerElapsed);
    }

    private void OnTimerElapsed(object sender, EventArgs e)
    {
      DeleteOldRecordings();
    }

    /// <summary>
    /// This method will get all the tv-recordings present in the tv database
    /// For each recording it looks at the Keep until settings. If the recording should be
    /// deleted by date, then it will delete the recording from the database, and harddisk
    /// if the the current date > keep until date
    /// </summary>
    /// <remarks>Note, this method will only work after a day-change has occured(and at startup)
    /// </remarks>
    private void DeleteOldRecordings()
    {
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        if (!rec.ShouldBeDeleted)
        {
          continue;
        }

        Log.Info("RecordingManagement: delete old recording: {0} date: {1}",
                 rec.FileName,
                 rec.StartTime.ToShortDateString());
        Recorder.DeleteRecording(rec);
      }
    }

    #region IPlugin Members

    public void Start()
    {
      _timer.Enabled = true;
    }

    public void Stop()
    {
      _timer.Enabled = false;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Deletes old TV recordings by date";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      // TODO:  Add CallerIdPlugin.GetWindowId implementation
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      // TODO:  Add CallerIdPlugin.GetHome implementation
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "TV Recordings Cleanup";
    }

    public bool HasSetup()
    {
      // TODO:  Add CallerIdPlugin.HasSetup implementation
      return false;
    }

    public void ShowPlugin()
    {
      // TODO:  Add CallerIdPlugin.ShowPlugin implementation
    }

    #endregion
  }
}