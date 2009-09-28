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
        void Install(FileItem fileItem);
        void Uninstall(FileItem fileItem);
        string GetZipEntry(FileItem fileItem);
        string GetInstallPath(FileItem fileItem);
        ValidationResponse Validate(FileItem fileItem);
    }
}
