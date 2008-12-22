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
using MediaPortal.Services;
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
  public class TimeShiftTvCommand : CardCommand
  {
    string _channelName;

    public string TvChannel
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    public TimeShiftTvCommand(string channelName)
    {
      if (channelName == null)
        throw new ArgumentNullException("channelname");
      if (channelName.Length == 0)
        throw new ArgumentException("channelname is empty");
      TvChannel = channelName;
    }

    public override void Execute(CommandProcessor handler)
    {
      Log.WriteFile(LogType.Recorder, "Recorder: StartTimeshift tv {0}", _channelName);
      TVCaptureDevice dev;

      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753);// "No tuner cards installed";
        Succeeded = false;
        return;
      }
      //string timeShiftFileName;

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
        Log.WriteFile(LogType.Recorder, "Recorder:  Found card:{0}", dev.CommercialName);

        string fileName = dev.TimeShiftFullFileName;
        ulong freeSpace = MediaPortal.Util.Utils.GetFreeDiskSpace(fileName);
        if (freeSpace < (1024L * 1024L * 1024L) )// 1 GB
        {
          Log.WriteFile(LogType.Recorder, true, "Recorder:  failed to start timeshifting since drive {0}: has less then 1GB freediskspace", fileName[0]);
          ErrorMessage = GUILocalizeStrings.Get(765);// "Not enough free diskspace";
          Succeeded = false;
          return;
        }


        //stop viewing on any other card
        TurnTvOff(handler, cardNo);
        handler.CurrentCardIndex = cardNo;
        handler.TVChannelName = _channelName;

        // do we want timeshifting?
        //yes


        if (!dev.IsRecording && !dev.IsTimeShifting && dev.SupportsTimeShifting)
        {

          Log.WriteFile(LogType.Recorder, "Recorder:  start timeshifting on card:{0}", dev.CommercialName);
          if (dev.StartTimeShifting(_channelName) == false)
          {
            ErrorMessage = String.Format("{0}\r{1}", GUILocalizeStrings.Get(759), dev.GetLastError());//"Failed to start timeshifting";
            Succeeded = false;
            return;
          }
          handler.TuneExternalChannel(_channelName, true);
        }
        else if (dev.TVChannel != _channelName)
        {
          dev.TVChannel = _channelName;
          handler.TuneExternalChannel(_channelName, true);
        }

        handler.OnTvStart(handler.CurrentCardIndex, dev);
        handler.ResetTimeshiftTimer();
        Succeeded = true;
        return;
      }//if (cardNo>=0)

      Log.WriteFile(LogType.Recorder, "Recorder:  find free card");


      // no cards are timeshifting the channel we want.
      // Find a card which can view the channel
      int card = -1;
      int prio = -1;
      bool cardCanViewChannel = false;
      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        dev = handler.TVCards[i];
        if (TVDatabase.CanCardViewTVChannel(_channelName, dev.ID) || handler.TVCards.Count == 1)
        {
          cardCanViewChannel = true;
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
        Log.WriteFile(LogType.Recorder, "Recorder:  No free card which can receive channel [{0}]", _channelName);
        return; // no card available
      }

      TurnTvOff(handler, card);

      handler.CurrentCardIndex = card;
      handler.TVChannelName = _channelName;
      dev = handler.TVCards[handler.CurrentCardIndex];

      Log.WriteFile(LogType.Recorder, "Recorder:  found free card {0} prio:{1} name:{2}", dev.ID, dev.Priority, dev.Graph.CommercialName);

      //do we want to use timeshifting ?
      // yep, then turn timeshifting on

      // yes, does card support it?
      if (!dev.SupportsTimeShifting)
      {
        ErrorMessage = GUILocalizeStrings.Get(760);// "No tuner available for timeshifting";
        Succeeded = false;
        return;
      }
      if (MediaPortal.Util.Utils.GetFreeDiskSpace(dev.TimeShiftFullFileName) < (1024L * 1024L * 1024L))// 1 GB
      {
        Log.WriteFile(LogType.Recorder, true, "Recorder:  failed to start timeshifting since drive {0}: has less then 1GB freediskspace", dev.TimeShiftFullFileName[0]);
        ErrorMessage = GUILocalizeStrings.Get(765);// "Not enough free diskspace";
        Succeeded = false;
        return;
      }
      Log.WriteFile(LogType.Recorder, "Recorder:  start timeshifting card {0} channel:{1}", dev.CommercialName, _channelName);

      if (dev.IsTimeShifting)
      {
        dev.TVChannel = _channelName;
      }

      if (dev.StartTimeShifting(_channelName) == false)
      {
        Succeeded = false;
        ErrorMessage = String.Format("{0}\r{1}", GUILocalizeStrings.Get(759), dev.GetLastError());//"Failed to start timeshifting";
        return;
      }
      handler.TuneExternalChannel(_channelName, true);
      handler.TVChannelName = _channelName;
      handler.OnTvStart(handler.CurrentCardIndex, dev);
      handler.ResetTimeshiftTimer();
      Succeeded = true;
    }

    void TurnTvOff(CommandProcessor handler, int exceptCard)
    {
      StopTvCommand cmd = new StopTvCommand(exceptCard);
      cmd.Execute(handler);
    }
  }
}
