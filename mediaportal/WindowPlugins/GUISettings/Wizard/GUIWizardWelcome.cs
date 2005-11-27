using System;
using System.Globalization;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class  GUIWizardWelcome : GUIWindow
	{
        [SkinControlAttribute(5)]
        protected GUIButtonControl btnNext = null;
        [SkinControlAttribute(6)]
        protected GUIButtonControl btnLanguage = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl btnCountry = null;
        [SkinControlAttribute(8)]
        protected GUILabelControl lblLanguage = null;
        [SkinControlAttribute(9)]
        protected GUILabelControl lblCountry = null;

		public  GUIWizardWelcome()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_WELCOME;
		}

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (control == btnNext)
            {
                OnNextPage();
            }
            if (control == btnLanguage)
            {
                OnLanguage();
            }
            if (control == btnCountry)
            {
                OnCountry();
            }
            base.OnClicked(controlId, control, actionType);
        }

        void OnLanguage()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
            dlg.ShowQuickNumbers = false;

            string[] LanguageList = GUILocalizeStrings.SupportedLanguages();
            int selected = 0;
            for (int i = 0; i < LanguageList.Length; ++i)
            {
                dlg.Add(LanguageList[i]);
            }

            dlg.SelectedLabel = selected;
            dlg.ShowQuickNumbers = false;
            dlg.DoModal(GetID);
            if (dlg.SelectedLabel < 0 || dlg.SelectedLabel >= LanguageList.Length)
                return;
            lblLanguage.Label = LanguageList[dlg.SelectedLabel];
        }

        void OnCountry()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
            dlg.ShowQuickNumbers = false;

            int selected = 0;
            for (int i=0; i < TunerCountries.Countries.Length;++i)
			{
                dlg.Add(TunerCountries.Countries[i].Country);
            }

            dlg.SelectedLabel = selected;
            dlg.ShowQuickNumbers = false;
            dlg.DoModal(GetID);
            if (dlg.SelectedLabel < 0 || dlg.SelectedLabel >= TunerCountries.Countries.Length) 
                return;
            lblCountry.Label = TunerCountries.Countries[dlg.SelectedLabel].Country;
            lblCountry.Data = TunerCountries.Countries[dlg.SelectedLabel].Id.ToString();
        }

        void OnNextPage()
        {
            using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
            {
                xmlwriter.SetValue("capture", "countryname", lblCountry.Label);
                xmlwriter.SetValue("capture", "country", (string) lblCountry.Data);
                xmlwriter.SetValue("skin", "language", lblLanguage.Label);
            }

            GUIPropertyManager.SetProperty("#WizardCard", (string)lblCountry.Data);
            GUIPropertyManager.SetProperty("#WizardCountry", lblCountry.Label);

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_GENERAL);
        }

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_welcome.xml");
		}

		protected override void OnPageLoad()
		{
            GUIPropertyManager.SetProperty("#Wizard.General.Done", "no");
			GUIPropertyManager.SetProperty("#Wizard.DVBT.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.DVBC.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.DVBS.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.ATSC.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.Analog.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.Remote.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.EPG.Done","no");
			GUIPropertyManager.SetProperty("#WizardCard","0");

            base.OnPageLoad();
            LoadSettings();
		}

        void LoadSettings()
        {
            using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
            {
                lblCountry.Label = xmlreader.GetValueAsString("capture", "countryname", "");
                lblCountry.Data = xmlreader.GetValueAsString("capture", "country", "");
                lblLanguage.Label = xmlreader.GetValueAsString("skin", "language", "");

                if (lblCountry.Label == "")
                    lblCountry.Label = CountryName(System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);

                if (lblLanguage.Label == "")
                    lblLanguage.Label = GUILocalizeStrings.LocalSupported();

                //lblCountry.Label = CountryName(System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);
                //lblCountry.Data = System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;
            }
        }

        private string CountryName(string ID)
        {
            for (int i = 0; i < TunerCountries.Countries.Length; ++i)
            {
                if (TunerCountries.Countries[i].CountryCode == ID)
                    return TunerCountries.Countries[i].Country;
            }
            return "Unknown";
        }
	}
}
