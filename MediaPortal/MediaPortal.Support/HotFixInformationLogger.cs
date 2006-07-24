using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace MediaPortal.Support
{
  public class HotFixInformationLogger : ILogCreator
  {
    public void CreateLogs(string destinationFolder)
    {
      string filename = Path.GetFullPath(destinationFolder) + "\\hotfixes.log";
      HotfixInformation hotfixes = new HotfixInformation();

      using (XmlWriter writer = XmlWriter.Create(filename))
      {
        writer.WriteStartElement("hotfixes");
        foreach (HotfixItem item in hotfixes)
        {
          writer.WriteStartElement("hotfix");
          writer.WriteAttributeString("name", item.Name);
          writer.WriteAttributeString("category", item.Category);
          writer.WriteAttributeString("displayName", item.DisplayName);
          writer.WriteAttributeString("installDate", item.InstallDate);
          writer.WriteAttributeString("releaseType", item.ReleaseType);
          writer.WriteAttributeString("uninstallString", item.UninstallString);
          writer.WriteAttributeString("url", item.URL);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }
    }

    public string ActionMessage
    {
      get { return "Gathering hotfix information..."; }
    }
  }
}
