using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Epg
{
  class EpgChannelUpdate
  {
    string _channelName;
    DateTime _firstEvent;
    DateTime _lastEvent;

    public EpgChannelUpdate(string channelName)
    {
      _channelName = channelName;
      _firstEvent = DateTime.MinValue;
      _lastEvent = DateTime.MinValue;
    }
    public string ChannelName
    {
      get { return _channelName; }
    }
    public void NewEvent(DateTime time)
    {
      if (_firstEvent == DateTime.MinValue)
        _firstEvent = time;
      if (_lastEvent == DateTime.MinValue)
        _lastEvent = time;
      if (time < _firstEvent)
        _firstEvent = time;
      if (time > _lastEvent)
        _lastEvent = time;
    }

    public void Update(ref List<TVChannel> listChannels)
    {
      int hours = -1;
      if (_firstEvent != DateTime.MinValue && _lastEvent != DateTime.MinValue)
      {
        TimeSpan ts = _lastEvent - _firstEvent;
        hours = (int)(ts.TotalHours - 2f);
      }

      foreach (TVChannel ch in listChannels)
      {
        if (String.Compare(ch.Name, _channelName, true) != 0) continue;
        if (ch.LastDateTimeEpgGrabbed < DateTime.Now.AddHours(-2))
        {
          Log.WriteFile(Log.LogType.EPG, "epg: channel:{0} received epg for : {1} hours", _channelName, hours);
          if (hours > 0)
          {
            ch.EpgHours = hours;
          }
          ch.LastDateTimeEpgGrabbed = DateTime.Now;
          TVDatabase.UpdateChannel(ch, ch.Sort);
        }
        else
        {
          Log.WriteFile(Log.LogType.EPG, "epg: channel:{0} received epg for : {1} hours (ignored last update:{2} {3})",
            _channelName, hours, ch.LastDateTimeEpgGrabbed.ToShortDateString(), ch.LastDateTimeEpgGrabbed.ToLongTimeString());
        }
        return;
      }
    }
  }
}
