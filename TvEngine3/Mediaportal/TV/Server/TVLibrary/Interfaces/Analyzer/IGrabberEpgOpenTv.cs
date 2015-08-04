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
  /// An OpenTV electronic programme guide grabber interface.
  /// </summary>
  [Guid("65e51de0-4a4e-43e0-8b13-c809e5be8a93"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGrabberEpgOpenTv
  {
    /// <summary>
    /// Set the grabber's call back delegate.
    /// </summary>
    /// <param name="callBack">The delegate.</param>
    [PreserveSig]
    void SetCallBack(ICallBackGrabber callBack);

    /// <summary>
    /// Start the grabber.
    /// </summary>
    [PreserveSig]
    void Start();

    /// <summary>
    /// Stop the grabber.
    /// </summary>
    [PreserveSig]
    void Stop();

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
    /// Get the number of OpenTV events received by the grabber.
    /// </summary>
    /// <returns>the number of OpenTV events received by the grabber</returns>
    [PreserveSig]
    uint GetEventCount();

    /// <summary>
    /// Retrieve an OpenTV event's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the event to retrieve. Should be in the range 0 to GetEventCount() - 1.</param>
    /// <param name="channelId">The identifier of the channel that the event is associated with.</param>
    /// <param name="eventId">The event's identifier. Only unique when combined with a <paramref name="channelId">channel identifier</paramref>.</param>
    /// <param name="startDateTime">The event's start date/time, encoded as a UTC Unix epoch reference.</param>
    /// <param name="duration">The event's duration in minutes.</param>
    /// <param name="title">A buffer containing the event's title, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="titleBufferSize">As an input, the size of the <paramref name="title">title buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="shortDescription">A buffer containing the event's short description, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="shortDescriptionBufferSize">As an input, the size of the <paramref name="shortDescription">short description buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="extendedDescription">A buffer containing the event's extended description, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="extendedDescriptionBufferSize">As an input, the size of the <paramref name="extendedDescription">extended description buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="categoryId">The identifier of the programme category that the event is associated with, if any.</param>
    /// <param name="subCategoryId">The identifier of the programme category sub-category that the event is associated with, if any.</param>
    /// <param name="isHighDefinition">An indication of whether the event's video will by high definition.</param>
    /// <param name="hasSubtitles">An indication of whether the event will have subtitles available.</param>
    /// <param name="parentalRating">The event's parental rating (classification), if any.</param>
    /// <param name="seriesLinkId">The identifier that links this event to other events from the same series. Value is <c>0xffff</c> if not available.</param>
    /// <returns><c>true</c> if the event's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetEvent(uint index,
                  out ushort channelId,
                  out ushort eventId,
                  out ulong startDateTime,
                  out ushort duration,
                  IntPtr title,
                  ref ushort titleBufferSize,
                  IntPtr shortDescription,
                  ref ushort shortDescriptionBufferSize,
                  IntPtr extendedDescription,
                  ref ushort extendedDescriptionBufferSize,
                  out byte categoryId,
                  out byte subCategoryId,
                  [MarshalAs(UnmanagedType.I1)] out bool isHighDefinition,
                  [MarshalAs(UnmanagedType.I1)] out bool hasSubtitles,
                  out byte parentalRating,
                  out ushort seriesLinkId);
  }
}