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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.TV.Database;
using MediaPortal.EPG.config;
using DShowNET;

namespace WindowPlugins.GUISettings.Epg
{
  /// <summary>
  /// Summary description for GUIWizardEpgSelect.
  /// </summary>
  public class GUIWizardEpgSelect : GUIEpgSelectBase
  {
    [SkinControlAttribute(5)]    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(25)]   protected GUIButtonControl btnBack = null;

    public GUIWizardEpgSelect()
    {

      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_EPG_SELECT;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_epg_select.xml");
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnNext)
      {
        OnNext();
      }
      if (control == btnBack)
      {
        OnBack();
      }
      base.OnClicked(controlId, control, actionType);
    }

    void OnBack()
    {
      if (epgGrabberSelected)
      {
        epgGrabberSelected = false;
        LoadGrabbers();
        return;
      }
      GUIWindowManager.ShowPreviousWindow();
    }


    void OnNext()
    {
      string country = GUIPropertyManager.GetProperty("#WizardCountryCode");

      MapChannels();
      GUIPropertyManager.SetProperty("#Wizard.EPG.Done", "yes");
      GUIWizardCardsDetected.ScanNextCardType();
    }
  }
}
