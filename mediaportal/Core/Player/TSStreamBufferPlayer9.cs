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
using Microsoft.Win32;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;

namespace MediaPortal.Player
{
  public class TStreamBufferPlayer9 : BaseTStreamBufferPlayer
  {
    #region structs

    static byte[] Mpeg2ProgramVideo = 
    {
      0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
      0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
      0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
      0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
      0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
      0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
      0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
      0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
      0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
      0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

      //0x051736=333667-> 10000000/333667 = 29.97fps
      //0x061A80=400000-> 10000000/400000 = 25fps
      0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
      0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
      0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
      0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
      0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
      0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
      0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
      0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
      0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
      0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
      0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
      0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
      0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
      0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
      0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
      0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
      0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
      0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
      0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
      //0x56, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
      0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
      0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
      0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
      0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
      /*
       * //  .dwSequenceHeader [1]
      0x00, 0x00, 0x01, 0xB3, 0x2D, 0x01, 0xE0, 0x24,
      0x09, 0xC4, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
      0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
      0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
      0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
      0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
      0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
      0x1A, 0x1A, 0x1A, 0x1A, 0x19, 0x1B, 0x1B, 0x1B, 
      0x1B, 0x1B, 0x1C, 0x1C, 0x1C, 0x1C, 0x1E, 0x1E, 
      0x1E, 0x1F, 0x1F, 0x21, 0x00, 0x00, 0x01, 0xB5, 
      0x14, 0x82, 0x00, 0x01, 0x00, 0x00*/
      0x00, 0x00, 0x00, 0x00
    };
    static byte[] MPEG1AudioFormat = 
    {
      0x50, 0x00,             // format type      = 0x0050=WAVE_FORMAT_MPEG
      0x02, 0x00,             // channels
      0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
      0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
      0x00, 0x03,             // nBlockAlign      = 0x0300 = 768
      0x10, 0x00,             // wBitsPerSample   = 16
      0x16, 0x00,             // extra size       = 0x0016 = 22 bytes
      0x02, 0x00,             // fwHeadLayer
      0x00, 0x70,0x17, 0x00,  // dwHeadBitrate
      0x01, 0x00,             // fwHeadMode
      0x01, 0x00,             // fwHeadModeExt
      0x01, 0x00,             // wHeadEmphasis
      0x1C, 0x00,             // fwHeadFlags
      0x00, 0x00, 0x00, 0x00, // dwPTSLow
      0x00, 0x00, 0x00, 0x00  // dwPTSHigh
    };
    #endregion
    VMR9Util _vmr9 = null;
    public TStreamBufferPlayer9()
    {
    }

    protected override void OnInitialized()
    {
      _log.Info("tsplayer9:OnInitialized");
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

      if (!_updateNeeded) return;

      _updateNeeded = false;
      _isStarted = true;

    }


    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      Speed = 1;
      _log.Info("TSStreamBufferPlayer9: GetInterfaces()");

