#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
  public class StopRecordingCommand : CardCommand
  {
    public override void Execute(CommandProcessor handler)
    {
      Log.WriteFile(LogType.Recorder, "Command:Stop recording");

      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753); //"No tuner cards installed";
        Succeeded = false;
        return;
      }
      //get the current selected card
      if (handler.CurrentCardIndex < 0 || handler.CurrentCardIndex >= handler.TVCards.Count)
      {
        return;
      }
      TVCaptureDevice dev = handler.TVCards[handler.CurrentCardIndex];

      //is it recording?
      if (dev.IsRecording == false)
      {
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(758); //"Tuner is not recording"
        return;
      }
      //yes. then cancel the recording
      Log.WriteFile(LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", dev.CommercialName, dev.TVChannel);
      int ID = dev.CurrentTVRecording.ID;

      if (dev.CurrentTVRecording.RecType == TVRecording.RecordingType.Once)
      {
        Log.WriteFile(LogType.Recorder, "Recorder: cancel recording");
        dev.CurrentTVRecording.Canceled = Util.Utils.datetolong(DateTime.Now);
      }
      else
      {
        long datetime = Util.Utils.datetolong(DateTime.Now);
        TVProgram prog = dev.CurrentProgramRecording;
        Log.WriteFile(LogType.Recorder, "Recorder: cancel {0}", prog);

        if (prog != null)
        {
          datetime = Util.Utils.datetolong(prog.StartTime);
          Log.WriteFile(LogType.Recorder, "Recorder: cancel serie {0} {1} {2}", prog.Title,
                        prog.StartTime.ToLongDateString(), prog.StartTime.ToLongTimeString());
        }
        else
        {
          Log.WriteFile(LogType.Recorder, "Recorder: cancel series");
        }
        dev.CurrentTVRecording.CanceledSeries.Add(datetime);
      }
      TVDatabase.UpdateRecording(dev.CurrentTVRecording, TVDatabase.RecordingChange.Canceled);

      //and tell the card to stop the recording
      dev.StopRecording();

      CheckRecordingsCommand cmd = new CheckRecordingsCommand();
      handler.AddCommand(cmd);
      Succeeded = true;
    }
  }
}