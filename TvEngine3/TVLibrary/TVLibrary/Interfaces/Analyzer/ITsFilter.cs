/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
Guid("5EB9F392-E7FD-4071-8E44-3590E5E767BA"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsFilter
  {
    [PreserveSig]
    int AddChannel([Out] out ITsChannel instance);
    [PreserveSig]
    int DeleteChannel([Out] out ITsChannel instance);
    [PreserveSig]
    int GetChannel(ref short index, [Out] out ITsChannel instance);
    [PreserveSig]
    int GetChannelCount(ref short count);
    [PreserveSig]
    int DeleteAllChannels();
  }
}
