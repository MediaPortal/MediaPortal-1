#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Epg;
using TvLibrary.ChannelLinkage;

namespace TvLibrary.Implementations.Hybrid
{
  /// <summary>
  /// This class is a wrapper for all cards that are part of a hybrid card group
  /// </summary>
  public class HybridCard : ITVCard
  {
    #region variables

    /// <summary>
    /// Hybrid card group
    /// </summary>
    private readonly HybridCardGroup _group;

    /// <summary>
    /// Internal card
    /// </summary>
    private readonly ITVCard _internalCard;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCard"/> class.
    /// </summary>
    /// <param name="group">The corresponding group for this card wrapper</param>
    /// <param name="internalCard">The internal card for this wrapper</param>
    public HybridCard(HybridCardGroup group, ITVCard internalCard)
    {
      _group = group;
      _internalCard = internalCard;
    }

    #endregion

    #region methods

    /// <summary>
    /// Checks if the active card is the one with given id
    /// </summary>
    /// <param name="idCard">ID of the card to check</param>
    /// <returns> 
    ///   <c>true</c> if card with the given id is active; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCardIdActive(int idCard)
    {
      return _group.IsCardIdActive(idCard);
    }

    #endregion

    #region properties

    /// <summary>
    /// returns true if card is currently present
    /// </summary>
    public bool CardPresent
    {
      get { return _internalCard.CardPresent; }
      set { _internalCard.CardPresent = value; }
    }

    /// <summary>
    /// Does the card have a CA module.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public bool HasCA
    {
      get { return _internalCard.HasCA; }
    }

    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get { return _internalCard.NumberOfChannelsDecrypting; }
    }

    /// <summary>
    /// Gets or sets the timeout parameters.
    /// </summary>
    /// <value>The parameters.</value>
    public ScanParameters Parameters
    {
      get { return _group.Parameters; }
      set { _group.Parameters = value; }
    }

    #endregion

    #region ITVCard Members

    /// <summary>
    /// Gets a value indicating whether card supports subchannels
    /// </summary>
    /// <value><c>true</c> if card supports sub channels; otherwise, <c>false</c>.</value>
    public bool SupportsSubChannels
    {
      get { return _internalCard.SupportsSubChannels; }
    }

    /// <summary>
    /// Returns if the tuner belongs to a hybrid card
    /// </summary>
    public bool IsHybrid
    {
      get { return false; }
      set { }
    }

    /// <summary>
    /// Gets/sets the card name
    /// </summary>
    /// <value></value>
    public string Name
    {
      get { return _internalCard.Name; }
      set { _internalCard.Name = value; }
    }

    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    /// <value></value>
    public string DevicePath
    {
      get { return _internalCard.DevicePath; }
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
      return _internalCard.CanTune(channel);
    }

    /// <summary>
    /// Stops the current graph
    /// </summary>
    public void StopGraph()
    {
      _group.StopGraph();
    }


    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    /// <value></value>
    public bool IsEpgGrabbing
    {
      get { return _internalCard.IsEpgGrabbing; }
      set { _group.IsEpgGrabbing = value; }
    }

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    /// <value></value>
    public bool IsScanning
    {
      get { return _internalCard.IsScanning; }
      set { _group.IsScanning = value; }
    }

    /// <summary>
    /// returns the min. channel number for analog cards
    /// </summary>
    /// <value></value>
    public int MinChannel
    {
      get { return _internalCard.MinChannel; }
    }

    /// <summary>
    /// returns the max. channel number for analog cards
    /// </summary>
    /// <value>The max channel.</value>
    public int MaxChannel
    {
      get { return _internalCard.MaxChannel; }
    }

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get { return _internalCard.CamType; }
      set { _internalCard.CamType = value; }
    }

    /// <summary>
    /// Gets/sets the card type
    /// </summary>
    /// <value></value>
    public CardType CardType
    {
      get { return _internalCard.CardType; }
    }

    /// <summary>
    /// Gets the interface for controlling the diseqc motor
    /// </summary>
    /// <value>Theinterface for controlling the diseqc motor.</value>
    public IDiSEqCMotor DiSEqCMotor
    {
      get { return _internalCard.DiSEqCMotor; }
    }

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
      _group.StartLinkageScanner(callback);
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner()
    {
      _group.ResetLinkageScanner();
    }

    /// <summary>
    /// Returns the channel linkages grabbed
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get { return _group.ChannelLinkages; }
    }

    /// <summary>
    /// Grabs the epg.
    /// </summary>
    /// <param name="callback">The callback which gets called when epg is received or canceled.</param>
    public void GrabEpg(BaseEpgGrabber callback)
    {
      _group.GrabEpg(callback);
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public void GrabEpg()
    {
      _group.GrabEpg();
    }

    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public void AbortGrabbing()
    {
      _group.AbortGrabbing();
    }

    /// <summary>
    /// returns a list of all epg data for each channel found.
    /// </summary>
    /// <value>The epg.</value>
    public List<EpgChannel> Epg
    {
      get { return _group.Epg; }
    }

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public ITVScanning ScanningInterface
    {
      get { return _internalCard.ScanningInterface; }
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      return _group.Tune(subChannelId, channel);
    }

    /// <summary>
    /// Get/Set the quality
    /// </summary>
    /// <value></value>
    public IQuality Quality
    {
      get { return _internalCard.Quality; }
      set { _internalCard.Quality = value; }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    /// <value></value>
    public bool SupportsQualityControl
    {
      get { return _group.SupportsQualityControl; }
    }

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    /// <value></value>
    public bool IsTunerLocked
    {
      get { return _internalCard.IsTunerLocked; }
    }

    /// <summary>
    /// returns the signal quality
    /// </summary>
    /// <value></value>
    public int SignalQuality
    {
      get { return _group.SignalQuality; }
    }

    /// <summary>
    /// returns the signal level
    /// </summary>
    /// <value></value>
    public int SignalLevel
    {
      get { return _group.SignalLevel; }
    }

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    public void ResetSignalUpdate()
    {
      _group.ResetSignalUpdate();
    }

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
    public object Context
    {
      get { return _group.Context; }
      set { _group.Context = value; }
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      _internalCard.Dispose();
    }

    /// <summary>
    /// Gets the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public ITvSubChannel GetSubChannel(int id)
    {
      return _group.GetSubChannel(id);
    }

    /// <summary>
    /// Gets the sub channels.
    /// </summary>
    /// <value>The sub channels.</value>
    public ITvSubChannel[] SubChannels
    {
      get { return _group.SubChannels; }
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void FreeSubChannel(int id)
    {
      _group.FreeSubChannel(id);
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="subchannelBusy">is the subcannel busy with other users.</param>
    public void FreeSubChannelContinueGraph(int id, bool subchannelBusy)
    {
      FreeSubChannelContinueGraph(id);
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void FreeSubChannelContinueGraph(int id)
    {
      _group.FreeSubChannel(id);
    }

    /// <summary>
    /// Reloads the card configuration
    /// </summary>
    public void ReloadCardConfiguration()
    {
      _internalCard.ReloadCardConfiguration();
    }

    #endregion
  }
}