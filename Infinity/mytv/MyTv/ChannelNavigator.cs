using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;

namespace MyTv
{
  class ChannelNavigator
  {
    static Channel _selectedChannel;
    static int _currentgroup = 0;
    static List<ChannelGroup> _groups = new List<ChannelGroup>();
    static public void Reload()
    {
      _groups.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "isTv", 1);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
      sb.AddOrderByField(true, "groupName");
      stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
      IList allgroupMaps = GroupMap.ListAll();
      bool found = false;
      foreach (ChannelGroup group in groups)
      {
        if (group.GroupName == "All Channels")//GUILocalizeStrings.Get(972))
        {
          found = true;
          TvBusinessLayer layer = new TvBusinessLayer();
          foreach (Channel channel in channels)
          {
            if (channel.IsTv == false) continue;
            bool groupContainsChannel = false;
            foreach (GroupMap map in allgroupMaps)
            {
              if (map.IdGroup != group.IdGroup) continue;
              if (map.IdChannel == channel.IdChannel)
              {
                groupContainsChannel = true;
                break;
              }
            }
            if (!groupContainsChannel)
            {
              layer.AddChannelToGroup(channel, "All channels");

            }
          }
          break;
        }
      }

      if (!found)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        //MediaPortal.GUI.Library.Log.Info(" group:{0} not found. create it", GUILocalizeStrings.Get(972));
        foreach (Channel channel in channels)
        {
          layer.AddChannelToGroup(channel, "All channels");//GUILocalizeStrings.Get(972));
        }
        //MediaPortal.GUI.Library.Log.Info(" group:{0} created", GUILocalizeStrings.Get(972));
      }

      groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
      {
        //group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
        _groups.Add(group);
      }

    }
    /// <summary>
    /// Gets the currently active channel group.
    /// </summary>
    static public ChannelGroup CurrentGroup
    {
      get { return (ChannelGroup)_groups[_currentgroup]; }
    }
    static public Channel SelectedChannel
    {
      get
      {
        return _selectedChannel;
      }
      set
      {
        _selectedChannel = value;
      }

    }

  }
}
