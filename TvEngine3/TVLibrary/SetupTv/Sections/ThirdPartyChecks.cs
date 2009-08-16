using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
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
    private static WmpServiceStatus _wmpServices;

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
      if (!_isStreamingOk)
      {
        CheckWindowsMediaSharingService();
      }

      RefreshForm();
    }

    private void RefreshForm()
    {
      switch (_mcsServices)
      {
        case McsPolicyStatus.PolicyInPlace:
          mpLabelStatusMCS.Text = "services disabled by policy";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Green;
          mpButtonMCS.Text = "Re-enable services";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          break;
        case McsPolicyStatus.ServicesStopped:
          mpLabelStatusMCS.Text = "services stopped";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Green;
          mpButtonMCS.Text = "Enable policy to prevent services startup";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          break;
        case McsPolicyStatus.NotAMceSystem:
          mpLabelStatusMCS.Text = "services not installed";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Green;
          mpButtonMCS.Visible = false;
          mpButtonMCS.Enabled = false;
          break;
        default:
          mpLabelStatusMCS.Text = "services running";
          mpLabelStatusMCS.ForeColor = System.Drawing.Color.Red;
          mpButtonMCS.Text = "Enable policy to prevent services startup";
          mpButtonMCS.Visible = true;
          mpButtonMCS.Enabled = true;
          break;
      }

      int osver = (OSInfo.OSInfo.OSMajorVersion * 10) + OSInfo.OSInfo.OSMinorVersion;
      if (_dvbVersion < new Version(6, 5, 2710, 2732))
      {
        mpLabelStatusDVBHotfix.Text = "not installed";
        mpLabelStatusDVBHotfix.ForeColor = System.Drawing.Color.Red;
        linkLabelDVBHotfix.Enabled = true;
        linkLabelDVBHotfix.Visible = true;
      }
      else
      {
        mpLabelStatusDVBHotfix.Text = osver < 60 ? "installed" : "not needed on Vista and up";
        mpLabelStatusDVBHotfix.ForeColor = System.Drawing.Color.Green;
        linkLabelDVBHotfix.Enabled = false;
        linkLabelDVBHotfix.Visible = false;
      }

      if (_isStreamingOk)
      {
        mpLabelStatusStreamingPort.Text = "port " + STREAMING_PORT + " is available";
        mpLabelStatusStreamingPort.ForeColor = System.Drawing.Color.Green;
        linkLabelStreamingPort.Enabled = false;
        linkLabelStreamingPort.Visible = false;
        mpLabelWindowsMediaSharingServiceStatus.Visible = false;
        mpLabelStatus4.Visible = false;
      }
      else
      {
        mpLabelStatusStreamingPort.Text = "port " + STREAMING_PORT + " is already bound";
        mpLabelStatusStreamingPort.ForeColor = System.Drawing.Color.Red;
        linkLabelStreamingPort.Enabled = true;
        linkLabelStreamingPort.Visible = true;
        mpLabelWindowsMediaSharingServiceStatus.Visible = true;
        mpLabelStatus4.Visible = true;
        switch (_wmpServices)
        {
          case WmpServiceStatus.StartupAutomatic:
            mpLabelWindowsMediaSharingServiceStatus.Text = "automatic";
            mpLabelWindowsMediaSharingServiceStatus.ForeColor = System.Drawing.Color.Red;
            break;
          case WmpServiceStatus.StartupManual:
            mpLabelWindowsMediaSharingServiceStatus.Text = "manual";
            mpLabelWindowsMediaSharingServiceStatus.ForeColor = System.Drawing.Color.Red;
            break;
          case WmpServiceStatus.StartupDisabled:
            mpLabelWindowsMediaSharingServiceStatus.Text = "disabled";
            mpLabelWindowsMediaSharingServiceStatus.ForeColor = System.Drawing.Color.Green;
            break;
          case WmpServiceStatus.NotInstalled:
            mpLabelWindowsMediaSharingServiceStatus.Text = "not installed";
            mpLabelWindowsMediaSharingServiceStatus.ForeColor = System.Drawing.Color.Green;
            break;
        }
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
      return McsPolicyStatus.ServicesStopped;
    }

    private static McsPolicyStatus McsPolicyManipulation(bool checkonly)
    {
      const string keyPath = "SOFTWARE\\Policies\\Microsoft\\WindowsMediaCenter";

      RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, !checkonly);

      if (checkonly)
      {
        if (key != null)
        {
          object objValue = key.GetValue("MediaCenter");
          key.Close();
          if (objValue != null && objValue.ToString() == "1")
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
          _mcsServices = McsPolicyStatus.ServicesStopped;
        }
        else
        {
          if (key == null)
          {
            key = Registry.LocalMachine.CreateSubKey(keyPath);
          }
          key.SetValue("MediaCenter", "1", RegistryValueKind.DWord);
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
      ServicesStopped,
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
      IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
      IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();
      foreach (IPEndPoint endPoint in tcpConnInfoArray)
      {
        if (endPoint.Port == STREAMING_PORT)
        {
          //check if the port is used by TvServer or by some other process
          try
          {
            int serverId = TvControl.RemoteControl.Instance.IdServer;
            return true;
          }
          catch
          {
            return false;
          }
        }
      }
      return true;
    }

    private static void CheckWindowsMediaSharingService()
    {
      const string keyPath = "SYSTEM\\CurrentControlSet\\Services\\WMPNetworkSvc";
      RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);

      if (key != null)
      {
        string strUninstall = key.GetValue("Start").ToString();
        key.Close();
        switch (strUninstall)
        {
          case "1":
            _wmpServices = WmpServiceStatus.StartupAutomatic;
            break;
          case "2":
            _wmpServices = WmpServiceStatus.StartupManual;
            break;
          case "4":
            _wmpServices = WmpServiceStatus.StartupDisabled;
            break;
        }
      }
      else
      {
        _wmpServices = WmpServiceStatus.NotInstalled;
      }
    }

    enum WmpServiceStatus
    {
      NotInstalled,
      StartupAutomatic,
      StartupManual,
      StartupDisabled
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
