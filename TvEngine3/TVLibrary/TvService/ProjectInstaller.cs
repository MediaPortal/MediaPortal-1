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