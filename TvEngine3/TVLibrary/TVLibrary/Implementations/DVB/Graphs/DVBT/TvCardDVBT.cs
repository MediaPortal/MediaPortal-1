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
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.DVB
{

  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-T BDA cards
  /// </summary>
  public class TvCardDVBT : TvCardDvbBase, IDisposable, ITVCard
  {
    #region variables
    /// <summary>
    /// holds the the DVB-T tuning space
    /// </summary>
    protected IDVBTuningSpace _tuningSpace = null;
    /// <summary>
    /// holds the current DVB-T tuning request
    /// </summary>
    protected IDVBTuneRequest _tuneRequest = null;

    /// <summary>
    /// Device of the card
    /// </summary>
    DsDevice _device;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:TvCardDVBT"/> class.
    /// </summary>
    /// <param name="device">The device.</param>
    public TvCardDVBT(DsDevice device)
    {
      _device = device;
      _name = device.Name;
      _devicePath = device.DevicePath;

      try
      {
        //        BuildGraph();
        //        RunGraph();
        //        StopGraph();
      }
      catch (Exception)
      {
      }
    }
    #endregion

    #region graphbuilding
    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
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
        AddMpeg2DemuxerToGraph();
        AddAndConnectBDABoardFilters(_device);
        AddBdaTransportFiltersToGraph();

        //        ConnectFilters();
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


    #region tuning & recording
    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbt:  Tune:{0}", channel);
      try
      {
        DVBTChannel dvbtChannel = channel as DVBTChannel;
        if (dvbtChannel == null)
        {
          Log.Log.WriteFile("Channel is not a DVBT channel!!! {0}", channel.GetType().ToString());
          return null;
        }

        DVBTChannel oldChannel = CurrentChannel as DVBTChannel;
        if (CurrentChannel != null)
        {
          //@FIX this fails for back-2-back recordings
          //if (oldChannel.Equals(channel)) return _mapSubChannels[0];
        }
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        //_pmtPid = -1;
        ILocator locator;
        _tuningSpace.get_DefaultLocator(out locator);
        IDVBTLocator dvbtLocator = (IDVBTLocator)locator;
        int hr = dvbtLocator.put_Bandwidth(dvbtChannel.BandWidth);
        hr = _tuneRequest.put_ONID(dvbtChannel.NetworkId);
        hr = _tuneRequest.put_SID(dvbtChannel.ServiceId);
        hr = _tuneRequest.put_TSID(dvbtChannel.TransportId);
        hr = locator.put_CarrierFrequency((int)dvbtChannel.Frequency);
        _tuneRequest.put_Locator(locator);

        ITvSubChannel ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest);
        RunGraph(ch.SubChannelId);
        return ch;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
      return null;
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
        if (!CheckThreadId()) return null;
        return new DVBTScanning(this);
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
      if ((channel as DVBTChannel) == null) return false;
      return true;
    }
  }
}
