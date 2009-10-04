using System;
using System.Collections.Generic;
using System.Text;
using MpeCore;
using MpeCore.Interfaces;
using MpeCore.Classes.InstallerType;
using MpeCore.Classes.PathProvider;
using MpeCore.Classes.SectionPanel;
using MpeCore.Classes.ZipProvider;
namespace MpeCore
{
    public static class MpeInstaller
    {
        static public Dictionary<string,IInstallerTypeProvider> InstallerTypeProviders { get; set; }
        static public Dictionary<string,IPathProvider> PathProviders { get; set; }
        static public Dictionary<string, ISectionPanel> SectionPanels { get; set; }
        static public ZipProviderClass ZipProvider { get; set; }

        static public void Init()
        {
            InstallerTypeProviders = new Dictionary<string, IInstallerTypeProvider>();
            PathProviders = new Dictionary<string, IPathProvider>();
            SectionPanels = new Dictionary<string, ISectionPanel>();
            ZipProvider = new ZipProviderClass();

            InstallerTypeProviders.Add("CopyFile",new CopyFile());

            PathProviders.Add("MediaPortalPaths", new MediaPortalPaths());
            PathProviders.Add("WindowsPaths", new WindowsPaths());

            SectionPanels.Add("Welcome", new Welcome());
            SectionPanels.Add("LicenseAgreement", new LicenseAgreement());
            SectionPanels.Add("ImageRadioSelector", new ImageRadioSelector());
            SectionPanels.Add("TreeViewSelector", new TreeViewSelector());
            SectionPanels.Add("InstallSection", new InstallSection());
            
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
    }
}
