#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace DShowNET.Helper
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
    public static FilterCollection VideoInputDevices;

    /// <summary> Collection of available audio capture devices. </summary>
    public static FilterCollection AudioInputDevices;

    /// <summary> Collection of available video compressors. </summary>
    public static FilterCollection VideoCompressors;

    /// <summary> Collection of available audio compressors. </summary>
    public static FilterCollection AudioCompressors;

    public static FilterCollection LegacyFilters;
    public static FilterCollection AudioRenderers;
    public static FilterCollection WDMEncoders;
    public static FilterCollection WDMcrossbars;
    public static FilterCollection WDMTVTuners;
    public static FilterCollection BDAReceivers;
    public static FilterCollection AllFilters;

    static Filters()
    {
      VideoInputDevices = new FilterCollection(FilterCategory.VideoInputDevice, true);
      AudioInputDevices = new FilterCollection(FilterCategory.AudioInputDevice, true);
      VideoCompressors = new FilterCollection(FilterCategory.VideoCompressorCategory, true);
      AudioCompressors = new FilterCollection(FilterCategory.AudioCompressorCategory, true);
      LegacyFilters = new FilterCollection(FilterCategory.LegacyAmFilterCategory, true);
      AudioRenderers = new FilterCollection(FilterCategory.AudioRendererDevice, true);
      WDMEncoders = new FilterCollection(FilterCategory.AM_KSEncoder, true);
      WDMcrossbars = new FilterCollection(FilterCategory.AM_KSCrossBar, true);
      WDMTVTuners = new FilterCollection(FilterCategory.AM_KSTvTuner, true);
      BDAReceivers = new FilterCollection(FilterCategory.AM_KS_BDA_RECEIVER_COMPONENT, true);
      AllFilters = new FilterCollection(FilterCategory.ActiveMovieCategory, true);
    }
  }
}