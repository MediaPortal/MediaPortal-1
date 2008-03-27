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

namespace MediaPortal.DeployTool
{
  class OldMsiChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MP/TV3 packages (MSI)";
    }
    public bool Download()
    {
        return true;
    }
    public bool Install()
    {
        return true;
    }
    public bool UnInstall()
    {
      Process setup;

      if( CheckUninstallString("{87819CFA-1786-484D-B0DE-10B5FBF2625D}") != null)
      {
          setup = Process.Start("msiexec.exe", "/I{87819CFA-1786-484D-B0DE-10B5FBF2625D}");
          setup.WaitForExit();
      }
      if( CheckUninstallString("{4B738773-EE07-413D-AFB7-BB0AB04A5488}") != null)
      {
          setup = Process.Start("msiexec.exe", "/I{4B738773-EE07-413D-AFB7-BB0AB04A5488}");
          setup.WaitForExit();
      }
      if( CheckUninstallString("{F7444E89-5BC0-497E-9650-E50539860DE0}") != null)
      {
          setup = Process.Start("msiexec.exe", "/I{F7444E89-5BC0-497E-9650-E50539860DE0}");
          setup.WaitForExit();
      }
      if (CheckUninstallString("{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}") != null)
      {
          setup = Process.Start("msiexec.exe", "/I{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}");
          setup.WaitForExit();
      }
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = false;
      result.state = CheckState.INSTALLED;

      //MP
      if (CheckUninstallString("{87819CFA-1786-484D-B0DE-10B5FBF2625D}") != null)
          result.state = CheckState.VERSION_MISMATCH;

      //TVServer
      if( CheckUninstallString("{4B738773-EE07-413D-AFB7-BB0AB04A5488}") != null)
          result.state = CheckState.VERSION_MISMATCH;

      //TVClient old
      if( CheckUninstallString("{F7444E89-5BC0-497E-9650-E50539860DE0}") != null)
          result.state = CheckState.VERSION_MISMATCH;

      //TVClient new
      if( CheckUninstallString("{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}") != null)
          result.state = CheckState.VERSION_MISMATCH;

      return result;
    }
    public string CheckUninstallString(string clsid)
    {
      string strUninstall;
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + clsid);
      if (key != null)
      {
          strUninstall = key.GetValue("UninstallString").ToString();
          key.Close();
          return strUninstall;
          
      }
      else
          return null;
    }
  }
}
