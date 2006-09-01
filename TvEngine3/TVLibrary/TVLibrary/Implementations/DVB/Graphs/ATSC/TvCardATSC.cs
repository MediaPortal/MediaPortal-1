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
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles ATSC BDA cards
  /// </summary>
  public class TvCardATSC : TvCardDvbBase, IDisposable, ITVCard
  {

    #region variables
    /// <summary>
    /// Hold the ATSC tuning space
    /// </summary>
    protected IATSCTuningSpace _tuningSpace = null;
    /// <summary>
    /// Holds the current ATSC tuning request
    /// </summary>
    protected IATSCChannelTuneRequest _tuneRequest = null;
    /// <summary>
    /// Holds the card's device moniker
    /// </summary>
    DsDevice _device;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TvCardATSC"/> class.
    /// </summary>
    /// <param name="device">The device.</param>
    public TvCardATSC(DsDevice device)
    {
      _device = device;
      _name = device.Name;
      _devicePath = device.DevicePath;
      try
      {
        //BuildGraph();
        //RunGraph();
        //StopGraph();
      }
      catch (Exception)
      {
      }
    }

    #region graphbuilding
    /// <summary>
    /// Builds the graph.
    /// </summary>
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
    /// <summary>
    /// Creates the tuning space.
    /// </summary>
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
    /// <summary>
    /// Gets/sets the card name
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    /// <value></value>
    public string FileName
    {
      get
      {
        return _recordingFileName;
      }
    }
    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    /// <value></value>
    public bool IsRecording
    {
      get
      {
        return (_graphState == GraphState.Recording);
      }
    }
    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    /// <value></value>
    public bool IsTimeShifting
    {
      get
      {
        return (_graphState == GraphState.TimeShifting);
      }
    }
    #endregion

    #region tuning & recording
    /// <summary>
    /// tune the card to the channel specified by IChannel
    /// </summary>
    /// <param name="channel">channel to tune</param>
    /// <returns></returns>
    public bool TuneScan(IChannel channel)
    {
      bool result = Tune(channel);
      RunGraph();
      return result;
    }
    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
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
        if (IsReceivingAudioVideo == false)
        {
          ATSCChannel oldChannel = _currentChannel as ATSCChannel;
          if (_currentChannel != null)
          {
            if (oldChannel.Equals(channel)) return true;
          }
        }
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        if (!CheckThreadId()) return false;
        ILocator locator;
        Log.Log.WriteFile("Tune: ", atscChannel.ToString());

        _tuningSpace.get_DefaultLocator(out locator);
        IATSCLocator atscLocator = (IATSCLocator)locator;
        int hr;
        hr = locator.put_CarrierFrequency(-1);//(int)atscChannel.Frequency);
        hr = atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
        hr = atscLocator.put_SymbolRate(-1);//atscChannel.SymbolRate);
        hr = atscLocator.put_TSID(-1);//atscChannel.TransportId);

        hr = atscLocator.put_InnerFEC(FECMethod.MethodNotSet);
        hr = atscLocator.put_Modulation(atscChannel.ModulationType);
        hr = _tuneRequest.put_MinorChannel(atscChannel.MinorChannel);
        hr = _tuneRequest.put_Channel(atscChannel.MajorChannel);
        _tuneRequest.put_Locator(locator);

        _currentChannel = channel;
        SubmitTuneRequest(_tuneRequest);


        SetAnalyzerMapping(atscChannel.PmtPid);
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
      return true;
    }
    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns></returns>
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
        SetTimeShiftFileName(fileName);
      }

      RunGraph();
      _graphState = GraphState.TimeShifting;
      Tune(Channel);
      FileAccessHelper.GrantFullControll(fileName);
      return true;
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns></returns>
    public bool StopTimeShifting()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("StopTimeShifting()");
      if (_graphState != GraphState.TimeShifting)
      {
        return true;
      }
      StopGraph();


      _graphState = GraphState.Created;
      return true;
    }
    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">time the recording should start (0=now)</param>
    /// <returns></returns>
    public bool StartRecording(RecordingType recordingType, string fileName, long startTime)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("StartRecording to {0}", fileName);
      if (_graphState != GraphState.TimeShifting)
      {
        throw new TvException("Card must be timeshifting before starting recording");
      }
      _recordingFileName = fileName;

      _graphState = GraphState.Recording;
      StartRecord(fileName, recordingType, ref startTime);
      Log.Log.WriteFile("Started recording on {0}", startTime);
      return true;
    }
    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Get/Set the quality
    /// </summary>
    /// <value></value>
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
    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    /// <value></value>
    public bool SupportsQualityControl
    {
      get
      {
        return false;
      }
    }
    #endregion


    #region epg & scanning
    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public ITVScanning ScanningInterface
    {
      get
      {
        return new ATSCScanning(this);
      }
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }


    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel"></param>
    /// <returns>
    /// true if card can tune to the channel otherwise false
    /// </returns>
    public bool CanTune(IChannel channel)
    {
      if ((channel as ATSCChannel) == null) return false;
      return true;
    }
  }
}
