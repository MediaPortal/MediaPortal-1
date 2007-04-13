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
