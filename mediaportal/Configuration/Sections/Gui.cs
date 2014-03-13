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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class Gui : SectionSettings
  {
    // IMPORTANT: the enumeration depends on the correct order of items in homeComboBox.
    // The order is chosen to allow compositing SelectedIndex from bitmapped flags.
    [Flags]
    private enum HomeUsageEnum
    {
      PreferClassic = 0,
      PreferBasic = 1,
      UseBoth = 0,
      UseOnlyOne = 2,
    }

    private string SkinDirectory;

    public Gui()
      : this("GUI") {}

    public Gui(string name)
      : base(name)
    {
      SkinDirectory = Config.GetFolder(Config.Dir.Skin);

      InitializeComponent();

      // Load available skins
      listViewAvailableSkins.Items.Clear();

      if (Directory.Exists(SkinDirectory))
      {
        string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");

        foreach (string skinFolder in skinFolders)
        {
          bool isInvalidDirectory = false;
          string[] invalidDirectoryNames = new string[] {"cvs"};

          string directoryName = skinFolder.Substring(SkinDirectory.Length + 1);

          if (!string.IsNullOrEmpty(directoryName))
          {
            foreach (string invalidDirectory in invalidDirectoryNames)
            {
              if (invalidDirectory.Equals(directoryName.ToLowerInvariant()))
              {
                isInvalidDirectory = true;
                break;
              }
            }

            if (isInvalidDirectory == false)
            {
              //
              // Check if we have a references.xml located in the directory, if so we consider it as a valid skin directory              
              string filename = Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
              if (File.Exists(filename))
              {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNode node = doc.SelectSingleNode("/controls/skin/version");
                ListViewItem item = listViewAvailableSkins.Items.Add(directoryName);
                if (node != null && node.InnerText != null)
                {
                  item.SubItems.Add(node.InnerText);
                }
                else
                {
                  item.SubItems.Add("?");
                }
              }
            }
          }
        }
      }
    }

    private string[][] sectionEntries = new string[][]
                                          {
                                            // 0 Allow remember last focused item on supported window/skin
                                            new string[] {"gui", "allowRememberLastFocusedItem", "true"},
                                            // 2 Hide file extensions like .mp3, .avi, .mpg,...
                                            new string[] {"gui", "hideextensions", "true"},
                                            // 3 Enable file existance cache
                                            new string[] {"gui", "fileexistscache", "false"},
                                            // 4 Enable skin sound effects
                                            new string[] {"gui", "enableguisounds", "true"},
                                            // 5 Show special mouse controls (scrollbars, etc)      
                                            new string[] {"gui", "mousesupport", "false"},
                                            // 6 Reduce frame rate when not in focus     
                                            new string[] {"gui", "reduceframerate", "false"},
                                            new string[] {"gui", "addVideoFilesToDb", "false"},
                                          };

    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

    private void listViewAvailableSkins_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewAvailableSkins.SelectedItems.Count == 0)
      {
        previewPictureBox.Image = null;
        previewPictureBox.Visible = false;
        mpButtonEditSkinSettings.Enabled = false;
        return;
      }
      string currentSkin = listViewAvailableSkins.SelectedItems[0].Text;
      string previewFile = Path.Combine(Path.Combine(SkinDirectory, currentSkin), @"media\preview.png");
      mpButtonEditSkinSettings.Enabled = true;

      //
      // Clear image
      //
      previewPictureBox.Image = null;

      Image img = Properties.Resources.mplogo;

      if (File.Exists(previewFile))
      {
        using (Stream s = new FileStream(previewFile, FileMode.Open, FileAccess.Read))
        {
          img = Image.FromStream(s);
        }
      }
      previewPictureBox.Width = img.Width;
      previewPictureBox.Height = img.Height;
      previewPictureBox.Image = img;
      previewPictureBox.Visible = true;
    }

    // PLEASE NOTE: when adding items, adjust the box so it doesn't get scrollbars    
    //              AND be careful cause depending on where you add a setting, the indexes might have changed!!!

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        // Load gui settings
        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          settingsCheckedListBox.SetItemChecked(index, xmlreader.GetValueAsBool(currentSection[0], currentSection[1], bool.Parse(currentSection[2])));
        }

        // Load skin settings.
        string currentSkin = xmlreader.GetValueAsString("skin", "name", "NoSkin");

        float screenHeight = GUIGraphicsContext.currentScreen.Bounds.Height;
        float screenWidth = GUIGraphicsContext.currentScreen.Bounds.Width;
        float screenRatio = (screenWidth / screenHeight);
        if (currentSkin == "NoSkin")
        {
          //Change default skin based on screen aspect ratio
          currentSkin = screenRatio > 1.5 ? "DefaultWide" : "Default";
        }

        //
        // Make sure the skin actually exists before setting it as the current skin
        //
        for (int i = 0; i < listViewAvailableSkins.Items.Count; i++)
        {
          string checkString = listViewAvailableSkins.Items[i].SubItems[0].Text;
          if (checkString.Equals(currentSkin, StringComparison.InvariantCultureIgnoreCase))
          {
            listViewAvailableSkins.Items[i].Selected = true;

            Log.Info("Skin selected: {0} (screenWidth={1}, screenHeight={2}, screenRatio={3})", checkString, screenWidth,
                     screenHeight, screenRatio);
            break;
          }
        }

        bool startWithBasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", true);
        bool useOnlyOneHome = xmlreader.GetValueAsBool("gui", "useonlyonehome", false);
        homeComboBox.SelectedIndex = (int)((useOnlyOneHome ? HomeUsageEnum.UseOnlyOne : HomeUsageEnum.UseBoth) |
                                           (startWithBasicHome ? HomeUsageEnum.PreferBasic : HomeUsageEnum.PreferClassic));
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        // Save general gui settings.
        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }

        // Save skin settings.
        string prevSkin = xmlwriter.GetValueAsString("skin", "name", "DefaultWide");
        string selectedSkin = prevSkin;
        try
        {
          selectedSkin = listViewAvailableSkins.SelectedItems[0].Text;
        }
        catch (Exception) {}
        if (prevSkin != selectedSkin)
        {
          xmlwriter.SetValueAsBool("general", "dontshowskinversion", false);
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, selectedSkin + @"\fonts"), "*");
        }
        xmlwriter.SetValue("skin", "name", selectedSkin);
        Config.SkinName = selectedSkin;
        xmlwriter.SetValue("general", "skinobsoletecount", 0);
        xmlwriter.SetValueAsBool("gui", "useonlyonehome", (homeComboBox.SelectedIndex & (int)HomeUsageEnum.UseOnlyOne) != 0);
        xmlwriter.SetValueAsBool("gui", "startbasichome", (homeComboBox.SelectedIndex & (int)HomeUsageEnum.PreferBasic) != 0);
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        // This url is a redirect, which shouldn't be changed.
        // If it's target should be changed, contact high, please.
        Process.Start(@"http://www.team-mediaportal.com/MP1/skingallery");
      }
      catch {}
    }

    private void mpButtonEditSkinSettings_Click(object sender, EventArgs e)
    {
      DlgSkinSettings dlg = new DlgSkinSettings(listViewAvailableSkins);
      dlg.ShowInTaskbar = false;
      dlg.MaximizeBox = false;
      dlg.MinimizeBox = false;

      if (dlg.ShowDialog() == DialogResult.OK)
      {
      }
    }
  }
}