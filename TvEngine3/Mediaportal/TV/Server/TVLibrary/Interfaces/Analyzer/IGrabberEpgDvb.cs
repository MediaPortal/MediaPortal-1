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
  /// A DVB electronic programme guide grabber interface.
  /// </summary>
  [Guid("0fa842df-9c1b-4164-96ac-47c0a44241de"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGrabberEpgDvb
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
    /// <param name="grabDvbEit"><c>True</c> to enable DVB EIT grabbing.</param>
    /// <param name="grabBellExpressVu"><c>True</c> to enable Bell ExpressVu (satellite, Canada) grabbing.</param>
    /// <param name="grabDish"><c>True</c> to enable Dish (satellite, USA) grabbing.</param>
    /// <param name="grabFreesat"><c>True</c> to enable Freesat (satellite, UK) grabbing.</param>
    /// <param name="grabMultiChoice"><c>True</c> to enable MultiChoice (satellite, South Africa) grabbing.</param>
    /// <param name="grabPremiere"><c>True</c> to enable Premiere (satellite/cable, Sky Germany) grabbing.</param>
    /// <param name="grabViasatSweden"><c>True</c> to enable Viasat Sweden (satellite, Sweden) grabbing.</param>
    [PreserveSig]
    void SetProtocols([MarshalAs(UnmanagedType.I1)] bool grabDvbEit,
                      [MarshalAs(UnmanagedType.I1)] bool grabBellExpressVu,
                      [MarshalAs(UnmanagedType.I1)] bool grabDish,
                      [MarshalAs(UnmanagedType.I1)] bool grabFreesat,
                      [MarshalAs(UnmanagedType.I1)] bool grabMultiChoice,
                      [MarshalAs(UnmanagedType.I1)] bool grabPremiere,
                      [MarshalAs(UnmanagedType.I1)] bool grabViasatSweden);

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
    /// Get the number of services received by the grabber.
    /// </summary>
    /// <returns>the number of services received by the grabber</returns>
    [PreserveSig]
    ushort GetServiceCount();

    /// <summary>
    /// Retrieve a service's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the service to retrieve. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="originalNetworkId">The identifier of the original network that the service is associated with.</param>
    /// <param name="transportStreamId">The identifier of the transport stream that the service is associated with.</param>
    /// <param name="serviceId">The service's identifier. Only unique when combined with the <paramref name="originalNetworkId">original network ID</paramref> and <paramref name="transportStreamId">transport stream ID</paramref>.</param>
    /// <param name="eventCount">The number of events associated with the service.</param>
    /// <returns><c>true</c> if the service's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetService(ushort index,
                    out ushort originalNetworkId,
                    out ushort transportStreamId,
                    out ushort serviceId,
                    out ushort eventCount);

    /// <summary>
    /// Retrieve an event's details from the grabber.
    /// </summary>
    /// <param name="serviceIndex">The event's service's index. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="eventIndex">The index of the event to retrieve. Should be in the range 0 to eventCount - 1 for the service.</param>
    /// <param name="eventId">The event's identifier. Only unique when combined with the service's original network ID, transport stream ID and service ID.</param>
    /// <param name="startDateTime">The event's start date/time, encoded as a UTC Unix epoch reference.</param>
    /// <param name="duration">The event's duration in minutes.</param>
    /// <param name="runningStatus">The event's running status, encoded according to DVB specifications.</param>
    /// <param name="freeCaMode">An indication of whether the event's streams will be encrypted or not.</param>
    /// <param name="referenceServiceId">The service identifier associated with the service's NVOD reference service, if any.</param>
    /// <param name="referenceEventId">The event identifier associated with the event's NVOD reference event, if any.</param>
    /// <param name="seriesId">A buffer containing the identifier that links this event to other events from the same series, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="seriesIdBufferSize">As an input, the size of the <paramref name="seriesId">series identifier buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="episodeId">A buffer containing the event's episode identifier, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="episodeIdBufferSize">As an input, the size of the <paramref name="episodeId">episode identifier buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="isHighDefinition">An indication of whether the event's video will by high definition.</param>
    /// <param name="isThreeDimensional">An indication of whether the event's video will by three dimensional.</param>
    /// <param name="isPreviouslyShown">An indication of whether the event has previously been shown.</param>
    /// <param name="audioLanguages">The languages in which the event's audio will be available. The caller must allocate this array.</param>
    /// <param name="audioLanguageCount">As an input, the size of the <paramref name="audioLanguages">audio languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="subtitlesLanguages">The languages in which the event's subtitles will be available. The caller must allocate this array.</param>
    /// <param name="subtitlesLanguageCount">As an input, the size of the <paramref name="subtitlesLanguages">subtitles languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="dvbContentTypeIds">The content type identifiers that the event is associated with. The caller must allocate this array.</param>
    /// <param name="dvbContentTypeIdCount">As an input, the size of the <paramref name="dvbContentTypeIds">content type identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="dvbParentalRatingCountryCodes">The country codes for the parental ratings that the event is associated with. The caller must allocate this array.</param>
    /// <param name="dvbParentalRatings">The parental ratings that the event is associated with. The caller must allocate this array.</param>
    /// <param name="dvbParentalRatingCount">As an input, the size of the parental ratings <paramref name="dvbParentalRatingCountryCodes">country codes</paramref> and <paramref name="dvbParentalRatings">ratings</paramref> arrays; as an output, the consumed array size (same for both arrays).</param>
    /// <param name="starRating">The event's star rating, if any.</param>
    /// <param name="mpaaClassification">The event's MPAA classification. Value is <c>0xff</c> if not available.</param>
    /// <param name="dishBevAdvisories">The event's advisories, encoded as flags.</param>
    /// <param name="vchipRating">The event's V-CHIP rating. Value is <c>0xff</c> if not available.</param>
    /// <param name="textCount">The number of languages in which the event's text is available.</param>
    /// <returns><c>true</c> if the event's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetEvent(ushort serviceIndex,
                  ushort eventIndex,
                  out ulong eventId,
                  out ulong startDateTime,
                  out ushort duration,
                  out byte runningStatus,
                  [MarshalAs(UnmanagedType.I1)] out bool freeCaMode,
                  out ushort referenceServiceId,
                  out ulong referenceEventId,
                  IntPtr seriesId,
                  ref ushort seriesIdBufferSize,
                  IntPtr episodeId,
                  ref ushort episodeIdBufferSize,
                  [MarshalAs(UnmanagedType.I1)] out bool isHighDefinition,
                  [MarshalAs(UnmanagedType.I1)] out bool isThreeDimensional,
                  [MarshalAs(UnmanagedType.I1)] out bool isPreviouslyShown,
                  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)] Iso639Code[] audioLanguages,
                  ref byte audioLanguageCount,
                  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 19)] Iso639Code[] subtitlesLanguages,
                  ref byte subtitlesLanguageCount,
                  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 21)] ushort[] dvbContentTypeIds,
                  ref byte dvbContentTypeIdCount,
                  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)] Iso639Code[] dvbParentalRatingCountryCodes,
                  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)] byte[] dvbParentalRatings,
                  ref byte dvbParentalRatingCount,
                  out byte starRating,
                  out byte mpaaClassification,
                  out ushort dishBevAdvisories,
                  out byte vchipRating,
                  out byte textCount);

    /// <summary>
    /// Retrieve an event's text from the grabber.
    /// </summary>
    /// <param name="serviceIndex">The event's service's index. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="eventIndex">The event's index. Should be in the range 0 to GetEventCount() - 1 for the service.</param>
    /// <param name="textIndex">The index of the text to retrieve. Should be in the range 0 to textCount - 1 for the event.</param>
    /// <param name="language">The text's language.</param>
    /// <param name="title">A buffer containing the event's title, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="titleBufferSize">As an input, the size of the <paramref name="title">title buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="shortDescription">A buffer containing the event's short description, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="shortDescriptionBufferSize">As an input, the size of the <paramref name="shortDescription">short description buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="extendedDescription">A buffer containing the event's extended description, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="extendedDescriptionBufferSize">As an input, the size of the <paramref name="extendedDescription">extended description buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="descriptionItemCount">The number of description items associated with the event.</param>
    /// <returns><c>true</c> if the event's text is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetEventText(ushort serviceIndex,
                      ushort eventIndex,
                      byte textIndex,
                      out Iso639Code language,
                      IntPtr title,
                      ref ushort titleBufferSize,
                      IntPtr shortDescription,
                      ref ushort shortDescriptionBufferSize,
                      IntPtr extendedDescription,
                      ref ushort extendedDescriptionBufferSize,
                      out byte descriptionItemCount);

    /// <summary>
    /// Retrieve an event text's description item from the grabber.
    /// </summary>
    /// <param name="serviceIndex">The event's service's index. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="eventIndex">The event's index. Should be in the range 0 to GetEventCount() - 1 for the service.</param>
    /// <param name="textIndex">The text's index. Should be in the range 0 to textCount - 1 for the event.</param>
    /// <param name="itemIndex">The index of the description item to retrieve. Should be in the range 0 to descriptionItemCount for the event text.</param>
    /// <param name="description">A buffer containing the item's description, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="descriptionBufferSize">As an input, the size of the <paramref name="description">description buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="text">A buffer containing the item's text, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="textBufferSize">As an input, the size of the <paramref name="text">text buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the description item is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetEventDescriptionItem(ushort serviceIndex,
                                  ushort eventIndex,
                                  byte textIndex,
                                  byte itemIndex,
                                  IntPtr description,
                                  ref ushort descriptionBufferSize,
                                  IntPtr text,
                                  ref ushort textBufferSize);
  }
}