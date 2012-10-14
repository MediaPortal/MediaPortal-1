using System.Collections.Generic;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public delegate void OnAfterCancelTuneDelegate(int subchannel);
  public delegate void OnAfterTuneDelegate(ITvCardHandler cardHandler);
  public delegate void OnBeforeTuneDelegate(ITvCardHandler cardHandler);

  public interface ICardTuner
  {    
    object CardReservationsLock { get; }
    CardTuneState CardTuneState { get; set; }
    ICardTuneReservationTicket ActiveCardTuneReservationTicket { get; set; }
    List<ICardTuneReservationTicket> ReservationsForTune { get; }
    CardStopState CardStopState { get; set; }
    List<ICardStopReservationTicket> ReservationsForStop { get; }
    bool HasActiveCardTuneReservationTicket { get; }

    /// <summary>
    /// Scans the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    TvResult Scan(ref IUser user, IChannel channel, int idChannel);

    void CancelTune(int subchannel);

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    TvResult Tune(ref IUser user, IChannel channel, int idChannel);

    void CleanUpPendingTune(int pendingSubchannel);
    event OnAfterCancelTuneDelegate OnAfterCancelTuneEvent;
    event OnAfterTuneDelegate OnAfterTuneEvent;
    event OnBeforeTuneDelegate OnBeforeTuneEvent;

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="dbChannel">The db channel</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult CardTune(ref IUser user, IChannel channel, Channel dbChannel);

    /// <summary>
    /// Determines whether card is tuned to the transponder specified by transponder
    /// </summary>
    /// <param name="transponder">The transponder.</param>
    /// <returns>
    /// 	<c>true</c> if card is tuned to the transponder; otherwise, <c>false</c>.
    /// </returns>
    bool IsTunedToTransponder(IChannel transponder);

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    bool CanTune(IChannel channel);

    void FreeAllTimeshiftingSubChannels(ref IUser user);
  }
}