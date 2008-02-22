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
#define COMPARE_PMT
#if (UseCaptureCardDefinitions)
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
using DShowNET.MPTSWriter;
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
  /// <summary>
  /// Implementation of IGraph for digital TV capture cards using the BDA driver architecture
  /// It handles any DVB-T, DVB-C, DVB-S TV Capture card with BDA drivers
  ///
  /// A graphbuilder object supports one or more TVCapture cards and
  /// contains all the code/logic necessary for
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// -radio
  /// </summary>
  public class DVBGraphBDA : DVBGraphBase
  {

    #region variables
    protected ArrayList _tunerStatistics = new ArrayList();
    #endregion

    #region constructor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pCard">instance of a TVCaptureDevice which contains all details about this card</param>
    public DVBGraphBDA(TVCaptureDevice pCard)
      : base(pCard)
    {
    }

    #endregion

    #region createGraph/DeleteGraph()
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard.
    /// This graph can be a DVB-T, DVB-C or DVB-S graph
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
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
        Log.Info("DVBGraphBDA:CreateGraph(). ");

        //no card defined? then we cannot build a graph
        if (_card == null)
        {
          _lastError = "No card setup";
          Log.Error("DVBGraphBDA:card is not defined");
          return false;
        }

        //check if definition contains a tv filter graph
        if ((_card.Graph == null) || (_card.Graph.TvFilterDefinitions == null))
        {
          _lastError = "definitions dont contain filters";
          Log.Error("DVBGraphBDA:FAILED card does not contain filters?");
          return false;
        }

        //check if definition contains <connections> for the tv filter graph
        if ((_card.Graph == null) || (_card.Graph.TvConnectionDefinitions == null))
        {
          _lastError = "definitions dont contain tv filters";
          Log.Error("DVBGraphBDA:FAILED card does not contain connections for tv?");
          return false;
        }

        //create new instance of VMR9 helper utility
        _vmr9 = new VMR9Util();

        // Make a new filter graph
        Log.Info("DVBGraphBDA:create new filter graph (IGraphBuilder)");
        _graphBuilder = (IGraphBuilder)new FilterGraph();

        // Get the Capture Graph Builder
        _captureGraphBuilderInterface = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

        //Log.WriteFile(LogType.Log,"DVBGraphBDA:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
        int hr = _captureGraphBuilderInterface.SetFiltergraph(_graphBuilder);
        if (hr < 0)
        {
          Log.Error("DVBGraphBDA:FAILED link :0x{0:X}", hr);
          return false;
        }
        //Log.WriteFile(LogType.Log,"DVBGraphBDA:Add graph to ROT table");
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

        //dont use samplegrabber in configuration.exe
        _filterSampleGrabber = null;
        _sampleInterface = null;
        //TESTTEST: DONT USE GRABBER AT ALL
        /*
                if (GUIGraphicsContext.DX9Device != null)
                {
                  _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
                  _sampleInterface = (ISampleGrabber)_filterSampleGrabber;
                  _graphBuilder.AddFilter(_filterSampleGrabber, "Sample Grabber");
                }
        */
        // Loop through configured filters for this card, bind them and add them to the graph
        // Note that while adding filters to a graph, some connections may already be created...
        Log.Info("DVBGraphBDA: Adding configured filters...");
        foreach (FilterDefinition dsFilter in _card.Graph.TvFilterDefinitions)
        {
          string catName = dsFilter.Category;
          Log.Info("DVBGraphBDA:  Adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
          if (dsFilter.MonikerDisplayName == string.Empty)
          {
            _lastError = String.Format("Cannot add filter {0}",dsFilter.FriendlyName);
            Log.Error("DVBGraphBDA:  no moniker found for filter:{0}", dsFilter.FriendlyName);
            return false;
          }
          dsFilter.DSFilter = Marshal.BindToMoniker(dsFilter.MonikerDisplayName) as IBaseFilter;
          hr = _graphBuilder.AddFilter(dsFilter.DSFilter, dsFilter.FriendlyName);
          if (hr == 0)
          {
            Log.Info("DVBGraphBDA:  Added filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
          }
          else
          {
            _lastError = String.Format("Cannot add filter {0}", dsFilter.FriendlyName);
            Log.Error("DVBGraphBDA:  Error! Failed adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
            Log.Error("DVBGraphBDA:  Error! Result code = 0x{0:X}", hr);
            return false;
          }

          // Support the "legacy" member variables. This could be done different using properties
          // through which the filters are accessable. More implementation independent...
          if (dsFilter.Category == "networkprovider")
          {
            _filterNetworkProvider = dsFilter.DSFilter;
            // Initialise Tuning Space (using the setupTuningSpace function)
            if (!setupTuningSpace())
            {
              _lastError = String.Format("Failed to setup the tuning space");
              Log.Error("DVBGraphBDA:CreateGraph() FAILED couldnt create tuning space");
              return false;
            }
          }
          if (dsFilter.Category == "tunerdevice") _filterTunerDevice = dsFilter.DSFilter;
          if (dsFilter.Category == "capture") _filterCaptureDevice = dsFilter.DSFilter;
        }//foreach (string catName in _card.TvFilterDefinitions.Keys)

        Log.Info("DVBGraphBDA: Adding configured filters...DONE");

        //no network provider specified? then we cannot build the graph
        if (_filterNetworkProvider == null)
        {
          _lastError = String.Format("No network provider filter present");
          Log.Error("DVBGraphBDA:CreateGraph() FAILED networkprovider filter not found");
          return false;
        }

        //no capture device specified? then we cannot build the graph
        if (_filterCaptureDevice == null)
        {
          _lastError = String.Format("No capture filter present");
          Log.Error("DVBGraphBDA:CreateGraph() FAILED capture filter not found");
        }

        FilterDefinition sourceFilter;
        FilterDefinition sinkFilter;
        IPin sourcePin = null;
        IPin sinkPin = null;

        // Create pin connections. These connections are also specified in the definitions file.
        // Note that some connections might fail due to the fact that the connection is already made,
        // probably during the addition of filters to the graph (checked with GraphEdit...)
        //
        // Pin connections can be defined in two ways:
        // 1. Using the name of the pin.
        //		This method does work, but might be language dependent, meaning the connection attempt
        //		will fail because the pin cannot be found...
        // 2.	Using the 0-based index number of the input or output pin.
        //		This method is save. It simply tells to connect output pin #0 to input pin #1 for example.
        //
        // The code assumes method 1 is used. If that fails, method 2 is tried...

        Log.Info("DVBGraphBDA: Adding configured pin connections...");
        for (int i = 0; i < _card.Graph.TvConnectionDefinitions.Count; i++)
        {
          //get the source filter for the connection
          sourceFilter = _card.GetTvFilterDefinition(((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourceCategory);
          if (sourceFilter == null)
          {
            Log.Error("DVBGraphBDA:FAILED Cannot find source filter for connection:{0}", i);
            continue;
          }

          //get the destination/sink filter for the connection
          sinkFilter = _card.GetTvFilterDefinition(((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkCategory);
          if (sinkFilter == null)
          {
            Log.Error("DVBGraphBDA:FAILED Cannot find sink filter for connection:{0}", i);
            continue;
          }

          Log.Info("DVBGraphBDA:  Connecting <{0}>:{1} with <{2}>:{3}",
            sourceFilter.FriendlyName, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName,
           sinkFilter.FriendlyName, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName);

          //find the pin of the source filter
          sourcePin = DirectShowUtil.FindPin(sourceFilter.DSFilter, PinDirection.Output, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName);
          if (sourcePin == null)
          {
            String strPinName = ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName;
            if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
            {
              sourcePin = DsFindPin.ByDirection(sourceFilter.DSFilter, PinDirection.Output, Convert.ToInt32(strPinName));
              if (sourcePin == null)
                Log.Error("DVBGraphBDA:FAILED   Unable to find sourcePin: <{0}>", strPinName);
              else
                Log.Info("DVBGraphBDA:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
            }
          }
          else
            Log.Info("DVBGraphBDA:   Found sourcePin: <{0}> ", ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SourcePinName);

          //find the pin of the sink filter
          sinkPin = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName);
          if (sinkPin == null)
          {
            String strPinName = ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName;
            if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
            {
              sinkPin = DsFindPin.ByDirection(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
              if (sinkPin == null)
                Log.Error("DVBGraphBDA:   Unable to find sinkPin: <{0}>", strPinName);
              else
                Log.Info("DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
            }
          }
          else
            Log.Info("DVBGraphBDA:   Found sinkPin: <{0}> ", ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName);

          //if we have both pins
          if (sourcePin != null && sinkPin != null)
          {
            // then connect them
            IPin conPin;
            hr = sourcePin.ConnectedTo(out conPin);
            if (hr != 0)
              hr = _graphBuilder.Connect(sourcePin, sinkPin);
            if (hr == 0)
              Log.Info("DVBGraphBDA:   Pins connected...");

            // Give warning and release pin...
            if (conPin != null)
            {
              Log.Info("DVBGraphBDA:   (Pin was already connected...)");
              DirectShowUtil.ReleaseComObject(conPin as Object);
              conPin = null;
              hr = 0;
            }
          }

          //log if connection failed
          //if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
          //	hr=1;
          if (hr != 0)
          {
            Log.Info("DVBGraphBDA:FAILED   unable to connect pins:0x{0:X}", hr);
            if (sourceFilter.Category == "tunerdevice" && sinkFilter.Category == "capture")
            {
              Log.Info("DVBGraphBDA:   try other instances");
              if (sinkPin != null)
                DirectShowUtil.ReleaseComObject(sinkPin);
              sinkPin = null;
              if (sinkFilter.DSFilter != null)
              {
                _graphBuilder.RemoveFilter(sinkFilter.DSFilter);
                DirectShowUtil.ReleaseComObject(sinkFilter.DSFilter);
              }
              sinkFilter.DSFilter = null;
              _filterCaptureDevice = null;

              foreach (string key in AvailableFilters.Filters.Keys)
              {
                Filter filter;
                List<Filter> al = AvailableFilters.Filters[key] ;
                filter = (Filter)al[0];
                if (filter.Name.Equals(sinkFilter.FriendlyName))
                {
                  Log.Info("DVBGraphBDA:   found {0} instances", al.Count);
                  for (int filterInstance = 0; filterInstance < al.Count; ++filterInstance)
                  {
                    filter = (Filter)al[filterInstance];
                    Log.Info("DVBGraphBDA:   try:{0}", filter.MonikerString);
                    sinkFilter.MonikerDisplayName = filter.MonikerString;
                    sinkFilter.DSFilter = Marshal.BindToMoniker(sinkFilter.MonikerDisplayName) as IBaseFilter;
                    hr = _graphBuilder.AddFilter(sinkFilter.DSFilter, sinkFilter.FriendlyName);
                    //find the pin of the sink filter
                    sinkPin = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName);
                    if (sinkPin == null)
                    {
                      String strPinName = ((ConnectionDefinition)_card.Graph.TvConnectionDefinitions[i]).SinkPinName;
                      if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
                      {
                        sinkPin = DsFindPin.ByDirection(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
                        if (sinkPin == null)
                          Log.Error("DVBGraphBDA:FAILED   Unable to find sinkPin: <{0}>", strPinName);
                        else
                          Log.Info("DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
                      }
                    }
                    if (sinkPin != null)
                    {
                      hr = _graphBuilder.Connect(sourcePin, sinkPin);
                      if (hr == 0)
                      {
                        Log.Info("DVBGraphBDA:   Pins connected...");
                        _filterCaptureDevice = sinkFilter.DSFilter;
                        break;
                      }
                      else
                      {
                        Log.Info("DVBGraphBDA:FAILED   cannot connect pins:0x{0:X}", hr);
                        if (sinkPin != null)
                          DirectShowUtil.ReleaseComObject(sinkPin);
                        sinkPin = null;
                        if (sinkFilter.DSFilter != null)
                        {
                          _graphBuilder.RemoveFilter(sinkFilter.DSFilter);
                          DirectShowUtil.ReleaseComObject(sinkFilter.DSFilter);
                          sinkFilter.DSFilter = null;
                        }
                      }
                    }
                  }//for (int filterInstance=0; filterInstance < al.Count;++filterInstance)
                }//if (filter.Name.Equals(sinkFilter.FriendlyName))
              }//foreach (string key in AvailableFilters.Filters.Keys)
            }//if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
          }//if (hr != 0)
        }//for (int i = 0; i < _card.TvConnectionDefinitions.Count; i++)
        //Log.WriteFile(LogType.Log,"DVBGraphBDA: Adding configured pin connections...DONE");


        if (sinkPin != null)
          DirectShowUtil.ReleaseComObject(sinkPin);
        sinkPin = null;

        if (sourcePin != null)
          DirectShowUtil.ReleaseComObject(sourcePin);
        sourcePin = null;

        // Find out which filter & pin is used as the interface to the rest of the graph.
        // The configuration defines the filter, including the Video, Audio and Mpeg2 pins where applicable
        // We only use the filter, as the software will find the correct pin for now...
        // This should be changed in the future, to allow custom graph endings (mux/no mux) using the
        // video and audio pins to connect to the rest of the graph (SBE, overlay etc.)
        // This might be needed by the ATI AIW cards (waiting for ob2 to release...)
        FilterDefinition lastFilter = _card.GetTvFilterDefinition(_card.Graph.TvInterfaceDefinition.FilterCategory);

        // no interface defined or interface not found? then return
        if (lastFilter == null)
        {
          _lastError = String.Format("Last filter not defined");
          Log.Error("DVBGraphBDA:CreateGraph() FAILED interface filter not found");
          return false;
        }

        Log.Info("DVBGraphBDA:CreateGraph() connect interface pin->sample grabber");
        if (GUIGraphicsContext.DX9Device != null && _sampleInterface != null)
        {
          if (!ConnectFilters(ref lastFilter.DSFilter, ref _filterSampleGrabber))
          {
            _lastError = String.Format("Failed to connect filters");
            Log.Error("DVBGraphBDA:Failed to connect Tee/Sink-Sink converter filter->grabber");
            return false;
          }
        }
        //=========================================================================================================
        // add the MPEG-2 Demultiplexer 
        //=========================================================================================================
        // Use CLSID_filterMpeg2Demultiplexer to create the filter
        Log.Info("DVBGraphBDA:CreateGraph() create MPEG2-Demultiplexer");
        _filterMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
        if (_filterMpeg2Demultiplexer == null)
        {
          _lastError = String.Format("Failed to add Mpeg2 Demultiplexer filter");
          Log.Error("DVBGraphBDA:Failed to create Mpeg2 Demultiplexer");
          return false;
        }

        // Add the Demux to the graph
        Log.Info("DVBGraphBDA:CreateGraph() add mpeg2 demuxer to graph");
        _graphBuilder.AddFilter(_filterMpeg2Demultiplexer, "MPEG-2 Demultiplexer");

        //=========================================================================================================
        // add the TIF 
        //=========================================================================================================

        object tmpObject;
        if (!findNamedFilter(FilterCategories.KSCATEGORY_BDA_TRANSPORT_INFORMATION, "BDA MPEG2 Transport Information Filter", out tmpObject))
        {
          _lastError = String.Format("Failed to add BDA MPEG2 Transport Information Filter");
          Log.Error("DVBGraphBDA:CreateGraph() FAILED Failed to find BDA MPEG2 Transport Information Filter");
          return false;
        }
        _filterTIF = (IBaseFilter)tmpObject;
        tmpObject = null;
        if (_filterTIF == null)
        {
          _lastError = String.Format("Failed to add BDA MPEG2 Transport Information Filter");
          Log.Error("DVBGraphBDA:CreateGraph() FAILED BDA MPEG2 Transport Information Filter is null");
          return false;
        }
        _graphBuilder.AddFilter(_filterTIF, "BDA MPEG2 Transport Information Filter");

        /*
                //=========================================================================================================
                //used for TS timeShifting
                if (UseTsTimeShifting)
                {
                  _filterTsMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
                  if (_filterTsMpeg2Demultiplexer == null)
                  {
                    Log.Error("DVBGraphBDA:Failed to create TS Mpeg2 Demultiplexer");
                    return false;
                  }
                  _graphBuilder.AddFilter(_filterTsMpeg2Demultiplexer, "TS MPEG-2 Demultiplexer");

                  _filterInfTee = (IBaseFilter)new InfTee();
                  _graphBuilder.AddFilter(_filterInfTee, "InfTee");
                  ConnectFilters(ref _filterCaptureDevice, ref _filterInfTee);
                  lastFilter.DSFilter = _filterInfTee;

                  //create ts timeshifting pin
                  IMpeg2Demultiplexer tsdemuxer = _filterTsMpeg2Demultiplexer as IMpeg2Demultiplexer;
                  AMMediaType mtTS = new AMMediaType();
                  mtTS.majorType = MediaType.Stream;
                  mtTS.subType = MediaSubType.Mpeg2Transport;
                  mtTS.formatType = FormatType.None;
                  hr = tsdemuxer.CreateOutputPin(mtTS, "TS", out _pinDemuxerTS);
                }
        */
        //=========================================================================================================
        if (GUIGraphicsContext.DX9Device != null && _sampleInterface != null)
        {
          Log.Info("DVBGraphBDA:CreateGraph() connect grabber->demuxer");
          if (!ConnectFilters(ref _filterSampleGrabber, ref _filterMpeg2Demultiplexer))
          {
            _lastError = String.Format("Failed to connect Sample grabber->Mpeg2 Demultiplexer");
            Log.Error("DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
            return false;
          }
        }
        else
        {
          Log.Info("DVBGraphBDA:CreateGraph() connect capture->demuxer");
          if (!ConnectFilters(ref lastFilter.DSFilter, ref _filterMpeg2Demultiplexer))
          {
            _lastError = String.Format("Failed to Capture filter->MPEG2 demultiplexer");
            Log.Error("DVBGraphBDA:Failed to connect Capture filter->mpeg2 demultiplexer");
            return false;
          }
        }

        /*
                //=========================================================================================================
                //used for TS timeShifting
                if (UseTsTimeShifting)
                {
                  ConnectFilters(ref _filterInfTee, ref _filterTsMpeg2Demultiplexer);
                }
        */
        //=========================================================================================================

        //        Log.Info("DVBGraphBDA:CreateGraph() connect demuxer->tif");
        if (!ConnectFilters(ref _filterMpeg2Demultiplexer, ref _filterTIF))
        {
          _lastError = String.Format("Failed to MPEG2 demultiplexer->TIF filter");
          Log.Error("DVBGraphBDA:Failed to connect mpeg2 demultiplexer->TIF");
          //return false;
        }
        IMpeg2Demultiplexer demuxer = _filterMpeg2Demultiplexer as IMpeg2Demultiplexer;

        Log.Info("DVBGraphBDA:CreateGraph() add stream analyzer");
        _filterDvbAnalyzer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(ClassId.MPStreamAnalyzer, true));
        _analyzerInterface = (IStreamAnalyzer)_filterDvbAnalyzer;
        _epgGrabberInterface = _filterDvbAnalyzer as IEPGGrabber;
        _mhwGrabberInterface = _filterDvbAnalyzer as IMHWGrabber;
        _atscGrabberInterface = _filterDvbAnalyzer as IATSCGrabber;
        hr = _graphBuilder.AddFilter(_filterDvbAnalyzer, "Stream-Analyzer");
        if (hr != 0)
        {
          _lastError = String.Format("Failed to add MPEG2 sections and tables filter");
          Log.Error("DVBGraphBDA: FAILED to add SectionsFilter 0x{0:X}", hr);
          return false;
        }

        //        Log.Info("DVBGraphBDA:CreateGraph() find audio/video pins");
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
                  //Log.Info("DVBGraphBDA: found audio pin");
                  _pinDemuxerAudio = pin[0];
                  break;
                }
                if (pinMediaType[0].majorType == MediaType.Video)
                {
                  //Log.Info("DVBGraphBDA: found video pin");
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
                    //Log.Info("DVBGraphBDA:connect mpeg2 demux->stream analyzer");
                    hr = _graphBuilder.Connect(pin[0], pinAnalyzerIn);
                    if (hr == 0)
                    {
                      connected = true;
                      //Log.Info("DVBGraphBDA:connected mpeg2 demux->stream analyzer");
                    }
                    else
                    {
                      Log.Error("DVBGraphBDA:FAILED to connect mpeg2 demux->stream analyzer");
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
          _lastError = String.Format("No video output pin found");
          Log.Error("DVBGraphBDA:Failed to get pin '{0}' (video out) from MPEG-2 Demultiplexer", _pinDemuxerVideo);
          return false;
        }
        if (_pinDemuxerAudio == null)
        {
          //audio pin not found
          _lastError = String.Format("No audio output pin found");
          Log.Error("DVBGraphBDA:Failed to get pin '{0}' (audio out)  from MPEG-2 Demultiplexer", _pinDemuxerAudio);
          return false;
        }

        //Log.Info("DVBGraphBDA:CreateGraph() create ac3/mpg1 pins");
        if (demuxer != null)
        {
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
          hr = demuxer.CreateOutputPin(mpegAudioOut, "audio", out _pinDemuxerAudio);
          if (hr != 0)
          {
            _lastError = String.Format("failed to add mpg1 audio pin");
            Log.Error("DVBGraphBDA: FAILED to create audio output pin on demuxer");
            return false;
          }

          hr = demuxer.CreateOutputPin(mpegVideoOut/*vidOut*/, "video", out _pinDemuxerVideo);
          if (hr != 0)
          {
            _lastError = String.Format("failed to add mpg2 video pin");
            Log.Error("DVBGraphBDA: FAILED to create video output pin on demuxer");
            return false;
          }

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
            _lastError = String.Format("failed to add ac3 audio pin");
            Log.Error("DVBGraphBDA:FAILED to create AC3 pin:0x{0:X}", hr);
          }

          //Log.Info("DVBGraphBDA: create mpg1 audio pin");
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
            _lastError = String.Format("failed to add mpg1 audio pin");
            Log.Error("DVBGraphBDA:FAILED to create MPG1 pin:0x{0:X}", hr);
          }

          //Log.Info("DVBGraphBDA: create mpg4 video pin");
          AMMediaType mediaMPG4 = new AMMediaType();
          mediaMPG4.majorType = MediaType.Video;
          mediaMPG4.subType = new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b);
          mediaMPG4.sampleSize = 0;
          mediaMPG4.temporalCompression = false;
          mediaMPG4.fixedSizeSamples = false;
          mediaMPG4.unkPtr = IntPtr.Zero;
          mediaMPG4.formatType = FormatType.Mpeg2Video;
          mediaMPG4.formatSize = Mpeg2ProgramVideo.GetLength(0);
          mediaMPG4.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaMPG4.formatSize);
          System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mediaMPG4.formatPtr, mediaMPG4.formatSize);

          hr = demuxer.CreateOutputPin(mediaMPG4, "MPG4", out _pinDemuxerVideoMPEG4);
          if (hr != 0 || _pinDemuxerVideoMPEG4 == null)
          {
            _lastError = String.Format("failed to add mpg4 video pin");
            Log.Error("DVBGraphBDA:FAILED to create MPG4 pin:0x{0:X}", hr);
          }



          //create EPG pins
          //Log.Info("DVBGraphBDA:Create EPG pin");
          AMMediaType mtEPG = new AMMediaType();
          mtEPG.majorType = MEDIATYPE_MPEG2_SECTIONS;
          mtEPG.subType = MediaSubType.None;
          mtEPG.formatType = FormatType.None;


          hr = demuxer.CreateOutputPin(mtEPG, "EPG", out _pinDemuxerEPG);
          if (hr != 0 || _pinDemuxerEPG == null)
          {
            _lastError = String.Format("failed to add EPG pin");
            Log.Error("DVBGraphBDA:FAILED to create EPG pin:0x{0:X}", hr);
            return false;
          }
          hr = demuxer.CreateOutputPin(mtEPG, "MHW1", out _pinDemuxerMHWd2);
          if (hr != 0 || _pinDemuxerMHWd2 == null)
          {
            _lastError = String.Format("failed to add MHW1 pin");
            Log.Error("DVBGraphBDA:FAILED to create MHW1 pin:0x{0:X}", hr);
            return false;
          }
          hr = demuxer.CreateOutputPin(mtEPG, "MHW2", out _pinDemuxerMHWd3);
          if (hr != 0 || _pinDemuxerMHWd3 == null)
          {
            _lastError = String.Format("failed to add MHW2 pin");
            Log.Error("DVBGraphBDA:FAILED to create MHW2 pin:0x{0:X}", hr);
            return false;
          }

          //Log.Info("DVBGraphBDA:Get EPGs pin of analyzer");
          IPin pinMHW1In = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 1);
          if (pinMHW1In == null)
          {
            _lastError = String.Format("failed to connect analyzer filter");
            Log.Error("DVBGraphBDA:FAILED to get MHW1 pin on MSPA");
            return false;
          }
          IPin pinMHW2In = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 2);
          if (pinMHW2In == null)
          {
            _lastError = String.Format("failed to connect analyzer filter");
            Log.Error("DVBGraphBDA:FAILED to get MHW2 pin on MSPA");
            return false;
          }
          IPin pinEPGIn = DsFindPin.ByDirection(_filterDvbAnalyzer, PinDirection.Input, 3);
          if (pinEPGIn == null)
          {
            _lastError = String.Format("failed to connect analyzer filter");
            Log.Error("DVBGraphBDA:FAILED to get EPG pin on MSPA");
            return false;
          }

          //Log.Info("DVBGraphBDA:Connect epg pins");
          hr = _graphBuilder.Connect(_pinDemuxerEPG, pinEPGIn);
          if (hr != 0)
          {
            _lastError = String.Format("failed to connect analyzer filter");
            Log.Error("DVBGraphBDA:FAILED to connect EPG pin:0x{0:X}", hr);
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerMHWd2, pinMHW1In);
          if (hr != 0)
          {
            _lastError = String.Format("failed to connect analyzer filter");
            Log.Error("DVBGraphBDA:FAILED to connect MHW1 pin:0x{0:X}", hr);
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerMHWd3, pinMHW2In);
          if (hr != 0)
          {
            _lastError = String.Format("failed to connect analyzer filter");
            Log.Error("DVBGraphBDA:FAILED to connect MHW2 pin:0x{0:X}", hr);
            return false;
          }
          //Log.Info("DVBGraphBDA:Demuxer is setup");

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
              _lastError = String.Format("failed to add ttx pin");
              Log.Error("DVBGraphBDA:FAILED to create ttx pin:0x{0:X}", hr);
              return false;
            }

            _filterSampleGrabber = (IBaseFilter)new SampleGrabber();
            _sampleInterface = (ISampleGrabber)_filterSampleGrabber;
            _graphBuilder.AddFilter(_filterSampleGrabber, "Sample Grabber");

            IPin pinIn = DsFindPin.ByDirection(_filterSampleGrabber, PinDirection.Input, 0);
            if (pinIn == null)
            {
              _lastError = String.Format("failed to sample grabber");
              Log.Error("DVBGraphBDA:unable to find sample grabber input:0x{0:X}", hr);
              return false;
            }
            hr = _graphBuilder.Connect(_pinTeletext, pinIn);
            if (hr != 0)
            {
              _lastError = String.Format("failed to sample grabber");
              Log.Error("DVBGraphBDA:FAILED to connect demux->sample grabber:0x{0:X}", hr);
              return false;
            }
            if (pinIn != null)
            {
              DirectShowUtil.ReleaseComObject(pinIn);
              pinIn = null;
            }
          }
        }
        else
        {
          _lastError = String.Format("Unable to get IMPEG2Demultiplexer");
          Log.Error("DVBGraphBDA:mapped IMPEG2Demultiplexer not found");
        }
        //=========================================================================================================
        // Create the streambuffer engine and mpeg2 video analyzer components since we need them for
        // recording and timeshifting
        //=========================================================================================================
        m_StreamBufferSink = new StreamBufferSink();
        m_mpeg2Analyzer = new VideoAnalyzer();
        m_IStreamBufferSink = (IStreamBufferSink3)m_StreamBufferSink;
        _graphState = State.Created;

        GetTunerSignalStatistics();
        if (_tunerStatistics.Count == 0)
        {
          _lastError = String.Format("Unable to get tuner statistics");
          Log.Info("DVBGraphBDA:Failed to get tuner statistics");
        }
        Log.Info("DVBGraphBDA:got {0} tuner statistics", _tunerStatistics.Count);


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
        _cardProperties = new VideoCaptureProperties(_filterTunerDevice);
        return true;
      }
      catch (Exception ex)
      {
        _lastError = String.Format("Unable to create graph");
        Log.Error(ex);
        return false;
      }
    }//public bool CreateGraph()

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// Frees any (unmanaged) resources
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public override void DeleteGraph()
    {
      try
      {
        if (_graphState < State.Created)
          return;
        int hr;
        _currentTuningObject = null;
        Log.Info("DVBGraphBDA:DeleteGraph(). ac3=false");
        _isUsingAC3 = false;

        Log.Info("DVBGraphBDA:DeleteGraph()");
        StopRecording();
        StopTimeShifting();
        StopViewing();
        //Log.WriteFile(LogType.Log,"DVBGraphBDA: free tuner interfaces");
        _cardProperties.Dispose();
        _cardProperties = null;

        // to clear buffers for epg and teletext
        if (_streamDemuxer != null)
        {
          _streamDemuxer.GrabTeletext(false);
          _streamDemuxer.SetChannelData(0, 0, 0, 0, 0, "", 0, 0);
        }

        if (_tunerStatistics != null)
        {
          for (int i = 0; i < _tunerStatistics.Count; i++)
          {
            if (_tunerStatistics[i] != null)
            {
              while ((hr = DirectShowUtil.ReleaseComObject(_tunerStatistics[i])) > 0) ;
              if (hr != 0)
                Log.Info("DVBGraphBDA:ReleaseComObject(tunerstat):{0}", hr);
              _tunerStatistics[i] = null;
            }
          }
          _tunerStatistics.Clear();
        }
        //Log.Info("DVBGraphBDA:stop graph");
        if (_mediaControl != null) _mediaControl.Stop();
        //Log.Info("DVBGraphBDA:graph stopped");

        if (_vmr9 != null)
        {
          //Log.Info("DVBGraphBDA:remove vmr9");
          _vmr9.Dispose();
          _vmr9 = null;
        }


        if (m_recorderId >= 0)
        {
          DvrMsStop(m_recorderId);
          m_recorderId = -1;
        }

        _isGraphRunning = false;
        _mediaControl = null;
        _basicVideoInterFace = null;
        _analyzerInterface = null;
        _epgGrabberInterface = null;
        _mhwGrabberInterface = null;
        _atscGrabberInterface = null;
        _cardProperties = null;
        _epgGrabber = null;
        _sampleInterface = null;



        //Log.Info("free pins");
        if (_pinAC3Out != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinAC3Out)) > 0) ;
          _pinAC3Out = null;
        }
        if (_pinDemuxerVideoMPEG4 != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerVideoMPEG4)) > 0) ;
          _pinDemuxerVideoMPEG4 = null;
        }
        if (_filterInfTee != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_filterInfTee)) > 0) ;
          _filterInfTee = null;
        }
        if (_filterTsMpeg2Demultiplexer != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_filterTsMpeg2Demultiplexer)) > 0) ;
          _filterTsMpeg2Demultiplexer = null;
        }
        if (_pinDemuxerTS != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerTS)) > 0) ;
          _pinDemuxerTS = null;
        }
        if (_pinDemuxerAudio != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerAudio)) > 0) ;
          _pinDemuxerAudio = null;
        }
        if (_pinDemuxerEPG != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerEPG)) > 0) ;
          _pinDemuxerEPG = null;
        }
        if (_pinDemuxerMHWd2 != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerMHWd2)) > 0) ;
          _pinDemuxerMHWd2 = null;
        }
        if (_pinDemuxerMHWd3 != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerMHWd3)) > 0) ;
          _pinDemuxerMHWd3 = null;
        }
        if (_pinDemuxerSections != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerSections)) > 0) ;
          _pinDemuxerSections = null;
        }
        if (_pinDemuxerVideo != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinDemuxerVideo)) > 0) ;
          _pinDemuxerVideo = null;
        }
        if (_pinMPG1Out != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinMPG1Out)) > 0) ;
          _pinMPG1Out = null;
        }
        if (_pinTeletext != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_pinTeletext)) > 0) ;
          _pinTeletext = null;
        }
        if (_filterDvbAnalyzer != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterDvbAnalyzer)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterDvbAnalyzer):{0}", hr);
          _filterDvbAnalyzer = null;
        }
        if (_filterCaptureDevice != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterCaptureDevice)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterCaptureDevice):{0}", hr);
          _filterCaptureDevice = null;
        }
        if (_filterMpeg2Demultiplexer != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterMpeg2Demultiplexer)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterMpeg2Demultiplexer):{0}", hr);
          _filterMpeg2Demultiplexer = null;
        }
        if (_filterNetworkProvider != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterNetworkProvider)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterNetworkProvider):{0}", hr);
          _filterNetworkProvider = null;
        }
        if (_filterSampleGrabber != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterSampleGrabber)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterSampleGrabber):{0}", hr);
          _filterSampleGrabber = null;
        }
        if (_filterSmartTee != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterSmartTee)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterSmartTee):{0}", hr);
          _filterSmartTee = null;
        }
        if (_filterTIF != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterTIF)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterTIF):{0}", hr);
          _filterTIF = null;
        }
        if (_filterTunerDevice != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(_filterTunerDevice)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(_filterTunerDevice):{0}", hr);
          _filterTunerDevice = null;
        }

        if (m_mpeg2Analyzer != null)
        {
          //Log.Info("free dvbanalyzer");
          while ((hr = DirectShowUtil.ReleaseComObject(m_mpeg2Analyzer)) > 0) ;
          if (hr != 0)
            Log.Info("ReleaseComObject(m_mpeg2Analyzer):{0}", hr);
          m_mpeg2Analyzer = null;
        }





        if (_videoWindowInterface != null)
        {
          //Log.Info("DVBGraphBDA:hide window");
          //Log.WriteFile(LogType.Log,"DVBGraphBDA: hide video window");
          _videoWindowInterface.put_Visible(OABool.False);
          //_videoWindowInterface.put_Owner(IntPtr.Zero);
          _videoWindowInterface = null;
        }

        if (m_IStreamBufferConfig != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(m_IStreamBufferConfig)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphBDA:ReleaseComObject(m_IStreamBufferConfig):{0}", hr);
          m_IStreamBufferConfig = null;
        }

        if (m_IStreamBufferSink != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(m_IStreamBufferSink)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphBDA:ReleaseComObject(m_IStreamBufferSink):{0}", hr);
          m_IStreamBufferSink = null;
        }

        if (m_StreamBufferSink != null)
        {
          //Log.Info("DVBGraphBDA:free streambuffersink");
          while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferSink)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphBDA:ReleaseComObject(m_StreamBufferSink):{0}", hr);
          m_StreamBufferSink = null;
        }


        if (m_StreamBufferConfig != null)
        {
          //Log.Info("DVBGraphBDA:free streambufferconfig");
          while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferConfig)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphBDA:ReleaseComObject(m_StreamBufferConfig):{0}", hr);
          m_StreamBufferConfig = null;
        }
        //Log.WriteFile(LogType.Log,"DVBGraphBDA: remove filters");


        //Log.WriteFile(LogType.Log,"DVBGraphBDA: clean filters");
        if ((_card != null) && (_card.Graph != null) && (_card.Graph.TvFilterDefinitions != null))
        {
          foreach (FilterDefinition dsFilter in _card.Graph.TvFilterDefinitions)
          {
            string strfileName = dsFilter.Category;
            if (dsFilter.DSFilter != null)
            {
              while ((hr = DirectShowUtil.ReleaseComObject(dsFilter.DSFilter)) > 0) ;
            }
            dsFilter.DSFilter = null;
          }
        }
        if (_graphBuilder != null)
          DirectShowUtil.RemoveFilters(_graphBuilder);


        //Log.Info("DVBGraphBDA:free remove graph");
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        //Log.WriteFile(LogType.Log,"DVBGraphBDA: remove graph");
        if (_captureGraphBuilderInterface != null)
        {
          //Log.Info("DVBGraphBDA:free remove capturegraphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_captureGraphBuilderInterface)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphBDA:ReleaseComObject(_captureGraphBuilderInterface):{0}", hr);
          _captureGraphBuilderInterface = null;
        }

        if (_graphBuilder != null)
        {
          //Log.Info("DVBGraphBDA:free graphbuilder");
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
          if (hr != 0) Log.Info("DVBGraphBDA:ReleaseComObject(_graphBuilder):{0}", hr);
          _graphBuilder = null;
        }

