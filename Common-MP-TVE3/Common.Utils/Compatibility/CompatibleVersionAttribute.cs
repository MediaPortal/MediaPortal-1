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

namespace MediaPortal.Common.Utils
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
  public class CompatibleVersionAttribute : Attribute
  {
    public static readonly Version OwnAssemblyVersion = new Version("0.0.0.0");
    private readonly Version _designedForVersion;
    private readonly Version _minRequiredVersion;


    private static Version ParseVersion(string version)
    {
      return version.Equals("own", StringComparison.InvariantCultureIgnoreCase) ? OwnAssemblyVersion : new Version(version);
    }

    public CompatibleVersionAttribute(Version designedForVersion, Version minRequiredVersion)
    {
      _designedForVersion = designedForVersion;
      _minRequiredVersion = minRequiredVersion;
    }

    public CompatibleVersionAttribute(Version designedForVersion)
      : this(designedForVersion, designedForVersion)
    {
    }

    public CompatibleVersionAttribute()
      : this(OwnAssemblyVersion, OwnAssemblyVersion)
    {
    }

    public CompatibleVersionAttribute(string designedForVersion, string minRequiredVersion)
      : this(ParseVersion(designedForVersion), ParseVersion(minRequiredVersion))
    {
    }

    public CompatibleVersionAttribute(string designedForVersion)
      : this(ParseVersion(designedForVersion))
    {
    }

    public Version DesignedForVersion
    {
      get { return _designedForVersion; }
    }

    public Version MinRequiredVersion
    {
      get { return _minRequiredVersion; }
    }
  }
}
