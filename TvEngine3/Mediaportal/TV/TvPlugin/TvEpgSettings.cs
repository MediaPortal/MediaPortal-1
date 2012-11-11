#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

//using MediaPortal.Utils.Services;

namespace Mediaportal.TV.TvPlugin
{
  public class TvEpgSettings : GUIInternalWindow
  {
    [SkinControl(10)] protected GUICheckListControl listChannels = null;
    [SkinControl(2)] protected GUIButtonControl btnSelectAll = null;
    [SkinControl(3)] protected GUIButtonControl btnSelectNone = null;

    public TvEpgSettings()
    {
      GetID = (int)Window.WINDOW_SETTINGS_TV_EPG;
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
      IEnumerable<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels();
      foreach (Channel chan in channels)
      {
        if (chan.MediaType == (int)MediaTypeEnum.TV)
        {
          continue;
        }
        bool isDigital = false;
        foreach (TuningDetail detail in chan.TuningDetails)
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
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan);
        listChannels.SelectedListItem.Selected = chan.GrabEpg;
      }
      if (control == btnSelectAll)
      {
        IEnumerable<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels();
        foreach (Channel chan in channels)
        {
          if (chan.MediaType == (int)MediaTypeEnum.TV)
          {
            continue;
          }
          if (!chan.GrabEpg)
          {
            chan.GrabEpg = true;
            ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan);            
          }
        }
        Update();
      }
      if (control == btnSelectNone)
      {
        IEnumerable<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels();
        foreach (Channel chan in channels)
        {
          if (chan.MediaType == (int)MediaTypeEnum.TV)
          {
            continue;
          }
          if (chan.GrabEpg)
          {
            chan.GrabEpg = false;
            ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan);
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