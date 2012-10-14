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
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.Base
{
  public class PluginLoader
  {
    private List<ITvServerPlugin> _plugins = new List<ITvServerPlugin>();
    private readonly List<Type> _incompatiblePlugins = new List<Type>();

    /// <summary>
    /// returns a list of all plugins loaded.
    /// </summary>
    /// <value>The plugins.</value>
    public List<ITvServerPlugin> Plugins
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
       /*   
       var container = new WindsorContainer();
       container.Register(Component.For<IService>().ImplementedBy<Service>()
       */
                 
      _plugins.Clear();
      _incompatiblePlugins.Clear();

      try
      {
        var container = new WindsorContainer(new XmlInterpreter());        
        var assemblyFilter = new AssemblyFilter("plugins");                
        container.Register(
        AllTypes.FromAssemblyInDirectory(assemblyFilter).                        
            BasedOn<ITvServerPlugin>().
            If(t => IsPluginCompatible(t)).            
            WithServiceBase().            
            LifestyleSingleton()
            );

        _plugins = new List<ITvServerPlugin>(container.ResolveAll<ITvServerPlugin>());

        foreach (ITvServerPlugin plugin in _plugins)
        {
          Log.WriteFile("PluginManager: Loaded {0} version:{1} author:{2}", plugin.Name, plugin.Version,
                        plugin.Author);
        }      
      }
      catch (Exception ex)
      {
        Log.WriteFile("PluginManager: Error while loading dll's.", ex);
      }
    }

    private bool IsPluginCompatible(Type type)
    {
      bool isPluginCompatible = CompatibilityManager.IsPluginCompatible(type);
      if (!isPluginCompatible)
      {
        _incompatiblePlugins.Add(type);
        Log.WriteFile("PluginManager: {0} is incompatible with the current tvserver version and won't be loaded!", type.FullName);
      }
      return isPluginCompatible;
    }    
  }
}