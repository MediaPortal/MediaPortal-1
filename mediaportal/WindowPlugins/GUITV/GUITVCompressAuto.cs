using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player ;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
namespace WindowPlugins.GUITV
{
	/// <summary>
	/// Summary description for GUITVCompressAuto.
	/// </summary>
	public class GUITVCompressAuto : GUIWindow	
	{
		[SkinControlAttribute(5)]				protected GUISpinControl spinHour=null;
		[SkinControlAttribute(2)]				protected GUICheckMarkControl checkAutoCompress=null;
		[SkinControlAttribute(7)]			protected GUICheckMarkControl checkDeleteOriginal=null;

		public GUITVCompressAuto()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\mytvcompressauto.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			SaveSettings();
			if (checkAutoCompress.Selected)
			{
				AutoCompress();
			}
		}
		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				spinHour.Value = xmlreader.GetValueAsInt("autocompression","hour",4);
				checkDeleteOriginal.Selected= xmlreader.GetValueAsBool("autocompression","deleteoriginal",true);
				checkAutoCompress.Selected= xmlreader.GetValueAsBool("autocompression","enabled",true);
			}
			UpdateButtons();
		}

		void SaveSettings()
		{
			
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlreader.SetValue("autocompression","hour",spinHour.Value);
				xmlreader.SetValueAsBool("autocompression","deleteoriginal",checkDeleteOriginal.Selected);
				xmlreader.SetValueAsBool("autocompression","enabled",checkAutoCompress.Selected);
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
			spinHour.Disabled = !checkAutoCompress.Selected;
			checkDeleteOriginal.Disabled = !checkAutoCompress.Selected;
		}

		void AutoCompress()
		{
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordedTV(ref recordings);
			foreach (TVRecorded rec in recordings)
			{
				if (Transcoder.IsTranscoding(rec)) continue; //already transcoding...
				try
				{
					if (!System.IO.File.Exists(rec.FileName)) continue;
					string ext=System.IO.Path.GetExtension(rec.FileName).ToLower();
					if (ext!=".dvr-ms" && ext != ".sbe") continue;
				}
				catch(Exception)
				{
					continue;
				}
				Transcoder.Transcode(rec,false);
			}
		}
	}
}
