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
  /// The TsWriter technotred ci interface
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("B0AB5587-DCEC-49f4-B1AA-06EF58DBF1D3"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITechnoTrend
  {
    /// <summary>
    /// Sets the tuner filter
    /// </summary>
    /// <param name="tunerFilter">The tuner filter</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTunerFilter(IBaseFilter tunerFilter);
    /// <summary>
    /// Checks, if the tuner is a technotrend
    /// </summary>
    /// <param name="yesNo">Result: true, when Technotrend</param>
    /// <returns></returns>
    [PreserveSig]
    int IsTechnoTrend(ref bool yesNo);
    /// <summary>
    /// Checks, if a cam is present
    /// </summary>
    /// <param name="yesNo">Result: true, when a cam is present</param>
    /// <returns></returns>
    [PreserveSig]
    int IsCamPresent(ref bool yesNo);
    /// <summary>
    /// Checks, if the cam is ready
    /// </summary>
    /// <param name="yesNo">Result: true, when the cam is ready</param>
    /// <returns></returns>
    [PreserveSig]
    int IsCamReady(ref bool yesNo);
    /// <summary>
    /// Sets the antenna power on/off
    /// </summary>
    /// <param name="onOff">Flag for the antenna power</param>
    /// <returns></returns>
    [PreserveSig]
    int SetAntennaPower(bool onOff);
    /// <summary>
    /// Sets the diseqc data
    /// </summary>
    /// <param name="diseqc">DiseqC data</param>
    /// <param name="len">len</param>
    /// <param name="Repeat">Repeat</param>
    /// <param name="Toneburst">Toneburst</param>
    /// <param name="ePolarity">ePolarity</param>
    /// <returns></returns>
    [PreserveSig]
    int SetDisEqc(IntPtr diseqc, byte len, byte Repeat, byte Toneburst, short ePolarity);
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
