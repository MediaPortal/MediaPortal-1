using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IUserManagement
  {
    IDictionary<int, ISubChannel> GetTunerSubChannels(int tunerId);
    bool CanSubChannelControlTuner(int subChannelId);
    int GetEffectivePriority(int subChannelId);
    int GetEffectivePriority(ISubChannel subChannel);
    bool StartTuning(int subChannelId, string userName, out ISubChannel subChannel, out string timeShiftOrRecordFileName);
    ISubChannel CreateSubChannel(string userName, UserType userType, int? subChannelPriorityOverride = null);
    void DeleteSubChannel(int subChannelId);
    bool CommitSubChannel(int subChannelId, int tuningDetailId, int tunerId);
    bool KickSubChannel(int subChannelId);
    bool FreeSubChannel(int subChannelId);
    bool ParkSubChannel(int subChannelId, double position);
    bool UnparkSubChannel(int subChannelId, out double position);
    HashSet<int> GetDistinctTimeShiftingUserPriorities();
    ICollection<int> GetRecordingChannelIds();


    /*IUser GetUserCopy(string name);

    /// <summary>
    /// Removes the user from this card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel"> </param>
    void RemoveUser(IUser user, int idChannel);

    bool HasEqualOrHigherPriority(IUser user);
    bool HasHighestPriority(IUser user);
    int GetTimeshiftingSubChannel(string userName);
    int GetRecentChannelId(string userName);
    int GetRecentSubChannelId(string userName);

    int GetSubChannelIdByChannelId(string userName, int idChannel, bool? isParked = null);

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
    ///   Sets the timeshifting stopped reason.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name = "reason">TvStoppedReason.</param>
    void SetTimeshiftStoppedReason(string userName, TvStoppedReason reason);

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

    void OnZap(IUser user, int idChannel);

    void SetOwnerSubChannel(int subChannelId, string userName);    
    bool IsAnyUserTimeShiftingOrRecording();
    void SetChannelStates(string name, Dictionary<int, ChannelState> channelStates);
    bool IsAnyUserTimeShifting();
    int UsersCount();
    bool IsAnyUserLockedOnChannel(int channelId, bool isParked);
    IEnumerable<int> GetAllSubChannelForChannel(int channelId, bool isParked);
    IDictionary<int, ChannelState> GetAllTimeShiftingAndRecordingChannelIds();
    IDictionary<string, IUser> UsersCopy { get; }
    IUser GetUserRecordingChannel(int idChannel);

    IEnumerable<IUser> GetUsersCopy(UserType? userType = null);
    IList<IUser> GetAllRecordingUsersCopy();
    int GetChannelId(string userName, bool isParked = false);

    void UpdatePrioritiesForAllUsers();

    // From parked user management
    void ParkUser(ref IUser user, double duration, int idChannel);
    void UnParkUser(ref IUser user, double duration, int idChannel);
    bool IsUserParkedOnChannel(string userName, int idChannel, out double parkedDuration, out DateTime parkedAt);
    bool IsUserParkedOnChannel(string userName, int idChannel);
    void CancelAllParkedUsers();
    bool HasAnyParkedUsers();
    void Dispose();
    bool HasParkedUserWithDuration(int channelId, double duration);*/
  }
}