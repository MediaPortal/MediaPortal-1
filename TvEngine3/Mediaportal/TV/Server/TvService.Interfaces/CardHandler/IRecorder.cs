using System;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IRecorder
  {
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns></returns>
    TvResult Start(ref IUser user, ref string fileName);

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    bool Stop(ref IUser user);

    /// <summary>
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    bool IsAnySubChannelRecording { get; }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>true when card is recording otherwise false</returns>
    bool IsRecording(string userName);

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>filename or null when not recording</returns>
    string FileName(string userName);

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    DateTime RecordingStarted(string userName);

    void ReloadConfiguration();
  }
}