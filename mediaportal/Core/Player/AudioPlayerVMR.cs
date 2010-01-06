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

using System;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Player
{
  public class AudioPlayerVMR7 : IPlayer
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }

    private string m_strCurrentFile = "";
    private PlayState m_state = PlayState.Init;
    private int m_iVolume = 100;
    private bool m_bNotifyPlaying = true;
    private IGraphBuilder graphBuilder;

    private DsROTEntry _rotEntry = null;

    /// <summary> control interface. </summary>
    private IMediaControl mediaCtrl;

    /// <summary> graph event interface. </summary>
    private IMediaEventEx mediaEvt;

    /// <summary> seek interface for positioning in stream. </summary>
    private IMediaSeeking mediaSeek;

    /// <summary> seek interface to set position in stream. </summary>
    private IMediaPosition mediaPos;

    /// <summary> video preview window interface. </summary>
    /// <summary> audio interface used to control volume. </summary>
    private IBasicAudio basicAudio;

    private const int WM_GRAPHNOTIFY = 0x00008001; // message from graph

    private const int WS_CHILD = 0x40000000; // attributes for video window
    private const int WS_CLIPCHILDREN = 0x02000000;
    private const int WS_CLIPSIBLINGS = 0x04000000;

    public AudioPlayerVMR7()
    {
    }

    public override bool Play(string strFile)
    {
      m_iVolume = 100;
      m_bNotifyPlaying = true;
      m_state = PlayState.Init;
      m_strCurrentFile = strFile;

      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      Log.Info("AudioPlayerVMR7.play {0}", strFile);
      lock (typeof (AudioPlayerVMR7))
      {
        CloseInterfaces();
        if (!GetInterfaces())
        {
          m_strCurrentFile = "";
          return false;
        }
        int hr = mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
        if (hr < 0)
        {
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }

        GetFrameStepInterface();

        _rotEntry = new DsROTEntry((IFilterGraph) graphBuilder);


        hr = mediaCtrl.Run();
        if (hr < 0)
        {
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }
        //        mediaPos.put_CurrentPosition(4*60);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
        msg.Label = strFile;
        GUIWindowManager.SendThreadMessage(msg);
      }
      m_state = PlayState.Playing;
      return true;
    }


    private void MovieEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      if (null != mediaCtrl)
      {
        Log.Info("AudioPlayerVMR7.ended {0}", m_strCurrentFile);
        m_strCurrentFile = "";
        if (!bManualStop)
        {
          m_state = PlayState.Ended;
        }
        else
        {
          m_state = PlayState.Init;
        }
      }
    }


    public override double Duration
    {
      get
      {
        if (m_state != PlayState.Init)
        {
          double m_dDuration;
          mediaPos.get_Duration(out m_dDuration);
          return m_dDuration;
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (m_state != PlayState.Init)
        {
          double dTime;
          mediaPos.get_CurrentPosition(out dTime);

          return dTime;
        }
        return 0.0d;
      }
    }

    public override void Pause()
    {
      if (m_state == PlayState.Paused)
      {
        mediaCtrl.Run();
        m_state = PlayState.Playing;
      }
      else if (m_state == PlayState.Playing)
      {
        m_state = PlayState.Paused;
        mediaCtrl.Pause();
      }
    }

    public override bool Paused
    {
      get { return (m_state == PlayState.Paused); }
    }

    public override bool Playing
    {
      get { return (m_state == PlayState.Playing || m_state == PlayState.Paused); }
    }

    public override bool Stopped
    {
      get { return (m_state == PlayState.Init); }
    }

    public override string CurrentFile
    {
      get { return m_strCurrentFile; }
    }

    public override void Stop()
    {
      if (m_state != PlayState.Init)
      {
        mediaCtrl.StopWhenReady();

        MovieEnded(true);
      }
    }

    public override int Speed
    {
      get
      {
        if (m_state == PlayState.Init)
        {
          return 1;
        }
        if (mediaCtrl == null)
        {
          return 1;
        }
        double rate = 0;
        mediaPos.get_Rate(out rate);
        return (int) rate;
      }
      set
      {
        if (mediaCtrl == null)
        {
          return;
        }
        if (m_state != PlayState.Init)
        {
          // For Rewind, we receive a negative value, which needs to be converted:
          // DX does not allow changing the rate to rewing, so we get the current position
          // and go back as many seconds as received.
          double position = 0.0;
          if (value < 0)
          {
            mediaPos.get_CurrentPosition(out position);
            position += value;
            mediaPos.put_CurrentPosition(position);
          }
          else
          {
            try
            {
              mediaPos.put_Rate((double) value);
            }
            catch (Exception)
            {
            }
          }
        }
      }
    }

    public override int Volume
    {
      get { return m_iVolume; }
      set
      {
        if (m_iVolume != value)
        {
          m_iVolume = value;
          if (m_state != PlayState.Init)
          {
            if (basicAudio != null)
            {
              // Divide by 100 to get equivalent decibel value. For example, –10,000 is –100 dB. 
              float fPercent = (float) m_iVolume/100.0f;
              int iVolume = (int) ((DirectShowVolume.VOLUME_MAX - DirectShowVolume.VOLUME_MIN)*fPercent);
              basicAudio.put_Volume((iVolume - DirectShowVolume.VOLUME_MIN));
            }
          }
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          double dCurTime;
          mediaPos.get_CurrentPosition(out dCurTime);

          dTime = dCurTime + dTime;
          if (dTime < 0.0d)
          {
            dTime = 0.0d;
          }
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          if (dTime < 0.0d)
          {
            dTime = 0.0d;
          }
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          double dCurrentPos;
          mediaPos.get_CurrentPosition(out dCurrentPos);
          double dDuration = Duration;

          double fCurPercent = (dCurrentPos/Duration)*100.0d;
          double fOnePercent = Duration/100.0d;
          fCurPercent = fCurPercent + (double) iPercentage;
          fCurPercent *= fOnePercent;
          if (fCurPercent < 0.0d)
          {
            fCurPercent = 0.0d;
          }
          if (fCurPercent < Duration)
          {
            mediaPos.put_CurrentPosition(fCurPercent);
          }
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          if (iPercentage < 0)
          {
            iPercentage = 0;
          }
          if (iPercentage >= 100)
          {
            iPercentage = 100;
          }
          double fPercent = Duration/100.0f;
          fPercent *= (double) iPercentage;
          mediaPos.put_CurrentPosition(fPercent);
        }
      }
    }


    public override bool HasVideo
    {
      get { return false; }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    private bool GetInterfaces()
    {
      int iStage = 1;
      string audioDevice;
      using (Settings xmlreader = new MPSettings())
      {
        audioDevice = xmlreader.GetValueAsString("audioplayer", "sounddevice", "Default DirectSound Device");
      }
      //Type comtype = null;
      //object comobj = null;
      try
      {
        graphBuilder = (IGraphBuilder) new FilterGraph();
        iStage = 5;
        DirectShowUtil.AddAudioRendererToGraph(graphBuilder, audioDevice, false);
        int hr = graphBuilder.RenderFile(m_strCurrentFile, null);
        if (hr != 0)
        {
          Error.SetError("Unable to play file", "Missing codecs to play this file");
          return false;
        }
        iStage = 6;
        mediaCtrl = (IMediaControl) graphBuilder;

        iStage = 7;
        mediaEvt = (IMediaEventEx) graphBuilder;
        iStage = 8;
        mediaSeek = (IMediaSeeking) graphBuilder;
        iStage = 9;
        mediaPos = (IMediaPosition) graphBuilder;
        iStage = 10;
        basicAudio = graphBuilder as IBasicAudio;
        iStage = 11;
        return true;
      }
      catch (Exception ex)
      {
        Log.Info("Can not start {0} stage:{1} err:{2} stack:{3}",
                 m_strCurrentFile, iStage,
                 ex.Message,
                 ex.StackTrace);
        return false;
      }
    }


    /// <summary> try to get the step interfaces. </summary>
    private bool GetFrameStepInterface()
    {
      return true;
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    private void CloseInterfaces()
    {
      int hr;
      try
      {

        if (mediaCtrl != null)
        {
          int counter = 0;
          FilterState state;
          hr = mediaCtrl.Stop();
          hr = mediaCtrl.GetState(10, out state);
          while (state != FilterState.Stopped || GUIGraphicsContext.InVmr9Render)
          {
            System.Threading.Thread.Sleep(100);
            hr = mediaCtrl.GetState(10, out state);
            counter++;
            if (counter >= 30)
            {
              if (state != FilterState.Stopped)
                Log.Debug("AudioPlayerVMR: graph still running");
              if (GUIGraphicsContext.InVmr9Render)
                Log.Debug("AudioPlayerVMR: in renderer");
              break;
            }
          }
          mediaCtrl = null;
        }

        m_state = PlayState.Init;

        if (mediaEvt != null)
        {
          hr = mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
          mediaEvt = null;
        }

        mediaSeek = null;
        mediaPos = null;
        basicAudio = null;

        if (graphBuilder != null)
        {
          if (_rotEntry != null)
          {
            _rotEntry.Dispose();
            _rotEntry = null;
          }
          DirectShowUtil.ReleaseComObject(graphBuilder);
          graphBuilder = null;
        }

        m_state = PlayState.Init;
      }
      catch (Exception)
      {
      }
    }

    public override void WndProc(ref Message m)
    {
      if (m.Msg == WM_GRAPHNOTIFY)
      {
        if (mediaEvt != null)
        {
          OnGraphNotify();
        }
        return;
      }
      base.WndProc(ref m);
    }

    public override bool Ended
    {
      get { return m_state == PlayState.Ended; }
    }

    private void OnGraphNotify()
    {
      int p1, p2, hr = 0;
      EventCode code;
      do
      {
        hr = mediaEvt.GetEvent(out code, out p1, out p2, 0);
        if (hr < 0)
        {
          break;
        }
        hr = mediaEvt.FreeEventParams(code, p1, p2);
        if (code == EventCode.Complete || code == EventCode.ErrorAbort)
        {
          MovieEnded(false);
        }
      } while (hr == 0);
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }
      if (CurrentPosition >= 10.0)
      {
        if (m_bNotifyPlaying)
        {
          m_bNotifyPlaying = false;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
          msg.Label = CurrentFile;
          GUIWindowManager.SendThreadMessage(msg);
        }
      }
    }

    #region IDisposable Members

    public override void Release()
    {
      CloseInterfaces();
    }

    #endregion
  }
}