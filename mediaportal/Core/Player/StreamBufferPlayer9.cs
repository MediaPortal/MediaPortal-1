#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DirectShowLib.SBE;
using DShowNET.Helper;

namespace MediaPortal.Player
{
  public class StreamBufferPlayer9 : BaseStreamBufferPlayer
  {

    VMR9Util _vmr9 = null;
    IPin _pinVmr9ConnectedTo = null;

    public StreamBufferPlayer9()
    {
    }
    protected override void OnInitialized()
    {
      if ( _vmr9 != null )
      {
        _vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }
    public override void SetVideoWindow()
    {
      if ( GUIGraphicsContext.IsFullScreenVideo != _isFullscreen )
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }

      if ( !_updateNeeded )
        return;

      _updateNeeded = false;
      _isStarted = true;

    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces( string filename )
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
        _graphBuilder = (IGraphBuilder)new FilterGraph();
        //Log.Info("StreamBufferPlayer9: add _vmr9");

        _vmr9 = new VMR9Util();
        _vmr9.AddVMR9(_graphBuilder);
        _vmr9.Enable(false);


        int hr;
        m_StreamBufferConfig = new StreamBufferConfig();
        streamConfig2 = m_StreamBufferConfig as IStreamBufferConfigure2;
        if ( streamConfig2 != null )
        {
          // setting the StreamBufferEngine registry key
          IntPtr HKEY = (IntPtr)unchecked((int)0x80000002L);
          IStreamBufferInitialize pTemp = (IStreamBufferInitialize)streamConfig2;
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
        _bufferSource = (IStreamBufferSource)new StreamBufferSource();
        if ( _bufferSource == null )
        {
          Log.Error("StreamBufferPlayer9:Failed to create instance of SBE (do you have WinXp SP1?)");
          return false;
        }


        IBaseFilter filter = (IBaseFilter)_bufferSource;
        hr = _graphBuilder.AddFilter(filter, "SBE SOURCE");
        if ( hr != 0 )
        {
          Log.Error("StreamBufferPlayer9:Failed to add SBE to graph");
          return false;
        }

        IFileSourceFilter fileSource = (IFileSourceFilter)_bufferSource;
        if ( fileSource == null )
        {
          Log.Error("StreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }


        //Log.Info("StreamBufferPlayer9: open file:{0}",filename);
        hr = fileSource.Load(filename, null);
        if ( hr != 0 )
        {
          Log.Error("StreamBufferPlayer9:Failed to open file:{0} :0x{1:x}", filename, hr);
          return false;
        }


        //Log.Info("StreamBufferPlayer9: add codecs");
        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAudioRenderer = "";
        bool bAddFFDshow = false;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
        {
          bAddFFDshow = xmlreader.GetValueAsBool("mytv", "ffdshow", false);
          strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "");
          string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
          if ( strValue.Equals("zoom") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
          if ( strValue.Equals("stretch") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
          if ( strValue.Equals("normal") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
          if ( strValue.Equals("original") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
          if ( strValue.Equals("letterbox") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
          if ( strValue.Equals("panscan") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
          if ( strValue.Equals("zoom149") )
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom14to9;

        }
        if ( strVideoCodec.Length > 0 )
          _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
        if ( strAudioCodec.Length > 0 )
          _audioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
        if ( strAudioRenderer.Length > 0 )
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, false);
        if ( bAddFFDshow )
          _ffdShowFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "ffdshow raw video filter");

        // render output pins of SBE
        DirectShowUtil.RenderOutputPins(_graphBuilder, (IBaseFilter)fileSource);

        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _mediaSeeking = _bufferSource as IStreamBufferMediaSeeking;
        _mediaSeeking2 = _bufferSource as IStreamBufferMediaSeeking2;
        if ( _mediaSeeking == null )
        {
          Log.Error("Unable to get IMediaSeeking interface#1");
        }
        if ( _mediaSeeking2 == null )
        {
          Log.Error("Unable to get IMediaSeeking interface#2");
        }
        if ( _audioRendererFilter != null )
        {
          IMediaFilter mp = _graphBuilder as IMediaFilter;
          IReferenceClock clock = _audioRendererFilter as IReferenceClock;
          hr = mp.SetSyncSource(clock);
        }


        //        Log.Info("StreamBufferPlayer9:SetARMode");
        //        DirectShowUtil.SetARMode(_graphBuilder,AspectRatioMode.Stretched);

        //Log.Info("StreamBufferPlayer9: set Deinterlace");

        if ( !_vmr9.IsVMR9Connected )
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
      catch ( Exception ex )
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

    void Cleanup()
    {
      if ( _graphBuilder == null )
      {
        Log.Info("StreamBufferPlayer9:grapbuilder=null");
        return;
      }

      int hr;
      Log.Info("StreamBufferPlayer9:cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
      try
      {
        if ( _vmr9 != null )
        {
          Log.Info("StreamBufferPlayer9: vmr9 disable");
          _vmr9.Enable(false);
        }
        int counter = 0;
        while ( GUIGraphicsContext.InVmr9Render )
        {
          counter++;
          System.Threading.Thread.Sleep(1);
          if ( counter > 200 )
            break;
        }

        if ( _mediaCtrl != null )
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

        if ( _pinVmr9ConnectedTo != null )
        {
          Marshal.ReleaseComObject(_pinVmr9ConnectedTo);
          _pinVmr9ConnectedTo = null;
        }

        if ( streamConfig2 != null )
        {
          while ( ( hr = Marshal.ReleaseComObject(streamConfig2) ) > 0 )
            ;
          streamConfig2 = null;
        }

        m_StreamBufferConfig = null;

        if ( _vmr9 != null )
        {
          Log.Info("StreamBufferPlayer9: vmr9 dispose");
          _vmr9.Dispose();
          _vmr9 = null;
        }
        if ( _videoCodecFilter != null )
        {
          while ( ( hr = Marshal.ReleaseComObject(_videoCodecFilter) ) > 0 )
            ;
          _videoCodecFilter = null;
        }
        if ( _audioCodecFilter != null )
        {
          while ( ( hr = Marshal.ReleaseComObject(_audioCodecFilter) ) > 0 )
            ;
          _audioCodecFilter = null;
        }

        if ( _audioRendererFilter != null )
        {
          while ( ( hr = Marshal.ReleaseComObject(_audioRendererFilter) ) > 0 )
            ;
          _audioRendererFilter = null;
        }

        if ( _ffdShowFilter != null )
        {
          while ( ( hr = Marshal.ReleaseComObject(_ffdShowFilter) ) > 0 )
            ;
          _ffdShowFilter = null;
        }

        DirectShowUtil.RemoveFilters(_graphBuilder);

        if ( _rotEntry != null )
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if ( _graphBuilder != null )
        {
          while ( ( hr = Marshal.ReleaseComObject(_graphBuilder) ) > 0 )
            ;
          _graphBuilder = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        _state = PlayState.Init;
        GC.Collect();
        GC.Collect();
        GC.Collect();
      }
      catch ( Exception ex )
      {
        Log.Error("StreamBufferPlayer9: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }

      //switch back to directx windowed mode
      if ( !GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow) )
      {
        Log.Info("StreamBufferPlayer9: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }

      Log.Info("StreamBufferPlayer9: Cleanup done");
    }

    protected override void OnProcess()
    {
      if ( _vmr9 != null )
      {
        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;
      }
    }


    public override void SeekAbsolute( double dTimeInSecs )
    {
      if ( IsTimeShifting && IsTV && dTimeInSecs == 0 )
      {
        if ( Duration < 5 )
        {
          if ( _vmr9 != null )
          {
            _vmr9.Enable(false);
          }
          _seekToBegin = true;
          return;
        }
      }
      _seekToBegin = false;

      if ( _vmr9 != null )
      {
        _vmr9.Enable(true);
      }
      if ( _state != PlayState.Init )
      {
        if ( _mediaCtrl != null && _mediaSeeking != null )
        {
          if ( dTimeInSecs < 0.0d )
            dTimeInSecs = 0.0d;
          if ( dTimeInSecs > Duration )
            dTimeInSecs = Duration;
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
          long lTime = (long)dTimeInSecs;
          int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          if ( hr != 0 )
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
      if ( SupportsReplay )
      {
        Log.Info("StreamBufferPlayer:stop");
        if ( _mediaCtrl == null )
          return;

        if ( _vmr9 != null )
        {
          Log.Info("StreamBufferPlayer9: vmr9 disable");
          _vmr9.Enable(false);
        }
        int counter = 0;
        while ( GUIGraphicsContext.InVmr9Render )
        {
          counter++;
          System.Threading.Thread.Sleep(1);
          if ( counter > 200 )
            break;
        }

        _mediaCtrl.Stop();

        if ( _vmr9 != null )
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
