using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Settings;
namespace MyTv
{
  public class TvChannelNavigator : ITvChannelNavigator, INotifyPropertyChanged
  {
    #region variables
    static TvChannelNavigator _instance = null;
    VirtualCard _card;
    Channel _selectedChannel;
    int _currentgroup = 0;
    List<ChannelGroup> _groups = new List<ChannelGroup>();
    public event PropertyChangedEventHandler PropertyChanged;
    System.Windows.Threading.DispatcherTimer _isRecordingTimer;
    bool _isRecording;
    bool _initialized = false;
    #endregion

    #region ctors
    /// <summary>
    /// Gets the ChannelNavigator instance.
    /// </summary>
    /// <value>The instance.</value>
    static public TvChannelNavigator Instance
    {
      get
      {
        if (_instance == null)
          _instance = new TvChannelNavigator();
        return _instance;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelNavigator"/> class.
    /// </summary>
    public TvChannelNavigator()
    {
      ServiceScope.Add<ITvChannelNavigator>(this);
      _isRecordingTimer = new System.Windows.Threading.DispatcherTimer();
      _isRecordingTimer.Interval = new TimeSpan(0, 0, 1);
      _isRecordingTimer.IsEnabled = false;
      _isRecordingTimer.Tick += new EventHandler(OnCheckIsRecording);
    }
    #endregion

    #region dispatch timer callback
    /// <summary>
    /// timer callback which checks if the tvserver is recording...
    /// </summary>
    void OnCheckIsRecording(object sender, EventArgs e)
    {
      try
      {
        if (!IsInitialized) return;
        if (RemoteControl.IsConnected == false) return;
        IsRecording = RemoteControl.Instance.IsAnyCardRecording();
      }
      catch (Exception)
      {
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Loads the tvgroups/channels from the database.
    /// </summary>
    public void Initialize()
    {
      ServiceScope.Get<ILogger>().Info("Navigator.Initialize");
      _groups.Clear();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "isTv", 1);
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      ServiceScope.Get<ILogger>().Info("Navigator #1");
      sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
      sb.AddOrderByField(true, "groupName");
      stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
      IList allgroupMaps = GroupMap.ListAll();
      bool found = false;

      ServiceScope.Get<ILogger>().Info("Navigator #2");
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

      ServiceScope.Get<ILogger>().Info("Navigator #3");
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
      ServiceScope.Get<ILogger>().Info("Navigator #4");

      groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
      {
        //group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
        _groups.Add(group);
      }

      ServiceScope.Get<ILogger>().Info("Navigator #5");
      TvSettings settings = new TvSettings();
      ServiceScope.Get<ISettingsManager>().Load(settings, "configuration.xml");
      int channelId = settings.CurrentChannel;
      try
      {
        SelectedChannel = Channel.Retrieve(channelId);
      }
      catch (Exception)
      {
      }
      ServiceScope.Get<ILogger>().Info("Navigator initialized");
      _isRecordingTimer.IsEnabled = true;
      _initialized = true;
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs("IsInitialized"));
      }
    }
    #endregion

    #region properties
    public bool IsInitialized
    {
      get
      {
        return _initialized;
      }
    }
    /// <summary>
    /// Gets the currently active channel group.
    /// </summary>
    public ChannelGroup CurrentGroup
    {

      get
      {
        if (_groups == null) return null;
        if (_currentgroup < 0 || _currentgroup >= _groups.Count) return null;
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
        if (value != null && _selectedChannel != null)
        {
          if (value.IdChannel == _selectedChannel.IdChannel) return;
        }
        _selectedChannel = value;

        TvSettings settings = new TvSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings, "configuration.xml");
        if (_selectedChannel!=null)
          settings.CurrentChannel=_selectedChannel.IdChannel;
        else
          settings.CurrentChannel=-1;
        ServiceScope.Get<ISettingsManager>().Save(settings, "configuration.xml");
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("SelectedChannel"));
        }
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

    /// <summary>
    /// Gets or sets a value indicating whether the tvserver is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsRecording
    {
      get
      {
        return _isRecording;
      }
      set
      {
        if (value != _isRecording)
        {
          _isRecording = value;
          if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs("IsRecording"));
        }
      }
    }
    #endregion
  }
}
