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

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace WindowPlugins.GUISettings.Epg
{
  /// <summary>
  /// Summary description for GUIWizardEpgMapping.
  /// </summary>
  public class GUIWizardEpgMapping : GUIEpgSelectBase
  {
    //string _country="";
    public GUIWizardEpgMapping()
    {
      GetID = (int) Window.WINDOW_SETTINGS_TV_EPG_MAPPING;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_tvepg_select_TVE2.xml");
    }


    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      MapChannels();
    }

    protected override void OnPageLoad()
    {
      using (Settings xmlReader = new MPSettings())
      {
        string countryCode = xmlReader.GetValueAsString("general", "country", "");
        string country = xmlReader.GetValueAsString("capture", "countryname", "");
        GUIPropertyManager.SetProperty("#WizardCountryCode", countryCode);
        GUIPropertyManager.SetProperty("#WizardCountry", country);
      }
      base.OnPageLoad();
    }
  }
}