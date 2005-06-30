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
			if ( !GUITVHome.IsTVWindow(newWindowId) )
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
