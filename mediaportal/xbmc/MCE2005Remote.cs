using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal
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

		[StructLayout(LayoutKind.Sequential)]
			struct DeviceInfoData
		{
			public int				Size;
			public Guid				guidClass;
			public uint				DevInst;
			public uint				Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
			struct DeviceInterfaceData
		{
			public int				Size;
			public Guid				guidClass;
			public uint				Flags;
			public uint				Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
			struct DeviceInterfaceDetailData 
		{
			public int				Size;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
			public string			DevicePath;
		}

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


		[DllImport("hid", SetLastError=true)]
		static extern void HidD_GetHidGuid(ref Guid guid);

		[DllImport("setupapi", SetLastError=true)]
		static extern IntPtr SetupDiGetClassDevs(ref Guid guid, int Enumerator, int hwndParent, int Flags);

		[DllImport("setupapi", SetLastError=true)]
		static extern bool SetupDiEnumDeviceInfo(IntPtr handle, int Index, ref DeviceInfoData deviceInfoData);

		[DllImport("setupapi", SetLastError=true)]
		static extern bool SetupDiEnumDeviceInterfaces(IntPtr handle, ref DeviceInfoData deviceInfoData, ref Guid guidClass, int MemberIndex, ref DeviceInterfaceData deviceInterfaceData);

		[DllImport("setupapi", SetLastError=true)]
		static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr handle, ref DeviceInterfaceData deviceInterfaceData, int unused1, int unused2, ref uint requiredSize, int unused3);
		
		[DllImport("setupapi", SetLastError=true)]
		static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr handle, ref DeviceInterfaceData deviceInterfaceData, ref DeviceInterfaceDetailData deviceInterfaceDetailData, uint detailSize, int unused1, int unused2);
		

    [DllImport("user32.dll", SetLastError=true)]
    extern static int GetRawInputData(IntPtr rawInput, int uiCmd, IntPtr pData, ref int dwSize, int dwHdrSize);
    
    [DllImport("User32.dll",EntryPoint="RegisterRawInputDevices",SetLastError=true)]
    public extern static bool RegisterRawInputDevices(
                                                      [In] RAWINPUTDEVICE[] pRawInputDevices,
                                                      [In] uint uiNumDevices,
                                                      [In] uint cbSize);  

		[DllImport("kernel32", SetLastError=true)]
		static extern unsafe IntPtr CreateFile(string FileName, [MarshalAs(UnmanagedType.U4)] FileAccess DesiredAccess, [MarshalAs(UnmanagedType.U4)] FileShare ShareMode, uint SecurityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode CreationDisposition, FileFlag FlagsAndAttributes, int hTemplateFile);

		[DllImport("kernel32", SetLastError=true)]
		static extern unsafe bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32", SetLastError=true)]
		static extern int GetLastError();

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
		private static extern IntPtr FindWindow(string strWindowClass, string strWindowName);

		[DllImport("user32")]
		static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32")]
		static extern bool IsIconic(IntPtr hWnd);

		#endregion

		enum FileFlag
		{
			Overlapped = 0x40000000,
		}

		const int					SW_RESTORE = 9;
    bool RemoteFound=false;
		bool RemoteEnabled=false;
		bool USAModel=false;
		FileStream					m_streamRead;
		byte[]						m_bufferRead = new byte[128];

		public MCE2005Remote()
		{
		}
    
    public void Init(IntPtr hwnd)
    {
      if (RemoteFound) return;// already registered
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				RemoteEnabled= xmlreader.GetValueAsBool("remote", "mce2005", false);
				USAModel= xmlreader.GetValueAsBool("remote", "USAModel", false);
				if (!RemoteEnabled) return;
				if (USAModel) Log.Write("Using USA MCE 2005 remote");
				else  Log.Write("Using European MCE 2005 remote");
			}

      try
      {
				Start();
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
/*
      if (msg.Msg==WM_APPCOMMAND)
      {
        action=new Action(Action.ActionType.ACTION_INVALID,0,0);
        lparam>>=32;
        Log.Write("lp:{0:X}",lparam);
        switch (lparam)
        {
          case 0x1032: //back
            key=(char)27;
            break;
          case 0x102E: //play
            action.wID=Action.ActionType.ACTION_PLAY;
            break;
          case 0x1031: //forward
            action.wID=Action.ActionType.ACTION_FORWARD;
            break;
          case 0x102F: //pause
            action.wID=Action.ActionType.ACTION_PAUSE;
            break;
          case 0xD: //stop
            action.wID=Action.ActionType.ACTION_STOP;
            break;
          case 0xC: //repeat
            //action.wID=Action.ActionType.;
            break;
          case 0xB: //next
            action.wID=Action.ActionType.ACTION_NEXT_ITEM;
            break;
          case 0x1: //previous
            action.wID=Action.ActionType.ACTION_PREV_ITEM;
            break;
          case 0x1033://channel +
            action.wID=Action.ActionType.ACTION_NEXT_CHANNEL;
            break;
          case 0x1034://channel -
            action.wID=Action.ActionType.ACTION_PREV_CHANNEL;
            break;
          case 0x8://mute
            break;
          case 0x1030://record
            action.wID=Action.ActionType.ACTION_RECORD;
            break;
          default:
            Log.Write("unknown wm_appcommand:{0:X}",lparam);
          break;
        }
        return true;
      }

      if (msg.Msg==WM_KEYDOWN)
      {
        Log.Write("wm_keydown:{0:X} {1:X}",wparam,lparam); 
        switch (wparam)
        {
          case 0xd://OK/enter
            break;
          case 0x38://*
            break;
          case 0x33://#
            break;
        }
      }
*/ 
			if (msg.Msg==WM_INPUT)
      {
        RAWINPUTHID header = new RAWINPUTHID ();
        int uiSize=0;
        int err=GetRawInputData( msg.LParam,RID_INPUT,IntPtr.Zero,ref uiSize,Marshal.SizeOf(typeof(RAWINPUTHEADER)));
        
        IntPtr ptrStruct=Marshal.AllocCoTaskMem(uiSize);
        Marshal.StructureToPtr(header,ptrStruct,false);
        err=GetRawInputData( msg.LParam,RID_INPUT,ptrStruct,ref uiSize,Marshal.SizeOf(typeof(RAWINPUTHEADER)));
        header=(RAWINPUTHID)Marshal.PtrToStructure(ptrStruct,typeof(RAWINPUTHID));
        /*
        Log.Write("header.dwsize:{0}",header.header.dwSize);
        Log.Write("header.dwType:{0}",header.header.dwType);
        Log.Write("header.hDevice:{0:X}",header.header.hDevice);
        Log.Write("header.wParam:{0:X}",header.header.wParam);
        Log.Write("hid.dwCount:{0:X}",header.hid.dwCount);
        Log.Write("hid.dwSizeHid:{0:X}",header.hid.dwSizeHid);
        Log.Write("hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
        */
        Log.Write("hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);

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
              Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
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
            }
            else if (header.hid.RawData3==2)
            {
              Log.Write("key=27");
              keyCode=Keys.Escape;
            }
            else
            {
              Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
              return false;
            }
            break;
          
					case 0x4D://DVD subtitle
            break;
          
					case 0x8D://TV guide
						  GUIMessage msgtv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TVGUIDE,0,null);
							GUIGraphicsContext.SendMessage(msgtv);
            break;

					case 0x46://(yellow/My tv  on USA model)
						//show context menu
						action = new Action(Action.ActionType.ACTION_CONTEXT_MENU,0,0); 
					break;
					
					case 0x25://My tv (or LiveTV on USA model)
							GUIMessage msgtv2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TV,0,null);
							GUIGraphicsContext.SendMessage(msgtv2);
						break;
          
          
					case 0x47://My Music (green on USA model)
							GUIMessage msgMusic = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_MUSIC_FILES,0,null);
							GUIGraphicsContext.SendMessage(msgMusic);
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
							GUIGraphicsContext.SendMessage(msgPics);
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
							GUIGraphicsContext.SendMessage(msgVids);
						}
						break;
          
					case 0x5B://TV show GUI/fullscreen (Red button)
            if (header.hid.RawData3==0)
            {
              action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);  
            }
            else
            {
              Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
              return false;
            }
            break;
          
					case 0x5C: //green home
						GUIMessage msgHome = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_HOME,0,null);
						GUIGraphicsContext.SendMessage(msgHome);
            break;

          case 0x5D://Show context menu (Yellow button)
            if (header.hid.RawData3==0)
            {
              action = new Action(Action.ActionType.ACTION_CONTEXT_MENU,0,0);  
            }
            else
            {
              Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
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
              Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
              return false;
            }
            break;
          
					case 0x80://OEM1
            break;
          
					case 0x81://OEM2
            break;
          
					case 0x48://Recorded TV
						GUIMessage msgTvRec = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_RECORDEDTV,0,null);
						GUIGraphicsContext.SendMessage(msgTvRec);

            break;
          
					case 0x82://Standbye
            break;
          
					case 0x0d:// home
						GUIMessage msgHome2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_HOME,0,null);
						GUIGraphicsContext.SendMessage(msgHome2);
            break;
          
					case 0xB0: //play
            action=new Action(Action.ActionType.ACTION_PLAY,0,0);
            break;
          
					case 0xB1: //pause
            action=new Action(Action.ActionType.ACTION_PAUSE,0,0);
            break;
          
					case 0xB2: //record
            action=new Action(Action.ActionType.ACTION_RECORD,0,0);
            break;
          
					case 0xB4: //rewind
            action=new Action(Action.ActionType.ACTION_REWIND,0,0);
            break;
          
					case 0xB3: //fast forward
            action=new Action(Action.ActionType.ACTION_FORWARD,0,0);
            break;
          
					case 0xB5: //next
            if ((g_Player.Playing) && (g_Player.IsDVD))
                action=new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
              else
                action=new Action(Action.ActionType.ACTION_NEXT_ITEM,0,0);
            break;
          
					case 0xB6: //previous
            if ((g_Player.Playing) && (g_Player.IsDVD))
              action=new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
            else
              action=new Action(Action.ActionType.ACTION_PREV_ITEM,0,0);
            break;
          
					case 0xb7: //stop
            action=new Action(Action.ActionType.ACTION_STOP,0,0);
            break;
          
					case 0xe9: //volume+
