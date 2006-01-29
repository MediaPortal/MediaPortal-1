#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Devices;
using IRTrans.NET;

namespace MediaPortal.InputDevices
{
  class IrTransListener
  {
    const int WM_APP = 0x8000;
    const int WM_REMOTE = WM_APP + 100;

    private IRTransServer irt;
    public bool IrTransEnabled = false;

    [StructLayout(LayoutKind.Sequential)]
    struct COPYDATASTRUCT
    {
      public IntPtr dwData;
      public int cbData;
      [MarshalAs(UnmanagedType.LPStr)]
      public string lpData;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, [In()] ref COPYDATASTRUCT lParam);
    public IrTransListener()
    {
    }

    public void Init(IntPtr hwnd)
    {
      try
      {
        irt = new IRTransServer("localhost");
        irt.StartAsnycReceiver();
        IrTransEnabled = true;
        Log.Write("IRTrans: Remote enabled");
        irt.IRReceive += new IRTransServer.IRReceiveEventHandler(IRReceived);
      }
      catch (IRTrans.NET.IRTransConnectionException)
      {
      }
    }

    public void DeInit()
    {
      if (IrTransEnabled)
        irt.StopAsyncReceiver();
    }

    private void IRReceived(object sender, EventArgs e, NETWORKRECV recv)
    {
      string remoteCommand = recv.command.Trim();
      COPYDATASTRUCT cds;
      cds.dwData = (IntPtr)0;
      cds.lpData = remoteCommand;
      cds.cbData = remoteCommand.Length + 1;
      SendMessage(GUIGraphicsContext.ActiveForm, WM_REMOTE, 0, ref cds);
    }

