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
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class WinTvCiModule
  {
    IBaseFilter _winTvUsbCIFilter;
    IWinTvUsbCI _interfaceWinTv;
    IntPtr _ptrMem = Marshal.AllocCoTaskMem(8192);
    public WinTvCiModule(IBaseFilter winTvUsbCIFilter, IBaseFilter analyzerFilter)
    {
      _winTvUsbCIFilter = winTvUsbCIFilter;
      _interfaceWinTv = (IWinTvUsbCI)analyzerFilter;
      _interfaceWinTv.SetFilter(winTvUsbCIFilter);
      Log.Log.Info("WinTvCI : module installed:{0}", IsDeviceInstalled);
      Log.Log.Info("WinTvCI : cam installed:{0}", IsCAMInstalled);
    }

    public bool IsDeviceInstalled
    {
      get
      {
        bool yesNo = false;
        _interfaceWinTv.IsModuleInstalled(ref yesNo);
        return yesNo;
      }
    }
    public bool IsCAMInstalled
    {
      get
      {
        bool yesNo = false;
        _interfaceWinTv.IsCAMInstalled(ref yesNo);
        return yesNo;
      }
    }
    public bool SendPMT(byte[] PMT, int pmtLength)
    {
      bool yesNo = false;
      for (int i = 0; i < pmtLength; ++i)
        Marshal.WriteByte(_ptrMem, 0, PMT[i]);
      _interfaceWinTv.DescrambleService(_ptrMem, (short)pmtLength, ref yesNo);
      return yesNo;
    }
  }
}
