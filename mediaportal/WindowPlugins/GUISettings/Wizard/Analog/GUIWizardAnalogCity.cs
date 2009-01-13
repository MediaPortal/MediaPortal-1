#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogCity.
  /// </summary>
  public class GUIWizardAnalogCity : GUIWindow, IComparer<GUIListItem>
  {
    [SkinControl(23)] protected GUIButtonControl btnManual = null;
    [SkinControl(27)] protected GUIButtonControl btnSkip = null;
    [SkinControl(26)] protected GUILabelControl lblCountry = null;
    [SkinControl(24)] protected GUIListControl listCities = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;

    public GUIWizardAnalogCity()
    {
      GetID = (int) Window.WINDOW_WIZARD_ANALOG_CITY;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_city.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadCities();
    }

    private void LoadCities()
    {
      string country = GUIPropertyManager.GetProperty("#WizardCountry");
      bool internetAccess = bool.Parse(GUIPropertyManager.GetProperty("#InternetAccess"));

      lblCountry.Label = country;

      if (internetAccess)
      {
        listCities.Clear();
        XmlDocument doc = new XmlDocument();
        doc = new XmlDocument();
        doc.Load("http://www.team-mediaportal.com/tvsetup/setup.xml");
        XmlNodeList countries = doc.DocumentElement.SelectNodes("/mediaportal/country");
        foreach (XmlNode nodeCountry in countries)
        {
          XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
          if (country == nodeCountryName.Value)
          {
            XmlNodeList cities = nodeCountry.SelectNodes("city");
            foreach (XmlNode nodeCity in cities)
            {
              XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
              XmlNode urlName = nodeCity.SelectSingleNode("analog");

              GUIListItem item = new GUIListItem();
              item.IsFolder = false;
              item.Label = listCitiesName.Value;
              item.Path = urlName.InnerText;
              listCities.Add(item);
            }
          }
        }
        if (listCities.Count == 0)
        {
          GUIControl.FocusControl(GetID, btnManual.GetID);
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Label = "No Cities Found";
          item.Path = "none";
          listCities.Add(item);
        }

        listCities.Sort(this);
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listCities)
      {
        GUIListItem item = listCities.SelectedListItem;
        DoScan(item.Label, item.Path);
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_IMPORTED);
        return;
      }
      if (control == btnManual)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_TUNE); //MANUAL_TUNE);
      }
      if (control == btnSkip)
      {
        GUIPropertyManager.SetProperty("#Wizard.Analog.Done", "yes");
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_EPG_SELECT);
      }
      if (btnBack == control)
      {
        GUIWindowManager.ShowPreviousWindow();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void DoScan(string city, string url)
    {
      GUIPropertyManager.SetProperty("#WizardCity", city);
      GUIPropertyManager.SetProperty("#WizardCityUrl", url);
    }

    #region IComparer Members

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      return String.Compare(item1.Label, item2.Label);
    }

    #endregion
  }
}