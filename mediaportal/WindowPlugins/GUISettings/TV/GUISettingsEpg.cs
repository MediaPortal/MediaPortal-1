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

    [SkinControlAttribute(2)]
    protected GUIButtonControl btnSelectAll = null;

    [SkinControlAttribute(3)]
    protected GUIButtonControl btnSelectNone = null;

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
      Update();
    }
    void Update()
    {
      listChannels.Clear();
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (chan.IsDigital)
        {
          GUIListItem item = new GUIListItem();
          item.Label = chan.Name;
          item.IsFolder = false;
          item.ThumbnailImage = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
          item.IconImage = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
          item.IconImageBig = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
          item.Selected = chan.AutoGrabEpg;
          item.TVTag = chan;
          listChannels.Add(item);
        }
      }
    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == listChannels)
      {
        TVChannel chan = listChannels.SelectedListItem.TVTag as TVChannel;
        chan.AutoGrabEpg = !chan.AutoGrabEpg;
        listChannels.SelectedListItem.Selected = chan.AutoGrabEpg;
        TVDatabase.UpdateChannel(chan, chan.Sort);
      }
      if (control == btnSelectAll)
      {
        List<TVChannel> channels = new List<TVChannel>();
        TVDatabase.GetChannels(ref channels);
        foreach (TVChannel chan in channels)
        {
          if (!chan.AutoGrabEpg)
          {
            chan.AutoGrabEpg = true;
            TVDatabase.UpdateChannel(chan, chan.Sort);
          }
        }
        Update();
      }
      if (control == btnSelectNone)
      {
        List<TVChannel> channels = new List<TVChannel>();
        TVDatabase.GetChannels(ref channels);
        foreach (TVChannel chan in channels)
        {
          if (chan.AutoGrabEpg)
          {
            chan.AutoGrabEpg = false;
            TVDatabase.UpdateChannel(chan, chan.Sort);
          }
        }
        Update();
      }
      base.OnClicked(controlId, control, actionType);
    }
  }
}