      // switch back to directx fullscreen mode
      _log.Info("TSStreamBufferPlayer9: Enabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
      GUIWindowManager.SendMessage(msg);

      //_log.Info("TSStreamBufferPlayer9: build graph");

      try
      {
        _graphBuilder = (IGraphBuilder)new FilterGraph();
        _log.Info("TSStreamBufferPlayer9: add _vmr9");

        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);
        #region vmr9
        _vmr9 = new VMR9Util();
        _vmr9.AddVMR9(_graphBuilder);
        _vmr9.Enable(false);
        #endregion

        #region codecs

        _log.Info("TSStreamBufferPlayer9: add codecs");

        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAudioRenderer = "";
        bool bAddFFDshow = false;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          bAddFFDshow = xmlreader.GetValueAsBool("mytv", "ffdshow", false);
          strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "");
          string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
          if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
          if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
          if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
          if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
          if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
          if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;

        }
        if (strVideoCodec.Length > 0) _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
        if (strAudioCodec.Length > 0) _audioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
        if (strAudioRenderer.Length > 0) _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, false);
        if (bAddFFDshow) _ffdShowFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "ffdshow raw video filter");

        // render output pins of SBE
        //DirectShowUtil.RenderOutputPins(_graphBuilder, (IBaseFilter)_fileSource);
        #endregion

        #region tsfilesource settings
        _log.Info("TSStreamBufferPlayer9:initialize tsfilesource");
        try
        {
          using (RegistryKey hklm = Registry.LocalMachine)
          {
            using (RegistryKey settings = hklm.OpenSubKey(@"SOFTWARE\TSFileSource\settings\default", true))
            {
              byte[] value4Zeros = new byte[4];
              byte[] valueZero = new byte[1];
              byte[] valueOne = new byte[1];
              value4Zeros[0] = 1;
              value4Zeros[1] = value4Zeros[2] = value4Zeros[3] = 0;
              valueZero[0] = 0;
              valueOne[0] = 1;
              settings.SetValue("clockType", value4Zeros, RegistryValueKind.Binary);
              settings.SetValue("enableAC3", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableAudio2", valueZero, RegistryValueKind.Binary);
              if (false == true && _isLive)
              {
                settings.SetValue("enableAuto", valueZero, RegistryValueKind.Binary);
              }
              else
              {
                settings.SetValue("enableAuto", valueOne, RegistryValueKind.Binary);
              }
              settings.SetValue("enableDelay", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableFixedAR", valueOne, RegistryValueKind.Binary);
              settings.SetValue("enableMP2", valueOne, RegistryValueKind.Binary);
              settings.SetValue("enableNPControl", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableNPSlave", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableRateControl", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableROT", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableTSPin", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableTxtPin", valueZero, RegistryValueKind.Binary);
              settings.SetValue("ProgramSID", value4Zeros, RegistryValueKind.Binary);
            }
          }
        }
        catch (Exception ex)
        {
          _log.Error(ex);
        }

        _fileSource = new TsFileSource();
        _log.Info("TSStreamBufferPlayer9:add tsfilesource to graph");
        IBaseFilter tsBaseFilter = (IBaseFilter)_fileSource;
        int hr = _graphBuilder.AddFilter(tsBaseFilter, "TsFileSource");
        if (hr != 0)
        {
          _log.Error("TSStreamBufferPlayer9:Failed to add SBE to graph");
          return false;
        }

        #endregion

        #region add mpeg-2 demux
        MPEG2Demultiplexer demux = new MPEG2Demultiplexer();
        _mpegDemux = (IBaseFilter)demux;
        hr = _graphBuilder.AddFilter(_mpegDemux, "MPEG-2 Demultiplexer");
        #endregion


        #region load file in tsfilesource

        IFileSourceFilter interfaceFile = (IFileSourceFilter)_fileSource;
        if (interfaceFile == null)
        {
          _log.Error("TSStreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }
        //_log.Info("TSStreamBufferPlayer9: open file:{0}",filename);
        if (false==true && _isLive)
        {
          AMMediaType mpeg2ProgramStream = new AMMediaType();
          mpeg2ProgramStream.majorType = MediaType.Stream;
          mpeg2ProgramStream.subType = MediaSubType.Mpeg2Program;

          mpeg2ProgramStream.unkPtr = IntPtr.Zero;
          mpeg2ProgramStream.sampleSize = 0;
          mpeg2ProgramStream.temporalCompression = false;
          mpeg2ProgramStream.fixedSizeSamples = true;
          mpeg2ProgramStream.formatType = FormatType.None;
          mpeg2ProgramStream.formatSize = 0;
          mpeg2ProgramStream.formatPtr = IntPtr.Zero;
          hr = interfaceFile.Load(filename, mpeg2ProgramStream);
        }
        else
        {
          hr = interfaceFile.Load(filename, null);
        }
        if (hr != 0)
        {
          _log.Error("TSStreamBufferPlayer9:Failed to open file:{0} :0x{1:x}", filename, hr);
          return false;
        }
        _log.Info("TSStreamBufferPlayer9:load timeshift file");
        #endregion
        if (false == true && _isLive)
        {
          #region connect tsfilesource->demux
          _log.Info("TSStreamBufferPlayer9:connect tsfilesource->mpeg2 demux");
          IPin pinTsOut = DsFindPin.ByDirection(tsBaseFilter, PinDirection.Output, 0);
          if (pinTsOut == null)
          {
            _log.Info("TSStreamBufferPlayer9:failed to find output pin of tsfilesource");
            return false;
          }
          IPin pinDemuxIn = DsFindPin.ByDirection(_mpegDemux, PinDirection.Input, 0);
          if (pinDemuxIn == null)
          {
            _log.Info("TSStreamBufferPlayer9:failed to find output pin of tsfilesource");
            return false;
          }

          hr = _graphBuilder.Connect(pinTsOut, pinDemuxIn);
          if (hr != 0)
          {
            _log.Info("TSStreamBufferPlayer9:failed to connect tsfilesource->mpeg2 demux:{0:X}", hr);
            return false;
          }
          Marshal.ReleaseComObject(pinTsOut);
          Marshal.ReleaseComObject(pinDemuxIn);
          #endregion

          #region create mpeg2 demux pins
          _log.Info("TSStreamBufferPlayer9:create pins");

          //create mpeg-2 demux output pins
          IPin pinAudio;
          IPin pinVideo;
          IMpeg2Demultiplexer demuxer = _mpegDemux as IMpeg2Demultiplexer;
          AMMediaType mpegAudioOut = new AMMediaType();
          mpegAudioOut.majorType = MediaType.Audio;
          mpegAudioOut.subType = MediaSubType.Mpeg2Audio;
          mpegAudioOut.sampleSize = 0;
          mpegAudioOut.temporalCompression = false;
          mpegAudioOut.fixedSizeSamples = true;
          mpegAudioOut.unkPtr = IntPtr.Zero;
          mpegAudioOut.formatType = FormatType.WaveEx;
          mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
          mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
          System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
          _log.Info("TSStreamBufferPlayer9:created audio output pin");
          hr = demuxer.CreateOutputPin(mpegAudioOut, "Audio", out pinAudio);
          if (hr != 0)
          {
            _log.Error("TSStreamBufferPlayer9 FAILED to create audio output pin on demuxer");
            return false;
          }

          AMMediaType mpegVideoOut = new AMMediaType();
          mpegVideoOut.majorType = MediaType.Video;
          mpegVideoOut.subType = MediaSubType.Mpeg2Video;

          Size FrameSize = new Size(100, 100);
          mpegVideoOut.unkPtr = IntPtr.Zero;
          mpegVideoOut.sampleSize = 0;
          mpegVideoOut.temporalCompression = false;
          mpegVideoOut.fixedSizeSamples = true;

          //Mpeg2ProgramVideo=new byte[Mpeg2ProgramVideo.GetLength(0)];
          mpegVideoOut.formatType = FormatType.Mpeg2Video;
          mpegVideoOut.formatSize = Mpeg2ProgramVideo.GetLength(0);
          mpegVideoOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegVideoOut.formatSize);
          System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mpegVideoOut.formatPtr, mpegVideoOut.formatSize);

          _log.Info("TSStreamBufferPlayer9:created video output pin");
          hr = demuxer.CreateOutputPin(mpegVideoOut, "Video", out pinVideo);
          if (hr != 0)
          {
            _log.Error("TSStreamBufferPlayer9 FAILED to create video output pin on demuxer");
            return false;
          }
          #endregion


          #region map demux pids
          _log.Info("TSStreamBufferPlayer9: map pid 0xe0->video pin");
          IMPEG2StreamIdMap pStreamId = (IMPEG2StreamIdMap)pinVideo;
          hr = pStreamId.MapStreamId(0xe0, MPEG2Program.ElementaryStream, 0, 0);
          if (hr != 0)
          {
            _log.Error("TSStreamBufferPlayer9: failed to map pid 0xe0->video pin");
            return false;
          }
          _log.Info("TSStreamBufferPlayer9: map audio 0xc0->audio pin");
          pStreamId = (IMPEG2StreamIdMap)pinAudio;
          hr = pStreamId.MapStreamId(0xc0, MPEG2Program.ElementaryStream, 0, 0);
          if (hr != 0)
          {
            _log.Error("TSStreamBufferPlayer9: failed  to map pid 0xc0->audio pin");
            return false;
          }

          #endregion

          #region render demux audio/video pins
          _log.Info("TSStreamBufferPlayer9:render video output pin");
          hr = _graphBuilder.Render(pinAudio);
          if (hr != 0)
          {
            _log.Info("TSStreamBufferPlayer9:failed to render video output pin:{0:X}", hr);
          }

          _log.Info("TSStreamBufferPlayer9:render audio output pin");
          hr = _graphBuilder.Render(pinVideo);
          if (hr != 0)
          {
            _log.Info("TSStreamBufferPlayer9:failed to render audio output pin:{0:X}", hr);
          }
          #endregion
        }
        else
        {
          DirectShowUtil.RenderOutputPins(_graphBuilder, (IBaseFilter)_fileSource);
        }


        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _mediaSeeking = _graphBuilder as IMediaSeeking;
        if (_mediaSeeking == null)
        {
          _log.Error("Unable to get IMediaSeeking interface#1");
        }
        if (_audioRendererFilter != null)
        {
          //IMediaFilter mp = _graphBuilder as IMediaFilter;
          //IReferenceClock clock = _audioRendererFilter as IReferenceClock;
          //hr = mp.SetSyncSource(clock);
        }


        //        _log.Info("TSStreamBufferPlayer9:SetARMode");
        //        DirectShowUtil.SetARMode(_graphBuilder,AspectRatioMode.Stretched);

        //_log.Info("TSStreamBufferPlayer9: set Deinterlace");

        if (!_vmr9.IsVMR9Connected)
        {
          //_vmr9 is not supported, switch to overlay
          _log.Info("TSStreamBufferPlayer9: switch to overlay");
          _mediaCtrl = null;
          Cleanup();
          return base.GetInterfaces(filename);
        }

        _vmr9.SetDeinterlaceMode();
        return true;

      }
      catch (Exception ex)
      {
        _log.Error("TSStreamBufferPlayer9:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
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
      if (_graphBuilder == null)
      {
        _log.Info("TSStreamBufferPlayer9:grapbuilder=null");
        return;
      }

      int hr;
      _log.Info("TSStreamBufferPlayer9:cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
      try
      {
        if (_vmr9 != null)
        {
          _log.Info("TSStreamBufferPlayer9: vmr9 disable");
          _vmr9.Enable(false);
        }
        int counter = 0;
        while (GUIGraphicsContext.InVmr9Render)
        {
          counter++;
          System.Threading.Thread.Sleep(1);
          if (counter > 200) break;
        }

        if (_mediaCtrl != null)
        {
          hr = _mediaCtrl.Stop();
        }
        _mediaCtrl = null;
        _mediaEvt = null;
        _mediaSeeking = null;
        _videoWin = null;
        _basicAudio = null;
        _basicVideo = null;


        if (_fileSource != null)
        {
          while ((hr = Marshal.ReleaseComObject(_fileSource)) > 0) ;
          _fileSource = null;
        }


        if (_vmr9 != null)
        {
          _log.Info("TSStreamBufferPlayer9: vmr9 dispose");
          _vmr9.Dispose();
          _vmr9 = null;
        }
        if (_videoCodecFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_videoCodecFilter)) > 0) ;
          _videoCodecFilter = null;
        }
        if (_audioCodecFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_audioCodecFilter)) > 0) ;
          _audioCodecFilter = null;
        }

        if (_audioRendererFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_audioRendererFilter)) > 0) ;
          _audioRendererFilter = null;
        }

        if (_ffdShowFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_ffdShowFilter)) > 0) ;
          _ffdShowFilter = null;
        }
        if (_mpegDemux != null)
        {
          while ((hr = Marshal.ReleaseComObject(_mpegDemux)) > 0) ;
          _mpegDemux = null;
        }


        DirectShowUtil.RemoveFilters(_graphBuilder);

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if (_graphBuilder != null)
        {
          while ((hr = Marshal.ReleaseComObject(_graphBuilder)) > 0) ;
          _graphBuilder = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        _state = PlayState.Init;
        GC.Collect(); GC.Collect(); GC.Collect();
      }
      catch (Exception ex)
      {
        _log.Error("TSStreamBufferPlayer9: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }


      //switch back to directx windowed mode
      if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        _log.Info("TSStreamBufferPlayer9: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }

      _log.Info("TSStreamBufferPlayer9: Cleanup done");
    }

    protected override void OnProcess()
    {

      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      if (_vmr9 != null)
      {
        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;
      }
    }


    public override void SeekAbsolute(double dTimeInSecs)
    {

      _log.Info("SeekAbsolute:seekabs:{0}", dTimeInSecs);
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          if (dTimeInSecs < 0.0d) dTimeInSecs = 0.0d;
          if (dTimeInSecs > Duration) dTimeInSecs = Duration;
          dTimeInSecs = Math.Floor(dTimeInSecs);
          //_log.Info("StreamBufferPlayer: seekabs: {0} duration:{1} current pos:{2}", dTimeInSecs,Duration, CurrentPosition);
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
          if (hr != 0)
          {
            _log.Error("seek failed->seek to 0 0x:{0:X}", hr);
          }
        }
        UpdateCurrentPosition();
        _log.Info("StreamBufferPlayer: current pos:{0}", CurrentPosition);

      }
    }

  }
}
