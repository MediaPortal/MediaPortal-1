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
  public class WinTv_CI_Wrapper
  {
    #region Callback definitions
    public delegate Int32 APDU_Callback([Out] IBaseFilter pUSBCIFilter,[Out, MarshalAs(UnmanagedType.LPArray)] byte[] APDU,[Out] Int32 SizeOfAPDU);
    public delegate Int32 Status_Callback([Out] IBaseFilter pUSBCIFilter, [Out] Int32 Status);
    public delegate Int32 CamInfo_Callback([Out] IntPtr Context,[Out] byte appType,[Out] ushort appManuf,[Out] ushort manufCode, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Info);
    public delegate Int32 CloseMMI_Callback([Out] IBaseFilter pUSBCIFilter);
    #endregion

    #region Public functions
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_Init(IBaseFilter pUSBCIFilter, Status_Callback onStatus,CamInfo_Callback onCamInfo,APDU_Callback onAPDU,CloseMMI_Callback onCloseMMI);

    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_SendPMT(IBaseFilter pUSBCIFilter, [In, MarshalAs(UnmanagedType.LPArray)]  byte[] pPMT, int lLength);

    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_SendAPDU(IBaseFilter pUSBCIFilter, [In, MarshalAs(UnmanagedType.LPArray)]  byte[] pAPDU, int lLength);

    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_OpenMMI(IBaseFilter pUSBCIFilter);

    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_EnableTrayIcon(IBaseFilter pUSBCIFilter);

    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_Shutdown(IBaseFilter pUSBCIFilter);
    #endregion
  }
}