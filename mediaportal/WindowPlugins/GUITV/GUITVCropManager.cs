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
using MediaPortal.Player;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

#endregion

namespace MediaPortal.GUI.TV
{
  public class GUITVCropManager
  {
    #region Ctor/Dtor

    public GUITVCropManager()
    {
      Recorder.OnTvViewingStarted += new Recorder.OnTvViewHandler(Recorder_OnTvViewingStarted);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      Log.Info("GUITVCropManager: Started");
    }

    ~GUITVCropManager()
    {
      Recorder.OnTvViewingStarted -= new Recorder.OnTvViewHandler(Recorder_OnTvViewingStarted);
      g_Player.PlayBackStarted -= new g_Player.StartedHandler(g_Player_PlayBackStarted);
      Log.Info("GUITVCropManager: Stopped");
    }

    #endregion

    /// <summary>
    /// Gets called by the Recorder when viewing of a channel started.
    /// This handles cropping TV with timeshifting off
    /// </summary>
    /// <param name="card">Card number</param>
    /// <param name="device">TvCaptureDevice used for viewing</param>
    private void Recorder_OnTvViewingStarted(int card, TVCaptureDevice device)
    {
      if (Recorder.IsViewing() && !Recorder.IsTimeShifting())
      {
        Log.Debug("GUITVCropManager.Recorder_OnTvViewingStarted: not timeshifting");
        SendCropMessage(device);
      }
    }

    /// <summary>
    /// Gets called by g_Player when playback of media has started.
    /// This handles cropping timeshifted TV and recordings.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filename"></param>
    private void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (Recorder.Running && Recorder.CommandProcessor != null)
      {
        Log.Debug("GUITVCropManager.g_Player_PlackBackStarted: media: {0} tv:{1} ts:{2}", type, g_Player.IsTV,
                  g_Player.IsTimeShifting);
        if (type == g_Player.MediaType.TV && !g_Player.IsTVRecording)
        {
          // This is timeshifted TV
          if (Recorder.CommandProcessor.CurrentCardIndex >= 0 &&
              Recorder.CommandProcessor.CurrentCardIndex < Recorder.Count)
          {
            SendCropMessage(Recorder.CommandProcessor.TVCards[Recorder.CommandProcessor.CurrentCardIndex]);
          }
        }
        else if (g_Player.IsTVRecording)
        {
          // This is a TV recording
          // Find out if any of the cards is still recording this file
          if (Recorder.IsAnyCardRecording())
          {
            for (int i = 0; i < Recorder.CommandProcessor.TVCards.Count; i++)
            {
              TVCaptureDevice dev = Recorder.CommandProcessor.TVCards[i];
              if (dev.IsRecording && dev.RecordingFileName.Equals(filename))
              {
                Log.Debug("GUITVCropManager.g_Player_PlackBackStarted: cropping in-progress recording:{0} card:{1}",
                          filename, dev.CommercialName);
                SendCropMessage(dev);
                return;
              }
            }
          }
          // Recording must be finished already - fetch cardID from database
          TVRecorded recording = new TVRecorded();
          if (TVDatabase.GetRecordedTVByFilename(filename, ref recording))
          {
            if ((recording.RecordedCardIndex > 0) &&
                (recording.RecordedCardIndex - 1 < Recorder.CommandProcessor.TVCards.Count))
            {
              try
              {
                TVCaptureDevice dev = Recorder.CommandProcessor.TVCards[recording.RecordedCardIndex - 1];
                Log.Debug("GUITVCropManager.g_Player_PlackBackStarted: cropping recorded tv:{0} card:{1}", filename,
                          dev.CommercialName);
                SendCropMessage(dev);
              }
              catch (ArgumentOutOfRangeException)
              {
                Log.Warn(
                  "GUITVCropManager.g_Player_PlackBackStarted: unable to find tvcard for recorded file:{0} with index:{1}",
                  filename, recording.RecordedCardIndex);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Handles the actual call to the TvCaptureDevice SendCropMessage()
    /// </summary>
    /// <param name="card">current card index</param>
    /// <param name="device">current TvCaptureDevice</param>
    private void SendCropMessage(TVCaptureDevice device)
    {
      Log.Debug("GUITVCropManager.SendCropMessage: send message for card:{0}", device.CommercialName);
      device.SendCropMessage();
    }
  }
}