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
using System.Text;
using MpeCore.Classes;

namespace MpeCore.Interfaces
{
  public interface IInstallerTypeProvider
  {
    string Name { get; }
    string Description { get; }
    void Install(PackageClass packageClass, FileItem fileItem);
    void Uninstall(PackageClass packageClass, UnInstallItem fileItem);
    string GetZipEntry(FileItem fileItem);

    /// <summary>
    /// Transform real path in a templated path based on PathProviders
    /// </summary>
    /// <param name="fileItem">The file item.</param>
    /// <returns></returns>
    string GetTemplatePath(FileItem fileItem);

    /// <summary>
    /// Transform templated path in a real path based on PathProviders
    /// </summary>
    /// <param name="fileItem">The file item.</param>
    /// <returns></returns>
    string GetInstallPath(FileItem fileItem);

    ValidationResponse Validate(FileItem fileItem);
  }
}