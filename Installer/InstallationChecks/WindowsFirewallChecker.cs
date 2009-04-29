#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.IO;
using System.Windows.Forms;
using NetFwTypeLib;
#if DEBUG // we need it for the msg box
using System.Windows.Forms;
#endif

namespace MediaPortal.DeployTool.InstallationChecks
{
  class WindowsFirewallChecker : IInstallationPackage
  {
    #region Firewall API functions
    private const string PROGID_FIREWALL_MANAGER = "HNetCfg.FwMgr";
    private const string PROGID_AUTHORIZED_APPLICATION = "HNetCfg.FwAuthorizedApplication";
    private const string PROGID_OPEN_PORT = "HNetCfg.FWOpenPort";

    private static INetFwMgr GetFirewallManager()
    {
      try
      {
        Type objectType = Type.GetTypeFromProgID(PROGID_FIREWALL_MANAGER);
        return Activator.CreateInstance(objectType) as INetFwMgr;
      }
      catch (Exception)
      {
        return null;
      }
    }
    private static void AuthorizeApplication(string title, string applicationPath, NET_FW_SCOPE_ scope, NET_FW_IP_VERSION_ ipVersion)
    {
      Type type = Type.GetTypeFromProgID(PROGID_AUTHORIZED_APPLICATION);
      INetFwAuthorizedApplication auth = Activator.CreateInstance(type) as INetFwAuthorizedApplication;
      if (auth != null)
      {
        auth.Name = title;
      }
      if (!File.Exists(applicationPath))
        return;
      if (auth != null)
      {
        auth.ProcessImageFileName = applicationPath;
        auth.Scope = scope;
        auth.IpVersion = ipVersion;
        auth.Enabled = true;
      }
      INetFwMgr manager = GetFirewallManager();
      try
      {
        manager.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(auth);
      }
      catch
      {
        return;
      }
    }
    private static void GloballyOpenPort(string title, int portNo, NET_FW_SCOPE_ scope, NET_FW_IP_PROTOCOL_ protocol, NET_FW_IP_VERSION_ ipVersion)
    {
      Type type = Type.GetTypeFromProgID(PROGID_OPEN_PORT);
      INetFwOpenPort port = Activator.CreateInstance(type) as INetFwOpenPort;
      if (port != null)
      {
        port.Name = title;
        port.Port = portNo;
        port.Scope = scope;
        port.Protocol = protocol;
        port.IpVersion = ipVersion;
      }
      INetFwMgr manager = GetFirewallManager();
      try
      {
        manager.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(port);
      }
      catch
      {
        return;
      }
    }

    #endregion
    #region Helper functions
    private static void ConfigureFirewallProfile()
    {
      string app;

      if (InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1")
      {
        //TVService
        app = InstallationProperties.Instance["TVServerDir"] + "\\TvService.exe";
        AuthorizeApplication("MediaPortal TV Server", app, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
      }
      if (InstallationProperties.Instance["ConfigureMediaPortalFirewall"] == "1")
      {
        //MediaProtal
        app = InstallationProperties.Instance["MPDir"] + "\\MediaPortal.exe";
        AuthorizeApplication("MediaPortal", app, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
      }
      if (InstallationProperties.Instance["ConfigureDBMSFirewall"] == "1")
      {
        int port;
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
      //Uninstall not handled: ports/app are left in the auth part of the firewall
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = false;
      result.state = CheckState.CONFIGURED;

      //If both applications don't request to configure fw, no need to go further
      if (InstallationProperties.Instance["ConfigureTVServerFirewall"] != "1" &&
          InstallationProperties.Instance["ConfigureMediaPortalFirewall"] != "1")
      {
        result.state = CheckState.SKIPPED;
        return result;
      }

#if DEBUG
      if (InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1")
        MessageBox.Show("TVServer request firewall cfg", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      if (InstallationProperties.Instance["ConfigureMediaPortalFirewall"] == "1")
        MessageBox.Show("MediaPortal request firewall cfg", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif

      INetFwMgr fwMgr = GetFirewallManager();
      if (fwMgr == null)
      {
        //If firewall service is stopped, no need to configure it
#if DEBUG
        MessageBox.Show("Firewall service stopped!", "GetFirewallManager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
        result.state = CheckState.SKIPPED;
        return result;
      }

      try
      {
        if (!fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled)
        {
          //If firewall service is disabled, no need to configure it
#if DEBUG
          MessageBox.Show("Firewall service disabled!", "fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
          result.state = CheckState.SKIPPED;
          return result;
        }
      }
      catch (Exception)
      {
        // If a 3rd party firewall is active, an exception is throw
#if DEBUG
        MessageBox.Show("3rd party firewall service detected!", "fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
        result.state = CheckState.SKIPPED;
        return result;
      }

      System.Collections.IEnumerator e1 = fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.GetEnumerator();

      //TvService
      string apptv = InstallationProperties.Instance["TVServerDir"] + "\\TvService.exe";
      bool chktv = false;
      if (InstallationProperties.Instance["ConfigureTVServerFirewall"] != "1") chktv = true;

      //MediaPortal
      string appmp = InstallationProperties.Instance["MPdir"] + "\\MediaPortal.exe";
      bool chkmp = false;
      if (InstallationProperties.Instance["ConfigureMediaPortalFirewall"] != "1") chkmp = true;

      while (e1.MoveNext())
      {
        INetFwAuthorizedApplication app = e1.Current as INetFwAuthorizedApplication;
        if (app != null)
        {
          if (app.ProcessImageFileName.ToLower() == apptv.ToLower())
            chktv = true;
          if (app.ProcessImageFileName.ToLower() == appmp.ToLower())
            chkmp = true;
        }
      }

      if (chktv && chkmp)
        result.state = CheckState.CONFIGURED;
      else
        result.state = CheckState.NOT_CONFIGURED;

      if (result.state == CheckState.CONFIGURED)
      {
        if (InstallationProperties.Instance["ConfigureDBMSFirewall"] == "1")
        {
          result.state = CheckState.NOT_CONFIGURED;

          System.Collections.IEnumerator e2 = fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts.GetEnumerator();

          while (e2.MoveNext())
          {
            INetFwOpenPort app = e2.Current as INetFwOpenPort;
            if (app != null)
            {
              if (InstallationProperties.Instance["DBMSType"] == "mssql2005")
              {
                if (app.Port == 1433)
                  result.state = CheckState.CONFIGURED;
              }
              else
              {
                if (app.Port == 3306)
                  result.state = CheckState.CONFIGURED;
              }
            }
          }
        }
      }
      return result;
    }
  }
}