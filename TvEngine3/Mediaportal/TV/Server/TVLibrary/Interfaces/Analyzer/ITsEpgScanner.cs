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
using System.Runtime.InteropServices;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// call back interface for the EPG
  /// </summary>
  [Guid("FFAB5D98-2309-4d90-9C71-E4B2F490CF5A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEpgCallBack
  {
    /// <summary>
    /// Called when epg is received.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int OnEpgReceived();
  }

  /// <summary>
  /// Interface to the epg grabber com object
  /// </summary>
  [Guid("5CDAC655-D9FB-4c71-8119-DD07FE86A9CE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface ITsEpgScanner
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
    int IsEPGReady([MarshalAs(UnmanagedType.Bool)] out bool yesNo);

    /// <summary>
    /// Gets the nummer of channels for which epg has been received
    /// </summary>
    /// <param name="channelCount">The channel count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGChannelCount(out uint channelCount);

    /// <summary>
    /// Gets the number of epg events for a channel
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="eventCount">The event count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGEventCount(uint channel, out uint eventCount);

    /// <summary>
    /// Gets the EPG channel details.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="networkId">The network id.</param>
    /// <param name="transportid">The transportid.</param>
    /// <param name="service_id">The service_id.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGChannel(uint channel, out ushort networkId, out ushort transportid, out ushort service_id);

    /// <summary>
    /// Gets the EPG event details.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="eventid">The eventid.</param>
    /// <param name="languageCount">The language count.</param>
    /// <param name="startDateTimeEpoch">The start date/time.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="genre">The genre.</param>
    /// <param name="starRating">The star rating</param>
    /// <param name="classification">The classification</param>
    /// <returns></returns>
    [PreserveSig]
    int GetEPGEvent(uint channel, uint eventid, out uint languageCount, out ulong startDateTimeEpoch,
                    out ushort duration, out IntPtr genre, out int starRating,
                    out IntPtr classification);

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
    int GetEPGLanguage(uint channel, uint eventid, uint languageIndex, out uint language,
                       out IntPtr eventText, out IntPtr eventDescription, out int parentalRating);

    /// <summary>
    /// Start grabbing Media Highway EPG data.
    /// </summary>
    /// <returns>an HRESULT indicating whether grabbing was started successfully</returns>
    [PreserveSig]
    int GrabMhw();

    /// <summary>
    /// Determine whether Media Highway EPG data grabbing is complete.
    /// </summary>
    /// <param name="yesNo"><c>True</c> if EPG data grabbing is complete.</param>
    /// <returns>an HRESULT indicating whether the grab state was retrieved successfully</returns>
    [PreserveSig]
    int IsMhwReady([MarshalAs(UnmanagedType.I1)] out bool yesNo);

    /// <summary>
    /// Get the number of Media Highway EPG programs that are available.
    /// </summary>
    /// <param name="count">The number of available programs.</param>
    /// <returns>an HRESULT indicating whether the program count was retrieved successfully</returns>
    [PreserveSig]
    int GetMhwProgramCount(out uint count);

    /// <summary>
    /// Get the details for a Media Highway EPG program.
    /// </summary>
    /// <param name="index">The program's index.</param>
    /// <param name="id">The program's identifier.</param>
    /// <param name="channelIndex">The index of the channel which the program is associated with.</param>
    /// <param name="themeId">The identifier of the theme associated with the program.</param>
    /// <param name="subThemeId">The identifier of the sub-theme associated with the program.</param>
    /// <param name="startDateTimeEpoch">The program's start date/time epoch.</param>
    /// <param name="hasSummary"><c>True</c> if the program has a summary.</param>
    /// <param name="duration">The duration of the program, in minutes.</param>
    /// <param name="title">The program's title (name).</param>
    /// <param name="payPerViewId">The pay-per-view identifier associated with the program.</param>
    /// <returns>an HRESULT indicating whether the program details were retrieved successfully</returns>
    [PreserveSig]
    int GetMhwProgram(uint index, out uint id, out byte channelIndex,
                      out byte themeId, out byte subThemeId,
                      out ulong startDateTimeEpoch,
                      [MarshalAs(UnmanagedType.I1)] out bool hasSummary,
                      out ushort duration, out IntPtr title,
                      out uint payPerViewId);

    /// <summary>
    /// Gets the details for a Media Highway EPG channel.
    /// </summary>
    /// <param name="index">The channel's index.</param>
    /// <param name="originalNetworkId">The channel's original network identifier.</param>
    /// <param name="transportStreamId">The channel's transport stream identifier.</param>
    /// <param name="serviceId">The channel's service identifier.</param>
    /// <param name="name">The channel's name.</param>
    /// <returns>an HRESULT indicating whether the channel details were retrieved successfully</returns>
    [PreserveSig]
    int GetMhwChannel(byte index, out ushort originalNetworkId,
                      out ushort transportStreamId, out ushort serviceId,
                      out IntPtr name);

    /// <summary>
    /// Get the summary for a Media Highway EPG program.
    /// </summary>
    /// <param name="programId">The program's identifier.</param>
    /// <param name="summary">The program's summary.</param>
    /// <param name="lineCount">The number of lines available as part of the summary.</param>
    /// <returns>an HRESULT indicating whether the summary was retrieved successfully</returns>
    [PreserveSig]
    int GetMhwSummary(uint programId, out IntPtr summary, out byte lineCount);

    /// <summary>
    /// Get a summary line for a Media Highway EPG program.
    /// </summary>
    /// <param name="programId">The program's identifier.</param>
    /// <param name="index">The line's index.</param>
    /// <param name="line">The summary line.</param>
    /// <returns>an HRESULT indicating whether the summary line was retrieved successfully</returns>
    [PreserveSig]
    int GetMhwSummaryLine(uint programId, byte index, out IntPtr line);

    /// <summary>
    /// Get the name for a Media Highway EPG program theme.
    /// </summary>
    /// <param name="id">The theme's identifier.</param>
    /// <param name="name">The name of the theme.</param>
    /// <returns>an HRESULT indicating whether the theme name was retrieved successfully</returns>
    [PreserveSig]
    int GetMhwThemeName(byte id, out IntPtr name);

    /// <summary>
    /// Get the name for a Media Highway EPG program sub-theme.
    /// </summary>
    /// <param name="themeId">The theme's identifier.</param>
    /// <param name="subThemeId">The sub-theme's identifier.</param>
    /// <param name="name">The name of the sub-theme.</param>
    /// <returns>an HRESULT indicating whether the sub-theme name was retrieved successfully</returns>
    [PreserveSig]
    int GetMhwSubThemeName(byte themeId, byte subThemeId, out IntPtr name);

    /// <summary>
    /// Resets this MHW grabber.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();

    /// <summary>
    /// Aborts grabbing and calls the call back function
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int AbortGrabbing();

    /// <summary>
    /// Sets the call back which will be called when MHW has been received
    /// </summary>
    /// <param name="callBack">The call back.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IEpgCallBack callBack);
  }
}