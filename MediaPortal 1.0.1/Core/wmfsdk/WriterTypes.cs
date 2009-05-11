#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;

using System.Text;

using System.Runtime.InteropServices;





namespace Yeti.WMFSdk

{

  [StructLayout(LayoutKind.Sequential)]

  public struct WM_WRITER_STATISTICS

  {

    public ulong qwSampleCount;

    public ulong qwByteCount;

    public ulong qwDroppedSampleCount;

    public ulong qwDroppedByteCount;

    public uint dwCurrentBitrate;

    public uint dwAverageBitrate;

    public uint dwExpectedBitrate;

    public uint dwCurrentSampleRate;

    public uint dwAverageSampleRate;

    public uint dwExpectedSampleRate;

  };



  [StructLayout(LayoutKind.Sequential)]

  public struct WM_WRITER_STATISTICS_EX

  {

    public uint dwBitratePlusOverhead;

    public uint dwCurrentSampleDropRateInQueue;

    public uint dwCurrentSampleDropRateInCodec;

    public uint dwCurrentSampleDropRateInMultiplexer;

    public uint dwTotalSampleDropsInQueue;

    public uint dwTotalSampleDropsInCodec;

    public uint dwTotalSampleDropsInMultiplexer;

  };



  [StructLayout(LayoutKind.Sequential)]

  public struct WMT_BUFFER_SEGMENT

  {

    public INSSBuffer pBuffer;

    public uint cbOffset;

    public uint cbLength;

  };



  [StructLayout(LayoutKind.Sequential)]

  public struct WMT_PAYLOAD_FRAGMENT

  {

    public uint dwPayloadIndex;

    public WMT_BUFFER_SEGMENT  segmentData;

  };



  [StructLayout(LayoutKind.Sequential)]

  public struct WMT_FILESINK_DATA_UNIT

  {

    public WMT_BUFFER_SEGMENT packetHeaderBuffer;

    public uint cPayloads;

    /*WMT_BUFFER_SEGMENT* */ public IntPtr pPayloadHeaderBuffers;

    public uint cPayloadDataFragments;

    /*WMT_PAYLOAD_FRAGMENT* */ public IntPtr pPayloadDataFragments;

  };

  

  [ComImport]

