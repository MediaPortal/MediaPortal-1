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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.ControlDevices
{
  public static class ControlDevices
  {
    #region vars

    private static List<IControlPlugin> _plugins;
    private static List<IControlInput> _input;
    private static List<IControlOutput> _output;

    private static bool _initialized;

    #endregion

    #region Methods

    /// <summary>
    /// Load the uninitialized controlplugins, load the settings and get the
    /// enabled plugins with there input/output interfaces.
    /// </summary>
    static ControlDevices()
    {
      _initialized = false;
      BuildControlStructure();
    }

    /// <summary>
    /// Initialize the linked lists and load the settings. This
    /// don't initialize the plugins, so no hardware resources
    /// are locked.
    /// </summary>
    private static void BuildControlStructure()
    {
      // Get all plugins
      List<IControlPlugin> pluginInstances = PluginInstances();

      // Initialize the arrays
      _plugins = new List<IControlPlugin>();
      _input = new List<IControlInput>();
      _output = new List<IControlOutput>();

      // Filter the ones that are enabled
      IEnumerator<IControlPlugin> pluginIterator = pluginInstances.GetEnumerator();
      while (pluginIterator.MoveNext())
      {
        // Get the settings interface
        IControlPlugin plugin = pluginIterator.Current;
        IControlSettings settings = plugin.Settings;
        if (null == settings)
        {
          Log.Error("ControlDevices: Error getting IControlSettings of {0} in {1}", ((Type)plugin).FullName,
                    plugin.LibraryName);
          continue;
        }
        // Load the settings
        settings.Load();

        if (settings.Enabled)
        {
          if (settings.EnableInput)
          {
            IControlInput input = plugin.InputInterface;
            if (null == input)
            {
              Log.Error("ControlDevices: Error getting IControlInput Interface of {0} in {1}", ((Type)plugin).FullName,
                        plugin.LibraryName);
              continue;
            }
            _input.Add(input);
          }
          if (settings.EnableOutput)
          {
            IControlOutput output = plugin.OutputInterface;
            if (null == output)
            {
              Log.Error("ControlDevices: Error getting IControlOutput Interface of {0} in {1}", ((Type)plugin).FullName,
                        plugin.LibraryName);
              continue;
            }
            _output.Add(output);
          }
        }
      }
    }

    /// <summary>
    /// Initialize all the enabled controlplugins.
    /// </summary>
    public static void Initialize()
    {
      if (_initialized)
      {
        Log.Error("ControlDevices: Init was called before more then once - restarting devices now");
        DeInitialize();
      }

      IEnumerator<IControlPlugin> pluginIterator = _plugins.GetEnumerator();
      while (pluginIterator.MoveNext())
      {
        // Get the settings interface
        IControlPlugin plugin = pluginIterator.Current;
      }

      _initialized = true;
    }

    /// <summary>
    /// Stop the enabled controlplugins and free all the resources.
    /// </summary>
    public static void DeInitialize()
    {
      DeInitialize(false);
    }

    /// <summary>
    /// Stop the enabled controlplugins and free all the resources.
    /// </summary>
    /// <param name="silent">supress error messages</param>
    private static void DeInitialize(bool silent)
    {
      if (!_initialized)
      {
        if (!silent)
        {
          Log.Error("ControlDevices: Stop was called without Initialize - exiting");
        }
        return;
      }

      IEnumerator<IControlPlugin> pluginIterator = _plugins.GetEnumerator();
      while (pluginIterator.MoveNext())
      {
        // Get the settings interface
        IControlPlugin plugin = pluginIterator.Current;
      }

      _initialized = false;
    }

    public static bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
    {
      IEnumerator<IControlInput> inputIterator = _input.GetEnumerator();

      action = null;
      key = (char)0;
      keyCode = Keys.Escape;

      while (inputIterator.MoveNext())
      {
        // Get the settings interface
        IControlInput input = inputIterator.Current;
        if (input.UseWndProc)
        {
          if (input.WndProc(ref msg, out action, out key, out keyCode))
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Get an list of uninitialized controlplugins instances.
    /// </summary>
    /// <returns>list of uninitialized controlplugins instances</returns>
    public static List<IControlPlugin> PluginInstances()
    {
      ArrayList cioPlugins = new ArrayList();
      cioPlugins.Add(Config.GetFile(Config.Dir.Base, "RemotePlugins.dll"));

      List<IControlPlugin> pluginInstances = new List<IControlPlugin>();

      foreach (string plugin in cioPlugins)
      {
        string pluginFileName = plugin.Substring(plugin.LastIndexOf(@"\") + 1);
        Assembly assembly = null;
        try
        {
          assembly = Assembly.LoadFrom(plugin);
          if (null != assembly)
          {
            Type[] types = assembly.GetExportedTypes();

            // Enumerate each type and see if it's a plugin. One assemly can
            // have multiple controlplugins.
            foreach (Type type in types)
            {
              try
              {
                // an abstract class cannot be instanciated
                if (type.IsAbstract)
                {
                  continue;
                }

                // Try to locate the interface we're interested in
                if (null != type.GetInterface("MediaPortal.ControlDevices.IControlPlugin"))
                {
                  // Create instance of the current type
                  object instance = null;
                  instance = Activator.CreateInstance(type);
                  if (null == instance)
                  {
                    Log.Error("ControlDevices: Error creating instance of {0} in control plugin {1}", type.FullName,
                              pluginFileName);
                    continue;
                  }
                  IControlPlugin controlPluginInterface = instance as IControlPlugin;
                  if (null == controlPluginInterface)
                  {
                    Log.Error("ControlDevices: Error getting IControlPlugin of {0} in {1}", type.FullName,
                              pluginFileName);
                    continue;
                  }

                  Log.Debug("ControlDevices: Found controlplugin {0} in {1}", type.FullName, pluginFileName);
                  pluginInstances.Add(controlPluginInterface);
                }
              }
              catch (Exception ex)
              {
                string message = String.Format("Plugin {0} is {1} incompatible with the current MediaPortal version!",
                                               type.FullName, pluginFileName);
                MessageBox.Show(string.Format("An error occured while loading a plugin.\n\n{0}", message,
                                              "Control Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error));
                Log.Error("Remote: {0}", message);
                Log.Error(ex);
                continue;
              }
            }
          }
        }
        catch (Exception ex)
        {
          string message = String.Format(
            "Plugin file {0} broken or incompatible with the current MediaPortal version!", pluginFileName);
          MessageBox.Show(string.Format("An error occured while loading a plugin.\n\n{0}", message,
                                        "Control Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error));
          Log.Error("Remote: {0}", message);
          Log.Error(ex);
        }

        //;
      }
      return pluginInstances;
    }

    #endregion Methods
  }
}