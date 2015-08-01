#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product.Struct;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product
{
  internal class HauppaugeWinTvCi : SmarDtvUsbCiProductBase
  {
    #region COM interface

    [Guid("dd5a9b44-348a-4607-bf72-cfd8239e4432"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IHauppaugeWinTvCi
    {
      [PreserveSig]
      int USB2CI_Init([In] ref CiCallBack callBack);

      [PreserveSig]
      int USB2CI_OpenMMI();

      [PreserveSig]
      int USB2CI_APDUToCAM(int apduLength, [MarshalAs(UnmanagedType.LPArray)] byte[] apdu);

      [PreserveSig]
      int USB2CI_GuiSendPMT([MarshalAs(UnmanagedType.LPArray)] byte[] pmt, short pmtLength);

      [PreserveSig]
      int USB2CI_GetVersion(out VersionInfo versionInfo);
    }

    #endregion

    #region variables

    private IHauppaugeWinTvCi _interface = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="HauppaugeWinTvCi"/> class.
    /// </summary>
    public HauppaugeWinTvCi()
      : base("Hauppauge WinTV-CI", "WinTVCIUSB", "WinTVCIUSBBDA Source", "winTvCiTuner")
    {
      // http://www.hauppauge.de/site/products/data_ci.html
    }

    /// <summary>
    /// Initialise and take control of the product.
    /// </summary>
    /// <returns>the product's DirectShow filter if successful, otherwise <c>null</c></returns>
    public override IBaseFilter Initialise()
    {
      if (_filter != null)
      {
        return _filter;
      }
      _filter = base.Initialise();
      _interface = _filter as IHauppaugeWinTvCi;
      return _filter;
    }

    /// <summary>
    /// Open the CI interface.
    /// </summary>
    /// <param name="callBack">A set of delegates for the driver to invoke when the CI state changes or MMI becomes available.</param>
    /// <returns>an HRESULT indicating whether the interface was successfully opened</returns>
    public override int OpenInterface(ref CiCallBack callBack)
    {
      return _interface.USB2CI_Init(ref callBack);
    }

    /// <summary>
    /// Open an MMI session with the CAM.
    /// </summary>
    /// <returns>an HRESULT indicating whether the session was successfully opened</returns>
    public override int OpenMmiSession()
    {
      return _interface.USB2CI_OpenMMI();
    }

    /// <summary>
    /// Send an APDU to the CAM.
    /// </summary>
    /// <param name="apdu">The APDU.</param>
    /// <param name="apduLength">The length of the APDU in bytes.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully received by the CAM</returns>
    public override int SendMmiApdu(int apduLength, byte[] apdu)
    {
      return _interface.USB2CI_APDUToCAM(apduLength, apdu);
    }

    /// <summary>
    /// Send PMT data to the CAM to request a service be descrambled.
    /// </summary>
    /// <param name="pmt">The PMT.</param>
    /// <param name="pmtLength">The length of the PMT in bytes.</param>
    /// <returns>an HRESULT indicating whether the PMT was successfully received by the CAM</returns>
    public override int SendPmt(byte[] pmt, short pmtLength)
    {
      return _interface.USB2CI_GuiSendPMT(pmt, pmtLength);
    }

    /// <summary>
    /// Get version information about the interface, driver and device.
    /// </summary>
    /// <param name="versionInfo">The version information.</param>
    /// <returns>an HRESULT indicating whether the version information was successfully retrieved</returns>
    public override int GetVersionInfo(out VersionInfo versionInfo)
    {
      return _interface.USB2CI_GetVersion(out versionInfo);
    }

    /// <summary>
    /// Deinitialise and release control of the product.
    /// </summary>
    public override void Deinitialise()
    {
      base.Deinitialise();
      Release.ComObject("SmarDTV USB CI Hauppauge WinTV-CI filter", ref _filter);
      _interface = null;
    }
  }
}