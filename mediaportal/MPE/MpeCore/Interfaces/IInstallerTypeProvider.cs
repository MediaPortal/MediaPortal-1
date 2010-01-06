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