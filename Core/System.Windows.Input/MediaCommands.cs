#region Copyright (C) 2005 Team MediaPortal

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

#endregion

namespace System.Windows.Input
{
	public sealed class MediaCommands
	{
		#region Constructors

		static MediaCommands()
		{
			BoostBass = new UICommand("BoostBass", typeof(MediaCommands));
			ChannelDown = new UICommand("ChannelDown", typeof(MediaCommands));
			ChannelUp = new UICommand("ChannelUp", typeof(MediaCommands));
			DecreaseBase = new UICommand("DecreaseBase", typeof(MediaCommands));
			DecreaseMicrophone = new UICommand("DecreaseMicrophone", typeof(MediaCommands));
			DecreaseTreble = new UICommand("DecreaseTreble", typeof(MediaCommands));
			DecreaseVolume = new UICommand("DecreaseVolume", typeof(MediaCommands));
			FastForward = new UICommand("FastForward", typeof(MediaCommands));
			IncreaseBase = new UICommand("IncreaseBase", typeof(MediaCommands));
			IncreaseMicrophoneVolume = new UICommand("IncreaseMicrophoneVolume", typeof(MediaCommands));
			IncreaseTreble = new UICommand("IncreaseTreble", typeof(MediaCommands));
			IncreaseVolume = new UICommand("IncreaseVolume", typeof(MediaCommands));
			MuteMicrophoneVolume = new UICommand("MuteMicrophoneVolume", typeof(MediaCommands));
			MuteVolume = new UICommand("MuteVolume", typeof(MediaCommands));
			NextTrack = new UICommand("NextTrack", typeof(MediaCommands));
			Pause = new UICommand("Pause", typeof(MediaCommands));
			Play = new UICommand("Play", typeof(MediaCommands));
			PreviousTrack = new UICommand("PreviousTrack", typeof(MediaCommands));
			Record = new UICommand("Record", typeof(MediaCommands));
			Rewind = new UICommand("Rewind", typeof(MediaCommands));
			Select = new UICommand("Select", typeof(MediaCommands));
			Stop = new UICommand("Stop", typeof(MediaCommands));
			ToggleMicrophoneOnOff = new UICommand("ToggleMicrophoneOnOff", typeof(MediaCommands));
			TogglePlayPause = new UICommand("TogglePlayPause", typeof(MediaCommands));
		}

		private MediaCommands()
		{
		}

		#endregion Constructors

		#region Fields
		
		public static readonly UICommand BoostBass;
		public static readonly UICommand ChannelDown;
		public static readonly UICommand ChannelUp;
		public static readonly UICommand DecreaseBase;
		public static readonly UICommand DecreaseMicrophone;
		public static readonly UICommand DecreaseTreble;
		public static readonly UICommand DecreaseVolume;
		public static readonly UICommand FastForward;
		public static readonly UICommand IncreaseBase;
		public static readonly UICommand IncreaseMicrophoneVolume;
		public static readonly UICommand IncreaseTreble;
		public static readonly UICommand IncreaseVolume;
		public static readonly UICommand MuteMicrophoneVolume;
		public static readonly UICommand MuteVolume;
		public static readonly UICommand NextTrack;
		public static readonly UICommand Pause;
		public static readonly UICommand Play;
		public static readonly UICommand PreviousTrack;
		public static readonly UICommand Record;
		public static readonly UICommand Rewind;
		public static readonly UICommand Select;
		public static readonly UICommand Stop;
		public static readonly UICommand ToggleMicrophoneOnOff;
		public static readonly UICommand TogglePlayPause;

		#endregion Fields
	}
}
