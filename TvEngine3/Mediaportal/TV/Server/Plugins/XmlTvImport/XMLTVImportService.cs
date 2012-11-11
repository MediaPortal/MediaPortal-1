using System.Threading;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{  
  public class XMLTVImportService : IXMLTVImportService
  {    
    public void ImportNow()
    {
      ThreadPool.QueueUserWorkItem(
        delegate
          {
            string folder =
              SettingsManagement.GetValue("xmlTv", XmlTvImporter.DefaultOutputFolder);
            bool importXML = SettingsManagement.GetValue("xmlTvImportXML", true);
            bool importLST = SettingsManagement.GetValue("xmlTvImportLST", false);

            var importer = new XmlTvImporter();
            importer.ForceImport(folder, importXML, importLST);
          }
        );
    }
  }
}
