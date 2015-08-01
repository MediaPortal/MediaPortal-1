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
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Epg
{
  /// <summary>
  /// Class which holds all channels for a transponder
  /// </summary>
  public class Transponder
  {
    #region variables

    private IList<Channel> _channels;
    private TuningDetail _tuningDetail;
    private IChannel _tuningChannel;
    private int _currentChannelIndex;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="Transponder"/> class.
    /// </summary>
    public Transponder(TuningDetail tuningDetail, IChannel tuningChannel)
    {
      _channels = new List<Channel>();
      _tuningDetail = tuningDetail;
      _tuningChannel = tuningChannel;
      _currentChannelIndex = -1;
    }

    #endregion

    #region properties

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
    /// Get the channels broadcast on this transponder
    /// </summary>
    public IList<Channel> Channels
    {
      get
      {
        return _channels;
      }
    }

    /// <summary>
    /// Get the tuning details for this transponder.
    /// </summary>
    public TuningDetail TuningDetail
    {
      get
      {
        return _tuningDetail;
      }
    }

    /// <summary>
    /// Get the tuning channel for this transponder.
    /// </summary>
    public IChannel TuningChannel
    {
      get
      {
        return _tuningChannel;
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
      ChannelManagement.SaveChannel(Channels[Index]);
      this.LogDebug("EPG: database updated for #{0} {1}", Index, Channels[Index].Name);
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
    /// Gets the a list of all transponders
    /// </summary>
    public void RefreshTransponders()
    {
      Clear();
      _currentTransponder = null;
      _currentTransponderIndex = -1;

      //get all channels
      IList<Channel> channels = ChannelManagement.ListAllChannelsForEpgGrabbing(ChannelIncludeRelationEnum.TuningDetails);
      foreach (Channel channel in channels)
      {
        //for each tuning detail of the channel
        foreach (TuningDetail detail in channel.TuningDetails)
        {
          IChannel tuningChannel = TuningDetailManagement.GetTuningChannel(detail);
          bool found = false;
          foreach (Transponder transponder in this)
          {
            if (!transponder.TuningChannel.IsDifferentTransmitter(tuningChannel))
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
            IChannelSatellite satelliteChannel = tuningChannel as IChannelSatellite;
            if (satelliteChannel != null)
            {
              satelliteChannel.LnbType = new LnbTypeBLL(detail.LnbType);
            }
            Transponder t = new Transponder(detail, tuningChannel);
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