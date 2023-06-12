#region Copyright (C) 2005-2023 Team MediaPortal

// Copyright (C) 2005-2023 Team MediaPortal
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

    protected override void OnWindowLoaded()
    {
      string layout = GUIPropertyManager.GetProperty("#home.myhome.layout");
      try
      {
        _layout = (GUIFacadeControl.Layout)Enum.Parse(typeof(GUIFacadeControl.Layout), layout);
      }
      catch
      {
        _layout = GUIFacadeControl.Layout.List;
      }

      base.OnWindowLoaded();
    }

    protected override void LoadButtonNames()
    {
      if (menuMain == null)
      {
        return;
      }

      if (menuMain is GUIMenuControl)
      {
        (menuMain as GUIMenuControl).ButtonInfos.Clear();
      }

      if (menuMain is GUIFacadeControl)
      {
        GUIControl.ClearControl(GetID, menuMain.GetID);
      }

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

            if (menuMain is GUIMenuControl)
            {
              (menuMain as GUIMenuControl).ButtonInfos.Add(new MenuButtonInfo(plugInText, setup.GetWindowId(), 
                                                                              focusTexture, nonFocusTexture,
                                                                              hover, nonFocusHover, index));
            }

            if (menuMain is GUIFacadeControl)
            {
              GUIListItem listItem = new GUIListItem(plugInText)
              {
                Path = index.ToString(),
                ItemId = setup.GetWindowId(),
                Label2 = setup.Author(),
                IsFolder = false,
                IconImage = focusTexture,
                ThumbnailImage = hover,
                AlbumInfoTag = setup
              };
              listItem.OnItemSelected += OnItemSelected;
              (menuMain as GUIFacadeControl).Add(listItem);
            }
          }
        }

        if ((_useMyPlugins) && (myPluginsCount > 0))
        {
          string focusTexture = GetFocusTextureFileName("my plugins");
          string nonFocusTexture = GetNonFocusTextureFileName("my plugins");
          string hover = GetHoverFileName("my plugins");
          string nonFocusHover = GetNonFocusHoverFileName("my plugins");
          int index = xmlreader.GetValueAsInt("pluginSorting", "my Plugins", Int32.MaxValue);

          if (menuMain is GUIMenuControl)
          {
            (menuMain as GUIMenuControl).ButtonInfos.Add(new MenuButtonInfo(GUILocalizeStrings.Get(913), (int)Window.WINDOW_MYPLUGINS,
                                                                            focusTexture, nonFocusTexture, 
                                                                            hover, nonFocusHover, index));
          }

          if (menuMain is GUIFacadeControl)
          {
            GUIListItem listItem = new GUIListItem(GUILocalizeStrings.Get(913))
            {
              Path = index.ToString(),
              ItemId = (int)Window.WINDOW_MYPLUGINS,
              Label2 = "Team Mediaportal",
              IsFolder = false,
              IconImage = focusTexture,
              ThumbnailImage = hover,
              AlbumInfoTag = null
            };
            listItem.OnItemSelected += OnItemSelected;

            (menuMain as GUIFacadeControl).Add(listItem);
          }
        }
      }
    }

    #endregion

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIPropertyManager.SetProperty("#pluginname", item.Label);
      GUIPropertyManager.SetProperty("#pluginauthor", string.Empty);
      GUIPropertyManager.SetProperty("#plugindescription", string.Empty);

      if (item.AlbumInfoTag == null)
      {
        return;
      }

      GUIPropertyManager.SetProperty("#pluginauthor", (item.AlbumInfoTag as ISetupForm).Author());
      GUIPropertyManager.SetProperty("#plugindescription", (item.AlbumInfoTag as ISetupForm).Description());

      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip != null)
      {
        filmstrip.InfoImageFileName = item.ThumbnailImage;
      }
    }

  }
}