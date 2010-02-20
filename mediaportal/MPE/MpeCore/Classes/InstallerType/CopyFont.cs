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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MpeCore.Interfaces;

namespace MpeCore.Classes.InstallerType
{
  internal class CopyFont : CopyFile, IInstallerTypeProvider
  {
    [DllImport("gdi32")]
    public static extern int AddFontResource(string lpFileName);

    [DllImport("gdi32")]
    public static extern int RemoveFontResource(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]

    static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

    [DllImport("user32.dll")]
    public static extern int SendMessage(int hWnd, // handle to destination window 
                                         uint Msg, // message 
                                         int wParam, // first message parameter 
                                         int lParam // second message parameter 
      );

    public new string Name
    {
      get { return "CopyFont"; }
    }

    public override string Description
    {
      get { return "Copy the file to specified location and register to system fonts\nUse for installing %Fonts% path."; }
    }

    public new string GetZipEntry(FileItem fileItem)
    {
      return string.Format("Installer{{Font}}\\{{{0}}}-{1}", Guid.NewGuid(), Path.GetFileName(fileItem.LocalFileName));
    }

    public new void Install(PackageClass packageClass, FileItem fileItem)
    {
      base.Install(packageClass, fileItem);
      int res = AddFontResource(fileItem.ExpandedDestinationFilename);
    }
  }
}