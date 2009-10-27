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
            if (componentItem.MinVersion.CompareTo(Version(componentItem.Id)) >= 0 && componentItem.MaxVersion.CompareTo(Version(componentItem.Id)) <= 0)
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