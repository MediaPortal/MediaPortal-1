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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal
{
	public class HIDListener
	{
		enum AppCommands
		{
			BrowserBackward					= 1,
			BrowserForward					= 2,
			BrowserRefresh					= 3,
			BrowserStop						= 4,
			BrowserSearch					= 5,
			BrowserFavorites				= 6,
			BrowserHome						= 7,
			VolumeMute						= 8,
			VolumeDown						= 9,
			VolumeUp						= 10,
			MediaNextTrack					= 11,
			MediaPreviousTrack				= 12,
			MediaStop						= 13,
			MediaPlayPause					= 14,
			LaunchMail						= 15,
			LaunchMediaSelect				= 16,
			LaunchApp1						= 17,
			LaunchApp2						= 18,
			BassDown						= 19,
			BassBoost						= 20,
			BassUp							= 21,
			TrebleDown						= 22,
			TrebleUp						= 23,
			MicrophoneVolumeMute			= 24,
			MicrophoneVolumeDown			= 25,
			MicrophoneVolumeUp				= 26,
			Help							= 27,
			Find							= 28,
			New								= 29,
			Open							= 30,
			Close							= 31,
			Save							= 32,
			Print							= 33,
			Undo							= 34,
			Redo							= 35,
			Copy							= 36,
			Cut								= 37,
			Paste							= 38,
			ReplyToMail						= 39,
			ForwardMail						= 40,
			SendMail						= 41,
			SpellCheck						= 42,
			DictateOrCommandControlToggle   = 43,
			MicrophoneOnOffToggle			= 44,
			CorrectionList					= 45,
			MediaPlay						= 46,
			MediaPause						= 47,
			MediaRecord						= 48,
			MediaFastForward				= 49,
			MediaRewind						= 50,
			MediaChannelUp					= 51,
			MediaChannelDown				= 52,
		}
		
		public static bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
		{
			action = null;
			key = (char)0;
			keyCode = Keys.A;

			// we are only interested in WM_APPCOMMAND
			if(msg.Msg != 0x0319)
				return false;

			switch((AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000))
			{
				case AppCommands.BrowserBackward:
					keyCode = Keys.Escape;
					break;
//					action = new Action(Action.ActionType.ACTION_PREVIOUS_MENU, 0, 0);
//					break;

				case AppCommands.MediaChannelUp:
					if (GUIGraphicsContext.IsFullScreenVideo)
						action = new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
					else
						action = new Action(Action.ActionType.ACTION_PAGE_UP,0,0);
					break;

				case AppCommands.MediaChannelDown:
					if (GUIGraphicsContext.IsFullScreenVideo)
						action = new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
					else
						action = new Action(Action.ActionType.ACTION_PAGE_DOWN,0,0);
					break;

				case AppCommands.MediaFastForward:
					action = new Action(Action.ActionType.ACTION_FORWARD,0,0);
					break;

				case AppCommands.MediaPause:
					action = new Action(Action.ActionType.ACTION_PAUSE,0,0);
					break;

				case AppCommands.MediaPlay:
					action = new Action(Action.ActionType.ACTION_PLAY,0,0);
					break;

				case AppCommands.MediaPlayPause:
					if(g_Player.Playing)
						action = new Action(Action.ActionType.ACTION_PAUSE,0,0);
					else if(g_Player.Paused)
						action = new Action(Action.ActionType.ACTION_PLAY,0,0);

					break;

				case AppCommands.MediaStop:
					action = new Action(Action.ActionType.ACTION_STOP,0,0);
					break;

				case AppCommands.MediaRecord:
					action = new Action(Action.ActionType.ACTION_RECORD,0,0);
					break;

				case AppCommands.MediaRewind:
					action = new Action(Action.ActionType.ACTION_REWIND,0,0);
					break;

				case AppCommands.MediaNextTrack:
					if ((g_Player.Playing) && (g_Player.IsDVD))
						action = new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
					else
						action = new Action(Action.ActionType.ACTION_NEXT_ITEM,0,0);
					break;

				case AppCommands.MediaPreviousTrack:
					if ((g_Player.Playing) && (g_Player.IsDVD))
						action = new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
					else
						action = new Action(Action.ActionType.ACTION_PREV_ITEM,0,0);
					break;

				case AppCommands.VolumeDown:
					action = new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
					break;

				case AppCommands.VolumeMute:
					action = new Action(Action.ActionType.ACTION_VOLUME_MUTE,0,0);
					break;

				case AppCommands.VolumeUp:
					action = new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
					break;

				default:
					Log.Write("Not handled by HIDListener: {0}", (AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000));
					return false;
			}

			msg.Result = new IntPtr(1);

			return true;
		}
	}
}
