using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{

  public delegate void TvServerEventHandler(object sender, EventArgs eventArgs);
  public delegate void OnPluginStatesChanged();

  public interface IInternalControllerService : IControllerService
  {

    /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardIndex">Index of the card.</param>
    /// <value>id of card</value>    
    int CardId(int cardIndex);

    /// <summary>
    /// Returns if the card is currently recording or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// true when card is recording otherwise false
    /// </returns>    
    bool IsRecording(ref IUser user);

    bool CanSuspend { get; }

    /// <summary>
    /// Returns a dictionary of channels that are timeshfiting and recording.
    /// </summary>
    IDictionary<int, ChannelState> GetAllTimeshiftingAndRecordingChannels();

    void ExecutePendingDeletions();
    void OnSuspend();
    void OnResume();

    /// <summary>
    /// Cleans up the controller
    /// </summary>
    void DeInit();

    void Init();
    void Init(OnPluginStatesChanged pluginStateChangeHandler);
    event TvServerEventHandler OnTvServerEvent;
    IDictionary<int, ChannelState> GetAllChannelStatesForIdleUserCached();
    IDictionary<int, ITvCardHandler> CardCollection { get; }
    bool AllCardsIdle { get; }

    /// <summary>
    /// returns if the card is enabled or disabled
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>true if enabled, otherwise false</value>
    bool IsCardEnabled(int cardId);

    List<IVirtualCard> GetAllRecordingCards();

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="ticket">card reservation ticket</param>
    /// <returns>true if succeeded</returns>
    TvResult Tune(ref IUser user, IChannel channel, int idChannel, object ticket);

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="ticket">card reservation ticket</param>
    /// <param name="cardResImpl"></param>
    /// <returns>true if succeeded</returns>
    TvResult Tune(ref IUser user, IChannel channel, int idChannel, object ticket, object cardResImpl);


    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="callBack">EPG grabber</param>    
    /// <param name="user"> </param>
    /// <returns></returns>
    bool GrabEpg(IEpgGrabberCallBack callBack, IUser user);

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    void AbortEPGGrabbing(int cardId);

    /// <summary>
    /// Call back invoked by EPG grabbers when import for a channel is completed.
    /// </summary>
    /// <param name="channelId">The imported channel's identifier.</param>
    void OnImportEpgPrograms(int channelId);

    TvResult StartTimeShifting(ref IUser user, ref string timeshiftFileName, int idChannel);

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="user">User</param>
    void StopGrabbingEpg(IUser user);

    /// <summary>
    /// Fires an ITvServerEvent to plugins.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void Fire(object sender, EventArgs args);    
    bool StopTimeShifting(ref IUser user, TvStoppedReason reason, int channelId);
    bool StopTimeShifting(ref IUser user, int channelId);
    bool IsScrambled(int cardId, int subChannel);
  }
}