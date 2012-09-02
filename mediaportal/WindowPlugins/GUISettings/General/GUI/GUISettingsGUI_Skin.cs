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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGUISkin : GUIInternalWindow
  {
    [SkinControl(10)] protected GUIButtonControl btnSkin = null;
    [SkinControl(11)] protected GUIButtonControl btnLanguage = null;
    [SkinControl(14)] protected GUICheckButton btnLanguagePrefix = null;
    [SkinControl(20)] protected GUIImage imgSkinPreview = null;
    
    private string _selectedLangName;
    private string _selectedSkinName;
    
    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo)x;
        CultureInfo info2 = (CultureInfo)y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

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
        string currentLanguage = string.Empty;
        currentLanguage = xmlreader.GetValueAsString("gui", "language", "English");
        btnLanguage.Label = currentLanguage;
        btnLanguagePrefix.Selected = xmlreader.GetValueAsBool("gui", "myprefix", false);

        SetSkins();
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("gui", "language", btnLanguage.Label);
        xmlwriter.SetValue("skin", "name", btnSkin.Label);
        xmlwriter.SetValueAsBool("gui", "myprefix", btnLanguagePrefix.Selected);
      }
      Config.SkinName = btnSkin.Label;
    }

    #endregion

    #region Overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnSkin)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(166); // menu

        List<string> installedSkins = new List<string>();
        installedSkins = GetInstalledSkins();

        foreach (string skin in installedSkins)
        {
          dlg.Add(skin);
        }
        
        dlg.SelectedLabel = btnSkin.SelectedItem;
        dlg.DoModal(GetID);
        
        if (dlg.SelectedId == -1)
        {
          return;
        }
        
        if (String.Compare(dlg.SelectedLabelText, btnSkin.Label, true) != 0)
        {
          btnSkin.Label = dlg.SelectedLabelText;
          OnSkinChanged();
        }
        
        return;
      }
      if (control == btnLanguage)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        
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
        
        string currentLanguage = btnLanguage.Label;
        dlg.SelectedLabel = 0;
        
        for (int i = 0; i < languages.Length; i++)
        {
          if (languages[i].ToLower() == currentLanguage.ToLower())
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
        
        if (String.Compare(dlg.SelectedLabelText, btnLanguage.Label, true) != 0)
        {
          btnLanguage.Label = dlg.SelectedLabelText;
          OnLanguageChanged();
        }
        
        return;
      }
      if (control == btnLanguagePrefix)
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
      List<string> installedSkins = new List<string>();
      string currentSkin = "";
      using (Settings xmlreader = new MPSettings())
      {
        currentSkin = xmlreader.GetValueAsString("skin", "name", "DefaultWide");
      }
      installedSkins = GetInstalledSkins();

      foreach (string skin in installedSkins)
      {
        if (String.Compare(skin, currentSkin, true) == 0)
        {
          btnSkin.Label = skin;
          imgSkinPreview.SetFileName(GUIGraphicsContext.GetThemedSkinFile(@"\media\preview.png"));
        }
      }
    }

    private List<string> GetInstalledSkins()
    {
      List<string> installedSkins = new List<string>();

      try
      {
        DirectoryInfo skinFolder = new DirectoryInfo(Config.GetFolder(Config.Dir.Skin));
        if (skinFolder.Exists)
        {
          DirectoryInfo[] skinDirList = skinFolder.GetDirectories();
          foreach (DirectoryInfo skinDir in skinDirList)
          {
            FileInfo refFile = new FileInfo(Config.GetFile(Config.Dir.Skin, skinDir.Name, "references.xml"));
            if (refFile.Exists)
            {
              installedSkins.Add(skinDir.Name);
            }
          }
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
      _selectedSkinName = btnSkin.Label;
      _selectedLangName = btnLanguage.Label;
    }

    private void RestoreButtons()
    {
      btnSkin.Label = _selectedSkinName;
      btnLanguage.Label = _selectedLangName;
    }

    private void OnSkinChanged()
    {
      // Backup the buttons, needed later
      BackupButtons();

      // Set the skin to the selected skin and reload GUI
      GUIGraphicsContext.Skin = btnSkin.Label;
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
      GUIControl.FocusControl(GetID, btnSkin.GetID);

      // Apply the selected buttons again, since they are cleared when we reload
      RestoreButtons();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("general", "skinobsoletecount", 0);
        bool autosize = xmlreader.GetValueAsBool("gui", "autosize", true);
        if (autosize && !GUIGraphicsContext.Fullscreen)
        {
          try
          {
            GUIGraphicsContext.form.ClientSize = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
            //Form.ActiveForm.ClientSize = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
          }
          catch (Exception ex)
          {
            Log.Error("OnSkinChanged exception:{0}", ex.ToString());
            Log.Error(ex);
          }
        }
      }
      if (BassMusicPlayer.Player != null && BassMusicPlayer.Player.VisualizationWindow != null)
      {
        BassMusicPlayer.Player.VisualizationWindow.Reinit();
      }

      // Send a message that the skin has changed.
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msg);
    }

    private void OnLanguageChanged()
    {
      // Backup the buttons, needed later
      BackupButtons();
      SaveSettings();
      GUILocalizeStrings.ChangeLanguage(btnLanguage.Label);
      GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
      GUIFontManager.InitializeDeviceObjects();
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID); // without this you cannot change skins / lang any more..
      GUIControl.FocusControl(GetID, btnLanguage.GetID);
      // Apply the selected buttons again, since they are cleared when we reload
      RestoreButtons();
    }
    
    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}