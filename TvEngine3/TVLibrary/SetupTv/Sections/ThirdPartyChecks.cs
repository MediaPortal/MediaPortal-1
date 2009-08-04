using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using Microsoft.Win32;

namespace SetupTv.Sections
{
  public partial class ThirdPartyChecks : SectionSettings
  {
    private const int STREAMING_PORT = 554;

    private static McsPolicyStatus _mcsServices;
    private static Version _dvbVersion;
    private static bool _isStreamingOk;

    public ThirdPartyChecks()
      : this("Additional 3rd party checks")
    {
    }

    public ThirdPartyChecks(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      _mcsServices = McsPolicyCheck();
      _dvbVersion = GetDvbhotFixVersion();
      _isStreamingOk = IsStreamingPortAvailable();

      RefreshForm();
    }

    private void RefreshForm()
    {
      switch (_mcsServices)
      {
        case McsPolicyStatus.PolicyInPlace:
          mpLabelStatusMCS.Text = "services disabled by policy";
          mpButtonMCS.Text = "Re-enable services";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          break;
        case McsPolicyStatus.ServiceStopped:
          mpLabelStatusMCS.Text = "services stopped";
          mpButtonMCS.Text = "Enable policy to prevent services startup";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          break;
        case McsPolicyStatus.NotAMceSystem:
          mpLabelStatusMCS.Text = "services not installed";
          mpButtonMCS.Visible = false;
          mpButtonMCS.Enabled = false;
          break;
        default:
          mpLabelStatusMCS.Text = "services running";
          mpButtonMCS.Text = "Enable policy to prevent services startup";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          break;
      }

      int osver = (OSInfo.OSInfo.OSMajorVersion * 10) + OSInfo.OSInfo.OSMinorVersion;
      if (_dvbVersion < new Version(6, 5, 2710, 2732))
      {
        mpLabelStatusDVBHotfix.Text = "not installed";
        linkLabelDVBHotfix.Enabled = true;
        linkLabelDVBHotfix.Visible = true;
      }
      else
      {
        mpLabelStatusDVBHotfix.Text = osver < 60 ? "installed" : "not needed on Vista and up";
        linkLabelDVBHotfix.Enabled = false;
        linkLabelDVBHotfix.Visible = false;
      }

      if (_isStreamingOk)
      {
        mpLabelStatusStreamingPort.Text = "port " + STREAMING_PORT + " is available";
        linkLabelStreamingPort.Enabled = false;
        linkLabelStreamingPort.Visible = false;
      }
      else
      {
        mpLabelStatusStreamingPort.Text = "port " + STREAMING_PORT + " is already bound";
        linkLabelStreamingPort.Enabled = true;
        linkLabelStreamingPort.Visible = true;
      }
    }

    #region MCS Policy Check

    private static McsPolicyStatus McsPolicyCheck()
    {
      // Check for status of MCE services
      bool mceSystem = false;
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController srv in services)
      {
        if (srv.ServiceName == "ehRecvr" || srv.ServiceName == "ehSched")
        {
          mceSystem = true;
          if (srv.Status == ServiceControllerStatus.Running)
          {
            return McsPolicyStatus.ServicesRunning;
          }
        }
      }

      // If services are not found, then this is not a MCE system
      if (!mceSystem)
      {
        return McsPolicyStatus.NotAMceSystem;
      }

      // Check for policy registry key
      if (McsPolicyManipulation(true) == McsPolicyStatus.PolicyInPlace)
      {
        return McsPolicyStatus.PolicyInPlace;
      }
      // No MCE services running and no policy: services are stopped
      return McsPolicyStatus.ServiceStopped;
    }

    private static McsPolicyStatus McsPolicyManipulation(bool checkonly)
    {
      const string keyPath = "SOFTWARE\\Policies\\Microsoft";
      const string keyPolicy = "WindowsMediaCenter";

      RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath + "\\" + keyPolicy, !checkonly);

      if (checkonly)
      {
        if (key != null)
        {
          string strUninstall = key.GetValue("MediaCenter").ToString();
          key.Close();
          if (strUninstall == "1")
          {
            _mcsServices = McsPolicyStatus.PolicyInPlace;
          }
        }
      }
      else
      {
        if (_mcsServices == McsPolicyStatus.PolicyInPlace)
        {
          key.DeleteValue("MediaCenter");
          key.Close();
          _mcsServices = McsPolicyStatus.ServiceStopped;
        }
        else
        {
          if (key == null)
          {
            key = Registry.LocalMachine.OpenSubKey(keyPath, true);
            key.CreateSubKey(keyPolicy);
            key.Close();
            key = Registry.LocalMachine.OpenSubKey(keyPath + "\\" + keyPolicy);
          }
          key.SetValue("MediaCenter", "1");
          key.Close();
          _mcsServices = McsPolicyStatus.PolicyInPlace;
        }
      }
      return _mcsServices;
    }

    enum McsPolicyStatus
    {
      NotAMceSystem,
      ServicesRunning,
      ServiceStopped,
      PolicyInPlace
    }

    private void mpButtonMCS_Click(object sender, EventArgs e)
    {
      McsPolicyManipulation(false);
      RefreshForm();
    }

    #endregion

    #region DVB HotFix Check

    private static Version GetDvbhotFixVersion()
    {
      List<string> dllPaths = Utils.GetRegisteredAssemblyPaths("PsisDecd");
      var aParamVersion = new Version(0, 0, 0, 0);
      Version mostRecentVer = aParamVersion;
      foreach (string dllPath in dllPaths)
      {
        Utils.CheckFileVersion(dllPath, "6.5.2710.2732", out aParamVersion);
        if (File.Exists(dllPath) && aParamVersion > mostRecentVer)
        {
          mostRecentVer = aParamVersion;
        }
      }
      return mostRecentVer;
    }

    #endregion

    #region Streaming Port Check

    private static bool IsStreamingPortAvailable()
    {
      IPGlobalProperties ipGlobalProperties =
        IPGlobalProperties.GetIPGlobalProperties();
      TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
      foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
      {
        if (tcpi.LocalEndPoint.Port == STREAMING_PORT)
        {
          return false;
        }
      }
      return true;
    }

    #endregion

    #region Link labels

    private void linkLabelDVBHotfix_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      // DVB hotfix download link
      Process.Start(@"http://wiki.team-mediaportal.com/GeneralRequirements");
    }

    private void linkLabelStreamingPort_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      // TCPView download link
      Process.Start(@"http://technet.microsoft.com/en-us/sysinternals/bb897437.aspx");
    }

    #endregion

  }
}
