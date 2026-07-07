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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;

using MediaPortal.Common.Utils;

namespace MpeCore.Classes
{
  public class PluginDependencyItem
  {
    [XmlAttribute]
    public string AssemblyName { get; set; }

    public CompatibleVersionCollection  CompatibleVersion { get; set; }

    public SubSystemItemCollection SubSystemsUsed { get; set; }

    public PluginDependencyItem()
    {
      AssemblyName = string.Empty;
      CompatibleVersion = new CompatibleVersionCollection();
      SubSystemsUsed = new SubSystemItemCollection();
    }

    internal bool ScanVersionInfo(string asm)
    {
      Assembly plugin = null;
      try
      {
        plugin = Assembly.LoadFrom(asm);
      }
      catch {}

      if (plugin == null)
      {
        return false;
      }

      CompatibleVersionAttribute[] versions = (CompatibleVersionAttribute[])CompatibilityManager.GetRequestedVersions(plugin);
      foreach (CompatibleVersionAttribute attr in versions)
      {
        CompatibleVersionItem compatibleVersionItem = new CompatibleVersionItem();
        compatibleVersionItem.MinRequiredVersion = attr.MinRequiredVersion.ToString();
        compatibleVersionItem.DesignedForVersion = attr.DesignedForVersion.ToString();
        CompatibleVersion.Add(compatibleVersionItem);
      }

      IEnumerable<UsesSubsystemAttribute> subSystems = (IEnumerable<UsesSubsystemAttribute>)CompatibilityManager.GetSubSystemsUsed(plugin);
      foreach (UsesSubsystemAttribute attr in subSystems)
      {
        SubSystemItem subsystemItem = new SubSystemItem();
        subsystemItem.Name = attr.Subsystem;
        //subsystemItem.Version = attr.Version.ToString();
        SubSystemsUsed.Add(subsystemItem);
      }
      SubSystemsUsed.Sort();
      return true;
    }
  }
}
