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
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class VCRedist2008Checker : IInstallationPackage
  {
    public static string prg = "VCRedist2008";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    public string GetDisplayName()
    {
      return "MS Visual C++ 2008 SP1 Redist (KB973552)";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      Process setup = Process.Start(_fileName, "/Q");
      try
      {
        if (setup != null)
        {
          setup.WaitForExit();
        }
        return true;
      }
      catch
      {
        return false;
      }
    }
    public bool UnInstall()
    {
      Utils.UninstallMSI("{FF66E9F6-83E7-3A3E-AF14-8DE9A809A6A4}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo vcRedistFile = new FileInfo(_fileName);

      if (vcRedistFile.Exists && vcRedistFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      string ManifestDir = Environment.GetEnvironmentVariable("SystemRoot") + "\\winsxs\\Manifests\\";
      //Manifests for Vista/2008/Windows7
      const string ManifestCRT_Vista = "x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.4148_none_5090ab56bcba71c2.manifest";
      const string ManifestMFC_Vista = "x86_microsoft.vc90.mfc_1fc8b3b9a1e18e3b_9.0.30729.4148_none_4bf5400abf9d60b7.manifest";
      const string ManifestATL_Vista = "x86_microsoft.vc90.atl_1fc8b3b9a1e18e3b_9.0.30729.4148_none_51ca66a2bbe76806.manifest";
      //Manifests for XP/2003
      const string ManifestCRT_XP = "x86_Microsoft.VC90.CRT_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_d495ac4e.manifest";
      const string ManifestMFC_XP = "x86_Microsoft.VC90.MFC_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_a57c1f53.manifest";
      const string ManifestATL_XP = "x86_Microsoft.VC90.ATL_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_353599c2.manifest";

      if (File.Exists(ManifestDir + ManifestCRT_Vista) && File.Exists(ManifestDir + ManifestMFC_Vista) && File.Exists(ManifestDir + ManifestATL_Vista))
        result.state = CheckState.INSTALLED;
      else if (File.Exists(ManifestDir + ManifestCRT_XP) && File.Exists(ManifestDir + ManifestMFC_XP) && File.Exists(ManifestDir + ManifestATL_XP))
        result.state = CheckState.INSTALLED;
      else
        result.state = CheckState.NOT_INSTALLED;
      return result;
    }
  }
}