/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
//using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;
using MediaPortal.TV.Teletext;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Implementation of IGraph for cards with an onboard MPEG 2 encoder
  /// like the Hauppauge PVR 250/350/USB2/MCE
  /// A graphbuilder object supports one or more TVCapture cards and
  /// contains all the code/logic necessary todo
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// </summary>
  public class SinkGraph : IGraph
  {
    protected enum State
    {
      None,
      Created,
      TimeShifting,
      Recording,
      Viewing,
      Radio
    };
    protected int _cardId = -1;
    protected string _videoCaptureFilterName = "";
    protected TVCaptureDevice _card;
    protected string _videoCaptureFilterMoniker = "";
    protected IGraphBuilder _graphBuilderInterface = null;
    protected ICaptureGraphBuilder2 _captureGraphBuilderInterface = null;
    protected IBaseFilter _filterCapture = null;
    protected IAMTVTuner _tvTunerInterface = null;
    protected IAMAnalogVideoDecoder _analogVideoDecoderInterface = null;
    protected VideoCaptureDevice _videoCaptureHelper = null;
    protected MPEG2Demux _mpeg2DemuxHelper = null;
    protected DsROTEntry _rotEntry = null;			// Cookie into the Running Object Table
    protected State _graphState = State.None;
    protected int _channelNumber = -1;
    protected int _countryCode = 31;
    protected bool _isUsingCable = false;
    protected DateTime _startTime = DateTime.Now;
    protected int _previousChannel = -1;
    protected Size _sizeFrame;
    protected double _frameRate;
    protected VideoProcAmp _videoProcAmpHelper = null;
    protected VMR9Util _vmr9 = null;
    DateTime _signalLostTimer;
    protected string _cardName;
    ArrayList _listAudioPids = new ArrayList();
    int _selectedAudioLanguage = 11;
    protected bool _grabTeletext = false;
    protected bool _hasTeletext = false;
    bool _isTuning = false;
    protected string _lastError = String.Empty;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="countryCode">country code</param>
    /// <param name="cable">use Cable or antenna</param>
    /// <param name="videoCaptureFilter">Filter name of the capture device</param>
    public SinkGraph(int ID, int countryCode, bool cable, string videoCaptureFilter, Size frameSize, double frameRate, string friendlyName)
    {
      _cardName = friendlyName;
      _cardId = ID;
      _isUsingCable = cable;
      _countryCode = countryCode;
      _graphState = State.None;
      _videoCaptureFilterName = videoCaptureFilter;
      _sizeFrame = frameSize;
      _frameRate = frameRate;

      if (_sizeFrame.Width == 0 || _sizeFrame.Height == 0)
        _sizeFrame = new Size(720, 576);


      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");

      }
      catch (Exception) { }
    }

    /// <summary>
    /// #MW# Added simple call while passing card object
    /// Easier to handle and to extent...
    /// </summary>
    /// <param name="pCard"></param>
    public SinkGraph(TVCaptureDevice pCard)
    {
      _card = pCard;

      // Add legacy code to be compliant to other call, ie fill in membervariables...
      _cardName = pCard.FriendlyName;
      _graphState = State.None;
      _cardId = _card.ID;
      _isUsingCable = _card.IsCableInput;
      _countryCode = _card.DefaultCountryCode;
      _videoCaptureFilterName = _card.VideoDevice;
      _videoCaptureFilterMoniker = _card.VideoDeviceMoniker;
      _sizeFrame = new Size(720, 576);

      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");
      }
      catch (Exception) { }
    }
    /// <summary>
    /// #MW#, Added moniker name... ie the REAL device!!!
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="countryCode"></param>
    /// <param name="cable"></param>
    /// <param name="videoCaptureFilter"></param>
    /// <param name="strVideoCaptureMoniker"></param>
    /// <param name="frameSize"></param>
    /// <param name="frameRate"></param>
    public SinkGraph(int ID, int countryCode, bool cable, string videoCaptureFilter, string strVideoCaptureMoniker, Size frameSize, double frameRate)
    {
      _cardId = ID;
      _isUsingCable = cable;
      _countryCode = countryCode;
      _graphState = State.None;
      _videoCaptureFilterName = videoCaptureFilter;
      // #MW#
      _videoCaptureFilterMoniker = strVideoCaptureMoniker;
      _sizeFrame = frameSize;
      _frameRate = frameRate;

      if (_sizeFrame.Width == 0 || _sizeFrame.Height == 0)
        _sizeFrame = new Size(720, 576);


      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");

      }
      catch (Exception) { }
    }
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public virtual bool CreateGraph(int Quality)
    {
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public virtual void DeleteGraph()
    {
    }

    /// <summary>
    /// Starts timeshifting the TV channel and stores the timeshifting 
    /// files in the specified filename
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <param name="strFileName">Filename for the timeshifting buffers</param>
    /// <returns>boolean indicating if timeshifting is running or not</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartTimeShifting(TVChannel channel, string strFileName)
    {
      if (_graphState != State.Created && _graphState != State.TimeShifting) return false;

      ulong freeSpace = Utils.GetFreeDiskSpace(strFileName);
      if (freeSpace < (1024L * 1024L * 1024L))// 1 GB
      {
        _lastError = GUILocalizeStrings.Get(765);// "Not enough free diskspace";
        Log.WriteFile(Log.LogType.Recorder, true, "Recorder:  failed to start timeshifting since drive {0}: has less then 1GB freediskspace", strFileName[0]);
        return false;
      }
      if (_mpeg2DemuxHelper == null) return false;
      _countryCode = channel.Country;

      if (_graphState == State.TimeShifting)
      {
        if (channel != null)
        {
          if (channel.Number != _channelNumber)
          {
            TuneChannel(channel);
          }
          return true;
        }
      }

      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }

      Log.WriteFile(Log.LogType.Log, "SinkGraph:StartTimeShifting()");
      _graphState = State.TimeShifting;
      SetFrameRateAndSize();
      TuneChannel(channel);
      _mpeg2DemuxHelper.StartTimeshifting(strFileName);

      //use default quality
      SetQuality(4);
      return true;
    }

    /// <summary>
    /// Connects the videocapture->MPEG2 Demuxer
    /// </summary>
    protected bool ConnectVideoCaptureToMPEG2Demuxer()
    {
      //			Log.WriteFile(Log.LogType.Log,"SinkGraph:Connect VideoCapture device to MPEG2Demuxer filter");
      if (_filterCapture == null || _graphBuilderInterface == null)
      {
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:ConnectVideoCaptureToMPEG2Demuxer() FAILED capture filter=null");
        return false;
      }
      if (_videoCaptureHelper == null)
      {
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:ConnectVideoCaptureToMPEG2Demuxer() FAILED videocapturedevice filter=null");
        return false;
      }
      if (_mpeg2DemuxHelper == null)
      {
        return false;
      }

      if (_mpeg2DemuxHelper.IsRendered)
      {
        return true;
      }

      //ask graph to render [capture] -> [mpeg2 demuxer]
      //it will use the encoder (if needed)
      Guid cat = PinCategory.Capture;
      int hr = _captureGraphBuilderInterface.RenderStream(cat, null/*new Guid[1]{ med}*/, _filterCapture, null, _mpeg2DemuxHelper.BaseFilter);
      if (hr == 0)
      {
        return true;
      }

      //ok, that failed. seems we have to do it ourselves
      //				Log.WriteFile(Log.LogType.Log,"SinkGraph:find MPEG2 demuxer input pin");
      IPin pinIn = DsFindPin.ByDirection(_mpeg2DemuxHelper.BaseFilter, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:FAILED could not find mpeg2 demux input pin");
        return false;
      }

      //					Log.WriteFile(Log.LogType.Log,"SinkGraph:found MPEG2 demuxer input pin");
      hr = _graphBuilderInterface.Connect(_videoCaptureHelper.CapturePin, pinIn);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:FAILED to connect Encoder->mpeg2 demuxer:{0:x}", hr);
        Marshal.ReleaseComObject(pinIn);
        return false;
      }

      Marshal.ReleaseComObject(pinIn);
      pinIn = null;
      return true;
    }

    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    public bool StopTimeShifting()
    {
      if (_graphState != State.TimeShifting) return false;

      Log.WriteFile(Log.LogType.Log, "SinkGraph:StopTimeShifting()");
      if (_mpeg2DemuxHelper != null)
        _mpeg2DemuxHelper.StopTimeShifting();
      _graphState = State.Created;

      return true;
    }

    /// <summary>
    /// Starts recording live TV to a file
    /// <param name="strFileName">filename for the new recording</param>
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// <param name="timeProgStart">Contains the starttime of the current tv program</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the timeProgStart till the moment recording is stopped again
    /// </remarks>
    public bool StartRecording(Hashtable attribtutes, TVRecording recording, TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (_graphState != State.TimeShifting) return false;
      if (_mpeg2DemuxHelper == null) return false;
      _countryCode = channel.Country;
      if (channel.Number != _channelNumber)
      {
        TuneChannel(channel);
      }

      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }


      Log.WriteFile(Log.LogType.Log, "SinkGraph:StartRecording({0} {1} {2})", strFileName, bContentRecording, recording.Quality);
      if (recording.Quality != TVRecording.QualityType.NotSet)
      {

        if (recording.Quality == TVRecording.QualityType.Portable)
          SetQuality(0);

        if (recording.Quality == TVRecording.QualityType.Low)
          SetQuality(1);

        if (recording.Quality == TVRecording.QualityType.Medium)
          SetQuality(2);

        if (recording.Quality == TVRecording.QualityType.High)
          SetQuality(3);
      }
      else
      {
        //use default quality
        SetQuality(4);
      }
      SetFrameRateAndSize();
      _mpeg2DemuxHelper.Record(attribtutes, strFileName, bContentRecording, timeProgStart, _startTime);
      _graphState = State.Recording;
      return true;
    }

    /// <summary>
    /// Stops recording 
    /// </summary>
    /// <remarks>
    /// Graph should be recording. When Recording is stopped the graph is still 
    /// timeshifting
    /// </remarks>
    public void StopRecording()
    {
      if (_graphState != State.Recording) return;

      Log.WriteFile(Log.LogType.Log, "SinkGraph:StopRecording()");
      if (_mpeg2DemuxHelper != null) _mpeg2DemuxHelper.StopRecording();
      _graphState = State.TimeShifting;
    }


    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
    public int GetChannelNumber()
    {
      return _channelNumber;
    }

    /// <summary>
    /// This method can be used to ask the graph if it should be rebuild when
    /// we want to tune to the new channel:ichannel
    /// </summary>
    /// <param name="iChannel">new channel to tune to</param>
    /// <returns>true : graph needs to be rebuild for this channel
    ///          false: graph does not need to be rebuild for this channel
    /// </returns>
    public bool ShouldRebuildGraph(TVChannel newChannel)
    {
      int iChannel = newChannel.Number;
      // if we switch from tuner <-> SVHS/Composite then 
      // we need to rebuild the capture graph
      bool bFixCrossbar = true;
      if (_previousChannel >= 0)
      {
        if (_previousChannel < (int)ExternalInputs.svhs && iChannel < (int)ExternalInputs.svhs) bFixCrossbar = false;
        if (_previousChannel == (int)ExternalInputs.rgb && iChannel == (int)ExternalInputs.rgb) bFixCrossbar = false;
        if (_previousChannel == (int)ExternalInputs.svhs && iChannel == (int)ExternalInputs.svhs) bFixCrossbar = false;
        if (_previousChannel == (int)ExternalInputs.cvbs1 && iChannel == (int)ExternalInputs.cvbs1) bFixCrossbar = false;
        if (_previousChannel == (int)ExternalInputs.cvbs2 && iChannel == (int)ExternalInputs.cvbs2) bFixCrossbar = false;
      }
      else bFixCrossbar = false;
      return bFixCrossbar;
    }

    /// <summary>
    /// Switches / tunes to another TV channel
    /// </summary>
    /// <param name="iChannel">New channel</param>
    /// <remarks>
    /// Graph should be timeshifting. 
    /// </remarks>
    public void TuneChannel(TVChannel channel)
    {
      if (_graphState != State.TimeShifting && _graphState != State.Viewing) return;
      //bool restartGraph = false;
      try
      {
        _isTuning = true;
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;

        /*
        if (_graphState == State.TimeShifting)
        {
          string fname = Recorder.GetTimeShiftFileNameByCardId(_cardId);
          if (g_Player.Playing && g_Player.CurrentFile == fname)
          {
            restartGraph = true;
            g_Player.PauseGraph();
            _mpeg2DemuxHelper.StopTimeShifting();
          }
        }*/

        _channelNumber = channel.Number;
        _countryCode = channel.Country;

        AnalogVideoStandard standard = channel.TVStandard;
        Log.WriteFile(Log.LogType.Log, "SinkGraph:TuneChannel() tune to channel:{0} country:{1} standard:{2} name:{3}",
          _channelNumber, _countryCode, standard, channel.Name);

        if (_channelNumber < (int)ExternalInputs.svhs)
        {
          if (_tvTunerInterface == null) return;
          try
          {
            InitializeTuner();
            SetVideoStandard(standard);


            Log.WriteFile(Log.LogType.Log, "SinkGraph:TuneChannel() tuningspace:0 country:{0} tv standard:{1} cable:{2}",
              _countryCode, standard.ToString(),
              _isUsingCable);
            AMTunerSubChannel iVideoSubChannel, iAudioSubChannel;
            int currentCountry, iCurrentChannel, currentFreq;

            _tvTunerInterface.get_VideoFrequency(out currentFreq);
            _tvTunerInterface.get_TVFormat(out standard);
            _tvTunerInterface.get_Channel(out iCurrentChannel, out iVideoSubChannel, out iAudioSubChannel);
            _tvTunerInterface.get_CountryCode(out currentCountry);
            if (iCurrentChannel != _channelNumber)
            {
              _tvTunerInterface.put_Channel(channel.Number, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
              DirectShowUtil.EnableDeInterlace(_graphBuilderInterface);
            }
            int iFreq, iChannel;
            double dFreq;
            AMTunerSignalStrength signalStrength;
            _tvTunerInterface.SignalPresent(out signalStrength);
            _tvTunerInterface.get_VideoFrequency(out iFreq);
            _tvTunerInterface.get_Channel(out iChannel, out iVideoSubChannel, out iAudioSubChannel);
            _tvTunerInterface.get_CountryCode(out currentCountry);
            _tvTunerInterface.get_TVFormat(out standard);
            if ((iChannel != iCurrentChannel) && (iFreq == currentFreq))
            {
              return;
            }
            dFreq = iFreq / 1000000d;
            Log.WriteFile(Log.LogType.Log, "SinkGraph:TuneChannel() tuned to channel:{0} county:{1} freq:{2} MHz. tvformat:{3} signal:{4}",
              iChannel, currentCountry, dFreq, standard.ToString(), signalStrength.ToString());


          }
          catch (Exception) { }
        }
        else
        {
          SetVideoStandard(channel.TVStandard);
        }

        bool bFixCrossbar = true;
        if (_previousChannel >= 0)
        {
          if (_previousChannel < (int)ExternalInputs.svhs && channel.Number < (int)ExternalInputs.svhs) bFixCrossbar = false;
          if (_previousChannel == (int)ExternalInputs.svhs && channel.Number == (int)ExternalInputs.svhs) bFixCrossbar = false;
          if (_previousChannel == (int)ExternalInputs.rgb && channel.Number == (int)ExternalInputs.rgb) bFixCrossbar = false;
          if (_previousChannel == (int)ExternalInputs.cvbs1 && channel.Number == (int)ExternalInputs.cvbs1) bFixCrossbar = false;
          if (_previousChannel == (int)ExternalInputs.cvbs2 && channel.Number == (int)ExternalInputs.cvbs2) bFixCrossbar = false;
        }
        if (bFixCrossbar)
        {
          CrossBar.RouteEx(_graphBuilderInterface,
            _captureGraphBuilderInterface,
            _filterCapture,
            channel.Number < (int)ExternalInputs.svhs,
            (channel.Number == (int)ExternalInputs.cvbs1),
            (channel.Number == (int)ExternalInputs.cvbs2),
            (channel.Number == (int)ExternalInputs.svhs),
            (channel.Number == (int)ExternalInputs.rgb),
            _cardName);
        }
      }
      finally
      {
        if (_mpeg2DemuxHelper != null)
          _mpeg2DemuxHelper.SetStartingPoint();

        _signalLostTimer = DateTime.Now;
        UpdateVideoState();
        _isTuning = false;
        /*
        if (restartGraph)
        {
          string fname = Recorder.GetTimeShiftFileNameByCardId(_cardId);
          _mpeg2DemuxHelper.StartTimeshifting(fname);
          g_Player.ContinueGraph();
        }*/
      }
      _previousChannel = channel.Number;
      _startTime = DateTime.Now;

    }


    /// <summary>
    /// Property indiciating if the graph supports timeshifting
    /// </summary>
    /// <returns>boolean in diciating if the graph supports timeshifting</returns>
    public bool SupportsTimeshifting()
    {
      return true;
    }

    /// <summary>
    /// Starts viewing the TV channel 
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartViewing(TVChannel channel)
    {

      Log.WriteFile(Log.LogType.Log, "SinkGraph:StartViewing()");
      if (_graphState != State.Created && _graphState != State.Viewing) return false;

      _countryCode = channel.Country;
      if (_mpeg2DemuxHelper == null)
      {
        _lastError="Graph not correctly build";
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:StartViewing() FAILED: no mpeg2 demuxer present");
        return false;
      }
      if (_videoCaptureHelper == null)
      {
        _lastError="Graph not correctly build";
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:StartViewing() FAILED: no video capture device present");
        return false;
      }
      if (_graphState == State.Viewing)
      {
        if (channel.Number != _channelNumber)
        {
          TuneChannel(channel);
        }
        return true;
      }

      // add VMR9 renderer to graph
      if (_vmr9 == null)
      {
        _vmr9 = new VMR9Util();
      }
      if (false == _vmr9.AddVMR9(_graphBuilderInterface))
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }


      AddPreferredCodecs(true, true);

      _graphState = State.Viewing;
      TuneChannel(channel);

      SetFrameRateAndSize();
      _mpeg2DemuxHelper.StartViewing(GUIGraphicsContext.ActiveForm, _vmr9);

      DirectShowUtil.EnableDeInterlace(_graphBuilderInterface);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;

      }

      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      GUIGraphicsContext_OnVideoWindowChanged();

      if (_vmr9 != null)
      {
        if (_vmr9.IsVMR9Connected)
        {
          _vmr9.SetDeinterlaceMode();
        }
        else
        {
          _vmr9.Dispose();
          _vmr9 = null;
        }
      }

      //use default quality
      SetQuality(4);
      Log.WriteFile(Log.LogType.Log, "SinkGraph:StartViewing() started ");
      return true;
    }


    /// <summary>
    /// Stops viewing the TV channel 
    /// </summary>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be viewing first with StartViewing()
    /// After stopping the graph is deleted
    /// </remarks>
    public bool StopViewing()
    {
      if (_graphState != State.Viewing) return false;

      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      Log.WriteFile(Log.LogType.Log, "SinkGraph:StopViewing()");
      if (_vmr9 != null)
      {
        _vmr9.Enable(false);
      }
      if (_mpeg2DemuxHelper != null)
        _mpeg2DemuxHelper.StopViewing(_vmr9);
      _vmr9 = null;
      _graphState = State.Created;



      return true;
    }

    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>
    private void GUIGraphicsContext_OnVideoWindowChanged()
    {
      if (!GUIGraphicsContext.VMR9Allowed) return;
      if (GUIGraphicsContext.Vmr9Active) return;
      if (_graphState != State.Viewing) return;
      if (_mpeg2DemuxHelper == null) return;

      if (GUIGraphicsContext.BlankScreen)
      {
        _mpeg2DemuxHelper.Overlay = false;
      }
      else
      {
        _mpeg2DemuxHelper.Overlay = true;
      }
      int aspectX, aspectY;
      int iVideoWidth, iVideoHeight;
      _mpeg2DemuxHelper.GetVideoSize(out iVideoWidth, out iVideoHeight);
      _mpeg2DemuxHelper.GetPreferredAspectRatio(out aspectX, out aspectY);
      GUIGraphicsContext.VideoSize = new Size(iVideoWidth, iVideoHeight);
      if (GUIGraphicsContext.IsFullScreenVideo || false == GUIGraphicsContext.ShowBackground)
      {
        float x = GUIGraphicsContext.OverScanLeft;
        float y = GUIGraphicsContext.OverScanTop;
        int nw = GUIGraphicsContext.OverScanWidth;
        int nh = GUIGraphicsContext.OverScanHeight;
        if (nw <= 0 || nh <= 0) return;


        System.Drawing.Rectangle rSource, rDest;
        MediaPortal.GUI.Library.Geometry m_geometry = new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth = iVideoWidth;
        m_geometry.ImageHeight = iVideoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(out rSource, out rDest);
        m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;

        Log.Write("overlay: video WxH  : {0}x{1}", iVideoWidth, iVideoHeight);
        Log.Write("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Write("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Write("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Write("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Write("overlay: src        : ({0},{1})-({2},{3})",
          rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
        Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
          rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);

        if (rSource.Left < 0 || rSource.Top < 0 || rSource.Width <= 0 || rSource.Height <= 0) return;
        if (rDest.Left < 0 || rDest.Top < 0 || rDest.Width <= 0 || rDest.Height <= 0) return;
        _mpeg2DemuxHelper.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        _mpeg2DemuxHelper.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
        _mpeg2DemuxHelper.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
      else
      {
        if (GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 ||
          GUIGraphicsContext.VideoWindow.Width <= 0 || GUIGraphicsContext.VideoWindow.Height <= 0) return;
        if (iVideoHeight <= 0 || iVideoWidth <= 0) return;

        _mpeg2DemuxHelper.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);

        if (GUIGraphicsContext.VideoWindow.Width > 0 && GUIGraphicsContext.VideoWindow.Height > 0)
          _mpeg2DemuxHelper.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);

        if (GUIGraphicsContext.VideoWindow.Width > 0 && GUIGraphicsContext.VideoWindow.Height > 0)
          _mpeg2DemuxHelper.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
      }
    }


    /// <summary>
    /// Add preferred mpeg2 audio/video codecs to the graph
    /// and if wanted add ffdshow postprocessing to the graph
    /// </summary>
    void AddPreferredCodecs(bool audio, bool video)
    {
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
      }
      if (video && strVideoCodec.Length > 0) DirectShowUtil.AddFilterToGraph(_graphBuilderInterface, strVideoCodec);
      if (audio && strAudioCodec.Length > 0) DirectShowUtil.AddFilterToGraph(_graphBuilderInterface, strAudioCodec);
      if (audio && strAudioRenderer.Length > 0) DirectShowUtil.AddAudioRendererToGraph(_graphBuilderInterface, strAudioRenderer, false);
      if (video && bAddFFDshow) DirectShowUtil.AddFilterToGraph(_graphBuilderInterface, "ffdshow raw video filter");
    }


    /// <summary>
    /// Returns whether the tvtuner is tuned to a tv channel or not
    /// </summary>
    /// <returns>
    /// true: tvtuner is tuned to a tv channel
    /// false: tvtuner is not tuner to a tv channel 
    /// </returns>
    public bool SignalPresent()
    {
      if (_tvTunerInterface == null) return false;
      if (_channelNumber >= (int)ExternalInputs.svhs) return true;
      AMTunerSignalStrength strength;
      _tvTunerInterface.SignalPresent(out strength);
      return (((int)strength) >= 1);
    }

    public int SignalQuality()
    {
      if (_tvTunerInterface == null) return 1;
      if (_channelNumber >= (int)ExternalInputs.svhs) return 100;
      AMTunerSignalStrength strength;
      _tvTunerInterface.SignalPresent(out strength);
      if (strength == AMTunerSignalStrength.SignalPresent) return 100;
      return 1;
    }

    public int SignalStrength()
    {
      if (_tvTunerInterface == null) return 1;
      if (_channelNumber >= (int)ExternalInputs.svhs) return 100;
      AMTunerSignalStrength strength;
      _tvTunerInterface.SignalPresent(out strength);
      if (strength == AMTunerSignalStrength.SignalPresent) return 100;
      return 1;
    }

    /// <summary>
    /// Return video frequency in Hz of current tv channel
    /// </summary>
    /// <returns>video frequency in Hz </returns>
    public long VideoFrequency()
    {
      if (_tvTunerInterface == null) return 0;
      int lFreq;
      _tvTunerInterface.get_VideoFrequency(out lFreq);
      return lFreq;
    }

    /// <summary>
    /// Callback from GUIGraphicsContext
    /// Will be called when the user changed the gamma, brightness,contrast settings
    /// This method takes the new settings and applies them to the video amplifier
    /// </summary>
    static bool reentrant = false;
    protected void OnGammaContrastBrightnessChanged()
    {
      if (_graphState != State.Recording && _graphState != State.TimeShifting && _graphState != State.Viewing) return;
      if (_videoProcAmpHelper == null) return;

      if (reentrant) return;
      reentrant = true;

      //set the changed values
      if (GUIGraphicsContext.Brightness > -1)
      {
        _videoProcAmpHelper.Brightness = GUIGraphicsContext.Brightness;
      }
      else
      {
        GUIGraphicsContext.Brightness = _videoProcAmpHelper.Brightness;
      }

      if (GUIGraphicsContext.Contrast > -1)
      {
        _videoProcAmpHelper.Contrast = GUIGraphicsContext.Contrast;
      }
      else
      {
        GUIGraphicsContext.Contrast = _videoProcAmpHelper.Contrast;
      }

      if (GUIGraphicsContext.Gamma > -1)
      {
        _videoProcAmpHelper.Gamma = GUIGraphicsContext.Gamma;
      }
      else
      {
        GUIGraphicsContext.Gamma = _videoProcAmpHelper.Gamma;
      }

      if (GUIGraphicsContext.Saturation > -1)
      {
        _videoProcAmpHelper.Saturation = GUIGraphicsContext.Saturation;
      }
      else
      {
        GUIGraphicsContext.Saturation = _videoProcAmpHelper.Saturation;
      }


      if (GUIGraphicsContext.Sharpness > -1)
      {
        _videoProcAmpHelper.Sharpness = GUIGraphicsContext.Sharpness;
      }
      else
      {
        GUIGraphicsContext.Sharpness = _videoProcAmpHelper.Sharpness;
      }


      //get back the changed values
      GUIGraphicsContext.Brightness = _videoProcAmpHelper.Brightness;
      GUIGraphicsContext.Contrast = _videoProcAmpHelper.Contrast;
      GUIGraphicsContext.Gamma = _videoProcAmpHelper.Gamma;
      GUIGraphicsContext.Saturation = _videoProcAmpHelper.Saturation;
      GUIGraphicsContext.Sharpness = _videoProcAmpHelper.Sharpness;
      reentrant = false;
    }

    /// <summary>
    /// Sets the tv standard used (pal,ntsc,secam,...) for the video decoder
    /// </summary>
    /// <param name="standard">TVStandard</param>
    protected void SetVideoStandard(AnalogVideoStandard standard)
    {
      VideoCaptureProperties props = new VideoCaptureProperties(_filterCapture);
      props.SetTvFormat(standard);
      if (standard == AnalogVideoStandard.None) return;

      if (_analogVideoDecoderInterface == null) return;
      AnalogVideoStandard currentStandard;
      int hr = _analogVideoDecoderInterface.get_TVFormat(out currentStandard);
      //if (currentStandard==standard) return;

      Log.WriteFile(Log.LogType.Log, "SinkGraph:Select tvformat:{0}", standard.ToString());
      if (standard == AnalogVideoStandard.None) standard = AnalogVideoStandard.PAL_B;
      hr = _analogVideoDecoderInterface.put_TVFormat(standard);
      if (hr != 0)
        Log.WriteFile(Log.LogType.Log, true, "SinkGraph:Unable to select tvformat:{0}", standard.ToString());
    }

    /// <summary>
    /// Initializes the TV tuner with the 
    ///		- tuning space
    ///		- country (depends on _countryCode)
    ///		- Sets tuner mode to TV
    ///		- Sets tuner to cable or antenna (depends on _isUsingCable)
    /// </summary>
    protected void InitializeTuner()
    {
      if (_tvTunerInterface == null) return;
      int iTuningSpace, iCountry;
      AMTunerModeType mode;

      _tvTunerInterface.get_TuningSpace(out iTuningSpace);
      if (iTuningSpace != 0) _tvTunerInterface.put_TuningSpace(0);

      _tvTunerInterface.get_CountryCode(out iCountry);
      if (iCountry != _countryCode)
        _tvTunerInterface.put_CountryCode(_countryCode);

      _tvTunerInterface.get_Mode(out mode);
      if (mode != AMTunerModeType.TV)
        _tvTunerInterface.put_Mode(AMTunerModeType.TV);

      TunerInputType inputType;
      _tvTunerInterface.get_InputType(0, out inputType);
      if (_isUsingCable)
      {
        if (inputType != TunerInputType.Cable)
          _tvTunerInterface.put_InputType(0, TunerInputType.Cable);
      }
      else
      {
        if (inputType != TunerInputType.Antenna)
          _tvTunerInterface.put_InputType(0, TunerInputType.Antenna);
      }
    }

    void UpdateVideoState()
    {
      //check if this card is used for watching tv
      bool isViewing = Recorder.IsCardViewing(_cardId);
      if (!isViewing) return;

      if (!SignalPresent())
      {
        TimeSpan ts = DateTime.Now - _signalLostTimer;
        if (ts.TotalSeconds < 5)
        {
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
          return;
        }
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
      }
      else
      {
        if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS < 1f)
        {
          if ((g_Player.Playing && !g_Player.Paused) || (!g_Player.Playing))
          {
            TimeSpan ts = DateTime.Now - _signalLostTimer;
            if (ts.TotalSeconds < 5)
            {
              VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
              return;
            }
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
            return;
          }
        }
        _signalLostTimer = DateTime.Now;
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      }
    }

    public void Process()
    {
      if (!GUIGraphicsContext.VMR9Allowed) return;
      if (_captureGraphBuilderInterface == null) return;
      if (_filterCapture == null) return;


      if (_graphState == State.Viewing)
      {
        if (GUIGraphicsContext.Vmr9Active && _vmr9 != null)
        {
          _vmr9.Process();
        }
      }
      if (!_isTuning)
      {
        UpdateVideoState();
      }
    }

    public PropertyPageCollection PropertyPages()
    {

      PropertyPageCollection propertyPages = null;
      {
        if (_captureGraphBuilderInterface == null) return null;
        if (_filterCapture == null) return null;
        try
        {
          SourceCollection VideoSources = new SourceCollection(_captureGraphBuilderInterface, _filterCapture, true);

          // #MW#, difficult to say if this must be changed, as it depends on the loaded
          // filters. The list below is fixed list however... So???
          propertyPages = new PropertyPageCollection(_captureGraphBuilderInterface,
            _filterCapture, null,
            null, null,
            VideoSources, null, null);

        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }

        return (propertyPages);
      }
    }


    public bool SupportsFrameSize(Size framesize)
    {
      return false;
    }

    public NetworkType Network()
    {
      return NetworkType.Analog;
    }
    public void Tune(object tuningObject, int disecqNo)
    {
    }
    public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels, ref int newRadioChannels, ref int updatedRadioChannels)
    {
      if (!SignalPresent()) return;
      if (tv)
      {
        TVChannel tvChan = null;
        int channelId = TVDatabase.GetChannelId(_channelNumber);
        tvChan = TVDatabase.GetChannelById(channelId);
        if (tvChan == null)
        {
          //doesn't exists
          tvChan = new TVChannel();
          tvChan.Frequency = VideoFrequency();
          tvChan.Scrambled = false;
          //then add a new channel to the database
          tvChan.Name = GetTeletextChannelName();
          if (tvChan.Name == string.Empty)
          {
            tvChan.Name = _channelNumber.ToString();
          }
          tvChan.ID = -1;
          tvChan.Number = _channelNumber;
          tvChan.Sort = 40000;
          Log.WriteFile(Log.LogType.Log, "SinkGraph: add new channel for {0}:{1}:{2}", tvChan.Name, tvChan.Number, tvChan.Sort);
          int id = TVDatabase.AddChannel(tvChan);
          if (id < 0)
          {
            Log.WriteFile(Log.LogType.Log, true, "SinkGraph: failed to add new channel for {0}:{1}:{2} to database", tvChan.Name, tvChan.Number, tvChan.Sort);
          }
          channelId = id;
          newChannels++;
        }
        else
        {
          TVDatabase.UpdateChannel(tvChan, tvChan.Sort);
          updatedChannels++;
          Log.WriteFile(Log.LogType.Log, "SinkGraph: update channel {0}:{1}:{2} {3}", tvChan.Name, tvChan.Number, tvChan.Sort, tvChan.ID);
        }
        TVDatabase.MapChannelToCard(tvChan.ID, ID);

        TVGroup group = new TVGroup();
        group.GroupName = "Analog";
        int groupid = TVDatabase.AddGroup(group);
        group.ID = groupid;
        TVDatabase.MapChannelToGroup(group, tvChan);
      }
      if (radio)
      {
        if (_graphState != State.Radio) return;
        int frequency = 0;
        int hr = _tvTunerInterface.get_AudioFrequency(out frequency);
        if ((hr != 0) || (frequency == 0))
        {
          return;
        }
        long stationFrequency = frequency;
        float floatFrequency = ((float)stationFrequency) / 1000000f;
        string stationName = String.Format("{0:###.##}", floatFrequency);
        //TODO get name from RDS
        MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
        if (!RadioDatabase.GetStation(stationName, out station))
        {
          //doesn't exists
          //then add a new station to the database
          station.Scrambled = false;
          station.ID = -1;
          station.Name = stationName;
          station.Frequency = stationFrequency;
          station.Sort = 40000;
          station.Channel = GetUniqueRadioChannel();
          Log.WriteFile(Log.LogType.Log, "Wizard_AnalogRadio: add new station for {0}:{1}", station.Name, station.Frequency);
          int id = RadioDatabase.AddStation(ref station);
          if (id < 0)
          {
            Log.WriteFile(Log.LogType.Log, true, "Wizard_AnalogRadio: failed to add new station for {0}:{1} to database", station.Name, station.Frequency);
          }
          newRadioChannels++;
        }
        else
        {
          station.Name = stationName;
          station.Frequency = stationFrequency;
          RadioDatabase.UpdateStation(station);
          updatedRadioChannels++;
          Log.WriteFile(Log.LogType.Log, "Wizard_AnalogRadio: update station {0}:{1} {2}", station.Name, station.Frequency, station.ID);
        }
        RadioDatabase.MapChannelToCard(station.ID, _card.ID);

      }
    }
    public string GetTeletextChannelName()
    {
      bool currentTeletextGrabbing = _grabTeletext;
      _grabTeletext = true;
      TeletextGrabber.TeletextCache.ClearTeletextChannelName();
      string channelName = "";
      for (int i = 0; i < 5; i++)
      {
        channelName = TeletextGrabber.TeletextCache.GetTeletextChannelName();
        if (channelName != string.Empty) break;
        System.Threading.Thread.Sleep(500);
      }
      _grabTeletext = currentTeletextGrabbing;
      return channelName;
    }
    int GetUniqueRadioChannel()
    {
      ArrayList stations = new ArrayList();
      RadioDatabase.GetStations(ref stations);
      int number = 1;
      while (true)
      {
        bool unique = true;
        foreach (MediaPortal.Radio.Database.RadioStation station in stations)
        {
          if (station.Channel == number)
          {
            unique = false;
            break;
          }
        }
        if (!unique)
        {
          number++;
        }
        else
        {
          return number;
        }
      }
    }


    public void TuneRadioChannel(RadioStation station)
    {
      Log.WriteFile(Log.LogType.Log, "SinkGraphEx:tune to {0} {1} hz", station.Name, station.Frequency);
      _isTuning = true;
      _tvTunerInterface.put_TuningSpace(0);
      _tvTunerInterface.put_CountryCode(_countryCode);
      _tvTunerInterface.put_Mode(AMTunerModeType.FMRadio);
      if (_isUsingCable)
      {
        _tvTunerInterface.put_InputType(0, TunerInputType.Cable);
      }
      else
      {
        _tvTunerInterface.put_InputType(0, TunerInputType.Antenna);
      }
      _tvTunerInterface.put_Channel((int)station.Frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
      int frequency;
      _tvTunerInterface.get_AudioFrequency(out frequency);
      Log.WriteFile(Log.LogType.Log, "SinkGraphEx:  tuned to {0} hz", frequency);
      _isTuning = false;
    }

    public void StartRadio(RadioStation station)
    {
      if (_graphState != State.Radio)
      {
        if (_graphState != State.Created) return;
        if (_mpeg2DemuxHelper == null) return;
        if (_videoCaptureHelper == null) return;
        if (_vmr9 != null)
        {
          _vmr9.Dispose();
          _vmr9 = null;
        }

        AddPreferredCodecs(true, false);

        CrossBar.RouteEx(_graphBuilderInterface,
          _captureGraphBuilderInterface,
          _filterCapture,
          true,
          false,
          false,
          false,
          false,
          _cardName);
        TuneRadioChannel(station);
        _mpeg2DemuxHelper.StartListening();


        Log.WriteFile(Log.LogType.Log, "SinkGraph:StartRadio() started");
        _graphState = State.Radio;
        return;
      }
      TuneRadioChannel(station);
    }

    public void StopRadio()
    {
      if (_graphState != State.Radio) return; ;
      _mpeg2DemuxHelper.StopListening();
      _graphState = State.Created;
    }

    public void TuneRadioFrequency(int frequency)
    {
      Log.WriteFile(Log.LogType.Log, "SinkGraphEx:tune to {0} hz", frequency);
      _isTuning = true;
      _tvTunerInterface.put_TuningSpace(0);
      _tvTunerInterface.put_CountryCode(_countryCode);
      _tvTunerInterface.put_Mode(AMTunerModeType.FMRadio);
      if (_isUsingCable)
      {
        _tvTunerInterface.put_InputType(0, TunerInputType.Cable);
      }
      else
      {
        _tvTunerInterface.put_InputType(0, TunerInputType.Antenna);
      }
      _tvTunerInterface.put_Channel(frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
      _tvTunerInterface.get_AudioFrequency(out frequency);
      Log.WriteFile(Log.LogType.Log, "SinkGraphEx:  tuned to {0} hz", frequency);
      _isTuning = false;
    }

    protected void SetFrameRateAndSize()
    {
      if (_videoCaptureHelper == null) return;
      string filename = String.Format(@"database\card_{0}.xml", _card.FriendlyName);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        string frameRate = xmlreader.GetValueAsString("analog", "framerate", "25 fps (PAL/SECAM)");
        string frameSize = xmlreader.GetValueAsString("analog", "framesize", "720x576 PAL/SECAM ITU-601 D1 (recommended)");
        if (frameRate == "29.97 fps (NTSC)")
          _videoCaptureHelper.SetFrameRate(29.97);
        else if (frameRate == "25 fps (PAL/SECAM)")
          _videoCaptureHelper.SetFrameRate(25);
        else if (frameRate == "15 fps")
          _videoCaptureHelper.SetFrameRate(15);

        if (frameSize == "768x576 PAL square pixels")
          _videoCaptureHelper.SetFrameSize(new Size(768, 576));
        else if (frameSize == "720x576 PAL/SECAM ITU-601 D1 (recommended)")
          _videoCaptureHelper.SetFrameSize(new Size(720, 576));
        else if (frameSize == "720x480 NTSC ITU-601 D1 (recommended)")
          _videoCaptureHelper.SetFrameSize(new Size(720, 480));
        else if (frameSize == "704x576 PAL/SECAM TV broadcast")
          _videoCaptureHelper.SetFrameSize(new Size(704, 576));
        else if (frameSize == "704x480 NTSC TV broadcast")
          _videoCaptureHelper.SetFrameSize(new Size(704, 480));
        else if (frameSize == "640x480 NTSC square pixels")
          _videoCaptureHelper.SetFrameSize(new Size(640, 480));
        else if (frameSize == "352x288 PAL CIF")
          _videoCaptureHelper.SetFrameSize(new Size(352, 288));
        else if (frameSize == "352x240 NTSC CIF")
          _videoCaptureHelper.SetFrameSize(new Size(352, 240));
        else if (frameSize == "320x240 NTSC CIF square pixels")
          _videoCaptureHelper.SetFrameSize(new Size(352, 240));
      }
    }
    protected void SetQuality(int Quality)
    {
      string filename = String.Format(@"database\card_{0}.xml", _card.FriendlyName);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        bool enabled = xmlreader.GetValueAsBool("quality", "enabled", false);
        if (!enabled) return;

        int defaultQuality = xmlreader.GetValueAsInt("quality", "default", 2);
        if (Quality == 4)
        {
          Quality = defaultQuality;
        }
        int portableMinKbps = xmlreader.GetValueAsInt("quality", "portLow", 100);
        int portableMaxKbps = xmlreader.GetValueAsInt("quality", "portMax", 300);
        bool portableVBR = xmlreader.GetValueAsBool("quality", "portVBR", false);

        int lowMinKbps = xmlreader.GetValueAsInt("quality", "LowLow", 500);
        int lowMaxKbps = xmlreader.GetValueAsInt("quality", "LowMax", 1500);
        bool lowVBR = xmlreader.GetValueAsBool("quality", "LowVBR", true);

        int mediumMinKbps = xmlreader.GetValueAsInt("quality", "MedLow", 2000);
        int mediumMaxKbps = xmlreader.GetValueAsInt("quality", "MedMax", 4000);
        bool mediumVBR = xmlreader.GetValueAsBool("quality", "MedVBR", false);

        int highMinKbps = xmlreader.GetValueAsInt("quality", "HighLow", 8000);
        int highMaxKbps = xmlreader.GetValueAsInt("quality", "HighMax", 12000);
        bool highVBR = xmlreader.GetValueAsBool("quality", "HighVBR", true);

        string comName = this._card.Graph.CommercialName;
        if (comName.IndexOf("usb") >= 0)
        {
          highMinKbps = xmlreader.GetValueAsInt("quality", "HighLow", 768);
          highMaxKbps = xmlreader.GetValueAsInt("quality", "HighMax", 4000);
        }


        VideoCaptureProperties props = new VideoCaptureProperties(_filterCapture);
        if (Quality >= 0)
        {
          switch (Quality)
          {
            case 0://Portable
              Log.WriteFile(Log.LogType.Log, "SinkGraph:Set quality:portable");
              props.SetVideoBitRate(portableMinKbps, portableMaxKbps, portableVBR);
              break;
            case 1://low
              Log.WriteFile(Log.LogType.Log, "SinkGraph:Set quality:low");
              props.SetVideoBitRate(lowMinKbps, lowMaxKbps, lowVBR);
              break;
            case 2://medium
              Log.WriteFile(Log.LogType.Log, "SinkGraph:Set quality:medium");
              props.SetVideoBitRate(mediumMinKbps, mediumMaxKbps, mediumVBR);
              break;
            case 3://hi
              Log.WriteFile(Log.LogType.Log, "SinkGraph:Set quality:high");
              props.SetVideoBitRate(highMinKbps, highMaxKbps, highVBR);
              break;

            default://
              Log.WriteFile(Log.LogType.Log, "SinkGraph:Set quality to default (medium)");
              props.SetVideoBitRate(mediumMinKbps, mediumMaxKbps, mediumVBR);
              break;
          }
          int minKbps, maxKbps;
          bool isVBR;
          props.GetVideoBitRate(out minKbps, out maxKbps, out isVBR);
          Log.WriteFile(Log.LogType.Log, " driver version:{0} min:{1} peak:{2} vbr:{3}", props.VersionInfo, minKbps, maxKbps, isVBR);
        }
      }//if (Quality>=0)
    }//protected void SetQuality(int Quality)

    public bool HasTeletext()
    {
      return _hasTeletext;
    }

    public int GetAudioLanguage()
    {
      return _selectedAudioLanguage;
    }
    public void SetAudioLanguage(int audioPid)
    {
      _selectedAudioLanguage = audioPid;
    }
    public ArrayList GetAudioLanguageList()
    {
#if DEBUG
			DVBSections.AudioLanguage al;
			_listAudioPids.Clear();

			// Add two debug languages
			al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
			al.AudioPid=10;
			al.AudioLanguageCode="eng";
			_listAudioPids.Add(al);

			al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
			al.AudioPid=11;
			al.AudioLanguageCode="dut";
			_listAudioPids.Add(al);

#endif
      return _listAudioPids;
    }

    public string TvTimeshiftFileName()
    {
      return "live.tv";
    }
    public string RadioTimeshiftFileName()
    {
      return String.Empty;
    }

    public void GrabTeletext(bool yesNo)
    {
      _grabTeletext = yesNo;
    }

    public IBaseFilter AudiodeviceFilter()
    {
      return null;
    }

    public bool IsTimeShifting()
    {
      return _graphState == State.TimeShifting;
    }

    public bool IsEpgDone()
    {
      return true;
    }
    public bool IsEpgGrabbing()
    {
      return false;
    }
    public bool GrabEpg(TVChannel chan)
    {
      return false;
    }

    public void RadioChannelMinMax(out int chanmin, out int chanmax)
    {
      Log.WriteFile(Log.LogType.Log, "SinkGraph:Getting Min and Max Radio channels");
      _tvTunerInterface.put_TuningSpace(0);
      _tvTunerInterface.put_CountryCode(_countryCode);
      _tvTunerInterface.put_Mode(AMTunerModeType.FMRadio);
      if (_isUsingCable)
      {
        _tvTunerInterface.put_InputType(0, TunerInputType.Cable);
      }
      else
      {
        _tvTunerInterface.put_InputType(0, TunerInputType.Antenna);
      }
      _tvTunerInterface.ChannelMinMax(out chanmin, out chanmax);
      Log.WriteFile(Log.LogType.Log, "SinkGraph:  Radio Channel Min {0} hz - Radio Channel Max {1}", chanmin, chanmax);

    }

    public void TVChannelMinMax(out int chanmin, out int chanmax)
    {
      Log.WriteFile(Log.LogType.Log, "SinkGraph:Getting Min and Max TV channels");
      InitializeTuner();
      _tvTunerInterface.ChannelMinMax(out chanmin, out chanmax);
      Log.WriteFile(Log.LogType.Log, "SinkGraph:  TV Channel Min {0} hz - Radio Channel Max {1}", chanmin, chanmax);

    }
    public bool StopEpgGrabbing()
    {
      return true;
    }

    public bool SupportsHardwarePidFiltering()
    {
      return false;
    }
    public bool SupportsCamSelection()
    {
      return false;
    }

    public bool CanViewTimeShiftFile()
    {
      if (_graphState != State.TimeShifting && _graphState != State.Recording) return false;
      return true;
    }
    public bool IsRadio()
    {
      return (_graphState == State.Radio);
    }
    public bool IsRecording()
    {
      return (_graphState == State.Recording);
    }
    public string LastError()
    {
      return _lastError;
    }
  }
}
