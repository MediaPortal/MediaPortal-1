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

    void OnSuspend();
    void OnResume();

    /// <summary>
    /// Cleans up the controller
    /// </summary>
    void DeInit();

    void Init();
    void Init(OnPluginStatesChanged pluginStateChangeHandler);
    event TvServerEventHandler OnTvServerEvent;
    IDictionary<int, ITvCardHandler> CardCollection { get; }

    IUserManagement UserManagement { get; }

    List<IVirtualCard> GetAllRecordingCards();

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
    bool StopTimeShifting(ref IUser user, int channelId, TvStoppedReason? reason = null);
  }
}