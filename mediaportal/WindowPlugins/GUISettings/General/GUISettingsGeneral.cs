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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGeneral : GUIInternalWindow
  {
    [SkinControl(10)] protected GUIButtonControl btnSkin = null;
    [SkinControl(11)] protected GUISelectButtonControl btnLanguage = null;
    [SkinControl(12)] protected GUIToggleButtonControl btnFullscreen = null;
    [SkinControl(13)] protected GUIToggleButtonControl btnScreenSaver = null;
    [SkinControl(20)] protected GUIImage imgSkinPreview = null;

    private int selectedLangIndex;
    private string selectedSkinName;
    private bool selectedFullScreen;
    private bool selectedScreenSaver;

    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo) x;
        CultureInfo info2 = (CultureInfo) y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

    public GUISettingsGeneral()
    {
      GetID = (int) Window.WINDOW_SETTINGS_SKIN;
    }

    public override bool Init()
    {
      //SkinDirectory = GUIGraphicsContext.Skin.Remove(GUIGraphicsContext.Skin.LastIndexOf(@"\")); 
      return Load(GUIGraphicsContext.Skin + @"\settings_general.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnSkin)
      {
        GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
        OnLanguageChanged();
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      SetFullScreen();
      SetScreenSaver();
      SetLanguages();
      SetSkins();
      //GUIControl.FocusControl(GetID, btnSkin.GetID);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      SaveSettings();
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "startfullscreen", btnFullscreen.Selected);
        xmlwriter.SetValueAsBool("general", "IdleTimer", btnScreenSaver.Selected);
        xmlwriter.SetValue("skin", "language", btnLanguage.SelectedLabel);
        xmlwriter.SetValue("skin", "name", btnSkin.Label);
      }
    }

    private void SetFullScreen()
    {
      using (Settings xmlreader = new MPSettings())
      {
        bool fullscreen = xmlreader.GetValueAsBool("general", "startfullscreen", false);
        btnFullscreen.Selected = fullscreen;
      }
    }

    private void SetScreenSaver()
    {
      using (Settings xmlreader = new MPSettings())
      {
        bool screensaver = xmlreader.GetValueAsBool("general", "IdleTimer", false);
        btnScreenSaver.Selected = screensaver;
      }
    }

    private void SetLanguages()
    {
      GUIControl.ClearControl(GetID, btnLanguage.GetID);
      string currentLanguage = string.Empty;
      using (Settings xmlreader = new MPSettings())
      {
        currentLanguage = xmlreader.GetValueAsString("skin", "language", "English");
      }
      string LanguageDirectory = Config.GetFolder(Config.Dir.Language);
      int lang = 0;

      string[] languages = GUILocalizeStrings.SupportedLanguages();

      foreach (string language in languages)
      {
        GUIControl.AddItemLabelControl(GetID, btnLanguage.GetID, language);

        if (language.ToLower() == currentLanguage.ToLower())
        {
          GUIControl.SelectItemControl(GetID, btnLanguage.GetID, lang);
        }
        lang++;
      }
    }

    private void SetSkins()
    {
      List<string> installedSkins = new List<string>();
      string currentSkin = "";
      using (Settings xmlreader = new MPSettings())
      {
        currentSkin = xmlreader.GetValueAsString("skin", "name", "Blue3wide");
      }
      installedSkins = GetInstalledSkins();

      foreach (string skin in installedSkins)
      {
        if (String.Compare(skin, currentSkin, true) == 0)
        {
          btnSkin.Label = skin;
          imgSkinPreview.SetFileName(Config.GetFile(Config.Dir.Skin, skin, @"media\preview.png"));
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
      selectedSkinName = btnSkin.Label;
      selectedLangIndex = btnLanguage.SelectedItem;
      selectedFullScreen = btnFullscreen.Selected;
      selectedScreenSaver = btnScreenSaver.Selected;
    }

    private void RestoreButtons()
    {
      btnSkin.Label = selectedSkinName;
      GUIControl.SelectItemControl(GetID, btnLanguage.GetID, selectedLangIndex);
      if (selectedFullScreen)
      {
        GUIControl.SelectControl(GetID, btnFullscreen.GetID);
      }
      if (selectedScreenSaver)
      {
        GUIControl.SelectControl(GetID, btnScreenSaver.GetID);
      }
    }

    //private void RefreshSkinPreview(object sender, EventArgs e)
    //{
    //  imgSkinPreview.SetFileName(Config.GetFile(Config.Dir.Skin, btnSkin.Label, @"media\preview.png"));
    //}

    private void OnSkinChanged()
    {
      // Backup the buttons, needed later
      BackupButtons();

      // Set the skin to the selected skin and reload GUI
      GUIGraphicsContext.Skin = btnSkin.Label;
      SaveSettings();
      GUITextureManager.Clear();
      GUITextureManager.Init();
      GUIFontManager.LoadFonts(GUIGraphicsContext.Skin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      GUIControlFactory.ClearReferences();
      GUIControlFactory.LoadReferences(GUIGraphicsContext.Skin + @"\references.xml");
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID);
      GUIControl.FocusControl(GetID, btnSkin.GetID);

      // Apply the selected buttons again, since they are cleared when we reload
      RestoreButtons();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("general", "skinobsoletecount", 0);
        bool autosize = xmlreader.GetValueAsBool("general", "autosize", true);
        if (autosize && !GUIGraphicsContext.Fullscreen)
        {
          try
          {
            Form.ActiveForm.ClientSize = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
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
    }

    private void OnLanguageChanged()
    {
      // Backup the buttons, needed later
      BackupButtons();
      SaveSettings();
      GUILocalizeStrings.ChangeLanguage(btnLanguage.SelectedLabel);
      GUIFontManager.LoadFonts(GUIGraphicsContext.Skin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID); // without this you cannot change skins / lang any more..
      GUIControl.FocusControl(GetID, btnLanguage.GetID);
      // Apply the selected buttons again, since they are cleared when we reload
      RestoreButtons();
    }
  }
}