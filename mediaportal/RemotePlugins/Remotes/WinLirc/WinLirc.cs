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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Threading;
using System.Text;
using System.IO;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.InputDevices
{
	/// <summary>
	/// Summary description for WinLirc.
	/// </summary>
	public class WinLirc
	{
		public const Int32 TOGGLE_HIDEWINDOW = 0x080;
		public const Int32 TOGGLE_UNHIDEWINDOW = 0x040;
		public const Int32 HWND_TOPMOST    = -1;
		public const Int32 HWND_NOTOPMOST  = -2;

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr FindWindow(
			[MarshalAs(UnmanagedType.LPTStr)] string lpClassName,
			[MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);
		
		[StructLayout(LayoutKind.Sequential)]
			struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;
			[MarshalAs(UnmanagedType.LPStr)] public string lpData;
		}

		[DllImport("User32")]
		private static extern int SetForegroundWindow(IntPtr hwnd);

		// Activates a window
		[DllImportAttribute("User32.DLL")] 
		private static extern bool ShowWindow(IntPtr hWnd,int nCmdShow); 

		[DllImport("user32.dll")]
		static extern IntPtr GetActiveWindow();

		private const int SW_SHOW = 5; 
		private const int SW_RESTORE = 9; 
		const int WM_COPYDATA = 0x004a; 

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam,
			[In()] ref COPYDATASTRUCT lParam);
		
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static extern int SendMessageA(IntPtr hwnd, int wMsg, int wParam,
			int lParam);

		private const string TAB = "	";
		protected IntPtr m_hwnd = IntPtr.Zero;
		protected string m_windowName;
		protected string m_remote = "";
		protected string m_repeat = "0";
		protected string m_pathtowinlirc = "0";
		protected bool m_bEnabled = false;
		//protected bool m_bMultipleRemotes = true;
		//protected bool m_bNeedsEnter = false;
		protected bool m_bInitRetry = true;
		protected int m_IRdelay = 300;
		

		public WinLirc()
    {

			Init();
		}

		public bool Init()
		{
			Log.Info("Initialising WinLirc...");
			//load settings
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				m_bEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
				if(m_bEnabled == false)
					return true;
				m_pathtowinlirc = xmlreader.GetValueAsString("WINLIRC", "winlircpath", "");
				string delay = xmlreader.GetValueAsString("WINLIRC", "delay", "300") ;
				try
				{
					m_IRdelay=Int32.Parse(delay);
					if (m_IRdelay< 0 || m_IRdelay >=1000) m_IRdelay=300;
				}
				catch(Exception)
				{
					m_IRdelay=300;
				}

				//m_bMultipleRemotes = xmlreader.GetValueAsString("WINLIRC", "use_multiple_remotes", "true") == "true";
				//m_remote = xmlreader.GetValueAsString("WINLIRC", "remote", "") ;
				//m_repeat = xmlreader.GetValueAsString("WINLIRC", "repeat", "0");
				//m_bNeedsEnter = xmlreader.GetValueAsString("WINLIRC", "needs_enter", "false") == "true";
			}


			//find winlirc
			m_windowName = "WinLIRC";
			m_hwnd = FindWindow(null, m_windowName);

			//check we found it - if not, start it!
			if(m_hwnd.ToInt32() <= 0) // try to find it and start it since it's not found
			{
				Log.Info("WinLirc window not found, starting WinLirc");
				IntPtr mpHwnd = GetActiveWindow();//Get MP
				StartWinLirc(m_pathtowinlirc);//Start Winlirc
				ShowWindow(mpHwnd,SW_RESTORE); //restore MP		
				SetForegroundWindow(mpHwnd);//restore MP
			}
			if(m_hwnd.ToInt32() > 0)
			{
				Log.Info("Winlirc OK");
				return true;
			}
			Log.Info("Winlirc process not found");
			return false;
		}

		public bool StartWinLirc(string exeName)
		{
			if (exeName==null) return false;
			if (exeName==string.Empty) return false;
			ProcessStartInfo psI = new ProcessStartInfo(exeName, "");
			Process newProcess = new Process();

			try
			{
				newProcess.StartInfo.FileName = exeName;
				newProcess.StartInfo.Arguments = "";
				newProcess.StartInfo.UseShellExecute = true;
				newProcess.StartInfo.CreateNoWindow = true;
				//newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
				newProcess.Start();

				for (int i = 0; i < 15; i++) // wait for up to 3 seconds for WinLirc to start.
				{
					Thread.Sleep(200);
					m_hwnd = FindWindow(null, m_windowName);
					if ( m_hwnd.ToInt32() > 0 ) // window handle was found
						break;
				}
			}
			catch(Exception)
			{
				Log.Info("Unable to start WinLIRC from {0}", exeName);
				return false;
			}
			return true;
		}

		public void ChangeTunerChannel(string channel_data) 
		{				
			//leave is not enabled
			if(m_bEnabled == false)
				return;

			try
			{
				if (channel_data==null) return;
				if (channel_data==string.Empty) return;
				if(m_hwnd.ToInt32() == 0)
				{
					Log.Info("WinLirc HWND is invalid. Check WinLirc is running");
					return;
				}

				//by default, use the remote set on config page
				string IRData;

				//our copy struct
				COPYDATASTRUCT cds;
				
				string[] sets = channel_data.Split("|".ToCharArray()); 
				foreach(string command in sets)
				{
					//make up the Channel Change parts...
					//channelparts[0] will be name of remote
					//channelparts[1] will be repeat count
					//channelparts[2] will be code(s)
					string[] channelparts = {m_remote,m_repeat,command}; //default to using m_remote:m_repeat:command
					
					//now if channel_data has a ':', split that & use it instead!
					//NOTE: channel_data should be Remote:Repeat:Codes
					channelparts = command.Split(":".ToCharArray(),3);

					if(channelparts.Length != 3)
					{
						Log.Info("WinLirc: '" + command + "' is invalid.  Check External Channel follows the correct format (Remote:Repeat:Code 1,Code 2,Code n)");
						continue;
					}

					Log.Info("WinLirc ChangeTunerChannel: Remote; " + channelparts[0] + " Channel; " + channelparts[2]);

					//go thru chan numbers / commands & output to winLIRC
					string[] Ops = channelparts[2].Split(",".ToCharArray());
					foreach(string s in Ops)
					{
						if(s == "") 
							continue;
						//IRData must be "remote+TAB+code+TAB+repeatcount"
						IRData = channelparts[0] + TAB + s + TAB + m_repeat;
						cds.dwData = (IntPtr) 0;
						cds.lpData = IRData;
						cds.cbData = IRData.Length + 1;
						SendMessage(m_hwnd, WM_COPYDATA, 0, ref cds);
						Thread.Sleep(m_IRdelay);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Info("Exception occured in winlirc plugin:{0}", ex.ToString());
			}
		}


	}
}
