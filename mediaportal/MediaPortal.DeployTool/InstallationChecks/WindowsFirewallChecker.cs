#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace MediaPortal.DeployTool
{
  class WindowsFirewallChecker: IInstallationPackage
  {
    #region Helper functions
    private void ConfigureFirewallProfile(string profile)
    {
      // Applications
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\" + profile + "\\AuthorizedApplications\\List", true);
      if (InstallationProperties.Instance["ConfigureTVServerFirewall"]=="1")
        key.SetValue(InstallationProperties.Instance["TVServerDir"] + "\\TvService.exe", InstallationProperties.Instance["TVServerDir"] + "\\TvService.exe:*:Enabled:TvService.exe", RegistryValueKind.String);
      if (InstallationProperties.Instance["ConfigureDBMSFirewall"] == "1")
      {
        if (InstallationProperties.Instance["DBMSType"] == "mssql")
        {
          key.SetValue(InstallationProperties.Instance["DBMSDir"] + "\\MSSQL.1\\MSSQL\\Binn\\sqlservr.exe", InstallationProperties.Instance["DBMSDir"] + "\\MSSQL.1\\MSSQL\\Binn\\sqlservr.exe:*:Enabled:sqlservr.exe", RegistryValueKind.String);
          key.SetValue(InstallationProperties.Instance["DBMSDir"] + "\\90\\Shared\\sqlbrowser.exe", InstallationProperties.Instance["DBMSDir"] + "\\90\\Shared\\sqlbrowser.exe:*:Enabled:sqlbrowser.exe", RegistryValueKind.String);
        }
        else
          key.SetValue(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld-net.exe", InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld-net.exe:*:Enable:mysqld-nt.exe", RegistryValueKind.String);
      }
      key.Flush();
      key.Close();
      // Ports
      key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\" + profile + "\\GloballyOpenPorts\\List", true);
      if (InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1")
        key.SetValue("554:TCP", "554:TCP:*:Enabled:MediaPortal TvServer RTSP Streaming (TCP)", RegistryValueKind.String);
      for (int i = 6970; i < 10000; i++)
        key.SetValue(i.ToString() + ":UDP", i.ToString() + ":UDP:*:Enabled:MediaPortal TvServer RTSP Streaming (UDP Port " + i.ToString() + ")", RegistryValueKind.String);
      if (InstallationProperties.Instance["ConfigureDBMSFirewall"] == "1")
      {
        if (InstallationProperties.Instance["DBMSType"] == "mssql")
        {
          key.SetValue("1433:TCP", "1433:TCP:*:Enabled:Microsoft SQL Server Express (TCP)", RegistryValueKind.String);
          key.SetValue("1434:UDP", "1434:UDP:*:Enabled:Microsoft SQL Server Express (UDP)");
        }
        else
          key.SetValue("3306:TCP", "3306:TCP:*:Enabled:MySQL Server 5 (TCP)");
      }
      key.Flush();
      key.Close();
    }
    private void ConfigureWindowsFirewall()
    {
      ConfigureFirewallProfile("StandardProfile");
      ConfigureFirewallProfile("DomainProfile");
    }
    #endregion
    public string GetDisplayName()
    {
      return "Windows Firewall Config";
    }

    public bool Download()
    {
      return true;
    }
    public bool Install()
    {
      ConfigureWindowsFirewall();
      return true;
    }
    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = false;
      result.state = CheckState.INSTALLED;
      if (InstallationProperties.Instance["ConfigureTVServerFirewall"]=="1")
      {
        RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\\AuthorizedApplications\\List", true);
      }

      return result;
    }
  }
}
