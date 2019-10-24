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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TvEngine;
using TvLibrary.Log;
using MediaPortal.Common.Utils;


namespace TvService
{
  internal class PluginLoader
  {
    private readonly List<ITvServerPlugin> _plugins = new List<ITvServerPlugin>();

    /// <summary>
    /// returns a list of all plugins loaded.
    /// </summary>
    /// <value>The plugins.</value>
    public List<ITvServerPlugin> Plugins
    {
      get { return _plugins; }
    }

    /// <summary>
    /// Loads all plugins.
    /// </summary>
    public void Load()
    {
      _plugins.Clear();
      try
      {
        // Load plugins from "plugins" subfolder, relative to calling assembly's location
        string pluginFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Plugins");
        string[] strFiles = System.IO.Directory.GetFiles(pluginFolder, "*.dll");
        foreach (string strFile in strFiles)
          LoadPlugin(strFile);
      }
      catch (Exception)
      {
        Log.Warn("PluginManager: Error while loading dll's");
      }
    }

    /// <summary>
    /// Loads the plugin.
    /// </summary>
    /// <param name="strFile">The STR file.</param>
    private void LoadPlugin(string strFile)
    {
      Type[] foundInterfaces;

      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Type[] types = assem.GetExportedTypes();

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                if (t.IsAbstract)
                  continue;

                ITvServerPlugin plugin;
                TypeFilter myFilter2 = MyInterfaceFilter;
                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "TvEngine.ITvServerPlugin");
                  if (foundInterfaces.Length > 0)
                  {
                    if (!CompatibilityManager.IsPluginCompatible(t, true))
                    {
                      Log.Warn(
                        "PluginManager: {0} is incompatible with the current tvserver version and won't be loaded!",
                        t.FullName);
                      continue;                      
                    }
                    Object newObj = Activator.CreateInstance(t);
                    plugin = (ITvServerPlugin)newObj;
                    _plugins.Add(plugin);
                    Log.Info("PluginManager: Loaded {0} version:{1} author:{2}", plugin.Name, plugin.Version,
                                  plugin.Author);
                  }
                }
                catch (TargetInvocationException ex)
                {
                  Log.Warn(
                    "PluginManager: {0} is incompatible with the current tvserver version and won't be loaded! Ex: {1}",
                    t.FullName, ex);
                  continue;
                }
                catch (Exception ex)
                {
                  Log.Warn("Exception while loading ITvServerPlugin instances: {0}", t.FullName);
                  Log.Warn(ex.ToString());
                  Log.Warn(ex.Message);
                  Log.Warn(ex.StackTrace);
                }
              }
            }
            catch (NullReferenceException) {}
          }
        }
      }
      catch (Exception ex)
      {
        Log.Warn(
          "PluginManager: Plugin file {0} is broken or incompatible with the current tvserver version and won't be loaded!",
          strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.Warn("PluginManager: Exception: {0}", ex);
      }
    }

    private static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }
  }
}