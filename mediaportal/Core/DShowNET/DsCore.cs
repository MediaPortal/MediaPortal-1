/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace DShowNET
{
  [StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode), ComVisible(false)]
  public struct PinInfo		// PIN_INFO
  {
    public IBaseFilter filter;
    public PinDirection dir;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] public string name;
  }

  [ComVisible(false)]
  public enum AmAspectRatioMode:int
  {
    AM_ARMODE_STRETCHED,    // don't do any aspect ratio correction
    AM_ARMODE_LETTER_BOX,   // letter box the video, paint background color in the excess region
    AM_ARMODE_CROP,         // crop the video to the right aspect ratio
    AM_ARMODE_STRETCHED_AS_PRIMARY  // follow whatever the primary stream does (in terms of the mode as well as pict-aspect-ratio values)
  } ;

  [ComVisible(false)]
  public enum VmrAspectRatioMode:int		// PIN_DIRECTION
  {
    VMR_ARMODE_NONE,
    VMR_ARMODE_LETTER_BOX
  } ;

	[ComVisible(false)]
public enum PinDirection		// PIN_DIRECTION
{
	Input,		// PINDIR_INPUT
	Output		// PINDIR_OUTPUT
}

	[ComVisible(false)]
	public enum PhysicalConnectorType
	{
		Video_Tuner = 1,
		Video_Composite,
		Video_SVideo,
		Video_RGB,
		Video_YRYBY,
		Video_SerialDigital,
		Video_ParallelDigital,
		Video_SCSI,
		Video_AUX,
		Video_1394,
		Video_USB,
		Video_VideoDecoder,
		Video_VideoEncoder,
		Video_SCART,

		Audio_Tuner = 4096,
		Audio_Line,
		Audio_Mic,
		Audio_AESDigital,
		Audio_SPDIFDigital,
		Audio_SCSI,
		Audio_AUX,
		Audio_1394,
		Audio_USB,
		Audio_AudioDecoder,
	};

	[ComVisible(false)]
public class DsHlp
{
	public const int OATRUE		= -1;
	public const int OAFALSE	= 0;

