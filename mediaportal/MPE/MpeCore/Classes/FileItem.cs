using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MpeCore.Classes;

namespace MpeCore.Classes
{
    /// <summary>
    /// Reprezent a single file item wich is included in package
    /// </summary>
    public class FileItem
    {
        public FileItem()
        {
            Init();
        }

        public FileItem(string fileName, bool systemFile)
        {
            Init();
            LocalFileName = fileName;
            SystemFile = systemFile;
        }

        public FileItem(FileItem fileItem)
        {
            LocalFileName = fileItem.LocalFileName;
            ZipFileName = fileItem.ZipFileName;
            InstallType = fileItem.InstallType;
            DestinationFilename = fileItem.DestinationFilename;
            SystemFile = fileItem.SystemFile;
        }

        /// <summary>
        /// Reinitialize the class
        /// </summary>
        public void Init()
        {
            LocalFileName = string.Empty;
            ZipFileName = string.Empty;
            InstallType = "CopyFile";
            SystemFile = false;
        }

        /// <summary>
        /// Gets or sets the name of the local file.
        /// This value used only when creating the package
        /// </summary>
        /// <value>The name of the local file.</value>
        public string LocalFileName { get; set; }

        private string _zipFileName;

        /// <summary>
        /// Gets or sets the name and path of the file from ziped package.
        /// </summary>
        /// <value>The name of the zip file.</value>
        public string ZipFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_zipFileName) && MpeInstaller.InstallerTypeProviders.ContainsKey(InstallType))
                    _zipFileName = MpeInstaller.InstallerTypeProviders[InstallType].GetZipEntry(this);
                return _zipFileName;
            }
            set { _zipFileName = value; }
        }

        private string _destinationFilename;

        /// <summary>
        /// Gets or sets the destination path and filename were the file will be installed.
        /// </summary>
        /// <value>The destination filename.</value>
        public string DestinationFilename
        {
            get
            {
                if (string.IsNullOrEmpty(_destinationFilename) && MpeInstaller.InstallerTypeProviders.ContainsKey(InstallType))
                {
                    _destinationFilename = MpeInstaller.InstallerTypeProviders[InstallType].GetInstallPath(this);
                }
                return _destinationFilename;
            }
            set { _destinationFilename = value; }
        }

        /// <summary>
        /// Gets or sets the type of the install. Type is  provided by Installer static class
        /// </summary>
        /// <value>The type of the install.</value>
        [XmlAttribute]
        public string InstallType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the item is used by the installer it self.
        /// </summary>
        /// <value><c>true</c> if [system file]; otherwise, <c>false</c>.</value>
        [XmlAttribute]
        public bool SystemFile { get; set; }

        /// <summary>
        /// Gets or sets the temp file location. This property have value if the item is extracted in a temporally location
        /// </summary>
        /// <value>The temp file location.</value>
        [XmlIgnore]
        public string TempFileLocation{ get; set; }

        public override string ToString()
        {
            return Path.GetFileName(LocalFileName);   
        }
	
    }
}
