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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using DShowNET;
using DShowNET.Helper;
using DShowNET.MPSA;
using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.SBE;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;
using MediaPortal.TV.Epg;
using TVCapture;
using System.Xml;
//using DirectX.Capture;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;
using MediaPortal.TV.BDA;
using MediaPortal.Configuration;
#endregion
#pragma warning disable 618

namespace MediaPortal.TV.Recording
{
  public class DVBGraphTTPremium : DVBGraphBase
  {
    #region Types
    public enum TTNetworkType
    {
	    Unkown,
	    DVB_S,		
	    DVB_C,				
	    DVB_T
    }

    public enum TTModType
    {
	    QAM_16   =  0,
	    QAM_32   =  1,
	    QAM_64   =  2,
	    QAM_128  =  3,
	    QAM_256  =  4,
    }

    public enum TTBandwidthType
    {
	    BW_6MHz  = 0,
	    BW_7MHz  = 1,
	    BW_8MHz  = 2,
	    BW_NONE  = 4
    }

#endregion

    #region variables
    private string _cardType = "";
    private string _cardFilename = "";

    protected IBaseFilter _filterTTPremium = null;
    protected ITTPremiumSource _interfaceTTPremium = null;

    protected static Guid IID_TTPremiumSource = new Guid(0x6aa08757, 0x7fa2, 0x48a2, 0xa9, 0xe5, 0x58, 0x91, 0x0e, 0x3f, 0xe8, 0xa7);
    protected static Guid CLSID_TTPremiumSource = new Guid(0x22b8142, 0x946, 0x11cf, 0xbc, 0xb1, 0x44, 0x45, 0x53, 0x54, 0x0, 0x0);

    #endregion

    #region ITTPremiumSource
    [ComVisible(true), ComImport,
      Guid("6aa08757-7fa2-48a2-a9e5-58910e3fe8a7"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITTPremiumSource
    {
      [PreserveSig]
      int Init();

      [PreserveSig]
      int Close();

      [PreserveSig]
      int GetNetworkType(out TTNetworkType network);

      [PreserveSig]
      int Tune(Int32 frequency, Int32 symbolRate, Int32 polarity, Int32 LNBKhz,
                   Int32 LNBFreq, bool LNBPower, Int32 diseq, TTModType modulation,
                   TTBandwidthType bwType, bool specInv);

      [PreserveSig]
      int GetSignalState(out bool locked, out Int32 quality, out Int32 level);
    }
    #endregion

    public DVBGraphTTPremium(TVCaptureDevice pCard)
      : base(pCard)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _cardType = xmlreader.GetValueAsString("DVBTTPremium", "cardtype", "");
        _cardFilename = xmlreader.GetValueAsString("dvb_ts_cards", "filename", "");
      }
      GetTunerCapabilities();
      _streamDemuxer.SetCardType((int)DVBEPG.EPGCard.TTPremiumCards, _networkType);
    }

    public override bool CreateGraph(int Quality)
    {
      try
      {
        _inScanningMode = false;
        //check if we didnt already create a graph
        if (_graphState != State.None)
        {
          return true;
        }

        _currentTuningObject = null;
        _isUsingAC3 = false;
        if (_streamDemuxer != null)
        {
          _streamDemuxer.GrabTeletext(false);
        }

        _captureGraphBuilderInterface = null;

        _isGraphRunning = false;
        Log.Info("DVBGraphTTPremium:CreateGraph(). ");

        //no card defined? then we cannot build a graph
        if (_card == null)
        {
          Log.Error("DVBGraphTTPremium:card is not defined");
          return false;
        }
        //create new instance of VMR9 helper utility
        _vmr9 = new VMR9Util();

        // Make a new filter graph
        // Log.WriteFile(LogType.Log,"DVBGraphTTPremium:create new filter graph (IGraphBuilder)");
        _graphBuilder = (IGraphBuilder)new FilterGraph();

        _captureGraphBuilderInterface = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
        int hr = _captureGraphBuilderInterface.SetFiltergraph(_graphBuilder);
        if (hr < 0)
        {
          Log.Error("DVBGraphTTPremium:FAILED link :0x{0:X}", hr);
          return false;
        }

        //=========================================================================================================
        // add the tt premium specific filters
        //=========================================================================================================
        Log.Info("DVBGraphTTPremium:CreateGraph() create TTPremium source filter");
        _filterTTPremium = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_TTPremiumSource, false));
        if (_filterTTPremium == null)
        {
          Log.Info("DVBGraphTTPremium:creategraph() _filterTTPremium not found");
          return false;
        }
        Log.Info("DVBGraphTTPremium:creategraph() add filters to graph");
        hr = _graphBuilder.AddFilter(_filterTTPremium, "TTPremiumSource");
        if (hr != 0)
        {
          Log.Info("DVBGraphTTPremium: FAILED to add TTPremium source filter to graph");
          return false;
        }

