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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi
{
  /// <summary>
  /// This class is used to hold and provide the relevant details for products that this plugin supports.
  /// </summary>
  public struct SmarDtvUsbCiProduct
  {
    #region COM imports

    // Each type definition is identical except for the GUID. It would be possible to eliminate these definitions
    // if property sets were used, however the properties are not documented and don't appear to be a one-to-one
    // mapping to functions.

    [Guid("a934e61e-2e24-4145-b45b-3e71830048f7"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITerraTecCinergyCiUsb
    {
      /// <summary>
      /// Initialise the CI interface. The call back delegate parameters are all optional.
      /// </summary>
      /// <param name="callBacks">A buffer containing a SmarDtvUsbCiCallBacks structure instance.</param>
      /// <returns>an HRESULT indicating whether the interface was successfully initialised</returns>
      [PreserveSig]
      Int32 USB2CI_Init([In] IntPtr callBacks);

      /// <summary>
      /// Open an MMI session with the CAM.
      /// </summary>
      /// <returns>an HRESULT indicating whether a session was successfully opened</returns>
      [PreserveSig]
      Int32 USB2CI_OpenMMI();

      /// <summary>
      /// Send an APDU to the CAM. This function can be used communicate with the CAM
      /// after an MMI session has been opened.
      /// </summary>
      /// <remarks>
      /// It is not possible send CA PMT APDUs with this interface because the underlying session is an MMI session.
      /// </remarks>
      /// <param name="apduLength">The length of the APDU in bytes.</param>
      /// <returns>an HRESULT indicating whether the APDU was successfully received by the CAM</returns>
      [PreserveSig]
      Int32 USB2CI_APDUToCAM([In] Int32 apduLength, [In, MarshalAs(UnmanagedType.LPArray)] byte[] apdu);

      /// <summary>
      /// Send PMT data to the CAM to request a service be descrambled.
      /// </summary>
      /// <remarks>
      /// It is not possible to descramble more than one service at a time with this interface.
      /// </remarks>
      /// <param name="pmt">The PMT.</param>
      /// <param name="pmtLength">The length of the PMT in bytes.</param>
      /// <returns>an HRESULT indicating whether the PMT was successfully received by the CAM</returns>
      [PreserveSig]
      Int32 USB2CI_GuiSendPMT([In, MarshalAs(UnmanagedType.LPArray)] byte[] pmt, [In] Int16 pmtLength);

      /// <summary>
      /// Get version information about the interface, driver and device.
      /// </summary>
      /// <param name="versionInfo">A buffer containing a VersionInfo structure instance.</param>
      /// <returns>an HRESULT indicating whether the version information was successfully retrieved</returns>
      [PreserveSig]
      Int32 USB2CI_GetVersion([In] IntPtr versionInfo);
    }

    [Guid("dd5a9b44-348a-4607-bf72-cfd8239e4432"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IHauppaugeWinTvCi
    {
      [PreserveSig]
      Int32 USB2CI_Init([In] IntPtr callBacks);

      [PreserveSig]
      Int32 USB2CI_OpenMMI();

      [PreserveSig]
      Int32 USB2CI_APDUToCAM([In] Int32 apduLength, [In, MarshalAs(UnmanagedType.LPArray)] byte[] apdu);

      [PreserveSig]
      Int32 USB2CI_GuiSendPMT([In, MarshalAs(UnmanagedType.LPArray)] byte[] pmt, [In] Int16 pmtLength);

      [PreserveSig]
      Int32 USB2CI_GetVersion([In] IntPtr version);
    }

    #endregion

    /// <summary>
    /// The official name of the product.
    /// </summary>
    public readonly string ProductName;

    /// <summary>
    /// The name of the device registered by the product's WDM driver (for example: WinTVCIUSB).
    /// </summary>
    public readonly string WdmDeviceName;

    /// <summary>
    /// The name of the device registered by the product's BDA driver (for example: WinTVCIUSBBDA Source).
    /// </summary>
    public readonly string BdaDeviceName;

    /// <summary>
    /// The name of the TV Server database setting that holds the product tuner association.
    /// </summary>
    public readonly string DbSettingName;

    /// <summary>
    /// The COM interface used to interact with the CI.
    /// </summary>
    public readonly Type ComInterface;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The official name of the product.</param>
    /// <param name="wdmDeviceName">The name of the device registered by the product's WDM driver.</param>
    /// <param name="bdaDeviceName">The name of the device registered by the product's BDA driver.</param>
    /// <param name="dbSettingName">The name of the TV Server database setting that holds the product tuner association.</param>
    /// <param name="comInterface">The COM interface used to interact with the CI.</param>
    public SmarDtvUsbCiProduct(string name, string wdmDeviceName, string bdaDeviceName, string dbSettingName, Type comInterface)
    {
      ProductName = name;
      WdmDeviceName = wdmDeviceName;
      BdaDeviceName = bdaDeviceName;
      DbSettingName = dbSettingName;
      ComInterface = comInterface;
    }

    private static ReadOnlyCollection<SmarDtvUsbCiProduct> _products = null;

    /// <summary>
    /// Get a list of the USB CI products that this plugin supports and which are based on the SmarDTV
    /// USB CI design.
    /// </summary>
    /// <returns>the product list</returns>
    public static ReadOnlyCollection<SmarDtvUsbCiProduct> GetProductList()
    {
      // We use a basic cache here.
      if (_products != null)
      {
        return _products;
      }

      List<SmarDtvUsbCiProduct> products = new List<SmarDtvUsbCiProduct>();

      // Hauppauge WinTV-CI
      // http://www.hauppauge.de/site/products/data_ci.html
      products.Add(new SmarDtvUsbCiProduct(
        "Hauppauge WinTV-CI",
        "WinTVCIUSB",
        "WinTVCIUSBBDA Source",
        "winTvCiTuner",
        typeof(IHauppaugeWinTvCi)
      ));

      // TerraTec Cinergy CI USB
      // http://www.terratec.net/en/products/Cinergy_CI_USB_2296.html
      products.Add(new SmarDtvUsbCiProduct(
        "TerraTec Cinergy CI USB",
        "US2CIBDA",
        "Cinergy CI USB Capture",
        "cinergyCiUsbTuner",
        typeof(ITerraTecCinergyCiUsb)
      ));

      _products = new ReadOnlyCollection<SmarDtvUsbCiProduct>(products);
      return _products;
    }
  }
}
