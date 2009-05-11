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
