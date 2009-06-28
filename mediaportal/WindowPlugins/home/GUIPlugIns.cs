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

#region Usings

using System;
using System.Collections;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

#endregion

namespace MediaPortal.GUI.Home
{
  /// <summary>
  /// The implementation of the GUIHome Window.  (This window is coupled to the home.xml skin file).
  /// </summary>
  public class GUIPlugIns : GUIHomeBaseWindow
  {
    #region Constructor

    public GUIPlugIns()
    {
      GetID = (int) Window.WINDOW_MYPLUGINS;
    }

    #endregion

    #region Override

    public override bool Init()
    {
      //GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
      return (Load(GUIGraphicsContext.Skin + @"\myHomePlugIns.xml"));
    }

    protected override void LoadButtonNames()
    {
      if (menuMain == null)
      {
        return;
      }
      menuMain.ButtonInfos.Clear();
      ArrayList plugins = PluginManager.SetupForms;

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

            string showInPlugIns = xmlreader.GetValue("myplugins", setup.PluginName());
            if ((showInPlugIns == null) || (showInPlugIns.Length < 1))
            {
              if ((showPlugin != null) && (showPlugin.ShowDefaultHome() == true))
              {
                continue;
              }
            }
            else
            {
              if (showInPlugIns.ToLower().Equals("no"))
              {
                continue;
              }
            }

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
            int index = xmlreader.GetValueAsInt("pluginSorting", "my Plugins", Int32.MaxValue);
            menuMain.ButtonInfos.Add(new MenuButtonInfo(plugInText, setup.GetWindowId(), focusTexture, nonFocusTexture,
                                                        hover, nonFocusHover, index));
          }
        }
      }
    }

    #endregion
  }
}