// ------------------------------------------------------------------
// DirectX.Capture
//
// History:
//	2003-Jan-24		BL		- created
//
// Copyright (c) 2003 Brian Low
// ------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using DShowNET;

namespace DirectX.Capture
{
	/// <summary>
	///  Capabilities of the video device such as 
	///  min/max frame size and frame rate.
	/// </summary>
	public class VideoCapabilities
	{

		// ------------------ Properties --------------------

		/// <summary> Native size of the incoming video signal. This is the largest signal the filter can digitize with every pixel remaining unique. Read-only. </summary>
		public Size InputSize;

		/// <summary> Minimum supported frame size. Read-only. </summary>
		public Size MinFrameSize;

		/// <summary> Maximum supported frame size. Read-only. </summary>
		public Size MaxFrameSize;

		/// <summary> Granularity of the output width. This value specifies the increments that are valid between MinFrameSize and MaxFrameSize. Read-only. </summary>
		public int FrameSizeGranularityX;

		/// <summary> Granularity of the output height. This value specifies the increments that are valid between MinFrameSize and MaxFrameSize. Read-only. </summary>
		public int FrameSizeGranularityY;

		/// <summary> Minimum supported frame rate. Read-only. </summary>
		public double MinFrameRate;

		/// <summary> Maximum supported frame rate. Read-only. </summary>
		public double MaxFrameRate;



		// ----------------- Constructor ---------------------

		/// <summary> Retrieve capabilities of a video device </summary>
		internal VideoCapabilities(IAMStreamConfig videoStreamConfig)
		{
			if ( videoStreamConfig == null ) 
				throw new ArgumentNullException( "videoStreamConfig" );

			AMMediaType mediaType ;
			VideoStreamConfigCaps caps = null;
			IntPtr pCaps = IntPtr.Zero;
			try
			{
				// Ensure this device reports capabilities
				int c, size;
				int hr = videoStreamConfig.GetNumberOfCapabilities( out c, out size );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				if ( c <= 0 ) 
					throw new NotSupportedException( "This video device does not report capabilities." );
				if ( size > Marshal.SizeOf( typeof( VideoStreamConfigCaps ) ) )
					throw new NotSupportedException( "Unable to retrieve video device capabilities. This video device requires a larger VideoStreamConfigCaps structure." );
				if ( c > 1 )
					Debug.WriteLine("This video device supports " + c + " capability structures. Only the first structure will be used." );

				// Alloc memory for structure
				pCaps = Marshal.AllocCoTaskMem( Marshal.SizeOf( typeof( VideoStreamConfigCaps ) ) ); 

				// Retrieve first (and hopefully only) capabilities struct
				hr = videoStreamConfig.GetStreamCaps( 0, out mediaType, pCaps );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );

				// Convert pointers to managed structures
				caps = (VideoStreamConfigCaps) Marshal.PtrToStructure( pCaps, typeof( VideoStreamConfigCaps ) );

				// Extract info
				InputSize = caps.InputSize;
				MinFrameSize = caps.MinOutputSize;
				MaxFrameSize = caps.MaxOutputSize;
				FrameSizeGranularityX = caps.OutputGranularityX;
				FrameSizeGranularityY = caps.OutputGranularityY;
				MinFrameRate = (double)10000000 / caps.MaxFrameInterval;
				MaxFrameRate = (double)10000000 / caps.MinFrameInterval;
			}
			finally
			{
				if ( pCaps != IntPtr.Zero )
					Marshal.FreeCoTaskMem( pCaps ); pCaps = IntPtr.Zero;
			}
		}
	}
}
