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
using System.Windows.Forms;
using Microsoft.Win32;

namespace MediaPortal.Support
{
  public class PlatformLogger : ILogCreator
  {
    private string GetCPUInfos()
    {
      RegistryKey mainKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor");
      string[] subKeys = mainKey.GetSubKeyNames();
      string cpuInfos = "";
      for (int i = 0; i < subKeys.Length; i++)
      {
        RegistryKey key = mainKey.OpenSubKey(subKeys[i]);
        string cpuType = (string)key.GetValue("ProcessorNameString", "<unknown>");
        int cpuSpeed = (int)key.GetValue("~MHz", 0);
        cpuInfos += cpuType + " running at ~" + cpuSpeed + " MHz.<br>";
        key.Close();
      }
      mainKey.Close();
      return cpuInfos;
    }

    public PlatformLogger() {}

    public void CreateLogs(string destinationFolder)
    {
      StreamWriter sw = new StreamWriter(destinationFolder + "\\PlatformInfo.html");
      sw.WriteLine("<html><head><title>Platform information log</title></head><body><table border=0>");
      sw.WriteLine("<tr><td><b>Operating system</b></td><td>" + Environment.OSVersion.VersionString + "</td></tr>");
      sw.WriteLine("<tr><td><b>Hostname</b></td><td>" + Environment.MachineName + "</td></tr>");
      sw.WriteLine("<tr><td><b>Network attached?</b></td><td>" + SystemInformation.Network.ToString() + "</td></tr>");
      sw.WriteLine("<tr><td><b>Primary monitor size</b></td><td>" + SystemInformation.PrimaryMonitorSize.ToString() +
                   "</td></tr>");
      sw.WriteLine("<tr><td><b>CPU details</b></td><td>" + GetCPUInfos() + "</td></tr>");
      sw.WriteLine("</body></html>");
      sw.Close();
    }

    public string ActionMessage
    {
      get { return "Gathering System platform informations..."; }
    }
  }
}