        // get interface
        _interfaceTTPremium = _filterTTPremium as ITTPremiumSource;
        if (_interfaceTTPremium == null)
        {
          Log.Info("DVBGraphTTPremium: cannot get ITTPremiumSource");
          return false;
        }

        //=========================================================================================================
        // initialize tuner
        //=========================================================================================================
        Log.Info("DVBGraphTTPremium: Initialize Tuner()");
        hr = _interfaceTTPremium.Init();
        if (hr != 0)
        {
          Log.Info("DVBGraphTTPremium: Tuner initialize failed:0x{0:X}", hr);
          return false;
        }

        _filterSampleGrabber = null;
        _sampleInterface = null;
        //TESTTEST: DONT USE GRABBER AT ALL
        /*

                if (GUIGraphicsContext.DX9Device != null)
                {
                  Log.Info("DVBGraphTTPremium: Add Sample Grabber");
                  _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
                  _sampleInterface = (ISampleGrabber)_filterSampleGrabber;
                  _graphBuilder.AddFilter(_filterSampleGrabber, "Sample Grabber");
                }
        */

        //=========================================================================================================
        // Add the MPEG-2 Demultiplexer 
        //=========================================================================================================
        // In this case the TTPremiumSource filter implements the IMpeg2Demuxer interface
        Log.Info("DVBGraphTTPremium: Get MPEG2-Demultiplexer filter from TTPremium source filter");
        _filterMpeg2Demultiplexer = _filterTTPremium;
        if (_filterMpeg2Demultiplexer == null)
        {
          Log.Error("DVBGraphTTPremium:Failed to get Mpeg2 Demultiplexer");
          return false;
        }

        // Add the Demux to the graph
        //_graphBuilder.AddFilter(_filterMpeg2Demultiplexer, "MPEG-2 Demultiplexer");
        IMpeg2Demultiplexer demuxer = _filterMpeg2Demultiplexer as IMpeg2Demultiplexer;

        //=========================================================================================================
        // create PSI output pin on demuxer
        //=========================================================================================================
        Log.Info("DVBGraphTTPremium: Create PSI output pin on MPEG2-Demultiplexer");
        AMMediaType mtSections = new AMMediaType();
        mtSections.majorType = MEDIATYPE_MPEG2_SECTIONS;
        mtSections.subType = MediaSubType.None;
        mtSections.formatType = FormatType.None;
        IPin pinSectionsOut;
        hr = demuxer.CreateOutputPin(mtSections, "sections", out pinSectionsOut);
        if (hr != 0 || pinSectionsOut == null)
        {
          Log.Error("DVBGraphTTPremium:FAILED to create sections pin:0x{0:X}", hr);
          return false;
        }

        Log.Info("DVBGraphTTPremium: create audio/video output pin");
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

        IPin filterMpeg2DemuxerVideoPin, filterMpeg2DemuxerAudioPin;
        hr = demuxer.CreateOutputPin(mpegVideoOut, "video", out filterMpeg2DemuxerVideoPin);
        if (hr != 0)
        {
          Log.Info("DVBGraphTTPremium:FAILED to create video output pin on demuxer");
          return false;
        }
        hr = demuxer.CreateOutputPin(mpegAudioOut, "audio", out filterMpeg2DemuxerAudioPin);
        if (hr != 0)
        {
          Log.Info("DVBGraphTTPremium: FAILED to create audio output pin on demuxer");
          return false;
        }

        //=========================================================================================================
        // add the stream analyzer
        //=========================================================================================================
        Log.Info("DVBGraphTTPremium: Add Stream Analyzer");
        _filterDvbAnalyzer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(ClassId.MPStreamAnalyzer, true));
        _analyzerInterface = (IStreamAnalyzer)_filterDvbAnalyzer;
        _epgGrabberInterface = _filterDvbAnalyzer as IEPGGrabber;
        _mhwGrabberInterface = _filterDvbAnalyzer as IMHWGrabber;
        _atscGrabberInterface = _filterDvbAnalyzer as IATSCGrabber;
        hr = _graphBuilder.AddFilter(_filterDvbAnalyzer, "Stream-Analyzer");
        if (hr != 0)
        {
          Log.Error("DVBGraphTTPremium: FAILED to add SectionsFilter 0x{0:X}", hr);
          return false;
        }

