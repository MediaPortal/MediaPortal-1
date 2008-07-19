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
using MediaPortal.Configuration;
#endregion

namespace MediaPortal.TV.Recording
{
  public class TvCardCollection
  {
    public delegate void OnTvChannelChangeHandler(string tvChannelName);
    public delegate void OnTvRecordingChangedHandler();
    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);

    //event which happens when the state of a recording changes (like started or stopped)
    public event OnTvRecordingChangedHandler OnTvRecordingChanged = null;

    //event which happens when a recording about to be recorded
    public event OnTvRecordingHandler OnTvRecordingStarted = null;

    //event which happens when a recording has ended
    public event OnTvRecordingHandler OnTvRecordingEnded = null;

    // list of all tv cards installed
    protected List<TVCaptureDevice> _tvcards = new List<TVCaptureDevice>();

    public TvCardCollection()
    {
      if (System.IO.File.Exists(Config.GetFile(Config.Dir.Plugins, "windows\\tvplugin.dll")))
      {
        Log.Info("TvCardCollection: TVPlugin detected -> no TVE2 card setup");
        _tvcards = new List<TVCaptureDevice>();
        return;
      }
    
      if (System.IO.File.Exists(Config.GetFile(Config.Dir.Config, "capturecards.xml")))
      {
        using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            SoapFormatter c = new SoapFormatter();
            ArrayList cards = (ArrayList)c.Deserialize(fileStream);
            foreach (TVCaptureDevice dev in cards)
            {
              _tvcards.Add(dev);
            }
          }
          catch (Exception)
          {
            Log.WriteFile(LogType.Recorder, true, "Recorder: invalid capturecards.xml found! please delete it");
          }
          finally
          {
            fileStream.Close();
          }
        }
      }
      else
      {
        _tvcards = new List<TVCaptureDevice>();
      }

      //subscribe to the recording events of each card
      for (int i = 0; i < _tvcards.Count; i++)
      {
        TVCaptureDevice card = _tvcards[i];
        card.ID = (i + 1);
        card.OnTvRecordingEnded += new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingEnded);
        card.OnTvRecordingStarted += new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingStarted);
        Log.WriteFile(LogType.Recorder, "Recorder:    card:{0} video device:{1} TV:{2}  record:{3} priority:{4}",
                              card.ID, card.VideoDevice, card.UseForTV, card.UseForRecording, card.Priority);
      }
      //clean up any old leftover timeshifting files from the last time
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        try
        {
          TVCaptureDevice dev = _tvcards[i];
          string dir = String.Format(@"{0}\card{1}", dev.RecordingPath, i + 1);
          System.IO.Directory.CreateDirectory(dir);
          MediaPortal.Util.Utils.DeleteOldTimeShiftFiles(dir);
        }
        catch (Exception) { }
      }

    }
    public TVCaptureDevice AddDummyCard(string name)
    {
      TVCaptureDevice dev = new TVCaptureDevice();
      dev.CommercialName = name;
      dev.FriendlyName = name;
      dev.CardType = TVCapture.CardTypes.Dummy;
      dev.ID = _tvcards.Count + 1;
      dev.RecordingPath = @"C:";
      dev.UseForRecording = true;
      dev.SupportsTV = true;
      dev.SupportsRadio = true;
      _tvcards.Add(dev);
      return dev;
    }

    public int Count
    {
      get { return _tvcards.Count; }
    }

    public TVCaptureDevice this[int index]
    {
      get { return _tvcards[index]; }
    }

    private void card_OnTvRecordingEnded(string recordingFileName, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(LogType.Recorder, "Recorder: recording ended '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6}", recording.Title, recording.Channel, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString(), recording.ID, recording.Priority, recording.Quality.ToString());
      if (OnTvRecordingEnded != null)
        OnTvRecordingEnded(recordingFileName, recording, program);
      if (OnTvRecordingChanged != null)
        OnTvRecordingChanged();
    }
    private void card_OnTvRecordingStarted(string recordingFileName, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(LogType.Recorder, "Recorder: recording started '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6}", recording.Title, recording.Channel, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString(), recording.ID, recording.Priority, recording.Quality.ToString());
      if (OnTvRecordingStarted != null)
        OnTvRecordingStarted(recordingFileName, recording, program);
      if (OnTvRecordingChanged != null)
        OnTvRecordingChanged();

    }
  }
}
