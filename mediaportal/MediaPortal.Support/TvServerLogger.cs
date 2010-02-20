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
using System.IO;

namespace MediaPortal.Support
{
  public class TvServerLogger : ILogCreator
  {
    public void CreateLogs(string destinationFolder)
    {
      string basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                        "\\Team MediaPortal\\MediaPortal TV Server";
      string logPath = basePath + "\\log";
      if (!Directory.Exists(logPath))
      {
        return;
      }
      DirectoryInfo dir = new DirectoryInfo(logPath);
      FileInfo[] logFiles = dir.GetFiles("*.log");
      foreach (FileInfo logFile in logFiles)
      {
        logFile.CopyTo(destinationFolder + "\\tvserver_" + logFile.Name, true);
      }

      FileInfo[] bakfiles = dir.GetFiles("*.bak");
      foreach (FileInfo bakFile in bakfiles)
      {
        bakFile.CopyTo(destinationFolder + "\\tvserver_" + bakFile.Name, true);
      }

      string AnalogPath = basePath + "\\AnalogCard";
      if (Directory.Exists(AnalogPath))
      {
        FileInfo[] xmlFiles = new DirectoryInfo(AnalogPath).GetFiles("*.xml");
        foreach (FileInfo xmlFile in xmlFiles)
        {
          xmlFile.CopyTo(destinationFolder + "\\tvserver_AnalogCard_" + xmlFile.Name, true);
        }
      }
    }

    public string ActionMessage
    {
      get { return "Gathering TvServer log information if any..."; }
    }
  }
}