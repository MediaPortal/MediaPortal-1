/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text;

namespace Tag.WMA
{
  public enum WMT_STREAM_SELECTION
  {
    WMT_OFF = 0,
    WMT_CLEANPOINT_ONLY = 1,
    WMT_ON = 2,
  };

  public enum WMT_VERSION
  {
    WMT_VER_4_0 = 0x00040000,
    WMT_VER_7_0 = 0x00070000,
    WMT_VER_8_0 = 0x00080000,
    WMT_VER_9_0 = 0x00090000,
  };

  [StructLayout(LayoutKind.Sequential)]
  public struct WMT_TIMECODE_EXTENSION_DATA
  {
    public ushort wRange;
    public uint dwTimecode;
    public uint dwUserbits;
    public uint dwAmFlags;
  };

  // IWMSyncReader
  [
      ComImport,
      Guid("9397F121-7705-4dc9-B049-98B698188414"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMSyncReader
  {
    void Open([In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename);

    void Close();

    void SetRange([In] ulong cnsStartTime, [In] long cnsDuration);

    void SetRangeByFrame([In] ushort wStreamNum, [In] ulong qwFrameNumber, [In]long cFramesToRead);

    void GetNextSample(
        [In] ushort wStreamNum,
        [Out] out INSSBuffer ppSample,
        [Out] out ulong pcnsSampleTime,
        [Out] out ulong pcnsDuration,
        [Out] out uint pdwFlags,
        [Out] out uint pdwOutputNum,
        [Out] out ushort pwStreamNum);

    void SetStreamsSelected(
        [In] ushort cStreamCount,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ushort[] pwStreamNumbers,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] WMT_STREAM_SELECTION[] pSelections);

    void GetStreamSelected(
        [In]ushort wStreamNum,
        [Out] out WMT_STREAM_SELECTION pSelection);

    void SetReadStreamSamples(
        [In] ushort wStreamNum,
        [In, MarshalAs(UnmanagedType.Bool)] bool fCompressed);

    void GetReadStreamSamples(
        [In] ushort wStreamNum,
        [Out, MarshalAs(UnmanagedType.Bool)] out bool pfCompressed);

    void GetOutputSetting(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
        [Out] out WMT_ATTR_DATATYPE pType,
        [In, Out] ref uint pcbLength);

    void SetOutputSetting(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
        [In] WMT_ATTR_DATATYPE Type,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pValue,
        [In] uint cbLength);

    void GetOutputCount([Out] out uint pcOutputs);

    void GetOutputProps([In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppOutput);

    void SetOutputProps([In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Interface)] IWMOutputMediaProps pOutput);

    void GetOutputFormatCount([In] uint dwOutputNum, [Out] out uint pcFormats);

    void GetOutputFormat(
        [In] uint dwOutputNum,
        [In] uint dwFormatNum,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppProps);

    void GetOutputNumberForStream([In] ushort wStreamNum, [Out] out uint pdwOutputNum);

    void GetStreamNumberForOutput([In] uint dwOutputNum, [Out] out ushort pwStreamNum);

    void GetMaxOutputSampleSize([In] uint dwOutput, [Out] out uint pcbMax);

    void GetMaxStreamSampleSize([In] ushort wStream, [Out] out uint pcbMax);

    void OpenStream([In, MarshalAs(UnmanagedType.Interface)] System.Runtime.InteropServices.ComTypes.IStream pStream);
  }

  // IWMSyncReader2
  [
      ComImport,
      Guid("faed3d21-1b6b-4af7-8cb6-3e189bbc187b"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMSyncReader2 : IWMSyncReader
  {
    new void Open([In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename);

    new void Close();

    new void SetRange([In] ulong cnsStartTime, [In] long cnsDuration);

    new void SetRangeByFrame([In] ushort wStreamNum, [In] ulong qwFrameNumber, [In]long cFramesToRead);

    new void GetNextSample(
        [In] ushort wStreamNum,
        [Out] out INSSBuffer ppSample,
        [Out] out ulong pcnsSampleTime,
        [Out] out ulong pcnsDuration,
        [Out] out uint pdwFlags,
        [Out] out uint pdwOutputNum,
        [Out] out ushort pwStreamNum);

    new void SetStreamsSelected(
        [In] ushort cStreamCount,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ushort[] pwStreamNumbers,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] WMT_STREAM_SELECTION[] pSelections);

    new void GetStreamSelected(
        [In]ushort wStreamNum,
        [Out] out WMT_STREAM_SELECTION pSelection);

    new void SetReadStreamSamples(
        [In] ushort wStreamNum,
        [In, MarshalAs(UnmanagedType.Bool)] bool fCompressed);

    new void GetReadStreamSamples(
        [In] ushort wStreamNum,
        [Out, MarshalAs(UnmanagedType.Bool)] out bool pfCompressed);

    new void GetOutputSetting(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
        [Out] out WMT_ATTR_DATATYPE pType,
        [In, Out] ref uint pcbLength);

    new void SetOutputSetting(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
        [In] WMT_ATTR_DATATYPE Type,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pValue,
        [In] uint cbLength);

    new void GetOutputCount([Out] out uint pcOutputs);

    new void GetOutputProps(
        [In] uint dwOutputNum,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppOutput);

    new void SetOutputProps(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.Interface)] IWMOutputMediaProps pOutput);

    new void GetOutputFormatCount([In] uint dwOutputNum, [Out] out uint pcFormats);

    new void GetOutputFormat(
        [In] uint dwOutputNum,
        [In] uint dwFormatNum,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppProps);

    new void GetOutputNumberForStream([In] ushort wStreamNum, [Out] out uint pdwOutputNum);

    new void GetStreamNumberForOutput([In] uint dwOutputNum, [Out] out ushort pwStreamNum);

    new void GetMaxOutputSampleSize([In] uint dwOutput, [Out] out uint pcbMax);

    new void GetMaxStreamSampleSize([In] ushort wStream, [Out] out uint pcbMax);

    new void OpenStream([In, MarshalAs(UnmanagedType.Interface)] System.Runtime.InteropServices.ComTypes.IStream pStream);

    //IWMSyncReader2

    void SetRangeByTimecode(
        [In] ushort wStreamNum,
        [In] ref WMT_TIMECODE_EXTENSION_DATA pStart,
        [In] ref WMT_TIMECODE_EXTENSION_DATA pEnd);

    void SetRangeByFrameEx(
        [In] ushort wStreamNum,
        [In] ulong qwFrameNumber,
        [In] long cFramesToRead,
        [Out] out ulong pcnsStartTime);

    void SetAllocateForOutput(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.Interface)] IWMReaderAllocatorEx pAllocator);

    void GetAllocateForOutput(
        [In] uint dwOutputNum,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMReaderAllocatorEx ppAllocator);

    void SetAllocateForStream(
        [In] ushort wStreamNum,
        [In, MarshalAs(UnmanagedType.Interface)] IWMReaderAllocatorEx pAllocator);

    void GetAllocateForStream(
        [In] ushort dwSreamNum,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMReaderAllocatorEx ppAllocator);
  }