        //=========================================================================================================
        // connect TTPremiumSource "output" -> samplegrabber
        //=========================================================================================================
        if (GUIGraphicsContext.DX9Device != null && _sampleInterface != null)
        {
          Log.Info("DVBGraphTTPremium: connect TTPremiumSource->sample grabber");
          IPin pinData0 = DsFindPin.ByDirection(_filterTTPremium, PinDirection.Output, 0);
          if (pinData0 == null)
          {
            Log.Error("DVBGraphTTPremium:Failed to get pin 'output' from TTPremium source");
            return false;
          }

          IPin pinIn = DsFindPin.ByDirection(_filterSampleGrabber, PinDirection.Input, 0);
          if (pinIn == null)
          {
            Log.Error("DVBGraphTTPremium:Failed to get input pin from sample grabber");
            return false;
          }

          hr = _graphBuilder.Connect(pinData0, pinIn);
          if (hr != 0)
          {
            Log.Error("DVBGraphTTPremium:Failed to connect TTPremium->sample grabber");
            return false;
          }

          // This won't work with TTPremiumSource as it is the Demuxer
          /* 
          Log.Info("DVBGraphTTPremium: connect sample grabber->MPEG2 demultiplexer");
          if (!ConnectFilters(ref _filterSampleGrabber, ref _filterMpeg2Demultiplexer))
          {
            Log.Error("DVBGraphTTPremium:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
            return false;
          }*/
        }

        //=========================================================================================================
        // 1. connect demuxer->analyzer
        // 2. find audio/video output pins on demuxer
        //=========================================================================================================
        Log.Info("DVBGraphTTPremium:CreateGraph() find audio/video pins");

        _pinDemuxerAudio = filterMpeg2DemuxerAudioPin;
        _pinDemuxerVideo = filterMpeg2DemuxerVideoPin;
        _pinDemuxerSections = pinSectionsOut;
         
        //get the video/audio output pins of the mpeg2 demultiplexer
        if (_pinDemuxerVideo == null)
        {
          //video pin not found
          Log.Error("DVBGraphTTPremium:Failed to get pin (video out) from MPEG-2 Demultiplexer", _pinDemuxerVideo);
          return false;
        }
        if (_pinDemuxerAudio == null)
        {
          //audio pin not found
          Log.Error("DVBGraphTTPremium:Failed to get pin (audio out)  from MPEG-2 Demultiplexer", _pinDemuxerAudio);
          return false;
        }

