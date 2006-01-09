using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using DShowNET;
using DShowNET.Helper;
namespace WindowPlugins.GUISettings.TV
{
	/// <summary>
	/// Summary description for GUISettingsDVD.
	/// </summary>
	public class GUISettingsDVD : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIButtonControl btnVideoCodec=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnAudioCodec=null;
		[SkinControlAttribute(26)]			protected GUIButtonControl btnVideoRenderer=null;
		[SkinControlAttribute(27)]			protected GUIButtonControl btnAudioRenderer=null;
		[SkinControlAttribute(28)]			protected GUIButtonControl btnAspectRatio=null;
		[SkinControlAttribute(29)]			protected GUIButtonControl btnSubtitle=null;
		[SkinControlAttribute(30)]			protected GUIButtonControl btnAudioLanguage=null;

		
		class CultureComparer :IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				CultureInfo info1=(CultureInfo)x;
				CultureInfo info2=(CultureInfo)y;
				return String.Compare(info1.EnglishName,info2.EnglishName,true);
			}

			#endregion

		}

		public GUISettingsDVD()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_DVD;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_dvd.xml");
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnVideoCodec) OnVideoCodec();
			if (control==btnAudioCodec) OnAudioCodec();
			if (control==btnVideoRenderer) OnVideoRenderer();
			if (control==btnAspectRatio) OnAspectRatio();
			if (control==btnAudioRenderer) OnAudioRenderer();
			if (control==btnSubtitle) OnSubtitle();
			if (control==btnAudioLanguage) OnAudioLanguage();
			base.OnClicked (controlId, control, actionType);
		}
		void OnVideoCodec()
		{
			string strVideoCodec="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				strVideoCodec=xmlreader.GetValueAsString("dvdplayer","videocodec","");
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
				xmlwriter.SetValue("dvdplayer","videocodec",(string)availableVideoFilters[dlg.SelectedLabel]);
			}
		}

		void OnAudioCodec()
		{
			string strAudioCodec="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				strAudioCodec=xmlreader.GetValueAsString("dvdplayer","audiocodec","");
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
				xmlwriter.SetValue("dvdplayer","audiocodec",(string)availableAudioFilters[dlg.SelectedLabel]);
			}
		}
		void OnVideoRenderer()
		{
			int vmr9Index=0;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				vmr9Index= xmlreader.GetValueAsInt("dvdplayer", "vmr9", 0);
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
					xmlwriter.SetValue("dvdplayer", "vmr9", dlg.SelectedLabel.ToString());
				}
			}
		}
		void OnAspectRatio()
		{
			string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };
			string defaultAspectRatio ="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				defaultAspectRatio = xmlreader.GetValueAsString("dvdplayer","defaultar", "normal");
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
					xmlwriter.SetValue("dvdplayer", "defaultar", aspectRatio[dlg.SelectedLabel]);
				}
			}
		}
		void OnAudioRenderer()
		{
			string strAudioRenderer="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				strAudioRenderer=xmlreader.GetValueAsString("dvdplayer","audiorenderer","");
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
				xmlwriter.SetValue("dvdplayer","audiorenderer",(string)availableAudioFilters[dlg.SelectedLabel]);
			}

		}

		void OnSubtitle()
		{
			string defaultSubtitleLanguage="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				defaultSubtitleLanguage= xmlreader.GetValueAsString("dvdplayer","subtitlelanguage", "English");
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				dlg.ShowQuickNumbers=false;
				int selected=0;
				ArrayList cultures = new ArrayList();
				CultureInfo[] culturesInfos=CultureInfo.GetCultures(CultureTypes.NeutralCultures);
				for (int i=0; i < culturesInfos.Length;++i)
				{
					cultures.Add(culturesInfos[i]);
				}
				cultures.Sort( new CultureComparer());

				for (int i=0; i < cultures.Count;++i)
				{
					CultureInfo info = (CultureInfo)cultures[i];
					if(info.EnglishName.Equals(defaultSubtitleLanguage))
					{
						selected=i;
					}
					dlg.Add(info.EnglishName);
				}
				dlg.SelectedLabel=selected;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					CultureInfo info = (CultureInfo)cultures[dlg.SelectedLabel];
					xmlwriter.SetValue("dvdplayer", "subtitlelanguage", info.EnglishName);
				}
			}
		}
		void OnAudioLanguage()
		{
			string defaultAudioLanguage="";
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				defaultAudioLanguage= xmlreader.GetValueAsString("dvdplayer","audiolanguage", "English");
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				dlg.ShowQuickNumbers=false;
				int selected=0;
				ArrayList cultures = new ArrayList();
				CultureInfo[] culturesInfos=CultureInfo.GetCultures(CultureTypes.NeutralCultures);
				for (int i=0; i < culturesInfos.Length;++i)
				{
					cultures.Add(culturesInfos[i]);
				}
				cultures.Sort( new CultureComparer());

				for (int i=0; i < cultures.Count;++i)
				{
					CultureInfo info = (CultureInfo)cultures[i];
					if(info.EnglishName.Equals(defaultAudioLanguage))
					{
						selected=i;
					}
					dlg.Add(info.EnglishName);
				}
				dlg.SelectedLabel=selected;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					CultureInfo info = (CultureInfo)cultures[dlg.SelectedLabel];
					xmlwriter.SetValue("dvdplayer", "audiolanguage", info.EnglishName);
				}
			}
		}
	}
}