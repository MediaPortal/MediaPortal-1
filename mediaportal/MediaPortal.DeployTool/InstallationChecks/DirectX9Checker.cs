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

namespace MediaPortal.DeployTool.InstallationChecks
{
  class DirectX9Checker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "DirectX 9";
    }

    public bool Download()
    {
      return false;
    }
    public bool Install()
    {
      return false;
    }
    public bool UnInstall()
    {
      return false;
    }
    public CheckResult CheckStatus()
    {
      RegistryKey key=Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\DirectX");
      if (key == null)
        return CheckResult.NOT_INSTALLED;
      string version=(string)key.GetValue("Version");
      key.Close();
      if (version == "4.09.0000.0900" || version == "4.09.00.0900" || version == "4.09.0000.0901" || version == "4.09.00.0901" || version == "4.09.0000.0902" || version == "4.09.00.0902" || version == "4.09.0000.0904" || version == "4.09.00.0904")
        return CheckResult.INSTALLED;
      else
        return CheckResult.VERSION_MISMATCH;
    }
  }
}
