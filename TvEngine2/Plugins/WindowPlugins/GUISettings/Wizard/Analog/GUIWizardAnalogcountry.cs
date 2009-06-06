#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogcountry.
  /// </summary>
  public class GUIWizardAnalogcountry : GUIWindow, IComparer<GUIListItem>
  {
    [SkinControl(24)] protected GUIListControl listCountries = null;
    [SkinControl(23)] protected GUIButtonControl btnManual = null;

    public GUIWizardAnalogcountry()
    {
      GetID = (int) Window.WINDOW_WIZARD_ANALOG_COUNTRY;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_country_TVE2.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadCountries();
    }

    private void LoadCountries()
    {
      listCountries.Clear();
      XmlDocument doc = new XmlDocument();
      doc = new XmlDocument();
      doc.Load("http://www.team-mediaportal.com/tvsetup/setup.xml");
      XmlNodeList countries = doc.DocumentElement.SelectNodes("/mediaportal/country");
      foreach (XmlNode nodeCountry in countries)
      {
        XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
        XmlNode nodeCountryCode = nodeCountry.Attributes.GetNamedItem("code");
        GUIListItem item = new GUIListItem();
        item.IsFolder = false;
        item.Label = nodeCountryName.Value;
        item.ItemId = Int32.Parse(nodeCountryCode.Value);
        listCountries.Add(item);
      }
      listCountries.Sort(this);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listCountries)
      {
        GUIListItem item = listCountries.SelectedListItem;
        DoScan(item);
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_CITY);
        return;
      }
      if (control == btnManual)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_MANUAL_TUNE);
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void DoScan(GUIListItem item)
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("capture", "countryname", item.Label);
        xmlwriter.SetValue("capture", "country", item.ItemId.ToString());
      }
      GUIPropertyManager.SetProperty("#WizardCountry", item.Label);
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      return String.Compare(item1.Label, item2.Label);
    }
  }
}