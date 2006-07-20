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
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.DVB
{

  public class TvCardDVBT : TvCardDvbBase, IDisposable, ITVCard
  {
    #region variables
    protected IDVBTuningSpace _tuningSpace = null;
    protected IDVBTuneRequest _tuneRequest = null;
    DsDevice _device;
    #endregion

    #region ctor
    public TvCardDVBT(DsDevice device)
    {
      _device = device;
      _name = device.Name;
      _devicePath = device.DevicePath;
    }
    #endregion

    #region graphbuilding
    public void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          throw new TvException("Graph already build");
        }
        Log.Log.WriteFile("BuildGraph");

        _graphBuilder = (IFilterGraph2)new FilterGraph();

        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);

        // Method names should be self explanatory
        AddNetworkProviderFilter(typeof(DVBTNetworkProvider).GUID);
        CreateTuningSpace();
        AddMpeg2DemuxerTif();
        AddAndConnectBDABoardFilters(_device);
        AddTransportStreamFiltersToGraph();

        //        ConnectFilters();
        AddAnalyzerFilter();
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
        Log.Log.WriteFile("Found tuningspace {0}", name);
        if (name == "DVBT TuningSpace")
        {
          Log.Log.WriteFile("Found correct tuningspace {0}", name);
          _tuningSpace = (IDVBTuningSpace)spaces[0];
          tuner.put_TuningSpace(_tuningSpace);
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IDVBTuneRequest)request;
          return;
        }
        Release.ComObject("ITuningSpace", spaces[0]);
      }

      Release.ComObject("IEnumTuningSpaces", enumTuning);
      Log.Log.WriteFile("Create new tuningspace");
      _tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
      _tuningSpace.put_UniqueName("DVBT TuningSpace");
      _tuningSpace.put_FriendlyName("DVBT TuningSpace");
      _tuningSpace.put__NetworkType(typeof(DVBTNetworkProvider).GUID);
      _tuningSpace.put_SystemType(DVBSystemType.Terrestrial);

      IDVBTLocator locator = (IDVBTLocator)new DVBTLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);

      object newIndex;
      _tuningSpace.put_DefaultLocator(locator);
      container.Add((ITuningSpace)_tuningSpace, out newIndex);
      tuner.put_TuningSpace(_tuningSpace);
      Release.ComObject("ITuningSpaceContainer", container);

      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;

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
      Log.Log.WriteFile("dvbt:  Tune:{0}", channel);
      try
      {
        _pmtVersion = -1;
        DVBTChannel dvbtChannel = channel as DVBTChannel;

        if (dvbtChannel == null)
        {
          Log.Log.WriteFile("Channel is not a DVBT channel!!! {0}", channel.GetType().ToString());
          return false;
        }
        //DVBTChannel oldChannel = _currentChannel as DVBTChannel;
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
        _tuningSpace.get_DefaultLocator(out locator);
        IDVBTLocator dvbtLocator = (IDVBTLocator)locator;
        int hr = dvbtLocator.put_Bandwidth(dvbtChannel.BandWidth);
        hr = _tuneRequest.put_ONID(dvbtChannel.NetworkId);
        hr = _tuneRequest.put_SID(dvbtChannel.ServiceId);
        hr = _tuneRequest.put_TSID(dvbtChannel.TransportId);
        hr = locator.put_CarrierFrequency((int)dvbtChannel.Frequency);
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
        SetAnalyzerMapping(dvbtChannel.PmtPid);
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
      try
      {

        Log.Log.WriteFile("StartTimeShifting()");

        if (!CheckThreadId()) return false;
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
          Log.Log.WriteFile("dvbt:StartTimeShifting not tuned to a channel");
          throw new TvException("StartTimeShifting not tuned to a channel");
        }

        DVBBaseChannel channel = (DVBBaseChannel)_currentChannel;
        if (channel.NetworkId == -1 || channel.TransportId == -1 || channel.ServiceId == -1)
        {
          Log.Log.WriteFile("dvbt:StartTimeShifting not tuned to a channel but to a transponder");
          throw new TvException("StartTimeShifting not tuned to a channel but to a transponder");
        }
        if (_graphState == GraphState.Created)
        {
          string extension = System.IO.Path.GetExtension(fileName).ToLower();
          StopGraph();

          CreateTimeShiftingGraph();
          SetTimeShiftFileName(fileName);
        }

        _graphState = GraphState.TimeShifting;
        RunGraph();
        Tune(Channel);
        FileAccessHelper.GrantFullControll(fileName);
        Log.Log.WriteFile("StartTimeShifting() done");
        return true;

      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
    }

    public bool StopTimeShifting()
    {
      try
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

      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
      return true;
    }

    public bool StartRecording(RecordingType recordingType, string fileName, long startTime)
    {
      try
      {
        if (!CheckThreadId()) return false;
        Log.Log.WriteFile("StartRecording to {0}", fileName);

        if (_graphState == GraphState.Recording) return false;

        if (_graphState != GraphState.TimeShifting)
        {
          throw new TvException("Card must be timeshifting before starting recording");
        }

        _graphState = GraphState.Recording;
        StartRecord(fileName, recordingType, ref startTime);

        _fileName = fileName;
        Log.Log.WriteFile("Started recording on {0}", startTime);

        return true;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
    }

    public bool StopRecording()
    {
      try
      {
        if (!CheckThreadId()) return false;
        if (_graphState != GraphState.Recording) return false;
        Log.Log.WriteFile("StopRecording");
        _graphState = GraphState.TimeShifting;
        StopRecord();
        return true;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
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
        if (!CheckThreadId()) return null;
        return new DVBTScanning(this);
      }
    }

    #endregion


    public override string ToString()
    {
      return _name;
    }

    public bool CanTune(IChannel channel)
    {
      if ((channel as DVBTChannel) == null) return false;
      return true;
    }
  }
}
