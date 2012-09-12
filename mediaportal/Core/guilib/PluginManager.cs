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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Common.Utils;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class PluginManager
  {
    private class Incompatibilities
    {
      public const string ConfigFilename = "IncompatiblePlugins.xml";

      private List<Type> _incompatibleTypes = new List<Type>();
      private List<Assembly> _incompatibleAssemblies = new List<Assembly>();
      private HashSet<string> _previousIncompatibilities;
      private HashSet<string> _incompatibilities = new HashSet<string>();

      public IList<Type> IncompatibleTypes
      {
        get { return _incompatibleTypes; }
      }

      public IList<Assembly> IncompatibleAssemblies
      {
        get { return _incompatibleAssemblies; }
      }

      public void Load()
      {
        HashSet<string> incompatibilities = new HashSet<string>();
        string filename = Config.GetFile(Config.Dir.Config, ConfigFilename);
        XmlDocument document = new XmlDocument();

        try
        {
          Log.Info("Loading known incompatible plugins");
          if (File.Exists(filename))
          {
            document.Load(filename);
            foreach (XmlNode node in document.SelectNodes("/plugins/plugin"))
            {
              string pluginName = node.InnerText.Trim();
              incompatibilities.Add(pluginName);
            }
          }
          _previousIncompatibilities = incompatibilities;

        }
        catch (Exception ex)
        {
          Log.Error("Failed to load known plugin incompatibilities:");
          Log.Error(ex);
        }
      }

      public void Save()
      {
        string filename = Config.GetFile(Config.Dir.Config, ConfigFilename);
        var writer = new XmlTextWriter(filename, Encoding.UTF8);

        try
        {
          writer.WriteStartDocument();
          writer.WriteStartElement("plugins");
          foreach(var plugin in _incompatibilities)
          {
            writer.WriteElementString("plugin", plugin);
          }
          writer.WriteEndElement();
          writer.WriteEndDocument();
          writer.Close();
        }
        catch(Exception ex)
        {
          Log.Error("Failed to save known plugin incompatibilities:");
          Log.Error(ex);          
        }
      }

      public void EnsureLoaded()
      {
        if (_previousIncompatibilities == null)
        {
          Load();
        }
      }

      public void Add(Assembly plugin)
      {
        EnsureLoaded();
        string pluginName = plugin.FullName;
        if (!_previousIncompatibilities.Contains(pluginName))
        {
          _incompatibleAssemblies.Add(plugin);
        }
        _incompatibilities.Add(pluginName);
      }

      public void Add(Type plugin)
      {
        EnsureLoaded();
        string pluginName = plugin.AssemblyQualifiedName;
        if (!_previousIncompatibilities.Contains(pluginName))
        {
          _incompatibleTypes.Add(plugin);
        }
        _incompatibilities.Add(pluginName);
      }
    }

    private static ArrayList _nonGuiPlugins = new ArrayList();
    private static ArrayList _guiPlugins = new ArrayList();
    private static ArrayList _setupForms = new ArrayList();
    private static ArrayList _wakeables = new ArrayList();
    private static HashSet<String> _whiteList;
    private static Incompatibilities _incompatibilities = new Incompatibilities();
    private static bool _started = false;
    private static bool _windowPluginsLoaded = false;
    private static bool _nonWindowPluginsLoaded = false;

    static PluginManager() { }

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

    public static IList<Type> IncompatiblePlugins
    {
      get { return _incompatibilities.IncompatibleTypes; }
    }

    public static IList<Assembly> IncompatiblePluginAssemblies
    {
      get { return _incompatibilities.IncompatibleAssemblies; }
    }

    public static void LoadWhiteList(string filename)
    {
      HashSet<String> whiteList = new HashSet<string>();
      XmlDocument document = new XmlDocument();

      try
      {
        Log.Info("  Loading plugins whitelist:");
        document.Load(filename);
        foreach (XmlNode node in document.SelectNodes("/whitelist/plugin"))
        {
          string pluginName = node.InnerText.Trim();
          if (!whiteList.Contains(pluginName))
          {
            whiteList.Add(pluginName);
            Log.Info("    {0}", pluginName);
          }
        }
        _whiteList = whiteList;
      }
      catch (Exception ex)
      {
        Log.Error("Failed to load plugins whitelist:");
        Log.Error(ex);
      }
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
      catch (Exception) { }

      string[] strFiles = MediaPortal.Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "process"), "dll");

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
      catch (Exception) { }
      LoadWindowPlugin(Config.GetFile(Config.Dir.Plugins, @"windows\WindowPlugins.dll")); //need to load this first!!!

      string[] strFiles = MediaPortal.Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "windows"), "dll");

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

      _incompatibilities.Save();

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
      _incompatibilities = new Incompatibilities();
      _windowPluginsLoaded = false;
      _nonWindowPluginsLoaded = false;
    }

    private static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }

    public static void CheckExternalPlayersCompatibility()
    {
      Log.Info("Checking external players plugins compatibility");
      string[] fileList = MediaPortal.Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "ExternalPlayers"), "dll");
      foreach (string fileName in fileList)
      {
        CheckPluginCompatibility(fileName, typeof(Player.IExternalPlayer));
      }
    }

    private static void CheckPluginCompatibility(string strFile, Type pluginInterface)
    {
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Type[] types = assem.GetExportedTypes();

          if (types.Any(t => t.IsClass && !t.IsAbstract && pluginInterface.IsAssignableFrom(t)) 
              && !CompatibilityManager.IsPluginCompatible(assem))
          {
            Log.Error(
              "PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!", assem.FullName);
            _incompatibilities.Add(assem);
          }
          else
          {

            foreach (Type t in types)
            {
              try
              {
                if (t.IsClass && !t.IsAbstract && pluginInterface.IsAssignableFrom(t) && !CompatibilityManager.IsPluginCompatible(t))
                {
                  Log.Error(
                    "PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!",
                    t.FullName);
                  _incompatibilities.Add(t);
                }
              }
              catch (NullReferenceException) {}
            }
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
          TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);

          if (types.Any(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t)) 
              && !CompatibilityManager.IsPluginCompatible(assem))
          {
            Log.Error(
              "PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!", assem.FullName);
            _incompatibilities.Add(assem);
          }
          else
          {

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
                  try
                  {
                    foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.IPlugin");
                    if (foundInterfaces.Length > 0)
                    {
                      if (!CompatibilityManager.IsPluginCompatible(t))
                      {
                        Log.Error(
                          "PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!",
                          t.FullName);
                        _incompatibilities.Add(t);
                        continue;
                      }

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

                  // If we get to this point, the plugin has loaded successfully
                  // Mark it as compatible.
                  //MarkPluginAsCompatible(t);

                  try
                  {
                    foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.ISetupForm");
                    if (foundInterfaces.Length > 0)
                    {
                      if (newObj == null)
                      {
                        newObj = (object) Activator.CreateInstance(t);
                      }
                      ISetupForm setup = (ISetupForm) newObj;
                      // don't activate plugins that have NO entry at all in 
                      // MediaPortal.xml
                      if (!PluginEntryExists(setup.PluginName()))
                      {
                        Log.Info("PluginManager:  {0} {1} not found in Mediaportal.xml so adding it now",
                                 setup.PluginName(), t.Assembly.ManifestModule.Name);
                        AddPluginEntry(setup.PluginName(), t.Assembly.ManifestModule.Name);
                        MPSettings.Instance.SetValueAsBool("home", setup.PluginName(), false);
                        MPSettings.Instance.SetValueAsBool("myplugins", setup.PluginName(), true);
                        MPSettings.Instance.SetValueAsBool("pluginswindows", t.ToString(), true);
                      }
                      if (IsPluginNameEnabled(setup.PluginName()))
                      {
                        _setupForms.Add(setup);
                        _nonGuiPlugins.Add(plugin);
                      }
                    }
                    else
                    {
                      //IPlugin without ISetupForm, adding anyway
                      if (!PluginEntryExists(t.Name))
                      {
                        AddPluginEntry(t.Name, t.Assembly.ManifestModule.Name);
                      }
                      _nonGuiPlugins.Add(plugin);
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
          if (types.Any(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(GUIWindow))) && !CompatibilityManager.IsPluginCompatible(assem))
          {
            Log.Error(
              "PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!",
              assem.FullName);
            _incompatibilities.Add(assem);
          }
          else
          {
            //MarkPluginAsCompatible(assem);

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
                      if (!CompatibilityManager.IsPluginCompatible(t))
                      {
                        Log.Error(
                          "PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!",
                          t.FullName);
                        _incompatibilities.Add(t);
                        continue;
                      }

                      newObj = (object)Activator.CreateInstance(t);
                      GUIWindow win = (GUIWindow)newObj;

                      if (win.GetID >= 0 && IsWindowPlugInEnabled(win.GetType().ToString()))
                      {
                        try
                        {
                          win.Init();
                          _guiPlugins.Add(win);
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

                  // If we get to this point, the plugin has loaded successfully
                  // Mark it as compatible.
                  //MarkPluginAsCompatible(t);

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
                      if (!PluginEntryExists(setup.PluginName()))
                      {
                        Log.Info("PluginManager:  {0} {1} not found in Mediaportal.xml so adding it now",
                                 setup.PluginName(), t.Assembly.ManifestModule.Name);
                        AddPluginEntry(setup.PluginName(), t.Assembly.ManifestModule.Name);
                      }
                      if (IsPluginNameEnabled(setup.PluginName()))
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
      }
      catch (BadImageFormatException) { }
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

      strDllname = strDllname.Substring(strDllname.LastIndexOf(@"\") + 1);
      // If a whitelist is applicable check if the plugin name is in the whitelist
      if (_whiteList != null && !_whiteList.Contains(strDllname))
      {
        return false;
      }

      // from the assembly name check the reference to plugin name
      // if available check to see if the plugin is enabled
      // if the plugin name is unknown suggest the assembly should be loaded

      return MPSettings.Instance.GetValueAsBool("pluginsdlls", strDllname, true);
    }

    public static bool IsWindowPlugInEnabled(string strType)
    {
      return MPSettings.Instance.GetValueAsBool("pluginswindows", strType, true);
    }

    public static bool IsPluginNameEnabled(string strPluginName)
    {
      return MPSettings.Instance.GetValueAsBool("plugins", strPluginName, true);
    }

    // hwahrmann: the previous method always returns true as a default, regardless if a plugin is in xml or not.
    // Don't know the reason why, but some code might rely on that and don't want to break it before release.
    public static bool IsPluginNameEnabled2(string strPluginName)
    {
      return MPSettings.Instance.GetValueAsBool("plugins", strPluginName, false) && IsPluginNameLoaded(strPluginName);
    }

    public static bool IsPluginNameLoaded(string strPluginName)
    {
      if (MediaPortal.Player.PlayerFactory.ExternalPlayerList != null &&
          MediaPortal.Player.PlayerFactory.ExternalPlayerList.Count > 0)
      {
        foreach (ISetupForm sf in MediaPortal.Player.PlayerFactory.ExternalPlayerList)
        {
          if (null != sf && sf.PluginName() == strPluginName && !string.IsNullOrEmpty(sf.PluginName()))
            return true;
        }
      }

      if (_setupForms != null && _setupForms.Count > 0)
      {
        foreach (ISetupForm sf in _setupForms)
        {
          if (null != sf && sf.PluginName() == strPluginName && !string.IsNullOrEmpty(sf.PluginName()))
            return true;
        }
      }

      return false;
    }

    public static bool PluginEntryExists(string strPluginName)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return (xmlreader.GetValueAsString("plugins", strPluginName, string.Empty) != string.Empty);
      }
    }

    public static void AddPluginEntry(string strPluginName, string DllName)
    {
      MPSettings.Instance.SetValueAsBool("plugins", strPluginName, true);
      MPSettings.Instance.SetValueAsBool("pluginsdlls", DllName, true);
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