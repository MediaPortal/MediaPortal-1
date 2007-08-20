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
using System.Windows.Forms;
using System.Diagnostics;

namespace MediaPortal.DeployTool
{
  class DirectX9Checker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "DirectX 9c";
    }

    public bool Download()
    {
      ManualDownload dlg = new ManualDownload();
      DialogResult result = dlg.ShowDialog(Utils.GetDownloadURL("DirectX9c"), Utils.GetDownloadFile("DirectX9c"), Application.StartupPath + "\\deploy");
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string exe = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("DirectX9c");
      Process setup = Process.Start(exe,"/q /t:\""+Path.GetTempPath()+"\\directx9c\"");
      setup.WaitForExit();
      return true;
    }
    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("DirectX9c"));
      RegistryKey key=Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\DirectX");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        string version = (string)key.GetValue("Version");
        key.Close();
        if (version == "4.09.0000.0904" || version == "4.09.00.0904")
          result.state = CheckState.INSTALLED;
        else
          result.state = CheckState.VERSION_MISMATCH;
      }
      return result;
    }
  }
}
