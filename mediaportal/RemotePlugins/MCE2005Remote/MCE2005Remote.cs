/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Devices;

namespace MediaPortal.InputDevices
{
	/// </summary>
	public class MCE2005Remote
	{
		#region constants
		const int WM_INPUT                       = 0x00FF;
		const int WM_KEYDOWN                     =0x0100;
		const int WM_APPCOMMAND                   =0x0319;
		const int APPCOMMAND_BROWSER_BACKWARD       =1;
		const int APPCOMMAND_BROWSER_FORWARD        =2;
		const int APPCOMMAND_BROWSER_REFRESH        =3;
		const int APPCOMMAND_BROWSER_STOP           =4;
		const int APPCOMMAND_BROWSER_SEARCH         =5;
		const int APPCOMMAND_BROWSER_FAVORITES      =6;
		const int APPCOMMAND_BROWSER_HOME           =7;
		const int APPCOMMAND_VOLUME_MUTE            =8;
		const int APPCOMMAND_VOLUME_DOWN            =9;
		const int APPCOMMAND_VOLUME_UP              =10;
		const int APPCOMMAND_MEDIA_NEXTTRACK        =11;
		const int APPCOMMAND_MEDIA_PREVIOUSTRACK    =12;
		const int APPCOMMAND_MEDIA_STOP             =13;
		const int APPCOMMAND_MEDIA_PLAY_PAUSE       =14;
		const int APPCOMMAND_LAUNCH_MAIL            =15;
		const int APPCOMMAND_LAUNCH_MEDIA_SELECT    =16;
		const int APPCOMMAND_LAUNCH_APP1            =17;
		const int APPCOMMAND_LAUNCH_APP2            =18;
		const int APPCOMMAND_BASS_DOWN              =19;
		const int APPCOMMAND_BASS_BOOST             =20;
		const int APPCOMMAND_BASS_UP                =21;
		const int APPCOMMAND_TREBLE_DOWN            =22;
		const int APPCOMMAND_TREBLE_UP              =23;
		const int APPCOMMAND_MICROPHONE_VOLUME_MUTE =24;
		const int APPCOMMAND_MICROPHONE_VOLUME_DOWN =25;
		const int APPCOMMAND_MICROPHONE_VOLUME_UP   =26;
		const int APPCOMMAND_HELP                   =27;
		const int APPCOMMAND_FIND                   =28;
		const int APPCOMMAND_NEW                    =29;
		const int APPCOMMAND_OPEN                   =30;
		const int APPCOMMAND_CLOSE                  =31;
		const int APPCOMMAND_SAVE                   =32;
		const int APPCOMMAND_PRINT                  =33;
		const int APPCOMMAND_UNDO                   =34;
		const int APPCOMMAND_REDO                   =35;
		const int APPCOMMAND_COPY                   =36;
		const int APPCOMMAND_CUT                    =37;
		const int APPCOMMAND_PASTE                  =38;
		const int APPCOMMAND_REPLY_TO_MAIL          =39;
		const int APPCOMMAND_FORWARD_MAIL           =40;
		const int APPCOMMAND_SEND_MAIL              =41;
		const int APPCOMMAND_SPELL_CHECK            =42;
		const int APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE    =43;
		const int APPCOMMAND_MIC_ON_OFF_TOGGLE      =44;
		const int APPCOMMAND_CORRECTION_LIST        =45;
		const int APPCOMMAND_MEDIA_PLAY             =46;
		const int APPCOMMAND_MEDIA_PAUSE            =47;
		const int APPCOMMAND_MEDIA_RECORD           =48;
		const int APPCOMMAND_MEDIA_FAST_FORWARD     =49;
		const int APPCOMMAND_MEDIA_REWIND           =50;
		const int APPCOMMAND_MEDIA_CHANNEL_UP       =51;
		const int APPCOMMAND_MEDIA_CHANNEL_DOWN     =52;

		const int RIM_TYPEMOUSE       =0;
		const int RIM_TYPEKEYBOARD    =1;
		const int RIM_TYPEHID         =2;
 