        //=========================================================================================================
        // add the AC3 pin, mpeg1 audio pin to the MPEG2 demultiplexer
        //=========================================================================================================
        Log.Info("DVBGraphTTPremium:CreateGraph() create ac3/mpg1 pins");
        if (demuxer != null)
        {
          AMMediaType mediaAC3 = new AMMediaType();
          mediaAC3.majorType = MediaType.Audio;
          mediaAC3.subType = MediaSubType.DolbyAC3;
          mediaAC3.sampleSize = 0;
          mediaAC3.temporalCompression = false;
          mediaAC3.fixedSizeSamples = false;
          mediaAC3.unkPtr = IntPtr.Zero;
          mediaAC3.formatType = FormatType.WaveEx;
          mediaAC3.formatSize = MPEG1AudioFormat.GetLength(0);
          mediaAC3.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaAC3.formatSize);
          System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mediaAC3.formatPtr, mediaAC3.formatSize);

          hr = demuxer.CreateOutputPin(mediaAC3, "AC3", out _pinAC3Out);
          if (hr != 0 || _pinAC3Out == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to create AC3 pin:0x{0:X}", hr);
          }

          AMMediaType mediaMPG1 = new AMMediaType();
          mediaMPG1.majorType = MediaType.Audio;
          mediaMPG1.subType = MediaSubType.MPEG1AudioPayload;
          mediaMPG1.sampleSize = 0;
          mediaMPG1.temporalCompression = false;
          mediaMPG1.fixedSizeSamples = false;
          mediaMPG1.unkPtr = IntPtr.Zero;
          mediaMPG1.formatType = FormatType.WaveEx;
          mediaMPG1.formatSize = MPEG1AudioFormat.GetLength(0);
          mediaMPG1.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaMPG1.formatSize);
          System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mediaMPG1.formatPtr, mediaMPG1.formatSize);

          hr = demuxer.CreateOutputPin(mediaMPG1, "audioMpg1", out _pinMPG1Out);
          if (hr != 0 || _pinMPG1Out == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to create MPG1 pin:0x{0:X}", hr);
          }

          //=========================================================================================================
          // add the EPG/MHW pin to the MPEG2 demultiplexer
          //=========================================================================================================
          //create EPG pins
          AMMediaType mtEPG = new AMMediaType();
          mtEPG.majorType = MEDIATYPE_MPEG2_SECTIONS;
          mtEPG.subType = MediaSubType.None;
          mtEPG.formatType = FormatType.None;

          //IPin pinEPGout, pinMHW1Out, pinMHW2Out;
          hr = demuxer.CreateOutputPin(mtEPG, "EPG", out _pinDemuxerEPG);
          if (hr != 0 || _pinDemuxerEPG == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to create EPG pin:0x{0:X}", hr);
            return false;
          }
          hr = demuxer.CreateOutputPin(mtEPG, "MHW1", out _pinDemuxerMHWd2);
          if (hr != 0 || _pinDemuxerMHWd2 == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to create MHW1 pin:0x{0:X}", hr);
            return false;
          }
          hr = demuxer.CreateOutputPin(mtEPG, "MHW2", out _pinDemuxerMHWd3);
          if (hr != 0 || _pinDemuxerMHWd3 == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to create MHW2 pin:0x{0:X}", hr);
            return false;
          }

          //bool connected = false;
          IPin pinAnalyzerIn = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 0);
          if (pinAnalyzerIn == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to get Input pin on MSPA");
            return false;
          }

          IPin pinMHW1In = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 1);
          if (pinMHW1In == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to get MHW1 pin on MSPA");
            return false;
          }
          IPin pinMHW2In = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 2);
          if (pinMHW2In == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to get MHW2 pin on MSPA");
            return false;
          }
          IPin pinEPGIn = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 3);
          if (pinEPGIn == null)
          {
            Log.Error("DVBGraphTTPremium:FAILED to get EPG pin on MSPA");
            return false;
          }

          //Log.Info("DVBGraphTTPremium:Connect epg pins");
          Log.Info("DVBGraphTTPremium:connect mpeg2 demux->stream analyzer");
          hr = _graphBuilder.Connect(_pinDemuxerSections, pinAnalyzerIn);
          if (hr == 0)
          {
            //connected = true;
          }
          else
          {
              Log.Error("DVBGraphTTPremium:FAILED to connect mpeg2 demux->stream analyzer");
          }

          hr = _graphBuilder.Connect(_pinDemuxerEPG, pinEPGIn);
          if (hr != 0)
          {
            Log.Error("DVBGraphTTPremium:FAILED to connect EPG pin:0x{0:X}", hr);
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerMHWd2, pinMHW1In);
          if (hr != 0)
          {
            Log.Error("DVBGraphTTPremium:FAILED to connect MHW1 pin:0x{0:X}", hr);
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerMHWd3, pinMHW2In);
          if (hr != 0)
          {
            Log.Error("DVBGraphTTPremium:FAILED to connect MHW2 pin:0x{0:X}", hr);
            return false;
          }
          //Log.Info("DVBGraphTTPremium:Demuxer is setup");

          if (pinMHW1In != null) DirectShowUtil.ReleaseComObject(pinMHW1In); pinMHW1In = null;
          if (pinMHW2In != null) DirectShowUtil.ReleaseComObject(pinMHW2In); pinMHW2In = null;
          if (pinEPGIn != null) DirectShowUtil.ReleaseComObject(pinEPGIn); pinEPGIn = null;
          if (pinAnalyzerIn != null) DirectShowUtil.ReleaseComObject(pinAnalyzerIn); pinAnalyzerIn = null;

          //setup teletext grabbing....
          if (GUIGraphicsContext.DX9Device != null)
          {
            AMMediaType txtMediaType = new AMMediaType();
            txtMediaType.majorType = MediaType.Stream;
            txtMediaType.subType = MediaSubTypeEx.MPEG2Transport;
            hr = demuxer.CreateOutputPin(txtMediaType, "ttx", out _pinTeletext);
            if (hr != 0 || _pinTeletext == null)
            {
              Log.Error("DVBGraphTTPremium:FAILED to create ttx pin:0x{0:X}", hr);
              return false;
            }

            _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
            _sampleInterface = (ISampleGrabber)_filterSampleGrabber;
            _graphBuilder.AddFilter(_filterSampleGrabber, "Sample Grabber");

            IPin pinIn = DsFindPin.ByDirection(_filterSampleGrabber, PinDirection.Input, 0);
            if (pinIn == null)
            {
              Log.Error("DVBGraphTTPremium:unable to find sample grabber input:0x{0:X}", hr);
              return false;
            }
            hr = _graphBuilder.Connect(_pinTeletext, pinIn);
            if (hr != 0)
            {
              Log.Error("DVBGraphTTPremium:FAILED to connect demux->sample grabber:0x{0:X}", hr);
              return false;
            }
          }
        }
        else
        {
          Log.Error("DVBGraphTTPremium:mapped IMPEG2Demultiplexer not found");
        }

        //=========================================================================================================
        // Create the streambuffer engine and mpeg2 video analyzer components since we need them for
        // recording and timeshifting
        //=========================================================================================================
        m_StreamBufferSink = new StreamBufferSink();
        m_mpeg2Analyzer = new VideoAnalyzer();
        m_IStreamBufferSink = m_StreamBufferSink as IStreamBufferSink3;
        _graphState = State.Created;

        //_streamDemuxer.OnAudioFormatChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnAudioChanged(m_streamDemuxer_OnAudioFormatChanged);
        //_streamDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(m_streamDemuxer_OnPMTIsChanged);
        _streamDemuxer.SetCardType((int)DVBEPG.EPGCard.TTPremiumCards, Network());
        //_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);

        if (_sampleInterface != null)
        {
          AMMediaType mt = new AMMediaType();
          mt.majorType = MediaType.Stream;
          mt.subType = MediaSubTypeEx.MPEG2Transport;
          _sampleInterface.SetCallback(_streamDemuxer, 1);
          _sampleInterface.SetMediaType(mt);
          _sampleInterface.SetBufferSamples(false);
        }

        if (Network() == NetworkType.ATSC)
        {
          _analyzerInterface.UseATSC(1);
        }
        else
        {
          _analyzerInterface.UseATSC(0);
        }

        _epgGrabber.EPGInterface = _epgGrabberInterface;
        _epgGrabber.MHWInterface = _mhwGrabberInterface;
        _epgGrabber.ATSCInterface = _atscGrabberInterface;
        _epgGrabber.AnalyzerInterface = _analyzerInterface;
        _epgGrabber.Network = Network();

        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium:Add graph to ROT table");
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }
      return true;
    }

    public override void DeleteGraph()
    {
      try
      {
        if (_graphState < State.Created)
        {
          return;
        }

        _currentTuningObject = null;
        Log.Info("DVBGraphTTPremium:DeleteGraph(). ac3=false");
        _isUsingAC3 = false;

        Log.Info("DVBGraphTTPremium:DeleteGraph()");
        StopRecording();
        StopTimeShifting();
        StopViewing();
        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: free tuner interfaces");

        // to clear buffers for epg and teletext
        if (_streamDemuxer != null)
        {
          _streamDemuxer.GrabTeletext(false);
          _streamDemuxer.SetChannelData(0, 0, 0, 0, 0, "", 0, 0);
        }

        //Log.Info("DVBGraphTTPremium:stop graph");
        if (_mediaControl != null) _mediaControl.Stop();
        _mediaControl = null;
        //Log.Info("DVBGraphTTPremium:graph stopped");

        if (_vmr9 != null)
        {
          //Log.Info("DVBGraphTTPremium:remove vmr9");
          _vmr9.Dispose();
          _vmr9 = null;
        }

        if (m_recorderId >= 0)
        {
          DvrMsStop(m_recorderId);
          m_recorderId = -1;
        }

        _isGraphRunning = false;
        _basicVideoInterFace = null;
        _analyzerInterface = null;
        _epgGrabberInterface = null;
        _mhwGrabberInterface = null;
#if USEMTSWRITER
				_tsWriterInterface=null;
				_tsRecordInterface=null;
#endif
        //Log.Info("free pins");

        if (_pinDemuxerSections != null)
        {
          DirectShowUtil.ReleaseComObject(_pinDemuxerSections);
          _pinDemuxerSections = null;
        }

        if (_pinAC3Out != null)
        {
          DirectShowUtil.ReleaseComObject(_pinAC3Out);
          _pinAC3Out = null;
        }

        if (_pinMPG1Out != null)
        {
          DirectShowUtil.ReleaseComObject(_pinMPG1Out);
          _pinMPG1Out = null;
        }

        if (_pinDemuxerVideo != null)
        {
          DirectShowUtil.ReleaseComObject(_pinDemuxerVideo);
          _pinDemuxerVideo = null;
        }

        if (_pinDemuxerAudio != null)
        {
          DirectShowUtil.ReleaseComObject(_pinDemuxerAudio);
          _pinDemuxerAudio = null;
        }

        int hr = 0;
        if (_interfaceTTPremium != null)
        {
          hr = _interfaceTTPremium.Close();
          while ((hr = DirectShowUtil.ReleaseComObject(_interfaceTTPremium)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_interfaceTTPremium):{0}", hr);
          _interfaceTTPremium = null;
        }

        if (_filterTTPremium != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_filterTTPremium)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_filterTTPremium):{0}", hr);
          _filterTTPremium = null;
        }

        if (_filterDvbAnalyzer != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterDvbAnalyzer)) > 0) ;
          if (hr != 0) Log.Info("ReleaseComObject(_filterDvbAnalyzer):{0}", hr);
          _filterDvbAnalyzer = null;
        }
