/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using TvLibrary.Teletext;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Sub Channel interface in TsWriter
  /// </summary>
  public interface ITvSubChannel
  {
    #region properties
    /// <summary>
    /// Gets the sub channel id.
    /// </summary>
    /// <value>The sub channel id.</value>
    int SubChannelId { get;}
    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    string TimeShiftFileName { get;}

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    DateTime StartOfTimeShift { get;}

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    DateTime RecordingStarted { get;}

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    bool IsReceivingAudioVideo { get;}

    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    string RecordingFileName { get;}

    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    bool IsRecording { get;}

    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    bool IsTimeShifting { get;}


    /// <summary>
    /// returns the IChannel to which the card is currently tuned
    /// </summary>
    IChannel CurrentChannel { get;}

    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    bool IsTimeshiftingTransportStream { get;}

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    bool IsRecordingTransportStream { get;}
    #endregion

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    int GetCurrentVideoStream { get; }

    #region teletext
    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    bool GrabTeletext { get;set;}

    /// <summary>
    /// returns the ITeletext interface used for retrieving the teletext pages
    /// </summary>
    ITeletext TeletextDecoder { get;}

    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    bool HasTeletext { get;}

    #endregion

    #region timeshifting and recording
    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns>true if succeeded else false</returns>
    bool StartTimeShifting(string fileName);

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns>true if succeeded else false</returns>
    bool StopTimeShifting();

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">if true, then record transport stream</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns>true if succeeded else false</returns>
    bool StartRecording(bool transportStream, string fileName);

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns>true if succeeded else false</returns>
    bool StopRecording();
    #endregion


    #region audio streams
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    List<IAudioStream> AvailableAudioStreams { get;}

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    IAudioStream CurrentAudioStream { get;set;}
    #endregion

  }
}
