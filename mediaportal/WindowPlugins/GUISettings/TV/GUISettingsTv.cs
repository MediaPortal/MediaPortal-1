using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using DShowNET;
using DShowNET.Helper;
namespace WindowPlugins.GUISettings.TV
{
	/// <summary>
	/// Summary description for GUISettingsTv.
	/// </summary>
	public class GUISettingsTv : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIButtonControl btnVideoCodec=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnAudioCodec=null;
		[SkinControlAttribute(26)]			protected GUIButtonControl btnVideoRenderer=null;
		[SkinControlAttribute(27)]			protected GUIButtonControl btnDeinterlace=null;
		[SkinControlAttribute(28)]			protected GUIButtonControl btnAspectRatio=null;
		[SkinControlAttribute(29)]			protected GUIButtonControl btnTimeshiftBuffer=null;
		[SkinControlAttribute(30)]			protected GUIButtonControl btnAutoTurnOnTv=null;
    [SkinControlAttribute(33)]
    protected GUIButtonControl btnAudioRenderer = null;
    [SkinControlAttribute(34)]
    protected GUIButtonControl btnEpg = null;
		public GUISettingsTv()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_TV;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_tv.xml");
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnAudioRenderer) OnAudioRenderer();
			if (control==btnVideoCodec) OnVideoCodec();
			if (control==btnAudioCodec) OnAudioCodec();
			if (control==btnVideoRenderer) OnVideoRenderer();
			if (control==btnAspectRatio) OnAspectRatio();
			if (control==btnTimeshiftBuffer) OnTimeshiftBuffer();
			if (control==btnDeinterlace) OnDeinterlace();
			if (control==btnAutoTurnOnTv) OnAutoTurnOnTv();
			base.OnClicked (controlId, control, actionType);
		}
		void OnVideoCodec()
		{
			string strVideoCodec="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
			}
			ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				int selected=0;
				int count=0;
				foreach (string codec in availableVideoFilters)
				{
					dlg.Add(codec);//delete
					if (codec==strVideoCodec)
						selected=count;
					count++;
				}
				dlg.SelectedLabel=selected;
			}
			dlg.DoModal(GetID);
			if (dlg.SelectedLabel<0) return;
			using (MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("mytv","videocodec",(string)availableVideoFilters[dlg.SelectedLabel]);
			}
		}

		void OnAudioCodec()
		{
			string strAudioCodec="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
			}
			ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				int selected=0;
				int count=0;
				foreach (string codec in availableAudioFilters)
				{
					dlg.Add(codec);//delete
					if (codec==strAudioCodec)
						selected=count;
					count++;
				}
				dlg.SelectedLabel=selected;
			}
			dlg.DoModal(GetID);
			if (dlg.SelectedLabel<0) return;
			using (MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("mytv","audiocodec",(string)availableAudioFilters[dlg.SelectedLabel]);
			}
		}
		void OnVideoRenderer()
		{
			int vmr9Index=0;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				vmr9Index= xmlreader.GetValueAsInt("mytv", "vmr9", 0);
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				dlg.Add("Video Mixing Renderer 7");
				dlg.Add("Video Mixing Renderer 9");
				dlg.SelectedLabel=vmr9Index;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
						xmlwriter.SetValue("mytv", "vmr9", dlg.SelectedLabel.ToString());
				}
			}
		}
		void OnAspectRatio()
		{
			string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };
			string defaultAspectRatio ="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				defaultAspectRatio = xmlreader.GetValueAsString("mytv","defaultar", "normal");
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				int selected=0;
				for(int index = 0; index < aspectRatio.Length; index++)
				{
					if(aspectRatio[index].Equals(defaultAspectRatio))
					{
						selected=index;
					}
					dlg.Add(aspectRatio[index]);
				}
				dlg.SelectedLabel=selected;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					xmlwriter.SetValue("mytv", "defaultar", aspectRatio[dlg.SelectedLabel]);
				}
			}
		}
		void OnTimeshiftBuffer()
		{
			int buflen=30;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				buflen=xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
			}
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				int selected=0;
				int count=0;
				for (int i=30; i <=90; i+=30)
				{
					dlg.Add( String.Format("{0} mins", i.ToString() ) ) ;
					if (i==buflen)
					{
						selected=count;
					}
					count++;
				}
				dlg.SelectedLabel=selected;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					buflen=(dlg.SelectedLabel*30)+30;
					xmlwriter.SetValue("capture", "timeshiftbuffer", buflen.ToString());
				}
			}
		}
		void OnDeinterlace()
		{
			string[] deinterlaceModes = { "None", "Bob", "Weave", "Best"};
			int deInterlaceMode=1;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				deInterlaceMode= xmlreader.GetValueAsInt("mytv", "deinterlace", 1);
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				
				for(int index = 0; index < deinterlaceModes.Length; index++)
				{
					dlg.Add(deinterlaceModes[index]);
				}
				dlg.SelectedLabel=deInterlaceMode;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					xmlwriter.SetValue("mytv", "deinterlace", dlg.SelectedLabel);
				}
			}
		}
		void OnAutoTurnOnTv()
		{

			bool autoTurnOn=true;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				autoTurnOn= xmlreader.GetValueAsBool("mytv", "tvon", true);
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				dlg.Add("Dont automaticly turn on tv when entering My TV");
				dlg.Add("Automaticly turn on tv when entering My TV");
				dlg.SelectedLabel=autoTurnOn?1:0;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					xmlwriter.SetValueAsBool("mytv", "tvon", (dlg.SelectedLabel==1));
				}
			}
		}
		void OnAudioRenderer()
		{
			string strAudioRenderer="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				strAudioRenderer=xmlreader.GetValueAsString("mytv","audiorenderer","");
			}
			ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				int selected=0;
				int count=0;
				foreach (string codec in availableAudioFilters)
				{
					dlg.Add(codec);//delete
					if (codec==strAudioRenderer)
						selected=count;
					count++;
				}
				dlg.SelectedLabel=selected;
			}
			dlg.DoModal(GetID);
			if (dlg.SelectedLabel<0) return;
			using (MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("mytv","audiorenderer",(string)availableAudioFilters[dlg.SelectedLabel]);
			}

		}

	}
}