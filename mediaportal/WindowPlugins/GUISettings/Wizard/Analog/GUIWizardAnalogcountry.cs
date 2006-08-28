using System;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogcountry.
	/// </summary>
	public class GUIWizardAnalogcountry:GUIWindow, IComparer<GUIListItem>
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCountries=null;
		[SkinControlAttribute(23)]			protected GUIButtonControl btnManual=null;
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
			doc.Load("http://www.team-mediaportal.com/tvsetup/setup.xml");
			XmlNodeList countries = doc.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in countries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				XmlNode nodeCountryCode = nodeCountry.Attributes.GetNamedItem("code");
				GUIListItem item = new GUIListItem();
				item.IsFolder=false;
				item.Label=nodeCountryName.Value;
				item.ItemId=Int32.Parse(nodeCountryCode.Value);
				listCountries.Add(item);
			}
			listCountries.Sort(this);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCountries)
			{
				GUIListItem item=listCountries.SelectedListItem;
				DoScan(item);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_CITY);
				return;
			}
			if (control==btnManual)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_MANUAL_TUNE);
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(GUIListItem item)
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "countryname", item.Label);
				xmlwriter.SetValue("capture", "country", item.ItemId.ToString());
			}			
			GUIPropertyManager.SetProperty("#WizardCountry",item.Label);
		}

    public int Compare(GUIListItem item1, GUIListItem item2)
		{
			return String.Compare(item1.Label,item2.Label);
		}
	}
}
