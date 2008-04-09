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
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  class VCRedistChecker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "Microsoft Visual C++ 2005 SP1 Redistributable";
    }

    public bool Download()
    {
        string prg = "VCRedist";
        string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile(prg);
        DialogResult result;
        result = Utils.RetryDownloadFile(FileName, prg);
        return (result == DialogResult.OK);
    }
    public bool Install()
    {
      Process setup = Process.Start(Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadFile("VCRedist"), "/Q");
      try
      {
          setup.WaitForExit();
          return true;
      }
      catch
      {
          return false;
      }
    }
    public bool UnInstall()
    {
      Process setup = Process.Start("msiexec", "/X{7299052b-02a4-4627-81f2-1818da5d550d}");
      setup.WaitForExit();
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("VCRedist"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
          if (result.needsDownload == false)
              result.state = CheckState.DOWNLOADED;
          else
              result.state = CheckState.NOT_DOWNLOADED;
          return result;
      }
      string ManifestDir = Environment.GetEnvironmentVariable("SystemRoot") + "\\winsxs\\Manifests\\";
      string ManifestCRT = "x86_microsoft.vc80.crt_1fc8b3b9a1e18e3b_8.0.50727.762_none_10b2f55f9bffb8f8.manifest";
      string ManifestMFC = "x86_microsoft.vc80.mfc_1fc8b3b9a1e18e3b_8.0.50727.762_none_0c178a139ee2a7ed.manifest";
      string ManifestATL = "x86_microsoft.vc80.atl_1fc8b3b9a1e18e3b_8.0.50727.762_none_11ecb0ab9b2caf3c.manifest";
      if (File.Exists(ManifestDir + ManifestCRT) && File.Exists(ManifestDir + ManifestMFC) && File.Exists(ManifestDir + ManifestATL))
          result.state = CheckState.INSTALLED;
      else
          result.state = CheckState.NOT_INSTALLED;
      return result;
    }
  }
}
