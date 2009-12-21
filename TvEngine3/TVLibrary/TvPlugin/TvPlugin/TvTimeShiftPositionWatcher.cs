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
  class TvTimeShiftPositionWatcher
  {
    private static Decimal idLastProgramEndsSoon = -1;
    private static Int64 programTimeShiftStart = -1;
    private static string programTimeShiftFile = "";
    private static decimal preRecordInterval = -1;
    private static int isEnabled = 0;

    private static bool IsEnabled()
    {
      if (isEnabled==0)
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
        if ((DateTime.Now.AddMinutes((double) preRecordInterval) >= TVHome.Navigator.Channel.CurrentProgram.EndTime &&
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
      catch(Exception ex)
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
          RemoteControl.Instance.CopyTimeShiftFile(programTimeShiftStart, programTimeShiftFile, currentPosition, currentFile, recordingFilename);
        }
      }
    }
  }
}