    public bool WndProc(ref System.Windows.Forms.Message msg, out MediaPortal.GUI.Library.Action action, out char key, out System.Windows.Forms.Keys keyCode)
    {
      keyCode = System.Windows.Forms.Keys.A;
      key = (char)0;
      action = null;
      if (IrTransEnabled)
      {
        if (msg.Msg == WM_REMOTE)
        {
          COPYDATASTRUCT cds = (COPYDATASTRUCT)msg.GetLParam(typeof(COPYDATASTRUCT));
          /* Note: Following keys are sent directly to MP via app.cfg of the IRTrans module. 
           * No need to reinvent the wheel. :-)
           * 0-9, ok, enter, left, right, down, up, back
          */
          switch (cds.lpData)
          {
            case "play":        // Play Button
              if (InputDevices.LastHidRequest == AppCommands.MediaPlay && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaPlay;

              action = new Action(Action.ActionType.ACTION_PLAY, 0, 0);
              break;
            case "power":       // Power Button ???
              break;
            case "stop":        // Stop Button
              if (InputDevices.LastHidRequest == AppCommands.MediaStop && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaStop;

              action = new Action(Action.ActionType.ACTION_STOP, 0, 0);
              break;
            case "rec":         // Record 
              if (InputDevices.LastHidRequest == AppCommands.MediaRecord && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaRecord;

              action = new Action(Action.ActionType.ACTION_RECORD, 0, 0);
              break;
            case "rew":         // Rewind
              if (InputDevices.LastHidRequest == AppCommands.MediaRewind && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaRewind;

              action = new Action(Action.ActionType.ACTION_REWIND, 0, 0);
              break;
            case "fwd":         // Fast Forward
              if (InputDevices.LastHidRequest == AppCommands.MediaFastForward && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaFastForward;

              action = new Action(Action.ActionType.ACTION_FORWARD, 0, 0);
              break;
            case "pause":       // Pause
              if (InputDevices.LastHidRequest == AppCommands.MediaPause && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaPause;

              action = new Action(Action.ActionType.ACTION_PAUSE, 0, 0);
              break;
            case "next":        // Next 
              if (InputDevices.LastHidRequest == AppCommands.MediaNextTrack && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaNextTrack;

              if ((g_Player.Playing) && (g_Player.IsDVD))
                action = new Action(Action.ActionType.ACTION_NEXT_CHAPTER, 0, 0);
              else
                action = new Action(Action.ActionType.ACTION_NEXT_ITEM, 0, 0);
              break;
            case "prev":        // Previous
              if (InputDevices.LastHidRequest == AppCommands.MediaPreviousTrack && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaPreviousTrack;

              if ((g_Player.Playing) && (g_Player.IsDVD))
                action = new Action(Action.ActionType.ACTION_PREV_CHAPTER, 0, 0);
              else
                action = new Action(Action.ActionType.ACTION_PREV_ITEM, 0, 0);
              break;
            case "ehome":       // The "Green" Button
              GUIMessage msgHome2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_HOME, 0, null);
              GUIWindowManager.SendThreadMessage(msgHome2);
              break;
            case "epg":         // TV Guide
              GUIMessage msgtv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_TVGUIDE, 0, null);
              GUIWindowManager.SendThreadMessage(msgtv);
              break;
            case "info":        // Info
              if (GUIGraphicsContext.IsFullScreenVideo)
              {
                //pop up OSD during fullscreen video or live tv (even without timeshift)
                action = new Action(Action.ActionType.ACTION_SHOW_OSD, 0, 0);
                return true;
              }

              // Pop up info display
              action = new Action(Action.ActionType.ACTION_SHOW_INFO, 0, 0);
              break;
            case "livetv":      // LiveTV
              GUIMessage msgtv2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_TV, 0, null);
              GUIWindowManager.SendThreadMessage(msgtv2);
              break;
            case "video":       // Video  ???
              break;
            case "music":       // Music ???
              break;
            case "tv":          // TV   ???
              break;
            case "pictures":    // Pictures  ???
              break;
            case "vol+":        // Volume +
              if (InputDevices.LastHidRequest == AppCommands.VolumeUp && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.VolumeUp;

              action = new Action(Action.ActionType.ACTION_VOLUME_UP, 0, 0);
              break;
            case "vol-":        // Volume -
              if (InputDevices.LastHidRequest == AppCommands.VolumeDown && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.VolumeDown;

              action = new Action(Action.ActionType.ACTION_VOLUME_DOWN, 0, 0);
              break;
            case "mute":        // Mute   
              if (InputDevices.LastHidRequest == AppCommands.VolumeMute && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.VolumeMute;

              action = new Action(Action.ActionType.ACTION_VOLUME_MUTE, 0, 0);
              break;
            case "ch+":         // Channel +  
              if (InputDevices.LastHidRequest == AppCommands.MediaChannelUp && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaChannelUp;

              if (GUIGraphicsContext.IsFullScreenVideo)
                action = new Action(Action.ActionType.ACTION_NEXT_CHANNEL, 0, 0);
              else
                action = new Action(Action.ActionType.ACTION_PAGE_UP, 0, 0);
              break;
            case "ch-":         // Channel -  
              if (InputDevices.LastHidRequest == AppCommands.MediaChannelDown && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.MediaChannelDown;

              if (GUIGraphicsContext.IsFullScreenVideo)
                action = new Action(Action.ActionType.ACTION_PREV_CHANNEL, 0, 0);
              else
                action = new Action(Action.ActionType.ACTION_PAGE_DOWN, 0, 0);
              break;
            case "clear":       // clear
              break;
            case "back":        // back
              if (InputDevices.LastHidRequest == AppCommands.BrowserBackward && Environment.TickCount - InputDevices.LastHidRequestTick < 300)
                return true;

              InputDevices.LastHidRequest = AppCommands.BrowserBackward;

              keyCode = Keys.Escape;
              break;
            case "dvdmenu":     // dvd menu
              if (g_Player.Playing && g_Player.IsDVD)
              {
                action = new Action(Action.ActionType.ACTION_DVD_MENU, 0, 0);
              }
              else
              {
                action = new Action(Action.ActionType.ACTION_CONTEXT_MENU, 0, 0);
              }
              break;
            case "rectv":       // Recorded TV
              GUIMessage msgTvRec = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_RECORDEDTV, 0, null);
              GUIWindowManager.SendThreadMessage(msgTvRec);
              break;
            case "messenger":   // Messenger  ???
              break;
            case "radio":       // Radio  ???
              break;
            case "teletext":    // Teletext 
              if (g_Player.IsTV)
              {
                if (GUIGraphicsContext.IsFullScreenVideo)
                {
                  // Activate fullscreen teletext
                  GUIMessage msgTxt1 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT, 0, null);
                  GUIWindowManager.SendThreadMessage(msgTxt1);
                }
                else
                {
                  // Activate teletext in window
                  GUIMessage msgTxt2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_TELETEXT, 0, null);
                  GUIWindowManager.SendThreadMessage(msgTxt2);
                }
              }
              break;
            case "red":         // Red Button (Full Screen)
              action = new Action(Action.ActionType.ACTION_SHOW_GUI, 0, 0);
              break;
            case "green":       // Green Button (Home)
              GUIMessage msgHome = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)GUIWindow.Window.WINDOW_HOME, 0, null);
              GUIWindowManager.SendThreadMessage(msgHome);
              break;
            case "yellow":      // Yellow Button (Context Menu)
              action = new Action(Action.ActionType.ACTION_CONTEXT_MENU, 0, 0);
              break;
            case "blue":        // Blue Button (Change Aspect Ratio)
              action = new Action(Action.ActionType.ACTION_ASPECT_RATIO, 0, 0);
              break;
            default:            // Wrong Key pressed
              return false;
          }
          return true;
        }
      }
      return false;
    }
  }
}
