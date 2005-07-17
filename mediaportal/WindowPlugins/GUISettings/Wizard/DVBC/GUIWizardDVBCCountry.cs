using System;
using System.Xml;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUISettings.Wizard.DVBC
{
	/// <summary>
	/// Summary description for GUIWizardDVBCCountry.
	/// </summary>
	public class GUIWizardDVBCCountry : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCountries=null;
		public GUIWizardDVBCCountry()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBC_COUNTRY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_DVBC_country.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadCountries();
		}

		void LoadCountries()
		{
			listCountries.Clear();
			XmlDocument doc= new XmlDocument();
			string [] files = System.IO.Directory.GetFiles( System.IO.Directory.GetCurrentDirectory()+@"\Tuningparameters");
			foreach (string file in files)
			{
				if (file.ToLower().IndexOf(".dvbc") >=0)
				{
					GUIListItem item = new GUIListItem();
					item.IsFolder=false;
					item.Label=System.IO.Path.GetFileNameWithoutExtension(file);
					item.Path=file;
					listCountries.Add(item);
				}
			}
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCountries)
			{
				GUIListItem item=listCountries.SelectedListItem;
				DoScan(item.Path);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_DVBC_SCAN);
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(string country)
		{
			GUIPropertyManager.SetProperty("#WizardCountry",country);
		}
	}
}
