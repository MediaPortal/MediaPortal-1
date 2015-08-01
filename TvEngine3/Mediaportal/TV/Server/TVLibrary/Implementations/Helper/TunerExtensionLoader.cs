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
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  internal class TunerExtensionLoader
  {
    private List<ITunerExtension> _extensions = new List<ITunerExtension>();
    private List<Type> _incompatibleExtensions = new List<Type>();
    private string _extensionDirectory = null;

    public TunerExtensionLoader()
    {
      _extensionDirectory = PathManager.BuildAssemblyRelativePath("plugins\\TunerExtensions");
      if (!Directory.Exists(_extensionDirectory))
      {
        this.LogError("tuner extension loader: directory doesn't exist or is not accessible");
        _extensionDirectory = null;
      }
    }

    /// <summary>
    /// Get the set of extensions which were successfully loaded.
    /// </summary>
    public IList<ITunerExtension> Extensions
    {
      get
      {
        return _extensions;
      }
    }

    /// <summary>
    /// Get the set of extensions which were not loaded because they are not compatible.
    /// </summary>
    public IList<Type> IncompatibleExtensions
    {
      get
      {
        return _incompatibleExtensions;
      }
    }

    /// <summary>
    /// Load all extensions.
    /// </summary>
    public IList<ITunerExtension> Load(ITuner tuner, object loadContext)
    {
      this.LogDebug("tuner extension loader: load extensions");
      if (_extensions.Count > 0 || _incompatibleExtensions.Count > 0)
      {
        this.LogWarn("tuner extension loader: extensions already loaded");
        return _extensions;
      }
      if (string.IsNullOrWhiteSpace(_extensionDirectory))
      {
        this.LogError("tuner extension loader: extension directory not set");
        return _extensions;
      }

      _extensions.Clear();
      _incompatibleExtensions.Clear();

      try
      {
        var assemblyFilter = new AssemblyFilter(_extensionDirectory);
        GlobalServiceProvider.Instance.Get<IWindsorContainer>().Register(
          Classes.FromAssemblyInDirectory(assemblyFilter).
            BasedOn<ITunerExtension>().
            If(IsExtensionCompatible).
            WithServiceBase().
            LifestyleTransient()
        );

        List<ITunerExtension> extensions = new List<ITunerExtension>(GlobalServiceProvider.Instance.Get<IWindsorContainer>().ResolveAll<ITunerExtension>());
        ITunerExtension x = tuner as ITunerExtension;
        if (x != null)
        {
          this.LogDebug("tuner extension loader: add tuner instance as extension");
          extensions.Add(x);
        }

        // There is a well defined loading/checking order for extensions: priority, name.
        extensions.Sort(
          delegate(ITunerExtension e1, ITunerExtension e2)
          {
            int priorityCompare = e2.Priority.CompareTo(e1.Priority);
            if (priorityCompare != 0)
            {
              return priorityCompare;
            }
            return e1.Name.CompareTo(e2.Name);
          }
        );

        // Log the name, priority and capabilities for each extension.
        foreach (ITunerExtension e in extensions)
        {
          Type[] interfaces = e.GetType().GetInterfaces();
          string[] interfaceNames = new string[interfaces.Length];
          for (int i = 0; i < interfaces.Length; i++)
          {
            interfaceNames[i] = interfaces[i].Name;
          }
          Array.Sort(interfaceNames);
          this.LogDebug("  {0} [{1} - {2}]: {3}", e.Name, e.Priority, e.GetType().Name, string.Join(", ", interfaceNames));
        }

        this.LogDebug("tuner extension loader: checking for supported extensions");
        HashSet<Type> foundInterfaces = new HashSet<Type>();
        foreach (ITunerExtension e in extensions)
        {
          // We only support one implementation of each interface for features
          // that are tied to hardware.
          Type[] interfaces = new Type[0];
          if (e.ControlsTunerHardware)
          {
            bool foundInterface = false;
            interfaces = e.GetType().GetInterfaces();
            foreach (Type i in interfaces)
            {
              // TODO can we avoid using an explicit list of interfaces?
              // TODO this will not support Hauppauge ECP for hardware that doesn't actually have an encoder + any other encoder
              if (
                foundInterfaces.Contains(i) &&
                (
                  i == typeof(IConditionalAccessProvider) ||
                  i == typeof(ICustomTuner) ||
                  i == typeof(IDiseqcDevice) ||
                  i == typeof(IEncoder) ||
                  i == typeof(IMpeg2PidFilter) ||
                  i == typeof(IPowerDevice) ||
                  i == typeof(IRemoteControlListener) ||
                  i == typeof(IStreamSelector)
                )
              )
              {
                this.LogDebug("tuner extension loader: extension \"{0}\" supports already found interface {1}, won't use", e.Name, i.Name);
                foundInterface = true;
                break;
              }
            }
            if (foundInterface)
            {
              continue;
            }
          }

          this.LogDebug("tuner extension loader: try extension \"{0}\"", e.Name);
          if (!e.Initialise(tuner.ExternalId, tuner.SupportedBroadcastStandards, loadContext))
          {
            IDisposable d = e as IDisposable;
            if (d != null)
            {
              d.Dispose();
            }
            continue;
          }

          _extensions.Add(e);
          foundInterfaces.UnionWith(interfaces);
        }

        this.LogDebug("tuner extension loader: {0} extension(s) supported", _extensions.Count);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner extension loader: failed to load extensions");
      }
      return _extensions;
    }

    private bool IsExtensionCompatible(Type type)
    {
      bool isExtensionCompatible = CompatibilityManager.IsPluginCompatible(type, true);
      if (!isExtensionCompatible)
      {
        _incompatibleExtensions.Add(type);
        this.LogDebug("tuner extension loader: skipping incompatible extension \"{0}\" ({1})", type.Name, type.Assembly.FullName);
      }
      return isExtensionCompatible;
    }
  }
}