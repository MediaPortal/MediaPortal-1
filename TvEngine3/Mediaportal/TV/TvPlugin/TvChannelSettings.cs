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

namespace Mediaportal.TV.TvPlugin
{
  public class ChannelSettings : GUIInternalWindow, IComparer<Channel>
  {
    [SkinControl(24)] protected GUIButtonControl btnTvGroup = null;
    [SkinControl(10)] protected GUIUpDownListControl listChannels = null;

    private ChannelGroup _currentGroup
    {
      get { return TVHome.Navigator.CurrentGroup; }
    }

    public ChannelSettings()
    {
      GetID = (int)Window.WINDOW_SETTINGS_SORT_CHANNELS;
    }

    public override bool Init()
    {
      /* Attention: to get this code working, be sure the skinfile uses an "updownlistcontrol" !
       * <type>updownlistcontrol</type> 
       * <!-- type>playlistcontrol</type-->
       */
      return Load(GUIGraphicsContext.Skin + @"\settings_tvSort.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      UpdateList();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      TVHome.Navigator.ReLoad();
    }

    private void UpdateList()
    {
      listChannels.Clear();
      int count = 0;      
      IList<GroupMap> maps = TVHome.Navigator.CurrentGroup.GroupMaps;
      foreach (GroupMap map in maps)
      {
        Channel chan = map.Channel;
        chan.SortOrder = count;
        GUIListItem item = new GUIListItem();
        item.Label = chan.DisplayName;
        item.MusicTag = chan;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.DisplayName);
        if (string.IsNullOrEmpty(strLogo))              
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImage = strLogo;
        item.IconImageBig = strLogo;
        listChannels.Add(item);
        count++;
      }
    }


    protected override void OnClickedUp(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listChannels)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveUp(iItem);
      }
    }

    protected override void OnClickedDown(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listChannels)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveDown(iItem);
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnTvGroup)
      {
        TVHome.OnSelectGroup();
        UpdateList();
      }
      base.OnClicked(controlId, control, actionType);
    }


    private void OnMoveDown(int item)
    {
      if (item + 1 >= listChannels.Count)
      {
        return;
      }
      GUIListItem item1 = listChannels[item];
      Channel chan1 = (Channel)item1.MusicTag;

      GUIListItem item2 = listChannels[item + 1];
      Channel chan2 = (Channel)item2.MusicTag;

      int prio = chan1.SortOrder;
      chan1.SortOrder = chan2.SortOrder;
      chan2.SortOrder = prio;

      if (_currentGroup == null)
      {
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan1);
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan2);        
      }
      else
      {
        List<Channel> channelsInGroup = new List<Channel>();
        IList<GroupMap> maps = TVHome.Navigator.CurrentGroup.GroupMaps;
        foreach (GroupMap map in maps)
        {
          Channel chan = map.Channel;
          channelsInGroup.Add(map.Channel);          
          ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroupMap(map.IdMap);
        }
        SaveGroup(channelsInGroup);
      }
      UpdateList();
      listChannels.SelectedListItemIndex = item + 1;
    }

    private void OnMoveUp(int item)
    {
      if (item < 1)
      {
        return;
      }
      GUIListItem item1 = listChannels[item];
      Channel chan1 = (Channel)item1.MusicTag;

      GUIListItem item2 = listChannels[item - 1];
      Channel chan2 = (Channel)item2.MusicTag;

      int prio = chan1.SortOrder;
      chan1.SortOrder = chan2.SortOrder;
      chan2.SortOrder = prio;
      if (_currentGroup == null)
      {
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan1);
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(chan2);        
      }
      else
      {
        List<Channel> channelsInGroup = new List<Channel>();
        IList<GroupMap> maps = TVHome.Navigator.CurrentGroup.GroupMaps;
        foreach (GroupMap map in maps)
        {
          channelsInGroup.Add(map.Channel);          
          ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroupMap(map.IdMap);
        }
        SaveGroup(channelsInGroup);
      }
      UpdateList();
      listChannels.SelectedListItemIndex = item - 1;
    }

    private void SaveGroup(List<Channel> channelsInGroup)
    {
      if (_currentGroup == null)
      {
        return;
      }
      channelsInGroup.Sort(this);
      MappingHelper.AddChannelsToGroup(channelsInGroup, _currentGroup);      
    }

    #region IComparer Members

    public int Compare(Channel x, Channel y)
    {
      Channel ch1 = x;
      Channel ch2 = y;
      if (ch1.SortOrder < ch2.SortOrder)
      {
        return -1;
      }
      if (ch1.SortOrder > ch2.SortOrder)
      {
        return 1;
      }
      return 0;
    }

    #endregion
  }
}