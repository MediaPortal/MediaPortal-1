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
using System;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// The TsWriter knc ci interface
  /// </summary>
  [ComVisible(true), ComImport,
Guid("C71E2EFA-2439-4dbe-A1F7-935ADC37A4EC"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IKNC
  {
    /// <summary>
    /// Sets the tuner filter
    /// </summary>
    /// <param name="tunerFilter">The tuner filter</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTunerFilter(IBaseFilter tunerFilter);
    /// <summary>
    /// Checks, if the tuner is a KNC
    /// </summary>
    /// <param name="yesNo">Result: true, when KNC</param>
    /// <returns></returns>
    [PreserveSig]
    int IsKNC(ref bool yesNo);
    /// <summary>
    /// Checks, if the cam is ready
    /// </summary>
    /// <param name="yesNo">Result: true, when the cam is ready</param>
    /// <returns></returns>
    [PreserveSig]
    int IsCamReady(ref bool yesNo);
    /// <summary>
    /// Checks, if the ci is available
    /// </summary>
    /// <param name="yesNo">Result: true, when the ci is available</param>
    /// <returns></returns>
    [PreserveSig]
    int IsCIAvailable(ref bool yesNo);
    /// <summary>
    /// Sets the diseqc data
    /// </summary>
    /// <param name="diseqcType">DiseqC type</param>
    /// <param name="hiband">hiband</param>
    /// <param name="vertical">vertical</param>
    /// <returns></returns>
    [PreserveSig]
    int SetDisEqc(short diseqcType, short hiband, short vertical);
    /// <summary>
    /// Descramble a service
    /// </summary>
    /// <param name="pmt">The PMT</param>
    /// <param name="pmtLen">The length of the pmt</param>
    /// <param name="succeeded">Result: true, if succeeded</param>
    /// <returns></returns>
    [PreserveSig]
    int DescrambleService(IntPtr pmt, short pmtLen, ref bool succeeded);
    /// <summary>
    /// Descrambles multiple services
    /// </summary>
    /// <param name="serviceIds">The service ids</param>
    /// <param name="nrOfServiceIds">Length of the service ids</param>
    /// <param name="succeeded">Result: true, if succeeded</param>
    /// <returns></returns>
    [PreserveSig]
    int DescrambleMultiple(IntPtr serviceIds, short nrOfServiceIds, ref bool succeeded);
  };
}
