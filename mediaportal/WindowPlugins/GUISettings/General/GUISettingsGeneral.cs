using System;
using System.IO;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

namespace WindowPlugins.GUISettings
{
	/// <summary>
	/// Summary description for GUISettingsGeneral.
	/// </summary>
	public class GUISettingsGeneral : GUIWindow
	{

		[SkinControlAttribute(10)]			protected GUISelectButtonControl btnSkin=null;
		[SkinControlAttribute(11)]			protected GUISelectButtonControl btnLanguage=null;
		[SkinControlAttribute(12)]			protected GUIToggleButtonControl btnFullscreen=null;
		[SkinControlAttribute(13)]			protected GUIToggleButtonControl btnScreenSaver=null;
		[SkinControlAttribute(20)]			protected GUIImage							 imgSkinPreview=null;
		
		string SkinDirectory=@"skin\";
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

		public GUISettingsGeneral()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_SKIN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_general.xml");
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnSkin)
				OnSkinChanged();
			if (control==btnLanguage)
				OnLanguageChanged();
			base.OnClicked (controlId, control, actionType);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();

			SetSkins();
			SetLanguages();
			SetFullScreen();
			SetScreenSaver();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			SaveSettings();
		}

		void SaveSettings()
		{
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("general", "startfullscreen",btnFullscreen.Selected);
				xmlwriter.SetValueAsBool("general", "screensaver",btnScreenSaver.Selected);
				xmlwriter.SetValue("skin", "language", btnLanguage.SelectedLabel);
				xmlwriter.SetValue("skin", "name", btnSkin.SelectedLabel);
			}
		}

		void SetFullScreen()
		{
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				bool fullscreen=xmlreader.GetValueAsBool("general", "startfullscreen", false);
				btnFullscreen.Selected=fullscreen;
			}
		}

		void SetScreenSaver()
		{
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				bool screensaver=xmlreader.GetValueAsBool("general", "screensaver", false);
				btnScreenSaver.Selected=screensaver;
			}
		}

		void SetLanguages()
		{
			GUIControl.ClearControl(GetID, btnLanguage.GetID);
			string currentLanguage=String.Empty;
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				currentLanguage = xmlreader.GetValueAsString("skin", "language", "English");
			}
			string LanguageDirectory = @"language\";
			int lang=0;
			if(Directory.Exists(LanguageDirectory))
			{
				string[] folders = Directory.GetDirectories(LanguageDirectory, "*.*");

				foreach(string folder in folders)
				{
					string fileName = folder.Substring(@"language\".Length);

					//
					// Exclude cvs folder
					//
					if(fileName.ToLower() != "cvs")
					{
						if(fileName.Length > 0)
						{
							fileName = fileName.Substring(0, 1).ToUpper() + fileName.Substring(1);
							GUIControl.AddItemLabelControl(GetID,btnLanguage.GetID,fileName);

							if (fileName.ToLower() == currentLanguage.ToLower())
							{
								GUIControl.SelectItemControl(GetID, btnLanguage.GetID, lang);
							}
							lang++;
						}
					}
				}
			}
		}

		void SetSkins()
		{
			string currentSkin="";
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				currentSkin = xmlreader.GetValueAsString("skin", "name", "BlueTwo");
			}

			GUIControl.ClearControl(GetID, btnSkin.GetID);
			int    skinNo=0;
			if(Directory.Exists(SkinDirectory))
			{
				string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");
				
				foreach(string skinFolder in skinFolders)
				{
					bool		isInvalidDirectory = false;
					string[]	invalidDirectoryNames = new string[] { "cvs" };
					
					string directoryName = skinFolder.Substring(SkinDirectory.Length);

					if(directoryName != null && directoryName.Length > 0)
					{
						foreach(string invalidDirectory in invalidDirectoryNames)
						{
							if(invalidDirectory.Equals(directoryName.ToLower()))
							{
								isInvalidDirectory = true;
								break;
							}
						}

						if(isInvalidDirectory == false)
						{
							//
							// Check if we have a home.xml located in the directory, if so we consider it as a
							// valid skin directory
							//
							string filename=Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
							if(File.Exists(filename))
							{	
								GUIControl.AddItemLabelControl(GetID,btnSkin.GetID,directoryName);
								if (String.Compare(directoryName,currentSkin,true)==0)
								{
									GUIControl.SelectItemControl(GetID, btnSkin.GetID, skinNo);
									imgSkinPreview.SetFileName( Path.Combine(SkinDirectory, Path.Combine(directoryName, @"media\preview.png")));
								}
								skinNo++;
							}
						}
					}
				}
			}
		}

		void OnSkinChanged()
		{
			SaveSettings();
			GUIGraphicsContext.Skin=@"skin\"+btnSkin.SelectedLabel;
      GUITextureManager.Init();
      GUIFontManager.LoadFonts(GUIGraphicsContext.Skin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      GUIFontManager.RestoreDeviceObjects();
			GUIWindowManager.OnResize();
      FreeResources();
      AllocResources();
			OnPageLoad();
			GUIControl.FocusControl(GetID,btnSkin.GetID);
		}
		void OnLanguageChanged()
		{
			SaveSettings();
			GUILocalizeStrings.Load(@"language\" + btnLanguage.SelectedLabel + @"\strings.xml");
			GUIWindowManager.OnResize();
			OnPageLoad();
			GUIControl.FocusControl(GetID,btnLanguage.GetID);
		}
	}
}