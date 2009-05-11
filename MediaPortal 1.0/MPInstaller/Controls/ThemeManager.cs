/*
 * Copyright © 2005, Patrik Bohman
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 *    - Redistributions of source code must retain the above copyright notice, 
 *      this list of conditions and the following disclaimer.
 * 
 *    - Redistributions in binary form must reproduce the above copyright notice, 
 *      this list of conditions and the following disclaimer in the documentation 
 *      and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
 * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, 
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
 * OF SUCH DAMAGE.
 */

using System;
using System.Runtime.InteropServices;  
using System.Drawing;


namespace Pabo.MozBar
{
	
	[StructLayout(LayoutKind.Sequential)]
	public struct DLLVERSIONINFO
	{
		public int cbSize;
		public int dwMajorVersion;
		public int dwMinorVersion;
		public int dwBuildNumber;
		public int dwPlatformID;
	}
	
	/// <summary>
	/// Summary description for ThemeManager.
	/// </summary>
	public class ThemeManager
	{
		
		// Declare functions used in uxTheme.dll and ComCtl32.dll

		[DllImport("uxTheme.dll", EntryPoint="GetThemeColor", ExactSpelling=true, PreserveSig=false, CharSet=CharSet.Unicode )]
		private extern static void GetThemeColor (System.IntPtr hTheme,
			int partID,
			int stateID,
			int propID,
			out int color);

		[DllImport( "uxtheme.dll", CharSet=CharSet.Unicode )]
		private static extern IntPtr OpenThemeData( IntPtr hwnd, string classes );
		
		[DllImport( "uxtheme.dll", EntryPoint="CloseThemeData", ExactSpelling=true, PreserveSig=false, CharSet=CharSet.Unicode) ]
		private static extern int CloseThemeData( IntPtr hwnd );

		[DllImport("uxtheme.dll", EntryPoint="GetWindowTheme", ExactSpelling=true,PreserveSig=false, CharSet=CharSet.Unicode)]
		private static extern int GetWindowTheme(IntPtr hWnd);
		
		[DllImport("uxtheme.dll", EntryPoint="IsThemeActive", ExactSpelling=true,PreserveSig=false, CharSet=CharSet.Unicode)]
		private static extern bool IsThemeActive();
		
		[DllImport("Comctl32.dll", EntryPoint="DllGetVersion", ExactSpelling=true,PreserveSig=false, CharSet=CharSet.Unicode)]
		private static extern int DllGetVersion(ref DLLVERSIONINFO s);

		public ThemeManager()
		{
			  
		}

		public bool _IsAppThemed()
		{
			try
			{
				// Check which version of ComCtl32 thats in use..
				DLLVERSIONINFO version = new DLLVERSIONINFO();
				version.cbSize = Marshal.SizeOf(typeof(DLLVERSIONINFO));
								
				int ret = DllGetVersion(ref version);
				// If MajorVersion > 5 themes are allowed.
				if (version.dwMajorVersion >= 6) 			
					return true;
				else
					return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void _CloseThemeData(IntPtr hwnd)
		{
			try
			{
				CloseThemeData(hwnd);
			}
			catch (Exception)
			{
				
			}
		}
		
		public IntPtr _OpenThemeData(IntPtr hwnd, string classes)
		{
			try
			{
				return OpenThemeData(hwnd, classes );
			}
			catch (Exception)
			{
				return System.IntPtr.Zero;
			}
		}

		public int _GetWindowTheme(IntPtr hwnd)
		{
			try
			{
				return GetWindowTheme(hwnd);
			}
			catch (Exception)
			{
				return -1;
			}

		}

		public bool _IsThemeActive()
		{
			try
			{
				return IsThemeActive();
			}
			catch (Exception)
			{
				return false;
			}
		}

		public Color _GetThemeColor ( IntPtr hTheme, int partID, int stateID,int propID )
		{
			int color;

			try 
			{
				GetThemeColor ( hTheme, partID, stateID, propID, out color );
				return ColorTranslator.FromWin32 ( color );
			}
			catch (Exception) 
			{
				return Color.Empty;
			}
		}
	}
}
