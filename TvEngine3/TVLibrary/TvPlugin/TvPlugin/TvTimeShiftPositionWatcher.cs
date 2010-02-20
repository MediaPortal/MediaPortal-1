#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvDatabase;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace TvPlugin
{
  internal class TvTimeShiftPositionWatcher
  {
    private static Decimal idLastProgramEndsSoon = -1;
    private static Int64 programTimeShiftStart = -1;
    private static string programTimeShiftFile = "";
    private static decimal preRecordInterval = -1;
    private static int isEnabled = 0;

    private static bool IsEnabled()
    {
      if (isEnabled == 0)
      {
        if (DebugSettings.EnableRecordingFromTimeshift)
          isEnabled = 1;
        else
          isEnabled = -1;
      }
      return (isEnabled == 1);
    }

    public static void CheckOrUpdateTimeShiftPosition(bool invalidate)
    {
      if (!IsEnabled())
        return;
      if (!TVHome.Connected)
        return;
      try
      {
        if (preRecordInterval == -1)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          preRecordInterval = Decimal.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        }
        if (TVHome.Navigator.Channel == null)
          return;
        if (TVHome.Navigator.Channel.CurrentProgram == null)
          return;
        if ((DateTime.Now.AddMinutes((double)preRecordInterval) >= TVHome.Navigator.Channel.CurrentProgram.EndTime &&
             idLastProgramEndsSoon != TVHome.Navigator.Channel.CurrentProgram.IdProgram) || invalidate)
        {
          User u = TVHome.Card.User;
          if (u == null)
            return;
          long bufferId = 0;
          if (RemoteControl.Instance.TimeShiftGetCurrentFilePosition(ref u, ref programTimeShiftStart, ref bufferId))
          {
            if (u == null || programTimeShiftStart == null || bufferId == null)
              return;
            programTimeShiftFile = RemoteControl.Instance.TimeShiftFileName(ref u) + bufferId.ToString() + ".ts";
            Log.Info("**");
            Log.Info("**");
            Log.Info("**");
            Log.Info("TimeshiftPositionWatcher: Found new Program that ends soon {0} ts pos: {1} , ts filename: {2}",
                     TVHome.Navigator.Channel.CurrentProgram.Title, programTimeShiftStart, programTimeShiftFile);
            Log.Info("**");
            Log.Info("**");
            Log.Info("**");

            idLastProgramEndsSoon = TVHome.Navigator.Channel.CurrentProgram.IdProgram;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeShiftPositionWatcher.CheckOrUpdateTimeShiftPosition exception : {0}", ex);
      }
    }

    public static void InitiateBufferFilesCopyProcess(string recordingFilename)
    {
      if (!IsEnabled())
        return;
      if (programTimeShiftStart != -1)
      {
        User u = TVHome.Card.User;
        long bufferId = 0;
        Int64 currentPosition = -1;
        if (RemoteControl.Instance.TimeShiftGetCurrentFilePosition(ref u, ref currentPosition, ref bufferId))
        {
          string currentFile = RemoteControl.Instance.TimeShiftFileName(ref u) + bufferId.ToString() + ".ts";
          Log.Info("**");
          Log.Info("**");
          Log.Info("**");
          Log.Info("TimeshiftPositionWatcher: Starting to copy buffer files for recording {0}", recordingFilename);
          Log.Info("**");
          Log.Info("**");
          Log.Info("**");
          RemoteControl.Instance.CopyTimeShiftFile(programTimeShiftStart, programTimeShiftFile, currentPosition,
                                                   currentFile, recordingFilename);
        }
      }
    }
  }
}