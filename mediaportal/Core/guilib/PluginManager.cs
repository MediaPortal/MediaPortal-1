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
		static bool _Started=false;
    
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

    static public void Load()
    {
      System.IO.Directory.CreateDirectory(@"plugins");
      System.IO.Directory.CreateDirectory(@"plugins\process");
      string[] strFiles=System.IO.Directory.GetFiles(@"plugins\process", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
    }
    static public void LoadWindowPlugins()
    {
      System.IO.Directory.CreateDirectory(@"plugins");
      System.IO.Directory.CreateDirectory(@"plugins\windows");
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
      foreach (IPlugin plugin in _NonGUIPlugins)
      {
        plugin.Stop();
      }
    }
    static public void Clear()
    {
      PluginManager.Stop();
      _NonGUIPlugins.Clear();
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
                  _SetupForms.Add(setup);
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
                if (t.IsSubclassOf (typeof(GUIWindow)))
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  GUIWindow win=(GUIWindow)newObj;
                  win.Init();
                  GUIWindowManager.Add(ref win);
                  //Log.Write("  load plugin:{0} in {1}",t.ToString(), strFile);
                }
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                Type[] foundInterfaces=t.FindInterfaces(myFilter2,"MediaPortal.GUI.Library.ISetupForm");
                if (foundInterfaces.Length>0)
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  ISetupForm  setup=(ISetupForm)newObj;
                  _SetupForms.Add(setup);
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


    static public bool IsPlugInEnabled(string strDllname)
    {
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
	}
}
