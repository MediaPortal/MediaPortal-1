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

using System.IO;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class OldPackageChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "Very old MP/TV-Engine installation";
    }
    public bool Download()
    {
      return true;
    }
    public bool Install()
    {
      UnInstall();
      return true;
    }
    public bool UnInstall()
    {
      //MP 0.2.3.0 RC3
      string keyUninstall = Utils.CheckUninstallString("MediaPortal 0.2.3.0 RC3", true);
      if (keyUninstall != null && File.Exists(keyUninstall))
      {
        Utils.UninstallNSIS(keyUninstall);
      }

      string[] UninstKeys = {"{87819CFA-1786-484D-B0DE-10B5FBF2625D}",  //MP < 0.2.3.0 RC3
                             "{4B738773-EE07-413D-AFB7-BB0AB04A5488}",  //TVServer MSI
                             "{F7444E89-5BC0-497E-9650-E50539860DE0}",  //TVClient (old MSI clsid)
                             "{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}"}; //TVClient (new MSI clsid)

      foreach (string UnistKey in UninstKeys)
      {
        Utils.UninstallMSI(UnistKey);
      }

      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = false;
      result.state = CheckState.REMOVED;

      //MP 0.2.3.0 RC3
      if (Utils.CheckUninstallString("MediaPortal 0.2.3.0 RC3", false) != null)
        result.state = CheckState.NOT_REMOVED;

      string[] UninstKeys = {"{87819CFA-1786-484D-B0DE-10B5FBF2625D}",  //MP < 0.2.3.0 RC3
                             "{4B738773-EE07-413D-AFB7-BB0AB04A5488}",  //TVServer MSI
                             "{F7444E89-5BC0-497E-9650-E50539860DE0}",  //TVClient (old MSI clsid)
                             "{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}"}; //TVClient (new MSI clsid)

      foreach (string UnistKey in UninstKeys)
      {
        if (Utils.CheckUninstallString(UnistKey, false) != null)
          result.state = CheckState.NOT_REMOVED;
      }

      return result;
    }

  }
}