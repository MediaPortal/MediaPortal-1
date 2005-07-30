/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Util
{
	/// <summary>
	/// Summary description for Win32API.
	/// </summary>
	public class Win32API
	{
//    [DllImportAttribute("kernel32", EntryPoint="RtlMoveMemory", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
//    public static extern void CopyMemory(ref KBDLLHOOKSTRUCT Destination, int Source, int Length);

 //   [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
 //   public static extern int GetKeyState(int nVirtKey);

//    [DllImportAttribute("user32", EntryPoint="SetWindowsHookExA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
//    public static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardDelegate lpfn, int hmod, int dwThreadId);
		[DllImport("gdi32.dll", EntryPoint="CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll", EntryPoint="SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hdc,IntPtr bmp);

		[DllImport("gdi32.dll", EntryPoint="DeleteDC")]
		public static extern IntPtr DeleteDC(IntPtr hDc);

    [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref int lParam);

    [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern int UnhookWindowsHookEx(int hHook);

    [DllImportAttribute("user32", EntryPoint="FindWindowA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern int FindWindow([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpWindowName);

    [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern int GetWindow(int hwnd, int wCmd);

    [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern int ShowWindow(int hwnd, int nCmdShow);

    [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern int EnableWindow(int hwnd, int fEnable);

    //Creating the extern function...
    [DllImport("wininet.dll")]
    private extern static bool InternetGetConnectedState( out int Description, int ReservedValue ) ;

		public Win32API()
		{
			//
			// TODO: Add constructor logic here
			//
		}

    //Checks if the computer is connected to the internet...
    public static bool IsConnectedToInternet( )
    {
#if DEBUG
      return true;
#else
      int Desc ;
      return InternetGetConnectedState( out Desc, 0 ) ;
#endif
    }

    public static void Show(string ClassName, string WindowName, bool bVisible)
    {
      int i = FindWindow(ref ClassName, ref WindowName);
      if (bVisible)
      {
        ShowWindow(i, 5);
      }
      else
      {
        ShowWindow(i, 0);
      }
    }

    public static void Enable(string ClassName, string WindowName, bool bEnable)
    {
      int i = FindWindow(ref ClassName, ref WindowName);
      if (bEnable)
      {
        EnableWindow(i, -1);
      }
      else
      {
        EnableWindow(i, 0);
      }
    }

    public static void ShowStartBar(bool bVisible)
    {
			try
			{
				Show("Shell_TrayWnd", "", bVisible);
			}
			catch(Exception){}
    }

    public static void EnableStartBar(bool bEnable)
    {
			try
			{
				Enable("Shell_TrayWnd", "", bEnable);
			}
			catch(Exception){}
    }
  }
}
