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
using System;
using System.Collections.Generic;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;

namespace TvService
{
  /// <summary>
  /// Class which holds all channels for a transponder
  public class Transponder
  {
    #region variables
    TuningDetail _detail;
    List<Channel> _channels;
    int _currentChannelIndex;
    bool _inUse;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="Transponder"/> class.
    /// </summary>
    /// <param name="detail">The detail.</param>
    public Transponder(TuningDetail detail)
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
      get
      {
        return _inUse;
      }
      set
      {
        _inUse = value;
      }
    }

    /// <summary>
    /// Gets or sets the current channel index.
    /// </summary>
    /// <value>The channel index.</value>
    public int Index
    {
      get
      {
        return _currentChannelIndex;
      }
    }

    public Channel CurrentChannel
    {
      get
      {
        return Channels[_currentChannelIndex];
      }
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
      get
      {
        return _channels;
      }
      set
      {
        _channels = value;
      }
    }

    /// <summary>
    /// Gets or sets the tuning details for this transponder.
    /// </summary>
    /// <value>The tuning detail.</value>
    public TuningDetail TuningDetail
    {
      get
      {
        return _detail;
      }
      set
      {
        _detail = value;
      }
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
          Log.Error("transponder index out of range:{0}/{1}", Index, Channels.Count);
          return null;
        }
        TvBusinessLayer layer = new TvBusinessLayer();
        return layer.GetTuningChannelByType(Channels[Index], TuningDetail.ChannelType);
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
      Channels[Index].LastGrabTime = DateTime.Now;
      Channels[Index].Persist();
      Log.Write("EPG: database updated for #{0} {1}", Index, Channels[Index].Name);

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
      if (other.TuningDetail.ChannelType != TuningDetail.ChannelType)
        return false;
      if (other.TuningDetail.Frequency != TuningDetail.Frequency)
        return false;
      if (other.TuningDetail.Modulation != TuningDetail.Modulation)
        return false;
      if (other.TuningDetail.Symbolrate != TuningDetail.Symbolrate)
        return false;
      if (other.TuningDetail.Bandwidth != TuningDetail.Bandwidth)
        return false;
      if (other.TuningDetail.Polarisation != TuningDetail.Polarisation)
        return false;
      return true;
    }

    /// <summary>
    /// Calculates a hashcode for comparing Transponder objects
    /// </summary>
    public override int GetHashCode()
    {
      return TuningDetail.Frequency + TuningDetail.ChannelType;
    }

    /// <summary>
    /// Logs the transponder info to the log file.
    /// </summary>
    public void Dump()
    {
      Log.Write("Transponder:{0} {1} {2} {3} {4} {5}", _currentChannelIndex, TuningDetail.ChannelType, TuningDetail.Frequency, TuningDetail.Modulation, TuningDetail.Symbolrate, TuningDetail.Bandwidth, TuningDetail.Polarisation);
      foreach (Channel c in _channels)
      {
        Log.Write(" {0}", c.Name);
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
      return string.Format("type:{0} freq:{1} mod:{2} sr:{3} bw:{4} pol:{5}",
        TuningDetail.ChannelType, TuningDetail.Frequency,
        TuningDetail.Modulation, TuningDetail.Symbolrate, TuningDetail.Bandwidth, TuningDetail.Polarisation);
    }
    #endregion

  }

  public class TransponderList : List<Transponder>
  {
    #region Singleton implementation
    static readonly TransponderList _instance = new TransponderList();

    public static TransponderList Instance
    {
      get
      {
        return _instance;
      }
    }
    #endregion

    private Transponder _currentTransponder;
    private int _currentTransponderIndex = -1;

    public Transponder CurrentTransponder
    {
      get
      {
        return _currentTransponder;
      }
    }
    public int CurrentIndex
    {
      get
      {
        return _currentTransponderIndex;
      }
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
      Gentle.Common.CacheManager.Clear();
      Reset();
      //get all channels
      IList<Channel> channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        //if epg grabbing is enabled and channel is a radio or tv channel
        if (channel.GrabEpg == false)
          continue;
        if (channel.IsRadio == false && channel.IsTv == false)
          continue;

        //for each tuning detail of the channel
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          //skip analog channels and webstream channels
          if (detail.ChannelType == 0 || detail.ChannelType == 5)
            continue;

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
