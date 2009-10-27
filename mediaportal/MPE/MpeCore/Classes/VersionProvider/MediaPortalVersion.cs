using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using MpeCore.Interfaces;
using MpeCore.Classes;

namespace MpeCore.Classes.VersionProvider
{
    public class MediaPortalVersion : IVersionProvider
    {
        public string DisplayName
        {
            get { return "MediaPortal"; }
        }

        public bool Validate(DependencyItem componentItem)
        {
            if (componentItem.MinVersion.CompareTo(Version(componentItem.Id)) >= 0 && componentItem.MaxVersion.CompareTo(Version(componentItem.Id)) <= 0)
                return true;
            return false;
        }

        public VersionInfo Version(string id)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal");
            if (key != null)
            {
                var version = new VersionInfo
                {
                    Build = ((int)key.GetValue("VersionBuild", 0)).ToString(),
                    Major = ((int)key.GetValue("VersionMajor", 0)).ToString(),
                    Minor = ((int)key.GetValue("VersionMinor", 0)).ToString(),
                    Revision = ((int)key.GetValue("VersionRevision", 0)).ToString(),
                };
                key.Close();
                return version;
            }
            return new VersionInfo();
        }
    }
}
