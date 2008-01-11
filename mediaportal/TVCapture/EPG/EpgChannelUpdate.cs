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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Radio.Database;
using MediaPortal.Services;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Epg
{
  class EpgChannelUpdate
  {
    bool _isTv;
    string _channelName;
    DateTime _firstEvent;
    DateTime _lastEvent;

    public EpgChannelUpdate(bool isTv,string channelName)
    {

      _isTv = isTv;
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

    public void Update(ref List<TVChannel> listChannels,ref ArrayList stations)
    {
      int hours = -1;
      if (_firstEvent != DateTime.MinValue && _lastEvent != DateTime.MinValue)
      {
        TimeSpan ts = _lastEvent - _firstEvent;
        hours = (int)(ts.TotalHours - 2f);
      }

      if (_isTv)
      {
        foreach (TVChannel ch in listChannels)
        {
          if (String.Compare(ch.Name, _channelName, true) != 0) continue;
          if (ch.LastDateTimeEpgGrabbed < DateTime.Now.AddHours(-2))
          {
            Log.WriteFile(LogType.EPG, "epg: channel:{0} received epg for : {1} hours", _channelName, hours);
            if (hours > 0)
            {
              ch.EpgHours = hours;
            }
            ch.LastDateTimeEpgGrabbed = DateTime.Now;
            TVDatabase.UpdateChannel(ch, ch.Sort);
          }
          else
          {
            Log.WriteFile(LogType.EPG, "epg: channel:{0} received epg for : {1} hours (ignored last update:{2} {3})",
              _channelName, hours, ch.LastDateTimeEpgGrabbed.ToShortDateString(), ch.LastDateTimeEpgGrabbed.ToLongTimeString());
          }
          return;
        }
      }
      else
      {
        foreach (RadioStation ch in stations)
        {
          if (String.Compare(ch.Name, _channelName, true) != 0) continue;
          if (ch.LastDateTimeEpgGrabbed < DateTime.Now.AddHours(-2))
          {
            Log.WriteFile(LogType.EPG, "epg: channel:{0} received epg for : {1} hours", _channelName, hours);
            if (hours > 0)
            {
              ch.EpgHours = hours;
            }
            ch.LastDateTimeEpgGrabbed = DateTime.Now;
            RadioDatabase.UpdateStation(ch);
          }
          else
          {
            Log.WriteFile(LogType.EPG, "epg: station:{0} received epg for : {1} hours (ignored last update:{2} {3})",
              _channelName, hours, ch.LastDateTimeEpgGrabbed.ToShortDateString(), ch.LastDateTimeEpgGrabbed.ToLongTimeString());
          }
          return;
        }
      }
    }
  }
}