  // IWMReaderAllocatorEx
  [
      ComImport,
      Guid("9F762FA7-A22E-428d-93C9-AC82F3AAFE5A"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMReaderAllocatorEx
  {
    void AllocateForStreamEx(
        [In] ushort wStreamNum,
        [In] uint cbBuffer,
        [Out] out INSSBuffer ppBuffer,
        [In] uint dwFlags,
        [In] ulong cnsSampleTime,
        [In] ulong cnsSampleDuration,
        [In] IntPtr pvContext);



    void AllocateForOutputEx(
        [In] uint dwOutputNum,
        [In] uint cbBuffer,
        [Out] out INSSBuffer ppBuffer,
        [In] uint dwFlags,
        [In] ulong cnsSampleTime,
        [In] ulong cnsSampleDuration,
        [In] IntPtr pvContext);
  }

  // IWMProfileManager
  [
      ComImport,
      Guid("d16679f2-6ca0-472d-8d31-2f5d55aee155"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMProfileManager
  {
    void CreateEmptyProfile(
        [In] WMT_VERSION dwVersion,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);

    void LoadProfileByID(
        [In] ref Guid guidProfile,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);

    void LoadProfileByData(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszProfile,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);

    void SaveProfile(
        [In, MarshalAs(UnmanagedType.Interface)] IWMProfile pIWMProfile,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszProfile,
        [In, Out] ref uint pdwLength);

    void GetSystemProfileCount([Out] out uint pcProfiles);

    void LoadSystemProfile(
        [In] uint dwProfileIndex,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);

  }

  // IWMProfile
  [
      ComImport,
      Guid("96406BDB-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMProfile
  {

    void GetVersion([Out] out WMT_VERSION pdwVersion);

    void GetName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                 [In, Out] ref uint pcchName);

    void SetName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszName);

    void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszDescription,
                        [In, Out] ref uint pcchDescription);

    void SetDescription([In, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription);

    void GetStreamCount([Out] out uint pcStreams);

    void GetStream([In] uint dwStreamIndex, [Out, MarshalAs(UnmanagedType.Interface)] out IWMStreamConfig ppConfig);

    void GetStreamByNumber([In] ushort wStreamNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMStreamConfig ppConfig);

    void RemoveStream([In, MarshalAs(UnmanagedType.Interface)] IWMStreamConfig pConfig);

    void RemoveStreamByNumber([In] ushort wStreamNum);

    void AddStream([In, MarshalAs(UnmanagedType.Interface)] IWMStreamConfig pConfig);

    void ReconfigStream([In, MarshalAs(UnmanagedType.Interface)] IWMStreamConfig pConfig);

    void CreateNewStream([In] ref Guid guidStreamType,
                         [Out, MarshalAs(UnmanagedType.Interface)] out IWMStreamConfig ppConfig);

    void GetMutualExclusionCount([Out] out uint pcME);

    void GetMutualExclusion([In] uint dwMEIndex,
                            [Out, MarshalAs(UnmanagedType.Interface)] out IWMMutualExclusion ppME);

    void RemoveMutualExclusion([In, MarshalAs(UnmanagedType.Interface)] IWMMutualExclusion pME);

    void AddMutualExclusion([In, MarshalAs(UnmanagedType.Interface)] IWMMutualExclusion pME);

    void CreateNewMutualExclusion([Out, MarshalAs(UnmanagedType.Interface)] out IWMMutualExclusion ppME);

  }

  // IWMStreamConfig
  [
      ComImport,
      Guid("96406BDC-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMStreamConfig
  {

    void GetStreamType([Out] out Guid pguidStreamType);

    void GetStreamNumber([Out] out ushort pwStreamNum);

    void SetStreamNumber([In] ushort wStreamNum);

    void GetStreamName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszStreamName,
                       [In, Out] ref ushort pcchStreamName);

    void SetStreamName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszStreamName);

    void GetConnectionName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszInputName,
                           [In, Out] ref ushort pcchInputName);

    void SetConnectionName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszInputName);

    void GetBitrate([Out] out uint pdwBitrate);

    void SetBitrate([In] uint pdwBitrate);

    void GetBufferWindow([Out] out uint pmsBufferWindow);

    void SetBufferWindow([In] uint msBufferWindow);

  };

  // IWMMutualExclusion
  [
      ComImport,
      Guid("96406BDE-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMMutualExclusion : IWMStreamList
  {
    new void GetStreams([Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
     [In, Out] ref ushort pcStreams);

    new void AddStream([In] ushort wStreamNum);

    new void RemoveStream([In] ushort wStreamNum);

    void GetType([Out] out Guid pguidType);

    void SetType([In] ref Guid guidType);
  };

  // IWMMutualExclusion2
  [
      ComImport,
      Guid("0302B57D-89D1-4ba2-85C9-166F2C53EB91"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMMutualExclusion2 : IWMMutualExclusion
  {
    new void GetStreams([Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
     [In, Out] ref ushort pcStreams);

    new void AddStream([In] ushort wStreamNum);

    new void RemoveStream([In] ushort wStreamNum);

    //IWMMutualExclusion

    new void GetType([Out] out Guid pguidType);

    new void SetType([In] ref Guid guidType);

    //IWMMutualExclusion2

    void GetName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                 [In, Out] ref ushort pcchName);

    void SetName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszName);

    void GetRecordCount([Out] out ushort pwRecordCount);

    void AddRecord();

    void RemoveRecord([In] ushort wRecordNumber);

    void GetRecordName([In] ushort wRecordNumber,
                       [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszRecordName,
                       [In, Out] ref ushort pcchRecordName);

    void SetRecordName([In] ushort wRecordNumber,
                       [In, MarshalAs(UnmanagedType.LPWStr)] string pwszRecordName);

    void GetStreamsForRecord([In] ushort wRecordNumber,
                             [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
                             [In, Out] ref ushort pcStreams);

    void AddStreamForRecord([In] ushort wRecordNumber, [In] ushort wStreamNumber);

    void RemoveStreamForRecord([In] ushort wRecordNumber, [In] ushort wStreamNumber);

  }

  // IWMBandwidthSharing
  [
      ComImport,
      Guid("AD694AF1-F8D9-42F8-BC47-70311B0C4F9E"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMBandwidthSharing : IWMStreamList
  {
    new void GetStreams([Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
     [In, Out] ref ushort pcStreams);

    new void AddStream([In] ushort wStreamNum);

    new void RemoveStream([In] ushort wStreamNum);

    //IWMBandwidthSharing    

    void GetType([Out] out Guid pguidType);

    void SetType([In] ref Guid guidType);

    void GetBandwidth([Out] out uint pdwBitrate, [Out] out uint pmsBufferWindow);

    void SetBandwidth([In] uint dwBitrate, [In] uint msBufferWindow);

  }

  // IWMStreamList
  [
      ComImport,
      Guid("96406BDD-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMStreamList
  {
    void GetStreams([Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
                    [In, Out] ref ushort pcStreams);

    void AddStream([In] ushort wStreamNum);

    void RemoveStream([In] ushort wStreamNum);
  };

  //IWMReader
  [
      ComImport,
      Guid("96406BD6-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMReader
  {
    void Open(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
        [In, MarshalAs(UnmanagedType.Interface)] IWMReaderCallback pCallback,
        [In] IntPtr pvContext);

    void Close();

    void GetOutputCount(
        [Out, MarshalAs(UnmanagedType.U4)] out uint pcOutputs);

    void GetOutputProps(
        [In, MarshalAs(UnmanagedType.U4)] uint dwOutputNum,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppOutput);

    void SetOutputProps(
        [In] uint dwOutputNum,
        [In, MarshalAs(UnmanagedType.Interface)] IWMOutputMediaProps pOutput);

    void GetOutputFormatCount(
        [In] uint dwOutputNumber,
        [Out] out uint pcFormats);

    void GetOutputFormat(
        [In] uint dwOutputNumber,
        [In] uint dwFormatNumber,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppProps);

    void Start(
        [In] ulong cnsStart,
        [In] ulong cnsDuration,
        [In] float fRate,
        [In] IntPtr pvContext);

    void Stop();
    void Pause();
    void Resume();

  }

  // IWMOutputMediaProps
  [
      ComImport,
      Guid("96406BD7-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMOutputMediaProps : IWMMediaProps
  {
    void GetStreamGroupName(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
        [In, Out] ref ushort pcchName);

    void GetConnectionName(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
        [In, Out] ref ushort pcchName);
  }

  //IWMStatusCallback
  [
      ComImport,
      Guid("96406BD8-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMReaderCallback : IWMStatusCallback
  {
    //IWMStatusCallback
    new void OnStatus(
        [In] WMT_STATUS Status,
        [In] IntPtr hr,
        [In] WMT_ATTR_DATATYPE dwType,
        [In] IntPtr pValue,
        [In] IntPtr pvContext);

    //IWMReaderCallback
    void OnSample(
        [In] uint dwOutputNum,
        [In] ulong cnsSampleTime,
        [In] ulong cnsSampleDuration,
        [In] uint dwFlags,
        [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample,
        [In] IntPtr pvContext);
  }

  //IWMStatusCallback
  [
      ComImport,
      Guid("6d7cdc70-9888-11d3-8edc-00c04f6109cf"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMStatusCallback
  {
    void OnStatus(
        [In] WMT_STATUS Status,
        [In] IntPtr hr,
        [In] WMT_ATTR_DATATYPE dwType,
        [In] IntPtr pValue,
        [In] IntPtr pvContext);
  }

  //INSSBuffer
  [
      ComImport,
      Guid("E1CD3524-03D7-11d2-9EED-006097D2D7CF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface INSSBuffer
  {
    void GetLength(
        [Out] out uint pdwLength);

    void SetLength(
        [In] uint dwLength);

    void GetMaxLength(
        [Out] out uint pdwLength);

    void GetBuffer(
        [Out] out IntPtr ppdwBuffer);

    void GetBufferAndLength(
        [Out] out IntPtr ppdwBuffer,
        [Out] out uint pdwLength);
  }

  // IWMMediaProps
  [
      ComImport,
      Guid("96406BCE-2B2B-11d3-B36B-00C04F6108FF"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMMediaProps
  {
    void GetType(
        [Out] out Guid pguidType);

    void GetMediaType(
        IntPtr pType,
        [In, Out] ref uint pcbType);

    void SetMediaType([In] ref WM_MEDIA_TYPE pType);
  }

  // IWMHeaderInfo3
  [
      ComImport,
      Guid("15cc68e3-27cc-4ecd-b222-3f5d02d80bd5"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMHeaderInfo3
  {
    void GetAttributeCount([In]	ushort wStreamNum, [Out] out ushort pcAttributes);


    void GetAttributeByIndex(
        [In] ushort wIndex,
        [Out, In] ref ushort pwStreamNum,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
        [Out, In] ref ushort pcchNameLen,
        [Out] out WMT_ATTR_DATATYPE pType,
        [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
        [Out, In] ref ushort pcbLength);


    void GetAttributeByName(
       [Out, In] ref ushort pwStreamNum,
       [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
       [Out] out WMT_ATTR_DATATYPE pType,
       [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
       [Out, In] ref ushort pcbLength);


    uint SetAttribute(
        [In]									ushort wStreamNum,
        [In, MarshalAs(UnmanagedType.LPWStr)]	string pszName,
        [In]									WMT_ATTR_DATATYPE Type,
        [In, MarshalAs(UnmanagedType.LPArray)]	byte[] pValue,
        [In]									ushort cbLength);

    uint ModifyAttribute(
        [In]									ushort wStreamNum,
        [In]									ushort wIndex,
        [In]									WMT_ATTR_DATATYPE Type,
        [In]									ushort wLangIndex,
        [In, MarshalAs(UnmanagedType.LPArray)]	byte[] pValue,
        [In]									uint dwLength);

    uint AddAttribute(
        [In]									ushort wStreamNum,
        [In, MarshalAs(UnmanagedType.LPWStr)]	string pszName,
        [Out]									out ushort pwIndex,
        [In]									WMT_ATTR_DATATYPE Type,
        [In]									ushort wLangIndex,
        [In, MarshalAs(UnmanagedType.LPArray)]	byte[] pValue,
        [In]									uint dwLength);

    uint DeleteAttribute(
        [In]									ushort wStreamNum,
        [In]									ushort wIndex);
  }


  // IWMMetadataEditor2
  [
      ComImport,
      Guid("203cffe3-2e18-4fdf-b59d-6e71530534cf"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMMetadataEditor2
  {
    void Open([In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename);

    void Flush();

    void Close();
  }


  public class WindowsMediaWrapper// : IDisposable
  {
    public static Guid WMFORMAT_WaveFormatEx { get { return new Guid("05589F81-C356-11CE-BF01-00AA0055595A"); } }

    // We don't want this class to be instantiated
    private WindowsMediaWrapper()
    {
    }

    [DllImport("WMVCore.dll", EntryPoint = "WMCreateEditor", PreserveSig = false,
        CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    public static extern void CreateEditor(
        [Out, MarshalAs(UnmanagedType.Interface)] 
            out IWMMetadataEditor2 ppEditor);


    [DllImport("WMVCore.dll", EntryPoint = "WMCreateReader",
        SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int WMCreateReader(
        IntPtr pUnkReserved,
        WMT_RIGHTS dwRights,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMReader ppReader);


    [DllImport("WMVCore.dll", EntryPoint = "WMCreateSyncReader", SetLastError = true,
        CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int WMCreateSyncReader(
        IntPtr pUnkCert,
        WMT_RIGHTS dwRights,
        [Out, MarshalAs(UnmanagedType.Interface)] out IWMSyncReader ppSyncReader);


    [DllImport("WMVCore.dll", EntryPoint = "WMCreateSyncReader", SetLastError = true,
    CharSet = CharSet.Unicode, ExactSpelling = true,
    CallingConvention = CallingConvention.StdCall)]
    private static extern int WMCreateProfileManager([Out, MarshalAs(UnmanagedType.Interface)] out IWMProfileManager ppProfileManager);
  }

  // Just the ones we handle
  public enum ComError
  {
    ASF_E_NOTFOUND = -1072887824,
    NS_E_FILE_OPEN_FAILED = -1072889827
  }

  public enum WMT_ATTR_DATATYPE : int
  {
    WMT_TYPE_DWORD = 0,
    WMT_TYPE_STRING = 1,
    WMT_TYPE_BINARY = 2,
    WMT_TYPE_BOOL = 3,
    WMT_TYPE_QWORD = 4,
    WMT_TYPE_WORD = 5,
    WMT_TYPE_GUID = 6,
  }

  public enum WMT_RIGHTS : uint
  {
    // Not part of the SDK but allows playing files without DRM info
    WMT_RIGHT_NODRM = 0x00000000,
    WMT_RIGHT_PLAYBACK = 0x00000001,
    WMT_RIGHT_COPY_TO_NON_SDMI_DEVICE = 0x00000002,
    WMT_RIGHT_COPY_TO_CD = 0x00000008,
    WMT_RIGHT_COPY_TO_SDMI_DEVICE = 0x00000010,
    WMT_RIGHT_ONE_TIME = 0x00000020,
    WMT_RIGHT_SAVE_STREAM_PROTECTED = 0x00000040,
    WMT_RIGHT_SDMI_TRIGGER = 0x00010000,
    WMT_RIGHT_SDMI_NOMORECOPIES = 0x00020000
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WM_MEDIA_TYPE
  {
    public Guid majortype;
    public Guid subtype;

    [MarshalAs(UnmanagedType.Bool)]
    public bool bFixedSizeSamples;

    [MarshalAs(UnmanagedType.Bool)]
    public bool bTemporalCompression;

    public uint lSampleSize;
    public Guid formattype;
    public IntPtr pUnk;
    public uint cbFormat;
    public IntPtr pbFormat;
  };

  public enum WMT_STATUS
  {
    WMT_ERROR = 0,
    WMT_OPENED = 1,
    WMT_BUFFERING_START = 2,
    WMT_BUFFERING_STOP = 3,
    WMT_EOF = 4,
    WMT_END_OF_FILE = 4,
    WMT_END_OF_SEGMENT = 5,
    WMT_END_OF_STREAMING = 6,
    WMT_LOCATING = 7,
    WMT_CONNECTING = 8,
    WMT_NO_RIGHTS = 9,
    WMT_MISSING_CODEC = 10,
    WMT_STARTED = 11,
    WMT_STOPPED = 12,
    WMT_CLOSED = 13,
    WMT_STRIDING = 14,
    WMT_TIMER = 15,
    WMT_INDEX_PROGRESS = 16,
    WMT_SAVEAS_START = 17,
    WMT_SAVEAS_STOP = 18,
    WMT_NEW_SOURCEFLAGS = 19,
    WMT_NEW_METADATA = 20,
    WMT_BACKUPRESTORE_BEGIN = 21,
    WMT_SOURCE_SWITCH = 22,
    WMT_ACQUIRE_LICENSE = 23,
    WMT_INDIVIDUALIZE = 24,
    WMT_NEEDS_INDIVIDUALIZATION = 25,
    WMT_NO_RIGHTS_EX = 26,
    WMT_BACKUPRESTORE_END = 27,
    WMT_BACKUPRESTORE_CONNECTING = 28,
    WMT_BACKUPRESTORE_DISCONNECTING = 29,
    WMT_ERROR_WITHURL = 30,
    WMT_RESTRICTED_LICENSE = 31,
    WMT_CLIENT_CONNECT = 32,
    WMT_CLIENT_DISCONNECT = 33,
    WMT_NATIVE_OUTPUT_PROPS_CHANGED = 34,
    WMT_RECONNECT_START = 35,
    WMT_RECONNECT_END = 36,
    WMT_CLIENT_CONNECT_EX = 37,
    WMT_CLIENT_DISCONNECT_EX = 38,
    WMT_SET_FEC_SPAN = 39,
    WMT_PREROLL_READY = 40,
    WMT_PREROLL_COMPLETE = 41,
    WMT_CLIENT_PROPERTIES = 42,
    WMT_LICENSEURL_SIGNATURE_STATE = 43
  };

  [StructLayout(LayoutKind.Sequential, Pack = 2)]
  public class WaveFormatEx
  {
    public short wFormatTag;
    public short nChannels;
    public int nSamplesPerSec;
    public int nAvgBytesPerSec;
    public short nBlockAlign;
    public short wBitsPerSample;
    public short cbSize;
  }
}