/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
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
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion

namespace MediaPortal.TV.Recording
{
  public class ViewTvCommand : CardCommand
  {
    string _channelName;

    public string TvChannel
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    public ViewTvCommand(string channelName)
    {
      if (channelName == null)
        throw new ArgumentNullException("channelname");
      if (channelName.Length == 0)
        throw new ArgumentException("channelname is empty");
      TvChannel = channelName;
    }

    public override void Execute(CommandProcessor handler)
    {
      string timeShiftFileName;
      TVCaptureDevice dev;
      Log.WriteFile(Log.LogType.Recorder, "Command:  view tv channel:{0}", _channelName);
      
      int cardNo = -1;
      // tv should be turned on
      // check if any card is already tuned to this channel...
      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        dev = handler.TVCards[i];
        //is card already viewing ?
        if (dev.IsTimeShifting || dev.View)
        {
          //can card view the new channel we want?
          if (TVDatabase.CanCardViewTVChannel(_channelName, dev.ID) || handler.TVCards.Count == 1)
          {
            // is it not recording ? or is it recording the channel we want to watch ?
            if (!dev.IsRecording || (dev.IsRecording && dev.TVChannel == _channelName))
            {
              if (dev.IsRecording)
              {
                cardNo = i;
                break;
              }
              cardNo = i;
            }
          }
        }
      }//for (int i=0; i < handler.TVCards.Count;++i)

      if (cardNo >= 0)
      {
        dev = handler.TVCards[cardNo];
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  Found card:{0}", dev.ID);

        //stop viewing on any other card
        TurnTvOff(handler,cardNo);

        handler.CurrentCardIndex = cardNo;
        handler.TVChannelName = _channelName;

        // do we want timeshifting?
        if (dev.IsRecording)
        {
          //yes
          timeShiftFileName = handler.GetTimeShiftFileName(handler.CurrentCardIndex);
          if (g_Player.CurrentFile != timeShiftFileName)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
          if (dev.TVChannel != _channelName)
          {
            handler.TuneExternalChannel(_channelName, true);
            dev.TVChannel = _channelName;
          }
          if (!dev.IsRecording && !dev.IsTimeShifting && dev.SupportsTimeShifting)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  start timeshifting on card:{0}", dev.ID);
            dev.StartTimeShifting(_channelName);
          }

          //yes, check if we're already playing/watching it
          timeShiftFileName = handler.GetTimeShiftFileName(handler.CurrentCardIndex);
          if (g_Player.CurrentFile != timeShiftFileName)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  start viewing timeshift file of card {0}", dev.ID);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
            msg.Label = timeShiftFileName;
            GUIGraphicsContext.SendMessage(msg);
          }
          handler.OnTvStart(handler.CurrentCardIndex, dev);
          handler.ResetTimeshiftTimer();
          return;
        }//if  (dev.IsRecording)
        else
        {
          //we dont want timeshifting so turn timeshifting off
          timeShiftFileName = handler.GetTimeShiftFileName(handler.CurrentCardIndex);
          if (g_Player.CurrentFile == timeShiftFileName)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
          if (dev.IsTimeShifting)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  stop timeshifting on card:{0}", dev.ID);
            dev.StopTimeShifting();
          }
          if (dev.TVChannel != _channelName)
          {
            handler.TuneExternalChannel(_channelName, true);
            dev.TVChannel = _channelName;
          }
          dev.StartViewing(_channelName);
          handler.OnTvStart(handler.CurrentCardIndex, dev);
          handler.ResetTimeshiftTimer();
          return;
        }
      }//if (cardNo>=0)

      Log.WriteFile(Log.LogType.Recorder, "Recorder:  find free card");


      // Find a card which can view the channel
      int card = -1;
      int prio = -1;
      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        dev = handler.TVCards[i];
        if (!dev.IsRecording)
        {
          if (TVDatabase.CanCardViewTVChannel(_channelName, dev.ID) || handler.TVCards.Count == 1)
          {
            if (dev.Priority > prio)
            {
              card = i;
              prio = dev.Priority;
            }
          }
        }
      }

      if (card < 0)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  No free card which can receive channel [{0}]", _channelName);
        return; // no card available
      }
      TurnTvOff(handler, card);

      handler.CurrentCardIndex = card;
      handler.TVChannelName = _channelName;
      dev = handler.TVCards[handler.CurrentCardIndex];

      Log.WriteFile(Log.LogType.Recorder, "Recorder:  found free card {0} prio:{1} name:{2}", dev.ID, dev.Priority, dev.Graph.CommercialName);

      //tv should be turned on without timeshifting
      //just present the overlay tv view
      // now start watching on our card
      Log.WriteFile(Log.LogType.Recorder, "Recorder:  start watching on card:{0} channel:{1}", dev.ID, _channelName);
      handler.TuneExternalChannel(_channelName, true);
      dev.StartViewing(_channelName);
      handler.OnTvStart(handler.CurrentCardIndex, dev);
      handler.ResetTimeshiftTimer();
    }//static public void StartViewing(string channel, bool TVOnOff, bool timeshift)


    void TurnTvOff(CommandProcessor handler, int exceptCard)
    {
      StopTvCommand cmd = new StopTvCommand(exceptCard);
      cmd.Execute(handler);
    }
  }
}
