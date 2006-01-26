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
  /// Summary description for GUIWizardEpgMapping.
  /// </summary>
  public class GUIWizardEpgMapping : GUIEpgSelectBase
  {
    string _country="";
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
      MapChannels(_country);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      EPGConfig config = new EPGConfig(@"webepg");
      config.Load();
      ArrayList list=config.GetAll();
      if (list.Count != 0)
      {
        _country = config.Country;
        if (_country.Length > 0)
        {
          ShowChannelMappingList(_country);
          return;
        }
      }
      LoadGrabbers();

    }

  }
}