//            action=new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
            break;
          
					case 0xea: //volume-
//            action=new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
            break;
          
					case 0x9c: //channel+
            action=new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
            break;
          
					case 0x9d: //channel-
            action=new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
            break;
          
					case 0xe2: //mute
            break;

					case 0x5A: //teletext
						if (g_Player.IsTV)
						{
							if (GUIGraphicsContext.IsFullScreenVideo)
							{
								// Activate fullscreen teletext
								GUIMessage msgTxt1 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT,0,null);
								GUIGraphicsContext.SendMessage(msgTxt1);
							}
							else
							{
								// Activate teletext in window
								GUIMessage msgTxt2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TELETEXT,0,null);
								GUIGraphicsContext.SendMessage(msgTxt2);
							}
						}
						break;

          
					default:
            Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
            return false;
        }
        return true;
      }
      return false;
    }
		#region greenbutton

		public void Start()
		{
			Log.Write("MyGreenButton.Start: Starting");

			string strDevice = FindDevice();

			if(strDevice == null)
			{
				// an appropriate error message will have been placed in the log by FindDevice
				Log.Write("MyGreenButton.Start: Failed to find device");
				return;
			}

			IntPtr handle = CreateFile(strDevice, FileAccess.ReadWrite, FileShare.ReadWrite, 0, FileMode.Open, FileFlag.Overlapped, 0);

			if(handle.ToInt32() == -1)
			{
				Log.Write("MyGreenButton.Start: Failed to open device ({0})", GetLastError());
				return;
			}

			// open a stream from the device and begin an asynchronous read
			m_streamRead = new FileStream(handle, FileAccess.ReadWrite, true, 128, true);
			m_streamRead.BeginRead(m_bufferRead, 0, m_bufferRead.Length, new AsyncCallback(OnReadComplete), m_bufferRead);

			Log.Write("MyGreenButton.Start: Started");
		}

		public void DeInit()
		{
			Log.Write("MyGreenButton.Stop: Stopping");

			if(m_streamRead != null)
			{
				m_streamRead.Close();
			}

			Log.Write("MyGreenButton.Stop: Stopped");
		}

		string FindDevice()
		{
			Guid guidHid = new Guid();

			// ask the OS for the guid that represents human input devices
			HidD_GetHidGuid(ref guidHid);

			IntPtr handle = SetupDiGetClassDevs(ref guidHid, 0, 0, 0x12);

			if(handle.ToInt32() == -1)
			{
				Console.WriteLine("MyGreenButton.FindDevice: Failed in call to SetupDiGetClassDevs ({0})", GetLastError());
				return null;
			}

			for(int nIndex = 0; ; nIndex++)
			{
				DeviceInfoData deviceInfoData = new DeviceInfoData();
				deviceInfoData.Size = Marshal.SizeOf(deviceInfoData);

				if(SetupDiEnumDeviceInfo(handle, nIndex, ref deviceInfoData) == false)
				{
					Console.WriteLine("MyGreenButton.FindDevice: Failed in call to SetupDiEnumDeviceInfo ({0})", GetLastError());
					return null;
				}

				DeviceInterfaceData deviceInterfaceData = new DeviceInterfaceData();
				deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);

				if(SetupDiEnumDeviceInterfaces(handle, ref deviceInfoData, ref guidHid, 0, ref deviceInterfaceData) == false)
				{
					Console.WriteLine("MyGreenButton.FindDevice: Failed in call to SetupDiEnumDeviceInterfaces ({0})", GetLastError());
					return null;
				}

				uint cbData = 0;

				if(SetupDiGetDeviceInterfaceDetail(handle, ref deviceInterfaceData, 0, 0, ref cbData, 0) == false && cbData == 0)
				{
					Console.WriteLine("MyGreenButton.FindDevice: Failed in call to SetupDiGetDeviceInterfaceDetail ({0})", GetLastError());
					return null;
				}

				DeviceInterfaceDetailData deviceInterfaceDetailData = new DeviceInterfaceDetailData();
				deviceInterfaceDetailData.Size = 5;

				if(SetupDiGetDeviceInterfaceDetail(handle, ref deviceInterfaceData, ref deviceInterfaceDetailData, cbData, 0, 0) == false)
				{
					Console.WriteLine("MyGreenButton.FindDevice: Failed in call to SetupDiGetDeviceInterfaceDetail ({0})", GetLastError());
					return null;
				}

				// is this device a 2005 MCE receiver?
				if(deviceInterfaceDetailData.DevicePath.IndexOf("#vid_0471&pid_0815&col01#") != -1)
				{
					return deviceInterfaceDetailData.DevicePath;
				}

				// is this device a 2004 MCE receiver?
				if(deviceInterfaceDetailData.DevicePath.IndexOf("#vid_045e&pid_006d&col01#") != -1)
				{
					return deviceInterfaceDetailData.DevicePath;
				}
			}

			return null;
		}
		
		void OnReadComplete(IAsyncResult asyncResult)
		{
			int bytesRead = m_streamRead.EndRead(asyncResult);

			if(bytesRead == 13 && m_bufferRead[5] == 13)
			{
				OnGreenButton();
			}

			// begin another asynchronous read from the device
			m_streamRead.BeginRead(m_bufferRead, 0, m_bufferRead.Length, new AsyncCallback(OnReadComplete), m_bufferRead);
		}

		void OnGreenButton()
		{
			// only the main thread can correctly handle switching to the home window
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_HOME, 0, null);
			GUIWindowManager.SendThreadMessage(msg);
		}
		#endregion
	}
}
