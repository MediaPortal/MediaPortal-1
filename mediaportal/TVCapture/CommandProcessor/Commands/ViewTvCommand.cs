/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
      _log.Info("Command:  view tv channel:{0}", _channelName);
      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753);//"No tuner cards installed";
        Succeeded = false;
        return;
      }
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
            // is it tuned to the channel we want ?
            if (dev.TVChannel == _channelName)
            {
              cardNo = i;
              if (dev.IsRecording)
              {
                break;
              }
            }
          }
        }
      }//for (int i=0; i < handler.TVCards.Count;++i)

      if (cardNo >= 0)
      {
        dev = handler.TVCards[cardNo];
        _log.Info("Recorder:  Use card:{0}", dev.CommercialName);

        //stop viewing on any other card
        TurnTvOff(handler,cardNo);

        handler.CurrentCardIndex = cardNo;
        handler.TVChannelName = _channelName;

        // is card recording ?
        if (dev.IsRecording)
        {
          //yes, then play the timeshift buffer file
          timeShiftFileName = handler.GetTimeShiftFileName(handler.CurrentCardIndex);
          if (g_Player.Playing && g_Player.CurrentFile != timeShiftFileName)
          {
            handler.StopPlayer();
            
          }
          if (dev.TVChannel != _channelName)
          {
            handler.TuneExternalChannel(_channelName, true);
            dev.TVChannel = _channelName;
          }
          if (!dev.IsRecording && !dev.IsTimeShifting && dev.SupportsTimeShifting)
          {
            _log.Info("Recorder:  start timeshifting on card:{0}", dev.CommercialName);
            if (dev.StartTimeShifting(_channelName) == false)
            {
              ErrorMessage = String.Format("{0}\r{1}", GUILocalizeStrings.Get(759),dev.GetLastError());//"Failed to start timeshifting";

              Succeeded = false;
              return;
            }
          }

          //yes, check if we're already playing/watching it
          /*
          timeShiftFileName = handler.GetTimeShiftFileName(handler.CurrentCardIndex);
          if (g_Player.CurrentFile != timeShiftFileName)
          {
            _log.Info("Recorder:  start viewing timeshift file of card {0}", dev.CommercialName);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
            msg.Label = timeShiftFileName;
            GUIGraphicsContext.SendMessage(msg);
          }*/
          handler.OnTvStart(handler.CurrentCardIndex, dev);
          handler.ResetTimeshiftTimer();
          Succeeded = true;
          return;
        }//if  (dev.IsRecording)
        else
        {
          //we dont want timeshifting so turn timeshifting off
          timeShiftFileName = handler.GetTimeShiftFileName(handler.CurrentCardIndex);
          if (g_Player.Playing && g_Player.CurrentFile == timeShiftFileName)
          {
            handler.StopPlayer();
          }
          if (dev.IsTimeShifting)
          {
            _log.Info("Recorder:  stop timeshifting on card:{0}", dev.CommercialName);
            if (false == dev.StopTimeShifting())
            {
              ErrorMessage = String.Format("{0}\r{1}", GUILocalizeStrings.Get(761), dev.GetLastError());//"Failed to stop timeshifting";
              Succeeded = false;
              return;
            }
          }
          if (dev.TVChannel != _channelName)
          {
            handler.TuneExternalChannel(_channelName, true);
            //dev.TVChannel = _channelName;
          }
          if (dev.StartViewing(_channelName) == false)
          {
            Succeeded = false;
            ErrorMessage = String.Format("{0}\r{1}", GUILocalizeStrings.Get(762), dev.GetLastError());//"Failed to start tv";
          }
          handler.OnTvStart(handler.CurrentCardIndex, dev);
          handler.ResetTimeshiftTimer();
          Succeeded = true;
          return;
        }
      }//if (cardNo>=0)

      _log.Info("Recorder:  find free card");

      bool cardCanViewChannel = false;
      // Find a card which can view the channel
      int card = -1;
      int prio = -1;
      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        dev = handler.TVCards[i];
        _log.Info("Analysing Card {0}",i.ToString());
        if (TVDatabase.CanCardViewTVChannel(_channelName, dev.ID) || handler.TVCards.Count == 1)
        {
          cardCanViewChannel = true;
          _log.Info("Card {0} can view channel {1} recording={2}", i.ToString(), _channelName, dev.IsRecording.ToString());
          if (!dev.IsRecording)
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
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(757);// "All tuners are busy";
        if (cardCanViewChannel == false)
        {
          ErrorMessage = String.Format(GUILocalizeStrings.Get(756), _channelName);//No tuner can receive:{0}
        }
        _log.Info("Recorder:  No free card which can receive channel [{0}]", _channelName);
        return; // no card available
      }
      TurnTvOff(handler, card);
      handler.CurrentCardIndex = card;
      handler.TVChannelName = _channelName;
      dev = handler.TVCards[handler.CurrentCardIndex];

      _log.Info("Recorder:  use free card {0} prio:{1} name:{2}", dev.CommercialName, dev.Priority, dev.Graph.CommercialName);

      //tv should be turned on without timeshifting
      // now start watching on our card
      _log.Info("Recorder:  start watching on card:{0} channel:{1}", dev.CommercialName, _channelName);
      handler.TuneExternalChannel(_channelName, true);
      if (dev.StartViewing(_channelName) == false)
      {
        ErrorMessage = String.Format("{0}\r{1}", GUILocalizeStrings.Get(762), dev.GetLastError());//"Failed to start tv";
        Succeeded = false;
        return;
      }
      handler.OnTvStart(handler.CurrentCardIndex, dev);
      handler.ResetTimeshiftTimer();
      Succeeded = true;
    }//static public void StartViewing(string channel, bool TVOnOff, bool timeshift)


    void TurnTvOff(CommandProcessor handler, int exceptCard)
    {
      StopTvCommand cmd = new StopTvCommand(exceptCard);
      cmd.Execute(handler);
    }
  }
}
