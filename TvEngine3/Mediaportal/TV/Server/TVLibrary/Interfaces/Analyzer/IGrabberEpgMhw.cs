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
  /// A MediaHighway electronic programme guide grabber interface.
  /// </summary>
  [Guid("83943f71-4a5a-4dde-9efc-d031fe77a85e"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGrabberEpgMhw
  {
    /// <summary>
    /// Set the grabber's call back delegate.
    /// </summary>
    /// <param name="callBack">The delegate.</param>
    [PreserveSig]
    void SetCallBack(ICallBackGrabber callBack);

    /// <summary>
    /// Enable or disable the grabber's protocols.
    /// </summary>
    /// <remarks>
    /// The grabber is stopped if all supported protocols are disabled.
    /// </remarks>
    /// <param name="grabMhw1"><c>True</c> to enable MediaHighway version 1 grabbing.</param>
    /// <param name="grabMhw2"><c>True</c> to enable MediaHighway version 2 grabbing.</param>
    [PreserveSig]
    void SetProtocols([MarshalAs(UnmanagedType.I1)] bool grabMhw1,
                      [MarshalAs(UnmanagedType.I1)] bool grabMhw2);

    /// <summary>
    /// Check if the grabber has received any sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsSeen();

    /// <summary>
    /// Check if the grabber has received all available sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReady();

    /// <summary>
    /// Get the number of MediaHighway events received by the grabber.
    /// </summary>
    /// <param name="eventCount">The number of MediaHighway events received by the grabber.</param>
    /// <param name="textLanguage">The language for all event text.</param>
    [PreserveSig]
    void GetEventCount(out uint eventCount, out Iso639Code textLanguage);

    /// <summary>
    /// Retrieve a MediaHighway event's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the event to retrieve. Should be in the range 0 to GetEventCount() - 1.</param>
    /// <param name="version">The version of the Media Highway protocol which the event data conforms with.</param>
    /// <param name="eventId">The event's identifier. Should be unique, even across services.</param>
    /// <param name="originalNetworkId">The original network identifier of the service that the event is associated with.</param>
    /// <param name="transportStreamId">The transport stream identifier of the service that the event is associated with.</param>
    /// <param name="serviceId">The service identifier of the service that the event is associated with.</param>
    /// <param name="serviceName">A buffer containing the name of the service that the event is associated with, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="serviceNameBufferSize">As an input, the size of the <paramref name="serviceName">service name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="startDateTime">The event's start date/time, encoded as an epoch/Unix/POSIX time-stamp.</param>
    /// <param name="duration">The event's duration in minutes.</param>
    /// <param name="title">A buffer containing the event's title, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="titleBufferSize">As an input, the size of the <paramref name="title">title buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="description">A buffer containing the event's description, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="descriptionBufferSize">As an input, the size of the <paramref name="description">description buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="descriptionLineCount">The number of additional description lines associated with the event.</param>
    /// <param name="seriesId">The identifier that links this event to other events from the same series. Value is <c>0xffffffff</c> if not available.</param>
    /// <param name="seasonName">A buffer containing the name of the season that the event is associated with, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="seasonNameBufferSize">As an input, the size of the <paramref name="seasonName">season name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="episodeId">The identifier of the episode or program that the event is associated with. Value is <c>0xffffffff</c> if not available.</param>
    /// <param name="episodeNumber">The human-readable identifier for the episode or program that the event is associated with. Value is <c>0</c> if not available.</param>
    /// <param name="episodeName">A buffer containing the name of the episode that the event is associated with, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="episodeNameBufferSize">As an input, the size of the <paramref name="episodeName">episode name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="themeName">A buffer containing the name of the theme that the event is associated with, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="themeNameBufferSize">As an input, the size of the <paramref name="themeName">theme name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="subThemeName">A buffer containing the name of the sub-theme that the event is associated with, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="subThemeNameBufferSize">As an input, the size of the <paramref name="subThemeName">sub-theme name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="classification">The event's classification (parental rating). Value is <c>0xff</c> if not available.</param>
    /// <param name="isHighDefinition">An indication of whether the event's video will be high definition. Only applicable for Media Highway version 2 events.</param>
    /// <param name="hasSubtitles">An indication of whether the event will have subtitles available. Only applicable for Media Highway version 2 events.</param>
    /// <param name="isRecommended">An indication of whether the event is recommended by the provider.</param>
    /// <param name="isPayPerView">An indication of whether viewing the event requires payment.</param>
    /// <param name="payPerViewId">The event's pay-per-view identifier, if any. Only applicable for Media Highway version 1 events.</param>
    /// <returns><c>true</c> if the event's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetEvent(uint index,
                  out byte version,
                  out uint eventId,
                  out ushort originalNetworkId,
                  out ushort transportStreamId,
                  out ushort serviceId,
                  IntPtr serviceName,
                  ref ushort serviceNameBufferSize,
                  out ulong startDateTime,
                  out ushort duration,
                  IntPtr title,
                  ref ushort titleBufferSize,
                  IntPtr description,
                  ref ushort descriptionBufferSize,
                  out byte descriptionLineCount,
                  out uint seriesId,
                  IntPtr seasonName,
                  ref ushort seasonNameBufferSize,
                  out uint episodeId,
                  out ushort episodeNumber,
                  IntPtr episodeName,
                  ref ushort episodeNameBufferSize,
                  IntPtr themeName,
                  ref ushort themeNameBufferSize,
                  IntPtr subThemeName,
                  ref ushort subThemeNameBufferSize,
                  out byte classification,
                  [MarshalAs(UnmanagedType.I1)] out bool isHighDefinition,
                  [MarshalAs(UnmanagedType.I1)] out bool hasSubtitles,
                  [MarshalAs(UnmanagedType.I1)] out bool isRecommended,
                  [MarshalAs(UnmanagedType.I1)] out bool isPayPerView,
                  out uint payPerViewId);

    /// <summary>
    /// Retrieve a description line for a MediaHighway event from the grabber.
    /// </summary>
    /// <param name="eventIndex">The index of the event that the line is associated with. Should be in the range 0 to GetEventCount() - 1.</param>
    /// <param name="lineIndex">The index of the line to retrieve. Should be in the range 0 to descriptionLineCount - 1 for the event.</param>
    /// <param name="line">A buffer containing the description line, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="lineBufferSize">As an input, the size of the <paramref name="line"/> buffer; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the description line is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetDescriptionLine(uint eventIndex,
                            byte lineIndex,
                            IntPtr line,
                            ref ushort lineBufferSize);
  }
}