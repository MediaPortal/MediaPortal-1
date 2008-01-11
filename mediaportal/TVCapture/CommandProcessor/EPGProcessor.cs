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
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
using MediaPortal.Configuration;

namespace MediaPortal.TV.Recording
{
  class EPGProcessor
  {
    bool _autoGrabEpg = false;
    DateTime _epgTimer = DateTime.MinValue;
    List<TVChannel> _tvChannelsList = new List<TVChannel>();

    public EPGProcessor()
    {
      TVDatabase.GetChannels(ref _tvChannelsList);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _autoGrabEpg = xmlreader.GetValueAsBool("xmltv", "epgdvb", true);
      }
    }

    public void Process(CommandProcessor handler)
    {
      if (_autoGrabEpg == false) return;
      TimeSpan ts = DateTime.Now - _epgTimer;
      if (ts.TotalSeconds < 60) return;

      //Log.WriteFile(LogType.EPG,"epg grabber process");
      bool isGrabbing = false;
      for (int counter = 0; counter < handler.TVCards.Count; counter++)
      {
        TVCaptureDevice card = handler.TVCards[counter];
        //card is empty
        if (card.Network == NetworkType.Analog) continue;
        if (card.IsRadio || card.IsRecording || card.IsTimeShifting || card.View) continue;

        if (card.IsEpgGrabbing)
        {
          isGrabbing = true;
          card.Process();
          if (card.IsEpgFinished)
          {
            card.StopEpgGrabbing();
            //reload tv channels...
            _tvChannelsList.Clear();
            TVDatabase.GetChannels(ref _tvChannelsList);
          }
        }
      }
      if (isGrabbing) return;

      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        TVCaptureDevice card = handler.TVCards[i];
        //card is empty
        if (card.Network == NetworkType.Analog) continue;
        if (card.IsRadio || card.IsRecording || card.IsTimeShifting || card.View) continue;
        //      Log.Info("card :{0} idle", card.ID);
        foreach (TVChannel chan in _tvChannelsList)
        {
          if (handler.IsBusy) break;
          if (!chan.AutoGrabEpg) continue;
          if (handler.TVCards.Count != 1)
          {
            if (TVDatabase.CanCardViewTVChannel(chan.Name, card.ID) == false) continue;
          }

          //Log.WriteFile(LogType.EPG, "  card:{0} ch:{1} epg hrs:{2} last:{3} {4} hrs:{5}", card.ID,chan.Name,chan.EpgHours, chan.LastDateTimeEpgGrabbed.ToShortDateString(), chan.LastDateTimeEpgGrabbed.ToLongTimeString(),ts.TotalHours);
          TVProgram prog = TVDatabase.GetLastProgramForChannel(chan);
          //Log.WriteFile(LogType.EPG,"last prog in tvguide:{0} {1}", prog.EndTime.ToShortDateString(), prog.EndTime.ToLongTimeString());
          if (prog.EndTime < DateTime.Now.AddHours(chan.EpgHours))
          {
            ts = DateTime.Now - chan.LastDateTimeEpgGrabbed;
            if (ts.TotalHours > 2)
            {
              //grab the epg
              Log.WriteFile(LogType.EPG, "auto-epg: card:{0} grab epg for channel:{1} expected:{2} hours, last event in tv guide:{3} {4}, last grab :{5} {6}",
                          card.CommercialName,
                          chan.Name, chan.EpgHours, prog.EndTime.ToShortDateString(), prog.EndTime.ToLongTimeString(),
                           chan.LastDateTimeEpgGrabbed.Date.ToShortDateString(), chan.LastDateTimeEpgGrabbed.Date.ToLongTimeString());
              if (card.GrabEpg(chan))
              {
                chan.LastDateTimeEpgGrabbed = DateTime.Now;
                TVDatabase.UpdateChannel(chan, chan.Sort);
              }
              _epgTimer = DateTime.Now;
              return;
            }
          }
        }
      }
      _epgTimer = DateTime.Now;
    }
  }
}
