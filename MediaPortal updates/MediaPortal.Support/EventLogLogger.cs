using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace MediaPortal.Support
{
  public class EventLogLogger : ILogCreator
  {
    private string logname;
    public EventLogLogger(string logname)
    {
      this.logname = logname;
    }

    public void CreateLogs(string destinationFolder)
    {
      string filename = Path.GetFullPath(destinationFolder) + "\\" + logname + "_events.xml";
      using (XmlWriter writer = XmlWriter.Create(filename))
      {
        EventLog log = new EventLog(logname);
        writer.WriteStartElement("events");
        foreach (EventLogEntry entry in log.Entries)
        {
          writer.WriteStartElement("event");
          writer.WriteAttributeString("category", entry.Category);
          writer.WriteAttributeString("categoryNumber", entry.CategoryNumber.ToString());
          writer.WriteAttributeString("entryType", entry.EntryType.ToString());
          writer.WriteAttributeString("application", entry.Source);
          writer.WriteAttributeString("time", entry.TimeGenerated.ToString("s"));
          writer.WriteCData(entry.Message);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }
    }

    public string ActionMessage
    {
      get { return "Gathering system eventlog information"; }
    }
  }
}
