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
using DShowNET;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogCountry2.
  /// </summary>
  public class GUIWizardAnalogCountry2 : GUIWindow, IComparer<GUIListItem>
  {
    [SkinControl(24)] protected GUIListControl listCountries = null;

    public GUIWizardAnalogCountry2()
    {
      GetID = (int) Window.WINDOW_WIZARD_ANALOG_MANUAL_TUNE;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_country2_TVE2.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadCountries();
    }

    private void LoadCountries()
    {
      listCountries.Clear();
      for (int i = 0; i < TunerCountries.Countries.Length; ++i)
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = false;
        item.Label = TunerCountries.Countries[i].Country;
        item.ItemId = TunerCountries.Countries[i].Id;
        listCountries.Add(item);
      }
      listCountries.Sort(this);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listCountries)
      {
        GUIListItem item = listCountries.SelectedListItem;
        DoScan(item.Label, item.ItemId);
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_TUNE);
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void DoScan(string country, int id)
    {
      GUIPropertyManager.SetProperty("#WizardCountry", country);
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("capture", "countryname", country);
        xmlwriter.SetValue("capture", "country", id.ToString());
      }
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      return String.Compare(item1.Label, item2.Label);
    }
  }
}