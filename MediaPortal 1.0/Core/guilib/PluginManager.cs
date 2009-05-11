#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections;
using System.Reflection;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class PluginManager
  {
    static ArrayList _nonGuiPlugins = new ArrayList();
    static ArrayList _guiPlugins = new ArrayList();
    static ArrayList _setupForms = new ArrayList();
    static ArrayList _wakeables = new ArrayList();
    static bool _started = false;
    static bool _windowPluginsLoaded = false;
    static bool _nonWindowPluginsLoaded = false;

    static PluginManager()
    {
    }

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
      if (_nonWindowPluginsLoaded) return;
      _nonWindowPluginsLoaded = true;
      Log.Info("  PlugInManager.Load()");
      try
      {
        System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
      }
      catch (Exception) { }
      string[] strFiles = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "process"), "*.dll");
      foreach (string strFile in strFiles)
        LoadPlugin(strFile);
    }

    public static void LoadWindowPlugins()
    {
      if (_windowPluginsLoaded)
        return;

      _windowPluginsLoaded = true;
      Log.Info("  LoadWindowPlugins()");
      try
      {
        System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        System.IO.Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      }
      catch (Exception) { }
      LoadWindowPlugin(Config.GetFile(Config.Dir.Plugins, @"windows\WindowPlugins.dll")); //need to load this first!!!

      string[] strFiles = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "windows"), "*.dll");
      foreach (string strFile in strFiles)
      {
        if (strFile.ToLower().IndexOf("windowplugins.dll") >= 0) continue;
        LoadWindowPlugin(strFile);
      }
    }

    public static void Start()
    {
      if (_started)
        return;

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
        return;

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
      PluginManager.Stop();
      _nonGuiPlugins.Clear();
      WakeablePlugins.Clear();
      GUIPlugins.Clear();
      _windowPluginsLoaded = false;
      _nonWindowPluginsLoaded = false;
    }

    static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }


    static void LoadPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile))
        return;

      Type[] foundInterfaces = null;

      Log.Info("  Load plugins from : {0}", strFile);
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Log.Info("  File Version : {0}", System.Diagnostics.FileVersionInfo.GetVersionInfo(strFile).ProductVersion);
          Type[] types = assem.GetExportedTypes();

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                if (t.IsAbstract) continue;

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
                catch (System.Reflection.TargetInvocationException ex)
                {
                  Log.Error(ex);
                  Log.Error("PluginManager: {0} is incompatible with the current MediaPortal version and won't be loaded!", t.FullName);
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
                  continue;

                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.ISetupForm");
                  if (foundInterfaces.Length > 0)
                  {
                    if (newObj == null)
                      newObj = (object)Activator.CreateInstance(t);
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
                      newObj = (object)Activator.CreateInstance(t);
                    IWakeable setup = (IWakeable)newObj;
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                      _wakeables.Add(setup);
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
            catch (System.NullReferenceException)
            { }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!", strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }

    public static void LoadWindowPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile))
        return;

      Log.Info("  Load plugins from : {0}", strFile);
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Log.Info("  File Version : {0}", System.Diagnostics.FileVersionInfo.GetVersionInfo(strFile).ProductVersion);
          Type[] types = assem.GetExportedTypes();
          Type[] foundInterfaces = null;

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                if (t.IsAbstract) continue;
                Object newObj = null;
                if (t.IsSubclassOf(typeof(MediaPortal.GUI.Library.GUIWindow)))
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
                        Log.Error("Error initializing window:{0} {1} {2} {3}", win.ToString(), ex.Message, ex.Source, ex.StackTrace);
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
                      newObj = (object)Activator.CreateInstance(t);
                    ISetupForm setup = (ISetupForm)newObj;
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                      _setupForms.Add(setup);
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
                      newObj = (object)Activator.CreateInstance(t);
                    IWakeable setup = (IWakeable)newObj;
                    if (PluginEntryExists(setup.PluginName()) && IsPluginNameEnabled(setup.PluginName()))
                      _wakeables.Add(setup);
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
            catch (System.NullReferenceException)
            { }
          }
        }
      }
      catch (System.BadImageFormatException)
      {
      }
      catch (Exception ex)
      {
        Log.Info("PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!", strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }

    public static bool IsPlugInEnabled(string strDllname)
    {
      if (strDllname.IndexOf("WindowPlugins.dll") >= 0) return true;
      if (strDllname.IndexOf("ProcessPlugins.dll") >= 0) return true;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        return xmlreader.GetValueAsBool("pluginswindows", strType, true);
    }

    public static bool IsPluginNameEnabled(string strPluginName)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        return xmlreader.GetValueAsBool("plugins", strPluginName, true);
    }

    // hwahrmann: the previous method always returns true as a default, regardless if a plugin is in xml or not.
    // Don't know the reason why, but some code might rely on that and don't want to break it before release.
    public static bool IsPluginNameEnabled2(string strPluginName)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        return xmlreader.GetValueAsBool("plugins", strPluginName, false);
    }

    public static bool PluginEntryExists(string strPluginName)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        return (xmlreader.GetValueAsString("plugins", strPluginName, string.Empty) != string.Empty);
    }

    public static bool WndProc(ref System.Windows.Forms.Message msg)
    {
      bool res = false;
      foreach (IPlugin plugin in _nonGuiPlugins)
      {
        if (plugin is IPluginReceiver)
        {
          IPluginReceiver pluginRev = plugin as IPluginReceiver;
          res = pluginRev.WndProc(ref msg);
          if (res)
            break;
        }
      }
      if (!res)
      {
        // some ISetupForm plugins like tvplugin need the wndproc method to determine when system has been resumed.      
        foreach (ISetupForm plugin in _setupForms)
        {
          if (plugin is IPluginReceiver)
          {
            IPluginReceiver pluginRev = plugin as IPluginReceiver;
            res = pluginRev.WndProc(ref msg);
            if (res)
              break;
          }
        }
      }

      return res;
    }
  }
}
