using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TvEngine;
using TvLibrary.Log;
namespace SetupTv
{
  class PluginLoader
  {
    List<PluginBase> _plugins = new List<PluginBase>();

    /// <summary>
    /// returns a list of all plugins loaded.
    /// </summary>
    /// <value>The plugins.</value>
    public List<PluginBase> Plugins
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
      }
    }
    /// <summary>
    /// Loads the plugin.
    /// </summary>
    /// <param name="strFile">The STR file.</param>
    void LoadPlugin(string strFile)
    {
      Type[] foundInterfaces = null;

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
                if (t.IsAbstract) continue;

                Object newObj = null;
                PluginBase plugin = null;
                TypeFilter myFilter2 = new TypeFilter(MyInterfaceFilter);
                try
                {
                  foundInterfaces = t.FindInterfaces(myFilter2, "TvEngine.PluginBase");
                  if (foundInterfaces.Length > 0)
                  {
                    newObj = (object)Activator.CreateInstance(t);
                    plugin = (PluginBase)newObj;
                    _plugins.Add(plugin);
                  }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                  Log.WriteFile("PluginManager: {0} is incompatible with the current tvserver version and won't be loaded!", t.FullName);
                  continue;
                }
                catch (Exception iPluginException)
                {
                  Log.WriteFile("Exception while loading IPlugin instances: {0}", t.FullName);
                  Log.WriteFile(iPluginException.ToString());
                  Log.WriteFile(iPluginException.Message);
                  Log.WriteFile(iPluginException.StackTrace);
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
        Log.WriteFile("PluginManager: Plugin file {0} is broken or incompatible with the current tvserver version and won't be loaded!", strFile.Substring(strFile.LastIndexOf(@"\") + 1));
        Log.WriteFile("PluginManager: Exception: {0}", ex);
      }
    }

    bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }

  }
}
