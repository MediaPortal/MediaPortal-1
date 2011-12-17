using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
                value;
            bool importXML =
              SettingsManagement.GetSetting("xmlTvImportXML", "true").value == "true";
            bool importLST =
              SettingsManagement.GetSetting("xmlTvImportLST", "false").value == "true";

            var importer = new XmlTvImporter();
            importer.ForceImport(folder, importXML, importLST);
          }
        );
    }
  }
}
