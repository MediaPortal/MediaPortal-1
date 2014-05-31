#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Epg
{
  /// <summary>
  /// Class which holds all channels for a transponder
  public class Transponder
  {


    #region variables

    private ServiceDetail _detail;
    private List<Channel> _channels;
    private int _currentChannelIndex;
    private bool _inUse;    

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="Transponder"/> class.
    /// </summary>
    /// <param name="detail">The detail.</param>
    public Transponder(ServiceDetail detail)
    {      
      _channels = new List<Channel>();
      _detail = detail;
      _currentChannelIndex = -1;
      _inUse = false;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets or sets a value indicating whether the transponder is in use or not
    /// </summary>
    /// <value><c>true</c> if in use; otherwise, <c>false</c>.</value>
    public bool InUse
    {
      get { return _inUse; }
      set { _inUse = value; }
    }

    /// <summary>
    /// Gets or sets the current channel index.
    /// </summary>
    /// <value>The channel index.</value>
    public int Index
    {
      get { return _currentChannelIndex; }
    }

    public Channel CurrentChannel
    {
      get { return Channels[_currentChannelIndex]; }
    }

    public Channel GetNextChannel()
    {
      _currentChannelIndex++;
      if (_currentChannelIndex == Channels.Count)
      {
        _currentChannelIndex = -1;
        return null;
      }
      return Channels[_currentChannelIndex];
    }

    /// <summary>
    /// Gets or sets the channels for this transponder
    /// </summary>
    /// <value>The channels.</value>
    public List<Channel> Channels
    {
      get { return _channels; }
      set { _channels = value; }
    }

    /// <summary>
    /// Gets or sets the tuning details for this transponder.
    /// </summary>
    /// <value>The tuning detail.</value>
    public ServiceDetail ServiceDetail
    {
      get { return _detail; }
      set { _detail = value; }
    }

    /// <summary>
    /// Gets the tuning detail for the current channel.
    /// </summary>
    /// <value>The tuning detail.</value>
    public IChannel Tuning
    {
      get
      {
        if (Index < 0 || Index >= Channels.Count)
        {
          this.LogError("transponder index out of range:{0}/{1}", Index, Channels.Count);
          return null;
        }
        return ChannelManagement.GetTuningChannel(ServiceDetail);        
      }
    }

    #endregion

    #region public members

    /// <summary>
    /// Called when epg times out, simply sets the lastgrabtime for the current channel
    /// </summary>
    public void OnTimeOut()
    {
      if (Index < 0 || Index >= Channels.Count)
        return;
      //Channels[Index].LastGrabTime = DateTime.Now;
      //todo: MM handle grabEPG
      ChannelManagement.SaveChannel(Channels[Index]);
      this.LogDebug("EPG: database updated for #{0} {1}", Index, Channels[Index].DisplayName);
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      Transponder other = (Transponder)obj;

      if (other.ServiceDetail.GetType() != ServiceDetail.GetType())
        return false;
      if (other.ServiceDetail.TuningDetail.GetType() != ServiceDetail.TuningDetail.GetType())
        return false;   
      
      int currModulation;
      int currFrequency;
      int currSymbolrate;
      int currPolarisation;
      int currBandwidth;
      GetCommonTuningDetails(ServiceDetail, out currModulation, out currFrequency, out currSymbolrate, out currPolarisation, out currBandwidth);

      int otherModulation;
      int otherFrequency;
      int otherSymbolrate;
      int otherPolarisation;
      int otherBandwidth;
      GetCommonTuningDetails(other.ServiceDetail, out otherModulation, out otherFrequency, out otherSymbolrate, out otherPolarisation, out otherBandwidth);

      if (otherFrequency != currFrequency)
        return false;
      if (otherModulation != currModulation)
        return false;
      if (otherSymbolrate != currSymbolrate)
        return false;
      if (otherBandwidth != currBandwidth)
        return false;
      if (otherPolarisation != currPolarisation)
        return false;
      return true;
    }

    /// <summary>
    /// Calculates a hashcode for comparing Transponder objects
    /// </summary>
    public override int GetHashCode()
    {
      int currModulation;
      int currFrequency;
      int currSymbolrate;
      int currPolarisation;
      int currBandwidth;
      GetCommonTuningDetails(ServiceDetail, out currModulation, out currFrequency, out currSymbolrate, out currPolarisation, out currBandwidth);
      
      return currFrequency + GetChannelType(ServiceDetail);
    }

    private int GetChannelType(ServiceDetail serviceDetail)
    {        
      if (serviceDetail.TuningDetail is TuningDetailAtsc)
      {
        return 0;
      }
      else if (serviceDetail.TuningDetail is TuningDetailAnalog)
      {
        return 1;
      }
      else if (serviceDetail.TuningDetail is TuningDetailCable)
      {
        return 2;        
      }
      else if (serviceDetail.TuningDetail is TuningDetailDvbS2)
      {
        return 3;
      }
      else if (serviceDetail.TuningDetail is TuningDetailSatellite)
      {
        return 4;
      }
      else if (serviceDetail.TuningDetail is TuningDetailDvbT2)
      {
        return 5;
      }
      else if (serviceDetail.TuningDetail is TuningDetailTerrestrial)
      {
        return 6;
      }
      else if (serviceDetail.TuningDetail is TuningDetailStream)
      {
        return 7;
      }
      return -1;
    }

    /// <summary>
    /// Logs the transponder info to the log file.
    /// </summary>
    public void Dump()
    {

      int currModulation;
      int currFrequency;
      int currSymbolrate;
      int currPolarisation;
      int currBandwidth;
      GetCommonTuningDetails(ServiceDetail, out currModulation, out currFrequency, out currSymbolrate, out currPolarisation, out currBandwidth);

      this.LogDebug("Transponder:{0} {1} {2} {3} {4} {5}", _currentChannelIndex, GetChannelType(ServiceDetail),
                currFrequency, currModulation, currSymbolrate, currBandwidth,
                currPolarisation);
      foreach (Channel c in _channels)
      {
        this.LogDebug(" {0}", c.DisplayName);
      }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      int modulation;
      int frequency;
      int symbolrate;
      int polarisation;
      int bandwidth;
      GetCommonTuningDetails(ServiceDetail, out modulation, out frequency, out symbolrate, out polarisation, out bandwidth);

      return string.Format("type:{0} freq:{1} mod:{2} sr:{3} bw:{4} pol:{5}",
                           ServiceDetail.TuningDetail.ToString(), frequency,
                           modulation, symbolrate, bandwidth,
                           polarisation);
    }

    private void GetCommonTuningDetails(ServiceDetail serviceDetail, out int modulation, out int frequency, out int symbolrate, out int polarisation, out int bandwidth)
    {
      frequency = 0;
      modulation = 0;
      symbolrate = 0;
      bandwidth = 0;
      polarisation = 0;

      var tuningDetailAtsc = serviceDetail.TuningDetail as TuningDetailAtsc;
      if (tuningDetailAtsc != null)
      {
        frequency = tuningDetailAtsc.Frequency.GetValueOrDefault(0);
        modulation = tuningDetailAtsc.Modulation.GetValueOrDefault(0);
      }

      var tuningDetailCable = ServiceDetail.TuningDetail as TuningDetailCable;
      if (tuningDetailCable != null)
      {
        frequency = tuningDetailCable.Frequency.GetValueOrDefault(0);
        modulation = tuningDetailCable.Modulation.GetValueOrDefault(0);
        symbolrate = tuningDetailCable.Modulation.GetValueOrDefault(0);
      }

      var tuningDetailTerrestrial = ServiceDetail.TuningDetail as TuningDetailTerrestrial;
      if (tuningDetailTerrestrial != null)
      {
        frequency = tuningDetailTerrestrial.Frequency.GetValueOrDefault(0);
        bandwidth = tuningDetailTerrestrial.Bandwidth.GetValueOrDefault(0);
      }

      var tuningDetailSatellite = ServiceDetail.TuningDetail as TuningDetailSatellite;
      if (tuningDetailSatellite != null)
      {
        frequency = tuningDetailSatellite.Frequency.GetValueOrDefault(0);
        modulation = tuningDetailSatellite.Modulation.GetValueOrDefault(0);
        symbolrate = tuningDetailSatellite.Modulation.GetValueOrDefault(0);
        polarisation = tuningDetailSatellite.Polarisation.GetValueOrDefault(0);
      }
    }

    #endregion
  }

  public class TransponderList : List<Transponder>
  {
    #region Singleton implementation

    private static readonly TransponderList _instance = new TransponderList();

    public static TransponderList Instance
    {
      get { return _instance; }
    }

    #endregion

    private Transponder _currentTransponder;
    private int _currentTransponderIndex = -1;

    public Transponder CurrentTransponder
    {
      get { return _currentTransponder; }
    }

    public int CurrentIndex
    {
      get { return _currentTransponderIndex; }
    }

    /// <summary>
    /// Clears the list
    /// </summary>
    public void Reset()
    {
      Clear();
      _currentTransponder = null;
      _currentTransponderIndex = -1;
    }

    /// <summary>
    /// Gets the a list of all transponders
    /// </summary>
    public void RefreshTransponders()
    {
      ////Gentle.Common.CacheManager.Clear();
      Reset();
      //get all channels
      IList<Channel> channels = ChannelManagement.ListAllChannelsForEpgGrabbing(ChannelIncludeRelationEnum.TuningDetails);
      foreach (Channel channel in channels)
      {
        //for each tuning detail of the channel
        foreach (ServiceDetail detail in channel.ServiceDetails)
        {
          //create a new transponder
          Transponder t = new Transponder(detail);
          bool found = false;

          //check if transonder already exists
          foreach (Transponder transponder in this)
          {
            if (transponder.Equals(t))
            {
              //yes, then simply add the channel to this transponder
              found = true;
              transponder.Channels.Add(channel);
              break;
            }
          }

          if (!found)
          {
            //new transponder, add the channel to this transponder
            //and add the transponder to the transponder list
            t.Channels.Add(channel);
            Add(t);
          }
        }
      }
    }

    /// <summary>
    /// Gets the next transponder from the list. Returns null if all transponders have been processed
    /// </summary>
    public Transponder GetNextTransponder()
    {
      _currentTransponderIndex++;
      if (_currentTransponderIndex == Count)
      {
        _currentTransponder = null;
        _currentTransponderIndex = -1;
        return null;
      }
      _currentTransponder = this[_currentTransponderIndex];
      return CurrentTransponder;
    }
  }
}