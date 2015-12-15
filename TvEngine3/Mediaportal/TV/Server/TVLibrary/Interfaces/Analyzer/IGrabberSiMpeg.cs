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
  /// An MPEG 2 transport stream service information grabber interface.
  /// </summary>
  [Guid("e8f4ec8c-c4e0-44a3-a18c-00789a0e955e"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGrabberSiMpeg
  {
    /// <summary>
    /// Set the grabber's call back delegate.
    /// </summary>
    /// <param name="callBack">The delegate.</param>
    [PreserveSig]
    void SetCallBack(ICallBackGrabber callBack);

    /// <summary>
    /// Check if the grabber has received all available program association table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available PAT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadyPat();

    /// <summary>
    /// Check if the grabber has received all available conditional access table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available CAT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadyCat();

    /// <summary>
    /// Check if the grabber has received all available program map table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available PMT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadyPmt();

    /// <summary>
    /// Retrieve the MPEG 2 transport stream's details from the grabber.
    /// </summary>
    /// <param name="transportStreamId">The transport stream's identifier.</param>
    /// <param name="networkPid">The transport stream's network PID, if any.</param>
    /// <param name="programCount">The number of programs that are available from the transport stream.</param>
    [PreserveSig]
    void GetTransportStreamDetail(out ushort transportStreamId,
                                  out ushort networkPid,
                                  out ushort programCount);

    /// <summary>
    /// Retrieve an MPEG 2 transport stream program's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the program to retrieve. Should be in the range 0 to programCount - 1.</param>
    /// <param name="programNumber">The program's identifier.</param>
    /// <param name="pmtPid">The PID which delivers the program's map table sections.</param>
    /// <param name="isPmtReceived">An indication of whether the program's map table was received by the grabber.</param>
    /// <param name="streamCountVideo">The number of video streams associated with the program.</param>
    /// <param name="streamCountAudio">The number of audio streams associated with the program.</param>
    /// <param name="isEncrypted">An indication of whether any of the streams associated with the program are encrypted.</param>
    /// <param name="isEncryptionDetectionAccurate">An indication of whether the program's <paramref name="isEncrypted">encryption state</paramref> is known to be accurate (based on stream analysis).</param>
    /// <param name="isThreeDimensional">An indication of whether the program's video is three dimensional.</param>
    /// <param name="audioLanguages">The languages in which the program's audio is available. The caller must allocate this array.</param>
    /// <param name="audioLanguageCount">As an input, the size of the <paramref name="audioLanguages">audio languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="subtitlesLanguages">The languages in which the program's subtitles will be available. The caller must allocate this array.</param>
    /// <param name="subtitlesLanguageCount">As an input, the size of the <paramref name="subtitlesLanguages">subtitles languages array</paramref>; as an output, the consumed array size.</param>
    /// <returns><c>true</c> if the program's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetProgramByIndex(ushort index,
                            out ushort programNumber,
                            out ushort pmtPid,
                            [MarshalAs(UnmanagedType.I1)] out bool isPmtReceived,
                            out ushort streamCountVideo,
                            out ushort streamCountAudio,
                            [MarshalAs(UnmanagedType.I1)] out bool isEncrypted,
                            [MarshalAs(UnmanagedType.I1)] out bool isEncryptionDetectionAccurate,
                            [MarshalAs(UnmanagedType.I1)] out bool isThreeDimensional,
                            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 10)] Iso639Code[] audioLanguages,
                            ref byte audioLanguageCount,
                            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 12)] Iso639Code[] subtitlesLanguages,
                            ref byte subtitlesLanguageCount);

    /// <summary>
    /// Retrieve an MPEG 2 transport stream program's details from the grabber.
    /// </summary>
    /// <param name="programNumber">The program's identifier.</param>
    /// <param name="pmtPid">The PID which delivers the program's map table sections.</param>
    /// <param name="isPmtReceived">An indication of whether the program's map table was received by the grabber.</param>
    /// <param name="streamCountVideo">The number of video streams associated with the program.</param>
    /// <param name="streamCountAudio">The number of audio streams associated with the program.</param>
    /// <param name="isEncrypted">An indication of whether any of the streams associated with the program are encrypted.</param>
    /// <param name="isEncryptionDetectionAccurate">An indication of whether the program's <paramref name="isEncrypted">encryption state</paramref> is known to be accurate (based on stream analysis).</param>
    /// <param name="isThreeDimensional">An indication of whether the program's video is three dimensional.</param>
    /// <param name="audioLanguages">The languages in which the program's audio is available. The caller must allocate this array.</param>
    /// <param name="audioLanguageCount">As an input, the size of the <paramref name="audioLanguages">audio languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="subtitlesLanguages">The languages in which the program's subtitles will be available. The caller must allocate this array.</param>
    /// <param name="subtitlesLanguageCount">As an input, the size of the <paramref name="subtitlesLanguages">subtitles languages array</paramref>; as an output, the consumed array size.</param>
    /// <returns><c>true</c> if the program's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetProgramByNumber(ushort programNumber,
                            out ushort pmtPid,
                            [MarshalAs(UnmanagedType.I1)] out bool isPmtReceived,
                            out ushort streamCountVideo,
                            out ushort streamCountAudio,
                            [MarshalAs(UnmanagedType.I1)] out bool isEncrypted,
                            [MarshalAs(UnmanagedType.I1)] out bool isEncryptionDetectionAccurate,
                            [MarshalAs(UnmanagedType.I1)] out bool isThreeDimensional,
                            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 9)] Iso639Code[] audioLanguages,
                            ref byte audioLanguageCount,
                            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 11)] Iso639Code[] subtitlesLanguages,
                            ref byte subtitlesLanguageCount);

    /// <summary>
    /// Retrieve an MPEG 2 transport stream's conditional access table from the grabber.
    /// </summary>
    /// <param name="table">The transport stream's CAT. The caller must allocate this array.</param>
    /// <param name="tableBufferSize">As an input, the size of the <paramref name="table"/> array; as an output, the consumed array size.</param>
    /// <returns><c>true</c> if the transport stream's CAT is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetCat([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] table, ref ushort tableBufferSize);

    /// <summary>
    /// Retrieve an MPEG 2 transport stream program's program map table from the grabber.
    /// </summary>
    /// <param name="programNumber">The program's identifier.</param>
    /// <param name="table">The program's PMT. The caller must allocate this array.</param>
    /// <param name="tableBufferSize">As an input, the size of the <paramref name="table"/> array; as an output, the consumed array size.</param>
    /// <returns><c>true</c> if the program's PMT is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetPmt(ushort programNumber,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] table,
                ref ushort tableBufferSize);
  }
}