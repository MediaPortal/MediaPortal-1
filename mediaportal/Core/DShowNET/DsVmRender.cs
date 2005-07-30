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
	[ComVisible(false)]
	public enum VMRMode : uint
	{
		Windowed                         = 0x00000001,
		Windowless                       = 0x00000002,
		Renderless                       = 0x00000004,
	}
  [ComVisible(false)]
  public enum VMRRenderPrefs : uint
  {
    ForceOffscreen               = 0x00000001,
    ForceOverlays                = 0x00000002,
    AllowOverlays                = 0x00000000,
    AllowOffscreen               = 0x00000000,
    DoNotRenderColorKeyAndBorder = 0x00000008,
    RestrictToInitialMonitor     = 0x00000010,
    PreferAGPMemWhenMixing       = 0x00000020,
    Mask                         = 0x0000003f

  }

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;
	}

	[ComVisible(true), ComImport,
	Guid("0eb1088c-4dcd-46f0-878f-39dae86a51b7"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRWindowlessControl
	{
		//
		//////////////////////////////////////////////////////////
		// Video size and position information
		//////////////////////////////////////////////////////////
		//
		int GetNativeVideoSize(
			[Out] out int		lpWidth,
			[Out] out int		lpHeight,
			[Out] out int		lpARWidth,
			[Out] out int		lpARHeight
		);

		int GetMinIdealVideoSize(
			[Out] out int		lpHeight
		);

		int GetMaxIdealVideoSize(
			[Out] out int		lpWidth,
			[Out] out int		lpHeight
		);
    int SetVideoPosition(ref RECT lpSRCRect, ref RECT lpDSTRect);
/*
		int SetVideoPosition(
			[In, MarshalAs(UnmanagedType.LPStruct)] RECT lpSRCRect,
			[In, MarshalAs(UnmanagedType.LPStruct)] RECT lpDSTRect
		);
*/
		int GetVideoPosition(
			[Out, MarshalAs(UnmanagedType.LPStruct)] out RECT lpSRCRect,
			[Out, MarshalAs(UnmanagedType.LPStruct)] out RECT lpDSTRect
		);

		int GetAspectRatioMode( [Out] out uint lpAspectRatioMode );

		int SetAspectRatioMode( [In] uint AspectRatioMode );

		//
		//////////////////////////////////////////////////////////
		// Display and clipping management
		//////////////////////////////////////////////////////////
		//
		int SetVideoClippingWindow( [In] IntPtr	hwnd );

		int RepaintVideo(
			[In] IntPtr			hwnd,
			[In] IntPtr			hdc
		);

		int DisplayModeChanged();


		//
		//////////////////////////////////////////////////////////
		// GetCurrentImage
		//
		// Returns the current image being displayed.  This images
		// is returned in the form of packed Windows DIB.
		//
		// GetCurrentImage can be called at any time, also
		// the caller is responsible for free the returned memory
		// by calling CoTaskMemFree.
		//
		// Excessive use of this function will degrade Video
		// playback performed.
		//////////////////////////////////////////////////////////
		//
		int GetCurrentImage( [Out] out IntPtr lpDib );

		//
		//////////////////////////////////////////////////////////
		// Border Color control
		//
		// The border color is color used to fill any area of the
		// the destination rectangle that does not contain Video.
		// It is typically used in two instances.  When the Video
		// straddles two monitors and when the VMR is trying
		// to maintain the aspect ratio of the movies by letter
		// boxing the Video to fit within the specified destination
		// rectangle. See SetAspectRatioMode above.
		//////////////////////////////////////////////////////////
		//
		int SetBorderColor( [In] uint Clr );

		int GetBorderColor( [Out] out uint lpClr );

		//
		//////////////////////////////////////////////////////////
		// Color key control only meaningful when the VMR is using
		// and overlay
		//////////////////////////////////////////////////////////
		//
		int SetColorKey( [In] uint Clr );

		int GetColorKey( [Out] out uint lpClr );

	}

	[ComVisible(true), ComImport,
	Guid("9e5530c5-7034-48b4-bb46-0b8a6efc8e36"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMRFilterConfig
	{
		[PreserveSig]
		int SetImageCompositor( [In] IntPtr lpVMRImgCompositor );

		[PreserveSig]
		int SetNumberOfStreams( [In] uint dwMaxStreams );

		[PreserveSig]
		int GetNumberOfStreams( [Out] out uint pdwMaxStreams );

		[PreserveSig]
		int SetRenderingPrefs( [In] uint dwRenderFlags );

		[PreserveSig]
		int GetRenderingPrefs( [Out] out uint pdwRenderFlags );

		[PreserveSig]
		int SetRenderingMode( [In] uint Mode );

		[PreserveSig]
		int GetRenderingMode( [Out] out VMRMode Mode );
	}

}