		[DllImport( "quartz.dll", CharSet=CharSet.Auto)]
	public static extern int AMGetErrorText( int hr, StringBuilder buf, int max );
}


	[ComVisible(true), ComImport,
	Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IPin
{
		[PreserveSig]
	int Connect(
		[In]											IPin		pReceivePin,
		[In]	ref		AMMediaType	pmt );

		[PreserveSig]
	int ReceiveConnection(
		[In]											IPin		pReceivePin,
		[In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType	pmt );

		[PreserveSig]
	int Disconnect();

		[PreserveSig]
	int ConnectedTo( [Out] out IPin ppPin );

		[PreserveSig]
	int ConnectionMediaType([Out] IntPtr pmt);

		[PreserveSig]
    int QueryPinInfo( [Out] out PinInfo pInfo );
	//int QueryPinInfo( IntPtr pInfo );

		[PreserveSig]
	int QueryDirection( out PinDirection pPinDir );

		[PreserveSig]
	int QueryId(
		[Out, MarshalAs(UnmanagedType.LPWStr) ]		out	string		Id );

		[PreserveSig]
	int QueryAccept(
		[In]		ref	AMMediaType	pmt );

		[PreserveSig]
	int EnumMediaTypes( [Out] out IEnumMediaTypes ppEnum );

		[PreserveSig]
	int QueryInternalConnections( IntPtr apPin, [In, Out] ref int nPin );

		[PreserveSig]
	int EndOfStream();

		[PreserveSig]
	int BeginFlush();

		[PreserveSig]
	int EndFlush();

		[PreserveSig]
	int NewSegment( long tStart, long tStop, double dRate );
}






	[ComVisible(true), ComImport,
	Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IFilterGraph
{
		[PreserveSig]
	int AddFilter(
		[In] IBaseFilter pFilter,
		[In, MarshalAs(UnmanagedType.LPWStr)]			string			pName );

		[PreserveSig]
	int RemoveFilter( [In] IBaseFilter pFilter );

		[PreserveSig]
	int EnumFilters( [Out] out IEnumFilters ppEnum );

		[PreserveSig]
	int FindFilterByName(
		[In, MarshalAs(UnmanagedType.LPWStr)]			string			pName,
		[Out]										out IBaseFilter		ppFilter );

		[PreserveSig]
	int ConnectDirect( [In] IPin ppinOut, [In] IPin ppinIn,
		[In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType	pmt );

		[PreserveSig]
	int Reconnect( [In] IPin ppin );

		[PreserveSig]
	int Disconnect( [In] IPin ppin );

		[PreserveSig]
	int SetDefaultSyncSource();

}



	[ComVisible(true), ComImport,
	Guid("36b73882-c2c8-11cf-8b46-00805f6cef60"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
	]
	public interface IFilterGraph2 //: IGraphBuilder
	{
		
		#region "IGraphBuilder Methods"
		#region "IFilterGraph Methods"
		[PreserveSig]
		int AddFilter(
			[In] IBaseFilter pFilter,
			[In, MarshalAs(UnmanagedType.LPWStr)]			string			pName );

		[PreserveSig]
		int RemoveFilter( [In] IBaseFilter pFilter );

		[PreserveSig]
		int EnumFilters( [Out] out IEnumFilters ppEnum );

		[PreserveSig]
		int FindFilterByName(
			[In, MarshalAs(UnmanagedType.LPWStr)]			string			pName,
			[Out]										out IBaseFilter		ppFilter );

		[PreserveSig]
		int ConnectDirect( [In] IPin ppinOut, [In] IPin ppinIn,
			[In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType	pmt );

		[PreserveSig]
		int Reconnect( [In] IPin ppin );

		[PreserveSig]
		int Disconnect( [In] IPin ppin );

		[PreserveSig]
		int SetDefaultSyncSource();
		#endregion

		[PreserveSig]
		int Connect( [In] IPin ppinOut, [In] IPin ppinIn );

		[PreserveSig]
		int Render( [In] IPin ppinOut );

		[PreserveSig]
		int RenderFile(
			[In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrFile,
			[In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrPlayList );

		[PreserveSig]
		int AddSourceFilter(
			[In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrFileName,
			[In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrFilterName,
			[Out]										out IBaseFilter		ppFilter );

		[PreserveSig]
		int SetLogFile(	IntPtr			lpcwstrFileName );

		[PreserveSig]
		int Abort();

		[PreserveSig]
		int ShouldOperationContinue();
		#endregion
//
		[PreserveSig]
		int AddSourceFilterForMoniker(IntPtr moniker, //IMoniker
									  IntPtr bindCtx, //IBindCtx
									  IntPtr	lpcwstrFilterName, // : LPCWSTR
									  [Out] IBaseFilter pFilter);

		[PreserveSig]
		int ReconnectEx(IPin ppin, AMMediaType mediaType);
		[PreserveSig]
		int RenderEx(IPin pPinOut, UInt32 dwFlags, [In, Out] ref IntPtr whatever);
	}
//--------------------------------------------------------------------------------



	[ComVisible(true), ComImport,
	Guid("5a804648-4f66-4867-9c43-4f5c822cf1b8"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRFilterConfig9 
	{
		[PreserveSig]
		int SetImageCompositor();

		[PreserveSig]
		int SetNumberOfStreams(int dwMaxStreams);

		[PreserveSig]
		int GetNumberOfStreams([Out] int dwMaxStreams);

		[PreserveSig]
		int SetRenderingPrefs(int pdwRenderFlags);

		[PreserveSig]
		int GetRenderingPrefs([Out] int pdwRenderFlags);

		[PreserveSig]
		int SetRenderingMode(int Mode);

		[PreserveSig]
		int GetRenderingMode([Out] int Mode);
   
	}
	public enum VMR9MixerPrefs
	{
		NoDecimation            = 0x00000001,
		DecimateOutput          = 0x00000002,
		ARAdjustXorY            = 0x00000004,
		NonSquareMixing         = 0x00000008,
		DecimateMask            = 0x0000000F,
		BiLinearFiltering       = 0x00000010,
		PointFiltering          = 0x00000020,
		AnisotropicFiltering    = 0x00000040,
		PyramidalQuadFiltering  = 0x00000080,
		GaussianQuadFiltering   = 0x00000100,
		FilteringReserved       = 0x00000E00,
		FilteringMask           = 0x00000FF0,
		RenderTargetRGB         = 0x00001000,
		RenderTargetYUV         = 0x00002000,
		RenderTargetReserved    = 0x000FC000,
		RenderTargetMask        = 0x000FF000,
		DynamicSwitchToBOB      = 0x00100000,
		DynamicDecimateBy2      = 0x00200000,
		DynamicReserved         = 0x00C00000,
		DynamicMask             = 0x00F00000
	} 
	public enum VMR9DeinterlacePrefs
	{
		DeinterlacePref9_NextBest = 0x01,
		DeinterlacePref9_BOB      = 0x02,
		DeinterlacePref9_Weave    = 0x04,
		DeinterlacePref9_Mask     = 0x07
	} 

	public enum VMR9DeinterlaceTech:uint
	{
		Unknown             = 0x0000,
		BOBLineReplicate    = 0x0001,
		BOBVerticalStretch  = 0x0002,
		MedianFiltering     = 0x0004,
		EdgeFiltering       = 0x0010,
		FieldAdaptive       = 0x0020,
		PixelAdaptive       = 0x0040,
		MotionVectorSteered = 0x0080
	} ;


	public enum VMR9AspectRatioMode  
	{
		VMR9ARMode_None,
		VMR9ARMode_LetterBox
	}

	public class VMR9 
	{
		public static readonly int VMRMode_Windowed = 1;
		public static readonly int VMRMode_Windowless = 2;
		public static readonly int VMRMode_Renderless = 4;
		public static readonly int VMRMode_Mask = 7;

		// surface types/usage
		public static readonly int VMR9AllocFlag_3DRenderTarget        = 0x0001;
		public static readonly int VMR9AllocFlag_DXVATarget            = 0x0002;

		//
		// VMR9AllocFlag_TextureSurface can be combined with
		// DXVATarget and 3DRenderTarget
		//
		public static readonly int VMR9AllocFlag_TextureSurface        = 0x0004;
		public static readonly int VMR9AllocFlag_OffscreenSurface      = 0x0008;
		public static readonly int VMR9AllocFlag_UsageReserved         = 0x00F0;
		public static readonly int VMR9AllocFlag_UsageMask             = 0x00FF;

	}

	public enum VMR9ProcAmpControlFlags:uint
	{
		ProcAmpControl9_Brightness            = 0x00000001,
		ProcAmpControl9_Contrast              = 0x00000002,
		ProcAmpControl9_Hue                   = 0x00000004,
		ProcAmpControl9_Saturation            = 0x00000008,
		ProcAmpControl9_Mask                  = 0x0000000F
	} ;

	public struct VMR9ProcAmpControl
	{
		public uint       dwSize;
		public uint       dwFlags;
		public float       Brightness;
		public float       Contrast;
		public float       Hue;
		public float       Saturation;
	} ;

	public struct VMR9ProcAmpControlRange
	{
		public uint                       dwSize;
		public VMR9ProcAmpControlFlags     dwProperty; // see VMR9ProcAmpControlFlags above
		public float                       MinValue;
		public float                       MaxValue;
		public float                       DefaultValue;
		public float                       StepSize;
	} ;
	public struct RGB 
	{
		public byte red;
		public byte green;
		public byte blu;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class VMR9AllocationInfo 
	{
		public Int32 dwFlags;
		public Int32 dwWidth;
		public Int32 dwHeight;
		public Format format;
		public Pool   pool;
		public Int32 MinBuffers;
		public System.Drawing.Size  szAspectRation;
		public System.Drawing.Size	 szNativeSize;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class VMR9PresentationInfo 
	{
		public Int32         dwFlags;
		public IntPtr			   lpSurf;
		public long					 rtStart;
		public long			     rtEnd;
		public Size          szAspectRatio;
		public Rectangle		 rcSrc;
		public Rectangle     rcDst;
		public Int32         dwReserved1;
		public Int32         dwReserved2;
	}

	
	[ComImport,
	Guid("8d5148ea-3f5d-46cf-9df1-d1b896eedb1f"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRSurfaceAllocator9
	{
		[PreserveSig]
		int InitializeDevice(Int32 dwUserId, VMR9AllocationInfo allocInfo, IntPtr numBuffers);
		[PreserveSig]
		int TerminateDevice(Int32 dwUserId);
		[PreserveSig]
		int GetSurface(Int32 dwUserId, Int32 surfaceIndex, Int32 SurfaceFlags, out IntPtr surface);
		[PreserveSig]
		int AdviseNotify(IVMRSurfaceAllocatorNotify9 allocNotify);
	}

	[ComImport,
	Guid("dca3f5df-bb3a-4d03-bd81-84614bfbfa0c"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRSurfaceAllocatorNotify9
	{
		[PreserveSig]
		int AdviseSurfaceAllocator(UInt32 dwUserID, IVMRSurfaceAllocator9 surfaceAllocator);

		[PreserveSig]
		int SetD3DDevice(IntPtr d3dDevice, IntPtr monitor);

		[PreserveSig]
		int ChangeD3DDevice(IntPtr d3dDevice, IntPtr monitor);

		[PreserveSig]
		int AllocateSurfaceHelper(VMR9AllocationInfo allocInfo, IntPtr numBuffers, out IntPtr surface);

		[PreserveSig]
		int NotifyEvent(UInt64 event_, UInt64 param1, UInt64 param2);

	}



	[ComVisible(true), ComImport,
	Guid("69188c61-12a3-40f0-8ffc-342e7b433fd7"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRImagePresenter9 
	{
		[PreserveSig]
		int StartPresenting(UInt32 uid);

		[PreserveSig]
		int StopPresenting(UInt32 uid);

		[PreserveSig]
		int PresentImage(UInt32 uid, VMR9PresentationInfo presInfo);
	}


	[ComVisible(true), ComImport,
	Guid("8f537d09-f85e-4414-b23b-502e54c79927"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRWindowlessControl9 
	{
		[PreserveSig]
		int GetNativeVideoSize ([Out] out Int32 lpWidth, [Out] out Int32 lpHeight, [Out] out Int32 lpARWidth, [Out] out Int32 lpARHeight);

		[PreserveSig]
		int GetMinIdealVideoSize ([Out] out Int32 lpWidth, [Out] out Int32 lpHeight);

		[PreserveSig]
		int GetMaxIdealVideoSize ([Out] out Int32 lpWidth, [Out] out Int32 lpHeight);

		[PreserveSig]
		int SetVideoPosition ( DsRECT lpSRCRect,  DsRECT lpDSTRect);
		[PreserveSig]
		int GetVideoPosition ([Out] out DsRECT lpSRCRect, [Out] out DsRECT lpDSTRect);

		[PreserveSig]
		int GetAspectRatioMode([Out] out VMR9AspectRatioMode mode);
		[PreserveSig]
		int SetAspectRatioMode(VMR9AspectRatioMode mode);


		[PreserveSig]
		int SetVideoClippingWindow (IntPtr HWND);
		[PreserveSig]
		int RepaintVideo (IntPtr HWND, IntPtr HDC);
		[PreserveSig]
		int DisplayModeChanged ();

		[PreserveSig]
		int GetCurrentImage([Out] out IntPtr pBmp);

		[PreserveSig]
		int SetBorderColor(RGB color);

		[PreserveSig]
		int GetBorderColor([Out] out  RGB color);

	}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class VMR9NormalizedRect 
{
		public float left;
		public float top;
		public float right;
		public float bottom;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class VMR9AlphaBitmap 
	{
		public UInt32 dwFlags;
		public IntPtr HDC;
		public IntPtr pDDS;  // not done yet!!
		public DsRECT rSrc;
		public VMR9NormalizedRect rDest;
		public float fAlpha;
		public RGB color;
		public UInt32 dwFilterMode;
	}

	[ComVisible(true), ComImport,
	Guid("1a777eaa-47c8-4930-b2c9-8fee1c1b0f3b"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRMixerControl9
	{
		[PreserveSig]
		int SetAlpha(int streamID, float alpha);
		[PreserveSig]
		int GetAlpha(int streamID, [Out] float alpha);
		[PreserveSig]
		int SetZOrder(int streamID, int order);
		[PreserveSig]
		int GetZOrder(int streamID, [Out] int alpha);

		[PreserveSig]
		int SetOutputRect(int streamID, VMR9NormalizedRect rect);
		[PreserveSig]
		int GetOutputRect(int streamID, [Out] VMR9NormalizedRect rect);

		[PreserveSig]
		int SetBackgroundColor(int streamID, RGB color);
		[PreserveSig]
		int GetBackgroundColor(int streamID,[Out] RGB color);
	
		[PreserveSig]
		int SetMixingPrefs([In] uint dwMixerPrefs  );

		[PreserveSig]
		int GetMixingPrefs([Out] out uint pdwMixerPrefs);

		[PreserveSig]
		int SetProcAmpControl([In] uint dwStreamID,[In] ref VMR9ProcAmpControl lpClrControl);

		[PreserveSig]
		int GetProcAmpControl([In] uint dwStreamID,
													[In, Out] ref VMR9ProcAmpControl lpClrControl);

		[PreserveSig]
		int GetProcAmpControlRange([In] uint dwStreamID,
															 [In, Out] ref VMR9ProcAmpControlRange lpClrControl);
	}

	[ComVisible(true), ComImport,
	Guid("ced175e5-1935-4820-81bd-ff6ad00c9108"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRMixerBitmap9 
	{
		[PreserveSig]
		int SetAlphaBitmap(VMR9AlphaBitmap bitmap);

		[PreserveSig]
		int UpdateAlphaBitmapParameters(VMR9AlphaBitmap bitmap);

		[PreserveSig]
		int GetAlphaBitmapParameters(VMR9AlphaBitmap bitmap);

	}



// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("0000010c-0000-0000-C000-000000000046"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IPersist
{
		[PreserveSig]
	int GetClassID(
		[Out]									out Guid		pClassID );
}



// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("BD1AE5E0-A6AE-11CE-BD37-504200C10000"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	interface IPersistMemory
	{
	#region "IPersist Methods"
		[PreserveSig]
		int GetClassID(
		[Out]									out Guid		pClassID );
	#endregion

		[PreserveSig]
		int IsDirty();

		[PreserveSig]
		int Load([In] IntPtr pMem, [In] uint cbSize);

		[PreserveSig]
		int Save([Out] IntPtr pMem, [In] bool fClearDirty, [In] uint cbSize);

		[PreserveSig]
		int GetSizeMax([Out] out uint pCbSize);

		[PreserveSig]
		int InitNew();
	}

// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("56a86899-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IMediaFilter
{
	#region "IPersist Methods"
			[PreserveSig]
		int GetClassID(
			[Out]									out Guid		pClassID );
	#endregion

		[PreserveSig]
	int Stop();

		[PreserveSig]
	int Pause();

		[PreserveSig]
	int Run( long tStart );

		[PreserveSig]
	int GetState( int dwMilliSecsTimeout, out int filtState );

		[PreserveSig]
	int SetSyncSource( [In] IReferenceClock pClock );

		[PreserveSig]
	int GetSyncSource( [Out] out IReferenceClock pClock );
}







// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("56a86895-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IBaseFilter
{
	#region "IPersist Methods"
			[PreserveSig]
		int GetClassID(
			[Out]									out Guid		pClassID );
	#endregion

	#region "IMediaFilter Methods"
			[PreserveSig]
		int Stop();

			[PreserveSig]
		int Pause();

			[PreserveSig]
		int Run( long tStart );

			[PreserveSig]
		int GetState( int dwMilliSecsTimeout, [Out] out int filtState );

			[PreserveSig]
		int SetSyncSource( [In] IReferenceClock pClock );

			[PreserveSig]
		int GetSyncSource( [Out] out IReferenceClock pClock );
	#endregion

		[PreserveSig]
	int EnumPins(
		[Out]										out IEnumPins		ppEnum );

		[PreserveSig]
	int FindPin(
		[In, MarshalAs(UnmanagedType.LPWStr)]			string			Id,
		[Out]										out IPin			ppPin );

		[PreserveSig]
	int QueryFilterInfo(
		[Out]											FilterInfo		pInfo );

		[PreserveSig]
	int JoinFilterGraph(
		[In]											IFilterGraph	pGraph,
		[In, MarshalAs(UnmanagedType.LPWStr)]			string			pName );

		[PreserveSig]
	int QueryVendorInfo(
		[Out, MarshalAs(UnmanagedType.LPWStr) ]		out	string			pVendorInfo );
}


	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), ComVisible(false)]
public class FilterInfo		//  FILTER_INFO
{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
	public string		achName;
		[MarshalAs(UnmanagedType.IUnknown)]
	public object		pUnk;
}









// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("36b73880-c2c8-11cf-8b46-00805f6cef60"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IMediaSeeking
{
		[PreserveSig]
	int GetCapabilities( out SeekingCapabilities pCapabilities );

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
	int GetDuration( out long pDuration );
		[PreserveSig]
	int GetStopPosition( out long pStop );
		//[PreserveSig]
	int GetCurrentPosition( out long pCurrent );

		[PreserveSig]
	int ConvertTimeFormat(	out long pTarget, [In] ref Guid pTargetFormat,
								long Source,  [In] ref Guid pSourceFormat );

		//[PreserveSig]
	int SetPositions(
//		[In, Out, MarshalAs(UnmanagedType.LPStruct)]		DsOptInt64	pCurrent,
		ref long pCurrent,
		int dwCurrentFlags,
//		[In, Out, MarshalAs(UnmanagedType.LPStruct)]		DsOptInt64	pStop,
		ref long pStop,
		SeekingFlags dwStopFlags );

		[PreserveSig]
	int GetPositions( out long pCurrent, out long pStop );

		[PreserveSig]
	int GetAvailable( out long pEarliest, out long pLatest );

		[PreserveSig]
	int SetRate( double dRate );
		[PreserveSig]
	int GetRate( out double pdRate );

		[PreserveSig]
	int GetPreroll( out long pllPreroll );
}


[ComVisible(true), ComImport,
Guid("436eee9c-264f-4242-90e1-4e330c107512"),
InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IMpeg2Demultiplexer 
{

	[PreserveSig]
	int CreateOutputPin([In] ref AMMediaType pMediaType, 
                      [In, MarshalAs(UnmanagedType.LPWStr)]			string pinName, 
                      [Out] out IPin outPin);

	[PreserveSig]
	int SetOutputPinMediaType(string pinName, ref AMMediaType pMediaType);

	[PreserveSig]
	int DeleteOutputPin(string pinName);

}


	public enum Mpeg2MediaSampleContent
	{
		Mpeg2Program_StreamMap           =      0x00000000,
		Mpeg2Program_ElementaryStream    =      0x00000001,
		Mpeg2Program_DirectoryPesPacket=       0x00000002,
		Mpeg2Program_PackHeader    =            0x00000003,
		Mpeg2Program_PesStream     =            0x00000004,
		Mpeg2Program_SystemHeader  =            0x00000005,
		SubStreamFilterValNone=            0x10000000
	};

	public struct StreamIdMap
	{
		public uint			stream_id ;                     //  mpeg-2 stream_id
		public uint			dwMediaSampleContent ;          //  #define'd above
		public uint			ulSubstreamFilterValue ;        //  filtering value
		public int				iDataOffset ;                   //  offset to elementary stream
	};

	[ComVisible(true), ComImport,
	Guid("945C1566-6202-46fc-96C7-D87F289C6534"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IEnumStreamIdMap
	{
		[PreserveSig]
		int Next(
			[In]															uint				cFilters,
			out StreamIdMap			x,
			[Out]															out uint pcFetched );


		[PreserveSig]
		int Skip( [In] int cFilters );
		void Reset();
		void Clone( [Out] out IEnumFilters ppEnum );
	}



	[ComVisible(true), ComImport,
	Guid("D0E04C47-25B8-4369-925A-362A01D95444"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IMPEG2StreamIdMap 
	{
//	[PreserveSig]
//	int CreateOutputPinMapStreamId(UInt32 ulStreamId, UInt32 MediaSampleContent, UInt32 ulSubstreamFilterValue, int iDataOffset);

	[PreserveSig]
	int MapStreamId(UInt32 ulStreamId, UInt32 MediaSampleContent, UInt32 ulSubstreamFilterValue, int iDataOffset);

	[PreserveSig]
	int UnmapStreamId(UInt32 culStreamId, ref UInt32 pulStreamId);

	[PreserveSig]
		int EnumStreamIdMap([Out] out IEnumStreamIdMap map);

	//function EnumStreamIdMap(out ppIEnumStreamIdMap: IEnumStreamIdMap): HResult; stdcall;

}
	public enum MediaSampleContent:uint
	{
		MEDIA_TRANSPORT_PACKET,
		MEDIA_ELEMENTARY_STREAM,
		MEDIA_MPEG2_PSI,
		MEDIA_TRANSPORT_PAYLOAD
	} 



	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), ComVisible(false)]	
	public struct PidMap 
	{
		public uint  ulPID ;
		public MediaSampleContent content;
	} ;

	[ComVisible(true), ComImport,
	Guid("afb6c2a2-2c41-11d3-8a60-0000f81e0e4a"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IEnumPIDMap
	{
		[PreserveSig]
		int Next(
			[In]			    uint				cFilters,
			[In,Out,MarshalAs(UnmanagedType.LPArray, ArraySubType= UnmanagedType.LPStruct)] ref PidMap[] maps,
			[Out]					out uint pcFetched );


		[PreserveSig]
		int Skip( [In] int cRecords );
		void Reset();
		void Clone( [Out] out IEnumPIDMap ppEnum );
	}


	[ComVisible(true), ComImport,
	Guid("afb6c2a1-2c41-11d3-8a60-0000f81e0e4a"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IMPEG2PIDMap 
	{
		[PreserveSig]
		int MapPID (  [In]    uint                   culPID,
									[In]    ref uint               pulPID,
									[In]    uint									 MediaSampleContent
									) ;

		[PreserveSig]
		int  UnmapPID ( [In]    uint   culPID, [In]    ref uint pulPID ) ;

		[PreserveSig]
		int EnumPIDMap ( [Out]  out IEnumPIDMap  pIEnumPIDMap);
	} ;


	[ComVisible(true), ComImport,
	Guid("9ce50f2d-6ba7-40fb-a034-50b1a674ec78"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IStreamBufferInitialize 
	{

		[PreserveSig]
		int SetHKEY(IntPtr hkeyRoot);

		[PreserveSig]
		int SetSIDs(UInt32 cSIDs, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType= 
										   UnmanagedType.LPStruct , SizeParamIndex=1)]  IntPtr[] PSid);
	
	}

	public enum  BitRateMode 
	{
		RC_VIDEOENCODINGMODE_CBR = 0, 
		RC_VIDEOENCODINGMODE_VBR = 1
	};

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct VideoBitRate 		//  VideoBitRate 
	{
		public BitRateMode bEncodingMode;
		public UInt16	   wBitRate;
		public UInt16	   dwPeak;
	}

	[ComVisible(true), ComImport,
	Guid("D2185A40-0398-11D3-A53E-00A0C9EF506A"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IAMVacCtrlProp 
	{

		[PreserveSig]
		int get_OutputType(out int outputType);
		[PreserveSig]
		int put_OutputType(int outputType);

		[PreserveSig]
		int get_Bitrate(out VideoBitRate bitRate);
		[PreserveSig]
		int put_Bitrate(IntPtr bitRate);

		[PreserveSig]
		int get_VideoResolution(out int samplingrate);
		[PreserveSig]
		int put_VideoResolution(int samplingrate);

		[PreserveSig]
		int get_AudioDataRate(out int samplingrate);
		[PreserveSig]
		int put_AudioDataRate(int samplingrate);

		[PreserveSig]
		int get_GOPSize(out int samplingrate);
		[PreserveSig]
		int put_GOPSize(int samplingrate);

		[PreserveSig]
		int get_ClosedGop(out int samplingrate);
		[PreserveSig]
		int put_ClosedGop(int samplingrate);

		[PreserveSig]
		int get_AudioSamplingRate(out int samplingrate);
		[PreserveSig]
		int put_AudioSamplingRate(int samplingrate);

		[PreserveSig]
		int get_AudioOutputMode(out int samplingrate);
		[PreserveSig]
		int put_AudioOutputMode(int samplingrate);

		[PreserveSig]
		int get_AudioCRC(out int samplingrate);
		[PreserveSig]
		int put_AudioCRC(int samplingrate);

		[PreserveSig]
		int get_PreFilterSettings(out int samplingrate);
		[PreserveSig]
		int put_PreFilterSettings(int samplingrate);

		[PreserveSig]
		int get_InverseTelecine(out int samplingrate);
		[PreserveSig]
		int put_InverseTelecine(int samplingrate);

		[PreserveSig]
		int put_CopyProtection(int samplingrate);
		[PreserveSig]
		int get_CaptureStatus(out int samplingrate);
		[PreserveSig]
		int get_BoardDescription(out int samplingrate);
		[PreserveSig]
		int get_VersionInfo(out int samplingrate);

		[PreserveSig]
		int reset_Msp4448G();

		[PreserveSig]
		int get_Msp4448G(uint addr, out uint value);
		[PreserveSig]
		int put_Msp4448G(uint addr, uint value);


	}



								  
	[Flags, ComVisible(false)]
public enum SeekingCapabilities	:uint	// AM_SEEKING_SeekingCapabilities AM_SEEKING_SEEKING_CAPABILITIES
{
	CanSeekAbsolute		= 0x001,
	CanSeekForwards		= 0x002,
	CanSeekBackwards	= 0x004,
	CanGetCurrentPos	= 0x008,
	CanGetStopPos		= 0x010,
	CanGetDuration		= 0x020,
	CanPlayBackwards	= 0x040,
	CanDoSegments		= 0x080,
	Source				= 0x100		// Doesn't pass thru used to count segment ends
}


	[Flags, ComVisible(false)]
public enum SeekingFlags	:int	// AM_SEEKING_SeekingFlags AM_SEEKING_SEEKING_FLAGS
{
	NoPositioning			= 0x00,		// No change
	AbsolutePositioning		= 0x01,		// Position is supplied and is absolute
	RelativePositioning		= 0x02,		// Position is supplied and is relative
	IncrementalPositioning	= 0x03,		// (Stop) position relative to current, useful for seeking when paused (use +1)
	PositioningBitsMask		= 0x03,		// Useful mask
	SeekToKeyFrame			= 0x04,		// Just seek to key frame (performance gain)
	ReturnTime				= 0x08,		// Plug the media time equivalents back into the supplied LONGLONGs
	Segment					= 0x10,		// At end just do EC_ENDOFSEGMENT, don't do EndOfStream
	NoFlush					= 0x20,		// Don't flush
	SeekAbsolutePositionKey=0x05 // absolute position + keyframe
}





// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("56a86897-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IReferenceClock
{
		[PreserveSig]
	int GetTime( out long pTime );

		[PreserveSig]
	int AdviseTime( long baseTime, long streamTime, IntPtr hEvent, out int pdwAdviseCookie);

		[PreserveSig]
	int AdvisePeriodic( long startTime, long periodTime, IntPtr hSemaphore, out int pdwAdviseCookie);

		[PreserveSig]
	int Unadvise( int dwAdviseCookie );
}







// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("89c31040-846b-11ce-97d3-00aa0055595a"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IEnumMediaTypes
	{
		[PreserveSig]
		int Next(
			[In]						uint				cFilters,
			out AMMediaTypeClass	x,
			[Out]						out uint pcFetched );


		[PreserveSig]
		int Skip( [In] int cFilters );
		void Reset();
		void Clone( [Out] out IEnumMediaTypes ppEnum );
	}


	[ComVisible(true), ComImport,
	Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IEnumFilters
{
    /*
		[PreserveSig]
	int Next(
		[In]															int				cFilters,
		[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)]	out	IBaseFilter[]	ppFilter,
		[Out]														out uint				pcFetched );
*/
    [PreserveSig]
    int Next(
      [In]															uint				cFilters,
      out IBaseFilter			x,
      [Out]															out uint pcFetched );


		[PreserveSig]
	int Skip( [In] int cFilters );
	void Reset();
	void Clone( [Out] out IEnumFilters ppEnum );
}


// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IEnumPins
{
		//[PreserveSig]
	//int Next(
//			int cPins, [MarshalAs(UnmanagedType.LPArray)] IntPtr[]
	//		ppPins, out int pcFetched
		//	);
    [PreserveSig]
    int Next(
      [In]														int			cPins,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]	IPin[]		ppPins,
      [Out]														out int		pcFetched );

		[PreserveSig]
	int Skip( [In] int cPins );
	void Reset();
	void Clone( [Out] out IEnumPins ppEnum );
}



  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public class AMMediaTypeClass		//  AM_MEDIA_TYPE
  {
    public Guid		majorType;
    public Guid		subType;
    [MarshalAs(UnmanagedType.Bool)]
    public bool		fixedSizeSamples;
    [MarshalAs(UnmanagedType.Bool)]
    public bool		temporalCompression;
    public int		sampleSize;
    public Guid		formatType;
    public IntPtr	unkPtr;
    public int		formatSize;
    public IntPtr	formatPtr;
  }


[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct AMMediaType		//  AM_MEDIA_TYPE
{
	public Guid		majorType;
	public Guid		subType;
		[MarshalAs(UnmanagedType.Bool)]
	public bool		fixedSizeSamples;
		[MarshalAs(UnmanagedType.Bool)]
	public bool		temporalCompression;
	public int		sampleSize;
	public Guid		formatType;
	public IntPtr	unkPtr;
	public int		formatSize;
	public IntPtr	formatPtr;
}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct Rect		//  Rect
{
	public int left;
	public int top;
	public int right;
	public int bottom;
}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct BitmapInfoHeader		//  Rect
{
  public UInt32 biSize;             // 4    4 added
	public int    biWidth;            // 4    8     
	public int    biHeight;           // 4    12
	public UInt16 biPlanes;           // 2    14
	public UInt16 biBitCount;         // 2    16
	public UInt32 biCompression;      // 4    20
	public UInt32 biSizeImage;        // 4    24
	public int    biXPelsPerMeter;    // 4    28
	public int    biYPelsPerMeter;    // 4    32
	public UInt32 biClrUsed;          // 4    36
	public UInt32 biClrImportant;     // 4    40
}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct VideoInfoHeader2		//  VideoInfoHeader2
{
	public Rect		  rcsource;
	public Rect		  rctarget;
	public UInt32		dwBitRate;
	public UInt32		dwBitErrorRate;
	public Int64		AvgTimePerFrame;
	public UInt32		dwInterlaceFlags;
	public UInt32		dwCopyProtectFlags;
	public UInt32		dwPictAspectRatioX;
	public UInt32		dwPictAspectRatioY;
  public UInt32		dwControlFlags;       //added
  public UInt32		dwReserved2;          //added    
	public BitmapInfoHeader bmpInfoHdr;
}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct MPEG2VideoInfo		//  MPEG2VideoInfo
{
	public VideoInfoHeader2 hdr;
	public UInt32		dwStartTimeCode;
  public UInt32   cbSequenceHeader;
	public UInt32		dwProfile;
	public UInt32		dwLevel;
	public UInt32		dwFlags;
	public UInt32		dwSequenceHeader;

}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct ADPCMCoef 		//  WaveFormatEx 
{
	public Int16 coef1;
	public Int16 coef2;
}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct WaveFormatEx 		//  WaveFormatEx 
{
	public UInt16	wFormatTag;
	public UInt16	nChannels;
	public int		nSamplesPerSec;
	public int		nAvgBytesPerSec;
	public UInt16	nBlockAlign;
	public UInt16	wBitsPerSample;
	public UInt16	cbSize;
	public Int16	wNumCoef;
	public ADPCMCoef[] CoefSet;

}

[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct Mpeg1WaveFormat 		//  Mpeg1WaveFormat 
{
	public WaveFormatEx wfx;
	public UInt16 fwHeadLayer;
	public UInt32 dwHeadBitrate;
	public UInt16 fwHeadMode;
	public UInt16 fwHeadModeExt;
	public UInt16 wHeadEmphasis;
	public UInt16 fwHeadFlags;
	public UInt32 dwPTSLow;
	public UInt32 dwPTSHigh;

}





// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IMediaSample
{
		[PreserveSig]
	int GetPointer( out IntPtr ppBuffer );
		[PreserveSig]
	int GetSize();

		[PreserveSig]
	int GetTime( out long pTimeStart, out long pTimeEnd );

		[PreserveSig]
	int SetTime(
		[In, MarshalAs(UnmanagedType.LPStruct)]			DsOptInt64	pTimeStart,
		[In, MarshalAs(UnmanagedType.LPStruct)]			DsOptInt64	pTimeEnd );

		[PreserveSig]
	int IsSyncPoint();
		[PreserveSig]
	int SetSyncPoint(
		[In, MarshalAs(UnmanagedType.Bool)]			bool	bIsSyncPoint );

		[PreserveSig]
	int IsPreroll();
		[PreserveSig]
	int SetPreroll(
		[In, MarshalAs(UnmanagedType.Bool)]			bool	bIsPreroll );

		[PreserveSig]
	int GetActualDataLength();
		[PreserveSig]
	int SetActualDataLength( int len );

		[PreserveSig]
	int GetMediaType(
		[Out, MarshalAs(UnmanagedType.LPStruct)]	out AMMediaType	ppMediaType );

		[PreserveSig]
	int SetMediaType(
		[In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType	pMediaType );

		[PreserveSig]
	int IsDiscontinuity();
		[PreserveSig]
	int SetDiscontinuity(
		[In, MarshalAs(UnmanagedType.Bool)]			bool	bDiscontinuity );

		[PreserveSig]
	int GetMediaTime( out long pTimeStart, out long pTimeEnd );

		[PreserveSig]
	int SetMediaTime(
		[In, MarshalAs(UnmanagedType.LPStruct)]			DsOptInt64	pTimeStart,
		[In, MarshalAs(UnmanagedType.LPStruct)]			DsOptInt64	pTimeEnd );
}

  [ComVisible(true), ComImport,
  Guid("593CDDE1-0759-11d1-9E69-00C04FD7C15B"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IMixerPinConfig
  {
    [PreserveSig]
    int SetRelativePosition( 
      uint dwLeft,
      uint dwTop,
      uint dwRight,
      uint dwBottom
      ) ;

    [PreserveSig]
    int GetRelativePosition( 
      out uint pdwLeft,
      out uint pdwTop,
      out uint pdwRight,
      out uint pdwBottom
      ) ;

    [PreserveSig]
    int SetZOrder( uint dwZOrder) ;

    [PreserveSig]
    int GetZOrder( out uint pdwZOrder) ;

    [PreserveSig]
    int SetColorKey( ref uint pColorKey) ;

    [PreserveSig]
    int GetColorKey( out uint pColorKey,out uint pColor) ;

    [PreserveSig]
    int SetBlendingParameter( uint dwBlendingParameter) ;

    [PreserveSig]
    int GetBlendingParameter( out uint pdwBlendingParameter) ;

    [PreserveSig]
    int SetAspectRatioMode( AmAspectRatioMode amAspectRatioMode) ;

    [PreserveSig]
    int GetAspectRatioMode( out AmAspectRatioMode pamAspectRatioMode) ;

    [PreserveSig]
    int SetStreamTransparent(  bool bStreamTransparent) ;

    [PreserveSig]
    int GetStreamTransparent( out bool pbStreamTransparent ) ;
  };

  [ComVisible(true), ComImport,
  Guid("00d96c29-bbde-4efc-9901-bb5036392146"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IVMRAspectRatioControl9
  {
    [PreserveSig]
    int GetAspectRatioMode(out uint lpdwARMode);

    int SetAspectRatioMode(uint dwARMode);
  };


  [ComVisible(true), ComImport,
  Guid("ede80b5c-bad6-4623-b537-65586c9f8dfd"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IVMRAspectRatioControl
  {
    [PreserveSig]
    int GetAspectRatioMode(out uint lpdwARMode);

    [PreserveSig]
    int SetAspectRatioMode(uint dwARMode);
  }


	enum CompressionCaps
		{
			CompressionCaps_CanQuality =  0x01,
			CompressionCaps_CanCrunch =   0x02,
			CompressionCaps_CanKeyFrame = 0x04,
			CompressionCaps_CanBFrame =   0x08,
			CompressionCaps_CanWindow =   0x10
		} ;
	[ComVisible(true), ComImport,
	Guid("C6E13343-30AC-11d0-A18C-00A0C9118956"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IAMVideoCompression 
	{
		[PreserveSig]
		int put_KeyFrameRate ([In] long KeyFrameRate);
		[PreserveSig]
		int get_KeyFrameRate ([Out] out long  pKeyFrameRate);

		[PreserveSig]
		int put_PFramesPerKeyFrame ([In] long PFramesPerKeyFrame);
		[PreserveSig]
		int get_PFramesPerKeyFrame ([Out] out long  pPFramesPerKeyFrame);
		
		[PreserveSig]
		int put_Quality ([In] double Quality);
		[PreserveSig]
		int get_Quality ([Out] out double  pQuality);

		[PreserveSig]
		int put_WindowSize ([In] ulong WindowSize);
		[PreserveSig]
		int get_WindowSize ([Out] out ulong pWindowSize);

		[PreserveSig]
		int GetInfo(
				[Out, MarshalAs(UnmanagedType.LPWStr)] out string pszVersion,
				[In,Out] ref int pcbVersion,
				[Out, MarshalAs(UnmanagedType.LPWStr)] out string pszDescription,
				[In,Out] ref int pcbDescription,
				[Out] out long pDefaultKeyFrameRate,
				[Out] out long pDefaultPFramesPerKey,
				[Out] out double pDefaultQuality,
				[Out] out long pCapabilities  //CompressionCaps
				);

		[PreserveSig]
		int OverrideKeyFrame([In]  long FrameNumber);
		[PreserveSig]
		int OverrideFrameSize([In]  long FrameNumber,[In]  long Size);

	}


	[ComVisible(true), ComImport,
	Guid("1BD0ECB0-F8E2-11CE-AAC6-0020AF0B99A3"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IQualProp
	{
		[PreserveSig]
		int get_FramesDroppedInRenderer( out int pcFrames) ;  // Out
		[PreserveSig]
		int get_FramesDrawn( out int pcFramesDrawn) ;         // Out
		[PreserveSig]
		int get_AvgFrameRate( out int piAvgFrameRate) ;       // Out
		[PreserveSig]
		int get_Jitter( out int iJitter) ;                    // Out
		[PreserveSig]
		int get_AvgSyncOffset( out int piAvg) ;               // Out
		[PreserveSig]
		int get_DevSyncOffset( out int piDev) ;               // Out
	}

	public enum VMRAlphaBitmapFlags
	{
		Disable=1,
		HDC=2,
		EntireDDS=4,
		SrcColorKey=8,
		SrcRect=16

	}
	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class NormalizedRect 
	{
		public float left;
		public float top;
		public float right;
		public float bottom;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class VMRAlphaBitmap 
	{
		public UInt32 dwFlags;
		public IntPtr HDC;
		public IntPtr pDDS;  // not done yet!!
		public DsRECT rSrc;
		public NormalizedRect rDest;
		public float fAlpha;
		public RGB color;
	}

	[ComVisible(true), ComImport,
	Guid("1E673275-0257-40aa-AF20-7C608D4A0428"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRMixerBitmap 
	{
		// Set bitmap, location to blend it, and blending value
		[PreserveSig]
		int SetAlphaBitmap( VMRAlphaBitmap bitmap);

		// Change bitmap location, size and blending value,
		// graph must be running for change to take effect.
		[PreserveSig]
		int UpdateAlphaBitmapParameters( VMRAlphaBitmap bitmap);

		// Get bitmap, location to blend it, and blending value
		[PreserveSig]
		int GetAlphaBitmapParameters(VMRAlphaBitmap bitmap);
	};
	[Flags]
	public enum AMStreamSelectInfoFlags
	{
		Enabled = 0x01,
		Exclusive = 0x02
	}

	/// <summary>
	/// From _AMSTREAMSELECTENABLEFLAGS
	/// </summary>
	[Flags]
	public enum AMStreamSelectEnableFlags
	{
		Enable = 0x01,
		EnableAll = 0x02
	}
	[Guid("c1960960-17f5-11d1-abe1-00a0c905f375"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAMStreamSelect
	{
		[PreserveSig]
		int Count([Out] out int pcStreams);

		[PreserveSig]
		int Info(
			[In] int lIndex,
			[Out] out AMMediaType ppmt,
			[Out] out AMStreamSelectInfoFlags pdwFlags,
			[Out] out int plcid,
			[Out] out int pdwGroup,
			[Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszName,
			[Out, MarshalAs(UnmanagedType.IUnknown)] out object ppObject,
			[Out, MarshalAs(UnmanagedType.IUnknown)] out object ppUnk
			);

		[PreserveSig]
		int Enable(
			[In] int lIndex,
			[In] AMStreamSelectEnableFlags dwFlags
			);
	} 
} // namespace DShowNET