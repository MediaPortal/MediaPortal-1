#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// WinTV_CI_Wrapper class
  /// </summary>
  public class WinTv_CI_Wrapper
  {
    #region Callback definitions

    ///<summary>
    /// APDU_Callback
    ///</summary>
    ///<param name="pUSBCIFilter">CI filter</param>
    ///<param name="APDU">APDU data</param>
    ///<param name="SizeOfAPDU">APDU data size</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public delegate Int32 APDU_Callback([Out] IBaseFilter pUSBCIFilter, [Out] IntPtr APDU, [Out] long SizeOfAPDU);

    /// <summary>
    /// Status callback
    /// </summary>
    /// <param name="pUSBCIFilter">CI filter</param>
    /// <param name="Status">Status</param>
    /// <returns></returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public delegate Int32 Status_Callback([Out] IBaseFilter pUSBCIFilter, [Out] long Status);

    /// <summary>
    /// Cam Info callback
    /// </summary>
    /// <param name="Context">Context</param>
    /// <param name="appType">AppType</param>
    /// <param name="appManuf">AppManuf</param>
    /// <param name="manufCode">Manufactor code</param>
    /// <param name="Info">Info</param>
    /// <returns></returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public delegate Int32 CamInfo_Callback(
      [Out] IntPtr Context, [Out] byte appType, [Out] ushort appManuf, [Out] ushort manufCode,
      [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Info);

    /// <summary>
    /// Close MMI Callback
    /// </summary>
    /// <param name="pUSBCIFilter">CI filter</param>
    /// <returns></returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public delegate Int32 CloseMMI_Callback([Out] IBaseFilter pUSBCIFilter);

    #endregion

    #region Public functions

    /// <summary>
    /// WinTV CI init
    /// </summary>
    /// <param name="pUSBCIFilter">Filter</param>
    /// <param name="onStatus">Status callback</param>
    /// <param name="onCamInfo">Cam Info callback</param>
    /// <param name="onAPDU">APDU callback</param>
    /// <param name="onCloseMMI">Close MMI Callback</param>
    /// <returns></returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_Init(IBaseFilter pUSBCIFilter, Status_Callback onStatus,
                                            CamInfo_Callback onCamInfo, APDU_Callback onAPDU,
                                            CloseMMI_Callback onCloseMMI);

    /// <summary>
    /// WinTV CI Send pmt
    /// </summary>
    /// <param name="pUSBCIFilter">Filter</param>
    /// <param name="pPMT">The PMT</param>
    /// <param name="lLength">Length of the pmt</param>
    /// <returns></returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_SendPMT(IBaseFilter pUSBCIFilter,
                                               [In, MarshalAs(UnmanagedType.LPArray)] byte[] pPMT, int lLength);

    /// <summary>
    /// WinTV CI 
    /// </summary>
    /// <param name="pUSBCIFilter">Filter</param>
    /// <param name="pAPDU">APDU data</param>
    /// <param name="lLength">Length of the apdu data</param>
    /// <returns></returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_SendAPDU(IBaseFilter pUSBCIFilter,
                                                [In, MarshalAs(UnmanagedType.LPArray)] byte[] pAPDU, int lLength);

    /// <summary>
    /// WinTV CI open the mmi
    /// </summary>
    /// <param name="pUSBCIFilter">filter</param>
    /// <returns></returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_OpenMMI(IBaseFilter pUSBCIFilter);

    /// <summary>
    /// WinTV CI Enable the tray icon
    /// </summary>
    /// <param name="pUSBCIFilter">filter</param>
    /// <returns></returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_EnableTrayIcon(IBaseFilter pUSBCIFilter);

    /// <summary>
    /// Shutdowns the WinTV CI filter
    /// </summary>
    /// <param name="pUSBCIFilter">filter</param>
    /// <returns></returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 WinTVCI_Shutdown(IBaseFilter pUSBCIFilter);

    #endregion
  }
}