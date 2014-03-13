#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Reflection;
using System.Xml;

namespace MediaPortal.Common.Utils
{
  public class CompatibilityManager
  {
    protected class UsesSubsystemAttributeComparer : IEqualityComparer<UsesSubsystemAttribute>
    {
      public bool Equals(UsesSubsystemAttribute x, UsesSubsystemAttribute y)
      {
        return x.Subsystem == y.Subsystem;
      }

      public int GetHashCode(UsesSubsystemAttribute obj)
      {
        return obj.Subsystem.GetHashCode();
      }
    }

    private static readonly HashSet<Assembly> AppAssemblies = new HashSet<Assembly>();
    private static readonly Dictionary<string, Version> SubSystemVersions = new Dictionary<string, Version>();
    private static readonly Version AppVersion;
    public static readonly Version SkinVersion = new Version(1, 4, 0, 0);
    private static readonly string MinRequiredVersionDefault = "1.1.8.0"; // 1.2.0 RC1

    static CompatibilityManager()
    {
      AppVersion = Assembly.GetEntryAssembly().GetName().Version;
    }

    /// <summary>
    /// Enumerate loaded assemblies and scan each of the, for subsystem compatibility attributes
    /// </summary>
    /// <remarks>
    /// This method can be called repeatedly. Each time it will 
    /// detect any newly loaded assemblies and scan those for 
    /// additional compatibility attributes
    /// </remarks>
    private static void CheckLoadedAssemblies()
    {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      if (assemblies.Length >= AppAssemblies.Count)
      {
        foreach (Assembly asm in assemblies)
        {
          if (AppAssemblies.Add(asm))
          {
            ScanAssembly(asm);
          }
        }
      }
    }

