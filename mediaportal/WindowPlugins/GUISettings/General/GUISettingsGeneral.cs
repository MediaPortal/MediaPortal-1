#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Globalization;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGeneral : GUIWindow
  {
    [SkinControlAttribute(10)]
    protected GUISelectButtonControl btnSkin = null;
    [SkinControlAttribute(11)]
    protected GUISelectButtonControl btnLanguage = null;
    [SkinControlAttribute(12)]
    protected GUIToggleButtonControl btnFullscreen = null;
    [SkinControlAttribute(13)]
    protected GUIToggleButtonControl btnScreenSaver = null;
    [SkinControlAttribute(20)]
    protected GUIImage imgSkinPreview = null;

    private string SkinDirectory = @"skin\";

    class CultureComparer : IComparer
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

    public GUISettingsGeneral()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_SKIN;
    }

    public override bool Init()
    {
      //SkinDirectory = GUIGraphicsContext.Skin.Remove(GUIGraphicsContext.Skin.LastIndexOf(@"\"));
      return Load(GUIGraphicsContext.Skin + @"\settings_general.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnSkin)
      {
        OnSkinChanged();
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
      btnSkin.CaptionChanged += new EventHandler(RefreshSkinPreview);
      GUIControl.FocusControl(GetID, btnSkin.GetID);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      btnSkin.CaptionChanged -= new EventHandler(RefreshSkinPreview);
      base.OnPageDestroy(newWindowId);
      SaveSettings();
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("general", "startfullscreen", btnFullscreen.Selected);
        xmlwriter.SetValueAsBool("general", "screensaver", btnScreenSaver.Selected);
        xmlwriter.SetValue("skin", "language", btnLanguage.SelectedLabel);
        xmlwriter.SetValue("skin", "name", btnSkin.SelectedLabel);
      }
    }

    void SetFullScreen()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        bool fullscreen = xmlreader.GetValueAsBool("general", "startfullscreen", false);
        btnFullscreen.Selected = fullscreen;
      }
    }

    void SetScreenSaver()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        bool screensaver = xmlreader.GetValueAsBool("general", "screensaver", false);
        btnScreenSaver.Selected = screensaver;
      }
    }

    void SetLanguages()
    {
      GUIControl.ClearControl(GetID, btnLanguage.GetID);
      string currentLanguage = String.Empty;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        currentLanguage = xmlreader.GetValueAsString("skin", "language", "English");
      }
      string LanguageDirectory = @"language\";
      int lang = 0;
      if (Directory.Exists(LanguageDirectory))
      {
        string[] folders = Directory.GetDirectories(LanguageDirectory, "*.*");

        foreach (string folder in folders)
        {
          string fileName = folder.Substring(@"language\".Length);

          //
          // Exclude cvs folder
          //
          if ((fileName.ToLower() != "cvs") && (fileName.ToLower() != ".svn"))
          {
            if (fileName.Length > 0)
            {
              fileName = fileName.Substring(0, 1).ToUpper() + fileName.Substring(1);
              GUIControl.AddItemLabelControl(GetID, btnLanguage.GetID, fileName);

              if (fileName.ToLower() == currentLanguage.ToLower())
              {
                GUIControl.SelectItemControl(GetID, btnLanguage.GetID, lang);
              }
              lang++;
            }
          }
        }
      }
    }

    void SetSkins()
    {
      string currentSkin = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        currentSkin = xmlreader.GetValueAsString("skin", "name", "BlueTwo");
      }

      GUIControl.ClearControl(GetID, btnSkin.GetID);
      int skinNo = 0;
      if (Directory.Exists(SkinDirectory))
      {
        string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");

        foreach (string skinFolder in skinFolders)
        {
          bool isInvalidDirectory = false;
          string[] invalidDirectoryNames = new string[] { "cvs", ".svn" };

          string directoryName = skinFolder.Substring(SkinDirectory.Length);

          if (directoryName != null && directoryName.Length > 0)
          {
            foreach (string invalidDirectory in invalidDirectoryNames)
            {
              if (invalidDirectory.Equals(directoryName.ToLower()))
              {
                isInvalidDirectory = true;
                break;
              }
            }

            if (isInvalidDirectory == false)
            {
              //
              // Check if we have a home.xml located in the directory, if so we consider it as a
              // valid skin directory
              //
              string filename = Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
              if (File.Exists(filename))
              {
                GUIControl.AddItemLabelControl(GetID, btnSkin.GetID, directoryName);
                if (String.Compare(directoryName, currentSkin, true) == 0)
                {
                  GUIControl.SelectItemControl(GetID, btnSkin.GetID, skinNo);
                  imgSkinPreview.SetFileName(Path.Combine(SkinDirectory, Path.Combine(directoryName, @"media\preview.png")));
                }
                skinNo++;
              }
            }
          }
        }
      }
    }

    void RefreshSkinPreview(object sender, EventArgs e)
    {
      imgSkinPreview.SetFileName(Path.Combine(SkinDirectory, Path.Combine(btnSkin.SelectedLabel, @"media\preview.png")));
    }

    void OnSkinChanged()
    {
      SaveSettings();
      int selectedSkinIndex = btnSkin.SelectedItem;
      GUIGraphicsContext.Skin = btnSkin.SelectedLabel;

      //FreeResources();
      GUITextureManager.Clear();
      //AllocResources();
      GUITextureManager.Init();
      GUIFontManager.LoadFonts(GUIGraphicsContext.Skin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      GUIControlFactory.ClearReferences();
      GUIControlFactory.LoadReferences(GUIGraphicsContext.Skin + @"\references.xml");
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID);
      GUIControl.FocusControl(GetID, btnSkin.GetID);
      GUIControl.SelectItemControl(GetID, btnSkin.GetID, selectedSkinIndex);
    }

    void OnLanguageChanged()
    {
      SaveSettings();
      GUILocalizeStrings.Clear();
      GUILocalizeStrings.Load(@"language\" + btnLanguage.SelectedLabel + @"\strings.xml");
      GUIWindowManager.OnResize();
      GUIWindowManager.ActivateWindow(GetID); // without this you cannot change skins / lang any more..
      GUIControl.FocusControl(GetID, btnLanguage.GetID);
    }
  }
}