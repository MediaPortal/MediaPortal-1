using System;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUITV
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
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			SaveSettings();
		}
		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
			
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
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


		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			UpdateButtons();
		}
		void UpdateButtons()
		{
			bool isMpeg2=(spinType.Value==0);
			spinBitrate.Disabled=isMpeg2;
			spinFPS.Disabled=isMpeg2;
			spinQuality.Disabled=isMpeg2;
			spinScreenSize.Disabled=isMpeg2;

			if (!isMpeg2)
			{
				bool isCustom=(spinQuality.Value==4);
				spinBitrate.Disabled=!isCustom;
				spinFPS.Disabled=!isCustom;
				spinScreenSize.Disabled=!isCustom;

			}
		}

	}
}
