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
  public class StopTvCommand : CardCommand
  {
    int _cardNumber=-1;
    public int CardNo
    {
      get { return _cardNumber; }
      set { _cardNumber=value; }
    }
    public StopTvCommand()
    {
    }

    public StopTvCommand(int cardNumber)
    {
      CardNo = cardNumber;
    }

    public override void Execute(CommandProcessor handler)
    {
      Log.WriteFile(LogType.Recorder, "Command:Stop all card except card:{0}", CardNo);
      
      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753);// "No tuner cards installed";
        Succeeded = false;
        return;
      }
      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        if (i == CardNo) continue;

        bool stopped = false;

        //stop playback of timeshift buffer file
        TVCaptureDevice dev = handler.TVCards[i];
        string timeShiftFileName = handler.GetTimeShiftFileName(i);
        if (g_Player.Playing && g_Player.CurrentFile == timeShiftFileName)
        {
          Log.WriteFile(LogType.Recorder, "Recorder:  stop playing timeshifting file for card:{0}", dev.CommercialName);

          handler.StopPlayer();
          stopped = true;
        }

        //if card is not recording, then stop the card
        if (!dev.IsRecording)
        {
          if (dev.IsTimeShifting || dev.View || dev.IsRadio)
          {
            stopped = (dev.View);
            Log.WriteFile(LogType.Recorder, "Recorder:  stop card:{0}", dev.CommercialName);
            
            dev.StopTimeShifting();
            dev.StopViewing();
            dev.StopRadio();
          }
        }
        if (stopped )
          handler.OnTvStopped(i, dev);

        if (i == handler.CurrentCardIndex)
        {
          handler.CurrentCardIndex = -1;
          handler.TVChannelName = string.Empty;
        }
      }
      Succeeded = true;
    }
  }
}
