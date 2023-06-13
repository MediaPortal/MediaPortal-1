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
  /// The implementation of the GUIHome Window.  (This window is coupled to the home.xml skin file).
  /// </summary>
  public class GUIPlugIns : GUIHomeBaseWindow
  {
    #region Constructor

    public GUIPlugIns()
    {
      GetID = (int)Window.WINDOW_MYPLUGINS;
    }

    #endregion

    #region Override

    public override bool Init()
    {
      //GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
      return (Load(GUIGraphicsContext.GetThemedSkinFile(@"\myHomePlugIns.xml")));
    }

    protected override void OnWindowLoaded()
    {
      string layout = GUIPropertyManager.GetProperty("#home.myhomeplugins.layout");
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
        (menuMain as GUIFacadeControl).Clear();
      }

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
              if (showInPlugIns.ToLowerInvariant().Equals("no"))
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