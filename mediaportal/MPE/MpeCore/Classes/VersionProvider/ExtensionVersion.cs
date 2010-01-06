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

using Microsoft.Win32;
using MpeCore;
using MpeCore.Interfaces;

namespace MpeCore.Classes.VersionProvider
{
  public class ExtensionVersion : IVersionProvider
  {
    public string DisplayName
    {
      get { return "Extension"; }
    }

    public bool Validate(DependencyItem componentItem)
    {
      if (componentItem.MinVersion.CompareTo(Version(componentItem.Id)) >= 0 &&
          componentItem.MaxVersion.CompareTo(Version(componentItem.Id)) <= 0)
        return true;
      return false;
    }

    public VersionInfo Version(string id)
    {
      PackageClass pak = MpeInstaller.InstalledExtensions.Get(id);
      if (pak != null)
        return pak.GeneralInfo.Version;
      return new VersionInfo();
    }
  }
}