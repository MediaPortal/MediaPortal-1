using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Configuration;
using MpeCore;
using MpeCore.Classes.ProviderHelpers;
using MpeCore.Interfaces;
using MpeCore.Classes;
using MpeCore.Classes.InstallerType;
using MpeCore.Classes.ActionType;
using MpeCore.Classes.PathProvider;
using MpeCore.Classes.SectionPanel;
using MpeCore.Classes.ZipProvider;
using MpeCore.Classes.VersionProvider;

namespace MpeCore
{
    public static class MpeInstaller
    {
        static public Dictionary<string,IInstallerTypeProvider> InstallerTypeProviders { get; set; }
        static public Dictionary<string,IPathProvider> PathProviders { get; set; }
        static public SectionProviderHelper SectionPanels { get; set; }
        static public Dictionary<string, IActionType> ActionProviders { get; set; }
        static public Dictionary<string, IVersionProvider> VersionProviders { get; set; }
        static public ZipProviderClass ZipProvider { get; set; }
        public static ExtensionCollection InstalledExtensions { get; set; }
        public static ExtensionCollection KnownExtensions { get; set; }

        static public void Init()
        {
            InstallerTypeProviders = new Dictionary<string, IInstallerTypeProvider>();
            PathProviders = new Dictionary<string, IPathProvider>();
            SectionPanels = new SectionProviderHelper();
            ActionProviders = new Dictionary<string, IActionType>();
            VersionProviders = new Dictionary<string, IVersionProvider>();
            ZipProvider = new ZipProviderClass();


            AddInstallType(new CopyFile());
            AddInstallType(new CopyFont());
            AddInstallType(new GenericSkinFile());
            
            PathProviders.Add("MediaPortalPaths", new MediaPortalPaths());
            PathProviders.Add("TvServerPaths", new TvServerPaths());
            PathProviders.Add("WindowsPaths", new WindowsPaths());

            AddSection(new Welcome());
            AddSection(new LicenseAgreement());
            AddSection(new ReadmeInformation());
            AddSection(new ImageRadioSelector());
            AddSection(new TreeViewSelector());
            AddSection(new InstallSection());
            AddSection(new Finish());
            AddSection(new GroupCheck());
            AddSection(new GroupCheckScript());

            AddActionProvider(new InstallFiles());
            AddActionProvider(new ShowMessageBox());
            AddActionProvider(new ClearSkinCache());
            AddActionProvider(new RunApplication());
            AddActionProvider(new KillTask());
            AddActionProvider(new CreateShortCut());
            AddActionProvider(new CreateFolder());
            AddActionProvider(new ExtensionInstaller());

            AddVersion(new MediaPortalVersion());
            AddVersion(new TvServerVersion());
            AddVersion(new ExtensionVersion());
            
            InstalledExtensions =
                ExtensionCollection.Load(string.Format("{0}\\V2\\InstalledExtensions.xml",
                                                       Config.GetFolder(Config.Dir.Installer)));
            KnownExtensions =
                ExtensionCollection.Load(string.Format("{0}\\V2\\KnownExtensions.xml",
                                                       Config.GetFolder(Config.Dir.Installer)));

        }

        public static void AddVersion(IVersionProvider provider)
        {
            VersionProviders.Add(provider.DisplayName, provider);
        }

        public static void AddSection(ISectionPanel sp)
        {
            SectionPanels.Add(sp.DisplayName, sp);
        }

        public static void AddInstallType(IInstallerTypeProvider provider)
        {
            InstallerTypeProviders.Add(provider.Name, provider);
        }

        public static void AddActionProvider(IActionType ac)
        {
            ActionProviders.Add(ac.DisplayName, ac);
        }

        public static void Save()
        {
            InstalledExtensions.Save(string.Format("{0}\\V2\\InstalledExtensions.xml", Config.GetFolder(Config.Dir.Installer)));
            KnownExtensions.Save(string.Format("{0}\\V2\\KnownExtensions.xml", Config.GetFolder(Config.Dir.Installer)));

        }

        /// <summary>
        /// Transfor a real path in a template path, based on providers
        /// </summary>
        /// <param name="localFile">The location of file.</param>
        /// <returns></returns>
        static public string TransformInTemplatePath(string localFile)
        {
            foreach (var pathProvider in PathProviders)
            {
                localFile = pathProvider.Value.Colapse(localFile);
            }
            return localFile;
        }

        /// <summary>
        /// Transfor a template path in a real system path path, based on providers
        /// </summary>
        /// <param name="localFile">The template of file or path.</param>
        /// <returns></returns>
        static public string TransformInRealPath(string localFile)
        {
            foreach (var pathProvider in PathProviders)
            {
                localFile = pathProvider.Value.Expand(localFile);
            }
            return localFile;
        }
    }
}
