using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  internal class CustomDeviceLoader
  {
    private List<ICustomDevice> _plugins = new List<ICustomDevice>();
    private readonly List<Type> _incompatiblePlugins = new List<Type>();
    private readonly string _customDevicesFolder;

    public CustomDeviceLoader()
    {      
      _customDevicesFolder = PathManager.BuildAssemblyRelativePath("plugins\\CustomDevices");
      if (!Directory.Exists(_customDevicesFolder))
      {
        this.LogDebug("CustomDeviceLoader: plugin directory doesn't exist or is not accessible");
        _customDevicesFolder = null;
      }
    }

    /// <summary>
    /// returns a list of all plugins loaded.
    /// </summary>
    /// <value>The plugins.</value>
    public List<ICustomDevice> Plugins
    {
      get { return _plugins; }
    }

    /// <summary>
    /// returns a list of plugins not loaded as incompatible.
    /// </summary>
    /// <value>The plugins.</value>
    public List<Type> IncompatiblePlugins
    {
      get { return _incompatiblePlugins; }
    }

    /// <summary>
    /// Loads all plugins.
    /// </summary>
    public virtual void Load()
    {       
      if (_plugins.Count > 0)
      {
        this.LogDebug("custom devices already loaded.");
        return;
      }

      if (String.IsNullOrWhiteSpace(_customDevicesFolder))
      {
        this.LogDebug("customDevicesFolder path is null or empty.");
        return;
      }

      _plugins.Clear();
      _incompatiblePlugins.Clear();

      try
      {
        
        var assemblyFilter = new AssemblyFilter(_customDevicesFolder);
      GlobalServiceProvider.Instance.Get<IWindsorContainer>().Register(
      AllTypes.FromAssemblyInDirectory(assemblyFilter).
          BasedOn<ICustomDevice>().
          If(t => IsPluginCompatible(t)).
          WithServiceBase().
          LifestyleTransient()
          );

        _plugins = new List<ICustomDevice>(GlobalServiceProvider.Instance.Get<IWindsorContainer>().ResolveAll<ICustomDevice>());            

         // There is a well defined loading/checking order for plugins: add-ons, priority, name.
        _plugins.Sort(
        delegate(ICustomDevice cd1, ICustomDevice cd2)
        {
          bool cd1IsAddOn = cd1 is IAddOnDevice;
          bool cd2IsAddOn = cd2 is IAddOnDevice;
          if (cd1IsAddOn && !cd2IsAddOn)
          {
            return -1;
          }
          if (cd2IsAddOn && !cd1IsAddOn)
          {
            return 1;
          }
          int priorityCompare = cd2.Priority.CompareTo(cd1.Priority);
          if (priorityCompare != 0)
          {
            return priorityCompare;
          }
          return cd1.Name.CompareTo(cd2.Name);
        }
      );

      // Log the name, priority and capabilities for each plugin, in priority order.
      foreach (ICustomDevice d in _plugins)
      {
        Type[] interfaces = d.GetType().GetInterfaces();
        String[] interfaceNames = new String[interfaces.Length];
        for (int i = 0; i < interfaces.Length; i++)
        {
          interfaceNames[i] = interfaces[i].Name;
        }
        Array.Sort(interfaceNames);
        this.LogDebug("  {0} [{1} - {2}]: {3}", d.Name, d.Priority, d.GetType().Name, String.Join(", ", interfaceNames));
      }   
      }
      catch (Exception ex)
      {
        this.LogDebug("CustomDeviceLoader: Error while loading dll's.", ex);
      }
    }

    private bool IsPluginCompatible(Type type)
    {
      bool isPluginCompatible = CompatibilityManager.IsPluginCompatible(type);
      if (!isPluginCompatible)
      {
        _incompatiblePlugins.Add(type);
        this.LogDebug("CustomDeviceLoader: skipping incompatible plugin \"{0}\" ({1})", type.Name, type.Assembly.FullName);
      }      
      return isPluginCompatible;
    }      
  }
}
