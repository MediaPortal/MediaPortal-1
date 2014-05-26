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
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces
{
  #region event delegates

  /// <summary>
  /// Delegate for the new sub-channel event.
  /// </summary>
  /// <param name="subChannelId">The ID of the new sub-channel.</param>
  public delegate void OnNewSubChannelDelegate(int subChannelId);

  /// <summary>
  /// Delegate for the after tune event.
  /// </summary>
  public delegate void OnAfterTuneDelegate();

  #endregion

  /// <summary>
  /// interface for a tv card
  /// </summary>
  public interface ITVCard
  {
    #region events

    /// <summary>
    /// Set the tuner's new sub-channel event handler.
    /// </summary>
    event OnNewSubChannelDelegate OnNewSubChannelEvent;

    /// <summary>
    /// Set the tuner's after tune event handler.
    /// </summary>
    event OnAfterTuneDelegate OnAfterTuneEvent;

    #endregion

    #region properties

    /// <summary>
    /// Get the tuner's name.
    /// </summary>
    string Name
    {
      get;
    }

    /// <summary>
    /// Get the tuner's group.
    /// </summary>
    ITunerGroup Group
    {
      get;
    }

    /// <summary>
    /// Get the tuner's unique identifier.
    /// </summary>
    /// <remarks>
    /// This is the TV Engine's unique internal identifier.
    /// </remarks>
    int TunerId
    {
      get;
    }

    /// <summary>
    /// Get the tuner's unique external identifier.
    /// </summary>		
    string ExternalId
    {
      get;
    }

    /// <summary>
    /// Get the tuner's product instance identifier.
    /// </summary>
    /// <remarks>
    /// The product instance identifier is a shared identifier for all tuner instances derived from a [multi-tuner] product.
    /// </remarks>
    string ProductInstanceId
    {
      get;
    }

    /// <summary>
    /// Get the tuner's instance identifier.
    /// </summary>
    /// <remarks>
    /// The tuner instance identifier is a shared identifier for all tuner instances derived from a single physical tuner.
    /// </remarks>
    string TunerInstanceId
    {
      get;
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    bool CanTune(IChannel channel);

    /// <summary>
    /// Stop the tuner.
    /// </summary>
    /// <remarks>
    /// The actual result of this function depends on tuner configuration.
    /// </remarks>
    [Obsolete("This function should not be used. Instead use FreeSubChannel() to free each remaining sub-channel. The tuner will be stopped after the last sub-channel is freed... but that is implementation detail which you should not have to care about.")]
    void Stop();

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    CamType CamType { get; }

    /// <summary>
    /// Get the tuner's type.
    /// </summary>
    CardType TunerType
    {
      get;
    }

    /// <summary>
    /// Does the tuner support conditional access?
    /// </summary>
    /// <value><c>true</c> if the tuner supports conditional access, otherwise <c>false</c></value>
    bool IsConditionalAccessSupported { get; }

    /// <summary>
    /// Get a count of the number of services that the device is currently decrypting.
    /// </summary>
    /// <value>The number of services currently being decrypted.</value>
    int NumberOfChannelsDecrypting { get; }

    #endregion

    #region Channel linkage handling

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    void StartLinkageScanner(BaseChannelLinkageScanner callBack);

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    void ResetLinkageScanner();

    /// <summary>
    /// Returns the channel linkages grabbed
    /// </summary>
    List<PortalChannel> ChannelLinkages { get; }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    IEpgGrabber EpgGrabberInterface
    {
      get;
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    IChannelScanner ChannelScanningInterface
    {
      get;
    }

    /// <summary>
    /// Get the tuner's DiSEqC control interface.
    /// </summary>
    /// <remarks>
    /// This interface is only applicable for satellite tuners. It is used for controlling switch,
    /// positioner and LNB settings.
    /// </remarks>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner does not support
    /// sending/receiving DiSEqC commands</value>
    IDiseqcController DiseqcController
    {
      get;
    }

    /// <summary>
    /// Get the tuner's conditional access menu interaction interface.
    /// </summary>
    /// <remarks>
    /// This interface is only applicable if conditional access is supported.
    /// </remarks>
    /// <value><c>null</c> if the tuner does not support conditional access</value>
    IConditionalAccessMenuActions CaMenuInterface { get; }

    #endregion

    #region tuning

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the sub-channel associated with the tuned channel</returns>
    ITvSubChannel Tune(int subChannelId, IChannel channel);

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being cancelled.</param>
    void CancelTune(int subChannelId);

    #endregion

    /// <summary>
    /// Get/Set the quality
    /// </summary>
    IQuality Quality { get; }

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    void ReloadConfiguration();

    #region properties

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    bool IsTunerLocked { get; }

    /// <summary>
    /// returns the signal quality
    /// </summary>
    int SignalQuality { get; }

    /// <summary>
    /// returns the signal level
    /// </summary>
    int SignalLevel { get; }

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    void ResetSignalUpdate();

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
    object Context { get; set; }

    #endregion

    #region idisposable

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    void Dispose();

    #endregion

    #region sub-channels

    /// <summary>
    /// Gets the sub-channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    ITvSubChannel GetSubChannel(int id);

    /// <summary>
    /// Frees the sub-channel.
    /// </summary>
    /// <param name="id">The id.</param>
    void FreeSubChannel(int id);

    /// <summary>
    /// Gets the sub-channels.
    /// </summary>
    /// <value>The sub-channels.</value>
    ITvSubChannel[] SubChannels { get; }

    #endregion

    /// <summary>
    /// Get the tuning parameters that have been applied to the hardware.
    /// This property returns null when the device is not in use.
    /// </summary>
    IChannel CurrentTuningDetail
    {
      get;
    }
  }
}