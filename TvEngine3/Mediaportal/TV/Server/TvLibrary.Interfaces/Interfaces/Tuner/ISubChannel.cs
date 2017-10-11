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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner
{
  /// <summary>
  /// The library sub-channel interface.
  /// </summary>
  public interface ISubChannel
  {
    #region properties

    /// <summary>
    /// Get the sub-channel's identifier.
    /// </summary>
    int SubChannelId { get; }

    /// <summary>
    /// Get the time-shift buffer register file name.
    /// </summary>
    string TimeShiftFileName { get; }

    /// <summary>
    /// Get the date/time when time-shifting was started.
    /// </summary>
    DateTime TimeShiftStartTime { get; }

    /// <summary>
    /// Is the sub-channel currently time-shifting?
    /// </summary>
    bool IsTimeShifting { get; }

    /// <summary>
    /// Get the recording file name.
    /// </summary>
    string RecordFileName { get; }

    /// <summary>
    /// Get the date/time when recording was started.
    /// </summary>
    DateTime RecordStartTime { get; }

    /// <summary>
    /// Is the sub-channel currently recording?
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Get the channel which the sub-channel is tuned to.
    /// </summary>
    IChannel CurrentChannel { get; }

    #endregion

    #region time-shifting and recording

    /// <summary>
    /// Start time-shifting.
    /// </summary>
    /// <param name="fileName">The name to use for the time-shift buffer register file.</param>
    /// <param name="fileCount">The number of buffer files to use during normal time-shifting.</param>
    /// <param name="fileCountMaximum">The maximum number of buffer files to use when time-shifting is paused.</param>
    /// <param name="fileSize">The size of each buffer file.</param>
    void StartTimeShifting(string fileName, uint fileCount, uint fileCountMaximum, ulong fileSize);

    /// <summary>
    /// Get the current time-shift position.
    /// </summary>
    /// <param name="bufferId">The identifier of the current buffer file.</param>
    /// <param name="position">The position within the current buffer file.</param>
    /// <returns><c>true</c> if the position was retrieved successfully, otherwise <c>false</c></returns>
    bool GetCurrentTimeShiftPosition(out uint bufferId, out ulong position);

    /// <summary>
    /// Stop time-shifting.
    /// </summary>
    /// <returns><c>true</c> if time-shifting was stopped successfully, otherwise <c>false</c></returns>
    bool StopTimeShifting();

    /// <summary>
    /// Start recording.
    /// </summary>
    /// <param name="fileName">The name to use for the recording file.</param>
    void StartRecording(string fileName);

    /// <summary>
    /// Stop recording.
    /// </summary>
    /// <returns><c>true</c> if recording was stopped successfully, otherwise <c>false</c></returns>
    bool StopRecording();

    #endregion

    /// <summary>
    /// Get the stream state.
    /// </summary>
    /// <param name="isReceivingVideo"><c>True</c> if video is being received.</param>
    /// <param name="isEncryptedVideo"><c>True</c> if the received video is currently encrypted.</param>
    /// <param name="isReceivingAudio"><c>True</c> if audio is being received.</param>
    /// <param name="isEncryptedAudio"><c>True</c> if the received audio is currently encrypted.</param>
    void GetStreamState(out bool isReceivingVideo, out bool isEncryptedVideo, out bool isReceivingAudio, out bool isEncryptedAudio);

    /// <summary>
    /// Get information about the stream's quality.
    /// </summary>
    /// <param name="countBytes">The number of bytes processed.</param>    
    /// <param name="countDiscontinuities">The number of discontinuities encountered.</param>
    /// <param name="countDroppedBytes">The number of bytes dropped.</param>
    void GetStreamQuality(out ulong countBytes, out ulong countDiscontinuities, out ulong countDroppedBytes);
  }
}