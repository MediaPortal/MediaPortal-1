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

using System;
using MediaPortal.GUI.Library;
using MediaPortal.Player ;
using MediaPortal.TV.Recording;
namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// Summary description for GUITVCompressMain.
	/// </summary>
	public class GUITVCompressMain : GUIWindow	
	{
		public GUITVCompressMain()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\mytvcompressmain.xml");
		}
		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing&&g_Player.HasVideo)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
					break;

			}
			base.OnAction(action);
		}

		protected override void OnPageDestroy(int newWindowId)
		{
			if ( !GUIGraphicsContext.IsTvWindow(newWindowId) )
			{
				if (Recorder.IsViewing() && ! (Recorder.IsTimeShifting()||Recorder.IsRecording()) )
				{
					if (GUIGraphicsContext.ShowBackground)
					{
						// stop timeshifting & viewing... 
	              
						Recorder.StopViewing();
					}
				}
			}
			base.OnPageDestroy (newWindowId);
		}
	}
}
