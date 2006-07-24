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
	/// Summary description for GUITVCompressSettings.
	/// </summary>
	public class GUITVCompressSettings : GUIWindow	
	{
		[SkinControlAttribute(3)]				protected GUISpinControl spinType=null;
		[SkinControlAttribute(5)]				protected GUISpinControl spinQuality=null;
		[SkinControlAttribute(7)]				protected GUISpinControl spinScreenSize=null;
		[SkinControlAttribute(9)]				protected GUISpinControl spinFPS=null;
		[SkinControlAttribute(11)]			protected GUISpinControl spinBitrate=null;
		[SkinControlAttribute(13)]			protected GUISpinControl spinPriority=null;
		[SkinControlAttribute(15)]			protected GUICheckMarkControl checkDeleteOriginal=null;

		public GUITVCompressSettings()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\mytvcompresssettings.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();
		}

		void LoadSettings()
		{
			using(MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				spinBitrate.Value = xmlreader.GetValueAsInt("compression","bitrate",4);
				spinFPS.Value		 = xmlreader.GetValueAsInt("compression","fps",1);
				spinPriority.Value= xmlreader.GetValueAsInt("compression","priority",0);
				spinQuality.Value= xmlreader.GetValueAsInt("compression","quality",3);
				spinScreenSize.Value= xmlreader.GetValueAsInt("compression","screensize",1);
				spinType.Value= xmlreader.GetValueAsInt("compression","type",0);
				checkDeleteOriginal.Selected= xmlreader.GetValueAsBool("compression","deleteoriginal",true);
			}
			UpdateButtons();
		}

		void SaveSettings()
		{
			
			using(MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlreader.SetValue("compression","bitrate",spinBitrate.Value);
				xmlreader.SetValue("compression","fps",spinFPS.Value);
				xmlreader.SetValue("compression","priority",spinPriority.Value);
				xmlreader.SetValue("compression","quality",spinQuality.Value);
				xmlreader.SetValue("compression","screensize",spinScreenSize.Value);
				xmlreader.SetValue("compression","type",spinType.Value);
				xmlreader.SetValueAsBool("compression","deleteoriginal",checkDeleteOriginal.Selected);
			}
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

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			UpdateButtons();
		}
		void UpdateButtons()
		{
			bool isMpeg2=(spinType.Value==0);
			bool isWMV=(spinType.Value==1);
			bool isXVID=(spinType.Value==2);
			spinBitrate.Disabled=(isMpeg2 || isXVID);
			spinFPS.Disabled=(isMpeg2 || isXVID);
			spinQuality.Disabled=(isMpeg2 );
			spinScreenSize.Disabled=(isMpeg2 || isXVID);

			if (isWMV)
			{
				bool isCustom=(spinQuality.Value==4);
				spinBitrate.Disabled=!isCustom;
				spinFPS.Disabled=!isCustom;
				spinScreenSize.Disabled=!isCustom;

			}
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

			SaveSettings();
		}
	}
}
