using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal
{
	/// <summary>
	/// 
	/// </summary>
	public class MCE2005Remote
	{
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

    public struct RAWINPUTDEVICE 
    {
      public ushort usUsagePage;
      public ushort usUsage;
      public uint dwFlags;
      public IntPtr hwndTarget;
    } ;

    [DllImport("User32.dll",EntryPoint="RegisterRawInputDevices",SetLastError=true)]
    public extern static bool RegisterRawInputDevices(
                                                      [In] RAWINPUTDEVICE[] pRawInputDevices,
                                                      [In] uint uiNumDevices,
                                                      [In] uint cbSize);  

    bool RemoteFound=false;
		public MCE2005Remote()
		{
		}
    
    public void Init()
    {

      try
      {
        //register device
        RAWINPUTDEVICE[] rid1= new RAWINPUTDEVICE[1];

        rid1[0].usUsagePage = 0xFFBC;
        rid1[0].usUsage = 0x88;
        rid1[0].dwFlags = 0;


        Log.Write("Register for MCE2005 remote");
        bool Success=RegisterRawInputDevices(rid1, (uint)rid1.Length,(uint)Marshal.SizeOf(rid1[0]));
        if (Success) 
        {
          Log.Write("Registered");
          RemoteFound=true;
          return;
        }
      }
      catch (Exception)
      {
      }
      Log.Write("Failed, MCE2005 remote not installed?");
    }
    

    public bool WndProc(ref Message msg)
    {
      if (!RemoteFound) return false;
      if (msg.Msg==WM_APPCOMMAND)
      {
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

        Log.Write("appcommand:{0} {1}",wparam,lparam);
        Action action = new Action(Action.ActionType.ACTION_INVALID,0,0);
        switch(wparam)
        {
          case APPCOMMAND_BROWSER_BACKWARD:
            action.wID=Action.ActionType.ACTION_PREVIOUS_MENU;
          break;

          case  APPCOMMAND_BROWSER_FORWARD        :
            break;
          case  APPCOMMAND_BROWSER_REFRESH        :
            break;
          case  APPCOMMAND_BROWSER_STOP           :
            break;
          case  APPCOMMAND_BROWSER_SEARCH         :
            break;
          case  APPCOMMAND_BROWSER_FAVORITES      :
            break;
          case  APPCOMMAND_BROWSER_HOME           :
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
            break;
          case  APPCOMMAND_VOLUME_MUTE            :
            break;
          case  APPCOMMAND_VOLUME_DOWN            :
            action.wID=Action.ActionType.ACTION_VOLUME_DOWN;
            break;
          case  APPCOMMAND_VOLUME_UP              :
            action.wID=Action.ActionType.ACTION_VOLUME_UP;
            break;
          case  APPCOMMAND_MEDIA_NEXTTRACK        :
            action.wID=Action.ActionType.ACTION_NEXT_ITEM;
            break;
          case  APPCOMMAND_MEDIA_PREVIOUSTRACK    :
            action.wID=Action.ActionType.ACTION_PREV_ITEM;
            break;
          case  APPCOMMAND_MEDIA_STOP             :
            action.wID=Action.ActionType.ACTION_STOP;
            break;
          case  APPCOMMAND_MEDIA_PLAY_PAUSE       :
            break;
          case  APPCOMMAND_LAUNCH_MAIL            :
            break;
          case  APPCOMMAND_LAUNCH_MEDIA_SELECT    :
            break;
          case  APPCOMMAND_LAUNCH_APP1            :
            break;
          case  APPCOMMAND_LAUNCH_APP2            :
            break;
          case  APPCOMMAND_BASS_DOWN              :
            break;
          case  APPCOMMAND_BASS_BOOST             :
            break;
          case  APPCOMMAND_BASS_UP                :
            break;
          case  APPCOMMAND_TREBLE_DOWN            :
            break;
          case  APPCOMMAND_TREBLE_UP              :
            break;
          case  APPCOMMAND_MICROPHONE_VOLUME_MUTE :
            break;
          case  APPCOMMAND_MICROPHONE_VOLUME_DOWN :
            break;
          case  APPCOMMAND_MICROPHONE_VOLUME_UP   :
            break;
          case  APPCOMMAND_HELP                   :
            break;
          case  APPCOMMAND_FIND                   :
            break;
          case  APPCOMMAND_NEW                    :
            break;
          case  APPCOMMAND_OPEN                   :
            break;
          case  APPCOMMAND_CLOSE                  :
            break;
          case  APPCOMMAND_SAVE                   :
            break;
          case  APPCOMMAND_PRINT                  :
            break;
          case  APPCOMMAND_UNDO                   :
            break;
          case  APPCOMMAND_REDO                   :
            break;
          case  APPCOMMAND_COPY                   :
            break;
          case  APPCOMMAND_CUT                    :
            break;
          case  APPCOMMAND_PASTE                  :
            break;
          case  APPCOMMAND_REPLY_TO_MAIL          :
            break;
          case  APPCOMMAND_FORWARD_MAIL           :
            break;
          case  APPCOMMAND_SEND_MAIL              :
            break;
          case  APPCOMMAND_SPELL_CHECK            :
            break;
          case  APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE :
            break;
          case  APPCOMMAND_MIC_ON_OFF_TOGGLE      :
            break;
          case  APPCOMMAND_CORRECTION_LIST        :
            break;
          case  APPCOMMAND_MEDIA_PLAY             :
            action.wID=Action.ActionType.ACTION_PLAY;
            break;
          case  APPCOMMAND_MEDIA_PAUSE            :
            action.wID=Action.ActionType.ACTION_PAUSE;
            break;
          case  APPCOMMAND_MEDIA_RECORD           :
            break;
          case  APPCOMMAND_MEDIA_FAST_FORWARD     :
            action.wID=Action.ActionType.ACTION_MUSIC_FORWARD;
            break;
          case  APPCOMMAND_MEDIA_REWIND           :
            action.wID=Action.ActionType.ACTION_MUSIC_REWIND;
            break;
          case  APPCOMMAND_MEDIA_CHANNEL_UP       :
            action.wID=Action.ActionType.ACTION_NEXT_CHANNEL;
            break;
          case  APPCOMMAND_MEDIA_CHANNEL_DOWN     :
            action.wID=Action.ActionType.ACTION_PREV_CHANNEL;
            break;
        }
        if (action.wID==Action.ActionType.ACTION_INVALID) return false;
        GUIWindowManager.OnAction(action);
        return true;
      }
      return false;
    }
	}
}
