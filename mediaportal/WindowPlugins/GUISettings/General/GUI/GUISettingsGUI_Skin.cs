#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;

// ReSharper disable CheckNamespace
namespace WindowPlugins.GUISettings
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public sealed class GUISettingsGUISkin : GUIInternalWindow
  {
    [SkinControl(10)] private readonly GUIButtonControl _btnSkin = null;
    [SkinControl(11)] private readonly GUIButtonControl _btnLanguage = null;
    [SkinControl(14)] private readonly GUICheckButton _btnLanguagePrefix = null;
    [SkinControl(20)] private readonly GUIImage _imgSkinPreview = null;
    
    private string _selectedLangName;
    private string _selectedSkinName;

    public GUISettingsGUISkin()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GUISKIN; //705
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_GUI_Skin.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        string currentLanguage = xmlreader.GetValueAsString("gui", "language", "English");
        _btnLanguage.Label = currentLanguage;
        _btnLanguagePrefix.Selected = xmlreader.GetValueAsBool("gui", "myprefix", false);

        SetSkins();
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("gui", "language", _btnLanguage.Label);
        xmlwriter.SetValue("skin", "name", _btnSkin.Label);
        xmlwriter.SetValueAsBool("gui", "myprefix", _btnLanguagePrefix.Selected);
      }
      Config.SkinName = _btnSkin.Label;
    }

    #endregion

    #region Overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == _btnSkin)
      {
        var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(166); // menu

        IEnumerable<string> installedSkins = GetInstalledSkins();

        foreach (string skin in installedSkins)
        {
          dlg.Add(skin);
        }
        
        dlg.SelectedLabel = _btnSkin.SelectedItem;
        dlg.DoModal(GetID);
        
        if (dlg.SelectedId == -1)
        {
          return;
        }
        
        if (String.Compare(dlg.SelectedLabelText, _btnSkin.Label, StringComparison.OrdinalIgnoreCase) != 0)
        {
          _btnSkin.Label = dlg.SelectedLabelText;

          // prevent MP from rendering when resource are disposed during live changing of a skin
          lock (GUIGraphicsContext.RenderLock)
          {
            OnSkinChanged();
          }
        }
        
        return;
      }
      if (control == _btnLanguage)
      {
        var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        
        if (dlg == null)
        {
          return;
        }
        
        dlg.Reset();
        dlg.SetHeading(248); // menu
        string[] languages = GUILocalizeStrings.SupportedLanguages();
        
        foreach (string lang in languages)
        {
          dlg.Add(lang);
        }
        
        string currentLanguage = _btnLanguage.Label;
        dlg.SelectedLabel = 0;
        
        for (int i = 0; i < languages.Length; i++)
        {
          if (languages[i].ToLowerInvariant() == currentLanguage.ToLowerInvariant())
          {
            dlg.SelectedLabel = i;
            break;
          }
        }
        
        dlg.DoModal(GetID);
        
        if (dlg.SelectedId == -1)
        {
          return;
        }
        
        if (String.Compare(dlg.SelectedLabelText, _btnLanguage.Label, StringComparison.OrdinalIgnoreCase) != 0)
        {
          _btnLanguage.Label = dlg.SelectedLabelText;
          OnLanguageChanged();
        }
        
        return;
      }
      if (control == _btnLanguagePrefix)
      {
        SettingsChanged(true);
      }
      
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100705));
      LoadSettings();

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
      SaveSettings();

      if (MediaPortal.GUI.Settings.GUISettings.SettingsChanged && !MediaPortal.Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        MediaPortal.GUI.Settings.GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
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
    
    private void SetSkins()
    {
      string currentSkin;
      using (Settings xmlreader = new MPSettings())
      {
        currentSkin = xmlreader.GetValueAsString("skin", "name", "DefaultWide");
      }
      IEnumerable<string> installedSkins = GetInstalledSkins();

      foreach (string skin in installedSkins)
      {
        if (String.Compare(skin, currentSkin, StringComparison.OrdinalIgnoreCase) == 0)
        {
          _btnSkin.Label = skin;
          _imgSkinPreview.SetFileName(GUIGraphicsContext.GetThemedSkinFile(@"\media\preview.png"));
        }
      }
    }

    private IEnumerable<string> GetInstalledSkins()
    {
      var installedSkins = new List<string>();

      try
      {
        var skinFolder = new DirectoryInfo(Config.GetFolder(Config.Dir.Skin));
        if (skinFolder.Exists)
        {
          DirectoryInfo[] skinDirList = skinFolder.GetDirectories();
          installedSkins.AddRange(from skinDir in skinDirList let refFile = new FileInfo(Config.GetFile(Config.Dir.Skin, skinDir.Name, "references.xml")) where refFile.Exists select skinDir.Name);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUISettingsGeneral: Error getting installed skins - {0}", ex.Message);
      }
      return installedSkins;
    }

    private void BackupButtons()
    {
      _selectedSkinName = _btnSkin.Label;
      _selectedLangName = _btnLanguage.Label;
    }

    private void RestoreButtons()
    {
      _btnSkin.Label = _selectedSkinName;
      _btnLanguage.Label = _selectedLangName;
    }

    private void OnSkinChanged()
    {
      // Backup the buttons, needed later
      BackupButtons();

      // Set the skin to the selected skin and reload GUI
      GUIGraphicsContext.Skin = _btnSkin.Label;
      SaveSettings();
      GUITextureManager.Clear();
      GUITextureManager.Init();
      SkinSettings.Load();
      GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
      GUIFontManager.InitializeDeviceObjects();
      GUIExpressionManager.ClearExpressionCache();
      GUIControlFactory.ClearReferences();
      GUIControlFactory.LoadReferences(GUIGraphicsContext.GetThemedSkinFile(@"\references.xml"));
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID);
      GUIControl.FocusControl(GetID, _btnSkin.GetID);

      // Apply the selected buttons again, since they are cleared when we reload
      RestoreButtons();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("general", "skinobsoletecount", 0);
        if (!GUIGraphicsContext.Fullscreen)
        {
          try
          {
            var border = new Size(GUIGraphicsContext.form.Width - GUIGraphicsContext.form.ClientSize.Width, 
                                  GUIGraphicsContext.form.Height - GUIGraphicsContext.form.ClientSize.Height);
            if (GUIGraphicsContext.SkinSize.Width + border.Width <= GUIGraphicsContext.currentScreen.WorkingArea.Width &&
                GUIGraphicsContext.SkinSize.Height + border.Height <= GUIGraphicsContext.currentScreen.WorkingArea.Height)
            {
              GUIGraphicsContext.form.ClientSize = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
            }
            else
            {
              double ratio = Math.Min((double)(GUIGraphicsContext.currentScreen.WorkingArea.Width - border.Width) / GUIGraphicsContext.SkinSize.Width,
                                      (double)(GUIGraphicsContext.currentScreen.WorkingArea.Height - border.Height) / GUIGraphicsContext.SkinSize.Height);
              GUIGraphicsContext.form.ClientSize = new Size((int)(GUIGraphicsContext.SkinSize.Width * ratio), (int)(GUIGraphicsContext.SkinSize.Height * ratio));
            }
          }
          catch (Exception ex)
          {
            Log.Error("OnSkinChanged exception:{0}", ex.ToString());
            Log.Error(ex);
          }
        }
      }

      // Send a message that the skin has changed.
      var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msg);
    }

    private void OnLanguageChanged()
    {
      // Backup the buttons, needed later
      BackupButtons();
      SaveSettings();
      GUILocalizeStrings.ChangeLanguage(_btnLanguage.Label);
      GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
      GUIFontManager.InitializeDeviceObjects();
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID); // without this you cannot change skins / lang any more..
      GUIControl.FocusControl(GetID, _btnLanguage.GetID);
      // Apply the selected buttons again, since they are cleared when we reload
      RestoreButtons();
    }
    
    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}