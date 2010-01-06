#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

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