#if DUMP
				if (fileout!=null)
				{
					fileout.Close();
					fileout=null;
				}
#endif

        //  GC.Collect(); GC.Collect(); GC.Collect();
        _graphState = State.None;
        //Log.WriteFile(LogType.Log,"DVBGraphBDA: delete graph done");
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }//public void DeleteGraph()

    #endregion

    protected override void UpdateSignalPresent()
    {
      //if we dont have an IBDA_SignalStatistics interface then return
      if (_tunerStatistics == null)
      {
        Log.Error("DVBGraphBDA:UpdateSignalPresent() no tuner stat interfaces");
        return;
      }
      if (_tunerStatistics.Count == 0)
      {
        Log.Error("DVBGraphBDA:UpdateSignalPresent() no tuner stat interfaces");
        return;
      }
      bool isTunerLocked = false;
      bool isSignalPresent = false;
      long signalQuality = 0;
      long signalStrength = 0;

      for (int i = 0; i < _tunerStatistics.Count; i++)
      {
        IBDA_SignalStatistics stat = (IBDA_SignalStatistics)_tunerStatistics[i];
        bool isLocked = false;
        bool isPresent = false;
        int quality = 0;
        int strength = 0;
        try
        {
          //is the tuner locked?
          stat.get_SignalLocked(out isLocked);
          isTunerLocked |= isLocked;
        }
        catch (COMException)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() locked :{0}", ex.Message);
        }
        catch (Exception)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() locked :{0}", ex.Message);
        }
        try
        {
          //is a signal present?
          stat.get_SignalPresent(out isPresent);
          isSignalPresent |= isPresent;
        }
        catch (COMException)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() present :{0}", ex.Message);
        }
        catch (Exception)
        {
          // Log.Error("DVBGraphBDA:UpdateSignalPresent() present :{0}", ex.Message);
        }
        try
        {
          //is a signal quality ok?
          stat.get_SignalQuality(out quality); //1-100
          if (quality > 0) signalQuality += quality;
        }
        catch (COMException)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() quality :{0}", ex.Message);
        }
        catch (Exception)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() quality :{0}", ex.Message);
        }
        try
        {
          //is a signal strength ok?
          stat.get_SignalStrength(out strength); //1-100
          if (strength > 0) signalStrength += strength;
        }
        catch (COMException)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() quality :{0}", ex.Message);
        }
        catch (Exception)
        {
          //Log.Error("DVBGraphBDA:UpdateSignalPresent() quality :{0}", ex.Message);
        }
        //Log.Info("  #{0}  locked:{1} present:{2} quality:{3} strength:{4}", i, isLocked, isPresent, quality, strength);
      }
      if (_tunerStatistics.Count > 0)
      {
        _signalQuality = (int)signalQuality / _tunerStatistics.Count;
        _signalLevel = (int)signalStrength / _tunerStatistics.Count;
      }
      if (isTunerLocked)
        _tunerLocked = true;
      else
        _tunerLocked = false;

      //some devices give different results about signal status
      //on some signalpresent is only true when tuned to a channel
      //on others  signalpresent is true when tuned to a transponder
      //so we just look if any variables returns true

      if (isTunerLocked)
      {
        _signalPresent = true;
      }
      else
      {
        _signalPresent = false;
      }
    }//public bool SignalPresent()

    public override NetworkType Network()
    {
      if (_networkType == NetworkType.Unknown)
      {
          foreach (FilterDefinition dsFilter in _card.Graph.TvFilterDefinitions)
          {
            string catName = dsFilter.Category;
            if (dsFilter.MonikerDisplayName == @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBC Network Provider")
            {
              _networkType = NetworkType.DVBC;
              return _networkType;
            }
            if (dsFilter.MonikerDisplayName == @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBT Network Provider")
            {
              _networkType = NetworkType.DVBT;
              return _networkType;
            }
            if (dsFilter.MonikerDisplayName == @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBS Network Provider")
            {
              _networkType = NetworkType.DVBS;
              return _networkType;
            }
            if (dsFilter.MonikerDisplayName == @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft ATSC Network Provider")
            {
              _networkType = NetworkType.ATSC;
              return _networkType;
            }
          }
      }
      return _networkType;
    }

    protected void GetTunerSignalStatistics()
    {
      //no tuner filter? then return;
      _tunerStatistics = new ArrayList();
      if (_filterTunerDevice == null)
      {
        Log.Error("DVBGraphBDA: could not get IBDA_Topology since no tuner device");
        return;
      }
      //get the IBDA_Topology from the tuner device
      //Log.WriteFile(LogType.Log,"DVBGraphBDA: get IBDA_Topology");
      IBDA_Topology topology = _filterTunerDevice as IBDA_Topology;
      if (topology == null)
      {
        Log.Error("DVBGraphBDA: could not get IBDA_Topology from tuner");
        return;
      }

      //get the NodeTypes from the topology
      //Log.WriteFile(LogType.Log,"DVBGraphBDA: GetNodeTypes");
      int nodeTypeCount = 0;
      int[] nodeTypes = new int[33];
      Guid[] guidInterfaces = new Guid[33];

      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Error("DVBGraphBDA: FAILED could not get node types from tuner:0x{0:X}", hr);
        return;
      }
      if (nodeTypeCount == 0)
      {
        Log.Error("DVBGraphBDA: FAILED could not get any node types");
      }
      Guid GuidIBDA_SignalStatistic = new Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338");
      //for each node type
      //Log.WriteFile(LogType.Log,"DVBGraphBDA: got {0} node types", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object objectNode;
        int numberOfInterfaces = 32;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out numberOfInterfaces, 32, guidInterfaces);
        if (hr != 0)
        {
          Log.Error("DVBGraphBDA: FAILED could not GetNodeInterfaces for node:{0} 0x:{1:X}", i, hr);
        }

        hr = topology.GetControlNode(0, 1, nodeTypes[i], out objectNode);
        if (hr != 0)
        {
          Log.Error("DVBGraphBDA: FAILED could not GetControlNode for node:{0} 0x:{1:X}", i, hr);
          return;
        }

        //and get the final IBDA_SignalStatistics
        for (int iface = 0; iface < numberOfInterfaces; iface++)
        {
          if (guidInterfaces[iface] == GuidIBDA_SignalStatistic)
          {
            //Log.Info("DVBGraphBDA: got IBDA_SignalStatistics on node:{0} interface:{1}", i, iface);
            _tunerStatistics.Add((IBDA_SignalStatistics)objectNode);
          }
        }

      }//for (int i=0; i < nodeTypeCount;++i)
      DirectShowUtil.ReleaseComObject(topology);
      return;
    }//IBDA_SignalStatistics GetTunerSignalStatistics()

    protected IBDA_LNBInfo[] GetBDALNBInfoInterface()
    {
      //no tuner filter? then return;
      if (_filterTunerDevice == null)
        return null;

      //get the IBDA_Topology from the tuner device
      //Log.Info("DVBGraphBDA: get IBDA_Topology");
      IBDA_Topology topology = _filterTunerDevice as IBDA_Topology;
      if (topology == null)
      {
        Log.Error("DVBGraphBDA: could not get IBDA_Topology from tuner");
        return null;
      }

      //get the NodeTypes from the topology
      //Log.Info("DVBGraphBDA: GetNodeTypes");
      int nodeTypeCount = 0;
      int[] nodeTypes = new int[33];
      Guid[] guidInterfaces = new Guid[33];

      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Error("DVBGraphBDA: FAILED could not get node types from tuner");
        return null;
      }
      IBDA_LNBInfo[] signal = new IBDA_LNBInfo[nodeTypeCount];
      //for each node type
      //Log.Info("DVBGraphBDA: got {0} node types", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object objectNode;
        hr = topology.GetControlNode(0, 1, nodeTypes[i], out objectNode);
        if (hr != 0)
        {
          Log.Error("DVBGraphBDA: FAILED could not GetControlNode for node:{0}", hr);
          return null;
        }
        //and get the final IBDA_LNBInfo
        try
        {
          signal[i] = (IBDA_LNBInfo)objectNode;
        }
        catch
        {
          Log.Info("No interface on node {0}", i);
        }
      }//for (int i=0; i < nodeTypeCount;++i)
      DirectShowUtil.ReleaseComObject(topology);
      return signal;
    }//IBDA_LNBInfo[] GetBDALNBInfoInterface()

    protected bool setupTuningSpace()
    {
      //int hr = 0;

      //Log.Info("DVBGraphBDA: setupTuningSpace()");
      if (_filterNetworkProvider == null)
      {
        Log.Error("DVBGraphBDA: FAILED:network provider is null ");
        return false;
      }
      System.Guid classID;
      int hr = _filterNetworkProvider.GetClassID(out classID);
      //			if (hr <=0)
      //			{
      //				Log.Error("DVBGraphBDA: FAILED:cannot get classid of network provider");
      //				return false;
      //			}

      string strClassID = classID.ToString();
      strClassID = strClassID.ToLower();
      switch (strClassID)
      {
        case "0dad2fdd-5fd7-11d3-8f50-00c04f7971e2":
          //Log.WriteFile(LogType.Log,"DVBGraphBDA: Network=ATSC");
          _networkType = NetworkType.ATSC;
          break;
        case "dc0c0fe7-0485-4266-b93f-68fbf80ed834":
          //Log.WriteFile(LogType.Log,"DVBGraphBDA: Network=DVB-C");
          _networkType = NetworkType.DVBC;
          break;
        case "fa4b375a-45b4-4d45-8440-263957b11623":
          //Log.WriteFile(LogType.Log,"DVBGraphBDA: Network=DVB-S");
          _networkType = NetworkType.DVBS;
          break;
        case "216c62df-6d7f-4e9a-8571-05f14edb766a":
          //Log.WriteFile(LogType.Log,"DVBGraphBDA: Network=DVB-T");
          _networkType = NetworkType.DVBT;
          break;
        default:
          Log.Error("DVBGraphBDA: FAILED:unknown network type:{0} ", classID);
          return false;
      }//switch (strClassID) 

      TunerLib.ITuningSpaceContainer TuningSpaceContainer = (TunerLib.ITuningSpaceContainer)Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_SystemTuningSpaces, true));
      if (TuningSpaceContainer == null)
      {
        Log.Error("DVBGraphBDA: Failed to get ITuningSpaceContainer");
        return false;
      }

      TunerLib.ITuningSpaces myTuningSpaces = null;
      string uniqueName = "";
      switch (_networkType)
      {
        case NetworkType.ATSC:
          {
            myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_ATSCTuningSpace);
            //ATSCInputType = "Antenna"; // Need to change to allow cable
            uniqueName = "MediaPortal ATSC";
          } break;
        case NetworkType.DVBC:
          {
            myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBTuningSpace);
            uniqueName = "MediaPortal DVB-C";
          } break;
        case NetworkType.DVBS:
          {
            myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBSTuningSpace);
            uniqueName = "MediaPortal DVB-S";
          } break;
        case NetworkType.DVBT:
          {
            myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBTuningSpace);
            uniqueName = "MediaPortal DVB-T";
          } break;
      }//switch (_networkType) 

      //Log.Info("DVBGraphBDA: check available tuningspaces");
      TunerLib.ITuner myTuner = _filterNetworkProvider as TunerLib.ITuner;

      int Count = 0;
      Count = myTuningSpaces.Count;
      if (Count > 0)
      {
        //Log.Info("DVBGraphBDA: found {0} tuning spaces", Count);
        TunerLib.IEnumTuningSpaces TuneEnum = myTuningSpaces.EnumTuningSpaces;
        if (TuneEnum != null)
        {
          uint ulFetched = 0;
          TunerLib.TuningSpace tuningSpaceFound;
          int counter = 0;
          TuneEnum.Reset();
          for (counter = 0; counter < Count; counter++)
          {
            TuneEnum.Next(1, out tuningSpaceFound, out ulFetched);
            if (ulFetched == 1)
            {
              if (tuningSpaceFound.UniqueName == uniqueName)
              {
                myTuner.TuningSpace = tuningSpaceFound;
                //Log.Info("DVBGraphBDA: used tuningspace:{0} {1} {2}", counter, tuningSpaceFound.UniqueName, tuningSpaceFound.FriendlyName);
                if (myTuningSpaces != null)
                  DirectShowUtil.ReleaseComObject(myTuningSpaces);
                if (TuningSpaceContainer != null)
                  DirectShowUtil.ReleaseComObject(TuningSpaceContainer);
                return true;
              }//if (tuningSpaceFound.UniqueName==uniqueName)
            }//if (ulFetched==1 )
          }//for (counter=0; counter < Count; counter++)
          if (myTuningSpaces != null)
            DirectShowUtil.ReleaseComObject(myTuningSpaces);
        }//if (TuneEnum !=null)
      }//if(Count > 0)

      TunerLib.ITuningSpace TuningSpace;
      //Log.Info("DVBGraphBDA: create new tuningspace");
      switch (_networkType)
      {
        case NetworkType.ATSC:
          {
            TuningSpace = (TunerLib.ITuningSpace)new ATSCTuningSpace();
            TunerLib.IATSCTuningSpace myTuningSpace = (TunerLib.IATSCTuningSpace)TuningSpace;
            myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_ATSCNetworkProvider);
            myTuningSpace.InputType = TunerLib.tagTunerInputType.TunerInputAntenna;
            myTuningSpace.MaxChannel = 10000;
            myTuningSpace.MaxMinorChannel = 1;
            myTuningSpace.MaxPhysicalChannel = 10000;
            myTuningSpace.MinChannel = 1;
            myTuningSpace.MinMinorChannel = 0;
            myTuningSpace.MinPhysicalChannel = 0;
            myTuningSpace.FriendlyName = uniqueName;
            myTuningSpace.UniqueName = uniqueName;

            TunerLib.Locator DefaultLocator = (TunerLib.Locator)new ATSCLocator();
            TunerLib.IATSCLocator myLocator = (TunerLib.IATSCLocator)DefaultLocator;

            myLocator.CarrierFrequency = -1;
            myLocator.InnerFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.InnerFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.Modulation = (TunerLib.ModulationType)ModulationType.ModNotSet;
            myLocator.OuterFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.OuterFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.PhysicalChannel = -1;
            myLocator.SymbolRate = -1;
            myLocator.TSID = -1;

            myTuningSpace.DefaultLocator = DefaultLocator;
            TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
            myTuner.TuningSpace = (TunerLib.TuningSpace)TuningSpace;
          } break;//case NetworkType.ATSC: 

        case NetworkType.DVBC:
          {
            TuningSpace = (TunerLib.ITuningSpace)new DVBTuningSpace();
            TunerLib.IDVBTuningSpace2 myTuningSpace = (TunerLib.IDVBTuningSpace2)TuningSpace;
            myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Cable;
            myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBCNetworkProvider);

            myTuningSpace.FriendlyName = uniqueName;
            myTuningSpace.UniqueName = uniqueName;
            TunerLib.Locator DefaultLocator = (TunerLib.Locator)new DVBCLocator();
            TunerLib.IDVBCLocator myLocator = (TunerLib.IDVBCLocator)DefaultLocator;

            myLocator.CarrierFrequency = -1;
            myLocator.InnerFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.InnerFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.Modulation = (TunerLib.ModulationType)ModulationType.ModNotSet;
            myLocator.OuterFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.OuterFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.SymbolRate = -1;

            myTuningSpace.DefaultLocator = DefaultLocator;
            TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
            myTuner.TuningSpace = (TunerLib.TuningSpace)TuningSpace;
          } break;//case NetworkType.DVBC: 

        case NetworkType.DVBS:
          {
            TuningSpace = (TunerLib.ITuningSpace)new DVBSTuningSpace();
            TunerLib.IDVBSTuningSpace myTuningSpace = (TunerLib.IDVBSTuningSpace)TuningSpace;
            myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Satellite;
            myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBSNetworkProvider);
            myTuningSpace.LNBSwitch = -1;
            myTuningSpace.HighOscillator = -1;
            myTuningSpace.LowOscillator = 11250000;
            myTuningSpace.FriendlyName = uniqueName;
            myTuningSpace.UniqueName = uniqueName;

            TunerLib.Locator DefaultLocator = (TunerLib.Locator)new DVBSLocator();
            TunerLib.IDVBSLocator myLocator = (TunerLib.IDVBSLocator)DefaultLocator;

            myLocator.CarrierFrequency = -1;
            myLocator.InnerFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.InnerFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.OuterFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.OuterFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.Modulation = (TunerLib.ModulationType)ModulationType.ModNotSet;
            myLocator.SymbolRate = -1;
            myLocator.Azimuth = -1;
            myLocator.Elevation = -1;
            myLocator.OrbitalPosition = -1;
            myLocator.SignalPolarisation = (TunerLib.Polarisation)Polarisation.NotSet;
            myLocator.WestPosition = false;

            myTuningSpace.DefaultLocator = DefaultLocator;
            TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
            myTuner.TuningSpace = (TunerLib.TuningSpace)TuningSpace;
          } break;//case NetworkType.DVBS: 

        case NetworkType.DVBT:
          {
            TuningSpace = (TunerLib.ITuningSpace)new DVBTuningSpace();
            TunerLib.IDVBTuningSpace2 myTuningSpace = (TunerLib.IDVBTuningSpace2)TuningSpace;
            myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Terrestrial;
            myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBTNetworkProvider);
            myTuningSpace.FriendlyName = uniqueName;
            myTuningSpace.UniqueName = uniqueName;

            TunerLib.Locator DefaultLocator = (TunerLib.Locator)new DVBTLocator();
            TunerLib.IDVBTLocator myLocator = (TunerLib.IDVBTLocator)DefaultLocator;

            myLocator.CarrierFrequency = -1;
            myLocator.Bandwidth = -1;
            myLocator.Guard = (TunerLib.GuardInterval)GuardInterval.GuardNotSet;
            myLocator.HAlpha = (TunerLib.HierarchyAlpha)HierarchyAlpha.HAlphaNotSet;
            myLocator.InnerFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.InnerFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.LPInnerFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.LPInnerFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.Mode = (TunerLib.TransmissionMode)TransmissionMode.ModeNotSet;
            myLocator.Modulation = (TunerLib.ModulationType)ModulationType.ModNotSet;
            myLocator.OtherFrequencyInUse = false;
            myLocator.OuterFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
            myLocator.OuterFECRate = (TunerLib.BinaryConvolutionCodeRate)BinaryConvolutionCodeRate.RateNotSet;
            myLocator.SymbolRate = -1;

            myTuningSpace.DefaultLocator = DefaultLocator;
            TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
            myTuner.TuningSpace = (TunerLib.TuningSpace)TuningSpace;

          } break;//case NetworkType.DVBT: 
      }//switch (_networkType) 
      return true;
    }//private bool setupTuningSpace() 

    protected override void SubmitTuneRequest(DVBChannel ch)
    {
      if (ch == null) return;
      try
      {
        if (_filterNetworkProvider == null) return;
        //get the ITuner interface from the network provider filter
        TunerLib.TuneRequest newTuneRequest = null;
        TunerLib.ITuner myTuner = _filterNetworkProvider as TunerLib.ITuner;
        if (myTuner == null) return;
        switch (_networkType)
        {
          case NetworkType.ATSC:
            {
              //get the IATSCTuningSpace from the tuner
              TunerLib.IATSCChannelTuneRequest myATSCTuneRequest = null;
              TunerLib.IATSCTuningSpace myAtscTuningSpace = null;
              myAtscTuningSpace = myTuner.TuningSpace as TunerLib.IATSCTuningSpace;
              if (myAtscTuningSpace == null)
              {
                Log.Error("DVBGraphBDA: failed SubmitTuneRequest() tuningspace=null");
                return;
              }

              //create a new tuning request
              newTuneRequest = myAtscTuningSpace.CreateTuneRequest();
              if (newTuneRequest == null)
              {
                Log.Error("DVBGraphBDA: failed SubmitTuneRequest() could not create new tuningrequest");
                return;
              }
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() cast new tuningrequest to IATSCChannelTuneRequest");
              myATSCTuneRequest = newTuneRequest as TunerLib.IATSCChannelTuneRequest;
              if (myATSCTuneRequest == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
                return;
              }

              //get the IATSCLocator interface from the new tuning request
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() get IATSCLocator interface");
              TunerLib.IATSCLocator myLocator = myATSCTuneRequest.Locator as TunerLib.IATSCLocator;
              if (myLocator == null)
              {
                myLocator = myAtscTuningSpace.DefaultLocator as TunerLib.IATSCLocator;
              }


              if (myLocator == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning to frequency:{0} kHz. cannot get IATSCLocator", ch.Frequency);
                return;
              }
              //set the properties on the new tuning request
              Log.Info("DVBGraphBDA:SubmitTuneRequest(ATSC) set tuning properties. Freq:{0} physical:{1} major:{2} minor:{3} SR:{4} mod:{5} tsid:{6}",
                                                ch.Frequency, ch.PhysicalChannel, ch.MajorChannel, ch.MinorChannel, ch.Symbolrate, ch.Modulation, ch.TransportStreamID);
              myLocator.CarrierFrequency = -1;//ch.Frequency;
              myLocator.PhysicalChannel = ch.PhysicalChannel;
              myLocator.SymbolRate = -1;
              myLocator.TSID = -1;//ch.TransportStreamID;

              myLocator.InnerFEC = (TunerLib.FECMethod)FECMethod.MethodNotSet;
              myLocator.Modulation = (TunerLib.ModulationType)ch.Modulation;
              myATSCTuneRequest.MinorChannel = ch.MinorChannel;
              myATSCTuneRequest.Channel = ch.MajorChannel;
              myATSCTuneRequest.Locator = (TunerLib.Locator)myLocator;
              myTuner.TuneRequest = newTuneRequest;
              //release com objects
              DirectShowUtil.ReleaseComObject(myATSCTuneRequest);
              DirectShowUtil.ReleaseComObject(newTuneRequest);
              DirectShowUtil.ReleaseComObject(myLocator);
              DirectShowUtil.ReleaseComObject(myTuner.TuneRequest);

            }
            break;

          case NetworkType.DVBC:
            {
              TunerLib.IDVBTuningSpace2 myTuningSpace = null;
              //get the IDVBTuningSpace2 from the tuner
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() get IDVBTuningSpace2");
              myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
              if (myTuningSpace == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. Invalid tuningspace");
                return;
              }


              //create a new tuning request
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() create new tuningrequest");
              newTuneRequest = myTuningSpace.CreateTuneRequest();
              if (newTuneRequest == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
                return;
              }


              TunerLib.IDVBTuneRequest myTuneRequest = null;
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() cast new tuningrequest to IDVBTuneRequest");
              myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
              if (myTuneRequest == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
                return;
              }

              //get the IDVBCLocator interface from the new tuning request
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() get IDVBCLocator interface");
              TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;
              if (myLocator == null)
              {
                myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
              }

              if (myLocator == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning to frequency:{0} kHz. cannot get locator", ch.Frequency);
                return;
              }
              //set the properties on the new tuning request


              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() set tuning properties to tuning request");
              myLocator.CarrierFrequency = ch.Frequency;
              myLocator.SymbolRate = ch.Symbolrate;
              myLocator.InnerFEC = (TunerLib.FECMethod)ch.FEC;
              myLocator.Modulation = (TunerLib.ModulationType)ch.Modulation;

              myTuneRequest.ONID = ch.NetworkID;					//original network id
              myTuneRequest.TSID = ch.TransportStreamID;					//transport stream id
              myTuneRequest.SID = ch.ProgramNumber;					//service id
              myTuneRequest.Locator = (TunerLib.Locator)myLocator;
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() submit tuning request");
              myTuner.TuneRequest = newTuneRequest;
              //release com objects
              DirectShowUtil.ReleaseComObject(myTuneRequest);
              DirectShowUtil.ReleaseComObject(newTuneRequest);
              DirectShowUtil.ReleaseComObject(myLocator);
              DirectShowUtil.ReleaseComObject(myTuner.TuneRequest);
            } break;

          case NetworkType.DVBS:
            {
              //get the IDVBSLocator interface
              int lowOsc, hiOsc, diseqcUsed;
              int lnbKhzTone = 0;
              if (ch.DiSEqC < 1) ch.DiSEqC = 1;
              if (ch.DiSEqC > 4) ch.DiSEqC = 4;

              GetDisEqcSettings(ref ch, out lowOsc, out hiOsc, out lnbKhzTone, out diseqcUsed);

              TunerLib.IDVBSTuningSpace dvbSpace = myTuner.TuningSpace as TunerLib.IDVBSTuningSpace;
              if (dvbSpace == null)
              {
                Log.Error("DVBGraphBDA: failed could not get IDVBSTuningSpace");
                return;
              }

              Log.Info("DVBGraphBDA: set LNBSwitch to {0} kHz lowOsc={1} MHz hiOsc={2} MHz disecq:{3}", ch.LnbSwitchFrequency, lowOsc, hiOsc, diseqcUsed);

              dvbSpace.LNBSwitch = ch.LnbSwitchFrequency;
              dvbSpace.SpectralInversion = TunerLib.SpectralInversion.BDA_SPECTRAL_INVERSION_AUTOMATIC;
              dvbSpace.LowOscillator = lowOsc * 1000;
              dvbSpace.HighOscillator = hiOsc * 1000;

              if (_cardProperties.SupportsDiseqCommand() && (diseqcUsed != 0))
              {
                _cardProperties.SendDiseqCommand(lowOsc, hiOsc, diseqcUsed, _currentTuningObject.Frequency, ch.LnbSwitchFrequency, _currentTuningObject.Polarity, diseqcUsed);
              }
              else
              {
                SetDVBSInputRangeParameter(diseqcUsed, dvbSpace);
              }
              newTuneRequest = dvbSpace.CreateTuneRequest();
              if (newTuneRequest == null)
              {
                Log.Error("DVBGraphBDA: failed SubmitTuneRequest() could not create new tuningrequest");
                return;
              }
              TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
              if (myTuneRequest == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
                return;
              }

              TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;
              if (myLocator == null)
                myLocator = dvbSpace.DefaultLocator as TunerLib.IDVBSLocator;
              if (myLocator == null)
              {
                Log.Error("DVBGraphBDA: failed SubmitTuneRequest() could not get IDVBSLocator");
                return;
              }
              //set the properties for the new tuning request.
              myLocator.CarrierFrequency = ch.Frequency;
              myLocator.InnerFEC = (TunerLib.FECMethod)ch.FEC;
              if (ch.Polarity == 0)
                myLocator.SignalPolarisation = TunerLib.Polarisation.BDA_POLARISATION_LINEAR_H;
              else
                myLocator.SignalPolarisation = TunerLib.Polarisation.BDA_POLARISATION_LINEAR_V;

              myLocator.SymbolRate = ch.Symbolrate;
              myTuneRequest.ONID = ch.NetworkID;	//original network id
              myTuneRequest.TSID = ch.TransportStreamID;	//transport stream id
              myTuneRequest.SID = ch.ProgramNumber;		//service id
              myTuneRequest.Locator = (TunerLib.Locator)myLocator;
              //and submit the tune request

              myTuner.TuneRequest = newTuneRequest;
              DirectShowUtil.ReleaseComObject(myTuneRequest);
              DirectShowUtil.ReleaseComObject(newTuneRequest);
              DirectShowUtil.ReleaseComObject(myLocator);
              DirectShowUtil.ReleaseComObject(dvbSpace);
              DirectShowUtil.ReleaseComObject(myTuner.TuneRequest);
            }
            break;

          case NetworkType.DVBT:
            {
              TunerLib.IDVBTuningSpace2 myTuningSpace = null;
              //get the IDVBTuningSpace2 from the tuner
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() get IDVBTuningSpace2");
              myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
              if (myTuningSpace == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. Invalid tuningspace");
                return;
              }


              //create a new tuning request
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() create new tuningrequest");
              newTuneRequest = myTuningSpace.CreateTuneRequest();
              if (newTuneRequest == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
                return;
              }


              TunerLib.IDVBTuneRequest myTuneRequest = null;
              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() cast new tuningrequest to IDVBTuneRequest");
              myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
              if (myTuneRequest == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
                return;
              }

              //Log.WriteFile(LogType.Log,"DVBGraphBDA:SubmitTuneRequest() get IDVBTLocator");
              TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;
              if (myLocator == null)
              {
                myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
              }

              if (myLocator == null)
              {
                Log.Error("DVBGraphBDA:FAILED tuning to frequency:{0} kHz ONID:{1} TSID:{2}, SID:{3}. cannot get locator", ch.Frequency, ch.NetworkID, ch.TransportStreamID, ch.ProgramNumber);
                return;
              }
              //set the properties on the new tuning request
              Log.Info("DVBGraphBDA:SubmitTuneRequest() frequency:{0} kHz Bandwidth:{1} ONID:{2} TSID:{3}, SID:{4}",
                ch.Frequency, ch.Bandwidth, ch.NetworkID, ch.TransportStreamID, ch.ProgramNumber);
              myLocator.CarrierFrequency = ch.Frequency;
              myLocator.Bandwidth = ch.Bandwidth;
              myTuneRequest.ONID = ch.NetworkID;					//original network id
              myTuneRequest.TSID = ch.TransportStreamID;					//transport stream id
              myTuneRequest.SID = ch.ProgramNumber;					//service id
              myTuneRequest.Locator = (TunerLib.Locator)myLocator;
              myTuner.TuneRequest = newTuneRequest;
              DirectShowUtil.ReleaseComObject(myTuneRequest);
              DirectShowUtil.ReleaseComObject(newTuneRequest);
              DirectShowUtil.ReleaseComObject(myLocator);
              DirectShowUtil.ReleaseComObject(myTuner.TuneRequest);
            } break;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      if (!_inScanningMode)
        SetHardwarePidFiltering();
      _processTimer = DateTime.MinValue;
      _pmtSendCounter = 0;
      UpdateSignalPresent();
      Log.Info("DVBGraphBDA: signal strength:{0} signal quality:{1} signal present:{2} locked:{3}", SignalStrength(), SignalQuality(), SignalPresent(), TunerLocked());
    }

    protected override void SetupDiseqc(int disecqNo)
    {
      _currentTuningObject.DiSEqC = disecqNo;
    }

    protected void SetupAntennae5v()
    {
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
        {
            bool isAntennae5vEnabled = xmlreader.GetValueAsBool("general", "Antennae5v", false);

            if (_cardProperties.Supports5vAntennae)
            {
                _cardProperties.EnableAntenna(isAntennae5vEnabled);
            }
        }
    }

    protected override void SendHWPids(ArrayList pids)
    {
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        bool isHardwareFilteringEnabled = xmlreader.GetValueAsBool("general", "hwfiltering", false);
        if (isHardwareFilteringEnabled == false) return;
      }

      _cardProperties.SetHardwarePidFiltering(Network() == NetworkType.DVBC,
                                              Network() == NetworkType.DVBT,
                                              Network() == NetworkType.DVBS,
                                              Network() == NetworkType.ATSC,
                                              pids);

    }
  }//public class DVBGraphBDA
}//namespace MediaPortal.TV.Recording
//end of file
#endif