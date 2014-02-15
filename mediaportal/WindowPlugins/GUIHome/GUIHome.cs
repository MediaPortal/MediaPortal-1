#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System;
using System.Collections;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

#endregion

namespace MediaPortal.GUI.Home
{
  /// <summary>
  /// The implementation of the GUIHome Window.
  /// </summary>
  public class GUIHome : GUIHomeBaseWindow
  {
    #region Constructors/Destructors

    public GUIHome()
    {
      GetID = (int)Window.WINDOW_HOME;
    }

    #endregion

    #region <Base class> Overrides

    public override bool Init()
    {
      return (Load(GUIGraphicsContext.GetThemedSkinFile(@"\myHome.xml")));
    }

    protected override void LoadButtonNames()
    {
      if (menuMain == null)
      {
        return;
      }
      menuMain.ButtonInfos.Clear();
      ArrayList plugins = PluginManager.SetupForms;
      int myPluginsCount = 0;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        foreach (ISetupForm setup in plugins)
        {
          string plugInText;
          string focusTexture;
          string nonFocusTexture;
          string hover;
          string nonFocusHover;
          if (setup.GetHome(out plugInText, out focusTexture, out nonFocusTexture, out hover))
          {
            if (setup.PluginName().Equals("Home"))
            {
              continue;
            }
            IShowPlugin showPlugin = setup as IShowPlugin;
            if (_useMyPlugins)
            {
              string showInHome = xmlreader.GetValue("home", setup.PluginName());
              if ((showInHome == null) || (showInHome.Length < 1))
              {
                if (showPlugin == null)
                {
                  continue;
                }
                if (showPlugin.ShowDefaultHome() == false)
                {
                  myPluginsCount++;
                  continue;
                }
              }
              else
              {
                if (showInHome.ToLowerInvariant().Equals("no"))
                {
                  myPluginsCount++;
                  continue;
                }
              }
            }
            int index = xmlreader.GetValueAsInt("pluginSorting", setup.PluginName(), Int32.MaxValue);
            if ((focusTexture == null) || (focusTexture.Length < 1))
            {
              focusTexture = setup.PluginName();
            }
            if ((nonFocusTexture == null) || (nonFocusTexture.Length < 1))
            {
              nonFocusTexture = setup.PluginName();
            }
            if ((hover == null) || (hover.Length < 1))
            {
              hover = setup.PluginName();
            }
            focusTexture = GetFocusTextureFileName(focusTexture);
            nonFocusTexture = GetNonFocusTextureFileName(nonFocusTexture);
            nonFocusHover = GetNonFocusHoverFileName(hover);
            hover = GetHoverFileName(hover);
            menuMain.ButtonInfos.Add(new MenuButtonInfo(plugInText, setup.GetWindowId(), focusTexture, nonFocusTexture,
                                                        hover, nonFocusHover, index));
          }
        }

        if ((_useMyPlugins) && (myPluginsCount > 0))
        {
          string focusTexture = GetFocusTextureFileName("my plugins");
          string nonFocusTexture = GetNonFocusTextureFileName("my plugins");
          string hover = GetHoverFileName("my plugins");
          string nonFocusHover = GetNonFocusHoverFileName("my plugins");
          int index = xmlreader.GetValueAsInt("pluginSorting", "my Plugins", Int32.MaxValue);
          menuMain.ButtonInfos.Add(new MenuButtonInfo(GUILocalizeStrings.Get(913), (int)Window.WINDOW_MYPLUGINS,
                                                      focusTexture, nonFocusTexture, hover, nonFocusHover, index));
        }
      }
    }

    #endregion
  }
}