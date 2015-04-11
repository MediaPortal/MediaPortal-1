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

using MediaPortal.GUI.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Reflection;
using MediaPortal.Common.Utils;
using System.Windows.Forms;
using MediaPortal.Player;

namespace MediaPortal.Configuration
{
  public static class Plugins
  {
    #region Private Variables

    private static ArrayList loadedPlugins = new ArrayList();
    private static ArrayList availablePlugins = new ArrayList();
    private static bool wasLastLoadAdvanced = false;
    private static bool isLoaded = false;

    #endregion


    #region Accessors
    public static ArrayList LoadedPlugins
    {
      get
      {
        return loadedPlugins;
      }
    }
    public static bool WasLastLoadAdvanced
    {
      get
      {
        return wasLastLoadAdvanced;
      }
    }
    public static bool IsLoaded
    {
      get
      {
        return isLoaded;
      }
      set
      {
        isLoaded = value;
      }
    }
    #endregion


    #region Public Methods

    public static void LoadPlugins()
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
                    tag.expType = type;
                    tag.PluginName = pluginForm.PluginName();
                    tag.SetupForm = pluginForm;
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

    public static void ClearLoadedPlugins()
    {
      availablePlugins.Clear();
      loadedPlugins.Clear();
    }

    public static void EnumeratePlugins()
    {
      // Save to determine whether the mode has changed
      wasLastLoadAdvanced = SettingsForm.AdvancedMode;

      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "subtitle"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "tagreaders"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "externalplayers"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
    }

    #endregion


    #region Private Methods

    private static void EnumeratePluginDirectory(string directory)
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

    #endregion
  }

  public class ItemTag
  {
    public string DllName;
    public ISetupForm SetupForm;
    public Type expType { get; set; }
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

}
