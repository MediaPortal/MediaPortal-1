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
      if (epgGrabberSelected)
        MapChannels();
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadEpgGrabberConfig();
    }

    void LoadEpgGrabberConfig()
    {

      EPGConfig config = new EPGConfig(@"webepg");
      config.Load();
      if (config.GetAll().Count == 0) return;
      epgGrabberSelected = true;
      ArrayList mappings = config.GetAll();
      
      listGrabbers.Clear();
      foreach (EPGConfigData data in mappings)
      {
        GUIListItem item = new GUIListItem();
        item.Label = data.DisplayName;
        item.Path = data.ChannelID;

        int idChannel;
        string strTvChannel;
        if (TVDatabase.GetEPGMapping(item.Path, out idChannel, out strTvChannel))
        {
          item.Label2 = strTvChannel;
          item.ItemId = idChannel;
        }
        listGrabbers.Add(item);
      }
    }


  }
}
