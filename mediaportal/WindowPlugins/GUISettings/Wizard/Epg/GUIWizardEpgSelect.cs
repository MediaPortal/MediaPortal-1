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
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(25)]
    protected GUIButtonControl btnBack = null;
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
      MapChannels();
      GUIPropertyManager.SetProperty("#Wizard.EPG.Done", "yes");
      GUIWizardCardsDetected.ScanNextCardType();
    }
  }
}
