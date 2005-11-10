using System;
using System.Collections.Generic;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
namespace WindowPlugins.GUISettings.TV
{
  public class GUISettingsEpg : GUIWindow
  {
    [SkinControlAttribute(10)]
    protected GUICheckListControl listChannels = null;

    public GUISettingsEpg()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_TV_EPG;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_tvEpg.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      listChannels.Clear();
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        GUIListItem item = new GUIListItem();
        item.Label = chan.Name;
        item.IsFolder = false;
        item.ThumbnailImage = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        item.IconImage = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        item.IconImageBig = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        listChannels.Add(item);
      }
    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
    }
  }
}