		const int RID_INPUT               =0x10000003;
		const int RID_HEADER              =0x10000005;
		#endregion
		#region structures
		public struct RAWINPUTDEVICE 
		{
			public ushort usUsagePage;
			public ushort usUsage;
			public uint dwFlags;
			public IntPtr hwndTarget;
		} ;
     
		public struct RAWINPUTHEADER 
		{
			public uint dwType;
			public uint dwSize;
			public IntPtr hDevice;
			public ushort wParam;
		} 
		public struct RAWHID 
		{
			public uint dwSizeHid;
			public uint dwCount;
			public byte RawData1;
			public byte RawData2;
			public byte RawData3;
		} 
		public struct RAWINPUTHID 
		{ 
			public RAWINPUTHEADER    header; 
			public RAWHID      hid; 
		} 
		#endregion
		#region imports


		[DllImport("user32.dll", SetLastError=true)]
		extern static int GetRawInputData(IntPtr rawInput, int uiCmd, IntPtr pData, ref int dwSize, int dwHdrSize);
    
		[DllImport("User32.dll",EntryPoint="RegisterRawInputDevices",SetLastError=true)]
		public extern static bool RegisterRawInputDevices(
			[In] RAWINPUTDEVICE[] pRawInputDevices,
			[In] uint uiNumDevices,
			[In] uint cbSize);  

		[DllImport("user32")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		
		[DllImport("user32")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32")]
		private static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32")] 
		private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("user32")]
		private static extern bool AttachThreadInput(int nThreadId, int nThreadIdTo, bool bAttach);
		
		[DllImport("user32")]
		private static extern int GetWindowThreadProcessId(IntPtr hWnd, int unused);

		[DllImport("user32")]
		static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32")]
		static extern bool IsIconic(IntPtr hWnd);

		#endregion

		bool RemoteFound=false;
		bool RemoteEnabled=false;
		bool USAModel=false;

		public MCE2005Remote()
		{
		}
    
		public void Init(IntPtr hwnd)
		{
			if (RemoteFound) return;// already registered
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				RemoteEnabled= xmlreader.GetValueAsBool("remote", "mce2005", false);
				USAModel= xmlreader.GetValueAsBool("remote", "USAModel", false);
				if (!RemoteEnabled) return;
				if (USAModel) Log.Write("Using USA MCE 2005 remote");
				else  Log.Write("Using European MCE 2005 remote");
			}

			try
			{
				// register for Start button clicks for non-MCE XP users
				Remote.Click += new RemoteEventHandler(OnRemoteClick);

				//register device
				RAWINPUTDEVICE[] rid1= new RAWINPUTDEVICE[1];

				rid1[0].usUsagePage = 0xFFBC;
				rid1[0].usUsage = 0x88;
				rid1[0].dwFlags = 0;
				rid1[0].hwndTarget=hwnd;

				Log.Write("Register for MCE2005 remote#1");
				bool Success=RegisterRawInputDevices(rid1, (uint)rid1.Length,(uint)Marshal.SizeOf(rid1[0]));
				if (Success) 
				{
					Log.Write("Registered#1");
					RemoteFound=true;
				}

				Log.Write("Register for MCE2005 remote#2");
				rid1[0].usUsagePage = 0x0C;
				rid1[0].usUsage = 0x01;
				rid1[0].dwFlags = 0;
				rid1[0].hwndTarget=hwnd;
				Success=RegisterRawInputDevices(rid1, (uint)rid1.Length,(uint)Marshal.SizeOf(rid1[0]));
				if (Success) 
				{
					Log.Write("Registered#2");
					RemoteFound=true;
				}
				Process[] myProcesses;
        
				// kill ehtray.exe since that program catches the mce remote keys
				// and will start mce 2005
				myProcesses = Process.GetProcesses();
				foreach(Process myProcess in myProcesses)
				{
					if (myProcess.ProcessName.ToLower().Equals("ehtray"))
					{
						try
						{
							myProcess.Kill();
						}
						catch(Exception){}
					}
				}
				if (RemoteFound) return;
			}
			catch (Exception)
			{
			}
			Log.Write("Failed, MCE2005 remote not installed?");
		}

		public static void DeInit()
		{
		}
    
