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
using System.Threading;
using DirectShowLib;
using DirectShowLib.SBE;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Player
{
  public class StreamBufferPlayer9 : BaseStreamBufferPlayer
  {
    private VMR9Util _vmr9 = null;
    private IPin _pinVmr9ConnectedTo = null;

    public StreamBufferPlayer9()
    {
    }

    protected override void OnInitialized()
    {
      if (_vmr9 != null)
      {
        _vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullscreen)
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }

      if (!_updateNeeded)
      {
        return;
      }

      _updateNeeded = false;
      _isStarted = true;
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      Speed = 1;
      Log.Info("StreamBufferPlayer9: GetInterfaces()");

      //switch back to directx fullscreen mode

      //		Log.Info("StreamBufferPlayer9: switch to fullscreen mode");
      Log.Info("StreamBufferPlayer9: Enabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
      GUIWindowManager.SendMessage(msg);

      //Log.Info("StreamBufferPlayer9: build graph");

      try
      {
        _graphBuilder = (IGraphBuilder) new FilterGraph();
        //Log.Info("StreamBufferPlayer9: add _vmr9");

        _vmr9 = new VMR9Util();
        _vmr9.AddVMR9(_graphBuilder);
        _vmr9.Enable(false);


        int hr;
        m_StreamBufferConfig = new StreamBufferConfig();
        streamConfig2 = m_StreamBufferConfig as IStreamBufferConfigure2;
        if (streamConfig2 != null)
        {
          // setting the StreamBufferEngine registry key
          IntPtr HKEY = (IntPtr) unchecked((int) 0x80000002L);
          IStreamBufferInitialize pTemp = (IStreamBufferInitialize) streamConfig2;
          IntPtr subKey = IntPtr.Zero;

          RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
          hr = pTemp.SetHKEY(subKey);
          hr = streamConfig2.SetFFTransitionRates(8, 32);
          //Log.Info("set FFTransitionRates:{0:X}",hr);

          int max, maxnon;
          hr = streamConfig2.GetFFTransitionRates(out max, out maxnon);

          streamConfig2.GetBackingFileCount(out _minBackingFiles, out _maxBackingFiles);
          streamConfig2.GetBackingFileDuration(out _backingFileDuration);
        }
        //Log.Info("StreamBufferPlayer9: add sbe");

        // create SBE source
        _bufferSource = (IStreamBufferSource) new StreamBufferSource();
        if (_bufferSource == null)
        {
          Log.Error("StreamBufferPlayer9:Failed to create instance of SBE (do you have WinXp SP1?)");
          return false;
        }


        IBaseFilter filter = (IBaseFilter) _bufferSource;
        hr = _graphBuilder.AddFilter(filter, "SBE SOURCE");
        if (hr != 0)
        {
          Log.Error("StreamBufferPlayer9:Failed to add SBE to graph");
          return false;
        }

        IFileSourceFilter fileSource = (IFileSourceFilter) _bufferSource;
        if (fileSource == null)
        {
          Log.Error("StreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }


        //Log.Info("StreamBufferPlayer9: open file:{0}",filename);
        hr = fileSource.Load(filename, null);
        if (hr != 0)
        {
          Log.Error("StreamBufferPlayer9:Failed to open file:{0} :0x{1:x}", filename, hr);
          return false;
        }


        //Log.Info("StreamBufferPlayer9: add codecs");
        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAudioRenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters
        using (Settings xmlreader = new MPSettings())
        {
          // FlipGer: load infos for custom filters
          int intCount = 0;
          while (xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") != "undefined")
          {
            if (xmlreader.GetValueAsBool("mytv", "usefilter" + intCount.ToString(), false))
            {
              strFilters += xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") + ";";
              intFilters++;
            }
            intCount++;
          }
          strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
          string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
          GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(strValue);
        }
        if (strVideoCodec.Length > 0)
        {
          _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
        }
        if (strAudioCodec.Length > 0)
        {
          _audioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
        }
        if (strAudioRenderer.Length > 0)
        {
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, true);
        }
        // FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          customFilters[i] = DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
        }

        // render output pins of SBE
        DirectShowUtil.RenderOutputPins(_graphBuilder, (IBaseFilter) fileSource);

        _mediaCtrl = (IMediaControl) _graphBuilder;
        _mediaEvt = (IMediaEventEx) _graphBuilder;
        _mediaSeeking = _bufferSource as IStreamBufferMediaSeeking;
        _mediaSeeking2 = _bufferSource as IStreamBufferMediaSeeking2;
        if (_mediaSeeking == null)
        {
          Log.Error("Unable to get IMediaSeeking interface#1");
        }
        if (_mediaSeeking2 == null)
        {
          Log.Error("Unable to get IMediaSeeking interface#2");
        }
        if (_audioRendererFilter != null)
        {
          IMediaFilter mp = _graphBuilder as IMediaFilter;
          IReferenceClock clock = _audioRendererFilter as IReferenceClock;
          hr = mp.SetSyncSource(clock);
        }

        // Set the IBasicAudioInterface

        _basicAudio = (IBasicAudio) _graphBuilder;

        //        Log.Info("StreamBufferPlayer9:SetARMode");
        //        DirectShowUtil.SetARMode(_graphBuilder,AspectRatioMode.Stretched);

        //Log.Info("StreamBufferPlayer9: set Deinterlace");

        if (!_vmr9.IsVMR9Connected)
        {
          //_vmr9 is not supported, switch to overlay
          Log.Info("StreamBufferPlayer9: switch to overlay");
          _mediaCtrl = null;
          Cleanup();
          return base.GetInterfaces(filename);
        }
        _pinVmr9ConnectedTo = _vmr9.PinConnectedTo;
        _vmr9.SetDeinterlaceMode();
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("StreamBufferPlayer9:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }


    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      Cleanup();
    }

    private void Cleanup()
    {
      if (_graphBuilder == null)
      {
        Log.Info("StreamBufferPlayer9:grapbuilder=null");
        return;
      }

      int hr;
      Log.Info("StreamBufferPlayer9:cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
      try
      {
        if (_vmr9 != null)
        {
          Log.Info("StreamBufferPlayer9: vmr9 disable");
          _vmr9.Enable(false);
        }
        int counter = 0;
        while (GUIGraphicsContext.InVmr9Render)
        {
          counter++;
          Thread.Sleep(100);
          if (counter > 100)
          {
            break;
          }
        }

        if (_mediaCtrl != null)
        {
          hr = _mediaCtrl.Stop();
        }
        _mediaCtrl = null;
        _mediaEvt = null;
        _mediaSeeking = null;
        _mediaSeeking2 = null;
        _videoWin = null;
        _basicAudio = null;
        _basicVideo = null;
        _bufferSource = null;
        _pinVmr9ConnectedTo = null;

        if (_pinVmr9ConnectedTo != null)
        {
          DirectShowUtil.ReleaseComObject(_pinVmr9ConnectedTo);
          _pinVmr9ConnectedTo = null;
        }

        if (streamConfig2 != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(streamConfig2)) > 0)
          {
            ;
          }
          streamConfig2 = null;
        }

        m_StreamBufferConfig = null;

        if (_vmr9 != null)
        {
          Log.Info("StreamBufferPlayer9: vmr9 dispose");
          _vmr9.Dispose();
          _vmr9 = null;
        }
        if (_videoCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_videoCodecFilter)) > 0)
          {
            ;
          }
          _videoCodecFilter = null;
        }
        if (_audioCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_audioCodecFilter)) > 0)
          {
            ;
          }
          _audioCodecFilter = null;
        }

        if (_audioRendererFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_audioRendererFilter)) > 0)
          {
            ;
          }
          _audioRendererFilter = null;
        }

        // FlipGer: release custom filters
        for (int i = 0; i < customFilters.Length; i++)
        {
          if (customFilters[i] != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(customFilters[i])) > 0)
            {
              ;
            }
          }
          customFilters[i] = null;
        }
        DirectShowUtil.RemoveFilters(_graphBuilder);

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if (_graphBuilder != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0)
          {
            ;
          }
          _graphBuilder = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        _state = PlayState.Init;

        GC.Collect();
        //GC.Collect();
        //GC.Collect();
      }
      catch (Exception ex)
      {
        Log.Error("StreamBufferPlayer9: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }

      //switch back to directx windowed mode
      Log.Info("StreamBufferPlayer9: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);

      Log.Info("StreamBufferPlayer9: Cleanup done");
    }

    protected override void OnProcess()
    {
      if (_vmr9 != null)
      {
        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;
      }
    }


    public override void SeekAbsolute(double dTimeInSecs)
    {
      if (IsTimeShifting && IsTV && dTimeInSecs == 0)
      {
        if (Duration < 5)
        {
          if (_vmr9 != null)
          {
            _vmr9.Enable(false);
          }
          _seekToBegin = true;
          return;
        }
      }
      _seekToBegin = false;

      if (_vmr9 != null)
      {
        _vmr9.Enable(true);
      }
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          if (dTimeInSecs < 0.0d)
          {
            dTimeInSecs = 0.0d;
          }
          if (dTimeInSecs > Duration)
          {
            dTimeInSecs = Duration;
          }
          dTimeInSecs = Math.Floor(dTimeInSecs);
          //Log.Info("StreamBufferPlayer: seekabs: {0} duration:{1} current pos:{2}", dTimeInSecs,Duration, CurrentPosition);
          dTimeInSecs *= 10000000d;
          long pStop = 0;
          long lContentStart, lContentEnd;
          double fContentStart, fContentEnd;
          _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
          fContentStart = lContentStart;
          fContentEnd = lContentEnd;

          dTimeInSecs += fContentStart;
          long lTime = (long) dTimeInSecs;
          int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning,
                                              new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          if (hr != 0)
          {
            Log.Error("seek failed->seek to 0 0x:{0:X}", hr);
          }
        }
        UpdateCurrentPosition();
        //Log.Info("StreamBufferPlayer: current pos:{0}", CurrentPosition);
      }
    }

    protected override void ReInit()
    {
      //if (_vmr9 != null)
      //{
      //  int xx = 2;
      //}
      _vmr9 = new VMR9Util();
      _vmr9.AddVMR9(_graphBuilder);
      _vmr9.Enable(false);
      _graphBuilder.Render(_pinVmr9ConnectedTo);
      //if (!_vmr9.IsVMR9Connected)
      //{
      //  int x = 1;
      //}
    }

    public override void Stop()
    {
      if (SupportsReplay)
      {
        Log.Info("StreamBufferPlayer:stop");
        if (_mediaCtrl == null)
        {
          return;
        }

        if (_vmr9 != null)
        {
          Log.Info("StreamBufferPlayer9: vmr9 disable");
          _vmr9.Enable(false);
        }
        int counter = 0;
        while (GUIGraphicsContext.InVmr9Render)
        {
          counter++;
          Thread.Sleep(100);
          if (counter > 100)
          {
            break;
          }
        }

        _mediaCtrl.Stop();

        if (_vmr9 != null)
        {
          Log.Info("StreamBufferPlayer9: vmr9 dispose");
          _vmr9.Dispose();
          _vmr9 = null;
        }
      }
      else
      {
        CloseInterfaces();
      }
      //CloseInterfaces();
    }
  }
}