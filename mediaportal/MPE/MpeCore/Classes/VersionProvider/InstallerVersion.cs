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
using System.Reflection;

using MpeCore.Interfaces;

namespace MpeCore.Classes.VersionProvider
{
  public class InstallerVersion : IVersionProvider
  {
    public string DisplayName
    {
      get { return "Installer"; }
    }

    public bool Validate(DependencyItem componentItem)
    {
      if (Version(componentItem.Id).CompareTo(componentItem.MinVersion) >= 0 &&
          Version(componentItem.Id).CompareTo(componentItem.MaxVersion) <= 0)
        return true;
      return false;
    }

    public VersionInfo Version(string id)
    {
      string file = MpeInstaller.TransformInRealPath("%Base%") + @"\MpeCore.dll";
      try
      {
        var ver = AssemblyName.GetAssemblyName(file).Version;
        return new VersionInfo()
                 {
                   Build = ver.Build.ToString(),
                   Major = ver.Major.ToString(),
                   Minor = ver.Minor.ToString(),
                   Revision = ver.Revision.ToString()
                 };
      }
      catch (Exception)
      {
        return new VersionInfo();
      }
    }
  }
}
