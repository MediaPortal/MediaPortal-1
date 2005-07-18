using System;
using System.Collections;
using System.Xml;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogCity.
	/// </summary>
	public class GUIWizardAnalogCity:GUIWindow, IComparer
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCities=null;
		[SkinControlAttribute(23)]			protected GUIListControl btnManual=null;
		public GUIWizardAnalogCity()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_CITY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_analog_city.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadCities();
		}

		void LoadCities()
		{
			string country=GUIPropertyManager.GetProperty("#WizardCountry");
			listCities.Clear();
			XmlDocument doc= new XmlDocument();
			doc = new XmlDocument();
			doc.Load("http://mediaportal.sourceforge.net/tvsetup/setup.xml");
			XmlNodeList countries = doc.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in countries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				if (country==nodeCountryName.Value)
				{
					XmlNodeList cities = nodeCountry.SelectNodes("city");
					foreach (XmlNode nodeCity in cities)
					{
						XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
						XmlNode urlName = nodeCity.SelectSingleNode("analog");

						GUIListItem item = new GUIListItem();
						item.IsFolder=false;
						item.Label=listCitiesName.Value;
						item.Path=urlName.InnerText;
						listCities.Add(item);
					}
				}
			}
			listCities.Sort(this);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCities)
			{
				GUIListItem item=listCities.SelectedListItem;
				DoScan(item.Label, item.Path);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_IMPORTED);
				return;
			}
			if (control==btnManual)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_MANUAL_TUNE);
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(string city, string url)
		{
			GUIPropertyManager.SetProperty("#WizardCity",city);
			GUIPropertyManager.SetProperty("#WizardCityUrl",url);
		}
		#region IComparer Members

		public int Compare(object x, object y)
		{
			GUIListItem item1=(GUIListItem)x;
			GUIListItem item2=(GUIListItem)y;
			return String.Compare(item1.Label,item2.Label);
		}

		#endregion
	}
}