		public bool WndProc(ref Message msg, out Action action,out char key, out Keys keyCode)
		{
			keyCode=Keys.A;
			key=(char)0;
			action=null;
			if (!RemoteFound) return false;
			if (!RemoteEnabled) return false;
			int wparam=0;
			int lparam=0;
			try
			{
				if (msg.WParam!=IntPtr.Zero) wparam=msg.WParam.ToInt32();
			}
			catch(Exception){}
			try
			{
				if (msg.LParam!=IntPtr.Zero) lparam=msg.LParam.ToInt32();
			}
			catch(Exception){}

			if(msg.Msg == WM_APPCOMMAND)
			{
				switch((lparam >> 16) & ~0xF000)
				{
					case 0x8:
					case 0x9:
					case 0xA:
						// we simply want to consume these messages to prevent the OS from handling them
						Log.Write("MCERemote2005.WndProc: Consuming WM_APPCOMMAND");
						return true;
				}
			}

			if (msg.Msg==WM_INPUT)
			{
				RAWINPUTHID header = new RAWINPUTHID ();
				int uiSize=0;
				int err=GetRawInputData( msg.LParam,RID_INPUT,IntPtr.Zero,ref uiSize,Marshal.SizeOf(typeof(RAWINPUTHEADER)));
        
				IntPtr ptrStruct=Marshal.AllocCoTaskMem(uiSize);
				Marshal.StructureToPtr(header,ptrStruct,false);
				err=GetRawInputData( msg.LParam,RID_INPUT,ptrStruct,ref uiSize,Marshal.SizeOf(typeof(RAWINPUTHEADER)));
				header=(RAWINPUTHID)Marshal.PtrToStructure(ptrStruct,typeof(RAWINPUTHID));

				switch(header.hid.RawData2)
				{
					case 0x9: // info
						if (header.hid.RawData3==2)
						{
							if (GUIGraphicsContext.IsFullScreenVideo)                   
							{
								//pop up OSD during fullscreen video or live tv (even without timeshift)
								action=new Action(Action.ActionType.ACTION_SHOW_OSD,0,0);
								return true;
							}

							// Pop up info display
							action=new Action(Action.ActionType.ACTION_SHOW_INFO,0,0);
						}
						else
						{
							//							Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
							return false;
						}
						break;
          
					case 0x4B://DVD angle
						break;
          
					case 0x4C://DVD audio
						break;
          
					case 0x24://DVD menu
						if (header.hid.RawData3==0)
						{
							if (g_Player.Playing && g_Player.IsDVD)
							{
								action = new Action(Action.ActionType.ACTION_DVD_MENU,0,0);  
							}
							else 
							{
								action = new Action(Action.ActionType.ACTION_CONTEXT_MENU,0,0); 
							}
						}
						else if (header.hid.RawData3==2)
						{
							if(InputDevices.LastHidRequest == AppCommands.BrowserBackward && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
								return true;

							InputDevices.LastHidRequest = AppCommands.BrowserBackward;

							keyCode=Keys.Escape;
						}
						else
						{
							//							Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
							return false;
						}
						break;
          
					case 0x4D://DVD subtitle
						break;
          
					case 0x8D://TV guide
						GUIMessage msgtv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TVGUIDE,0,null);
						GUIWindowManager.SendThreadMessage(msgtv);
						break;

					case 0x46://(yellow/My tv  on USA model)
						//show context menu
						action = new Action(Action.ActionType.ACTION_CONTEXT_MENU,0,0); 
						break;
					
					case 0x25://My tv (or LiveTV on USA model)
						GUIMessage msgtv2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TV,0,null);
						GUIWindowManager.SendThreadMessage(msgtv2);
						break;
          
          
					case 0x47://My Music (green on USA model)
						GUIMessage msgMusic = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_MUSIC_FILES,0,null);
						GUIWindowManager.SendThreadMessage(msgMusic);
						break;
          
					case 0x49://My Pictures (blue on USA model)
						if (USAModel)
						{
							//TV Change aspect ratio (Blue button)
							action = new Action(Action.ActionType.ACTION_ASPECT_RATIO,0,0);  
						}
						else
						{
							GUIMessage msgPics = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_PICTURES,0,null);
							GUIWindowManager.SendThreadMessage(msgPics);
						}
						break;
          
