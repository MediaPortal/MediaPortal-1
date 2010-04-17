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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class PluginManager
  {
    private static ArrayList _nonGuiPlugins = new ArrayList();
    private static ArrayList _guiPlugins = new ArrayList();
    private static ArrayList _setupForms = new ArrayList();
    private static ArrayList _wakeables = new ArrayList();
    private static bool _started = false;
    private static bool _windowPluginsLoaded = false;
    private static bool _nonWindowPluginsLoaded = false;

    static PluginManager() {}

    public static ArrayList GUIPlugins
    {
      get { return _guiPlugins; }
    }

    public static ArrayList NonGUIPlugins
    {
      get { return _nonGuiPlugins; }
    }

    public static ArrayList SetupForms
    {
      get { return _setupForms; }
    }

    public static ArrayList WakeablePlugins
    {
      get { return _wakeables; }
    }

    public static void Load()
    {
      if (_nonWindowPluginsLoaded)
      {
        return;
      }
      _nonWindowPluginsLoaded = true;
      Log.Info("  PlugInManager.Load()");
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
      }
      catch (Exception) {}
      string[] strFiles = Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "process"), "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
    }

    public static void LoadWindowPlugins()
    {
      if (_windowPluginsLoaded)
      {
        return;
      }

      _windowPluginsLoaded = true;
      Log.Info("  LoadWindowPlugins()");
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      }
      catch (Exception) {}
      LoadWindowPlugin(Config.GetFile(Config.Dir.Plugins, @"windows\WindowPlugins.dll")); //need to load this first!!!

      string[] strFiles = Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "windows"), "*.dll");
      foreach (string strFile in strFiles)
      {
        if (strFile.ToLower().IndexOf("windowplugins.dll") >= 0)
        {
          continue;
        }
        LoadWindowPlugin(strFile);
      }
    }

    public static void Start()
    {
      if (_started)
      {
        return;
      }

      Log.Info("  PlugInManager.Start()");
      foreach (IPlugin plugin in _nonGuiPlugins)
      {
        try
        {
          plugin.Start();
        }
        catch (Exception ex)
        {
          Log.Error("Unable to start plugin:{0} exception:{1}", plugin.ToString(), ex.ToString());
        }
      }
      _started = true;
    }

    public static void Stop()
    {
      if (!_started)
      {
        return;
      }

      Log.Info("  PlugInManager.Stop()");
      foreach (IPlugin plugin in _nonGuiPlugins)
      {
        Log.Info("PluginManager: stopping {0}", plugin.ToString());
        plugin.Stop();
      }
      _started = false;
    }

    public static void Clear()
    {
      Log.Info("PlugInManager.Clear()");
      Stop();
      _nonGuiPlugins.Clear();
      WakeablePlugins.Clear();
      GUIPlugins.Clear();
      _windowPluginsLoaded = false;
      _nonWindowPluginsLoaded = false;
    }

    private static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }


    private static void LoadPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile))
      {
        return;
      }

      Type[] foundInterfaces = null;

      Log.Info("  Load plugins from : {0}", strFile);
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Log.Info("  File Version : {0}", FileVersionInfo.GetVersionInfo(strFile).ProductVersion);
          Type[] types = assem.GetExportedTypes();

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                if (t.IsAbstract)
                {
                  continue;
                }

                Object newObj = null;
                IPlugin plugin = null;
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.IPlugin");
                  if (foundInterfaces.Length > 0)
                  {
                    newObj = (object)Activator.CreateInstance(t);
                    plugin = (IPlugin)newObj;
                  }
                }
                catch (TargetInvocationException ex)
                {
                  Log.Error(ex);
                  Log.Error(
                    "PluginManager: {0} is incompatible with the current MediaPortal version and won't be loaded!",
                    t.FullName);
                  continue;
                }
                catch (Exception iPluginException)
                {
                  Log.Error("Exception while loading IPlugin instances: {0}", t.FullName);
                  Log.Error(iPluginException.ToString());
                  Log.Error(iPluginException.Message);
                  Log.Error(iPluginException.StackTrace);
                }
                if (plugin == null)
                {
                  continue;
                }

                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.ISetupForm");
                  if (foundInterfaces.Length > 0)
                  {
                    if (newObj == null)
                    {
                      newObj = (object)Activator.CreateInstance(t);
                    }
                    ISetupForm setup = (ISetupForm)newObj;
                    // don't activate plugins that have NO entry at all in 
                    // MediaPortal.xml
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                    {
                      _setupForms.Add(setup);
                      _nonGuiPlugins.Add(plugin);
                    }
                  }
                }
                catch (Exception iSetupFormException)
                {
                  Log.Error("Exception while loading ISetupForm instances: {0}", t.FullName);
                  Log.Error(iSetupFormException.Message);
                  Log.Error(iSetupFormException.StackTrace);
                }

                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.IWakeable");
                  if (foundInterfaces.Length > 0)
                  {
                    if (newObj == null)
                    {
                      newObj = (object)Activator.CreateInstance(t);
                    }
                    IWakeable setup = (IWakeable)newObj;
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                    {
                      _wakeables.Add(setup);
                    }
                  }
                }
                catch (Exception iWakeableException)
                {
                  Log.Error("Exception while loading IWakeable instances: {0}", t.FullName);
                  Log.Error(iWakeableException.Message);
                  Log.Error(iWakeableException.StackTrace);
                }
              }
            }
            catch (NullReferenceException) {}
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info(
          "PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!",
          strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }

    public static void LoadWindowPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile))
      {
        return;
      }

      Log.Info("  Load plugins from : {0}", strFile);
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Log.Info("  File Version : {0}", FileVersionInfo.GetVersionInfo(strFile).ProductVersion);
          Type[] types = assem.GetExportedTypes();
          Type[] foundInterfaces = null;

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                if (t.IsAbstract)
                {
                  continue;
                }
                Object newObj = null;
                if (t.IsSubclassOf(typeof (GUIWindow)))
                {
                  try
                  {
                    newObj = (object)Activator.CreateInstance(t);
                    GUIWindow win = (GUIWindow)newObj;

                    if (win.GetID >= 0 && IsWindowPlugInEnabled(win.GetType().ToString()))
                    {
                      try
                      {
                        win.Init();
                      }
                      catch (Exception ex)
                      {
                        Log.Error("Error initializing window:{0} {1} {2} {3}", win.ToString(), ex.Message, ex.Source,
                                  ex.StackTrace);
                      }
                      GUIWindowManager.Add(ref win);
                    }
                    //else Log.Info("  plugin:{0} not enabled",win.GetType().ToString());
                  }
                  catch (Exception guiWindowsException)
                  {
                    Log.Error("Exception while loading GUIWindows instances: {0}", t.FullName);
                    Log.Error(guiWindowsException.Message);
                    Log.Error(guiWindowsException.StackTrace);
                  }
                }
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.ISetupForm");
                  if (foundInterfaces.Length > 0)
                  {
                    if (newObj == null)
                    {
                      newObj = (object)Activator.CreateInstance(t);
                    }
                    ISetupForm setup = (ISetupForm)newObj;
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                    {
                      _setupForms.Add(setup);
                    }
                  }
                }
                catch (Exception iSetupFormException)
                {
                  Log.Error("Exception while loading ISetupForm instances: {0}", t.FullName);
                  Log.Error(iSetupFormException.Message);
                  Log.Error(iSetupFormException.StackTrace);
                }

                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.IWakeable");
                  if (foundInterfaces.Length > 0)
                  {
                    if (newObj == null)
                    {
                      newObj = (object)Activator.CreateInstance(t);
                    }
                    IWakeable setup = (IWakeable)newObj;
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                    {
                      _wakeables.Add(setup);
                    }
                  }
                }
                catch (Exception iWakeableException)
                {
                  Log.Error("Exception while loading IWakeable instances: {0}", t.FullName);
                  Log.Error(iWakeableException.Message);
                  Log.Error(iWakeableException.StackTrace);
                }
              }
            }
            catch (NullReferenceException) {}
          }
        }
      }
      catch (BadImageFormatException) {}
      catch (Exception ex)
      {
        Log.Info(
          "PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!",
          strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }

    public static bool IsPlugInEnabled(string strDllname)
    {
      if (strDllname.IndexOf("WindowPlugins.dll") >= 0)
      {
        return true;
      }
      if (strDllname.IndexOf("ProcessPlugins.dll") >= 0)
      {
        return true;
      }

      using (Settings xmlreader = new MPSettings())
      {
        // from the assembly name check the reference to plugin name
        // if available check to see if the plugin is enabled
        // if the plugin name is unknown suggest the assembly should be loaded

        strDllname = strDllname.Substring(strDllname.LastIndexOf(@"\") + 1);
        return xmlreader.GetValueAsBool("pluginsdlls", strDllname, true);
      }
    }

    public static bool IsWindowPlugInEnabled(string strType)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return xmlreader.GetValueAsBool("pluginswindows", strType, true);
      }
    }

    public static bool IsPluginNameEnabled(string strPluginName)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return xmlreader.GetValueAsBool("plugins", strPluginName, true);
      }
    }

    // hwahrmann: the previous method always returns true as a default, regardless if a plugin is in xml or not.
    // Don't know the reason why, but some code might rely on that and don't want to break it before release.
    public static bool IsPluginNameEnabled2(string strPluginName)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return xmlreader.GetValueAsBool("plugins", strPluginName, false);
      }
    }

    public static bool PluginEntryExists(string strPluginName)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return (xmlreader.GetValueAsString("plugins", strPluginName, string.Empty) != string.Empty);
      }
    }

    public static bool WndProc(ref Message msg)
    {
      bool res = false;
      // some ISetupForm plugins like tvplugin need the wndproc method to determine when system has been resumed.      
      foreach (ISetupForm plugin in _setupForms)
      {
        if (plugin is IPluginReceiver)
        {
          IPluginReceiver pluginRev = plugin as IPluginReceiver;
          res = pluginRev.WndProc(ref msg);
          if (res)
          {
            break;
          }
        }
      }
      return res;
    }
  }
}