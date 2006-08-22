using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.TV
{
	/// <summary>
	/// Summary description for GUISettingsRecordings.
	/// </summary>
	public class GUISettingsRecordings : GUIWindow
	{
		[SkinControlAttribute(4)]			protected GUICheckMarkControl cbAutoDeleteRecordings=null;
		[SkinControlAttribute(5)]			protected GUICheckMarkControl cbAddRecordingsToDbs=null;
		[SkinControlAttribute(27)]		protected GUISpinControl spinPreRecord=null;
		[SkinControlAttribute(30)]		protected GUISpinControl spinPostRecord=null;

		public GUISettingsRecordings()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_RECORDINGS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_recording.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			spinPreRecord.SetRange(0,30);
			spinPostRecord.SetRange(0,30);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				spinPreRecord.Value = (xmlreader.GetValueAsInt("capture", "prerecord", 5));
				spinPostRecord.Value = (xmlreader.GetValueAsInt("capture", "postrecord", 5));				
				cbAutoDeleteRecordings.Selected= xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
				cbAddRecordingsToDbs.Selected= xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);

			}		
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==cbAutoDeleteRecordings) OnAutoDeleteRecordings();
			if (control==cbAddRecordingsToDbs) OnAddRecordingsToMovieDatabase();
			if (control==spinPreRecord) OnPreRecord();
			if (control==spinPostRecord) OnPostRecord();
			base.OnClicked (controlId, control, actionType);
		}

		void OnAutoDeleteRecordings()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("capture", "deletewatchedshows", cbAutoDeleteRecordings.Selected);
			}		
		}

		void OnAddRecordingsToMovieDatabase()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("capture", "addrecordingstomoviedatabase", cbAddRecordingsToDbs.Selected);
			}		

		}

		void OnPreRecord()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "prerecord", spinPreRecord.Value.ToString());
			}		
		}
		
		void OnPostRecord()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "postrecord", spinPostRecord.Value.ToString());
			}		
		}

	}
}
