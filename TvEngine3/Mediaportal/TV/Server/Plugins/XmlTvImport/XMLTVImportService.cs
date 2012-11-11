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
              SettingsManagement.GetSetting("xmlTv", XmlTvImporter.DefaultOutputFolder).
                Value;
            bool importXML =
              SettingsManagement.GetSetting("xmlTvImportXML", "true").Value == "true";
            bool importLST =
              SettingsManagement.GetSetting("xmlTvImportLST", "false").Value == "true";

            var importer = new XmlTvImporter();
            importer.ForceImport(folder, importXML, importLST);
          }
        );
    }
  }
}
