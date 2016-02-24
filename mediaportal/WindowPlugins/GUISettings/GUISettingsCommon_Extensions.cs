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

using System;
using System.Collections;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Music;
using MediaPortal.GUI.Video;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  public class GUISettingsExtensions : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIListControl extensionsListcontrol = null;
    [SkinControl(3)] protected GUIButtonControl btnAdd = null;
    [SkinControl(4)] protected GUIButtonControl btnRemove= null;
    [SkinControl(5)] protected GUIButtonControl btnDefault= null;

    private string _section = string.Empty;

    public string Section
    {
      get { return _section; }
      set { _section = value; }
    }
    
    public GUISettingsExtensions()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_EXTENSIONS;
    }

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Common_Extensions.xml");
    }

    protected override void OnPageLoad()
    {
      OnExtensions();
      base.OnPageLoad();
      
      string module = string.Empty;
      switch (_section)
      {
        case "movies":
          module = GUILocalizeStrings.Get(300042); // Videos - File extensions
          break;
        case "music":
          module = GUILocalizeStrings.Get(300043);
          break;
        case "pictures":
          module = GUILocalizeStrings.Get(300044);
          break;
      }

      GUIPropertyManager.SetProperty("#currentmodule", module);
      
      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (MediaPortal.GUI.Settings.GUISettings.SettingsChanged && !MediaPortal.Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        MediaPortal.GUI.Settings.GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnAdd)
      {
        string ext = string.Empty;
        GetStringFromKeyboard(ref ext);
        if (string.IsNullOrEmpty(ext))
        {
          return;
        }
        ext = "." + ext.Replace(".", string.Empty).ToLowerInvariant();

        foreach (var lItem in extensionsListcontrol.ListItems)
        {
          if (ext.Equals(lItem.Label,StringComparison.InvariantCultureIgnoreCase))
          {
            return;
          }
        }

        GUIListItem item = new GUIListItem();
        item.Label = ext.Trim();
        extensionsListcontrol.Add(item);
        SaveExtensions();
        OnExtensions();
      }
      if (control == btnRemove)
      {
        extensionsListcontrol.RemoveItem(extensionsListcontrol.SelectedListItemIndex);
        SaveExtensions();
        OnExtensions();
      }
      if (control == btnDefault)
      {
        using (Profile.Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValue("movies", "extensions", Util.Utils.VideoExtensionsDefault);
        }
        // update internal plugins settings
        if (VirtualDirectories.Instance.Movies != null)
        {
          VirtualDirectories.Instance.Movies.LoadSettings(_section);
        }
        OnExtensions();
      }

      base.OnClicked(controlId, control, actionType);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion

    private void OnExtensions()
    {
      string extensions = string.Empty;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        extensions = xmlreader.GetValueAsString(_section, "extensions", Util.Utils.VideoExtensionsDefault);
      }
      string[] vExtensions = (extensions).Split(',');

      if (extensionsListcontrol != null)
      {
        extensionsListcontrol.Clear();

        foreach (var vExtension in vExtensions)
        {
          GUIListItem item = new GUIListItem();
          item.Label = vExtension;
          extensionsListcontrol.Add(item);
        }
      }
    }

    private void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      strLine = string.Empty;
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    private void SaveExtensions()
    {
      ArrayList aExtensions = new ArrayList();

      foreach (var lItem in extensionsListcontrol.ListItems)
      {
        aExtensions.Add(lItem.Label);
      }

      aExtensions.Sort();

      string extensions = string.Empty;

      foreach (string aExtension in aExtensions)
      {
        extensions = extensions + aExtension + ",";
      }

      extensions = extensions.Remove(extensions.LastIndexOf(","));

      using (Profile.Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(_section, "extensions", extensions);
      }
      
      switch (_section)
      {
        case "movies":
          GUIVideoFiles.ResetExtensions(aExtensions);
          break;
        case "music":
          GUIMusicFiles.ResetExtensions(aExtensions);
          break;
        case "pictures":
          Pictures.GUIPictures.ResetExtensions(aExtensions);
          break;
      }
    }
  }
}
