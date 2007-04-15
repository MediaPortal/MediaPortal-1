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
