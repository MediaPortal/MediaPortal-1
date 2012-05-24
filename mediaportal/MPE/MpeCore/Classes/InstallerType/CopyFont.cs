#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Drawing;
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
    private static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr SendMessageTimeout(
      IntPtr hWnd,
      uint Msg,
      UIntPtr wParam,
      IntPtr lParam,
      SendMessageTimeoutFlags fuFlags,
      uint uTimeout,
      out UIntPtr lpdwResult
      );

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_FONTCHANGE = 0x001D;
    private IntPtr HWND_BROADCAST = new IntPtr(0xffff);

    [Flags]
    enum SendMessageTimeoutFlags : uint
    {
      SMTO_NORMAL = 0x0,
      SMTO_BLOCK = 0x1,
      SMTO_ABORTIFHUNG = 0x2,
      SMTO_NOTIMEOUTIFNOTHUNG = 0x8
    }

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
      string fontFilePath = fileItem.ExpandedDestinationFilename;
      if (AddFontResource(fontFilePath) != 0)
      {
        bool result;
        //SendMessageTimeout(HWND_BROADCAST, WM_FONTCHANGE, UIntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NOTIMEOUTIFNOTHUNG, 10000, out result);
        result = PostMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);

        if (result)
          WriteProfileString("fonts", Path.GetFileNameWithoutExtension(fontFilePath) + " (TrueType)",
                           Path.GetFileName(fontFilePath));
        //TODO: Log error message or somehow inform user
        //else
        //{
          //int err = Marshal.GetLastWin32Error();
        //}
      }
    }
  }
}