using System;
using System.Collections;
using System.Reflection;

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
    private PluginManager()
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
      Log.Write("PlugInManager.Load()");
			try
			{
				System.IO.Directory.CreateDirectory(@"plugins");
				System.IO.Directory.CreateDirectory(@"plugins\process");
			}
			catch(Exception){}
      string[] strFiles=System.IO.Directory.GetFiles(@"plugins\process", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
    }
    static public void LoadWindowPlugins()
    {
      if (windowPluginsLoaded) return;
      windowPluginsLoaded=true;
      Log.Write("PlugInManager.LoadWindowPlugins()");
			try
			{
				System.IO.Directory.CreateDirectory(@"plugins");
				System.IO.Directory.CreateDirectory(@"plugins\windows");
			}
			catch(Exception){}
      string [] strFiles=System.IO.Directory.GetFiles(@"plugins\windows", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadWindowPlugin(strFile);
      }
      LoadWindowPlugin("Dialogs.dll");
    }

    static public void Start()
    {
      if (_Started) return;
      
      Log.Write("PlugInManager.Start()");
      foreach (IPlugin plugin in _NonGUIPlugins)
      {
        try
        {
          plugin.Start();
        }
        catch(Exception ex)
        {
          Log.Write("Unable to start plugin:{0} exception:{1}", plugin.ToString(), ex.ToString());
        }
      }
      _Started=true;
    }

    static public void Stop()
    {
      if (!_Started) return;
      Log.Write("PlugInManager.Stop()");
      foreach (IPlugin plugin in _NonGUIPlugins)
      {
        plugin.Stop();
      }
      _Started=false;
    }
    static public void Clear()
    {
      Log.Write("PlugInManager.Clear()");
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
      if (!IsPlugInEnabled(strFile)) return;
			Log.Write("Load plugins from :{0}", strFile);
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
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                Type[] foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.IPlugin");
                if (foundInterfaces.Length>0)
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  IPlugin  plugin=(IPlugin)newObj;
                  _NonGUIPlugins.Add(plugin);
                  
                  //Log.Write("  load plugin:{0} in {1}",t.ToString(), strFile);
                }
                foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.ISetupForm");
                if (foundInterfaces.Length>0)
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  ISetupForm  setup=(ISetupForm)newObj;
                  
                  if (IsPluginNameEnabled(setup.PluginName()))
                  {
                    _SetupForms.Add(setup);
                  }
                }
				        foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.IWakeable");
				        if (foundInterfaces.Length>0)
				        {
					        object newObj=(object)Activator.CreateInstance(t);
					        IWakeable  setup=(IWakeable)newObj;
					        if (IsPluginNameEnabled(setup.PluginName()))
					        {
						        _Wakeables.Add(setup);
					        }
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

			Log.Write("Load plugins from :{0}", strFile);
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
                if (t.IsSubclassOf (typeof(MediaPortal.GUI.Library.GUIWindow)))
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  GUIWindow win=(GUIWindow)newObj;
                  if (IsWindowPlugInEnabled(win.GetType().ToString()))
                  {
										try
										{
											win.Init();
										}
										catch(Exception ex)
										{
											Log.Write("Error initializing window:{0} {1} {2} {3}", win.ToString(), ex.Message,ex.Source,ex.StackTrace);
										}
                    GUIWindowManager.Add(ref win);
                  }
                  //else Log.Write("  plugin:{0} not enabled",win.GetType().ToString());
                }
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                Type[] foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.ISetupForm");
                if (foundInterfaces.Length>0)
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  ISetupForm  setup=(ISetupForm)newObj;
                  if (IsPluginNameEnabled(setup.PluginName()))
                  {
                    _SetupForms.Add(setup);
                  }
                }
				        foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.IWakeable");
				        if (foundInterfaces.Length>0)
				        {
					        object newObj=(object)Activator.CreateInstance(t);
					        IWakeable  setup=(IWakeable)newObj;
					        if (IsPluginNameEnabled(setup.PluginName()))
					        {
						        _Wakeables.Add(setup);
					        }
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
				Log.Write("ex:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
      }
    }


    static public bool IsPlugInEnabled(string strDllname)
    {
      if (strDllname.IndexOf("WindowPlugins.dll")>=0) return true;

      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
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

      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        bool bEnabled=xmlreader.GetValueAsBool("pluginswindows",strType,true);
        return bEnabled;
        
      } 
    }
    static public bool IsPluginNameEnabled(string strPluginName)
    {

      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        bool bEnabled=xmlreader.GetValueAsBool("plugins",strPluginName,true);
        return bEnabled;
        
      } 
    }
	}
}
