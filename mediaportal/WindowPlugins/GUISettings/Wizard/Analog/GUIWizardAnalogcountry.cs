using System;
using System.Collections;
using System.Xml;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogcountry.
	/// </summary>
	public class GUIWizardAnalogcountry:GUIWindow, IComparer
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCountries=null;
		[SkinControlAttribute(23)]			protected GUIListControl btnManual=null;
		public GUIWizardAnalogcountry()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_COUNTRY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_analog_country.xml");
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
			doc = new XmlDocument();
			doc.Load("http://mediaportal.sourceforge.net/tvsetup/setup.xml");
			XmlNodeList countries = doc.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in countries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				GUIListItem item = new GUIListItem();
				item.IsFolder=false;
				item.Label=nodeCountryName.Value;
				listCountries.Add(item);
			}
			listCountries.Sort(this);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCountries)
			{
				GUIListItem item=listCountries.SelectedListItem;
				DoScan(item.Label);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_CITY);
				return;
			}
			if (control==btnManual)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_MANUAL_TUNE);
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(string country)
		{
			GUIPropertyManager.SetProperty("#WizardCountry",country);
		}

		public int Compare(object x, object y)
		{
			GUIListItem item1=(GUIListItem)x;
			GUIListItem item2=(GUIListItem)y;
			return String.Compare(item1.Label,item2.Label);
		}
	}
}
