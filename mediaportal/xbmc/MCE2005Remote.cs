using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal
{
	/// <summary>
  ///
  //  terug 
  //  appcommand:3013C 10320000
  //
  //  play 
  //  appcommand:3013C 102E0000
  //
  //  vooruit 
  //  appcommand:3013C 10310000
  //
  //  pause 
  //  appcommand:3013C 102F0000
  //
  //  stoppen
  //  appcommand:3013C D0000
  //  wm_keydown:B2 1000001
  //
  //  herhalen
  //  appcommand:3013C C0000
  //  wm_keydown:B1 1000001
  //
  //  overslaan
  //  appcommand:3013C B0000
  //  wm_keydown:B0 1000001
  //
  //  vorige
  //  appcommand:3013C 10000
  //  wm_keydown:A6 1000001
  //
  //  meer info ??
  //
  //  OK:
  //  wm_keydown:D 1C0001
  //
  //  kanaal +
  //  appcommand:3013C 10330000
  //
  //  kanaal -
  //  appcommand:3013C 10340000
  //
  //  dempen:
  //  appcommand:3013C 80000
  //  wm_keydown:AD 1000001
  //
  //  wis:
  //  wm_keydown:1B 10001
  //
  //  enter:
  //  wm_keydown:D 1C0001
  //
  //
  //  *
  //  wm_keydown:10 2A0001
  //  wm_keydown:38 90001
  //
  //  #
  //  wm_keydown:10 2A0001
  //  wm_keydown:33 40001
  //
  //
  //  dempen:
  //  appcommand:3013C 80000
  //  wm_keydown:AD 1000001
  //
  //  opnemen:
  //  appcommand:3013C 10300000
   
	/// </summary>
	public class MCE2005Remote
	{
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
    [DllImport("user32.dll", SetLastError=true)]
    extern static int GetRawInputData(IntPtr rawInput, int uiCmd, IntPtr pData, ref int dwSize, int dwHdrSize);
    
    [DllImport("User32.dll",EntryPoint="RegisterRawInputDevices",SetLastError=true)]
    public extern static bool RegisterRawInputDevices(
                                                      [In] RAWINPUTDEVICE[] pRawInputDevices,
                                                      [In] uint uiNumDevices,
                                                      [In] uint cbSize);  

    bool RemoteFound=false;
		public MCE2005Remote()
		{
		}
    
    public void Init(IntPtr hwnd)
    {
      if (RemoteFound) return;// already registered

      try
      {
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
              if (g_Player.Player != null)
              {
                if (g_Player.Playing && GUIGraphicsContext.IsFullScreenVideo)                   
                {
                  //pop up OSD during fullscreen video
                  action=new Action(Action.ActionType.ACTION_SHOW_OSD,0,0);
                  return true;
                }
              }

              action=new Action(Action.ActionType.ACTION_SHOW_INFO,0,0);
              return true;
            }
            else
            {
              Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
              return false;
            }
          break;
          case 0x25://My tv
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_TV);
            break;
          case 0x4B://DVD angle
            break;
          case 0x4C://DVD audio
            break;
          case 0x24://DVD menu
            if (header.hid.RawData3==0)
            {
              if (g_Player.Player != null)
              {
                if (g_Player.Playing && g_Player.IsDVD)
                {
                  action = new Action(Action.ActionType.ACTION_DVD_MENU,0,0);  
                }
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
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_TVGUIDE);
            break;
          case 0x47://My Music
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_MUSIC_FILES);
            break;
          case 0x49://My Pictures
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_PICTURES);
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
          case 0x5E://TV Change aspect ratio (Blue button)
            if (header.hid.RawData3==0)
            {
              action = new Action(Action.ActionType.ACTION_ASPECT_RATIO,0,0);  
            }
            break;
          case 0x4A://My Video
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_VIDEOS);
            break;
          case 0x80://OEM1
            break;
          case 0x81://OEM2
            break;
          case 0x48://Recorded TV
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_RECORDEDTV);
            break;
          case 0x82://Standbye
            break;
          case 0x0d:// home
            GUIGraphicsContext.IsFullScreenVideo=false;
            GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_HOME);
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
            if (g_Player.Player != null)
              if ((g_Player.Player.Playing) && (g_Player.IsDVD))
                action=new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
              else
                action=new Action(Action.ActionType.ACTION_NEXT_ITEM,0,0);
            break;
          case 0xb6: //previous
            if (g_Player.Player != null)
              if ((g_Player.Player.Playing) && (g_Player.IsDVD))
                  action=new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
                else
                  action=new Action(Action.ActionType.ACTION_PREV_ITEM,0,0);
            break;
          case 0xb7: //stop
            action=new Action(Action.ActionType.ACTION_STOP,0,0);
            break;
          case 0xe9: //volume+
            action=new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
            break;
          case 0xea: //volume-
            action=new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
            break;
          case 0x9c: //channel+
            action=new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
            break;
          case 0x9d: //channel-
            action=new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
            break;
          case 0xe2: //mute
            break;
          default:
            Log.Write("unknown key pressed hid.RawData1:{0:X} {1:X} {2:X}",header.hid.RawData1,header.hid.RawData2,header.hid.RawData3);
          break;
        }
        return true;
      }
      return false;
    }
	}
}
