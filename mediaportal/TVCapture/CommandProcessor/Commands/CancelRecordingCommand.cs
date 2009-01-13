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
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.TV.Database;

#endregion

namespace MediaPortal.TV.Recording
{
  public class CancelRecordingCommand : CardCommand
  {
    private TVRecording _recordingToStop;

    public CancelRecordingCommand(TVRecording recordingToStop)
    {
      _recordingToStop = recordingToStop;
    }

    public override void Execute(CommandProcessor handler)
    {
      if (_recordingToStop == null)
      {
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(752); // "No recording specified";
        return;
      }

      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753); //"No tuner cards installed";
        Succeeded = false;
        return;
      }
      bool stopped = false;
      Log.WriteFile(LogType.Recorder, "Command:Cancel recording {0} {1}-{2}",
                    _recordingToStop.Title,
                    _recordingToStop.StartTime.ToLongTimeString(),
                    _recordingToStop.EndTime.ToLongTimeString());

      //find card which currently records the 'rec'
      for (int card = 0; card < handler.TVCards.Count; ++card)
      {
        TVCaptureDevice dev = handler.TVCards[card];
        //is this card recording
        if (dev.IsRecording)
        {
          //yes, is it recording the 'rec' ?
          if (dev.CurrentTVRecording.ID == _recordingToStop.ID)
          {
            stopped = true;
            //yep then cancel the recording
            if (_recordingToStop.RecType == TVRecording.RecordingType.Once)
            {
              Log.WriteFile(LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", dev.CommercialName,
                            dev.TVChannel);
              _recordingToStop.Canceled = Util.Utils.datetolong(DateTime.Now);
            }
            else
            {
              Log.WriteFile(LogType.Recorder, "Recorder: Stop serie of recording card:{0} channel:{1}",
                            dev.CommercialName, dev.TVChannel);
              long datetime = Util.Utils.datetolong(DateTime.Now);
              TVProgram prog = dev.CurrentProgramRecording;
              if (prog != null)
              {
                datetime = Util.Utils.datetolong(prog.StartTime);
              }
              _recordingToStop.CanceledSeries.Add(datetime);
              _recordingToStop.Canceled = 0;
            }
            //and tell the card to stop recording
            TVDatabase.UpdateRecording(_recordingToStop, TVDatabase.RecordingChange.Canceled);
            dev.StopRecording();

            //if we're not viewing this card
            if (handler.CurrentCardIndex != card)
            {
              //then stop card
              dev.Stop();
            }
          }
        }
      }
      if (stopped)
      {
        Succeeded = true;
      }
      else
      {
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(754); // "No tuner is recording the specified show";
      }
    }
  }
}