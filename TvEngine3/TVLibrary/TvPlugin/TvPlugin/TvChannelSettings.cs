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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using AMS.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
//using MediaPortal.Utils.Services;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;


using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  public class ChannelSettings : GUIWindow, IComparer<Channel>
  {
    [SkinControlAttribute(24)]    protected GUIButtonControl btnTvGroup = null;
    [SkinControlAttribute(10)]    protected GUIUpDownListControl listChannels = null;

    ChannelGroup _currentGroup = null;
    public ChannelSettings()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_SORT_CHANNELS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_tvSort.xml");
    }

    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_SETTINGS_SORT_CHANNELS, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    protected override void OnPageLoad()
    {
      _currentGroup = null;
      base.OnPageLoad();
      UpdateList();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      TVHome.Navigator.ReLoad();

    }

    void UpdateList()
    {
      IList channels;
      listChannels.Clear();
      if (_currentGroup == null)
      {
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Equals, "isTv", 1);
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        int count = 0;
        foreach (Channel chan in channels)
        {
          if (chan.SortOrder != count)
          {
            chan.SortOrder = count;
            chan.Persist();
          }
          GUIListItem item = new GUIListItem();
          item.Label = chan.DisplayName;
          item.MusicTag = chan;
          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.DisplayName);
          if (!System.IO.File.Exists(strLogo))
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
      else
      {
        int count = 0;
        IList<GroupMap> maps = _currentGroup.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          Channel chan = map.ReferencedChannel();
          chan.SortOrder = count;
          GUIListItem item = new GUIListItem();
          item.Label = chan.DisplayName;
          item.MusicTag = chan;
          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.DisplayName);
          if (!System.IO.File.Exists(strLogo))
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
    }


    protected override void OnClickedUp(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listChannels)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveUp(iItem);
      }
    }
    protected override void OnClickedDown(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listChannels)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveDown(iItem);
      }
    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnTvGroup) OnTvGroup();
      base.OnClicked(controlId, control, actionType);
    }


    void OnMoveDown(int item)
    {
      if (item + 1 >= listChannels.Count) return;
      GUIListItem item1 = listChannels[item];
      Channel chan1 = (Channel)item1.MusicTag;

      GUIListItem item2 = listChannels[item + 1];
      Channel chan2 = (Channel)item2.MusicTag;

      int prio = chan1.SortOrder;
      chan1.SortOrder = chan2.SortOrder;
      chan2.SortOrder = prio;

      if (_currentGroup == null)
      {
        chan1.Persist();
        chan2.Persist();
      }
      else
      {
        List<Channel> channelsInGroup = new List<Channel>();
        IList<GroupMap> maps = _currentGroup.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          Channel chan = map.ReferencedChannel();
          channelsInGroup.Add(map.ReferencedChannel());
          map.Remove();
        }
        SaveGroup(channelsInGroup);
      }
      UpdateList();
      listChannels.SelectedListItemIndex = item + 1;
    }

    void OnMoveUp(int item)
    {
      if (item < 1) return;
      GUIListItem item1 = listChannels[item];
      Channel chan1 = (Channel)item1.MusicTag;

      GUIListItem item2 = listChannels[item - 1];
      Channel chan2 = (Channel)item2.MusicTag;

      int prio = chan1.SortOrder;
      chan1.SortOrder = chan2.SortOrder;
      chan2.SortOrder = prio;
      if (_currentGroup == null)
      {
        chan1.Persist();
        chan2.Persist();
      }
      else
      {
        List<Channel> channelsInGroup = new List<Channel>();
        IList<GroupMap> maps = _currentGroup.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          Channel chan = map.ReferencedChannel();
          channelsInGroup.Add(map.ReferencedChannel());
          map.Remove();
        }
        SaveGroup(channelsInGroup);
      }
      UpdateList();
      listChannels.SelectedListItemIndex = item - 1;
    }

    void OnTvGroup()
    {
      IList<ChannelGroup> tvGroups = ChannelGroup.ListAll();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
        dlg.SelectedLabel = 0;
        dlg.Add("All channels");
        for (int i = 0; i < tvGroups.Count; ++i)
        {
          ChannelGroup group = (ChannelGroup)tvGroups[i];
          dlg.Add(group.GroupName);
          if (_currentGroup != null)
          {
            if (group.GroupName == _currentGroup.GroupName)
            {
              dlg.SelectedLabel = i + 1;
            }
          }
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == 0)
        {
          _currentGroup = null;
          UpdateList();
          return;
        }

        if (dlg.SelectedLabel > 0)
        {
          _currentGroup = (ChannelGroup)tvGroups[dlg.SelectedLabel - 1];
          UpdateList();
        }
      }
    }
    void SaveGroup(List<Channel> channelsInGroup)
    {
      if (_currentGroup == null) return;
      channelsInGroup.Sort(this);
      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (Channel ch in channelsInGroup)
      {
        layer.AddChannelToGroup(ch, _currentGroup.GroupName);
      }
    }
    #region IComparer Members

    public int Compare(Channel x, Channel y)
    {
      Channel ch1 = (Channel)x;
      Channel ch2 = (Channel)y;
      if (ch1.SortOrder < ch2.SortOrder) return -1;
      if (ch1.SortOrder > ch2.SortOrder) return 1;
      return 0;
    }

    #endregion
  }
}
