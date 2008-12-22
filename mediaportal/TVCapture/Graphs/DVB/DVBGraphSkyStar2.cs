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

#define HW_PID_FILTERING
//#define DUMP
//#define USEMTSWRITER
#define COMPARE_PMT
#region usings
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

namespace MediaPortal.TV.Recording
{
  public class DVBGraphSkyStar2 : DVBGraphBase
  {
    #region enums

    public enum TunerType
    {
      ttSat = 0,
      ttCable = 1,
      ttTerrestrial = 2,
      ttATSC = 3,
      ttUnknown = -1
    }
    protected enum eModulationTAG
    {
      QAM_4 = 2,
      QAM_16,
      QAM_32,
      QAM_64,
      QAM_128,
      QAM_256,
      MODE_UNKNOWN = -1
    };
    protected enum GuardIntervalType
    {
      Interval_1_32 = 0,
      Interval_1_16,
      Interval_1_8,
      Interval_1_4,
      Interval_Auto
    };
    protected enum BandWidthType
    {
      MHz_6 = 6,
      MHz_7 = 7,
      MHz_8 = 8,
    };
    protected enum DisEqcType
    {
      None = 0,
      Simple_A = 1,
      Simple_B = 2,
      Level_1_A_A = 3,
      Level_1_B_A = 4,
      Level_1_A_B = 5,
      Level_1_B_B = 6,
    };
    protected enum FecType
    {
      Fec_1_2 = 1,
      Fec_2_3 =2,
      Fec_3_4 = 3,
      Fec_5_6 = 4,
      Fec_7_8 = 5,
      Fec_Auto =6,
    }

    protected enum LNBSelectionType
    {
      Lnb0 = 0,
      Lnb22kHz = 1,
      Lnb33kHz = 2,
      Lnb44kHz = 3,
    } ;

    protected enum PolarityType
    {
      Horizontal = 0,
      Vertical = 1,
    };

    #endregion

    #region Structs
    /*
				*	Structure completedy by GetTunerCapabilities() to return tuner capabilities
				*/
    public struct tTunerCapabilities
    {
      public TunerType eModulation;
      public int dwConstellationSupported;       // Show if SetModulation() is supported
      public int dwFECSupported;                 // Show if SetFec() is suppoted
      public int dwMinTransponderFreqInKHz;
      public int dwMaxTransponderFreqInKHz;
      public int dwMinTunerFreqInKHz;
      public int dwMaxTunerFreqInKHz;
      public int dwMinSymbolRateInBaud;
      public int dwMaxSymbolRateInBaud;
      public int bAutoSymbolRate;				// Obsolte		
      public int dwPerformanceMonitoring;        // See bitmask definitions below
      public int dwLockTimeInMilliSecond;		// lock time in millisecond
      public int dwKernelLockTimeInMilliSecond;	// lock time for kernel
      public int dwAcquisitionCapabilities;
    }

