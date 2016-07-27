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

using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An <see cref="ISubChannel"/> implementation for scanning the out-of-band (OOB) channel.
  /// </summary>
  /// <remarks>
  /// The OOB channel is also known as the forward data channel (FDC).
  /// </remarks>
  internal class SubChannelDriOutOfBandScan : SubChannelBase
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelDriOutOfBandScan"/> class.
    /// </summary>
    /// <param name="subChannelId">The sub-channel ID to associate with this instance.</param>
    public SubChannelDriOutOfBandScan(int subChannelId)
      : base(subChannelId)
    {
      this.LogDebug("DRI OOB scan sub-channel: new sub-channel, ID = {0}", _subChannelId);
    }

    #region sub-channel base implementations/overrides

    /// <summary>
    /// Get the stream state.
    /// </summary>
    /// <param name="isReceivingVideo"><c>True</c> if video is being received.</param>
    /// <param name="isEncryptedVideo"><c>True</c> if the received video is currently encrypted.</param>
    /// <param name="isReceivingAudio"><c>True</c> if audio is being received.</param>
    /// <param name="isEncryptedAudio"><c>True</c> if the received audio is currently encrypted.</param>
    public override void GetStreamState(out bool isReceivingVideo, out bool isEncryptedVideo, out bool isReceivingAudio, out bool isEncryptedAudio)
    {
      isReceivingVideo = false;
      isEncryptedVideo = true;
      isReceivingAudio = false;
      isEncryptedAudio = true;
    }

    /// <summary>
    /// Get information about the stream's quality.
    /// </summary>
    /// <param name="countBytes">The number of bytes processed.</param>    
    /// <param name="countDiscontinuities">The number of discontinuities encountered.</param>
    /// <param name="countDroppedBytes">The number of bytes dropped.</param>
    public override void GetStreamQuality(out ulong countBytes, out ulong countDiscontinuities, out ulong countDroppedBytes)
    {
      countBytes = 0;
      countDiscontinuities = 0;
      countDroppedBytes = 0;
    }

    /// <summary>
    /// Implementation of starting time-shifting.
    /// </summary>
    /// <param name="fileName">The name to use for the time-shift buffer register file.</param>
    /// <param name="fileCount">The number of buffer files to use during normal time-shifting.</param>
    /// <param name="fileCountMaximum">The maximum number of buffer files to use when time-shifting is paused.</param>
    /// <param name="fileSize">The size of each buffer file.</param>
    protected override void OnStartTimeShifting(string fileName, uint fileCount, uint fileCountMaximum, ulong fileSize)
    {
      throw new TvException("DRI out-of-band scan sub-channels cannot time-shift or record.");
    }

    /// <summary>
    /// Implementation of getting the current time-shift position.
    /// </summary>
    /// <param name="bufferId">The identifier of the current buffer file.</param>
    /// <param name="position">The position within the current buffer file.</param>
    protected override void OnGetCurrentTimeShiftPosition(out uint bufferId, out ulong position)
    {
      bufferId = 0;
      position = 0;
    }

    /// <summary>
    /// Implementation of stopping time-shifting.
    /// </summary>
    protected override void OnStopTimeShifting()
    {
      throw new TvException("DRI out-of-band scan sub-channels cannot time-shift or record.");
    }

    /// <summary>
    /// Implementation of starting recording.
    /// </summary>
    /// <param name="fileName">The name to use for the recording file.</param>
    protected override void OnStartRecording(string fileName)
    {
      throw new TvException("DRI out-of-band scan sub-channels cannot time-shift or record.");
    }

    /// <summary>
    /// Implementation of stopping recording.
    /// </summary>
    protected override void OnStopRecording()
    {
      throw new TvException("DRI out-of-band scan sub-channels cannot time-shift or record.");
    }

    /// <summary>
    /// Implementation of tune cancellation.
    /// </summary>
    protected override void OnCancelTune()
    {
    }

    /// <summary>
    /// Implementation of decomposition.
    /// </summary>
    protected override void OnDecompose()
    {
    }

    #endregion
  }
}