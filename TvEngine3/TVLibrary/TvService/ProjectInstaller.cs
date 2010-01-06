using System;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;

namespace TvService
{
  [RunInstaller(true)]
  public partial class ProjectInstaller : Installer
  {
    public ProjectInstaller()
    {
      InitializeComponent();
      Committed += ProjectInstaller_Committed;
    }

    private static void ProjectInstaller_Committed(object sender, InstallEventArgs e)
    {
      SetRegistryOptions();
    }

    /// <summary>
    /// Set Service options like "Interact with Desktop" for TVService. Since "InteractDesktop" is readonly it cannot be set with WMI directly.
    /// </summary>
    private static void SetRegistryOptions()
    {
      try
      {
        using (
          RegistryKey tveKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TVService", true))
        {
          if (tveKey != null)
          {
            // enable "Interact with desktop support
            if (tveKey.GetValue("Type") != null)
              tveKey.SetValue("Type", ((int)tveKey.GetValue("Type") | 256));
          }
        }
      }
      catch (Exception) {}
    }
  }
}