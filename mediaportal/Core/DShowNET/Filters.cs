// ------------------------------------------------------------------
// DirectX.Capture
//
// History:
//	2003-Jan-24		BL		- created
//
// Copyright (c) 2003 Brian Low
// ------------------------------------------------------------------

using System;
using DShowNET;

namespace DShowNET
{
	/// <summary>
	///  Provides collections of devices and compression codecs
	///  installed on the system. 
	/// </summary>
	/// <example>
	///  Devices and compression codecs are implemented in DirectShow 
	///  as filters, see the <see cref="Filter"/> class for more 
	///  information. To list the available video devices:
	///  <code><div style="background-color:whitesmoke;">
	///   Filters filters = new Filters();
	///   foreach ( Filter f in filters.VideoInputDevices )
	///   {
	///		Debug.WriteLine( f.Name );
	///   }
	///  </div></code>
	///  <seealso cref="Filter"/>
	/// </example>
	public class Filters
	{
		// ------------------ Public Properties --------------------

		/// <summary> Collection of available video capture devices. </summary>
		public FilterCollection VideoInputDevices = new FilterCollection( FilterCategory.VideoInputDevice ); 

		/// <summary> Collection of available audio capture devices. </summary>
		public FilterCollection AudioInputDevices = new FilterCollection( FilterCategory.AudioInputDevice ); 

		/// <summary> Collection of available video compressors. </summary>
		public FilterCollection VideoCompressors = new FilterCollection( FilterCategory.VideoCompressorCategory ); 

		/// <summary> Collection of available audio compressors. </summary>
		public FilterCollection AudioCompressors = new FilterCollection( FilterCategory.AudioCompressorCategory ); 

    public FilterCollection LegacyFilters = new FilterCollection( FilterCategory.LegacyAmFilterCategory ); 
		public FilterCollection AudioRenderers = new FilterCollection( FilterCategory.AudioRendererDevice); 
    public FilterCollection WDMEncoders = new FilterCollection( FilterCategory.AM_KSEncoder); 
    public FilterCollection WDMcrossbars = new FilterCollection( FilterCategory.AM_KSCrossBar); 
    public FilterCollection WDMTVTuners = new FilterCollection( FilterCategory.AM_KSTvTuner); 
		public FilterCollection BDAReceivers = new FilterCollection( FilterCategory.AM_KS_BDA_RECEIVER_COMPONENT); 
		/// <summary>
		/// #MW#
		/// </summary>
		public FilterCollection AllFilters = new FilterCollection(FilterCategory.ActiveMovieCategory);

	}
}