#if USEMTSWRITER
				if (_filterTsWriter!=null)
				{
					Log.Info("free MPTSWriter");
					hr=DirectShowUtil.ReleaseComObject(_filterTsWriter);
					if (hr!=0) Log.Info("ReleaseComObject(_filterTsWriter):{0}",hr);
					_filterTsWriter=null;
				}
#endif
        if (_filterSmartTee != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_filterSmartTee)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_filterSmartTee):{0}", hr);
          _filterSmartTee = null;
        }

        if (_videoWindowInterface != null)
        {
          //Log.Info("DVBGraphTTPremium:hide window");
          //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: hide video window");
          _videoWindowInterface.put_Visible(OABool.False);
          //_videoWindowInterface.put_Owner(IntPtr.Zero);
          _videoWindowInterface = null;
        }

        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: free other interfaces");
        _sampleInterface = null;
        if (_filterSampleGrabber != null)
        {
          //Log.Info("DVBGraphTTPremium:free samplegrabber");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterSampleGrabber)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_filterSampleGrabber):{0}", hr);
          _filterSampleGrabber = null;
        }


        if (m_IStreamBufferConfig != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(m_IStreamBufferConfig)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(m_IStreamBufferConfig):{0}", hr);
          m_IStreamBufferConfig = null;
        }

        if (m_IStreamBufferSink != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(m_IStreamBufferSink)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(m_IStreamBufferSink):{0}", hr);
          m_IStreamBufferSink = null;
        }

        if (m_StreamBufferSink != null)
        {
          //Log.Info("DVBGraphTTPremium:free streambuffersink");
          while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferSink)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(m_StreamBufferSink):{0}", hr);
          m_StreamBufferSink = null;
        }


        if (m_StreamBufferConfig != null)
        {
          //Log.Info("DVBGraphTTPremium:free streambufferconfig");
          while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferConfig)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(m_StreamBufferConfig):{0}", hr);
          m_StreamBufferConfig = null;
        }

        if (_filterMpeg2Demultiplexer != null)
        {
          //Log.Info("DVBGraphTTPremium:free demux");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterMpeg2Demultiplexer)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_filterMpeg2Demultiplexer):{0}", hr);
          _filterMpeg2Demultiplexer = null;
        }

        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: remove filters");

        if (_graphBuilder != null)
        {
          DirectShowUtil.RemoveFilters(_graphBuilder);
        }

        //Log.Info("DVBGraphTTPremium:free remove graph");
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;

        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: remove graph");
        if (_captureGraphBuilderInterface != null)
        {
          //Log.Info("DVBGraphTTPremium:free remove capturegraphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_captureGraphBuilderInterface)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_captureGraphBuilderInterface):{0}", hr);
          _captureGraphBuilderInterface = null;
        }

        if (_graphBuilder != null)
        {
          //Log.Info("DVBGraphTTPremium:free graphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_graphBuilder):{0}", hr);
          _graphBuilder = null;
        }