    #endregion
    #region imports

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetPidToPin(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin, UInt16 pid);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteAllPIDs(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetSNR(DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 tunerCtrl, [Out] out int a, [Out] out int b);
    #endregion

    #region variables
    protected IBaseFilter _filterB2C2Adapter = null;
    protected DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 _interfaceB2C2DataCtrl = null;
    protected DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 _interfaceB2C2TunerCtrl = null;
    //protected DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2 _interfaceB2C2AvcCtrl = null;
    string _cardType = "";


    #endregion

    public DVBGraphSkyStar2(TVCaptureDevice pCard)
      : base(pCard)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _cardType = xmlreader.GetValueAsString("DVBSS2", "cardtype", "");
      }
      GetTunerCapabilities();
      _streamDemuxer.SetCardType((int)DVBEPG.EPGCard.TechnisatStarCards, _networkType);
      _cardProperties = new VideoCaptureProperties(null);
    }
    public override bool CreateGraph(int Quality)
    {
      try
      {

        _inScanningMode = false;
        //check if we didnt already create a graph
        if (_graphState != State.None)
          return false;
        _currentTuningObject = null;
        _isUsingAC3 = false;
        if (_streamDemuxer != null)
          _streamDemuxer.GrabTeletext(false);

        _isGraphRunning = false;
        Log.Info("DVBGraphSkyStar2:CreateGraph(). ");

        //no card defined? then we cannot build a graph
        if (_card == null)
        {
          Log.Error("DVBGraphSkyStar2:card is not defined");
          return false;
        }
        //create new instance of VMR9 helper utility
        _vmr9 = new VMR9Util();

        // Make a new filter graph
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2:create new filter graph (IGraphBuilder)");
        _graphBuilder = (IGraphBuilder)new FilterGraph();


        // Get the Capture Graph Builder
        _captureGraphBuilderInterface = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
        int hr = _captureGraphBuilderInterface.SetFiltergraph(_graphBuilder);
        if (hr < 0)
        {
          Log.Error("DVBGraphSkyStar2:FAILED link :0x{0:X}", hr);
          return false;
        }
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2:Add graph to ROT table");
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);



        //=========================================================================================================
        // add the skystar 2 specific filters
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2:CreateGraph() create B2C2 adapter");
        _filterB2C2Adapter = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_B2C2Adapter, false));
        if (_filterB2C2Adapter == null)
        {
          Log.Info("DVBGraphSkyStar2:creategraph() _filterB2C2Adapter not found");
          return false;
        }
        Log.Info("DVBGraphSkyStar2:creategraph() add filters to graph");
        hr = _graphBuilder.AddFilter(_filterB2C2Adapter, "B2C2-Source");
        if (hr != 0)
        {
          Log.Info("DVBGraphSkyStar2: FAILED to add B2C2-Adapter");
          return false;
        }
        // get interfaces
        _interfaceB2C2DataCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3;
        if (_interfaceB2C2DataCtrl == null)
        {
          Log.Info("DVBGraphSkyStar2: cannot get IB2C2MPEG2DataCtrl3");
          return false;
        }
        _interfaceB2C2TunerCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2;
        if (_interfaceB2C2TunerCtrl == null)
        {
          Log.Info("DVBGraphSkyStar2: cannot get IB2C2MPEG2TunerCtrl3");
          return false;
        }
        /*        _interfaceB2C2AvcCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2;
                if (_interfaceB2C2AvcCtrl == null)
                {
                  Log.Info("DVBGraphSkyStar2: cannot get IB2C2MPEG2AVCtrl2");
                  return false;
                }*/

        //=========================================================================================================
        // initialize skystar 2 tuner
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2: Initialize Tuner()");
        hr = _interfaceB2C2TunerCtrl.Initialize();
        if (hr != 0)
        {
          Log.Info("DVBGraphSkyStar2: Tuner initialize failed:0x{0:X}", hr);
          return false;
        }
        // call checklock once, the return value dont matter

        hr = _interfaceB2C2TunerCtrl.CheckLock();
        //=========================================================================================================
        // add the Sample grabber (not in configuration.exe) 
        //=========================================================================================================

        _filterSampleGrabber = null;
        _sampleInterface = null;
        //TESTTEST: DONT USE GRABBER AT ALL
        /*

                if (GUIGraphicsContext.DX9Device != null)
                {
                  Log.Info("DVBGraphSkyStar2: Add Sample Grabber");
                  _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
                  _sampleInterface = (ISampleGrabber)_filterSampleGrabber;
                  _graphBuilder.AddFilter(_filterSampleGrabber, "Sample Grabber");
                }
        */
        //=========================================================================================================
        // add the MPEG-2 Demultiplexer 
        //=========================================================================================================
        // Use CLSID_filterMpeg2Demultiplexer to create the filter
        Log.Info("DVBGraphSkyStar2: Add Sample MPEG2-Demultiplexer");
        _filterMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
        if (_filterMpeg2Demultiplexer == null)
        {
          Log.Error("DVBGraphSkyStar2:Failed to create Mpeg2 Demultiplexer");
          return false;
        }

        // Add the Demux to the graph
        _graphBuilder.AddFilter(_filterMpeg2Demultiplexer, "MPEG-2 Demultiplexer");
        IMpeg2Demultiplexer demuxer = _filterMpeg2Demultiplexer as IMpeg2Demultiplexer;

        //=========================================================================================================
        // create PSI output pin on demuxer
        //=========================================================================================================

        Log.Info("DVBGraphSkyStar2: Create PSI output pin on MPEG2-Demultiplexer");
        AMMediaType mtSections = new AMMediaType();
        mtSections.majorType = MEDIATYPE_MPEG2_SECTIONS;
        mtSections.subType = MediaSubType.None;
        mtSections.formatType = FormatType.None;
        IPin pinSectionsOut;
        hr = demuxer.CreateOutputPin(mtSections, "sections", out pinSectionsOut);
        if (hr != 0 || pinSectionsOut == null)
        {
          Log.Error("DVBGraphSS2:FAILED to create sections pin:0x{0:X}", hr);
          return false;
        }

        Log.Info("DVBGraphSkyStar2: create audio/video output pin");
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
        hr = demuxer.CreateOutputPin(mpegVideoOut/*vidOut*/, "video", out filterMpeg2DemuxerVideoPin);
        if (hr != 0)
        {
          Log.Info("DVBGraphSkyStar2:FAILED to create video output pin on demuxer");
          return false;
        }
        hr = demuxer.CreateOutputPin(mpegAudioOut, "audio", out filterMpeg2DemuxerAudioPin);
        if (hr != 0)
        {
          Log.Info("DVBGraphSkyStar2: FAILED to create audio output pin on demuxer");
          return false;
        }

        //=========================================================================================================
        // add the stream analyzer
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2: Add Stream Analyzer");
        _filterDvbAnalyzer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(ClassId.MPStreamAnalyzer, true));
        _analyzerInterface = (IStreamAnalyzer)_filterDvbAnalyzer;
        _epgGrabberInterface = _filterDvbAnalyzer as IEPGGrabber;
        _mhwGrabberInterface = _filterDvbAnalyzer as IMHWGrabber;
        _atscGrabberInterface = _filterDvbAnalyzer as IATSCGrabber;
        hr = _graphBuilder.AddFilter(_filterDvbAnalyzer, "Stream-Analyzer");
        if (hr != 0)
        {
          Log.Error("DVBGraphSkyStar2: FAILED to add SectionsFilter 0x{0:X}", hr);
          return false;
        }

        //=========================================================================================================
        // connect B2BC-Source "Data 0" -> samplegrabber
        //=========================================================================================================

        if (GUIGraphicsContext.DX9Device != null && _sampleInterface != null)
        {
          Log.Info("DVBGraphSkyStar2: connect B2C2->sample grabber");
          IPin pinData0 = DsFindPin.ByDirection(_filterB2C2Adapter, PinDirection.Output, 2);
          if (pinData0 == null)
          {
            Log.Error("DVBGraphSkyStar2:Failed to get pin 'Data 0' from B2BC source");
            return false;
          }

          IPin pinIn = DsFindPin.ByDirection(_filterSampleGrabber, PinDirection.Input, 0);
          if (pinIn == null)
          {
            Log.Error("DVBGraphSkyStar2:Failed to get input pin from sample grabber");
            return false;
          }

          hr = _graphBuilder.Connect(pinData0, pinIn);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:Failed to connect B2BC->sample grabber");
            return false;
          }
        }

        if (GUIGraphicsContext.DX9Device != null && _sampleInterface != null)
        {
          Log.Info("DVBGraphSkyStar2: connect sample grabber->MPEG2 demultiplexer");
          if (!ConnectFilters(ref _filterSampleGrabber, ref _filterMpeg2Demultiplexer))
          {
            Log.Error("DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
            return false;
          }
        }
        else
        {
          Log.Info("DVBGraphSkyStar2: connect B2C2->MPEG2 demultiplexer");
          IPin pinData0 = DsFindPin.ByDirection(_filterB2C2Adapter, PinDirection.Output, 2);
          if (pinData0 == null)
          {
            Log.Error("DVBGraphSkyStar2:Failed to get pin 'Data 0' from B2BC source");
            return false;
          }
          IPin pinIn = DsFindPin.ByDirection(_filterMpeg2Demultiplexer, PinDirection.Input, 0);
          if (pinIn == null)
          {
            Log.Error("DVBGraphSkyStar2:Failed to get input pin from sample grabber");
            return false;
          }

          hr = _graphBuilder.Connect(pinData0, pinIn);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:Failed to connect B2BC->demuxer");
            return false;
          }
        }

        //=========================================================================================================
        // 1. connect demuxer->analyzer
        // 2. find audio/video output pins on demuxer
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2:CreateGraph() find audio/video pins");
        bool connected = false;
        IPin pinAnalyzerIn = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 0);
        IEnumPins pinEnum;
        _filterMpeg2Demultiplexer.EnumPins(out pinEnum);
        pinEnum.Reset();
        IPin[] pin = new IPin[1];
        int fetched = 0;
        while (pinEnum.Next(1, pin, out fetched) == 0)
        {
          if (fetched == 1)
          {
            IEnumMediaTypes enumMedia;
            pin[0].EnumMediaTypes(out enumMedia);
            enumMedia.Reset();
            DirectShowLib.AMMediaType[] pinMediaType = new DirectShowLib.AMMediaType[2];
            int fetchedm = 0;
            while (enumMedia.Next(1, pinMediaType, out fetchedm) == 0)
            {
              if (fetchedm == 1)
              {
                if (pinMediaType[0].majorType == MediaType.Audio)
                {
                  Log.Info("DVBGraphSkyStar2: found audio pin");
                  _pinDemuxerAudio = pin[0];
                  break;
                }
                if (pinMediaType[0].majorType == MediaType.Video)
                {
                  Log.Info("DVBGraphSkyStar2: found video pin");
                  _pinDemuxerVideo = pin[0];
                  break;
                }
                if (pinMediaType[0].majorType == MEDIATYPE_MPEG2_SECTIONS && !connected)
                {
                  IPin pinConnectedTo = null;
                  pin[0].ConnectedTo(out pinConnectedTo);
                  if (pinConnectedTo == null)
                  {
                    _pinDemuxerSections = pin[0];
                    Log.Info("DVBGraphSkyStar2:connect mpeg2 demux->stream analyzer");
                    hr = _graphBuilder.Connect(pin[0], pinAnalyzerIn);
                    if (hr == 0)
                    {
                      connected = true;
                      Log.Info("DVBGraphSkyStar2:connected mpeg2 demux->stream analyzer");
                    }
                    else
                    {
                      Log.Error("DVBGraphSkyStar2:FAILED to connect mpeg2 demux->stream analyzer");
                    }
                    pin[0] = null;
                  }
                  if (pinConnectedTo != null)
                  {
                    DirectShowUtil.ReleaseComObject(pinConnectedTo);
                    pinConnectedTo = null;
                  }
                }
              }
            }
            if (enumMedia != null)
              DirectShowUtil.ReleaseComObject(enumMedia);
            enumMedia = null;
            if (pin[0] != null)
              DirectShowUtil.ReleaseComObject(pin[0]);
            pin[0] = null;
          }
        }
        DirectShowUtil.ReleaseComObject(pinEnum); pinEnum = null;
        if (pinAnalyzerIn != null) DirectShowUtil.ReleaseComObject(pinAnalyzerIn); pinAnalyzerIn = null;
        //get the video/audio output pins of the mpeg2 demultiplexer
        if (_pinDemuxerVideo == null)
        {
          //video pin not found
          Log.Error("DVBGraphSkyStar2:Failed to get pin (video out) from MPEG-2 Demultiplexer", _pinDemuxerVideo);
          return false;
        }
        if (_pinDemuxerAudio == null)
        {
          //audio pin not found
          Log.Error("DVBGraphSkyStar2:Failed to get pin (audio out)  from MPEG-2 Demultiplexer", _pinDemuxerAudio);
          return false;
        }

        //=========================================================================================================
        // add the AC3 pin, mpeg1 audio pin to the MPEG2 demultiplexer
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2:CreateGraph() create ac3/mpg1 pins");
        if (demuxer != null)
        {

          //Log.Info("mpeg2: create ac3 pin");
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

          hr = demuxer.CreateOutputPin(mediaAC3/*vidOut*/, "AC3", out _pinAC3Out);
          if (hr != 0 || _pinAC3Out == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to create AC3 pin:0x{0:X}", hr);
          }

          //Log.Info("DVBGraphSkyStar2: create mpg1 audio pin");
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

          hr = demuxer.CreateOutputPin(mediaMPG1/*vidOut*/, "audioMpg1", out _pinMPG1Out);
          if (hr != 0 || _pinMPG1Out == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to create MPG1 pin:0x{0:X}", hr);
          }

          //=========================================================================================================
          // add the EPG/MHW pin to the MPEG2 demultiplexer
          //=========================================================================================================
          //create EPG pins
          //Log.Info("DVBGraphSkyStar2:Create EPG pin");
          AMMediaType mtEPG = new AMMediaType();
          mtEPG.majorType = MEDIATYPE_MPEG2_SECTIONS;
          mtEPG.subType = MediaSubType.None;
          mtEPG.formatType = FormatType.None;

          //IPin pinEPGout, pinMHW1Out, pinMHW2Out;
          hr = demuxer.CreateOutputPin(mtEPG, "EPG", out _pinDemuxerEPG);
          if (hr != 0 || _pinDemuxerEPG == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to create EPG pin:0x{0:X}", hr);
            return false;
          }
          hr = demuxer.CreateOutputPin(mtEPG, "MHW1", out _pinDemuxerMHWd2);
          if (hr != 0 || _pinDemuxerMHWd2 == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to create MHW1 pin:0x{0:X}", hr);
            return false;
          }
          hr = demuxer.CreateOutputPin(mtEPG, "MHW2", out _pinDemuxerMHWd3);
          if (hr != 0 || _pinDemuxerMHWd3 == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to create MHW2 pin:0x{0:X}", hr);
            return false;
          }

          //Log.Info("DVBGraphSkyStar2:Get EPGs pin of analyzer");
          IPin pinMHW1In = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 1);
          if (pinMHW1In == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to get MHW1 pin on MSPA");
            return false;
          }
          IPin pinMHW2In = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 2);
          if (pinMHW2In == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to get MHW2 pin on MSPA");
            return false;
          }
          IPin pinEPGIn = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 3);
          if (pinEPGIn == null)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to get EPG pin on MSPA");
            return false;
          }

          //Log.Info("DVBGraphSkyStar2:Connect epg pins");
          hr = _graphBuilder.Connect(_pinDemuxerEPG, pinEPGIn);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to connect EPG pin:0x{0:X}", hr);
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerMHWd2, pinMHW1In);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to connect MHW1 pin:0x{0:X}", hr);
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerMHWd3, pinMHW2In);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:FAILED to connect MHW2 pin:0x{0:X}", hr);
            return false;
          }
          //Log.Info("DVBGraphSkyStar2:Demuxer is setup");

          if (pinMHW1In != null) DirectShowUtil.ReleaseComObject(pinMHW1In); pinMHW1In = null;
          if (pinMHW2In != null) DirectShowUtil.ReleaseComObject(pinMHW2In); pinMHW2In = null;
          if (pinEPGIn != null) DirectShowUtil.ReleaseComObject(pinEPGIn); pinEPGIn = null;

          //setup teletext grabbing....
          if (GUIGraphicsContext.DX9Device != null)
          {
            AMMediaType txtMediaType = new AMMediaType();
            txtMediaType.majorType = MediaType.Stream;
            txtMediaType.subType = MediaSubTypeEx.MPEG2Transport;
            hr = demuxer.CreateOutputPin(txtMediaType, "ttx", out _pinTeletext);
            if (hr != 0 || _pinTeletext == null)
            {
              Log.Error("DVBGraphBDA:FAILED to create ttx pin:0x{0:X}", hr);
              return false;
            }

            _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
            _sampleInterface = (ISampleGrabber)_filterSampleGrabber;
            _graphBuilder.AddFilter(_filterSampleGrabber, "Sample Grabber");

            IPin pinIn = DsFindPin.ByDirection(_filterSampleGrabber, PinDirection.Input, 0);
            if (pinIn == null)
            {
              Log.Error("DVBGraphBDA:unable to find sample grabber input:0x{0:X}", hr);
              return false;
            }
            hr = _graphBuilder.Connect(_pinTeletext, pinIn);
            if (hr != 0)
            {
              Log.Error("DVBGraphBDA:FAILED to connect demux->sample grabber:0x{0:X}", hr);
              return false;
            }
          }
        }
        else
          Log.Error("DVBGraphSkyStar2:mapped IMPEG2Demultiplexer not found");

        //=========================================================================================================
        // Create the streambuffer engine and mpeg2 video analyzer components since we need them for
        // recording and timeshifting
        //=========================================================================================================
        m_StreamBufferSink = new StreamBufferSink();
        m_mpeg2Analyzer = new VideoAnalyzer();
        m_IStreamBufferSink = (IStreamBufferSink3)m_StreamBufferSink;
        _graphState = State.Created;


        //_streamDemuxer.OnAudioFormatChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnAudioChanged(m_streamDemuxer_OnAudioFormatChanged);
        //_streamDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(m_streamDemuxer_OnPMTIsChanged);
        _streamDemuxer.SetCardType((int)DVBEPG.EPGCard.BDACards, Network());
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
          _analyzerInterface.UseATSC(1);
        else
          _analyzerInterface.UseATSC(0);

        _epgGrabber.EPGInterface = _epgGrabberInterface;
        _epgGrabber.MHWInterface = _mhwGrabberInterface;
        _epgGrabber.ATSCInterface = _atscGrabberInterface;
        _epgGrabber.AnalyzerInterface = _analyzerInterface;
        _epgGrabber.Network = Network();
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
          return;
        int hr;
        _currentTuningObject = null;
        Log.Info("DVBGraphSkyStar2:DeleteGraph(). ac3=false");
        _isUsingAC3 = false;

        Log.Info("DVBGraphSkyStar2:DeleteGraph()");
        StopRecording();
        StopTimeShifting();
        StopViewing();
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2: free tuner interfaces");

        // to clear buffers for epg and teletext
        if (_streamDemuxer != null)
        {
          _streamDemuxer.GrabTeletext(false);
          _streamDemuxer.SetChannelData(0, 0, 0, 0, 0, "", 0, 0);
        }

        //Log.Info("DVBGraphSkyStar2:stop graph");
        if (_mediaControl != null) _mediaControl.Stop();
        _mediaControl = null;
        //Log.Info("DVBGraphSkyStar2:graph stopped");

        if (_vmr9 != null)
        {
          //Log.Info("DVBGraphSkyStar2:remove vmr9");
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
          DirectShowUtil.ReleaseComObject(_pinDemuxerSections);
        _pinDemuxerSections = null;

        if (_pinAC3Out != null)
          DirectShowUtil.ReleaseComObject(_pinAC3Out);
        _pinAC3Out = null;

        if (_pinMPG1Out != null)
          DirectShowUtil.ReleaseComObject(_pinMPG1Out);
        _pinMPG1Out = null;

        if (_pinDemuxerVideo != null)
          DirectShowUtil.ReleaseComObject(_pinDemuxerVideo);
        _pinDemuxerVideo = null;

        if (_pinDemuxerAudio != null)
          DirectShowUtil.ReleaseComObject(_pinDemuxerAudio);
        _pinDemuxerAudio = null;


        if (_filterB2C2Adapter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_filterB2C2Adapter)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_filterB2C2Adapter):{0}", hr);
          _filterB2C2Adapter = null;
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
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_filterSmartTee):{0}", hr);
          _filterSmartTee = null;
        }

        if (_videoWindowInterface != null)
        {
          //Log.Info("DVBGraphSkyStar2:hide window");
          //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2: hide video window");
          _videoWindowInterface.put_Visible(OABool.False);
          //_videoWindowInterface.put_Owner(IntPtr.Zero);
          _videoWindowInterface = null;
        }

        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2: free other interfaces");
        _sampleInterface = null;
        if (_filterSampleGrabber != null)
        {
          //Log.Info("DVBGraphSkyStar2:free samplegrabber");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterSampleGrabber)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_filterSampleGrabber):{0}", hr);
          _filterSampleGrabber = null;
        }


        if (m_IStreamBufferConfig != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(m_IStreamBufferConfig)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(m_IStreamBufferConfig):{0}", hr);
          m_IStreamBufferConfig = null;
        }

        if (m_IStreamBufferSink != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(m_IStreamBufferSink)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(m_IStreamBufferSink):{0}", hr);
          m_IStreamBufferSink = null;
        }

        if (m_StreamBufferSink != null)
        {
          //Log.Info("DVBGraphSkyStar2:free streambuffersink");
          while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferSink)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(m_StreamBufferSink):{0}", hr);
          m_StreamBufferSink = null;
        }


        if (m_StreamBufferConfig != null)
        {
          //Log.Info("DVBGraphSkyStar2:free streambufferconfig");
          while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferConfig)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(m_StreamBufferConfig):{0}", hr);
          m_StreamBufferConfig = null;
        }

        if (_filterMpeg2Demultiplexer != null)
        {
          //Log.Info("DVBGraphSkyStar2:free demux");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterMpeg2Demultiplexer)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_filterMpeg2Demultiplexer):{0}", hr);
          _filterMpeg2Demultiplexer = null;
        }

        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2: remove filters");

        if (_graphBuilder != null)
          DirectShowUtil.RemoveFilters(_graphBuilder);


        //Log.Info("DVBGraphSkyStar2:free remove graph");
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2: remove graph");
        if (_captureGraphBuilderInterface != null)
        {
          //Log.Info("DVBGraphSkyStar2:free remove capturegraphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_captureGraphBuilderInterface)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_captureGraphBuilderInterface):{0}", hr);
          _captureGraphBuilderInterface = null;
        }

        if (_graphBuilder != null)
        {
          //Log.Info("DVBGraphSkyStar2:free graphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_graphBuilder):{0}", hr);
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
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2: delete graph done");
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }


    protected override void UpdateSignalPresent()
    {
      if (_graphState == State.None || _interfaceB2C2TunerCtrl == null)
      {
        _signalPresent = false;
        _signalQuality = 0;
        _signalLevel = 0;
        return;
      }
      int level, quality;
      _signalPresent = (_interfaceB2C2TunerCtrl.CheckLock() == 0);
      GetSNR(_interfaceB2C2TunerCtrl, out level, out quality);
      if (level < 0) level = 0;
      if (level > 100) level = 100;
      if (quality < 0) quality = 0;
      if (quality > 100) quality = 100;
      _signalQuality = quality;
      _signalLevel = level;
    }

    public override NetworkType Network()
    {
      return _networkType;
    }


    protected override void SubmitTuneRequest(DVBChannel ch)
    {
      //DVBS-LoBand example
      //Transponder       10832 MHz
      //Tuner frequency   1082  MHz
      //SymbolRate        22000 kS/s
      //Fec               5/6
      //Polarity          Horizontal/Left (high)
      //LNB frequency     9750 MHz
      //LNB selection     none
      //DisEQC            none
      int frequency = ch.Frequency;
      if (frequency > 13000)
        frequency /= 1000;
      Log.Info("DVBGraphSkyStar2:  Transponder Frequency:{0} MHz", frequency);
      int hr = _interfaceB2C2TunerCtrl.SetFrequency(frequency);
      if (hr != 0)
      {
        Log.Error("DVBGraphSkyStar2:SetFrequencyKHz() failed:0x{0:X}", hr);
        return;
      }

      switch (Network())
      {
        case NetworkType.ATSC:
          Log.Info("DVBGraphSkyStar2:  ATSC Channel:{0}", ch.PhysicalChannel);
          //#DM B2C2 SDK says ATSC is tuned by frequency. Here we work the OTA frequency by channel number#
          int atscfreq = 0;
          if (ch.PhysicalChannel <= 6) atscfreq = 45+(ch.PhysicalChannel*6);
          if (ch.PhysicalChannel >= 7 && ch.PhysicalChannel <= 13) atscfreq = 177 + ((ch.PhysicalChannel - 7) * 6);
          if (ch.PhysicalChannel >= 14) atscfreq = 473+((ch.PhysicalChannel - 14) * 6);
          //#DM changed tuning parameter from physical channel to calculated frequency above.
          //Log.Info("DVBGraphSkyStar2:  Channel:{0} KHz", ch.Frequency);
          //hr = _interfaceB2C2TunerCtrl.SetChannel(ch.PhysicalChannel);
          Log.Info("DVBGraphSkyStar2:  ATSC Frequency:{0} MHz", atscfreq);
          hr = _interfaceB2C2TunerCtrl.SetFrequency(atscfreq);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetFrequency() failed:0x{0:X}", hr);
            return;
          }
          break;
        
        case NetworkType.DVBC:
          {
            Log.Info("DVBGraphSkyStar2:  SymbolRate:{0} KS/s", ch.Symbolrate);
            hr = _interfaceB2C2TunerCtrl.SetSymbolRate(ch.Symbolrate);
            if (hr != 0)
            {
              Log.Error("DVBGraphSkyStar2:SetSymbolRate() failed:0x{0:X}", hr);
              return;
            }

            int modulation = (int)eModulationTAG.QAM_64;
            switch (ch.Modulation)
            {
              case (int)TunerLib.ModulationType.BDA_MOD_16QAM:
                modulation = (int)eModulationTAG.QAM_16;
                break;
              case (int)TunerLib.ModulationType.BDA_MOD_32QAM:
                modulation = (int)eModulationTAG.QAM_32;
                break;
              case (int)TunerLib.ModulationType.BDA_MOD_64QAM:
                modulation = (int)eModulationTAG.QAM_64;
                break;
              case (int)TunerLib.ModulationType.BDA_MOD_128QAM:
                modulation = (int)eModulationTAG.QAM_128;
                break;
              case (int)TunerLib.ModulationType.BDA_MOD_256QAM:
                modulation = (int)eModulationTAG.QAM_256;
                break;
            }
            Log.Info("DVBGraphSkyStar2:  Modulation:{0}", ((eModulationTAG)modulation));
            hr = _interfaceB2C2TunerCtrl.SetModulation(modulation);
            if (hr != 0)
            {
              Log.Error("DVBGraphSkyStar2:SetModulation() failed:0x{0:X}", hr);
              return;
            }
          }
          break;

        case NetworkType.DVBT:
          Log.Info("DVBGraphSkyStar2:  GuardInterval:auto");
          hr = _interfaceB2C2TunerCtrl.SetGuardInterval((int)GuardIntervalType.Interval_Auto);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetGuardInterval() failed:0x{0:X}", hr);
            return;
          }

          Log.Info("DVBGraphSkyStar2:  Bandwidth:{0} MHz", ch.Bandwidth);
          //hr = _interfaceB2C2TunerCtrl.SetBandwidth((int)ch.Bandwidth);
          // Set Channel Bandwidth (NOTE: Temporarily use polarity function to avoid having to 
          // change SDK interface for SetBandwidth)
          // from Technisat SDK 02/2005
          hr = _interfaceB2C2TunerCtrl.SetPolarity((int)ch.Bandwidth);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetBandwidth() failed:0x{0:X}", hr);
            return;
          }
          break;

        case NetworkType.DVBS:

          int lowOsc, hiOsc, disEqcUsed, lnbKhzTone;
          //#DM - I think the new driver allows for expanded DiSEqC support so lets try it :) #
          if (ch.DiSEqC < 1) ch.DiSEqC = 1;
          //if (ch.DiSEqC > 4) ch.DiSEqC = 4;
          GetDisEqcSettings(ref ch, out lowOsc, out hiOsc, out lnbKhzTone, out disEqcUsed);
          if (ch.LNBFrequency >= frequency)
          {
            Log.Error("DVBGraphSkyStar2:  Error: LNB Frequency must be less than Transponder frequency");
          }
          Log.Info("DVBGraphSkyStar2:  SymbolRate:{0} KS/s", ch.Symbolrate);
          hr = _interfaceB2C2TunerCtrl.SetSymbolRate(ch.Symbolrate);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetSymbolRate() failed:0x{0:X}", hr);
            return;
          }

          // #DM - whats the line below all about ??? #
          //ch.LnbSwitchFrequency /= 1000;//in MHz
          Log.Info("DVBGraphSkyStar2:  LNBFrequency:{0} MHz", ch.LNBFrequency);
          hr = _interfaceB2C2TunerCtrl.SetLnbFrequency(ch.LNBFrequency);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetLnbFrequency() failed:0x{0:X}", hr);
            return;
          }

          int fec = (int)FecType.Fec_Auto;
          Log.Info("DVBGraphSkyStar2:  Fec:{0} {1}", ((FecType)fec), fec);
          hr = _interfaceB2C2TunerCtrl.SetFec(fec);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetFec() failed:0x{0:X}", hr);
            return;
          }

          //0=horizontal,1=vertical
          Log.Info("DVBGraphSkyStar2:  Polarity:{0} {1}", ((PolarityType)ch.Polarity), ch.Polarity);
          hr = _interfaceB2C2TunerCtrl.SetPolarity(ch.Polarity);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetPolarity() failed:0x{0:X}", hr);
            return;
          }

          LNBSelectionType lnbSelection = LNBSelectionType.Lnb0;
          switch (lnbKhzTone)
          {
            case 0:
              lnbSelection = LNBSelectionType.Lnb0;
              break;
            case 22:
              lnbSelection = LNBSelectionType.Lnb22kHz;
              break;
            case 33:
              lnbSelection = LNBSelectionType.Lnb33kHz;
              break;
            case 44:
              lnbSelection = LNBSelectionType.Lnb44kHz;
              break;
          }
          if (ch.Frequency < ch.LnbSwitchFrequency)
          {
            lnbSelection = LNBSelectionType.Lnb0;
          }
          Log.Info("DVBGraphSkyStar2:  Lnb: {0} Khz", lnbKhzTone);
          hr = _interfaceB2C2TunerCtrl.SetLnbKHz((int)lnbSelection);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetLnbKHz() failed:0x{0:X}", hr);
            return;
          }

          DisEqcType disType = DisEqcType.None;
          switch (disEqcUsed)
          {
            case 0: // none
              disType = DisEqcType.None;
              break;
            case 1: // Simple A
              disType = DisEqcType.Simple_A;
              break;
            case 2: // Simple B
              disType = DisEqcType.Simple_B;
              break;
            case 3: // Level 1 A/A
              disType = DisEqcType.Level_1_A_A;
              break;
            case 4: // Level 1 B/A
              disType = DisEqcType.Level_1_B_A;
              break;
            case 5: // Level 1 A/B
              disType = DisEqcType.Level_1_A_B;
              break;
            case 6: // Level 1 B/B
              disType = DisEqcType.Level_1_B_B;
              break;
          }
          Log.Info("DVBGraphSkyStar2:  Diseqc:{0} {1}", disType, (int)disType);
          hr = _interfaceB2C2TunerCtrl.SetDiseqc((int)disType);
          if (hr != 0)
          {
            Log.Error("DVBGraphSkyStar2:SetDiseqc() failed:0x{0:X}", hr);
            return;
          }

          break;
      }

      //hr=_interfaceB2C2TunerCtrl.SetTunerStatusEx(5);//20*50ms= max 2 sec to lock tuner
      hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
      _interfaceB2C2TunerCtrl.CheckLock();
      if (((uint)hr) == (uint)0x90010115)
      {
        Log.Error("DVBGraphSkyStar2:could not lock tuner");
        //dump all values:
        int ss2freq, ss2symb, ss2lnbfreq, ss2fec, ss2pol, ss2lnbkhz, ss2diseqc;
        Log.Info("DVBGraphSkyStar2 tuner dump:");

        _interfaceB2C2TunerCtrl.GetFrequency(out ss2freq);
        Log.Info("DVBGraphSkyStar2    freq:{0} MHz", ss2freq);

        _interfaceB2C2TunerCtrl.GetSymbolRate(out ss2symb);
        Log.Info("DVBGraphSkyStar2    symbol rate:{0} KS/s", ss2symb);

        _interfaceB2C2TunerCtrl.GetLnbFrequency(out ss2lnbfreq);
        Log.Info("DVBGraphSkyStar2    LNB freq:{0} MHz", ss2lnbfreq);

        _interfaceB2C2TunerCtrl.GetFec(out ss2fec);
        Log.Info("DVBGraphSkyStar2    fec:{0}", (FecType)ss2fec);
        //Log.Info("DVBGraphSkyStar2    fec:{0}", ss2fec);

        _interfaceB2C2TunerCtrl.GetPolarity(out ss2pol);
        Log.Info("DVBGraphSkyStar2    polarity:{0}", (PolarityType)ss2pol);

        _interfaceB2C2TunerCtrl.GetLnbKHz(out ss2lnbkhz);
        Log.Info("DVBGraphSkyStar2    LNB {0} kHz: ", ss2lnbkhz);

        _interfaceB2C2TunerCtrl.GetDiseqc(out ss2diseqc);
        Log.Info("DVBGraphSkyStar2    diseqc:{0}", (DisEqcType)ss2diseqc);
        //Log.Info("DVBGraphSkyStar2    diseqc:{0}", ss2diseqc);
      }
      else
      {
        if (hr != 0)
          hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        if (hr != 0)
        {
          Log.Error("DVBGraphSkyStar2:SetTunerStatus failed:0x{0:X}", hr);
          return;
        }
      }
      _interfaceB2C2TunerCtrl.CheckLock();
      UpdateSignalPresent();
      if (!_inScanningMode)
        SetHardwarePidFiltering();
      _processTimer = DateTime.MinValue;
      _pmtSendCounter = 0;
      Log.Info("DVBGraphSkyStar2: signal strength:{0} signal quality:{1} signal present:{2}", SignalStrength(), SignalQuality(), SignalPresent());

    }


    protected override void SendHWPids(ArrayList pids)
    {
      const int PID_CAPTURE_ALL_INCLUDING_NULLS = 0x2000;//Enables reception of all PIDs in the transport stream including the NULL PID
      // const int PID_CAPTURE_ALL_EXCLUDING_NULLS = 0x2001;//Enables reception of all PIDs in the transport stream excluding the NULL PID.

      if (!DeleteAllPIDs(_interfaceB2C2DataCtrl, 0))
      {
        Log.Error("DVBGraphSkyStar2:DeleteAllPIDs() failed pid:0x2000");
      }
      if (pids.Count == 0)
      {
        int added = SetPidToPin(_interfaceB2C2DataCtrl, 0, PID_CAPTURE_ALL_INCLUDING_NULLS);
        if (added != 1)
        {
          Log.Error("DVBGraphSkyStar2:SetPidToPin() failed pid:0x2000");
        }
      }
      else
      {
        int maxPids;
        _interfaceB2C2DataCtrl.GetMaxPIDCount(out maxPids);
        for (int i = 0; i < pids.Count && i < maxPids; ++i)
        {
          ushort pid = (ushort)pids[i];
          SetPidToPin(_interfaceB2C2DataCtrl, 0, pid);
        }
      }
    }

    DVBChannel LoadDiseqcSettings(DVBChannel ch, int disNo)
    {

      int lnbKhz = 0;
      int lnbKhzVal = 0;
      int diseqc = 0;
      int lnbKind = 0;
      // lnb config
      int lnb0MHZ = 0;
      int lnb1MHZ = 0;
      int lnbswMHZ = 0;
      int cbandMHZ = 0;
      int circularMHZ = 0;

      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        lnb0MHZ = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750);
        lnb1MHZ = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600);
        lnbswMHZ = xmlreader.GetValueAsInt("dvbs", "Switch", 11700);
        cbandMHZ = xmlreader.GetValueAsInt("dvbs", "CBand", 5150);
        circularMHZ = xmlreader.GetValueAsInt("dvbs", "Circular", 10750);
        //				bool useLNB1=xmlreader.GetValueAsBool("dvbs","useLNB1",false);
        //				bool useLNB2=xmlreader.GetValueAsBool("dvbs","useLNB2",false);
        //				bool useLNB3=xmlreader.GetValueAsBool("dvbs","useLNB3",false);
        //				bool useLNB4=xmlreader.GetValueAsBool("dvbs","useLNB4",false);
        switch (disNo)
        {
          case 1:
            // config a
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind", 0);
            break;
          case 2:
            // config b
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
            break;
          case 3:
            // config c
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
            break;
          //
          case 4:
            // config d
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
            //
            break;
        }// switch(disNo)
        switch (lnbKhz)
        {
          case 0: lnbKhzVal = (int)LNBSelectionType.Lnb0; break;
          case 22: lnbKhzVal = (int)LNBSelectionType.Lnb22kHz; break;
          case 33: lnbKhzVal = (int)LNBSelectionType.Lnb33kHz; break;
          case 44: lnbKhzVal = (int)LNBSelectionType.Lnb44kHz; break;
        }


      }//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(_cardFilename))

      // set values to dvbchannel-object
      ch.DiSEqC = diseqc;
      // set the lnb parameter
      if (ch.Frequency >= lnbswMHZ * 1000)
      {
        ch.LNBFrequency = lnb1MHZ;
        ch.LnbSwitchFrequency = lnbKhzVal;
      }
      else
      {
        ch.LNBFrequency = lnb0MHZ;
        ch.LnbSwitchFrequency = 0;
      }
      //Log.Info("auto-tune ss2: freq={0} lnbKHz={1} lnbFreq={2} diseqc={3}", ch.Frequency, ch.LnbSwitchFrequency, ch.LNBFrequency, ch.DiSEqC);
      Log.Info("auto-tune ss2: freq={0} lnbKHz={1} lnbFreq={2} diseqc={3}", ch.Frequency, lnbKhz, ch.LNBFrequency, ch.DiSEqC);
      return ch;

    }// LoadDiseqcSettings()

    protected override void SetupDiseqc(int disNo)
    {
      if (_currentTuningObject == null) return;
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));

      int lnbKhz = 0;
      int lnbKhzVal = 0;
      int diseqc = 0;
      int lnbKind = 0;
      // lnb config
      int lnb0MHZ = 0;
      int lnb1MHZ = 0;
      int lnbswMHZ = 0;
      int cbandMHZ = 0;
      int circularMHZ = 0;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        lnb0MHZ = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750);
        lnb1MHZ = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600);
        lnbswMHZ = xmlreader.GetValueAsInt("dvbs", "Switch", 11700);
        cbandMHZ = xmlreader.GetValueAsInt("dvbs", "CBand", 5150);
        circularMHZ = xmlreader.GetValueAsInt("dvbs", "Circular", 10750);
        //				bool useLNB1=xmlreader.GetValueAsBool("dvbs","useLNB1",false);
        //				bool useLNB2=xmlreader.GetValueAsBool("dvbs","useLNB2",false);
        //				bool useLNB3=xmlreader.GetValueAsBool("dvbs","useLNB3",false);
        //				bool useLNB4=xmlreader.GetValueAsBool("dvbs","useLNB4",false);
        switch (disNo)
        {
          case 1:
            // config a
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind", 0);
            break;
          case 2:
            // config b
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
            break;
          case 3:
            // config c
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
            break;
          //
          case 4:
            // config d
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 22);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 1);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
            //
            break;
        }// switch(disNo)
        switch (lnbKhz)
        {
          case 0: lnbKhzVal = (int)LNBSelectionType.Lnb0; break;
          case 22: lnbKhzVal = (int)LNBSelectionType.Lnb22kHz; break;
          case 33: lnbKhzVal = (int)LNBSelectionType.Lnb33kHz; break;
          case 44: lnbKhzVal = (int)LNBSelectionType.Lnb44kHz; break;
        }


      }//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(_cardFilename))

      // set values to dvbchannel-object
      _currentTuningObject.DiSEqC = disNo;
      // set the lnb parameter
      if (_currentTuningObject.Frequency >= lnbswMHZ * 1000)
      {
        _currentTuningObject.LNBFrequency = lnb1MHZ;
        _currentTuningObject.LnbSwitchFrequency = lnbKhzVal;
      }
      else
      {
        _currentTuningObject.LNBFrequency = lnb0MHZ;
        _currentTuningObject.LnbSwitchFrequency = 0;
      }
      Log.Info("auto-tune ss2: freq={0} lnbKHz={1} lnbFreq={2} diseqc={3}", _currentTuningObject.Frequency, _currentTuningObject.LnbSwitchFrequency, _currentTuningObject.LNBFrequency, _currentTuningObject.DiSEqC);
    }// LoadDiseqcSettings()

    public override bool SupportsHardwarePidFiltering()
    {
      return true;
    }

    public override bool Supports5vAntennae()
    {
        return false;
    }

    private void GetTunerCapabilities()
    {
      try
      {
        // Make a new filter graph
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2:create new filter graph (IGraphBuilder)");
        _graphBuilder = (IGraphBuilder)new FilterGraph();

        // Get the Capture Graph Builder
        _captureGraphBuilderInterface = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
        int hr = _captureGraphBuilderInterface.SetFiltergraph(_graphBuilder);
        if (hr < 0)
        {
          Log.Error("DVBGraphSkyStar2:FAILED link :0x{0:X}", hr);
          return;
        }
        //Log.WriteFile(LogType.Log,"DVBGraphSkyStar2:Add graph to ROT table");
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

        //=========================================================================================================
        // add the skystar 2 specific filters
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2:GetTunerCapabilities() create B2C2 adapter");
        _filterB2C2Adapter = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_B2C2Adapter, false));
        if (_filterB2C2Adapter == null)
        {
          Log.Info("DVBGraphSkyStar2:GetTunerCapabilities() _filterB2C2Adapter not found");
          return;
        }
        _interfaceB2C2TunerCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2;
        if (_interfaceB2C2TunerCtrl == null)
        {
          Log.Info("DVBGraphSkyStar2: cannot get IB2C2MPEG2TunerCtrl3");
          return;
        }

        //=========================================================================================================
        // initialize skystar 2 tuner
        //=========================================================================================================
        Log.Info("DVBGraphSkyStar2: Initialize Tuner()");
        hr = _interfaceB2C2TunerCtrl.Initialize();
        if (hr != 0)
        {
          Log.Info("DVBGraphSkyStar2: Tuner initialize failed:0x{0:X}", hr);
          return;
        }
        // Get tuner type (DVBS, DVBC, DVBT, ATSC)

        tTunerCapabilities tc;
        int lTunerCapSize = Marshal.SizeOf(typeof(tTunerCapabilities));

        IntPtr ptCaps = Marshal.AllocHGlobal(lTunerCapSize);

        hr = _interfaceB2C2TunerCtrl.GetTunerCapabilities(ptCaps, ref lTunerCapSize);
        if (hr != 0)
        {
          Log.Info("DVBGraphSkyStar2: Tuner Type failed:0x{0:X}", hr);
          return;
        }

        tc = (tTunerCapabilities)Marshal.PtrToStructure(ptCaps, typeof(tTunerCapabilities));

        switch (tc.eModulation)
        {
          case TunerType.ttSat:
            Log.Info("DVBGraphSkyStar2: Network type=DVBS");
            _networkType = NetworkType.DVBS;
            break;
          case TunerType.ttCable:
            Log.Info("DVBGraphSkyStar2: Network type=DVBC");
            _networkType = NetworkType.DVBC;
            break;
          case TunerType.ttTerrestrial:
            Log.Info("DVBGraphSkyStar2: Network type=DVBT");
            _networkType = NetworkType.DVBT;
            break;
          case TunerType.ttATSC:
            Log.Info("DVBGraphSkyStar2: Network type=ATSC");
            _networkType = NetworkType.ATSC;
            break;
          case TunerType.ttUnknown:
            Log.Info("DVBGraphSkyStar2: Network type=unknown?");
            _networkType = NetworkType.Unknown;
            break;
        }
        Marshal.FreeHGlobal(ptCaps);

        if (_filterB2C2Adapter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_filterB2C2Adapter)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_filterB2C2Adapter):{0}", hr);
          _filterB2C2Adapter = null;
        }

        if (_graphBuilder != null)
          DirectShowUtil.RemoveFilters(_graphBuilder);

        //Log.Info("DVBGraphSkyStar2:free remove graph");
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        //Log.WriteFile(LogType.Capture,"DVBGraphSkyStar2: remove graph");
        if (_captureGraphBuilderInterface != null)
        {
          //Log.Info("DVBGraphSkyStar2:free remove capturegraphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_captureGraphBuilderInterface)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_captureGraphBuilderInterface):{0}", hr);
          _captureGraphBuilderInterface = null;
        }
        if (_graphBuilder != null)
        {
          //Log.Info("DVBGraphSkyStar2:free graphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphSkyStar2:ReleaseComObject(_graphBuilder):{0}", hr);
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
