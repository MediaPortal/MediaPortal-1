#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using NetFwTypeLib;

namespace MediaPortal.DeployTool
{
  class WindowsFirewallChecker: IInstallationPackage
  {
    #region Firewall API functions
      private const string PROGID_FIREWALL_MANAGER = "HNetCfg.FwMgr";
      private const string PROGID_AUTHORIZED_APPLICATION = "HNetCfg.FwAuthorizedApplication";
      private const string PROGID_OPEN_PORT = "HNetCfg.FWOpenPort";

      private static NetFwTypeLib.INetFwMgr GetFirewallManager()
      {
          Type objectType = Type.GetTypeFromProgID(PROGID_FIREWALL_MANAGER);
          return Activator.CreateInstance(objectType) as NetFwTypeLib.INetFwMgr;
      }
      private bool AuthorizeApplication(string title, string applicationPath, NET_FW_SCOPE_ scope, NET_FW_IP_VERSION_ ipVersion)
      {
          Type type = Type.GetTypeFromProgID(PROGID_AUTHORIZED_APPLICATION);  
          INetFwAuthorizedApplication auth = Activator.CreateInstance(type) as INetFwAuthorizedApplication;
          auth.Name  = title;  
          auth.ProcessImageFileName = applicationPath;  
          auth.Scope = scope;  
          auth.IpVersion = ipVersion;  
          auth.Enabled = true;
          INetFwMgr manager = GetFirewallManager(); 
          try 
          { 
              manager.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(auth); 
          }
          catch
          { 
              return false; 
          } 
          return true;
      }   
      private bool GloballyOpenPort(string title, int portNo, NET_FW_SCOPE_ scope, NET_FW_IP_PROTOCOL_ protocol, NET_FW_IP_VERSION_ ipVersion)
      {  
          Type type = Type.GetTypeFromProgID(PROGID_OPEN_PORT);  
          INetFwOpenPort port = Activator.CreateInstance(type) as INetFwOpenPort;  
          port.Name = title;  
          port.Port = portNo;  
          port.Scope = scope;  
          port.Protocol = protocol;  
          port.IpVersion = ipVersion;  
          INetFwMgr manager = GetFirewallManager();  
          try  
          {    
              manager.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(port);  
          }  
          catch
          {    
              return false;  
          }
          return true;
      }

      #endregion
    #region Helper functions
    private void ConfigureFirewallProfile()
    {
        int port = 0;
        string app = "";

        if (InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1")
        {
            //TVService
            app = InstallationProperties.Instance["TVServerDir"] + "\\TvService.exe";
            AuthorizeApplication("MediaPortal TV Server", app, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
        } 
        if  (InstallationProperties.Instance["ConfigureDBMSFirewall"] == "1")
        {
            if (InstallationProperties.Instance["DBMSType"] == "mssql2005")
            {
                //SQL2005 TCP Port
                port = 1433;
                GloballyOpenPort("Microsoft SQL (TCP)", port, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
                
                //SQL2005 UDP Port
                port = 1434;
                GloballyOpenPort("Microsoft SQL (UDP)", port, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
            }
            else
            {
                //MySQL TCP Port
                port = 3306;
                GloballyOpenPort("MySQL", port, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
            }
        }
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
      ConfigureFirewallProfile();
      return true;
    }
    public bool UnInstall()
    {
      //Uninstall not yet handled...
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = false;
      result.state = CheckState.INSTALLED;
      INetFwMgr fwMgr = GetFirewallManager();

      if (InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1")
      {
          //If firewall is not enabled, no need to configure it
          if (fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled == false)
              result.state = CheckState.INSTALLED;

          System.Collections.IEnumerator e = null;
          e = fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.GetEnumerator();

          result.state = CheckState.NOT_INSTALLED;
          while (e.MoveNext())
          {
              INetFwAuthorizedApplication app = e.Current as INetFwAuthorizedApplication;
              string apptv = InstallationProperties.Instance["TVServerDir"] + "\\TvService.exe";
              if (app.ProcessImageFileName.ToLower() == apptv.ToLower())
                  result.state = CheckState.INSTALLED;           
          }
      }
      if (result.state == CheckState.INSTALLED)
      {
        if (InstallationProperties.Instance["ConfigureDBMSFirewall"] == "1")
        {
            result.state = CheckState.NOT_INSTALLED;
            
            System.Collections.IEnumerator e = null;
            e = fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts.GetEnumerator();

            while (e.MoveNext())
            {
                INetFwOpenPort app = e.Current as INetFwOpenPort;
                if (InstallationProperties.Instance["DBMSType"] == "mssql2005")
                {
                    if(app.Port == 1433)  
                        result.state = CheckState.INSTALLED;
                }
                else
                {
                    if(app.Port == 3306)
                        result.state = CheckState.INSTALLED;
                }
            }
        }
      }
      return result;
    }
  }
}