    /// <summary>
    /// Sets a subsystem's version, but only if there is no version information 
    /// about the subsystem or new version is later than the existing one.
    /// </summary>
    /// <param name="subSystem">The subsystem whose version to set</param>
    /// <param name="version">The new version</param>
    /// <returns>True if the version was set/changed</returns>
    private static bool SetSubSystemVersion(string subSystem, Version version)
    {
      Version oldVersion;
      if (!SubSystemVersions.TryGetValue(subSystem, out oldVersion) || CompareVersions(oldVersion, version) < 0)
      {
        SubSystemVersions[subSystem] = version;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Scan a loaded assembly for subsystem compatibility attributes.
    /// </summary>
    /// <param name="asm">The assembly to scan</param>
    private static void ScanAssembly(Assembly asm)
    {
      var mpAttributes =
        asm.GetCustomAttributes(typeof(SubsystemVersionAttribute), false).Cast<SubsystemVersionAttribute>();

      foreach (SubsystemVersionAttribute attr in mpAttributes)
      {
        string subSystem = attr.Subsystem;
        while(!string.IsNullOrEmpty(subSystem))
        {
          if (!subSystem.EndsWith(".")) // ignore subsystems ending in dot, next iteration will pick up subsystem without the dot
          {
            SetSubSystemVersion(subSystem, attr.Version);
          }
          int pos = subSystem.LastIndexOf('.');
          if (pos<0)
          {
            pos = 0;
          }
          subSystem = subSystem.Substring(0, pos);
        }
      }
    }

    /// <summary>
    /// Compare two versions.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    /// <remarks>
    /// The comparison done by this function differs from that of 
    /// the relational operators in Version class, in that any version with a 
    /// zero Build number is considered a release version and compares greater than
    /// any other version with the same Major,Minor and Revision numbers 
    /// but non-zero Build number.
    /// </remarks>
    public static int CompareVersions(Version v1, Version v2)
    {
      //if (v1.Major == v2.Major && v1.Minor ==v2.Minor && v1.Revision == v2.Revision)
      //{
      //  if (v1.Build == v2.Build)
      //  {
      //    return 0;
      //  }
      //  if (v1.Build == 0) // release is always higher than any SVN build
      //  {
      //    return 1;
      //  }
      //  if (v2.Build == 0) // release is always higher than any SVN build
      //  {
      //    return -1;
      //  }
      //  if (v1.Build < v2.Build)
      //  {
      //    return -1;
      //  }
      //  return 1;
      //}

      return v1.CompareTo(v2);
    }

    /// <summary>
    /// Verify a plugin compatibility based on the subsystems used
    /// and the MP version it was built against.
    /// </summary>
    /// <param name="plugin">The plugin to check</param>
    /// <returns>True if the plugin is compatible with this version of the application</returns>
    /// <remarks>
    /// A plugin is compatible with this version of the application if
    /// none of the subsystems it uses had a breaking change in a version
    /// after the one the plugin was build against and the minimum required version
    /// is older than the current application version.
    /// A special subsystem "*" is used to signify that compatibility with all plugins
    /// has been broken.
    /// 
    /// The plugin is any type that is decorated using the UsesSubsystemAttribute 
    /// and CompatibleVersionAttribute. If an attribute is not specified on the plugin,
    /// its defining assembly is searched for the same attribute.
    /// </remarks>
    public static bool IsPluginCompatible(Type plugin)
    {
      var mpVersions =
        (CompatibleVersionAttribute[])plugin.GetCustomAttributes(typeof(CompatibleVersionAttribute), true);
      if (mpVersions.Length == 0)
      {
        mpVersions = (CompatibleVersionAttribute[])plugin.Assembly.GetCustomAttributes(typeof(CompatibleVersionAttribute), true);
      }
      var minRequiredVersion = new Version(MinRequiredVersionDefault);
      var designedForVersion = new Version(1, 0, 0, 0);

      if (mpVersions.Length > 0)
      {
        minRequiredVersion = mpVersions[0].MinRequiredVersion;
        if (minRequiredVersion == CompatibleVersionAttribute.OwnAssemblyVersion)
        {
          minRequiredVersion = plugin.Assembly.GetName().Version;
        }
        designedForVersion = mpVersions[0].DesignedForVersion;
        if (designedForVersion == CompatibleVersionAttribute.OwnAssemblyVersion)
        {
          designedForVersion = plugin.Assembly.GetName().Version;
        }
      }

      CheckLoadedAssemblies();
      Version lastFullyBreakingVersion;

      if (CompareVersions(AppVersion, minRequiredVersion) < 0 ||                 // MP version is too old
          (SubSystemVersions.TryGetValue("*", out lastFullyBreakingVersion) &&   
            CompareVersions(lastFullyBreakingVersion, designedForVersion) > 0))  // MP breaking version after plugin released
      {
        return false;
      }

      IEnumerable<UsesSubsystemAttribute> subsystemsUsed = (UsesSubsystemAttribute[])plugin.GetCustomAttributes(typeof(UsesSubsystemAttribute), true);
      subsystemsUsed = subsystemsUsed.Union((UsesSubsystemAttribute[])plugin.Assembly.GetCustomAttributes(typeof(UsesSubsystemAttribute), true),
                                            new UsesSubsystemAttributeComparer()).Where(attr => attr.Used);

      Version ver;
      // Have all used subsystem known versions and prior to the one the plugin was designed for?
      return subsystemsUsed.All(attr => SubSystemVersions.TryGetValue(attr.Subsystem, out ver) && CompareVersions(ver, designedForVersion) <= 0);
    }

    /// <summary>
    /// Verify a plugin assembly compatibility based on the subsystems used
    /// and the MP version it was built against.
    /// </summary>
    /// <param name="plugin">The plugin assemby to check</param>
    /// <returns>True if the plugin is compatible with this version of the application</returns>
    /// <remarks>
    /// A plugin assembly is compatible with this version of the application if
    /// none of the subsystems it uses had a breaking change in a version
    /// after the one the plugin was build against and the minimum required version
    /// is older than the current application version.
    /// A special subsystem "*" is used to signify that compatibility with all plugins
    /// has been broken.
    /// 
    /// The plugin is any assembly that is decorated using the UsesSubsystemAttribute 
    /// and CompatibleVersionAttribute.
    /// </remarks>
    public static bool IsPluginCompatible(Assembly plugin)
    {
      var mpVersions =
        (CompatibleVersionAttribute[])plugin.GetCustomAttributes(typeof(CompatibleVersionAttribute), true);

      var minRequiredVersion = new Version(MinRequiredVersionDefault);
      var designedForVersion = new Version(1, 0, 0, 0);

      if (mpVersions.Length > 0)
      {
        minRequiredVersion = mpVersions[0].MinRequiredVersion;
        if (minRequiredVersion == CompatibleVersionAttribute.OwnAssemblyVersion)
        {
          minRequiredVersion = plugin.GetName().Version;
        }
        designedForVersion = mpVersions[0].DesignedForVersion;
        if (designedForVersion == CompatibleVersionAttribute.OwnAssemblyVersion)
        {
          designedForVersion = plugin.GetName().Version;
        }
      }

      CheckLoadedAssemblies();
      Version lastFullyBreakingVersion;

      if (CompareVersions(AppVersion, minRequiredVersion) < 0 ||                 // MP version is too old
          (SubSystemVersions.TryGetValue("*", out lastFullyBreakingVersion) &&
            CompareVersions(lastFullyBreakingVersion, designedForVersion) > 0))  // MP breaking version after plugin released
      {
        return false;
      }

      IEnumerable<UsesSubsystemAttribute> subsystemsUsed = ((UsesSubsystemAttribute[])plugin.GetCustomAttributes(typeof(UsesSubsystemAttribute), true))
                                                           .Where(attr => attr.Used);

      Version ver;
      // Have all used subsystem known versions and prior to the one the plugin was designed for?
      return subsystemsUsed.All(attr => SubSystemVersions.TryGetValue(attr.Subsystem, out ver) && CompareVersions(ver, designedForVersion) <= 0);
    }

    public static Version GetCurrentVersion()
    {
      CheckLoadedAssemblies();
      Version version;
      if (!SubSystemVersions.TryGetValue("*", out version))
      {
        return AppVersion;
      }
      return version;
    }

    public static Version GetCurrentMaxVersion()
    {
      CheckLoadedAssemblies();
      return SubSystemVersions.Max(v => v.Value);
    }

    public static Version GetCurrentSubSystemVersion(string subSystem)
    {
      Version version = null;
      SubSystemVersions.TryGetValue(subSystem, out version);
      return version;
    }

    public static IEnumerable<UsesSubsystemAttribute> GetSubSystemsUsed(Assembly asm)
    {
      return ((UsesSubsystemAttribute[])asm.GetCustomAttributes(typeof(UsesSubsystemAttribute), true)).Where(attr => attr.Used);
    }

    public static IEnumerable<CompatibleVersionAttribute> GetRequestedVersions(Assembly asm)
    {
      return (CompatibleVersionAttribute[])asm.GetCustomAttributes(typeof(CompatibleVersionAttribute), true);
    }

    public static bool IsPluginCompatible(System.Xml.XmlElement rootNode)
    {
      XmlNode versionNode = rootNode.SelectSingleNode("CompatibleVersion/Items");
      if(versionNode == null)
      {
        return false;
      }
      var minRequiredVersion = new Version(MinRequiredVersionDefault);
      var designedForVersion = new Version(1, 0, 0, 0);

      foreach (XmlNode node in versionNode.ChildNodes)
      {
        XmlNode minVersionNode = node.SelectSingleNode("MinRequiredVersion");
        XmlNode designedVersionNode = node.SelectSingleNode("DesignedForVersion");
        if (minVersionNode != null)
        {
          minRequiredVersion = new Version(minVersionNode.InnerText);
        }
        if (designedForVersion == null)
        {
          return false;
        }
        designedForVersion = new Version(designedVersionNode.InnerText);
        break; //Break cause we only check first instance??
      }

      CheckLoadedAssemblies();
      Version lastFullyBreakingVersion;

      if (CompareVersions(AppVersion, minRequiredVersion) < 0 ||                 // MP version is too old
          (SubSystemVersions.TryGetValue("*", out lastFullyBreakingVersion) &&
            CompareVersions(lastFullyBreakingVersion, designedForVersion) > 0))  // MP breaking version after plugin released
      {
        return false;
      }

      List<string> subsystemsUsed = new List<string>();
      XmlNode subsystemNode = rootNode.SelectSingleNode("SubSystemsUsed/Items");
      if (subsystemNode == null)
      {
        return false;
      }
      foreach (XmlNode node in subsystemNode.ChildNodes)
      {
        XmlAttribute nameAttrib = node.Attributes["Name"];
        if(nameAttrib == null || string.IsNullOrEmpty(nameAttrib.InnerText))
        {
          continue;
        }
        subsystemsUsed.Add(nameAttrib.InnerText);
      }

      if (subsystemsUsed.Count == 0)
      {
        return true;
      }

      Version ver;
      // Have all used subsystem known versions and prior to the one the plugin was designed for?
      return subsystemsUsed.All(attr => SubSystemVersions.TryGetValue(attr, out ver) && CompareVersions(ver, designedForVersion) <= 0);
    }

    static readonly Dictionary<Version, string> MpReleaseApi = new Dictionary<Version, string>()
    {
      { new Version("1.1.6.27644"), "1.2.0 Beta" },
      { new Version("1.2.100.0"), "1.3.0 Alpha" },
      { new Version("1.3.100.0"), "1.4.0 Pre Release" },
      { new Version("1.4.100.0"), "1.5.0 Pre Release" },
      { new Version("1.5.100.0"), "1.6.0 Pre Release" },
      { new Version("1.6.100.0"), "1.7.0 Pre Release" }
    };

    public static string MediaPortalReleaseForApiVersion(Version apiVersion)
    {
      return MpReleaseApi.First(v => v.Key >= apiVersion).Value;
    }
  }
}
