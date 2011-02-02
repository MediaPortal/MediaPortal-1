#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Profile;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class Gui : SectionSettings
  {
    public Gui()
      : this("GUI") { }

    public Gui(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private string[][] sectionEntries = new string[][]
                                          {                                            
                                            new string[] {"gui", "allowRememberLastFocusedItem", "true"},
                                            // 0 Allow remember last focused item on supported window/skin
                                            new string[] {"gui", "autoSizeWindowModeToSkin", "true"},
                                            // 1 Autosize window mode to skin dimensions
                                            new string[] {"gui", "hideextensions", "true"},
                                            // 2 Hide file extensions like .mp3, .avi, .mpg,...
                                            new string[] {"gui", "fileexistscache", "false"},
                                            // 3 Enable file existance cache
                                            new string[] {"gui", "enableSkinSoundEffects", "true"},
                                            // 4 Enable skin sound effects
                                            new string[] {"gui", "setLoopDelayWhenScrollingLists", "true"},
                                            // 5 Set loop delay when scrolling lists
                                            new string[] {"gui", "mousesupport", "false"},
                                            // 6 Show special mouse controls (scrollbars, etc)      
                                          };

    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

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
          settingsCheckedListBox.SetItemChecked(index,
                                                xmlreader.GetValueAsBool(currentSection[0], currentSection[1],
                                                                         bool.Parse(currentSection[2])));

        }

        listLoopDelayUpDown.Value = xmlreader.GetValueAsInt("gui", "listLoopDelay", 100);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }

        xmlwriter.SetValue("gui", "listLoopDelay", listLoopDelayUpDown.Value);
      }
    }

    private void settingsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (sectionEntries[e.Index][1].Equals("setLoopDelayWhenScrollingLists"))
      {
        listLoopDelayUpDown.Enabled = e.NewValue == CheckState.Checked;
      }
    }
  }
}