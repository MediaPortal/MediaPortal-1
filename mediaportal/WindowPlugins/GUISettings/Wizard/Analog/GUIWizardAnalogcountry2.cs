using System;
using System.Collections;
using System.Xml;
using MediaPortal.GUI.Library;
using DShowNET;
namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogCountry2.
	/// </summary>
	public class GUIWizardAnalogCountry2:GUIWindow, IComparer
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
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_TUNE);
				return;
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
