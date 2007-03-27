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
    #region variables
    static ChannelNavigator _instance = null;
    VirtualCard _card;
    Channel _selectedChannel;
    int _currentgroup = 0;
    List<ChannelGroup> _groups = new List<ChannelGroup>();
    #endregion

    /// <summary>
    /// Gets the ChannelNavigator instance.
    /// </summary>
    /// <value>The instance.</value>
    static public ChannelNavigator Instance
    {
      get
      {
        if (_instance == null)
          _instance = new ChannelNavigator();
        return _instance;
      }
    }
    /// <summary>
    /// Loads the tvgroups/channels from the database.
    /// </summary>
    public void Initialize()
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

      int channelId = UserSettings.GetInt("tv", "channel");
      SelectedChannel = Channel.Retrieve(channelId);
    }
    /// <summary>
    /// Gets the currently active channel group.
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      
      get {
        if (_groups == null) return null;
        if (_currentgroup < 0 || _currentgroup>=_groups.Count ) return null;
        return (ChannelGroup)_groups[_currentgroup]; 
      }
    }
    /// <summary>
    /// Gets or sets the selected channel.
    /// </summary>
    /// <value>The selected channel.</value>
    public Channel SelectedChannel
    {
      get
      {
        return _selectedChannel;
      }
      set
      {
        if (value != _selectedChannel && value != null)
        {
          UserSettings.SetInt("tv", "channel", value.IdChannel);
        }
        _selectedChannel = value;
      }

    }

    /// <summary>
    /// Gets or sets the card.
    /// </summary>
    /// <value>The card.</value>
    public VirtualCard Card
    {
      get
      {
        return _card;
      }
      set
      {
        _card = value;
      }
    }
  }
}
