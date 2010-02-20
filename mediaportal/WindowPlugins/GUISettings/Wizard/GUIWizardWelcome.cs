#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Globalization;
using DShowNET;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIWizardWelcome : GUIInternalWindow
  {
    [SkinControl(5)] protected GUIButtonControl btnNext = null;
    [SkinControl(6)] protected GUIButtonControl btnLanguage = null;
    [SkinControl(7)] protected GUIButtonControl btnCountry = null;
    [SkinControl(8)] protected GUILabelControl lblLanguage = null;
    [SkinControl(9)] protected GUILabelControl lblCountry = null;

    public GUIWizardWelcome()
    {
      GetID = (int)Window.WINDOW_WIZARD_WELCOME;
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
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

    private void OnLanguage()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu
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
      {
        return;
      }
      lblLanguage.Label = LanguageList[dlg.SelectedLabel];
    }

    private void OnCountry()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu
      dlg.ShowQuickNumbers = false;

      int selected = 0;
      for (int i = 0; i < TunerCountries.Countries.Length; ++i)
      {
        dlg.Add(TunerCountries.Countries[i].Country);
      }

      dlg.SelectedLabel = selected;
      dlg.ShowQuickNumbers = false;
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0 || dlg.SelectedLabel >= TunerCountries.Countries.Length)
      {
        return;
      }
      lblCountry.Label = TunerCountries.Countries[dlg.SelectedLabel].Country;
      lblCountry.Data = TunerCountries.Countries[dlg.SelectedLabel];
    }

    private void OnNextPage()
    {
      TunerCountry country = (TunerCountry)lblCountry.Data;

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("general", "country", country.CountryCode);
        xmlwriter.SetValue("capture", "countryname", country.Country);
        xmlwriter.SetValue("capture", "country", country.Id.ToString());
        xmlwriter.SetValue("skin", "language", lblLanguage.Label);
      }


      GUIPropertyManager.SetProperty("#WizardCard", country.Id.ToString());
      GUIPropertyManager.SetProperty("#WizardCountryCode", country.CountryCode);
      GUIPropertyManager.SetProperty("#WizardCountry", country.Country);

      GUIWindowManager.ActivateWindow((int)Window.WINDOW_WIZARD_GENERAL);
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_welcome.xml");
    }

    protected override void OnPageLoad()
    {
      GUIPropertyManager.SetProperty("#Wizard.General.Done", "no");
      GUIPropertyManager.SetProperty("#Wizard.Remote.Done", "no");
      GUIPropertyManager.SetProperty("#WizardCard", "0");

      base.OnPageLoad();
      LoadSettings();
    }

    private void LoadSettings()
    {
      string countryCode = "";
      string language = "";

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        countryCode = xmlreader.GetValueAsString("general", "country", "");
        language = xmlreader.GetValueAsString("skin", "language", "");
      }

      if (countryCode == "")
      {
        countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
      }

      if (language == "")
      {
        language = GUILocalizeStrings.LocalSupported();
      }

      TunerCountry country = FindCountry(countryCode);

      lblCountry.Label = country.Country;
      lblCountry.Data = country;
      lblLanguage.Label = language;
    }

    private TunerCountry FindCountry(string ID)
    {
      for (int i = 0; i < TunerCountries.Countries.Length; ++i)
      {
        if (TunerCountries.Countries[i].CountryCode == ID)
        {
          return TunerCountries.Countries[i];
        }
      }
      TunerCountry unknown = new TunerCountry(-1, "Unknown", "");
      return unknown;
    }
  }
}