/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

namespace DShowNET
{


  public enum RecordingType : uint
  {
    ContentRecording=0,
    ReferenceRecording=1
  };

	[ComVisible(true), ComImport,
	Guid("6B652FFF-11FE-4fce-92AD-0266B5D7C78F"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface ISampleGrabber
{
		[PreserveSig]
	int SetOneShot(
		[In, MarshalAs(UnmanagedType.Bool)]				bool	OneShot );

		[PreserveSig]
	int SetMediaType(
		[In ]ref		AMMediaType	pmt );

		[PreserveSig]
	int GetConnectedMediaType(
		[Out, MarshalAs(UnmanagedType.LPStruct)]		AMMediaType	pmt );

		[PreserveSig]
	int SetBufferSamples(
		[In, MarshalAs(UnmanagedType.Bool)]				bool	BufferThem );

		[PreserveSig]
	int GetCurrentBuffer( ref int pBufferSize, IntPtr pBuffer );

		[PreserveSig]
	int GetCurrentSample( IntPtr ppSample );

		[PreserveSig]
	int SetCallback( ISampleGrabberCB pCallback, int WhichMethodToCallback );
}



	[ComVisible(true), ComImport,
	Guid("0579154A-2B53-4994-B0D0-E773148EFF85"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface ISampleGrabberCB
{
		[PreserveSig]
	int SampleCB( double SampleTime, IMediaSample pSample );

		[PreserveSig]
	int BufferCB( double SampleTime, IntPtr pBuffer, int BufferLen );
}



[ComVisible(true), ComImport,
	Guid("56A868A6-0AD4-11CE-B03A-0020AF0BA770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IFileSourceFilter
{
	[PreserveSig]
	int Load(string FileName, IntPtr pmt);

	[PreserveSig]
	int GetCurFile(out string FileName, AMMediaType pmt);

}

	[ComVisible(true), ComImport,
	Guid("ce14dfae-4098-4af7-bbf7-d6511f835414"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IStreamBufferConfigure //: IStreamBufferConfigure
	{
		[PreserveSig]
		int SetDirectory([MarshalAs(UnmanagedType.LPWStr)] string directoryName);
		[PreserveSig]
		int GetDirectory([MarshalAs(UnmanagedType.LPWStr)] out string directoryName);

		[PreserveSig]
		int SetBackingFileCount(uint min, uint max);
		[PreserveSig]
		int GetBackingFileCount(out uint min, out uint max);

		[PreserveSig]
		int SetBackingFileDuration(uint secs);
		[PreserveSig]
		int GetBackingFileDuration(out uint secs);
	}
	[ComVisible(true), ComImport,
	Guid("53E037BF-3992-4282-AE34-2487B4DAE06B"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IStreamBufferConfigure2 //: IStreamBufferConfigure
	{
		[PreserveSig]
		int SetDirectory([MarshalAs(UnmanagedType.LPWStr)] string directoryName);
		[PreserveSig]
		int GetDirectory([MarshalAs(UnmanagedType.LPWStr)] out string directoryName);

		[PreserveSig]
		int SetBackingFileCount(uint min, uint max);
		[PreserveSig]
		int GetBackingFileCount(out uint min, out uint max);

		[PreserveSig]
		int SetBackingFileDuration(uint secs);
		[PreserveSig]
		int GetBackingFileDuration(out uint secs);

		[PreserveSig]
		int SetMultiplexedPacketSize ([In]    uint   cbBytesPerPacket) ;
		[PreserveSig]
		int GetMultiplexedPacketSize ([Out]   out uint pcbBytesPerPacket) ;
		[PreserveSig]
		int SetFFTransitionRates ( [In]    uint   dwMaxFullFrameRate, [In]    uint dwMaxNonSkippingRate ) ;
		[PreserveSig]
		int GetFFTransitionRates ( [Out]   out uint pdwMaxFullFrameRate, [Out]   out uint pdwMaxNonSkippingRate ) ;


	}



	[ComVisible(true), ComImport,
	Guid("f61f5c26-863d-4afa-b0ba-2f81dc978596"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStreamBufferMediaSeeking 
	{
    [PreserveSig]
    int GetCapabilities( [Out] out SeekingCapabilities pCapabilities );

    [PreserveSig]
    int CheckCapabilities( [In, Out] ref SeekingCapabilities pCapabilities );

    [PreserveSig]
    int IsFormatSupported( [In] ref Guid pFormat );
    [PreserveSig]
    int QueryPreferredFormat( [Out] out Guid pFormat );

    [PreserveSig]
    int GetTimeFormat( [Out] out Guid pFormat );
    [PreserveSig]
    int IsUsingTimeFormat( [In] ref Guid pFormat );
    [PreserveSig]
    int SetTimeFormat( [In] ref Guid pFormat );

    [PreserveSig]
    int GetDuration( [Out] out long pDuration );
    [PreserveSig]
    int GetStopPosition( [Out] out long pStop );
    [PreserveSig]
    int GetCurrentPosition( [Out] out long pCurrent );

    [PreserveSig]
    int ConvertTimeFormat(	[Out] out long pTarget, [In] ref Guid pTargetFormat,
                            [In] long Source,  [In] ref Guid pSourceFormat );

    [PreserveSig]
    int SetPositions([In,Out] ref long pCurrent, [In] SeekingFlags dwCurrentFlags,
                     [In,Out] ref long pStop, [In] SeekingFlags dwStopFlags );

    [PreserveSig]
    int GetPositions( [Out] out long pCurrent, [Out] out long pStop );

    [PreserveSig]
    int GetAvailable( [Out] out long pEarliest, [Out] out long pLatest );

    [PreserveSig]
    int SetRate( [In] double dRate );

    [PreserveSig]
    int GetRate( [Out] out double pdRate );

    [PreserveSig]
    int GetPreroll( [Out] out long pllPreroll );

	}


	[ComVisible(true), ComImport,
	Guid("3a439ab0-155f-470a-86a6-9ea54afd6eaf"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStreamBufferMediaSeeking2
	{
		[PreserveSig]
		int GetCapabilities( [Out] out SeekingCapabilities pCapabilities );

		[PreserveSig]
		int CheckCapabilities( [In, Out] ref SeekingCapabilities pCapabilities );

		[PreserveSig]
		int IsFormatSupported( [In] ref Guid pFormat );
		[PreserveSig]
		int QueryPreferredFormat( [Out] out Guid pFormat );

		[PreserveSig]
		int GetTimeFormat( [Out] out Guid pFormat );
		[PreserveSig]
		int IsUsingTimeFormat( [In] ref Guid pFormat );
		[PreserveSig]
		int SetTimeFormat( [In] ref Guid pFormat );

		[PreserveSig]
		int GetDuration( [Out] out long pDuration );
		[PreserveSig]
		int GetStopPosition( [Out] out long pStop );
		[PreserveSig]
		int GetCurrentPosition( [Out] out long pCurrent );

		[PreserveSig]
		int ConvertTimeFormat(	[Out] out long pTarget, [In] ref Guid pTargetFormat,
			[In] long Source,  [In] ref Guid pSourceFormat );

		[PreserveSig]
		int SetPositions([In,Out] ref long pCurrent, [In] SeekingFlags dwCurrentFlags,
			[In,Out] ref long pStop, [In] SeekingFlags dwStopFlags );

		[PreserveSig]
		int GetPositions( [Out] out long pCurrent, [Out] out long pStop );

		[PreserveSig]
		int GetAvailable( [Out] out long pEarliest, [Out] out long pLatest );

		[PreserveSig]
		int SetRate( [In] double dRate );

		[PreserveSig]
		int GetRate( [Out] out double pdRate );

		[PreserveSig]
		int GetPreroll( [Out] out long pllPreroll );

		[PreserveSig]
		int SetRateEx ([In]    double  dRate,[In]    uint   dwFramesPerSec) ;

	}

	[ComVisible(true), ComImport,
	Guid("ba9b6c99-f3c7-4ff2-92db-cfdd4851bf31"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IStreamBufferRecordControl //: IMediaSeeking
	{
		[PreserveSig]
		int Start([In,Out]ref long referenceTime); // we may need to change this

		[PreserveSig]
		int Stop(long referenceTime);

		[PreserveSig]
		int GetRecordingStatus(out int hResult, out bool started, out bool stopped);
	}


  [ComVisible(true), ComImport,
  Guid("afd1f242-7efd-45ee-ba4e-407a25c9a77a"),   //IStreamBufferSink
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IStreamBufferSink
  {
    [PreserveSig]
    int LockProfile(string FileName);

    [PreserveSig]
    int CreateRecorder(string FileName, uint dwRecordType, out IntPtr pRecording);

    [PreserveSig]
    int IsProfileLocked();
  }

  
  [ComVisible(true), ComImport,
  Guid("974723f2-887a-4452-9366-2cff3057bc8f"),     //IStreamBufferSink3
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IStreamBufferSink3
  {
    [PreserveSig]
    int LockProfile(string FileName);

    [PreserveSig]
    int CreateRecorder(string FileName, uint dwRecordType, out IntPtr pRecording);

    [PreserveSig]
    int IsProfileLocked();
    
    [PreserveSig]
    int UnlockProfile();

    [PreserveSig]
    int SetAvailableFilter ([In, Out]  ref long prtMin) ;
  }

[ComVisible(true), ComImport,
	Guid("1c5bd776-6ced-4f44-8164-5eab0e98db12"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IStreamBufferSource
{
	[PreserveSig]
	int SetStreamSink( IStreamBufferSink pIStreamBufferSink);
}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public class VideoInfoHeader		// VIDEOINFOHEADER
{
	public DsRECT	SrcRect;
	public DsRECT	TagRect;
	public int		BitRate;
	public int		BitErrorRate;
	public long					AvgTimePerFrame;
	public DsBITMAPINFOHEADER	BmiHeader;
}


} // namespace DShowNET
