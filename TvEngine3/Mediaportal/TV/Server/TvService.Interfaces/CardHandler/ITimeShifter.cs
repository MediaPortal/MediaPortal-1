using System;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface ITimeShifter
  {
    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    string FileName(ref IUser user);

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    bool GetCurrentFilePosition(ref IUser user, ref Int64 position, ref long bufferId);

    /// <summary>
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    bool IsAnySubChannelTimeshifting { get; }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    bool IsTimeShifting(ref IUser user);

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    DateTime TimeShiftStarted(IUser user, int idChannel);

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <param name="subChannelId"> </param>
    /// <param name="idChannel"> </param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult Start(ref IUser user, ref string fileName, int subChannelId, int idChannel);

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns></returns>    
    bool Stop(ref IUser user, int idChannel);

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>   
    /// <param name="user">user</param>    
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    void GetStreamQualityCounters(IUser user, out int totalTSpackets, out int discontinuityCounter);

    void OnBeforeTune();
    void OnAfterTune();
  }
}