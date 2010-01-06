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

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Change and tweak settings like Video codecs, skins, etc from inside MP
  /// </summary>
  [PluginIcons("WindowPlugins.GUISettings.Settings.gif", "WindowPlugins.GUISettings.SettingsDisabled.gif")]
  public class GUISettings : GUIInternalWindow, ISetupForm, IShowPlugin
  {
    [SkinControl(11)] protected GUIButtonControl btnMiniDisplay = null;
    [SkinControl(10)] protected GUIButtonControl btnTV = null;

    public GUISettings()
    {
      GetID = (int)Window.WINDOW_SETTINGS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings.xml");
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            btnMiniDisplay.Visible = MiniDisplayHelper.IsSetupAvailable();
            btnTV.Visible = Util.Utils.UsingTvServer;
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT: {}
          break;
      }
      return base.OnMessage(message);
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public string PluginName()
    {
      return "Settings";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(5);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Configure MediaPortal with wizards and a graphical user interface";
    }

    public void ShowPlugin() {}

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}