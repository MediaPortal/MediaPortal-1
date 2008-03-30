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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
Guid("C71E2EFA-2439-4dbe-A1F7-935ADC37A4EC"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IKNC
  {
    [PreserveSig]
    int SetTunerFilter(IBaseFilter tunerFilter);
    [PreserveSig]
    int IsKNC(ref bool yesNo);
    [PreserveSig]
    int IsCamReady(ref bool yesNo);
    [PreserveSig]
    int IsCIAvailable(ref bool yesNo);
    [PreserveSig]
    int SetDisEqc(short diseqcType, short hiband, short vertical);
    [PreserveSig]
    int DescrambleService(IntPtr pmt, short pmtLen, ref bool succeeded);
    [PreserveSig]
    int DescrambleMultiple(IntPtr serviceIds, short nrOfServiceIds, ref bool succeeded);
  };
}
