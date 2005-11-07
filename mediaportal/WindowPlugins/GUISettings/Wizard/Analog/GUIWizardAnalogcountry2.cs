using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using MediaPortal.GUI.Library;
using DShowNET;
namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogCountry2.
	/// </summary>
	public class GUIWizardAnalogCountry2:GUIWindow, IComparer<GUIListItem>
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCountries=null;
		public GUIWizardAnalogCountry2()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_MANUAL_TUNE;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_analog_country2.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadCountries();
		}

		void LoadCountries()
		{
			listCountries.Clear();
			for (int i=0; i < TunerCountries.Countries.Length;++i)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder=false;
				item.Label=TunerCountries.Countries[i].Country;
				item.ItemId=TunerCountries.Countries[i].Id;
				listCountries.Add(item);
			}
			listCountries.Sort(this);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCountries)
			{
				GUIListItem item=listCountries.SelectedListItem;
				DoScan(item.Label,item.ItemId);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_TUNE);
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(string country, int id)
		{
			GUIPropertyManager.SetProperty("#WizardCountry",country);
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "countryname", country);
				xmlwriter.SetValue("capture", "country", id.ToString());
			}
		}

    public int Compare(GUIListItem item1, GUIListItem item2)
		{
			return String.Compare(item1.Label,item2.Label);
		}
	}
}
