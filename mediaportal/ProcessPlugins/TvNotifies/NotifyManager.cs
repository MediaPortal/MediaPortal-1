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
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;

namespace MediaPortal.TvNotifies
{
  [PluginIcons("ProcessPlugins.TvNotifies.Notifybutton.gif", "ProcessPlugins.TvNotifies.Notifybutton_disabled.gif")]
  public class NotifyManager : ISetupForm, IPlugin //, IShowPlugin
  {
    private Timer _timer;
    // flag indicating that notifies have been added/changed/removed
    private bool _notifiesListChanged;
    private int _preNotifyConfig;
    //list of all notifies (alert me n minutes before program starts)
    private List<TVNotify> _notifiesList;

    public NotifyManager()
    {
      _notifiesList = new List<TVNotify>();
      TVDatabase.OnNotifiesChanged += new TVDatabase.OnChangedHandler(OnNotifiesChanged);
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _preNotifyConfig = xmlreader.GetValueAsInt("movieplayer", "notifyTVBefore", 300);
      }
      _timer = new Timer();
      // check every 15 seconds for notifies
      _timer.Interval = 15000;
      _timer.Enabled = false;
      _timer.Tick += new EventHandler(_timer_Tick);
    }

    private void OnNotifiesChanged()
    {
      _notifiesListChanged = true;
    }

    private void LoadNotifies()
    {
      _notifiesList.Clear();
      TVDatabase.GetNotifies(_notifiesList, true);
    }


    private void _timer_Tick(object sender, EventArgs e)
    {
      if (_notifiesListChanged)
      {
        LoadNotifies();
        _notifiesListChanged = false;
      }
      DateTime preNotifySecs = DateTime.Now.AddSeconds(_preNotifyConfig);
      for (int i = 0; i < _notifiesList.Count; ++i)
      {
        TVNotify notify = _notifiesList[i];
        if (preNotifySecs > notify.Program.StartTime)
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
      return "Display notify OSD before a program starts";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
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
      return "Frodo, rtv";
    }

    public string PluginName()
    {
      return "TV Notifier";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      Form NotifySetup = new NotifySetupForm();
      NotifySetup.ShowDialog();
    }

    #endregion
  }
}