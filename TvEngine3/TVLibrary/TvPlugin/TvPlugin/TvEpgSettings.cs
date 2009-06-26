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

using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using TvDatabase;
//using MediaPortal.Utils.Services;

namespace TvPlugin
{
  public class TvEpgSettings : GUIWindow
  {
    [SkinControl(10)] protected GUICheckListControl listChannels = null;
    [SkinControl(2)] protected GUIButtonControl btnSelectAll = null;
    [SkinControl(3)] protected GUIButtonControl btnSelectNone = null;

    public TvEpgSettings()
    {
      GetID = (int) Window.WINDOW_SETTINGS_TV_EPG;
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

    private void Update()
    {
      listChannels.Clear();
      IList<Channel> channels = Channel.ListAll();
      foreach (Channel chan in channels)
      {
        if (chan.IsTv)
        {
          continue;
        }
        bool isDigital = false;
        foreach (TuningDetail detail in chan.ReferringTuningDetail())
        {
          if (detail.ChannelType != 0)
          {
            isDigital = true;
            break;
          }
        }
        if (isDigital)
        {
          GUIListItem item = new GUIListItem();
          item.Label = chan.DisplayName;
          item.IsFolder = false;
          item.ThumbnailImage = Utils.GetCoverArt(Thumbs.TVChannel, chan.DisplayName);
          item.IconImage = Utils.GetCoverArt(Thumbs.TVChannel, chan.DisplayName);
          item.IconImageBig = Utils.GetCoverArt(Thumbs.TVChannel, chan.DisplayName);
          item.Selected = chan.GrabEpg;
          item.TVTag = chan;
          listChannels.Add(item);
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listChannels)
      {
        Channel chan = listChannels.SelectedListItem.TVTag as Channel;
        chan.GrabEpg = !chan.GrabEpg;
        chan.Persist();
        listChannels.SelectedListItem.Selected = chan.GrabEpg;
      }
      if (control == btnSelectAll)
      {
        IList<Channel> channels = Channel.ListAll();
        foreach (Channel chan in channels)
        {
          if (chan.IsTv)
          {
            continue;
          }
          if (!chan.GrabEpg)
          {
            chan.GrabEpg = true;
            chan.Persist();
          }
        }
        Update();
      }
      if (control == btnSelectNone)
      {
        IList<Channel> channels = Channel.ListAll();
        foreach (Channel chan in channels)
        {
          if (chan.IsTv)
          {
            continue;
          }
          if (chan.GrabEpg)
          {
            chan.GrabEpg = false;
            chan.Persist();
          }
        }
        Update();
      }
      base.OnClicked(controlId, control, actionType);
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}