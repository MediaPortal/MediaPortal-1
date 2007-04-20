using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Plugins
{
  /// <summary>
  /// A <see cref="IPluginManager"/> implementation that uses reflection to determine
  /// what plug-ins are available
  /// </summary>
  public class ReflectionPluginManager //: IPluginManager
  {
    private IDictionary<string, Type> pluginTypes = new Dictionary<string, Type>();
    private IDictionary<string, IPluginInfo> pluginInfo = new Dictionary<string, IPluginInfo>();
    protected IDictionary<string, IPlugin> runningPlugins = new Dictionary<string, IPlugin>();

    public ReflectionPluginManager()
    {
      ServiceScope.Get<IMessageBroker>().Register(this);
      LoadPlugins();
    }

    #region IPluginManager Members

    public event EventHandler<PluginStartStopEventArgs> PluginStarted;
    public event EventHandler<PluginStartStopEventArgs> PluginStopped;

    public List<T> BuildItems<T>(string treePath)
    {
      //required for the tree PluginManager
      return null;
    }

    /// <summary>
    /// Gets an enumerable list of available plugins
    /// </summary>
    /// <returns>An <see cref="IEnumerable<IPlugin>"/> list.</returns>
    /// <remarks>A configuration program can use this list to present the user a list of available plugins that he can (de)activate.</remarks>
    public IEnumerable<IPluginInfo> GetAvailablePlugins()
    {
      return pluginInfo.Values;
    }

    /// <summary>
    /// Stops the given plug-in.
    /// </summary>
    /// <param name="pluginName">name of the plug-in to stop</param>
    public void Stop(string pluginName)
    {
      if (!runningPlugins.ContainsKey(pluginName))
      {
        return; //Plugin was not running
      }
      IPlugin plugin = runningPlugins[pluginName];
      runningPlugins.Remove(pluginName);
      plugin.Dispose();
      OnPluginStopped(new PluginStartStopEventArgs(pluginName));
    }

    /// <summary>
    /// Stops all plug-ins
    /// </summary>
    public void StopAll()
    {
      foreach (IPlugin plugin in runningPlugins.Values)
      {
        plugin.Dispose();
      }
      runningPlugins.Clear();
    }

    /// <summary>
    /// Starts the given plug-in.
    /// </summary>
    /// <param name="pluginName">name of the plug-in to start</param>
    public void Start(string pluginName)
    {
      //TODO: add support for starting the same plug-in more than once.
      if (!pluginTypes.ContainsKey(pluginName))
      {
        throw new ArgumentException(string.Format("{0} Plug-in does not exist", pluginName), "pluginName");
      }
      IPlugin plugin = (IPlugin) Activator.CreateInstance(pluginTypes[pluginName]);
      plugin.Initialize("");
      if (!runningPlugins.ContainsKey(pluginName))
      {
        runningPlugins.Add(pluginName, plugin);
      }
      OnPluginStarted(new PluginStartStopEventArgs(pluginName));
    }


    /// <summary>
    /// Starts all plug-ins that are activated by the user.
    /// </summary>
    public void StartAll()
    {
      foreach (IPluginInfo info in GetAvailablePlugins())
      {
        if (info.AutoStart)
          Start(info.Name);
      }
    }

    #endregion

    /// <summary>
    /// Triggers the PluginStarted message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPluginStarted(PluginStartStopEventArgs e)
    {
      if (PluginStarted != null)
      {
        PluginStarted(this, e);
      }
    }

    /// <summary>
    /// Triggers the PluginStopped message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPluginStopped(PluginStartStopEventArgs e)
    {
      if (PluginStopped != null)
      {
        PluginStopped(this, e);
      }
    }


    /// <summary>
    /// Loads all the available plugins
    /// </summary>
    /// <remarks>
    /// Note that by using an attribute to hold the plugin name and description, we 
    /// do not need to actually start the plugin to get it's name and description
    /// </remarks>
    private void LoadPlugins()
    {
      //Lookup all DLLs in the current folder
      DirectoryInfo dir = new DirectoryInfo(".");
      FileInfo[] files = dir.GetFiles("*.dll");
      foreach (FileInfo file in files)
      {
        //Load the DLL
        Assembly ass = Assembly.LoadFrom(file.Name);
        //Loop through all types
        foreach (Type type in ass.GetTypes())
        {
          //if the type does not implement IPlugin, skip it
          if (type.GetInterface("IPlugin") == null)
          {
            continue;
          }
          PluginAttribute[] attributes = (PluginAttribute[])type.GetCustomAttributes(typeof(PluginAttribute), false);
          if (attributes.Length == 0)
          {
            continue;
          }
          PluginAttribute attribute = attributes[0];
          pluginTypes.Add(attribute.Name, type);
          pluginInfo.Add(attribute.Name, new PluginInfo(attribute));
        }
      }
    }
  }
}