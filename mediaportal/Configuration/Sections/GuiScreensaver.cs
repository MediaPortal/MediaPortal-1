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
    private class ItemTag
    {
      public string DllName;
      public ISetupForm SetupForm;
      public string PluginName { get; set; }
      public string Type { get; set; }
      public int WindowId = -1;
      public bool IsProcess = false;
      public bool IsWindow = false;
      public bool IsExternalPlayer = false;
      public bool IsHome = false;
      public bool IsEnabled = false;
      public bool IsPlugins = false;
      public bool ShowDefaultHome = false;
      public bool IsIncompatible = false;
      private Image activeImage = null;
      private Image inactiveImage = null;

      public Image ActiveImage
      {
        get { return activeImage; }
        set { activeImage = value; }
      }

      public Image InactiveImage
      {
        get { return inactiveImage; }
        set { inactiveImage = value; }
      }
    }
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
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      LoadPlugins();
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
        xmlreader.SetValue("general", "IdlePluginWindow", loadedPlugins[pluginsComboBox.SelectedIndex].WindowId);
      }
    }

    #endregion

    private void EnumeratePluginDirectory(string directory)
    {
      if (Directory.Exists(directory))
      {
        //
        // Enumerate files
        //
        string[] files = Directory.GetFiles(directory, "*.dll");

        //
        // Add to list
        //
        foreach (string file in files)
        {
          availablePlugins.Add(file);
        }
      }
    }

    private void LoadPlugins()
    {
      foreach (string pluginFile in availablePlugins)
      {
        Assembly pluginAssembly = null;
        try
        {
          Log.Debug("PluginsNew: loadPlugins {0}", pluginFile);
          pluginAssembly = Assembly.LoadFrom(pluginFile);
        }
        catch (BadImageFormatException)
        {
          Log.Warn("PluginsNew: {0} has a bad image format", pluginFile);
        }

        if (pluginAssembly != null)
        {
          try
          {
            Type[] exportedTypes = pluginAssembly.GetExportedTypes();
            List<object> NonSetupWindows = new List<object>();

            if (!exportedTypes.Any(t => t.IsClass && !t.IsAbstract &&
                (typeof(ISetupForm).IsAssignableFrom(t) || typeof(GUIWindow).IsAssignableFrom(t))))
            {
              continue; // there are no plugins in the assembly, skip it
            }

            foreach (Type type in exportedTypes)
            {
              bool isPlugin = (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null);
              bool isGuiWindow = ((type.IsClass) && (type.IsSubclassOf(typeof(GUIWindow))));

              // an abstract class cannot be instanciated
              if (type.IsAbstract)
              {
                continue;
              }

              bool isIncompatible = !CompatibilityManager.IsPluginCompatible(type);
              if (isIncompatible)
              {
                Log.Warn(
                  "Plugin Manager: Plugin {0} is incompatible with the current MediaPortal version! (File: {1})",
                  type.FullName, pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
              }

              // Try to locate the interface we're interested in
              if (isPlugin || isGuiWindow)
              {
                // Create instance of the current type
                object pluginObject;
                try
                {
                  pluginObject = Activator.CreateInstance(type);
                }
                catch (TargetInvocationException)
                {
                  MessageBox.Show(
                    string.Format(
                      "An error occured while loading the plugin {0}.\n\nIt's incompatible with the current MediaPortal version and won't be loaded.",
                      type.FullName
                      ), "Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  Log.Warn(
                    "Plugin Manager: Plugin {0} is incompatible with the current MediaPortal version! (File: {1})",
                    type.FullName, pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
                  continue;
                }

                if (isPlugin)
                {
                  ISetupForm pluginForm = pluginObject as ISetupForm;
                  IExternalPlayer extPlayer = pluginObject as IExternalPlayer;
                  IShowPlugin showPlugin = pluginObject as IShowPlugin;

                  if (pluginForm != null)
                  {
                    ItemTag tag = new ItemTag();
                    tag.SetupForm = pluginForm;
                    tag.PluginName = pluginForm.PluginName();
                    tag.DllName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                    tag.WindowId = pluginForm.GetWindowId();

                    if (isGuiWindow)
                    {
                      GUIWindow win = (GUIWindow)pluginObject;
                      if (tag.WindowId == win.GetID)
                      {
                        tag.Type = win.GetType().ToString();
                        tag.IsProcess = false;
                        tag.IsWindow = true;
                      }
                    }
                    else if (extPlayer != null)
                    {
                      tag.IsExternalPlayer = true;
                    }
                    else
                    {
                      tag.IsProcess = true;
                    }

                    if (showPlugin != null)
                    {
                      tag.ShowDefaultHome = showPlugin.ShowDefaultHome();
                    }

                    tag.IsIncompatible = isIncompatible;

                    //LoadPluginImages(type, tag);
                    loadedPlugins.Add(tag);
                  }
                }
                else
                {
                  NonSetupWindows.Add(pluginObject);
                }
              }
            }
            // Filter plugins from e.g. dialogs or other windows.
            foreach (GUIWindow win in NonSetupWindows)
            {
              foreach (ItemTag tag in loadedPlugins)
              {
                if (tag.WindowId == win.GetID)
                {
                  tag.Type = win.GetType().ToString();
                  tag.IsProcess = false;
                  tag.IsWindow = true;
                  Log.Debug(
                    "PluginsNew: {0}, window plugin, does not implement \"ISetupForm\" and \"GUIWindow\" in the same class",
                    tag.Type);
                  break;
                }
              }
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show(
              string.Format(
                "An error occured while loading the plugin file {0}.\n\nIt's broken or incompatible with the current MediaPortal version and won't be loaded.",
                pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1)), "Plugin Manager", MessageBoxButtons.OK,
              MessageBoxIcon.Error);
            Log.Warn(
              "PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!",
              pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
            Log.Error("PluginManager: Exception: {0}", ex);
          }
        }
      }
    }

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