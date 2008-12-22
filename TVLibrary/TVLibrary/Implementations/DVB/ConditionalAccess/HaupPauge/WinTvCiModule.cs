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
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class to handle the WinTV CI module and interaction with closed source dll.
  /// </summary>
  public class WinTvCiModule: WinTv_CI_Wrapper
  {
    IBaseFilter _winTvUsbCIFilter;
    IntPtr _ptrMem = Marshal.AllocCoTaskMem(8192);

    APDU_Callback cbOnAPDU;
    Status_Callback cbOnStatus;
    CamInfo_Callback cbOnCamInfo;
    CloseMMI_Callback cbOnCloseMMI;
    
    #region Constructor
    public WinTvCiModule(IBaseFilter winTvUsbCIFilter)
    {
      _winTvUsbCIFilter = winTvUsbCIFilter;
      cbOnAPDU = new APDU_Callback(OnAPDU);
      cbOnStatus = new Status_Callback(OnStatus);
      cbOnCamInfo = new CamInfo_Callback(OnCamInfo);
      cbOnCloseMMI = new CloseMMI_Callback(OnMMIClosed);
    }
    #endregion

    #region Callbacks
    public static Int32 OnAPDU(IBaseFilter pUSBCIFilter, byte[] APDU, Int32 SizeOfAPDU)
    {
      Log.Log.Info("WinTvCi OnAPDU: SizeOfAPDU={0}", SizeOfAPDU);
      return 0;
    }
    public static Int32 OnStatus(IBaseFilter pUSBCIFilter, Int32 Status)
    {
      //Log.Log.Info("WinTvCI OnStatus: Status={0}", Status);
      if (Status == 1) Log.Log.Info("WinTvCI: Module installed but no CAM inserted?");
      if (Status == 2) Log.Log.Info("WinTvCI: Module installed & CAM inserted");
      return 0;
    }
    public static Int32 OnCamInfo(IntPtr Context, byte appType, ushort appManuf, ushort manufCode, StringBuilder Info)
    {
      Log.Log.Info("WinTvCi OnCamInfo: appType={0} appManuf={1} manufCode={2} info={3}", appType, appManuf, manufCode, Info.ToString());
      return 0;
    }
    public static Int32 OnMMIClosed(IBaseFilter pUSBCIFilter)
    {
      Log.Log.Info("WinTvCi OnMMIClosed");
      return 0;
    }
    #endregion

    #region Public functions
    public Int32 Init()
    {
      //return WinTVCI_Init(_winTvUsbCIFilter, cbOnStatus, cbOnCamInfo, cbOnAPDU, cbOnCloseMMI);
      return WinTVCI_Init(_winTvUsbCIFilter, null, null, null, null);
    }

    public Int32 SendPMT(byte[] PMT, int pmtLength)
    {
      return WinTVCI_SendPMT(_winTvUsbCIFilter, PMT, pmtLength);
    }

    public Int32 SendAPDU(byte[] APDU, int apduLength)
    {
      return WinTVCI_SendPMT(_winTvUsbCIFilter, APDU, apduLength);
    }

    public Int32 OpenMMI()
    {
      return WinTVCI_OpenMMI(_winTvUsbCIFilter);
    }

    public Int32 EnableTrayIcon()
    {
      return WinTVCI_EnableTrayIcon(_winTvUsbCIFilter);
    }

    public Int32 Shutdown()
    {
      return WinTVCI_Shutdown(_winTvUsbCIFilter);
    }
    #endregion
  }
}