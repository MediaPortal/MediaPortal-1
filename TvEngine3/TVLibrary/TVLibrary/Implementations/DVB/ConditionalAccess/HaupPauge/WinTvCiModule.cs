/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Text;
using DirectShowLib;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class to handle the WinTV CI module and interaction with closed source dll.
  /// </summary>
  public class WinTvCiModule : WinTv_CI_Wrapper, IDisposable, ICiMenuActions
  {
    readonly IBaseFilter _winTvUsbCIFilter;

    readonly APDU_Callback cbOnAPDU;
    readonly Status_Callback cbOnStatus;
    readonly CamInfo_Callback cbOnCamInfo;
    readonly CloseMMI_Callback cbOnCloseMMI;
    private ICiMenuCallbacks m_ciMenuCallback;
    private DVB_MMI_Handler MMI;
    #region Constructor
    ///<summary>
    /// WinTV CI control
    ///</summary>
    ///<param name="winTvUsbCIFilter">WinTV CI filter</param>
    public WinTvCiModule(IBaseFilter winTvUsbCIFilter)
    {
      _winTvUsbCIFilter = winTvUsbCIFilter;
      cbOnAPDU = OnAPDU;
      cbOnStatus = OnStatus;
      cbOnCamInfo = OnCamInfo;
      cbOnCloseMMI = OnMMIClosed;
      MMI = new DVB_MMI_Handler("WinTvCI"); // callbacks are set on first access
    }
    #endregion

    #region Callbacks

    /// <summary>
    /// Gets the on Close MMI callback
    /// </summary>
    public CloseMMI_Callback CbOnCloseMMI
    {
      get { return cbOnCloseMMI; }
    }

    /// <summary>
    /// Gets the on cam info callback
    /// </summary>
    public CamInfo_Callback CbOnCamInfo
    {
      get { return cbOnCamInfo; }
    }

    /// <summary>
    /// Gets the OnStatus callback
    /// </summary>
    public Status_Callback CbOnStatus
    {
      get { return cbOnStatus; }
    }

    ///<summary>
    /// Gets the OnAPPDU callback
    ///</summary>
    public APDU_Callback CbOnAPDU
    {
      get { return cbOnAPDU; }
    }

    /// <summary>
    /// On APDU callback
    /// </summary>
    /// <param name="pUSBCIFilter">CI filter</param>
    /// <param name="APDU">APDU</param>
    /// <param name="SizeOfAPDU">Size of APDU</param>
    /// <returns></returns>
    public Int32 OnAPDU(IBaseFilter pUSBCIFilter, IntPtr APDU, long SizeOfAPDU)
    {
      Int32 MMI_length = (Int32)SizeOfAPDU;
      Log.Log.Info("WinTvCi OnAPDU: SizeOfAPDU={0}", MMI_length);
      byte[] bMMI = DVB_MMI.IntPtrToByteArray(APDU, 0, MMI_length);
#if DEBUG
      DVB_MMI.DumpBinary(bMMI, 0, MMI_length);
#endif
      try
      {
        MMI.HandleMMI(bMMI, MMI_length);
      }
      catch (Exception e)
      {
        Log.Log.Write("WinTvCI error:");
        Log.Log.Write(e);
      }
      return 0;
    }
    /// <summary>
    /// On Status callback
    /// </summary>
    /// <param name="pUSBCIFilter">CI filter</param>
    /// <param name="Status">Status</param>
    /// <returns></returns>
    public Int32 OnStatus(IBaseFilter pUSBCIFilter, long Status)
    {
      //Log.Log.Info("WinTvCI OnStatus: Status={0}", Status);
      if (Status == 1)
        Log.Log.Info("WinTvCI: Module installed but no CAM inserted?");
      if (Status == 2)
        Log.Log.Info("WinTvCI: Module installed & CAM inserted");
      return 0;
    }
    /// <summary>
    /// On CAM Info callback
    /// </summary>
    /// <param name="Context">Context</param>
    /// <param name="appType">AppType</param>
    /// <param name="appManuf">AppManufactor</param>
    /// <param name="manufCode">ManufactorCode</param>
    /// <param name="Info">Info</param>
    /// <returns></returns>
    public Int32 OnCamInfo(IntPtr Context, byte appType, ushort appManuf, ushort manufCode, StringBuilder Info)
    {
      Log.Log.Info("WinTvCi OnCamInfo: appType={0} appManuf={1} manufCode={2} info={3}", appType, appManuf, manufCode, Info.ToString());
      return 0;
    }
    /// <summary>
    /// On MMI Closed callback
    /// </summary>
    /// <param name="pUSBCIFilter">WinTV CI filter</param>
    /// <returns></returns>
    public Int32 OnMMIClosed(IBaseFilter pUSBCIFilter)
    {
      Log.Log.Info("WinTvCi OnMMIClosed");
      if (m_ciMenuCallback != null)
      {
        m_ciMenuCallback.OnCiCloseDisplay(0);
      }
      return 0;
    }
    #endregion

    #region Public functions
    /// <summary>
    /// Initiliases the WinTV CI
    /// </summary>
    /// <returns></returns>
    public Int32 Init()
    {
      return WinTVCI_Init(_winTvUsbCIFilter, cbOnStatus, cbOnCamInfo, cbOnAPDU, cbOnCloseMMI);
      //return WinTVCI_Init(_winTvUsbCIFilter, null, null, null, null);
    }

    /// <summary>
    /// Send the PMT
    /// </summary>
    /// <param name="PMT">The PMT</param>
    /// <param name="pmtLength">The length of the PMT</param>
    /// <returns></returns>
    public Int32 SendPMT(byte[] PMT, int pmtLength)
    {
      return WinTVCI_SendPMT(_winTvUsbCIFilter, PMT, pmtLength);
    }

    /// <summary>
    /// Send APDU
    /// </summary>
    /// <param name="APDU">APDU data</param>
    /// <param name="apduLength">APDU data length</param>
    /// <returns></returns>
    public Int32 SendAPDU(byte[] APDU, int apduLength)
    {
      return WinTVCI_SendAPDU(_winTvUsbCIFilter, APDU, apduLength);
    }

    /// <summary>
    /// Enables the tray icon
    /// </summary>
    /// <returns></returns>
    public Int32 EnableTrayIcon()
    {
      return WinTVCI_EnableTrayIcon(_winTvUsbCIFilter);
    }
    /// <summary>
    /// Shut the WinTV ci down
    /// </summary>
    /// <returns></returns>
    public Int32 Shutdown()
    {
      return WinTVCI_Shutdown(_winTvUsbCIFilter);
    }
    #endregion

    #region ICiMenuActions Member

    /// <summary>
    /// Sets the callback handler
    /// </summary>
    /// <param name="ciMenuHandler"></param>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        m_ciMenuCallback = ciMenuHandler;
        MMI.SetCiMenuHandler(ref m_ciMenuCallback); // pass through to handler
        return true;
      }
      return false;
    }


    /// <summary>
    /// Enters the CI menu of KNC1 card
    /// </summary>
    /// <returns></returns>
    public bool EnterCIMenu()
    {
      int res = WinTVCI_OpenMMI(_winTvUsbCIFilter);
      Log.Log.Debug("WinTvCi: Enter CI Menu Result:{0:x}", res);
      return true;
    }

    /// <summary>
    /// Closes the CI menu of KNC1 card
    /// </summary>
    /// <returns></returns>
    public bool CloseCIMenu()
    {
      Log.Log.Debug("WinTvCi: Close CI Menu");
      byte[] uData = new byte[5]; // default answer length
      DVB_MMI.CreateMMIClose(ref uData);
      SendAPDU(uData, uData.Length); // send to cam
      return true;
    }

    /// <summary>
    /// Selects a CI menu entry
    /// </summary>
    /// <param name="choice"></param>
    /// <returns></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Log.Debug("WinTvCi: Select CI Menu entry {0}", choice);
      byte[] uData = new byte[5]; // default answer length
      DVB_MMI.CreateMMISelect(choice, ref uData);
      SendAPDU(uData, uData.Length); // send to cam
      return true;
    }

    /// <summary>
    /// Sends an answer after CI request
    /// </summary>
    /// <param name="Cancel"></param>
    /// <param name="Answer"></param>
    /// <returns></returns>
    public bool SendMenuAnswer(bool Cancel, String Answer)
    {
      if (Answer == null) Answer = "";
      Log.Log.Debug("WinTvCi: Send Menu Answer: {0}, Cancel: {1}", Answer, Cancel);
      byte[] uData = new byte[1024];
      byte uLength1 = 0;
      byte uLength2 = 0;
      DVB_MMI.CreateMMIAnswer(Cancel, Answer, ref uData, ref uLength1, ref uLength2);
      SendAPDU(uData, uLength1); // send to cam
      return true;
    }
    #endregion


    #region IDisposable Member

    /// <summary>
    /// Disposing ressources
    /// </summary>
    public void Dispose()
    {
      Shutdown();
    }

    #endregion
  }
}