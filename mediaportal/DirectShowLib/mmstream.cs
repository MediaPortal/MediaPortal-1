#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2005
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DirectShowLib.MultimediaStreaming
{

    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    /// <summary>
    /// From COMPLETION_STATUS_FLAGS
    /// </summary>
    [Flags]
    public enum CompletionStatusFlags
    {
        None = 0x0,
        NoUpdateOk = 0x1,
        Wait = 0x2,
        Abort = 0x4
    }

    /// <summary>
    /// From unnamed enum
    /// </summary>
    [Flags]
    public enum SSUpdate
    {
        None = 0x0,
        ASync = 0x1,
        Continuous = 0x2
    }

    /// <summary>
    /// From STREAM_STATE
    /// </summary>
    public enum StreamState
    {
        // Fields
        Run = 1,
        Stop = 0
    }

    /// <summary>
    /// From STREAM_TYPE
    /// </summary>
    public enum StreamType
    {
        // Fields
        Read = 0,
        Transform = 2,
        Write = 1
    }

    /// <summary>
    /// From unnamed enum
    /// </summary>
    public enum MMSSF
    {
        HasClock = 0x1,
        SupportSeek	= 0x2,
        Asynchronous = 0x4
    }


#endif
    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
    Guid("B502D1BD-9A57-11D0-8FDE-00C04FD9189D")]
    public interface IMediaStream
    {
        [PreserveSig]
        int GetMultiMediaStream(
            [MarshalAs(UnmanagedType.Interface)] out IMultiMediaStream ppMultiMediaStream
            );

        [PreserveSig]
        int GetInformation(
            out Guid pPurposeId,
            out StreamType pType
            );

        [PreserveSig]
        int SetSameFormat(
            [In, MarshalAs(UnmanagedType.Interface)] IMediaStream pStreamThatHasDesiredFormat,
            [In] int dwFlags
            );

        [PreserveSig]
        int AllocateSample(
            [In] int dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IStreamSample ppSample
            );

        [PreserveSig]
        int CreateSharedSample(
            [In, MarshalAs(UnmanagedType.Interface)] IStreamSample pExistingSample,
            [In] int dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IStreamSample ppNewSample
            );

        [PreserveSig]
        int SendEndOfStream(
            int dwFlags
            );
    }


    [Guid("B502D1BC-9A57-11D0-8FDE-00C04FD9189D"), 
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMultiMediaStream
    {
        [PreserveSig]
        int GetInformation(
            out MMSSF pdwFlags,
            out StreamType pStreamType
            );

        [PreserveSig]
        int GetMediaStream(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid idPurpose,
            [MarshalAs(UnmanagedType.Interface)] out IMediaStream ppMediaStream
            );

        [PreserveSig]
        int EnumMediaStreams(
            [In] int Index,
            [MarshalAs(UnmanagedType.Interface)] out IMediaStream ppMediaStream
            );

        [PreserveSig]
        int GetState(
            out StreamState pCurrentState
            );

        [PreserveSig]
        int SetState(
            [In] StreamState NewState
            );

        [PreserveSig]
        int GetTime(
            out long pCurrentTime
            );

        [PreserveSig]
        int GetDuration(
            out long pDuration
            );

        [PreserveSig]
        int Seek(
            [In] long SeekTime
            );

        [PreserveSig]
        int GetEndOfStreamEventHandle(
            out IntPtr phEOS
            );
    }


    [Guid("B502D1BE-9A57-11D0-8FDE-00C04FD9189D"), 
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamSample
    {
        [PreserveSig]
        int GetMediaStream(
            [MarshalAs(UnmanagedType.Interface)] out IMediaStream ppMediaStream
            );

        [PreserveSig]
        int GetSampleTimes(
            out long pStartTime,
            out long pEndTime,
            out long pCurrentTime
            );

        [PreserveSig]
        int SetSampleTimes(
            [In] ref long pStartTime,
            [In] ref long pEndTime
            );

        [PreserveSig]
        int Update(
            [In] SSUpdate dwFlags,
            [In] IntPtr hEvent,
            [In, MarshalAs(UnmanagedType.Interface)] IStreamSample pfnAPC,
            [In] IntPtr dwAPCData
            );

        [PreserveSig]
        int CompletionStatus(
            [In] CompletionStatusFlags dwFlags,
            [In] int dwMilliseconds
            );
    }


#endif
    #endregion
}

