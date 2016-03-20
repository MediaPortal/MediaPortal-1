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
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  internal abstract class SubChannelManagerBase : ISubChannelManager
  {
    #region variables

    /// <summary>
    /// The manager's sub-channels.
    /// </summary>
    private Dictionary<int, ISubChannelInternal> _subChannels = new Dictionary<int, ISubChannelInternal>();

    /// <summary>
    /// The identifier to use for the next new sub-channel.
    /// </summary>
    private int _nextSubChannelId = 0;

    /// <summary>
    /// Can the tuner receive all sub-channels from the tuned transmitter
    /// simultaneously?
    /// </summary>
    /// <remarks>
    /// This variable may seem obvious and unnecessary, especially for modern
    /// tuners. However even today there are tuners that cannot receive more
    /// than one sub-channel simultaneously. CableCARD tuners are a good
    /// example.
    /// </remarks>
    private bool _canSimultaneouslyReceiveTransmitterSubChannels = true;

    /// <summary>
    /// The maximum time to wait for implementation-dependent stream
    /// information (eg. PAT, PMT and CAT) to be received during tuning.
    /// </summary>
    private TimeSpan _timeLimitReceiveStreamInfo = new TimeSpan(0, 0, 5);

    /// <summary>
    /// Should the current tuning process be aborted immediately?
    /// </summary>
    private volatile bool _cancelTune = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelManagerBase"/> class.
    /// </summary>
    /// <param name="canSimultaneouslyReceiveTransmitterSubChannels"><c>True</c> if the tuner can simultaneously receive all sub-channels from the tuned transmitter.</param>
    protected SubChannelManagerBase(bool canSimultaneouslyReceiveTransmitterSubChannels = true)
    {
      _canSimultaneouslyReceiveTransmitterSubChannels = canSimultaneouslyReceiveTransmitterSubChannels;
    }

    #region ISubChannelManager members

    /// <summary>
    /// Reload the manager's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public virtual void ReloadConfiguration(Tuner configuration)
    {
      _timeLimitReceiveStreamInfo = new TimeSpan(0, 0, 0, 0, SettingsManagement.GetValue("timeLimitReceiveStreamInfo", 5000));
    }

    /// <summary>
    /// Set the manager's extensions.
    /// </summary>
    /// <param name="extensions">A list of the tuner's extensions, in priority order.</param>
    public abstract void SetExtensions(IList<ITunerExtension> extensions);

    /// <summary>
    /// Decompose the sub-channel manager.
    /// </summary>
    public void Decompose()
    {
      this.LogDebug("sub-channel manager base: decompose, sub-channel count = {0}", _subChannels.Count);
      foreach (var subChannel in _subChannels.Values)
      {
        subChannel.Decompose();
      }
      _subChannels.Clear();
      _nextSubChannelId = 0;

      OnDecompose();
    }

    #region tuning

    /// <summary>
    /// This function should be called before the tuner is tuned to a new
    /// transmitter.
    /// </summary>
    public virtual void OnBeforeTune()
    {
      if (_subChannels.Count > 1)
      {
        throw new TvException("Tune attempt with more than one active sub-channel.");
      }
    }

    /// <summary>
    /// Tune a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="isNew"><c>True</c> if the sub-channel is newly created.</param>
    /// <returns>the sub-channel</returns>
    public ISubChannel Tune(int id, IChannel channel, out bool isNew)
    {
      _cancelTune = false;
      ISubChannelInternal subChannel = null;
      if (_subChannels.TryGetValue(id, out subChannel) && subChannel != null)
      {
        this.LogInfo("sub-channel manager base: using existing sub-channel, ID = {0}, count = {1}", id, _subChannels.Count);
        isNew = false;
      }
      else
      {
        id = _nextSubChannelId++;
        this.LogInfo("sub-channel manager base: create new sub-channel, ID = {0}, count = {1}", id, _subChannels.Count);
        isNew = true;
      }

      // Some tuners (for example: CableCARD tuners) are only able to deliver
      // one sub-channel.
      if (!_canSimultaneouslyReceiveTransmitterSubChannels && _subChannels.Count > 0)
      {
        if (isNew)
        {
          // New sub-channel.
          foreach (var sc in _subChannels.Values)
          {
            if (sc.CurrentChannel != channel)
            {
              // The tuner is currently receiving a different sub-channel.
              throw new TvException("Tuner is not able to receive more than one sub-channel.");
            }
          }
        }
        else if (_subChannels.Count != 1)
        {
          // Existing sub-channel.
          // If this is not the only sub-channel then by definition this must
          // must be an attempt to tune to a different sub-channel. Not allowed.
          throw new TvException("Tuner is not able to receive more than one sub-channel.");
        }
      }

      subChannel = OnTune(id, channel, _timeLimitReceiveStreamInfo);
      if (isNew && subChannel != null)
      {
        _subChannels[id] = subChannel;
      }
      return subChannel;
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="id">The identifier of the sub-channel associated with the tuning process that is being cancelled.</param>
    public void CancelTune(int id)
    {
      _cancelTune = true;
      ISubChannelInternal subChannel;
      if (_subChannels.TryGetValue(id, out subChannel) && subChannel != null)
      {
        subChannel.CancelTune();
      }
    }

    #endregion

    #region sub-channels

    /// <summary>
    /// Can the sub-channel manager receive multiple sub-channels from the current transmitter simultaneously?
    /// </summary>
    public bool CanSimultaneouslyReceiveTransmitterSubChannels
    {
      get
      {
        return _canSimultaneouslyReceiveTransmitterSubChannels;
      }
    }

    /// <summary>
    /// Get a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <returns>the sub-channel if it exists, otherwise <c>null</c></returns>
    public ISubChannel GetSubChannel(int id)
    {
      ISubChannelInternal subChannel = null;
      _subChannels.TryGetValue(id, out subChannel);
      return subChannel;
    }

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    public void FreeSubChannel(int id)
    {
      this.LogDebug("sub-channel manager base: free sub-channel, ID = {0}, count = {1}", id, _subChannels.Count);
      ISubChannelInternal subChannel;
      if (!_subChannels.TryGetValue(id, out subChannel))
      {
        this.LogWarn("sub-channel manager base: sub-channel to free not found, ID = {0}", id);
        return;
      }

      if (subChannel.IsTimeShifting)
      {
        throw new TvException("Asked to free sub-channel {0}, still time-shifting.", id);
      }
      if (subChannel.IsRecording)
      {
        throw new TvException("Asked to free sub-channel {0}, still recording.", id);
      }

      OnFreeSubChannel(id);
      subChannel.Decompose();
      _subChannels.Remove(id);
      if (_subChannels.Count == 0)
      {
        _nextSubChannelId = 0;
      }
    }

    /// <summary>
    /// Get the count of sub-channels.
    /// </summary>
    public int SubChannelCount
    {
      get
      {
        return _subChannels.Count;
      }
    }

    /// <summary>
    /// Get the set of sub-channel identifiers for each channel the tuner is
    /// currently decrypting.
    /// </summary>
    /// <returns>a collection of sub-channel identifier lists</returns>
    public abstract ICollection<IList<int>> GetDecryptedSubChannelDetails();

    /// <summary>
    /// Determine whether a sub-channel is being decrypted.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the sub-channel is being decrypted, otherwise <c>false</c></returns>
    public abstract bool IsDecrypting(IChannel channel);

    #endregion

    #endregion

    #region protected members

    #region abstract members

    /// <summary>
    /// Tune a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="timeLimitReceiveStreamInfo">The maximum time to wait for required implementation-dependent stream information during tuning.</param>
    /// <returns>the sub-channel</returns>
    protected abstract ISubChannelInternal OnTune(int id, IChannel channel, TimeSpan timeLimitReceiveStreamInfo);

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    protected abstract void OnFreeSubChannel(int id);

    /// <summary>
    /// Decompose the sub-channel manager.
    /// </summary>
    protected abstract void OnDecompose();

    #endregion

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an
    /// exception if it has.
    /// </summary>
    protected void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    #endregion
  }
}