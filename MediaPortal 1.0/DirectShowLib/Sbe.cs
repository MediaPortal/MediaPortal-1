#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2007
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
using System.Runtime.InteropServices;
using System.Text;

namespace DirectShowLib.SBE
{
    #region Declarations

    /// <summary>
    /// From unnamed structure
    /// </summary>
    public enum RecordingType
    {
        Content = 0, //  no post-recording or overlapped
        Reference //  allows post-recording & overlapped
    }

    /// <summary>
    /// From STREAMBUFFER_ATTR_DATATYPE
    /// </summary>
    public enum StreamBufferAttrDataType
    {
        DWord = 0,
        String = 1,
        Binary = 2,
        Bool = 3,
        QWord = 4,
        Word = 5,
        Guid = 6
    }

    /// <summary>
    /// From unnamed structure
    /// </summary>
    public enum StreamBufferEventCode
    {
        TimeHole = 0x0326, // STREAMBUFFER_EC_TIMEHOLE
        StaleDataRead, // STREAMBUFFER_EC_STALE_DATA_READ
        StaleFileDeleted, // STREAMBUFFER_EC_STALE_FILE_DELETED
        ContentBecomingStale, // STREAMBUFFER_EC_CONTENT_BECOMING_STALE
        WriteFailure, // STREAMBUFFER_EC_WRITE_FAILURE
        ReadFailure, // STREAMBUFFER_EC_READ_FAILURE
        RateChanged, // STREAMBUFFER_EC_RATE_CHANGED
        PrimaryAudio // STREAMBUFFER_EC_PRIMARY_AUDIO
    }


    /// <summary>
    /// From g_wszStreamBufferRecording* static const WCHAR
    /// </summary>
    sealed public class StreamBufferRecording
    {
        private StreamBufferRecording()
        {
        }

        ////////////////////////////////////////////////////////////////
        //
        // List of pre-defined attributes
        //
        public readonly string Duration = "Duration";

        public readonly string Bitrate = "Bitrate";
        public readonly string Seekable = "Seekable";
        public readonly string Stridable = "Stridable";
        public readonly string Broadcast = "Broadcast";
        public readonly string Protected = "Is_Protected";
        public readonly string Trusted = "Is_Trusted";
        public readonly string Signature_Name = "Signature_Name";
        public readonly string HasAudio = "HasAudio";
        public readonly string HasImage = "HasImage";
        public readonly string HasScript = "HasScript";
        public readonly string HasVideo = "HasVideo";
        public readonly string CurrentBitrate = "CurrentBitrate";
        public readonly string OptimalBitrate = "OptimalBitrate";
        public readonly string HasAttachedImages = "HasAttachedImages";
        public readonly string SkipBackward = "Can_Skip_Backward";
        public readonly string SkipForward = "Can_Skip_Forward";
        public readonly string NumberOfFrames = "NumberOfFrames";
        public readonly string FileSize = "FileSize";
        public readonly string HasArbitraryDataStream = "HasArbitraryDataStream";
        public readonly string HasFileTransferStream = "HasFileTransferStream";

        ////////////////////////////////////////////////////////////////
        //
        // The content description object supports 5 basic attributes.
        //
        public readonly string Title = "Title";

        public readonly string Author = "Author";
        public readonly string Description = "Description";
        public readonly string Rating = "Rating";
        public readonly string Copyright = "Copyright";

        ////////////////////////////////////////////////////////////////
        //
        // These attributes are used to configure DRM using IWMDRMWriter::SetDRMAttribute.
        //
        public readonly string Use_DRM = "Use_DRM";

        public readonly string DRM_Flags = "DRM_Flags";
        public readonly string DRM_Level = "DRM_Level";

        ////////////////////////////////////////////////////////////////
        //
        // These are the additional attributes defined in the WM attribute
        // namespace that give information about the content.
        //
        public readonly string AlbumTitle = "WM/AlbumTitle";

