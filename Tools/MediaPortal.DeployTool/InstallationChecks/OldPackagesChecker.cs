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
  class OldPackageChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
        return "Old MP/TV-Engine installation";
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

      //MP < 0.2.3.0 RC3
      if( CheckUninstallString("{87819CFA-1786-484D-B0DE-10B5FBF2625D}") != null)
      {
          setup = Process.Start("msiexec.exe", "/x {87819CFA-1786-484D-B0DE-10B5FBF2625D} /qn");
          setup.WaitForExit();
      }
      //MP 0.2.3.0 RC3
      if (CheckUninstallString("MediaPortal 0.2.3.0 RC3") != null)
      {
          string RegistryFullPathName = CheckUninstallString("MediaPortal 0.2.3.0 RC3");
          string FileName  = Path.GetFileName(RegistryFullPathName);
          string Directory = Path.GetDirectoryName(RegistryFullPathName);
          string TempFullPathName = Environment.GetEnvironmentVariable("TEMP") + "\\" + FileName;
          File.Copy(RegistryFullPathName, TempFullPathName);
          setup = Process.Start(TempFullPathName, " /S _?=" + Directory);
          setup.WaitForExit();
          File.Delete(TempFullPathName);
      }
      //MP 0.2.3.0
      if (CheckUninstallString("MediaPortal 0.2.3.0") != null)
      {
          string RegistryFullPathName = CheckUninstallString("MediaPortal 0.2.3.0");
          string FileName  = Path.GetFileName(RegistryFullPathName);
          string Directory = Path.GetDirectoryName(RegistryFullPathName);
          string TempFullPathName = Environment.GetEnvironmentVariable("TEMP") + "\\" + FileName;
          File.Copy(RegistryFullPathName, TempFullPathName);
          setup = Process.Start(TempFullPathName, " /S _?=" + Directory);
          setup.WaitForExit();
          File.Delete(TempFullPathName);
      }
      //TVServer
      if( CheckUninstallString("{4B738773-EE07-413D-AFB7-BB0AB04A5488}") != null)
      {
          setup = Process.Start("msiexec.exe", "/x {4B738773-EE07-413D-AFB7-BB0AB04A5488} /qn");
          setup.WaitForExit();
      }
      //TVClient old
      if( CheckUninstallString("{F7444E89-5BC0-497E-9650-E50539860DE0}") != null)
      {
          setup = Process.Start("msiexec.exe", "/x {F7444E89-5BC0-497E-9650-E50539860DE0} /qn");
          setup.WaitForExit();
      }
      //TVClient new
      if (CheckUninstallString("{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}") != null)
      {
          setup = Process.Start("msiexec.exe", "/x {FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA} /qn");
          setup.WaitForExit();
      }
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = false;
      result.state = CheckState.REMOVED;

      //MP < 0.2.3.0 RC3
      if (CheckUninstallString("{87819CFA-1786-484D-B0DE-10B5FBF2625D}") != null)
          result.state = CheckState.NOT_REMOVED;

      //MP 0.2.3.0 RC3
      if (CheckUninstallString("MediaPortal 0.2.3.0 RC3") != null)
          result.state = CheckState.NOT_REMOVED;

      //MP 0.2.3.0
      if (CheckUninstallString("MediaPortal 0.2.3.0") != null)
          result.state = CheckState.NOT_REMOVED;

      //TVServer
      if( CheckUninstallString("{4B738773-EE07-413D-AFB7-BB0AB04A5488}") != null)
          result.state = CheckState.NOT_REMOVED;

      //TVClient old
      if( CheckUninstallString("{F7444E89-5BC0-497E-9650-E50539860DE0}") != null)
          result.state = CheckState.NOT_REMOVED;

      //TVClient new
      if( CheckUninstallString("{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}") != null)
          result.state = CheckState.NOT_REMOVED;

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
