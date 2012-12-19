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

using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Threading;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUISettingsMusic : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnNowPlaying= null;
    [SkinControl(8)] protected GUIButtonControl btnPlaylist= null;
    [SkinControl(10)] protected GUIButtonControl btnDeletealbuminfo= null;
    [SkinControl(13)] protected GUIButtonControl btnDeletealbum = null;
    [SkinControl(35)] protected GUIButtonControl btnExtensions = null;
    [SkinControl(40)] protected GUIButtonControl btnFolders = null;
    [SkinControl(41)] protected GUIButtonControl btnDatabase = null;
    
    
    private string _section = "music";

    public GUISettingsMusic()
    {
      GetID = (int)Window.WINDOW_SETTINGS_MUSIC; //14
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\Settings_MyMusic.xml"));
    }

    #region serialization

    private void LoadSettings()
    {}

    private void SaveSettings()
    {}

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100014));
      LoadSettings();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void  OnPageDestroy(int new_windowId)
    {
      SaveSettings();
 	    base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnNowPlaying)
      {
        OnNowPlaying();
      }

      if (control == btnPlaylist)
      {
        OnPlayList();
      }
      
      if (control == btnDeletealbum)
      {
        OnDeleteAlbum();
      }

      if (control == btnDeletealbuminfo)
      {
        OnDeleteAlbumInfo();
      }

      if (control == btnFolders)
      {
        OnFolders();
      }

      if (control == btnExtensions)
      {
        OnExtensions();
      }

      if (control == btnDatabase)
      {
        OnDatabase();
      }

      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    private void OnNowPlaying()
    {
      GUISettingsMusicNowPlaying dlg = (GUISettingsMusicNowPlaying)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_MUSICNOWPLAYING);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_MUSICNOWPLAYING);
    }

    private void OnDeleteAlbum ()
    {
      var dbreorg = new MusicDatabaseReorg(GetID);
      dbreorg.DeleteSingleAlbum();
    }

    private void OnDeleteAlbumInfo()
    {
      var dbreorg = new MusicDatabaseReorg(GetID);
      dbreorg.DeleteAlbumInfo();
    }

    private void OnFolders()
    {
      GUIShareFolders dlg = (GUIShareFolders)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_FOLDERS);
      if (dlg == null)
      {
        return;
      }
      dlg.Section = _section;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_FOLDERS);
    }

    private void OnExtensions()
    {
      GUISettingsExtensions dlg = (GUISettingsExtensions)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_EXTENSIONS);
      if (dlg == null)
      {
        return;
      }
      dlg.Section = _section;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_EXTENSIONS);
    }

    private void OnDatabase()
    {
      GUISettingsMusicDatabase dlg = (GUISettingsMusicDatabase)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_MUSICDATABASE);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_MUSICDATABASE);
    }

    private void OnPlayList()
    {
      GUISettingsPlaylist dlg = (GUISettingsPlaylist)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_PLAYLIST);
      if (dlg == null)
      {
        return;
      }
      dlg.Section = _section;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_PLAYLIST);
    }
  }
}