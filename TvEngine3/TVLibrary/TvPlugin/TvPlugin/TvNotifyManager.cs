#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

using Gentle.Common;
using Gentle.Framework;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;

namespace TvPlugin
{
  public class TvNotifyManager
  {
    System.Windows.Forms.Timer _timer;
    // flag indicating that notifies have been added/changed/removed
    static bool _notifiesListChanged;
    int _preNotifyConfig;
    //list of all notifies (alert me n minutes before program starts)
    IList _notifiesList;

    public TvNotifyManager()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        _preNotifyConfig = xmlreader.GetValueAsInt("movieplayer", "notifyTVBefore", 300);
      _timer = new System.Windows.Forms.Timer();

      // check every 15 seconds for notifies
      _timer.Interval = 15000;
      _timer.Enabled = true;
      _timer.Tick += new EventHandler(_timer_Tick);
    }

    static public void OnNotifiesChanged()
    {
      Log.Info("TvNotify:OnNotifiesChanged");
      _notifiesListChanged = true;
    }

    void LoadNotifies()
    {
      try
      {
        Log.Info("TvNotify:LoadNotifies");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
        sb.AddConstraint(Operator.Equals, "notify", 1);
        SqlStatement stmt = sb.GetStatement(true);
        _notifiesList = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
        if (_notifiesList != null)
        {
          Log.Info("TvNotify: {0} notifies", _notifiesList.Count);
        }
      }
      catch (Exception )
      {
      }
    }


    void _timer_Tick(object sender, EventArgs e)
    {
      if (_notifiesListChanged)
      {
        LoadNotifies();
        _notifiesListChanged = false;
      }
      if (_notifiesList == null) return;
      if (_notifiesList.Count == 0) return;
      DateTime preNotifySecs = DateTime.Now.AddSeconds(_preNotifyConfig);
      foreach (Program program in _notifiesList)
      {
        if (preNotifySecs > program.StartTime)
        {
          Log.Info("Notify {0} on {1} start {2}", program.Title, program.ReferencedChannel().Name, program.StartTime);
          program.Notify = false;
          program.Persist();

          MediaPortal.TV.Database.TVProgram tvProg = new MediaPortal.TV.Database.TVProgram();
          tvProg.Channel = program.ReferencedChannel().Name;
          tvProg.Title = program.Title;
          tvProg.Description = program.Description;
          tvProg.Genre = program.Genre;
          tvProg.Start = Utils.datetolong(program.StartTime);
          tvProg.End = Utils.datetolong(program.EndTime);

          _notifiesList.Remove(program);
          Log.Info("send notify");
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
          msg.Object = tvProg;
          GUIGraphicsContext.SendMessage(msg);
          msg = null;
          Log.Info("send notify done");
          return;
        }
      }
    }
  }
}
