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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.TV.Database;
using MediaPortal.EPG.config;
using DShowNET;

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
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_TV_EPG_MAPPING;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_tvepg_select.xml");
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

    }


    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      MapChannels();
    }

    protected override void OnPageLoad()
    {

      using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
