using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
namespace WindowPlugins.GUISettings.TV
{
	/// <summary>
	/// Summary description for GUISettingsMovies.
	/// </summary>
	public class GUISettingsMovies : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIButtonControl btnVideoCodec=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnAudioCodec=null;
		//[SkinControlAttribute(26)]			protected GUIButtonControl btnVideoRenderer=null;
		[SkinControlAttribute(27)]			protected GUIButtonControl btnAudioRenderer=null;
		[SkinControlAttribute(28)]			protected GUIButtonControl btnAspectRatio=null;
		public GUISettingsMovies()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_MOVIES;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_movies.xml");
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnVideoCodec) OnVideoCodec();
			if (control==btnAudioCodec) OnAudioCodec();
			//if (control==btnVideoRenderer) OnVideoRenderer();
			if (control==btnAspectRatio) OnAspectRatio();
			if (control==btnAudioRenderer) OnAudioRenderer();
			base.OnClicked (controlId, control, actionType);
		}
		void OnVideoCodec()
		{
			string strVideoCodec="";
			using (MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				strVideoCodec=xmlreader.GetValueAsString("movieplayer","videocodec","");
			}
			ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);

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
			using (MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValue("movieplayer","videocodec",(string)availableVideoFilters[dlg.SelectedLabel]);
			}
		}

		void OnAudioCodec()
		{
			string strAudioCodec="";
			using (MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				strAudioCodec=xmlreader.GetValueAsString("movieplayer","audiocodec","");
			}
			ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);

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
			using (MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValue("movieplayer","audiocodec",(string)availableAudioFilters[dlg.SelectedLabel]);
			}
		}/*
		void OnVideoRenderer()
		{
			int vmr9Index=0;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				vmr9Index= xmlreader.GetValueAsInt("movieplayer", "vmr9", 0);
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
					xmlwriter.SetValue("movieplayer", "vmr9", dlg.SelectedLabel.ToString());
				}
			}
		}*/
		void OnAspectRatio()
		{
			string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };
			string defaultAspectRatio ="";
			using (MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				defaultAspectRatio = xmlreader.GetValueAsString("movieplayer","defaultar", "normal");
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
				using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
				{
					xmlwriter.SetValue("movieplayer", "defaultar", aspectRatio[dlg.SelectedLabel]);
				}
			}
		}
		void OnAudioRenderer()
		{
			string strAudioRenderer="";
			using (MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				strAudioRenderer=xmlreader.GetValueAsString("movieplayer","audiorenderer","");
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
			using (MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValue("movieplayer","audiorenderer",(string)availableAudioFilters[dlg.SelectedLabel]);
			}

		}

	}
}