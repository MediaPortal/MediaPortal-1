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

using System;
using System.Diagnostics;
using System.IO;

namespace MediaPortal.Support
{
  public class EventLogCsvLogger : ILogCreator
  {
    private string logname;

    public EventLogCsvLogger(string logname)
    {
      this.logname = logname;
    }

    public void CreateLogs(string destinationFolder)
    {
      string filename = Path.GetFullPath(destinationFolder) + "\\" + logname + "_eventlog.csv";
      using (StreamWriter writer = new StreamWriter(filename))
      {
        writer.WriteLine("\"TimeGenerated\";\"Source\";\"Category\";\"EntryType\";\"Message\";\"InstanceID\"");
        EventLog log = new EventLog(logname);
        foreach (EventLogEntry entry in log.Entries)
        {
          string line = "\"" + entry.TimeGenerated.ToString() + "\";";
          line += "\"" + entry.Source + "\";";
          line += "\"" + entry.Category + "\";";
          line += "\"" + entry.EntryType.ToString() + "\";";
          line += "\"" + entry.Message.Replace(Environment.NewLine, " ") + "\";";
          line += "\"" + entry.InstanceId.ToString() + "\"";
          writer.WriteLine(line);
        }
        writer.Close();
      }
    }

    public string ActionMessage
    {
      get { return "Gathering system eventlog information..."; }
    }
  }
}