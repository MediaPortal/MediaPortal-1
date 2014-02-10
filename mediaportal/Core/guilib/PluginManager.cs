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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Common.Utils;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Library
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// 
  /// </summary>
  public class PluginManager
  {
    private class Incompatibilities
    {
      private const string ConfigFilename = "IncompatiblePlugins.xml";

      private readonly List<Type> _incompatibleTypes = new List<Type>();
      private readonly List<Assembly> _incompatibleAssemblies = new List<Assembly>();
      private HashSet<string> _previousIncompatibilities;
      // ReSharper disable MemberHidesStaticFromOuterClass
      private readonly HashSet<string> _incompatibilities = new HashSet<string>();
      // ReSharper restore MemberHidesStaticFromOuterClass

      public IList<Type> IncompatibleTypes
      {
        get { return _incompatibleTypes; }
      }

      public IList<Assembly> IncompatibleAssemblies
      {
        get { return _incompatibleAssemblies; }
      }

      // ReSharper disable MemberHidesStaticFromOuterClass
      private void Load()
      // ReSharper restore MemberHidesStaticFromOuterClass
      {
        var incompatibilities = new HashSet<string>();
        string filename = Config.GetFile(Config.Dir.Config, ConfigFilename);
        var document = new XmlDocument();

        try
        {
          Log.Info("Loading known incompatible plugins");
          if (File.Exists(filename))
          {
            document.Load(filename);
            XmlNodeList xmlNodeList = document.SelectNodes("/plugins/plugin");
            if (xmlNodeList != null)
            {
              foreach (XmlNode node in xmlNodeList)
              {
                string pluginName = node.InnerText.Trim();
                incompatibilities.Add(pluginName);
              }
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

      private void EnsureLoaded()
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
        if (pluginName != null && !_previousIncompatibilities.Contains(pluginName))
        {
          _incompatibleTypes.Add(plugin);
        }
        if (pluginName != null)
        {
          _incompatibilities.Add(pluginName);
        }
      }
    }

    // ReSharper disable InconsistentNaming
    private static readonly ArrayList _nonGuiPlugins = new ArrayList();
    private static readonly ArrayList _guiPlugins = new ArrayList();
    private static readonly ArrayList _setupForms = new ArrayList();
    private static readonly ArrayList _wakeables = new ArrayList();
    // ReSharper restore InconsistentNaming
    private static HashSet<String> _whiteList;
    private static Incompatibilities _incompatibilities = new Incompatibilities();
    private static bool _started;
    private static bool _windowPluginsLoaded;
    private static bool _nonWindowPluginsLoaded;

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
      var whiteList = new HashSet<string>();
      var document = new XmlDocument();

      try
      {
        Log.Info("Loading plugins whitelist:");
        document.Load(filename);
        XmlNodeList xmlNodeList = document.SelectNodes("/whitelist/plugin");
        if (xmlNodeList != null)
        {
          foreach (XmlNode node in xmlNodeList)
          {
            string pluginName = node.InnerText.Trim();
            if (!whiteList.Contains(pluginName))
            {
              whiteList.Add(pluginName);
              Log.Info("    {0}", pluginName);
            }
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

    /// <summary>
    /// 
    /// </summary>
    public static void LoadProcessPlugins()
    {
      if (_nonWindowPluginsLoaded)
      {
        return;
      }

      Log.Debug("PlugInManager: LoadProcessPlugins()");
      _nonWindowPluginsLoaded = true;
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch (Exception) { }
      // ReSharper restore EmptyGeneralCatchClause

      string[] strFiles = MediaPortal.Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "process"), "dll");

      foreach (string strFile in strFiles)
      {
        // get relative plugin file name
        string removeString = Config.GetFolder(Config.Dir.Plugins);
        int index = strFile.IndexOf(removeString, StringComparison.Ordinal);
        string pluginFile = (index < 0) ? strFile : strFile.Remove(index, removeString.Length);

        DateTime startTime = DateTime.Now;
        Log.Debug("PluginManager: Begin Loading '{0}'", pluginFile);

        LoadPlugin(strFile);

        DateTime endTime = DateTime.Now;
        TimeSpan runningTime = endTime - startTime;
        Log.Debug("PluginManager: End loading '{0}' ({1} ms running time)", pluginFile, runningTime.TotalMilliseconds);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void LoadWindowPlugins()
    {
      using (Settings xmlreader = new MPSettings())
      {
        if (xmlreader.GetValueAsBool("general", "threadedstartup", false))
        {
          LoadWindowPluginsThreaded();
        }
        else
        {
          LoadWindowPluginsNonThreaded();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void LoadWindowPluginsNonThreaded()
    {
      if (_windowPluginsLoaded)
      {
        return;
      }

      Log.Debug("PlugInManager: LoadWindowPlugins()");

      _windowPluginsLoaded = true;
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch (Exception) { }
      // ReSharper restore EmptyGeneralCatchClause

      string[] strFiles = MediaPortal.Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "windows"), "dll");

      // load all window plugins in the main thread
      foreach (string file in strFiles)
      {
        // get relative plugin file name
        string removeString = Config.GetFolder(Config.Dir.Plugins);
        int index = file.IndexOf(removeString, StringComparison.Ordinal);
        string pluginFile = (index < 0) ? file : file.Remove(index, removeString.Length);

        DateTime startTime = DateTime.Now;
        Log.Debug("PluginManager: Begin loading '{0}' (non threaded)", pluginFile);

        LoadWindowPlugin(file);

        DateTime endTime = DateTime.Now;
        TimeSpan runningTime = endTime - startTime;
        Log.Debug("PluginManager: End loading '{0}' ({1} ms running time)", pluginFile, runningTime.TotalMilliseconds);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    private static void LoadWindowPluginsThreaded()
    {
      if (_windowPluginsLoaded)
      {
        return;
      }

      Log.Debug("PlugInManager: LoadWindowPlugins()");

      _windowPluginsLoaded = true;
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Plugins));
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch (Exception) { }
      // ReSharper restore EmptyGeneralCatchClause

      string[] strFiles = MediaPortal.Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "windows"), "dll");

      int pluginsToLoad = strFiles.Length;
      using (var resetEvent = new ManualResetEvent(false))
      {
        // initialize state list
        var states = new List<int>();
        for (int i = 0; i < strFiles.Length; i++)
        {
          states.Add(i);
        }

        // load all window plugins using available worker threads
        for (int i = 0; i < strFiles.Length; i++)
        {
          string file = strFiles[i];
          DateTime queueTime = DateTime.Now;
          ThreadPool.QueueUserWorkItem(x =>
          {
            // get relative plugin file name
            string removeString = Config.GetFolder(Config.Dir.Plugins);
            int index = file.IndexOf(removeString, StringComparison.Ordinal);
            string pluginFile = (index < 0) ? file : file.Remove(index, removeString.Length);

            DateTime startTime = DateTime.Now;
            TimeSpan delay = startTime - queueTime;
            Log.Debug("PluginManager: Begin loading '{0}' ({1} ms thread delay)", pluginFile, delay.TotalMilliseconds);

            LoadWindowPlugin(file);

            DateTime endTime = DateTime.Now;
            TimeSpan runningTime = endTime - startTime;
            Log.Debug("PluginManager: End loading '{0}' ({1} ms running time)", pluginFile, runningTime.TotalMilliseconds);

            // safely decrement the counter
            if (Interlocked.Decrement(ref pluginsToLoad) == 0)
            {
              // ReSharper disable AccessToDisposedClosure
              resetEvent.Set();
              // ReSharper restore AccessToDisposedClosure
            }
          }, states[i]);
        }

        // wait until all worker threads are finished
        resetEvent.WaitOne();
      }      
    }

    /// <summary>
    /// 
    /// </summary>
    public static void StartProcessPlugins()
    {
      if (_started)
      {
        return;
      }

      Log.Debug("PlugInManager: StartProcessPlugins()");

      _incompatibilities.Save();

      foreach (IPlugin plugin in _nonGuiPlugins)
      {
        DateTime startTime = DateTime.Now;
        Log.Debug("PluginManager: Begin starting '{0}'", plugin.ToString());

        try
        {
          plugin.Start();
        }
        catch (Exception ex)
        {
          Log.Error("PluginManager: Unable to start plugin: {0} exception: {1}", plugin.ToString(), ex.ToString());
        }

        DateTime endTime = DateTime.Now;
        TimeSpan runningTime = endTime - startTime;
        Log.Debug("PluginManager: End starting '{0}' ({1} ms running time)", plugin.ToString(), runningTime.TotalMilliseconds);
     }
     _started = true;
   }


    /// <summary>
    /// 
    /// </summary>
    public static void Stop()
    {
      if (!_started)
      {
        return;
      }

      Log.Debug("PlugInManager: Stop()");
      foreach (IPlugin plugin in _nonGuiPlugins)
      {
        Log.Debug("PluginManager: Stopping plugin '{0}'", plugin.ToString());
        try
        {
          plugin.Stop();
        }
        catch (Exception ex)
        {
          Log.Error("PluginManager: Unable to stop plugin: {0} exception: {1}", plugin.ToString(), ex.ToString());
        }
      }
      _started = false;
    }


    /// <summary>
    /// 
    /// </summary>
    public static void Clear()
    {
      Log.Debug("PlugInManager: Clear()");
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

          if (types.Any(t => t.IsClass && !t.IsAbstract && pluginInterface.IsAssignableFrom(t)) && !CompatibilityManager.IsPluginCompatible(assem))
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
                  Log.Error("PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!", t.FullName);
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
          strFile.Substring(strFile.LastIndexOf(@"\", StringComparison.Ordinal) + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="strFile"></param>
    private static void LoadPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile))
      {
        return;
      }

      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Log.Info("PluginManager: Plugin: '{0}' / Version: {1}", strFile, FileVersionInfo.GetVersionInfo(strFile).ProductVersion);

          Type[] types = assem.GetExportedTypes();
          TypeFilter myFilter2 = MyInterfaceFilter;

          if (types.Any(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t)) && !CompatibilityManager.IsPluginCompatible(assem))
          {
            Log.Error("PluginManager: '{0}' is tagged as incompatible with the current MediaPortal version and won't be loaded!", assem.FullName);
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
                  Type[] foundInterfaces;
                  try
                  {
                    foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.IPlugin");
                    if (foundInterfaces.Length > 0)
                    {
                      if (!CompatibilityManager.IsPluginCompatible(t))
                      {
                        Log.Error("PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!", t.FullName);
                        _incompatibilities.Add(t);
                        continue;
                      }

                      newObj = Activator.CreateInstance(t);
                      plugin = (IPlugin)newObj;
                    }
                  }
                  catch (TargetInvocationException ex)
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
                        newObj = Activator.CreateInstance(t);
                      }
                      var setup = (ISetupForm) newObj;
                      // don't activate plugins that have NO entry at all in MediaPortal.xml
                      if (!PluginEntryExists(setup.PluginName()))
                      {
                        Log.Info("PluginManager: {0} {1} not found in Mediaportal.xml so adding it now", setup.PluginName(), t.Assembly.ManifestModule.Name);
                        AddPluginEntry(setup.PluginName(), t.Assembly.ManifestModule.Name);
                        MPSettings.Instance.SetValueAsBool("home", setup.PluginName(), false);
                        MPSettings.Instance.SetValueAsBool("myplugins", setup.PluginName(), true);
                        MPSettings.Instance.SetValueAsBool("pluginswindows", t.ToString(), true);
                      }

                      // Load PowerScheduler if PS++ is enabled and remove PS++ entry
                      if (setup.PluginName() == "PowerScheduler")
                      {
                        if (MPSettings.Instance.GetValueAsBool("plugins", "PowerScheduler++", false))
                        {
                          MPSettings.Instance.SetValueAsBool("plugins", "PowerScheduler", true);
                        }
                        MPSettings.Instance.RemoveEntry("plugins", "PowerScheduler++");
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
                        newObj = Activator.CreateInstance(t);
                      }
                      var setup = (IWakeable)newObj;
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
        Log.Info("PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!", strFile.Substring(strFile.LastIndexOf(@"\", StringComparison.Ordinal) + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }

    public static void LoadWindowPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile))
      {
        return;
      }

      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
          Log.Info("PluginManager: '{0}' file version: {1}", strFile, FileVersionInfo.GetVersionInfo(strFile).ProductVersion);

          Type[] types = assem.GetExportedTypes();
          if (types.Any(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(GUIWindow))) && !CompatibilityManager.IsPluginCompatible(assem))
          {
            Log.Error("PluginManager: '{0}' is tagged as incompatible with the current MediaPortal version and won't be loaded!", assem.FullName);
            _incompatibilities.Add(assem);
          }
          else
          {
            //MarkPluginAsCompatible(assem);

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
                        Log.Error("PluginManager: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!", t.FullName);
                        _incompatibilities.Add(t);
                        continue;
                      }

                      newObj = Activator.CreateInstance(t);
                      var win = (GUIWindow)newObj;

                      if (win.GetID >= 0 && IsWindowPlugInEnabled(win.GetType().ToString()))
                      {
                        try
                        {
                          win.Init();
                          _guiPlugins.Add(win);
                        }
                        catch (Exception ex)
                        {
                          Log.Error("Error initializing window:{0} {1} {2} {3}", win.ToString(), ex.Message, ex.Source, ex.StackTrace);
                        }
                        GUIWindowManager.Add(ref win);
                      }
                    }
                    catch (Exception guiWindowsException)
                    {
                      Log.Error("Exception while loading GUIWindows instances: {0}", t.FullName);
                      Log.Error(guiWindowsException.Message);
                      Log.Error(guiWindowsException.StackTrace);
                    }
                  }

                  TypeFilter myFilter2 = MyInterfaceFilter;
                  Type[] foundInterfaces;
                  try
                  {
                    foundInterfaces = t.FindInterfaces(myFilter2, "MediaPortal.GUI.Library.ISetupForm");
                    if (foundInterfaces.Length > 0)
                    {
                      if (newObj == null)
                      {
                        newObj = Activator.CreateInstance(t);
                      }
                      var setup = (ISetupForm)newObj;
                      if (!PluginEntryExists(setup.PluginName()))
                      {
                        Log.Info("PluginManager: {0} {1} not found in Mediaportal.xml so adding it now", setup.PluginName(), t.Assembly.ManifestModule.Name);
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
                        newObj = Activator.CreateInstance(t);
                      }
                      var setup = (IWakeable)newObj;
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
        Log.Info("PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!", strFile.Substring(strFile.LastIndexOf(@"\", StringComparison.Ordinal) + 1));
        Log.Info("PluginManager: Exception: {0}", ex);
      }
    }


    public static bool IsPlugInEnabled(string strDllname)
    {
      if (strDllname.IndexOf("ProcessPlugins.dll", StringComparison.Ordinal) >= 0)
      {
        return true;
      }

      strDllname = strDllname.Substring(strDllname.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
      // If a white list is applicable check if the plugin name is in the white list
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
      if (Player.PlayerFactory.ExternalPlayerList != null && Player.PlayerFactory.ExternalPlayerList.Count > 0)
      {
        if (Player.PlayerFactory.ExternalPlayerList.Cast<ISetupForm>().Any(sf => sf != null && sf.PluginName() == strPluginName && !string.IsNullOrEmpty(sf.PluginName())))
        {
          return true;
        }
      }

      if (_setupForms != null && _setupForms.Count > 0)
      {
        return _setupForms.Cast<ISetupForm>().Any(sf => sf != null && sf.PluginName() == strPluginName && !string.IsNullOrEmpty(sf.PluginName()));
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

    public static void AddPluginEntry(string strPluginName, string dllName)
    {
      MPSettings.Instance.SetValueAsBool("plugins", strPluginName, true);
      MPSettings.Instance.SetValueAsBool("pluginsdlls", dllName, true);
    }

    public static bool WndProc(ref Message msg)
    {
      bool res = false;
      // some ISetupForm plugins like tv plugin need the WndProc() method to determine when system has been resumed.      
      foreach (ISetupForm plugin in _setupForms)
      {
        if (plugin is IPluginReceiver)
        {
          var pluginRev = plugin as IPluginReceiver;
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