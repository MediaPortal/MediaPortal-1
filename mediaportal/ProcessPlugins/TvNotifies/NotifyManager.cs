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
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace ProcessPlugins.TVNotifies
{
  public class NotifyManager: IPlugin, ISetupForm 
  {
    System.Windows.Forms.Timer _timer;
    // flag indicating that notifies have been added/changed/removed
    bool _notifiesListChanged;
    //list of all notifies (alert me 2 minutes before program starts)
    List<TVNotify> _notifiesList;

    public NotifyManager()
    {
      _notifiesList = new List<TVNotify>();
      TVDatabase.OnNotifiesChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(OnNotifiesChanged);
      _timer = new System.Windows.Forms.Timer();
      _timer.Interval = 30000;
      _timer.Enabled = false;
      _timer.Tick += new EventHandler(_timer_Tick);
    }

    void OnNotifiesChanged()
    {
      _notifiesListChanged = true;
    }
    void LoadNotifies()
    {
      _notifiesList.Clear();
      TVDatabase.GetNotifies(_notifiesList, true);
    }


    void _timer_Tick(object sender, EventArgs e)
    {
      if (_notifiesListChanged)
      {
        LoadNotifies(); 
        _notifiesListChanged = false;
      }
      DateTime dt5Mins = DateTime.Now.AddMinutes(5);
      for (int i = 0; i < _notifiesList.Count; ++i)
      {
        TVNotify notify = _notifiesList[i];
        if (dt5Mins > notify.Program.StartTime)
        {
          TVDatabase.DeleteNotify(notify);
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
          msg.Object = notify.Program;
          GUIGraphicsContext.SendMessage(msg);
          msg = null;
        }
      }
    }

    #region IPlugin Members

    public void Start()
    {
      LoadNotifies();
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
      return "Sends notifications 5 minutes before a program starts";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      // TODO:  Add CallerIdPlugin.GetWindowId implementation
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
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
      return "TV Notifier";
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
