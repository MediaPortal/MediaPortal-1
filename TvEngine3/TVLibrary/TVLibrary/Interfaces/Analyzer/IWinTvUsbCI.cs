/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
Guid("3B687F98-41DD-4b40-A7A3-FD6A08799D5B"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWinTvUsbCI
  {
    [PreserveSig]
    int SetFilter(IBaseFilter tunerFilter);
    [PreserveSig]
    int IsModuleInstalled(ref bool yesNo);
    [PreserveSig]
    int IsCAMInstalled(ref bool yesNo);
    [PreserveSig]
    int DescrambleService(IntPtr pmt, short pmtLen, ref bool succeeded);
  };
}
