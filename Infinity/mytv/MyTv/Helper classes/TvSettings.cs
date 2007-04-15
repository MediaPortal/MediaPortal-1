using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Settings;

namespace MyTv
{
  public class TvSettings
  {
    string _hostName;
    int _currentChannelId;
    string _seekSteps;

    /// <summary>
    /// Gets or sets the  steps used for seeking forward/backward
    /// steps are in seconds
    /// </summary>
    /// <value>The seek steps.</value>
    [Setting(SettingScope.Global, "0,15,30,60,180,300,600,900,1800,3600,7200")]
    public string SeekSteps
    {
      get
      {
        return _seekSteps;
      }
      set
      {
        _seekSteps = value;
      }
    }

    /// <summary>
    /// Gets or sets the hostname of the tvserver
    /// </summary>
    /// <value>The name of the host.</value>
    [Setting(SettingScope.Global, "")]
    public string HostName
    {
      get
      {
        return _hostName;
      }
      set
      {
        _hostName = value;
      }
    }

    /// <summary>
    /// Gets or sets the id of the current tv channel.
    /// </summary>
    /// <value>The id of the current tv channel.</value>
    [Setting(SettingScope.Global, "")]
    public int CurrentChannel
    {
      get
      {
        return _currentChannelId;
      }
      set
      {
        _currentChannelId = value;
      }
    }
  }
}