					case 0x4A://My Video (red on USA model)
						if (USAModel)
						{
							//TV show GUI/fullscreen (Red button)
							action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);  
						}
						else
						{
							GUIMessage msgVids = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_VIDEOS,0,null);
							GUIWindowManager.SendThreadMessage(msgVids);
						}
						break;
          
					case 0x5B://TV show GUI/fullscreen (Red button)
						if (header.hid.RawData3==0)
						{
							action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);  
						}
						else
						{
							//							Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
							return false;
						}
						break;
          
					case 0x5C: //green home
						GUIMessage msgHome = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_HOME,0,null);
						GUIWindowManager.SendThreadMessage(msgHome);
						break;

					case 0x5D://Show context menu (Yellow button)
						if (header.hid.RawData3==0)
						{
							action = new Action(Action.ActionType.ACTION_CONTEXT_MENU,0,0);  
						}
						else
						{
							//							Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
							return false;
						}
						break;
          
					case 0x5E: //TV Change aspect ratio (Blue button)
						if (header.hid.RawData3==0)
						{
							action = new Action(Action.ActionType.ACTION_ASPECT_RATIO,0,0);  
						}
						else
						{
							//							Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
							return false;
						}
						break;
          
					case 0x80://OEM1
						break;
          
					case 0x81://OEM2
						break;
          
					case 0x48://Recorded TV
						GUIMessage msgTvRec = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_RECORDEDTV,0,null);
						GUIWindowManager.SendThreadMessage(msgTvRec);

						break;
          
					case 0x82://Standbye
						break;
          
					case 0x0d:// home
						GUIMessage msgHome2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_HOME,0,null);
						GUIWindowManager.SendThreadMessage(msgHome2);
						break;
          
					case 0xB0: //play

						if(InputDevices.LastHidRequest == AppCommands.MediaPlay && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaPlay;

						action=new Action(Action.ActionType.ACTION_PLAY,0,0);
						break;
         
					case 0xB1: //pause

						if(InputDevices.LastHidRequest == AppCommands.MediaPause && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaPause;

						action=new Action(Action.ActionType.ACTION_PAUSE,0,0);
						break;
          
					case 0xB2: //record
						
						if(InputDevices.LastHidRequest == AppCommands.MediaRecord && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaRecord;

						action=new Action(Action.ActionType.ACTION_RECORD,0,0);
						break;
          
					case 0xB4: //rewind

						if(InputDevices.LastHidRequest == AppCommands.MediaRewind && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaRewind;
						
						action=new Action(Action.ActionType.ACTION_REWIND,0,0);
						break;
          
					case 0xB3: //fast forward

						if(InputDevices.LastHidRequest == AppCommands.MediaFastForward && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaFastForward;

						action=new Action(Action.ActionType.ACTION_FORWARD,0,0);
						break;
          
					case 0xB5: //next
						
						if(InputDevices.LastHidRequest == AppCommands.MediaNextTrack && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaNextTrack;

						if ((g_Player.Playing) && (g_Player.IsDVD))
							action=new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
						else
							action=new Action(Action.ActionType.ACTION_NEXT_ITEM,0,0);
						break;
          
					case 0xB6: //previous

						if(InputDevices.LastHidRequest == AppCommands.MediaPreviousTrack && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaPreviousTrack;

						if ((g_Player.Playing) && (g_Player.IsDVD))
							action=new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
						else
							action=new Action(Action.ActionType.ACTION_PREV_ITEM,0,0);
						break;
         
					case 0xb7: //stop

						if(InputDevices.LastHidRequest == AppCommands.MediaStop && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaStop;

						action=new Action(Action.ActionType.ACTION_STOP,0,0);
						break;
          
					case 0xe9: //volume+
						
						if(InputDevices.LastHidRequest == AppCommands.VolumeUp && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.VolumeUp;

						action=new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
						break;
          
					case 0xea: //volume-

						if(InputDevices.LastHidRequest == AppCommands.VolumeDown && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.VolumeDown;

						action=new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
						break;
          
					case 0x9c: //channel+

						if(InputDevices.LastHidRequest == AppCommands.MediaChannelUp && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaChannelUp;
	
						if (GUIGraphicsContext.IsFullScreenVideo)
							action=new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
						else
							action=new Action(Action.ActionType.ACTION_PAGE_UP,0,0);
						break;
          
					case 0x9d: //channel-

						if(InputDevices.LastHidRequest == AppCommands.MediaChannelDown && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.MediaChannelDown;

						if (GUIGraphicsContext.IsFullScreenVideo)
							action=new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
						else
							action=new Action(Action.ActionType.ACTION_PAGE_DOWN,0,0);
						break;
          
					case 0xe2: //mute
						
						if(InputDevices.LastHidRequest == AppCommands.VolumeMute && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
							return true;

						InputDevices.LastHidRequest = AppCommands.VolumeMute;

						action=new Action(Action.ActionType.ACTION_VOLUME_MUTE,0,0);
						break;

					case 0x5A: //teletext
						if (g_Player.IsTV)
						{
							if (GUIGraphicsContext.IsFullScreenVideo)
							{
								// Activate fullscreen teletext
								GUIMessage msgTxt1 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT,0,null);
								GUIWindowManager.SendThreadMessage(msgTxt1);
							}
							else
							{
								// Activate teletext in window
								GUIMessage msgTxt2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TELETEXT,0,null);
								GUIWindowManager.SendThreadMessage(msgTxt2);
							}
						}
						break;
					default:
						//						Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
						return false;
				}
				return true;
			}
			return false;
		}

		#region Green Button & other problematic buttons

		void OnRemoteClick(RemoteButton button)
		{
			if(button == RemoteButton.MyTV)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TV,0,null);
				GUIWindowManager.SendThreadMessage(msg);
				return;
			}

			if(button == RemoteButton.AspectRatio)
			{
				GUIGraphicsContext.OnAction(new Action(Action.ActionType.ACTION_ASPECT_RATIO, 0, 0));
				return;
			}

			if(button == RemoteButton.MyRadio)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_RADIO,0,null);
				GUIWindowManager.SendThreadMessage(msg);
				return;
			}

			if(button == RemoteButton.Messenger)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_MSN,0,null);
				GUIWindowManager.SendThreadMessage(msg);
				return;
			}

			if(button != RemoteButton.Start)
				return;

			IntPtr window = GUIGraphicsContext.ActiveForm;

			if(window == IntPtr.Zero)
				window = Process.GetCurrentProcess().MainWindowHandle;

			if(window == IntPtr.Zero)
			{
				Log.Write("MCE2005Remote.OnGreenButton: Invalid window handle");
				return;
			}

			if(IsIconic(window) || !IsWindowVisible(window))
				ShowWindowAsync(window, 0x9);

			if(GetForegroundWindow() != window)
			{
				SetForegroundWindow(window, true);
				return;
			}

			if(GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_HOME)
			{
				// only the main thread can correctly handle switching to the home window
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_HOME, 0, null);
				GUIWindowManager.SendThreadMessage(msg);
			}
		}

		static bool SetForegroundWindow(IntPtr window, bool force)
		{
			IntPtr windowForeground = GetForegroundWindow(); 

			if(window == windowForeground || SetForegroundWindow(window))
				return true;

			if(force == false)
				return false;
			
			if(windowForeground == IntPtr.Zero)
				return false;

			// if we don't attach successfully to the windows thread then we're out of options
      if (!AttachThreadInput(System.Threading.Thread.CurrentThread.ManagedThreadId, GetWindowThreadProcessId(windowForeground, 0), true))
				return false;

			SetForegroundWindow(window);
			BringWindowToTop(window);

      AttachThreadInput(System.Threading.Thread.CurrentThread.ManagedThreadId, GetWindowThreadProcessId(windowForeground, 0), false);

			// we've done all that we can so base our return value on whether we have succeeded or not
			return (GetForegroundWindow() == window);
		}

		#endregion Green Button
	}
}
