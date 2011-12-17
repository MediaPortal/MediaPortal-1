using System.Collections.Generic;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IUserManagement
  {
    /// <summary>
    /// Locks the card to the user specified
    /// </summary>
    /// <param name="user">The user.</param>
    void Lock(IUser user);

    /// <summary>
    /// Unlocks this card.
    /// </summary>
    /// <param name="user">The user.</param>
    /// 
    void Unlock(IUser user);

    /// <summary>
    /// Determines whether the specified user is owner of this card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    bool IsOwner(IUser user);

    /// <summary>
    /// Removes the user from this card
    /// </summary>
    /// <param name="user">The user.</param>
    void RemoveUser(IUser user);

    TvStoppedReason GetTvStoppedReason(IUser user);
    void SetTvStoppedReason(IUser user, TvStoppedReason reason);

    /// <summary>
    /// Determines whether the card is locked and ifso returns by which user
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified card is locked; otherwise, <c>false</c>.
    /// </returns>
    bool IsLocked(out IUser user);

    /// <summary>
    /// Gets the users for this card.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, IUser> Users { get; }

    bool HasEqualOrHigherPriority(IUser user);
    bool HasHighestPriority(IUser user);    
  }
}