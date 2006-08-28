/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Reflection;
using MediaPortal.Util;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
  public class PluginManager
  {
    static ArrayList _NonGUIPlugins = new ArrayList();
    static ArrayList _GUIPlugins = new ArrayList();
    static ArrayList _SetupForms = new ArrayList();
    static ArrayList _Wakeables = new ArrayList();
    static bool _Started=false;
    static bool windowPluginsLoaded=false;
    static bool nonWindowPluginsLoaded=false;

    static PluginManager()
    {
    }

    static public ArrayList GUIPlugins
    {
      get 
      {
        return _GUIPlugins;
      }
    }

    static public ArrayList NonGUIPlugins
    {
      get 
      {
        return _NonGUIPlugins;
      }
    }

    static public ArrayList SetupForms
    {
      get 
      {
        return _SetupForms;
      }
    }
    static public ArrayList WakeablePlugins
    {
      get 
      {
        return _Wakeables;
      }
    }

    static public void Load()
    {
      if (nonWindowPluginsLoaded) return;
      nonWindowPluginsLoaded=true;
      Log.Info("  PlugInManager.Load()");
      try
      {
        System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Plugins));
        System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Plugins) + "process");
      }
      catch(Exception){}
      string[] strFiles=System.IO.Directory.GetFiles(Config.Get(Config.Dir.Plugins) + "process", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
    }
    static public void LoadWindowPlugins()
    {
      if (windowPluginsLoaded) return;
      windowPluginsLoaded=true;
      Log.Info("  LoadWindowPlugins()");
      try
      {
        System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Plugins));
        System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Plugins) + "windows");
      }
      catch(Exception){}
      LoadWindowPlugin(Config.Get(Config.Dir.Plugins) + @"windows\WindowPlugins.dll");//need to load this first!!!

      string [] strFiles=System.IO.Directory.GetFiles(Config.Get(Config.Dir.Plugins) + "windows", "*.dll");
      foreach (string strFile in strFiles)
      {
        if (strFile.ToLower().IndexOf("windowplugins.dll") >= 0) continue;
        LoadWindowPlugin(strFile);
      }
      //LoadWindowPlugin("Dialogs.dll");
    }

    static public void Start()
    {
      if (_Started) return;
      
      Log.Info("  PlugInManager.Start()");
      foreach (IPlugin plugin in _NonGUIPlugins)
      {
        try
        {
          plugin.Start();
        }
        catch(Exception ex)
        {
          Log.Error("Unable to start plugin:{0} exception:{1}", plugin.ToString(), ex.ToString());
        }
      }
      _Started=true;
    }

    static public void Stop()
    {
      if (!_Started) return;
      Log.Info("  PlugInManager.Stop()");
      foreach (IPlugin plugin in _NonGUIPlugins)
      {
        Log.Info("PluginManager: stopping {0}", plugin.ToString());
        plugin.Stop();
      }
      _Started=false;
    }
    static public void Clear()
    {
      Log.Info("PlugInManager.Clear()");
      PluginManager.Stop();
      _NonGUIPlugins.Clear();
      WakeablePlugins.Clear();
      GUIPlugins.Clear();
      windowPluginsLoaded=false;
      nonWindowPluginsLoaded=false;
    }

    static bool MyInterfaceFilter(Type typeObj,Object criteriaObj)
    {
      if( typeObj.ToString() .Equals( criteriaObj.ToString()))
        return true;
      else
        return false;
    }


    static void LoadPlugin(string strFile)
    {
      Type[] foundInterfaces = null;
      if (!IsPlugInEnabled(strFile)) return;
      Log.Info("  Load plugins from :{0}", strFile);
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem!=null)
        {
          Type[] types = assem.GetExportedTypes();

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                if( t.IsAbstract ) continue;

                Object newObj = null;
                IPlugin  plugin=null;
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                try
                {
                  foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.IPlugin");
                  if (foundInterfaces.Length>0)
                  {
                    newObj=(object)Activator.CreateInstance(t);
                    plugin=(IPlugin)newObj;
                  }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                  Log.Error(ex);
                  Log.Error("PluginManager: {0} is incompatible with the current MediaPortal version and won't be loaded!", t.FullName);
                  continue;
                }
                catch (Exception iPluginException )
                {
                  Log.Error("Exception while loading IPlugin instances: {0}", t.FullName);
                  Log.Error(iPluginException.ToString());
                  Log.Error(iPluginException.Message);
                  Log.Error(iPluginException.StackTrace);
                }
                if (plugin==null)
                  continue;

                try
                {
                  foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.ISetupForm");
                  if (foundInterfaces.Length>0)
                  {
                    if (newObj==null)
                      newObj=(object)Activator.CreateInstance(t);
                    ISetupForm  setup=(ISetupForm)newObj;
                    // waeberd:
                    // don't activate plugins that have NO entry at all in 
                    // MediaPortal.xml
                    if (PluginEntryExists(setup.PluginName()))
                    {
                      if (IsPluginNameEnabled(setup.PluginName()))
                      {
                        _SetupForms.Add(setup);
                        _NonGUIPlugins.Add(plugin);
                      }
                    }
                  }
                }
                catch( Exception iSetupFormException )
                {
                  Log.Error("Exception while loading ISetupForm instances: {0}", t.FullName);
                  Log.Error(iSetupFormException.Message);
                  Log.Error(iSetupFormException.StackTrace);
                }

                try
                {
                  foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.IWakeable");
                  if (foundInterfaces.Length>0)
                  {
                    if (newObj==null)
                      newObj=(object)Activator.CreateInstance(t);
                    IWakeable  setup=(IWakeable)newObj;
                    if (IsPluginNameEnabled(setup.PluginName()))
                    {
                      _Wakeables.Add(setup);
                    }
                  }
                }
                catch( Exception iWakeableException )
                {
                  Log.Error("Exception while loading IWakeable instances: {0}", t.FullName);
                  Log.Error(iWakeableException.Message);
                  Log.Error(iWakeableException.StackTrace);
                }
              }
            }
            catch (System.NullReferenceException)
            {
							
            }
          }
        }
      }
      catch (Exception ex)
      {
        string strEx=ex.Message;
      }
    }

    static public void LoadWindowPlugin(string strFile)
    {
      if (!IsPlugInEnabled(strFile)) return;

      Log.Info("  Load plugins from :{0}", strFile);
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem != null)
        {
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
                    if (IsPluginNameEnabled(setup.PluginName()))
                    {
                      _SetupForms.Add(setup);
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
                    if (IsPluginNameEnabled(setup.PluginName()))
                    {
                      _Wakeables.Add(setup);
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
            catch (System.NullReferenceException)
            {

            }
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


    static public bool IsPlugInEnabled(string strDllname)
    {
      if (strDllname.IndexOf("WindowPlugins.dll")>=0) return true;
      if (strDllname.IndexOf("ProcessPlugins.dll")>=0) return true;

      using (MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        // from the assembly name check the reference to plugin name
        // if available check to see if the plugin is enabled
        // if the plugin name is unknown suggest the assembly should be loaded
        
        strDllname=strDllname.Substring(strDllname.LastIndexOf(@"\")+1);
        bool bEnabled=xmlreader.GetValueAsBool("pluginsdlls",strDllname,true);
        return bEnabled;
        
      } 
    }
    static public bool IsWindowPlugInEnabled(string strType)
    {

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        bool bEnabled=xmlreader.GetValueAsBool("pluginswindows",strType,true);
        return bEnabled;
        
      } 
    }
    static public bool IsPluginNameEnabled(string strPluginName)
    {

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        bool bEnabled=xmlreader.GetValueAsBool("plugins",strPluginName,true);
        return bEnabled;
        
      } 
    }

    static public bool PluginEntryExists(string strPluginName)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        string val = xmlreader.GetValueAsString("plugins", strPluginName, "");
        return (val != "");
      } 
    }

    static public bool WndProc(ref System.Windows.Forms.Message msg)
    {
      bool res = false;
      foreach (IPlugin plugin in _NonGUIPlugins)
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