  [Guid("96406BE4-2B2B-11d3-B36B-00C04F6108FF")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterSink

  {

    void OnHeader( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pHeader );

    void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    void AllocateDataUnit( [In] uint cbDataUnit,

                           [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppDataUnit );

    void OnDataUnit( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pDataUnit );

    void OnEndWriting();

  }

  

  [ComImport]

  [Guid("96406BE5-2B2B-11d3-B36B-00C04F6108FF")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterFileSink : IWMWriterSink

  {

    //IWMWriterSink

    new void OnHeader( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pHeader );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void AllocateDataUnit( [In] uint cbDataUnit,

      [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppDataUnit );

    new void OnDataUnit( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pDataUnit );

    new void OnEndWriting();

    //IWMWriterFileSink

    void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );

  }



  [ComImport]

  [Guid("14282BA7-4AEF-4205-8CE5-C229035A05BC")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterFileSink2 : IWMWriterFileSink

  {

    //IWMWriterSink

    new void OnHeader( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pHeader );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void AllocateDataUnit( [In] uint cbDataUnit,

      [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppDataUnit );

    new void OnDataUnit( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pDataUnit );

    new void OnEndWriting();

    //IWMWriterFileSink

    new void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );

    //IWMWriterFileSink2

    void Start( [In] ulong cnsStartTime );

    void Stop( [In] ulong cnsStopTime );

    void IsStopped( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfStopped );

    void GetFileDuration( [Out] out ulong pcnsDuration );

    void GetFileSize( [Out] out ulong pcbFile );

    void Close();

    void IsClosed( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfClosed );

  }



  [ComImport]

  [Guid("3FEA4FEB-2945-47A7-A1DD-C53A8FC4C45C")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterFileSink3 : IWMWriterFileSink2

  {

    //IWMWriterSink

    new void OnHeader( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pHeader );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void AllocateDataUnit( [In] uint cbDataUnit,

      [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppDataUnit );

    new void OnDataUnit( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pDataUnit );

    new void OnEndWriting();

    //IWMWriterFileSink

    new void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );

    //IWMWriterFileSink2

    new void Start( [In] ulong cnsStartTime );

    new void Stop( [In] ulong cnsStopTime );

    new void IsStopped( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfStopped );

    new void GetFileDuration( [Out] out ulong pcnsDuration );

    new void GetFileSize( [Out] out ulong pcbFile );

    new void Close();

    new void IsClosed( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfClosed );

    //IWMWriterFileSink3

    void SetAutoIndexing( [In, MarshalAs(UnmanagedType.Bool)] bool fDoAutoIndexing );

    void GetAutoIndexing( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAutoIndexing );

    void SetControlStream( [In] ushort wStreamNumber,

                           [In, MarshalAs(UnmanagedType.Bool)] bool fShouldControlStartAndStop );

    void GetMode( [Out] out uint pdwFileSinkMode );

    void OnDataUnitEx( [In] ref WMT_FILESINK_DATA_UNIT pFileSinkDataUnit );

    void SetUnbufferedIO( [In, MarshalAs(UnmanagedType.Bool)] bool fUnbufferedIO,

                          [In, MarshalAs(UnmanagedType.Bool)] bool fRestrictMemUsage );

    void GetUnbufferedIO( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfUnbufferedIO );

    void CompleteOperations( );

  }



  [ComImport]

  [Guid("96406BE7-2B2B-11d3-B36B-00C04F6108FF")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterNetworkSink : IWMWriterSink

  {

    //IWMWriterSink

    new void OnHeader( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pHeader );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void AllocateDataUnit( [In] uint cbDataUnit,

      [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppDataUnit );

    new void OnDataUnit( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pDataUnit );

    new void OnEndWriting();

    //IWMWriterNetworkSink

    void SetMaximumClients( [In] uint dwMaxClients );

    void GetMaximumClients( [Out] out uint pdwMaxClients );

    void SetNetworkProtocol( [In] WMT_NET_PROTOCOL protocol );

    void GetNetworkProtocol( [Out] out WMT_NET_PROTOCOL pProtocol );

    void GetHostURL( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszURL,

                     [In, Out] ref uint pcchURL );

    void Open( [In, Out] ref uint pdwPortNum );

    void Disconnect();

    void Close();

  }



  [ComImport]

  [Guid("dc10e6a5-072c-467d-bf57-6330a9dde12a")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterPushSink : IWMWriterSink

  {

    //IWMWriterSink

    new void OnHeader( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pHeader );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void AllocateDataUnit( [In] uint cbDataUnit,

      [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppDataUnit );

    new void OnDataUnit( [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pDataUnit );

    new void OnEndWriting();

    //IWMWriterPushSink

    void Connect( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,

                  [In, MarshalAs(UnmanagedType.LPWStr)] string pwszTemplateURL,              

                  [In, MarshalAs(UnmanagedType.Bool)] bool fAutoDestroy);

    void Disconnect();

    void EndSession();

  }



  [ComImport]

  [Guid("96406BD4-2B2B-11d3-B36B-00C04F6108FF")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriter

  {

    void SetProfileByID( [In] ref Guid guidProfile );

    void SetProfile( [In, MarshalAs(UnmanagedType.Interface)] IWMProfile pProfile );

    void SetOutputFilename( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );

    void GetInputCount( [Out] out uint pcInputs );

    void GetInputProps( [In] uint dwInputNum,

                        [Out, MarshalAs(UnmanagedType.Interface)] out IWMInputMediaProps ppInput );

    void SetInputProps( [In] uint dwInputNum,

                        [In, MarshalAs(UnmanagedType.Interface)] IWMInputMediaProps pInput );

    void GetInputFormatCount( [In] uint dwInputNumber, [Out] out uint pcFormats );

    void GetInputFormat( [In] uint dwInputNumber,

                         [In] uint dwFormatNumber,

                         [Out, MarshalAs(UnmanagedType.Interface)] out IWMInputMediaProps pProps );

    void BeginWriting();

    void EndWriting();

    void AllocateSample( [In] uint dwSampleSize,

                         [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppSample );

    void WriteSample( [In] uint dwInputNum,

                      [In] ulong cnsSampleTime,

                      [In] uint dwFlags,

                      [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample );

    void Flush();

  }



  [ComImport]

  [Guid("96406BE3-2B2B-11d3-B36B-00C04F6108FF")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterAdvanced 

  {

    void GetSinkCount( [Out] out uint pcSinks );



    void GetSink( [In] uint dwSinkNum,

                  [Out, MarshalAs(UnmanagedType.Interface)] out IWMWriterSink ppSink );

    void AddSink( [In, MarshalAs(UnmanagedType.Interface)] IWMWriterSink pSink );

    void RemoveSink( [In, MarshalAs(UnmanagedType.Interface)] IWMWriterSink pSink );

    void WriteStreamSample( [In] ushort wStreamNum,

                            [In] ulong cnsSampleTime,

                            [In] uint msSampleSendTime,

                            [In] ulong cnsSampleDuration,

                            [In] uint dwFlags,

                            [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample );

    void SetLiveSource( [MarshalAs(UnmanagedType.Bool)]bool fIsLiveSource );

    void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    void GetWriterTime( [Out] out ulong pcnsCurrentTime );

    void GetStatistics( [In] ushort wStreamNum,

                        [Out] out WM_WRITER_STATISTICS pStats );

    void SetSyncTolerance( [In] uint   msWindow );

    void GetSyncTolerance( [Out] out uint  pmsWindow );

  }



  [ComImport]

  [Guid("962dc1ec-c046-4db8-9cc7-26ceae500817")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterAdvanced2 : IWMWriterAdvanced

  {

    //IWMWriterAdvanced

    new void GetSinkCount( [Out] out uint pcSinks );

    new void GetSink( [In] uint dwSinkNum,

      [Out, MarshalAs(UnmanagedType.Interface)] out IWMWriterSink ppSink );

    new void AddSink( [In, MarshalAs(UnmanagedType.Interface)] IWMWriterSink pSink );

    new void RemoveSink( [In, MarshalAs(UnmanagedType.Interface)] IWMWriterSink pSink );

    new void WriteStreamSample( [In] ushort wStreamNum,

      [In] ulong cnsSampleTime,

      [In] uint msSampleSendTime,

      [In] ulong cnsSampleDuration,

      [In] uint dwFlags,

      [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample );

    new void SetLiveSource( [MarshalAs(UnmanagedType.Bool)]bool fIsLiveSource );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void GetWriterTime( [Out] out ulong pcnsCurrentTime );

    new void GetStatistics( [In] ushort wStreamNum,

      [Out] out WM_WRITER_STATISTICS pStats );

    new void SetSyncTolerance( [In] uint   msWindow );

    new void GetSyncTolerance( [Out] out uint  pmsWindow );

    //IWMWriterAdvanced2

    void GetInputSetting([In] uint dwInputNum,

                         [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,

                         [Out] out WMT_ATTR_DATATYPE pType,

                         [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,

                         [In, Out] ref ushort pcbLength );

    void SetInputSetting( [In] uint dwInputNum,

                          [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,

                          [In] WMT_ATTR_DATATYPE Type,

                          [In, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex=4)] byte[] pValue,

                          [In] ushort cbLength );

  }



  [ComImport]

  [Guid("2cd6492d-7c37-4e76-9d3b-59261183a22e")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterAdvanced3 : IWMWriterAdvanced2

  {

    //IWMWriterAdvanced

    new void GetSinkCount( [Out] out uint pcSinks );

    new void GetSink( [In] uint dwSinkNum,

      [Out, MarshalAs(UnmanagedType.Interface)] out IWMWriterSink ppSink );

    new void AddSink( [In, MarshalAs(UnmanagedType.Interface)] IWMWriterSink pSink );

    new void RemoveSink( [In, MarshalAs(UnmanagedType.Interface)] IWMWriterSink pSink );

    new void WriteStreamSample( [In] ushort wStreamNum,

      [In] ulong cnsSampleTime,

      [In] uint msSampleSendTime,

      [In] ulong cnsSampleDuration,

      [In] uint dwFlags,

      [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample );

    new void SetLiveSource( [MarshalAs(UnmanagedType.Bool)]bool fIsLiveSource );

    new void IsRealTime( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfRealTime );

    new void GetWriterTime( [Out] out ulong pcnsCurrentTime );

    new void GetStatistics( [In] ushort wStreamNum,

      [Out] out WM_WRITER_STATISTICS pStats );

    new void SetSyncTolerance( [In] uint   msWindow );

    new void GetSyncTolerance( [Out] out uint  pmsWindow );

    //IWMWriterAdvanced2

    new void GetInputSetting([In] uint dwInputNum,

      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,

      [Out] out WMT_ATTR_DATATYPE pType,

      [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,

      [In, Out] ref ushort pcbLength );

    new void SetInputSetting( [In] uint dwInputNum,

      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,

      [In] WMT_ATTR_DATATYPE Type,

      [In, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex=4)] byte[] pValue,

      [In] ushort cbLength );

    //IWMWriterAdvanced3

    void GetStatisticsEx( [In] ushort wStreamNum,

                          [Out] out WM_WRITER_STATISTICS_EX pStats );

    void SetNonBlocking( );

  };



  [ComImport]

  [Guid("fc54a285-38c4-45b5-aa23-85b9f7cb424b")]

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

  public interface IWMWriterPreprocess 

  {

    void GetMaxPreprocessingPasses( [In] uint dwInputNum,

                                    [In] uint dwFlags,

                                    [Out] out uint pdwMaxNumPasses );

    void SetNumPreprocessingPasses( [In] uint dwInputNum,

                                    [In] uint dwFlags,

                                    [In] uint dwNumPasses );

    void BeginPreprocessingPass( [In] uint dwInputNum, [In] uint dwFlags );

    void PreprocessSample( [In] uint dwInputNum,

                           [In] ulong cnsSampleTime,

                           [In] uint dwFlags,

                           [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample );

    void EndPreprocessingPass( [In] uint dwInputNum, [In] uint dwFlags );

  }

}

