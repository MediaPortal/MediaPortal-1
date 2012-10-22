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

using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Integration
{
  /// <summary>
  /// Helper class for loading the <see cref="IIntegrationProvider"/> from an external assembly using a naming pattern: <c>"*.Integration.*.dll"</c>
  /// </summary>
  public static class IntegrationProviderHelper
  {
    /// <summary>
    /// Finds an assembly with matching type <see cref="IIntegrationProvider"/> and adds it into <seealso cref="GlobalServiceProvider"/>.
    /// </summary>
    public static void Register()
    {
      var container = new WindsorContainer(new XmlInterpreter());
      var assemblyFilter = new AssemblyFilter(".", "*.Integration.*.dll");
      container.Register(AllTypes.FromAssemblyInDirectory(assemblyFilter).BasedOn<IIntegrationProvider>().WithServiceBase().LifestyleSingleton());
      GlobalServiceProvider.Add(container.Resolve<IIntegrationProvider>());
    }
  }
}
