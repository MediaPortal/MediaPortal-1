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
using System;
using System.Collections.Generic;
using System.Reflection;
using TvEngine;
using TvLibrary.Log;

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
        string[] strFiles = System.IO.Directory.GetFiles("plugins", "*.dll");
        foreach (string strFile in strFiles)
          LoadPlugin(strFile);
      }
      catch (Exception)
      {
        Log.WriteFile("PluginManager: Error while loading dll's");
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
                    Object newObj = Activator.CreateInstance(t);
                    plugin = (ITvServerPlugin)newObj;
                    _plugins.Add(plugin);
                    Log.WriteFile("PluginManager: Loaded {0} version:{1} author:{2}", plugin.Name, plugin.Version,
                                  plugin.Author);
                  }
                }
                catch (TargetInvocationException)
                {
                  Log.WriteFile(
                    "PluginManager: {0} is incompatible with the current tvserver version and won't be loaded!",
                    t.FullName);
                  continue;
                }
                catch (Exception ex)
                {
                  Log.WriteFile("Exception while loading ITvServerPlugin instances: {0}", t.FullName);
                  Log.WriteFile(ex.ToString());
                  Log.WriteFile(ex.Message);
                  Log.WriteFile(ex.StackTrace);
                }
              }
            }
            catch (NullReferenceException) {}
          }
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(
          "PluginManager: Plugin file {0} is broken or incompatible with the current tvserver version and won't be loaded!",
          strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.WriteFile("PluginManager: Exception: {0}", ex);
      }
    }

    private static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }
  }
}