        public readonly string Track = "WM/Track";
        public readonly string PromotionURL = "WM/PromotionURL";
        public readonly string AlbumCoverURL = "WM/AlbumCoverURL";
        public readonly string Genre = "WM/Genre";
        public readonly string Year = "WM/Year";
        public readonly string GenreID = "WM/GenreID";
        public readonly string MCDI = "WM/MCDI";
        public readonly string Composer = "WM/Composer";
        public readonly string Lyrics = "WM/Lyrics";
        public readonly string TrackNumber = "WM/TrackNumber";
        public readonly string ToolName = "WM/ToolName";
        public readonly string ToolVersion = "WM/ToolVersion";
        public readonly string IsVBR = "IsVBR";
        public readonly string AlbumArtist = "WM/AlbumArtist";

        ////////////////////////////////////////////////////////////////
        //
        // These optional attributes may be used to give information
        // about the branding of the content.
        //
        public readonly string BannerImageType = "BannerImageType";

        public readonly string BannerImageData = "BannerImageData";
        public readonly string BannerImageURL = "BannerImageURL";
        public readonly string CopyrightURL = "CopyrightURL";

        ////////////////////////////////////////////////////////////////
        //
        // Optional attributes, used to give information
        // about video stream properties.
        //
        public readonly string AspectRatioX = "AspectRatioX";

        public readonly string AspectRatioY = "AspectRatioY";

        ////////////////////////////////////////////////////////////////
        //
        // The NSC file supports the following attributes.
        //
        public readonly string NSCName = "NSC_Name";

        public readonly string NSCAddress = "NSC_Address";
        public readonly string NSCPhone = "NSC_Phone";
        public readonly string NSCEmail = "NSC_Email";
        public readonly string NSCDescription = "NSC_Description";
    }


