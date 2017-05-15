#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
      string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server");
      string logPath = Path.Combine(basePath, "log");
      if (!Directory.Exists(logPath))
      {
        return;
      }
      DirectoryInfo dir = new DirectoryInfo(logPath);
      FileInfo[] logFiles = dir.GetFiles("*.log");
      foreach (FileInfo logFile in logFiles)
      {
        logFile.CopyTo(Path.Combine(destinationFolder, "tvserver_" + logFile.Name), true);
      }

      FileInfo[] bakfiles = dir.GetFiles("*.bak");
      foreach (FileInfo bakFile in bakfiles)
      {
        bakFile.CopyTo(Path.Combine(destinationFolder, "tvserver_" + bakFile.Name), true);
      }

      var gentleConfigPath = Path.Combine(basePath, "Gentle.config");
      if (File.Exists(gentleConfigPath))
      {
        var destGentleConfigPath = Path.Combine(destinationFolder, "tvserver_Gentle.config");
        File.Copy(gentleConfigPath, destGentleConfigPath, true);
      }

      var iptvSourceConfigPath = Path.Combine(basePath, "MPIPTVSource.ini");
      if (File.Exists(iptvSourceConfigPath))
      {
        var destIptvSourceConfigPath = Path.Combine(destinationFolder, "tvserver_MPIPTVSource.ini");
        File.Copy(iptvSourceConfigPath, destIptvSourceConfigPath, true);
      }

      var webEpgConfigPath = Path.Combine(basePath, "WebEPG", "WebEPG.xml");
      if (File.Exists(webEpgConfigPath))
      {
        var destWebEpgConfigPath = Path.Combine(destinationFolder, "tvserver_WebEPG.xml");
        File.Copy(webEpgConfigPath, destWebEpgConfigPath, true);
      }

      string analogTunerInfoPath = Path.Combine(basePath, "AnalogCard");
      if (Directory.Exists(analogTunerInfoPath))
      {
        FileInfo[] xmlFiles = new DirectoryInfo(analogTunerInfoPath).GetFiles("*.xml");
        foreach (FileInfo xmlFile in xmlFiles)
        {
          xmlFile.CopyTo(Path.Combine(destinationFolder, "tvserver_AnalogCard_" + xmlFile.Name), true);
        }
      }
    }

    public string ActionMessage
    {
      get { return "Gathering TV Server log files and configuration if any..."; }
    }
  }
}