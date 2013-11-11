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

using MediaPortal.Common.Utils;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;

namespace MediaPortal.Configuration.Sections
{
  public partial class GuiScreensaver : SectionSettings
  {
    #region ctor    
    private ArrayList availablePlugins = new ArrayList();
    private BindingList<ItemTag> loadedPlugins = new BindingList<ItemTag>();
    public GuiScreensaver()
      : this("Screensaver") { }

    public GuiScreensaver(string name)
      : base(name)
    {
      InitializeComponent();
    }
    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadAll();
    }

    public void LoadAll()
    {
      if (!Plugins.IsLoaded || (Plugins.WasLastLoadAdvanced != SettingsForm.AdvancedMode))
      {
        Plugins.ClearLoadedPlugins();
        Plugins.IsLoaded = true;
        Plugins.EnumeratePlugins();
        Plugins.LoadPlugins();       
      }
      loadedPlugins.Clear();
      foreach (ItemTag tag in Plugins.LoadedPlugins)
      {
        loadedPlugins.Add(tag);
      }
      LoadSettings();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      int windowid = 0;

      using (Settings xmlreader = new MPSettings())
      {
        checkBoxEnableScreensaver.Checked = xmlreader.GetValueAsBool("general", "IdleTimer", true);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
        radioBtnBlankScreen.Checked = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
        radioButtonLoadPlugin.Checked = xmlreader.GetValueAsBool("general", "IdlePlugin", false);
        windowid = xmlreader.GetValueAsInt("general", "IdlePluginWindow", 0);
      }    
      pluginsComboBox.DataSource = loadedPlugins;
      pluginsComboBox.DisplayMember = "PluginName";
      pluginsComboBox.ValueMember = "PluginName";
      if (windowid != 0)
      {
        for (int i = 0; i < loadedPlugins.Count; i++)
        {
          ItemTag t = loadedPlugins[i];
          if (t.WindowId == windowid)
          {
            pluginsComboBox.SelectedIndex = i;
            break;
          }
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValueAsBool("general", "IdleTimer", checkBoxEnableScreensaver.Checked);
        xmlreader.SetValue("general", "IdleTimeValue", numericUpDownDelay.Value);
        xmlreader.SetValueAsBool("general", "IdleBlanking", radioBtnBlankScreen.Checked);
        xmlreader.SetValueAsBool("general", "IdlePlugin", radioButtonLoadPlugin.Checked);
        if (loadedPlugins.Count > 0 & pluginsComboBox.SelectedIndex > -1)
        {
          xmlreader.SetValue("general", "IdlePluginWindow", loadedPlugins[pluginsComboBox.SelectedIndex].WindowId);
        }
      }
    }
    public override void OnSectionDeActivated()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValueAsBool("general", "IdleTimer", checkBoxEnableScreensaver.Checked);
        xmlreader.SetValue("general", "IdleTimeValue", numericUpDownDelay.Value);
        xmlreader.SetValueAsBool("general", "IdleBlanking", radioBtnBlankScreen.Checked);
        xmlreader.SetValueAsBool("general", "IdlePlugin", radioButtonLoadPlugin.Checked);
        if (loadedPlugins.Count > 0 & pluginsComboBox.SelectedIndex > -1)
        {
          xmlreader.SetValue("general", "IdlePluginWindow", loadedPlugins[pluginsComboBox.SelectedIndex].WindowId);
        }
      }
      base.OnSectionDeActivated();
    }

    #endregion   

    private void checkBoxEnableScreensaver_CheckedChanged(object sender, System.EventArgs e)
    {
      pluginsComboBox.Enabled = groupBoxIdleAction.Enabled = numericUpDownDelay.Enabled = checkBoxEnableScreensaver.Checked;
    }

    private void radioButtonLoadPlugin_CheckedChanged(object sender, EventArgs e)
    {
      pluginsComboBox.Enabled = radioButtonLoadPlugin.Checked;
    }
  }
}