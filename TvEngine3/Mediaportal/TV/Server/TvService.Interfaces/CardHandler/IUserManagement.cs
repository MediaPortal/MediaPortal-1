using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IUserManagement
  {


    IUser GetUserCopy(string name);

    /// <summary>
    /// Removes the user from this card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel"> </param>
    void RemoveUser(IUser user, int idChannel);

    

    /// <summary>
    /// Gets the users for this card.
    /// </summary>
    /// <returns></returns>
    //IDictionary<string, IUser> Users { get; }

    bool HasEqualOrHigherPriority(IUser user);
    bool HasHighestPriority(IUser user);
    int GetTimeshiftingSubChannel(string userName);
    int GetTimeshiftingChannelId(string userName);
    int GetRecentChannelId(string userName);
    int GetRecentSubChannelId(string userName);
    void AddSubChannel(IUser user, int id, int idChannel, TvUsage tvUsage);
    int GetSubChannelIdByChannelId(string userName, int idChannel);
    int GetSubChannelIdByChannelId(string userName, int idChannel, TvUsage tvUsage);

    /// <summary>
    ///   Determines whether the the card is locked and ifso returns by which used.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is locked; otherwise, <c>false</c>.
    /// </returns>
    bool IsLocked(out IUser user);

    /// <summary>
    ///   Determines whether the specified user is owner.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    bool IsOwner(string name);

    /// <summary>
    ///   Adds the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name="idChannel"> </param>
    /// <param name="subChannelId"> </param>
    void AddSubChannelOrUser(IUser user, int idChannel, int subChannelId);

    void RemoveChannelFromUser(IUser user, int subChannelId);

    /// <summary>
    ///   Removes the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    void RemoveUser(IUser user);
    

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "exists">IUser exists</param>
    void RefreshUser(ref IUser user);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "userExists">IUser exists</param>
    void RefreshUser(ref IUser user, out bool userExists);

    /// <summary>
    ///   Returns if the user exists or not
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns></returns>
    bool DoesUserExist(string userName);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel idChannel.</param>
    /// <param name = "user">The user.</param>
    IUser GetUserCopy(int subChannelId);

    /// <summary>
    ///   Sets the timeshifting stopped reason.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name = "reason">TvStoppedReason.</param>
    void SetTimeshiftStoppedReason(string userName, TvStoppedReason reason);

    /// <summary>
    ///   Gets the timeshifting stopped reason.
    /// </summary>
    /// <param name="userName"> </param>
    TvStoppedReason GetTimeshiftStoppedReason(string userName);

    int GetNextAvailableSubchannel(string userName);
    bool HasUserEqualOrHigherPriority(IUser user);
    bool HasUserHighestPriority(IUser user);

    /// <summary>
    ///   Determines whether one or more users exist for the given subchannel
    /// </summary>
    /// <param name = "subchannelId">The subchannel idChannel.</param>
    /// <returns>
    ///   <c>true</c> if users exists; otherwise, <c>false</c>.
    /// </returns>
    bool ContainsUsersForSubchannel(int subchannelId);

    /// <summary>
    ///   Removes all users
    /// </summary>
    void Clear();

    void OnStopUser(IUser user);
    void OnZap(IUser user, int idChannel);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel idChannel.</param>
    /// <param name = "user">The user.</param>
    ISubChannel GetSubChannel(string userName, int subChannelId);

    ISubChannel GetSubChannelByChannelId(string userName, int idChannel);
    void SetOwnerSubChannel(int subChannelId, string userName);    
    int NumberOfOtherUsers(string name);
    int GetNumberOfUsersOnChannel(int currentChannelId);
    bool IsAnyUserTimeShiftingOrRecording();
    bool IsAnyUserOnTuningDetail(IChannel tuningDetail);
    IList<IUser> GetActiveUsersCopy();
    void SetChannelStates(string name, Dictionary<int, ChannelState> channelStates);
    bool IsAnyUserTimeShifting();
    int UsersCount();
    bool IsAnyUserLockedOnChannel(int channelId, TvUsage tvUsage);
    bool IsAnyUserLockedOnChannel(int channelId);
    IEnumerable<int> GetAllSubChannelForChannel(int channelId, TvUsage tvUsage);
    IDictionary<int, ChannelState> GetAllTimeShiftingAndRecordingChannelIds();
    IDictionary<string, IUser> UsersCopy { get; }
    IUser GetUserRecordingChannel(int idChannel);

    IEnumerable<IUser> GetUsersCopy(UserType? userType = null);
    IList<IUser> GetAllRecordingUsersCopy();
    int GetChannelId(string userName, TvUsage tvUsage);
  }
}