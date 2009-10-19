using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MpeCore.Interfaces;

namespace MpeCore.Classes.InstallerType
{
    class CopyFont : CopyFile, IInstallerTypeProvider
    {
        [DllImport("gdi32")]
        public static extern int AddFontResource(string lpFileName);
        [DllImport("gdi32")]
        public static extern int RemoveFontResource(string lpFileName); 

        public new string Name
        {
            get { return "CopyFont"; }
        }

        override public string Description
        {
            get { return "Copy the file to specified location and register to system fonts"; }
        }

        public new string GetZipEntry(FileItem fileItem)
        {
            return string.Format("Installer{{Font}}\\{{{0}}}-{1}", Guid.NewGuid(), Path.GetFileName(fileItem.LocalFileName));
        }

        public new void Install(PackageClass packageClass, FileItem fileItem)
        {
            base.Install(packageClass, fileItem);
            int res = AddFontResource(fileItem.ExpandedDestinationFilename);
        }
    }
}
