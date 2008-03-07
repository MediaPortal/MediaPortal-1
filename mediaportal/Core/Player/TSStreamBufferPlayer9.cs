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
using DirectShowLib.BDA;
using DShowNET.Helper;
using DShowNET.TsFileSink;
using MediaPortal.Configuration;
using MediaPortal.Player.Subtitles;

namespace MediaPortal.Player
{
  public class TStreamBufferPlayer9 : BaseTStreamBufferPlayer
  {
    #region structs
    static byte[] Mpeg2ProgramVideo = 
    {
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.left
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.top
          0xd0, 0x02, 0x00, 0x00,							//  .hdr.rcSource.right
          0x40, 0x02, 0x00, 0x00,							//  .hdr.rcSource.bottom
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.left
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.top
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.right
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.bottom
          0xc0, 0xe1, 0xe4, 0x00,							//  .hdr.dwBitRate
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwBitErrorRate
          0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwInterlaceFlags
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwCopyProtectFlags
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioX
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioY
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved1
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved2
          0x28, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSize
          0xd0, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biWidth
          0x40, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biHeight
          0x00, 0x00,										//  .hdr.bmiHeader.biPlanes
          0x00, 0x00,										//  .hdr.bmiHeader.biBitCount
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biCompression
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSizeImage
          0xd0, 0x07, 0x00, 0x00,							//  .hdr.bmiHeader.biXPelsPerMeter
          0x42, 0xd8, 0x00, 0x00,							//  .hdr.bmiHeader.biYPelsPerMeter
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrUsed
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrImportant
          0x00, 0x00, 0x00, 0x00,							//  .dwStartTimeCode
          0x4c, 0x00, 0x00, 0x00,							//  .cbSequenceHeader
          0x00, 0x00, 0x00, 0x00,							//  .dwProfile
          0x00, 0x00, 0x00, 0x00,							//  .dwLevel
          0x00, 0x00, 0x00, 0x00,							//  .Flags
					                        //  .dwSequenceHeader [1]
          0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
          0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
          0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
          0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
          0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
          0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
          0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
          0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
          0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
          0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    static byte[] MPEG2AudioFormat =
      {	
        0x50, 0x00,				//wFormatTag
	      0x02, 0x00,				//nChannels
	      0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
	      0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
	      0x01, 0x00,				//nBlockAlign
	      0x00, 0x00,				//wBitsPerSample
	      0x16, 0x00,				//cbSize
	      0x02, 0x00,				//wValidBitsPerSample
	      0x00, 0xe8,				//wSamplesPerBlock
	      0x03, 0x00,				//wReserved
	      0x01, 0x00, 0x01, 0x00, //dwChannelMask
	      0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };
    #endregion
    #region variables
    VMR9Util _vmr9 = null;
    IPin _pinAudio = null;
    IPin _pinVideo = null;
    IPin _pinPcr = null;
    IPin _pinSubtitle = null;
    IPin _pinPMT = null;
    bool enableDvbSubtitles = false;
    #endregion

    #region ctor
    public TStreamBufferPlayer9()
      : base()
    {
    }
    public TStreamBufferPlayer9(g_Player.MediaType type)
      : base(type)
    {
    }
    #endregion


    protected override void OnInitialized()
    {
      Log.Info("tsplayer9:OnInitialized");
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
        return;

      _updateNeeded = false;
      _isStarted = true;

    }


    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      //create the pins on the demux then connect the demux then mapp the pids.
      Speed = 1;
      Log.Info("TSStreamBufferPlayer9: GetInterfaces()");

      // switch back to directx fullscreen mode
      Log.Info("TSStreamBufferPlayer9: Enabling DX9 exclusive mode");
      if (_isRadio == false)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      //Log.Info("TSStreamBufferPlayer9: build graph");

      try
      {
        _graphBuilder = (IGraphBuilder)new FilterGraph();

        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);
        #region add vmr9
        if (_isRadio == false)
        {
          Log.Info("TSStreamBufferPlayer9: add _vmr9");
          _vmr9 = new VMR9Util();
          _vmr9.AddVMR9(_graphBuilder);
          _vmr9.Enable(false);
        }
        #endregion

        #region add codecs

        Log.Info("TSStreamBufferPlayer9: add codecs");
        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strH264VideoCodec = "";
        string strAudioCodec = "";
        string strAACAudioCodec = "";
        string strAudioRenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
          strH264VideoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAACAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
          enableDvbSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbsubtitles", false);
          string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
          if (strValue.Equals("zoom"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
          if (strValue.Equals("stretch"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
          if (strValue.Equals("normal"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
          if (strValue.Equals("original"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
          if (strValue.Equals("letterbox"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
          if (strValue.Equals("panscan"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
          if (strValue.Equals("zoom149"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom14to9;
        }
        if (_isRadio == false)
        {
          if (strVideoCodec.Length > 0)
            _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
        }
        if (strH264VideoCodec.Length > 0)
          _h264videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strH264VideoCodec);
        if (strAudioCodec.Length > 0)
          _audioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
        if (strAACAudioCodec.Length > 0)
          _aacaudioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAACAudioCodec);
        if (strAudioRenderer.Length > 0)
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, true);
        if (enableDvbSubtitles == true)
        {
          try
          {
            _subtitleFilter = SubtitleRenderer.GetInstance().AddSubtitleFilter(_graphBuilder);
            SubtitleRenderer.GetInstance().SetPlayer(this);
            dvbSubRenderer = SubtitleRenderer.GetInstance();
          }
          catch (Exception e)
          {
            Log.Error(e);
          }
        }
        Log.Debug("Is subtitle fitler null? {0}",(_subtitleFilter == null));
        // FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          customFilters[i] = DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
        }
        #endregion

        bool demuxControl = true;     // let tsfilesource control demux 
        bool supplyMediaType = false;  // supply media type during load
        bool autoBuildGraph = true;   // true: let tsfilesource create graph, else we do it ourselves
        if (_isLive == false)
        {
          demuxControl = true;
          supplyMediaType = false;
          autoBuildGraph = true;
        }

        #region set tsfilesource settings

        Log.Info("TSStreamBufferPlayer9:initialize tsfilesource");
        try
        {
          using (RegistryKey hklm = Registry.CurrentUser)
          {
            using (RegistryKey settings = hklm.OpenSubKey(@"SOFTWARE\TSFileSource\settings\default", true))
            {
              byte[] clockType = new byte[4] { 3, 0, 0, 0 };
              byte[] programSid = new byte[4] { 0, 0, 0, 0 };
              byte[] value1Zeros = new byte[4];
              byte[] valueZero = new byte[1];
              byte[] valueOne = new byte[1];
              valueZero[0] = 0;
              valueOne[0] = 1;
              //                     
              // --------------------
              //clocktype:           
              // --------------------
              // 0=default           
              // 1=tsfilesource      
              // 2=demux             
              // 3=audio renderer    
              settings.SetValue("clockType", clockType, RegistryValueKind.Binary);
              settings.SetValue("enableAC3", valueZero, RegistryValueKind.Binary);//prefer AC3
              settings.SetValue("enableAudio2", valueZero, RegistryValueKind.Binary);

              if (demuxControl == false)
              {
                Log.Info("set tsfilesource to manual mode");
                settings.SetValue("enableAuto", valueZero, RegistryValueKind.Binary);
              }
              else
              {
                Log.Info("set tsfilesource to auto mode");
                settings.SetValue("enableAuto", valueOne, RegistryValueKind.Binary);
              }
              settings.SetValue("enableInsertMode", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableDelay", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableFixedAR", valueOne, RegistryValueKind.Binary);
              settings.SetValue("enableMP2", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableNPControl", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableNPSlave", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableRateControl", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableROT", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableTSPin", valueZero, RegistryValueKind.Binary);
              settings.SetValue("enableTxtPin", valueZero, RegistryValueKind.Binary);
              settings.SetValue("ProgramSID", programSid, RegistryValueKind.Binary);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }

        _fileSource = (IBaseFilter)new TsFileSource();
        Log.Info("TSStreamBufferPlayer9:add tsfilesource to graph");
        int hr = _graphBuilder.AddFilter((IBaseFilter)_fileSource, "TsFileSource");
        if (hr != 0)
        {
          Log.Error("TSStreamBufferPlayer9:Failed to add TsFileSource to graph");
          return false;
        }
        ITSFileSource source = _fileSource as ITSFileSource;
        if (source != null)
        {
          source.SetMP2Mode(0);
          source.SetAC3Mode(0);
          //source.SetFixedAspectRatio(1);
          //source.SetAutoMode(1);
          source.SetClockMode(3);
        }
        else
        {
          Log.Error("TSStreamBufferPlayer9:unable to get ITSFileSource interface");
        }
        #endregion

        #region add mpeg-2 demux filter
        //forces tsfilesource to connect to the ms-demuxer and not another demuxer registered
        Log.Info("TSStreamBufferPlayer9:add mpeg-2 demultiplexer to graph");
        _mpegDemux = (IBaseFilter)new MPEG2Demultiplexer();
        hr = _graphBuilder.AddFilter(_mpegDemux, "MPEG-2 Demultiplexer");

        #endregion

        #region create mpeg2 demux pins when autoBuildGraph == false
        if (autoBuildGraph == false)
        {
          Log.Info("TSStreamBufferPlayer9:create audio/video pins");
          //create mpeg-2 demux output pins
          IMpeg2Demultiplexer demuxer = _mpegDemux as IMpeg2Demultiplexer;

          Log.Info("TSStreamBufferPlayer9:created audio output pin");
          hr = demuxer.CreateOutputPin(GetAudioMpg2Media(), "Audio", out _pinAudio);
          if (hr != 0)
          {
            Log.Error("TSStreamBufferPlayer9 FAILED to create audio output pin on demuxer");
            return false;
          }
          if (_isRadio == false)
          {
            Log.Info("TSStreamBufferPlayer9:created video output pin");
            hr = demuxer.CreateOutputPin(GetVideoMpg2Media(), "Video", out _pinVideo);
            if (hr != 0)
            {
              Log.Error("TSStreamBufferPlayer9 FAILED to create video output pin on demuxer");
              return false;
            }
          }
        }
        #endregion

        #region load file in tsfilesource
        //call the load() on tsfilesource. This is needed so tsfilesource will configure itself
        //to mpeg-2 program stream mode instead of mpeg-2 transport stream mode.
        //when its in program stream mode we can connect it to the demuxer
        IFileSourceFilter interfaceFile = (IFileSourceFilter)_fileSource;
        if (interfaceFile == null)
        {
          Log.Error("TSStreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }
        //Log.Info("TSStreamBufferPlayer9: open file:{0}",filename);
        if (supplyMediaType)
        {
          Log.Info("TSStreamBufferPlayer9: open file with mediatype:{0}", filename);
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
          Log.Info("TSStreamBufferPlayer9: open file:{0}", filename);
          hr = interfaceFile.Load(filename, null);
        }
        if (hr != 0)
        {
          Log.Error("TSStreamBufferPlayer9:Failed to open file:{0} :0x{1:x}", filename, hr);
          return false;
        }
        
        #endregion

        #region connect tsfilesource->demux
        Log.Info("TSStreamBufferPlayer9:connect tsfilesource->mpeg2 demux");
        IPin pinTsOut = DsFindPin.ByDirection((IBaseFilter)_fileSource, PinDirection.Output, 0);
        if (pinTsOut == null)
        {
          Log.Info("TSStreamBufferPlayer9:failed to find output pin of tsfilesource");
          return false;
        }
        IPin pinDemuxIn = DsFindPin.ByDirection(_mpegDemux, PinDirection.Input, 0);
        if (pinDemuxIn == null)
        {
          Log.Info("TSStreamBufferPlayer9:failed to find output pin of tsfilesource");
          return false;
        }

        hr = _graphBuilder.Connect(pinTsOut, pinDemuxIn);
        if (hr != 0)
        {
          Log.Info("TSStreamBufferPlayer9:failed to connect tsfilesource->mpeg2 demux:{0:X}", hr);
          return false;
        }
        DirectShowUtil.ReleaseComObject(pinTsOut);
        DirectShowUtil.ReleaseComObject(pinDemuxIn);

        #endregion

        #region render demux output pins
        if (_isRadio)
        {
          IEnumPins enumPins;
          _mpegDemux.EnumPins(out enumPins);
          IPin[] pins = new IPin[2];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1) break;
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input) continue;
            IEnumMediaTypes enumMediaTypes;
            pins[0].EnumMediaTypes(out enumMediaTypes);
            AMMediaType[] mediaTypes = new AMMediaType[20];
            int fetchedTypes;
            enumMediaTypes.Next(20, mediaTypes, out fetchedTypes);
            for (int i = 0; i < fetchedTypes; ++i)
            {
              if (mediaTypes[i].majorType == MediaType.Audio)
              {
                _graphBuilder.Render(pins[0]);
                break;
              }
            }
          }
        }
        else
        {
          Log.Info("TSStreamBufferPlayer9:render demux outputs");
          IEnumPins enumPins;
          _mpegDemux.EnumPins(out enumPins);
          IPin[] pins = new IPin[2];
          int fetched = 0;
          while (enumPins.Next(1, pins, out fetched) == 0)
          {
            if (fetched != 1) break;
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input) continue;
            _graphBuilder.Render(pins[0]);
          }
        }
        #endregion

        if (autoBuildGraph == false)
        {
          MapPids();
        }

        // Connect DVB subtitle filter pins in the graph
        if (_mpegDemux != null && enableDvbSubtitles == true)
        {
          IMpeg2Demultiplexer demuxer = _mpegDemux as IMpeg2Demultiplexer;
          hr = demuxer.CreateOutputPin(GetTSMedia(), "Pcr", out _pinPcr);

          if (hr == 0)
          {
            Log.Info("TSStreamBufferPlayer9:_pinPcr OK");

            IPin pDemuxerPcr = DsFindPin.ByName(_mpegDemux, "Pcr");
            IPin pSubtitlePcr = DsFindPin.ByName(_subtitleFilter, "Pcr");
            hr = _graphBuilder.Connect(pDemuxerPcr, pSubtitlePcr);
          }
          else
          {
            Log.Info("TSStreamBufferPlayer9:Failed to create _pinPcr in demuxer:{0:X}", hr);
          }

          hr = demuxer.CreateOutputPin(GetTSMedia(), "Subtitle", out _pinSubtitle);
          if (hr == 0)
          {
            Log.Info("TSStreamBufferPlayer9:_pinSubtitle OK");

            IPin pDemuxerSubtitle = DsFindPin.ByName(_mpegDemux, "Subtitle");
            IPin pSubtitle = DsFindPin.ByName(_subtitleFilter, "In");
            hr = _graphBuilder.Connect(pDemuxerSubtitle, pSubtitle);
          }
          else
          {
            Log.Info("TSStreamBufferPlayer9:Failed to create _pinSubtitle in demuxer:{0:X}", hr);
          }

          hr = demuxer.CreateOutputPin(GetTSMedia(), "PMT", out _pinPMT);
          if (hr == 0)
          {
            Log.Info("TSStreamBufferPlayer9:_pinPMT OK");

            IPin pDemuxerSubtitle = DsFindPin.ByName(_mpegDemux, "PMT");
            IPin pSubtitle = DsFindPin.ByName(_subtitleFilter, "PMT");
            hr = _graphBuilder.Connect(pDemuxerSubtitle, pSubtitle);
          }
          else
          {
            Log.Info("TSStreamBufferPlayer9:Failed to create _pinPMT in demuxer:{0:X}", hr);
          }
        }

        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _mediaSeeking = _graphBuilder as IMediaSeeking;
        if (_mediaSeeking == null)
        {
          Log.Error("Unable to get IMediaSeeking interface#1");
        }
        
        source.SetClockMode(3);//audio renderer
        if (_audioRendererFilter != null)
        {
          //Log.Info("TSStreamBufferPlayer9:set reference clock");
          IMediaFilter mp = _graphBuilder as IMediaFilter;
          IReferenceClock clock = _audioRendererFilter as IReferenceClock;
          hr = mp.SetSyncSource(null);
          hr = mp.SetSyncSource(clock);
          //Log.Info("TSStreamBufferPlayer9:set reference clock:{0:X}", hr);
          _basicAudio = (IBasicAudio)_graphBuilder;
          //_mediaSeeking.SetPositions(new DsLong(0), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(0), AMSeekingSeekingFlags.NoPositioning);
        }

        if (_isRadio == false)
        {
          if (!_vmr9.IsVMR9Connected)
          {
            while (true)
            {
              Application.DoEvents();
              System.Threading.Thread.Sleep(100);
            }
            /*
            //_vmr9 is not supported, switch to overlay
            Log.Info("TSStreamBufferPlayer9: switch to overlay");
            _mediaCtrl = null;
            Cleanup();
            return base.GetInterfaces(filename);
             */
          }

          _vmr9.SetDeinterlaceMode();
        }
        return true;

      }
      catch (Exception ex)
      {
        Log.Error("TSStreamBufferPlayer9:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
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
        Log.Info("TSStreamBufferPlayer9:grapbuilder=null");
        return;
      }

      int hr;
      Log.Info("TSStreamBufferPlayer9:cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
      try
      {
        if (_vmr9 != null)
        {
          Log.Info("TSStreamBufferPlayer9: vmr9 disable");
          _vmr9.Enable(false);
        }
        int counter = 0;
        while (GUIGraphicsContext.InVmr9Render)
        {
          counter++;
          System.Threading.Thread.Sleep(100);
          if (counter > 100)
            break;
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
          while ((hr = DirectShowUtil.ReleaseComObject(_fileSource)) > 0)
            ;
          _fileSource = null;
        }
        if (_pinAudio != null)
        {
          DirectShowUtil.ReleaseComObject(_pinAudio);
          _pinAudio = null;
        }
        if (_pinVideo != null)
        {
          DirectShowUtil.ReleaseComObject(_pinVideo);
          _pinVideo = null;
        }
        if (_videoCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_videoCodecFilter)) > 0) ;
          _videoCodecFilter = null;
        }
        if (_h264videoCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_h264videoCodecFilter)) > 0) ;
          _h264videoCodecFilter = null;
        }
        if (_audioCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_audioCodecFilter)) > 0) ;
          _audioCodecFilter = null;
        }
        if (_aacaudioCodecFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_aacaudioCodecFilter)) > 0) ;
          _aacaudioCodecFilter = null;
        }
        if (_audioRendererFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_audioRendererFilter)) > 0)
            ;
          _audioRendererFilter = null;
        }

        if (_subtitleFilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_subtitleFilter)) > 0)
            ;
          _subtitleFilter = null;
          if(this.dvbSubRenderer != null) this.dvbSubRenderer.SetPlayer(null);
          this.dvbSubRenderer = null;
        }

        // FlipGer: release custom filters
        for (int i = 0; i < customFilters.Length; i++)
        {
          if (customFilters[i] != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(customFilters[i])) > 0) ;
          }
          customFilters[i] = null;
        }
        if (_mpegDemux != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_mpegDemux)) > 0)
            ;
          _mpegDemux = null;
        }

        DirectShowUtil.RemoveFilters(_graphBuilder);

        if (_vmr9 != null)
        {
          Log.Info("TSStreamBufferPlayer9: vmr9 dispose");
          _vmr9.Dispose();
          _vmr9 = null;
        }

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if (_graphBuilder != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0)
            ;
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
        Log.Error("TSStreamBufferPlayer9: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }

      //switch back to directx windowed mode
      Log.Info("TSStreamBufferPlayer9: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);

      Log.Info("TSStreamBufferPlayer9: Cleanup done");
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

      Log.Info("SeekAbsolute:seekabs:{0} duration:{1} pos:{2}", dTimeInSecs, Duration, CurrentPosition);
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          if (dTimeInSecs < 0.0d)
            dTimeInSecs = 0.0d;
          //if (dTimeInSecs > Duration)
          //  dTimeInSecs = Duration;
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
          if (hr != 0)
          {
            Log.Error("seek failed->seek to 0 0x:{0:X}", hr);
          }
        }
        UpdateCurrentPosition();
        if (dvbSubRenderer != null) dvbSubRenderer.OnSeek(CurrentPosition);
        _state = PlayState.Playing;
        Log.Info("StreamBufferPlayer: current pos:{0} dur:{1}", CurrentPosition,Duration);
      }
    }

    AMMediaType GetAudioMpg2Media()
    {
      AMMediaType mediaAudio = new AMMediaType();
      mediaAudio.majorType = MediaType.Audio;
      mediaAudio.subType = MediaSubType.Mpeg2Audio;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatPtr = IntPtr.Zero;
      mediaAudio.sampleSize = 1;
      mediaAudio.temporalCompression = false;
      mediaAudio.fixedSizeSamples = true;
      mediaAudio.unkPtr = IntPtr.Zero;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatSize = MPEG2AudioFormat.GetLength(0);
      mediaAudio.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaAudio.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(MPEG2AudioFormat, 0, mediaAudio.formatPtr, mediaAudio.formatSize);
      return mediaAudio;
    }
    AMMediaType GetVideoMpg2Media()
    {
      AMMediaType mediaVideo = new AMMediaType();
      mediaVideo.majorType = MediaType.Video;
      mediaVideo.subType = MediaSubType.Mpeg2Video;
      mediaVideo.formatType = FormatType.Mpeg2Video;
      mediaVideo.unkPtr = IntPtr.Zero;
      mediaVideo.sampleSize = 1;
      mediaVideo.temporalCompression = false;
      mediaVideo.fixedSizeSamples = true;
      mediaVideo.formatSize = Mpeg2ProgramVideo.GetLength(0);
      mediaVideo.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaVideo.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mediaVideo.formatPtr, mediaVideo.formatSize);
      return mediaVideo;
    }

    AMMediaType GetTSMedia()
    {
      AMMediaType mediaAudioTS = new AMMediaType();
      mediaAudioTS.majorType = MediaType.Stream;
      mediaAudioTS.subType = MediaSubType.Mpeg2Transport;
      mediaAudioTS.formatType = FormatType.Null;
      mediaAudioTS.formatPtr = IntPtr.Zero;
      mediaAudioTS.sampleSize = 1;
      mediaAudioTS.temporalCompression = false;
      mediaAudioTS.fixedSizeSamples = true;
      mediaAudioTS.unkPtr = IntPtr.Zero;
      mediaAudioTS.formatType = FormatType.None;
      mediaAudioTS.formatSize = 0;
      mediaAudioTS.formatPtr = IntPtr.Zero;
      return mediaAudioTS;
    }

    AMMediaType GetSubtitleMedia()
    {
      AMMediaType mediaSubtitle = new AMMediaType();
      mediaSubtitle.majorType = MediaType.Null;
      mediaSubtitle.subType = MediaSubType.Null;
      mediaSubtitle.formatType = FormatType.Null;
      mediaSubtitle.formatPtr = IntPtr.Zero;
      mediaSubtitle.sampleSize = 1;
      mediaSubtitle.temporalCompression = false;
      mediaSubtitle.fixedSizeSamples = true;
      mediaSubtitle.unkPtr = IntPtr.Zero;
      mediaSubtitle.formatType = FormatType.None;
      mediaSubtitle.formatSize = 0;
      mediaSubtitle.formatPtr = IntPtr.Zero;
      return mediaSubtitle;
    }
    void MapPids()
    {
      if (_pinAudio == null) return;
      if (_pinVideo == null) return;
      #region map demux pids
      int hr;
      IMPEG2StreamIdMap pStreamId;
      if (_isRadio == false)
      {
        Log.Info("TSStreamBufferPlayer9: map pid 0xe0->video pin");
        pStreamId = (IMPEG2StreamIdMap)_pinVideo;
        for (int pid = 0xe0; pid <= 0xef; pid++)
        {
          hr = pStreamId.MapStreamId(pid, MPEG2Program.ElementaryStream, 0, 0);
          if (hr != 0)
          {
            Log.Error("TSStreamBufferPlayer9: failed to map pid 0xe0->video pin");
            return;
          }
        }
      }
      Log.Info("TSStreamBufferPlayer9: map audio 0xc0->audio pin");
      pStreamId = (IMPEG2StreamIdMap)_pinAudio;
      hr = pStreamId.MapStreamId(0xc0, MPEG2Program.ElementaryStream, 0, 0);
      if (hr != 0)
      {
        Log.Error("TSStreamBufferPlayer9: failed  to map pid 0xc0->audio pin");
        return;
      }

      #endregion
    }
  }
}
