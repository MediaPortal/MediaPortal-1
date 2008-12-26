/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// Interface to the epg grabber com object
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("5CDAC655-D9FB-4c71-8119-DD07FE86A9CE"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsEpgScanner
  {
    /// <summary>
    /// Start grabbing the EPG.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int GrabEPG();

    /// <summary>
    /// Determines whether EPG has been received
    /// </summary>
    /// <param name="yesNo">if set to <c>true</c> then epg is ready.</param>
    /// <returns></returns>
    [PreserveSig]
    int IsEPGReady(out bool yesNo);

    /// <summary>
    /// Gets the nummer of channels for which epg has been received
    /// </summary>
    /// <param name="channelCount">The channel count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGChannelCount([Out] out uint channelCount);

    /// <summary>
    /// Gets the number of epg events for a channel
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="eventCount">The event count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGEventCount([In] uint channel, [Out] out uint eventCount);

    /// <summary>
    /// Gets the EPG channel details.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="networkId">The network id.</param>
    /// <param name="transportid">The transportid.</param>
    /// <param name="service_id">The service_id.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGChannel([In] uint channel, [In, Out] ref UInt16 networkId, [In, Out] ref UInt16 transportid, [In, Out] ref UInt16 service_id);

    /// <summary>
    /// Gets the EPG event details.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="eventid">The eventid.</param>
    /// <param name="languageCount">The language count.</param>
    /// <param name="date">The date.</param>
    /// <param name="time">The time.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="genre">The genre.</param>
    /// <param name="starRating">The star rating</param>
    /// <param name="classification">The classification</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGEvent([In] uint channel, [In] uint eventid, [Out] out uint languageCount, [Out] out uint date, [Out] out uint time, [Out] out uint duration, out IntPtr genre, [Out] out int starRating, out IntPtr classification);

    /// <summary>
    /// Gets the EPG language.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="eventid">The eventid.</param>
    /// <param name="languageIndex">Index of the language.</param>
    /// <param name="language">The language.</param>
    /// <param name="eventText">The event text.</param>
    /// <param name="eventDescription">The event description.</param>
    /// <param name="parentalRating">The parental rating</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGLanguage([In] uint channel, [In] uint eventid, [In]uint languageIndex, [Out] out uint language, [Out] out IntPtr eventText, [Out] out IntPtr eventDescription, [Out] out int parentalRating);

    /// <summary>
    /// Start grabbing MGW
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int GrabMHW();

    /// <summary>
    /// Determines whether MHW has been received or not
    /// </summary>
    /// <param name="yesNo">if MHW has been received then <c>true</c> .</param>
    /// <returns></returns>
    [PreserveSig]
    int IsMHWReady(out bool yesNo);

    /// <summary>
    /// Gets the number of MHW titles received.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMHWTitleCount(out uint count);

    /// <summary>
    /// Gets the details for a MHW title.
    /// </summary>
    /// <param name="program">The program.</param>
    /// <param name="id">The id.</param>
    /// <param name="transportId">The transport id.</param>
    /// <param name="networkId">The network id.</param>
    /// <param name="channelId">The channel id.</param>
    /// <param name="programId">The program id.</param>
    /// <param name="themeId">The theme id.</param>
    /// <param name="PPV">The PPV.</param>
    /// <param name="Summaries">The summaries.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="dateStart">The date start.</param>
    /// <param name="timeStart">The time start.</param>
    /// <param name="title">The title.</param>
    /// <param name="programName">Name of the program.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMHWTitle(uint program, ref uint id, ref uint transportId, ref uint networkId, ref uint channelId, ref UInt32 programId, ref uint themeId, ref uint PPV, ref byte Summaries, ref uint duration, ref uint dateStart, ref uint timeStart, out IntPtr title, out IntPtr programName);

    /// <summary>
    /// Gets the details for a MHW channel.
    /// </summary>
    /// <param name="channelNr">The channel nr.</param>
    /// <param name="channelId">The channel id.</param>
    /// <param name="networkId">The network id.</param>
    /// <param name="transportId">The transport id.</param>
    /// <param name="channelName">Name of the channel.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMHWChannel(uint channelNr, ref uint channelId, ref uint networkId, ref uint transportId, out IntPtr channelName);

    /// <summary>
    /// Gets the MHW summary.
    /// </summary>
    /// <param name="programId">The program id.</param>
    /// <param name="summary">The summary.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMHWSummary(UInt32 programId, out IntPtr summary);

    /// <summary>
    /// Gets the MHW theme.
    /// </summary>
    /// <param name="themeId">The theme id.</param>
    /// <param name="theme">The theme.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMHWTheme(uint themeId, out IntPtr theme);

    /// <summary>
    /// Resets this MHW grabber.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();

    /// <summary>
    /// Aborts grabbing and calls the callback function
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int AbortGrabbing();

    /// <summary>
    /// Sets the call back which will be called when MHW has been received
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IEpgCallback callback);
  }
}