#if DUMP
				if (fileout!=null)
				{
					fileout.Close();
					fileout=null;
				}
#endif

        GC.Collect(); GC.Collect(); GC.Collect();
        _graphState = State.None;

        Thread.Sleep(200);
        //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: delete graph done");
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }


    protected override void UpdateSignalPresent()
    {
      if (_graphState == State.None)
      {
        _signalPresent = false;
        _signalQuality = 0;
        _signalLevel = 0;
        return;
      }

      if (_interfaceTTPremium == null)
      {
        return;
      }

      _interfaceTTPremium.GetSignalState(out _tunerLocked, out _signalQuality, out _signalLevel);
      _signalPresent = _tunerLocked;
    }

    public override NetworkType Network()
    {
      return _networkType;
    }

    protected override void SubmitTuneRequest(DVBChannel ch)
    {
      if (_interfaceTTPremium == null)
      {
        return;
      }

      TTModType mod = TTModType.QAM_16;
      TTBandwidthType bw = TTBandwidthType.BW_NONE;

      if (_networkType == NetworkType.DVBC || _networkType == NetworkType.DVBT)
      {
        switch ((ModulationType)ch.Modulation)
        {
          case ModulationType.Mod16Qam:
            mod = TTModType.QAM_16;
            break;

          case ModulationType.Mod32Qam:
            mod = TTModType.QAM_32;
            break;

          case ModulationType.Mod64Qam:
            mod = TTModType.QAM_64;
            break;

          case ModulationType.Mod128Qam:
            mod = TTModType.QAM_128;
            break;

          case ModulationType.Mod256Qam:
            mod = TTModType.QAM_256;
            break;

          default:
            Log.Error("DVBGraphTTPremium:SubmitTureRequest(), unsupported modulation type:{0}", (ModulationType)ch.Modulation);
            break;
        }

        switch (ch.Bandwidth)
        {
          case 0:
            bw = TTBandwidthType.BW_NONE;
            break;

          case 6:
            bw = TTBandwidthType.BW_6MHz;
            break;

          case 7:
            bw = TTBandwidthType.BW_7MHz;
            break;

          case 8:
            bw = TTBandwidthType.BW_8MHz;
            break;

          default:
            Log.Error("DVBGraphTTPremium:SubmitTureRequest(), unsupported bandwidth:{0}mhz", ch.Bandwidth);
            break;
        }

        int hr = _interfaceTTPremium.Tune(ch.Frequency, ch.Symbolrate, ch.Polarity, ch.LnbSwitchFrequency, ch.LNBFrequency, true, ch.DiSEqC, mod, bw, false);
        if (hr != 0)
        {
          Log.Error("DVBGraphTTPremium:SubmitTureRequest(), filter tune request failed:{0}", hr);
        }
      }
      else if (_networkType == NetworkType.DVBS)
      {
        int diseq = 0;
        int low = 0;
        int high = 0;
        int lnbKhzTone=0;
        GetDisEqcSettings(ref ch, out low, out high,out  lnbKhzTone, out diseq);
        ch.LNBFrequency = low;
        int hr = _interfaceTTPremium.Tune(ch.Frequency, ch.Symbolrate, ch.Polarity, ch.LnbSwitchFrequency, ch.LNBFrequency, true, diseq, mod, bw, false);
        if (hr != 0)
        {
          Log.Error("DVBGraphTTPremium:SubmitTuneRequest(), filter tune request failed:{0},{1},{2},{3},{4},{5},{6},{7},{8}", ch.Frequency, ch.Symbolrate, ch.Polarity, ch.LnbSwitchFrequency, ch.LNBFrequency, true, diseq, mod, bw, false);
          _signalPresent = false;
          _signalQuality = 0;
          _signalLevel = 0;
          return;
        }
      }

      Thread.Sleep(120);

      UpdateSignalPresent();
      if (!_inScanningMode)
      {
        SetHardwarePidFiltering();
      }
      _processTimer = DateTime.MinValue;
      _pmtSendCounter = 0;
    }

    protected override void SendHWPids(ArrayList pids)
    {
    }

    protected override void SetupDiseqc(int disNo)
    {
      _currentTuningObject.DiSEqC = disNo;
    }// SetupDiseqc()

     protected void DeleteAllPids(int pinIndex)
    {
      // Get the index pin and delete any PIDs that are mapped to it
      IPin pin = DsFindPin.ByDirection(_filterTTPremium, PinDirection.Output, pinIndex);
      if (pin == null)
      {
        Log.Error("DVBGraphTTPremium:DeleteAllPids(), couldn't find pin:{0}", pinIndex);
      }


      IMPEG2PIDMap mpPidMap = pin as IMPEG2PIDMap;
      IEnumPIDMap enumPid = null;
      // Enumerate all the PIDs
      int hr = mpPidMap.EnumPIDMap(out enumPid);
      if (hr != 0)
      {
        Log.Error("DVBGraphTTPremium:DeleteAllPids(), couldn't enumerate PIDs on pin:{0}, hr:{1}", pinIndex, hr);
      }
      
      List<int> pids = new List<int>();
      PIDMap []pidMap = new PIDMap[1];

      int count = 0;
      while (enumPid.Next(1, pidMap, out count) == 0)
      {
        if (count == 1)
        {
          pids.Add(pidMap[0].ulPID);
        }
      }

      int[] deletePids = new int[pids.Count];
      pids.CopyTo(deletePids);
      hr = mpPidMap.UnmapPID(deletePids.Length, deletePids);
      if (hr != 0)
      {
        Log.Error("DVBGraphTTPremium:DeleteAllPids(), couldn't delete PIDs on pin:{0}, hr:{1}", pinIndex, hr);
      }
    }
    private void GetTunerCapabilities()
    {
      try
        {
          // Make a new filter graph
          // Log.WriteFile(LogType.Log,"DVBGraphTTPremium:create new filter graph (IGraphBuilder)");
          _graphBuilder = (IGraphBuilder)new FilterGraph();

          // Get the Capture Graph Builder
          _captureGraphBuilderInterface = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

          //Log.WriteFile(LogType.Log,"DVBGraphTTPremium:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
          int hr = _captureGraphBuilderInterface.SetFiltergraph(_graphBuilder);
          if (hr < 0)
          {
            Log.Error("DVBGraphTTPremium:FAILED link :0x{0:X}", hr);
              return;
          }

          //=========================================================================================================
          // add the tt premium specific filters
          //=========================================================================================================
          Log.Info("DVBGraphTTPremium:CreateGraph() create TTPremium source filter");
          _filterTTPremium = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_TTPremiumSource, false));
          if (_filterTTPremium == null)
          {
            Log.Info("DVBGraphTTPremium:creategraph() _filterTTPremium not found");
              return;
          }
          // get interface
          _interfaceTTPremium = _filterTTPremium as ITTPremiumSource;
          if (_interfaceTTPremium == null)
          {
            Log.Info("DVBGraphTTPremium: cannot get ITTPremiumSource");
              return;
          }

          //=========================================================================================================
          // initialize tuner
          //=========================================================================================================
          Log.Info("DVBGraphTTPremium: Initialize Tuner()");
          hr = _interfaceTTPremium.Init();
          if (hr != 0)
          {
            Log.Info("DVBGraphTTPremium: Tuner initialize failed:0x{0:X}", hr);
              return;
          }

          // Get network type (DVBS, DVBC, DVBT)
          TTNetworkType nt;
          hr = _interfaceTTPremium.GetNetworkType(out nt);
          if (hr != 0)
          {
            Log.Info("DVBGraphTTPremium: Network Type failed:0x{0:X}", hr);
              return;
          }

          switch (nt)
          {
            case TTNetworkType.DVB_S:
              Log.Info("DVBGraphTTPremium: Network type=DVBS");
              _networkType = NetworkType.DVBS;
              break;
            case TTNetworkType.DVB_C:
              Log.Info("DVBGraphTTPremium: Network type=DVBC");
              _networkType = NetworkType.DVBC;
              break;
            case TTNetworkType.DVB_T:
              Log.Info("DVBGraphTTPremium: Network type=DVBT");
              _networkType = NetworkType.DVBT;
              break;
            case TTNetworkType.Unkown:
              Log.Info("DVBGraphTTPremium: Network type=unknown?");
              _networkType = NetworkType.Unknown;
              break;
          }

          if (_interfaceTTPremium != null)
          {
            hr = _interfaceTTPremium.Close();
            while ((hr = DirectShowUtil.ReleaseComObject(_interfaceTTPremium)) > 0) ;
            if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_interfaceTTPremium):{0}", hr);
            _interfaceTTPremium = null;
          }

          if (_filterTTPremium != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(_filterTTPremium)) > 0) ;
            if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_filterTTPremium):{0}", hr);
            _filterTTPremium = null;
          }

          //Log.WriteFile(LogType.Log,"DVBGraphTTPremium: remove graph");
          if (_captureGraphBuilderInterface != null)
          {
            //Log.Info("DVBGraphTTPremium:free remove capturegraphbuilder");
            while ((hr = DirectShowUtil.ReleaseComObject(_captureGraphBuilderInterface)) > 0) ;
            if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_captureGraphBuilderInterface):{0}", hr);
            _captureGraphBuilderInterface = null;
          }

          if (_graphBuilder != null)
          {
            //Log.Info("DVBGraphTTPremium:free graphbuilder");
            while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
            if (hr != 0) Log.Info("DVBGraphTTPremium:ReleaseComObject(_graphBuilder):{0}", hr);
            _graphBuilder = null;
          }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
}