    /// <summary>
    /// From STREAMBUFFER_ATTRIBUTE
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StreamBufferAttribute
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pszName;
        public StreamBufferAttrDataType StreamBufferAttributeType;
        public IntPtr pbAttribute; // BYTE *
        public short cbLength;
    }

    /// <summary>
    /// From SBE_PIN_DATA
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SBEPinData
    {
        public long cDataBytes; //  total sample payload bytes
        public long cSamplesProcessed; //  samples processed
        public long cDiscontinuities; //  number of discontinuities
        public long cSyncPoints; //  number of syncpoints
        public long cTimestamps; //  number of timestamps
    }

    #endregion

    #region Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7E2D2A1E-7192-4bd7-80C1-061FD1D10402"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferConfigure3 : IStreamBufferConfigure2
    {
        #region IStreamBufferConfigure

        [PreserveSig]
        new int SetDirectory([In, MarshalAs(UnmanagedType.LPWStr)] string pszDirectoryName);

        [PreserveSig]
        new int GetDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] out string pszDirectoryName);

        [PreserveSig]
        new int SetBackingFileCount(
            [In] int dwMin,
            [In] int dwMax
            );

        [PreserveSig]
        new int GetBackingFileCount(
            [Out] out int dwMin,
            [Out] out int dwMax
            );

        [PreserveSig]
        new int SetBackingFileDuration([In] int dwSeconds);

        [PreserveSig]
        new int GetBackingFileDuration([Out] out int pdwSeconds);

        #endregion

        #region IStreamBufferConfigure2

        [PreserveSig]
        new int SetMultiplexedPacketSize([In] int cbBytesPerPacket);

        [PreserveSig]
        new int GetMultiplexedPacketSize([Out] out int pcbBytesPerPacket);

        [PreserveSig]
        new int SetFFTransitionRates(
            [In] int dwMaxFullFrameRate,
            [In] int dwMaxNonSkippingRate
            );

        [PreserveSig]
        new int GetFFTransitionRates(
            [Out] out int pdwMaxFullFrameRate,
            [Out] out int pdwMaxNonSkippingRate
            );

        #endregion

        [PreserveSig]
        int SetStartRecConfig([In, MarshalAs(UnmanagedType.Bool)] bool fStartStopsCur);

        [PreserveSig]
        int GetStartRecConfig([Out, MarshalAs(UnmanagedType.Bool)] out bool pfStartStopsCur);

        [PreserveSig]
        int SetNamespace([In, MarshalAs(UnmanagedType.LPWStr)] string pszNamespace);

        [PreserveSig]
        int GetNamespace([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszNamespace);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("9ce50f2d-6ba7-40fb-a034-50b1a674ec78"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferInitialize
    {
        [PreserveSig]
        int SetHKEY([In] IntPtr hkeyRoot); // HKEY

        [PreserveSig]
        int SetSIDs(
            [In] int cSIDs,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr [] ppSID // PSID *
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("afd1f242-7efd-45ee-ba4e-407a25c9a77a"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferSink
    {
        [PreserveSig]
        int LockProfile([In, MarshalAs(UnmanagedType.LPWStr)] string pszStreamBufferFilename);

        [PreserveSig]
        int CreateRecorder(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFilename,
            [In] RecordingType dwRecordType,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object pRecordingIUnknown
            );

        [PreserveSig]
        int IsProfileLocked();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("DB94A660-F4FB-4bfa-BCC6-FE159A4EEA93"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferSink2 : IStreamBufferSink
    {
        #region IStreamBufferSink Methods

        [PreserveSig]
        new int LockProfile([In, MarshalAs(UnmanagedType.LPWStr)] string pszStreamBufferFilename);

        [PreserveSig]
        new int CreateRecorder(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFilename,
            [In] RecordingType dwRecordType,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object pRecordingIUnknown
            );

        [PreserveSig]
        new int IsProfileLocked();

        #endregion

        [PreserveSig]
        int UnlockProfile();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("974723f2-887a-4452-9366-2cff3057bc8f"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferSink3 : IStreamBufferSink2
    {
        #region IStreamBufferSink Methods

        [PreserveSig]
        new int LockProfile([In, MarshalAs(UnmanagedType.LPWStr)] string pszStreamBufferFilename);

        [PreserveSig]
        new int CreateRecorder(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFilename,
            [In] RecordingType dwRecordType,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object pRecordingIUnknown
            );

        [PreserveSig]
        new int IsProfileLocked();

        #endregion

        #region IStreamBufferSink2

        [PreserveSig]
        new int UnlockProfile();

        #endregion

        [PreserveSig]
        int SetAvailableFilter([In, Out] ref long prtMin);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1c5bd776-6ced-4f44-8164-5eab0e98db12"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferSource
    {
        [PreserveSig]
        int SetStreamSink([In] IStreamBufferSink pIStreamBufferSink);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("ba9b6c99-f3c7-4ff2-92db-cfdd4851bf31"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferRecordControl
    {
        [PreserveSig]
        int Start([In, Out] ref long prtStart);

        [PreserveSig]
        int Stop([In] long rtStop);

        [PreserveSig]
        int GetRecordingStatus(
            [Out] out int phResult,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pbStarted,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pbStopped
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("9E259A9B-8815-42ae-B09F-221970B154FD"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferRecComp
    {
        [PreserveSig]
        int Initialize(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszTargetFilename,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszSBRecProfileRef
            );

        [PreserveSig]
        int Append([In, MarshalAs(UnmanagedType.LPWStr)] string pszSBRecording);

        [PreserveSig]
        int AppendEx(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszSBRecording,
            [In] long rtStart,
            [In] long rtStop
            );

        [PreserveSig]
        int GetCurrentLength([Out] out int pcSeconds);

        [PreserveSig]
        int Close();

        [PreserveSig]
        int Cancel();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("16CA4E03-FE69-4705-BD41-5B7DFC0C95F3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferRecordingAttribute
    {
        [PreserveSig]
        int SetAttribute(
            [In] int ulReserved,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
            [In] StreamBufferAttrDataType StreamBufferAttributeType,
            [In] IntPtr pbAttribute, // BYTE *
            [In] short cbAttributeLength
            );

        [PreserveSig]
        int GetAttributeCount(
            [In] int ulReserved,
            [Out] out short pcAttributes
            );

        [PreserveSig]
        int GetAttributeByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
            [In] int pulReserved,
            [Out] out StreamBufferAttrDataType pStreamBufferAttributeType,
            [In, Out] IntPtr pbAttribute, // BYTE *
            [In, Out] ref short pcbLength
            );

        [PreserveSig]
        int GetAttributeByIndex(
            [In] short wIndex,
            [In] int pulReserved,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszAttributeName,
            [In, Out] ref short pcchNameLength,
            [Out] out StreamBufferAttrDataType pStreamBufferAttributeType,
            IntPtr pbAttribute, // BYTE *
            [In, Out] ref short pcbLength
            );

        int EnumAttributes([Out] out IEnumStreamBufferRecordingAttrib ppIEnumStreamBufferAttrib);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("C18A9162-1E82-4142-8C73-5690FA62FE33"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumStreamBufferRecordingAttrib
    {
        [PreserveSig]
        int Next(
            [In] int cRequest,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] StreamBufferAttribute[] pStreamBufferAttribute,
            [In] IntPtr pcReceived
            );

        [PreserveSig]
        int Skip([In] int cRecords);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumStreamBufferRecordingAttrib ppIEnumStreamBufferAttrib);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("ce14dfae-4098-4af7-bbf7-d6511f835414"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferConfigure
    {
        [PreserveSig]
        int SetDirectory([In, MarshalAs(UnmanagedType.LPWStr)] string pszDirectoryName);

        [PreserveSig]
        int GetDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] out string pszDirectoryName);

        [PreserveSig]
        int SetBackingFileCount(
            [In] int dwMin,
            [In] int dwMax
            );

        [PreserveSig]
        int GetBackingFileCount(
            [Out] out int dwMin,
            [Out] out int dwMax
            );

        [PreserveSig]
        int SetBackingFileDuration([In] int dwSeconds);

        [PreserveSig]
        int GetBackingFileDuration([Out] out int pdwSeconds);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("53E037BF-3992-4282-AE34-2487B4DAE06B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferConfigure2 : IStreamBufferConfigure
    {
        #region IStreamBufferConfigure

        [PreserveSig]
        new int SetDirectory([In, MarshalAs(UnmanagedType.LPWStr)] string pszDirectoryName);

        [PreserveSig]
        new int GetDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] out string pszDirectoryName);

        [PreserveSig]
        new int SetBackingFileCount(
            [In] int dwMin,
            [In] int dwMax
            );

        [PreserveSig]
        new int GetBackingFileCount(
            [Out] out int dwMin,
            [Out] out int dwMax
            );

        [PreserveSig]
        new int SetBackingFileDuration([In] int dwSeconds);

        [PreserveSig]
        new int GetBackingFileDuration([Out] out int pdwSeconds);

        #endregion

        [PreserveSig]
        int SetMultiplexedPacketSize([In] int cbBytesPerPacket);

        [PreserveSig]
        int GetMultiplexedPacketSize([Out] out int pcbBytesPerPacket);

        [PreserveSig]
        int SetFFTransitionRates(
            [In] int dwMaxFullFrameRate,
            [In] int dwMaxNonSkippingRate
            );

        [PreserveSig]
        int GetFFTransitionRates(
            [Out] out int pdwMaxFullFrameRate,
            [Out] out int pdwMaxNonSkippingRate
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("f61f5c26-863d-4afa-b0ba-2f81dc978596"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferMediaSeeking : IMediaSeeking
    {
        #region IMediaSeeking Methods

        [PreserveSig]
        new int GetCapabilities([Out] out AMSeekingSeekingCapabilities pCapabilities);

        [PreserveSig]
        new int CheckCapabilities([In, Out] ref AMSeekingSeekingCapabilities pCapabilities);

        [PreserveSig]
        new int IsFormatSupported([In, MarshalAs(UnmanagedType.LPStruct)] Guid pFormat);

        [PreserveSig]
        new int QueryPreferredFormat([Out] out Guid pFormat);

        [PreserveSig]
        new int GetTimeFormat([Out] out Guid pFormat);

        [PreserveSig]
        new int IsUsingTimeFormat([In, MarshalAs(UnmanagedType.LPStruct)] Guid pFormat);

        [PreserveSig]
        new int SetTimeFormat([In, MarshalAs(UnmanagedType.LPStruct)] Guid pFormat);

        [PreserveSig]
        new int GetDuration([Out] out long pDuration);

        [PreserveSig]
        new int GetStopPosition([Out] out long pStop);

        [PreserveSig]
        new int GetCurrentPosition([Out] out long pCurrent);

        [PreserveSig]
        new int ConvertTimeFormat(
            [Out] out long pTarget,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid pTargetFormat,
            [In] long Source,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid pSourceFormat
            );

        [PreserveSig]
        new int SetPositions(
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] DsLong pCurrent,
            [In] AMSeekingSeekingFlags dwCurrentFlags,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] DsLong pStop,
            [In] AMSeekingSeekingFlags dwStopFlags
            );

        [PreserveSig]
        new int GetPositions(
            [Out] out long pCurrent,
            [Out] out long pStop
            );

        [PreserveSig]
        new int GetAvailable(
            [Out] out long pEarliest,
            [Out] out long pLatest
            );

        [PreserveSig]
        new int SetRate([In] double dRate);

        [PreserveSig]
        new int GetRate([Out] out double pdRate);

        [PreserveSig]
        new int GetPreroll([Out] out long pllPreroll);

        #endregion
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("3a439ab0-155f-470a-86a6-9ea54afd6eaf"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferMediaSeeking2 : IStreamBufferMediaSeeking
    {
        #region IMediaSeeking

        [PreserveSig]
        new int GetCapabilities([Out] out AMSeekingSeekingCapabilities pCapabilities);

        [PreserveSig]
        new int CheckCapabilities([In, Out] ref AMSeekingSeekingCapabilities pCapabilities);

        [PreserveSig]
        new int IsFormatSupported([In, MarshalAs(UnmanagedType.LPStruct)] Guid pFormat);

        [PreserveSig]
        new int QueryPreferredFormat([Out] out Guid pFormat);

        [PreserveSig]
        new int GetTimeFormat([Out] out Guid pFormat);

        [PreserveSig]
        new int IsUsingTimeFormat([In, MarshalAs(UnmanagedType.LPStruct)] Guid pFormat);

        [PreserveSig]
        new int SetTimeFormat([In, MarshalAs(UnmanagedType.LPStruct)] Guid pFormat);

        [PreserveSig]
        new int GetDuration([Out] out long pDuration);

        [PreserveSig]
        new int GetStopPosition([Out] out long pStop);

        [PreserveSig]
        new int GetCurrentPosition([Out] out long pCurrent);

        [PreserveSig]
        new int ConvertTimeFormat(
            [Out] out long pTarget,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid pTargetFormat,
            [In] long Source,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid pSourceFormat
            );

        [PreserveSig]
        new int SetPositions(
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] DsLong pCurrent,
            [In] AMSeekingSeekingFlags dwCurrentFlags,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] DsLong pStop,
            [In] AMSeekingSeekingFlags dwStopFlags
            );

        [PreserveSig]
        new int GetPositions(
            [Out] out long pCurrent,
            [Out] out long pStop
            );

        [PreserveSig]
        new int GetAvailable(
            [Out] out long pEarliest,
            [Out] out long pLatest
            );

        [PreserveSig]
        new int SetRate([In] double dRate);

        [PreserveSig]
        new int GetRate([Out] out double pdRate);

        [PreserveSig]
        new int GetPreroll([Out] out long pllPreroll);

        #endregion

        [PreserveSig]
        int SetRateEx(
            [In] double dRate,
            [In] int dwFramesPerSec
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("9D2A2563-31AB-402e-9A6B-ADB903489440"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferDataCounters
    {
        [PreserveSig]
        int GetData([Out] out SBEPinData pPinData);

        [PreserveSig]
        int ResetData();
    }

    #endregion
}
