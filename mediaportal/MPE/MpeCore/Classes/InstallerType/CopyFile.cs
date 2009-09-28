using System;
using System.Collections.Generic;
using System.IO;
using MpeCore.Interfaces;
using MpeCore;

namespace MpeCore.Classes.InstallerType
{
    internal class CopyFile:IInstallerTypeProvider
    {
        public string Name
        {
            get { return "CopyFile"; }
        }

        public string Description
        {
            get { return "Copy the file to speified location"; }
        }

        public void Install(FileItem fileItem)
        {
            throw new NotImplementedException();
        }

        public void Uninstall(FileItem fileItem)
        {
            throw new NotImplementedException();
        }

        public string GetZipEntry(FileItem fileItem)
        {
            return string.Format("Installer{{CopyFile}}\\{{{0}}}-{1}", Guid.NewGuid().ToString(), Path.GetFileName(fileItem.LocalFileName));
        }

        public string GetInstallPath(FileItem fileItem)
        {
            string localFile = fileItem.LocalFileName;
            foreach (var pathProvider in MpeCore.MpeInstaller.PathProviders)
            {
                localFile = pathProvider.Value.Colapse(localFile);
            }
            if (!localFile.Contains("%"))
                localFile = string.Empty;
            return localFile;
        }

        public ValidationResponse Validate(FileItem fileItem)
        {
            ValidationResponse response = new ValidationResponse();
            if(!File.Exists(fileItem.LocalFileName))
            {
                response.Valid = false;
                response.Message = "Source file not found !";
            }
            if (string.IsNullOrEmpty(fileItem.DestinationFilename))
            {
                response.Valid = false;
                response.Message = "No install location specified !";
            }
            return response;
        }
    }
}
