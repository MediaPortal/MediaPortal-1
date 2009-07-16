/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using System.Runtime.InteropServices;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{

  [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
  class MpTsAnalyzer { }

  /// <summary>
  /// Constructor if TvCardDVBIP
  /// </summary>
  public abstract class TvCardDVBIP : TvCardDvbBase, ITVCard
  {
    /// Stream source filter
    protected IBaseFilter _filterStreamSource;
    /// default url
    protected string _defaultUrl;
    /// sequence
    protected int _sequence; 

    /// <summary>
    /// Contstructor
    /// </summary>
    /// <param name="epgEvents"></param>
    /// <param name="device"></param>
    /// <param name="sequence"></param>
    public TvCardDVBIP(IEpgEvents epgEvents, DsDevice device, int sequence) : base(epgEvents, device)
    {
      _cardType = CardType.DvbIP;
      _sequence = sequence;
      if (_sequence > 0)
      {
        _name = _name + "_" + _sequence;
      }
    }

    #region graphbuilding

    /// <summary>
    /// Build graph
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

        AddMpeg2DemuxerToGraph();
        AddStreamSourceFilter(_defaultUrl);
        ConnectMpeg2DemuxToInfTee();
        AddTsWriterFilterToGraph();
        _conditionalAccess = new ConditionalAccess(_filterStreamSource, _filterTsWriter, null, this);
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
    /// AddStreamSourceFilter
    /// </summary>
    /// <param name="url">url</param>
    protected abstract void AddStreamSourceFilter(string url);

    /// <summary>
    /// RemoveStreamSourceFilter
    /// </summary>
    protected abstract void RemoveStreamSourceFilter();

    /// <summary>
    /// RunGraph
    /// </summary>
    /// <param name="subChannel">subchannel</param>
    /// <param name="url">url</param>
    protected abstract void RunGraph(int subChannel, string url);

    #endregion

    #region Implementation of ITVCard

    /// <summary>
    /// Checks if channel can be tuned by IPTV
    /// </summary>
    /// <param name="channel">channel</param>
    /// <returns>true if DVBIPChannel</returns>
    public bool CanTune(IChannel channel)
    {
      if ((channel as DVBIPChannel) == null) return false;
      return true;
    }

    /// <summary>
    /// ScanningInterface
    /// </summary>
    public ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId()) return null;
        return new DVBIPScanning(this);
      }
    }

    /// <summary>
    /// Tune to channel
    /// </summary>
    /// <param name="subChannelId"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbip:  Tune:{0}", channel);
      try
      {
        DVBIPChannel dvbipChannel = channel as DVBIPChannel;
        if (dvbipChannel == null)
        {
          Log.Log.WriteFile("Channel is not a IP TV channel!!! {0}", channel.GetType().ToString());
          return null;
        }

        Log.Log.Info("dvbip: tune: Assigning oldChannel");
        DVBIPChannel oldChannel = CurrentChannel as DVBIPChannel;
        if (CurrentChannel != null)
        {
          //@FIX this fails for back-2-back recordings
          //if (oldChannel.Equals(channel)) return _mapSubChannels[0];
          Log.Log.Info("dvbip: tune: Current Channel != null {0}", CurrentChannel.ToString());
        }
        else
        { Log.Log.Info("dvbip: tune: Current channel is null"); }
        if (_graphState == GraphState.Idle)
        {
          Log.Log.Info("dvbip: tune: Building graph");
          BuildGraph();
          if (_mapSubChannels.ContainsKey(subChannelId) == false)
          {
            subChannelId = GetNewSubChannel(channel);
          }
        }
        else
        { Log.Log.Info("dvbip: tune: Graph is running"); }

        //_pmtPid = -1;

        Log.Log.Info("dvb:Submiting tunerequest Channel:{0} subChannel:{1} ", channel.Name, subChannelId);
        if (_mapSubChannels.ContainsKey(subChannelId) == false)
        {
          Log.Log.Info("dvb:Getting new subchannel");
          subChannelId = GetNewSubChannel(channel);
        }
        else
        {
        }
        Log.Log.Info("dvb:Submit tunerequest size:{0} new:{1}", _mapSubChannels.Count, subChannelId);
        _mapSubChannels[subChannelId].CurrentChannel = channel;

        _mapSubChannels[subChannelId].OnBeforeTune();

        if (_interfaceEpgGrabber != null)
        {
          _interfaceEpgGrabber.Reset();
        }

        Log.Log.WriteFile("dvb:Submit tunerequest calling put_TuneRequest");
        _lastSignalUpdate = DateTime.MinValue;

        _mapSubChannels[subChannelId].OnAfterTune();
        ITvSubChannel ch = _mapSubChannels[subChannelId];
        Log.Log.Info("dvbip: tune: Running graph for channel {0}", ch.ToString());
        Log.Log.Info("dvbip: tune: SubChannel {0}", ch.SubChannelId);
        RunGraph(ch.SubChannelId, dvbipChannel.Url);
        Log.Log.Info("dvbip: tune: Graph running. Returning {0}", ch.ToString());
        return ch;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
      //unreachable return null;
    }

    #endregion

    /// <summary>
    /// Dispose resources
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      if (_filterStreamSource != null)
      {
        Release.ComObject("_filterStreamSource filter", _filterStreamSource);
        _filterStreamSource = null;
      }
    }

    /// <summary>
    /// Stops graph
    /// </summary>
    public override void StopGraph()
    {
      base.StopGraph();
      //FIXME: this logic should be checked (removing and adding filters in stopgraph?) (morpheus_xx)
      if (_graphState != GraphState.Idle)
      {
        RemoveStreamSourceFilter();
        AddStreamSourceFilter(_defaultUrl);
      }
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _name;
    }

    /// <summary>
    /// UpdateSignalQuality
    /// </summary>
    protected override void UpdateSignalQuality()
    {
      if (GraphRunning() == false)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }
      if (CurrentChannel == null)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }
      if (_filterStreamSource == null)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }
      if (!CheckThreadId())
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }
      _tunerLocked = true;
      _signalLevel = 100;
      _signalPresent = true;
      _signalQuality = 100;
    }

    /// <summary>
    /// return the DevicePath
    /// </summary>
    public override string DevicePath
    {
      get
      {
        if (_sequence == 0)
        {
          return base.DevicePath;
        }
        return base.DevicePath + "(" + _sequence + ")";
      }
    }
  }
}