using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Epg;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.DVB
{
  public class TvCardATSC : TvCardDvbBase, IDisposable, ITVCard
  {

    #region variables
    protected IATSCTuningSpace _tuningSpace = null;
    protected IATSCChannelTuneRequest _tuneRequest = null;
    DsDevice _device;
    #endregion

    public TvCardATSC(DsDevice device)
    {
      _device = device;
      _name = device.Name;
      _devicePath = device.DevicePath;
    }

    #region graphbuilding
    public void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.WriteFile("Graph already build");
          throw new TvException("Graph already build");
        }
        Log.Log.WriteFile("BuildGraph");

        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);

        // Method names should be self explanatory
        AddNetworkProviderFilter(typeof(ATSCNetworkProvider).GUID);
        CreateTuningSpace();
        AddMpeg2DemuxerTif();
        AddAndConnectBDABoardFilters(_device);
        AddTransportStreamFiltersToGraph();

        
        AddSampleGrabber();
        GetTunerSignalStatistics();
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw ex;
      }
    }
    protected void CreateTuningSpace()
    {
      Log.Log.WriteFile("CreateTuningSpace()");
      ITuner tuner = (ITuner)_filterNetworkProvider;
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      IEnumTuningSpaces enumTuning;
      ITuningSpace[] spaces = new ITuningSpace[2];

      ITuneRequest request;
      int fetched;
      container.get_EnumTuningSpaces(out enumTuning);
      while (true)
      {
        enumTuning.Next(1, spaces, out fetched);
        if (fetched != 1) break;
        string name;
        spaces[0].get_UniqueName(out name);
        if (name == "ATSC TuningSpace2")
        {
          Log.Log.WriteFile("got tuningspace");
          _tuningSpace = (IATSCTuningSpace)spaces[0];
          tuner.put_TuningSpace(_tuningSpace);
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IATSCChannelTuneRequest)request;
          return;
        }
      }
      Log.Log.WriteFile("Create new tuningspace");
      _tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();
      _tuningSpace.put_UniqueName("ATSC TuningSpace2");
      _tuningSpace.put_FriendlyName("ATSC TuningSpace2");
      _tuningSpace.put_MaxChannel(10000);
      _tuningSpace.put_MaxMinorChannel(10000);
      _tuningSpace.put_MaxPhysicalChannel(10000);
      _tuningSpace.put__NetworkType(typeof(ATSCNetworkProvider).GUID);


      _tuningSpace.put_MinChannel(0);
      _tuningSpace.put_MinMinorChannel(0);
      _tuningSpace.put_MinPhysicalChannel(0);
      _tuningSpace.put_InputType(TunerInputType.Antenna);


      IATSCLocator locator = (IATSCLocator)new ATSCLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      locator.put_CarrierFrequency(-1);
      locator.put_PhysicalChannel(-1);
      locator.put_TSID(-1);

      object newIndex;
      _tuningSpace.put_DefaultLocator(locator);
      container.Add((ITuningSpace)_tuningSpace, out newIndex);
      tuner.put_TuningSpace(_tuningSpace);
      Release.ComObject("TuningSpaceContainer", container);

      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IATSCChannelTuneRequest)request;

    }
    #endregion

    #region properties
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        _name = value;
      }
    }

    public string FileName
    {
      get
      {
        return _fileName;
      }
    }
    public bool IsRecording
    {
      get
      {
        return (_graphState == GraphState.Recording);
      }
    }
    public bool IsTimeShifting
    {
      get
      {
        return (_graphState == GraphState.TimeShifting);
      }
    }
    #endregion

    #region tuning & recording
    public bool TuneScan(IChannel channel)
    {
      bool result = Tune(channel);
      RunGraph();
      return result;
    }
    public bool Tune(IChannel channel)
    {
      Log.Log.WriteFile("atsc:  Tune:{0}", channel);
      try
      {
        _pmtVersion = -1;
        ATSCChannel atscChannel = channel as ATSCChannel;

        if (atscChannel == null)
        {
          Log.Log.WriteFile("Channel is not a ATSC channel!!! {0}", channel.GetType().ToString());
          return false;
        }
        //ATSCChannel oldChannel = _currentChannel as ATSCChannel;
        //if (_currentChannel != null)
          //{
        //  if (oldChannel.Equals(channel)) return true;
        //}
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        if (!CheckThreadId()) return false;
        ILocator locator;
        Log.Log.WriteFile("Tune: ", atscChannel.ToString());

        _tuningSpace.get_DefaultLocator(out locator);
        IATSCLocator atscLocator = (IATSCLocator)locator;
        int hr = atscLocator.put_Modulation(atscChannel.ModulationType);
        hr = atscLocator.put_InnerFEC(FECMethod.MethodNotSet);
        hr = atscLocator.put_SymbolRate(atscChannel.SymbolRate);
        hr = atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
        hr = atscLocator.put_TSID(atscChannel.TransportId);
        hr = locator.put_CarrierFrequency((int)atscChannel.Frequency);
        hr = _tuneRequest.put_Channel(atscChannel.MajorChannel);
        hr = _tuneRequest.put_MinorChannel(atscChannel.MinorChannel);
        _tuneRequest.put_Locator(locator);

        SubmitTuneRequest(_tuneRequest);
        _currentChannel = channel;

        if (_currentChannel.IsTv)
        {
          FilterGraphTools.ConnectPin(_graphBuilder, _pinVideoTimeShift, _filterMpegMuxerTimeShift, 0);
        }
        else
        {
          _pinVideoTimeShift.Disconnect();
        }
        SetAnalyzerMapping(atscChannel.PmtPid);
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
      return true;
    }
    public bool StartTimeShifting(string fileName)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("StartTimeShifting()");
      if (_graphState == GraphState.TimeShifting)
      {
        return true;
      }
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }

      if (_currentChannel == null)
      {
        Log.Log.WriteFile("atsc:StartTimeShifting not tuned to a channel");
        throw new TvException("StartTimeShifting not tuned to a channel");
      }
      ATSCChannel channel = (ATSCChannel)_currentChannel;
      if (channel.MajorChannel == -1 || channel.MinorChannel == -1)
      {
        Log.Log.WriteFile("atsc:StartTimeShifting not tuned to a channel but to a transponder");
        throw new TvException("StartTimeShifting not tuned to a channel but to a transponder");
      }
      if (_graphState == GraphState.Created)
      {
        string extension = System.IO.Path.GetExtension(fileName).ToLower();
        StopGraph();
        CreateTimeShiftingGraph();
        SetTimeShiftFileName(fileName);
      }

      RunGraph();
      _graphState = GraphState.TimeShifting;
      Tune(Channel);
      FileAccessHelper.GrantFullControll(fileName);
      return true;
    }

    public bool StopTimeShifting()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("StopTimeShifting()");
      if (_graphState != GraphState.TimeShifting)
      {
        return true;
      }
      StopGraph();
      DeleteTimeShiftingGraph();



      _graphState = GraphState.Created;
      return true;
    }
    public bool StartRecording(RecordingType recordingType, string fileName, long startTime)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("StartRecording to {0}", fileName);
      if (_graphState != GraphState.TimeShifting)
      {
        throw new TvException("Card must be timeshifting before starting recording");
      }
      _fileName = fileName;

      _graphState = GraphState.Recording;
      StartRecord(fileName, recordingType, ref startTime);
      Log.Log.WriteFile("Started recording on {0}", startTime);
      return true;
    }
    public bool StopRecording()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("StopRecording");
      _graphState = GraphState.TimeShifting;
      StopRecord();
      return true;
    }
    #endregion

    #region quality control
    public IQuality Quality
    {
      get
      {
        return null;
      }
      set
      {
      }
    }
    public bool SupportsQualityControl
    {
      get
      {
        return false;
      }
    }
    #endregion


    #region epg & scanning
    public ITVScanning ScanningInterface
    {
      get
      {
        return new ATSCScanning(this);
      }
    }

    #endregion

    public override string ToString()
    {
      return _name;
    }


    public bool CanTune(IChannel channel)
    {
      if ((channel as ATSCChannel) == null) return false;
      return true;
    }
  }
}
